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
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;
using System.Diagnostics;

namespace IronRuby.Builtins {
    /// <summary>
    /// Implements operations on flag enumeration.
    /// </summary>
    [RubyModule("FlagEnumeration", DefineIn = typeof(IronRubyOps.Clr), Extends = typeof(FlagEnumeration), Restrictions = ModuleRestrictions.NoUnderlyingType)]
    public static class FlagEnumerationOps {
        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(RubyContext/*!*/ context, object/*!*/ self, [NotNull]object/*!*/ other) {
            Debug.Assert(self is Enum);

            var result = EnumUtils.BitwiseOr(self, other);
            if (result != null) {
                return result;
            }

            throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
        }

        [RubyMethod("&")]
        public static object/*!*/ BitwiseAnd(RubyContext/*!*/ context, object/*!*/ self, [NotNull]object/*!*/ other) {
            Debug.Assert(self is Enum);

            var result = EnumUtils.BitwiseAnd(self, other);
            if (result != null) {
                return result;
            }
            throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
        }

        [RubyMethod("^")]
        public static object/*!*/ Xor(RubyContext/*!*/ context, object/*!*/ self, [NotNull]object/*!*/ other) {
            Debug.Assert(self is Enum);

            var result = EnumUtils.ExclusiveOr(self, other);
            if (result != null) {
                return result;
            }
            throw RubyExceptions.CreateUnexpectedTypeError(context, other, context.GetClassDisplayName(self));
        }

        [RubyMethod("~")]
        public static object/*!*/ OnesComplement(RubyContext/*!*/ context, object/*!*/ self) {
            Debug.Assert(self is Enum);

            return EnumUtils.OnesComplement(self);
        }
    }
}
