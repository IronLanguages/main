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
using System.Reflection;
using Microsoft.Scripting.Math;

namespace Microsoft.Scripting.Ast {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public static partial class Utils {
        private static readonly ConstantExpression TrueLiteral = Expression.Constant(true, typeof(bool));
        private static readonly ConstantExpression FalseLiteral = Expression.Constant(false, typeof(bool));
        private static readonly ConstantExpression NullLiteral = Expression.Constant(null, typeof(object));
        private static readonly ConstantExpression EmptyStringLiteral = Expression.Constant(String.Empty, typeof(string));
        private static readonly ConstantExpression[] IntCache = new ConstantExpression[100];

        /// <summary>
        /// Wraps the given value in a WeakReference and returns a tree that will retrieve
        /// the value from the WeakReference.
        /// </summary>
        public static MemberExpression WeakConstant(object value) {
            System.Diagnostics.Debug.Assert(!(value is Expression));
            return Expression.Property(
                Constant(new WeakReference(value)),
                typeof(WeakReference).GetProperty("Target")
            );
        }

        public static ConstantExpression Constant(object value, Type type) {
            return Expression.Constant(value, type);
        }

        // The helper API should return ConstantExpression after SymbolConstantExpression goes away
        public static Expression Constant(object value) {
            if (value == null) {
                return NullLiteral;
            }
            if (value is SymbolId) {
                return new SymbolConstantExpression((SymbolId)value);
            }

            BigInteger bi = value as BigInteger;
            if ((object)bi != null) {
                return BigIntegerConstant(bi);
            } else if (value is Complex64) {
                return ComplexConstant((Complex64)value);
            } else if (value is Type) {
                return Expression.Constant(value, typeof(Type));
            } else if (value is ConstructorInfo) {
                return Expression.Constant(value, typeof(ConstructorInfo));
            } else if (value is EventInfo) {
                return Expression.Constant(value, typeof(EventInfo));
            } else if (value is FieldInfo) {
                return Expression.Constant(value, typeof(FieldInfo));
            } else if (value is MethodInfo) {
                return Expression.Constant(value, typeof(MethodInfo));
            } else if (value is PropertyInfo) {
                return Expression.Constant(value, typeof(PropertyInfo));
            } else {
                Type t = value.GetType();
                if (!t.IsEnum) {
                    switch (Type.GetTypeCode(t)) {
                        case TypeCode.Boolean:
                            return (bool)value ? TrueLiteral : FalseLiteral;
                        case TypeCode.Int32:
                            int x = (int)value;
                            int cacheIndex = x + 2;
                            if (cacheIndex >= 0 && cacheIndex < IntCache.Length) {
                                ConstantExpression res;
                                if ((res = IntCache[cacheIndex]) == null) {
                                    IntCache[cacheIndex] = res = Constant(x, typeof(int));
                                }
                                return res;
                            }
                            break;
                        case TypeCode.String:
                            if (String.IsNullOrEmpty((string)value)) {
                                return EmptyStringLiteral;
                            }
                            break;
                    }
                }
                return Expression.Constant(value);
            }
        }

        private static Expression BigIntegerConstant(BigInteger value) {
            int ival;
            if (value.AsInt32(out ival)) {
                return Expression.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(int) }),
                    Constant(ival)
                );
            }

            long lval;
            if (value.AsInt64(out lval)) {
                return Expression.Call(
                    typeof(BigInteger).GetMethod("Create", new Type[] { typeof(long) }),
                    Constant(lval)
                );
            }

            return Expression.New(
                typeof(BigInteger).GetConstructor(new Type[] { typeof(int), typeof(uint[]) }),
                Constant((int)value.Sign),
                CreateUIntArray(value.GetBits())
            );
        }

        private static Expression CreateUIntArray(uint[] array) {
            Expression[] init = new Expression[array.Length];
            for (int i = 0; i < init.Length; i++) {
                init[i] = Constant(array[i]);
            }
            return Expression.NewArrayInit(typeof(uint), init);
        }

        private static Expression ComplexConstant(Complex64 value) {
            if (value.Real != 0.0) {
                if (value.Imag != 0.0) {
                    return Expression.Call(
                        typeof(Complex64).GetMethod("Make"),
                        Constant(value.Real),
                        Constant(value.Imag)
                    );
                } else {
                    return Expression.Call(
                        typeof(Complex64).GetMethod("MakeReal"),
                        Constant(value.Real)
                    );
                }
            } else {
                return Expression.Call(
                    typeof(Complex64).GetMethod("MakeImaginary"),
                    Constant(value.Imag)
                );
            }
        }
    }
}
