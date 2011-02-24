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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using Microsoft.Scripting.Generation;
using System.Runtime.CompilerServices;

namespace IronRuby.Builtins {
    /// <summary>
    /// Mixed-in all .NET numeric primitive types that can be widened to 32 bit signed integer 
    /// (byte, sbyte, short, ushort, int). 
    /// 
    /// TODO: we might want to specialize some of the methods to preserve the exact type if possible (like adding byte and byte).
    /// </summary>
    [RubyModule("Integer", DefineIn = typeof(IronRubyOps.Clr))]
    public static class ClrInteger {
        public static readonly object Zero = ScriptingRuntimeHelpers.Int32ToObject(0);
        public static readonly object One = ScriptingRuntimeHelpers.Int32ToObject(1);
        public static readonly object MinusOne = ScriptingRuntimeHelpers.Int32ToObject(-1);

        internal static object/*!*/ MinusMinValue() {
            return -(BigInteger)Int32.MinValue;
        }

        public static object/*!*/ Narrow(long value) {
            return (value >= Int32.MinValue && value <= Int32.MaxValue) ? (object)(Int32)value : (BigInteger)value;
        }

        #region Bitwise Operators

        #region <<

        /// <summary>
        /// Returns the value after shifting to the left (right if count is negative) the value in self by other bits.
        /// (where other is Fixnum)
        /// </summary>
        /// <returns>The value after the shift</returns>
        /// <remarks>Converts to Bignum if the result cannot fit into Fixnum</remarks>
        [RubyMethod("<<")]
        public static object/*!*/ LeftShift(int self, int shift) {
            if (self == 0) {
                return Zero;
            }

            if (shift == 0) {
                return self;
            }

            if (shift < 0) {
                if (shift == Int32.MinValue) {
                    return 0;
                } else {
                    return RightShift(self, -shift);
                }
            }
                
            // If 'self' has more than '31 - other' significant digits it will overflow:
            if (shift >= 31 || (self & ~((1 << (31 - shift)) - 1)) != 0) {
                return BigInteger.LeftShift(self, shift);
            }

            return self << shift;
        }

        /// <summary>
        /// Returns the value after shifting to the left (right if count is negative) the value in self by other bits.
        /// (where other is not Fixnum)
        /// </summary>
        /// <returns>The value after the shift</returns>
        /// <remarks>Converts to Bignum if the result cannot fit into Fixnum</remarks>
        [RubyMethod("<<")]
        public static object/*!*/ LeftShift(RubyContext/*!*/ context, int self, [DefaultProtocol]IntegerValue other) {
            return ClrBigInteger.LeftShift(context, self, other);
        }

        #endregion

        #region >>

        /// <summary>
        /// Returns the value after shifting to the right (left if count is negative) the value in self by other bits.
        /// (where other is Fixnum)
        /// </summary>
        /// <returns>The value after the shift</returns>
        /// <remarks>Converts to Bignum if the result cannot fit into Fixnum</remarks>
        [RubyMethod(">>")]
        public static object/*!*/ RightShift(int self, int shift) {
            if (shift < 0) {
                if (shift == Int32.MinValue) {
                    throw RubyExceptions.CreateRangeError("bignum too big to convert into long");
                } else {
                    return LeftShift(self, -shift);
                }
            } else if (shift == 0) {
                return self;
            } else if (shift >= 32) {
                return self < 0 ? MinusOne : Zero;
            } else {
                return self >> shift;
            }
        }

        /// <summary>
        /// Returns the value after shifting to the right (left if count is negative) the value in self by other bits.
        /// (where other is not Fixnum)
        /// </summary>
        /// <returns>The value after the shift</returns>
        /// <remarks>Converts to Bignum if the result cannot fit into Fixnum</remarks>
        [RubyMethod(">>")]
        public static object/*!*/ RightShift(RubyContext/*!*/ context, int self, [DefaultProtocol]IntegerValue other) {
            return ClrBigInteger.RightShift(context, self, other);
        }

        #endregion

        #region []

