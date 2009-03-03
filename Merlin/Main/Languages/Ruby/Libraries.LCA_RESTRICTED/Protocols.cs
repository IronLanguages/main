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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;
using IronRuby.Compiler;

namespace IronRuby.Runtime {
    /// <summary>
    /// Class for implementing standard Ruby conversion logic
    /// 
    /// Ruby conversion rules aren't always consistent, but we should try to capture all
    /// common conversion patterns here. They're more likely to be correct than something
    /// created by hand.
    /// </summary>
    public static class Protocols {

        #region Bignum/Fixnum Normalization

        /// <summary>
        /// Converts a BigInteger to int if it is small enough
        /// </summary>
        /// <param name="x">The value to convert</param>
        /// <returns>An int if x is small enough, otherwise x.</returns>
        /// <remarks>
        /// Use this helper to downgrade BigIntegers as necessary.
        /// </remarks>
        public static object/*!*/ Normalize(BigInteger/*!*/ x) {
            int result;
            if (x.AsInt32(out result)) {
                return ScriptingRuntimeHelpers.Int32ToObject(result);
            }
            return x;
        }

        public static object Normalize(long x) {
            if (x >= Int32.MinValue && x <= Int32.MaxValue) {
                return ScriptingRuntimeHelpers.Int32ToObject((int)x);
            } else {
                return BigInteger.Create(x);
            }
        }

        public static object Normalize(object x) {
            int result;
            if (x is BigInteger) {
                if (((BigInteger)x).AsInt32(out result)) {
                    return ScriptingRuntimeHelpers.Int32ToObject(result);
                }
            }
            return x;
        }

        #endregion

        #region CastToString, TryCastToString, ConvertToString

        /// <summary>
        /// Converts an object to string using to_str protocol (<see cref="ConvertToStrAction"/>).
        /// </summary>
        public static MutableString/*!*/ CastToString(ConversionStorage<MutableString>/*!*/ stringCast, RubyContext/*!*/ context, object obj) {
            var site = stringCast.GetSite(ConvertToStrAction.Instance);
            return site.Target(site, context, obj);
        }

        /// <summary>
        /// Converts an object to string using try-to_str protocol (<see cref="TryConvertToStrAction"/>).
        /// </summary>
        public static MutableString TryCastToString(ConversionStorage<MutableString>/*!*/ stringTryCast, RubyContext/*!*/ context, object obj) {
            var site = stringTryCast.GetSite(TryConvertToStrAction.Instance);
            return site.Target(site, context, obj);
        }

        /// <summary>
        /// Convert to string using to_s protocol (<see cref="ConvertToSAction"/>).
        /// </summary>
        public static MutableString/*!*/ ConvertToString(ConversionStorage<MutableString>/*!*/ tosStorage, RubyContext/*!*/ context, object obj) {
            var site = tosStorage.GetSite(ConvertToSAction.Instance);
            return site.Target(site, context, obj);
        }

        #endregion

        #region CastToArray, TryCastToArray, TryConvertToArray

        public static IList/*!*/ CastToArray(ConversionStorage<IList>/*!*/ arrayCast, RubyContext/*!*/ context, object obj) {
            var site = arrayCast.GetSite(ConvertToArrayAction.Instance);
            return site.Target(site, context, obj);
        }

        public static IList TryCastToArray(ConversionStorage<IList>/*!*/ arrayTryCast, RubyContext/*!*/ context, object obj) {
            var site = arrayTryCast.GetSite(TryConvertToArrayAction.Instance);
            return site.Target(site, context, obj);
        }

        public static IList TryConvertToArray(ConversionStorage<IList>/*!*/ tryToA, RubyContext/*!*/ context, object obj) {
            var site = tryToA.GetSite(TryConvertToAAction.Instance);
            return site.Target(site, context, obj);
        }

        #endregion

        #region ConvertStringToFloat, ConvertToInteger, CastToInteger, CastToFixnum, CastToUInt32Unchecked, CastToUInt64Unchecked

        public static double ConvertStringToFloat(RubyContext/*!*/ context, MutableString/*!*/ value) {
            double result;
            bool complete;
            if (Tokenizer.TryParseDouble(value.ConvertToString(), out result, out complete) && complete) {
                return result;
            }

            throw RubyExceptions.InvalidValueForType(context, value, "Float");
        }

        public static IntegerValue ConvertToInteger(ConversionStorage<IntegerValue>/*!*/ integerConversion, RubyContext/*!*/ context, object value) {
            var site = integerConversion.GetSite(CompositeConversionAction.ToIntToI);
            return site.Target(site, context, value); 
        }

        public static IntegerValue CastToInteger(ConversionStorage<IntegerValue>/*!*/ integerConversion, RubyContext/*!*/ context, object value) {
            var site = integerConversion.GetSite(ConvertToIntAction.Instance);
            return site.Target(site, context, value);
        }

