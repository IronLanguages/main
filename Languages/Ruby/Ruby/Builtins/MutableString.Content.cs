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
using IronRuby.Runtime;
using System.Diagnostics;
using System.IO;
using IronRuby.Compiler;

namespace IronRuby.Builtins {
    public partial class MutableString {
        // TODO: Add range checks to APIs or change the implementation to be out-of-range tolerant (see GetSlice).
        // Tha latter is preferred if the semantics is reasonable since it reduces the amounts of content size dependent checks that the user needs to do.
        [Serializable]
        internal abstract class Content {
            protected MutableString _owner;

            #region Utils

            internal void SetOwner(MutableString/*!*/ owner) {
                Assert.NotNull(owner);
                Debug.Assert(_owner == null || _owner.Encoding == owner.Encoding);
                _owner = owner;
            }

            protected Content(MutableString owner) {
                _owner = owner;
            }

            protected BinaryContent/*!*/ WrapContent(byte[]/*!*/ bytes, int count) {
                BinaryContent result = new BinaryContent(bytes, count, _owner);
                _owner.SetContent(result);
                return result;
            }
            
            protected CharArrayContent/*!*/ WrapContent(char[]/*!*/ chars, int count) {
                var result = new CharArrayContent(chars, count, _owner);
                _owner.SetContent(result);
                return result;
            }

            internal static uint UpdateAsciiAndSurrogatesFlags(string/*!*/ str, uint flags) {
                int sum = 0;
                for (int i = 0; i < str.Length; i++) {
                    int c = str[i];
                    if (Tokenizer.IsSurrogate(c)) {
                        return flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag | MutableString.IsAsciiFlag | MutableString.NoSurrogatesFlag);
                    }
                    sum |= c;
                }

