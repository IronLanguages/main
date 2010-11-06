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

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [RubyClass(Extends = typeof(TypeTracker), Restrictions = ModuleRestrictions.None)]
    public static class TypeTrackerOps {
        [RubyMethod("to_module")]
        public static RubyModule/*!*/ ToModule(RubyContext/*!*/ context, TypeTracker/*!*/ self) {
            return context.GetModule(self.Type);
        }

        [RubyMethod("to_class")]
        public static RubyClass/*!*/ ToClass(RubyContext/*!*/ context, TypeTracker/*!*/ self) {
            if (self.Type.IsInterface) {
                RubyExceptions.CreateTypeError("Cannot convert a CLR interface to a Ruby class");
            }
            return context.GetClass(self.Type);
        }
    }
}
