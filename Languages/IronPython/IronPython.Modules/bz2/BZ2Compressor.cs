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
        public class BZip2Compressor {
            public const string __doc__ = 
@"BZ2Compressor([compresslevel=9]) -> compressor object

Create a new compressor object. This object may be used to compress
data sequentially. If you want to compress data in one shot, use the
compress() function instead. The compresslevel parameter, if given,
must be a number between 1 and 9.
";

            private int compresslevel;

            public BZip2Compressor([DefaultParameterValue(DEFAULT_COMPRESSLEVEL)]int compresslevel) {
                this.compresslevel = compresslevel;
            }

            public string compress([BytesConversion]IList<byte> data) {
                return null;
            }

            public string flush() {
                return null;
            }
        }
    }
}

