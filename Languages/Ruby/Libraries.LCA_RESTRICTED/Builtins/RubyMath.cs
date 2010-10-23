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

using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using SM = System.Math;

namespace IronRuby.Builtins {

    [RubyModule("Math")]
    public static class RubyMath {

        [RubyConstant]
        public const double E = System.Math.E;

        [RubyConstant]
        public const double PI = System.Math.PI;

        #region Private Implementation Details

        private static double DomainCheck(double result, string/*!*/ functionName) {
            if (double.IsNaN(result)) {
                throw new Errno.DomainError("Domain error - " + functionName);
            }
            return result;
        }

        private static ushort Exponent(byte[] v) {
            return (ushort)((((ushort)(v[7] & 0x7F)) << (ushort)4) | (((ushort)(v[6] & 0xF0)) >> 4));
        }

        private static ulong Mantissa(byte[] v) {
            uint i1 = ((uint)v[0] | ((uint)v[1] << 8) | ((uint)v[2] << 16) | ((uint)v[3] << 24));
            uint i2 = ((uint)v[4] | ((uint)v[5] << 8) | ((uint)(v[6] & 0xF) << 16));

            return (ulong)((ulong)i1 | ((ulong)i2 << 32));
        }
        
        #endregion

        #region Private Instance & Singleton Methods

        [RubyMethodAttribute("acos", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("acos", RubyMethodAttributes.PublicSingleton)]
        public static double Acos(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Acos(x), "acos");
        }

