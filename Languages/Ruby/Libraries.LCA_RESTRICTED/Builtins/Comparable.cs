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
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    [RubyModule("Comparable")]
    public static class Comparable {
        [RubyMethod("<")]
        public static bool Less(
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            object self, object other) {

            return Compare(compareStorage, lessThanStorage, greaterThanStorage, self, other).GetValueOrDefault(0) < 0;
        }

        [RubyMethod("<=")]
        public static bool LessOrEqual(
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,            
            object self, object other) {

            return Compare(compareStorage, lessThanStorage, greaterThanStorage, self, other).GetValueOrDefault(1) <= 0;
        }

        [RubyMethod(">=")]
        public static bool GreaterOrEqual(
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            object self, object other) {

            return Compare(compareStorage, lessThanStorage, greaterThanStorage, self, other).GetValueOrDefault(-1) >= 0;
        }

        [RubyMethod(">")]
        public static bool Greater(
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            object self, object other) {

            return Compare(compareStorage, lessThanStorage, greaterThanStorage, self, other).GetValueOrDefault(0) > 0;
        }

        /// <summary>
        /// Try to compare the lhs and rhs. Throws and exception if comparison returns null. Returns null on failure, -1/0/+1 otherwise.
        /// </summary>
        private static int? Compare(BinaryOpStorage/*!*/ compareStorage, BinaryOpStorage/*!*/ lessThanStorage, BinaryOpStorage/*!*/ greaterThanStorage,
            object lhs, object rhs) {

            // calls method_missing, doesn't catch any exception:
            var compare = compareStorage.GetCallSite("<=>");
            object compareResult = compare.Target(compare, lhs, rhs);
            
            if (compareResult != null) {
                return Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, compareResult);
            } else {
                throw RubyExceptions.MakeComparisonError(lessThanStorage.Context, lhs, rhs);
            }
        }

        [RubyMethod("between?")]
        public static bool Between(
            BinaryOpStorage/*!*/ compareStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            object self, object min, object max) {

            return !Less(compareStorage, lessThanStorage, greaterThanStorage, self, min)
                && !Greater(compareStorage, lessThanStorage, greaterThanStorage, self, max);
        }

        [RubyMethod("==")]
        public static object Equal(BinaryOpStorage/*!*/ compareStorage, object self, object other) {

            if (self == other) {
                return ScriptingRuntimeHelpers.True;
            }

            // calls method_missing:
            var compare = compareStorage.GetCallSite("<=>");

            object compareResult;
            try {
                compareResult = compare.Target(compare, self, other);
            } catch (SystemException) {
                // catches StandardError (like rescue)
                return null;
            }

            if (compareResult == null || !(compareResult is int)) {
                return null;
            }

            return ScriptingRuntimeHelpers.BooleanToObject((int)compareResult == 0);
        }
    }
}
