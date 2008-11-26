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
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;

namespace IronRuby.Runtime {
    /// <summary>
    /// Class for implementing standard Ruby conversion logic
    /// 
    /// Ruby conversion rules aren't always consistent, but we should try to capture all
    /// common conversion patterns here. They're more likely to be correct than something
    /// created by hand.
    /// </summary>
    public static class Protocols {

        #region dynamic sites

        private static readonly CallSite<Func<CallSite, RubyContext, object, object>>/*!*/
            _ToF = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_f")),
            _ToI = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_i")),
            _ToInt = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_int")),
            _ToS = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_s")),
            _ToStr = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_str")),
            _ToA = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_a")),
            _ToAry = CallSite<Func<CallSite, RubyContext, object, object>>.Create(RubySites.InstanceCallAction("to_ary"));

        #endregion

        #region Bignum/Fixnum Normalization

        /// <summary>
        /// Converts a BigInteger to int if it is small enough
        /// </summary>
        /// <param name="x">The value to convert</param>
        /// <returns>An int if x is small enough, otherwise x.</returns>
        /// <remarks>
        /// Use this helper to downgrade BigIntegers as necessary.
        /// </remarks>
        public static object Normalize(BigInteger x) {
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

        #region CastToString, AsString, TryConvertToString

        /// <summary>
        /// Standard way to convert to a Ruby String, using to_str
        /// 
        /// Checks if it's already a string, and if so returns it.
        /// Then calls to_str if it exists, otherwise throw a TypeError
        /// </summary>
        public static MutableString/*!*/ CastToString(RubyContext/*!*/ context, object obj) {
            MutableString str = AsString(context, obj);
            if (str != null) {
                return str;
            }
            throw RubyExceptions.CannotConvertTypeToTargetType(context, obj, "String");
        }

        public static MutableString[]/*!*/ CastToStrings(RubyContext/*!*/ context, object[]/*!*/ objs) {
            var result = new MutableString[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                result[i] = Protocols.CastToString(context, objs[i]);
            }
            return result;
        }

        /// <summary>
        /// Standard way to convert to a Ruby String, using to_str
        /// 
        /// Checks if it's already a string, and if so returns it.
        /// Then calls to_str if it exists, otherwise returns null
        /// </summary>
        public static MutableString AsString(RubyContext/*!*/ context, object obj) {
            MutableString str = obj as MutableString;
            if (str != null) {
                return str;
            }
            if (RubySites.RespondTo(context, obj, "to_str")) {
                str = _ToStr.Target(_ToStr, context, obj) as MutableString;
                if (str != null) {
                    return str;
                }

                throw RubyExceptions.MethodShouldReturnType(context, obj, "to_str", "String");
            }

            return null;
        }

        /// <summary>
        /// Convert to string using to_s
        /// 
        /// The behavior is different from the typical conversion protocol:
        ///   * it assumes that to to_s is defined, and just calls it
        ///   * if to_s returns a non-string value, we fall back to Kernel.ToString 
        /// </summary>
        public static MutableString/*!*/ ConvertToString(RubyContext/*!*/ context, object obj) {
            MutableString str = obj as MutableString;
            if (str != null) {
                return str;
            }

            str = _ToS.Target(_ToS, context, obj) as MutableString;
            if (str != null) {
                return str;
            }

            // fallback to Kernel#to_s if to_s returned a non-string
            return KernelOps.ToS(context, obj);
        }

        #endregion

        #region ConvertToFloat

