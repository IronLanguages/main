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
using System.Text;
using Microsoft.Scripting.Math;
using System.Globalization;

namespace IronRuby.StandardLibrary.BigDecimal {
    public class Fraction {

        #region Constants
        // Number of decimal places held in each word [BASE_FIG == log10(BASE) and 10^BASE_FIG == BASE]
        public const int BASE_FIG = 9;
        // Each word contains a value between 0 and BASE-1
        public const uint BASE = 1000000000;
        // Half the BASE value used in rounding
        public const uint HALF_BASE = BASE / 2;
        // BASE divided by 10
        public const uint BASE_DIV_10 = BASE / 10;
        #endregion

        #region Special Values
        public static readonly Fraction Zero = new Fraction(new uint[] { 0 });
        public static readonly Fraction One = new Fraction(new uint[] { BASE_DIV_10 });
        public static readonly Fraction Five = new Fraction(new uint[] { 5 * BASE_DIV_10 });
        #endregion

        #region Private Fields
        // The digits of the fraction held in words.
        // Each word holds BASE_FIG decimal digits.
        // The words are stored in most significant word first
        // e.g 0.1234567890123456789 will be stored as new uint[] { 123456789, 012345678, 900000000 }
        private uint[] _words;
        // The current number of elements in use in the _words array
        private int _precision;
        private readonly static uint[] _powers;
        static Fraction() {
            // Generate the powers array - should look like [BASE/10, BASE/100, ... , 1]
            _powers = new uint[BASE_FIG];
            int i = 0;
            for (uint p = BASE/10; p > 0; p /= 10 ) {
                _powers[i] = p;
                ++i;
            }
        }
        #endregion

        #region Creation
        public Fraction(uint[] data) {
            this._words = data;
            this.Precision = MaxPrecision;
        }

        public Fraction(int maxPrecision)
            : this(maxPrecision, maxPrecision) {
        }

        public Fraction(Fraction/*!*/ copyFrom, int maxPrecision)
            : this(maxPrecision, maxPrecision) {
            // Copy the fraction
            int length = Math.Min(maxPrecision, copyFrom.MaxPrecision);
            Array.Copy(copyFrom._words, _words, length);
        }

        public Fraction(int maxPrecision, int precision) {
            if (maxPrecision <= 0) {
                throw new ArgumentException("maxPrecision must be greater than zero");
            }
            _words = new uint[maxPrecision];
            Precision = precision;
        }
        public static Fraction/*!*/ Create(string/*!*/ digits) {
            int digitOffset;
            return Parse(digits, out digitOffset);
        }
        #endregion

        #region Public Properties
        public int Precision {
            get { return _precision; }
            set {
                if (value < 1 || value > _words.Length) {
                    throw new ArgumentException("Precision must be in [1,MaxPrecision]");
                }
                _precision = value;
            }
        }
        public int MaxPrecision { get { return _words.Length; } }

        public int DigitCount {
            get {
                // Find last non-zero word
                int wordIndex = Precision-1;
                while (wordIndex > 0 && _words[wordIndex] == 0) {
                    --wordIndex;
                }

                // Estimate digit count
                int digits = (wordIndex+1) * BASE_FIG;

                // Subtract trailing zeros count from digits in last word
                uint lastWord = _words[wordIndex];
                uint factor = 10;
                while (factor <= BASE && (lastWord % factor) == 0) {
                    --digits;
                    factor = factor * 10;
                }
                return digits;
            }
        }

        public bool IsOne {
            get { return Precision == 1 && _words[0] == BASE_DIV_10; }
        }
        public bool IsZero {
            get { return Precision == 1 && _words[0] == 0; }
        }
        #endregion

        #region Public Methods
        public override string ToString() {
            StringBuilder sb = new StringBuilder(Precision * BASE_FIG);
            for (int wordIndex = 0; wordIndex < Precision; wordIndex++) {
                uint word = _words[wordIndex];
                sb.AppendFormat("{0:D"+BASE_FIG+"}", word);
            }
            // Trim off last zeros
            string str = sb.ToString().TrimEnd('0');

            // Special case for zero
            if (str.Length == 0) {
                str = "0";
            }
 
            return str;
        }

        public override int GetHashCode() {
            int hash = 0;
            for(int i=0; i<Precision; ++i) {
                int word = (int)_words[i];
                hash = (31 * hash + word) ^ word;
            }
            return hash;
        }

