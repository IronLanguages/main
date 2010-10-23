/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.IronStudio.Core.Repl {
    internal class Writer : TextWriter {
        private readonly Action<string> _writer;

        internal Writer(Action<string> writer) {
            _writer = writer;
        }

        public override void Write(char[] buffer, int index, int count) {
            _writer(new string(buffer, index, count));
        }

        public override void Write(string value) {
            _writer(value);
        }

        public override void Write(char value) {
            _writer(new String(value, 1));
        }

        public override Encoding Encoding {
            get { return Encoding.UTF8; }
        }

        public override object InitializeLifetimeService() {
            return null;
        }
    }
}
