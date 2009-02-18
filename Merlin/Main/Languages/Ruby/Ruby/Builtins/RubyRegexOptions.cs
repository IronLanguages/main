/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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

        // encoding:
        FIXED = 16,
        EUC = 32,
        SJIS = 48,
        UTF8 = 64,

        EncodingMask = FIXED | EUC | SJIS | UTF8
    }
}
