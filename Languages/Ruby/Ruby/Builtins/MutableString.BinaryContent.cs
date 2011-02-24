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
        internal class BinaryContent : Content {
            protected byte[] _data;
            protected int _count;

            internal BinaryContent(byte[]/*!*/ data, MutableString owner) 
                : this(data, data.Length, owner) {
            }

            internal BinaryContent(byte[]/*!*/ data, int count, MutableString owner)
                : base(owner) {
                Assert.NotNull(data);
                Debug.Assert(count >= 0 && count <= data.Length);
                _data = data;
                _count = count;
            }

            protected virtual BinaryContent/*!*/ Create(byte[]/*!*/ data, MutableString owner) {
                return new BinaryContent(data, owner);
            }

            private void Decode(out char[] chars, out List<byte[]> invalidCharacters) {
#if SILVERLIGHT
                chars = _owner.Encoding.Encoding.GetChars(_data, 0, _count);
                invalidCharacters = null;
#else
                Decoder decoder = _owner.Encoding.Encoding.GetDecoder();
                var fallback = new LosslessDecoderFallback();
                decoder.Fallback = fallback;

                // TODO: split into chunks for large strings?
                fallback.Track = true;
                chars = new char[decoder.GetCharCount(_data, 0, _count, true)];
                fallback.Track = false;
                decoder.GetChars(_data, 0, _count, chars, 0, true);

                invalidCharacters = fallback.InvalidCharacters;
#endif
            }

            // TODO: we can remember both representations until a mutable operation is performed
            /// <exception cref="DecoderFallbackException">Invalid character.</exception>
            private CharArrayContent/*!*/ SwitchToChars() {
                return SwitchToChars(0);
            }

            /// <exception cref="DecoderFallbackException">Invalid character.</exception>
            private CharArrayContent/*!*/ SwitchToChars(int additionalCapacity) {
                char[] chars = DataToChars(additionalCapacity, _owner._encoding.StrictEncoding);
                return WrapContent(chars, chars.Length - additionalCapacity);
            }

            /// <exception cref="DecoderFallbackException">Invalid character.</exception>
            private char[]/*!*/ DataToChars(int additionalCapacity, Encoding/*!*/ encoding) {
                if (_count == 0) {
                    return (additionalCapacity == 0) ? Utils.EmptyChars : new char[additionalCapacity];
                } else if (additionalCapacity == 0) {
                    return encoding.GetChars(_data, 0, _count);
                } else {
                    var result = new char[encoding.GetCharCount(_data, 0, _count) + additionalCapacity];
                    encoding.GetChars(_data, 0, _count, result, 0);
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

            internal void AppendBytes(string/*!*/ str, int start, int count) {
                _count = Utils.Append(ref _data, _count, str, start, count, _owner._encoding.StrictEncoding);
            }

            internal void AppendBytes(char[]/*!*/ chars, int start, int count) {
                _count = Utils.Append(ref _data, _count, chars, start, count, _owner._encoding.StrictEncoding);
            }

            #region UpdateCharacterFlags, CalculateHashCode, Length, Clone, Count (read-only)

            public override uint UpdateCharacterFlags(uint flags) {
                // can't determine surrogates for binary repr (w/o conversion):
                if (_data.IsAscii(_count)) {
                    return (flags & ~MutableString.AsciiUnknownFlag) | MutableString.IsAsciiFlag;
                } else {
                    return flags & ~(MutableString.AsciiUnknownFlag | MutableString.IsAsciiFlag);
                }
            }

            public override int CalculateHashCode() {
                if (_count == 0) {
                    return String.Empty.GetHashCode();
                }

                char[] chars;
                List<byte[]> invalidCharacters;
                Decode(out chars, out invalidCharacters);

                // TODO: we can also cache invalid bytes if needed (maybe have a special content repr?)
                // cache conversion result if there are not invalid characters (switching content to char array):
                if (invalidCharacters == null) {
                    return WrapContent(chars, chars.Length).CalculateHashCode();
                } else {
                    // Unfortunately, BLC doesn't have a method to calculate hash code directly out of char[]:
                    return new string(chars).GetHashCode();
                }
            }

            public override int Count {
                get { return _count; }
                set {
                    if (_data.Length < value) {
                        Array.Resize(ref _data, Utils.GetExpandedSize(_data, value));
                    } else {
                        Utils.Fill(_data, _count, (byte)0, value - _count);
                    }
                    _count = value;
                }
            }

            public override bool IsEmpty {
                get { return _count == 0; }
            }

            public override int GetCharCount() {
                return (_owner.HasSingleByteCharacters || _count == 0) ? _count : SwitchToChars().GetCharCount();
            }

            public override int GetCharacterCount() {
                if (_owner.HasSingleByteCharacters || _count == 0) {
                    return _count;
                }

                //
                // We need to 
                // 1) decode bytes to UTF16 chars replacing sequences of undecodable bytes with U+FFFF markers
                //    (MRI counts each such sequence as 1 character),
                // 2) subtract the number of surrogates in the resulting char sequence.
                //
                // Unfortunately, this is a bit complex and not very efficient. We might be able to amortize the cost 
                // by caching the resulting char array via switching to char content in anticipation of subsequent 
                // character based operations.
                //
                char[] chars;
                List<byte[]> invalidCharacters;
                Decode(out chars, out invalidCharacters);

                // TODO: we can also cache invalid bytes if needed (maybe have a special content repr?)
                // cache conversion result if there are not invalid characters (switching content to char array):
                if (invalidCharacters == null) {
                    return WrapContent(chars, chars.Length).GetCharacterCount();
                } else {
                    return chars.GetCharacterCount(chars.Length);
                }
            }

            public override int GetByteCount() {
                return _count;
            }

            public override Content/*!*/ Clone() {
                return Create(ToByteArray(), _owner);
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
                return SwitchToChars().ConvertToString();
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

            internal override byte[]/*!*/ GetByteArray(out int count) {
                count = _count;
                return _data;
            }

            public override Content/*!*/ SwitchToBinaryContent() {
                return this;
            }

            public override Content/*!*/ SwitchToStringContent() {
                return SwitchToChars();
            }

            public override Content/*!*/ SwitchToMutableContent() {
                return this;
            }

            public override Content/*!*/ EscapeRegularExpression() {
                // TODO:
                var a = ToByteArray();
                return Create(
                    BinaryEncoding.Instance.GetBytes(RubyRegex.Escape(BinaryEncoding.Instance.GetString(a, 0, a.Length))),
                    _owner
                );
            }

            public override void CheckEncoding() {
                _owner._encoding.StrictEncoding.GetCharCount(_data, 0, _count);
            }

            public override bool ContainsInvalidCharacters() {
                return Utils.ContainsInvalidCharacters(_data, 0, _count, _owner._encoding.StrictEncoding);
            }

            #endregion

            #region CompareTo (read-only)

            public override int OrdinalCompareTo(string/*!*/ str) {
                if (_owner.HasByteCharacters) {
                    return Utils.ValueCompareTo(_data, _count, str);
                } else {
                    return SwitchToChars().OrdinalCompareTo(str);
                }
            }

            internal int OrdinalCompareTo(byte[]/*!*/ bytes, int count) {
                return _data.ValueCompareTo(_count, bytes, count);
            }

            // this <=> content
            public override int OrdinalCompareTo(Content/*!*/ content) {
                return content.ReverseOrdinalCompareTo(this);
            }

            // content.bytes <=> this.bytes
            public override int ReverseOrdinalCompareTo(BinaryContent/*!*/ content) {
                return content.OrdinalCompareTo(_data, _count);
            }

            // content.chars <=> this.bytes
            public override int ReverseOrdinalCompareTo(CharArrayContent/*!*/ content) {
                return content.SwitchToBinary().OrdinalCompareTo(_data, _count);
            }

            // content.chars <=> this.bytes
            public override int ReverseOrdinalCompareTo(StringContent/*!*/ content) {
                return content.SwitchToBinary().OrdinalCompareTo(_data, _count);
            }

            #endregion

            #region Slices (read-only)

            public override char GetChar(int index) {
                if (_owner.HasSingleByteCharacters || index == 0) {
                    // character index == byte index
                    if (index >= _count) {
                        throw new IndexOutOfRangeException();
                    }
                    var result = _data[index];
                    if (result < 0x80 || _owner.HasByteCharacters) {
                        // no decoding needed:
                        return (char)result;
                    }
                }

                // TODO: invalid characters
                return SwitchToChars().GetChar(index);
            }

            public override byte GetByte(int index) {
                if (index >= _count) {
                    throw new IndexOutOfRangeException();
                }
                return _data[index];
            }

            public override string/*!*/ GetStringSlice(int start, int count) {
                return SwitchToChars().GetStringSlice(start, count);
            }

            public override byte[]/*!*/ GetBinarySlice(int start, int count) {
                return _data.GetSlice(_count, start, count);
            }

            public override Content/*!*/ GetSlice(int start, int count) {
                return Create(_data.GetSlice(_count, start, count), _owner);
            }

            public override CharacterEnumerator/*!*/ GetCharacters() {
                if (_owner.HasByteCharacters) {
                    return new MutableString.BinaryCharacterEnumerator(_owner.Encoding, _data, _count);
                }

                char[] allValid;
                var result = MutableString.EnumerateAsCharacters(_data, _count, _owner.Encoding, out allValid);
                if (allValid != null) {
                    // we can switch the content type if all characters are valid:
                    WrapContent(allValid, allValid.Length);
                }
                return result;
            }

            public override IEnumerable<byte>/*!*/ GetBytes() {
                return Utils.Enumerate(_data, _count);
            }

            #endregion

            #region StartsWith (read-only)

            public override bool StartsWith(char c) {
                if (_count == 0) {
                    return false;
                }

                if (_owner.HasByteCharacters || c < 0x80 && _owner._encoding.IsAsciiIdentity) {
                    return _data[0] == c;    
                }

                byte[] bytes = new byte[_owner.Encoding.MaxBytesPerChar];
                int byteCount = _owner.Encoding.StrictEncoding.GetBytes(new char[] { c }, 0, 1, bytes, 0);
                if (byteCount > _count) {
                    return false;
                }

                return Utils.ValueCompareTo(_data, byteCount, bytes, byteCount) == 0;
            }

            #endregion

            #region IndexOf (read-only)

            public override int IndexOf(char c, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.IndexOf(_data, _count, c, start, count);
                } else {
                    return SwitchToChars().IndexOf(c, start, count);
                }
            }

            public override int IndexOf(byte b, int start, int count) {
                count = Utils.NormalizeCount(_count, start, count);
                return count > 0 ? Array.IndexOf(_data, b, start, count) : -1;
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.IndexOf(_data, _count, str, start, count);
                } else {
                    return SwitchToChars().IndexOf(str, start, count);
                }
            }

            public override int IndexOf(byte[]/*!*/ bytes, int start, int count) {
                return Utils.IndexOf(_data, _count, bytes, start, count);
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
                Utils.NormalizeLastIndexOfIndices(_count, ref start, ref count);
                return Array.LastIndexOf(_data, b, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                if (_owner.HasByteCharacters) {
                    return Utils.LastIndexOf(_data, _count, str, start, count);
                } else {
                    return SwitchToChars().LastIndexOf(str, start, count);
                }
            }

            public override int LastIndexOf(byte[]/*!*/ bytes, int start, int count) {
                // Array.LastIndexOf has different semantics for indices than String.LastIndexOf:
                return Utils.LastIndexOf(_data, _count, bytes, start, count);
            }

            public override int LastIndexIn(Content/*!*/ str, int start, int count) {
                return str.LastIndexOf(_data, start, count);
            }

            #endregion

            #region Concatenate (read-only)

            public override Content/*!*/ Concat(Content/*!*/ content) {
                return content.ConcatTo(this);
            }

            internal BinaryContent/*!*/ Concatenate(CharArrayContent/*!*/ content) {
                int count = content.GetDataByteCount();
                var result = new byte[_count + count];
                Array.Copy(_data, 0, result, 0, _count);
                content.GetDataBytes(result, _count);
                return Create(result, null);
            }

            internal BinaryContent/*!*/ Concatenate(StringContent/*!*/ content) {
                int count = content.GetDataByteCount();
                var result = new byte[_count + count];
                Array.Copy(_data, 0, result, 0, _count);
                content.GetDataBytes(result, _count);
                return Create(result, null);
            }

            // binary + binary(self) -> binary
            public override Content/*!*/ ConcatTo(BinaryContent/*!*/ content) {
                return Create(Utils.Concatenate(content._data, content._count, _data, _count), null);
            }

            // chars + binary(self) -> binary
            public override Content/*!*/ ConcatTo(CharArrayContent/*!*/ content) {
                int count = content.GetDataByteCount();
                var result = new byte[count + _count];
                content.GetDataBytes(result, 0);
                Array.Copy(_data, 0, result, count, _count);
                return Create(result, null);
            }

            // string + binary(self) -> binary
            public override Content/*!*/ ConcatTo(StringContent/*!*/ content) {
                int count = content.GetDataByteCount();
                var result = new byte[count + _count];
                content.GetDataBytes(result, 0);
                Array.Copy(_data, 0, result, count, _count);
                return Create(result, null);
            }

            #endregion

            #region Append

            public override void Append(char c, int repeatCount) {
                if (_owner.HasByteCharacters || c < 0x80 && _owner._encoding.IsAsciiIdentity) {
                    Append((byte)c, repeatCount);
                } else {
                    _count = Utils.Append(ref _data, _count, c, repeatCount, _owner._encoding.StrictEncoding);
                }
            }

            public override void Append(byte b, int repeatCount) {
                _count = Utils.Append(ref _data, _count, b, repeatCount);
            }

            public override void Append(string/*!*/ str, int start, int count) {
                AppendBytes(str, start, count);
            }

            public override void Append(char[]/*!*/ chars, int start, int count) {
                AppendBytes(chars, start, count);
            }

            public override void Append(byte[]/*!*/ bytes, int start, int count) {
                _count = Utils.Append(ref _data, _count, bytes, start, count);
            }

            public override void Append(Stream/*!*/ stream, int count) {
                Utils.Resize(ref _data, _count + count);
                _count += stream.Read(_data, _count, count);
            }

            public override void AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args) {
                var formatted = String.Format(provider, format, args);
                AppendBytes(formatted, 0, formatted.Length);
            }

            // this + content[start, count]
            public override void Append(Content/*!*/ content, int start, int count) {
                content.AppendTo(this, start, count);
            }

            // content.bytes + this.bytes[start, count]
            public override void AppendTo(BinaryContent/*!*/ content, int start, int count) {
                if (start > _count - count) {
                    throw new ArgumentOutOfRangeException("start");
                }
                
                content.Append(_data, start, count);
            }

            // content.chars + this.bytes[start, count]
            public override void AppendTo(CharArrayContent/*!*/ content, int start, int count) {
                if (start > _count - count) {
                    throw new ArgumentOutOfRangeException("start");
                }

                content.SwitchToBinary().Append(_data, start, count);
            }

            // content.chars + this.bytes[start, count]
            public override void AppendTo(StringContent/*!*/ content, int start, int count) {
                if (start > _count - count) {
                    throw new ArgumentOutOfRangeException("start");
                }

                content.SwitchToBinary().Append(_data, start, count);
            }

            #endregion

            #region Insert

            public override void Insert(int index, char c) {
                if (_owner.HasByteCharacters || c < 0x80 && _owner._encoding.IsAsciiIdentity) {
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
                if (index >= _count) {
                    throw new IndexOutOfRangeException();
                }
                _data[index] = b;
            }

            public override void SetChar(int index, char c) {
                if (_owner.HasByteCharacters || c < 0x80 && _owner.HasSingleByteCharacters) {
                    SetByte(index, (byte)c);
                } else {
                    SwitchToChars().DataSetChar(index, c);
                }
            }

            #endregion

            #region Remove, Write

            public override void Remove(int start, int count) {
                _count = Utils.Remove(ref _data, _count, start, count);
            }

            public override void Write(int offset, byte[]/*!*/ value, int start, int count) {
                Utils.Resize(ref _data, offset + count);
                _count = Math.Max(_count, offset + count);
                Buffer.BlockCopy(value, start, _data, offset, count);
            }

            public override void Write(int offset, byte value, int repeatCount) {
                int end = offset + repeatCount;
                Utils.Resize(ref _data, end);
                if (end > _count) {
                    _count = end;
                }
                for (int i = offset; i < end; i++) {
                    _data[i] = value;
                }
            }

            #endregion
        }
    }
}