        /// <summary>
        /// Returns the value of the bit at the indexth bit position of self, where index is Fixnum
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
        public static int Bit(int self, [DefaultProtocol]int index) {
            if (index < 0) {
                return 0;
            }
            if (index > 32) {
                return self < 0 ? 1 : 0;
            }
            return (self & (1 << index)) != 0 ? 1 : 0;
        }

        /// <summary>
        /// Returns the value of the bit at the indexth bit position of self, where index is Bignum
        /// </summary>
        /// <returns>
        /// 0 if index is negative or self is positive
        /// 1 otherwise
        /// </returns>
        /// <remarks>
        /// Since representation is supposed to be 2s complement and index must be extremely big,
        /// we asssume we can always return 1 if self is negative and 0 otherwise</remarks>
        [RubyMethod("[]")]
        public static int Bit(int self, [NotNull]BigInteger/*!*/ index) {
            // BigIntegers as indexes are always going to be outside the range.
            if (index.IsNegative() || self >= 0) {
                return 0;
            } else {
                return 1;
            }
        }

        #endregion

        #region ^

        /// <summary>
        /// Performs bitwise XOR on self and other
        /// </summary>
        [RubyMethod("^")]
        public static object/*!*/ BitwiseXor(int self, int other) {
            return self ^ other;
        }

        /// <summary>
        /// Performs bitwise XOR on self and other
        /// </summary>
        [RubyMethod("^")]
        public static object/*!*/ BitwiseXor(int self, [NotNull]BigInteger/*!*/ other) {
            return other ^ self;
        }

        /// <summary>
        /// Performs bitwise XOR on self and other, where other is not Fixnum or Bignum
        /// </summary>
        [RubyMethod("^")]
        public static object/*!*/ BitwiseXor(RubyContext/*!*/ context, int self, [DefaultProtocol]IntegerValue other) {
            return ClrBigInteger.Xor(context, self, other);
        }

        #endregion

        #region &

        /// <summary>
        /// Performs bitwise AND on self and other, where other is Fixnum
        /// </summary>
        [RubyMethod("&")]
        public static int BitwiseAnd(int self, int other) {
            return self & other;
        }

        /// <summary>
        /// Performs bitwise AND on self and other, where other is Bignum
        /// </summary>
        [RubyMethod("&")]
        public static object/*!*/ BitwiseAnd(int self, [NotNull]BigInteger/*!*/ other) {
            BigInteger result = other & self;
            int ret;
            if (result.AsInt32(out ret)) {
                return ret;
            } else {
                return result;
            }
        }

        /// <summary>
        /// Performs bitwise AND on self and other, where other is not Fixnum or Bignum
        /// </summary>
        [RubyMethod("&")]
        public static object/*!*/ BitwiseAnd(RubyContext/*!*/ context, int self, [DefaultProtocol]IntegerValue other) {
            return ClrBigInteger.And(context, self, other);
        }

        #endregion

        #region |

        /// <summary>
        /// Performs bitwise OR on self and other
        /// </summary>
        [RubyMethod("|")]
        public static int BitwiseOr(int self, int other) {
            return self | other;
        }

        /// <summary>
        /// Performs bitwise OR on self and other
        /// </summary>
        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(int self, [NotNull]BigInteger/*!*/ other) {
            BigInteger result = other | self;
            int ret;
            if (result.AsInt32(out ret)) {
                return ret;
            } else {
                return result;
            }
        }

        /// <summary>
        /// Performs bitwise OR on self and other, where other is not Fixnum or Bignum
        /// </summary>
        [RubyMethod("|")]
        public static object/*!*/ BitwiseOr(RubyContext/*!*/ context, int self, [DefaultProtocol]IntegerValue other) {
            return ClrBigInteger.BitwiseOr(context, self, other);
        }

        #endregion

        #region ~

        /// <summary>
        /// Returns the ones complement of self; a number where each bit is flipped. 
        /// </summary>
        [RubyMethod("~")]
        public static int OnesComplement(int self) {
            return ~self;
        }

        #endregion

        #endregion

        #region Arithmetic Operators
        
        #region *

