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
using System.Collections.Generic;
using System.Text;

namespace Cassini {
    class ByteString {
        byte[] _bytes;
        int _offset;
        int _length;

        public ByteString(byte[] bytes, int offset, int length) {
            _bytes = bytes;
            _offset = offset;
            _length = length;
        }

        public byte[] Bytes { get { return _bytes; } }
        public bool IsEmpty { get { return (_bytes == null || _length == 0); } }
        public int Length { get { return _length; } }
        public int Offset { get { return _offset; } }
        public byte this[int index] { get { return _bytes[_offset+index]; } }

        public string GetString() {
            return GetString(Encoding.UTF8);
        }

        public string GetString(Encoding enc) {
            if (IsEmpty) return string.Empty;
            return enc.GetString(_bytes, _offset, _length);
        }

        public byte[] GetBytes() {
            byte[] bytes = new byte[_length];
            if (_length > 0) Buffer.BlockCopy(_bytes, _offset, bytes, 0, _length);
            return bytes;
        }

        public int IndexOf(char ch) {
            return IndexOf(ch, 0);
        }

        public int IndexOf(char ch, int offset) {
            for (int i = offset; i < _length; i++) {
                if (this[i] == (byte)ch) return i;
            }
            return -1;
        }

        public ByteString Substring(int offset) {
            return Substring(offset, _length-offset);
        }

        public ByteString Substring(int offset, int len) {
            return new ByteString(_bytes, _offset+offset, len);
        }

        public ByteString[] Split(char sep) {
            var list = new List<ByteString>();

            int pos = 0;
            while (pos < _length) {
                int i = IndexOf(sep, pos);
                if (i < 0) {
                    break;
                }

                list.Add(Substring(pos, i-pos));
                pos = i+1;

                while (this[pos] == (byte)sep && pos < _length) {
                    pos++;
                }
            }

            if (pos < _length)
                list.Add(Substring(pos));

            return list.ToArray();
        }
    }
}
