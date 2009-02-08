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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Types;

using SpecialNameAttribute = System.Runtime.CompilerServices.SpecialNameAttribute;

namespace IronPython.Runtime.Operations {

    public static partial class DoubleOps {
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.Double) return 0.0;

            return cls.CreateInstance(context);
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, object x) {
            if (cls == TypeCache.Double) {
                if (x is string) {
                    return ParseFloat((string)x);
                } else if (x is Extensible<string>) {
                    return ParseFloat(((Extensible<string>)x).Value);
                } else if (x is char) {
                    return ParseFloat(ScriptingRuntimeHelpers.CharToString((char)x));
                }

                double doubleVal;
                if (Converter.TryConvertToDouble(x, out doubleVal)) return doubleVal;

                if (x is Complex64) throw PythonOps.TypeError("can't convert complex to float; use abs(z)");

                object d = PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, x, Symbols.ConvertToFloat));
                if (d is double) return d;
                throw PythonOps.TypeError("__float__ returned non-float (type %s)", DynamicHelpers.GetPythonType(d));
            } else {
                return cls.CreateInstance(context, x);
            }
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext/*!*/ context, PythonType cls, IList<byte> s) {
            if (cls == TypeCache.Double) {
                object value;
                IPythonObject po = s as IPythonObject;
                if (po != null &&
                    PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, po, Symbols.ConvertToFloat, out value)) {
                    return value;
                }

                return ParseFloat(StringOps.FromByteArray(s));
            }

            return cls.CreateInstance(context, s);
        }

        private static object ParseFloat(string x) {
            try {
                return LiteralParser.ParseFloat(x);
            } catch (FormatException) {
                throw PythonOps.ValueError("invalid literal for float(): {0}", x);
            }
        }


        #region Binary operators

        [SpecialName]
        [return: MaybeNotImplemented]
        public static object DivMod(double x, double y) {
            object div = FloorDivide(x, y);
            if (div == NotImplementedType.Value) return div;
            return PythonTuple.MakeTuple(div, Mod(x, y));
        }

        [SpecialName]
        public static double Mod(double x, double y) {
            if (y == 0) throw PythonOps.ZeroDivisionError();

            double r = x % y;
            if (r > 0 && y < 0) {
                r = r + y;
            } else if (r < 0 && y > 0) {
                r = r + y;
            }
            return r;
        }

        [SpecialName]
        public static double Power(double x, double y) {
            if (x == 0.0 && y < 0.0)
                throw PythonOps.ZeroDivisionError("0.0 cannot be raised to a negative power");
            if (x < 0 && (Math.Floor(y) != y)) {
                throw PythonOps.ValueError("negative number cannot be raised to fraction");
            }
            double result = Math.Pow(x, y);
            if (double.IsInfinity(result)) {
                throw PythonOps.OverflowError("result too large");
            }
            return result;
        }
        #endregion

        public static PythonTuple __coerce__(CodeContext context, double x, object o) {
            // called via builtin.coerce()
            double d = (double)__new__(context, TypeCache.Double, o);

            if (Double.IsInfinity(d)) {
                throw PythonOps.OverflowError("number too big");
            }

            return PythonTuple.MakeTuple(x, d);
        }
        
        #region Unary operators

        public static object __int__(double d) {
            if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                return (int)d;
            } else if (Int64.MinValue <= d && d <= Int64.MaxValue) {
                return (long)d;
            } else {
                return BigInteger.Create(d);
            }
        }

        public static object __getnewargs__(CodeContext context, double self) {
            return PythonTuple.MakeTuple(DoubleOps.__new__(context, TypeCache.Double, self));
        }

        #endregion

        #region ToString

        public static string __str__(CodeContext/*!*/ context, double x) {
            StringFormatter sf = new StringFormatter(context, "%.12g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static string __str__(double x, IFormatProvider provider) {
            return x.ToString(provider);
        }

        public static string __str__(double x, string format) {
            return x.ToString(format);
        }

        public static string __str__(double x, string format, IFormatProvider provider) {
            return x.ToString(format, provider);
        }

        public static int __hash__(double d) {
            // Python allows equality between floats, ints, and big ints.
            if ((d % 1) == 0) {
                // This double represents an integer number, so it must hash like an integer number.
                if (Int32.MinValue <= d && d <= Int32.MaxValue) {
                    return ((int)d).GetHashCode();
                }
                // Big integer
                BigInteger b = BigInteger.Create(d);
                return BigIntegerOps.__hash__(b);
            }
            return d.GetHashCode();
        }

        #endregion

        [SpecialName]
        public static bool LessThan(double x, double y) {
            if (Double.IsInfinity(x) && Double.IsNaN(y)) {
                return false;
            } else if (Double.IsNaN(x) && Double.IsInfinity(y)) {
                return false;
            }

            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }

            return x < y;
        }

        [SpecialName]
        public static bool GreaterThan(double x, double y) {
            if (Double.IsInfinity(x) && Double.IsNaN(y)) {
                return false;
            } else if (Double.IsNaN(x) && Double.IsInfinity(y)) {
                return false;
            }

            return x > y;
        }

        [SpecialName]
        public static bool GreaterThanOrEqual(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }

            return x > y;
        }

        [SpecialName]
        public static bool Equals(double x, double y) {
            if (x == y) {
                return x != Double.NaN;
            }
            return x == y;
        }

        [SpecialName]
        public static bool NotEquals(double x, double y) {
            return !Equals(x, y);
        }

        [SpecialName]
        public static bool LessThan(double x, BigInteger y) {
            return Compare(x, y) < 0;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) <= 0;
        }
        [SpecialName]
        public static bool GreaterThan(double x, BigInteger y) {
            return Compare(x, y) > 0;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(double x, BigInteger y) {
            return Compare(x, y) >= 0;
        }
        [SpecialName]
        public static bool Equals(double x, BigInteger y) {
            return Compare(x, y) == 0;
        }
        [SpecialName]
        public static bool NotEquals(double x, BigInteger y) {
            return Compare(x, y) != 0;
        }

        internal static int Compare(double x, double y) {
            if (Double.IsInfinity(x) && Double.IsNaN(y)) {
                return 1;
            } else if (Double.IsNaN(x) && Double.IsInfinity(y)) {
                return -1;
            }

            return x > y ? 1 : x == y ? 0 : -1;
        }

        internal static int Compare(double x, BigInteger y) {
            return -Compare(y, x);
        }

        internal static int Compare(BigInteger x, double y) {
            if (y == Double.PositiveInfinity) {
                return -1;
            } else if (y == Double.NegativeInfinity) {
                return 1;
            }

            // BigInts can hold doubles, but doubles can't hold BigInts, so
            // if we're comparing against a BigInt then we should convert ourself
            // to a long and then compare.
            if (object.ReferenceEquals(x,null)) return -1;
            BigInteger by = BigInteger.Create(y);
            if (by == x) {
                double mod = y % 1;
                if (mod == 0) return 0;
                if (mod > 0) return -1;
                return +1;
            }
            if (by > x) return -1;
            return +1;
        }

        [SpecialName]
        public static bool LessThan(double x, decimal y) {
            return Compare(x, y) < 0;
        }
        [SpecialName]
        public static bool LessThanOrEqual(double x, decimal y) {
            return Compare(x, y) <= 0;
        }
        [SpecialName]
        public static bool GreaterThan(double x, decimal y) {
            return Compare(x, y) > 0;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(double x, decimal y) {
            return Compare(x, y) >= 0;
        }
        [SpecialName]
        public static bool Equals(double x, decimal y) {
            return Compare(x, y) == 0;
        }
        [SpecialName]
        public static bool NotEquals(double x, decimal y) {
            return Compare(x, y) != 0;
        }


        internal static int Compare(double x, decimal y) {
            if (x > (double)decimal.MaxValue) return +1;
            if (x < (double)decimal.MinValue) return -1;
            return ((decimal)x).CompareTo(y);
        }

        [SpecialName]
        public static bool LessThan(Double x, int y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(Double x, int y) {
            return x <= y;
        }
        [SpecialName]
        public static bool GreaterThan(Double x, int y) {
            return x > y;
        }
        [SpecialName]
        public static bool GreaterThanOrEqual(Double x, int y) {
            return x >= y;
        }
        [SpecialName]
        public static bool Equals(Double x, int y) {
            return x == y;
        }
        [SpecialName]
        public static bool NotEquals(Double x, int y) {
            return x != y;
        }

        public static string __repr__(CodeContext/*!*/ context, double self) {
            StringFormatter sf = new StringFormatter(context, "%.17g", self);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static BigInteger/*!*/ __long__(double self) {
            return BigInteger.Create(self);
        }

        public static double __float__(double self) {
            return self;
        }

        public static string __getformat__(CodeContext/*!*/ context, string typestr) {
            switch (typestr) {
                case "float":
                    return PythonContext.GetContext(context).FloatFormat ?? DefaultFloatFormat();
                case "double":
                    return PythonContext.GetContext(context).DoubleFormat ?? DefaultFloatFormat();
                default: throw PythonOps.ValueError("__getformat__() argument 1 must be 'double' or 'float'");
            }
        }

        public static string __format__(CodeContext/*!*/ context, double self, [NotNull]string/*!*/ formatSpec) {
            StringFormatSpec spec = StringFormatSpec.FromString(formatSpec);
            string digits;

            if (Double.IsPositiveInfinity(self) || Double.IsNegativeInfinity(self)) {
                if (spec.Sign == null) {
                    digits = "inf";
                } else {
                    digits = "1.0#INF";
                }
            } else if (Double.IsNaN(self)) {
                if (spec.Sign == null) {
                    digits = "nan";
                } else {
                    digits = "1.0#IND";
                }
            } else {
                digits = DoubleToFormatString(context, self, spec);
            }

            if (spec.Sign == null) {
                // This is special because its not "-nan", it's nan.
                return spec.AlignNumericText(digits, self == 0, Double.IsNaN(self) || self > 0);
            } else {
                return spec.AlignNumericText(digits, self == 0, self > 0);
            }
        }

        /// <summary>
        /// Returns the digits for the format spec, no sign is included.
        /// </summary>
        private static string DoubleToFormatString(CodeContext/*!*/ context, double self, StringFormatSpec/*!*/ spec) {
            self = Math.Abs(self);
            const int DefaultPrecision = 6;
            int precision = spec.Precision ?? DefaultPrecision;

            string digits;
            switch (spec.Type) {
                case '%': digits = self.ToString("0." + new string('0', precision) + "%", CultureInfo.InvariantCulture); break;
                case 'f':
                case 'F': digits = self.ToString("#." + new string('0', precision), CultureInfo.InvariantCulture); break;
                case 'e': digits = self.ToString("0." + new string('0', precision) + "e+00", CultureInfo.InvariantCulture); break;
                case 'E': digits = self.ToString("0." + new string('0', precision) + "E+00", CultureInfo.InvariantCulture); break;
                case '\0':
                case null:
                    if (spec.Precision != null) {
                        // precision applies to the combined digits before and after the decimal point
                        // so we first need find out how many digits we have before...
                        int digitCnt = 1;
                        double cur = Math.Abs(self);
                        while (cur >= 10) {
                            cur /= 10;
                            digitCnt++;
                        }

                        // Use exponents if we don't have enough room for all the digits before.  If we
                        // only have as single digit avoid exponents.
                        if (digitCnt > spec.Precision.Value && digitCnt != 1) {
                            // first round off the decimal value
                            self = MathUtils.RoundAwayFromZero(self, 0);

                            // then remove any insignificant digits
                            double pow = Math.Pow(10, digitCnt - Math.Max(spec.Precision.Value, 1));
                            self = self - (self % pow);

                            // finally format w/ the requested precision
                            string fmt = "0.0" + new string('#', spec.Precision.Value);

                            digits = self.ToString(fmt + "e+00", CultureInfo.InvariantCulture);
                        } else {
                            // we're including all the numbers to the right of the decimal we can, we explicitly 
                            // round to match CPython's behavior
                            int decimalPoints = Math.Max(spec.Precision.Value - digitCnt, 0);

                            self = MathUtils.RoundAwayFromZero(self, decimalPoints);
                            digits = self.ToString("0.0" + new string('#', decimalPoints));
                        }
                    } else {
                        // just the default formatting
                        if (self >= 1000000000000 || self <= -1000000000000) {
                            digits = self.ToString("0.#e+00", CultureInfo.InvariantCulture);
                        } else {
                            digits = self.ToString("0.0", CultureInfo.InvariantCulture);
                        }
                    }
                    break;
                case 'n':
                case 'g':
                case 'G':
                    {
                        // precision applies to the combined digits before and after the decimal point
                        // so we first need find out how many digits we have before...
                        int digitCnt = 1;
                        double cur = Math.Abs(self);
                        while (cur >= 10) {
                            cur /= 10;
                            digitCnt++;
                        }

                        // Use exponents if we don't have enough room for all the digits before.  If we
                        // only have as single digit avoid exponents.
                        if (digitCnt > precision && digitCnt != 1) {
                            // first round off the decimal value
                            self = MathUtils.RoundAwayFromZero(self, 0);

                            // then remove any insignificant digits
                            double pow = Math.Pow(10, digitCnt - Math.Max(precision, 1));
                            self = self - (self % pow);

                            string fmt;
                            if (spec.Type == 'n' && PythonContext.GetContext(context).NumericCulture != CultureInfo.InvariantCulture) {
                                // we've already figured out, we don't have any digits for decimal points, so just format as a number + exponent
                                fmt = "0";
                            } else if (spec.Precision > 1) {
                                // include the requested percision to the right of the decimal
                                fmt = "0.0" + new string('#', precision);
                            } else {
                                // zero precision, no decimal
                                fmt = "0";
                            }                            

                            digits = self.ToString(fmt + (spec.Type == 'G' ? "E+00" : "e+00"), CultureInfo.InvariantCulture);
                        } else {
                            // we're including all the numbers to the right of the decimal we can, we explicitly 
                            // round to match CPython's behavior
                            int decimalPoints = Math.Max(precision - digitCnt, 0);

                            self = MathUtils.RoundAwayFromZero(self, decimalPoints);

                            if (spec.Type == 'n' && PythonContext.GetContext(context).NumericCulture != CultureInfo.InvariantCulture) {
                                if (digitCnt != precision && (self % 1) != 0) {
                                    digits = self.ToString("#,0.0" + new string('#', decimalPoints));
                                } else {
                                    // leave out the decimal if the precision == # of digits or we have a whole number
                                    digits = self.ToString("#,0");
                                }
                            } else {
                                if (digitCnt != precision && (self % 1) != 0) {
                                    digits = self.ToString("0.0" + new string('#', decimalPoints));
                                } else {
                                    // leave out the decimal if the precision == # of digits or we have a whole number
                                    digits = self.ToString("0");
                                }
                            }
                        }
                    }                   
                    break;
                default:
                    throw PythonOps.ValueError("Unknown conversion type {0}", spec.Type.ToString());
            }

            return digits;
        }

        private static string DefaultFloatFormat() {
            if (BitConverter.IsLittleEndian) {
                return "IEEE, little-endian";
            }

            return "IEEE, big-endian";
        }

        public static void __setformat__(CodeContext/*!*/ context, string typestr, string fmt) {            
            switch (fmt) {
                case "unknown":
                    break;
                case "IEEE, little-endian":
                    if (!BitConverter.IsLittleEndian) {
                        throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
                    }
                    break;
                case "IEEE, big-endian":
                    if (BitConverter.IsLittleEndian) {
                        throw PythonOps.ValueError("can only set double format to 'unknown' or the detected platform value");
                    }
                    break;
                default:
                    throw PythonOps.ValueError(" __setformat__() argument 2 must be 'unknown', 'IEEE, little-endian' or 'IEEE, big-endian'");
            }

            switch (typestr) {
                case "float":
                    PythonContext.GetContext(context).FloatFormat = fmt;
                    break;
                case "double":
                    PythonContext.GetContext(context).DoubleFormat = fmt;
                    break;
                default: 
                    throw PythonOps.ValueError("__setformat__() argument 1 must be 'double' or 'float'");
            }            
        }
    }

    public partial class SingleOps {
        [SpecialName]
        public static bool LessThan(float x, float y) {
            return x < y;
        }
        [SpecialName]
        public static bool LessThanOrEqual(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }

            return x < y;
        }

        [SpecialName]
        public static bool GreaterThan(float x, float y) {
            return x > y;
        }

        [SpecialName]
        public static bool GreaterThanOrEqual(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }

            return x > y;
        }

        [SpecialName]
        public static bool Equals(float x, float y) {
            if (x == y) {
                return x != Single.NaN;
            }
            return x == y;
        }

        [SpecialName]
        public static bool NotEquals(float x, float y) {
            return !Equals(x, y);
        }

        [SpecialName]
        public static float Mod(float x, float y) {
            return (float)DoubleOps.Mod(x, y);
        }

        [SpecialName]
        public static float Power(float x, float y) {
            return (float)DoubleOps.Power(x, y);
        }

        public static string __str__(CodeContext/*!*/ context, float x) {
            // Python does not natively support System.Single. However, we try to provide
            // formatting consistent with System.Double.
            StringFormatter sf = new StringFormatter(context, "%.6g", x);
            sf._TrailingZeroAfterWholeFloat = true;
            return sf.Format();
        }

        public static string __repr__(CodeContext/*!*/ context, float self) {
            return __str__(context, self);
        }

        public static int __hash__(float x) {
            return DoubleOps.__hash__(((double)x));
        }
    }
}
