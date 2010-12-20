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
using System.Text;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.IO;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        internal sealed class StringContent : Content {
            private readonly string/*!*/ _data;

            internal StringContent(string/*!*/ data, MutableString owner) 
                : base(owner) {
                Assert.NotNull(data);
                _data = data;
            }

            internal string/*!*/ Data {
                get { return _data; }
            }

            internal BinaryContent/*!*/ SwitchToBinary() {
                var bytes = DataToBytes();
                return WrapContent(bytes, bytes.Length);
            }

            internal BinaryContent/*!*/ SwitchToBinary(int additionalCapacity) {
                // TODO:
                return SwitchToBinary();
            }

            private CharArrayContent/*!*/ SwitchToMutable() {
                return WrapContent(_data.ToCharArray(), _data.Length);
            }

            private CharArrayContent/*!*/ SwitchToMutable(int additionalCapacity) {
                // TODO:
                return SwitchToMutable();
            }

            internal byte[]/*!*/ DataToBytes() {
                return _data.Length > 0 ? _owner._encoding.StrictEncoding.GetBytes(_data) : Utils.EmptyBytes;
            }

            internal int GetDataByteCount() {
                return _owner._encoding.StrictEncoding.GetByteCount(_data);
            }

            internal void GetDataBytes(byte[]/*!*/ bytes, int start) {
                _owner._encoding.StrictEncoding.GetBytes(_data, 0, _data.Length, bytes, start);
            }

            #region UpdateCharacterFlags, CalculateHashCode, Length, Clone, Count (read-only)

            public override uint UpdateCharacterFlags(uint flags) {
                return UpdateAsciiAndSurrogatesFlags(_data, flags);
            }

            public override int CalculateHashCode() {
                return _data.GetHashCode();
            }

            public override int Count {
                get { return _data.Length; }
                set {
                    SwitchToMutable(value - _data.Length).Count = value;
                }
            }

            public override bool IsEmpty {
                get { return _data.Length == 0; }
            }

            public override int GetCharCount() {
                return _data.Length;
            }

            public override int GetCharacterCount() {
                if (!_owner.HasSurrogates()) {
                    return _data.Length;
                }
                return _data.GetCharacterCount();
            }

            public override int GetByteCount() {
                if (_data.Length == 0 || _owner.HasSingleByteCharacters && !_owner.HasSurrogates()) {
                    return _data.Length;
                }
                return SwitchToBinary().GetByteCount();
            }

            public override Content/*!*/ Clone() {
                return new StringContent(_data, _owner);
            }

            public override void TrimExcess() {
                // nop
            }

            public override int GetCapacity() {
                return _data.Length;
            }

            public override void SetCapacity(int capacity) {
                if (capacity < _data.Length) {
                    throw new InvalidOperationException();
                }
                SwitchToMutable(capacity - _data.Length);
            }

            #endregion

            #region Conversions (read-only)

            public override string/*!*/ ConvertToString() {
                // internal representation is immutable so we can pass it outside:
                return _data;
            }

            public override byte[]/*!*/ ConvertToBytes() {
                var binary = SwitchToBinary();
                return binary.GetBinarySlice(0, binary.GetByteCount());
            }

            public override string/*!*/ ToString() {
                return _data;
            }

            public override byte[]/*!*/ ToByteArray() {
                return DataToBytes();
            }

            internal override byte[]/*!*/ GetByteArray(out int count) {
                return SwitchToBinary().GetByteArray(out count);
            }

            public override Content/*!*/ SwitchToBinaryContent() {
                return SwitchToBinary();
            }

            public override Content/*!*/ SwitchToStringContent() {
                return this;
            }

            public override Content/*!*/ SwitchToMutableContent() {
                return SwitchToMutable();
            }

            public override Content/*!*/ EscapeRegularExpression() {
                StringBuilder sb = RubyRegex.EscapeToStringBuilder(_data);
                return (sb != null) ? new StringContent(sb.ToString(), _owner) : this;
            }

            public override void CheckEncoding() {
                GetDataByteCount();
            }

            public override bool ContainsInvalidCharacters() {
                return Utils.ContainsInvalidCharacters(_data.ToCharArray(), 0, _data.Length, _owner._encoding.StrictEncoding);
            }

            #endregion

            #region CompareTo (read-only)

            public override int OrdinalCompareTo(string/*!*/ str) {
                return _data.ValueCompareTo(str);
            }

            internal int OrdinalCompareTo(char[]/*!*/ chars, int count) {
                return -chars.ValueCompareTo(count, _data);
            }

            // this <=> content
            public override int OrdinalCompareTo(Content/*!*/ content) {
                return content.ReverseOrdinalCompareTo(this);
            }

            // content.bytes <=> this.chars
            public override int ReverseOrdinalCompareTo(BinaryContent/*!*/ content) {
                return SwitchToBinary().ReverseOrdinalCompareTo(content);
            }

            // content.chars <=> this.chars
            public override int ReverseOrdinalCompareTo(CharArrayContent/*!*/ content) {
                return content.OrdinalCompareTo(_data);
            }

            // content.chars <=> this.chars
            public override int ReverseOrdinalCompareTo(StringContent/*!*/ content) {
                return content.OrdinalCompareTo(_data);
            }

            #endregion

            #region Slices (read-only)

            public override char GetChar(int index) {
                return _data[index];
            }

            public override byte GetByte(int index) {
                if (index == 0 || _owner.HasSingleByteCharacters && !_owner.HasSurrogates()) {
                    if (index >= _data.Length) {
                        throw new IndexOutOfRangeException();
                    }
                    var result = _data[index];
                    if (result < 0x80 || _owner.HasByteCharacters) {
                        return (byte)_data[index];
                    }
                }
                return SwitchToBinary().GetByte(index);
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return _data.GetSlice(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return SwitchToBinary().GetBinarySlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new StringContent(_data.GetSlice(start, count), _owner);
            }

            public override CharacterEnumerator/*!*/ GetCharacters() {
                return new MutableString.StringCharacterEnumerator(_owner.Encoding, _data);
            }

            public override IEnumerable<byte>/*!*/ GetBytes() {
                if (_owner.HasByteCharacters) {
                    return Utils.EnumerateAsBytes(_data);
                } else {
                    return SwitchToBinary().GetBytes();
                }
            }

            #endregion

            #region StartsWith (read-only)

            public override bool StartsWith(char c) {
                return _data.Length != 0 && _data[0] == c;
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                count = Utils.NormalizeCount(_data.Length, start, count);
                return count > 0 ? _data.IndexOf(c, start, count) : -1;
            }

            public override int IndexOf(byte b, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.IndexOf(_data, b, start, count);
                } else {
                    return SwitchToBinary().IndexOf(b, start, count);
                }
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                if (str.Length == 0) {
                    return start;
                }

                count = Utils.NormalizeCount(_data.Length, start, count);
                return count > 0 ? _data.IndexOf(str, start, count, StringComparison.Ordinal) : -1;
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.IndexOf(_data, bytes, start, count);
                } else {
                    return SwitchToBinary().IndexOf(bytes, start, count);
                }
            }

            public override int IndexIn(Content/*!*/ str, int start, int count) {
                return str.IndexOf(_data, start, count);
            }

            #endregion

            #region LastIndexOf (read-only)

            public override int LastIndexOf(char c, int start, int count) {
                Utils.NormalizeLastIndexOfIndices(_data.Length, ref start, ref count);
                return _data.LastIndexOf(c, start, count);
            }

            public override int LastIndexOf(byte b, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.LastIndexOf(_data, b, start, count);
                } else {
                    return SwitchToBinary().LastIndexOf(b, start, count);
                }
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                Utils.NormalizeLastIndexOfIndices(_data.Length, ref start, ref count);
                return _data.LastIndexOf(str, start, count, StringComparison.Ordinal);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.LastIndexOf(_data, bytes, start, count);
                } else {
                    return SwitchToBinary().LastIndexOf(bytes, start, count);
                }
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data, start, count);
            }

            #endregion

            #region Concatenate (read-only)

            public override Content/*!*/ Concat(Content/*!*/ content) {
                return content.ConcatTo(this);
            }

            // binary + string -> binary
            public override Content/*!*/ ConcatTo(BinaryContent/*!*/ content) {
                return content.Concatenate(this);
            }

            // chars + string -> chars
            public override Content/*!*/ ConcatTo(CharArrayContent/*!*/ content) {
                return content.Concatenate(this);
            }

            // string + string -> string
            public override Content/*!*/ ConcatTo(StringContent/*!*/ content) {
                return new StringContent(content.Data + _data, null);
            }

            #endregion

            #region Append

            public override void Append(char c, int repeatCount) {
                SwitchToMutable(repeatCount).Append(c, repeatCount);
            }

            public override void Append(byte b, int repeatCount) {
                SwitchToBinary(repeatCount).Append(b, repeatCount);
            }

            public override void Append(string/*!*/ str, int start, int count) {
                SwitchToMutable(count).Append(str, start, count);
            }

            public override void Append(char[]/*!*/ chars, int start, int count) {
                SwitchToMutable(count).Append(chars, start, count);
            }

            public override void Append(byte[]/*!*/ bytes, int start, int count) {
                SwitchToBinary(count).Append(bytes, start, count);
            }

            public override void Append(Stream/*!*/ stream, int count) {
                SwitchToBinary(count).Append(stream, count);
            }

            public override void AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                SwitchToMutable().AppendFormat(provider, format, args);
            }

            // this + content[start, count]
            public override void Append(Content/*!*/ content, int start, int count) {
                content.AppendTo(this, start, count);
            }

            // content.bytes + this.chars[start, count]
            public override void AppendTo(BinaryContent/*!*/ content, int start, int count) {
                content.AppendBytes(_data, start, count);
            }

            // content.chars + this.chars[start, count]
            public override void AppendTo(CharArrayContent/*!*/ content, int start, int count) {
                content.Append(_data, start, count);
            }

            // content.chars + this.chars[start, count]
            public override void AppendTo(StringContent/*!*/ content, int start, int count) {
                content.Append(_data, start, count);
            }

            #endregion

            #region Insert

            public override void Insert(int index, char c) {
                SwitchToMutable().Insert(index, c);
            }

            public override void Insert(int index, byte b) {
                SwitchToBinary().Insert(index, b);
            }

            public override void Insert(int index, string/*!*/ str, int start, int count) {
                SwitchToMutable().Insert(index, str, start, count);
            }

            public override void Insert(int index, char[]/*!*/ chars, int start, int count) {
                SwitchToMutable().Insert(index, chars, start, count);
            }

            public override void Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                SwitchToBinary().Insert(index, bytes, start, count);
            }

            public override void InsertTo(Content/*!*/ str, int index, int start, int count) {
                str.Insert(index, _data, start, count);
            }

            public override void SetByte(int index, byte b) {
                SwitchToBinary().SetByte(index, b);
            }

            public override void SetChar(int index, char c) {
                SwitchToMutable().DataSetChar(index, c);
            }

            #endregion

            #region Remove, Write

            public override void Remove(int start, int count) {
                SwitchToMutable().Remove(start, count);
            }

            public override void Write(int offset, byte[]/*!*/ value, int start, int count) {
                SwitchToBinary().Write(offset, value, start, count);
            }

            public override void Write(int offset, byte value, int repeatCount) {
                SwitchToBinary().Write(offset, value, repeatCount);
            }

            #endregion
        }
    }
}
