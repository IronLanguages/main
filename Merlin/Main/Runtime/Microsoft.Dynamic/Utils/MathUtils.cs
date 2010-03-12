/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !CLR2
using BigInt = System.Numerics.BigInteger;
using Complex = System.Numerics.Complex;
#endif

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Utils {
    using Math = System.Math;

    public static class MathUtils {
        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int32.MinValue, y = -1)</remarks>
        public static int FloorDivideUnchecked(int x, int y) {
            int q = x / y;

            if (x >= 0) {
                if (y > 0) {
                    return q;
                } else if (x % y == 0) {
                    return q;
                } else {
                    return q - 1;
                }
            } else {
                if (y > 0) {
                    if (x % y == 0) {
                        return q;
                    } else {
                        return q - 1;
                    }
                } else {
                    return q;
                }
            }
        }

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int64.MinValue, y = -1)</remarks>
        public static long FloorDivideUnchecked(long x, long y) {
            long q = x / y;

            if (x >= 0) {
                if (y > 0) {
                    return q;
                } else if (x % y == 0) {
                    return q;
                } else {
                    return q - 1;
                }
            } else {
                if (y > 0) {
                    if (x % y == 0) {
                        return q;
                    } else {
                        return q - 1;
                    }
                } else {
                    return q;
                }
            }
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        public static int FloorRemainder(int x, int y) {
            if (y == -1) return 0;
            int r = x % y;

            if (x >= 0) {
                if (y > 0) {
                    return r;
                } else if (r == 0) {
                    return 0;
                } else {
                    return r + y;
                }
            } else {
                if (y > 0) {
                    if (r == 0) {
                        return 0;
                    } else {
                        return r + y;
                    }
                } else {
                    return r;
                }
            }
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="DivideByZeroException"><paramref name="y"/> is 0.</exception>
        public static long FloorRemainder(long x, long y) {
            if (y == -1) return 0;
            long r = x % y;

            if (x >= 0) {
                if (y > 0) {
                    return r;
                } else if (r == 0) {
                    return 0;
                } else {
                    return r + y;
                }
            } else {
                if (y > 0) {
                    if (r == 0) {
                        return 0;
                    } else {
                        return r + y;
                    }
                } else {
                    return r;
                }
            }
        }

        /// <summary>
        /// Behaves like Math.Round(value, MidpointRounding.AwayFromZero)
        /// Needed because CoreCLR doesn't support this particular overload of Math.Round
        /// </summary>
        public static double RoundAwayFromZero(double value) {
#if !SILVERLIGHT
            return Math.Round(value, MidpointRounding.AwayFromZero);
#else
            if (value < 0) {
                return -RoundAwayFromZero(-value);
            }
        
            // we can assume positive value
            double result = Math.Floor(value);
            if (value - result >= 0.5) {
                result += 1.0;
            }
            return result;
#endif
        }

        private static readonly double[] _RoundPowersOfTens = new double[] { 1E0, 1E1, 1E2, 1E3, 1E4, 1E5, 1E6, 1E7, 1E8, 1E9, 1E10, 1E11, 1E12, 1E13, 1E14, 1E15 };

        private static double GetPowerOf10(int precision) {
            return (precision < 16) ? _RoundPowersOfTens[precision] : Math.Pow(10, precision);
        }

        /// <summary>
        /// Behaves like Math.Round(value, precision, MidpointRounding.AwayFromZero)
        /// However, it works correctly on negative precisions and cases where precision is
        /// outside of the [-15, 15] range.
        /// 
        /// (This function is also needed because CoreCLR lacks this overload.)
        /// </summary>
        public static double RoundAwayFromZero(double value, int precision) {
            if (precision >= 0) {
                double num = GetPowerOf10(precision);
                return RoundAwayFromZero(value * num) / num;
            } else {
                // Note: this code path could be merged with the precision >= 0 path,
                // (by extending the cache to negative powers of 10)
                // but the results seem to be more precise if we do it this way
                double num = GetPowerOf10(-precision);
                return RoundAwayFromZero(value / num) * num;
            }
        }

        public static bool IsNegativeZero(double self) {
#if SILVERLIGHT // BitConverter.DoubleToInt64Bits
            if ( self != 0.0 ) {
              return false;
            }
            byte[] bits = BitConverter.GetBytes(self);
            return (bits[7] == 0x80 && bits[6] == 0x00 && bits[5] == 0x00 && bits[4] == 0x00
                && bits[3] == 0x00 && bits[2] == 0x00 && bits[1] == 0x00 && bits[0] == 0x00);
#else
            return (self == 0.0 && 1.0 / self < 0);
#endif
        }

        public static double Hypot(double x, double y) {
            //
            // sqrt(x*x + y*y) == sqrt(x*x * (1 + (y*y)/(x*x))) ==
            // sqrt(x*x) * sqrt(1 + (y/x)*(y/x)) ==
            // abs(x) * sqrt(1 + (y/x)*(y/x))
            //

            // Handle infinities
            if (double.IsInfinity(x) || double.IsInfinity(y)) {
                return double.PositiveInfinity;
            }

            //  First, get abs
            if (x < 0.0) x = -x;
            if (y < 0.0) y = -y;

            // Obvious cases
            if (x == 0.0) return y;
            if (y == 0.0) return x;

            // Divide smaller number by bigger number to safeguard the (y/x)*(y/x)
            if (x < y) {
                double temp = y; y = x; x = temp;
            }

            y /= x;

            // calculate abs(x) * sqrt(1 + (y/x)*(y/x))
            return x * System.Math.Sqrt(1 + y * y);
        }

        #region BigInteger

        // generated by scripts/radix_generator.py
        private static readonly uint[] maxCharsPerDigit = { 0, 0, 31, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
        private static readonly uint[] groupRadixValues = { 0, 0, 2147483648, 3486784401, 1073741824, 1220703125, 2176782336, 1977326743, 1073741824, 3486784401, 1000000000, 2357947691, 429981696, 815730721, 1475789056, 2562890625, 268435456, 410338673, 612220032, 893871739, 1280000000, 1801088541, 2494357888, 3404825447, 191102976, 244140625, 308915776, 387420489, 481890304, 594823321, 729000000, 887503681, 1073741824, 1291467969, 1544804416, 1838265625, 2176782336 };

        internal static string BigIntegerToString(uint[] d, int sign, int radix) {
            if (radix < 2) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            }
            if (radix > 36) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be <= 36");
            }

            int dl = d.Length;
            if (dl == 0) {
                return "0";
            }

            List<uint> digitGroups = new List<uint>();

            uint groupRadix = groupRadixValues[radix];
            while (dl > 0) {
                uint rem = div(d, ref dl, groupRadix);
                digitGroups.Add(rem);
            }

            StringBuilder ret = new StringBuilder();
            if (sign == -1) {
                ret.Append("-");
            }

            int digitIndex = digitGroups.Count - 1;

            char[] tmpDigits = new char[maxCharsPerDigit[radix]];

            AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, false);
            while (digitIndex >= 0) {
                AppendRadix((uint)digitGroups[digitIndex--], (uint)radix, tmpDigits, ret, true);
            }
            return ret.Length == 0 ? "0" : ret.ToString();
        }

        private const int BitsPerDigit = 32;

        private static uint div(uint[] n, ref int nl, uint d) {
            ulong rem = 0;
            int i = nl;
            bool seenNonZero = false;
            while (--i >= 0) {
                rem <<= BitsPerDigit;
                rem |= n[i];
                uint v = (uint)(rem / d);
                n[i] = v;
                if (v == 0) {
                    if (!seenNonZero) nl--;
                } else {
                    seenNonZero = true;
                }
                rem %= d;
            }
            return (uint)rem;
        }

        private static void AppendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros) {
            const string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            int digits = tmp.Length;
            int i = digits;
            while (i > 0 && (leadingZeros || rem != 0)) {
                uint digit = rem % radix;
                rem /= radix;
                tmp[--i] = symbols[(int)digit];
            }
            if (leadingZeros) buf.Append(tmp);
            else buf.Append(tmp, i, digits - i);
        }

        // Helper for GetRandBits
        private static uint GetWord(byte[] bytes, int start, int end) {
            uint four = 0;
            int bits = end - start;
            int shift = 0;
            if (bits > 32) {
                bits = 32;
            }
            start /= 8;
            while (bits > 0) {
                uint value = bytes[start];
                if (bits < 8) {
                    value &= (1u << bits) - 1u;
                }
                value <<= shift;
                four |= value;
                bits -= 8;
                shift += 8;
                start++;
            }

            return four;
        }

