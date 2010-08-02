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
            _io.WriteBytes(new string(value, 1));
        }

        public override void Write(char[] buffer, int index, int count) {
            _io.WriteBytes(buffer, index, count);
        }

        public override void Flush() {
            _io.Flush();
        }
    }
}
