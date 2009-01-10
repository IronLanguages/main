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
using IronRuby.Compiler;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private class BinaryContent : Content {
            // TODO: replace by resizable byte[]
            // List<byte> is not efficient
            private readonly List<byte> _data;

            internal BinaryContent(MutableString/*!*/ owner, List<byte>/*!*/ data)
                : base(owner) {
                Assert.NotNull(data);
                _data = data;
            }

            //private Content/*!*/ SwitchToText() {
            //    return SwitchToStringBuilder(0);
            //}

            private StringBuilderContent/*!*/ SwitchToStringBuilder() {
                return SwitchToStringBuilder(0);
            }

            private StringBuilderContent/*!*/ SwitchToStringBuilder(int additionalCapacity) {
                string data = DataToString();
                return WrapContent(new StringBuilder(data, data.Length + additionalCapacity));
            }

            #region Data Operations

            private string/*!*/ DataToString() {
                byte[] bytes = _data.ToArray();
                return _owner._encoding.GetString(bytes, 0, bytes.Length);
            }

            public int DataCompareTo(byte[]/*!*/ other) {
                int min = _data.Count, defaultResult;
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

            public byte DataGetByte(int index) {
                return _data[index];
            }

            public BinaryContent/*!*/ DataSetByte(int index, byte b) {
                _data[index] = b;
                return this;
            }

            public byte[]/*!*/ DataGetSlice(int start, int count) {
                byte[] range = new byte[count];
                Buffer.BlockCopy(_data.ToArray(), start, range, 0, count);
                return range; 
            }

            public BinaryContent/*!*/ DataAppend(byte b, int repeatCount) {
                _data.Add(b);
                return this;
            }

            public BinaryContent/*!*/ DataAppend(byte[]/*!*/ bytes, int start, int count) {
                for (int i = 0; i < count; i++) {
                    _data.Add(bytes[start + i]);
                }
                return this;
            }

            public BinaryContent/*!*/ DataInsert(int index, byte b) {
                _data.Insert(index, b);
                return this;
            }

            public BinaryContent/*!*/ DataInsert(int index, byte[]/*!*/ bytes, int start, int count) {
                _data.InsertRange(index, bytes);
                return this;
            }

            // TODO: range checking and throw appropriate exceptions

            public int DataIndexOf(byte b, int start, int count) {
                for (int i = start; i < start + count; i++) {
                    if (_data[i] == b) {
                        return i;
                    }
                }
                return -1;
            }

            public int DataIndexOf(byte[]/*!*/ bytes, int start, int count) {
                // TODO:
                for (int i = start; i < start + count - bytes.Length + 1; i++) {
                    bool match = true;
                    for (int j = 0; j < bytes.Length; j++) {
                        if (bytes[j] != _data[i + j]) {
                            match = false;
                            break;
                        }
                    }

                    if (match) {
                        return i;
                    }
                }
                return -1;
            }

            public int DataLastIndexOf(byte b, int start, int count) {
                int finish = start - count < 0 ? 0 : start - count;
                for (int i = start; i >= finish; --i) {
                    if (_data[i] == b) {
                        return i;
                    }
                }
                return -1;
            }

            public int DataLastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                // TODO:
                int finish = start - count < 0 ? bytes.Length - 1 : start - count + bytes.Length;
                //for (int i = start; i < start + count - bytes.Length + 1; i++) {
                for (int i = start; i >= finish; --i) {
                    bool match = true;
                    for (int j = 0; j < bytes.Length; j++) {
                        if (bytes[j] != _data[i + j]) {
                            match = false;
                            break;
                        }
                    }

                    if (match) {
                        return i;
                    }
                }
                return -1;
            }

            #endregion

            #region GetHashCode, Length, Clone (read-only)

            public override bool IsBinary {
                get { return true; }
            }

            public override int GetHashCode() {
                // TODO: we need the same hash as if this was string. could be optimized.
                return SwitchToStringBuilder().GetHashCode();
            }

            public int DataLength {
                get { return _data.Count; }
            }

            public override int Length {
                get { return _data.Count; }
            }

            public override bool IsEmpty {
                get { return _data.Count == 0; }
            }

            public override int GetCharCount() {
                return (_data.Count > 0) ? SwitchToStringBuilder().DataLength : 0;
            }

            public override int GetByteCount() {
                return _data.Count;
            }

            public override Content/*!*/ Clone(MutableString/*!*/ newOwner) {
                return new BinaryContent(newOwner, new List<byte>(_data));
            }

            public override void GetDebugView(out string/*!*/ value, out string/*!*/ type) {
                value = DataToString();
                type = "String (binary)";
            }

            #endregion

            #region Conversions (read-only)

            public override string/*!*/ ConvertToString() {
                var builder = SwitchToStringBuilder();
                return builder.DataGetSlice(0, builder.DataLength);
            }

            public override byte[]/*!*/ ConvertToBytes() {
                return DataGetSlice(0, _data.Count);
            }

            public override string/*!*/ ToString() {
                return DataToString();
            }

            public override byte[]/*!*/ ToByteArray() {
                return _data.ToArray();
            }

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                // TODO: Fix BinaryRegex and use instead
                return new StringRegex(ToString(), options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                return new BinaryContent(_owner, new List<byte>(BinaryRegex.Escape(_data.ToArray())));
            }

            #endregion

            #region CompareTo (read-only)

            public override int CompareTo(string/*!*/ str) {
                return SwitchToStringBuilder().DataCompareTo(str);
            }

            public override int CompareTo(byte[]/*!*/ bytes) {
                return DataCompareTo(bytes);
            }

            public override int ReverseCompareTo(Content/*!*/ str) {
                return str.CompareTo(_data.ToArray());
            }

            #endregion

            #region Slices (read-only)

            public override char GetChar(int index) {
                return SwitchToStringBuilder().DataGetChar(index);
            }

            public override byte GetByte(int index) {
                return DataGetByte(index);
            }

            public override char PeekChar(int index) {
                int bc = _owner._encoding.GetMaxByteCount(1);
                if (_data.Count < bc) bc = _data.Count;
                return _owner._encoding.GetChars(_data.ToArray(), index, bc)[0];
            }

            public override byte PeekByte(int index) {
                return _data[index];
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return SwitchToStringBuilder().DataGetSlice(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return DataGetSlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new BinaryContent(_owner, new List<byte>(DataGetSlice(start, count)));
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                return SwitchToStringBuilder().DataIndexOf(c, start, count);
            }

            public override int IndexOf(byte b, int start, int count) {
                return DataIndexOf(b, start, count);
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                return SwitchToStringBuilder().DataIndexOf(str, start, count);
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return DataIndexOf(bytes, start, count);
            }

            public override int IndexIn(Content/*!*/ str, int start, int count) {
                return str.IndexOf(_data.ToArray(), start, count);
            }

            #endregion

            #region LastIndexOf (read-only)

            public override int LastIndexOf(char c, int start, int count) {
                return SwitchToStringBuilder().DataLastIndexOf(c, start, count);
            }

            public override int LastIndexOf(byte b, int start, int count) {
                return DataLastIndexOf(b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                return SwitchToStringBuilder().DataLastIndexOf(str, start, count);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                return DataLastIndexOf(bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data.ToArray(), start, count);
            }

            #endregion


            #region Append

            public override Content/*!*/ Append(char c, int repeatCount) {
                return SwitchToStringBuilder(repeatCount).DataAppend(c, repeatCount);
            }

            public override Content/*!*/ Append(byte b, int repeatCount) {
                return DataAppend(b, repeatCount);
            }

            public override Content/*!*/ Append(string/*!*/ str, int start, int count) {
                return SwitchToStringBuilder(count).DataAppend(str, start, count);
            }

            public override Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count) {
                return DataAppend(bytes, start, count);
            }

            public override Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                return SwitchToStringBuilder().DataAppendFormat(provider, format, args);
            }

            public override Content/*!*/ AppendTo(Content/*!*/ str, int start, int count) {
                return str.Append(_data.ToArray(), start, count);
            }

            #endregion

            #region Insert

            public override Content/*!*/ Insert(int index, char c) {
                return SwitchToStringBuilder(1).DataInsert(index, c);
            }

            public override Content/*!*/ Insert(int index, byte b) {
                return DataInsert(index, b);
            }

            public override Content/*!*/ Insert(int index, string/*!*/ str, int start, int count) {
                return SwitchToStringBuilder(count).DataInsert(index, str, start, count);
            }

            public override Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                return DataInsert(index, bytes, start, count);
            }

            public override Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count) {
                return str.Insert(index, _data.ToArray(), start, count);
            }

            public override Content/*!*/ SetItem(int index, byte b) {
                return DataSetByte(index, b);
            }

            public override Content/*!*/ SetItem(int index, char c) {
                return SwitchToStringBuilder().DataSetChar(index, c);
            }

            #endregion

            #region Remove

            public override Content/*!*/ Remove(int start, int count) {
                _data.RemoveRange(start, count);
                return this;
            }

            #endregion
        }
    }
}
