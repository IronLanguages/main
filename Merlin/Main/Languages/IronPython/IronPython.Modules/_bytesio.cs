/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;
using IronPython.Runtime.Exceptions;

[assembly: PythonModule("_bytesio", typeof(IronPython.Modules.PythonBytesIOModule))]
namespace IronPython.Modules {
    public static class PythonBytesIOModule {
        /// <summary>
        /// BytesIO([initializer]) -> object
        /// 
        /// Create a buffered I/O implementation using an in-memory bytes
        /// buffer, ready for reading and writing.
        /// </summary>
        [PythonType("_BytesIO")]
        public class _BytesIO : IEnumerable, IEnumerator, IDisposable {
            #region Fields and constructors

            private static readonly int DEFAULT_BUF_SIZE = 20;

            private byte[] _data;
            private int _pos, _length;

            public _BytesIO([DefaultParameterValue(null)]object buffer) {
                __init__(buffer);
            }

            public void __init__([DefaultParameterValue(null)]object buffer) {
                if (Object.ReferenceEquals(_data, null)) {
                    _data = new byte[DEFAULT_BUF_SIZE];
                }

                _pos = _length = 0;
                if (buffer != null) {
                    DoWrite(buffer);
                    _pos = 0;
                }
            }

            #endregion

            #region Public API

            /// <summary>
            /// close() -> None.  Disable all I/O operations.
            /// </summary>
            public void close() {
                _data = null;
            }

            /// <summary>
            /// True if the file is closed.
            /// </summary>
            public bool closed {
                get {
                    return _data == null;
                }
            }

            /// <summary>
            /// flush() -> None.  Does nothing.
            /// </summary>
            public void flush() { }

            /// <summary>
            /// getvalue() -> bytes.
            /// 
            /// Retrieve the entire contents of the BytesIO object.
            /// </summary>
            public Bytes getvalue() {
                EnsureOpen();

                if (_length == 0) {
                    return Bytes.Empty;
                }

                byte[] arr = new byte[_length];
                Array.Copy(_data, arr, _length);
                return Bytes.Make(arr);
            }

            [Documentation("isatty() -> False\n\n"
                + "Always returns False since BytesIO objects are not connected\n"
                + "to a TTY-like device."
                )]
            public bool isatty() {
                EnsureOpen();

                return false;
            }

            [Documentation("read([size]) -> read at most size bytes, returned as a bytes object.\n\n"
                + "If the size argument is negative, read until EOF is reached.\n"
                + "Return an empty string at EOF."
                )]
            public Bytes read([DefaultParameterValue(-1)]int size) {
                EnsureOpen();

                int len = Math.Max(0, _length - _pos);
                if (size >= 0) {
                    len = Math.Min(len, size);
                }
                if (len == 0) {
                    return Bytes.Empty;
                }

                byte[] arr = new byte[len];
                Array.Copy(_data, _pos, arr, 0, len);
                _pos += len;

                return Bytes.Make(arr);
            }

            public Bytes read(object size) {
                if (size == null) {
                    return read(-1);
                }

                EnsureOpen();

                throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
            }

            [Documentation("read1(size) -> read at most size bytes, returned as a bytes object.\n\n"
                + "If the size argument is negative or omitted, read until EOF is reached.\n"
                + "Return an empty string at EOF."
                )]
            public Bytes read1(int size) {
                return read(size);
            }

            public Bytes read1(object size) {
                return read(size);
            }

            public bool readable() {
                return true;
            }

            [Documentation("readinto(array_or_bytearray) -> int.  Read up to len(b) bytes into b.\n\n"
                + "Returns number of bytes read (0 for EOF)."
                )]
            public int readinto([NotNull]ByteArray buffer) {
                EnsureOpen();

                int len = Math.Min(_length - _pos, buffer.Count);
                for (int i = 0; i < len; i++) {
                    buffer[i] = _data[_pos++];
                }

                return len;
            }

            public int readinto([NotNull]ArrayModule.PythonArray buffer) {
                EnsureOpen();

                int len = Math.Min(_length - _pos, buffer.__len__() * buffer.itemsize);
                int tailLen = len % buffer.itemsize;
                buffer.FromStream(new MemoryStream(_data, _pos, len - tailLen, false, false), 0);
                _pos += len - tailLen;

                if (tailLen != 0) {
                    byte[] tail = buffer.RawGetItem(len / buffer.itemsize);
                    for (int i = 0; i < tailLen; i++) {
                        tail[i] = _data[_pos++];
                    }
                    buffer.FromStream(new MemoryStream(tail), len / buffer.itemsize);
                }

                return len;
            }