        /// <summary>
        /// Convert to a Float, using to_f
        /// Throws if conversion fails
        /// </summary>
        public static double ConvertToFloat(RubyContext/*!*/ context, object value) {
            if (value == null) {
                throw RubyExceptions.CreateTypeError("can't convert nil into Float");
            }
            if (value is int || value is double) {
                return Converter.ConvertToDouble(value);
            }
            if (value is BigInteger) {
                return ((BigInteger)value).ToFloat64();
            }
            if (value is MutableString) {
                return ConvertStringToFloat(context, (MutableString)value);
            }

            if (RubySites.RespondTo(context, value, "to_f")) {
                object obj = _ToF.Target(_ToF, context, value);
                if (!(obj is double)) {
                    throw RubyExceptions.MethodShouldReturnType(context, value, "to_f", "Float");
                }
                return (double)obj;
            }

            throw RubyExceptions.CannotConvertTypeToTargetType(context, value, "Float");
        }
        public static double ConvertStringToFloat(RubyContext context, MutableString value) {
            try {
                return double.Parse(value.ConvertToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            } catch (FormatException x) {
                MutableString valueString = RubySites.Inspect(context, value);
                throw RubyExceptions.CreateArgumentError("invalid value for Float(): " + valueString.ConvertToString(), x);
            }
        }
        #endregion

        #region TryConvertToInteger, ConvertToInteger, AsInteger, CastToInteger, CastToFixnum, IsInteger, IntegerAsFixnum

        private static bool AsPrimitiveInteger(object obj, out int intValue, out BigInteger bigValue) {
            // TODO: All CLR primitive numeric types?
            
            if (obj is int) {
                intValue = (int)obj;
                bigValue = null;
                return true;
            }

            var big = obj as BigInteger;
            if ((object)big != null) {
                intValue = 0;
                bigValue = big;
                return true;
            }

            intValue = 0;
            bigValue = null;
            return false;
        }

        public static object/*!*/ ConvertToInteger(RubyContext/*!*/ context, object obj) {
            int fixnum;
            BigInteger bignum;
            ConvertToInteger(context, obj, out fixnum, out bignum);
            return (object)bignum ?? ScriptingRuntimeHelpers.Int32ToObject(fixnum);
        }

        /// <summary>
        /// Standard way to convert to a Ruby Integer, using to_int and to_i
        /// Trys to call to_int, followed by to_i (if implemented).
        /// If neither is callable, throws a type error.
        /// </summary>
        public static void ConvertToInteger(RubyContext/*!*/ context, object obj, out int fixnum, out BigInteger bignum) {
            // Don't call to_int, to_i on primitive types:
            if (AsPrimitiveInteger(obj, out fixnum, out bignum)) {
                return;
            }

            if (RubySites.RespondTo(context, obj, "to_int")) {
                object result = _ToInt.Target(_ToInt, context, obj);
                if (AsPrimitiveInteger(result, out fixnum, out bignum)) {
                    return;
                }

                throw RubyExceptions.MethodShouldReturnType(context, obj, "to_int", "Integer");
            }

            if (RubySites.RespondTo(context, obj, "to_i")) {
                object result = _ToI.Target(_ToI, context, obj);
                if (AsPrimitiveInteger(result, out fixnum, out bignum)) {
                    return;
                }

                throw RubyExceptions.MethodShouldReturnType(context, obj, "to_i", "Integer");
            }

            throw RubyExceptions.CannotConvertTypeToTargetType(context, obj, "Integer");
        }

        /// <summary>
        /// Try to cast the object to an Integer using to_int
        /// Returns null if the object doesn't implement to_int
        /// Can return either Bignum or Fixnum
        /// </summary>
        public static bool AsInteger(RubyContext/*!*/ context, object obj, out int fixnum, out BigInteger bignum) {
            // Don't call to_int on types derived from Integer
            if (AsPrimitiveInteger(obj, out fixnum, out bignum)) {
                return true;
            }

            if (RubySites.RespondTo(context, obj, "to_int")) {
                object result = _ToInt.Target(_ToInt, context, obj);
                if (AsPrimitiveInteger(result, out fixnum, out bignum)) {
                    return true;
                }

                throw RubyExceptions.InvalidValueForType(context, result, "Integer");
            }

            return false;
        }

        /// <summary>
        /// Converts an Integer to a Fixnum.
        /// Don't call any conversion methods--just handles Fixnum & Bignum
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if value is an Integer, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws a RangeError if value is a
        /// BigInteger but can't be converted to a Fixnum</exception>
        public static bool IntegerAsFixnum(object value, out int result) {
            if (value is int) {
                result = (int)value;
                return true;
            }

            var bignum = value as BigInteger;
            if ((object)bignum != null) {
                if (!bignum.AsInt32(out result)) {
                    throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
                }
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        /// Try to cast the object to an Integer using to_int
        /// Throws if the cast fails
        /// Can return either Bignum or Fixnum
        /// </summary>
        public static void CastToInteger(RubyContext/*!*/ context, object obj, out int fixnum, out BigInteger bignum) {
            if (AsInteger(context, obj, out fixnum, out bignum)) {
                return;
            }

            throw RubyExceptions.CannotConvertTypeToTargetType(context, obj, "Integer");
        }

        /// <summary>
        /// Like CastToInteger, but converts the result to a Fixnum
        /// </summary>
        public static int CastToFixnum(RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
            }

            int fixnum;
            BigInteger bignum;
            CastToInteger(context, obj, out fixnum, out bignum);
            if ((object)bignum != null && !bignum.AsInt32(out fixnum)) {
                throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
            }

            return fixnum;
        }

        /// <summary>
        /// Like CastToInteger, but converts the result to an unsigned int.
        /// </summary>
        public static uint CastToUInt32Unchecked(RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
            }

            int fixnum;
            BigInteger bignum;
            CastToInteger(context, obj, out fixnum, out bignum);
            if ((object)bignum != null) {
                uint u;
                if (bignum.AsUInt32(out u)) {
                    return u;
                }
                throw RubyExceptions.CreateRangeError("bignum too big to convert into `unsigned long'");
            }

            return unchecked((uint)fixnum);
        }

        /// <summary>
        /// Like CastToInteger, but converts the result to an unsigned int.
        /// </summary>
        public static ulong CastToUInt64Unchecked(RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                throw RubyExceptions.CreateTypeError("no implicit conversion from nil to integer");
            }

            int fixnum;
            BigInteger bignum;
            CastToInteger(context, obj, out fixnum, out bignum);
            if ((object)bignum != null) {
                ulong u;
                if (bignum.AsUInt64(out u)) {
                    return u;
                }
                throw RubyExceptions.CreateRangeError("bignum too big to convert into `quad long'");
            }

            return unchecked((ulong)fixnum);
        }

