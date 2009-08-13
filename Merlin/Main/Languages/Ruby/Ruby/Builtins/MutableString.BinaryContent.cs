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
using System.IO;

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

            public override int GetBinaryHashCode(out int binarySum) {
                return GetHashCode(out binarySum);
            }

            public override int Count {
                get { return _count; }
                set {
                    Utils.Resize(ref _data, value);
                    _count = value;
                }
            }

            public override bool IsEmpty {
                get { return _count == 0; }
            }

            public override int GetCharCount() {
                return (_owner.HasByteCharacters) ? _count : (_count == 0) ? 0 : SwitchToChars().GetCharCount();
            }

            public override int GetByteCount() {
                return _count;
            }

            public override Content/*!*/ Clone() {
                return new BinaryContent(ToByteArray(), _owner);
            }

            public override void TrimExcess() {
                Utils.TrimExcess(ref _data, _count);
            }

            public override int GetCapacity() {
                return _data.Length;
            }

            public override void SetCapacity(int capacity) {
                if (capacity < _count) {
                    throw new InvalidOperationException();
                }
                Array.Resize(ref _data, capacity);
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

            internal override byte[]/*!*/ GetByteArray() {
                return _data;
            }

            public override void SwitchToBinaryContent() {
                // nop
            }

            public override void SwitchToStringContent() {
                SwitchToChars();
            }

            public override void SwitchToMutableContent() {
                // nop
            }

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                // TODO: Fix BinaryRegex and use instead
                return new StringRegex(ToString(), options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                return new BinaryContent(BinaryRegex.Escape(ToByteArray()), _owner);
            }

            public override void CheckEncoding() {
                _owner._encoding.StrictEncoding.GetCharCount(_data, 0, _count);
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
                if (_owner.HasByteCharacters) {
                    return (char)_data[index];
                }
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

            public override void Append(char c, int repeatCount) {
                if (c < 0x80 || _owner.IsBinaryEncoded) {
                    Append((byte)c, repeatCount);
                } else {
                    SwitchToChars(repeatCount).Append(c, repeatCount);
                }
            }

            public override void Append(byte b, int repeatCount) {
                _count = Utils.Append(ref _data, _count, b, repeatCount);
            }

            public override void Append(string/*!*/ str, int start, int count) {
                SwitchToChars(count).Append(str, start, count);
            }

            public override void Append(char[]/*!*/ chars, int start, int count) {
                SwitchToChars(count).Append(chars, start, count);
            }

            public override void Append(byte[]/*!*/ bytes, int start, int count) {
                _count = Utils.Append(ref _data, _count, bytes, start, count);
            }

            public override void Append(Stream/*!*/ stream, int count) {
                Utils.Resize(ref _data, _count + count);
                _count += stream.Read(_data, _count, count);
            }

            public override void AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                SwitchToChars().AppendFormat(provider, format, args);
            }

            public override void AppendTo(Content/*!*/ str, int start, int count) {
                str.Append(_data, start, count);
            }

            #endregion

            #region Insert

            // requires: encoding is ascii-identity
            public override void Insert(int index, char c) {
                if (_owner.HasByteCharacters) {
                    Debug.Assert(c < 0x80 || _owner.IsBinaryEncoded);
                    _count = Utils.InsertAt(ref _data, _count, index, (byte)c, 1);
                } else {
                    SwitchToChars(1).Insert(index, c);
                }
            }

            public override void Insert(int index, byte b) {
                _count = Utils.InsertAt(ref _data, _count, index, b, 1);
            }

            public override void Insert(int index, string/*!*/ str, int start, int count) {
                SwitchToChars(count).Insert(index, str, start, count);
            }

            public override void Insert(int index, char[]/*!*/ chars, int start, int count) {
                SwitchToChars(count).Insert(index, chars, start, count);
            }

            public override void Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                _count = Utils.InsertAt(ref _data, _count, index, bytes, start, count);
            }

            public override void InsertTo(Content/*!*/ str, int index, int start, int count) {
                str.Insert(index, _data, start, count);
            }

            public override void SetByte(int index, byte b) {
                Debug.Assert(index < _count);
                _data[index] = b;
            }

            // requires: encoding is ascii-identity
            public override void SetChar(int index, char c) {
                if (_owner.HasByteCharacters) {
                    Debug.Assert(c < 0x80 || _owner.IsBinaryEncoded);
                    SetByte(index, (byte)c);
                } else {
                    SwitchToChars().DataSetChar(index, c);
                }
            }

            #endregion

            #region Remove

            public override void Remove(int start, int count) {
                _count = Utils.Remove(ref _data, _count, start, count);
            }

            #endregion
        }
    }
}
