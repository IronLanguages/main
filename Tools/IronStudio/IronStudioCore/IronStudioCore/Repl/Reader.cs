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
using System.IO;

namespace Microsoft.IronStudio.Core.Repl {
    class Reader : TextReader {
        private readonly Func<string> _readInput;
        private string _read;
        private int _curIndex;

        public Reader(Func<string> ReadInput) {
            _readInput = ReadInput;
        }

        public override int Read(char[] buffer, int index, int count) {
            ReadBuffer();

            int copying = Math.Min(count, _read.Length - _curIndex);
            _read.CopyTo(_curIndex, buffer, index, copying);
            _curIndex += copying;
            return copying;
        }

        public override int Read() {
            ReadBuffer();

            if (_read.Length == 0) {
                return -1;
            }

            return _read[_curIndex++];
        }

        private void ReadBuffer() {
            if (_read == null || _curIndex >= _read.Length) {
                _read = _readInput();
                _curIndex = 0;
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }
    }
}
