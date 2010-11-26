/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using Microsoft.Scripting.Generation;
using IronRuby.Runtime.Calls;
using System.Globalization;
using System.Text;

namespace IronRuby.Builtins {

    [RubyModule("Marshal")]
    public class RubyMarshal {

        public sealed class WriterSites : RubyCallSiteStorage {
            public WriterSites(RubyContext/*!*/ context) : base(context) { }
            
            private CallSite<Func<CallSite, object, object>> _marshalDump;
            private CallSite<Func<CallSite, object, int, object>> _dump;

            public CallSite<Func<CallSite, object, object>>/*!*/ MarshalDump {
                get { return RubyUtils.GetCallSite(ref _marshalDump, Context, "marshal_dump", 0); }
            }

            public CallSite<Func<CallSite, object, int, object>>/*!*/ Dump {
                get { return RubyUtils.GetCallSite(ref _dump, Context, "_dump", 1); }
            }
        }

        public sealed class ReaderSites : RubyCallSiteStorage {
            public ReaderSites(RubyContext/*!*/ context) : base(context) { }
           
            private CallSite<Func<CallSite, object, object, object>> _marshalLoad;
            private CallSite<Func<CallSite, object, MutableString, object>> _load;
            public CallSite<Func<CallSite, Proc, object, object>> _procCall;

            public CallSite<Func<CallSite, object, object, object>>/*!*/ MarshalLoad {
                get { return RubyUtils.GetCallSite(ref _marshalLoad, Context, "marshal_load", 1); }
            }

            public CallSite<Func<CallSite, object, MutableString, object>>/*!*/ Load {
                get { return RubyUtils.GetCallSite(ref _load, Context, "_load", 1); }
            }

            public CallSite<Func<CallSite, Proc, object, object>>/*!*/ ProcCall {
                get { return RubyUtils.GetCallSite(ref _procCall, Context, "call", 1); }
            }
        }

            
        #region Constants

        private static readonly MutableString _positiveInfinityString = MutableString.CreateAscii("inf").Freeze();
        private static readonly MutableString _negativeInfinityString = MutableString.CreateAscii("-inf").Freeze();
        private static readonly MutableString _nanString = MutableString.CreateAscii("nan").Freeze();

        #endregion

        #region MarshalWriter

        internal class MarshalWriter {
            private readonly BinaryWriter/*!*/ _writer;
            private readonly RubyContext/*!*/ _context;
            private readonly WriterSites/*!*/ _sites;
            private int _recursionLimit;
            private readonly Dictionary<string/*!*/, int>/*!*/ _symbols;
            private readonly Dictionary<object, int>/*!*/ _objects;

            internal MarshalWriter(WriterSites/*!*/ sites, BinaryWriter/*!*/ writer, RubyContext/*!*/ context, int? limit) {
                Assert.NotNull(sites, writer, context);

                _sites = sites;
                _writer = writer;
                _context = context;
                _recursionLimit = (limit.HasValue ? limit.Value : -1);
                _symbols = new Dictionary<string, int>();
                _objects = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Instance);
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
                uint[] bits = value.GetWords();
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
                    _buffer[1] = (byte)(value & 0xff);
                    _buffer[2] = (byte)((value >> 8) & 0xff);
                    _buffer[3] = (byte)((value >> 16) & 0xff);
                    _buffer[4] = (byte)((value >> 24) & 0xff);

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
                    _buffer[0] = unchecked((byte)lenbyte);

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
                    StringFormatter sf = new StringFormatter(_context, "%.15g", RubyEncoding.Binary, new object[] { value });
                    sf.TrailingZeroAfterWholeFloat = false;
                    WriteStringValue(sf.Format());
                }
            }

            
            private void WriteSubclassData(object/*!*/ obj, Type type) {
                RubyClass libClass = _context.GetClass(type);
                RubyClass theClass = _context.GetClassOf(obj);

                if (libClass != theClass && !(obj is RubyStruct)) {
                    _writer.Write((byte)'C');
                    WriteModuleName(theClass);
                }
            }