            [Documentation("readline([size]) -> next line from the file, as bytes.\n\n"
                + "Retain newline.  A non-negative size argument limits the maximum\n"
                + "number of bytes to return (an incomplete line may be returned then).\n"
                + "Return an empty string at EOF."
                )]
            public Bytes readline([DefaultParameterValue(-1)]int size) {
                EnsureOpen();
                if (_pos >= _length || size == 0) {
                    return Bytes.Empty;
                }

                int origPos = _pos;
                while ((size < 0 || _pos - origPos < size) && _pos < _length) {
                    if (_data[_pos] == '\n') {
                        _pos++;
                        break;
                    }
                    _pos++;
                }

                byte[] arr = new byte[_pos - origPos];
                Array.Copy(_data, origPos, arr, 0, _pos - origPos);
                return Bytes.Make(arr);
            }

            public Bytes readline(object size) {
                if (size == null) {
                    return readline(-1);
                }

                EnsureOpen();

                throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
            }

            [Documentation("readlines([size]) -> list of bytes objects, each a line from the file.\n\n"
                + "Call readline() repeatedly and return a list of the lines so read.\n"
                + "The optional size argument, if given, is an approximate bound on the\n"
                + "total number of bytes in the lines returned."
                )]
            public List readlines([DefaultParameterValue(-1)]int size) {
                EnsureOpen();

                List lines = new List();
                for (Bytes line = readline(-1); line.Count > 0; line = readline(-1)) {
                    lines.append(line); 
                    if (size > 0) {
                        size -= line.Count;
                        if (size <= 0) {
                            break;
                        }
                    }
                }

                return lines;
            }

            public List readlines(object size) {
                if (size == null) {
                    return readlines(-1);
                }

                EnsureOpen();

                throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
            }

            [Documentation("seek(pos, whence=0) -> int.  Change stream position.\n\n"
                + "Seek to byte offset pos relative to position indicated by whence:\n"
                + "     0  Start of stream (the default).  pos should be >= 0;\n"
                + "     1  Current position - pos may be negative;\n"
                + "     2  End of stream - pos usually negative.\n"
                + "Returns the new absolute position."
                )]
            public int seek(int pos, [DefaultParameterValue(0)]int whence) {
                EnsureOpen();
                
                switch (whence) {
                    case 0:
                        if (pos < 0) {
                           throw PythonOps.ValueError("negative seek value {0}", pos);
                        }
                        _pos = pos;
                        return _pos;
                    case 1:
                        _pos = Math.Max(0, _pos + pos);
                        return _pos;
                    case 2:
                        _pos = Math.Max(0, _length + pos);
                        return _pos;
                    default:
                        throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1 or 2)", whence);
                }
            }

            public int seek(CodeContext/*!*/ context, object pos, [DefaultParameterValue(0)]object whence) {
                EnsureOpen();

                if (pos == null || whence == null) {
                    throw PythonOps.TypeError("an integer is required");
                }

                int intPos;
                if (pos is int) {
                    intPos = (int)pos;
                } else if (pos is Extensible<int>) {
                    intPos = ((Extensible<int>)pos).Value;
                } else if (pos is BigInteger) {
                    intPos = ((BigInteger)pos).ToInt32();
                } else if (pos is Extensible<BigInteger>) {
                    intPos = ((Extensible<BigInteger>)pos).Value.ToInt32();
                } else if (pos is double || pos is Extensible<double>) {
                    throw PythonOps.TypeError("position argument must be an integer");
                } else if (PythonContext.GetContext(context).PythonOptions.Python30) {
                    throw PythonOps.TypeError("'{0}' object cannot be interpreted as an integer", PythonTypeOps.GetOldName(pos));
                } else {
                    throw PythonOps.TypeError("an integer is required");
                }

                if (whence is int) {
                    return seek(intPos, (int)whence);
                } else if (whence is Extensible<int>) {
                    return seek(intPos, ((Extensible<int>)pos).Value);
                }else if (whence is BigInteger) {
                    return seek(intPos, ((BigInteger)whence).ToInt32());
                } else if (whence is Extensible<BigInteger>) {
                    return seek(intPos, ((Extensible<BigInteger>)whence).Value.ToInt32());
                } else if (whence is double || whence is Extensible<double>) {
                    if (PythonContext.GetContext(context).PythonOptions.Python30) {
                        throw PythonOps.TypeError("integer argument expected, got float");
                    } else {
                        PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "integer argument expected, got float");
                        return seek(intPos, Converter.ConvertToInt32(whence));
                    }
                } else if (PythonContext.GetContext(context).PythonOptions.Python30) {
                    throw PythonOps.TypeError("'{0}' object cannot be interpreted as an integer", PythonTypeOps.GetOldName(whence));
                } else {
                    throw PythonOps.TypeError("an integer is required");
                }
            }

            public Boolean seekable() {
                return true;
            }

            [Documentation("tell() -> current file position, an integer")]
            public int tell() {
                EnsureOpen();

                return _pos;
            }

            [Documentation("truncate([size]) -> int.  Truncate the file to at most size bytes.\n\n"
                + "Size defaults to the current file position, as returned by tell().\n"
                + "Returns the new size.  Imply an absolute seek to the position size."
                )]
            public int truncate() {
                return truncate(_pos);
            }

