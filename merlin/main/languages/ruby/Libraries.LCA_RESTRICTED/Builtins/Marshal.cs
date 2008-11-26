/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {

    [RubyModule("Marshal")]
    public class RubyMarshal {

        #region Sites

        private const string _MarshalDumpSymbol = "marshal_dump";
        private const string _DumpSymbol = "_dump";

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>> _MarshalDumpSite =
            CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("marshal_dump"));

        private static readonly CallSite<Func<CallSite, RubyContext, object, int, object>> _DumpSite =
            CallSite<Func<CallSite, RubyContext, object, int, object>>.Create(RubySites.InstanceCallAction("_dump", 1));

        private static readonly CallSite<Func<CallSite, RubyContext, object, object, object>> _MarshalLoadSite =
            CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(RubySites.InstanceCallAction("marshal_load", 1));

        private static readonly CallSite<Func<CallSite, RubyContext, object, MutableString, object>> _LoadSite =
            CallSite<Func<CallSite, RubyContext, object, MutableString, object>>.Create(RubySites.InstanceCallAction("_load", 1));

        private static readonly CallSite<Func<CallSite, RubyContext, Proc, object, object>> _ReaderProcSite =
            CallSite<Func<CallSite, RubyContext, Proc, object, object>>.Create(RubySites.InstanceCallAction("call", 1));

        #endregion

        #region Constants

        private static readonly MutableString _positiveInfinityString = MutableString.Create("inf");
        private static readonly MutableString _negativeInfinityString = MutableString.Create("-inf");
        private static readonly MutableString _nanString = MutableString.Create("nan");

        #endregion

        #region MarshalWriter

        internal class MarshalWriter {
            private readonly BinaryWriter/*!*/ _writer;
            private readonly RubyContext/*!*/ _context;
            private int _recursionLimit;
            private readonly Dictionary<string/*!*/, int>/*!*/ _symbols;
            private readonly Dictionary<object, int>/*!*/ _objects;
#if !SILVERLIGHT
            private readonly StreamingContext/*!*/ _streamingContext;
#endif

            internal MarshalWriter(BinaryWriter/*!*/ writer, RubyContext/*!*/ context, int? limit) {
                Assert.NotNull(writer, context);

                _writer = writer;
                _context = context;
                _recursionLimit = (limit.HasValue ? limit.Value : -1);
                _symbols = new Dictionary<string, int>();
                _objects = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
#if !SILVERLIGHT
                _streamingContext = new StreamingContext(StreamingContextStates.Other, _context);
#endif
            }

            private void WritePreamble() {
                _writer.Write((byte)MAJOR_VERSION);
                _writer.Write((byte)MINOR_VERSION);
            }

            private void WriteBignumValue(BigInteger/*!*/ value) {
                char sign;
                if (value.Sign > 0) {
                    sign = '+';
                } else if (value.Sign < 0) {
                    sign = '-';
                } else {
                    sign = '0';
                }
                _writer.Write((byte)sign);
                uint[] bits = value.GetBits();
                int n = bits.Length * 2, mn = bits.Length - 1;
                bool truncate = false;
                if (bits.Length > 0 && (bits[mn] >> 16) == 0) {
                    n--;
                    truncate = true;
                }
                WriteInt32(n);
                for (int i = 0; i < bits.Length; i++) {
                    if (truncate && i == mn) {
                        _writer.Write(unchecked((ushort)(bits[i])));
                    } else {
                        _writer.Write(bits[i]);
                    }
                }
            }

            private void WriteBignum(BigInteger/*!*/ value) {
                _writer.Write((byte)'l');
                WriteBignumValue(value);
            }

            private void WriteInt32(int value) {
                if (value == 0) {
                    _writer.Write((byte)0);
                } else if (value > 0 && value < 123) {
                    _writer.Write((byte)(value + 5));
                } else if (value < 0 && value > -124) {
                    _writer.Write((sbyte)(value - 5));
                } else {
                    byte[] _buffer = new byte[5];
                    _buffer[1] = (byte)value;
                    _buffer[2] = (byte)(value >> 8);
                    _buffer[3] = (byte)(value >> 16);
                    _buffer[4] = (byte)(value >> 24);

                    int len = 4;
                    sbyte lenbyte;
                    if (value < 0) {
                        while (_buffer[len] == 0xff) {
                            len--;
                        }
                        lenbyte = (sbyte)-len;
                    } else {
                        while (_buffer[len] == 0x00) {
                            len--;
                        }
                        lenbyte = (sbyte)len;
                    }
                    _buffer[0] = (byte)lenbyte;

                    _writer.Write(_buffer, 0, len + 1);
                }
            }

            private void WriteFixnum(int value) {
                _writer.Write((byte)'i');
                WriteInt32(value);
            }

            private void WriteFloat(double value) {
                // TODO: Ruby appears to have an optimization that saves the (binary) mantissa at the end of the string
                _writer.Write((byte)'f');
                if (Double.IsInfinity(value)) {
                    if (Double.IsPositiveInfinity(value)) {
                        WriteStringValue(_positiveInfinityString);
                    } else {
                        WriteStringValue(_negativeInfinityString);
                    }
                } else if (Double.IsNaN(value)) {
                    WriteStringValue(_nanString);
                } else {
                    StringFormatter sf = new StringFormatter(_context, "%.15g", new object[] { value });
                    sf.TrailingZeroAfterWholeFloat = false;
                    WriteStringValue(sf.Format());
                }
            }

            class SubclassData {
                internal SubclassData(MarshalWriter/*!*/ marshaller, object/*!*/ obj, Type type) {
                    RubyClass libClass = marshaller._context.GetClass(type);
                    RubyClass theClass = marshaller._context.GetClassOf(obj);

                    if (libClass != theClass && !(obj is RubyStruct)) {
                        marshaller._writer.Write((byte)'C');
                        marshaller.WriteSymbol(theClass.Name);
                    }
                }
            }

            private void WriteStringValue(MutableString/*!*/ value) {
                // TODO: MutableString should be able to return a byte[] without converting the repr?
                byte[] data = MutableString.Create(value).ConvertToBytes();
                WriteInt32(data.Length);
                _writer.Write(data);
            }

            private void WriteStringValue(string/*!*/ value) {
                byte[] data = MutableString.Create(value).ConvertToBytes();
                WriteInt32(data.Length);
                _writer.Write(data);
            }

            private void WriteString(MutableString/*!*/ value) {
                SubclassData instanceWriter = new SubclassData(this, value, typeof(MutableString));
                _writer.Write((byte)'"');
                WriteStringValue(value);
            }

            private void WriteRegex(RubyRegex/*!*/ value) {
                SubclassData instanceWriter = new SubclassData(this, value, typeof(RubyRegex));
                _writer.Write((byte)'/');
                WriteStringValue(value.GetPattern());
                _writer.Write((byte)value.Options);
            }

            private void WriteArray(RubyArray/*!*/ value) {
                SubclassData instanceWriter = new SubclassData(this, value, typeof(RubyArray));
                _writer.Write((byte)'[');
                WriteInt32(value.Count);
                foreach (object obj in value) {
                    WriteAnObject(obj);
                }
            }

            private void WriteHash(Hash/*!*/ value) {
                if (value.DefaultProc != null) {
                    throw RubyExceptions.CreateTypeError("can't dump hash with default proc");
                }

                SubclassData instanceWriter = new SubclassData(this, value, typeof(Hash));
                char typeFlag = (value.DefaultValue != null) ? '}' : '{';
                _writer.Write((byte)typeFlag);
                WriteInt32(value.Count);
                foreach (KeyValuePair<object, object> pair in value) {
                    WriteAnObject(pair.Key);
                    WriteAnObject(pair.Value);
                }
                if (value.DefaultValue != null) {
                    WriteAnObject(value.DefaultValue);
                }
            }

            private void WriteSymbol(string/*!*/ value) {
                int position;
                if (_symbols.TryGetValue(value, out position)) {
                    _writer.Write((byte)';');
                    WriteInt32(position);
                } else {
                    position = _symbols.Count;
                    _symbols[value] = position;
                    _writer.Write((byte)':');
                    WriteStringValue(value);
                }
            }

            private void TestForAnonymous(RubyModule/*!*/ theModule) {
                if (theModule.Name == null) {
                    string objectType = (theModule is RubyClass) ? "class" : "module";
                    string displayName = theModule.GetDisplayName(false).ConvertToString();
                    string message = String.Format("can't dump anonymous {0} {1}", objectType, displayName);
                    throw RubyExceptions.CreateTypeError(message);
                }
            }

            private void WriteObject(object/*!*/ obj) {
                _writer.Write((byte)'o');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteSymbol(theClass.Name);

#if !SILVERLIGHT
                ISerializable serializableObj = (obj as ISerializable);
                if (serializableObj != null) {
                    SerializationInfo info = new SerializationInfo(theClass.GetUnderlyingSystemType(), new FormatterConverter());
                    serializableObj.GetObjectData(info, _streamingContext);
                    int count = info.MemberCount;
                    try {
                        // We need this attribute for CLR serialization but it's not compatible with MRI serialization
                        // Unfortunately, there's no way to test for a value without either iterating over all values
                        // or throwing an exception if it's not present
                        if (info.GetValue("#class", typeof(RubyClass)) != null) {
                            count--;
                        }
                    } catch (Exception) {
                    }
                    WriteInt32(count);
                    foreach (SerializationEntry entry in info) {
                        if (!entry.Name.Equals("#class")) {
                            WriteSymbol(entry.Name);
                            WriteAnObject(entry.Value);
                        }
                    }
                    return;
                }
#endif
            }

            private void WriteUsingDump(object/*!*/ obj) {
                _writer.Write((byte)'u');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteSymbol(theClass.Name);
                MutableString dumpResult = _DumpSite.Target(_DumpSite, _context, obj, _recursionLimit) as MutableString;
                if (dumpResult == null) {
                    throw RubyExceptions.CreateTypeError("_dump() must return string");
                }
                WriteStringValue(dumpResult);
            }

            private void WriteUsingMarshalDump(object/*!*/ obj) {
                _writer.Write((byte)'U');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteSymbol(theClass.Name);
                WriteAnObject(_MarshalDumpSite.Target(_MarshalDumpSite, _context, obj));
            }

            private void WriteClass(RubyClass/*!*/ obj) {
                _writer.Write((byte)'c');
                TestForAnonymous(obj);
                WriteStringValue(obj.Name);
            }

            private void WriteModule(RubyModule/*!*/ obj) {
                _writer.Write((byte)'m');
                TestForAnonymous(obj);
                WriteStringValue(obj.Name);
            }

            private void WriteStruct(RubyStruct/*!*/ obj) {
                SubclassData instanceWriter = new SubclassData(this, obj, typeof(RubyStruct));
                _writer.Write((byte)'S');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteSymbol(theClass.Name);
                var names = obj.GetNames();
                WriteInt32(names.Count);
                foreach (string name in names) {
                    int index = obj.GetIndex(name);
                    WriteSymbol(name);
                    WriteAnObject(obj[index]);
                }
            }

            private void WriteAnObject(object obj) {
                if (_recursionLimit == 0) {
                    throw RubyExceptions.CreateArgumentError("exceed depth limit");
                }
                if (_recursionLimit > 0) {
                    _recursionLimit--;
                }

                if (obj is int) {
                    int value = (int)obj;
                    if (value < -(1 << 30) || value >= (1 << 30)) {
                        obj = (BigInteger)value;
                    }
                }

                // TODO: use RubyUtils.IsRubyValueType?
                if (obj == null) {
                    _writer.Write((byte)'0');
                } else if (obj is bool) {
                    _writer.Write((byte)((bool)obj ? 'T' : 'F'));
                } else if (obj is int) {
                    WriteFixnum((int)obj);
                } else if (obj is SymbolId) {
                    WriteSymbol(SymbolTable.IdToString((SymbolId)obj));
                } else {
                    int objectRef;
                    if (_objects.TryGetValue(obj, out objectRef)) {
                        _writer.Write((byte)'@');
                        WriteInt32(objectRef);
                    } else {
                        objectRef = _objects.Count;
                        _objects[obj] = objectRef;

                        // TODO: replace with a table-driven implementation
                        // TODO: visibility?
                        bool implementsDump = (_context.ResolveMethod(obj, _DumpSymbol, true) != null);
                        bool implementsMarshalDump = (_context.ResolveMethod(obj, _MarshalDumpSymbol, true) != null);

                        bool writeInstanceData = false;
                        string[] instanceNames = null;

                        if (!implementsDump && !implementsMarshalDump) {
                            // Neither "_dump" nor "marshal_dump" writes instance vars separately
                            instanceNames = _context.GetInstanceVariableNames(obj);
                            if (instanceNames.Length > 0) {
                                _writer.Write((byte)'I');
                                writeInstanceData = true;
                            }
                        }

                        if (!implementsDump || implementsMarshalDump) {
                            // "_dump" doesn't write "extend" info but "marshal_dump" does
                            RubyClass theClass = _context.GetImmediateClassOf(obj);
                            if (theClass.IsSingletonClass) {
                                foreach (var mixin in theClass.Mixins) {
                                    _writer.Write((byte)'e');
                                    WriteSymbol(mixin.Name);
                                }
                            }
                        }

                        if (obj is double) {
                            WriteFloat((double)obj);
                        } else if (obj is float) {
                            WriteFloat((double)(float)obj);
                        } else if (obj is BigInteger) {
                            WriteBignum((BigInteger)obj);
                        } else if (implementsMarshalDump) {
                            WriteUsingMarshalDump(obj);
                        } else if (implementsDump) {
                            WriteUsingDump(obj);
                        } else if (obj is MutableString) {
                            WriteString((MutableString)obj);
                        } else if (obj is RubyArray) {
                            WriteArray((RubyArray)obj);
                        } else if (obj is Hash) {
                            WriteHash((Hash)obj);
                        } else if (obj is RubyRegex) {
                            WriteRegex((RubyRegex)obj);
                        } else if (obj is RubyClass) {
                            WriteClass((RubyClass)obj);
                        } else if (obj is RubyModule) {
                            WriteModule((RubyModule)obj);
                        } else if (obj is RubyStruct) {
                            WriteStruct((RubyStruct)obj);
                        } else {
                            if (writeInstanceData) {
                                // Overwrite the "I"; we always have instance data
                                _writer.BaseStream.Seek(-1, SeekOrigin.Current);
                            } else {
                                writeInstanceData = true;
                            }
                            WriteObject(obj);
                        }

                        if (writeInstanceData) {
                            WriteInt32(instanceNames.Length);
                            foreach (string name in instanceNames) {
                                object value;
                                if (!_context.TryGetInstanceVariable(obj, name, out value)) {
                                    value = null;
                                }
                                WriteSymbol(name);
                                WriteAnObject(value);
                            }
                        }
                    }
                }
                if (_recursionLimit >= 0) {
                    _recursionLimit++;
                }
            }

            internal void Dump(object obj) {
                WritePreamble();
                WriteAnObject(obj);
                _writer.BaseStream.Flush();
            }
        }

        #endregion

        #region MarshalReader

        internal class MarshalReader {
            private readonly BinaryReader/*!*/ _reader;
            private readonly RubyContext/*!*/ _context;
            private readonly Scope/*!*/ _globalScope;
            private readonly Proc _proc;
            private readonly Dictionary<int, string>/*!*/ _symbols;
            private readonly Dictionary<int, object>/*!*/ _objects;

            internal MarshalReader(BinaryReader/*!*/ reader, RubyContext/*!*/ context, Scope/*!*/ globalScope, Proc proc) {
                _reader = reader;
                _context = context;
                _globalScope = globalScope;
                _proc = proc;
                _symbols = new Dictionary<int, string>();
                _objects = new Dictionary<int, object>();
            }

            private void CheckPreamble() {
                int major = _reader.ReadByte();
                int minor = _reader.ReadByte();
                if (major != MAJOR_VERSION || minor > MINOR_VERSION) {
                    string message = String.Format(
                        "incompatible marshal file format (can't be read)\n\tformat version {0}.{1} required; {2}.{3} given",
                        MAJOR_VERSION, MINOR_VERSION, major, minor);
                    throw RubyExceptions.CreateTypeError(message);
                }
                if (minor < MINOR_VERSION) {
                    string message = String.Format(
                        "incompatible marshal file format (can be read)\n\tformat version {0}.{1} required; {2}.{3} given",
                        MAJOR_VERSION, MINOR_VERSION, major, minor);
                    _context.ReportWarning(message);
                }
            }

            private BigInteger/*!*/ ReadBignum() {
                int sign;
                int csign = _reader.ReadByte();
                if (csign == '+') {
                    sign = 1;
                } else if (csign == '-') {
                    sign = -1;
                } else {
                    sign = 0;
                }
                int words = ReadInt32();
                int dwords_lo = words / 2;
                int dwords_hi = (words + 1) / 2;
                uint[] bits = new uint[dwords_hi];
                for (int i = 0; i < dwords_lo; i++) {
                    bits[i] = _reader.ReadUInt32();
                }
                if (dwords_lo != dwords_hi) {
                    bits[dwords_lo] = _reader.ReadUInt16();
                }

                return new BigInteger(sign, bits);
            }

            private int ReadInt32() {
                sbyte first = _reader.ReadSByte();
                if (first == 0) {
                    return 0;
                } else if (first > 4) {
                    return (first - 5);
                } else if (first < -4) {
                    return (first + 5);
                } else {
                    byte fill;
                    if (first < 0) {
                        fill = 0xff;
                        first = (sbyte)-first;
                    } else {
                        fill = 0x00;
                    }
                    uint value = 0;
                    for (int i = 0; i < 4; i++) {
                        byte nextByte;
                        if (i < first) {
                            nextByte = _reader.ReadByte();
                        } else {
                            nextByte = fill;
                        }
                        value = value | (uint)(nextByte << (i * 8));
                    }
                    return (int)value;
                }
            }

            private double ReadFloat() {
                MutableString value = ReadString();
                if (value.Equals(_positiveInfinityString)) {
                    return Double.PositiveInfinity;
                }
                if (value.Equals(_negativeInfinityString)) {
                    return Double.NegativeInfinity;
                }
                if (value.Equals(_nanString)) {
                    return Double.NaN;
                }

                // TODO: MRI appears to have an optimization that saves the (binary) mantissa at the end of the string
                int pos = value.IndexOf((byte)0);
                if (pos >= 0) {
                    value.Remove(pos, value.Length - pos);
                }
                return Protocols.ConvertToFloat(_context, value);
            }

            private MutableString/*!*/ ReadString() {
                int count = ReadInt32();
                byte[] data = _reader.ReadBytes(count);
                return MutableString.CreateBinary(data);
            }

            private RubyRegex/*!*/ ReadRegex() {
                MutableString pattern = ReadString();
                int flags = _reader.ReadByte();
                return new RubyRegex(pattern, (RubyRegexOptions)flags);
            }

            private RubyArray/*!*/ ReadArray() {
                int count = ReadInt32();
                RubyArray result = new RubyArray(count);
                for (int i = 0; i < count; i++) {
                    result.Add(ReadAnObject(false));
                }
                return result;
            }

            private Hash/*!*/ ReadHash(int typeFlag) {
                int count = ReadInt32();
                Hash result = new Hash(_context);
                for (int i = 0; i < count; i++) {
                    object key = ReadAnObject(false);
                    result[key] = ReadAnObject(false);
                }
                if (typeFlag == '}') {
                    result.DefaultValue = ReadAnObject(false);
                }
                return result;
            }

            private string ReadSymbol() {
                return ReadSymbol(_reader.ReadByte());
            }

            private string ReadSymbol(int typeFlag) {
                string result = null;
                if (typeFlag == ';') {
                    int position = ReadInt32();
                    if (!_symbols.TryGetValue(position, out result)) {
                        throw RubyExceptions.CreateArgumentError("bad symbol");
                    }
                } else {
                    // Ruby appears to assume that it's a ':' in this context
                    MutableString symbolName = ReadString();
                    result = symbolName.ToString();
                    int position = _symbols.Count;
                    _symbols[position] = result;
                }
                return result;
            }

            private RubyClass/*!*/ ReadType() {
                string name = ReadSymbol();
                return (RubyClass)ReadClassOrModule('c', name);
            }

            private object/*!*/ UnmarshalNewObject() {
                return RubyUtils.CreateObject(ReadType());
            }

            private object/*!*/ ReadObject() {
                RubyClass theClass = ReadType();
                int count = ReadInt32();
                Hash attributes = new Hash(_context);
                for (int i = 0; i < count; i++) {
                    string name = ReadSymbol();
                    attributes[name] = ReadAnObject(false);
                }
                return RubyUtils.CreateObject(theClass, attributes, false);
            }

            private object/*!*/ ReadUsingLoad() {
                RubyClass theClass = ReadType();
                return _LoadSite.Target(_LoadSite, _context, theClass, ReadString());
            }

            private object/*!*/ ReadUsingMarshalLoad() {
                object obj = UnmarshalNewObject();
                _MarshalLoadSite.Target(_MarshalLoadSite, _context, obj, ReadAnObject(false));
                return obj;
            }

            private object/*!*/ ReadClassOrModule(int typeFlag) {
                string name = ReadString().ToString();
                return ReadClassOrModule(typeFlag, name);
            }

            private object/*!*/ ReadClassOrModule(int typeFlag, string/*!*/ name) {
                RubyModule result;
                if (!_context.TryGetModule(_globalScope, name, out result)) {
                    throw RubyExceptions.CreateArgumentError(String.Format("undefined class/module {0}", name));
                }

                bool isClass = (result is RubyClass);
                if (isClass && typeFlag == 'm') {
                    throw RubyExceptions.CreateArgumentError(
                        String.Format("{0} does not refer module", name));
                }
                if (!isClass && typeFlag == 'c') {
                    throw RubyExceptions.CreateArgumentError(
                        String.Format("{0} does not refer class", name));
                }
                return result;
            }

            private RubyStruct/*!*/ ReadStruct() {
                RubyStruct obj = (UnmarshalNewObject() as RubyStruct);
                if (obj == null) {
                    throw RubyExceptions.CreateArgumentError("non-initialized struct");
                }

                var names = obj.GetNames();
                int count = ReadInt32();
                if (count != names.Count) {
                    throw RubyExceptions.CreateArgumentError("struct size differs");
                }

                for (int i = 0; i < count; i++) {
                    string name = ReadSymbol();
                    if (name != names[i]) {
                        RubyClass theClass = _context.GetClassOf(obj);
                        string message = String.Format("struct {0} not compatible ({1} for {2})", theClass.Name, name, names[i]);
                        throw RubyExceptions.CreateTypeError(message);
                    }
                    obj[i] = ReadAnObject(false);
                }

                return obj;
            }

            private object/*!*/ ReadInstanced() {
                object obj = ReadAnObject(true);
                int count = ReadInt32();
                for (int i = 0; i < count; i++) {
                    string name = ReadSymbol();
                    _context.SetInstanceVariable(obj, name, ReadAnObject(false));
                }
                return obj;
            }


            private object/*!*/ ReadExtended() {
                string extensionName = ReadSymbol();
                RubyModule module = (ReadClassOrModule('m', extensionName) as RubyModule);
                object obj = ReadAnObject(true);
                ModuleOps.ExtendObject(module, obj);
                return obj;
            }

            private object/*!*/ ReadUserClass() {
                object obj = UnmarshalNewObject();
                bool loaded = false;
                int typeFlag = _reader.ReadByte();
                switch (typeFlag) {
                    case '"':
                        MutableString msc = (obj as MutableString);
                        if (msc != null) {
                            msc.Replace(0, msc.Length, ReadString());
                            loaded = true;
                        }
                        break;

                    case '/':
                        RubyRegex rsc = (obj as RubyRegex);
                        if (rsc != null) {
                            RubyRegex regex = ReadRegex();
                            rsc.Set(regex.GetPattern(), regex.Options);
                            loaded = true;
                        }
                        break;

                    case '[':
                        RubyArray asc = (obj as RubyArray);
                        if (asc != null) {
                            asc.AddRange(ReadArray());
                            loaded = true;
                        }
                        break;

                    case '{':
                    case '}':
                        Hash hsc = (obj as Hash);
                        if (hsc != null) {
                            Hash hash = ReadHash(typeFlag);
                            hsc.DefaultProc = hash.DefaultProc;
                            hsc.DefaultValue = hash.DefaultValue;
                            foreach (var pair in hash) {
                                hsc.Add(pair.Key, pair.Value);
                            }
                            loaded = true;
                        }
                        break;
                    default:
                        break;
                }
                if (!loaded) {
                    throw RubyExceptions.CreateArgumentError("incompatible base type");
                }
                return obj;
            }

            private object ReadAnObject(bool noCache) {
                object obj = null;
                bool runProc = (!noCache && _proc != null);
                int typeFlag = _reader.ReadByte();
                switch (typeFlag) {
                    case '0':
                        obj = null;
                        break;

                    case 'T':
                        obj = true;
                        break;

                    case 'F':
                        obj = false;
                        break;

                    case 'i':
                        obj = ReadInt32();
                        break;

                    case ':':
                        obj = SymbolTable.StringToId(ReadSymbol(typeFlag));
                        break;

                    case ';':
                        obj = SymbolTable.StringToId(ReadSymbol(typeFlag));
                        runProc = false;
                        break;

                    case '@':
                        obj = _objects[ReadInt32()];
                        runProc = false;
                        break;

                    default:
                        // Reserve a reference
                        int objectRef = _objects.Count;
                        if (!noCache) {
                            _objects[objectRef] = null;
                        }

                        switch (typeFlag) {
                            case 'f':
                                obj = ReadFloat();
                                break;
                            case 'l':
                                obj = ReadBignum();
                                break;
                            case '"':
                                obj = ReadString();
                                break;
                            case '/':
                                obj = ReadRegex();
                                break;
                            case '[':
                                obj = ReadArray();
                                break;
                            case '{':
                            case '}':
                                obj = ReadHash(typeFlag);
                                break;
                            case 'o':
                                obj = ReadObject();
                                break;
                            case 'u':
                                obj = ReadUsingLoad();
                                break;
                            case 'U':
                                obj = ReadUsingMarshalLoad();
                                break;
                            case 'c':
                            case 'm':
                                obj = ReadClassOrModule(typeFlag);
                                break;
                            case 'S':
                                obj = ReadStruct();
                                break;
                            case 'I':
                                obj = ReadInstanced();
                                break;
                            case 'e':
                                obj = ReadExtended();
                                break;
                            case 'C':
                                obj = ReadUserClass();
                                break;
                            default:
                                throw RubyExceptions.CreateArgumentError(String.Format("dump format error({0})", (int)typeFlag));
                        }
                        if (!noCache) {
                            _objects[objectRef] = obj;
                        }
                        break;
                }
                if (runProc) {
                    _ReaderProcSite.Target(_ReaderProcSite, _context, _proc, obj);
                }
                return obj;
            }

            internal object Load() {
                try {
                    CheckPreamble();
                    return ReadAnObject(false);
                } catch (IOException e) {
                    throw RubyExceptions.CreateArgumentError("marshal data too short", e);
                }
            }
        }

        #endregion

        #region Public Instance Methods

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Dump(RubyModule/*!*/ self, object obj) {
            return Dump(self, obj, -1);
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Dump(RubyModule/*!*/ self, object obj, int limit) {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);
            MarshalWriter dumper = new MarshalWriter(writer, self.Context, limit);
            dumper.Dump(obj);
            return MutableString.CreateBinary(buffer.ToArray());
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(RubyModule/*!*/ self, object obj, [NotNull]RubyIO/*!*/ io, [Optional]int? limit) {
            BinaryWriter writer = io.GetBinaryWriter();
            MarshalWriter dumper = new MarshalWriter(writer, self.Context, limit);
            dumper.Dump(obj);
            return io;
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(RubyModule/*!*/ self, object obj, object io, [Optional]int? limit) {
            Stream stream = null;
            if (io != null) {
                stream = new IOWrapper(self.Context, io, FileAccess.Write);
            }
            if (stream == null || !stream.CanWrite) {
                throw RubyExceptions.CreateTypeError("instance of IO needed");
            }

            BinaryWriter writer = new BinaryWriter(stream);
            MarshalWriter dumper = new MarshalWriter(writer, self.Context, limit);
            dumper.Dump(obj);
            return io;
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]MutableString/*!*/ source, [Optional]Proc proc) {
            BinaryReader reader = new BinaryReader(new MemoryStream(source.ConvertToBytes()));
            MarshalReader loader = new MarshalReader(reader, scope.RubyContext, scope.GlobalScope, proc);
            return loader.Load();
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]RubyIO/*!*/ source, [Optional]Proc proc) {
            BinaryReader reader = source.GetBinaryReader();
            MarshalReader loader = new MarshalReader(reader, scope.RubyContext, scope.GlobalScope, proc);
            return loader.Load();
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(RubyScope/*!*/ scope, RubyModule/*!*/ self, object source, [Optional]Proc proc) {
            Stream stream = null;
            if (source != null) {
                stream = new IOWrapper(self.Context, source, FileAccess.Read);
            }
            if (stream == null || !stream.CanRead) {
                throw RubyExceptions.CreateTypeError("instance of IO needed");
            }
            BinaryReader reader = new BinaryReader(stream);
            MarshalReader loader = new MarshalReader(reader, scope.RubyContext, scope.GlobalScope, proc);
            return loader.Load();
        }

        #endregion

        #region Declared Constants

        [RubyConstant]
        public const int MAJOR_VERSION = 4;

        [RubyConstant]
        public const int MINOR_VERSION = 8;

        #endregion
    }
}