        public static int CastToFixnum(ConversionStorage<int>/*!*/ conversionStorage, RubyContext/*!*/ context, object value) {
            var site = conversionStorage.GetSite(ConvertToFixnumAction.Instance);
            return site.Target(site, context, value);
        }

        /// <summary>
        /// Like CastToInteger, but converts the result to an unsigned int.
        /// </summary>
        public static uint CastToUInt32Unchecked(ConversionStorage<IntegerValue>/*!*/ integerConversion, RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
            }

            IntegerValue integer = CastToInteger(integerConversion, context, obj);
            if (integer.IsFixnum) {
                return unchecked((uint)integer.Fixnum);
            } 

            uint u;
            if (integer.Bignum.AsUInt32(out u)) {
                return u;
            }
            throw RubyExceptions.CreateRangeError("bignum too big to convert into `unsigned long'");
        }

        /// <summary>
        /// Like CastToInteger, but converts the result to an unsigned int.
        /// </summary>
        public static ulong CastToUInt64Unchecked(ConversionStorage<IntegerValue>/*!*/ integerConversion, RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
            }

            IntegerValue integer = CastToInteger(integerConversion, context, obj);
            if (integer.IsFixnum) {
                return unchecked((ulong)integer.Fixnum);
            }

            ulong u;
            if (integer.Bignum.AsUInt64(out u)) {
                return u;
            }
            throw RubyExceptions.CreateRangeError("bignum too big to convert into `quad long'");
        }

        #endregion

        #region Compare (<=>), ConvertCompareResult

        /// <summary>
        /// Try to compare the lhs and rhs. Throws and exception if comparison returns null. Returns -1/0/+1 otherwise.
        /// </summary>
        public static int Compare(
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, object lhs, object rhs) {

            var compare = comparisonStorage.GetCallSite("<=>");

            var result = compare.Target(compare, context, lhs, rhs);
            if (result != null) {
                return Protocols.ConvertCompareResult(lessThanStorage, greaterThanStorage, context, result);
            } else {
                throw RubyExceptions.MakeComparisonError(context, lhs, rhs);
            }
        }
    
        public static int ConvertCompareResult(
            BinaryOpStorage/*!*/ lessThanStorage, 
            BinaryOpStorage/*!*/ greaterThanStorage,
            RubyContext/*!*/ context, object/*!*/ result) {

            Debug.Assert(result != null);

            var greaterThanSite = greaterThanStorage.GetCallSite(">");
            if (RubyOps.IsTrue(greaterThanSite.Target(greaterThanSite, context, result, 0))) {
                return 1;
            }

            var lessThanSite = lessThanStorage.GetCallSite("<");
            if (RubyOps.IsTrue(lessThanSite.Target(lessThanSite, context, result, 0))) {
                return -1;
            }

            return 0;
        }

        #endregion

        #region IsTrue, IsEqual, RespondTo, Write

        /// <summary>
        /// Protocol for determining truth in Ruby (not null and not false)
        /// </summary>
        public static bool IsTrue(object obj) {
            return (obj is bool) ? (bool)obj == true : obj != null;
        }

        /// <summary>
        /// Protocol for determining value equality in Ruby (uses IsTrue protocol on result of == call)
        /// </summary>
        public static bool IsEqual(BinaryOpStorage/*!*/ equals, RubyContext/*!*/ context, object lhs, object rhs) {
            var site = equals.GetCallSite("==");
            return IsTrue(site.Target(site, context, lhs, rhs));
        }

        public static bool RespondTo(RespondToStorage/*!*/ respondToStorage, RubyContext/*!*/ context, object target, string/*!*/ methodName) {
            var site = respondToStorage.GetCallSite();
            return IsTrue(site.Target(site, context, target, SymbolTable.StringToId(methodName)));
        }

        public static void Write(BinaryOpStorage/*!*/ writeStorage, RubyContext/*!*/ context, object target, object value) {
            var site = writeStorage.GetCallSite("write");
            site.Target(site, context, target, value);
        }

        #endregion

        #region Coercion

        /// <summary>
        /// Try to coerce the values of self and other (using other as the target object) then dynamically invoke "&lt;=&gt;".
        /// </summary>
        /// <returns>
        /// Result of &lt;=&gt; on coerced values or <c>null</c> if "coerce" method is not defined, throws a subclass of SystemException, 
        /// or returns something other than a pair of objects.
        /// </returns>
        public static object CoerceAndCompare(
            BinaryOpStorage/*!*/ coercionStorage,
            BinaryOpStorage/*!*/ comparisonStorage, 
            RubyContext/*!*/ context, object self, object other) {

            object result;
            return TryCoerceAndApply(coercionStorage, comparisonStorage, "<=>", context, self, other, out result) ? result : null;
        }

        /// <summary>
        /// Applies given operator on coerced values and converts its result to Ruby truth (using Protocols.IsTrue).
        /// </summary>
        /// <exception cref="ArgumentError">
        /// "coerce" method is not defined, throws a subclass of SystemException, or returns something other than a pair of objects.
        /// </exception>
        public static bool CoerceAndRelate(
            BinaryOpStorage/*!*/ coercionStorage,
            BinaryOpStorage/*!*/ comparisonStorage, string/*!*/ relationalOp, 
            RubyContext/*!*/ context, object self, object other) {

            object result;
            if (TryCoerceAndApply(coercionStorage, comparisonStorage, relationalOp, context, self, other, out result)) {
                return RubyOps.IsTrue(result);
            }
            
            throw RubyExceptions.MakeComparisonError(context, self, other);
        }

        /// <summary>
        /// Applies given operator on coerced values and returns the result.
        /// </summary>
        /// <exception cref="TypeError">
        /// "coerce" method is not defined, throws a subclass of SystemException, or returns something other than a pair of objects.
        /// </exception>
        public static object CoerceAndApply(
            BinaryOpStorage/*!*/ coercionStorage,
            BinaryOpStorage/*!*/ binaryOpStorage, string/*!*/ binaryOp,
            RubyContext/*!*/ context, object self, object other) {

            object result;
            if (TryCoerceAndApply(coercionStorage, binaryOpStorage, binaryOp, context, self, other, out result)) {
                return result;
            }

            throw RubyExceptions.MakeCoercionError(context, other, self);
        }

        /// <summary>
        /// Applies given operator on coerced values and returns the result.
        /// </summary>
        /// <exception cref="TypeError">
        /// "coerce" method is not defined, throws a subclass of SystemException, or returns something other than a pair of objects.
        /// </exception>
        public static object TryCoerceAndApply(
            BinaryOpStorage/*!*/ coercionStorage,
            BinaryOpStorage/*!*/ binaryOpStorage, string/*!*/ binaryOp,
            RubyContext/*!*/ context, object self, object other) {

            if (other == null) {
                return null;
            }

            object result;
            if (TryCoerceAndApply(coercionStorage, binaryOpStorage, binaryOp, context, self, other, out result)) {
                if (result != null) {
                    return RubyOps.IsTrue(result);
                }
            }
            return null;
        }

        private static bool TryCoerceAndApply(
            BinaryOpStorage/*!*/ coercionStorage,
            BinaryOpStorage/*!*/ binaryOpStorage, string/*!*/ binaryOp,
            RubyContext/*!*/ context, object self, object other, out object result) {

            // doesn't call method_missing:
            var coerce = coercionStorage.GetCallSite("coerce", new RubyCallSignature(1, RubyCallFlags.IsTryCall | RubyCallFlags.HasImplicitSelf));

            IList coercedValues;

            try {
                // Swap self and other around to do the coercion.
                coercedValues = coerce.Target(coerce, context, other, self) as IList;
            } catch (SystemException) { 
                // catches StandardError (like rescue)
                result = null;
                return false;
            }

            if (coercedValues != null && coercedValues.Count == 2) {
                var compare = binaryOpStorage.GetCallSite(binaryOp);
                result = compare.Target(compare, context, coercedValues[0], coercedValues[1]);
                return true;
            }

            result = null;
            return false;
        }
    
        #endregion

        #region CLR Types

        public static Type[]/*!*/ ToTypes(RubyContext/*!*/ context, object[]/*!*/ values) {
            Type[] args = new Type[values.Length];
            for (int i = 0; i < args.Length; i++) {
                args[i] = ToType(context, values[i]);
            }

            return args;
        }

        public static Type/*!*/ ToType(RubyContext/*!*/ context, object value) {
            TypeTracker tt = value as TypeTracker;
            if (tt != null) {
                return tt.Type;
            }

            RubyClass rc = value as RubyClass;
            if (rc != null) {
                return rc.GetUnderlyingSystemType();
            }

            throw RubyExceptions.InvalidValueForType(context, value, "Class");
        }

        #endregion

        #region Security

        public static void CheckSafeLevel(RubyContext/*!*/ context, int level) {
            if (level <= context.CurrentSafeLevel) {
                throw RubyExceptions.CreateSecurityError("Insecure operation at level " + context.CurrentSafeLevel);
            }
        }
        public static void CheckSafeLevel(RubyContext/*!*/ context, int level, string/*!*/ method) {
            if (level <= context.CurrentSafeLevel) {
                throw RubyExceptions.CreateSecurityError(String.Format("Insecure operation {0} at level {1}", method, context.CurrentSafeLevel));
            }
        }

        #endregion
    }
}
