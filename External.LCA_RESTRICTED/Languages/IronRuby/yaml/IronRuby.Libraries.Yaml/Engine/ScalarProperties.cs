/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronRuby.StandardLibrary.Yaml {
    internal enum ScalarProperties {
        None = 0,
        Empty = 1,
        Multiline = 2,
        AllowFlowPlain = 4,
        AllowBlockPlain = 8,
        AllowSingleQuoted = 16,
        AllowDoubleQuoted = 32,
        AllowBlock = 64,
        SpecialCharacters = 128,
    }
}
