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
using System.IO;
using System.Text;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Yaml {
    internal class MutableStringReader : TextReader {
        private readonly MutableString/*!*/ _str;
        private int _pos = 0;

        internal MutableStringReader(MutableString/*!*/ str) {
            Assert.NotNull(str);
            _str = str;
        }

        public override int Peek() {
            return _str.GetChar(_pos);
        }

        public override int Read() {
            return _pos < _str.Length ? _str.GetChar(_pos++) : -1;
        }

        public override int Read(char[]/*!*/ buffer, int index, int count) {
            int read = _str.Length - _pos;
            if (read > 0) {
                if (read > count) {
                    read = count;
                }
                _str.ConvertToString().CopyTo(_pos, buffer, index, read);
                _pos += read;
            }
            return read;
        }
    }
}
