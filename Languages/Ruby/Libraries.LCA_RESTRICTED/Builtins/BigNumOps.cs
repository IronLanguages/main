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
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// Bignum objects hold integers outside the range of Fixnum. Bignum objects are created automatically when integer calculations would otherwise overflow a Fixnum. When a calculation involving Bignum objects returns a result that will fit in a Fixnum, the result is automatically converted. 
    /// For the purposes of the bitwise operations and [], a Bignum is treated as if it were an infinite-length bitstring with 2s complement representation. 
    /// While Fixnum values are immediate, Bignum objects are notassignment and parameter passing work with references to objects, not the objects themselves. 
    /// </summary>
    [RubyClass("Bignum", Extends = typeof(BigInteger), Inherits = typeof(Integer)), Includes(typeof(ClrBigInteger), Copy = true)]
    [HideMethod(">")]
    [HideMethod(">=")]
    [HideMethod("<")]
    [HideMethod("<=")]
    [UndefineMethod("new", IsStatic = true)]
    public static partial class BignumOps {
        /// <summary>
        /// Returns the number of bytes in the machine representation of self. 
        /// </summary>
        /// <example>
        ///    (256**10 - 1).size   #=> 12
        ///    (256**20 - 1).size   #=> 20
        ///    (256**40 - 1).size   #=> 40
        /// </example>
        [RubyMethod("size")]
        public static int Size(BigInteger/*!*/ self) {
            return self.GetWordCount() * 4;
        }
    }
}
