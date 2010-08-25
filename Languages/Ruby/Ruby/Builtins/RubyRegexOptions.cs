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

using System;

namespace IronRuby.Builtins {
    [Flags]
    public enum RubyRegexOptions {
        NONE = 0,
        IgnoreCase = 1,
        Extended = 2,
        Multiline = 4,
        Once = 8,

        // encoding
        FIXED = 16,
        EUC = 32,
        SJIS = 64,
        UTF8 = 128,

        EncodingMask = EUC | SJIS | UTF8 | FIXED
    }
}
