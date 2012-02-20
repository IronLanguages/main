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
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Globalization;

namespace IronRuby.Builtins {

    /// <summary>
    /// Mixed-in all .NET numeric primitive types that cannot be widened to 32 bit signed integer.
    /// (uint, long, ulong, BigInteger). 
    /// </summary>
    [RubyModule("BigInteger", DefineIn = typeof(IronRubyOps.Clr))]
    public sealed class ClrBigInteger {
        #region Arithmetic Operators

        #region -@

        /// <summary>
        /// Unary minus (returns a new Bignum whose value is 0-self)
        /// </summary>
        /// <returns>0 minus self</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("-@")]
        public static object Negate(BigInteger/*!*/ self) {
            return Protocols.Normalize(BigInteger.Negate(self));
        }

        #endregion

        #region abs

        /// <summary>
        /// Returns the absolute value of self
        /// </summary>
        /// <returns>self if self >= 0; -self if self &lt; 0</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("abs")]
        public static object Abs(BigInteger/*!*/ self) {
            return Protocols.Normalize(self.Abs());
        }

        #endregion

        #region +

        /// <summary>
        /// Adds self and other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self + other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("+")]
        public static object Add(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self + other);
        }

        /// <summary>
        /// Adds self and other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self + other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("+")]
        public static object Add(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self + other);
        }

        /// <summary>
        /// Adds self and other, where other is Float
        /// </summary>
        /// <returns>self + other as Float</returns>
        [RubyMethod("+")]
        public static object Add(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() + other;
        }

        /// <summary>
        /// Adds self and other, where other is not a Float, Fixnum or Bignum
        /// </summary>
        /// <returns>self + other</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes +</remarks>
        [RubyMethod("+")]
        public static object Add(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
        }

        #endregion

        #region -

        /// <summary>
        /// Subtracts other from self, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self - other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("-")]
        public static object Subtract(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self - other);
        }

        /// <summary>
        /// Subtracts other from self, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self - other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("-")]
        public static object Subtract(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self - other);
        }

        /// <summary>
        /// Subtracts other from self, where other is Float
        /// </summary>
        /// <returns>self - other as Float</returns>
        [RubyMethod("-")]
        public static object Subtract(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() - other;
        }

        /// <summary>
        /// Subtracts other from self, where other is not a Float, Fixnum or Bignum
        /// </summary>
        /// <returns>self - other</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes -</remarks>
        [RubyMethod("-")]
        public static object Subtract(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
        }

        #endregion

        #region *

        /// <summary>
        /// Multiplies self by other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self * other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("*")]
        public static object Multiply(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self * other);
        }

        /// <summary>
        /// Multiplies self by other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self * other</returns>
        /// <remarks>Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("*")]
        public static object Multiply(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self * other);
        }

        /// <summary>
        /// Multiplies self by other, where other is Float
        /// </summary>
        /// <returns>self * other as Float</returns>
        [RubyMethod("*")]
        public static object Multiply(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() * other;
        }

        /// <summary>
        /// Multiplies self by other, where other is not a Float, Fixnum or Bignum
        /// </summary>
        /// <returns>self * other</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes *</remarks>
        [RubyMethod("*")]
        public static object Multiply(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
        }

        #endregion

        #region /, div, fdiv

        /// <summary>
        /// Divides self by other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self / other</returns>
        /// <remarks>Uses DivMod to do the division (directly).  Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("/"), RubyMethod("div")]
        public static object Divide(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return DivMod(self, other)[0];
        }

        /// <summary>
        /// Divides self by other, where other is Bignum or Fixnum
        /// </summary>
        /// <returns>self / other</returns>
        /// <remarks>Uses DivMod to do the division (directly).  Normalizes to a Fixnum if necessary</remarks>
        [RubyMethod("/"), RubyMethod("div")]
        public static object Divide(BigInteger/*!*/ self, int other) {
            return DivMod(self, other)[0];
        }

        /// <summary>
        /// Divides self by other, where other is Float
        /// </summary>
        /// <returns>self / other as Float</returns>
        [RubyMethod("/")]
        public static object DivideOp(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() / other;
        }

        /// <summary>
        /// Divides self by other, where other is Float
        /// </summary>
        /// <returns>self divided by other as Float</returns>
        [RubyMethod("div")]
        public static object Divide(BigInteger/*!*/ self, double other) {
            return DivMod(self, other)[0];
        }

        /// <summary>
        /// Divides self by other, where other is not a Float, Fixnum or Bignum
        /// </summary>
        /// <returns>self / other</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes /</remarks>
        [RubyMethod("/")]
        public static object Divide(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
        }

        /// <summary>
        /// Divides self by other, where other is not a Float, Fixnum or Bignum
        /// </summary>
        /// <returns>self.div(other)</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes div</remarks>
        [RubyMethod("div")]
        public static object Div(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "div", self, other);
        }

        [RubyMethod("fdiv", Compatibility = RubyCompatibility.Ruby19)]
        public static double FDiv(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return ((double)self) / ((double)other);
        }

        #endregion

        #region quo

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is Bignum or Fixnum. 
        /// </summary>
        /// <returns>self divided by other as Float</returns>
        /// <remarks>Converts self and other to Float and then divides.</remarks>
        [RubyMethod("quo")]
        public static object Quotient(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Quotient(self, other.ToFloat64());
        }

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is Bignum or Fixnum. 
        /// </summary>
        /// <returns>self divided by other as Float</returns>
        /// <remarks>Converts self and other to Float and then divides.</remarks>
        [RubyMethod("quo")]
        public static object Quotient(BigInteger/*!*/ self, int other) {
            return Quotient(self, (double)other);
        }

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is Float. 
        /// </summary>
        /// <returns>self divided by other as Float</returns>
        /// <remarks>Converts self to Float and then divides.</remarks>
        [RubyMethod("quo")]
        public static object Quotient(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() / other;
        }

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is not Bignum, Fixnum or Float. 
        /// </summary>
        /// <returns>self divided by other as Float</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes quo</remarks>
        [RubyMethod("quo")]
        public static object Quotient(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "quo", self, other);
        }

        #endregion

        #region **

        /// <summary>
        /// Raises self to the exponent power, where exponent is Bignum.
        /// </summary>
        /// <returns>self ** exponent as Float </returns>
        /// <remarks>Converts self and exponent to Float (directly) and then calls System.Math.Pow</remarks>
        [RubyMethod("**")]
        public static object Power(RubyContext/*!*/ context, BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ exponent) {
            context.ReportWarning("in a**b, b may be too big");
            double result = Math.Pow(self.ToFloat64(), exponent.ToFloat64());
            return result;
        }

        /// <summary>
        /// Raises self to the exponent power, where exponent is Fixnum
        /// </summary>
        /// <returns>self ** exponent</returns>
        /// <remarks>
        /// Returns Bignum or Fixnum if exponent &gt;= 0.
        /// Returns Float if exponent &lt; 0
        /// </remarks>
        [RubyMethod("**")]
        public static object Power(BigInteger/*!*/ self, int exponent) {
            // BigInteger doesn't handle negative exponents.
            if (exponent < 0) {
                return Power(self, (double)exponent);
            }
            return Protocols.Normalize(self.Power(exponent));
        }

        /// <summary>
        /// Raises self to the exponent power, where exponent is Float
        /// </summary>
        /// <returns>self ** exponent as Float</returns>
        /// <remarks>Converts self to Float (directly) then calls System.Math.Pow</remarks>
        [RubyMethod("**")]
        public static object Power(BigInteger/*!*/ self, double exponent) {
            return Math.Pow(self.ToFloat64(), exponent);
        }

        /// <summary>
        /// Raises self to the exponent power, where exponent is not Fixnum, Bignum or Float
        /// </summary>
        /// <returns>self ** exponent</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes **</remarks>
        [RubyMethod("**")]
        public static object Power(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object exponent) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, exponent);
        }

        #endregion

        #region modulo, %

        /// <summary>
        /// Returns self modulo other, where other is Fixnum or Bignum.
        /// </summary>
        /// <returns>self modulo other, as Fixnum or Bignum</returns>
        /// <remarks>Calls divmod directly to get the modulus.</remarks>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static object Modulo(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            RubyArray result = DivMod(self, other);
            return result[1];
        }

        /// <summary>
        /// Returns self modulo other, where other is Fixnum or Bignum.
        /// </summary>
        /// <returns>self modulo other, as Fixnum or Bignum</returns>
        /// <remarks>Calls divmod directly to get the modulus.</remarks>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static object Modulo(BigInteger/*!*/ self, int other) {
            RubyArray result = DivMod(self, other);
            return result[1];
        }

        /// <summary>
        /// Returns self modulo other, where other is Float.
        /// </summary>
        /// <returns>self modulo other, as Float</returns>
        /// <remarks>Calls divmod directly to get the modulus.</remarks>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static object Modulo(BigInteger/*!*/ self, double other) {
            if (other == 0.0) {
                return Double.NaN;
            }
            RubyArray result = DivMod(self, other);
            return result[1];
        }

        /// <summary>
        /// Returns self % other, where other is not Fixnum or Bignum.
        /// </summary>
        /// <returns>self % other, as Fixnum or Bignum</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes %</remarks>
        [RubyMethod("%")]
        public static object ModuloOp(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
        }

        /// <summary>
        /// Returns self modulo other, where other is not Fixnum or Bignum.
        /// </summary>
        /// <returns>self modulo other, as Fixnum or Bignum</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes modulo</remarks>
        [RubyMethod("modulo")]
        public static object Modulo(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
        }

        #endregion

        #region divmod

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other, where other is Fixnum or Bignum.
        /// If <code>q, r = x.divmod(y)</code>, then 
        ///     <code>q = floor(float(x)/float(y))</code>
        ///     <code>x = q*y + r</code>
        /// </summary>
        /// <returns>[self div other, self modulo other] as RubyArray</returns>
        /// <remarks>Normalizes div and mod to Fixnum as necessary</remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            BigInteger mod;
            BigInteger div = BigInteger.DivRem(self, other, out mod);
            if (self.Sign != other.Sign && !mod.IsZero()) {
                div = div - 1;
                mod = mod + other;
            }
            return RubyOps.MakeArray2(Protocols.Normalize(div), Protocols.Normalize(mod));
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other, where other is Fixnum or Bignum.
        /// If <code>q, r = x.divmod(y)</code>, then 
        ///     <code>q = floor(float(x)/float(y))</code>
        ///     <code>x = q*y + r</code>
        /// </summary>
        /// <returns>[self div other, self modulo other] as RubyArray</returns>
        /// <remarks>Normalizes div and mod to Fixnum as necessary</remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(BigInteger/*!*/ self, int other) {
            return DivMod(self, (BigInteger)other);
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other, where other is Float.
        /// If <code>q, r = x.divmod(y)</code>, then 
        ///     <code>q = floor(float(x)/float(y))</code>
        ///     <code>x = q*y + r</code>
        /// </summary>
        /// <returns>[self div other, self modulo other] as RubyArray</returns>
        /// <remarks>Normalizes div to Fixnum as necessary</remarks>
        [RubyMethod("divmod")]
        public static RubyArray DivMod(BigInteger/*!*/ self, double other) {
            if (other == 0.0) {
                throw new FloatDomainError("NaN");
            }

            double selfFloat = self.ToFloat64();
            BigInteger div = BigInteger.Create(selfFloat / other);
            double mod = selfFloat % other;

            return RubyOps.MakeArray2(Protocols.Normalize(div), mod);
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other, where other is not Fixnum or Bignum.
        /// If <code>q, r = x.divmod(y)</code>, then 
        ///     <code>q = floor(float(x)/float(y))</code>
        ///     <code>x = q*y + r</code>
        /// </summary>
        /// <returns>Should return [self div other, self modulo other], but the divmod implementation is free to return an arbitrary object.</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes divmod</remarks>
        [RubyMethod("divmod")]
        public static object DivMod(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
        }

        #endregion

        #region remainder

        /// <summary>
        /// Returns the remainder after dividing self by other, where other is Fixnum or Bignum.
        /// </summary>
        /// <example>
        /// -1234567890987654321.remainder(13731)      #=> -6966
        /// </example>
        /// <returns>Fixnum or Bignum</returns>
        [RubyMethod("remainder")]
        public static object Remainder(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            BigInteger remainder;
            BigInteger.DivRem(self, other, out remainder);
            return Protocols.Normalize(remainder);
        }

        /// <summary>
        /// Returns the remainder after dividing self by other, where other is Fixnum or Bignum.
        /// </summary>
        /// <example>
        /// -1234567890987654321.remainder(13731)      #=> -6966
        /// </example>
        /// <returns>Fixnum or Bignum</returns>
        [RubyMethod("remainder")]
        public static object Remainder(BigInteger/*!*/ self, int other) {
            BigInteger remainder;
            BigInteger.DivRem(self, other, out remainder);
            return Protocols.Normalize(remainder);
        }

        /// <summary>
        /// Returns the remainder after dividing self by other, where other is Float.
        /// </summary>
        /// <example>
        /// -1234567890987654321.remainder(13731.24)   #=> -9906.22531493148
        /// </example>
        /// <returns>Float</returns>
        [RubyMethod("remainder")]
        public static double Remainder(BigInteger/*!*/ self, double other) {
            return self.ToFloat64() % other;
        }

        /// <summary>
        /// Returns the remainder after dividing self by other, where other is not Fixnum or Bignum.
        /// </summary>
        /// <example>
        /// -1234567890987654321.remainder(13731)      #=> -6966
        /// </example>
        /// <returns>Fixnum or Bignum</returns>
        /// <remarks>Coerces self and other using other.coerce(self) then dynamically invokes remainder</remarks>
        [RubyMethod("remainder")]
        public static object Remainder(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "remainder", self, other);
        }

        #endregion

        #endregion

        #region Comparisons

        #region <=>

        /// <summary>
        /// Comparison operator, where other is Bignum or Fixnum. This is the basis for the tests in Comparable.
        /// </summary>
        /// <returns>
        /// Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other.
        /// </returns>
        [RubyMethod("<=>")]
        public static int Compare(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return BigInteger.Compare(self, other);
        }

        /// <summary>
        /// Comparison operator, where other is Bignum or Fixnum. This is the basis for the tests in Comparable.
        /// </summary>
        /// <returns>
        /// Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other.
        /// </returns>
        [RubyMethod("<=>")]
        public static int Compare(BigInteger/*!*/ self, int other) {
            return BigInteger.Compare(self, (BigInteger)other);
        }

        /// <summary>
        /// Comparison operator, where other is Float. This is the basis for the tests in Comparable.
        /// </summary>
        /// <returns>
        /// Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other.
        /// </returns>
        /// <remarks>
        /// Converts self to Float and then directly invokes &lt;=&gt;.
        /// Correctly copes if self is too big to fit into a Float, i.e. assumes self is +/-Infinity.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(RubyContext/*!*/ context, BigInteger/*!*/ self, double other) {
            return ClrFloat.Compare(ToFloat(context, self), other);
        }

        /// <summary>
        /// Comparison operator, where other is not Bignum, Fixnum or Float. This is the basis for the tests in Comparable.
        /// </summary>
        /// <returns>
        /// Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other.
        /// </returns>
        /// <remarks>
        /// Dynamically invokes &lt;=&gt;.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, BigInteger/*!*/ self, object other) {
            return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
        }

        #endregion

        #region ==

        /// <summary>
        /// Returns true if other has the same value as self, where other is Fixnum or Bignum.
        /// Contrast this with Bignum#eql?, which requires other to be a Bignum.
        /// </summary>
        /// <returns>true or false</returns>
        [RubyMethod("==")]
        public static bool Equal(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return self == other;
        }

        /// <summary>
        /// Returns true if other has the same value as self, where other is Fixnum or Bignum.
        /// Contrast this with Bignum#eql?, which requires other to be a Bignum.
        /// </summary>
        /// <returns>true or false</returns>
        [RubyMethod("==")]
        public static bool Equal(BigInteger/*!*/ self, int other) {
            return self == other;
        }

        /// <summary>
        /// Returns true if other has the same value as self, where other is Float.
        /// Contrast this with Bignum#eql?, which requires other to be a Bignum.
        /// </summary>
        /// <returns>true or false</returns>
        /// <remarks>Returns false if other is NaN.</remarks>
        [RubyMethod("==")]
        public static bool Equal(RubyContext/*!*/ context, BigInteger/*!*/ self, double other) {
            return !Double.IsNaN(other) && Protocols.ConvertToDouble(context, self) == other;
        }

        /// <summary>
        /// Returns true if other has the same value as self, where other is not Fixnum, Bignum or Float.
        /// Contrast this with Bignum#eql?, which requires other to be a Bignum.
        /// </summary>
        /// <returns>true or false</returns>
        /// <remarks>Dynamically invokes other == self (i.e. swaps self and other around)</remarks>
        [RubyMethod("==")]
        public static bool Equal(BinaryOpStorage/*!*/ equals, BigInteger/*!*/ self, object other) {
            // If we can't convert then swap self and other and try again.
            return Protocols.IsEqual(equals, other, self);
        }

        #endregion

        #region eql?

        /// <summary>
        /// Returns true only if other is a Bignum with the same value as self.
        /// Contrast this with Bignum#==, which performs type conversions. 
        /// </summary>
        /// <returns>true if other is Bignum and self == other</returns>
        [RubyMethod("eql?")]
        public static bool Eql(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return self == other;
        }

        /// <summary>
        /// Returns true only if other is a Bignum with the same value as self, where other is Fixnum.
        /// Contrast this with Bignum#==, which performs type conversions. 
        /// </summary>
        /// <returns>false</returns>
        /// <remarks>
        /// Always returns false since other is not Bignum.
        /// This overload is necessary otherwise the int will be implicitly cast to BigInteger,
        /// even though it should always then return false in that case since other should
        /// be too small to be equal to self, it is just a waste of a conversion.</remarks>
        [RubyMethod("eql?")]
        public static bool Eql(BigInteger/*!*/ self, int other) {
            return false;
        }

        /// <summary>
        /// Returns true only if other is a Bignum with the same value as self, where other is not Bignum or Fixnum.
        /// Contrast this with Bignum#==, which performs type conversions. 
        /// </summary>
        /// <returns>false</returns>
        /// <remarks>Always returns false since other is not Bignum</remarks>
        [RubyMethod("eql?")]
        public static bool Eql(BigInteger/*!*/ self, object other) {
            return false;
        }

        #endregion

        #endregion

        #region Bitwise Operators

        #region <<

        /// <summary>
        /// Shifts self to the left by other bits (or to the right if other is negative).
        /// </summary>
        /// <returns>self &lt;&lt; other, as Bignum or Fixnum</returns>
        /// <remarks>
        /// If self is negative we have to check for running out of bits, in which case we return -1.
        /// This is because Bignum is supposed to look like it is stored in 2s complement format.
        /// </remarks>
        [RubyMethod("<<")]
        public static object/*!*/ LeftShift(BigInteger/*!*/ self, int other) {
            BigInteger result = self << other;
            result = ShiftOverflowCheck(self, result);
            return Protocols.Normalize(result);
        }

        [RubyMethod("<<")]
        public static object/*!*/ LeftShift(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            // Dodgy error message but matches MRI
            throw RubyExceptions.CreateRangeError("bignum too big to convert into long");
        }

        /// <summary>
        /// Shifts self to the left by other bits (or to the right if other is negative).
        /// </summary>
        /// <returns>self &lt;&lt; other, as Bignum or Fixnum</returns>
        /// <remarks>other is converted to an Integer by dynamically invoking self.to_int</remarks>
        [RubyMethod("<<")]
        public static object/*!*/ LeftShift(RubyContext/*!*/ context, BigInteger/*!*/ self, [DefaultProtocol]IntegerValue other) {
            return other.IsFixnum ? LeftShift(self, other.Fixnum) : LeftShift(self, other.Bignum);
        }

        #endregion

        #region >>

        /// <summary>
        /// Shifts self to the right by other bits (or to the left if other is negative).
        /// </summary>
        /// <returns>self >> other, as Bignum or Fixnum</returns>
        /// <remarks>
        /// If self is negative we have to check for running out of bits, in which case we return -1.
        /// This is because Bignum is supposed to look like it is stored in 2s complement format.
        /// </remarks>
        [RubyMethod(">>")]
        public static object/*!*/ RightShift(BigInteger/*!*/ self, int other) {
            BigInteger result = self >> other;
            result = ShiftOverflowCheck(self, result);
            return Protocols.Normalize(result);
        }

        [RubyMethod(">>")]
        public static object/*!*/ RightShift(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            if (self.IsNegative()) {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Shifts self to the left by other bits (or to the right if other is negative).
        /// </summary>
        /// <returns>self >> other, as Bignum or Fixnum</returns>
        /// <remarks>other is converted to an Integer by dynamically invoking self.to_int</remarks>
        [RubyMethod(">>")]
        public static object/*!*/ RightShift(RubyContext/*!*/ context, BigInteger/*!*/ self, [DefaultProtocol]IntegerValue other) {
            return other.IsFixnum ? RightShift(self, other.Fixnum) : RightShift(self, other.Bignum);
        }

        #endregion

        #region |

        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self | other);
        }

        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self | other);
        }

        /// <summary>
        /// Performs bitwise or between self and other, where other is not Fixnum or Bignum. 
        /// </summary>
        /// <remarks>other is dynamically converted to an Integer by other.to_int then | is invoked dynamically. E.g. self | (index.to_int)</remarks>
        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(RubyContext/*!*/ context, BigInteger/*!*/ self, [DefaultProtocol]IntegerValue other) {
            return other.IsFixnum ? BitwiseOr(self, other.Fixnum) : BitwiseOr(self, other.Bignum);
        }

        #endregion

        #region &

        [RubyMethod("&")]
        public static object/*!*/ And(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self & other);
        }

        [RubyMethod("&")]
        public static object/*!*/ And(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self & other);
        }

        /// <summary>
        /// Performs bitwise and between self and other, where other is not Fixnum or Bignum. 
        /// </summary>
        /// <remarks>other is dynamically converted to an Integer by other.to_int then "&amp;" is invoked dynamically. E.g. self &amp; (index.to_int)</remarks>
        [RubyMethod("&")]
        public static object/*!*/ And(RubyContext/*!*/ context, BigInteger/*!*/ self, [DefaultProtocol]IntegerValue other) {
            return other.IsFixnum ? And(self, other.Fixnum) : And(self, other.Bignum);
        }

        #endregion

        #region ^

        [RubyMethod("^")]
        public static object/*!*/ Xor(BigInteger/*!*/ self, int other) {
            return Protocols.Normalize(self ^ other);
        }

        [RubyMethod("^")]
        public static object/*!*/ Xor(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return Protocols.Normalize(self ^ other);
        }

        /// <summary>
        /// Performs bitwise xor between self and other, where other is not Fixnum or Bignum. 
        /// </summary>
        /// <remarks>other is dynamically converted to an Integer by other.to_int then ^ is invoked dynamically. E.g. self ^ (index.to_int)</remarks>
        [RubyMethod("^")]
        public static object/*!*/ Xor(RubyContext/*!*/ context, BigInteger/*!*/ self, [DefaultProtocol]IntegerValue other) {
            return other.IsFixnum ? Xor(self, other.Fixnum) : Xor(self, other.Bignum);
        }

        #endregion

        #region ~

        /// <summary>
        /// Performs bitwise inversion on self.
        /// </summary>
        [RubyMethod("~")]
        public static object Invert(BigInteger/*!*/ self) {
            return Protocols.Normalize(~self);
        }

        #endregion

        #region []

        /// <summary>
        /// Returns the Bit value at the reference index, where index is Fixnum
        /// </summary>
        /// <example>
        /// <code>
        ///   a = 9**15
        ///   50.downto(0) do |n|
        ///     print a[n]
        ///   end
        /// </code>
        /// produces: 
        /// <code>
        ///   000101110110100000111000011110010100111100010111001
        /// </code>
        /// </example>
        /// <returns>indexth bit in the (assumed) binary representation of self, where self[0] is the least significant bit.</returns>
        /// <remarks>Since representation is supposed to be 2s complement, we return always 1 if self is negative and index is greater than most signifcant bit in BigInteger</remarks>
        [RubyMethod("[]")]
        public static int Bit(BigInteger/*!*/ self, [DefaultProtocol]int index) {
            // If we are outside the range then return 0 ...
            if (index < 0) return 0;

            int bytePos = index / 8;
            int bitOffset = index % 8;
            byte[] data = self.ToByteArray();

            // ... or 1 if the index is too high and BigInteger is negative.
            if (bytePos >= data.Length) return (self.Sign > 0) ? 0 : 1;

            return (data[bytePos] & (1 << bitOffset)) != 0 ? 1 : 0;
        }

        /// <summary>
        /// Returns the Bit value at the reference index, where index is Bignum
        /// </summary>
        /// <returns>
        /// 0 if index is negative or self is positive
        /// 1 otherwise
        /// </returns>
        /// <remarks>
        /// Since representation is supposed to be 2s complement and index must be extremely big,
        /// we asssume we can always return 1 if self is negative and 0 otherwise</remarks>
        [RubyMethod("[]")]
        public static int Bit(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ index) {
            // BigIntegers as indexes are always going to be outside the range.
            if (index.IsNegative() || self.IsPositive()) return 0;
            return 1;
        }

        #endregion

        #endregion

        #region Conversion methods

        #region to_f

        /// <summary>
        /// Converts self to a Float. If self doesnt fit in a Float, the result is infinity. 
        /// </summary>
        /// <returns>self as a Float</returns>
        [RubyMethod("to_f")]
        public static double ToFloat(RubyContext/*!*/ context, BigInteger/*!*/ self) {
            return Protocols.ConvertToDouble(context, self);
        }

        #endregion

        #region to_s

        /// <summary>
        /// Returns a string containing the representation of self base 10.
        /// </summary>
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(BigInteger/*!*/ self) {
            return MutableString.CreateAscii(self.ToString());
        }

        /// <summary>
        /// Returns a string containing the representation of self base radix (2 through 36).
        /// </summary>
        /// <param name="radix">An integer between 2 and 36 inclusive</param>
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToString(BigInteger/*!*/ self, int radix) {
            if (radix < 2 || radix > 36) {
                throw RubyExceptions.CreateArgumentError("illegal radix {0}", radix);
            }

            // TODO: Can we do the ToLower in BigInteger?
            return MutableString.CreateAscii(self.ToString(radix).ToLowerInvariant());
        }

        #endregion

        #region coerce

        /// <summary>
        /// Attempts to coerce other to a Bignum.
        /// </summary>
        /// <returns>[other, self] as Bignums</returns>
        [RubyMethod("coerce")]
        public static RubyArray Coerce(BigInteger/*!*/ self, [NotNull]BigInteger/*!*/ other) {
            return RubyOps.MakeArray2(other, self);
        }

        /// <summary>
        /// Attempts to coerce other to a Bignum, where other is not Fixnum or Bignum.
        /// </summary>
        /// <exception cref="InvalidOperationException">For any value of other.</exception>
        [RubyMethod("coerce")]
        public static RubyArray Coerce(RubyContext/*!*/ context, BigInteger/*!*/ self, object other) {
            throw RubyExceptions.CreateTypeError("can't coerce {0} to Bignum", context.GetClassDisplayName(other));
        }

        #endregion

        #endregion

        #region hash

        /// <summary>
        /// Compute a hash based on the value of self. 
        /// </summary>
        [RubyMethod("hash")]
        public static int Hash(BigInteger/*!*/ self) {
            return self.GetHashCode();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Test for shift overflow on negative BigIntegers
        /// </summary>
        /// <param name="self">Value before shifting</param>
        /// <param name="result">Value after shifting</param>
        /// <returns>-1 if we overflowed, otherwise result</returns>
        /// <remarks>
        /// Negative Bignums are supposed to look like they are stored in 2s complement infinite bit string, 
        /// a negative number should always have spare 1s available for on the left hand side for right shifting.
        /// E.g. 8 == ...0001000; -8 == ...1110111, where the ... means that the left hand value is repeated indefinitely.
        /// The test here checks whether we have overflowed into the infinite 1s.
        /// [Arguably this should get factored into the BigInteger class.]
        /// </remarks>
        private static BigInteger/*!*/ ShiftOverflowCheck(BigInteger/*!*/ self, BigInteger/*!*/ result) {
            if (self.IsNegative() && result.IsZero()) {
                return -1;
            }
            return result;
        }

        #endregion
    }
}
