/* **************************************************************************
 *
 * Copyright 2012 Jeff Hardy
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * *************************************************************************/

using System.Collections.Generic;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using System;
using System.IO;
using System.Linq;
using Ionic.BZip2;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules.Bz2 {
    public static partial class Bz2Module {
        [PythonType]
        public class BZ2File {
            public const string __doc__ = 
@"BZ2File(name [, mode='r', buffering=0, compresslevel=9]) -> file object

Open a bz2 file. The mode can be 'r' or 'w', for reading (default) or
writing. When opened for writing, the file will be created if it doesn't
exist, and truncated otherwise. If the buffering argument is given, 0 means
unbuffered, and larger numbers specify the buffer size. If compresslevel
is given, must be a number between 1 and 9.
";

            private readonly string mode;
            private readonly int buffering;
            private readonly int compresslevel;

            private Stream underlyingStream, bz2Stream;

            [Documentation(@"close() -> None or (perhaps) an integer

Close the file. Sets data attribute .closed to true. A closed file
cannot be used for further I/O operations. close() may be called more
than once without error.
")]
            public BZ2File(string filename,
                [DefaultParameterValue("r")]string mode,
                [DefaultParameterValue(0)]int buffering,
                [DefaultParameterValue(DEFAULT_COMPRESSLEVEL)]int compresslevel) {

                this.mode = mode;
                this.buffering = buffering;
                this.compresslevel = compresslevel;

                if(mode.Contains("r")) {
                    this.underlyingStream = File.OpenRead(filename);
                    this.bz2Stream = new BZip2InputStream(this.underlyingStream);
                } else if(mode.Contains("w")) {
                    this.underlyingStream = File.Open(filename, FileMode.Create, FileAccess.Write);

                    if(mode.Contains("p")) {
                        this.bz2Stream = new ParallelBZip2OutputStream(this.underlyingStream);
                    } else {
                        this.bz2Stream = new BZip2OutputStream(this.underlyingStream);
                    }
                } else {
                    throw new ArgumentException("Mode must be 'r' or 'w'.", "mode");
                }
            }

            [Documentation(@"close() -> None or (perhaps) an integer

Close the file. Sets data attribute .closed to true. A closed file
cannot be used for further I/O operations. close() may be called more
than once without error.
")]
            public void close() {
                this.bz2Stream.Close();
            }

            [Documentation(@"read([size]) -> string

Read at most size uncompressed bytes, returned as a string. If the size
argument is negative or omitted, read until EOF is reached.
")]
            public string read([DefaultParameterValue(0)]int size) {
                byte[] bytes;
                int count;

                if (size < 1) {
                    bytes = ReadAll(this.bz2Stream);
                    count = bytes.Length;
                } else {
                    bytes = new byte[size];
                    count = this.bz2Stream.Read(bytes, 0, bytes.Length);
                }

                return PythonAsciiEncoding.Instance.GetString(bytes, 0, count);
            }

            private static byte[] ReadAll(Stream s) {
                byte[] output = null;
                byte[] buffer = new byte[64 * 1024];
                int pos = 0;

                while(true) {
                    int n = s.Read(buffer, 0, buffer.Length);
                    if (n <= 0)
                        break;

                    Array.Resize(ref output, pos + n);
                    Array.Copy(buffer, 0, output, pos, n);
                    pos = output.Length;
                }

                return output;
            }

            [Documentation(@"readline([size]) -> string

Return the next line from the file, as a string, retaining newline.
A non-negative size argument will limit the maximum number of bytes to
return (an incomplete line may be returned then). Return an empty
string at EOF.
")]
            public string readline([DefaultParameterValue(0)]int size) {
                return null;
            }

            [Documentation(@"readlines([size]) -> list

Call readline() repeatedly and return a list of lines read.
The optional size argument, if given, is an approximate bound on the
total number of bytes in the lines returned.
")]
            public List readlines([DefaultParameterValue(0)]int size) {
                return null;
            }

            [Documentation(@"seek(offset [, whence]) -> None

Move to new file position. Argument offset is a byte count. Optional
argument whence defaults to 0 (offset from start of file, offset
should be >= 0); other values are 1 (move relative to current position,
positive or negative), and 2 (move relative to end of file, usually
negative, although many platforms allow seeking beyond the end of a file).

Note that seeking of bz2 files is emulated, and depending on the parameters
the operation may be extremely slow.
")]
            public void seek(object offset, [DefaultParameterValue(0)]int whence) {
                
            }

            [Documentation(@"tell() -> int

Return the current file position, an integer (may be a long integer).
")]
            public object tell() {
                return null;
            }

            [Documentation(@"write(data) -> None

Write the 'data' string to file. Note that due to buffering, close() may
be needed before the file on disk reflects the data written.
")]
            public void write([BytesConversion]IList<byte> data) {
                byte[] bytes = data.ToArray();
                this.bz2Stream.Write(bytes, 0, bytes.Length);
            }

            [Documentation(@"writelines(sequence_of_strings) -> None

Write the sequence of strings to the file. Note that newlines are not
added. The sequence can be any iterable object producing strings. This is
equivalent to calling write() for each string.
")]
            public void writelines(IEnumerable<string> sequence_of_strings) {
            }
        }
    }
}

