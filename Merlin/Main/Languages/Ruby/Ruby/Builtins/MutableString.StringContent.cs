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
using System.Text;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private class StringContent : Content {
            private readonly string/*!*/ _data;

            public StringContent(MutableString/*!*/ owner, string/*!*/ data)
                : base(owner) {
                Assert.NotNull(data);
                _data = data;
            }

            protected virtual BinaryContent/*!*/ SwitchToBinary() {
                return WrapContent(DataToBytes());
            }

            private StringBuilderContent/*!*/ SwitchToMutable() {
                return WrapContent(new StringBuilder(_data));
            }

            #region Data Operations

            protected byte[] DataToBytes() {
                return _data.Length > 0 ? _owner._encoding.GetBytes(_data) : IronRuby.Runtime.Utils.EmptyBytes;
            }

            // TODO:
            public int DataCompareTo(string/*!*/ other) {
                int min = _data.Length, defaultResult;
                if (min < other.Length) {
                    defaultResult = -1;
                } else if (min > other.Length) {
                    min = other.Length;
                    defaultResult = +1;
                } else {
                    defaultResult = 0;
                }

                for (int i = 0; i < min; i++) {
                    if (_data[i] != other[i]) {
                        return (int)_data[i] - other[i];
                    }
                }

                return defaultResult;
            }

            #endregion

            #region GetHashCode, Length, Clone (read-only)

            public override int GetHashCode() {
                // TODO: Duping our hash function from the StringBuilder case to ensure that we return the same hash value for "get".hash vs. "GET".downcase.hash
                int result = 5381;
                for (int i = 0; i < _data.Length; i++) {
                    result = unchecked(((result << 5) + result) ^ _data[i]);
                }
                return result;
            }

            public override bool IsBinary {
                get { return false; }
            }

            public override int Length {
                get { return _data.Length; }
            }

            public override bool IsEmpty {
                get { return _data.Length == 0; }
            }

            public override int GetCharCount() {
                return _data.Length;
            }

            public override int GetByteCount() {
                return (_data.Length > 0) ? SwitchToBinary().DataLength : 0;
            }

            public override void GetDebugView(out string/*!*/ value, out string/*!*/ type) {
                value = _data;
                type = "String (immutable)";
            }

            public override Content/*!*/ Clone(MutableString/*!*/ newOwner) {
                return new StringContent(newOwner, _data);
            }

            #endregion

            #region Conversions (read-only)

            // internal representation is immutable so we can pass it outside:
            public override string/*!*/ ConvertToString() {
                return _data;
            }

            public override byte[]/*!*/ ConvertToBytes() {
                var binary = SwitchToBinary();
                return binary.DataGetSlice(0, binary.DataLength);
            }

            public override string/*!*/ ToString() {
                return _data;
            }

            public override byte[]/*!*/ ToByteArray() {
                return DataToBytes();
            }

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                return new StringRegex(_data, options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                StringBuilder sb = StringRegex.EscapeToStringBuilder(_data);
                return (sb != null) ? new StringContent(_owner, sb.ToString()) : this;
            }

            #endregion

            #region CompareTo (read-only)

            public override int CompareTo(string/*!*/ str) {
                return DataCompareTo(str);
            }

            public override int CompareTo(byte[]/*!*/ bytes) {
                return SwitchToBinary().CompareTo(bytes);
            }

            public override int ReverseCompareTo(Content/*!*/ str) {
                return str.CompareTo(_data);
            }

            #endregion

            #region Slices (read-only)

            public override char GetChar(int index) {
                return _data[index];
            }

            public override byte GetByte(int index) {
                return SwitchToBinary().DataGetByte(index);
            }

            public override char PeekChar(int index) {
                return _data[index];
            }

            public override byte PeekByte(int index) {
                byte[] bytes = new byte[_owner._encoding.GetMaxByteCount(1)];
                _owner._encoding.GetBytes(_data, index, 1, bytes, 0);
                return bytes[0];
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return _data.Substring(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return SwitchToBinary().DataGetSlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new StringContent(_owner, _data.Substring(start, count));
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                return _data.IndexOf(c, start, count);
            }

            public override int IndexOf(byte b, int start, int count) {
                return SwitchToBinary().DataIndexOf(b, start, count);
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                return _data.IndexOf(str, start, count);
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataIndexOf(bytes, start, count);
            }

            public override int IndexIn(Content/*!*/ str, int start, int count) {
                return str.IndexOf(_data, start, count);
            }

            #endregion

            #region LastIndexOf (read-only)

            public override int LastIndexOf(char c, int start, int count) {
                return _data.LastIndexOf(c, start, count);
            }

            public override int LastIndexOf(byte b, int start, int count) {
                return SwitchToBinary().DataLastIndexOf(b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                return _data.LastIndexOf(str, start, count);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataLastIndexOf(bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data, start, count);
            }

            #endregion

            #region Append

            public override Content/*!*/ Append(char c, int repeatCount) {
                return SwitchToMutable().DataAppend(c, repeatCount);
            }

            public override Content/*!*/ Append(byte b, int repeatCount) {
                return SwitchToBinary().DataAppend(b, repeatCount);
            }

            public override Content/*!*/ Append(string/*!*/ str, int start, int count) {
                return SwitchToMutable().DataAppend(str, start, count);
            }

            public override Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataAppend(bytes, start, count);
            }

            public override Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                return SwitchToMutable().DataAppendFormat(provider, format, args);
            }

            public override Content/*!*/ AppendTo(Content/*!*/ str, int start, int count) {
                return str.Append(_data, start, count);
            }

            #endregion

            #region Insert

            public override Content/*!*/ Insert(int index, char c) {
                return SwitchToMutable().DataInsert(index, c);
            }

            public override Content/*!*/ Insert(int index, byte b) {
                return SwitchToBinary().DataInsert(index, b);
            }

            public override Content/*!*/ Insert(int index, string/*!*/ str, int start, int count) {
                return SwitchToMutable().DataInsert(index, str, start, count);
            }

            public override Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataInsert(index, bytes, start, count);
            }

            public override Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count) {
                return str.Insert(index, _data, start, count);
            }

            public override Content/*!*/ SetItem(int index, byte b) {
                return SwitchToBinary().DataSetByte(index, b);
            }

            public override Content/*!*/ SetItem(int index, char c) {
                return SwitchToMutable().DataSetChar(index, c);
            }

            #endregion

            #region Remove

            public override Content/*!*/ Remove(int start, int count) {
                return SwitchToMutable().DataRemove(start, count);
            }

            #endregion
        }
    }
}