        [RubyMethodAttribute("acosh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("acosh", RubyMethodAttributes.PublicSingleton)]
        public static double Acosh(object self, [DefaultProtocol]double x) {
            //ln(x + sqrt(x*x - 1)) for x >= 1
            return DomainCheck(SM.Log(x + SM.Sqrt(x*x - 1)), "acosh");
        }

        [RubyMethodAttribute("asin", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("asin", RubyMethodAttributes.PublicSingleton)]
        public static double Asin(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Asin(x), "asin");
        }

        [RubyMethodAttribute("asinh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("asinh", RubyMethodAttributes.PublicSingleton)]
        public static double Asinh(object self, [DefaultProtocol]double x) {
            //ln(x + sqrt(x*x + 1))
            return SM.Log(x + SM.Sqrt(x * x + 1));
        }

        [RubyMethodAttribute("atan", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("atan", RubyMethodAttributes.PublicSingleton)]
        public static double Atan(object self, [DefaultProtocol]double x) {
            return SM.Atan(x);
        }

        [RubyMethodAttribute("atan2", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("atan2", RubyMethodAttributes.PublicSingleton)]
        public static double Atan2(object self, [DefaultProtocol]double y, [DefaultProtocol]double x) {
            return SM.Atan2(y, x);
        }
        
        [RubyMethodAttribute("atanh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("atanh", RubyMethodAttributes.PublicSingleton)]
        public static double Atanh(object self, [DefaultProtocol]double x) {
            //(1/2) * ln((1+x)/(1-x))
            return DomainCheck(0.5 * SM.Log((1 + x) / (1 - x)), "atanh");
        }
        
        [RubyMethodAttribute("cos", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("cos", RubyMethodAttributes.PublicSingleton)]
        public static double Cos(object self, [DefaultProtocol]double x) {
            return SM.Cos(x);
        }

        [RubyMethodAttribute("cosh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("cosh", RubyMethodAttributes.PublicSingleton)]
        public static double Cosh(object self, [DefaultProtocol]double x) {
            return SM.Cosh(x);
        }

        [RubyMethodAttribute("cbrt", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("cbrt", RubyMethodAttributes.PublicSingleton)]
        public static double CubeRoot(object self, [DefaultProtocol]double x) {
            return System.Math.Pow(x, 1.0 / 3.0);
        }

        [RubyMethodAttribute("erf", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("erf", RubyMethodAttributes.PublicSingleton)]
        public static double Erf(object self, [DefaultProtocol]double x) {
            return MathUtils.Erf(x);
        }

        [RubyMethodAttribute("erfc", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("erfc", RubyMethodAttributes.PublicSingleton)]
        public static double Erfc(object self, [DefaultProtocol]double x) {
            return MathUtils.ErfComplement(x);
        }

        [RubyMethodAttribute("exp", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("exp", RubyMethodAttributes.PublicSingleton)]
        public static double Exp(object self, [DefaultProtocol]double x) {
            return SM.Exp(x);
        }

        [RubyMethodAttribute("gamma", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("gamma", RubyMethodAttributes.PublicSingleton)]
        public static double Gamma(object self, [DefaultProtocol]double x) {
            return DomainCheck(MathUtils.Gamma(x), "gamma");
        }

        [RubyMethodAttribute("hypot", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("hypot", RubyMethodAttributes.PublicSingleton)]
        public static double Hypot(object self, [DefaultProtocol]double x, [DefaultProtocol]double y) {
            return DomainCheck(SM.Sqrt(x*x+y*y), "hypot");
        }

        [RubyMethodAttribute("frexp", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("frexp", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Frexp(object self, [DefaultProtocol]double x) {
            byte[] bytes;
            double mantissa;
            int exponent;
            RubyArray result = new RubyArray(2);

            bytes = System.BitConverter.GetBytes(x);
            mantissa = (Mantissa(bytes) * SM.Pow(2, -52) + 1.0) / 2;
            exponent = Exponent(bytes) - 1022;

            result.Add(mantissa);
            result.Add(exponent);
            return result;
        }

        [RubyMethodAttribute("ldexp", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("ldexp", RubyMethodAttributes.PublicSingleton)]
        public static double Ldexp(object self, [DefaultProtocol]double x, [DefaultProtocol]IntegerValue y) {
            return x * SM.Pow(2, y.IsFixnum ? (double)y.Fixnum : y.Bignum.ToFloat64());
        }

        [RubyMethodAttribute("lgamma", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("lgamma", RubyMethodAttributes.PublicSingleton)]
        public static double LogGamma(object self, [DefaultProtocol]double x) {
            return DomainCheck(MathUtils.LogGamma(x), "lgamma");
        }
        
        [RubyMethodAttribute("log", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("log", RubyMethodAttributes.PublicSingleton)]
        public static double Log(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Log(x), "log");
        }

        [RubyMethodAttribute("log10", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("log10", RubyMethodAttributes.PublicSingleton)]
        public static double Log10(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Log10(x), "log10");
        }

        [RubyMethodAttribute("log2", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("log2", RubyMethodAttributes.PublicSingleton)]
        public static double Log2(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Log(x)/SM.Log(2), "log2");
        }

        [RubyMethodAttribute("sin", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("sin", RubyMethodAttributes.PublicSingleton)]
        public static double Sin(object self, [DefaultProtocol]double x) {
            return SM.Sin(x);
        }

        [RubyMethodAttribute("sinh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("sinh", RubyMethodAttributes.PublicSingleton)]
        public static double Sinh(object self, [DefaultProtocol]double x) {
            return SM.Sinh(x);
        }

        [RubyMethodAttribute("sqrt", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("sqrt", RubyMethodAttributes.PublicSingleton)]
        public static double Sqrt(object self, [DefaultProtocol]double x) {
            return DomainCheck(SM.Sqrt(x), "sqrt");
        }

        [RubyMethodAttribute("tan", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("tan", RubyMethodAttributes.PublicSingleton)]
        public static double Tan(object self, [DefaultProtocol]double x) {
            return SM.Tan(x);
        }

        [RubyMethodAttribute("tanh", RubyMethodAttributes.PrivateInstance)]
        [RubyMethodAttribute("tanh", RubyMethodAttributes.PublicSingleton)]
        public static double Tanh(object self, [DefaultProtocol]double x) {
            return SM.Tanh(x);
        }

        #endregion
    }
}