            public int truncate(int size) {
                EnsureOpen();
                if (size < 0) {
                    throw PythonOps.ValueError("negative size value {0}", size);
                }

                _length = Math.Min(_length, size);
                return seek(size, 0);
            }

            public int truncate(object size) {
                if (size == null) {
                    return truncate();
                }

                EnsureOpen();

                throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
            }

            public bool writable() {
                return true;
            }

            [Documentation("write(bytes) -> int.  Write bytes to file.\n\n"
                + "Return the number of bytes written."
                )]
            public int write(object bytes) {
                EnsureOpen();

                return DoWrite(bytes);
            }

            [Documentation("writelines(sequence_of_strings) -> None.  Write strings to the file.\n\n"
                + "Note that newlines are not added.  The sequence can be any iterable\n"
                + "object producing strings. This is equivalent to calling write() for\n"
                + "each string."
                )]
            public void writelines([NotNull]IEnumerable lines) {
                EnsureOpen();

                IEnumerator en = lines.GetEnumerator();
                while (en.MoveNext()) {
                    DoWrite(en.Current);
                }
            }

            #endregion

            #region IDisposable methods

            public void Dispose() {
                close();
            }

            #endregion
            
            #region IEnumerable methods

            public IEnumerator GetEnumerator() {
                return this;
            }

            #endregion
            
            #region IEnumerator methods

            private object _current = null;

            public object Current {
                get {
                    EnsureOpen();
                    return _current;
                }
            }

            public bool MoveNext() {
                Bytes line = readline(-1);
                if (line.Count == 0) {
                    return false;
                }
                _current = line;
                return true;
            }

            public void Reset() {
                seek(0, 0);
                _current = null;
            }

            #endregion

            #region Private implementation details

            private int DoWrite(byte[] bytes) {
                if (bytes.Length == 0) {
                    return 0;
                }

                EnsureSizeSetLength(_pos + bytes.Length);
                Array.Copy(bytes, 0, _data, _pos, bytes.Length);

                _pos += bytes.Length;
                return bytes.Length;
            }

            private int DoWrite(ICollection<byte> bytes) {
                int nbytes = bytes.Count;
                if (nbytes == 0) {
                    return 0;
                }

                EnsureSizeSetLength(_pos + nbytes);
                bytes.CopyTo(_data, _pos);

                _pos += nbytes;
                return nbytes;
            }

            private int DoWrite(IEnumerable bytes) {
                int origPos = _pos;
                IEnumerator en = ((IEnumerable)bytes).GetEnumerator();

                if (en.MoveNext()) {
                    byte b = ByteOps.GetByte(en.Current);
                    EnsureSizeSetLength(_pos + 1);
                    _data[_pos++] = b;
                } else {
                    return 0;
                }

                while (en.MoveNext()) {
                    byte b = ByteOps.GetByte(en.Current);
                    EnsureSize(_pos + 1);
                    _data[_pos++] = b;
                }

                _length = Math.Max(_length, _pos);
                return _pos - origPos;
            }

            private int DoWrite(object bytes) {
                if (bytes is byte[]) {
                    return DoWrite((byte[])bytes);
                } else if (bytes is Bytes) {
                    return DoWrite(((Bytes)bytes)._bytes);
                } else if (bytes is ArrayModule.PythonArray) {
                    return DoWrite(((ArrayModule.PythonArray)bytes).ToByteArray());
                } else if (bytes is PythonBuffer) {
                    return DoWrite(((PythonBuffer)bytes).ToString());
                } else if (bytes is ICollection<byte>) {
                    return DoWrite((ICollection<byte>)bytes);
                } else if (bytes is string) {
                    return DoWrite((IEnumerable)bytes);
                }

                throw PythonOps.TypeError("expected a readable buffer object");
            }

            private void EnsureOpen() {
                if (closed) {
                    throw PythonOps.ValueError("I/O operation on closed file.");
                }
            }

            private void EnsureSize(int size) {
                Debug.Assert(size > 0);

                if (_data.Length < size) {
                    if (size <= DEFAULT_BUF_SIZE) {
                        size = DEFAULT_BUF_SIZE;
                    } else {
                        size = Math.Max(size, _data.Length * 2);
                    }

                    byte[] oldBuffer = _data;
                    _data = new byte[size];
                    Array.Copy(oldBuffer, _data, _length);
                }
            }

            private void EnsureSizeSetLength(int size) {
                Debug.Assert(size >= _pos);
                Debug.Assert(_length <= _data.Length);

                if (_data.Length < size) {
                    // EnsureSize is guaranteed to resize, so we need not write any zeros here.
                    EnsureSize(size);
                    _length = size;
                    return;
                }

                // _data[_pos:size] is about to be overwritten, so we only need to zero out _data[_length:_pos]
                while (_length < _pos) {
                    _data[_length++] = 0;
                }

                _length = Math.Max(_length, size);
            }

            #endregion
        }
    }
}
