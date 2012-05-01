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

[assembly: PythonModule("bz2", typeof(IronPython.Modules.Bz2.Bz2Module))]

namespace IronPython.Modules.Bz2 {
    public static partial class Bz2Module {
        public const string __doc__ = 
@"The python bz2 module provides a comprehensive interface for
the bz2 compression library. It implements a complete file
interface, one shot (de)compression functions, and types for
sequential (de)compression.";

        private const int DEFAULT_COMPRESSLEVEL = 9;

        public static string compress([BytesConversion]IList<byte> data, 
                                      [DefaultParameterValue(DEFAULT_COMPRESSLEVEL)]int compresslevel) {
            return null;
        }

        public static string decompress([BytesConversion]IList<byte> data) {
            return null;
        }
    }
}