        public static Fraction/*!*/ Add(Fraction/*!*/ x, Fraction/*!*/y, int exponentDiff, out int exponentOffset) {
            Fraction upper = x;
            Fraction lower = y;

            // Switch x and y around if the exponentDiff is negative
            bool swap = exponentDiff < 0;
            if (swap) {
                exponentDiff = -exponentDiff;
                upper = y;
                lower = x;
            }

            // Calculate the word and digit offsets between upper and lower
            int wordOffset = exponentDiff / BASE_FIG;
            int digitOffset = exponentDiff % BASE_FIG;

            // If necessary we need to scroll one of the arrays
            if (digitOffset != 0) {
                lower = ScrollRight(lower, digitOffset);
            }

            int upperStart = 0;
            int lowerStart = wordOffset;

            // We should now have something like:
            // UUU UUU UU0 000 000 000 (upperStart=0, upperLength=8)
            // 000 0LL LLL LLL LLL LL0 (lowerStart=4, lowerLength=13)
            // assuming that exponentDiff is 4 (or -4) and BASE_FIG is 3
            // where each character above is a decimal digit and a space indicates a word boundary

            int zPrecision = Math.Max(upper.Precision, lower.Precision+wordOffset) + 1;
            Fraction z = new Fraction(zPrecision);

            uint[] upperWords = upper._words;
            uint[] lowerWords = lower._words;
            uint[] zWords = z._words;

            // Copy words of lower straight into z
            Array.Copy(lowerWords, 0, zWords, lowerStart+1, lower.Precision);

            // Add words of upper into z, carrying as necessary
            ulong carry = 0;
            for (int i = upper.Precision - 1; i >= upperStart; i--) {
                carry += upperWords[i] + zWords[i+1];
                zWords[i+1] = (uint)(carry % BASE);
                carry /= BASE;
            }
            Debug.Assert(carry / BASE == 0);
            zWords[0] = (uint)(carry % BASE);

            // We expect there to be BASE_FIG offset when normalizing unless
            // the carry overflowed into the top word.
            z = Fraction.Normalize(z, out exponentOffset);
            exponentOffset += BASE_FIG;
            return z;
        }

        public static Fraction/*!*/ Subtract(Fraction/*!*/ x, Fraction/*!*/ y, int exponentDiff, out int exponentOffset, out int sign) {
            Fraction upper = x;
            Fraction lower = y;

            sign = Compare(x, y, exponentDiff);
            if (sign== 0) {
                exponentOffset = 0;
                return new Fraction(1);
            } else if (sign < 0) {
                exponentDiff = -exponentDiff;
                upper = y;
                lower = x;
            }

            // Calculate the word and digit offsets between upper and lower
            int wordOffset = exponentDiff / BASE_FIG;
            int digitOffset = exponentDiff % BASE_FIG;

            // If necessary we need to scroll one of the arrays
            if (digitOffset != 0) {
                lower = ScrollRight(lower, digitOffset);
            }

            int lowerStart = wordOffset;

            // We should now have something like:
            // UUU UUU UU0 000 000 000 (upperStart=0, upperLength=8)
            // 000 0LL LLL LLL LLL LL0 (lowerStart=4, lowerLength=13)
            // assuming that exponentDiff is 4 (or -4) and BASE_FIG is 3
            // where each character above is a decimal digit and a space indicates a word boundary
            // Also, upper should be larger than lower

            int zPrecision = Math.Max(upper.Precision, lower.Precision + lowerStart);
            Fraction z = new Fraction(zPrecision);

            uint[] upperWords = upper._words;
            uint[] lowerWords = lower._words;
            uint[] zWords = z._words;

            // Copy words of upper straight into z
            Array.Copy(upperWords, 0, zWords, 0, upper.Precision);

            //// Subtract words of lower from z, borrowing as necessary
            SubtractInPlace(zWords, lowerWords, lower.Precision, lowerStart);

            z = Fraction.Normalize(z, out exponentOffset);
            return z;
        }

        public static Fraction/*!*/ Multiply(Fraction/*!*/ x, Fraction/*!*/ y, out int offset) {
            int xPrecision = x.Precision;
            int yPrecision = y.Precision;
            int zPrecision = xPrecision + yPrecision;
            uint[] xData = x._words;
            uint[] yData = y._words;
            uint[] zData = new uint[zPrecision];
            Fraction z = new Fraction(zData);

            for (int xIndex = xPrecision-1; xIndex >= 0; xIndex--) {
                uint xValue = xData[xIndex];
                int zIndex = zPrecision - (xPrecision - xIndex);
                ulong carry = 0;
                for (int yIndex = yPrecision-1; yIndex >= 0; yIndex--) {
                    carry = carry + ((ulong)xValue) * yData[yIndex] + zData[zIndex];
                    zData[zIndex--] = (uint)(carry%BASE);
                    carry /= BASE;
                }
                while (carry != 0) {
                    carry += zData[zIndex];
                    zData[zIndex--] = (uint)carry;
                    carry /= BASE;
                }
            }
            z = Fraction.Normalize(z, out offset);
            return z;
        }

