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

namespace IronPython.Modules.Bz2 {
    public static partial class BZip2Module {
        [PythonType]
        public class BZip2File {
            public const string __doc__ = 
@"BZ2File(name [, mode='r', buffering=0, compresslevel=9]) -> file object

Open a bz2 file. The mode can be 'r' or 'w', for reading (default) or
writing. When opened for writing, the file will be created if it doesn't
exist, and truncated otherwise. If the buffering argument is given, 0 means
unbuffered, and larger numbers specify the buffer size. If compresslevel
is given, must be a number between 1 and 9.
";

            public BZip2File(string filename,
                [DefaultParameterValue("r")]string mode,
                [DefaultParameterValue(0)]int buffering,
                [DefaultParameterValue(DEFAULT_COMPRESSLEVEL)]int compresslevel) {
            }

            public void close() {
            }

            public string read([DefaultParameterValue(0)]int size) {
                return null;
            }

            public string readline([DefaultParameterValue(0)]int size) {
                return null;
            }

            public List readlines([DefaultParameterValue(0)]int size) {
                return null;
            }

            public void seek(object offset, [DefaultParameterValue(0)]int whence) {
                

            }

            public object tell() {
                return null;
            }

            public void write([BytesConversion]IList<byte> data) {
            }

            public void writelines(IEnumerable<string> seqeunce_of_strings) {
            }
        }
    }
}

