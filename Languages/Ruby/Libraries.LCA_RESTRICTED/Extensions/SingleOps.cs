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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;

namespace IronRuby.Builtins {
    [RubyClass(Extends = typeof(float), Inherits = typeof(Numeric), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(ClrFloat), Copy = true), Includes(typeof(Precision))]
    public static class SingleOps {
        // TODO: add constants as defined in FloatOps

        [RubyConstructor]
        public static float Create(RubyClass/*!*/ self, [DefaultProtocol]double value) {
            return (float)value;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(object/*!*/ self) {
            return MutableString.CreateMutable(RubyEncoding.Binary).Append(self.ToString()).Append(" (Single)");
        }
    }
}