        public static Fraction/*!*/ Divide(Fraction/*!*/ a, Fraction/*!*/ b, int minPrecision, out Fraction/*!*/ r, out int cOffset, out int rOffset) {
            int precision = Math.Max(a.MaxPrecision + b.MaxPrecision+1, minPrecision);
            Fraction c = new Fraction(precision);
            r = new Fraction(precision * 2);

            uint[] aWords = a._words;
            uint[] bWords = b._words;
            uint[] cWords = c._words;
            uint[] rWords = r._words;

            int aSize = a.Precision;
            int bSize = b.Precision;
            int cSize = c.MaxPrecision;
            int rSize = r.MaxPrecision;

            // Initialize remainder (we add an extra word at the front to catch the overflow
            Array.Copy(aWords, 0, rWords, 1, Math.Min(aSize, rSize-1));

            // Setup basic values
            ulong b1        = bWords[0];
            ulong b1plus1   = bSize <= 1 ? b1               : b1 + 1;
            ulong b1b2      = bSize <= 1 ? (ulong)bWords[0] * BASE : GetDoubleWord(bWords, 0, b.Precision);
            ulong b1b2plus1 = bSize <= 2 ? b1b2             : b1b2 + 1;

            // Main loop
            int index = 1;
            int size = Math.Min(cSize, rSize);
            while (index < size) {
                if (rWords[index] == 0) {
                    ++index;
                    continue;
                }

                ulong r1r2 = GetDoubleWord(rWords, index, rSize);
                if (r1r2 == b1b2) {
                    // Iterate through the rest of b comparing words of r and b until they are not equal
                    int bIndex = 2;
                    int rIndex = index + 2;
                    FindNextNonEqual(rWords, bWords, bSize, ref rIndex, ref bIndex);
                    if (rIndex < rSize && bIndex < bSize && rWords[rIndex] < bWords[bIndex]) {
                        if (index + 1 > rSize) {
                            break;
                        }
                        InternalDivide(rWords, rSize, r1r2 / b1plus1, bWords, bSize, index, cWords);
                    } else {
                        // Quotient is 1, just subtract b from r
                        SubtractInPlace(rWords, bWords, bSize, index);
                        cWords[index-1]++;
                        Carry(cWords, index);
                    }
                } else if (r1r2 >= b1b2plus1) {
                    InternalDivide(rWords, rSize, r1r2 / b1b2plus1, bWords, bSize, index - 1, cWords);
                } else {
                    InternalDivide(rWords, rSize, r1r2 / b1plus1, bWords, bSize, index, cWords);
                }
            }
            c = Normalize(c, out cOffset);
            r = Normalize(r, out rOffset);
            // We added artificially an extra word onto r and c to cope with carry overflow (so take away again now)
            cOffset += BASE_FIG;
            rOffset += BASE_FIG;
            return c;
        }

        public static int Compare(Fraction/*!*/ x, Fraction/*!*/ y, int exponentDiff) {
            if (exponentDiff != 0) {
                return exponentDiff > 0 ? 1 : -1;
            }
            int wordIndex = 0;
            while (wordIndex < x.Precision && wordIndex < y.Precision) {
                if (x._words[wordIndex] != y._words[wordIndex]) {
                    return x._words[wordIndex] > y._words[wordIndex] ? 1 : -1;
                }
                wordIndex++;
            }
            if (wordIndex == x.Precision) {
                while (wordIndex < y.Precision) {
                    if (y._words[wordIndex] != 0) {
                        return -1;
                    }
                    wordIndex++;
                }
            } else {
                while (wordIndex < x.Precision) {
                    if (x._words[wordIndex] != 0) {
                        return 1;
                    }
                    wordIndex++;
                }
            }
            return 0;
        }

#if SILVERLIGHT || WIN8 || WP75
        public static int DivRem(int a, int b, out int result) {
            result = a % b;
            return (a / b);
        }
#endif

