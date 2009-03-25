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

using System;

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
    }

}
