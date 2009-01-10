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

namespace IronRuby.Builtins {
    public partial class MutableString {
        [Serializable]
        private abstract class Content {
            protected MutableString/*!*/ _owner;

            #region Utils

            internal void SetOwner(MutableString/*!*/ owner) {
                Assert.NotNull(owner);
                _owner = owner;
            }

            protected Content(MutableString/*!*/ owner) {
                Assert.NotNull(owner);
                _owner = owner;
            }

            protected BinaryContent/*!*/ WrapContent(byte[]/*!*/ bytes) {
                var result = new BinaryContent(_owner, new List<byte>(bytes)); // TODO: do not copy
                _owner.SetContent(result);
                return result;
            }
            
            protected StringBuilderContent/*!*/ WrapContent(StringBuilder/*!*/ sb) {
                var result = new StringBuilderContent(_owner, sb);
                _owner.SetContent(result);
                return result;
            }

            #endregion

            public abstract string/*!*/ ConvertToString();
            public abstract byte[]/*!*/ ConvertToBytes();

            public abstract byte[]/*!*/ ToByteArray();
            public abstract GenericRegex/*!*/ ToRegularExpression(RubyRegexOptions options);

            // returns self if there are no characters to be escaped:
            public abstract Content/*!*/ EscapeRegularExpression();

            // read:
            public abstract bool IsBinary { get; }
            public abstract bool IsEmpty { get; }
            public abstract int Length { get; }
            public abstract int GetCharCount();
            public abstract int GetByteCount();
            public abstract void GetDebugView(out string/*!*/ value, out string/*!*/ type);
            public abstract Content/*!*/ Clone(MutableString/*!*/ newOwner);

            public abstract char GetChar(int index);
            public abstract byte GetByte(int index);
            public abstract byte PeekByte(int index);
            public abstract char PeekChar(int index);
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
            public abstract Content/*!*/ Append(char c, int repeatCount);
            public abstract Content/*!*/ Append(byte b, int repeatCount);
            public abstract Content/*!*/ Append(string/*!*/ str, int start, int count);
            public abstract Content/*!*/ Append(byte[]/*!*/ bytes, int start, int count);
            public abstract Content/*!*/ AppendTo(Content/*!*/ str, int start, int count);

            public abstract Content/*!*/ AppendFormat(IFormatProvider provider, string/*!*/ format, object[]/*!*/ args);
            
            public abstract Content/*!*/ Insert(int index, char c);
            public abstract Content/*!*/ Insert(int index, byte b);
            public abstract Content/*!*/ Insert(int index, string/*!*/ str, int start, int count);
            public abstract Content/*!*/ Insert(int index, byte[]/*!*/ bytes, int start, int count);
            public abstract Content/*!*/ InsertTo(Content/*!*/ str, int index, int start, int count);

            public abstract Content/*!*/ SetItem(int index, byte b);
            public abstract Content/*!*/ SetItem(int index, char c);

            public abstract Content/*!*/ Remove(int start, int count);
        }
    }
}
