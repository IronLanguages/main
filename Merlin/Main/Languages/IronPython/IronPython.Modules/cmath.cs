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
using Microsoft.Scripting.Math; 

[assembly: PythonModule("cmath", typeof(IronPython.Modules.ComplexMath))]
namespace IronPython.Modules {
    public class ComplexMath {
        public const double pi = Math.PI;
        public const double e = Math.E;

        //cos(a+ ib) = cosa*coshb - i*sina*sinhb
        public static Complex64 cos(object x) {
            Complex64 num = GetComplexNum(x);

            double real, imag;
            real = Math.Cos(num.Real) * Math.Cosh(num.Imag);
            imag = -(Math.Sin(num.Real) * Math.Sinh(num.Imag));

            return new Complex64(real, imag);
        }

        //sin(a+ ib) = sina*coshb + i*cosa*sinhb
        public static Complex64 sin(object x) {
            Complex64 num = GetComplexNum(x);

            double real, imag;
            real = Math.Sin(num.Real) * Math.Cosh(num.Imag);
            imag = Math.Cos(num.Real) * Math.Sinh(num.Imag);

            return new Complex64(real, imag);
        }

        public static Complex64 tan(object x) {
            return sin(x) / cos(x);
        }

        //cosh(a+ ib) = cosha*cosb + i*sinha*sinb
        public static Complex64 cosh(object x) {
            Complex64 num = GetComplexNum(x);

            double real, imag;
            real = Math.Cosh(num.Real) * Math.Cos(num.Imag);
            imag = Math.Sinh(num.Real) * Math.Sin(num.Imag);

            return new Complex64(real, imag);
        }

        //sin(a+ ib) = sinha*cosb + i*cosha*sinb
        public static Complex64 sinh(object x) {
            Complex64 num = GetComplexNum(x);

            double real, imag;
            real = Math.Sinh(num.Real) * Math.Cos(num.Imag);
            imag = Math.Cosh(num.Real) * Math.Sin(num.Imag);

            return new Complex64(real, imag);
        }

        public static Complex64 tanh(object x) {
            return sinh(x) / cosh(x);
        }

        //acos(x) = -i*ln( x + i*(1-x*x)^1/2)
        public static Complex64 acos(object x) {
            Complex64 num = GetComplexNum(x);
            Complex64 i = Complex64.MakeImaginary(1);

            return -i * log(num + sqrt(1 - num * num) * i);
        }

        //asin(x) = -i*ln( i*x + (1-x*x)^1/2)
        public static Complex64 asin(object x) {
            Complex64 num = GetComplexNum(x);
            Complex64 i = Complex64.MakeImaginary(1);

            return -i * log(i * num + sqrt(1 - num * num));
        }

        //atan(x) = i/2*ln( (i+x)/ (i-x))
        public static Complex64 atan(object x) {
            Complex64 num = GetComplexNum(x);
            Complex64 i = Complex64.MakeImaginary(1);

            return i / 2 * log((i + num) / (i - num));
        }

        //acosh(x) = ln( x + (x*x -1)^1/2)
        public static Complex64 acosh(object x) {
            Complex64 num = GetComplexNum(x);

            return log(num + sqrt(num * num - 1));
        }

        //asin(x) = ln( x + (x*x +1)^1/2)
        public static Complex64 asinh(object x) {
            Complex64 num = GetComplexNum(x);

            return log(num + sqrt(num * num + 1));
        }

        //atanh(x) = (ln(1 +x) - ln(1-x))/2
        public static Complex64 atanh(object x) {
            Complex64 num = GetComplexNum(x);

            return (log(1 + num) - log(1 - num)) / 2;
        }

        //ln(re^iO) = ln(r) + iO 
        public static Complex64 log(object x) {
            Complex64 num = GetComplexNum(x);

            double r, theta;
            r = num.Abs();
            theta = Math.Atan2(num.Imag, num.Real);

            return new Complex64(Math.Log(r), theta);
        }

        //log b to base a = ln b / ln a 
        public static Complex64 log(object x, int logBase) {
            return log(x) / Math.Log(logBase);
        }

        public static Complex64 log10(object x) {
            return log(x, 10);
        }

        public static Complex64 exp(object x) {
            Complex64 num = GetComplexNum(x);

            return new Complex64(e, 0).Power(num);
        }

        public static Complex64 sqrt(object x) {
            Complex64 num = GetComplexNum(x);

            return num.Power(new Complex64(0.5, 0));
        }

        #region Helpers

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
