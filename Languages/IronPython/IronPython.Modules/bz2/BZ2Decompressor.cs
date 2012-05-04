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
using IronPython.Runtime;

namespace IronPython.Modules.Bz2 {
    public static partial class Bz2Module {
        [PythonType]
        public class BZip2Decompressor {
            public const string __doc__ = 
@"BZ2Decompressor() -> decompressor object

Create a new decompressor object. This object may be used to decompress
data sequentially. If you want to decompress data in one shot, use the
decompress() function instead.
";

            public BZip2Decompressor() {
            }

            public string decompress([BytesConversion]IList<byte> data) {
                return null;
            }
        }
    }
}