#if CLR2
        public static BigInteger GetRandBits(this Random generator, int bits) {
            ContractUtils.Requires(bits > 0);

            // equivalent to (bits + 7) / 8 without possibility of overflow
            int count = bits % 8 == 0 ? bits / 8 : bits / 8 + 1;

            // Pad the end (most significant) with zeros if we align to the byte
            // to ensure that we end up with a positive value
            byte[] bytes = new byte[bits % 8 == 0 ? count + 1 : count];
            generator.NextBytes(bytes);
            if (bits % 8 == 0) {
                bytes[bytes.Length - 1] = 0;
            } else {
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] & ((1 << (bits % 8)) - 1));
            }

            if (bits <= 32) {
                return (BigInteger)GetWord(bytes, 0, bits);
            } else if (bits <= 64) {
                ulong a = GetWord(bytes, 0, bits);
                ulong b = GetWord(bytes, 32, bits);
                return (BigInteger)(a | (b << 32));
            } else {
                count = (count + 3) / 4;
                uint[] data = new uint[count];
                for (int i = 0; i < count; i++) {
                    data[i] = GetWord(bytes, i * 32, bits);
                }
                return new BigInteger(1, data);
            }
        }

        public static BigInteger Random(this Random generator, BigInteger limit) {
            ContractUtils.Requires(limit.Sign > 0, "limit");
            ContractUtils.RequiresNotNull(generator, "generator");

            // TODO: this doesn't yield a uniform distribution (small numbers will be picked more frequently):
            uint[] result = new uint[limit.GetWordCount() + 1];
            for (int i = 0; i < result.Length; i++) {
                result[i] = unchecked((uint)generator.Next());
            }
            return new BigInteger(1, result) % limit;
        }
