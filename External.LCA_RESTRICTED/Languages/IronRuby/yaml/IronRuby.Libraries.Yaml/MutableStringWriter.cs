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

    internal class MutableStringWriter : TextWriter {
        private readonly MutableString _storage;

        public MutableStringWriter(MutableString/*!*/ storage) {
            Assert.NotNull(storage);
            _storage = storage;
        }

        public override Encoding/*!*/ Encoding {
            get { return _storage.Encoding.Encoding; }
        }

        public override void  Write(char value) {
            _storage.Append(value);
        }

        public override void Write(char[]/*!*/ buffer, int index, int count) {
            _storage.Append(buffer, index, count);
        }

        internal MutableString/*!*/ String {
            get { return _storage; }
        }
    }
}
