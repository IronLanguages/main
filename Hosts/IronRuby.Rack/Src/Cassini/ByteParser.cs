/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections;
using System.Text;

namespace Cassini {
    class ByteParser {
        byte[] _bytes;
        int _pos;

        public ByteParser(byte[] bytes) {
            _bytes = bytes;
            _pos = 0;
        }

        public int CurrentOffset { get { return _pos; } }

        public ByteString ReadLine() {
            ByteString line = null;

            for (int i = _pos; i < _bytes.Length; i++) {
                if (_bytes[i] == (byte)'\n') {
                    int len = i-_pos;
                    if (len > 0 && _bytes[i-1] == (byte)'\r') {
                        len--;
                    }

                    line = new ByteString(_bytes, _pos, len);
                    _pos = i+1;
                    return line;
                }
            }

            if (_pos < _bytes.Length) {
                line = new ByteString(_bytes, _pos, _bytes.Length-_pos);
            }

            _pos = _bytes.Length;
            return line;
        }
    }
}
