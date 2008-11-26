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
using System.Dynamic.Utils;

namespace IronRuby.StandardLibrary.Yaml {
    internal class RubyIOReader : TextReader {
        private readonly RubyIO _io;

        internal RubyIOReader(RubyIO io) {
            _io = io;
        }

        public override int Peek() {
            return _io.PeekByteNormalizeEoln();
        }
        public override int Read() {
            return _io.ReadByteNormalizeEoln();
        }
    }
}
