/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System;
using System.Diagnostics;

namespace Microsoft.Scripting.Metadata {
    /// <summary>
    /// Reads data from a memory block. Maintains a position.
    /// </summary>
    public class MemoryReader {
        private readonly MemoryBlock _block;
        private int _position;

        public MemoryReader(MemoryBlock block) {
            if (block == null) {
                throw new ArgumentNullException("block");
            }
            _block = block;
        }

        public MemoryBlock Block {
            get { return _block; }
        }

        public int Position {
            get { return _position; }
        }

        public int RemainingBytes {
            get { return (int)_block.Length - _position; }
        }

        public MemoryBlock GetRemainingBlock() {
            return _block.GetRange(_position, RemainingBytes);
        }

        public void Seek(int position) {
            if (position < 0 || position > _block.Length) {
                throw new BadImageFormatException();
            }
            _position = position;
        }

        public void SeekRelative(int offset) {
            Seek(_position + offset);
        }

        internal void Align(int alignment) {
            Debug.Assert(alignment > 0 && (alignment & 1) == 0);
            int remainder = _position & (alignment - 1);
            if (remainder != 0) {
                SeekRelative(alignment - remainder);
            }
        }

        public Char ReadChar() {
            var result = _block.ReadChar(_position);
            _position += sizeof(char);
            return result;
        }

        [CLSCompliant(false)]
        public SByte ReadSByte() {
            var result = _block.ReadSByte(_position);
            _position += sizeof(sbyte);
            return result;
        }

        public short ReadInt16() {
            var result = _block.ReadInt16(_position);
            _position += sizeof(short);
            return result;
        }

        public int ReadInt32() {
            var result = _block.ReadInt32(_position);
            _position += sizeof(int);
            return result;
        }

        public long ReadInt64() {
            var result = _block.ReadInt64(_position);
            _position += sizeof(long);
            return result;
        }

        public byte ReadByte() {
            var result = _block.ReadByte(_position);
            _position += sizeof(byte);
            return result;
        }

        [CLSCompliant(false)]
        public ushort ReadUInt16() {
            var result = _block.ReadUInt16(_position);
            _position += sizeof(ushort);
            return result;
        }

        [CLSCompliant(false)]
        public uint ReadUInt32() {
            var result = _block.ReadUInt32(_position);
            _position += sizeof(uint);
            return result;
        }

        [CLSCompliant(false)]
        public ulong ReadUInt64() {
            var result = _block.ReadUInt64(_position);
            _position += sizeof(ulong);
            return result;
        }

        public Single ReadSingle() {
            var result = _block.ReadSingle(_position);
            _position += sizeof(Single);
            return result;
        }

        public Double ReadDouble() {
            var result = _block.ReadDouble(_position);
            _position += sizeof(Double);
            return result;
        }

        /// <summary>
        /// Reads zero terminated sequence of bytes of given maximal length and converts it into an ASCII string.
        /// </summary>
        public string ReadAscii(int maxByteCount) {
            int current = _position;
            string result = _block.ReadAscii(current, maxByteCount);
            _position = current + result.Length + 1; // terminating \0
            return result;
        }
    }
}