        #endregion

        #region TryConvertToArray, ConvertToArray, AsArray, CastToArray

        /// <summary>
        /// Try to convert obj to an Array using #to_a
        /// 1. If obj is an Array (or a subtype), returns it
        /// 2. Calls to_a if it exists, possibly throwing if to_a doesn't return an Array
        /// 3. else returns null
        /// </summary>
        public static IList TryConvertToArray(RubyContext/*!*/ context, object obj) {
            // Don't call to_a on types derived from Array
            IList ary = obj as IList;
            if (ary != null) {
                return ary;
            }

            if (RubySites.RespondTo(context, obj, "to_a")) {
                object result = _ToA.Target(_ToA, context, obj);
                ary = result as List<object>;
                if (ary != null) {
                    return ary;
                }

                throw RubyExceptions.MethodShouldReturnType(context, obj, "to_a", "Array");
            }

            return null;
        }

        /// <summary>
        /// Works like TryConvertToArray, but throws a type error if the conversion fails
        /// </summary>
        public static IList/*!*/ ConvertToArray(RubyContext/*!*/ context, object obj) {
            IList ary = TryConvertToArray(context, obj);
            if (ary != null) {
                return ary;
            }

            throw RubyExceptions.CannotConvertTypeToTargetType(context, obj, "Array");
        }

        /// <summary>
        /// Try to convert obj to an Array using #to_ary
        /// 1. If obj is an Array (or a subtype), returns it
        /// 2. Calls to_ary if it exists, possibly throwing if to_ary doesn't return an Array
        /// 3. else returns null
        /// </summary>
        public static IList AsArray(RubyContext/*!*/ context, object obj) {
            // Don't call to_a on types derived from Array
            IList ary = obj as IList;
            if (ary != null) {
                return ary;
            }

            if (RubySites.RespondTo(context, obj, "to_ary")) {
                object result = _ToAry.Target(_ToAry, context, obj);
                ary = result as IList;
                if (ary != null) {
                    return ary;
                }

                throw RubyExceptions.MethodShouldReturnType(context, obj, "to_ary", "Array");
            }

            return null;
        }

