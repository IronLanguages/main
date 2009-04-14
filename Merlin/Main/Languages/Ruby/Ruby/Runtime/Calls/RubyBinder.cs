/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Dynamic;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using Actions = Microsoft.Scripting.Actions;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime.Calls {
    public sealed class RubyBinder : DefaultBinder {
        private readonly RubyContext/*!*/ _context;

        internal RubyBinder(RubyContext/*!*/ context)
            : base(context.DomainManager) {
            _context = context;
        }

        public override string GetTypeName(Type t) {
            return _context.GetTypeName(t);
        }



#if TODO
        protected override IList<Type>/*!*/ GetExtensionTypes(Type/*!*/ t) {
            Type extentionType = _rubyContext.RubyContext.GetClass(t).ExtensionType;

            if (extentionType != null) {
                List<Type> result = new List<Type>();
                result.Add(extentionType);
                result.AddRange(base.GetExtensionTypes(t));
                return result;
            }

            return base.GetExtensionTypes(t);
        }
#endif

        #region Conversions

        public override object Convert(object obj, Type/*!*/ toType) {
            Assert.NotNull(toType);
            return Converter.Convert(obj, toType);
        }

        public override Expression ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType, ConversionResultKind kind, Expression context) {
            Type fromType = expr.Type;

            if (toType == typeof(object)) {
                if (fromType.IsValueType) {
                    return Ast.Convert(expr, toType);
                } else {
                    return expr;
                }
            }

            if (toType.IsAssignableFrom(fromType)) {
                return expr;
            }

            // We used to have a special case for int -> double...
            if (fromType != typeof(object) && fromType.IsValueType) {
                expr = Ast.Convert(expr, typeof(object));
            }

            MethodInfo fastConvertMethod = GetFastConvertMethod(toType);
            if (fastConvertMethod != null) {
                return Ast.Call(fastConvertMethod, AstUtils.Convert(expr, typeof(object)));
            }

            if (typeof(Delegate).IsAssignableFrom(toType)) {
                return Ast.Convert(
                    Ast.Call(
                        typeof(Converter).GetMethod("ConvertToDelegate"),
                        AstUtils.Convert(expr, typeof(object)),
                        AstUtils.Constant(toType)
                    ),
                    toType
                );
            }

            Expression typeIs;
            Type visType = CompilerHelpers.GetVisibleType(toType);
            if (toType.IsVisible) {
                typeIs = Ast.TypeIs(expr, toType);
            } else {
                typeIs = Ast.Call(
                    AstUtils.Convert(AstUtils.Constant(toType), typeof(Type)),
                    typeof(Type).GetMethod("IsInstanceOfType"),
                    AstUtils.Convert(expr, typeof(object))
                );
            }

            Expression convertExpr = null;
            if (expr.Type == typeof(DynamicNull)) {
                convertExpr = AstUtils.Default(visType);
            } else {
                convertExpr = Ast.Convert(expr, visType);
            }

            return Ast.Condition(
                typeIs,
                convertExpr,
                Ast.Convert(
                    Ast.Call(
                        GetGenericConvertMethod(visType),
                        AstUtils.Convert(expr, typeof(object)),
                        AstUtils.Constant(visType.TypeHandle)
                    ),
                    visType
                )
            );
        }

        public override bool CanConvertFrom(Type/*!*/ fromType, Type/*!*/ toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level);
        }

        public override Candidate PreferConvert(Type t1, Type t2) {
            return Converter.PreferConvert(t1, t2);
            //            return t1 == t2;
        }



        internal static MethodInfo/*!*/ GetGenericConvertMethod(Type/*!*/ toType) {
            if (toType.IsValueType) {
                if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return typeof(Converter).GetMethod("ConvertToNullableType");
                } else {
                    return typeof(Converter).GetMethod("ConvertToValueType");
                }
            } else {
                return typeof(Converter).GetMethod("ConvertToReferenceType");
            }
        }

        internal static MethodInfo GetFastConvertMethod(Type/*!*/ toType) {
            if (toType == typeof(char)) {
                return typeof(Converter).GetMethod("ConvertToChar");
            } else if (toType == typeof(int)) {
                return typeof(Converter).GetMethod("ConvertToInt32");
            } else if (toType == typeof(string)) {
                return typeof(Converter).GetMethod("ConvertToString");
            } else if (toType == typeof(long)) {
                return typeof(Converter).GetMethod("ConvertToInt64");
            } else if (toType == typeof(double)) {
                return typeof(Converter).GetMethod("ConvertToDouble");
            } else if (toType == typeof(bool)) {
                return typeof(Converter).GetMethod("ConvertToBoolean");
            } else if (toType == typeof(BigInteger)) {
                return typeof(Converter).GetMethod("ConvertToBigInteger");
            } else if (toType == typeof(Complex64)) {
                return typeof(Converter).GetMethod("ConvertToComplex64");
            } else if (toType == typeof(IEnumerable)) {
                return typeof(Converter).GetMethod("ConvertToIEnumerable");
            } else if (toType == typeof(float)) {
                return typeof(Converter).GetMethod("ConvertToSingle");
            } else if (toType == typeof(byte)) {
                return typeof(Converter).GetMethod("ConvertToByte");
            } else if (toType == typeof(sbyte)) {
                return typeof(Converter).GetMethod("ConvertToSByte");
            } else if (toType == typeof(short)) {
                return typeof(Converter).GetMethod("ConvertToInt16");
            } else if (toType == typeof(uint)) {
                return typeof(Converter).GetMethod("ConvertToUInt32");
            } else if (toType == typeof(ulong)) {
                return typeof(Converter).GetMethod("ConvertToUInt64");
            } else if (toType == typeof(ushort)) {
                return typeof(Converter).GetMethod("ConvertToUInt16");
            } else if (toType == typeof(Type)) {
                return typeof(Converter).GetMethod("ConvertToType");
            } else {
                return null;
            }
        }

        #endregion

        #region MetaObjects

        // negative start reserves as many slots at the beginning of the new array:
        internal static Expression/*!*/[]/*!*/ ToExpressions(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new Expression[args.Length - start];
            for (int i = Math.Max(0, -start); i < result.Length; i++) {
                result[i] = args[start + i].Expression;
            }
            return result;
        }

        // negative start reserves as many slots at the beginning of the new array:
        internal static object/*!*/[]/*!*/ ToValues(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new object[args.Length - start];
            for (int i = Math.Max(0, -start); i < result.Length; i++) {
                result[i] = args[start + i].Value;
            }
            return result;
        }

        #endregion
        
    }
}
