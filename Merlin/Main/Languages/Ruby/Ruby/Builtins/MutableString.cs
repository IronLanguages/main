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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Text;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using IronRuby.Compiler;

namespace IronRuby.Builtins {

    // Doesn't implement IRubyObject since that would require to hold on a RubyClass object and flow it into each factory.
    // We don't want to do so since it would make libraries complex and frozen per-appdomain singletons impossible.
    // It would also consume more memory while the string subclassing is not a common scenario.
    // To allow inheriting from String in Ruby, we need a subclass that implements IRubyObject.
    // We could genrate one the first time a String is subclassed. Having it defined explicitly (MutableString.Subclass) however
    // saves that code gen and also makes it simpler to detect whether or not we need to create a subclass of a string fast. 
    // That's a common operation String methods do.
    [Serializable]
    [DebuggerDisplay("{GetDebugValue()}", Type = "{GetDebugType()}")]
    public partial class MutableString : IEquatable<MutableString>, IComparable<MutableString>, IRubyObjectState, IDuplicable {
        private Content/*!*/ _content; 
        private Encoding/*!*/ _encoding;
        
        // The lowest bit is tainted flag.
        // The version is set to FrozenVersion when the string is frozen. FrozenVersion is the maximum version, so any update to the version 
        // triggers an OverflowException, which we convert to InvalidOperationException.
        private uint _versionAndFlags;

        private const uint IsTaintedFlag = 1;
        private const int FlagsCount = 1;
        private const uint FlagsMask = (1U << FlagsCount) - 1;
        private const uint VersionMask = ~FlagsMask;
        private const uint FrozenVersion = VersionMask;

        public static readonly MutableString/*!*/ Empty = MutableString.Create(String.Empty).Freeze();

        #region Construction

        private MutableString(Content/*!*/ content, Encoding/*!*/ encoding) {
            Assert.NotNull(content, encoding);
            content.SetOwner(this);
            _content = content;
            _encoding = encoding;
        }

        private void SetContent(Content/*!*/ content) {
            Assert.NotNull(content);
            _content = content;
        }

        // creates a copy including the taint flag, not including the version:
        protected MutableString(MutableString/*!*/ str) {
            Assert.NotNull(str);
            _content = str._content.Clone(this);
            _encoding = str._encoding;
            IsTainted = str.IsTainted;
        }

        // mutable
        private MutableString(StringBuilder/*!*/ sb, Encoding/*!*/ encoding) {
            _content = new StringBuilderContent(this, sb);
            _encoding = encoding;
        }
        
        // binary
        private MutableString(List<byte>/*!*/ bytes, Encoding/*!*/ encoding) {
            _content = new BinaryContent(this, bytes);
            _encoding = encoding;
        }

        // immutable
        private MutableString(string/*!*/ str, Encoding/*!*/ encoding) {
            _content = new StringContent(this, str);
            _encoding = encoding;
        }

        // Ruby subclasses (MutableString.Subclass):
        protected MutableString(Encoding encoding) 
            : this(new StringBuilder(), encoding) {
        }

        // Ruby allocator
        public MutableString() 
            : this(String.Empty, BinaryEncoding.Instance) {
        }

        /// <summary>
        /// Creates a blank instance of self type with no flags set.
        /// Copies encoding from the current class.
        /// </summary>
        public virtual MutableString/*!*/ CreateInstance() {
            return new MutableString(new StringBuilder(), _encoding);
        }

        /// <summary>
        /// Creates a copy of this instance, including content and taint.
        /// Doesn't copy frozen state and instance variables. 
        /// Preserves the class of the String.
        /// </summary>
        public virtual MutableString/*!*/ Clone() {
            return new MutableString(this);
        }

