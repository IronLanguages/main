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
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Scripting.Metadata {
    /// <summary>
    /// Represents a block in memory.
    /// </summary>
    public unsafe sealed class MemoryBlock {
        private readonly byte* _pointer;

        private readonly int _length;
        private readonly object _owner;

        // [SecurityCritical]
        internal MemoryBlock(object owner, byte* pointer, int length) {
            _pointer = pointer;
            _length = length;
            _owner = owner;
        }

        public MemoryBlock GetRange(int start, int length) {
            if (start < 0) {
                throw new ArgumentOutOfRangeException("start");
            }
            if (length < 0 || length > _length - start) {
                throw new ArgumentOutOfRangeException("length");
            }
            return new MemoryBlock(_owner, _pointer + start, length);
        }

        [CLSCompliant(false)]
        public byte* Pointer {
            get { return _pointer; }
        }

        public int Length {
            get { return _length; }
        }

        public byte ReadByte(int offset) {
            if (offset < 0 || offset > _length - sizeof(byte)) {
                throw new ArgumentOutOfRangeException("offset");
            }
            var result = *(_pointer + offset);
            GC.KeepAlive(_owner);
            return result;
        }

        public short ReadInt16(int offset) {
            if (offset < 0 || offset > _length - sizeof(short)) {
                throw new ArgumentOutOfRangeException("offset");
            }
            var result = *(short*)(_pointer + offset);
            GC.KeepAlive(_owner);
            return result;
        }

        public int ReadInt32(int offset) {
            if (offset < 0 || offset > _length - sizeof(int)) {
                throw new ArgumentOutOfRangeException("offset");
            }
            var result = *(int*)(_pointer + offset);
            GC.KeepAlive(_owner);
            return result;
        }

        public long ReadInt64(int offset) {
            if (offset < 0 || offset > _length - sizeof(long)) {
                throw new ArgumentOutOfRangeException("offset");
            }
            var result = *(long*)(_pointer + offset);
            GC.KeepAlive(_owner);
            return result;
        }

        public Guid ReadGuid(int offset) {
            if (offset < 0 || offset > _length - sizeof(Guid)) {
                throw new ArgumentOutOfRangeException("offset");
            }
            var result = *(Guid*)(_pointer + offset);
            GC.KeepAlive(_owner);
            return result;
        }

        public void Read(int offset, byte[] result) {
            if (result == null) {
                throw new ArgumentNullException("result");
            }
            if (offset < 0 || offset > _length - result.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }

            byte* pIter = _pointer + offset;
            byte* end = pIter + result.Length;

            fixed (byte* pResult = result) {
                byte* resultIter = pResult;
                while (pIter < end) {
                    *resultIter = *pIter;
                    pIter++;
                    resultIter++;
                }
            }
            GC.KeepAlive(_owner);
        }

        public string ReadUtf16(int offset, int byteCount) {
            if (offset < 0 || offset > _length - byteCount) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (byteCount <= 1) {
                return String.Empty;
            }
            var result = Marshal.PtrToStringUni((IntPtr)(_pointer + offset), byteCount / 2);
            GC.KeepAlive(_owner);
            return result;
        }

        public string ReadAscii(int offset) {
            return ReadAscii(offset, _length - offset);
        }

        public string ReadAscii(int offset, int maxByteCount) {
            if (maxByteCount < 0) {
                throw new ArgumentOutOfRangeException("maxByteCount");
            }
            if (offset < 0 || offset > _length - maxByteCount) {
                throw new ArgumentOutOfRangeException("offset");
            }
            
            sbyte* pStart = (sbyte*)_pointer + offset;
            sbyte* pEnd = pStart + maxByteCount;
            sbyte* pIter = pStart;

            while (*pIter != '\0' && pIter < pEnd) {
                pIter++;
            }

            return new string((sbyte*)pStart, 0, (int)(pIter - pStart), Encoding.ASCII);
        }

        [CLSCompliant(false)]
        public sbyte ReadSByte(int offset) {
            return unchecked((sbyte)ReadByte(offset));
        }

        [CLSCompliant(false)]
        public ushort ReadUInt16(int offset) {
            return unchecked((ushort)ReadInt16(offset));
        }

        public char ReadChar(int offset) {
            return unchecked((char)ReadInt16(offset));
        }

        [CLSCompliant(false)]
        public uint ReadUInt32(int offset) {
            return unchecked((uint)ReadInt32(offset));
        }

        [CLSCompliant(false)]
        public ulong ReadUInt64(int offset) {
            return unchecked((ulong)ReadInt64(offset));
        }

        public float ReadSingle(int offset) {
            return unchecked((float)ReadInt32(offset));
        }

        public double ReadDouble(int offset) {
            return unchecked((double)ReadInt64(offset));
        }

        #region Metadata-specific

        internal uint ReadReference(int offset, bool smallRefSize) {
            if (smallRefSize) {
                return ReadUInt16(offset);
            }
            return ReadUInt32(offset);
        }

        internal int ReadCompressedInt32(int offset, out int numberOfBytesRead) {
            byte headerByte = ReadByte(offset);
            int result;
            if ((headerByte & 0x80) == 0x00) {
                result = headerByte;
                numberOfBytesRead = 1;
            } else if ((headerByte & 0x40) == 0x00) {
                result = ((headerByte & 0x3f) << 8) | ReadByte(offset + 1);
                numberOfBytesRead = 2;
            } else if (headerByte == 0xFF) {
                throw new BadImageFormatException();
            } else {
                // TODO: read int32
                result = ((headerByte & 0x3f) << 24) | (ReadByte(offset + 1) << 16) | (ReadByte(offset + 2) << 8) | ReadByte(offset + 3);
                numberOfBytesRead = 4;
            }
            Debug.Assert(result >= 0);
            return result;
        }

        internal MetadataName ReadName(uint offset) {
            if (offset >= _length) {
                throw new BadImageFormatException();
            }

            // there is always a terminating zero:
            Debug.Assert(_pointer[_length - 1] == 0);

            return new MetadataName(_pointer + offset, _owner);
        }

        //  Returns RowNumber
        internal int BinarySearchForSlot(int numberOfRows, int numberOfChildren, int rowSize, int referenceOffset, uint childRid, bool isReferenceSmall) {
            int startRowNumber = 0;          // inclusive     
            int endRowNumber = numberOfRows; // exclusive  

            uint startRid = ReadReference(startRowNumber * rowSize + referenceOffset, isReferenceSmall);
            uint endRid = (uint)numberOfChildren + 1;

            if (childRid < startRid || childRid >= endRid) {
                return -1; // error
            }

            while (endRowNumber - startRowNumber > 1) {
                Debug.Assert(childRid >= startRid && childRid < endRid);

                int midRowNumber = (startRowNumber + endRowNumber) / 2;
                uint midValue = ReadReference(midRowNumber * rowSize + referenceOffset, isReferenceSmall);
                if (childRid > midValue) {
                    startRowNumber = midRowNumber;
                    startRid = midValue;
                } else if (childRid < midValue) {
                    endRowNumber = midRowNumber;
                    endRid = midValue;
                } else {
                    startRowNumber = midRowNumber;
                    startRid = midValue;
                    break;
                }
            }

            // We found a slot whose range include searched reference.
            // However this slot might be empty, so we need to find first non-empty slot that follows.
            while (startRowNumber < numberOfRows - 1) {
                if (startRid != ReadReference((startRowNumber + 1) * rowSize + referenceOffset, isReferenceSmall)) {
                    break;
                }

                startRowNumber++;
            }

            return startRowNumber;
        }

        //  Returns RowNumber....
        internal int BinarySearchReference(int numberOfRows, int rowSize, int referenceOffset, uint referenceValue, bool isReferenceSmall) {
            int startRowNumber = 0;
            int endRowNumber = numberOfRows - 1;
            while (startRowNumber <= endRowNumber) {
                int midRowNumber = (startRowNumber + endRowNumber) / 2;
                uint midReferenceValue = ReadReference(midRowNumber * rowSize + referenceOffset, isReferenceSmall);
                if (referenceValue > midReferenceValue) {
                    startRowNumber = midRowNumber + 1;
                } else if (referenceValue < midReferenceValue) {
                    endRowNumber = midRowNumber - 1;
                } else {
                    return midRowNumber;
                }
            }
            return -1;
        }

        //  Returns RowNumber....
        internal int LinearSearchReference(int rowSize, int referenceOffset, uint referenceValue, bool isReferenceSmall) {
            int currOffset = referenceOffset;
            int totalSize = (int)_length;
            while (currOffset < totalSize) {
                uint currReference = ReadReference(currOffset, isReferenceSmall);
                if (currReference == referenceValue) {
                    return currOffset / rowSize;
                }
                currOffset += rowSize;
            }
            return -1;
        }

        #endregion
    }
}