        /// <summary>
        /// Returns self multiplied by other, where other is Fixnum or Bignum.
        /// </summary>
        /// <returns>
        /// Returns either Fixnum or Bignum if the result is too large for Fixnum.
        /// </returns>
        [RubyMethod("*")]
        public static object/*!*/ Multiply(int self, int other) {
            return Narrow((long)self * other);
        }

        /// Returns self multiplied by other, where other is Fixnum or Bignum.
        /// </summary>
        /// <returns>
        /// Returns either Fixnum or Bignum if the result is too large for Fixnum.
        /// </returns>
        [RubyMethod("*")]
        public static BigInteger/*!*/ Multiply(int self, [NotNull]BigInteger/*!*/ other) {
            return BigInteger.Multiply(self, other);
        }

        /// <summary>
        /// Returns self multiplied by other, where other is Float.
        /// </summary>
        /// <returns>
        /// Returns a Float
        /// </returns>
        /// <remarks>
        /// Converts self to a float and multiplies the two floats directly.
        /// </remarks>
        [RubyMethod("*")]
        public static double Multiply(int self, double other) {
            return (double)self * other;
        }

        /// <summary>
        /// Returns self multiplied by other.
        /// </summary>
        /// <returns>
        /// The class of the resulting object depends on the class of other and on the magnitude of the result. 
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the * operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("*")]
        public static object/*!*/ Multiply(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, 
            object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "*", self, other);
        }

        #endregion

        #region **

        /// <summary>
        /// Raises self to the other power, which may be negative.
        /// </summary>
        /// <returns>
        /// Integer (Bignum or Fixnum) if other is positive
        /// Float otherwise.
        /// </returns>
        [RubyMethod("**")]
        public static object/*!*/ Power(int self, int other) {
            if (other >= 0) {
                BigInteger bigSelf = (BigInteger)self;
                return Protocols.Normalize(bigSelf.Power(other));
            } else if (self == 1) {
                return One;
            } else {
                return Math.Pow(self, other);
            }
        }

        /// <summary>
        /// Raises self to the other power, which may be negative or fractional.
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("**")]
        public static double Power(int self, double other) {
            return Math.Pow(self, other);
        }

        /// <summary>
        /// Raises self to the other power, where other is not Integer or Float.
        /// </summary>
        /// <remarks>
        /// Self is first coerced by other and then the ** operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("**")]
        public static object/*!*/ Power(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, 
            RubyContext/*!*/ context, int self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "**", self, other);
        }

        #endregion

        #region +

        /// <summary>
        /// Returns self added to other, where other is Fixnum.
        /// </summary>
        /// <returns>Fixnum or Bignum if result is too large for Fixnum.</returns>
        [RubyMethod("+")]
        public static object Add(int self, int other) {
            return Narrow((long)self + other);
        }

        [RubyMethod("+")]
        public static object/*!*/ Add(int self, [NotNull]BigInteger/*!*/ other) {
            return (BigInteger)self + other;
        }

        /// <summary>
        /// Returns self added to other, where other is Float
        /// </summary>
        /// <returns>Float</returns>
        /// <remarks>
        /// Converts self to Float and then adds the two floats directly.
        /// </remarks>
        [RubyMethod("+")]
        public static double Add(int self, double other) {
            return (double)self + other;
        }

        /// <summary>
        /// Returns self added to other.
        /// </summary>
        /// <returns>
        /// The class of the resulting object depends on the class of other and on the magnitude of the result. 
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the + operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("+")]
        public static object/*!*/ Add(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, 
            object/*!*/ self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "+", self, other);
        }

        #endregion

        #region -

        /// <summary>
        /// Subtracts other from self (i.e. self - other), where other is Fixnum.
        /// </summary>
        /// <returns>Fixnum, or Bignum if result is too large for Fixnum.</returns>
        [RubyMethod("-")]
        public static object/*!*/ Subtract(int self, int other) {
            return Narrow((long)self - other);
        }

        [RubyMethod("-")]
        public static object/*!*/ Subtract(int self, BigInteger other) {
            return Protocols.Normalize((BigInteger)self - other);
        }

        /// <summary>
        /// Subtracts other from self (i.e. self - other), where other is Float.
        /// </summary>
        /// <returns>Float</returns>
        /// <remarks>
        /// Converts self to a double then executes the subtraction directly.
        /// </remarks>
        [RubyMethod("-")]
        public static double Subtract(int self, double other) {
            return (double)self - other;
        }

        /// <summary>
        /// Subtracts other from self (i.e. self - other), where other is not Fixnum, or Float.
        /// </summary>
        /// <returns>
        /// The class of the resulting object depends on the class of other and on the magnitude of the result. 
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the - operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("-")]
        public static object Subtract(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, 
            RubyContext/*!*/ context, object self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "-", self, other);
        }

        #endregion

        #region -@

        [RubyMethod("-@")]
        public static object/*!*/ Minus(int self) {
            return self != Int32.MinValue ? -self : MinusMinValue();
        }

        #endregion

        #region /, div, fdiv, %, modulo, divmod

        /// <summary>
        /// Divides self by other, where other is a Fixnum.
        /// Aliased as / and div
        /// </summary>
        /// <returns>Fixnum, or Bignum if result is too large for Fixnum.</returns>
        /// <remarks>
        /// Since both operands are Integer, the result returned is Integer, rounded toward -Infinity.
        /// </remarks>
        [RubyMethod("/"), RubyMethod("div")]
        public static object/*!*/ Divide(int self, int other) {
            if (self == Int32.MinValue && other == -1) {
                return MinusMinValue();
            }
            return MathUtils.FloorDivideUnchecked(self, other);
        }

        /// <summary>
        /// Divides self by other, where other is not a Fixnum.
        /// </summary>
        /// <returns>
        /// The class of the resulting object depends on the class of other and on the magnitude of the result. 
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the / operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("/")]
        public static object DivideOp(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "/", self, other);
        }

        /// <summary>
        /// Divides self by other, where other is not a Fixnum.
        /// </summary>
        /// <returns>
        /// The class of the resulting object depends on the class of other and on the magnitude of the result. 
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the div method is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("div")]
        public static object Divide(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "div", self, other);
        }

        [RubyMethod("fdiv", Compatibility = RubyCompatibility.Ruby19)]
        public static double FDiv(int self, [DefaultProtocol]int other) {
            return self / (double)other;
        }
        /// <summary>
        /// Returns self modulo other, where other is Fixnum.  See <see cref="FloatOps.Divmod"/> for more information.
        /// </summary>
        [RubyMethod("%"), RubyMethod("modulo")]
        public static int Modulo(int self, int other) {
            return MathUtils.FloorRemainder(self, other);
        }

        /// <summary>
        /// Returns self % other, where other is not Fixnum.
        /// </summary>
        /// <remarks>
        /// First coerces self on other then calls % on the coerced self value.
        /// </remarks>
        [RubyMethod("%")]
        public static object ModuloOp(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "%", self, other);
        }

        /// <summary>
        /// Returns self modulo other, where other is not Fixnum.
        /// </summary>
        /// <remarks>
        /// First coerces self on other then calls modulo on the coerced self value.
        /// </remarks>
        [RubyMethod("modulo")]
        public static object Modulo(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, object self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "modulo", self, other);
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other.
        /// </summary>
        /// <returns>RubyArray of the form: [div, mod], where div is Integer</returns>
        /// <remarks>
        /// If q, r = x.divmod(y), then 
        ///    q = floor(float(x)/float(y))
        ///    x = q*y + r
        /// The quotient is rounded toward -infinity, as shown in the following table: 
        ///
        ///   a    |  b  |  a.divmod(b)  |   a/b   | a.modulo(b) | a.remainder(b)
        ///  ------+-----+---------------+---------+-------------+---------------
        ///   13   |  4  |   3,    1     |   3     |    1        |     1
        ///  ------+-----+---------------+---------+-------------+---------------
        ///   13   | -4  |  -4,   -3     |  -4     |   -3        |     1
        ///  ------+-----+---------------+---------+-------------+---------------
        ///  -13   |  4  |  -4,    3     |  -4     |    3        |    -1
        ///  ------+-----+---------------+---------+-------------+---------------
        ///  -13   | -4  |   3,   -1     |   3     |   -1        |    -1
        ///  ------+-----+---------------+---------+-------------+---------------
        ///   11.5 |  4  |   2,    3.5   |   2.875 |    3.5      |     3.5
        ///  ------+-----+---------------+---------+-------------+---------------
        ///   11.5 | -4  |  -3,   -0.5   |  -2.875 |   -0.5      |     3.5
        ///  ------+-----+---------------+---------+-------------+---------------
        ///  -11.5 |  4  |  -3,    0.5   |  -2.875 |    0.5      |    -3.5
        ///  ------+-----+---------------+---------+-------------+---------------
        ///  -11.5 | -4  |   2    -3.5   |   2.875 |   -3.5      |    -3.5
        /// </remarks>
        [RubyMethod("divmod")]
        public static RubyArray/*!*/ DivMod(int self, int other) {
            return RubyOps.MakeArray2(Divide(self, other), Modulo(self, other));
        }

        /// <summary>
        /// Returns an array containing the quotient and modulus obtained by dividing self by other.
        /// </summary>
        /// <returns>RubyArray of the form: [div, mod], where div is Integer</returns>
        /// <remarks>
        /// Self is first coerced by other and then the divmod method is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("divmod")]
        public static object DivMod(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, int self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "divmod", self, other);
        }

        #endregion

        #region abs

        /// <summary>
        /// Returns the absolute value of self.
        /// </summary>
        /// <returns>Fixnum</returns>
        [RubyMethod("abs")]
        public static object/*!*/ Abs(int self) {
            return self >= 0 ? self : self != Int32.MinValue ? -self : MinusMinValue();
        }

        #endregion

        #region quo

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is Fixnum. 
        /// </summary>
        /// <returns>Float</returns>
        [RubyMethod("quo")]
        public static double Quotient(int self, int other) {
            return (double)self / (double)other;
        }

        /// <summary>
        /// Returns the floating point result of dividing self by other, where other is not Fixnum. 
        /// </summary>
        /// <remarks>
        /// Self is first coerced by other and then the quo method is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("quo")]
        public static object Quotient(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ binaryOpSite, int self, object other) {
            return Protocols.CoerceAndApply(coercionStorage, binaryOpSite, "quo", self, other);
        }

        #endregion

        #region zero?

        /// <summary>
        /// Returns true if self is zero.
        /// </summary>
        /// <returns>True if self is zero, false otherwise.</returns>
        [RubyMethod("zero?")]
        public static bool IsZero(int self) {
            return self == 0;
        }

        #endregion

        #endregion

        #region Comparison Operators

        #region <

        /// <summary>
        /// Returns true if the value of self is less than other, where other is Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        [RubyMethod("<")]
        public static bool LessThan(int self, int other) {
            return self < other;
        }

        /// <summary>
        /// Returns true if the value of self is less than other, where other is not Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        /// <remarks>
        /// Self is first coerced by other and then the &lt; operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("<")]
        public static bool LessThan(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, object/*!*/ self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<", self, other);
        }

        #endregion

        #region <=

        /// <summary>
        /// Returns true if the value of self is less than or equal to other, where other is Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(int self, int other) {
            return self <= other;
        }

        /// <summary>
        /// Returns true if the value of self is less than or equal to other, where other is not Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        /// <remarks>
        /// Self is first coerced by other and then the &lt;= operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("<=")]
        public static bool LessThanOrEqual(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, object/*!*/ self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, "<=", self, other);
        }

        #endregion

        #region <=>

        /// <summary>
        /// Comparison: Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other, where other is Fixnum.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self cannot be compared to other
        /// </returns>
        [RubyMethod("<=>")]
        public static int Compare(int self, int other) {
            return self.CompareTo(other);
        }

        /// <summary>
        /// Comparison: Returns -1, 0, or +1 depending on whether self is less than, equal to, or greater than other, where other is not Fixnum.
        /// </summary>
        /// <returns>
        /// -1 if self is less than other
        /// 0 if self is equal to other
        /// +1 if self is greater than other
        /// nil if self cannot be compared to other
        /// </returns>
        /// <remarks>
        /// Self is first coerced by other and then the &lt;=&gt; operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, object/*!*/ self, object other) {
            return Protocols.CoerceAndCompare(coercionStorage, comparisonStorage, self, other);
        }

        #endregion

        #region ==

        /// <summary>
        /// Test whether self is numerically equivalent to other.  (Does not require type equivalence).
        /// </summary>
        /// <returns>True if self and other are numerically equal.</returns>
        /// <remarks>
        /// Since other is Fixnum here, we just test for direct equality.
        /// </remarks>
        [RubyMethod("==")]
        public static bool Equal(int self, int other) {
            return self == other;
        }

        /// <summary>
        /// Test whether self is numerically equivalent to other.  (Does not require type equivalence).
        /// </summary>
        /// <returns>True if self and other are numerically equal.</returns>
        /// <remarks>
        /// Since other is not Fixnum, we turn the equivalence check around,
        /// i.e. call other == self
        /// </remarks>
        [RubyMethod("==")]
        public static bool Equal(BinaryOpStorage/*!*/ equals, int self, object other) {
            // If self == other doesn't work then try other == self
            return Protocols.IsEqual(equals, other, self);
        }

        #endregion
        
        #region >

        /// <summary>
        /// Returns true if the value of self is greater than other, where other is Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        [RubyMethod(">")]
        public static bool GreaterThan(int self, int other) {
            return self > other;
        }

        /// <summary>
        /// Returns true if the value of self is greater than other, where other is not Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        /// <remarks>
        /// Self is first coerced by other and then the &gt; operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod(">")]
        public static bool GreaterThan(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage, object/*!*/ self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">", self, other);
        }

        #endregion

        #region >=

        /// <summary>
        /// Returns true if the value of self is greater than or equal to other, where other is Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(int self, int other) {
            return self >= other;
        }

        /// <summary>
        /// Returns true if the value of self is greater than or equal to other, where other is not Fixnum.
        /// </summary>
        /// <returns>True or false</returns>
        /// <remarks>
        /// Self is first coerced by other and then the &gt;= operator is invoked on the coerced self.
        /// </remarks>
        [RubyMethod(">=")]
        public static bool GreaterThanOrEqual(BinaryOpStorage/*!*/ coercionStorage, BinaryOpStorage/*!*/ comparisonStorage,
            object/*!*/ self, object other) {
            return Protocols.CoerceAndRelate(coercionStorage, comparisonStorage, ">=", self, other);
        }

        #endregion

        #endregion

        #region Conversion Methods

        #region to_f

        /// <summary>
        /// Convert self to Float.
        /// </summary>
        /// <returns>Float version of self</returns>
        [RubyMethod("to_f")]
        public static double ToFloat(int self) {
            return (double)self;
        }

        #endregion

        #region to_s

        /// <summary>
        /// Returns a string representing the value of self using base 10.
        /// </summary>
        /// <returns>MutableString</returns>
        /// <example>12345.to_s => "12345"</example>
        [RubyMethod("to_s")]
        public static object ToString(object/*!*/ self) {
            return MutableString.CreateAscii(self.ToString());
        }

        /// <summary>
        /// Returns a string representing the value of self using base radix.
        /// </summary>
        /// <returns>MutableString</returns>
        /// <example>
        /// 12345.to_s(2)    #=> "11000000111001"
        /// 12345.to_s(8)    #=> "30071"
        /// 12345.to_s(10)   #=> "12345"
        /// 12345.to_s(16)   #=> "3039"
        /// 12345.to_s(36)   #=> "9ix"
        /// </example>
        [RubyMethod("to_s")]
        public static object ToString([NotNull]BigInteger/*!*/ self, int radix) {
            if (radix < 2 || radix > 36) {
                throw RubyExceptions.CreateArgumentError("illegal radix {0}" , radix);
            }
            // TODO: Should we try to use a Fixnum specific ToString?
            // TODO: Can we do the ToLower in BigInteger?
            return MutableString.CreateAscii(self.ToString(radix).ToLowerInvariant());
        }

        #endregion

        #endregion
    }
}
 