        /// <summary>
        /// Creates an empty copy of this instance, taint and instance variables. 
        /// </summary>
        public MutableString/*!*/ Duplicate(RubyContext/*!*/ context, bool copySingletonMembers, MutableString/*!*/ result) {
            context.CopyInstanceData(this, result, false, false, copySingletonMembers);
            return result;
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            return Duplicate(context, copySingletonMembers, CreateInstance());
        }

        /// <summary>
        /// Creates an empty textual MutableString.
        /// </summary>
        public static MutableString/*!*/ CreateMutable() {
            // TODO: encoding
            return new MutableString(new StringBuilder(), BinaryEncoding.Obsolete);
        }

        // TODO: encoding
        public static MutableString/*!*/ CreateMutable(int capacity) {
            return new MutableString(new StringBuilder(capacity), BinaryEncoding.Obsolete);
        }

        // TODO: encoding
        public static MutableString/*!*/ CreateMutable(string/*!*/ str, int capacity) {
            return new MutableString(new StringBuilder(str, capacity), BinaryEncoding.Obsolete);
        }

        // TODO: encoding
        public static MutableString/*!*/ CreateMutable(string/*!*/ str) {
            return new MutableString(new StringBuilder(str), BinaryEncoding.Obsolete);
        }

        public static MutableString/*!*/ CreateMutable(string/*!*/ str, Encoding encoding) {
            return new MutableString(new StringBuilder(str), encoding);
        }

        // TODO: encoding
        public static MutableString/*!*/ Create(string/*!*/ str) {
            return Create(str, BinaryEncoding.Obsolete);
        }

        public static MutableString/*!*/ Create(string/*!*/ str, Encoding/*!*/ encoding) {
            ContractUtils.RequiresNotNull(str, "str");
            ContractUtils.RequiresNotNull(encoding, "encoding");
            return new MutableString(str, encoding);
        }

        /// <summary>
        /// Creates an instance of MutableString with content and taint copied from a given string.
        /// </summary>
        public static MutableString/*!*/ Create(MutableString/*!*/ str) {
            ContractUtils.RequiresNotNull(str, "str");
            return new MutableString(str);
        }

        // used by RubyOps:
        internal static MutableString/*!*/ CreateInternal(MutableString str) {
            if (str != null) {
                // "...#{str}..."
                return new MutableString(str);
            } else {
                // empty literal: "...#{nil}..."
                return CreateMutable(String.Empty, BinaryEncoding.Instance);
            }
        }

        public static MutableString/*!*/ CreateBinary() {
            return new MutableString(new List<byte>(), BinaryEncoding.Instance);
        }

        public static MutableString/*!*/ CreateBinary(int capacity) {
            return new MutableString(new List<byte>(capacity), BinaryEncoding.Instance);
        }

        public static MutableString/*!*/ CreateBinary(byte[]/*!*/ bytes) {
            ContractUtils.RequiresNotNull(bytes, "bytes");
            return CreateBinary((IList<byte>)bytes, bytes.Length);
        }

        public static MutableString/*!*/ CreateBinary(IList<byte>/*!*/ bytes) {
            ContractUtils.RequiresNotNull(bytes, "bytes");
            return CreateBinary(bytes, bytes.Count);
        }

        public static MutableString/*!*/ CreateBinary(byte[]/*!*/ bytes, int capacity) {
            return CreateBinary((IList<byte>)bytes, capacity);
        }

        public static MutableString/*!*/ CreateBinary(IList<byte>/*!*/ bytes, int capacity) {
            ContractUtils.RequiresNotNull(bytes, "bytes");
            List<Byte> list = new List<byte>(capacity);
            list.AddRange(bytes);
            return new MutableString(list, BinaryEncoding.Instance);
        }

        public static MutableString[]/*!*/ MakeArray(ICollection<string>/*!*/ stringCollection) {
            ContractUtils.RequiresNotNull(stringCollection, "stringCollection");
            MutableString[] result = new MutableString[stringCollection.Count];
            int i = 0;
            foreach (var str in stringCollection) {
                result[i++] = MutableString.Create(str);
            }
            return result;
        }

