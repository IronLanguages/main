/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

[assembly: PythonModule("cPickle", typeof(IronPython.Modules.PythonPickle))]
namespace IronPython.Modules {
    public static class PythonPickle {
        public const string __doc__ = "Fast object serialization/deserialization.\n\n"
            + "Differences from CPython:\n"
            + " - does not implement the undocumented fast mode\n";
        [System.Runtime.CompilerServices.SpecialName]
        public static void PerformModuleReload(PythonContext/*!*/ context, PythonDictionary/*!*/ dict) {
            context.EnsureModuleException("PickleError", dict, "PickleError", "cPickle");
            context.EnsureModuleException("PicklingError", dict, "PicklingError", "cPickle");
            context.EnsureModuleException("UnpicklingError", dict, "UnpicklingError", "cPickle");
            context.EnsureModuleException("UnpickleableError", dict, "UnpickleableError", "cPickle");
            context.EnsureModuleException("BadPickleGet", dict, "BadPickleGet", "cPickle");
            dict["__builtins__"] = context.BuiltinModuleInstance;
            dict["compatible_formats"] = PythonOps.MakeList("1.0", "1.1", "1.2", "1.3", "2.0");
        }

        private static readonly PythonStruct.Struct _float64 = PythonStruct.Struct.Create(">d");
        private static readonly PythonStruct.Struct _uint8 = PythonStruct.Struct.Create("B");
        private static readonly PythonStruct.Struct _uint16 = PythonStruct.Struct.Create("<H");
        private static readonly PythonStruct.Struct _uint32 = PythonStruct.Struct.Create("<i");
        
        private const int highestProtocol = 2;
        
        public const string __version__ = "1.71";
        public const string format_version = "2.0";
        
        public static int HIGHEST_PROTOCOL {
            get { return highestProtocol; }
        }
        
        private const string Newline = "\n";

        #region Public module-level functions

        [Documentation("dump(obj, file, protocol=0) -> None\n\n"
            + "Pickle obj and write the result to file.\n"
            + "\n"
            + "See documentation for Pickler() for a description the file, protocol, and\n"
            + "(deprecated) bin parameters."
            )]
        public static void dump(CodeContext/*!*/ context, object obj, object file, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            PicklerObject pickler = new PicklerObject(context, file, protocol, bin);
            pickler.dump(context, obj);
        }

        [Documentation("dumps(obj, protocol=0) -> pickle string\n\n"
            + "Pickle obj and return the result as a string.\n"
            + "\n"
            + "See the documentation for Pickler() for a description of the protocol and\n"
            + "(deprecated) bin parameters."
            )]
        public static string dumps(CodeContext/*!*/ context, object obj, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin) {
            //??? possible perf enhancement: use a C# TextWriter-backed IFileOutput and
            // thus avoid Python call overhead. Also do similar thing for LoadFromString.
            object stringIO = PythonOps.Invoke(context, DynamicHelpers.GetPythonTypeFromType(typeof(PythonStringIO)), "StringIO");
            PicklerObject pickler = new PicklerObject(context, stringIO, protocol, bin);
            pickler.dump(context, obj);
            return Converter.ConvertToString(PythonOps.Invoke(context, stringIO, "getvalue"));
        }

        [Documentation("load(file) -> unpickled object\n\n"
            + "Read pickle data from the open file object and return the corresponding\n"
            + "unpickled object. Data after the first pickle found is ignored, but the file\n"
            + "cursor is not reset, so if a file objects contains multiple pickles, then\n"
            + "load() may be called multiple times to unpickle them.\n"
            + "\n"
            + "file: an object (such as an open file or a StringIO) with read(num_chars) and\n"
            + "    readline() methods that return strings\n"
            + "\n"
            + "load() automatically determines if the pickle data was written in binary or\n"
            + "text mode."
            )]
        public static object load(CodeContext/*!*/ context, object file) {
            return new UnpicklerObject(context, file).load(context);
        }

        [Documentation("loads(string) -> unpickled object\n\n"
            + "Read a pickle object from a string, unpickle it, and return the resulting\n"
            + "reconstructed object. Characters in the string beyond the end of the first\n"
            + "pickle are ignored."
            )]
        public static object loads(CodeContext/*!*/ context, string @string) {
            PythonFile pf = PythonFile.Create(
                context,
                new MemoryStream(@string.MakeByteArray()),
                "loads",
                "b"
            );

            return new UnpicklerObject(context, pf).load(context);
        }

        #endregion

        #region File I/O wrappers

        /// <summary>
        /// Interface for "file-like objects" that implement the protocol needed by load() and friends.
        /// This enables the creation of thin wrappers that make fast .NET types and slow Python types look the same.
        /// </summary>
        internal interface IFileInput {
            string Read(CodeContext/*!*/ context, int size);
            string ReadLine(CodeContext/*!*/ context);
        }

        /// <summary>
        /// Interface for "file-like objects" that implement the protocol needed by dump() and friends.
        /// This enables the creation of thin wrappers that make fast .NET types and slow Python types look the same.
        /// </summary>
        internal interface IFileOutput {
            void Write(CodeContext/*!*/ context, string data);
        }

        private class PythonFileInput : IFileInput {
            private object _readMethod;
            private object _readLineMethod;

            public PythonFileInput(CodeContext/*!*/ context, object file) {
                if (!PythonOps.TryGetBoundAttr(context, file, "read", out _readMethod) ||
                    !PythonOps.IsCallable(context, _readMethod) ||
                    !PythonOps.TryGetBoundAttr(context, file, "readline", out _readLineMethod) ||
                    !PythonOps.IsCallable(context, _readLineMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'read' and 'readline' attributes");
                }
            }

            public string Read(CodeContext/*!*/ context, int size) {
                return Converter.ConvertToString(PythonCalls.Call(context, _readMethod, size));
            }

            public string ReadLine(CodeContext/*!*/ context) {
                return Converter.ConvertToString(PythonCalls.Call(context, _readLineMethod));
            }
        }

        private class PythonFileOutput : IFileOutput {
            private object _writeMethod;

            public PythonFileOutput(CodeContext/*!*/ context, object file) {
                if (!PythonOps.TryGetBoundAttr(context, file, "write", out _writeMethod) ||
                    !PythonOps.IsCallable(context, this._writeMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'write' attribute");
                }
            }

            public void Write(CodeContext/*!*/ context, string data) {
                PythonCalls.Call(context, _writeMethod, data);
            }
        }

        private class PythonReadableFileOutput : PythonFileOutput {
            private object _getValueMethod;

            public PythonReadableFileOutput(CodeContext/*!*/ context, object file)
                : base(context, file) {
                if (!PythonOps.TryGetBoundAttr(context, file, "getvalue", out _getValueMethod) ||
                    !PythonOps.IsCallable(context, _getValueMethod)
                ) {
                    throw PythonOps.TypeError("argument must have callable 'getvalue' attribute");
                }
            }

            public object GetValue(CodeContext/*!*/ context) {
                return PythonCalls.Call(context, _getValueMethod);
            }
        }

        #endregion

        #region Opcode constants

        internal static class Opcode {
            public const string Append = "a";
            public const string Appends = "e";
            public const string BinFloat = "G";
            public const string BinGet = "h";
            public const string BinInt = "J";
            public const string BinInt1 = "K";
            public const string BinInt2 = "M";
            public const string BinPersid = "Q";
            public const string BinPut = "q";
            public const string BinString = "T";
            public const string BinUnicode = "X";
            public const string Build = "b";
            public const string Dict = "d";
            public const string Dup = "2";
            public const string EmptyDict = "}";
            public const string EmptyList = "]";
            public const string EmptyTuple = ")";
            public const string Ext1 = "\x82";
            public const string Ext2 = "\x83";
            public const string Ext4 = "\x84";
            public const string Float = "F";
            public const string Get = "g";
            public const string Global = "c";
            public const string Inst = "i";
            public const string Int = "I";
            public const string List = "l";
            public const string Long = "L";
            public const string Long1 = "\x8a";
            public const string Long4 = "\x8b";
            public const string LongBinGet = "j";
            public const string LongBinPut = "r";
            public const string Mark = "(";
            public const string NewFalse = "\x89";
            public const string NewObj = "\x81";
            public const string NewTrue = "\x88";
            public const string NoneValue = "N";
            public const string Obj = "o";
            public const string PersId = "P";
            public const string Pop = "0";
            public const string PopMark = "1";
            public const string Proto = "\x80";
            public const string Put = "p";
            public const string Reduce = "R";
            public const string SetItem = "s";
            public const string SetItems = "u";
            public const string ShortBinstring = "U";
            public const string Stop = ".";
            public const string String = "S";
            public const string Tuple = "t";
            public const string Tuple1 = "\x85";
            public const string Tuple2 = "\x86";
            public const string Tuple3 = "\x87";
            public const string Unicode = "V";
        }

