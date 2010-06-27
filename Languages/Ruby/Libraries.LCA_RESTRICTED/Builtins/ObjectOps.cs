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

using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyClass("Object", Extends = typeof(object), Restrictions = ModuleRestrictions.NoNameMapping | ModuleRestrictions.NotPublished)]
    [Includes(typeof(Kernel))]
    public static class ObjectOps {
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance | RubyMethodAttributes.Empty)]
        public static void Reinitialize(object self) {
            // nop
        }

        [RubyConstant]
        public readonly static bool TRUE = true;

        [RubyConstant]
        public readonly static bool FALSE = false;

        [RubyConstant]
        public readonly static object NIL = null;
    }
}