        /// <summary>
        /// Works like AsArray, but throws a type error if the conversion fails
        /// </summary>
        public static IList/*!*/ CastToArray(RubyContext/*!*/ context, object obj) {
            IList ary = AsArray(context, obj);
            if (ary != null) {
                return ary;
            }

            throw RubyExceptions.CannotConvertTypeToTargetType(context, obj, "Array");
        }
        #endregion

        #region CastToSymbol

        /// <summary>
        /// Casts to symbol. Note that this doesn't actually use to_sym -- it uses to_str.
        /// That's just how Ruby does it.
        /// 
        /// Another fun detail: you can pass Fixnums as Symbols. If you pass a Fixnum that
        /// doesn't map to a Symbol (i.e. Fixnum#to_sym returns nil), you get an ArgumentError
        /// instead of a TypeError. At least it produces a warning about using Fixnums as Symbols
        /// </summary>
        public static string/*!*/ CastToSymbol(RubyContext/*!*/ context, object obj) {
            if (obj is SymbolId) {
                return SymbolTable.IdToString((SymbolId)obj);
            }

            if (obj is int) {
                return RubyOps.ConvertFixnumToSymbol(context, (int)obj);
            } else {
                MutableString str = AsString(context, obj);
                if (str != null) {
                    return RubyOps.ConvertMutableStringToSymbol(str);
                }
            }

            throw RubyExceptions.CreateTypeError(String.Format("{0} is not a symbol", RubySites.Inspect(context, obj)));
        }

        public static string[]/*!*/ CastToSymbols(RubyContext/*!*/ context, object[]/*!*/ objects) {
            string[] result = new string[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                result[i] = Protocols.CastToSymbol(context, objects[i]);
            }
            return result;
        }

        #endregion

        #region Compare (<=>), ConvertCompareResult

        private static CallSite<Func<CallSite, RubyContext, object, object, object>> _CompareSharedSite =
            CallSite<Func<CallSite, RubyContext, object, object, object>>.Create(
                RubySites.InstanceCallAction("<=>", 1));

        private static CallSite<Func<CallSite, RubyContext, object, int, object>> _GreaterSharedSite =
            CallSite<Func<CallSite, RubyContext, object, int, object>>.Create(
                RubySites.InstanceCallAction(">", 1));

        private static CallSite<Func<CallSite, RubyContext, object, int, object>> _LessSharedSite =
            CallSite<Func<CallSite, RubyContext, object, int, object>>.Create(
                RubySites.InstanceCallAction("<", 1));


        // Standard Ruby way of calling "<=>"
        // Call it, and then see if the result is > 0 by calling ">"
        // If not, see if the result is < 0 by calling "<" 
        // Returns -1, 0, 1 with the usual interpretation
        public static int Compare(RubyContext/*!*/ context, object lhs, object rhs) {
            object result = _CompareSharedSite.Target(_CompareSharedSite, context, lhs, rhs);
            if (result == null) {
                throw RubyExceptions.MakeComparisonError(context, lhs, rhs);
            }
            return ConvertCompareResult(context, result);
        }

        /// <summary>
        /// Try to compare the lhs and rhs.  If they can't be compared return nil
        /// </summary>
        public static int? TryCompare(RubyContext/*!*/ context, object lhs, object rhs) {
            object result = _CompareSharedSite.Target(_CompareSharedSite, context, lhs, rhs);
            if (result == null) {
                return null;
            }
            return ConvertCompareResult(context, result);
        }

