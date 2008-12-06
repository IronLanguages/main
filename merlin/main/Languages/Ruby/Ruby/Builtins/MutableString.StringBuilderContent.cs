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
using Microsoft.Scripting.Utils;
using System.Text;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private sealed class StringBuilderContent : Content {
            private readonly StringBuilder/*!*/ _data;

            public StringBuilderContent(MutableString/*!*/ owner, StringBuilder/*!*/ data)
                : base(owner) {
                Assert.NotNull(data);
                _data = data;
            }

            private BinaryContent/*!*/ SwitchToBinary() {
                return WrapContent(DataToBytes());
            }

            #region Data Operations

            public byte[] DataToBytes() {
                return _data.Length > 0 ? _owner._encoding.GetBytes(_data.ToString()) : IronRuby.Runtime.Utils.Array.EmptyBytes;
            }

            public char DataGetChar(int index) {
                return _data[index];
            }

            public StringBuilderContent/*!*/ DataSetChar(int index, char c) {
                _data[index] = c;
                return this;
            }

            public string/*!*/ DataGetSlice(int start, int count) {
                return _data.ToString(start, count);
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

            public int DataIndexOf(char c, int start, int count) {
                for (int i = start; i < start + count; i++) {
                    if (_data[i] == c) {
                        return i;
                    }
                }
                return -1;
            }

            public int DataLastIndexOf(char c, int start, int count) {
                for (int i = start; i > start - count; i--) {
                    if (_data[i] == c) {
                        return i;
                    }
                }
                return -1;
            }

            public int DataIndexOf(string str, int start, int count) {
                // TODO: is there a better way?
                return _data.ToString().IndexOf(str, start, count);
            }

            public int DataLastIndexOf(string str, int start, int count) {
                // TODO: is there a better way?
                return _data.ToString().LastIndexOf(str, start, count);
            }

            public StringBuilderContent/*!*/ DataAppend(char c, int repeatCount) {
                _data.Append(c, repeatCount);
                return this;
            }

            public StringBuilderContent/*!*/ DataAppend(string/*!*/ str, int start, int count) {
                _data.Append(str, start, count);
                return this;
            }

            public StringBuilderContent/*!*/ DataAppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                _data.AppendFormat(provider, format, args);
                return this;
            }

            public StringBuilderContent/*!*/ DataInsert(int index, char c) {
                _data.Insert(index, new String(c, 1));
                return this;
            }

            public StringBuilderContent/*!*/ DataInsert(int index, string/*!*/ str, int start, int count) {
                _data.Insert(index, str.ToCharArray(), start, count);
                return this;
            }

            public StringBuilderContent/*!*/ DataInsert(int index, char c, int repeatCount) {
                // TODO:
                _data.Insert(index, c.ToString(), repeatCount);
                return this;
            }

            public StringBuilderContent/*!*/ DataRemove(int start, int count) {
                _data.Remove(start, count);
                return this;
            }

            #endregion

            #region GetHashCode, Length, Clone (read-only)

            public override int GetHashCode() {
                int result = 5381;
                for (int i = 0; i < _data.Length; i++) {
                    result = unchecked(((result << 5) + result) ^ _data[i]);
                }
                return result;
            }

            public override bool IsBinary {
                get { return false; }
            }

            public int DataLength {
                get { return _data.Length; }
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

            public override Content/*!*/ Clone(MutableString/*!*/ newOwner) {
                return new StringBuilderContent(newOwner, new StringBuilder(_data.ToString()));
            }

            public override void GetDebugView(out string/*!*/ value, out string/*!*/ type) {
                value = _data.ToString();
                type = "String (mutable)";
            }

            #endregion

            #region Conversions (read-only)

            public override string/*!*/ ConvertToString() {
                return _data.ToString();
            }

            public override byte[]/*!*/ ConvertToBytes() {
                var binary = SwitchToBinary();
                return binary.DataGetSlice(0, binary.DataLength);
            }

            public override string/*!*/ ToString() {
                return _data.ToString();
            }

            public override byte[]/*!*/ ToByteArray() {
                return DataToBytes();
            }

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                return new StringRegex(_data.ToString(), options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                StringBuilder sb = StringRegex.EscapeToStringBuilder(_data.ToString());
                return (sb != null) ? new StringBuilderContent(_owner, sb) : this;
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
                return str.CompareTo(_data.ToString());
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
                _owner._encoding.GetBytes(_data.ToString(), index, 1, bytes, 0);
                return bytes[0];
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return _data.ToString(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return SwitchToBinary().DataGetSlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new StringBuilderContent(_owner, new StringBuilder(_data.ToString(start, count)));
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                return DataIndexOf(c, start, count);
            }

            public override int IndexOf(byte b, int start, int count) {
                return SwitchToBinary().DataIndexOf(b, start, count);
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                return DataIndexOf(str, start, count);
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataIndexOf(bytes, start, count);
            }

            public override int IndexIn(Content/*!*/ str, int start, int count) {
                return str.IndexOf(_data.ToString(), start, count);
            }

            #endregion

            #region LastIndexOf (read-only)

            public override int LastIndexOf(char c, int start, int count) {
                return DataLastIndexOf(c, start, count);
            }

            public override int LastIndexOf(byte b, int start, int count) {
                return SwitchToBinary().DataLastIndexOf(b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                return DataLastIndexOf(str, start, count);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataLastIndexOf(bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data.ToString(), start, count);
            }

            #endregion

            #region Append

            public override Content/*!*/ Append(char c, int repeatCount) {
                return DataAppend(c, repeatCount);
            }

            public override Content/*!*/ Append(byte b, int repeatCount) {
                return SwitchToBinary().DataAppend(b, repeatCount);
            }

            public override Content/*!*/ Append(string/*!*/ str, int start, int count) {
                return DataAppend(str, start, count);
            }

            public override Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataAppend(bytes, start, count);
            }

            public override Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                return DataAppendFormat(provider, format, args);
            }

            public override Content/*!*/ AppendTo(Content/*!*/ str, int start, int count) {
                return str.Append(_data.ToString(), start, count);
            }

            #endregion

            #region Insert

            public override Content/*!*/ Insert(int index, char c) {
                return DataInsert(index, c);
            }

            public override Content/*!*/ Insert(int index, byte b) {
                return SwitchToBinary().DataInsert(index, b);
            }

            public override Content/*!*/ Insert(int index, string/*!*/ str, int start, int count) {
                return DataInsert(index, str, start, count);
            }

            public override Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().DataInsert(index, bytes, start, count);
            }

            public override Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count) {
                return str.Insert(index, _data.ToString(), start, count);
            }

            public override Content/*!*/ SetItem(int index, byte b) {
                return SwitchToBinary().DataSetByte(index, b);
            }

            public override Content/*!*/ SetItem(int index, char c) {
                return DataSetChar(index, c);
            }

            #endregion

            #region Remove

            public override Content/*!*/ Remove(int start, int count) {
                return DataRemove(start, count);
            }

            #endregion
        }
    }
}