        /// <summary>
        /// Limits the precision of the given Fraction.
        /// </summary>
        /// <param name="sign">The sign of the BigDecimal</param>
        /// <param name="fraction">The fraction to limit</param>
        /// <param name="digits">The number of digits to which we are limiting</param>
        /// <param name="mode">The rounding mode to use when limiting</param>
        /// <returns>A new fraction that has no more than <paramref name="digits"/> digits.</returns>
        /// <example>
        /// Consider a fraction of 123456789 using default HalfUp rounding.
        /// Limit : Result
        /// 1       1
        /// 2       12
        /// 3       123
        /// 4       1234
        /// 5       12346
        /// 6       123457
        /// 7       1234568
        /// 8       12345679
        /// 9       123456789
        /// 10      123456789
        /// </example>
        public static Fraction/*!*/ LimitPrecision(int sign, Fraction/*!*/ fraction, int digits, BigDecimal.RoundingModes mode, out int offset) {
            Fraction result;

            if (digits <= 0) {
                uint digit = (uint)(fraction.IsZero ? 0 : 1);
                if (RoundDigit(sign, digit, 0, mode) > 0) {
                    offset = 1-digits;
                    return One;
                } else {
                    offset = 0;
                    return Zero;
                }
            }

            if (digits >= fraction.DigitCount) {
                offset = 0;
                return fraction;
            }

            // Calculate offsets of relevant digits
            int secondLastDigitIndex; // i.e. fraction[digits-1]
            int secondLastWordIndex;
            uint secondLastDigit;
            int lastDigitIndex; // i.e. fraction[digits]
            int lastWordIndex;
            uint lastDigit;

#if SILVERLIGHT || WIN8 || WP75
            secondLastWordIndex = DivRem(digits - 1, BASE_FIG, out secondLastDigitIndex);
#else
            secondLastWordIndex = Math.DivRem(digits - 1, BASE_FIG, out secondLastDigitIndex);
#endif
            if (secondLastDigitIndex == BASE_FIG-1) {
                lastWordIndex = secondLastWordIndex+1;
                lastDigitIndex = 0;
            } else {
                lastWordIndex = secondLastWordIndex;
                lastDigitIndex = secondLastDigitIndex + 1;
            }

            // TODO: Extract these calculations out into static readonly arrays
            // Mask for last digit.  E.g. lastDigitIndex = 3, BASE_FIG = 9 => lastFactor = 1000000
            uint lastFactor=_powers[lastDigitIndex];
            // Mask for second last digit
            uint secondLastFactor= _powers[secondLastDigitIndex];

            // Calculate the current digits and rounding direction
            secondLastDigit = (fraction._words[secondLastWordIndex] / secondLastFactor) % 10;
            if (lastWordIndex < fraction.MaxPrecision) {
                lastDigit = (fraction._words[lastWordIndex] / lastFactor) % 10;
            } else {
                lastDigit = 0;
            }
            int round = RoundDigit(sign, lastDigit, secondLastDigit, mode);

            // Create a temporary fraction used to cause the rounding in the original
            result = new Fraction(lastWordIndex+1);
            Array.Copy(fraction._words, 0, result._words, 0, Math.Min(lastWordIndex+1, fraction.Precision));
            // Clear the digits beyond the second last digit
            result._words[secondLastWordIndex] = result._words[secondLastWordIndex] - (fraction._words[secondLastWordIndex] % secondLastFactor);
            if (round > 0) {
                // Increment the last digit of result by 1
                Fraction temp = new Fraction(secondLastWordIndex + 1);
                temp._words[secondLastWordIndex] = secondLastFactor;
                result = Fraction.Add(result, temp, 0, out offset);
            } else {
                offset = 0;
            }

            result.Precision = Math.Min(secondLastWordIndex+1, result.MaxPrecision);
            return result;
        }
        #endregion

