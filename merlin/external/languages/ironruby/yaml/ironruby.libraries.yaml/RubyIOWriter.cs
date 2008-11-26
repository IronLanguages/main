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
    internal class RubyIOWriter : TextWriter {
        private readonly RubyIO/*!*/ _io;

        internal RubyIOWriter(RubyIO/*!*/ io) {
            Assert.NotNull(io);
            _io = io;
        }

        public override Encoding Encoding {
            get {
                // TODO: return RubyIO encoding
                throw new NotImplementedException();
            }
        }

        public override void Write(char value) {
            _io.Write(new string(value, 1));
        }

        public override void Write(char[] buffer, int index, int count) {
            _io.Write(buffer, index, count);
        }

        public override void Flush() {
            _io.Flush();
        }
    }
}
