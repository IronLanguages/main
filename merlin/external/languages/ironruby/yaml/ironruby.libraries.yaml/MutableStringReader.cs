/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola.bini@gmail.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

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
