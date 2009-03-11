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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Math;

namespace IronPython.Runtime.Operations {
    public class ExtensibleComplex : Extensible<Complex64> {
        public ExtensibleComplex() : base() { }
        public ExtensibleComplex(double real) : base(Complex64.MakeReal(real)) { }
        public ExtensibleComplex(double real, double imag) : base(new Complex64(real, imag)) { }
    }

    public static partial class ComplexOps {
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls) {
            if (cls == TypeCache.Complex64) return new Complex64();
            return cls.CreateInstance(context);
        }

        [StaticExtensionMethod]
        public static object __new__(
            CodeContext context, 
            PythonType cls,
            [DefaultParameterValue(null)]object real,
            [DefaultParameterValue(null)]object imag
           ) {
            Complex64 real2, imag2;
            real2 = imag2 = new Complex64();

            if (real == null && imag == null && cls == TypeCache.Complex64) throw PythonOps.TypeError("argument must be a string or a number");

            if (imag != null) {
                if (real is string) throw PythonOps.TypeError("complex() can't take second arg if first is a string");
                if (imag is string) throw PythonOps.TypeError("complex() second arg can't be a string");
                imag2 = Converter.ConvertToComplex64(imag);
            }

            if (real != null) {
                if (real is string) {
                    real2 = LiteralParser.ParseComplex64((string)real);
                } else if (real is Extensible<string>) {
                    real2 = LiteralParser.ParseComplex64(((Extensible<string>)real).Value);
                } else if (real is Complex64) {
                    if (imag == null && cls == TypeCache.Complex64) return real;
                    else real2 = (Complex64)real;
                } else {
                    real2 = Converter.ConvertToComplex64(real);
                }
            }

            double real3 = real2.Real - imag2.Imag;
            double imag3 = real2.Imag + imag2.Real;
            if (cls == TypeCache.Complex64) {
                return new Complex64(real3, imag3);
            } else {
                return cls.CreateInstance(context, real3, imag3);
            }
        }
        
        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, double real) {
            if (cls == TypeCache.Complex64) {
                return new Complex64(real, 0.0);
            } else {
                return cls.CreateInstance(context, real, 0.0);
            }
        }

        [StaticExtensionMethod]
        public static object __new__(CodeContext context, PythonType cls, double real, double imag) {
            if (cls == TypeCache.Complex64) {
                return new Complex64(real, imag);
            } else {
                return cls.CreateInstance(context, real, imag);
            }
        }

        [SpecialName, PropertyMethod]
        public static double Getreal(Complex64 self) {
            return self.Real;
        }

        [SpecialName, PropertyMethod]
        public static double Getimag(Complex64 self) {
            return self.Imag;
        }

        #region Binary operators
        [SpecialName]
        public static Complex64 TrueDivide(Complex64 x, Complex64 y) {
            return x / y;
        }

        [SpecialName]
        public static Complex64 op_Power(Complex64 x, Complex64 y) {
            if (x.IsZero && (y.Real < 0.0 || y.Imag != 0.0))
                throw PythonOps.ZeroDivisionError("0.0 to a negative or complex power");
            return x.Power(y);
        }

        // floordiv for complex numbers is deprecated in the Python 2.
        // specification; this function implements the observable
        // functionality in CPython 2.4: 
        //   Let x, y be complex.
        //   Re(x//y) := floor(Re(x/y))
        //   Im(x//y) := 0
        [SpecialName]
        public static Complex64 FloorDivide(CodeContext context, Complex64 x, Complex64 y) {
            PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "complex divmod(), // and % are deprecated");
            Complex64 quotient = x / y;
            return Complex64.MakeReal(PythonOps.CheckMath(Math.Floor(quotient.Real)));
        }

        // mod for complex numbers is also deprecated. IronPython
        // implements the CPython semantics, that is:
        // x % y = x - (y * (x//y)).
        [SpecialName]
        public static Complex64 Mod(CodeContext context, Complex64 x, Complex64 y) {
            Complex64 quotient = FloorDivide(context, x, y);
            return x - (quotient * y);
        }

        [SpecialName]
        public static PythonTuple DivMod(CodeContext context, Complex64 x, Complex64 y) {
            Complex64 quotient = FloorDivide(context, x, y);
            return PythonTuple.MakeTuple(quotient, x - (quotient * y));
        }

        #endregion

        #region Unary operators

        public static int __hash__(Complex64 x) {
            if (x.Imag == 0) {
                return DoubleOps.__hash__(x.Real);
            }
            return x.GetHashCode();
        }

        public static bool __nonzero__(Complex64 x) {
            return !x.IsZero;
        }

        public static Complex64 conjugate(Complex64 x) {
            return x.Conjugate();
        }

        public static object __getnewargs__(CodeContext context, Complex64 self) {
            if (!Object.ReferenceEquals(self, null)) {
                return PythonTuple.MakeTuple(
                    ComplexOps.__new__(context,
                        TypeCache.Complex64,
                        PythonOps.GetBoundAttr(context, self, Symbols.RealPart),
                        PythonOps.GetBoundAttr(context, self, Symbols.ImaginaryPart)
                    )
                );
            }
            throw PythonOps.TypeErrorForBadInstance("__getnewargs__ requires a 'complex' object but received a '{0}'", self);
        }

        #endregion

        public static object __coerce__(Complex64 x, object y) {
            Complex64 right;
            if (Converter.TryConvertToComplex64(y, out right)) return PythonTuple.MakeTuple(x, right);

            if (y is BigInteger || y is Extensible<BigInteger>) throw PythonOps.OverflowError("long too large to convert");

            return NotImplementedType.Value;
        }

        public static string __repr__(CodeContext/*!*/ context, Complex64 x) {
            string j = (double.IsInfinity(x.Imag) || double.IsNaN(x.Imag)) ? "*j" : "j";

            if (x.Real != 0) {
                if (x.Imag < 0 || DoubleOps.IsNegativeZero(x.Imag)) {
                    return "(" + FormatComplexValue(context, x.Real) + FormatComplexValue(context, x.Imag) + j + ")";
                } else /* x.Imag is NaN or >= +0.0 */ {
                    return "(" + FormatComplexValue(context, x.Real) + "+" + FormatComplexValue(context, x.Imag) + j + ")";
                }
            }

            return FormatComplexValue(context, x.Imag) + j;
        }

        // report the same errors as CPython for these invalid conversions
        public static double __float__(Complex64 self) {
            throw PythonOps.TypeError("can't convert complex to float; use abs(z)");
        }

        public static int __int__(Complex64 self) {
            throw PythonOps.TypeError(" can't convert complex to int; use int(abs(z))");
        }

        public static BigInteger __long__(Complex64 self) {
            throw PythonOps.TypeError("can't convert complex to long; use long(abs(z))");
        }

        private static string FormatComplexValue(CodeContext/*!*/ context, double x) {
            StringFormatter sf = new StringFormatter(context, "%.6g", x);
            return sf.Format();
        }
        
        // Unary Operations
        [SpecialName]
        public static double Abs(Complex64 x) {
            double res = x.Abs();

            if (double.IsInfinity(res) && !double.IsInfinity(x.Real) && !double.IsInfinity(x.Imag)) {
                throw PythonOps.OverflowError("absolute value too large");
            }

            return res;
        }

        // Binary Operations - Comparisons (eq & ne defined on Complex64 type as operators)

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool LessThan(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool LessThanOrEqual(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), SpecialName]
        public static bool GreaterThan(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), SpecialName]
        public static bool GreaterThanOrEqual(Complex64 x, Complex64 y) {
            throw PythonOps.TypeError("complex is not an ordered type");
        }

    }
}
