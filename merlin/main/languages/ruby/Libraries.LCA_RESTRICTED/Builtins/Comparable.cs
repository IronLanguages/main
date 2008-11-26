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

using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyModule("Comparable")]
    public static class Comparable {
        [RubyMethod("<")]
        public static bool Less(RubyContext/*!*/ context, object self, object other) {
            return Protocols.Compare(context, self, other) < 0;
        }

        [RubyMethod("<=")]
        public static bool LessOrEqual(RubyContext/*!*/ context, object self, object other) {
            return Protocols.Compare(context, self, other) <= 0;
        }

        [RubyMethod(">=")]
        public static bool GreaterOrEqual(RubyContext/*!*/ context, object self, object other) {
            return Protocols.Compare(context, self, other) >= 0;
        }

        [RubyMethod(">")]
        public static bool Greater(RubyContext/*!*/ context, object self, object other) {
            return Protocols.Compare(context, self, other) > 0;
        }

        [RubyMethod("==")]
        public static object Equal(RubyContext/*!*/ context, object self, object other) {
            // Short circuit long winded comparison if the objects are actually the same.
            if (self == other) {
                return true;
            }

            // TODO: handle exceptions thrown from Compare()
            // Compare may return null
            object result = RubySites.Compare(context, self, other);
            if (result is int) {
                return (int)result == 0;
            } else {
                return null;
            }
        }

        [RubyMethod("between?")]
        public static bool Between(RubyContext/*!*/ context, object self, object min, object max) {
            return (!Less(context, self, min) && !Greater(context, self, max));
        }
    }
}
