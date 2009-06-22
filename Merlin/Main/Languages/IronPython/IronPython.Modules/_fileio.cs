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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Runtime.Exceptions;

// TODO: Documentation copied from CPython is inadequate in some places and wrong in others.

[assembly: PythonModule("_fileio", typeof(IronPython.Modules.PythonFileIOModule))]
namespace IronPython.Modules {
    [Documentation("Fast implementation of io.FileIO.")]
    public static class PythonFileIOModule {
        [Documentation("file(name: str[, mode: str]) -> file IO object\n\n"
            + "Open a file.  The mode can be 'r', 'w' or 'a' for reading (default),\n"
            + "writing or appending.   The file will be created if it doesn't exist\n"
            + "when opened for writing or appending; it will be truncated when\n"
            + "opened for writing.  Add a '+' to the mode to allow simultaneous\n"
            + "reading and writing."
            )]
        [PythonType("_FileIO")]
        public class _FileIO : IDisposable, IWeakReferenceable {

            #region Fields and constructors

            private static readonly int DEFAULT_BUF_SIZE = 2;

            private Stream _readStream;
            private Stream _writeStream;
            private bool _closed;
            private string _mode;
            private WeakRefTracker _tracker;
            private PythonContext _context;

            public _FileIO(CodeContext/*!*/ context, string name, [DefaultParameterValue("r")]string mode, [DefaultParameterValue(true)]bool closefd) {
                if (!closefd) {
                    throw PythonOps.ValueError("Cannot use closefd=False with file name");
                }
                
                PlatformAdaptationLayer pal = PythonContext.GetContext(context).DomainManager.Platform;

                switch (mode) {
                    case "r":
                        _readStream = _writeStream = OpenFile(pal, name, FileMode.Open, FileAccess.Read, FileShare.None);
                        _mode = "r";
                        break;
                    case "w":
                        _readStream = _writeStream = OpenFile(pal, name, FileMode.Create, FileAccess.Write, FileShare.None);
                        _mode = "w";
                        break;
                    case "a":
                        _readStream = _writeStream = OpenFile(pal, name, FileMode.Append, FileAccess.Write, FileShare.None);
                        _mode = "w";
                        break;
                    case "r+":
                    case "+r":
                        _readStream = _writeStream = OpenFile(pal, name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                        _mode = "r+";
                        break;
                    case "w+":
                    case "+w":
                        _readStream = _writeStream = OpenFile(pal, name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        _mode = "r+";
                        break;
                    case "a+":
                    case "+a":
                        _readStream = OpenFile(pal, name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        _writeStream = OpenFile(pal, name, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        _mode = "r+";
                        break;
                    default:
                        bool foundMode = false, foundPlus = false;
                        foreach (char c in mode) {
                            switch (c) {
                                case 'r':
                                case 'w':
                                case 'a':
                                    if (foundMode) {
                                        throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
                                    } else {
                                        foundMode = true;
                                        continue;
                                    }
                                case '+':
                                    if (foundPlus) {
                                        throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
                                    } else {
                                        foundPlus = true;
                                        continue;
                                    }
                                default:
                                    throw PythonOps.ValueError("invalid mode: {0}", mode);
                            }
                        }

                        throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
                }

                _closed = false;
                _context = PythonContext.GetContext(context);
            }

            #endregion

            #region Public API

            public string __repr__() {
                StringBuilder sb = new StringBuilder("_fileio._FileIO(", 25);
                if (closed) {
                    sb.Append(-1);
                } else {
                    sb.Append(fileno());
                    sb.Append(", '");
                    sb.Append(_mode);
                }
                sb.Append("')");

                return sb.ToString();
            }

            [Documentation("close() -> None.  Close the file.\n\n"
                + "A closed file cannot be used for further I/O operations.  close() may be"
                + "called more than once without error.  Changes the fileno to -1."
                )]
            public void close() {
                if (_closed) {
                    return;
                }
                
                _closed = true;
                _readStream.Close();
                _readStream.Dispose();
                if (!object.ReferenceEquals(_readStream, _writeStream)) {
                    _writeStream.Close();
                    _writeStream.Dispose();
                }
            }

            [Documentation("True if the file is closed")]
            public bool closed {
                get {
                    return _closed;
                }
            }

            [Documentation("fileno() -> int. \"file descriptor\".\n\n"
                + "This is needed for lower-level file interfaces, such as the fcntl module."
                )]
            public int fileno() {
                EnsureOpen();

                return _context.FileManager.GetIdFromObject(this);
            }

            [Documentation("isatty() -> bool.  True if the file is connected to a tty device.")]
            public bool isatty() {
                EnsureOpen();

                return false;
            }

            [Documentation("String giving the file mode")]
            public string mode {
                get {
                    return _mode;
                }
            }

            [Documentation("read(size: int) -> bytes.  read at most size bytes, returned as bytes.\n\n"
                + "Only makes one system call, so less data may be returned than requested\n"
                + "In non-blocking mode, returns None if no data is available.\n"
                + "On end-of-file, returns ''."
                )]
            public Bytes read([DefaultParameterValue(0)]int size) {
                if (size < 0) {
                    return readall();
                }
                EnsureReadable();

                byte[] buffer = new byte[size];
                int bytesRead = _readStream.Read(buffer, 0, size);
                
                Array.Resize(ref buffer, bytesRead);
                return Bytes.Make(buffer);
            }

            [Documentation("readable() -> bool.  True if file was opened in a read mode.")]
            public bool readable() {
                EnsureOpen();

                return _readStream.CanRead;
            }

            [Documentation("readall() -> bytes.  read all data from the file, returned as bytes.\n\n"
                + "In non-blocking mode, returns as much as is immediately available,\n"
                + "or None if no data is available.  On end-of-file, returns ''."
                )]
            public Bytes readall() {
                EnsureReadable();

                int bufSize = DEFAULT_BUF_SIZE;
                byte[] buffer = new byte[bufSize];
                int bytesRead = _readStream.Read(buffer, 0, bufSize);

                if (bytesRead < bufSize) {
                    Array.Resize(ref buffer, bytesRead);
                    return Bytes.Make(buffer);
                }

                for (; bytesRead == bufSize; bufSize *= 2) {
                    Array.Resize(ref buffer, bufSize * 2);
                    bytesRead += _readStream.Read(buffer, bufSize, bufSize);
                }

                Array.Resize(ref buffer, bytesRead);
                return Bytes.Make(buffer);
            }

            [Documentation("readinto() -> Undocumented.  Don't use this; it may go away.")]
            public BigInteger readinto([NotNull]ArrayModule.PythonArray buffer) {
                EnsureReadable();

                return BigInteger.Create(buffer.FromStream(_readStream, 0, buffer.__len__() * buffer.itemsize));
            }

            public BigInteger readinto([NotNull]ByteArray buffer) {
                EnsureReadable();

                for (int i = 0; i < buffer.Count; i++) {
                    int b = _readStream.ReadByte();
                    if (b == -1) return i - 1;
                    buffer[i] = (byte)b;
                }
                return buffer.Count;
            }

            public BigInteger readinto([NotNull]PythonBuffer buffer) {
                EnsureReadable();

                throw PythonOps.TypeError("buffer is read-only");
            }

            public BigInteger readinto(object o) {
                EnsureReadable();

                throw PythonOps.TypeError("argument 1 must be read/write buffer, not {0}", DynamicHelpers.GetPythonType(o).Name);
            }

            [Documentation("seek(offset: int[, whence: int]) -> None.  Move to new file position.\n\n"
                + "Argument offset is a byte count.  Optional argument whence defaults to\n"
                + "0 (offset from start of file, offset should be >= 0); other values are 1\n"
                + "(move relative to current position, positive or negative), and 2 (move\n"
                + "relative to end of file, usually negative, although many platforms allow\n"
                + "seeking beyond the end of a file).\n"
                + "Note that not all file objects are seekable."
                )]
            public BigInteger seek(BigInteger offset, [DefaultParameterValue(0)]int whence) {
                EnsureOpen();

                return BigInteger.Create(_readStream.Seek(offset.ToInt64(), (SeekOrigin)whence));
            }

            public BigInteger seek(double offset, [DefaultParameterValue(0)]int whence) {
                EnsureOpen();

                throw PythonOps.TypeError("an integer is required");
            }

            [Documentation("seekable() -> bool.  True if file supports random-access.")]
            public bool seekable() {
                EnsureOpen();

                return _readStream.CanSeek;
            }

            [Documentation("tell() -> int.  Current file position")]
            public BigInteger tell() {
                EnsureOpen();

                return BigInteger.Create(_readStream.Position);
            }

            [Documentation("truncate([size: int]) -> None.  Truncate the file to at most size bytes.\n\n"
                + "Size defaults to the current file position, as returned by tell()."
                + "The current file position is changed to the value of size."
                )]
            public BigInteger truncate() {
                return truncate(tell());
            }

            public BigInteger truncate(BigInteger size) {
                EnsureWritable();

                _writeStream.SetLength(size.ToInt64());
                SeekToEnd();

                return size;
            }

            public BigInteger truncate(double size) {
                EnsureWritable();

                throw PythonOps.TypeError("an integer is required");
            }

            [Documentation("writable() -> bool.  True if file was opened in a write mode.")]
            public bool writable() {
                EnsureOpen();

                return _writeStream.CanWrite;
            }

            [Documentation("write(b: bytes) -> int.  Write bytes b to file, return number written.\n\n"
                + "Only makes one system call, so not all the data may be written.\n"
                + "The number of bytes actually written is returned."
                )]
            public BigInteger write([NotNull]byte[] b) {
                EnsureWritable();

                _writeStream.Write(b, 0, b.Length);
                SeekToEnd();

                return BigInteger.Create(b.Length);
            }

            public BigInteger write([NotNull]Bytes b) {
                return write(b._bytes);
            }

            public BigInteger write([NotNull]ICollection<byte> b) {
                EnsureWritable();

                int len = b.Count;
                byte[] bytes = new byte[len];
                b.CopyTo(bytes, 0);
                _writeStream.Write(bytes, 0, len);
                SeekToEnd();

                return BigInteger.Create(len);
            }

            public BigInteger write([NotNull]string s) {
                return write(s.MakeByteArray());
            }

            public BigInteger write(object b) {
                if (b is PythonBuffer) {
                    return write(((PythonBuffer)b).ToString());
                } else if (b is ArrayModule.PythonArray) {
                    return write(((ArrayModule.PythonArray)b).ToByteArray());
                }

                EnsureWritable();

                throw PythonOps.TypeError("expected a readable buffer object");
            }

            #endregion

            #region IDisposable methods

            public void Dispose() {
                close();

                PythonFileManager myManager = _context.RawFileManager;
                if (myManager != null) {
                    myManager.Remove(this);
                }
            }

            #endregion

            #region IWeakReferenceable Members

            public WeakRefTracker GetWeakRef() {
                return _tracker;
            }

            public bool SetWeakRef(WeakRefTracker value) {
                _tracker = value;
                return true;
            }

            public void SetFinalizer(WeakRefTracker value) {
                SetWeakRef(value);
            }

            #endregion

            #region Private implementation details

            private static Stream OpenFile(PlatformAdaptationLayer pal, string name, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) {
                if (pal.DirectoryExists(name)) {
                    // TODO: properly set errno
                    throw PythonOps.IOError("[Errno 13] Permission denied");
                }
                return pal.OpenInputFileStream(name, fileMode, fileAccess, fileShare);
            }
            
            private void EnsureOpen() {
                if (closed) {
                    throw PythonOps.ValueError("I/O operation on closed file");
                }
            }

            // Implied call to EnsureOpen()
            private void EnsureReadable() {
                if (!readable()) {
                    throw PythonOps.ValueError("File not open for reading");
                }
            }

            // Implied call to EnsureOpen()
            private void EnsureWritable() {
                if (!writable()) {
                    throw PythonOps.ValueError("File not open for writing");
                }
            }

            private void SeekToEnd() {
                if (!object.ReferenceEquals(_readStream, _writeStream)) {
                    _readStream.Seek(_writeStream.Position, SeekOrigin.Begin);
                }
            }

            #endregion
        }
    }
}