                return (sum < 0x80)
                  ? flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag) | MutableString.IsAsciiFlag | MutableString.NoSurrogatesFlag
                  : flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag | MutableString.IsAsciiFlag) | MutableString.NoSurrogatesFlag;
            }

            internal static uint UpdateAsciiAndSurrogatesFlags(char[]/*!*/ str, int itemCount, uint flags) {
                int sum = 0;
                for (int i = 0; i < itemCount; i++) {
                    int c = str[i];
                    if (Tokenizer.IsSurrogate(c)) {
                        return flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag | MutableString.IsAsciiFlag | MutableString.NoSurrogatesFlag);
                    }
                    sum |= c;
                }

                return (sum < 0x80)
                  ? flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag) | MutableString.IsAsciiFlag | MutableString.NoSurrogatesFlag
                  : flags & ~(MutableString.SurrogatesUnknownFlag | MutableString.AsciiUnknownFlag | MutableString.IsAsciiFlag) | MutableString.NoSurrogatesFlag;
            }

            #endregion

            public abstract string/*!*/ ConvertToString();
            public abstract byte[]/*!*/ ConvertToBytes();
            public abstract Content/*!*/ SwitchToBinaryContent();
            public abstract Content/*!*/ SwitchToStringContent();
            public abstract Content/*!*/ SwitchToMutableContent();
            public abstract void CheckEncoding();
            public abstract bool ContainsInvalidCharacters();

            public abstract byte[]/*!*/ ToByteArray();
            internal abstract byte[]/*!*/ GetByteArray(out int count);

            // returns self if there are no characters to be escaped:
            public abstract Content/*!*/ EscapeRegularExpression();

            // read:
            public abstract bool IsEmpty { get; }
            public abstract int Count { get; set; }
            public abstract int CalculateHashCode();
            public abstract uint UpdateCharacterFlags(uint flags);

            /// <summary>
            /// Returns the number of characters in the string.
            /// Counts surrogates as two characters.
            /// </summary>
            /// <exception cref="DecoderFallbackException">Invalid character.</exception>
            public abstract int GetCharCount();

            /// <summary>
            /// Returns the number of true Unicode characters in the string.
            /// </summary>
            public abstract int GetCharacterCount();

            /// <summary>
            /// Returns the number of bytes in the string.
            /// Throws if the string includes invalid characters.
            /// </summary>
            public abstract int GetByteCount();

            public abstract void TrimExcess();
            public abstract int GetCapacity();
            public abstract void SetCapacity(int capacity);
            public abstract Content/*!*/ Clone();

            /// <summary>
            /// Gets index'th character of the string.
            /// Throws if the string includes invalid characters.
            /// </summary>
            public abstract char GetChar(int index);

            /// <summary>
            /// Gets index'th byte of the string.
            /// Throws if the string includes invalid characters.
            /// </summary>
            public abstract byte GetByte(int index);

            public abstract CharacterEnumerator/*!*/ GetCharacters();
            public abstract IEnumerable<byte>/*!*/ GetBytes();

            public abstract int OrdinalCompareTo(string/*!*/ str);

            public abstract int OrdinalCompareTo(Content/*!*/ content);
            public abstract int ReverseOrdinalCompareTo(BinaryContent/*!*/ content);
            public abstract int ReverseOrdinalCompareTo(CharArrayContent/*!*/ content);
            public abstract int ReverseOrdinalCompareTo(StringContent/*!*/ content);
            
            /// <summary>
            /// Returns a slice of the content. The size of the slice could be less than the requested count if there is not enough data in the content.
            /// Returns an empty content if start is greater than the size of the content.
            /// The owner of the result is the current owner.
            /// </summary>
            public abstract Content/*!*/ GetSlice(int start, int count);
            public abstract string/*!*/ GetStringSlice(int start, int count);
            public abstract byte[]/*!*/ GetBinarySlice(int start, int count);

            public abstract bool StartsWith(char c);

            public abstract int IndexOf(char c, int start, int count);
            public abstract int IndexOf(byte b, int start, int count);
            public abstract int IndexOf(string/*!*/ str, int start, int count);
            public abstract int IndexOf(byte[]/*!*/ bytes, int start, int count);
            public abstract int IndexIn(Content/*!*/ str, int start, int count);

            public abstract int LastIndexOf(char c, int start, int count);
            public abstract int LastIndexOf(byte b, int start, int count);
            public abstract int LastIndexOf(string/*!*/ str, int start, int count);
            public abstract int LastIndexOf(byte[]/*!*/ bytes, int start, int count);
            public abstract int LastIndexIn(Content/*!*/ str, int start, int count);

            public abstract Content/*!*/ Concat(Content/*!*/ content);
            public abstract Content/*!*/ ConcatTo(BinaryContent/*!*/ content);
            public abstract Content/*!*/ ConcatTo(CharArrayContent/*!*/ content);
            public abstract Content/*!*/ ConcatTo(StringContent/*!*/ content);

            // write:
            public abstract void Append(char c, int repeatCount);
            public abstract void Append(byte b, int repeatCount);
            public abstract void Append(string/*!*/ str, int start, int count);
            public abstract void Append(char[]/*!*/ chars, int start, int count);
            public abstract void Append(byte[]/*!*/ bytes, int start, int count);
            public abstract void Append(Stream/*!*/ stream, int count);

            public abstract void Append(Content/*!*/ content, int start, int count);
            public abstract void AppendTo(BinaryContent/*!*/ content, int start, int count);
            public abstract void AppendTo(CharArrayContent/*!*/ content, int start, int count);
            public abstract void AppendTo(StringContent/*!*/ content, int start, int count);

            public abstract void AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args);
            
            public abstract void Insert(int index, char c);
            public abstract void Insert(int index, byte b);
            public abstract void Insert(int index, string/*!*/ str, int start, int count);
            public abstract void Insert(int index, char[]/*!*/ chars, int start, int count);
            public abstract void Insert(int index, byte[]/*!*/ bytes, int start, int count);
            public abstract void InsertTo(Content/*!*/ str, int index, int start, int count);

            public abstract void SetByte(int index, byte b);
            public abstract void SetChar(int index, char c);

            public abstract void Remove(int start, int count);

            public abstract void Write(int offset, byte[]/*!*/ value, int start, int count);
            public abstract void Write(int offset, byte value, int repeatCount);
        }
    }
}