        #endregion

        #region Versioning and Flags

        [CLSCompliant(false)]
        public uint Version {
            get {
                return _versionAndFlags >> FlagsCount; 
            }
        }

        private void Mutate() {
            try {
                checked { _versionAndFlags += (1 << FlagsCount); }
            } catch (OverflowException) {
                throw RubyExceptions.CreateTypeError("can't modify frozen object");
            }
        }

        public bool IsTainted {
            get {
                return (_versionAndFlags & IsTaintedFlag) != 0; 
            }
            set {
                Mutate();
                _versionAndFlags = (_versionAndFlags & ~IsTaintedFlag) | (value ? IsTaintedFlag : 0);
            }
        }

        public bool IsFrozen {
            get {
                return (_versionAndFlags & VersionMask) == FrozenVersion;
            }
        }

        void IRubyObjectState.Freeze() {
            Freeze();
        }

        public MutableString/*!*/ Freeze() {
            _versionAndFlags |= FrozenVersion;
            return this;
        }

        /// <summary>
        /// Makes this string tainted if the specified string is tainted.
        /// </summary>
        public MutableString/*!*/ TaintBy(MutableString/*!*/ str) {
            IsTainted |= str.IsTainted;
            return this;
        }

        /// <summary>
        /// Makes this string tainted if the specified string is tainted.
        /// </summary>
        public MutableString/*!*/ TaintBy(object/*!*/ obj, RubyContext/*!*/ context) {
            IsTainted |= context.IsObjectTainted(obj);
            return this;
        }

        /// <summary>
        /// Makes this string tainted if the specified string is tainted.
        /// </summary>
        public MutableString/*!*/ TaintBy(object/*!*/ obj, RubyScope/*!*/ scope) {
            IsTainted |= scope.RubyContext.IsObjectTainted(obj);
            return this;
        }

        #endregion

        #region Misc (read-only)

        public bool IsBinary {
            get {
                return _content.IsBinary;
            }
        }

        public Encoding/*!*/ Encoding {
            get { return _encoding; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _encoding = value;
            }
        }

        public override int GetHashCode() {
            return _content.GetHashCode();
        }

        internal string/*!*/ GetDebugValue() {
            string value, type;
            _content.GetDebugView(out value, out type);
            return value;
        }

        internal string/*!*/ GetDebugType() {
            string value, type;
            _content.GetDebugView(out value, out type);
            return type;
        }

        internal MutableString/*!*/ EscapeRegularExpression() {
            return new MutableString(_content.EscapeRegularExpression(), _encoding);
        }

        #endregion

        #region Conversions (read-only)

        /// <summary>
        /// Returns a copy of the content in a form of an read-only string.
        /// The internal representation of the MutableString is preserved.
        /// </summary>
        public override string/*!*/ ToString() {
            return _content.ToString();
        }

        /// <summary>
        /// Returns a copy of the content in a form of an byte array.
        /// The internal representation of the MutableString is preserved.
        /// </summary>
        public byte[]/*!*/ ToByteArray() {
            return _content.ToByteArray();
        }

        public GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
            return _content.ToRegularExpression(options);
        }

        /// <summary>
        /// Switches internal representation to textual.
        /// </summary>
        /// <returns>A copy of the internal representation unless it is read-only (string).</returns>
        public string/*!*/ ConvertToString() {
            return _content.ConvertToString();
        }

        /// <summary>
        /// Switches internal representation to binary.
        /// </summary>
        /// <returns>A copy of the internal representation.</returns>
        public byte[]/*!*/ ConvertToBytes() {
            return _content.ConvertToBytes();
        }

        // used by auto-conversions
        [Obsolete("Do not use in code")]
        public static implicit operator string(MutableString/*!*/ self) {
            return self._content.ConvertToString();
        }

