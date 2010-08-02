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
    /// <summary>
    /// Mixed-in .NET floating point numeric primitive types (float, double).
    /// </summary>
    [RubyModule("Float", DefineIn = typeof(IronRubyOps.Clr))]
    public static class ClrFloat {

        #region induced_from

        /// <summary>
        /// Convert value to Float, where value is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static double InducedFrom(RubyModule/*!*/ self, double value) {
            return value;
        }

        /// <summary>
        /// Convert value to Float, where value is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static object InducedFrom(UnaryOpStorage/*!*/ tofStorage, RubyModule/*!*/ self, int value) {
            var site = tofStorage.GetCallSite("to_f");
            return site.Target(site, ScriptingRuntimeHelpers.Int32ToObject(value));
        }

        /// <summary>
        /// Convert value to Float, where value is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static object InducedFrom(UnaryOpStorage/*!*/ tofStorage, RubyModule/*!*/ self, [NotNull]BigInteger/*!*/ value) {
            var site = tofStorage.GetCallSite("to_f");
            return site.Target(site, value);
        }

        /// <summary>
        /// Convert value to Float, where value is not Float, Fixnum or Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("induced_from", RubyMethodAttributes.PublicSingleton)]
        public static double InducedFrom(RubyModule/*!*/ self, object value) {
            throw RubyExceptions.CreateTypeError("failed to convert {0} into Float", self.Context.GetClassDisplayName(value));
        }

        #endregion

        #region Arithmetic Operators

        #region *

        /// <summary>
        /// Returns a new float which is the product of <code>self</code> * and <code>other</code>, where <code>other</code> is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("*")]
        public static double Multiply(double self, int other) {
            return self * (double)other;
        }

        /// <summary>
        /// Returns a new float which is the product of <code>self</code> * and <code>other</code>, where <code>other</code> is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("*")]
        public static double Multiply(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return self * Protocols.ConvertToDouble(context, other);
        }

        /// <summary>
        /// Returns a new float which is the product of <code>self</code> * and <code>other</code>, where <code>other</code> is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("*")]
        public static double Multiply(double self, double other) {
            return self * other;
        }

        /// <summary>
        /// Returns a new float which is the product of <code>self</code> * and <code>other</code>, , where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("*")]
        public static object Multiply(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
        }

        #endregion

        #region +

        /// <summary>
        /// Returns a new float which is the sum of <code>self</code> * and <code>other</code>, where <code>other</code> is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("+")]
        public static double Add(double self, int other) {
            return self + (double)other;
        }

        /// <summary>
        /// Returns a new float which is the sum of <code>self</code> * and <code>other</code>, where <code>other</code> is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("+")]
        public static double Add(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return self + Protocols.ConvertToDouble(context, other);
        }

        /// <summary>
        /// Returns a new float which is the sum of <code>self</code> * and <code>other</code>, where <code>other</code> is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("+")]
        public static double Add(double self, double other) {
            return self + other;
        }

        /// <summary>
        /// Returns a new float which is the sum of <code>self</code> * and <code>other</code>, , where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("+")]
        public static object Add(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
        }

        #endregion

        #region -

        /// <summary>
        /// Returns a new float which is the difference between <code>self</code> * and <code>other</code>, where <code>other</code> is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("-")]
        public static double Subtract(double self, int other) {
            return self - (double)other;
        }

        /// <summary>
        /// Returns a new float which is the difference between <code>self</code> * and <code>other</code>, where <code>other</code> is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("-")]
        public static double Subtract(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return self - Protocols.ConvertToDouble(context, other);
        }

        /// <summary>
        /// Returns a new float which is the difference between <code>self</code> * and <code>other</code>, where <code>other</code> is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("-")]
        public static double Subtract(double self, double other) {
            return self - other;
        }

        /// <summary>
        /// Returns a new float which is the difference between <code>self</code> * and <code>other</code>, , where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("-")]
        public static object Subtract(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
        }

        #endregion

        #region /

        /// <summary>
        /// Returns a new float which is the result of dividing <code>self</code> * by <code>other</code>, where <code>other</code> is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("/")]
        public static double Divide(double self, int other) {
            return self / (double)other;
        }

        /// <summary>
        /// Returns a new float which is the result of dividing <code>self</code> * by <code>other</code>, where <code>other</code> is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("/")]
        public static double Divide(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return self / Protocols.ConvertToDouble(context, other);
        }

        /// <summary>
        /// Returns a new float which is the result of dividing <code>self</code> * by <code>other</code>, where <code>other</code> is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("/")]
        public static double Divide(double self, double other) {
            return self / other;
        }

        /// <summary>
        /// Returns a new float which is the result of dividing <code>self</code> * by <code>other</code>, where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("/")]
        public static object Divide(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
        }

        #endregion

        #region %, modulo

        /// <summary>
        /// Return the modulo after division of <code>self</code> by <code>other</code>, where <code>other</code> is Fixnum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static double Modulo(double self, int other) {
            return (double)InternalDivMod(self, (double)other)[1];
        }

        /// <summary>
        /// Return the modulo after division of <code>self</code> by <code>other</code>, where <code>other</code> is Bignum.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static double Modulo(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return (double)InternalDivMod(self, Protocols.ConvertToDouble(context, other))[1];
        }

        /// <summary>
        /// Return the modulo after division of <code>self</code> by <code>other</code>, where <code>other</code> is Float.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static double Modulo(double self, double other) {
            return (double)InternalDivMod(self, other)[1];
        }

        /// <summary>
        /// Return the modulo after division of <code>self</code> by <code>other</code>, where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("%")]
        public static object ModuloOp(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
        }

        /// <summary>
        /// Return the modulo after division of <code>self</code> by <code>other</code>, where <code>other</code> is not Fixnum, Bignum or Float.
        /// </summary>
        /// <returns></returns>
        [RubyMethod("modulo")]
        public static object Modulo(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
        }

        #endregion

        #region **

        /// <summary>
        /// Raises <code>self</code> the <code>other</code> power, where other is Fixnum.
        /// </summary>
        [RubyMethod("**")]
        public static double Power(double self, int other) {
            return Math.Pow(self, (double)other);
        }

        /// <summary>
        /// Raises <code>self</code> the <code>other</code> power, where other is Bignum.
        /// </summary>
        [RubyMethod("**")]
        public static double Power(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return Math.Pow(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Raises <code>self</code> the <code>other</code> power, where other is Float.
        /// </summary>
        [RubyMethod("**")]
        public static double Power(double self, double other) {
            return Math.Pow(self, other);
        }

        /// <summary>
        /// Raises <code>self</code> the <code>other</code> power, where other is not Fixnum, Bignum or Float.
        /// </summary>
        [RubyMethod("**")]
        public static object Power(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, other);
        }

        #endregion

        #region divmod

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing <i>self</i> by <i>other</i>, where other is Fixnum.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// If <code>q, r = x.divmod(y)</code>, then
        /// <code>q = floor(float(x)/float(y))
        /// x = q*y + r</code>
        /// The quotient is rounded toward -infinity
        /// </remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(double self, int other) {
            return DivMod(self, (double)other);
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing <i>self</i> by <i>other</i>, where other is Bignum.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// If <code>q, r = x.divmod(y)</code>, then
        /// <code>q = floor(float(x)/float(y))
        /// x = q*y + r</code>
        /// The quotient is rounded toward -infinity
        /// </remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return DivMod(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing <i>self</i> by <i>other</i>, where other is Float
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// If <code>q, r = x.divmod(y)</code>, then
        /// <code>q = floor(float(x)/float(y))
        /// x = q*y + r</code>
        /// The quotient is rounded toward -infinity
        /// </remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(double self, double other) {
            RubyArray result = InternalDivMod(self, other);
            // Unlike modulo, divmod blows up if the quotient or modulus are not finite, so we can't put this inside InternalDivMod
            // We only need to test if the quotient is double since it should have been converted to Integer (Fixnum or Bignum) if it was OK.
            if (result[0] is double || Double.IsNaN((double)result[1])) {
                throw CreateFloatDomainError("NaN");
            }
            return result;
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing <i>self</i> by <i>other</i>, where other is not Fixnum, Bignum or Float.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <remarks>
        /// If <code>q, r = x.divmod(y)</code>, then
        /// <code>q = floor(float(x)/float(y))
        /// x = q*y + r</code>
        /// The quotient is rounded toward -infinity
        /// </remarks>
        [RubyMethod("divmod")]
        public static object DivMod(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, double self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
        }

        #endregion

        #region abs

        /// <summary>
        /// Returns the absolute value of <i>self</i>.
        /// </summary>
        /// <example>
        ///  (-34.56).abs   #=> 34.56
        ///  -34.56.abs     #=> 34.56
        /// </example>
        [RubyMethod("abs")]
        public static double Abs(double self) {
            return Math.Abs(self);
        }

        #endregion

        #endregion

        #region Conversion Methods

        #region ceil

        /// <summary>
        /// Returns the smallest <code>Integer</code> greater than or equal to <code>self</code>
        /// </summary>
        /// <example>
        /// 1.2.ceil      #=> 2
        /// 2.0.ceil      #=> 2
        /// (-1.2).ceil   #=> -1
        /// (-2.0).ceil   #=> -2
        /// </example>
        [RubyMethod("ceil")]
        public static object Ceil(double self) {
            double ceil = System.Math.Ceiling(self);
            return CastToInteger(ceil);
        }

        #endregion

        #region floor

        /// <summary>
        /// Returns the largest <code>Integer</code> less than or equal to <code>self</code>.
        /// </summary>
        /// <example>
        /// 1.2.floor      #=> 1
        /// 2.0.floor      #=> 2
        /// (-1.2).floor   #=> -2
        /// (-2.0).floor   #=> -2
        /// </example>
        [RubyMethod("floor")]
        public static object Floor(double self) {
            double floor = System.Math.Floor(self);
            return CastToInteger(floor);
        }

        #endregion

        #region to_i, to_int, truncate

        /// <summary>
        /// Returns <code>self</code> truncated to an <code>Integer</code>.
        /// </summary>
        [RubyMethod("to_i"), RubyMethod("to_int"), RubyMethod("truncate")]
        public static object ToInt(double self) {
            if (self >= 0) {
                return Floor(self);
            } else {
                return Ceil(self);
            }
        }

        #endregion

        #region coerce

        /// <summary>
        /// Attempts to coerce other to a Float.
        /// </summary>
        /// <returns>[other, self] as Floats</returns>
        [RubyMethod("coerce")]
        public static RubyArray/*!*/ Coerce(double self, [DefaultProtocol]double other) {
            return RubyOps.MakeArray2(other, self);
        }

        #endregion

        #region to_f

        /// <summary>
        /// Converts self to Float
        /// </summary>
        /// <remarks>
        /// As <code>self</code> is already Float, returns <code>self</code>.
        /// </remarks>
        [RubyMethod("to_f")]
        public static double ToFloat(double self) {
            return self;
        }

        #endregion

        #region round

        /// <summary>
        /// Rounds <code>self</code> to the nearest <code>Integer</code>.
        /// </summary>
        /// <remarks>
        /// This is equivalent to:
        /// <code>
        /// def round
        ///     return (self+0.5).floor if self &gt; 0.0
        ///     return (self-0.5).ceil  if self &lt; 0.0
        ///     return 0
        /// end
        /// </code>
        /// </remarks>
        /// <example>
        /// 1.5.round      #=> 2
        /// (-1.5).round   #=> -2
        /// </example>
        [RubyMethod("round")]
        public static object Round(double self) {
            if (self > 0) { return Floor(self + 0.5); }
            if (self < 0) { return Ceil(self - 0.5); }
            return 0;
        }

        #endregion

        #region inspect, to_s

        /// <summary>
        /// Returns a string containing a representation of self.
        /// </summary>
        /// <remarks>
        /// As well as a fixed or exponential form of the number, the call may return
        /// "<code>NaN</code>", "<code>Infinity</code>", and "<code>-Infinity</code>".
        /// </remarks>
        [RubyMethod("to_s")]
        public static MutableString ToS(RubyContext/*!*/ context, double self) {
            StringFormatter sf = new StringFormatter(context, "%.15g", RubyEncoding.Binary, new object[] { self });
            sf.TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        #endregion

        #region hash

        /// <summary>
        /// Returns a hash code for <code>self</code>.
        /// </summary>
        [RubyMethod("hash")]
        public static int Hash(double self) {
            return self.GetHashCode();
        }

        #endregion

        #endregion

        #region Comparison Operators

        #region ==

        /// <summary>
        /// Returns <code>true</code> only if <i>other</i> has the same value as <i>self</i>, where other is Float
        /// </summary>
        /// <returns>True or False</returns>
        /// <remarks>
        /// Contrast this with <code>Float#eql?</code>, which requires <i>other</i> to be a <code>Float</code>.
        /// </remarks>
        [RubyMethod("==")]
        public static bool Equal(double self, double other) {
            return self == other;
        }

        /// <summary>
        /// Returns <code>true</code> only if <i>other</i> has the same value as <i>self</i>, where other is not a Float
        /// </summary>
        /// <returns>True or False</returns>
        /// <remarks>
        /// Contrast this with <code>Float#eql?</code>, which requires <i>other</i> to be a <code>Float</code>.
        /// Dynamically invokes other == self (i.e. swaps operands around).
        /// </remarks>
        [RubyMethod("==")]
        public static bool Equal(BinaryOpStorage/*!*/ equals, double self, object other) {
            // Call == on the right operand like Float#== does
            return Protocols.IsEqual(equals, other, self);
        }

        #endregion

        #region <=>

        /// <summary>
        /// Compares self with other, where other is Float.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self is not comparable to other (for instance if either is NaN).
        /// </returns>
        /// <remarks>
        /// This is the basis for the tests in <code>Comparable</code>.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(double self, double other) {
            if (Double.IsNaN(self) || Double.IsNaN(other)) {
                return null;
            }
            return self.CompareTo(other);
        }

        /// <summary>
        /// Compares self with other, where other is Fixnum.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self is not comparable to other (for instance if either is NaN).
        /// </returns>
        /// <remarks>
        /// This is the basis for the tests in <code>Comparable</code>.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(double self, int other) {
            if (Double.IsNaN(self)) {
                return null;
            }
            return self.CompareTo((double)other);
        }

        /// <summary>
        /// Compares self with other, where other is Bignum.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self is not comparable to other (for instance if either is NaN).
        /// </returns>
        /// <remarks>
        /// This is the basis for the tests in <code>Comparable</code>.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            if (Double.IsNaN(self)) {
                return null;
            }
            return self.CompareTo(Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Compares self with other, where other is not Float, Fixnum or Bignum.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self is not comparable to other (for instance if either is NaN).
        /// </returns>
        /// <remarks>
        /// This is the basis for the tests in <code>Comparable</code>.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, double self, object other) {
            return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
        }

        #endregion

        #region <

        /// <summary>
        /// Returns true if self is less than other, where other is Float.
        /// </summary>
        [RubyMethod("<")]
        public static bool LessThan(double self, double other) {
            if (double.IsNaN(self) || double.IsNaN(other)) {
                return false;
            }
            return self < other;
        }

        /// <summary>
        /// Returns true if self is less than other, where other is Fixnum.
        /// </summary>
        [RubyMethod("<")]
        public static bool LessThan(double self, int other) {
            return LessThan(self, (double)other);
        }

        /// <summary>
        /// Returns true if self is less than other, where other is Bignum.
        /// </summary>
        [RubyMethod("<")]
        public static bool LessThan(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return LessThan(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Returns true if self is less than other, where other is not Float, Fixnum or Bignum.
        /// </summary>
        [RubyMethod("<")]
        public static bool LessThan(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, double self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<", self, other);
        }

        #endregion

        #region <=

        /// <summary>
        /// Returns true if self is less than or equal to other, where other is Float.
        /// </summary>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(double self, double other) {
            if (double.IsNaN(self) || double.IsNaN(other)) {
                return false;
            }
            return self <= other;
        }

        /// <summary>
        /// Returns true if self is less than or equal to other, where other is Fixnum.
        /// </summary>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(double self, int other) {
            return LessThanOrEqual(self, (double)other);
        }

        /// <summary>
        /// Returns true if self is less than or equal to other, where other is Bignum.
        /// </summary>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return LessThanOrEqual(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Returns true if self is less than or equal to other, where other is not Float, Fixnum or Bignum.
        /// </summary>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, double self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<=", self, other);
        }

        #endregion

        #region >

        /// <summary>
        /// Returns true if self is greater than other, where other is Float.
        /// </summary>
        [RubyMethod(">")]
        public static bool GreaterThan(double self, double other) {
            if (double.IsNaN(self) || double.IsNaN(other)) {
                return false;
            }
            return self > other;
        }

        /// <summary>
        /// Returns true if self is greater than other, where other is Fixnum.
        /// </summary>
        [RubyMethod(">")]
        public static bool GreaterThan(double self, int other) {
            return GreaterThan(self, (double)other);
        }

        /// <summary>
        /// Returns true if self is greater than other, where other is Bignum.
        /// </summary>
        [RubyMethod(">")]
        public static bool GreaterThan(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return GreaterThan(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Returns true if self is greater than other, where other is not Float, Fixnum or Bignum.
        /// </summary>
        [RubyMethod(">")]
        public static bool GreaterThan(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, double self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">", self, other);
        }

        #endregion

        #region >=

        /// <summary>
        /// Returns true if self is greater than or equal to other, where other is Float.
        /// </summary>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(double self, double other) {
            if (double.IsNaN(self) || double.IsNaN(other)) {
                return false;
            }
            return self >= other;
        }

        /// <summary>
        /// Returns true if self is greater than or equal to other, where other is Fixnum.
        /// </summary>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(double self, int other) {
            return GreaterThanOrEqual(self, (double)other);
        }

        /// <summary>
        /// Returns true if self is greater than or equal to other, where other is Bignum.
        /// </summary>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(RubyContext/*!*/ context, double self, [NotNull]BigInteger/*!*/ other) {
            return GreaterThanOrEqual(self, Protocols.ConvertToDouble(context, other));
        }

        /// <summary>
        /// Returns true if self is greater than or equal to other, where other is not Float, Fixnum or Bignum.
        /// </summary>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, double self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">=", self, other);
        }

        #endregion

        #region finite?

        /// <summary>
        /// Returns <code>true</code> if <code>self</code> is a valid IEEE floating point number
        /// (it is not infinite, and <code>nan?</code> is <code>false</code>).
        /// </summary>
        [RubyMethod("finite?")]
        public static bool IsFinite(double self) {
            return !double.IsInfinity(self);
        }

        #endregion

        #region infinite?

        /// <summary>
        /// Returns <code>nil</code>, -1, or +1 depending on whether <code>self</code> is finite, -infinity, or +infinity.
        /// </summary>
        /// <example>
        /// (0.0).infinite?        #=> nil
        /// (-1.0/0.0).infinite?   #=> -1
        /// (+1.0/0.0).infinite?   #=> 1
        /// </example>
        [RubyMethod("infinite?")]
        public static object IsInfinite(double self) {
            if (double.IsInfinity(self)) {
                return double.IsPositiveInfinity(self) ? 1 : -1;
            } else {
                return null;
            }
        }

        #endregion

        #region nan?

        /// <summary>
        /// Returns <code>true</code> if <i>self</i> is an invalid IEEE floating point number.
        /// </summary>
        /// <example>
        /// a = -1.0      #=> -1.0
        /// a.nan?        #=> false
        /// a = 0.0/0.0   #=> NaN
        /// a.nan?        #=> true
        /// </example>
        [RubyMethod("nan?")]
        public static bool IsNan(double self) {
            return double.IsNaN(self);
        }

        #endregion

        #region zero?

        /// <summary>
        /// Returns <code>true</code> if <code>self</code> is 0.0.
        /// </summary>
        [RubyMethod("zero?")]
        public static bool IsZero(double self) {
            return self.Equals(0.0);
        }

        #endregion

        #endregion

        #region Helpers

        private static RubyArray InternalDivMod(double self, double other) {
            double div = System.Math.Floor(self / other);
            double mod = self - (div * other);
            if (other * mod < 0) {
                mod += other;
                div -= 1.0;
            }
            object intDiv = div;
            if (!Double.IsInfinity(div) && !Double.IsNaN(div)) {
                intDiv = ToInt(div);
            }
            return RubyOps.MakeArray2(intDiv, mod);
        }

        // I did think about whether to put this into Protocols but really it is only checking the FloatDomainErrors
        // Also, it is only used in FloatOps.Floor and FloatOps.Ceil.
        // These get invoked from Protocols.ConvertToInteger anyway so it would all get a bit circular.
        private static object CastToInteger(double value) {
            try {
                if (Double.IsPositiveInfinity(value)) {
                    throw new FloatDomainError("Infinity");
                }
                if (Double.IsNegativeInfinity(value)) {
                    throw new FloatDomainError("-Infinity");
                }
                if (Double.IsNaN(value)) {
                    throw new FloatDomainError("NaN");
                }
                return System.Convert.ToInt32(value);
            } catch (OverflowException) {
                return BigInteger.Create(value);
            }
        }

        public static Exception CreateFloatDomainError(string message) {
            return new FloatDomainError("NaN");
        }

        public static Exception CreateFloatDomainError(string message, Exception inner) {
            return new FloatDomainError("NaN", inner);
        }

        #endregion
    }
}
