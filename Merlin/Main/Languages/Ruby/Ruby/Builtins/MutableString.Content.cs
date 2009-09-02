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
using IronRuby.Runtime;
using System.Diagnostics;
using System.IO;

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private abstract class Content {
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
                var result = new BinaryContent(bytes, count, _owner);
                _owner.SetContent(result);
                return result;
            }
            
            protected CharArrayContent/*!*/ WrapContent(char[]/*!*/ chars, int count) {
                var result = new CharArrayContent(chars, count, _owner);
                _owner.SetContent(result);
                return result;
            }

            #endregion

            public abstract string/*!*/ ConvertToString();
            public abstract byte[]/*!*/ ConvertToBytes();
            public abstract void SwitchToBinaryContent();
            public abstract void SwitchToStringContent();
            public abstract void SwitchToMutableContent();
            public abstract void CheckEncoding();

            public abstract byte[]/*!*/ ToByteArray();
            internal abstract byte[]/*!*/ GetByteArray();
            public abstract GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options);

            // returns self if there are no characters to be escaped:
            public abstract Content/*!*/ EscapeRegularExpression();

            // read:
            public abstract bool IsBinary { get; }
            public abstract bool IsEmpty { get; }
            public abstract int Count { get; set; }
            public abstract int GetBinaryHashCode(out int binarySum);
            public abstract int GetHashCode(out int binarySum);
            public abstract int GetCharCount();
            public abstract int GetByteCount();
            public abstract void TrimExcess();
            public abstract int GetCapacity();
            public abstract void SetCapacity(int capacity);
            public abstract Content/*!*/ Clone();

            public abstract char GetChar(int index);
            public abstract byte GetByte(int index);
            public abstract string/*!*/ GetStringSlice(int start, int count);
            public abstract byte[]/*!*/ GetBinarySlice(int start, int count);

            public abstract int CompareTo(string/*!*/ str);
            public abstract int CompareTo(byte[]/*!*/ bytes);
            public abstract int ReverseCompareTo(Content/*!*/ str);
            
            // the owner of the result is the current owner:
            public abstract Content/*!*/ GetSlice(int start, int count);

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

            // write:
            public abstract void Append(char c, int repeatCount);
            public abstract void Append(byte b, int repeatCount);
            public abstract void Append(string/*!*/ str, int start, int count);
            public abstract void Append(char[]/*!*/ chars, int start, int count);
            public abstract void Append(byte[]/*!*/ bytes, int start, int count);
            public abstract void Append(Stream/*!*/ stream, int count);
            public abstract void AppendTo(Content/*!*/ str, int start, int count);

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
