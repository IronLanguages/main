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
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    [RubyModule("Comparable")]
    public static class Comparable {
        [RubyMethod("<")]
        public static bool Less(ComparisonStorage/*!*/ comparisonStorage, object self, object other) {
            return Compare(comparisonStorage, self, other).GetValueOrDefault(0) < 0;
        }

        [RubyMethod("<=")]
        public static bool LessOrEqual(ComparisonStorage/*!*/ comparisonStorage, object self, object other) {
            return Compare(comparisonStorage, self, other).GetValueOrDefault(1) <= 0;
        }

        [RubyMethod(">=")]
        public static bool GreaterOrEqual(ComparisonStorage/*!*/ comparisonStorage, object self, object other) {
            return Compare(comparisonStorage, self, other).GetValueOrDefault(-1) >= 0;
        }

        [RubyMethod(">")]
        public static bool Greater(ComparisonStorage/*!*/ comparisonStorage, object self, object other) {
            return Compare(comparisonStorage, self, other).GetValueOrDefault(0) > 0;
        }

        /// <summary>
        /// Try to compare the lhs and rhs. Throws and exception if comparison returns null. Returns null on failure, -1/0/+1 otherwise.
        /// </summary>
        private static int? Compare(ComparisonStorage/*!*/ comparisonStorage, object lhs, object rhs) {

            // calls method_missing, doesn't catch any exception:
            var compare = comparisonStorage.CompareSite;
            object compareResult = compare.Target(compare, lhs, rhs);
            if (compareResult != null) {
                return Protocols.ConvertCompareResult(comparisonStorage, compareResult);
            } 

            throw RubyExceptions.MakeComparisonError(comparisonStorage.Context, lhs, rhs);
        }

        [RubyMethod("between?")]
        public static bool Between(ComparisonStorage/*!*/ comparisonStorage, object self, object min, object max) {
            return !Less(comparisonStorage, self, min) && !Greater(comparisonStorage, self, max);
        }

        [RubyMethod("==")]
        public static bool Equal(BinaryOpStorage/*!*/ compareStorage, object self, object other) {
            if (self == other) {
                return true;
            }

            // calls method_missing:
            var compare = compareStorage.GetCallSite("<=>");

            object compareResult;
            try {
                compareResult = compare.Target(compare, self, other);
            } catch (SystemException) {
                // catches StandardError (like rescue)
                return false;
            }

            return compareResult is int && (int)compareResult == 0;
        }
    }
}
