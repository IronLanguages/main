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

using System.Collections;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    // For every .NET type that implements IEnumerable, extend it to include the Ruby Enumerable
    // module, and inject an "each" method.
    [RubyModule(Extends = typeof(IEnumerable), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(Enumerable))]
    public static class IEnumerableOps {

        [RubyMethod("each", RubyMethodAttributes.PublicInstance)]
        public static object Each(BlockParam block, IEnumerable/*!*/ self) {
            foreach (object obj in self) {
                object result;
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                if (block.Yield(obj, out result)) {
                    return result;
                }
            }
            return self;
        }
    }
}
