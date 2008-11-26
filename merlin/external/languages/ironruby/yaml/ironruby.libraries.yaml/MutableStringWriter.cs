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

namespace IronRuby.StandardLibrary.Yaml {

    internal class MutableStringWriter : TextWriter {
        private readonly MutableString _str = MutableString.Create("");

        public override Encoding Encoding {
            get {
                // TODO: return MutableString encoding
                throw new NotImplementedException();
            }
        }

        public override void  Write(char value) {
            _str.Append(value);
        }

        public override void Write(char[] buffer, int index, int count) {
            // TODO: MutableString needs Append(char[], index, count)
            _str.Append(new string(buffer), index, count);
        }

        internal MutableString String {
            get { return _str; }
        }
    }
}