#else
        public static BigInt GetRandBits(this Random generator, int bits) {
            ContractUtils.Requires(bits > 0);

            // equivalent to (bits + 7) / 8 without possibility of overflow
            int count = bits % 8 == 0 ? bits / 8 : bits / 8 + 1;

            // Pad the end (most significant) with zeros if we align to the byte
            // to ensure that we end up with a positive value
            byte[] bytes = new byte[bits % 8 == 0 ? count + 1 : count];
            generator.NextBytes(bytes);
            if (bits % 8 == 0) {
                bytes[bytes.Length - 1] = 0;
            } else {
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] & ((1 << (bits % 8)) - 1));
            }

            if (bits <= 32) {
                return (BigInt)GetWord(bytes, 0, bits);
            } else if (bits <= 64) {
                ulong a = GetWord(bytes, 0, bits);
                ulong b = GetWord(bytes, 32, bits);
                return (BigInt)(a | (b << 32));
            }
            
            return new BigInt(bytes);
        }

        public static BigInteger Random(this Random generator, BigInteger limit) {
            return new BigInteger(generator.Random(limit.Value));
        }

        public static BigInt Random(this Random generator, BigInt limit) {
            ContractUtils.Requires(limit.Sign > 0, "limit");
            ContractUtils.RequiresNotNull(generator, "generator");

            BigInt res = BigInt.Zero;

            while (true) {
                // if we've run out of significant digits, we can return the total
                if (limit == BigInt.Zero) {
                    return res;
                }

                // if we're small enough to fit in an int, do so
                int iLimit;
                if (limit.AsInt32(out iLimit)) {
                    return res + generator.Next(iLimit);
                }

                // get the 3 or 4 uppermost bytes that fit into an int
                int hiData;
                byte[] data = limit.ToByteArray();
                int index = data.Length;
                while (data[--index] == 0) ;
                if (data[index] < 0x80) {
                    hiData = data[index] << 24;
                    data[index--] = (byte)0;
                } else {
                    hiData = 0;
                }
                hiData |= data[index] << 16;
                data[index--] = (byte)0;
                hiData |= data[index] << 8;
                data[index--] = (byte)0;
                hiData |= data[index];
                data[index--] = (byte)0;

                // get a uniform random number for the uppermost portion of the bigint
                byte[] randomData = new byte[index + 2];
                generator.NextBytes(randomData);
                randomData[index + 1] = (byte)0;
                res += new BigInt(randomData);
                res += (BigInt)generator.Next(hiData) << ((index + 1) * 8);

                // sum it with a uniform random number for the remainder of the bigint
                limit = new BigInt(data);
            }
        }

        public static bool TryToFloat64(this BigInt self, out double result) {
            return StringUtils.TryParseDouble(
                self.ToString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result
            );
        }

        public static double ToFloat64(this BigInt self) {
            return double.Parse(
                self.ToString(),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
            );
        }
