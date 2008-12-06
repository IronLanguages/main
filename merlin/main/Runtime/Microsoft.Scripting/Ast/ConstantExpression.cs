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
using System.Linq.Expressions;
using Microsoft.Scripting.Math;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public static partial class Utils {
        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        public static MemberExpression WeakConstant(object value) {
            System.Diagnostics.Debug.Assert(!(value is Expression));
            return Expression.Property(
                Expression.Constant(new WeakReference(value)),
                typeof(WeakReference).GetProperty("Target")
            );
        }

        public static Expression Constant(object value) {
            if (value is SymbolId) {
                return new SymbolConstantExpression((SymbolId)value);
            }
            BigInteger bi = value as BigInteger;
            if ((object)bi != null) {
                return BigIntegerConstant(bi);
            } else if (value is Complex64) {
                return ComplexConstant((Complex64)value);
            } else {
                return Expression.Constant(value);
            }
        }

        private static Expression BigIntegerConstant(BigInteger value) {
            int ival;
            if (value.AsInt32(out ival)) {
                return Expression.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(int) }),
                    Expression.Constant(ival)
                );
            }

            long lval;
            if (value.AsInt64(out lval)) {
                return Expression.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(long) }),
                    Expression.Constant(lval)
                );
            }

            return Expression.New(
                typeof(BigInteger).GetConstructor(new Type[] { typeof(int), typeof(uint[]) }),
                Expression.Constant((int)value.Sign),
                CreateUIntArray(value.GetBits())
            );
        }

        private static Expression CreateUIntArray(uint[] array) {
            Expression[] init = new Expression[array.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = Expression.Constant(array[i]);
            }
            return Expression.NewArrayInit(typeof(uint), init);
        }

        private static Expression ComplexConstant(Complex64 value) {
            if (value.Real != 0.0) {
                if (value.Imag != 0.0) {
                    return Expression.Call(
                        typeof(Complex64).GetMethod("Make"),
                        Expression.Constant(value.Real),
                        Expression.Constant(value.Imag)
                    );
                } else {
                    return Expression.Call(
                        typeof(Complex64).GetMethod("MakeReal"),
                        Expression.Constant(value.Real)
                    );
                }
            } else {
                return Expression.Call(
                    typeof(Complex64).GetMethod("MakeImaginary"),
                    Expression.Constant(value.Imag)
                );
            }
        }
    }
}