        // used by auto-conversions
        [Obsolete("Do not use in code")]
        public static implicit operator byte[](MutableString/*!*/ self) {
            return self._content.ConvertToBytes();
        }

        #endregion

        #region Comparisons (read-only)

        public static bool operator ==(MutableString self, char other) {
            return Equals(self, other);
        }

        public static bool operator !=(MutableString self, char other) {
            return !Equals(self, other);
        }

        public static bool operator ==(MutableString self, MutableString other) {
            return Equals(self, other);
        }

        public static bool operator !=(MutableString self, MutableString other) {
            return !Equals(self, other);
        }

        private static bool Equals(MutableString self, MutableString other) {
            if (ReferenceEquals(self, other)) return true;
            if (ReferenceEquals(self, null)) return false;
            if (ReferenceEquals(other, null)) return false;
            return other._content.ReverseCompareTo(self._content) == 0;
        }

        private static bool Equals(MutableString self, char other) {
            if (ReferenceEquals(self, null)) return false;
            return self.GetCharCount() == 1 && self.GetChar(0) == other;
        }

        public override bool Equals(object other) {
            return Equals(other as MutableString);
        }

        public bool Equals(MutableString other) {
            return CompareTo(other) == 0;
        }

        public int CompareTo(MutableString other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(other, null)) return 1;
            return other._content.ReverseCompareTo(_content);
        }

        #endregion

        #region Length (read-only)

        public static bool IsNullOrEmpty(MutableString/*!*/ str) {
            return ReferenceEquals(str, null) || str.IsEmpty;
        }

        public bool IsEmpty { 
            get { return _content.IsEmpty; } 
        }

        // TODO: replace by CharCount, ByteCount
        //[Obsolete("Use GetCharCount(), GetByteCount()")]
        public int Length {
            get { return _content.Length; }
        }
        
        public int GetLength() {
            return _content.Length;
        }

        public int GetCharCount() {
            return _content.GetCharCount();
        }

        public int GetByteCount() {
            return _content.GetByteCount();
        }

        #endregion

        #region StartsWith, EndsWith (read-only)

        public bool EndsWith(char value) {
            return GetLastChar() == value;
        }
        
        public bool EndsWith(string/*!*/ value) {
            // TODO:
            return _content.ConvertToString().EndsWith(value);
        }
        
        #endregion

        #region Slices (read-only)

        // converts the result, not the string
        public char PeekChar(int index) {
            return _content.PeekChar(index);
        }

        // converts the result, not the string
        public byte PeekByte(int index) {
            return _content.PeekByte(index);
        }

        // converts the string representation to text if not already
        public char GetChar(int index) {
            return _content.GetChar(index);
        }

        // converts the string representation to binary if not already
        public byte GetByte(int index) {
            return _content.GetByte(index);
        }

        // returns -1 if the string is empty
        public int GetLastChar() {
            return (_content.IsEmpty) ? -1 : _content.GetChar(_content.Length - 1); 
        }

        // returns -1 if the string is empty
        public int GetFirstChar() {
            return (_content.IsEmpty) ? -1 : _content.GetChar(0);
        }

        /// <summary>
        /// Returns a new mutable string containing a substring of the current one.
        /// </summary>
        public MutableString/*!*/ GetSlice(int start) {
            return GetSlice(start, _content.Length - start);
        }

        public MutableString/*!*/ GetSlice(int start, int count) {
            //RequiresArrayRange(start, count);
            return new MutableString(_content.GetSlice(start, count), _encoding);
        }

        public string/*!*/ GetStringSlice(int start) {
            return GetStringSlice(start, _content.GetCharCount() - start);
        }

        public string/*!*/ GetStringSlice(int start, int count) {
            //RequiresArrayRange(start, count);
            return _content.GetStringSlice(start, count);
        }

        public byte[]/*!*/ GetBinarySlice(int start) {
            return GetBinarySlice(start, _content.GetByteCount() - start);
        }

        public byte[]/*!*/ GetBinarySlice(int start, int count) {
            //RequiresArrayRange(start, count);
            return _content.GetBinarySlice(start, count);
        }

        #endregion

        #region Split (read-only)

        // TODO: binary ops, ...
        public MutableString[]/*!*/ Split(char[]/*!*/ separators, int maxComponents, StringSplitOptions options) {
            // TODO:
            return MakeArray(StringUtils.Split(_content.ConvertToString(), separators, maxComponents, options));
        }
        
        #endregion

        #region IndexOf (read-only)

        public int IndexOf(char value) {
            return IndexOf(value, 0);
        }

        public int IndexOf(char value, int start) {
            return IndexOf(value, start, _content.GetCharCount() - start);
        }

        public int IndexOf(char value, int start, int count) {
            //RequiresArrayRange(start, count);
            return _content.IndexOf(value, start, count);
        }

        public int IndexOf(byte value) {
            return IndexOf(value, 0);
        }

        public int IndexOf(byte value, int start) {
            return IndexOf(value, start, _content.GetByteCount() - start);
        }

        public int IndexOf(byte value, int start, int count) {
            //RequiresArrayRange(start, count);
            return _content.IndexOf(value, start, count);
        }

        public int IndexOf(string/*!*/ value) {
            return IndexOf(value, 0);
        }

        public int IndexOf(string/*!*/ value, int start) {
            return IndexOf(value, start, _content.GetCharCount() - start);
        }

        public int IndexOf(string/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresArrayRange(start, count);

            return _content.IndexOf(value, start, count);
        }

        public int IndexOf(byte[]/*!*/ value) {
            return IndexOf(value, 0);
        }

        public int IndexOf(byte[]/*!*/ value, int start) {
            return IndexOf(value, start, _content.GetByteCount() - start);
        }

        public int IndexOf(byte[]/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresArrayRange(start, count);

            return _content.IndexOf(value, start, count);
        }

        public int IndexOf(MutableString/*!*/ value) {
            return IndexOf(value, 0);
        }

        public int IndexOf(MutableString/*!*/ value, int start) {
            return IndexOf(value, start, _content.Length - start);
        }

        public int IndexOf(MutableString/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresArrayRange(start, count);

            return value._content.IndexIn(_content, start, count);
        }

        #endregion

        #region LastIndexOf (read-only)

        public int LastIndexOf(char value) {
            int length = _content.GetCharCount();
            return LastIndexOf(value, length - 1, length);
        }

        public int LastIndexOf(char value, int start) {
            return LastIndexOf(value, start, start + 1);
        }

        public int LastIndexOf(char value, int start, int count) {
            //RequiresReverseArrayRange(start, count);
            return _content.LastIndexOf(value, start, count);
        }

        public int LastIndexOf(byte value) {
            int length = _content.GetByteCount();
            return LastIndexOf(value, length - 1, length);
        }

        public int LastIndexOf(byte value, int start) {
            return LastIndexOf(value, start, start + 1);
        }

        public int LastIndexOf(byte value, int start, int count) {
            //RequiresReverseArrayRange(start, count);
            return _content.LastIndexOf(value, start, count);
        }

        public int LastIndexOf(string/*!*/ value) {
            int length = _content.GetCharCount();
            return LastIndexOf(value, length - 1, length);
        }

        public int LastIndexOf(string/*!*/ value, int start) {
            return LastIndexOf(value, start, start + 1);
        }

        public int LastIndexOf(string/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresReverseArrayRange(start, count);

            return _content.LastIndexOf(value, start, count);
        }

        public int LastIndexOf(byte[]/*!*/ value) {
            int length = _content.GetByteCount();
            return LastIndexOf(value, length - 1, length);
        }

        public int LastIndexOf(byte[]/*!*/ value, int start) {
            return LastIndexOf(value, start, start + 1);
        }

        public int LastIndexOf(byte[]/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresReverseArrayRange(start, count);

            return _content.LastIndexOf(value, start, count);
        }

        public int LastIndexOf(MutableString/*!*/ value) {
            int length = _content.Length;
            return LastIndexOf(value, length - 1, length);
        }

        public int LastIndexOf(MutableString/*!*/ value, int start) {
            return LastIndexOf(value, start, start + 1);
        }

        public int LastIndexOf(MutableString/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            //RequiresReverseArrayRange(start, count);

            return value._content.LastIndexIn(_content, start, count);
        }

        #endregion

        #region Append

        public MutableString/*!*/ Append(char value) {
            Mutate();
            _content.Append(value, 1);
            return this;
        }

        public MutableString/*!*/ Append(char value, int repeatCount) {
            Mutate();
            _content.Append(value, repeatCount);
            return this;
        }

        public MutableString/*!*/ Append(byte value) {
            Mutate();
            _content.Append(value, 1);
            return this;
        }

        public MutableString/*!*/ Append(byte value, int repeatCount) {
            Mutate();
            _content.Append(value, repeatCount);
            return this;
        }

        public MutableString/*!*/ Append(StringBuilder value) {
            if (value != null) {
                Mutate();
                _content.Append(value.ToString(), 0, value.Length);
            }
            return this;
        }

        public MutableString/*!*/ Append(string value) {
            if (value != null) {
                Mutate();
                _content.Append(value, 0, value.Length);
            }
            return this;
        }

        public MutableString/*!*/ Append(string/*!*/ value, int startIndex, int charCount) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresArrayRange(value, startIndex, charCount, "startIndex", "charCount");
            Mutate();

            _content.Append(value, startIndex, charCount);
            return this;
        }

        public MutableString/*!*/ Append(byte[] value) {
            if (value != null) {
                Mutate();
                _content.Append(value, 0, value.Length);
            }
            return this;
        }

        public MutableString/*!*/ Append(byte[]/*!*/ value, int start, int count) {
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresArrayRange(value, start, count, "startIndex", "count");

            Mutate();
            _content.Append(value, start, count);
            return this;
        }

        public MutableString/*!*/ Append(MutableString/*!*/ value) {
            if (value != null) {
                Mutate();
                value._content.AppendTo(_content, 0, value._content.Length);
            }
            return this;
        }

        public MutableString/*!*/ Append(MutableString/*!*/ str, int start, int count) {
            ContractUtils.RequiresNotNull(str, "str");
            //RequiresArrayRange(start, count);

            Mutate();
            str._content.AppendTo(_content, start, count);
            return this;
        }

        public MutableString/*!*/ AppendFormat(string/*!*/ format, params object[] args) {
            Mutate();
            return AppendFormat(null, format, args);
        }

        public MutableString/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, params object[] args) {
            ContractUtils.RequiresNotNull(format, "format");
            Mutate();

            _content.AppendFormat(provider, format, args);
            return this;
        }

        #endregion

        #region Insert

        public MutableString/*!*/ SetChar(int index, char c) {
            Mutate();
            _content.SetItem(index, c);
            return this;
        }

        public MutableString/*!*/ SetByte(int index, byte b) {
            Mutate();
            _content.SetItem(index, b);
            return this;
        }

        public MutableString/*!*/ Insert(int index, char c) {
            //RequiresArrayInsertIndex(index);
            Mutate();
            _content.Insert(index, c);
            return this;
        }

        public MutableString/*!*/ Insert(int index, byte b) {
            //RequiresArrayInsertIndex(index);
            Mutate();
            _content.Insert(index, b);
            return this;
        }

        public MutableString/*!*/ Insert(int index, string value) {
            //RequiresArrayInsertIndex(index);
            if (value != null) {
                Mutate();
                _content.Insert(index, value, 0, value.Length);
            }
            return this;
        }

        public MutableString/*!*/ Insert(int index, string/*!*/ value, int start, int count) {
            //RequiresArrayInsertIndex(index);
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresArrayRange(value, start, count, "start", "count");

            Mutate();
            _content.Insert(index, value, start, count);
            return this;
        }

        public MutableString/*!*/ Insert(int index, byte[] value) {
            //RequiresArrayInsertIndex(index);
            if (value != null) {
                Mutate();
                _content.Insert(index, value, 0, value.Length);
            }
            return this;
        }

        public MutableString/*!*/ Insert(int index, byte[]/*!*/ value, int start, int count) {
            //RequiresArrayInsertIndex(index);
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresArrayRange(value, start, count, "start", "count");

            Mutate();
            _content.Insert(index, value, start, count);
            return this;
        }

        public MutableString/*!*/ Insert(int index, MutableString value) {
            //RequiresArrayInsertIndex(index);
            if (value != null) {
                Mutate();
                value._content.InsertTo(_content, index, 0, value._content.Length);
            }
            return this;
        }

        public MutableString/*!*/ Insert(int index, MutableString/*!*/ value, int start, int count) {
            //RequiresArrayInsertIndex(index);
            ContractUtils.RequiresNotNull(value, "value");
            //value.RequiresArrayRange(start, count);

            Mutate();
            value._content.InsertTo(_content, index, start, count);
            return this;
        }

        #endregion

        #region Reverse

        public MutableString/*!*/ Reverse() {
            Mutate();

            // TODO:
            if (IsBinary) {
                int length = _content.Length;
                for (int i = 0; i < length / 2; i++) {
                    byte a = GetByte(i);
                    byte b = GetByte(length - i - 1);
                    SetByte(i, b);
                    SetByte(length - i - 1, a);
                }
            } else {
                int length = _content.Length;
                for (int i = 0; i < length / 2; i++) {
                    char a = GetChar(i);
                    char b = GetChar(length - i - 1);
                    SetChar(i, b);
                    SetChar(length - i - 1, a);
                }
            }

            return this;
        }

        #endregion

        #region Replace, Remove, Trim, Clear

        public MutableString/*!*/ Replace(int start, int count, MutableString value) {
            //RequiresArrayRange(start, count);

            // TODO:
            Mutate();
            return Remove(start, count).Insert(start, value);
        }

        public MutableString/*!*/ Remove(int start, int count) {
            //RequiresArrayRange(start, count);
            Mutate();
            _content.Remove(start, count);
            return this;
        }

        public MutableString/*!*/ Trim(int start, int count) {
            Mutate();
            _content = _content.GetSlice(start, count);
            return this;
        }

        public MutableString/*!*/ Clear() {
            Mutate();
            _content = _content.GetSlice(0, 0);
            return this;
        }


        #endregion
               

#if OBSOLETE
        #region Utils

        /// <summary>
        /// Requires the range [offset, offset + count] to be a subset of [0, dataLength].
        /// </summary>
        /// <exception cref="ArgumentNullException">String is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        private void RequiresArrayRange(int start, int count, int dataLength) {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (start < 0 || dataLength - start < count) throw new ArgumentOutOfRangeException("start");
        }

        /// <summary>
        /// Requires the range [offset - count, offset] to be a subset of [0, dataLength].
        /// </summary>
        /// <exception cref="ArgumentNullException">String is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Offset or count are out of range.</exception>
        private void //RequiresReverseArrayRange(int start, int count, int dataLength) {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (start < count - 1 || start >= dataLength) throw new ArgumentOutOfRangeException("start");
        }

        /// <summary>
        /// Requires the specified index to point inside the array or at the end.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Index is outside the array.</exception>
        private void RequiresArrayInsertIndex(int index, int dataLength) {
            if (index < 0 || index > dataLength) throw new ArgumentOutOfRangeException("index");
        }

        #endregion
#endif        
    }
}