            private void WriteStringValue(MutableString/*!*/ value) {
                byte[] data = value.ToByteArray();
                WriteInt32(data.Length);
                _writer.Write(data);
            }

            private void WriteModuleName(RubyModule/*!*/ module) {
                WriteSymbol(module.Name, module.Context.GetIdentifierEncoding());
            }

            private void WriteStringValue(string/*!*/ value, RubyEncoding/*!*/ encoding) {
                byte[] data = encoding.StrictEncoding.GetBytes(value);
                WriteInt32(data.Length);
                _writer.Write(data);
            }

            private void WriteString(MutableString/*!*/ value) {
                WriteSubclassData(value, typeof(MutableString));
                _writer.Write((byte)'"');
                WriteStringValue(value);
            }

            private void WriteRegex(RubyRegex/*!*/ value) {
                WriteSubclassData(value, typeof(RubyRegex));
                _writer.Write((byte)'/');
                WriteStringValue(value.Pattern);
                _writer.Write((byte)value.Options);
            }

            private void WriteArray(RubyArray/*!*/ value) {
                WriteSubclassData(value, typeof(RubyArray));
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

                WriteSubclassData(value, typeof(Hash));
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

            private void WriteSymbol(string/*!*/ value, RubyEncoding/*!*/ encoding) {
                int position;
                if (_symbols.TryGetValue(value, out position)) {
                    _writer.Write((byte)';');
                    WriteInt32(position);
                } else {
                    position = _symbols.Count;
                    _symbols[value] = position;
                    _writer.Write((byte)':');
                    WriteStringValue(value, encoding);
                }
            }

            private void TestForAnonymous(RubyModule/*!*/ theModule) {
                if (theModule.Name == null) {
                    throw RubyExceptions.CreateTypeError("can't dump anonymous {0} {1}", 
                        theModule.IsClass ? "class" : "module", 
                        theModule.GetDisplayName(_context, false)
                    );
                }
            }

            private void WriteRange(Range/*!*/ range) {
                WriteObject(range);
                WriteInt32(3);
                // Write the attributes that are implemented in C#. Any user-defined attributes (for subtypes of Range)
                // will be handled by the default handling of IRubyObject
                WriteSymbol("begin", RubyEncoding.Binary);
                WriteAnObject(range.Begin);
                WriteSymbol("end", RubyEncoding.Binary);
                WriteAnObject(range.End);
                WriteSymbol("excl", RubyEncoding.Binary);
                WriteAnObject(range.ExcludeEnd);
            }

            private void WriteObject(object/*!*/ obj) {
                _writer.Write((byte)'o');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteModuleName(theClass);
            }

            private void WriteUsingDump(object/*!*/ obj) {
                _writer.Write((byte)'u');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteModuleName(theClass);
                MutableString dumpResult = _sites.Dump.Target(_sites.Dump, obj, _recursionLimit) as MutableString;
                if (dumpResult == null) {
                    throw RubyExceptions.CreateTypeError("_dump() must return string");
                }
                WriteStringValue(dumpResult);
            }

            private void WriteUsingMarshalDump(object/*!*/ obj) {
                _writer.Write((byte)'U');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteModuleName(theClass);
                WriteAnObject(_sites.MarshalDump.Target(_sites.MarshalDump, obj));
            }

            private void WriteClass(RubyClass/*!*/ obj) {
                _writer.Write((byte)'c');
                TestForAnonymous(obj);
                WriteStringValue(obj.Name, _context.GetIdentifierEncoding());
            }

            private void WriteModule(RubyModule/*!*/ obj) {
                _writer.Write((byte)'m');
                TestForAnonymous(obj);
                WriteStringValue(obj.Name, _context.GetIdentifierEncoding());
            }

            private void WriteStruct(RubyStruct/*!*/ obj) {
                WriteSubclassData(obj, typeof(RubyStruct));
                _writer.Write((byte)'S');
                RubyClass theClass = _context.GetClassOf(obj);
                TestForAnonymous(theClass);
                WriteModuleName(theClass);
                var names = obj.GetNames();
                WriteInt32(names.Count);
                foreach (string name in names) {
                    int index = obj.GetIndex(name);
                    // TODO (encoding):
                    WriteSymbol(name, _context.GetIdentifierEncoding());
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
                RubySymbol sym;
                if (obj == null) {
                    _writer.Write((byte)'0');
                } else if (obj is bool) {
                    _writer.Write((byte)((bool)obj ? 'T' : 'F'));
                } else if (obj is int) {
                    WriteFixnum((int)obj);
                } else if ((sym = obj as RubySymbol) != null) {
                    // TODO (encoding):
                    WriteSymbol(sym.ToString(), sym.Encoding);
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
                        bool implementsDump = _context.ResolveMethod(obj, "_dump", VisibilityContext.AllVisible).Found;
                        bool implementsMarshalDump = _context.ResolveMethod(obj, "marshal_dump", VisibilityContext.AllVisible).Found;

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
                                foreach (var mixin in theClass.GetMixins()) {
                                    _writer.Write((byte)'e');
                                    WriteModuleName(mixin);
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
                        } else if (obj is Range) {
                            WriteRange((Range)obj);
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
                            var encoding = _context.GetIdentifierEncoding();
                            foreach (string name in instanceNames) {
                                object value;
                                if (!_context.TryGetInstanceVariable(obj, name, out value)) {
                                    value = null;
                                }
                                // TODO (encoding):
                                WriteSymbol(name, encoding);
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
            private sealed class Symbol {
                private string _string;
                private RubySymbol _symbol;

                public Symbol(string str, RubySymbol sym) {
                    _string = str;
                    _symbol = sym;
                }

                public string/*!*/ GetString() {
                    return _string ?? (_string = _symbol.ToString());
                }

                public RubySymbol/*!*/ GetSymbol(RubyContext/*!*/ context) {
                    return _symbol ?? (_symbol = context.EncodeIdentifier(_string));
                }
            }

            private readonly BinaryReader/*!*/ _reader;
            private readonly ReaderSites/*!*/ _sites;
            private readonly RubyGlobalScope/*!*/ _globalScope;
            private readonly Proc _proc;
            private readonly Dictionary<int, Symbol>/*!*/ _symbols; 
            private readonly Dictionary<int, object>/*!*/ _objects;

            private RubyContext/*!*/ Context {
                get { return _globalScope.Context; }
            }

            internal MarshalReader(ReaderSites/*!*/ sites, BinaryReader/*!*/ reader, 
                RubyGlobalScope/*!*/ globalScope, Proc proc) {
                _sites = sites;
                _reader = reader;
                _globalScope = globalScope;
                _proc = proc;
                _symbols = new Dictionary<int, Symbol>();
                _objects = new Dictionary<int, object>();
            }

            private void CheckPreamble() {
                int major = _reader.ReadByte();
                int minor = _reader.ReadByte();
                if (major != MAJOR_VERSION || minor > MINOR_VERSION) {
                    throw RubyExceptions.CreateTypeError(
                        "incompatible marshal file format (can't be read)\n\tformat version {0}.{1} required; {2}.{3} given",
                        MAJOR_VERSION, MINOR_VERSION, major, minor
                    );                
                }

                if (minor < MINOR_VERSION) {
                    Context.ReportWarning(
                        String.Format(CultureInfo.InvariantCulture, 
                            "incompatible marshal file format (can be read)\n\tformat version {0}.{1} required; {2}.{3} given",
                            MAJOR_VERSION, MINOR_VERSION, major, minor
                        )
                    );
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
                        uint nextByte;
                        if (i < first) {
                            nextByte = _reader.ReadByte();
                        } else {
                            nextByte = fill;
                        }
                        value |= nextByte << (i * 8);
                    }
                    return unchecked((int)value);
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
                return Protocols.ConvertStringToFloat(Context, value);
            }

            private MutableString/*!*/ ReadString() {
                // TODO: encoding
                int count = ReadInt32();
                byte[] data = _reader.ReadBytes(count);
                return MutableString.CreateBinary(data, RubyEncoding.Binary);
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
                Hash result = new Hash(Context);
                for (int i = 0; i < count; i++) {
                    object key = ReadAnObject(false);
                    result[key] = ReadAnObject(false);
                }
                if (typeFlag == '}') {
                    result.DefaultValue = ReadAnObject(false);
                }
                return result;
            }

            private string/*!*/ ReadIdentifier() {
                return ReadSymbolOrIdentifier(_reader.ReadByte(), false).GetString();
            }

            // We don't want to intern identifiers unnecessarily, so we read them as CLR strings.
            private Symbol/*!*/ ReadSymbolOrIdentifier(int typeFlag, bool symbol) {
                Symbol result;
                if (typeFlag == ';') {
                    int position = ReadInt32();
                    if (!_symbols.TryGetValue(position, out result)) {
                        throw RubyExceptions.CreateArgumentError("bad symbol");
                    }
                } else {
                    // Ruby appears to assume ':'

                    // TODO: encoding
                    int count = ReadInt32();
                    byte[] data = _reader.ReadBytes(count);
                    if (symbol) {
                        result = new Symbol(null, Context.CreateSymbol(data, RubyEncoding.Binary));
                    } else {
                        result = new Symbol(Context.GetIdentifierEncoding().Encoding.GetString(data, 0, data.Length), null);
                    }

                    _symbols[_symbols.Count] = result;
                }
                return result;
            }

            private RubyClass/*!*/ ReadType() {
                return (RubyClass)ReadClassOrModule('c', ReadIdentifier());
            }

            private object/*!*/ UnmarshalNewObject() {
                return RubyUtils.CreateObject(ReadType());
            }

            private object/*!*/ ReadObject() {
                RubyClass theClass = ReadType();
                int count = ReadInt32();
                var attributes = new Dictionary<string, object>();
                for (int i = 0; i < count; i++) {
                    string name = ReadIdentifier();
                    attributes[name] = ReadAnObject(false);
                }
                return RubyUtils.CreateObject(theClass, attributes);
            }

            private object/*!*/ ReadUsingLoad() {
                RubyClass theClass = ReadType();
                return _sites.Load.Target(_sites.Load, theClass, ReadString());
            }

            private object/*!*/ ReadUsingMarshalLoad() {
                object obj = UnmarshalNewObject();
                _sites.MarshalLoad.Target(_sites.MarshalLoad, obj, ReadAnObject(false));
                return obj;
            }

            private object/*!*/ ReadClassOrModule(int typeFlag) {
                string name = ReadString().ToString();
                return ReadClassOrModule(typeFlag, name);
            }

            private object/*!*/ ReadClassOrModule(int typeFlag, string/*!*/ name) {
                RubyModule result;
                if (!Context.TryGetModule(_globalScope, name, out result)) {
                    throw RubyExceptions.CreateArgumentError("undefined class/module {0}", name);
                }

                bool isClass = result is RubyClass;
                if (isClass && typeFlag == 'm') {
                    throw RubyExceptions.CreateArgumentError("{0} does not refer module", name);
                }
                if (!isClass && typeFlag == 'c') {
                    throw RubyExceptions.CreateArgumentError("{0} does not refer class", name);
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
                    string name = ReadIdentifier();
                    if (name != names[i]) {
                        RubyClass theClass = Context.GetClassOf(obj);
                        throw RubyExceptions.CreateTypeError("struct {0} not compatible ({1} for {2})", theClass.Name, name, names[i]);
                    }
                    obj[i] = ReadAnObject(false);
                }

                return obj;
            }

            private object/*!*/ ReadInstanced() {
                object obj = ReadAnObject(true);
                int count = ReadInt32();
                for (int i = 0; i < count; i++) {
                    string name = ReadIdentifier();
                    Context.SetInstanceVariable(obj, name, ReadAnObject(false));
                }
                return obj;
            }


            private object/*!*/ ReadExtended() {
                string extensionName = ReadIdentifier();
                RubyModule module = ReadClassOrModule('m', extensionName) as RubyModule;
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
                            rsc.Set(regex.Pattern, regex.Options);
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
                        obj = ReadSymbolOrIdentifier(typeFlag, true).GetSymbol(Context);
                        break;

                    case ';':
                        obj = ReadSymbolOrIdentifier(typeFlag, true).GetSymbol(Context);
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
                                throw RubyExceptions.CreateArgumentError("dump format error({0})", (int)typeFlag);
                        }
                        if (!noCache) {
                            _objects[objectRef] = obj;
                        }
                        break;
                }
                if (runProc) {
                    _sites.ProcCall.Target(_sites.ProcCall, _proc, obj);
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
        public static MutableString Dump(WriterSites/*!*/ sites, RubyModule/*!*/ self, object obj) {
            return Dump(sites, self, obj, -1);
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Dump(WriterSites/*!*/ sites, RubyModule/*!*/ self, object obj, int limit) {
            MemoryStream buffer = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(buffer);
            MarshalWriter dumper = new MarshalWriter(sites, writer, self.Context, limit);
            dumper.Dump(obj);
            return MutableString.CreateBinary(buffer.ToArray());
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(WriterSites/*!*/ sites, RubyModule/*!*/ self, object obj, [NotNull]RubyIO/*!*/ io, [Optional]int? limit) {
            BinaryWriter writer = io.GetBinaryWriter();
            MarshalWriter dumper = new MarshalWriter(sites, writer, self.Context, limit);
            dumper.Dump(obj);
            return io;
        }

        // TODO: Use DefaultValue attribute when it works with the binder
        [RubyMethod("dump", RubyMethodAttributes.PublicSingleton)]
        public static object Dump(WriterSites/*!*/ sites, RespondToStorage/*!*/ respondToStorage, 
            RubyModule/*!*/ self, object obj, object io, [Optional]int? limit) {
            Stream stream = null;
            if (io != null) {
                stream = RubyIOOps.CreateIOWrapper(respondToStorage, io, FileAccess.Write);
            }
            if (stream == null || !stream.CanWrite) {
                throw RubyExceptions.CreateTypeError("instance of IO needed");
            }

            BinaryWriter writer = new BinaryWriter(stream);
            MarshalWriter dumper = new MarshalWriter(sites, writer, self.Context, limit);
            dumper.Dump(obj);
            return io;
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(ReaderSites/*!*/ sites, RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]MutableString/*!*/ source, [Optional]Proc proc) {
            BinaryReader reader = new BinaryReader(new MemoryStream(source.ConvertToBytes()));
            MarshalReader loader = new MarshalReader(sites, reader, scope.GlobalScope, proc);
            return loader.Load();
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(ReaderSites/*!*/ sites, RubyScope/*!*/ scope, RubyModule/*!*/ self, [NotNull]RubyIO/*!*/ source, [Optional]Proc proc) {
            BinaryReader reader = source.GetBinaryReader();
            MarshalReader loader = new MarshalReader(sites, reader, scope.GlobalScope, proc);
            return loader.Load();
        }

        [RubyMethod("load", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("restore", RubyMethodAttributes.PublicSingleton)]
        public static object Load(ReaderSites/*!*/ sites, RespondToStorage/*!*/ respondToStorage, 
            RubyScope/*!*/ scope, RubyModule/*!*/ self, object source, [Optional]Proc proc) {

            Stream stream = null;
            if (source != null) {
                stream = RubyIOOps.CreateIOWrapper(respondToStorage, source, FileAccess.Read);
            }
            if (stream == null || !stream.CanRead) {
                throw RubyExceptions.CreateTypeError("instance of IO needed");
            }
            BinaryReader reader = new BinaryReader(stream);
            MarshalReader loader = new MarshalReader(sites, reader, scope.GlobalScope, proc);
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
