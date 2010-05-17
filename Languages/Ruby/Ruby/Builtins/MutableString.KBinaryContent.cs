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
        /// Mutable byte array. Treats bytes like characters.
        /// </summary>
        [Serializable]
        private sealed class KBinaryContent : BinaryContent {
            internal KBinaryContent(byte[]/*!*/ data, MutableString owner) 
                : base(data, owner) {
            }

            internal KBinaryContent(byte[]/*!*/ data, int count, MutableString owner)
                : base(data, count, owner) {
            }

            protected override BinaryContent/*!*/ Create(byte[]/*!*/ data, MutableString owner) {
                return new KBinaryContent(data, owner);
            }

            public override int GetCharCount() {
                return GetByteCount();
            }

            public override Content/*!*/ SwitchToStringContent() {
                return this;
            }

            public override char GetChar(int index) {
                return (char)GetByte(index);
            }

            public override void SetChar(int index, char c) {
                // If we got the character from a string its encoding is compatible with the current encoding and thus all characters are single-byte.
                Debug.Assert(c <= 0xff);
                SetByte(index, (byte)c);
            }

            public override int OrdinalCompareTo(string/*!*/ str) {
                return Utils.ValueCompareTo(_data, _count, str);
            }

            public override int IndexOf(char c, int start, int count) {
                // If we got the character from a string its encoding is compatible with the current encoding and thus all characters are single-byte.
                Debug.Assert(c <= 0xff);
                count = Utils.NormalizeCount(_count, start, count);
                return count > 0 ? Array.IndexOf(_data, (byte)c, start, count) : -1;
            }

            public override int IndexOf(string/*!*/ str, int start, int count) {
                Debug.Assert(str.IsBinary());
                return Utils.IndexOf(_data, _count, str, start, count);
            }

            public override int LastIndexOf(char c, int start, int count) {
                // If we got the character from a string its encoding is compatible with the current encoding and thus all characters are single-byte.
                Debug.Assert(c <= 0xff);
                Utils.NormalizeLastIndexOfIndices(_count, ref start, ref count);
                return Array.LastIndexOf(_data, (byte)c, start, count);
            }

            public override int LastIndexOf(string/*!*/ str, int start, int count) {
                Debug.Assert(str.IsBinary());
                return Utils.LastIndexOf(_data, _count, str, start, count);
            }

        }
    }
}