        #region Private Helpers
        private static int RoundDigit(int sign, uint lastDigit, uint secondLastDigit, BigDecimal.RoundingModes roundingMode) {
            int result = -1;
            switch (roundingMode) {
                case BigDecimal.RoundingModes.Up:
                    if (lastDigit != 0) {
                        result = 1;
                    }
                    break;
                case BigDecimal.RoundingModes.HalfUp:
                    if (lastDigit >= 5) {
                        result = 1;
                    }
                    break;
                case BigDecimal.RoundingModes.HalfDown:
                    if (lastDigit > 5) {
                        result = 1;
                    }
                    break;
                case BigDecimal.RoundingModes.HalfEven:
                    if (lastDigit > 5) {
                        result = 1;
                    } else if ((lastDigit == 5) && (secondLastDigit % 2 != 0)) {
                        result = 1;
                    }
                    break;
                case BigDecimal.RoundingModes.Ceiling:
                    if (sign == 1 && lastDigit != 0) {
                        result = 1;
                    }
                    break;
                case BigDecimal.RoundingModes.Floor:
                    if (sign == -1 && lastDigit != 0) {
                        result = 1;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        /// Divide r[index, ...] by q
        /// </summary>
        private static void InternalDivide(uint[] rWords, int rSize, ulong q, uint[] bWords, int bSize, int index, uint[] cWords) {
            cWords[index] += (uint)q;
            SubtractMultiple(rWords, rSize, q, bWords, bSize, index, bSize+index);
        }

        // r = r - q * b
        private static void SubtractMultiple(uint[] rWords, int rSize, ulong q, uint[] bWords, int bSize, int index, int rIndex) {
            int bIndex = bSize - 1;
            uint borrow1 = 0;
            uint borrow2 = 0;

            while (bIndex >= 0) {
                ulong qb = q * bWords[bIndex];
                if (qb < BASE) {
                    borrow1 = 0;
                } else {
                    borrow1 = (uint)(qb / BASE);
                    qb -= (ulong)borrow1 * BASE;
                }

                if (rWords[rIndex] < qb) {
                    rWords[rIndex] += (uint)(BASE - qb);
                    borrow2 += borrow1 + 1;
                } else{ 
				    rWords[rIndex] -= (uint)qb;
                    borrow2 += borrow1;
                }

                if (borrow2 != 0) {
                    if (rWords[rIndex - 1] < borrow2) {
                        rWords[rIndex - 1] += (BASE - borrow2);
                        borrow2 = 1;
                    } else {
                        rWords[rIndex - 1] -= borrow2;
                        borrow2 = 0;
                    }
                }

                --rIndex;
                --bIndex;
            }
        }

        private static void FindNextNonEqual(uint[] i, uint[] j, int iSize, ref int iIndex, ref int jIndex) {
            while (iIndex < iSize) {
                if (i[iIndex] != j[jIndex]) {
                    break;
                }
                ++iIndex;
                ++jIndex;
            }
        }

        /// <summary>
        /// Subtract y from x in place.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void SubtractInPlace(uint[] x, uint[] y, int count, int offset) {
            uint borrow = 0;
            int yIndex = count - 1;
            int xIndex = offset + yIndex;
			while(yIndex >= 0) {
				if(x[xIndex] < y[yIndex] + borrow) {
					x[xIndex] = x[xIndex] + (BASE -(y[yIndex] + borrow));
					borrow = 1;
                } else {
                    x[xIndex] = x[xIndex] - (y[yIndex] + borrow);
					borrow = 0;
                }
				--yIndex;
				--xIndex;
            }
            // Continue to borrow as necessary
            while (borrow != 0) {
				if(x[xIndex] < borrow) {
					x[xIndex] = x[xIndex] + (BASE - borrow);
					borrow = 1;
                } else {
                    x[xIndex] = x[xIndex] - borrow;
					borrow = 0;
                }
				--xIndex;
            }
        }

        private static ulong GetDoubleWord(uint[] a, int i, int precision) {
            if (i+1 == precision) {
                return (ulong)a[i] * BASE;
            } else {
                return (ulong)a[i] * BASE + a[i + 1];
            }
        }

        private static void Carry(uint[] a, int i) {
            while (a[i] >= BASE) {
                a[i] -= BASE;
                --i;
                ++a[i];
            }
        }

        // Note: Scrolling left is just the same as scrolling right by (BASE_FIG-offset)
        private static Fraction/*!*/ ScrollRight(Fraction/*!*/ fraction, int offset) {
            Debug.Assert(offset <= BASE_FIG);
            Debug.Assert(offset >= 0);

            // Don't do anything if offset is not going to change the internal digit layout
            if (offset % BASE_FIG == 0) {
                return fraction;
            }

            int oldPrecision = fraction.Precision;
            int newPrecision = oldPrecision + 1;

            Fraction newFraction = new Fraction(newPrecision);

            // Calculate masking values
            // divisor is used to mask off and bring top digits down to bottom digits (TTTTTTBBBB / divisor == TTTTTT)
            // also divisor is used to mask off the bottom digits (TTTTTTBBBB % divisor == BBBB)
            // factor is then used to bring bottom digits up to top digits (BBBB * factor == BBBB000000)
            uint downFactor = 1;
            for (int i = 0; i < offset; ++i ) {
                downFactor *= 10;
            }
            uint upFactor = BASE / downFactor;

            // Scroll the digits
            // We want to go from TTTTTTBBBB TTTTTTBBBB TTTTTTBBBB to 0000TTTTTT BBBBTTTTTT BBBBTTTTTT BBBB000000
            // I.E. We are pushing all the digits to the right by "offset" amount and padding with zeros
            uint topDigits = 0;
            uint bottomDigits = 0;
            int wordIndex = 0;
            while(wordIndex < oldPrecision) {
                topDigits = fraction._words[wordIndex] / downFactor;
                newFraction._words[wordIndex] = bottomDigits + topDigits;
                bottomDigits = (fraction._words[wordIndex] % downFactor) * upFactor;
                wordIndex++;
            }

            // Fix up the last word
            newFraction._words[wordIndex] = bottomDigits;

            return newFraction;
        }

         private static Fraction/*!*/ Parse(string/*!*/ digits, out int digitOffset) {
            // Trim off any trailing zeros
            digits = digits.TrimEnd('0');

            if (digits == "") {
                digitOffset = 0;
                return Zero;
            }

            // Create the new fraction
            int precision = digits.Length / BASE_FIG;
            int finalDigits = digits.Length % BASE_FIG;
            if (finalDigits > 0) { ++precision; }
            Fraction/*!*/ fraction = new Fraction(precision);

            // Iterate through groups of BASE_FIG digits
            int digitIndex;
            int wordIndex = 0;
            for (digitIndex = 0; digitIndex+BASE_FIG <= digits.Length; digitIndex+=BASE_FIG) {
                fraction._words[wordIndex] = uint.Parse(digits.Substring(digitIndex, BASE_FIG), CultureInfo.InvariantCulture);
                wordIndex++;
            }

            // Add on the last few digits, adding extra zeros as necessary
            if (finalDigits > 0) {
                uint lastWord = uint.Parse(digits.Substring(digitIndex), CultureInfo.InvariantCulture);
                while (finalDigits < BASE_FIG) {
                    lastWord *= 10;
                    ++finalDigits;
                }
                fraction._words[wordIndex] = lastWord;
            }

            fraction = Fraction.Normalize(fraction, out digitOffset);
            return fraction;
        }

        private static Fraction/*!*/ Normalize(Fraction/*!*/ f, out int digitOffset) {
            // Count leading zero words
            int leadingZeroWords = 0;
            while (leadingZeroWords < f.MaxPrecision && f._words[leadingZeroWords] == 0) {
                leadingZeroWords++;
            }
            
            if (leadingZeroWords == f.MaxPrecision) {
                // The fraction is just all zeros
                digitOffset = 0;
                return f;
            }

            // Count trailing zero words
            int trailingZeroWords = 0;
            while (f._words[f.Precision - 1 - trailingZeroWords] == 0) {
                trailingZeroWords++;
            }
            // Count leading zero digits (within the first non-zero word)
            // and also calculate up and down factors for masking
            int leadingZeroDigits = BASE_FIG;
            uint firstWord = f._words[leadingZeroWords];
            uint downFactor = 1;
            while (firstWord != 0) {
                firstWord /= 10;
                leadingZeroDigits--;
                downFactor *= 10;
            }
            uint upFactor = BASE / downFactor;

            int newPrecision = f.Precision - leadingZeroWords - trailingZeroWords;
            Fraction n = new Fraction(newPrecision);
            // Copy the non-zero words across
            Array.Copy(f._words, leadingZeroWords, n._words, 0, n.MaxPrecision);

            if (leadingZeroDigits > 0) {
                // Scroll the digits within the non-zero words to trim off the first zero digits
                uint bottomDigits = n._words[0] * upFactor;
                uint topDigits = 0;
                int wordIndex = 1;
                while (wordIndex < n.Precision) {
                    topDigits = n._words[wordIndex] / downFactor;
                    n._words[wordIndex - 1] = bottomDigits + topDigits;
                    bottomDigits = (n._words[wordIndex] % downFactor) * upFactor;
                    wordIndex++;
                }

                // Fix up the last word
                n._words[wordIndex-1] = bottomDigits;
                // and the Precision
                n.Precision = wordIndex;
            }


            // Return the offset in digits caused by the normalization.
            digitOffset = -(leadingZeroWords * BASE_FIG + leadingZeroDigits);
            return n;
        }

       #endregion
    }
}