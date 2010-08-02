/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.BigDecimal {
    public class BigDecimal : IComparable<BigDecimal>, IEquatable<BigDecimal> {

        static BigDecimal() {
            CreateSC();
            CreateSR();
        }

        /// <summary>
        /// This class holds the configuration information for the BigDecimal calculations
        /// This information needs to be shared amongst all the instances of BigDecimal
        /// but cannot be held in static variable storage since we need to scope more finely
        /// than per assembly.
        /// </summary>
        public class Config {
            public Config() {
                Limit = 0;
                RoundingMode = RoundingModes.HalfUp;
                OverflowMode = OverflowExceptionModes.None;
            }
            public int Limit { get; set; }
            public RoundingModes RoundingMode { get; set; }
            public OverflowExceptionModes OverflowMode { get; set; }
        }

        #region Instance Fields
        private readonly NumberTypes _type;
        private readonly int _sign;
        private readonly Fraction _fraction;
        private readonly int _exponent;
        private readonly int _maxPrecision;
        #endregion

        #region Private Constants

        private const string NaNString = "NaN";
        private const string InfinityString = "Infinity";
        private const string NegativeInfinityString = "-Infinity";
        private const string ZeroString = "0.0";
        private const string NegativeZeroString = "-0.0";

        #endregion

        #region Enumerations

        public enum RoundingModes {
            /// <summary>
            /// No rounding
            /// </summary>
            None = 0,
            /// <summary>
            /// Round away from zero
            /// </summary>
            Up = 1,
            /// <summary>
            /// Round toward zero
            /// </summary>
            Down = 2,
            /// <summary>
            /// Round towards "nearest neighbour" unless both neighbours are equidistant, in which case round up
            /// </summary>
            HalfUp = 3,
            /// <summary>
            /// Round towards "nearest neighbour" unless both neighbours are equidistant, in which case round down
            /// </summary>
            HalfDown = 4,
            /// <summary>
            /// Round toward positive infinity
            /// </summary>
            Ceiling = 5,
            /// <summary>
            /// Round toward negative infinity
            /// </summary>
            Floor = 6,
            /// <summary>
            /// Round towards "nearest neighbour" unless both neighbours are equidistant, in which case round to the even neighbour
            /// </summary>
            HalfEven = 7
        }

        /// <summary>
        /// Flags of when exceptions should be thrown on special values
        /// </summary>
        /// <remarks>
        /// Currently Infinity, Overflow and ZeroDivide are the same
        /// </remarks>
        [Flags]
        public enum OverflowExceptionModes {
            None = 0,
            NaN = 2,
            Infinity = 1,
            Underflow = 4,
            Overflow = 1,
            ZeroDivide = 1,
            All = 255
        }

        public enum NumberTypes {
            NaN = 0,
            Finite = 1,
            Infinite = 2
        }

        private enum BasicOperations {
            Add = 0,
            Subtract = 1,
            Multiply = 2,
            Divide = 3
        }

        #endregion

        #region Special BigDecimal Values

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal One = new BigDecimal(1, Fraction.One, 1);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal Half = new BigDecimal(1, Fraction.Five, 0);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal NaN = new BigDecimal(NumberTypes.NaN, 0);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal PositiveInfinity = new BigDecimal(NumberTypes.Infinite, 1);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal NegativeInfinity = new BigDecimal(NumberTypes.Infinite, -1);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal PositiveZero = new BigDecimal(1, Fraction.Zero, 1);
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BigDecimal NegativeZero = new BigDecimal(-1, Fraction.Zero, 1);
        public const uint BASE = Fraction.BASE;
        public const int BASE_FIG = Fraction.BASE_FIG;
        public const int DOUBLE_FIG = 16;

        #endregion

        #region Private Constructors

        public BigDecimal() : this(0, NaN) {
        }

        private BigDecimal(int sign, Fraction/*!*/ fraction, int exponent)
            : this(sign, fraction, exponent, fraction.Precision) {
        }

        private BigDecimal(int sign, Fraction/*!*/ fraction, int exponent, int maxPrecision) {
            Debug.Assert(sign == 1 || sign == -1);
            _type = NumberTypes.Finite;
            _sign = sign;
            _fraction = fraction;
            _exponent = exponent;
            // If maxPrecision is 0 then default to the precision of the fraction
            _maxPrecision = maxPrecision == 0 ? fraction.Precision : maxPrecision;
        }

        private BigDecimal(int sign, BigDecimal/*!*/ copyFrom) {
            _type = copyFrom._type;
            _sign = sign;
            _fraction = copyFrom._fraction;
            _exponent = copyFrom._exponent;
            _maxPrecision = copyFrom._maxPrecision;
        }

        private BigDecimal(int sign, BigDecimal/*!*/ copyFrom, int maxPrecision) {
            _type = copyFrom._type;
            _sign = sign;
            _fraction = new Fraction(copyFrom._fraction, maxPrecision);
            _exponent = copyFrom._exponent;
            _maxPrecision = maxPrecision;
        }

        private BigDecimal(NumberTypes type, int sign)
            : this(type, sign, 1) {
        }

        private BigDecimal(NumberTypes type, int sign, int maxPrecision) {
            Debug.Assert(type != NumberTypes.Finite);
            _type = type;
            _sign = sign;
            _fraction = Fraction.Zero;
            _exponent = 0;
            _maxPrecision = maxPrecision;
        }

        #endregion

        #region Public Properties
        public NumberTypes NumberType { get { return _type; } }
        public int Sign { get { return _sign; } }
        public int Exponent { get { return _exponent; } }
        public int Precision { get { return _fraction.Precision; } }
        public int MaxPrecision { get { return _maxPrecision; } }
        public int Digits { get { return _fraction.DigitCount; } }
        public int MaxPrecisionDigits { get { return _maxPrecision * BASE_FIG; } }
        public int PrecisionDigits { get { return _fraction.Precision * BASE_FIG; } }
        #endregion

        #region Create Methods

        public static BigDecimal Create(Config/*!*/config, object value) {
            Debug.Assert(!(value is double) && !(value is float)); // Need to use CultureInfo.InvariantCulture for double
            return Create(config, value.ToString(), 0);
        }

        public static BigDecimal Create(Config/*!*/config, double value) {
            return Create(config, value.ToString(CultureInfo.InvariantCulture), 0);
        }

        public static BigDecimal Create(Config/*!*/config, string value) {
            return Create(config, value, 0);
        }

        public static BigDecimal Create(Config/*!*/config, string value, int maxPrecision) {
            value = value.Trim();
            BigDecimal result = CheckSpecialCases(value);

            if (result == null) {
                Match m = Regex.Match(value, @"^(?<sign>[-+]?)(?<integer>[\d_]*)\.?(?<fraction>[\d_]*)([eEdD](?<exponent>[-+]?[\d_]+))?", RegexOptions.ExplicitCapture);

                int sign = m.Groups["sign"].Value == "-" ? -1 : 1;

                // Get the integer part of the mantissa (stripping off leading zeros and any '_' chars)
                string integerStr = (m.Groups["integer"].Value ?? "0").TrimStart('0').Replace("_", "");
                // Get the fraction part of the mantissa (stripping off following zeros and any '_' chars)
                string decimalStr = (m.Groups["fraction"].Value ?? "").TrimEnd('0').Replace("_", "");
                // Build the whole of the mantissa string
                string fractionStr = (integerStr + decimalStr);
                string strippedFractionStr = fractionStr.TrimStart('0');

                // Finally strip off any excess zeros left over
                string trimmedFractionStr = strippedFractionStr.Trim('0');

                Fraction fraction = Fraction.Create(trimmedFractionStr);

                string exponentStr = m.Groups["exponent"].Value.Replace("_", "");
                int exponent = 0;
                if (!fraction.IsZero) {
                    if (!string.IsNullOrEmpty(exponentStr)) {
                        try {
                            exponent = int.Parse(exponentStr, CultureInfo.InvariantCulture);
                        } catch (OverflowException) {
                            exponent = exponentStr.StartsWith("-", StringComparison.Ordinal) ? -1 : 1;
                            return ExponentOverflow(config, sign, exponent);
                        }
                    }
                    // Normalize exponent where mantissa > 1.0
                    exponent += integerStr.Length;
                    // Normalize exponent where mantissa < 1.0
                    exponent -= (fractionStr.Length - strippedFractionStr.Length);
                }
                result = new BigDecimal(sign, fraction, exponent, maxPrecision);
            }
            return CheckOverflowExceptions(config, result);
        }

        private static BigDecimal CheckSpecialCases(string value) {
            BigDecimal result = null;
            if (value == null) {
                result = PositiveZero;
            } else {
                value = value.Trim();
                if (value == "NaN") {
                    result = NaN;
                } else if (value.Contains("Infinity")) {
                    if (value == "+Infinity" || value == "Infinity") {
                        result = PositiveInfinity;
                    } else if (value == "-Infinity") {
                        result = NegativeInfinity;
                    } else {
                        result = PositiveZero;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Special Value Tests

        public static bool IsNaN(BigDecimal x) {
            return x._type == NumberTypes.NaN;
        }

        public static bool IsInfinite(BigDecimal x) {
            return (x._type == NumberTypes.Infinite);
        }

        public static bool IsFinite(BigDecimal x) {
            return x._type == NumberTypes.Finite;
        }

        public static bool IsZero(BigDecimal/*!*/ x) {
            return IsFinite(x) && x._fraction.IsZero;
        }

        public static bool IsNonZeroFinite(BigDecimal x) {
            return IsFinite(x) && !x._fraction.IsZero;
        }

        public static bool IsPositive(BigDecimal x) {
            return x._sign == 1;
        }

        public static bool IsNegative(BigDecimal x) {
            return x._sign == -1;
        }

        public static bool IsPositiveInfinite(BigDecimal x) {
            return IsInfinite(x) && IsPositive(x);
        }

        public static bool IsNegativeInfinite(BigDecimal x) {
            return IsInfinite(x) && IsNegative(x);
        }

        public static bool IsPositiveZero(BigDecimal x) {
            return IsZero(x) && IsPositive(x);
        }

        public static bool IsNegativeZero(BigDecimal x) {
            return IsZero(x) && IsNegative(x);
        }

        public static bool IsOne(BigDecimal x) {
            return x._fraction.IsOne && x._exponent == 1;
        }

        public static bool IsPositiveOne(BigDecimal x) {
            return IsOne(x) && IsPositive(x);
        }

        public static bool IsNegativeOne(BigDecimal x) {
            return IsOne(x) && IsPositive(x);
        }

        #endregion

        #region Public Static Operations

        public static double ToFloat(Config config, BigDecimal/*!*/ x) {
            try {
                if (BigDecimal.IsNegativeZero(x)) {
                    return -0.0;
                }
                return Double.Parse(x.ToString(0, "", true), CultureInfo.InvariantCulture);
            } catch (OverflowException) {
                return Double.PositiveInfinity;
            }
        }

        public static object ToInteger(Config/*!*/ config, BigDecimal/*!*/ x) {
            if (IsFinite(x)) {
                BigDecimal i = IntegerPart(config, x);
                BigInteger d = BigInteger.Create(0);
                string digits = i._fraction.ToString();
                foreach (char c in digits) {
                    d *= 10;
                    d += (c - '0');
                }
                if (IsNegative(x)) {
                    d = BigInteger.Negate(d);
                }
                return ClrBigInteger.Multiply(d, BigInteger.Create(10).Power(i.Exponent - digits.Length));
            } else {
                return null;
            }
        }

        public static BigDecimal Abs(Config config, BigDecimal x) {
            if (IsNegative(x)) {
                return Negate(config, x);
            } else {
                return x;
            }
        }

        public static BigDecimal Negate(Config config, BigDecimal x) {
            if (IsFinite(x)) {
                return new BigDecimal(-x._sign, x._fraction, x._exponent);
            } else {
                return new BigDecimal(x.NumberType, -x.Sign);
            }
        }

        public static BigDecimal/*!*/ Add(Config/*!*/ config, BigDecimal/*!*/ x, BigDecimal/*!*/ y) {
            return Add(config, x, y, 0);
        }

        public static BigDecimal/*!*/ Add(Config/*!*/ config, BigDecimal/*!*/ x, BigDecimal/*!*/ y, int limit) {
            return InternalAdd(config, x, y, limit);
        }

        public static BigDecimal/*!*/ Subtract(Config/*!*/ config, BigDecimal/*!*/ x, BigDecimal/*!*/ y) {
            return Subtract(config, x, y, 0);
        }

        public static BigDecimal/*!*/ Subtract(Config config, BigDecimal x, BigDecimal y, int limit) {
            return InternalAdd(config, x, BigDecimal.Negate(config, y), limit);
        }

        public static BigDecimal Multiply(Config config, BigDecimal x, BigDecimal y) {
            return Multiply(config, x, y, 0);
        }

        public static BigDecimal Multiply(Config config, BigDecimal x, BigDecimal y, int limit) {
            BigDecimal result = CheckSpecialResult(config, x, y, BasicOperations.Multiply);
            if (result != null) {
                return result;
            }

            // Default to the global limit
            if (limit == 0) {
                limit = config.Limit;
            }

            // Calculate the number of digits if not specified
            if (limit == 0) {
                limit = x.Digits + y.Digits;
            }

            int sign = y._sign * x._sign;
            if (IsOne(x)) {
                return LimitPrecision(config, new BigDecimal(sign, y._fraction, y._exponent), limit, config.RoundingMode);
            }
            if (IsOne(y)) {
                return LimitPrecision(config, new BigDecimal(sign, x._fraction, x._exponent), limit, config.RoundingMode);
            }

            int exponent;
            Fraction fraction;
            try {
                checked {
                    int diffDigits1, diffDigits2;
                    fraction = Fraction.Multiply(x._fraction, y._fraction, out diffDigits1);
                    fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out diffDigits2);
                    exponent = x._exponent + y._exponent + diffDigits1 + diffDigits2;
                }
            } catch (OverflowException) {
                return ExponentOverflow(config, sign, x._exponent);
            }

            result = new BigDecimal(sign, fraction, exponent, limit);
            return result;
        }

        public static BigDecimal Divide(Config config, BigDecimal x, BigDecimal y, int limit, out BigDecimal remainder) {
            BigDecimal result = CheckSpecialResult(config, x, y, BasicOperations.Divide);
            if (result != null) {
                remainder = BigDecimal.PositiveZero;
                return result;
            }

            // Default to global limit if not specified
            if (limit == 0) {
                limit = config.Limit;
            }

            // Sign
            int sign = x._sign * y._sign;

            if (IsOne(y)) {
                remainder = BigDecimal.PositiveZero;
                return new BigDecimal(sign, x._fraction, x._exponent);
            }

            // Fraction
            int xOffset, rOffset, limitOffset;
            int minPrecision = (int)Math.Ceiling((double)limit / BASE_FIG) + 1;
            Fraction rFraction;
            Fraction fraction = Fraction.Divide(x._fraction, y._fraction, minPrecision, out rFraction, out xOffset, out rOffset);
            if (limit == 0) {
                limitOffset = 0;
            } else {
                fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out limitOffset);
            }

            // Exponent
            int exponent, rExponent;
            try {
                checked {
                    exponent = x._exponent - y._exponent + xOffset + limitOffset;
                    rExponent = x._exponent - y._exponent + rOffset;
                }
            } catch (OverflowException) {
                remainder = BigDecimal.PositiveZero;
                return ExponentOverflow(config, sign, x._exponent);
            }

            remainder = new BigDecimal(sign, rFraction, rExponent, rFraction.MaxPrecision);
            result = new BigDecimal(sign, fraction, exponent, fraction.MaxPrecision);
            return result;
        }

        public static void DivMod(Config/*!*/ config, BigDecimal/*!*/ x, BigDecimal/*!*/ y, out BigDecimal/*!*/ div, out BigDecimal/*!*/ mod) {
            // TODO: This is horribly suboptimal!
            div = BigDecimal.NaN;
            mod = BigDecimal.NaN;
            if (BigDecimal.IsFinite(x) && BigDecimal.IsFinite(y)) {
                BigDecimal remainder;
                BigDecimal quotient = Divide(config, x, y, 0, out remainder);
                if (BigDecimal.IsFinite(quotient)) {
                    // Calculate the div: floor(quotient)
                    div = LimitPrecision(config, quotient, 0, BigDecimal.RoundingModes.Floor);
                    // Calculate the mod: x - (div * y)
                    mod = Subtract(config, x, Multiply(config, div, y));
                }
            }
        }

        public static BigDecimal/*!*/ Power(Config/*!*/ config, BigDecimal/*!*/ x, int power) {
            if (!IsFinite(x)) {
                return CheckOverflowExceptions(config, NaN);
            }

            if (power == 0) {
                return One;
            }

            // power is odd  => sign = x.Sign
            // power is even => sign = 1
            int sign = (power % 2 != 0) ? x.Sign : 1;

            if (IsOne(x)) {
                return new BigDecimal(sign, One);
            }

            if (IsZero(x)) {
                if (power < 0) {
                    return CheckOverflowExceptions(config, new BigDecimal(sign, PositiveInfinity));
                } else {
                    return new BigDecimal(sign, PositiveZero);
                }
            }

            BigDecimal result = x;
            int n = Math.Abs(power) - 1;
            int precision = x.Precision * (n + 2);
            while (n > 0) {
                // Copy x into temp ensuring a good precision
                BigDecimal temp = new BigDecimal(x.Sign, x, precision);
                int s = 2;
                int ss = 1;
                while (s <= n) {
                    temp = Multiply(config, temp, temp);
                    ss = s;
                    s += s;
                }
                n -= ss;
                result = Multiply(config, result, temp);
            }
            if (power < 0) {
                BigDecimal remainder;
                result = Divide(config, One, result, precision * (BASE_FIG + 1), out remainder);
            }
            return result;
        }

        public static BigDecimal/*!*/ SquareRoot(Config/*!*/ config, BigDecimal/*!*/ x, int limit) {
            if (limit < 0) {
                throw new ArgumentException("argument must be positive");
            }
            // Special cases
            if (IsZero(x) || IsPositiveOne(x)) {
                return x;
            }
            if (x.Sign < 0) {
                throw new FloatDomainError("SQRT(negative value)");
                // Really it seems more consistent to check the overflow exceptions
                // return CheckOverflowExceptions(config, NaN);
            }
            if (IsNaN(x)) {
                throw new FloatDomainError("SQRT(NaN)");
            }
            if (!IsFinite(x)) {
                return CheckOverflowExceptions(config, x);
            }

            // Precision
            if (limit == 0) {
                limit = config.Limit;
            }
            int prec = x.Exponent;
            if (prec > 0) {
                ++prec;
            } else {
                --prec;
            }
            prec = prec - Math.Max(x.PrecisionDigits, limit + DOUBLE_FIG);

            // Estimate the value using double (this round-about computation is to get round double's exponent limitation.
            BigDecimal fraction = new BigDecimal(x._sign, x._fraction, 0, x._maxPrecision);
            int exponent = x._exponent / 2;
            if (x._exponent - exponent * 2 != 0) {
                // The exponent is odd so we have to tweak the values
                exponent = (x._exponent + 1) / 2;
                fraction = BigDecimal.Multiply(config, fraction, BigDecimal.Create(config, 0.1));
            }
            double d = ToFloat(config, fraction);
            BigDecimal y = BigDecimal.Create(config, Math.Sqrt(d));
            y = new BigDecimal(y._sign, y._fraction, y._exponent + exponent);

            // Converge on more accurate result
            const int max_iterations = 100;
            BigDecimal r = PositiveZero;
            BigDecimal f = PositiveZero;
            int iterations = Math.Max(limit, max_iterations);
            int i = 0;
            while (i < iterations) {
                f = Divide(config, x, y, limit, out r); /* f = x/y    */
                r = Subtract(config, f, y, limit); /* r = f - y  */
                f = Multiply(config, Half, r, limit); /* f = 0.5*r  */
                if (IsZero(f)) {
                    break;
                }
                y = Add(config, y, f); /* y = y + f  */
                if (f.Exponent <= prec) {
                    break;
                }
                i++;
            }
            return y;
            //            return LimitPrecision(config, y, limit, config.RoundingMode);
        }

        public static BigDecimal/*!*/ FractionalPart(Config/*!*/ config, BigDecimal/*!*/ x) {
            if (IsFinite(x)) {
                if (x.Exponent > 0) {
                    if (x.Exponent < x.Digits) {
                        return new BigDecimal(x.Sign, Fraction.Create(x._fraction.ToString().Substring(x.Exponent)), 0);
                    } else {
                        return x.Sign > 0 ? BigDecimal.PositiveZero : BigDecimal.NegativeZero;
                    }
                } else {
                    return x;
                }
            }
            return x;
        }

        public static BigDecimal/*!*/ IntegerPart(Config/*!*/ config, BigDecimal/*!*/ x) {
            if (IsFinite(x)) {
                if (x.Exponent > 0) {
                    if (x.Exponent < x._fraction.DigitCount) {
                        return new BigDecimal(x.Sign, Fraction.Create(x._fraction.ToString().Substring(0, x.Exponent)), x.Exponent);
                    } else {
                        return x;
                    }
                } else {
                    return x.Sign > 0 ? BigDecimal.PositiveZero : BigDecimal.NegativeZero;
                }
            } else {
                return x;
            }
        }

        public static BigDecimal/*!*/ LimitPrecision(Config/*!*/ config, BigDecimal/*!*/ x, int limit, RoundingModes mode) {
            try {
                checked {
                    if (IsFinite(x)) {
                        int diffDigits;
                        Fraction fraction = Fraction.LimitPrecision(x._sign, x._fraction, limit + x.Exponent, mode, out diffDigits);
                        return new BigDecimal(x._sign, fraction, x._exponent + diffDigits);
                    } else {
                        return x;
                    }
                }
            } catch (OverflowException) {
                return ExponentOverflow(config, x._sign, x._exponent);
            }
        }

        #endregion

        #region Private Helpers

        private static BigDecimal InternalAdd(Config config, BigDecimal x, BigDecimal y, int limit) {
            if (limit < 0) {
                throw new ArgumentException("limit must be positive");
            }
            if (limit == 0) {
                limit = config.Limit;
            }

            // This method is used for both adding and subtracting, but the special cases are the same.
            BigDecimal result = CheckSpecialResult(config, x, y, BasicOperations.Add);
            if (result != null) {
                return result;
            }

            if (IsZero(x)) {
                if (limit == 0) {
                    return y;
                } else {
                    return LimitPrecision(config, y, limit, config.RoundingMode);
                }
            }
            if (IsZero(y)) {
                if (limit == 0) {
                    return x;
                } else {
                    return LimitPrecision(config, x, limit, config.RoundingMode);
                }
            }

            int exponent = Math.Max(x._exponent, y._exponent);
            int sign = x._sign;
            Fraction fraction;
            int exponentDiff;

            try {
                checked {
                    exponentDiff = checked(x._exponent - y._exponent);
                    int diffDigits;
                    if (x._sign == y._sign) {
                        fraction = Fraction.Add(x._fraction, y._fraction, exponentDiff, out diffDigits);
                        exponent = exponent + diffDigits;
                        sign = x._sign;
                    } else {
                        // Signs are different then we have to do a subtraction instead
                        fraction = Fraction.Subtract(x._fraction, y._fraction, exponentDiff, out diffDigits, out sign);
                        exponent = exponent + diffDigits;
                        if (sign == 0) {
                            return PositiveZero;
                        }
                        sign = x._sign * sign;
                    }
                    if (limit == 0) {
                        limit = fraction.DigitCount;
                    }
                    fraction = Fraction.LimitPrecision(sign, fraction, limit, config.RoundingMode, out diffDigits);
                    exponent = exponent + diffDigits;
                }
            } catch (OverflowException) {
                return ExponentOverflow(config, sign, x._exponent);
            }

            result = new BigDecimal(sign, fraction, exponent);
            return result;
        }

        private static BigDecimal ExponentOverflow(Config config, int sign, int exponent) {
            if (exponent > 0) {
                if ((config.OverflowMode & OverflowExceptionModes.Overflow) == OverflowExceptionModes.Overflow) {
                    throw new FloatDomainError("Exponent overflow");
                }
            } else {
                if ((config.OverflowMode & OverflowExceptionModes.Underflow) == OverflowExceptionModes.Underflow) {
                    throw new FloatDomainError("Exponent underflow");
                }
            }
            // Always returns Zero on exponent overflow.
            // It would seem more appropriate to return +/-Infinity or Zero depending on exponent and sign.
            return PositiveZero;
        }

        #endregion

        #region Special Comparisons

        //TODO: There is some redundancy in this table that could be removed.
        private static int? CheckSpecialComparison(BigDecimal x, BigDecimal y) {
            // Non-zero comparisons are not allowed (since these are not special cases)
            Debug.Assert(!IsNonZeroFinite(x) || !IsNonZeroFinite(y));

            // Encode the array indices
            int xSign = x._sign == -1 ? 0 : 1;
            int ySign = y._sign == -1 ? 0 : 1;
            int xZero = IsZero(x) ? 0 : 1;
            int yZero = IsZero(y) ? 0 : 1;
            int xType = (int)x._type;
            int yType = (int)y._type;
            return SC[xSign, xZero, xType, ySign, yZero, yType];
        }
        /// <summary>
        /// Generated by the following ruby code:
        //require 'bigdecimal'

        //signs = [:-, :+]
        //zeros = [true, false]
        //types = [:NaN, :Finite, :Infinite]
        //ops = [:+, :-, :*, :/]

        //def CreateBigDecimal(sign, zero, type, finiteValue) 
        //  if zero
        //    BigDecimal.new("#{sign}0.0")
        //  else
        //    if type == :NaN
        //      BigDecimal.new("NaN")
        //    elsif type == :Infinite
        //      BigDecimal.new("#{sign}Infinity")
        //    else
        //      BigDecimal.new("#{sign}#{finiteValue}")
        //    end
        //  end
        //end

        //def map(r, null)
        //   if r.sign == BigDecimal::SIGN_NaN
        //     result = 'NaN'
        //   elsif r.sign == BigDecimal::SIGN_POSITIVE_INFINITE
        //     result = 'PositiveInfinity'
        //   elsif r.sign == BigDecimal::SIGN_NEGATIVE_INFINITE
        //     result = 'NegativeInfinity'
        //   elsif r.sign == BigDecimal::SIGN_POSITIVE_ZERO
        //     result = 'PositiveZero'
        //   elsif r.sign == BigDecimal::SIGN_NEGATIVE_ZERO
        //     result = 'NegativeZero'
        //   elsif null
        //     result = 'null'
        //   else
        //     result = 'Finite'
        //   end
        //end
        //signs.each do |xSign|
        //    zeros.each do |xZero|
        //        types.each do |xType|
        //            signs.each do |ySign|
        //                zeros.each do |yZero|
        //                    types.each do |yType|
        //                        x = CreateBigDecimal(xSign, xZero, xType, 7)
        //                        y = CreateBigDecimal(ySign, yZero, yType, 11)
        //                        cmp = (x <=> y) || "null"
        //                        puts "// #{map(x, false)} <=> #{map(y, false)} = #{cmp}"
        //                        puts "SC[#{signs.index(xSign)}, #{zeros.index(xZero)}, #{types.index(xType)}, #{signs.index(ySign)}, #{zeros.index(yZero)}, #{types.index(yType)}] = #{cmp};"
        //                    end
        //                end
        //            end
        //        end
        //    end
        //end
        /// </summary>
        private static readonly int?[, , , , ,] SC = new int?[2, 2, 3, 2, 2, 3];
        private static void CreateSC() {
            // xSign, xZero, xType, ySign, yZero, yType
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 0, 0, 0, 0] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 0, 0, 0, 1] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 0, 0, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 0, 0, 1, 0] = null;
            // NegativeZero <=> Finite = 1
            SC[0, 0, 0, 0, 1, 1] = 1;
            // NegativeZero <=> NegativeInfinity = 1
            SC[0, 0, 0, 0, 1, 2] = 1;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 0, 1, 0, 0] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 0, 1, 0, 1] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 0, 1, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 0, 1, 1, 0] = null;
            // NegativeZero <=> Finite = -1
            SC[0, 0, 0, 1, 1, 1] = -1;
            // NegativeZero <=> PositiveInfinity = -1
            SC[0, 0, 0, 1, 1, 2] = -1;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 1, 0, 0, 0] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 1, 0, 0, 1] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 1, 0, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 1, 0, 1, 0] = null;
            // NegativeZero <=> Finite = 1
            SC[0, 0, 1, 0, 1, 1] = 1;
            // NegativeZero <=> NegativeInfinity = 1
            SC[0, 0, 1, 0, 1, 2] = 1;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 1, 1, 0, 0] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 1, 1, 0, 1] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 1, 1, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 1, 1, 1, 0] = null;
            // NegativeZero <=> Finite = -1
            SC[0, 0, 1, 1, 1, 1] = -1;
            // NegativeZero <=> PositiveInfinity = -1
            SC[0, 0, 1, 1, 1, 2] = -1;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 2, 0, 0, 0] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 2, 0, 0, 1] = 0;
            // NegativeZero <=> NegativeZero = 0
            SC[0, 0, 2, 0, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 2, 0, 1, 0] = null;
            // NegativeZero <=> Finite = 1
            SC[0, 0, 2, 0, 1, 1] = 1;
            // NegativeZero <=> NegativeInfinity = 1
            SC[0, 0, 2, 0, 1, 2] = 1;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 2, 1, 0, 0] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 2, 1, 0, 1] = 0;
            // NegativeZero <=> PositiveZero = 0
            SC[0, 0, 2, 1, 0, 2] = 0;
            // NegativeZero <=> NaN = null
            SC[0, 0, 2, 1, 1, 0] = null;
            // NegativeZero <=> Finite = -1
            SC[0, 0, 2, 1, 1, 1] = -1;
            // NegativeZero <=> PositiveInfinity = -1
            SC[0, 0, 2, 1, 1, 2] = -1;
            // NaN <=> NegativeZero = null
            SC[0, 1, 0, 0, 0, 0] = null;
            // NaN <=> NegativeZero = null
            SC[0, 1, 0, 0, 0, 1] = null;
            // NaN <=> NegativeZero = null
            SC[0, 1, 0, 0, 0, 2] = null;
            // NaN <=> NaN = null
            SC[0, 1, 0, 0, 1, 0] = null;
            // NaN <=> Finite = null
            SC[0, 1, 0, 0, 1, 1] = null;
            // NaN <=> NegativeInfinity = null
            SC[0, 1, 0, 0, 1, 2] = null;
            // NaN <=> PositiveZero = null
            SC[0, 1, 0, 1, 0, 0] = null;
            // NaN <=> PositiveZero = null
            SC[0, 1, 0, 1, 0, 1] = null;
            // NaN <=> PositiveZero = null
            SC[0, 1, 0, 1, 0, 2] = null;
            // NaN <=> NaN = null
            SC[0, 1, 0, 1, 1, 0] = null;
            // NaN <=> Finite = null
            SC[0, 1, 0, 1, 1, 1] = null;
            // NaN <=> PositiveInfinity = null
            SC[0, 1, 0, 1, 1, 2] = null;
            // Finite <=> NegativeZero = -1
            SC[0, 1, 1, 0, 0, 0] = -1;
            // Finite <=> NegativeZero = -1
            SC[0, 1, 1, 0, 0, 1] = -1;
            // Finite <=> NegativeZero = -1
            SC[0, 1, 1, 0, 0, 2] = -1;
            // Finite <=> NaN = null
            SC[0, 1, 1, 0, 1, 0] = null;
            // Finite <=> Finite = 1
            SC[0, 1, 1, 0, 1, 1] = 1;
            // Finite <=> NegativeInfinity = 1
            SC[0, 1, 1, 0, 1, 2] = 1;
            // Finite <=> PositiveZero = -1
            SC[0, 1, 1, 1, 0, 0] = -1;
            // Finite <=> PositiveZero = -1
            SC[0, 1, 1, 1, 0, 1] = -1;
            // Finite <=> PositiveZero = -1
            SC[0, 1, 1, 1, 0, 2] = -1;
            // Finite <=> NaN = null
            SC[0, 1, 1, 1, 1, 0] = null;
            // Finite <=> Finite = -1
            SC[0, 1, 1, 1, 1, 1] = -1;
            // Finite <=> PositiveInfinity = -1
            SC[0, 1, 1, 1, 1, 2] = -1;
            // NegativeInfinity <=> NegativeZero = -1
            SC[0, 1, 2, 0, 0, 0] = -1;
            // NegativeInfinity <=> NegativeZero = -1
            SC[0, 1, 2, 0, 0, 1] = -1;
            // NegativeInfinity <=> NegativeZero = -1
            SC[0, 1, 2, 0, 0, 2] = -1;
            // NegativeInfinity <=> NaN = null
            SC[0, 1, 2, 0, 1, 0] = null;
            // NegativeInfinity <=> Finite = -1
            SC[0, 1, 2, 0, 1, 1] = -1;
            // NegativeInfinity <=> NegativeInfinity = 0
            SC[0, 1, 2, 0, 1, 2] = 0;
            // NegativeInfinity <=> PositiveZero = -1
            SC[0, 1, 2, 1, 0, 0] = -1;
            // NegativeInfinity <=> PositiveZero = -1
            SC[0, 1, 2, 1, 0, 1] = -1;
            // NegativeInfinity <=> PositiveZero = -1
            SC[0, 1, 2, 1, 0, 2] = -1;
            // NegativeInfinity <=> NaN = null
            SC[0, 1, 2, 1, 1, 0] = null;
            // NegativeInfinity <=> Finite = -1
            SC[0, 1, 2, 1, 1, 1] = -1;
            // NegativeInfinity <=> PositiveInfinity = -1
            SC[0, 1, 2, 1, 1, 2] = -1;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 0, 0, 0, 0] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 0, 0, 0, 1] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 0, 0, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 0, 0, 1, 0] = null;
            // PositiveZero <=> Finite = 1
            SC[1, 0, 0, 0, 1, 1] = 1;
            // PositiveZero <=> NegativeInfinity = 1
            SC[1, 0, 0, 0, 1, 2] = 1;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 0, 1, 0, 0] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 0, 1, 0, 1] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 0, 1, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 0, 1, 1, 0] = null;
            // PositiveZero <=> Finite = -1
            SC[1, 0, 0, 1, 1, 1] = -1;
            // PositiveZero <=> PositiveInfinity = -1
            SC[1, 0, 0, 1, 1, 2] = -1;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 1, 0, 0, 0] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 1, 0, 0, 1] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 1, 0, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 1, 0, 1, 0] = null;
            // PositiveZero <=> Finite = 1
            SC[1, 0, 1, 0, 1, 1] = 1;
            // PositiveZero <=> NegativeInfinity = 1
            SC[1, 0, 1, 0, 1, 2] = 1;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 1, 1, 0, 0] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 1, 1, 0, 1] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 1, 1, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 1, 1, 1, 0] = null;
            // PositiveZero <=> Finite = -1
            SC[1, 0, 1, 1, 1, 1] = -1;
            // PositiveZero <=> PositiveInfinity = -1
            SC[1, 0, 1, 1, 1, 2] = -1;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 2, 0, 0, 0] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 2, 0, 0, 1] = 0;
            // PositiveZero <=> NegativeZero = 0
            SC[1, 0, 2, 0, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 2, 0, 1, 0] = null;
            // PositiveZero <=> Finite = 1
            SC[1, 0, 2, 0, 1, 1] = 1;
            // PositiveZero <=> NegativeInfinity = 1
            SC[1, 0, 2, 0, 1, 2] = 1;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 2, 1, 0, 0] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 2, 1, 0, 1] = 0;
            // PositiveZero <=> PositiveZero = 0
            SC[1, 0, 2, 1, 0, 2] = 0;
            // PositiveZero <=> NaN = null
            SC[1, 0, 2, 1, 1, 0] = null;
            // PositiveZero <=> Finite = -1
            SC[1, 0, 2, 1, 1, 1] = -1;
            // PositiveZero <=> PositiveInfinity = -1
            SC[1, 0, 2, 1, 1, 2] = -1;
            // NaN <=> NegativeZero = null
            SC[1, 1, 0, 0, 0, 0] = null;
            // NaN <=> NegativeZero = null
            SC[1, 1, 0, 0, 0, 1] = null;
            // NaN <=> NegativeZero = null
            SC[1, 1, 0, 0, 0, 2] = null;
            // NaN <=> NaN = null
            SC[1, 1, 0, 0, 1, 0] = null;
            // NaN <=> Finite = null
            SC[1, 1, 0, 0, 1, 1] = null;
            // NaN <=> NegativeInfinity = null
            SC[1, 1, 0, 0, 1, 2] = null;
            // NaN <=> PositiveZero = null
            SC[1, 1, 0, 1, 0, 0] = null;
            // NaN <=> PositiveZero = null
            SC[1, 1, 0, 1, 0, 1] = null;
            // NaN <=> PositiveZero = null
            SC[1, 1, 0, 1, 0, 2] = null;
            // NaN <=> NaN = null
            SC[1, 1, 0, 1, 1, 0] = null;
            // NaN <=> Finite = null
            SC[1, 1, 0, 1, 1, 1] = null;
            // NaN <=> PositiveInfinity = null
            SC[1, 1, 0, 1, 1, 2] = null;
            // Finite <=> NegativeZero = 1
            SC[1, 1, 1, 0, 0, 0] = 1;
            // Finite <=> NegativeZero = 1
            SC[1, 1, 1, 0, 0, 1] = 1;
            // Finite <=> NegativeZero = 1
            SC[1, 1, 1, 0, 0, 2] = 1;
            // Finite <=> NaN = null
            SC[1, 1, 1, 0, 1, 0] = null;
            // Finite <=> Finite = 1
            SC[1, 1, 1, 0, 1, 1] = 1;
            // Finite <=> NegativeInfinity = 1
            SC[1, 1, 1, 0, 1, 2] = 1;
            // Finite <=> PositiveZero = 1
            SC[1, 1, 1, 1, 0, 0] = 1;
            // Finite <=> PositiveZero = 1
            SC[1, 1, 1, 1, 0, 1] = 1;
            // Finite <=> PositiveZero = 1
            SC[1, 1, 1, 1, 0, 2] = 1;
            // Finite <=> NaN = null
            SC[1, 1, 1, 1, 1, 0] = null;
            // Finite <=> Finite = -1
            SC[1, 1, 1, 1, 1, 1] = -1;
            // Finite <=> PositiveInfinity = -1
            SC[1, 1, 1, 1, 1, 2] = -1;
            // PositiveInfinity <=> NegativeZero = 1
            SC[1, 1, 2, 0, 0, 0] = 1;
            // PositiveInfinity <=> NegativeZero = 1
            SC[1, 1, 2, 0, 0, 1] = 1;
            // PositiveInfinity <=> NegativeZero = 1
            SC[1, 1, 2, 0, 0, 2] = 1;
            // PositiveInfinity <=> NaN = null
            SC[1, 1, 2, 0, 1, 0] = null;
            // PositiveInfinity <=> Finite = 1
            SC[1, 1, 2, 0, 1, 1] = 1;
            // PositiveInfinity <=> NegativeInfinity = 1
            SC[1, 1, 2, 0, 1, 2] = 1;
            // PositiveInfinity <=> PositiveZero = 1
            SC[1, 1, 2, 1, 0, 0] = 1;
            // PositiveInfinity <=> PositiveZero = 1
            SC[1, 1, 2, 1, 0, 1] = 1;
            // PositiveInfinity <=> PositiveZero = 1
            SC[1, 1, 2, 1, 0, 2] = 1;
            // PositiveInfinity <=> NaN = null
            SC[1, 1, 2, 1, 1, 0] = null;
            // PositiveInfinity <=> Finite = 1
            SC[1, 1, 2, 1, 1, 1] = 1;
            // PositiveInfinity <=> PositiveInfinity = 0
            SC[1, 1, 2, 1, 1, 2] = 0;
        }
        #endregion

        #region Special Results
        //TODO: Probably don't need the subtract operation since it is symetric with addition operation in terms of special results
        /// <summary>
        /// Look up special results for calculations that involve unusual values such as NaN, infinity and zero.
        /// </summary>
        /// <param name="x">The left hand side of the operation</param>
        /// <param name="y">The right hand side of the operation</param>
        /// <param name="op">The operation itself</param>
        /// <returns>The special result or null if the result is not special</returns>
        private static BigDecimal CheckSpecialResult(Config config, BigDecimal x, BigDecimal y, BasicOperations op) {
            // Encode the array indices
            int xSign = x._sign == -1 ? 0 : 1;
            int ySign = y._sign == -1 ? 0 : 1;
            int xZero = IsZero(x) ? 0 : 1;
            int yZero = IsZero(y) ? 0 : 1;
            int xType = (int)x._type;
            int yType = (int)y._type;
            int opValue = (int)op;

            // Look up the special result
            BigDecimal result = SR[xSign, xZero, xType, opValue, ySign, yZero, yType];
            if (result != null) {
                return CheckOverflowExceptions(config, result);
            }
            return null;
        }

        private static BigDecimal/*!*/ CheckOverflowExceptions(Config/*!*/ config, BigDecimal/*!*/ result) {
            if (IsNaN(result) && (config.OverflowMode & OverflowExceptionModes.NaN) == OverflowExceptionModes.NaN) {
                throw new FloatDomainError("Computation results to 'NaN'");
            } else if (IsInfinite(result) && (config.OverflowMode & OverflowExceptionModes.Infinity) == OverflowExceptionModes.Infinity) {
                throw new FloatDomainError("Computation results to 'Infinity'");
            }
            return result;
        }

        /// <remarks>
        /// Generated by the following ruby code
        //require 'bigdecimal'

        //signs = [:-, :+]
        //zeros = [true, false]
        //types = [:NaN, :Finite, :Infinite]
        //ops = [:+, :-, :*, :/]

        //def CreateBigDecimal(sign, zero, type, finiteValue) 
        //  if zero
        //    BigDecimal.new("#{sign}0.0")
        //  else
        //    if type == :NaN
        //      BigDecimal.new("NaN")
        //    elsif type == :Infinite
        //      BigDecimal.new("#{sign}Infinity")
        //    else
        //      BigDecimal.new("#{sign}#{finiteValue}")
        //    end
        //  end
        //end

        //def map(r, null)
        //   if r.sign == BigDecimal::SIGN_NaN
        //     result = 'NaN'
        //   elsif r.sign == BigDecimal::SIGN_POSITIVE_INFINITE
        //     result = 'PositiveInfinity'
        //   elsif r.sign == BigDecimal::SIGN_NEGATIVE_INFINITE
        //     result = 'NegativeInfinity'
        //   elsif r.sign == BigDecimal::SIGN_POSITIVE_ZERO
        //     result = 'PositiveZero'
        //   elsif r.sign == BigDecimal::SIGN_NEGATIVE_ZERO
        //     result = 'NegativeZero'
        //   elsif null
        //     result = 'null'
        //   else
        //     result = 'Finite'
        //   end
        //end

        //signs.each do |xSign|
        //zeros.each do |xZero|
        // types.each do |xType|
        //   ops.each do |op|
        //     signs.each do |ySign|
        //       zeros.each do |yZero|
        //         types.each do |yType|
        //           x = CreateBigDecimal(xSign, xZero, xType, 7)
        //           y = CreateBigDecimal(ySign, yZero, yType, 11)
        //           r = x.send(op, y)
        //           result = map(r, true)
        //           puts "// #{map(x, false)} #{op} #{map(y, false)} = #{result}"
        //           puts "SR[#{signs.index(xSign)}, #{zeros.index(xZero)}, #{types.index(xType)}, #{ops.index(op)}, #{signs.index(ySign)}, #{zeros.index(yZero)}, #{types.index(yType)}] = #{result};"
        //         end
        //       end
        //     end
        //   end
        // end
        //end
        //end 
        /// </remarks>
        private static readonly BigDecimal[, , , , , ,] SR = new BigDecimal[2, 2, 3, 4, 2, 2, 3];
        private static void CreateSR() {
            //[xSign, xZero, xType, Op, ySign, yZero, yType]
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 0, 0, 0, 0, 0] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 0, 0, 0, 0, 1] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 0, 0, 0, 0, 2] = NegativeZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 0, 0, 0, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 0, 0, 0, 1, 1] = null;
            // NegativeZero + NegativeInfinity = NegativeInfinity
            SR[0, 0, 0, 0, 0, 1, 2] = NegativeInfinity;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 0, 0, 1, 0, 0] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 0, 0, 1, 0, 1] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 0, 0, 1, 0, 2] = PositiveZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 0, 0, 1, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 0, 0, 1, 1, 1] = null;
            // NegativeZero + PositiveInfinity = PositiveInfinity
            SR[0, 0, 0, 0, 1, 1, 2] = PositiveInfinity;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 0, 1, 0, 0, 0] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 0, 1, 0, 0, 1] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 0, 1, 0, 0, 2] = PositiveZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 0, 1, 0, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 0, 1, 0, 1, 1] = null;
            // NegativeZero - NegativeInfinity = PositiveInfinity
            SR[0, 0, 0, 1, 0, 1, 2] = PositiveInfinity;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 0, 1, 1, 0, 0] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 0, 1, 1, 0, 1] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 0, 1, 1, 0, 2] = NegativeZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 0, 1, 1, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 0, 1, 1, 1, 1] = null;
            // NegativeZero - PositiveInfinity = NegativeInfinity
            SR[0, 0, 0, 1, 1, 1, 2] = NegativeInfinity;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 0, 2, 0, 0, 0] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 0, 2, 0, 0, 1] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 0, 2, 0, 0, 2] = PositiveZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 0, 2, 0, 1, 0] = NaN;
            // NegativeZero * Finite = PositiveZero
            SR[0, 0, 0, 2, 0, 1, 1] = PositiveZero;
            // NegativeZero * NegativeInfinity = NaN
            SR[0, 0, 0, 2, 0, 1, 2] = NaN;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 0, 2, 1, 0, 0] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 0, 2, 1, 0, 1] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 0, 2, 1, 0, 2] = NegativeZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 0, 2, 1, 1, 0] = NaN;
            // NegativeZero * Finite = NegativeZero
            SR[0, 0, 0, 2, 1, 1, 1] = NegativeZero;
            // NegativeZero * PositiveInfinity = NaN
            SR[0, 0, 0, 2, 1, 1, 2] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 0, 3, 0, 0, 0] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 0, 3, 0, 0, 1] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 0, 3, 0, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 0, 3, 0, 1, 0] = NaN;
            // NegativeZero / Finite = PositiveZero
            SR[0, 0, 0, 3, 0, 1, 1] = PositiveZero;
            // NegativeZero / NegativeInfinity = PositiveZero
            SR[0, 0, 0, 3, 0, 1, 2] = PositiveZero;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 0, 3, 1, 0, 0] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 0, 3, 1, 0, 1] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 0, 3, 1, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 0, 3, 1, 1, 0] = NaN;
            // NegativeZero / Finite = NegativeZero
            SR[0, 0, 0, 3, 1, 1, 1] = NegativeZero;
            // NegativeZero / PositiveInfinity = NegativeZero
            SR[0, 0, 0, 3, 1, 1, 2] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 1, 0, 0, 0, 0] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 1, 0, 0, 0, 1] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 1, 0, 0, 0, 2] = NegativeZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 1, 0, 0, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 1, 0, 0, 1, 1] = null;
            // NegativeZero + NegativeInfinity = NegativeInfinity
            SR[0, 0, 1, 0, 0, 1, 2] = NegativeInfinity;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 1, 0, 1, 0, 0] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 1, 0, 1, 0, 1] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 1, 0, 1, 0, 2] = PositiveZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 1, 0, 1, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 1, 0, 1, 1, 1] = null;
            // NegativeZero + PositiveInfinity = PositiveInfinity
            SR[0, 0, 1, 0, 1, 1, 2] = PositiveInfinity;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 1, 1, 0, 0, 0] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 1, 1, 0, 0, 1] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 1, 1, 0, 0, 2] = PositiveZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 1, 1, 0, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 1, 1, 0, 1, 1] = null;
            // NegativeZero - NegativeInfinity = PositiveInfinity
            SR[0, 0, 1, 1, 0, 1, 2] = PositiveInfinity;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 1, 1, 1, 0, 0] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 1, 1, 1, 0, 1] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 1, 1, 1, 0, 2] = NegativeZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 1, 1, 1, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 1, 1, 1, 1, 1] = null;
            // NegativeZero - PositiveInfinity = NegativeInfinity
            SR[0, 0, 1, 1, 1, 1, 2] = NegativeInfinity;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 1, 2, 0, 0, 0] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 1, 2, 0, 0, 1] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 1, 2, 0, 0, 2] = PositiveZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 1, 2, 0, 1, 0] = NaN;
            // NegativeZero * Finite = PositiveZero
            SR[0, 0, 1, 2, 0, 1, 1] = PositiveZero;
            // NegativeZero * NegativeInfinity = NaN
            SR[0, 0, 1, 2, 0, 1, 2] = NaN;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 1, 2, 1, 0, 0] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 1, 2, 1, 0, 1] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 1, 2, 1, 0, 2] = NegativeZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 1, 2, 1, 1, 0] = NaN;
            // NegativeZero * Finite = NegativeZero
            SR[0, 0, 1, 2, 1, 1, 1] = NegativeZero;
            // NegativeZero * PositiveInfinity = NaN
            SR[0, 0, 1, 2, 1, 1, 2] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 1, 3, 0, 0, 0] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 1, 3, 0, 0, 1] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 1, 3, 0, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 1, 3, 0, 1, 0] = NaN;
            // NegativeZero / Finite = PositiveZero
            SR[0, 0, 1, 3, 0, 1, 1] = PositiveZero;
            // NegativeZero / NegativeInfinity = PositiveZero
            SR[0, 0, 1, 3, 0, 1, 2] = PositiveZero;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 1, 3, 1, 0, 0] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 1, 3, 1, 0, 1] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 1, 3, 1, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 1, 3, 1, 1, 0] = NaN;
            // NegativeZero / Finite = NegativeZero
            SR[0, 0, 1, 3, 1, 1, 1] = NegativeZero;
            // NegativeZero / PositiveInfinity = NegativeZero
            SR[0, 0, 1, 3, 1, 1, 2] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 2, 0, 0, 0, 0] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 2, 0, 0, 0, 1] = NegativeZero;
            // NegativeZero + NegativeZero = NegativeZero
            SR[0, 0, 2, 0, 0, 0, 2] = NegativeZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 2, 0, 0, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 2, 0, 0, 1, 1] = null;
            // NegativeZero + NegativeInfinity = NegativeInfinity
            SR[0, 0, 2, 0, 0, 1, 2] = NegativeInfinity;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 2, 0, 1, 0, 0] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 2, 0, 1, 0, 1] = PositiveZero;
            // NegativeZero + PositiveZero = PositiveZero
            SR[0, 0, 2, 0, 1, 0, 2] = PositiveZero;
            // NegativeZero + NaN = NaN
            SR[0, 0, 2, 0, 1, 1, 0] = NaN;
            // NegativeZero + Finite = null
            SR[0, 0, 2, 0, 1, 1, 1] = null;
            // NegativeZero + PositiveInfinity = PositiveInfinity
            SR[0, 0, 2, 0, 1, 1, 2] = PositiveInfinity;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 2, 1, 0, 0, 0] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 2, 1, 0, 0, 1] = PositiveZero;
            // NegativeZero - NegativeZero = PositiveZero
            SR[0, 0, 2, 1, 0, 0, 2] = PositiveZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 2, 1, 0, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 2, 1, 0, 1, 1] = null;
            // NegativeZero - NegativeInfinity = PositiveInfinity
            SR[0, 0, 2, 1, 0, 1, 2] = PositiveInfinity;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 2, 1, 1, 0, 0] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 2, 1, 1, 0, 1] = NegativeZero;
            // NegativeZero - PositiveZero = NegativeZero
            SR[0, 0, 2, 1, 1, 0, 2] = NegativeZero;
            // NegativeZero - NaN = NaN
            SR[0, 0, 2, 1, 1, 1, 0] = NaN;
            // NegativeZero - Finite = null
            SR[0, 0, 2, 1, 1, 1, 1] = null;
            // NegativeZero - PositiveInfinity = NegativeInfinity
            SR[0, 0, 2, 1, 1, 1, 2] = NegativeInfinity;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 2, 2, 0, 0, 0] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 2, 2, 0, 0, 1] = PositiveZero;
            // NegativeZero * NegativeZero = PositiveZero
            SR[0, 0, 2, 2, 0, 0, 2] = PositiveZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 2, 2, 0, 1, 0] = NaN;
            // NegativeZero * Finite = PositiveZero
            SR[0, 0, 2, 2, 0, 1, 1] = PositiveZero;
            // NegativeZero * NegativeInfinity = NaN
            SR[0, 0, 2, 2, 0, 1, 2] = NaN;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 2, 2, 1, 0, 0] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 2, 2, 1, 0, 1] = NegativeZero;
            // NegativeZero * PositiveZero = NegativeZero
            SR[0, 0, 2, 2, 1, 0, 2] = NegativeZero;
            // NegativeZero * NaN = NaN
            SR[0, 0, 2, 2, 1, 1, 0] = NaN;
            // NegativeZero * Finite = NegativeZero
            SR[0, 0, 2, 2, 1, 1, 1] = NegativeZero;
            // NegativeZero * PositiveInfinity = NaN
            SR[0, 0, 2, 2, 1, 1, 2] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 2, 3, 0, 0, 0] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 2, 3, 0, 0, 1] = NaN;
            // NegativeZero / NegativeZero = NaN
            SR[0, 0, 2, 3, 0, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 2, 3, 0, 1, 0] = NaN;
            // NegativeZero / Finite = PositiveZero
            SR[0, 0, 2, 3, 0, 1, 1] = PositiveZero;
            // NegativeZero / NegativeInfinity = PositiveZero
            SR[0, 0, 2, 3, 0, 1, 2] = PositiveZero;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 2, 3, 1, 0, 0] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 2, 3, 1, 0, 1] = NaN;
            // NegativeZero / PositiveZero = NaN
            SR[0, 0, 2, 3, 1, 0, 2] = NaN;
            // NegativeZero / NaN = NaN
            SR[0, 0, 2, 3, 1, 1, 0] = NaN;
            // NegativeZero / Finite = NegativeZero
            SR[0, 0, 2, 3, 1, 1, 1] = NegativeZero;
            // NegativeZero / PositiveInfinity = NegativeZero
            SR[0, 0, 2, 3, 1, 1, 2] = NegativeZero;
            // NaN + NegativeZero = NaN
            SR[0, 1, 0, 0, 0, 0, 0] = NaN;
            // NaN + NegativeZero = NaN
            SR[0, 1, 0, 0, 0, 0, 1] = NaN;
            // NaN + NegativeZero = NaN
            SR[0, 1, 0, 0, 0, 0, 2] = NaN;
            // NaN + NaN = NaN
            SR[0, 1, 0, 0, 0, 1, 0] = NaN;
            // NaN + Finite = NaN
            SR[0, 1, 0, 0, 0, 1, 1] = NaN;
            // NaN + NegativeInfinity = NaN
            SR[0, 1, 0, 0, 0, 1, 2] = NaN;
            // NaN + PositiveZero = NaN
            SR[0, 1, 0, 0, 1, 0, 0] = NaN;
            // NaN + PositiveZero = NaN
            SR[0, 1, 0, 0, 1, 0, 1] = NaN;
            // NaN + PositiveZero = NaN
            SR[0, 1, 0, 0, 1, 0, 2] = NaN;
            // NaN + NaN = NaN
            SR[0, 1, 0, 0, 1, 1, 0] = NaN;
            // NaN + Finite = NaN
            SR[0, 1, 0, 0, 1, 1, 1] = NaN;
            // NaN + PositiveInfinity = NaN
            SR[0, 1, 0, 0, 1, 1, 2] = NaN;
            // NaN - NegativeZero = NaN
            SR[0, 1, 0, 1, 0, 0, 0] = NaN;
            // NaN - NegativeZero = NaN
            SR[0, 1, 0, 1, 0, 0, 1] = NaN;
            // NaN - NegativeZero = NaN
            SR[0, 1, 0, 1, 0, 0, 2] = NaN;
            // NaN - NaN = NaN
            SR[0, 1, 0, 1, 0, 1, 0] = NaN;
            // NaN - Finite = NaN
            SR[0, 1, 0, 1, 0, 1, 1] = NaN;
            // NaN - NegativeInfinity = NaN
            SR[0, 1, 0, 1, 0, 1, 2] = NaN;
            // NaN - PositiveZero = NaN
            SR[0, 1, 0, 1, 1, 0, 0] = NaN;
            // NaN - PositiveZero = NaN
            SR[0, 1, 0, 1, 1, 0, 1] = NaN;
            // NaN - PositiveZero = NaN
            SR[0, 1, 0, 1, 1, 0, 2] = NaN;
            // NaN - NaN = NaN
            SR[0, 1, 0, 1, 1, 1, 0] = NaN;
            // NaN - Finite = NaN
            SR[0, 1, 0, 1, 1, 1, 1] = NaN;
            // NaN - PositiveInfinity = NaN
            SR[0, 1, 0, 1, 1, 1, 2] = NaN;
            // NaN * NegativeZero = NaN
            SR[0, 1, 0, 2, 0, 0, 0] = NaN;
            // NaN * NegativeZero = NaN
            SR[0, 1, 0, 2, 0, 0, 1] = NaN;
            // NaN * NegativeZero = NaN
            SR[0, 1, 0, 2, 0, 0, 2] = NaN;
            // NaN * NaN = NaN
            SR[0, 1, 0, 2, 0, 1, 0] = NaN;
            // NaN * Finite = NaN
            SR[0, 1, 0, 2, 0, 1, 1] = NaN;
            // NaN * NegativeInfinity = NaN
            SR[0, 1, 0, 2, 0, 1, 2] = NaN;
            // NaN * PositiveZero = NaN
            SR[0, 1, 0, 2, 1, 0, 0] = NaN;
            // NaN * PositiveZero = NaN
            SR[0, 1, 0, 2, 1, 0, 1] = NaN;
            // NaN * PositiveZero = NaN
            SR[0, 1, 0, 2, 1, 0, 2] = NaN;
            // NaN * NaN = NaN
            SR[0, 1, 0, 2, 1, 1, 0] = NaN;
            // NaN * Finite = NaN
            SR[0, 1, 0, 2, 1, 1, 1] = NaN;
            // NaN * PositiveInfinity = NaN
            SR[0, 1, 0, 2, 1, 1, 2] = NaN;
            // NaN / NegativeZero = NaN
            SR[0, 1, 0, 3, 0, 0, 0] = NaN;
            // NaN / NegativeZero = NaN
            SR[0, 1, 0, 3, 0, 0, 1] = NaN;
            // NaN / NegativeZero = NaN
            SR[0, 1, 0, 3, 0, 0, 2] = NaN;
            // NaN / NaN = NaN
            SR[0, 1, 0, 3, 0, 1, 0] = NaN;
            // NaN / Finite = NaN
            SR[0, 1, 0, 3, 0, 1, 1] = NaN;
            // NaN / NegativeInfinity = NaN
            SR[0, 1, 0, 3, 0, 1, 2] = NaN;
            // NaN / PositiveZero = NaN
            SR[0, 1, 0, 3, 1, 0, 0] = NaN;
            // NaN / PositiveZero = NaN
            SR[0, 1, 0, 3, 1, 0, 1] = NaN;
            // NaN / PositiveZero = NaN
            SR[0, 1, 0, 3, 1, 0, 2] = NaN;
            // NaN / NaN = NaN
            SR[0, 1, 0, 3, 1, 1, 0] = NaN;
            // NaN / Finite = NaN
            SR[0, 1, 0, 3, 1, 1, 1] = NaN;
            // NaN / PositiveInfinity = NaN
            SR[0, 1, 0, 3, 1, 1, 2] = NaN;
            // Finite + NegativeZero = null
            SR[0, 1, 1, 0, 0, 0, 0] = null;
            // Finite + NegativeZero = null
            SR[0, 1, 1, 0, 0, 0, 1] = null;
            // Finite + NegativeZero = null
            SR[0, 1, 1, 0, 0, 0, 2] = null;
            // Finite + NaN = NaN
            SR[0, 1, 1, 0, 0, 1, 0] = NaN;
            // Finite + Finite = null
            SR[0, 1, 1, 0, 0, 1, 1] = null;
            // Finite + NegativeInfinity = NegativeInfinity
            SR[0, 1, 1, 0, 0, 1, 2] = NegativeInfinity;
            // Finite + PositiveZero = null
            SR[0, 1, 1, 0, 1, 0, 0] = null;
            // Finite + PositiveZero = null
            SR[0, 1, 1, 0, 1, 0, 1] = null;
            // Finite + PositiveZero = null
            SR[0, 1, 1, 0, 1, 0, 2] = null;
            // Finite + NaN = NaN
            SR[0, 1, 1, 0, 1, 1, 0] = NaN;
            // Finite + Finite = null
            SR[0, 1, 1, 0, 1, 1, 1] = null;
            // Finite + PositiveInfinity = PositiveInfinity
            SR[0, 1, 1, 0, 1, 1, 2] = PositiveInfinity;
            // Finite - NegativeZero = null
            SR[0, 1, 1, 1, 0, 0, 0] = null;
            // Finite - NegativeZero = null
            SR[0, 1, 1, 1, 0, 0, 1] = null;
            // Finite - NegativeZero = null
            SR[0, 1, 1, 1, 0, 0, 2] = null;
            // Finite - NaN = NaN
            SR[0, 1, 1, 1, 0, 1, 0] = NaN;
            // Finite - Finite = null
            SR[0, 1, 1, 1, 0, 1, 1] = null;
            // Finite - NegativeInfinity = PositiveInfinity
            SR[0, 1, 1, 1, 0, 1, 2] = PositiveInfinity;
            // Finite - PositiveZero = null
            SR[0, 1, 1, 1, 1, 0, 0] = null;
            // Finite - PositiveZero = null
            SR[0, 1, 1, 1, 1, 0, 1] = null;
            // Finite - PositiveZero = null
            SR[0, 1, 1, 1, 1, 0, 2] = null;
            // Finite - NaN = NaN
            SR[0, 1, 1, 1, 1, 1, 0] = NaN;
            // Finite - Finite = null
            SR[0, 1, 1, 1, 1, 1, 1] = null;
            // Finite - PositiveInfinity = NegativeInfinity
            SR[0, 1, 1, 1, 1, 1, 2] = NegativeInfinity;
            // Finite * NegativeZero = PositiveZero
            SR[0, 1, 1, 2, 0, 0, 0] = PositiveZero;
            // Finite * NegativeZero = PositiveZero
            SR[0, 1, 1, 2, 0, 0, 1] = PositiveZero;
            // Finite * NegativeZero = PositiveZero
            SR[0, 1, 1, 2, 0, 0, 2] = PositiveZero;
            // Finite * NaN = NaN
            SR[0, 1, 1, 2, 0, 1, 0] = NaN;
            // Finite * Finite = null
            SR[0, 1, 1, 2, 0, 1, 1] = null;
            // Finite * NegativeInfinity = PositiveInfinity
            SR[0, 1, 1, 2, 0, 1, 2] = PositiveInfinity;
            // Finite * PositiveZero = NegativeZero
            SR[0, 1, 1, 2, 1, 0, 0] = NegativeZero;
            // Finite * PositiveZero = NegativeZero
            SR[0, 1, 1, 2, 1, 0, 1] = NegativeZero;
            // Finite * PositiveZero = NegativeZero
            SR[0, 1, 1, 2, 1, 0, 2] = NegativeZero;
            // Finite * NaN = NaN
            SR[0, 1, 1, 2, 1, 1, 0] = NaN;
            // Finite * Finite = null
            SR[0, 1, 1, 2, 1, 1, 1] = null;
            // Finite * PositiveInfinity = NegativeInfinity
            SR[0, 1, 1, 2, 1, 1, 2] = NegativeInfinity;
            // Finite / NegativeZero = PositiveInfinity
            SR[0, 1, 1, 3, 0, 0, 0] = PositiveInfinity;
            // Finite / NegativeZero = PositiveInfinity
            SR[0, 1, 1, 3, 0, 0, 1] = PositiveInfinity;
            // Finite / NegativeZero = PositiveInfinity
            SR[0, 1, 1, 3, 0, 0, 2] = PositiveInfinity;
            // Finite / NaN = NaN
            SR[0, 1, 1, 3, 0, 1, 0] = NaN;
            // Finite / Finite = null
            SR[0, 1, 1, 3, 0, 1, 1] = null;
            // Finite / NegativeInfinity = PositiveZero
            SR[0, 1, 1, 3, 0, 1, 2] = PositiveZero;
            // Finite / PositiveZero = NegativeInfinity
            SR[0, 1, 1, 3, 1, 0, 0] = NegativeInfinity;
            // Finite / PositiveZero = NegativeInfinity
            SR[0, 1, 1, 3, 1, 0, 1] = NegativeInfinity;
            // Finite / PositiveZero = NegativeInfinity
            SR[0, 1, 1, 3, 1, 0, 2] = NegativeInfinity;
            // Finite / NaN = NaN
            SR[0, 1, 1, 3, 1, 1, 0] = NaN;
            // Finite / Finite = null
            SR[0, 1, 1, 3, 1, 1, 1] = null;
            // Finite / PositiveInfinity = NegativeZero
            SR[0, 1, 1, 3, 1, 1, 2] = NegativeZero;
            // NegativeInfinity + NegativeZero = NegativeInfinity
            SR[0, 1, 2, 0, 0, 0, 0] = NegativeInfinity;
            // NegativeInfinity + NegativeZero = NegativeInfinity
            SR[0, 1, 2, 0, 0, 0, 1] = NegativeInfinity;
            // NegativeInfinity + NegativeZero = NegativeInfinity
            SR[0, 1, 2, 0, 0, 0, 2] = NegativeInfinity;
            // NegativeInfinity + NaN = NaN
            SR[0, 1, 2, 0, 0, 1, 0] = NaN;
            // NegativeInfinity + Finite = NegativeInfinity
            SR[0, 1, 2, 0, 0, 1, 1] = NegativeInfinity;
            // NegativeInfinity + NegativeInfinity = NegativeInfinity
            SR[0, 1, 2, 0, 0, 1, 2] = NegativeInfinity;
            // NegativeInfinity + PositiveZero = NegativeInfinity
            SR[0, 1, 2, 0, 1, 0, 0] = NegativeInfinity;
            // NegativeInfinity + PositiveZero = NegativeInfinity
            SR[0, 1, 2, 0, 1, 0, 1] = NegativeInfinity;
            // NegativeInfinity + PositiveZero = NegativeInfinity
            SR[0, 1, 2, 0, 1, 0, 2] = NegativeInfinity;
            // NegativeInfinity + NaN = NaN
            SR[0, 1, 2, 0, 1, 1, 0] = NaN;
            // NegativeInfinity + Finite = NegativeInfinity
            SR[0, 1, 2, 0, 1, 1, 1] = NegativeInfinity;
            // NegativeInfinity + PositiveInfinity = NaN
            SR[0, 1, 2, 0, 1, 1, 2] = NaN;
            // NegativeInfinity - NegativeZero = NegativeInfinity
            SR[0, 1, 2, 1, 0, 0, 0] = NegativeInfinity;
            // NegativeInfinity - NegativeZero = NegativeInfinity
            SR[0, 1, 2, 1, 0, 0, 1] = NegativeInfinity;
            // NegativeInfinity - NegativeZero = NegativeInfinity
            SR[0, 1, 2, 1, 0, 0, 2] = NegativeInfinity;
            // NegativeInfinity - NaN = NaN
            SR[0, 1, 2, 1, 0, 1, 0] = NaN;
            // NegativeInfinity - Finite = NegativeInfinity
            SR[0, 1, 2, 1, 0, 1, 1] = NegativeInfinity;
            // NegativeInfinity - NegativeInfinity = NaN
            SR[0, 1, 2, 1, 0, 1, 2] = NaN;
            // NegativeInfinity - PositiveZero = NegativeInfinity
            SR[0, 1, 2, 1, 1, 0, 0] = NegativeInfinity;
            // NegativeInfinity - PositiveZero = NegativeInfinity
            SR[0, 1, 2, 1, 1, 0, 1] = NegativeInfinity;
            // NegativeInfinity - PositiveZero = NegativeInfinity
            SR[0, 1, 2, 1, 1, 0, 2] = NegativeInfinity;
            // NegativeInfinity - NaN = NaN
            SR[0, 1, 2, 1, 1, 1, 0] = NaN;
            // NegativeInfinity - Finite = NegativeInfinity
            SR[0, 1, 2, 1, 1, 1, 1] = NegativeInfinity;
            // NegativeInfinity - PositiveInfinity = NegativeInfinity
            SR[0, 1, 2, 1, 1, 1, 2] = NegativeInfinity;
            // NegativeInfinity * NegativeZero = NaN
            SR[0, 1, 2, 2, 0, 0, 0] = NaN;
            // NegativeInfinity * NegativeZero = NaN
            SR[0, 1, 2, 2, 0, 0, 1] = NaN;
            // NegativeInfinity * NegativeZero = NaN
            SR[0, 1, 2, 2, 0, 0, 2] = NaN;
            // NegativeInfinity * NaN = NaN
            SR[0, 1, 2, 2, 0, 1, 0] = NaN;
            // NegativeInfinity * Finite = PositiveInfinity
            SR[0, 1, 2, 2, 0, 1, 1] = PositiveInfinity;
            // NegativeInfinity * NegativeInfinity = PositiveInfinity
            SR[0, 1, 2, 2, 0, 1, 2] = PositiveInfinity;
            // NegativeInfinity * PositiveZero = NaN
            SR[0, 1, 2, 2, 1, 0, 0] = NaN;
            // NegativeInfinity * PositiveZero = NaN
            SR[0, 1, 2, 2, 1, 0, 1] = NaN;
            // NegativeInfinity * PositiveZero = NaN
            SR[0, 1, 2, 2, 1, 0, 2] = NaN;
            // NegativeInfinity * NaN = NaN
            SR[0, 1, 2, 2, 1, 1, 0] = NaN;
            // NegativeInfinity * Finite = NegativeInfinity
            SR[0, 1, 2, 2, 1, 1, 1] = NegativeInfinity;
            // NegativeInfinity * PositiveInfinity = NegativeInfinity
            SR[0, 1, 2, 2, 1, 1, 2] = NegativeInfinity;
            // NegativeInfinity / NegativeZero = PositiveInfinity
            SR[0, 1, 2, 3, 0, 0, 0] = PositiveInfinity;
            // NegativeInfinity / NegativeZero = PositiveInfinity
            SR[0, 1, 2, 3, 0, 0, 1] = PositiveInfinity;
            // NegativeInfinity / NegativeZero = PositiveInfinity
            SR[0, 1, 2, 3, 0, 0, 2] = PositiveInfinity;
            // NegativeInfinity / NaN = NaN
            SR[0, 1, 2, 3, 0, 1, 0] = NaN;
            // NegativeInfinity / Finite = PositiveInfinity
            SR[0, 1, 2, 3, 0, 1, 1] = PositiveInfinity;
            // NegativeInfinity / NegativeInfinity = NaN
            SR[0, 1, 2, 3, 0, 1, 2] = NaN;
            // NegativeInfinity / PositiveZero = NegativeInfinity
            SR[0, 1, 2, 3, 1, 0, 0] = NegativeInfinity;
            // NegativeInfinity / PositiveZero = NegativeInfinity
            SR[0, 1, 2, 3, 1, 0, 1] = NegativeInfinity;
            // NegativeInfinity / PositiveZero = NegativeInfinity
            SR[0, 1, 2, 3, 1, 0, 2] = NegativeInfinity;
            // NegativeInfinity / NaN = NaN
            SR[0, 1, 2, 3, 1, 1, 0] = NaN;
            // NegativeInfinity / Finite = NegativeInfinity
            SR[0, 1, 2, 3, 1, 1, 1] = NegativeInfinity;
            // NegativeInfinity / PositiveInfinity = NaN
            SR[0, 1, 2, 3, 1, 1, 2] = NaN;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 0, 0, 0, 0, 0] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 0, 0, 0, 0, 1] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 0, 0, 0, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 0, 0, 0, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 0, 0, 0, 1, 1] = null;
            // PositiveZero + NegativeInfinity = NegativeInfinity
            SR[1, 0, 0, 0, 0, 1, 2] = NegativeInfinity;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 0, 0, 1, 0, 0] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 0, 0, 1, 0, 1] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 0, 0, 1, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 0, 0, 1, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 0, 0, 1, 1, 1] = null;
            // PositiveZero + PositiveInfinity = PositiveInfinity
            SR[1, 0, 0, 0, 1, 1, 2] = PositiveInfinity;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 0, 1, 0, 0, 0] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 0, 1, 0, 0, 1] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 0, 1, 0, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 0, 1, 0, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 0, 1, 0, 1, 1] = null;
            // PositiveZero - NegativeInfinity = PositiveInfinity
            SR[1, 0, 0, 1, 0, 1, 2] = PositiveInfinity;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 0, 1, 1, 0, 0] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 0, 1, 1, 0, 1] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 0, 1, 1, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 0, 1, 1, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 0, 1, 1, 1, 1] = null;
            // PositiveZero - PositiveInfinity = NegativeInfinity
            SR[1, 0, 0, 1, 1, 1, 2] = NegativeInfinity;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 0, 2, 0, 0, 0] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 0, 2, 0, 0, 1] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 0, 2, 0, 0, 2] = NegativeZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 0, 2, 0, 1, 0] = NaN;
            // PositiveZero * Finite = NegativeZero
            SR[1, 0, 0, 2, 0, 1, 1] = NegativeZero;
            // PositiveZero * NegativeInfinity = NaN
            SR[1, 0, 0, 2, 0, 1, 2] = NaN;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 0, 2, 1, 0, 0] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 0, 2, 1, 0, 1] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 0, 2, 1, 0, 2] = PositiveZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 0, 2, 1, 1, 0] = NaN;
            // PositiveZero * Finite = PositiveZero
            SR[1, 0, 0, 2, 1, 1, 1] = PositiveZero;
            // PositiveZero * PositiveInfinity = NaN
            SR[1, 0, 0, 2, 1, 1, 2] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 0, 3, 0, 0, 0] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 0, 3, 0, 0, 1] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 0, 3, 0, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 0, 3, 0, 1, 0] = NaN;
            // PositiveZero / Finite = NegativeZero
            SR[1, 0, 0, 3, 0, 1, 1] = NegativeZero;
            // PositiveZero / NegativeInfinity = NegativeZero
            SR[1, 0, 0, 3, 0, 1, 2] = NegativeZero;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 0, 3, 1, 0, 0] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 0, 3, 1, 0, 1] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 0, 3, 1, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 0, 3, 1, 1, 0] = NaN;
            // PositiveZero / Finite = PositiveZero
            SR[1, 0, 0, 3, 1, 1, 1] = PositiveZero;
            // PositiveZero / PositiveInfinity = PositiveZero
            SR[1, 0, 0, 3, 1, 1, 2] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 1, 0, 0, 0, 0] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 1, 0, 0, 0, 1] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 1, 0, 0, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 1, 0, 0, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 1, 0, 0, 1, 1] = null;
            // PositiveZero + NegativeInfinity = NegativeInfinity
            SR[1, 0, 1, 0, 0, 1, 2] = NegativeInfinity;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 1, 0, 1, 0, 0] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 1, 0, 1, 0, 1] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 1, 0, 1, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 1, 0, 1, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 1, 0, 1, 1, 1] = null;
            // PositiveZero + PositiveInfinity = PositiveInfinity
            SR[1, 0, 1, 0, 1, 1, 2] = PositiveInfinity;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 1, 1, 0, 0, 0] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 1, 1, 0, 0, 1] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 1, 1, 0, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 1, 1, 0, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 1, 1, 0, 1, 1] = null;
            // PositiveZero - NegativeInfinity = PositiveInfinity
            SR[1, 0, 1, 1, 0, 1, 2] = PositiveInfinity;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 1, 1, 1, 0, 0] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 1, 1, 1, 0, 1] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 1, 1, 1, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 1, 1, 1, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 1, 1, 1, 1, 1] = null;
            // PositiveZero - PositiveInfinity = NegativeInfinity
            SR[1, 0, 1, 1, 1, 1, 2] = NegativeInfinity;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 1, 2, 0, 0, 0] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 1, 2, 0, 0, 1] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 1, 2, 0, 0, 2] = NegativeZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 1, 2, 0, 1, 0] = NaN;
            // PositiveZero * Finite = NegativeZero
            SR[1, 0, 1, 2, 0, 1, 1] = NegativeZero;
            // PositiveZero * NegativeInfinity = NaN
            SR[1, 0, 1, 2, 0, 1, 2] = NaN;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 1, 2, 1, 0, 0] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 1, 2, 1, 0, 1] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 1, 2, 1, 0, 2] = PositiveZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 1, 2, 1, 1, 0] = NaN;
            // PositiveZero * Finite = PositiveZero
            SR[1, 0, 1, 2, 1, 1, 1] = PositiveZero;
            // PositiveZero * PositiveInfinity = NaN
            SR[1, 0, 1, 2, 1, 1, 2] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 1, 3, 0, 0, 0] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 1, 3, 0, 0, 1] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 1, 3, 0, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 1, 3, 0, 1, 0] = NaN;
            // PositiveZero / Finite = NegativeZero
            SR[1, 0, 1, 3, 0, 1, 1] = NegativeZero;
            // PositiveZero / NegativeInfinity = NegativeZero
            SR[1, 0, 1, 3, 0, 1, 2] = NegativeZero;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 1, 3, 1, 0, 0] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 1, 3, 1, 0, 1] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 1, 3, 1, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 1, 3, 1, 1, 0] = NaN;
            // PositiveZero / Finite = PositiveZero
            SR[1, 0, 1, 3, 1, 1, 1] = PositiveZero;
            // PositiveZero / PositiveInfinity = PositiveZero
            SR[1, 0, 1, 3, 1, 1, 2] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 2, 0, 0, 0, 0] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 2, 0, 0, 0, 1] = PositiveZero;
            // PositiveZero + NegativeZero = PositiveZero
            SR[1, 0, 2, 0, 0, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 2, 0, 0, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 2, 0, 0, 1, 1] = null;
            // PositiveZero + NegativeInfinity = NegativeInfinity
            SR[1, 0, 2, 0, 0, 1, 2] = NegativeInfinity;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 2, 0, 1, 0, 0] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 2, 0, 1, 0, 1] = PositiveZero;
            // PositiveZero + PositiveZero = PositiveZero
            SR[1, 0, 2, 0, 1, 0, 2] = PositiveZero;
            // PositiveZero + NaN = NaN
            SR[1, 0, 2, 0, 1, 1, 0] = NaN;
            // PositiveZero + Finite = null
            SR[1, 0, 2, 0, 1, 1, 1] = null;
            // PositiveZero + PositiveInfinity = PositiveInfinity
            SR[1, 0, 2, 0, 1, 1, 2] = PositiveInfinity;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 2, 1, 0, 0, 0] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 2, 1, 0, 0, 1] = PositiveZero;
            // PositiveZero - NegativeZero = PositiveZero
            SR[1, 0, 2, 1, 0, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 2, 1, 0, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 2, 1, 0, 1, 1] = null;
            // PositiveZero - NegativeInfinity = PositiveInfinity
            SR[1, 0, 2, 1, 0, 1, 2] = PositiveInfinity;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 2, 1, 1, 0, 0] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 2, 1, 1, 0, 1] = PositiveZero;
            // PositiveZero - PositiveZero = PositiveZero
            SR[1, 0, 2, 1, 1, 0, 2] = PositiveZero;
            // PositiveZero - NaN = NaN
            SR[1, 0, 2, 1, 1, 1, 0] = NaN;
            // PositiveZero - Finite = null
            SR[1, 0, 2, 1, 1, 1, 1] = null;
            // PositiveZero - PositiveInfinity = NegativeInfinity
            SR[1, 0, 2, 1, 1, 1, 2] = NegativeInfinity;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 2, 2, 0, 0, 0] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 2, 2, 0, 0, 1] = NegativeZero;
            // PositiveZero * NegativeZero = NegativeZero
            SR[1, 0, 2, 2, 0, 0, 2] = NegativeZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 2, 2, 0, 1, 0] = NaN;
            // PositiveZero * Finite = NegativeZero
            SR[1, 0, 2, 2, 0, 1, 1] = NegativeZero;
            // PositiveZero * NegativeInfinity = NaN
            SR[1, 0, 2, 2, 0, 1, 2] = NaN;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 2, 2, 1, 0, 0] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 2, 2, 1, 0, 1] = PositiveZero;
            // PositiveZero * PositiveZero = PositiveZero
            SR[1, 0, 2, 2, 1, 0, 2] = PositiveZero;
            // PositiveZero * NaN = NaN
            SR[1, 0, 2, 2, 1, 1, 0] = NaN;
            // PositiveZero * Finite = PositiveZero
            SR[1, 0, 2, 2, 1, 1, 1] = PositiveZero;
            // PositiveZero * PositiveInfinity = NaN
            SR[1, 0, 2, 2, 1, 1, 2] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 2, 3, 0, 0, 0] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 2, 3, 0, 0, 1] = NaN;
            // PositiveZero / NegativeZero = NaN
            SR[1, 0, 2, 3, 0, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 2, 3, 0, 1, 0] = NaN;
            // PositiveZero / Finite = NegativeZero
            SR[1, 0, 2, 3, 0, 1, 1] = NegativeZero;
            // PositiveZero / NegativeInfinity = NegativeZero
            SR[1, 0, 2, 3, 0, 1, 2] = NegativeZero;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 2, 3, 1, 0, 0] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 2, 3, 1, 0, 1] = NaN;
            // PositiveZero / PositiveZero = NaN
            SR[1, 0, 2, 3, 1, 0, 2] = NaN;
            // PositiveZero / NaN = NaN
            SR[1, 0, 2, 3, 1, 1, 0] = NaN;
            // PositiveZero / Finite = PositiveZero
            SR[1, 0, 2, 3, 1, 1, 1] = PositiveZero;
            // PositiveZero / PositiveInfinity = PositiveZero
            SR[1, 0, 2, 3, 1, 1, 2] = PositiveZero;
            // NaN + NegativeZero = NaN
            SR[1, 1, 0, 0, 0, 0, 0] = NaN;
            // NaN + NegativeZero = NaN
            SR[1, 1, 0, 0, 0, 0, 1] = NaN;
            // NaN + NegativeZero = NaN
            SR[1, 1, 0, 0, 0, 0, 2] = NaN;
            // NaN + NaN = NaN
            SR[1, 1, 0, 0, 0, 1, 0] = NaN;
            // NaN + Finite = NaN
            SR[1, 1, 0, 0, 0, 1, 1] = NaN;
            // NaN + NegativeInfinity = NaN
            SR[1, 1, 0, 0, 0, 1, 2] = NaN;
            // NaN + PositiveZero = NaN
            SR[1, 1, 0, 0, 1, 0, 0] = NaN;
            // NaN + PositiveZero = NaN
            SR[1, 1, 0, 0, 1, 0, 1] = NaN;
            // NaN + PositiveZero = NaN
            SR[1, 1, 0, 0, 1, 0, 2] = NaN;
            // NaN + NaN = NaN
            SR[1, 1, 0, 0, 1, 1, 0] = NaN;
            // NaN + Finite = NaN
            SR[1, 1, 0, 0, 1, 1, 1] = NaN;
            // NaN + PositiveInfinity = NaN
            SR[1, 1, 0, 0, 1, 1, 2] = NaN;
            // NaN - NegativeZero = NaN
            SR[1, 1, 0, 1, 0, 0, 0] = NaN;
            // NaN - NegativeZero = NaN
            SR[1, 1, 0, 1, 0, 0, 1] = NaN;
            // NaN - NegativeZero = NaN
            SR[1, 1, 0, 1, 0, 0, 2] = NaN;
            // NaN - NaN = NaN
            SR[1, 1, 0, 1, 0, 1, 0] = NaN;
            // NaN - Finite = NaN
            SR[1, 1, 0, 1, 0, 1, 1] = NaN;
            // NaN - NegativeInfinity = NaN
            SR[1, 1, 0, 1, 0, 1, 2] = NaN;
            // NaN - PositiveZero = NaN
            SR[1, 1, 0, 1, 1, 0, 0] = NaN;
            // NaN - PositiveZero = NaN
            SR[1, 1, 0, 1, 1, 0, 1] = NaN;
            // NaN - PositiveZero = NaN
            SR[1, 1, 0, 1, 1, 0, 2] = NaN;
            // NaN - NaN = NaN
            SR[1, 1, 0, 1, 1, 1, 0] = NaN;
            // NaN - Finite = NaN
            SR[1, 1, 0, 1, 1, 1, 1] = NaN;
            // NaN - PositiveInfinity = NaN
            SR[1, 1, 0, 1, 1, 1, 2] = NaN;
            // NaN * NegativeZero = NaN
            SR[1, 1, 0, 2, 0, 0, 0] = NaN;
            // NaN * NegativeZero = NaN
            SR[1, 1, 0, 2, 0, 0, 1] = NaN;
            // NaN * NegativeZero = NaN
            SR[1, 1, 0, 2, 0, 0, 2] = NaN;
            // NaN * NaN = NaN
            SR[1, 1, 0, 2, 0, 1, 0] = NaN;
            // NaN * Finite = NaN
            SR[1, 1, 0, 2, 0, 1, 1] = NaN;
            // NaN * NegativeInfinity = NaN
            SR[1, 1, 0, 2, 0, 1, 2] = NaN;
            // NaN * PositiveZero = NaN
            SR[1, 1, 0, 2, 1, 0, 0] = NaN;
            // NaN * PositiveZero = NaN
            SR[1, 1, 0, 2, 1, 0, 1] = NaN;
            // NaN * PositiveZero = NaN
            SR[1, 1, 0, 2, 1, 0, 2] = NaN;
            // NaN * NaN = NaN
            SR[1, 1, 0, 2, 1, 1, 0] = NaN;
            // NaN * Finite = NaN
            SR[1, 1, 0, 2, 1, 1, 1] = NaN;
            // NaN * PositiveInfinity = NaN
            SR[1, 1, 0, 2, 1, 1, 2] = NaN;
            // NaN / NegativeZero = NaN
            SR[1, 1, 0, 3, 0, 0, 0] = NaN;
            // NaN / NegativeZero = NaN
            SR[1, 1, 0, 3, 0, 0, 1] = NaN;
            // NaN / NegativeZero = NaN
            SR[1, 1, 0, 3, 0, 0, 2] = NaN;
            // NaN / NaN = NaN
            SR[1, 1, 0, 3, 0, 1, 0] = NaN;
            // NaN / Finite = NaN
            SR[1, 1, 0, 3, 0, 1, 1] = NaN;
            // NaN / NegativeInfinity = NaN
            SR[1, 1, 0, 3, 0, 1, 2] = NaN;
            // NaN / PositiveZero = NaN
            SR[1, 1, 0, 3, 1, 0, 0] = NaN;
            // NaN / PositiveZero = NaN
            SR[1, 1, 0, 3, 1, 0, 1] = NaN;
            // NaN / PositiveZero = NaN
            SR[1, 1, 0, 3, 1, 0, 2] = NaN;
            // NaN / NaN = NaN
            SR[1, 1, 0, 3, 1, 1, 0] = NaN;
            // NaN / Finite = NaN
            SR[1, 1, 0, 3, 1, 1, 1] = NaN;
            // NaN / PositiveInfinity = NaN
            SR[1, 1, 0, 3, 1, 1, 2] = NaN;
            // Finite + NegativeZero = null
            SR[1, 1, 1, 0, 0, 0, 0] = null;
            // Finite + NegativeZero = null
            SR[1, 1, 1, 0, 0, 0, 1] = null;
            // Finite + NegativeZero = null
            SR[1, 1, 1, 0, 0, 0, 2] = null;
            // Finite + NaN = NaN
            SR[1, 1, 1, 0, 0, 1, 0] = NaN;
            // Finite + Finite = null
            SR[1, 1, 1, 0, 0, 1, 1] = null;
            // Finite + NegativeInfinity = NegativeInfinity
            SR[1, 1, 1, 0, 0, 1, 2] = NegativeInfinity;
            // Finite + PositiveZero = null
            SR[1, 1, 1, 0, 1, 0, 0] = null;
            // Finite + PositiveZero = null
            SR[1, 1, 1, 0, 1, 0, 1] = null;
            // Finite + PositiveZero = null
            SR[1, 1, 1, 0, 1, 0, 2] = null;
            // Finite + NaN = NaN
            SR[1, 1, 1, 0, 1, 1, 0] = NaN;
            // Finite + Finite = null
            SR[1, 1, 1, 0, 1, 1, 1] = null;
            // Finite + PositiveInfinity = PositiveInfinity
            SR[1, 1, 1, 0, 1, 1, 2] = PositiveInfinity;
            // Finite - NegativeZero = null
            SR[1, 1, 1, 1, 0, 0, 0] = null;
            // Finite - NegativeZero = null
            SR[1, 1, 1, 1, 0, 0, 1] = null;
            // Finite - NegativeZero = null
            SR[1, 1, 1, 1, 0, 0, 2] = null;
            // Finite - NaN = NaN
            SR[1, 1, 1, 1, 0, 1, 0] = NaN;
            // Finite - Finite = null
            SR[1, 1, 1, 1, 0, 1, 1] = null;
            // Finite - NegativeInfinity = PositiveInfinity
            SR[1, 1, 1, 1, 0, 1, 2] = PositiveInfinity;
            // Finite - PositiveZero = null
            SR[1, 1, 1, 1, 1, 0, 0] = null;
            // Finite - PositiveZero = null
            SR[1, 1, 1, 1, 1, 0, 1] = null;
            // Finite - PositiveZero = null
            SR[1, 1, 1, 1, 1, 0, 2] = null;
            // Finite - NaN = NaN
            SR[1, 1, 1, 1, 1, 1, 0] = NaN;
            // Finite - Finite = null
            SR[1, 1, 1, 1, 1, 1, 1] = null;
            // Finite - PositiveInfinity = NegativeInfinity
            SR[1, 1, 1, 1, 1, 1, 2] = NegativeInfinity;
            // Finite * NegativeZero = NegativeZero
            SR[1, 1, 1, 2, 0, 0, 0] = NegativeZero;
            // Finite * NegativeZero = NegativeZero
            SR[1, 1, 1, 2, 0, 0, 1] = NegativeZero;
            // Finite * NegativeZero = NegativeZero
            SR[1, 1, 1, 2, 0, 0, 2] = NegativeZero;
            // Finite * NaN = NaN
            SR[1, 1, 1, 2, 0, 1, 0] = NaN;
            // Finite * Finite = null
            SR[1, 1, 1, 2, 0, 1, 1] = null;
            // Finite * NegativeInfinity = NegativeInfinity
            SR[1, 1, 1, 2, 0, 1, 2] = NegativeInfinity;
            // Finite * PositiveZero = PositiveZero
            SR[1, 1, 1, 2, 1, 0, 0] = PositiveZero;
            // Finite * PositiveZero = PositiveZero
            SR[1, 1, 1, 2, 1, 0, 1] = PositiveZero;
            // Finite * PositiveZero = PositiveZero
            SR[1, 1, 1, 2, 1, 0, 2] = PositiveZero;
            // Finite * NaN = NaN
            SR[1, 1, 1, 2, 1, 1, 0] = NaN;
            // Finite * Finite = null
            SR[1, 1, 1, 2, 1, 1, 1] = null;
            // Finite * PositiveInfinity = PositiveInfinity
            SR[1, 1, 1, 2, 1, 1, 2] = PositiveInfinity;
            // Finite / NegativeZero = NegativeInfinity
            SR[1, 1, 1, 3, 0, 0, 0] = NegativeInfinity;
            // Finite / NegativeZero = NegativeInfinity
            SR[1, 1, 1, 3, 0, 0, 1] = NegativeInfinity;
            // Finite / NegativeZero = NegativeInfinity
            SR[1, 1, 1, 3, 0, 0, 2] = NegativeInfinity;
            // Finite / NaN = NaN
            SR[1, 1, 1, 3, 0, 1, 0] = NaN;
            // Finite / Finite = null
            SR[1, 1, 1, 3, 0, 1, 1] = null;
            // Finite / NegativeInfinity = NegativeZero
            SR[1, 1, 1, 3, 0, 1, 2] = NegativeZero;
            // Finite / PositiveZero = PositiveInfinity
            SR[1, 1, 1, 3, 1, 0, 0] = PositiveInfinity;
            // Finite / PositiveZero = PositiveInfinity
            SR[1, 1, 1, 3, 1, 0, 1] = PositiveInfinity;
            // Finite / PositiveZero = PositiveInfinity
            SR[1, 1, 1, 3, 1, 0, 2] = PositiveInfinity;
            // Finite / NaN = NaN
            SR[1, 1, 1, 3, 1, 1, 0] = NaN;
            // Finite / Finite = null
            SR[1, 1, 1, 3, 1, 1, 1] = null;
            // Finite / PositiveInfinity = PositiveZero
            SR[1, 1, 1, 3, 1, 1, 2] = PositiveZero;
            // PositiveInfinity + NegativeZero = PositiveInfinity
            SR[1, 1, 2, 0, 0, 0, 0] = PositiveInfinity;
            // PositiveInfinity + NegativeZero = PositiveInfinity
            SR[1, 1, 2, 0, 0, 0, 1] = PositiveInfinity;
            // PositiveInfinity + NegativeZero = PositiveInfinity
            SR[1, 1, 2, 0, 0, 0, 2] = PositiveInfinity;
            // PositiveInfinity + NaN = NaN
            SR[1, 1, 2, 0, 0, 1, 0] = NaN;
            // PositiveInfinity + Finite = PositiveInfinity
            SR[1, 1, 2, 0, 0, 1, 1] = PositiveInfinity;
            // PositiveInfinity + NegativeInfinity = NaN
            SR[1, 1, 2, 0, 0, 1, 2] = NaN;
            // PositiveInfinity + PositiveZero = PositiveInfinity
            SR[1, 1, 2, 0, 1, 0, 0] = PositiveInfinity;
            // PositiveInfinity + PositiveZero = PositiveInfinity
            SR[1, 1, 2, 0, 1, 0, 1] = PositiveInfinity;
            // PositiveInfinity + PositiveZero = PositiveInfinity
            SR[1, 1, 2, 0, 1, 0, 2] = PositiveInfinity;
            // PositiveInfinity + NaN = NaN
            SR[1, 1, 2, 0, 1, 1, 0] = NaN;
            // PositiveInfinity + Finite = PositiveInfinity
            SR[1, 1, 2, 0, 1, 1, 1] = PositiveInfinity;
            // PositiveInfinity + PositiveInfinity = PositiveInfinity
            SR[1, 1, 2, 0, 1, 1, 2] = PositiveInfinity;
            // PositiveInfinity - NegativeZero = PositiveInfinity
            SR[1, 1, 2, 1, 0, 0, 0] = PositiveInfinity;
            // PositiveInfinity - NegativeZero = PositiveInfinity
            SR[1, 1, 2, 1, 0, 0, 1] = PositiveInfinity;
            // PositiveInfinity - NegativeZero = PositiveInfinity
            SR[1, 1, 2, 1, 0, 0, 2] = PositiveInfinity;
            // PositiveInfinity - NaN = NaN
            SR[1, 1, 2, 1, 0, 1, 0] = NaN;
            // PositiveInfinity - Finite = PositiveInfinity
            SR[1, 1, 2, 1, 0, 1, 1] = PositiveInfinity;
            // PositiveInfinity - NegativeInfinity = PositiveInfinity
            SR[1, 1, 2, 1, 0, 1, 2] = PositiveInfinity;
            // PositiveInfinity - PositiveZero = PositiveInfinity
            SR[1, 1, 2, 1, 1, 0, 0] = PositiveInfinity;
            // PositiveInfinity - PositiveZero = PositiveInfinity
            SR[1, 1, 2, 1, 1, 0, 1] = PositiveInfinity;
            // PositiveInfinity - PositiveZero = PositiveInfinity
            SR[1, 1, 2, 1, 1, 0, 2] = PositiveInfinity;
            // PositiveInfinity - NaN = NaN
            SR[1, 1, 2, 1, 1, 1, 0] = NaN;
            // PositiveInfinity - Finite = PositiveInfinity
            SR[1, 1, 2, 1, 1, 1, 1] = PositiveInfinity;
            // PositiveInfinity - PositiveInfinity = NaN
            SR[1, 1, 2, 1, 1, 1, 2] = NaN;
            // PositiveInfinity * NegativeZero = NaN
            SR[1, 1, 2, 2, 0, 0, 0] = NaN;
            // PositiveInfinity * NegativeZero = NaN
            SR[1, 1, 2, 2, 0, 0, 1] = NaN;
            // PositiveInfinity * NegativeZero = NaN
            SR[1, 1, 2, 2, 0, 0, 2] = NaN;
            // PositiveInfinity * NaN = NaN
            SR[1, 1, 2, 2, 0, 1, 0] = NaN;
            // PositiveInfinity * Finite = NegativeInfinity
            SR[1, 1, 2, 2, 0, 1, 1] = NegativeInfinity;
            // PositiveInfinity * NegativeInfinity = NegativeInfinity
            SR[1, 1, 2, 2, 0, 1, 2] = NegativeInfinity;
            // PositiveInfinity * PositiveZero = NaN
            SR[1, 1, 2, 2, 1, 0, 0] = NaN;
            // PositiveInfinity * PositiveZero = NaN
            SR[1, 1, 2, 2, 1, 0, 1] = NaN;
            // PositiveInfinity * PositiveZero = NaN
            SR[1, 1, 2, 2, 1, 0, 2] = NaN;
            // PositiveInfinity * NaN = NaN
            SR[1, 1, 2, 2, 1, 1, 0] = NaN;
            // PositiveInfinity * Finite = PositiveInfinity
            SR[1, 1, 2, 2, 1, 1, 1] = PositiveInfinity;
            // PositiveInfinity * PositiveInfinity = PositiveInfinity
            SR[1, 1, 2, 2, 1, 1, 2] = PositiveInfinity;
            // PositiveInfinity / NegativeZero = NegativeInfinity
            SR[1, 1, 2, 3, 0, 0, 0] = NegativeInfinity;
            // PositiveInfinity / NegativeZero = NegativeInfinity
            SR[1, 1, 2, 3, 0, 0, 1] = NegativeInfinity;
            // PositiveInfinity / NegativeZero = NegativeInfinity
            SR[1, 1, 2, 3, 0, 0, 2] = NegativeInfinity;
            // PositiveInfinity / NaN = NaN
            SR[1, 1, 2, 3, 0, 1, 0] = NaN;
            // PositiveInfinity / Finite = NegativeInfinity
            SR[1, 1, 2, 3, 0, 1, 1] = NegativeInfinity;
            // PositiveInfinity / NegativeInfinity = NaN
            SR[1, 1, 2, 3, 0, 1, 2] = NaN;
            // PositiveInfinity / PositiveZero = PositiveInfinity
            SR[1, 1, 2, 3, 1, 0, 0] = PositiveInfinity;
            // PositiveInfinity / PositiveZero = PositiveInfinity
            SR[1, 1, 2, 3, 1, 0, 1] = PositiveInfinity;
            // PositiveInfinity / PositiveZero = PositiveInfinity
            SR[1, 1, 2, 3, 1, 0, 2] = PositiveInfinity;
            // PositiveInfinity / NaN = NaN
            SR[1, 1, 2, 3, 1, 1, 0] = NaN;
            // PositiveInfinity / Finite = PositiveInfinity
            SR[1, 1, 2, 3, 1, 1, 1] = PositiveInfinity;
            // PositiveInfinity / PositiveInfinity = NaN
            SR[1, 1, 2, 3, 1, 1, 2] = NaN;
        }
        #endregion

        #region ToString
        public string GetFractionString() {
            if (IsFinite(this)) {
                return this._fraction.ToString();
            } else if (IsInfinite(this)) {
                return InfinityString;
            } else {
                return NaNString;
            }
        }
        public override string ToString() {
            return ToString(0);
        }
        public string ToString(int separateAt) {
            return ToString(separateAt, "", false);
        }
        public string ToString(int separateAt, string plusSign, bool floatStyleFormat) {
            if (separateAt < 0) {
                throw new ArgumentException("argument must be positive");
            }

            if (IsFinite(this) && !IsZero(this)) {
                StringBuilder sb = new StringBuilder();
                if (IsNegative(this)) {
                    sb.Append("-");
                } else {
                    sb.Append(plusSign);
                }
                if (floatStyleFormat) {
                    AppendFloatStyle(sb, separateAt);
                } else {
                    AppendEngineeringStyle(sb, separateAt);
                }
                return sb.ToString();
            }
            if (IsPositiveZero(this)) {
                return plusSign + ZeroString;
            }
            if (IsNegativeZero(this)) {
                return NegativeZeroString;
            }
            if (IsPositiveInfinite(this)) {
                return plusSign + InfinityString;
            }
            if (IsNegativeInfinite(this)) {
                return NegativeInfinityString;
            }
            return NaNString;
        }

        private void AppendFloatStyle(StringBuilder/*!*/ sb, int separateAt) {
            if (_exponent <= 0) {
                sb.Append("0.");
                AppendDigits(sb, new string('0', -_exponent) + _fraction.ToString(), 0, Digits - _exponent, separateAt);
            } else {
                int expLenDiff = _exponent - Digits;
                if (expLenDiff >= 0) {
                    AppendDigits(sb, _fraction.ToString() + new string('0', expLenDiff), 0, _exponent, separateAt);
                    sb.Append(".0");
                } else {
                    AppendDigits(sb, _fraction.ToString(), 0, _exponent, separateAt);
                    sb.Append(".");
                    AppendDigits(sb, _fraction.ToString(), _exponent, Digits - _exponent, separateAt);
                }
            }
        }

        private void AppendEngineeringStyle(StringBuilder/*!*/ sb, int separateAt) {
            sb.Append("0.");
            AppendDigits(sb, _fraction.ToString(), 0, Digits, separateAt);
            sb.Append("E");
            sb.AppendFormat("{0}", this._exponent);
        }

        private void AppendDigits(StringBuilder/*!*/ sb, string digits, int start, int length, int separateAt) {
            int current = start;
            if (separateAt > 0) {
                while (current + separateAt < start + length) {
                    sb.Append(digits.Substring(current, separateAt));
                    current += separateAt;
                    sb.Append(" ");
                }
            }
            sb.Append(digits.Substring(current, length - (current - start)));
        }
        #endregion

        #region Comparison
        #region IEquatable<BigDecimal> Members

        public bool Equals(BigDecimal other) {
            return CompareTo(other) == 0;
        }

        #endregion

        public int? CompareBigDecimal(BigDecimal other) {
            int? result;
            if (other == null) {
                result = null;
            } else if (!IsNonZeroFinite(this) || !IsNonZeroFinite(other)) {
                result = CheckSpecialComparison(this, other);
            } else {
                if (this._sign != other._sign) {
                    // Different signs
                    result = this._sign > other._sign ? 1 : -1;
                } else {
                    result = this._sign * (Fraction.Compare(this._fraction, other._fraction, this._exponent - other._exponent));
                }
            }
            return result;
        }

        #region IComparable<BigDecimal> Members
        public int CompareTo(BigDecimal other) {
            int? result = CompareBigDecimal(other);
            if (result.HasValue) {
                return result.Value;
            } else {
                // Must have NaNs so we map them to Double.CompareTo results
                if (IsNaN(this)) {
                    if (IsNaN(other)) {
                        // Both NaN so 
                        return 0;
                    } else {
                        // this == NaN && other != NaN
                        return -1;
                    }
                } else {
                    return 1;
                }
            }
        }
        #endregion
        #endregion

        #region Hash Code
        public int GetSignCode() {
            switch (this.NumberType) {
                case BigDecimal.NumberTypes.NaN:
                    return 0;
                case BigDecimal.NumberTypes.Infinite:
                    return this.Sign * 3;
                default:
                case BigDecimal.NumberTypes.Finite:
                    if (BigDecimal.IsZero(this)) {
                        return this.Sign;
                    } else {
                        return this.Sign * 2;
                    }
            }
        }
        public override int GetHashCode() {
            int hash = this.GetSignCode();
            // Special values have special hash codes
            if (hash == 2) {
                int fHash = _fraction.GetHashCode();
                hash = (31 * hash + fHash) ^ fHash + Exponent;
            }
            return hash;
        }
        #endregion

        //#region Conversion
        //public static implicit operator BigDecimal/*!*/(int i) {
        //    Config dummy = new Config();
        //    return Create(dummy, i.ToString());
        //}
        //public static implicit operator BigDecimal/*!*/(BigInteger/*!*/ i) {
        //    Config dummy = new Config();
        //    return Create(dummy, i.ToString());
        //}
        //public static implicit operator BigDecimal/*!*/(double d) {
        //    Config dummy = new Config();
        //    return Create(dummy, d.ToString());
        //}
        //#endregion
    }

}