        #endregion

        #region Pickler object

        public static PicklerObject Pickler(CodeContext/*!*/ context, [DefaultParameterValue(null)]object file, [DefaultParameterValue(null)]object protocol, [DefaultParameterValue(null)]object bin) {
            return new PicklerObject(context, file, protocol, bin);
        }

        [Documentation("Pickler(file, protocol=0) -> Pickler object\n\n"
            + "A Pickler object serializes Python objects to a pickle bytecode stream, which\n"
            + "can then be converted back into equivalent objects using an Unpickler.\n"
            + "\n"
            + "file: an object (such as an open file) that has a write(string) method.\n"
            + "protocol: if omitted, protocol 0 is used. If HIGHEST_PROTOCOL or a negative\n"
            + "    number, the highest available protocol is used.\n"
            + "bin: (deprecated; use protocol instead) for backwards compability, a 'bin'\n"
            + "    keyword parameter is supported. When protocol is specified it is ignored.\n"
            + "    If protocol is not specified, then protocol 0 is used if bin is false, and\n"
            + "    protocol 1 is used if bin is true."
            )]
        [PythonType("Pickler"), PythonHidden]
        public class PicklerObject {

            private const char LowestPrintableChar = (char)32;
            private const char HighestPrintableChar = (char)126;
            // max elements that can be set/appended at a time using SETITEMS/APPENDS

            private delegate void PickleFunction(CodeContext/*!*/ context, object value);
            private readonly Dictionary<PythonType, PickleFunction> dispatchTable;

            private int _batchSize = 1000;
            private IFileOutput _file;
            private int _protocol;
            private IDictionary _memo;
            private object _persist_id;

            #region Public API

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            public IDictionary memo {
                get { return _memo; }
                set { _memo = value; }
            }

            public int proto {
                get { return _protocol; }
                set { _protocol = value; }
            }

            public int _BATCHSIZE {
                get { return _batchSize; }
                set { _batchSize = value; }
            }

            public object persistent_id {
                get {
                    return _persist_id;
                }
                set {
                    _persist_id = value;
                }
            }

            public int binary {
                get { return _protocol == 0 ? 1 : 0; }
                set { _protocol = value; }
            }

            public int fast {
                // We don't implement fast, but we silently ignore it when it's set so that test_cpickle works.
                // For a description of fast, see http://mail.python.org/pipermail/python-bugs-list/2001-October/007695.html
                get { return 0; }
                set { /* ignore */ }
            }

            public PicklerObject(CodeContext/*!*/ context, object file, object protocol, object bin) {
                dispatchTable = new Dictionary<PythonType, PickleFunction>();
                dispatchTable[TypeCache.Boolean] = SaveBoolean;
                dispatchTable[TypeCache.Int32] = SaveInteger;
                dispatchTable[TypeCache.Null] = SaveNone;
                dispatchTable[TypeCache.Dict] = SaveDict;
                dispatchTable[TypeCache.BigInteger] = SaveLong;
                dispatchTable[TypeCache.Double] = SaveFloat;
                dispatchTable[TypeCache.String] = SaveUnicode;
                dispatchTable[TypeCache.PythonTuple] = SaveTuple;
                dispatchTable[TypeCache.List] = SaveList;
                dispatchTable[TypeCache.OldClass] = SaveGlobal;
                dispatchTable[TypeCache.Function] = SaveGlobal;
                dispatchTable[TypeCache.BuiltinFunction] = SaveGlobal;
                dispatchTable[TypeCache.PythonType] = SaveGlobal;
                dispatchTable[TypeCache.OldInstance] = SaveInstance;

                int intProtocol;
                if (file == null) {
                    _file = new PythonReadableFileOutput(context, new PythonStringIO.StringO());
                } else if (Converter.TryConvertToInt32(file, out intProtocol)) {
                    // For undocumented (yet tested in official CPython tests) list-based pickler, the
                    // user could do something like Pickler(1), which would create a protocol-1 pickler
                    // with an internal string output buffer (retrievable using GetValue()). For a little
                    // more info, see
                    // https://sourceforge.net/tracker/?func=detail&atid=105470&aid=939395&group_id=5470
                    _file = new PythonReadableFileOutput(context, new PythonStringIO.StringO());
                    protocol = file;
                } else if (file is IFileOutput) {
                    _file = (IFileOutput)file;
                } else {
                    _file = new PythonFileOutput(context, file);
                }

                this._memo = new PythonDictionary();

                if (protocol == null) protocol = PythonOps.IsTrue(bin) ? 1 : 0;

                intProtocol = PythonContext.GetContext(context).ConvertToInt32(protocol);
                if (intProtocol > highestProtocol) {
                    throw PythonOps.ValueError("pickle protocol {0} asked for; the highest available protocol is {1}", intProtocol, highestProtocol);
                } else if (intProtocol < 0) {
                    this._protocol = highestProtocol;
                } else {
                    this._protocol = intProtocol;
                }
            }

            [Documentation("dump(obj) -> None\n\n"
                + "Pickle obj and write the result to the file object that was passed to the\n"
                + "constructor\n."
                + "\n"
                + "Note that you may call dump() multiple times to pickle multiple objects. To\n"
                + "unpickle the stream, you will need to call Unpickler's load() method a\n"
                + "corresponding number of times.\n"
                + "\n"
                + "The first time a particular object is encountered, it will be pickled normally.\n"
                + "If the object is encountered again (in the same or a later dump() call), a\n"
                + "reference to the previously generated value will be pickled. Unpickling will\n"
                + "then create multiple references to a single object."
                )]
            public void dump(CodeContext/*!*/ context, object obj) {
                if (_protocol >= 2) WriteProto(context);
                Save(context, obj);
                Write(context, Opcode.Stop);
            }

            [Documentation("clear_memo() -> None\n\n"
                + "Clear the memo, which is used internally by the pickler to keep track of which\n"
                + "objects have already been pickled (so that shared or recursive objects are\n"
                + "pickled only once)."
                )]
            public void clear_memo() {
                _memo.Clear();
            }

            [Documentation("getvalue() -> string\n\n"
                + "Return the value of the internal string. Raises PicklingError if a file object\n"
                + "was passed to this pickler's constructor."
                )]
            public object getvalue(CodeContext/*!*/ context) {
                if (_file is PythonReadableFileOutput) {
                    return ((PythonReadableFileOutput)_file).GetValue(context);
                }
                throw PythonExceptions.CreateThrowable(PicklingError(context), "Attempt to getvalue() a non-list-based pickler");
            }

            #endregion

            #region Save functions

            private void Save(CodeContext/*!*/ context, object obj) {
                if (_persist_id != null) {
                    string res = Converter.ConvertToString(PythonContext.GetContext(context).CallSplat(_persist_id, obj));
                    if (res != null) {
                        SavePersId(context, res);
                        return;
                    }
                }

                if (_memo.Contains(PythonOps.Id(obj))) {
                    WriteGet(context, obj);
                } else {                    
                    PickleFunction pickleFunction;
                    PythonType objType = DynamicHelpers.GetPythonType(obj);
                    if (!dispatchTable.TryGetValue(objType, out pickleFunction)) {
                        if (objType.IsSubclassOf(TypeCache.PythonType)) {
                            // treat classes with metaclasses like regular classes
                            pickleFunction = SaveGlobal;
                        } else {
                            pickleFunction = SaveObject;
                        }
                    }
                    pickleFunction(context, obj);
                }
            }

            private void SavePersId(CodeContext/*!*/ context, string res) {
                if (this.binary != 0) {
                    Save(context, res);
                    Write(context, Opcode.BinPersid);
                } else {
                    Write(context, Opcode.PersId);
                    Write(context, res);
                    Write(context, "\n");
                }
            }

            private void SaveBoolean(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Boolean), "arg must be bool");
                if (_protocol < 2) {
                    Write(context, Opcode.Int);
                    Write(context, String.Format("0{0}", ((bool)obj) ? 1 : 0));
                    Write(context, Newline);
                } else {
                    if ((bool)obj) {
                        Write(context, Opcode.NewTrue);
                    } else {
                        Write(context, Opcode.NewFalse);
                    }
                }
            }

