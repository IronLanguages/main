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
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private class StringContent : Content {
            private readonly string/*!*/ _data;

            public StringContent(string/*!*/ data, MutableString owner) 
                : base(owner) {
                Assert.NotNull(data);
                _data = data;
            }

            protected virtual BinaryContent/*!*/ SwitchToBinary() {
                var bytes = DataToBytes();
                return WrapContent(bytes, bytes.Length);
            }

            private CharArrayContent/*!*/ SwitchToMutable() {
                return WrapContent(_data.ToCharArray(), _data.Length);
            }

            protected byte[]/*!*/ DataToBytes() {
                return _data.Length > 0 ? _owner._encoding.StrictEncoding.GetBytes(_data) : Utils.EmptyBytes;
            }

            #region GetHashCode, Length, Clone (read-only)

            public override int GetHashCode(out int binarySum) {
                return _data.GetValueHashCode(out binarySum);
            }

            public override int GetBinaryHashCode() {
                return IsBinaryEncoded ? GetHashCode() : SwitchToBinary().GetBinaryHashCode();
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
                return (IsBinaryEncoded) ? _data.Length : (_data.Length == 0) ? 0 : SwitchToBinary().GetByteCount();
            }

            public override Content/*!*/ Clone() {
                return new StringContent(_data, _owner);
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

            public override GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options) {
                return new StringRegex(_data, options);
            }

            public override Content/*!*/ EscapeRegularExpression() {
                StringBuilder sb = StringRegex.EscapeToStringBuilder(_data);
                return (sb != null) ? new StringContent(sb.ToString(), _owner) : this;
            }

            #endregion

            #region CompareTo (read-only)

            public override int CompareTo(string/*!*/ str) {
                // TODO: Ruby compares characters w/o taking locale into account:
                return _data.ValueCompareTo(str);
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
                return SwitchToBinary().GetByte(index);
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return _data.Substring(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return SwitchToBinary().GetBinarySlice(start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return new StringContent(_data.Substring(start, count), _owner);
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                return _data.IndexOf(c, start, count);
            }

            public override int IndexOf(byte b, int start, int count) {
                return SwitchToBinary().IndexOf(b, start, count);
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                return _data.IndexOf(str, start, count);
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().IndexOf(bytes, start, count);
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
                return SwitchToBinary().LastIndexOf(b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                return _data.LastIndexOf(str, start, count);
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().LastIndexOf(bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data, start, count);
            }

            #endregion

            #region Append

            public override Content/*!*/ Append(char c, int repeatCount) {
                return SwitchToMutable().Append(c, repeatCount);
            }

            public override Content/*!*/ Append(byte b, int repeatCount) {
                return SwitchToBinary().Append(b, repeatCount);
            }

            public override Content/*!*/ Append(string/*!*/ str, int start, int count) {
                return SwitchToMutable().Append(str, start, count);
            }

            public override Content/*!*/ Append(char[]/*!*/ chars, int start, int count) {
                return SwitchToMutable().Append(chars, start, count);
            }

            public override Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().Append(bytes, start, count);
            }

            public override Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                return SwitchToMutable().AppendFormat(provider, format, args);
            }

            public override Content/*!*/ AppendTo(Content/*!*/ str, int start, int count) {
                return str.Append(_data, start, count);
            }

            #endregion

            #region Insert

            public override Content/*!*/ Insert(int index, char c) {
                return SwitchToMutable().Insert(index, c);
            }

            public override Content/*!*/ Insert(int index, byte b) {
                return SwitchToBinary().Insert(index, b);
            }

            public override Content/*!*/ Insert(int index, string/*!*/ str, int start, int count) {
                return SwitchToMutable().Insert(index, str, start, count);
            }

            public override Content/*!*/ Insert(int index, char[]/*!*/ chars, int start, int count) {
                return SwitchToMutable().Insert(index, chars, start, count);
            }

            public override Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count) {
                return SwitchToBinary().Insert(index, bytes, start, count);
            }

            public override Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count) {
                return str.Insert(index, _data, start, count);
            }

            public override Content/*!*/ SetItem(int index, byte b) {
                return SwitchToBinary().SetItem(index, b);
            }

            public override Content/*!*/ SetItem(int index, char c) {
                return SwitchToMutable().DataSetChar(index, c);
            }

            #endregion

            #region Remove

            public override Content/*!*/ Remove(int start, int count) {
                return SwitchToMutable().Remove(start, count);
            }

            #endregion
        }
    }
}
