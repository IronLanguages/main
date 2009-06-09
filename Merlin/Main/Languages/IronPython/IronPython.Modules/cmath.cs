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
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Math;

[assembly: PythonModule("cmath", typeof(IronPython.Modules.ComplexMath))]
namespace IronPython.Modules {
    public class ComplexMath {
        public const double pi = Math.PI;
        public const double e = Math.E;

        //cos(a+ ib) = cosa*coshb - i*sina*sinhb
        public static Complex64 cos(object x) {
            Complex64 num = GetComplexNum(x);

            // magnitude is always NaN
            if (double.IsNaN(num.Imag)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-Infinity
            if (double.IsInfinity(num.Real)) {
                throw PythonOps.ValueError("math domain error");
            }

            double real, imag;
            real = Math.Cos(num.Real) * Math.Cosh(num.Imag);
            imag = -(Math.Sin(num.Real) * Math.Sinh(num.Imag));

            return new Complex64(real, imag);
        }

        //sin(a+ ib) = sina*coshb + i*cosa*sinhb
        public static Complex64 sin(object x) {
            Complex64 num = GetComplexNum(x);

            // magnitude is always NaN
            if (double.IsNaN(num.Imag)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-Infinity
            if (double.IsInfinity(num.Real)) {
                throw PythonOps.ValueError("math domain error");
            }

            double real, imag;
            real = Math.Sin(num.Real) * Math.Cosh(num.Imag);
            imag = Math.Cos(num.Real) * Math.Sinh(num.Imag);

            return new Complex64(real, imag);
        }

        public static Complex64 tan(object x) {
            Complex64 num = GetComplexNum(x);

            // limit as num.Imag -> Infinity
            if (double.IsPositiveInfinity(num.Imag)) {
                return new Complex64(0.0, 1.0);
            }

            // limit as num.Imag -> -Infinity
            if (double.IsNegativeInfinity(num.Imag)) {
                return new Complex64(0.0, -1.0);
            }

            return sin(num) / cos(num);
        }

        //cosh(a+ ib) = cosha*cosb + i*sinha*sinb
        public static Complex64 cosh(object x) {
            Complex64 num = GetComplexNum(x);

            // magnitude is always NaN
            if (double.IsNaN(num.Real)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-Infinity
            if (double.IsInfinity(num.Imag)) {
                throw PythonOps.ValueError("math domain error");
            }

            double real, imag;
            real = Math.Cosh(num.Real) * Math.Cos(num.Imag);
            imag = Math.Sinh(num.Real) * Math.Sin(num.Imag);

            return new Complex64(real, imag);
        }

        //sin(a+ ib) = sinha*cosb + i*cosha*sinb
        public static Complex64 sinh(object x) {
            Complex64 num = GetComplexNum(x);

            // magnitude is always NaN
            if (double.IsNaN(num.Real)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-Infinity
            if (double.IsInfinity(num.Imag)) {
                throw PythonOps.ValueError("math domain error");
            }

            double real, imag;
            real = Math.Sinh(num.Real) * Math.Cos(num.Imag);
            imag = Math.Cosh(num.Real) * Math.Sin(num.Imag);

            return new Complex64(real, imag);
        }

        public static Complex64 tanh(object x) {
            Complex64 num = GetComplexNum(x);

            // limit as num.Real -> Infinity
            if (double.IsPositiveInfinity(num.Real)) {
                return new Complex64(1.0, 0.0);
            }

            // limit as num.Real -> -Infinity
            if (double.IsNegativeInfinity(num.Real)) {
                return new Complex64(-1.0, 0.0);
            }

            return sinh(num) / cosh(num);
        }
        
        //acos(x) = -i*ln( x + i*(1-x*x)^1/2)
        public static Complex64 acos(object x) {
            Complex64 num = GetComplexNum(x);

            double a = Complex64.Hypot(num.Real + 1.0, num.Imag);
            double b = Complex64.Hypot(num.Real - 1.0, num.Imag);
            double c = 0.5 * (a + b);
            double real = Math.Acos(0.5 * (a - b));
            double imag = Math.Log(c + Math.Sqrt(c + 1) * Math.Sqrt(c - 1));

            return new Complex64(real, num.Imag >= 0 ? imag : -imag);
        }

        //asin(x) = -i*ln( i*x + (1-x*x)^1/2)
        public static Complex64 asin(object x) {
            Complex64 num = GetComplexNum(x);

            double a = Complex64.Hypot(num.Real + 1.0, num.Imag);
            double b = Complex64.Hypot(num.Real - 1.0, num.Imag);
            double c = 0.5 * (a + b);
            double real = Math.Asin(0.5 * (a - b));
            double imag = Math.Log(c + Math.Sqrt(c + 1) * Math.Sqrt(c - 1));

            return new Complex64(real, num.Imag >= 0 ? imag : -imag);
        }

        //atan(x) = i/2*ln( (i+x)/ (i-x))
        public static Complex64 atan(object x) {
            Complex64 num = GetComplexNum(x);
            Complex64 i = Complex64.MakeImaginary(1);

            return i * 0.5 * (log(i + num) - log(i - num));
        }

        //acosh(x) = ln( x + (x*x -1)^1/2)
        public static Complex64 acosh(object x) {
            Complex64 num = GetComplexNum(x);

            return log(num + sqrt(num + 1) * sqrt(num - 1));
        }

        //asin(x) = ln( x + (x*x +1)^1/2)
        public static Complex64 asinh(object x) {
            Complex64 num = GetComplexNum(x);

            if (num.IsZero) {
                // preserve -0.0 imaginary component
                return Complex64.MakeImaginary(num.Imag);
            }

            Complex64 recip = 1 / num;
            return log(num) + log(1 + sqrt(recip * recip + 1));
        }

        //atanh(x) = (ln(1 +x) - ln(1-x))/2
        public static Complex64 atanh(object x) {
            Complex64 num = GetComplexNum(x);

            return (log(1 + num) - log(1 - num)) * 0.5;
        }

        //ln(re^iO) = ln(r) + iO 
        public static Complex64 log(object x) {
            Complex64 num = GetComplexNum(x);

            if (num.IsZero) {
                throw PythonOps.ValueError("math domain error");
            }

            double r, theta;
            r = num.Abs();
            theta = GetAngle(num);

            return new Complex64(Math.Log(r), theta);
        }

        //log b to base a = ln b / ln a
        public static Complex64 log(object x, object logBase) {
            return log(x) / log(logBase);
        }

        public static Complex64 log10(object x) {
            return log(x, 10);
        }

        public static Complex64 exp(object x) {
            Complex64 num = GetComplexNum(x);
            
            // degenerate case: num is real
            if (num.Imag == 0.0) {
                if (double.IsPositiveInfinity(num.Real)) {
                    return new Complex64(double.PositiveInfinity, 0.0);
                }

                double expt = Math.Exp(num.Real);
                if (double.IsInfinity(expt)) {
                    throw PythonOps.OverflowError("math range error");
                }

                return new Complex64(expt, 0.0);
            }

            // magnitude is always 0
            if (double.IsNegativeInfinity(num.Real)) {
                return new Complex64(0.0, 0.0);
            }

            // magnitude is always NaN
            if (double.IsNaN(num.Real)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // angle is always NaN
            if (double.IsNaN(num.Imag)) {
                return new Complex64(double.IsInfinity(num.Real) ? double.PositiveInfinity : double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-infinity
            if (double.IsInfinity(num.Imag)) {
                throw PythonOps.ValueError("math domain error");
            }
            
            // use c*(e^x) = (sign(c))*e^(x+log(abs(c))) for fewer overflows in corner cases
            double real;
            double cosImag = Math.Cos(num.Imag);
            if (cosImag > 0.0) {
                real = Math.Exp(num.Real + Math.Log(cosImag));
            } else if (cosImag < 0.0) {
                real = -Math.Exp(num.Real + Math.Log(-cosImag));
            } else {
                real = 0.0;
            }

            // use c*(e^x) = (sign(c))*e^(x+log(abs(c))) for fewer overflows in corner cases
            double imag;
            double sinImag = Math.Sin(num.Imag);
            if (sinImag > 0.0) {
                imag = Math.Exp(num.Real + Math.Log(sinImag));
            } else if (sinImag < 0.0) {
                imag = -Math.Exp(num.Real + Math.Log(-sinImag));
            } else {
                imag = 0.0;
            }

            // check for overflow
            if ((double.IsInfinity(real) || double.IsInfinity(imag)) && !double.IsInfinity(num.Real)) {
                throw PythonOps.OverflowError("math range error");
            }

            return new Complex64(real, imag);
        }

        public static Complex64 sqrt(object x) {
            Complex64 num = GetComplexNum(x);

            if (num.Imag == 0.0) {
                if (num.Real >= 0.0) {
                    return Complex64.MakeReal(Math.Sqrt(num.Real));
                } else {
                    return Complex64.MakeImaginary(Math.Sqrt(-num.Real));
                }
            }

            double c = num.Abs() + num.Real;
            double real = Math.Sqrt(0.5 * c);
            double imag = num.Imag / Math.Sqrt(2 * c);

            return new Complex64(real, imag);
        }

        public static double phase(object x) {
            Complex64 num = GetComplexNum(x);

            return GetAngle(num);
        }

        public static PythonTuple polar(object x) {
            Complex64 num = GetComplexNum(x);

            double[] res = new double[] { num.Abs(), GetAngle(num) };

            // check for overflow
            if (double.IsInfinity(res[0]) && !IsInfinity(num)) {
                throw PythonOps.OverflowError("math range error");
            }

            return new PythonTuple(res);
        }

        public static Complex64 rect(double r, double theta) {
            // magnitude is always 0
            if (r == 0.0) {
                return new Complex64(0.0, 0.0);
            }

            // angle is always 0
            if (theta == 0.0) {
                return new Complex64(r, 0.0);
            }

            // magnitude is always NaN
            if (double.IsNaN(r)) {
                return new Complex64(double.NaN, double.NaN);
            }

            // angle is always NaN
            if (double.IsNaN(theta)) {
                return new Complex64(double.IsInfinity(r) ? double.PositiveInfinity : double.NaN, double.NaN);
            }

            // can't take sin or cos of +/-Infinity
            if (double.IsInfinity(theta)) {
                throw PythonOps.ValueError("math domain error");
            }

            return new Complex64(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        public static bool isinf(object x) {
            Complex64 num = GetComplexNum(x);

            return IsInfinity(num);
        }

        public static bool isnan(object x) {
            Complex64 num = GetComplexNum(x);

            return IsNaN(num);
        }

        #region Helpers

        private static bool IsInfinity(Complex64 num) {
            return double.IsInfinity(num.Real) || double.IsInfinity(num.Imag);
        }

        private static bool IsNaN(Complex64 num) {
            return double.IsNaN(num.Real) || double.IsNaN(num.Imag);
        }

        private static double GetAngle(Complex64 num) {
            if (IsNaN(num)) {
                return double.NaN;
            }

            if (double.IsPositiveInfinity(num.Real)) {
                if (double.IsPositiveInfinity(num.Imag)) {
                    return Math.PI * 0.25;
                } else if (double.IsNegativeInfinity(num.Imag)) {
                    return Math.PI * -0.25;
                } else {
                    return 0.0;
                }
            }

            if (double.IsNegativeInfinity(num.Real)) {
                if (double.IsPositiveInfinity(num.Imag)) {
                    return Math.PI * 0.75;
                } else if (double.IsNegativeInfinity(num.Imag)) {
                    return Math.PI * -0.75;
                } else {
                    return DoubleOps.Sign(num.Imag) * Math.PI;
                }
            }

            if (num.Real == 0.0) {
                if (num.Imag != 0.0) {
                    return Math.PI * 0.5 * Math.Sign(num.Imag);
                } else {
                    return (DoubleOps.IsPositiveZero(num.Real) ? 0.0 : Math.PI) * DoubleOps.Sign(num.Imag);
                }
            }

            return Math.Atan2(num.Imag, num.Real);
        }

        private static Complex64 GetComplexNum(object num) {
            Complex64 complexNum;
            if (num != null) {
                complexNum = Converter.ConvertToComplex64(num);
            } else {
                throw new NullReferenceException("The input was null");
            }

            return complexNum;
        }
        #endregion
    }
}