        public static int ConvertCompareResult(RubyContext/*!*/ context, object result) {
            // Should throw if a call to <=> returns null
            Debug.Assert(result != null);

            if (RubyOps.IsTrue(_GreaterSharedSite.Target(_GreaterSharedSite, context, result, 0))) {
                return 1;
            } else if (RubyOps.IsTrue(_LessSharedSite.Target(_LessSharedSite, context, result, 0))) {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Protocol for determining truth in Ruby (not null and not false)
        /// </summary>
        public static bool IsTrue(object obj) {
            return (obj is bool) ? (bool)obj == true : obj != null;
        }

        /// <summary>
        /// Protocol for determining value equality in Ruby (uses IsTrue protocol on result of == call)
        /// </summary>
        public static bool IsEqual(RubyContext/*!*/ context, object lhs, object rhs) {
            return IsTrue(RubySites.Equal(context, lhs, rhs));
        }

        public delegate object DynamicInvocation(RubyContext/*!*/ context, object self, object other);

        /// <summary>
        /// Coerce the values of self and other (using other as the object) then call the passed in method.
        /// </summary>
        public static object CoerceAndCall(RubyContext/*!*/ context, object self, object other, DynamicInvocation invoke) {
            RubyArray coercedValues;
            try {
                // Swap self and other around to do the coercion.
                coercedValues = RubySites.Coerce(context, other, self);
            } catch (MemberAccessException x) {
                throw RubyExceptions.MakeCoercionError(context, self, other, x);
            } catch (ArgumentException x) {
                throw RubyExceptions.MakeCoercionError(context, self, other, x);
            }
            // But then swap them back when invoking the operation
            return invoke(context, coercedValues[0], coercedValues[1]);
        }

        /// <summary>
        /// Try to coerce the values of self and other (using other as the object) then dynamically invoke "&lt;=&gt;".
        /// </summary>
        /// <returns>
        /// 1 if self is greater than other
        /// 0 if self is equal to other
        /// -1 if self is less than other
        /// null otherwise (like if self and other cannot be coerced)
        /// </returns>
        /// <remarks>
        /// This method is used when comparing Numerics.
        /// If we can't coerce then they are not the same, so return null.
        /// But if the &lt;=&gt; doesn't exist on the first item in the array returned from coerce then throw missing method error.
        /// </remarks>
        public static object CoerceAndCallCompare(RubyContext/*!*/ context, object self, object other) {
            RubyArray coercedValues;
            try {
                // Swap self and other around to do the coercion.
                coercedValues = RubySites.Coerce(context, other, self);
            } catch (Exception) {
                // The coercion failed so return null.
                return null;
            }
            // This call is outside the try block as we do want problems with this call being raised up.
            return LibrarySites.Compare(context, coercedValues[0], coercedValues[1]);
        }

        /// <summary>
        /// Compare the values of self and other (coerce using other as the object then call the passed in comparison operator).
        /// </summary>
        /// <remarks>
        /// This method is used for &lt;, &lt;=, &gt; and &gt;= operators.  If we can't coerce then throw a specific comparison exception.
        /// </remarks>
        /// <exception cref="ArgumentException">If self and other cannot be coerced to the same type.</exception>
        public static bool CoerceAndCallRelationOperator(RubyContext/*!*/ context, object self, object other, DynamicInvocation invoke) {
            try {
                // Swap self and other around to do the coercion.
                RubyArray coercedValues = RubySites.Coerce(context, other, self);
                return RubyOps.IsTrue(invoke(context, coercedValues[0], coercedValues[1]));
            } catch (MemberAccessException x) {
                throw RubyExceptions.MakeComparisonError(context, self, other, x);
            } catch (ArgumentException x) {
                throw RubyExceptions.MakeComparisonError(context, self, other, x);
            } catch (NullReferenceException x) {
                throw RubyExceptions.MakeComparisonError(context, self, other, x);
            }
        }

        #endregion

        #region Range

        public static void ConvertToIntegerRange(RubyContext/*!*/ context, Range/*!*/ range, out int begin, out int end, out bool excludeEnd) {
            begin = Protocols.CastToFixnum(context, RubySites.RangeBegin(context, range));
            end = Protocols.CastToFixnum(context, RubySites.RangeEnd(context, range));
            excludeEnd = RubySites.RangeExcludeEnd(context, range);
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