            private void SaveDict(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Dict), "arg must be dict");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);

                if (_protocol < 1) {
                    Write(context, Opcode.Mark);
                    Write(context, Opcode.Dict);
                } else {
                    Write(context, Opcode.EmptyDict);
                }

                WritePut(context, obj);
                BatchSetItems(context, (DictionaryOps.iteritems((IDictionary<object, object>)obj)));
            }

            private void SaveFloat(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Double), "arg must be float");

                if (_protocol < 1) {
                    Write(context, Opcode.Float);
                    WriteFloatAsString(context, obj);
                } else {
                    Write(context, Opcode.BinFloat);
                    WriteFloat64(context, obj);
                }
            }

            private void SaveGlobal(CodeContext/*!*/ context, object obj) {
                Debug.Assert(
                    DynamicHelpers.GetPythonType(obj).Equals(TypeCache.OldClass) ||
                    DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Function) ||
                    DynamicHelpers.GetPythonType(obj).Equals(TypeCache.BuiltinFunction) ||
                    DynamicHelpers.GetPythonType(obj).Equals(TypeCache.PythonType) ||
                    DynamicHelpers.GetPythonType(obj).IsSubclassOf(TypeCache.PythonType),
                    "arg must be classic class, function, built-in function or method, or new-style type"
                );

                object name;
                if (PythonOps.TryGetBoundAttr(context, obj, "__name__", out name)) {
                    SaveGlobalByName(context, obj, name);
                } else {
                    throw CannotPickle(context, obj, "could not determine its __name__");
                }
            }

            private void SaveGlobalByName(CodeContext/*!*/ context, object obj, object name) {
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));

                object moduleName = FindModuleForGlobal(context, obj, name);

                if (_protocol >= 2) {
                    object code;
                    if (((IDictionary<object, object>)PythonCopyReg.GetExtensionRegistry(context)).TryGetValue(PythonTuple.MakeTuple(moduleName, name), out code)) {
                        if (IsUInt8(context, code)) {
                            Write(context, Opcode.Ext1);
                            WriteUInt8(context, code);
                        } else if (IsUInt16(context, code)) {
                            Write(context, Opcode.Ext2);
                            WriteUInt16(context, code);
                        } else if (IsInt32(context, code)) {
                            Write(context, Opcode.Ext4);
                            WriteInt32(context, code);
                        } else {
                            throw PythonOps.RuntimeError("unrecognized integer format");
                        }
                        return;
                    }
                }

                Memoize(obj);

                Write(context, Opcode.Global);
                WriteStringPair(context, moduleName, name);
                WritePut(context, obj);
            }

            private void SaveInstance(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.OldInstance), "arg must be old-class instance");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));

                Write(context, Opcode.Mark);

                // Memoize() call isn't in the usual spot to allow class to be memoized before
                // instance (when using proto other than 0) to match CPython's bytecode output

                object objClass;
                if (!PythonOps.TryGetBoundAttr(context, obj, "__class__", out objClass)) {
                    throw CannotPickle(context, obj, "could not determine its __class__");
                }

                if (_protocol < 1) {
                    object className, classModuleName;
                    if (!PythonOps.TryGetBoundAttr(context, objClass, "__name__", out className)) {
                        throw CannotPickle(context, obj, "its __class__ has no __name__");
                    }
                    classModuleName = FindModuleForGlobal(context, objClass, className);

                    Memoize(obj);
                    WriteInitArgs(context, obj);
                    Write(context, Opcode.Inst);
                    WriteStringPair(context, classModuleName, className);
                } else {
                    Save(context, objClass);
                    Memoize(obj);
                    WriteInitArgs(context, obj);
                    Write(context, Opcode.Obj);
                }

                WritePut(context, obj);

                object getStateCallable;
                if (PythonOps.TryGetBoundAttr(context, obj, "__getstate__", out getStateCallable)) {
                    Save(context, PythonCalls.Call(context, getStateCallable));
                } else {
                    Save(context, PythonOps.GetBoundAttr(context, obj, "__dict__"));
                }

                Write(context, Opcode.Build);
            }

            private void SaveInteger(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Int32), "arg must be int");
                if (_protocol < 1) {
                    Write(context, Opcode.Int);
                    WriteIntAsString(context, obj);
                } else {
                    if (IsUInt8(context, obj)) {
                        Write(context, Opcode.BinInt1);
                        WriteUInt8(context, obj);
                    } else if (IsUInt16(context, obj)) {
                        Write(context, Opcode.BinInt2);
                        WriteUInt16(context, obj);
                    } else if (IsInt32(context, obj)) {
                        Write(context, Opcode.BinInt);
                        WriteInt32(context, obj);
                    } else {
                        throw PythonOps.RuntimeError("unrecognized integer format");
                    }
                }
            }

            private void SaveList(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.List), "arg must be list");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);
                if (_protocol < 1) {
                    Write(context, Opcode.Mark);
                    Write(context, Opcode.List);
                } else {
                    Write(context, Opcode.EmptyList);
                }

                WritePut(context, obj);
                BatchAppends(context, ((IEnumerable)obj).GetEnumerator());
            }

            private void SaveLong(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.BigInteger), "arg must be long");

                if (_protocol < 2) {
                    Write(context, Opcode.Long);
                    WriteLongAsString(context, obj);
                } else {
                    if (((BigInteger)obj).IsZero()) {
                        Write(context, Opcode.Long1);
                        WriteUInt8(context, 0);
                    } else {
                        byte[] dataBytes = ((BigInteger)obj).ToByteArray();
                        if (dataBytes.Length < 256) {
                            Write(context, Opcode.Long1);
                            WriteUInt8(context, dataBytes.Length);
                        } else {
                            Write(context, Opcode.Long4);
                            WriteInt32(context, dataBytes.Length);
                        }

                        foreach (byte b in dataBytes) {
                            WriteUInt8(context, b);
                        }
                    }
                }
            }

            private void SaveNone(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.Null), "arg must be None");
                Write(context, Opcode.NoneValue);
            }

            /// <summary>
            /// Call the appropriate reduce method for obj and pickle the object using
            /// the resulting data. Use the first available of
            /// copy_reg.dispatch_table[type(obj)], obj.__reduce_ex__, and obj.__reduce__.
            /// </summary>
            private void SaveObject(CodeContext/*!*/ context, object obj) {
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);

                object reduceCallable, result;
                PythonType objType = DynamicHelpers.GetPythonType(obj);

                if (((IDictionary<object, object>)PythonCopyReg.GetDispatchTable(context)).TryGetValue(objType, out reduceCallable)) {
                    result = PythonCalls.Call(context, reduceCallable, obj);
                } else if (PythonOps.TryGetBoundAttr(context, obj, "__reduce_ex__", out reduceCallable)) {
                    if (obj is PythonType) {
                        result = context.LanguageContext.Call(context, reduceCallable, obj, _protocol);
                    } else {
                        result = context.LanguageContext.Call(context, reduceCallable, _protocol);
                    }
                } else if (PythonOps.TryGetBoundAttr(context, obj, "__reduce__", out reduceCallable)) {
                    if (obj is PythonType) {
                        result = context.LanguageContext.Call(context, reduceCallable, obj);
                    } else {
                        result = context.LanguageContext.Call(context, reduceCallable);
                    }
                } else {
                    throw PythonOps.AttributeError("no reduce function found for {0}", obj);
                }

                if (objType.Equals(TypeCache.String)) {
                    if (_memo.Contains(PythonOps.Id(obj))) {
                        WriteGet(context, obj);
                    } else {
                        SaveGlobalByName(context, obj, result);
                    }
                } else if (result is PythonTuple) {
                    PythonTuple rt = (PythonTuple)result;
                    switch (rt.__len__()) {
                        case 2:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], null, null, null);
                            break;
                        case 3:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], null, null);
                            break;
                        case 4:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], rt[3], null);
                            break;
                        case 5:
                            SaveReduce(context, obj, reduceCallable, rt[0], rt[1], rt[2], rt[3], rt[4]);
                            break;
                        default:
                            throw CannotPickle(context, obj, "tuple returned by {0} must have to to five elements", reduceCallable);
                    }
                } else {
                    throw CannotPickle(context, obj, "{0} must return string or tuple", reduceCallable);
                }
            }

            /// <summary>
            /// Pickle the result of a reduce function.
            /// 
            /// Only context, obj, func, and reduceCallable are required; all other arguments may be null.
            /// </summary>
            private void SaveReduce(CodeContext/*!*/ context, object obj, object reduceCallable, object func, object args, object state, object listItems, object dictItems) {
                if (!PythonOps.IsCallable(context, func)) {
                    throw CannotPickle(context, obj, "func from reduce() should be callable");
                } else if (!(args is PythonTuple) && args != null) {
                    throw CannotPickle(context, obj, "args from reduce() should be a tuple");
                } else if (listItems != null && !(listItems is IEnumerator)) {
                    throw CannotPickle(context, obj, "listitems from reduce() should be a list iterator");
                } else if (dictItems != null && !(dictItems is IEnumerator)) {
                    throw CannotPickle(context, obj, "dictitems from reduce() should be a dict iterator");
                }

                object funcName;
                string funcNameString;
                if (!PythonOps.TryGetBoundAttr(context, func, "__name__", out funcName)) {
                    throw CannotPickle(context, obj, "func from reduce() ({0}) should have a __name__ attribute");
                } else if (!Converter.TryConvertToString(funcName, out funcNameString) || funcNameString == null) {
                    throw CannotPickle(context, obj, "__name__ of func from reduce() must be string");
                }

                if (_protocol >= 2 && "__newobj__" == funcNameString) {
                    if (args == null) {
                        throw CannotPickle(context, obj, "__newobj__ arglist is None");
                    }
                    PythonTuple argsTuple = (PythonTuple)args;
                    if (argsTuple.__len__() == 0) {
                        throw CannotPickle(context, obj, "__newobj__ arglist is empty");
                    } else if (!DynamicHelpers.GetPythonType(obj).Equals(argsTuple[0])) {
                        throw CannotPickle(context, obj, "args[0] from __newobj__ args has the wrong class");
                    }
                    Save(context, argsTuple[0]);
                    Save(context, argsTuple[new Slice(1, null)]);
                    Write(context, Opcode.NewObj);
                } else {
                    Save(context, func);
                    Save(context, args);
                    Write(context, Opcode.Reduce);
                }

                WritePut(context, obj);

                if (state != null) {
                    Save(context, state);
                    Write(context, Opcode.Build);
                }

                if (listItems != null) {
                    BatchAppends(context, (IEnumerator)listItems);
                }

                if (dictItems != null) {
                    BatchSetItems(context, (IEnumerator)dictItems);
                }
            }

            private void SaveTuple(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.PythonTuple), "arg must be tuple");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                PythonTuple t = (PythonTuple)obj;
                string opcode;
                bool needMark = false;
                if (_protocol > 0 && t.__len__() == 0) {
                    opcode = Opcode.EmptyTuple;
                } else if (_protocol >= 2 && t.__len__() == 1) {
                    opcode = Opcode.Tuple1;
                } else if (_protocol >= 2 && t.__len__() == 2) {
                    opcode = Opcode.Tuple2;
                } else if (_protocol >= 2 && t.__len__() == 3) {
                    opcode = Opcode.Tuple3;
                } else {
                    opcode = Opcode.Tuple;
                    needMark = true;
                }

                if (needMark) Write(context, Opcode.Mark);
                foreach (object o in t) {
                    Save(context, o);
                }

                if (_memo.Contains(PythonOps.Id(obj))) {
                    // recursive tuple
                    if (_protocol == 1) {
                        Write(context, Opcode.PopMark);
                    } else {
                        if (_protocol == 0) {
                            Write(context, Opcode.Pop);
                        }
                        for (int i = 0; i < t.__len__(); i++) {
                            Write(context, Opcode.Pop);
                        }
                    }
                    WriteGet(context, obj);
                    return;
                }

                Write(context, opcode);

                if (t.__len__() > 0) {
                    Memoize(t);
                    WritePut(context, t);
                }
            }

            private void SaveUnicode(CodeContext/*!*/ context, object obj) {
                Debug.Assert(DynamicHelpers.GetPythonType(obj).Equals(TypeCache.String), "arg must be unicode");
                Debug.Assert(!_memo.Contains(PythonOps.Id(obj)));
                Memoize(obj);
                if (_protocol < 1) {
                    Write(context, Opcode.Unicode);
                    WriteUnicodeStringRaw(context, obj);
                } else {
                    Write(context, Opcode.BinUnicode);
                    WriteUnicodeStringUtf8(context, obj);
                }

                WritePut(context, obj);
            }

            #endregion

            #region Output encoding

            /// <summary>
            /// Write value in pickle decimalnl_short format.
            /// </summary>
            private void WriteFloatAsString(CodeContext/*!*/ context, object value) {                
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.Double));
                Write(context, DoubleOps.__repr__(context, (double)value));
                Write(context, Newline);
            }

            /// <summary>
            /// Write value in pickle float8 format.
            /// </summary>
            private void WriteFloat64(CodeContext/*!*/ context, object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.Double));
                Write(context, _float64.pack(context, value));
            }

            /// <summary>
            /// Write value in pickle uint1 format.
            /// </summary>
            private void WriteUInt8(CodeContext/*!*/ context, object value) {
                Debug.Assert(IsUInt8(context, value));
                Write(context, _uint8.pack(context, value));
            }

            /// <summary>
            /// Write value in pickle uint2 format.
            /// </summary>
            private void WriteUInt16(CodeContext/*!*/ context, object value) {
                Debug.Assert(IsUInt16(context, value));
                Write(context, _uint16.pack(context, value));
            }

            /// <summary>
            /// Write value in pickle int4 format.
            /// </summary>
            private void WriteInt32(CodeContext/*!*/ context, object value) {
                Debug.Assert(IsInt32(context, value));
                Write(context, _uint32.pack(context, value));
            }

            /// <summary>
            /// Write value in pickle decimalnl_short format.
            /// </summary>
            private void WriteIntAsString(CodeContext/*!*/ context, object value) {
                Debug.Assert(IsInt32(context, value));
                Write(context, PythonOps.Repr(context, value));
                Write(context, Newline);
            }

            /// <summary>
            /// Write value in pickle decimalnl_long format.
            /// </summary>
            private void WriteLongAsString(CodeContext/*!*/ context, object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.BigInteger));
                Write(context, PythonOps.Repr(context, value));
                Write(context, Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestringnl format.
            /// </summary>
            private void WriteUnicodeStringRaw(CodeContext/*!*/ context, object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                // manually escape backslash and newline
                Write(context, StringOps.RawUnicodeEscapeEncode(((string)value).Replace("\\", "\\u005c").Replace("\n", "\\u000a")));
                Write(context, Newline);
            }

            /// <summary>
            /// Write value in pickle unicodestring4 format.
            /// </summary>
            private void WriteUnicodeStringUtf8(CodeContext/*!*/ context, object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                string encodedString = System.Text.Encoding.UTF8.GetBytes((string)value).MakeString();
                WriteInt32(context, encodedString.Length);
                Write(context, encodedString);
            }

            /// <summary>
            /// Write value in pickle stringnl_noescape_pair format.
            /// </summary>
            private void WriteStringPair(CodeContext/*!*/ context, object value1, object value2) {
                Debug.Assert(DynamicHelpers.GetPythonType(value1).Equals(TypeCache.String));
                Debug.Assert(DynamicHelpers.GetPythonType(value2).Equals(TypeCache.String));
#if DEBUG
                Debug.Assert(IsPrintableAscii(value1));
                Debug.Assert(IsPrintableAscii(value2));
#endif
                Write(context, (string)value1);
                Write(context, Newline);
                Write(context, (string)value2);
                Write(context, Newline);
            }

            #endregion

            #region Type checking

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle uint1 format.
            /// </summary>
            private bool IsUInt8(CodeContext/*!*/ context, object value) {
                PythonContext pc = PythonContext.GetContext(context);

                return pc.LessThanOrEqual(0, value) && pc.LessThan(value, 1 << 8);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle uint2 format.
            /// </summary>
            private bool IsUInt16(CodeContext/*!*/ context, object value) {
                PythonContext pc = PythonContext.GetContext(context);

                return pc.LessThanOrEqual(1 << 8, value) && pc.LessThan(value, 1 << 16);
            }

            /// <summary>
            /// Return true if value is appropriate for formatting in pickle int4 format.
            /// </summary>
            private bool IsInt32(CodeContext/*!*/ context, object value) {
                PythonContext pc = PythonContext.GetContext(context);

                return pc.LessThanOrEqual(Int32.MinValue, value) && pc.LessThanOrEqual(value, Int32.MaxValue);
            }

#if DEBUG
            /// <summary>
            /// Return true if value is a string where each value is in the range of printable ASCII characters.
            /// </summary>
            private bool IsPrintableAscii(object value) {
                Debug.Assert(DynamicHelpers.GetPythonType(value).Equals(TypeCache.String));
                string strValue = (string)value;
                foreach (char c in strValue) {
                    if (!(LowestPrintableChar <= c && c <= HighestPrintableChar)) return false;
                }
                return true;
            }
#endif

            #endregion

            #region Output generation helpers

            private void Write(CodeContext/*!*/ context, string data) {
                _file.Write(context, data);
            }

            private void WriteGet(CodeContext/*!*/ context, object obj) {
                Debug.Assert(_memo.Contains(PythonOps.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)_memo[PythonOps.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(PythonContext.GetContext(context).GreaterThanOrEqual(index, 0));
                if (_protocol < 1) {
                    Write(context, Opcode.Get);
                    WriteIntAsString(context, index);
                } else {
                    if (IsUInt8(context, index)) {
                        Write(context, Opcode.BinGet);
                        WriteUInt8(context, index);
                    } else {
                        Write(context, Opcode.LongBinGet);
                        WriteInt32(context, index);
                    }
                }
            }

            private void WriteInitArgs(CodeContext/*!*/ context, object obj) {
                object getInitArgsCallable;
                if (PythonOps.TryGetBoundAttr(context, obj, "__getinitargs__", out getInitArgsCallable)) {
                    object initArgs = PythonCalls.Call(context, getInitArgsCallable);
                    if (!(initArgs is PythonTuple)) {
                        throw CannotPickle(context, obj, "__getinitargs__() must return tuple");
                    }
                    foreach (object arg in (PythonTuple)initArgs) {
                        Save(context, arg);
                    }
                }
            }

            private void WritePut(CodeContext/*!*/ context, object obj) {
                Debug.Assert(_memo.Contains(PythonOps.Id(obj)));
                // Memo entries are tuples, and the first element is the memo index
                IList<object> memoEntry = (IList<object>)_memo[PythonOps.Id(obj)];

                object index = memoEntry[0];
                Debug.Assert(PythonContext.GetContext(context).GreaterThanOrEqual(index, 0));
                
                if (_protocol < 1) {
                    Write(context, Opcode.Put);
                    WriteIntAsString(context, index);
                } else {
                    if (IsUInt8(context, index)) {
                        Write(context, Opcode.BinPut);
                        WriteUInt8(context, index);
                    } else {
                        Write(context, Opcode.LongBinPut);
                        WriteInt32(context, index);
                    }
                }
            }

            private void WriteProto(CodeContext/*!*/ context) {
                Write(context, Opcode.Proto);
                WriteUInt8(context, _protocol);
            }

            /// <summary>
            /// Emit a series of opcodes that will set append all items indexed by iter
            /// to the object at the top of the stack. Use APPENDS if possible, but
            /// append no more than BatchSize items at a time.
            /// </summary>
            private void BatchAppends(CodeContext/*!*/ context, IEnumerator enumerator) {
                if (_protocol < 1) {
                    while (enumerator.MoveNext()) {
                        Save(context, enumerator.Current);
                        Write(context, Opcode.Append);
                    }
                } else {
                    object next;
                    if (enumerator.MoveNext()) {
                        next = enumerator.Current;
                    } else {
                        return;
                    }

                    int batchCompleted = 0;
                    object current;

                    // We do a one-item lookahead to avoid emitting an APPENDS for a
                    // single remaining item.
                    while (enumerator.MoveNext()) {
                        current = next;
                        next = enumerator.Current;

                        if (batchCompleted == _BATCHSIZE) {
                            Write(context, Opcode.Appends);
                            batchCompleted = 0;
                        }

                        if (batchCompleted == 0) {
                            Write(context, Opcode.Mark);
                        }

                        Save(context, current);
                        batchCompleted++;
                    }

                    if (batchCompleted == _BATCHSIZE) {
                        Write(context, Opcode.Appends);
                        batchCompleted = 0;
                    }
                    Save(context, next);
                    batchCompleted++;

                    if (batchCompleted > 1) {
                        Write(context, Opcode.Appends);
                    } else {
                        Write(context, Opcode.Append);
                    }
                }
            }

            /// <summary>
            /// Emit a series of opcodes that will set all (key, value) pairs indexed by
            /// iter in the object at the top of the stack. Use SETITEMS if possible,
            /// but append no more than BatchSize items at a time.
            /// </summary>
            private void BatchSetItems(CodeContext/*!*/ context, IEnumerator enumerator) {
                PythonTuple kvTuple;
                if (_protocol < 1) {
                    while (enumerator.MoveNext()) {
                        kvTuple = (PythonTuple)enumerator.Current;
                        Save(context, kvTuple[0]);
                        Save(context, kvTuple[1]);
                        Write(context, Opcode.SetItem);
                    }
                } else {
                    object nextKey, nextValue;
                    if (enumerator.MoveNext()) {
                        kvTuple = (PythonTuple)enumerator.Current;
                        nextKey = kvTuple[0];
                        nextValue = kvTuple[1];
                    } else {
                        return;
                    }

                    int batchCompleted = 0;
                    object curKey, curValue;

                    // We do a one-item lookahead to avoid emitting a SETITEMS for a
                    // single remaining item.
                    while (enumerator.MoveNext()) {
                        curKey = nextKey;
                        curValue = nextValue;
                        kvTuple = (PythonTuple)enumerator.Current;
                        nextKey = kvTuple[0];
                        nextValue = kvTuple[1];

                        if (batchCompleted == _BATCHSIZE) {
                            Write(context, Opcode.SetItems);
                            batchCompleted = 0;
                        }

                        if (batchCompleted == 0) {
                            Write(context, Opcode.Mark);
                        }

                        Save(context, curKey);
                        Save(context, curValue);
                        batchCompleted++;
                    }

                    if (batchCompleted == _BATCHSIZE) {
                        Write(context, Opcode.SetItems);
                        batchCompleted = 0;
                    }
                    Save(context, nextKey);
                    Save(context, nextValue);
                    batchCompleted++;

                    if (batchCompleted > 1) {
                        Write(context, Opcode.SetItems);
                    } else {
                        Write(context, Opcode.SetItem);
                    }
                }
            }

            #endregion

            #region Other private helper methods

            private Exception CannotPickle(CodeContext/*!*/ context, object obj, string format, params object[] args) {
                StringBuilder msgBuilder = new StringBuilder();
                msgBuilder.Append("Can't pickle ");
                msgBuilder.Append(PythonOps.ToString(context, obj));
                if (format != null) {
                    msgBuilder.Append(": ");
                    msgBuilder.Append(String.Format(format, args));
                }
                return PythonExceptions.CreateThrowable(PickleError(context), msgBuilder.ToString());
            }

            private void Memoize(object obj) {
                if (!_memo.Contains(PythonOps.Id(obj))) {
                    _memo[PythonOps.Id(obj)] = PythonTuple.MakeTuple(_memo.Count, obj);
                }
            }

            /// <summary>
            /// Find the module for obj and ensure that obj is reachable in that module by the given name.
            /// 
            /// Throw PicklingError if any of the following are true:
            ///  - The module couldn't be determined.
            ///  - The module couldn't be loaded.
            ///  - The given name doesn't exist in the module.
            ///  - The given name is a different object than obj.
            /// 
            /// Otherwise, return the name of the module.
            /// 
            /// To determine which module obj lives in, obj.__module__ is used if available. The
            /// module named by obj.__module__ is loaded if needed. If obj has no __module__
            /// attribute, then each loaded module is searched. If a loaded module has an
            /// attribute with the given name, and that attribute is the same object as obj,
            /// then that module is used.
            /// </summary>
            private object FindModuleForGlobal(CodeContext/*!*/ context, object obj, object name) {
                object module;
                object moduleName;
                if (PythonOps.TryGetBoundAttr(context, obj, "__module__", out moduleName)) {
                    // TODO: Global SystemState
                    Builtin.__import__(context, Converter.ConvertToString(moduleName));

                    object foundObj;
                    if (Importer.TryGetExistingModule(context, Converter.ConvertToString(moduleName), out module) &&
                        PythonOps.TryGetBoundAttr(context, module, Converter.ConvertToString(name), out foundObj)) {
                        if (PythonOps.IsRetBool(foundObj, obj)) {
                            return moduleName;
                        } else {
                            throw CannotPickle(context, obj, "it's not the same object as {0}.{1}", moduleName, name);
                        }
                    } else {
                        throw CannotPickle(context, obj, "it's not found as {0}.{1}", moduleName, name);
                    }
                } else {
                    // No obj.__module__, so crawl through all loaded modules looking for obj
                    foreach (KeyValuePair<object, object> modulePair in PythonContext.GetContext(context).SystemStateModules) {
                        moduleName = modulePair.Key;
                        module = modulePair.Value;
                        object foundObj;
                        if (PythonOps.TryGetBoundAttr(context, module, Converter.ConvertToString(name), out foundObj) &&
                            PythonOps.IsRetBool(foundObj, obj)
                        ) {
                            return moduleName;
                        }
                    }
                    throw CannotPickle(context, obj, "could not determine its module");
                }

            }

            #endregion

        }

        #endregion

        #region Unpickler object

        public static UnpicklerObject Unpickler(CodeContext/*!*/ context, object file) {
            return new UnpicklerObject(context, file);
        }

        [Documentation("Unpickler(file) -> Unpickler object\n\n"
            + "An Unpickler object reads a pickle bytecode stream and creates corresponding\n"
            + "objects."
            + "\n"
            + "file: an object (such as an open file or a StringIO) with read(num_chars) and\n"
            + "    readline() methods that return strings"
            )]
        [PythonType("Unpickler"), PythonHidden]
        public class UnpicklerObject {

            private readonly object _mark = new object();

            private delegate void LoadFunction(CodeContext/*!*/ context);
            private readonly Dictionary<string, LoadFunction> _dispatch;

            private IFileInput _file;
            private List _stack;
            private IDictionary<object, object> _memo;
            private object _pers_loader;

            public UnpicklerObject(CodeContext context, object file) {
                this._file = file as IFileInput ?? new PythonFileInput(context, file);
                _memo = new PythonDictionary();

                _dispatch = new Dictionary<string, LoadFunction>();
                _dispatch[""] = LoadEof;
                _dispatch[Opcode.Append] = LoadAppend;
                _dispatch[Opcode.Appends] = LoadAppends;
                _dispatch[Opcode.BinFloat] = LoadBinFloat;
                _dispatch[Opcode.BinGet] = LoadBinGet;
                _dispatch[Opcode.BinInt] = LoadBinInt;
                _dispatch[Opcode.BinInt1] = LoadBinInt1;
                _dispatch[Opcode.BinInt2] = LoadBinInt2;
                _dispatch[Opcode.BinPersid] = LoadBinPersid;
                _dispatch[Opcode.BinPut] = LoadBinPut;
                _dispatch[Opcode.BinString] = LoadBinString;
                _dispatch[Opcode.BinUnicode] = LoadBinUnicode;
                _dispatch[Opcode.Build] = LoadBuild;
                _dispatch[Opcode.Dict] = LoadDict;
                _dispatch[Opcode.Dup] = LoadDup;
                _dispatch[Opcode.EmptyDict] = LoadEmptyDict;
                _dispatch[Opcode.EmptyList] = LoadEmptyList;
                _dispatch[Opcode.EmptyTuple] = LoadEmptyTuple;
                _dispatch[Opcode.Ext1] = LoadExt1;
                _dispatch[Opcode.Ext2] = LoadExt2;
                _dispatch[Opcode.Ext4] = LoadExt4;
                _dispatch[Opcode.Float] = LoadFloat;
                _dispatch[Opcode.Get] = LoadGet;
                _dispatch[Opcode.Global] = LoadGlobal;
                _dispatch[Opcode.Inst] = LoadInst;
                _dispatch[Opcode.Int] = LoadInt;
                _dispatch[Opcode.List] = LoadList;
                _dispatch[Opcode.Long] = LoadLong;
                _dispatch[Opcode.Long1] = LoadLong1;
                _dispatch[Opcode.Long4] = LoadLong4;
                _dispatch[Opcode.LongBinGet] = LoadLongBinGet;
                _dispatch[Opcode.LongBinPut] = LoadLongBinPut;
                _dispatch[Opcode.Mark] = LoadMark;
                _dispatch[Opcode.NewFalse] = LoadNewFalse;
                _dispatch[Opcode.NewObj] = LoadNewObj;
                _dispatch[Opcode.NewTrue] = LoadNewTrue;
                _dispatch[Opcode.NoneValue] = LoadNoneValue;
                _dispatch[Opcode.Obj] = LoadObj;
                _dispatch[Opcode.PersId] = LoadPersId;
                _dispatch[Opcode.Pop] = LoadPop;
                _dispatch[Opcode.PopMark] = LoadPopMark;
                _dispatch[Opcode.Proto] = LoadProto;
                _dispatch[Opcode.Put] = LoadPut;
                _dispatch[Opcode.Reduce] = LoadReduce;
                _dispatch[Opcode.SetItem] = LoadSetItem;
                _dispatch[Opcode.SetItems] = LoadSetItems;
                _dispatch[Opcode.ShortBinstring] = LoadShortBinstring;
                _dispatch[Opcode.String] = LoadString;
                _dispatch[Opcode.Tuple] = LoadTuple;
                _dispatch[Opcode.Tuple1] = LoadTuple1;
                _dispatch[Opcode.Tuple2] = LoadTuple2;
                _dispatch[Opcode.Tuple3] = LoadTuple3;
                _dispatch[Opcode.Unicode] = LoadUnicode;
            }

            [Documentation("load() -> unpickled object\n\n"
                + "Read pickle data from the file object that was passed to the constructor and\n"
                + "return the corresponding unpickled objects."
               )]
            public object load(CodeContext/*!*/ context) {
                _stack = new List();

                string opcode = Read(context, 1);

                while (opcode != Opcode.Stop) {
                    if (!_dispatch.ContainsKey(opcode)) {
                        throw CannotUnpickle(context, "invalid opcode: {0}", PythonOps.Repr(context, opcode));
                    }
                    _dispatch[opcode](context);
                    opcode = Read(context, 1);
                }

                return _stack.pop();
            }

            [Documentation("noload() -> unpickled object\n\n"
                // 1234567890123456789012345678901234567890123456789012345678901234567890123456789
                + "Like load(), but don't import any modules or create create any instances of\n"
                + "user-defined types. (Builtin objects such as ints, tuples, etc. are created as\n"
                + "with load().)\n"
                + "\n"
                + "This is primarily useful for scanning a pickle for persistent ids without\n"
                + "incurring the overhead of completely unpickling an object. See the pickle\n"
                + "module documentation for more information about persistent ids."
               )]
            public void noload(CodeContext/*!*/ context) {
                throw PythonOps.NotImplementedError("noload() is not implemented");
            }

            private Exception CannotUnpickle(CodeContext/*!*/ context, string format, params object[] args) {
                return PythonExceptions.CreateThrowable(UnpicklingError(context), String.Format(format, args));
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            public IDictionary<object, object> memo {
                get { return _memo; }
                set { _memo = value; }
            }

            public object persistent_load {
                get {
                    return _pers_loader;
                }
                set {
                    _pers_loader = value;
                }
            }

            private object MemoGet(CodeContext/*!*/ context, long key) {
                object value;
                if (_memo.TryGetValue(key, out value)) return value;
                throw PythonExceptions.CreateThrowable(BadPickleGet(context), String.Format("memo key {0} not found", key));
            }

            private void MemoPut(long key, object value) {
                _memo[key] = value;
            }

            [PropertyMethod, System.Runtime.CompilerServices.SpecialName]
            public int GetMarkIndex(CodeContext/*!*/ context) {
                int i = _stack.__len__() - 1;
                while (i > 0 && _stack[i] != _mark) i -= 1;
                if (i == -1) throw CannotUnpickle(context, "mark not found");
                return i;
            }

            private string Read(CodeContext/*!*/ context, int size) {
                string res = _file.Read(context, size);
                if (res.Length < size) {
                    throw PythonOps.EofError("unexpected EOF while unpickling");
                }
                return res;
            }

            private string ReadLineNoNewline(CodeContext/*!*/ context) {
                string raw = _file.ReadLine(context);
                return raw.Substring(0, raw.Length - 1);
            }

            private object ReadFloatString(CodeContext/*!*/ context) {
                return DoubleOps.__new__(context, TypeCache.Double, ReadLineNoNewline(context));
            }

            private double ReadFloat64(CodeContext/*!*/ context) {
                int index = 0;
                return PythonStruct.CreateDoubleValue(context, ref index, false, Read(context, 8));
            }

            private object ReadIntFromString(CodeContext/*!*/ context) {
                string raw = ReadLineNoNewline(context);
                if ("00" == raw) return ScriptingRuntimeHelpers.False;
                else if ("01" == raw) return ScriptingRuntimeHelpers.True;
                return Int32Ops.__new__(context, TypeCache.Int32, raw);
            }

            private int ReadInt32(CodeContext/*!*/ context) {
                int index = 0;
                return PythonStruct.CreateIntValue(context, ref index, true, Read(context, 4));
            }

            private object ReadLongFromString(CodeContext/*!*/ context) {
                return BigIntegerOps.__new__(context, TypeCache.BigInteger, ReadLineNoNewline(context));
            }

            private object ReadLong(CodeContext/*!*/ context, int size) {
                return new BigInteger(Read(context, size).MakeByteArray());
            }

            private char ReadUInt8(CodeContext/*!*/ context) {
                int index = 0;
                return PythonStruct.CreateCharValue(context, ref index, Read(context, 1));
            }

            private ushort ReadUInt16(CodeContext/*!*/ context) {
                int index = 0;
                return PythonStruct.CreateUShortValue(context, ref index, true, Read(context, 2));
            }

            public object find_global(CodeContext/*!*/ context, object module, object attr) {
                object moduleObject;
                if (!Importer.TryGetExistingModule(context, Converter.ConvertToString(module), out moduleObject)) {
                    Builtin.__import__(context, Converter.ConvertToString(module));
                    moduleObject = PythonContext.GetContext(context).SystemStateModules[module];
                }
                return PythonOps.GetBoundAttr(context, moduleObject, Converter.ConvertToString(attr));
            }

            private object MakeInstance(CodeContext/*!*/ context, object cls, object[] args) {
                OldClass oc = cls as OldClass;
                if (oc != null) {
                    OldInstance inst = new OldInstance(context, oc);
                    if (args.Length != 0 || PythonOps.HasAttr(context, cls, "__getinitargs__")) {
                        PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, inst, "__init__"), args);
                    }
                    return inst;
                }
                return PythonOps.CallWithContext(context, cls, args);
            }

            private void PopMark(int markIndex) {
                _stack.__delslice__(markIndex, _stack.__len__());
            }

            /// <summary>
            /// Interpret everything from markIndex to the top of the stack as a sequence
            /// of key, value, key, value, etc. Set dict[key] = value for each. Pop
            /// everything from markIndex up when done.
            /// </summary>
            private void SetItems(PythonDictionary dict, int markIndex) {
                for (int i = markIndex + 1; i < _stack.__len__(); i += 2) {
                    dict[_stack[i]] = _stack[i+1];
                }
                PopMark(markIndex);
            }

            private void LoadEof(CodeContext/*!*/ context) {
                throw PythonOps.EofError("unexpected end of opcode stream");
            }

            private void LoadAppend(CodeContext/*!*/ context) {
                object item = _stack.pop();
                object seq = _stack[-1];
                if (seq is List) {
                    ((List)seq).append(item);
                } else {
                    PythonCalls.Call(context, PythonOps.GetBoundAttr(context, seq, "append"), item);
                }
            }

            private void LoadAppends(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                object seq = _stack[markIndex - 1];
                object stackSlice = _stack.__getslice__(markIndex + 1, _stack.__len__());
                if (seq is List) {
                    ((List)seq).extend(stackSlice);
                } else {
                    PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, seq, "extend"), stackSlice);
                }
                PopMark(markIndex);
            }

            private void LoadBinFloat(CodeContext/*!*/ context) {
                _stack.append(ReadFloat64(context));
            }

            private void LoadBinGet(CodeContext/*!*/ context) {
                _stack.append(MemoGet(context, (long)ReadUInt8(context)));
            }

            private void LoadBinInt(CodeContext/*!*/ context) {
                _stack.append(ReadInt32(context));
            }

            private void LoadBinInt1(CodeContext/*!*/ context) {
                _stack.append((int)ReadUInt8(context));
            }

            private void LoadBinInt2(CodeContext/*!*/ context) {
                _stack.append((int)ReadUInt16(context));
            }

            private void LoadBinPersid(CodeContext/*!*/ context) {
                if (_pers_loader == null) throw CannotUnpickle(context, "cannot unpickle binary persistent ID w/o persistent_load");

                _stack.append(PythonContext.GetContext(context).CallSplat(_pers_loader, _stack.pop()));
            }

            private void LoadBinPut(CodeContext/*!*/ context) {
                MemoPut((long)ReadUInt8(context), _stack[-1]);
            }

            private void LoadBinString(CodeContext/*!*/ context) {
                _stack.append(Read(context, ReadInt32(context)));
            }

            private void LoadBinUnicode(CodeContext/*!*/ context) {
                _stack.append(StringOps.decode(context, Read(context, ReadInt32(context)), "utf-8", "strict"));
            }

            private void LoadBuild(CodeContext/*!*/ context) {
                object arg = _stack.pop();
                object inst = _stack[-1];
                object setStateCallable;
                if (PythonOps.TryGetBoundAttr(context, inst, "__setstate__", out setStateCallable)) {
                    PythonOps.CallWithContext(context, setStateCallable, arg);
                    return;
                }

                PythonDictionary dict;
                PythonDictionary slots;
                if (arg == null) {
                    dict = null;
                    slots = null;
                } else if (arg is PythonDictionary) {
                    dict = (PythonDictionary)arg;
                    slots = null;
                } else if (arg is PythonTuple) {
                    PythonTuple argsTuple = (PythonTuple)arg;
                    if (argsTuple.__len__() != 2) {
                        throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                    }
                    dict = (PythonDictionary)argsTuple[0];
                    slots = (PythonDictionary)argsTuple[1];
                } else {
                    throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
                }

                if (dict != null) {
                    object instDict;
                    if (PythonOps.TryGetBoundAttr(context, inst, "__dict__", out instDict)) {
                        PythonDictionary realDict = instDict as PythonDictionary;
                        if (realDict != null) {
                            realDict.update(context, dict);
                        } else {
                            object updateCallable;
                            if (PythonOps.TryGetBoundAttr(context, instDict, "update", out updateCallable)) {
                                PythonOps.CallWithContext(context, updateCallable, dict);
                            } else {
                                throw CannotUnpickle(context, "could not update __dict__ {0} when building {1}", dict, inst);
                            }
                        }
                    }
                }

                if (slots != null) {
                    foreach(object key in (IEnumerable)slots) {
                        PythonOps.SetAttr(context, inst, (string)key, slots[key]);
                    }
                }
            }

            private void LoadDict(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                PythonDictionary dict = new PythonDictionary((_stack.__len__() - 1 - markIndex) / 2);
                SetItems(dict, markIndex);
                _stack.append(dict);
            }

            private void LoadDup(CodeContext/*!*/ context) {
                _stack.append(_stack[-1]);
            }

            private void LoadEmptyDict(CodeContext/*!*/ context) {
                _stack.append(new PythonDictionary());
            }

            private void LoadEmptyList(CodeContext/*!*/ context) {
                _stack.append(PythonOps.MakeList());
            }

            private void LoadEmptyTuple(CodeContext/*!*/ context) {
                _stack.append(PythonTuple.MakeTuple());
            }

            private void LoadExt1(CodeContext/*!*/ context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[(int)ReadUInt8(context)];
                _stack.append(find_global(context, global[0], global[1]));
            }

            private void LoadExt2(CodeContext/*!*/ context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[(int)ReadUInt16(context)];
                _stack.append(find_global(context, global[0], global[1]));
            }

            private void LoadExt4(CodeContext/*!*/ context) {
                PythonTuple global = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[ReadInt32(context)];
                _stack.append(find_global(context, global[0], global[1]));
            }

            private void LoadFloat(CodeContext/*!*/ context) {
                _stack.append(ReadFloatString(context));
            }

            private void LoadGet(CodeContext/*!*/ context) {
                try {
                    _stack.append(MemoGet(context, (long)(int)ReadIntFromString(context)));
                } catch (ArgumentException) {
                    throw PythonExceptions.CreateThrowable(BadPickleGet(context), "while executing GET: invalid integer value");
                }
            }

            private void LoadGlobal(CodeContext/*!*/ context) {
                string module = ReadLineNoNewline(context);
                string attr = ReadLineNoNewline(context);
                _stack.append(find_global(context, module, attr));
            }

            private void LoadInst(CodeContext/*!*/ context) {
                LoadGlobal(context);
                object cls = _stack.pop();
                if (cls is OldClass || cls is PythonType) {
                    int markIndex = GetMarkIndex(context);
                    object[] args = _stack.GetSliceAsArray(markIndex + 1, _stack.__len__());
                    PopMark(markIndex);

                    _stack.append(MakeInstance(context, cls, args));
                } else {
                    throw PythonOps.TypeError("expected class or type after INST, got {0}", DynamicHelpers.GetPythonType(cls));
                }
            }

            private void LoadInt(CodeContext/*!*/ context) {
                _stack.append(ReadIntFromString(context));
            }

            private void LoadList(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                object list = _stack.__getslice__(markIndex + 1, _stack.__len__());
                PopMark(markIndex);
                _stack.append(list);
            }

            private void LoadLong(CodeContext/*!*/ context) {
                _stack.append(ReadLongFromString(context));
            }

            private void LoadLong1(CodeContext/*!*/ context) {
                _stack.append(ReadLong(context, ReadUInt8(context)));
            }

            private void LoadLong4(CodeContext/*!*/ context) {
                _stack.append(ReadLong(context, ReadInt32(context)));
            }

            private void LoadLongBinGet(CodeContext/*!*/ context) {
                _stack.append(MemoGet(context, (long)(int)ReadInt32(context)));
            }

            private void LoadLongBinPut(CodeContext/*!*/ context) {
                MemoPut((long)(int)ReadInt32(context), _stack[-1]);
            }

            private void LoadMark(CodeContext/*!*/ context) {
                _stack.append(_mark);
            }

            private void LoadNewFalse(CodeContext/*!*/ context) {
                _stack.append(ScriptingRuntimeHelpers.False);
            }

            private void LoadNewObj(CodeContext/*!*/ context) {
                PythonTuple args = _stack.pop() as PythonTuple;
                if (args == null) {
                    throw PythonOps.TypeError("expected tuple as second argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(args));
                }

                PythonType cls = _stack.pop() as PythonType;
                if (args == null) {
                    throw PythonOps.TypeError("expected new-style type as first argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(args));
                }

                PythonTypeSlot dts;
                object value;
                if (cls.TryResolveSlot(context, "__new__", out dts) &&
                    dts.TryGetValue(context, null, cls, out value)) {
                    object[] newargs = new object[args.__len__() + 1];
                    ((ICollection)args).CopyTo(newargs, 1);
                    newargs[0] = cls;

                    _stack.append(PythonOps.CallWithContext(context, value, newargs));
                    return;
                }
                
                throw PythonOps.TypeError("didn't find __new__");
            }

            private void LoadNewTrue(CodeContext/*!*/ context) {
                _stack.append(ScriptingRuntimeHelpers.True);
            }

            private void LoadNoneValue(CodeContext/*!*/ context) {
                _stack.append(null);
            }

            private void LoadObj(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                if ((markIndex + 1) >= _stack.Count) {
                    throw PythonExceptions.CreateThrowable(UnpicklingError(context), "could not find MARK");
                }

                object cls = _stack[markIndex + 1];
                if (cls is OldClass || cls is PythonType) {
                    object[] args = _stack.GetSliceAsArray(markIndex + 2, _stack.__len__());
                    PopMark(markIndex);
                    _stack.append(MakeInstance(context, cls, args));
                } else {
                    throw PythonOps.TypeError("expected class or type as first argument to INST, got {0}", DynamicHelpers.GetPythonType(cls));
                }
            }

            private void LoadPersId(CodeContext/*!*/ context) {
                if (_pers_loader == null) {
                    throw CannotUnpickle(context, "A load persistent ID instruction is present but no persistent_load function is available");
                }
                _stack.append(PythonContext.GetContext(context).CallSplat(_pers_loader, ReadLineNoNewline(context)));
            }

            private void LoadPop(CodeContext/*!*/ context) {
                _stack.pop();
            }

            private void LoadPopMark(CodeContext/*!*/ context) {
                PopMark(GetMarkIndex(context));
            }

            private void LoadProto(CodeContext/*!*/ context) {
                int proto = ReadUInt8(context);
                if (proto > 2) throw PythonOps.ValueError("unsupported pickle protocol: {0}", proto);
                // discard result
            }

            private void LoadPut(CodeContext/*!*/ context) {
                MemoPut((long)(int)ReadIntFromString(context), _stack[-1]);
            }

            private void LoadReduce(CodeContext/*!*/ context) {
                object args = _stack.pop();
                object callable = _stack.pop();
                if (args == null) {
                    _stack.append(PythonCalls.Call(context, PythonOps.GetBoundAttr(context, callable, "__basicnew__")));
                } else if (!DynamicHelpers.GetPythonType(args).Equals(TypeCache.PythonTuple)) {
                    throw PythonOps.TypeError(
                        "while executing REDUCE, expected tuple at the top of the stack, but got {0}",
                        DynamicHelpers.GetPythonType(args)
                    );
                }
                _stack.append(PythonOps.CallWithArgsTupleAndContext(context, callable, ArrayUtils.EmptyObjects, args));
            }

            private void LoadSetItem(CodeContext/*!*/ context) {
                object value = _stack.pop();
                object key = _stack.pop();
                PythonDictionary dict = _stack[-1] as PythonDictionary;
                if (dict == null) {
                    throw PythonOps.TypeError(
                        "while executing SETITEM, expected dict at stack[-3], but got {0}",
                        DynamicHelpers.GetPythonType(_stack[-1])
                    );
                }
                dict[key] = value;
            }

            private void LoadSetItems(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                PythonDictionary dict = _stack[markIndex - 1] as PythonDictionary;
                if (dict == null) {
                    throw PythonOps.TypeError(
                        "while executing SETITEMS, expected dict below last mark, but got {0}",
                        DynamicHelpers.GetPythonType(_stack[markIndex - 1])
                    );
                }
                SetItems(dict, markIndex);
            }

            private void LoadShortBinstring(CodeContext/*!*/ context) {
                _stack.append(Read(context, ReadUInt8(context)));
            }

            private void LoadString(CodeContext/*!*/ context) {
                string repr = ReadLineNoNewline(context);
                if (repr.Length < 2 ||
                    !(
                    repr[0] == '"' && repr[repr.Length - 1] == '"' ||
                    repr[0] == '\'' && repr[repr.Length - 1] == '\''
                    )
                ) {
                    throw PythonOps.ValueError("while executing STRING, expected string that starts and ends with quotes");
                }
                _stack.append(StringOps.decode(context, repr.Substring(1, repr.Length - 2), "string-escape", "strict"));
            }

            private void LoadTuple(CodeContext/*!*/ context) {
                int markIndex = GetMarkIndex(context);
                PythonTuple tuple = PythonTuple.MakeTuple(_stack.GetSliceAsArray(markIndex + 1, _stack.__len__()));
                PopMark(markIndex);
                _stack.append(tuple);
            }

            private void LoadTuple1(CodeContext/*!*/ context) {
                object item0 = _stack.pop();
                _stack.append(PythonTuple.MakeTuple(item0));
            }

            private void LoadTuple2(CodeContext/*!*/ context) {
                object item1 = _stack.pop();
                object item0 = _stack.pop();
                _stack.append(PythonTuple.MakeTuple(item0, item1));
            }

            private void LoadTuple3(CodeContext/*!*/ context) {
                object item2 = _stack.pop();
                object item1 = _stack.pop();
                object item0 = _stack.pop();
                _stack.append(PythonTuple.MakeTuple(item0, item1, item2));
            }

            private void LoadUnicode(CodeContext/*!*/ context) {
                _stack.append(StringOps.decode(context, ReadLineNoNewline(context), "raw-unicode-escape", "strict"));
            }
        }

        #endregion

        private static PythonType PicklingError(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("PicklingError");
        }

        private static PythonType PickleError(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("PickleError");
        }

        private static PythonType UnpicklingError(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("UnpicklingError");
        }

        private static PythonType BadPickleGet(CodeContext/*!*/ context) {
            return (PythonType)PythonContext.GetContext(context).GetModuleState("BadPickleGet");
        }
    }
}