#endif

        public static bool TryToFloat64(this BigInteger self, out double result) {
            return StringUtils.TryParseDouble(
                self.ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                out result
            );
        }

        public static double ToFloat64(this BigInteger self) {
            return double.Parse(
                self.ToString(10),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat
            );
        }

        #region Extending BigInt with BigInteger API
#if !CLR2

        public static bool AsInt32(this BigInt self, out int ret) {
            if (self >= Int32.MinValue && self <= Int32.MaxValue) {
                ret = (Int32)self;
                return true;
            }
            ret = 0;
            return false;
        }

        public static bool AsInt64(this BigInt self, out long ret) {
            if (self >= Int64.MinValue && self <= Int64.MaxValue) {
                ret = (long)self;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt32(this BigInt self, out uint ret) {
            if (self >= UInt32.MinValue && self <= UInt32.MaxValue) {
                ret = (UInt32)self;
                return true;
            }
            ret = 0;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt64(this BigInt self, out ulong ret) {
            if (self >= UInt64.MinValue && self <= UInt64.MaxValue) {
                ret = (UInt64)self;
                return true;
            }
            ret = 0;
            return false;
        }

        public static BigInt Abs(this BigInt self) {
            return BigInt.Abs(self);
        }

        public static bool IsZero(this BigInt self) {
            return self.IsZero;
        }

        public static bool IsPositive(this BigInt self) {
            return self.Sign > 0;
        }

        public static bool IsNegative(this BigInt self) {
            return self.Sign < 0;
        }

        public static double Log(this BigInt self) {
            return BigInt.Log(self);
        }

        public static double Log(this BigInt self, double baseValue) {
            return BigInt.Log(self, baseValue);
        }

        public static double Log10(this BigInt self) {
            return BigInt.Log10(self);
        }

        public static BigInt Power(this BigInt self, int exp) {
            return BigInt.Pow(self, exp);
        }

        public static BigInt ModPow(this BigInt self, int power, BigInt mod) {
            return BigInt.ModPow(self, power, mod);
        }

        public static BigInt ModPow(this BigInt self, BigInt power, BigInt mod) {
            return BigInt.ModPow(self, power, mod);
        }

        public static string ToString(this BigInt self, int radix) {
            const string symbols = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be >= 2");
            }
            if (radix > 36) {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be <= 36");
            }

            bool isNegative = false;
            if (self < BigInt.Zero) {
                self = -self;
                isNegative = true;
            } else if (self == BigInt.Zero) {
                return "0";
            }

            List<char> digits = new List<char>();
            while (self > 0) {
                digits.Add(symbols[(int)(self % radix)]);
                self /= radix;
            }

            StringBuilder ret = new StringBuilder();
            if (isNegative) {
                ret.Append('-');
            }
            for (int digitIndex = digits.Count - 1; digitIndex >= 0; digitIndex--) {
                ret.Append(digits[digitIndex]);
            }
            return ret.ToString();
        }
#endif
        #endregion

        #region Exposing underlying data
#if !CLR2

        [CLSCompliant(false)]
        public static uint[] GetWords(this BigInt self) {
            if (self.IsZero) {
                return new uint[] { 0 };
            }

            int hi;
            byte[] bytes;
            GetHighestByte(self, out hi, out bytes);

            uint[] result = new uint[(hi + 1 + 3) / 4];
            int i = 0;
            int j = 0;
            uint u = 0;
            int shift = 0;
            while (i < bytes.Length) {
                u |= (uint)bytes[i++] << shift;
                if (i % 4 == 0) {
                    result[j++] = u;
                    u = 0;
                }
                shift += 8;
            }
            if (u != 0) {
                result[j] = u;
            }
            return result;
        }

        [CLSCompliant(false)]
        public static uint GetWord(this BigInt self, int index) {
            return GetWords(self)[index];
        }

        public static int GetWordCount(this BigInt self) {
            int index;
            byte[] bytes;
            GetHighestByte(self, out index, out bytes);
            return index / 4 + 1; // return (index + 1 + 3) / 4;
        }

        public static int GetByteCount(this BigInt self) {
            int index;
            byte[] bytes;
            GetHighestByte(self, out index, out bytes);
            return index + 1;
        }

        public static int GetBitCount(this BigInt self) {
            if (self.IsZero) {
                return 1;
            }
            byte[] bytes = BigInt.Abs(self).ToByteArray();

            int index = bytes.Length;
            while (bytes[--index] == 0) ;

            int count = index * 8;
            for (int hiByte = bytes[index]; hiByte > 0; hiByte >>= 1) {
                count++;
            }
            return count;
        }

        private static byte GetHighestByte(BigInt self, out int index, out byte[] byteArray) {
            byte[] bytes = BigInt.Abs(self).ToByteArray();
            if (self.IsZero) {
                byteArray = bytes;
                index = 0;
                return 1;
            }

            int hi = bytes.Length;
            byte b;
            do {
                b = bytes[--hi];
            } while (b == 0);
            index = hi;
            byteArray = bytes;
            return b;
        }

#endif
        #endregion

        #endregion

        #region Complex

#if CLR2
        public static Complex64 MakeReal(double real) {
            return new Complex64(real, 0.0);
        }

        public static Complex64 MakeImaginary(double imag) {
            return new Complex64(0.0, imag);
        }

        public static Complex64 MakeComplex(double real, double imag) {
            return new Complex64(real, imag);
        }

        public static double Imaginary(this Complex64 self) {
            return self.Imag;
        }

        public static bool IsZero(this Complex64 self) {
            return self.IsZero;
        }

        public static Complex64 Pow(this Complex64 self, Complex64 power) {
            return self.Power(power);
        }
#else
        public static Complex MakeReal(double real) {
            return new Complex(real, 0.0);
        }

        public static Complex MakeImaginary(double imag) {
            return new Complex(0.0, imag);
        }

        public static Complex MakeComplex(double real, double imag) {
            return new Complex(real, imag);
        }

        public static double Imaginary(this Complex self) {
            return self.Imaginary;
        }

        public static bool IsZero(this Complex self) {
            return self.Equals(Complex.Zero);
        }

        public static Complex Conjugate(this Complex self) {
            return new Complex(self.Real, -self.Imaginary);
        }

        public static double Abs(this Complex self) {
            return Complex.Abs(self);
        }

        public static Complex Pow(this Complex self, Complex power) {
            return Complex.Pow(self, power);
        }
#endif

        #endregion
    }

}
