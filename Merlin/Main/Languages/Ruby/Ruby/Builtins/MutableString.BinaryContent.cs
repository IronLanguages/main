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
using System.Diagnostics;

namespace IronRuby.Builtins {
    public partial class MutableString {
        /// <summary>
        /// Mutable byte array. 
        /// All indices and counts are in bytes.
        /// </summary>
        [Serializable]
        private class BinaryContent : Content {
            private byte[] _data;
            private int _count;

            public BinaryContent(byte[]/*!*/ data, MutableString owner) 
                : this(data, data.Length, owner) {
            }

            internal BinaryContent(byte[]/*!*/ data, int count, MutableString owner)
                : base(owner) {
                Assert.NotNull(data);
                Debug.Assert(count >= 0 && count <= data.Length);
                _data = data;
                _count = count;
            }

            // TODO: we can remember both representations until a mutable operation is performed
            private CharArrayContent/*!*/ SwitchToChars() {
                return SwitchToChars(0);
            }

            private CharArrayContent/*!*/ SwitchToChars(int additionalCapacity) {
                var chars = DataToChars(additionalCapacity);
                return WrapContent(chars, chars.Length - additionalCapacity);
            }

            private char[]/*!*/ DataToChars(int additionalCapacity) {
                if (_count == 0) {
                    return (additionalCapacity == 0) ? Utils.EmptyChars : new char[additionalCapacity];
                } else if (additionalCapacity == 0) {
                    return _owner._encoding.StrictEncoding.GetChars(_data, 0, _count);
                } else {
                    var result = new char[_owner._encoding.StrictEncoding.GetCharCount(_data, 0, _count) + additionalCapacity];
                    _owner._encoding.StrictEncoding.GetChars(_data, 0, _count, result, 0);
                    return result;
                }
            }

            private string/*!*/ DataToString() {
                if (_count == 0) {
                    return String.Empty;
                } else {
                    return _owner._encoding.StrictEncoding.GetString(_data, 0, _count);
                } 
            }

            #region GetHashCode, Length, Clone (read-only)

            public override bool IsBinary {
                get { return true; }
            }

            public override int GetHashCode(out int binarySum) {
                return _data.GetValueHashCode(_count, out binarySum);
            }

            public override int GetBinaryHashCode() {
                return GetHashCode();
            }

            public override int Length {
                get { return _count; }
            }

            public override bool IsEmpty {
                get { return _count == 0; }
            }

            public override int GetCharCount() {
                return (IsBinaryEncoded) ? _count : (_count == 0) ? 0 : SwitchToChars().GetCharCount();
            }

            public override int GetByteCount() {
                return _count;
            }

            public override Content/*!*/ Clone() {
                return new BinaryContent(ToByteArray(), _owner);
            }

            #endregion

            #region Conversions (read-only)

            public override string/*!*/ ConvertToString() {
                var builder = SwitchToChars();
                return builder.GetStringSlice(0, builder.DataLength);
            }

            public override byte[]/*!*/ ConvertToBytes() {
                return _data.GetSlice(0, _count);
            }

            public override string/*!*/ ToString() {
                return DataToString();
            }

            public override byte[]/*!*/ ToByteArray() {
                return _data.GetSlice(0, _count);
            }

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                // TODO: Fix BinaryRegex and use instead
                return new StringRegex(ToString(), options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                return new BinaryContent(BinaryRegex.Escape(ToByteArray()), _owner);
            }

            #endregion

            #region CompareTo (read-only)

            public override int CompareTo(string/*!*/ str) {
                return SwitchToChars().CompareTo(str);
            }

            public override int CompareTo(byte[]/*!*/ bytes) {
                return _data.ValueCompareTo(_count, bytes);
            }

            public override int ReverseCompareTo(Content/*!*/ str) {
                return str.CompareTo(ToByteArray());
            }

            #endregion

            #region Slices (read-only)

            public override char GetChar(int index) {
                return SwitchToChars().DataGetChar(index);
            }

            public override byte GetByte(int index) {
                return _data[index];
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return SwitchToChars().GetStringSlice(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return _data.GetSlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new BinaryContent(_data.GetSlice(start, count), _owner);
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                return SwitchToChars().IndexOf(c, start, count);
            }

            public override int IndexOf(byte b, int start, int count) {
                return Array.IndexOf(_data, b, start, count);
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                return SwitchToChars().IndexOf(str, start, count);
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return Utils.IndexOf(_data, bytes, start, count);
            }

            public override int IndexIn(Content/*!*/ str, int start, int count) {
                return str.IndexOf(_data, start, count);
            }

            #endregion

            #region LastIndexOf (read-only)

            public override int LastIndexOf(char c, int start, int count) {
                return SwitchToChars().LastIndexOf(c, start, count);
            }

            public override int LastIndexOf(byte b, int start, int count) {
                return Array.LastIndexOf(_data, b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                return SwitchToChars().LastIndexOf(str, start, count);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                return Utils.LastIndexOf(_data, bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data, start, count);
            }

            #endregion


            #region Append

            public override Content/*!*/ Append(char c, int repeatCount) {
                return SwitchToChars(repeatCount).Append(c, repeatCount);
            }

            public override Content/*!*/ Append(byte b, int repeatCount) {
                _count = Utils.Append(ref _data, _count, b, repeatCount);
                return this;
            }

            public override Content/*!*/ Append(string/*!*/ str, int start, int count) {
                return SwitchToChars(count).Append(str, start, count);
            }

            public override Content/*!*/ Append(char[]/*!*/ chars, int start, int count) {
                return SwitchToChars(count).Append(chars, start, count);
            }

            public override Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count) {
                _count = Utils.Append(ref _data, _count, bytes, start, count);
                return this;
            }

            public override Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                return SwitchToChars().AppendFormat(provider, format, args);
            }

            public override Content/*!*/ AppendTo(Content/*!*/ str, int start, int count) {
                return str.Append(_data, start, count);
            }

            #endregion

            #region Insert

            public override Content/*!*/ Insert(int index, char c) {
                return SwitchToChars(1).Insert(index, c);
            }

            public override Content/*!*/ Insert(int index, byte b) {
                _count = Utils.InsertAt(ref _data, _count, index, b, 1);
                return this;
            }

            public override Content/*!*/ Insert(int index, string/*!*/ str, int start, int count) {
                return SwitchToChars(count).Insert(index, str, start, count);
            }

            public override Content/*!*/ Insert(int index, char[]/*!*/ chars, int start, int count) {
                return SwitchToChars(count).Insert(index, chars, start, count);
            }

            public override Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                _count = Utils.InsertAt(ref _data, _count, index, bytes, start, count);
                return this;
            }

            public override Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count) {
                return str.Insert(index, _data, start, count);
            }

            public override Content/*!*/ SetItem(int index, byte b) {
                _data[index] = b;
                return this;
            }

            public override Content/*!*/ SetItem(int index, char c) {
                return SwitchToChars().DataSetChar(index, c);
            }

            #endregion

            #region Remove

            public override Content/*!*/ Remove(int start, int count) {
                _count = Utils.Remove(ref _data, _count, start, count);
                return this;
            }

            #endregion
        }
    }
}
