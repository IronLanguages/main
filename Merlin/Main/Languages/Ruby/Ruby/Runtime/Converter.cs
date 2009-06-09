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
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime {
    internal static partial class Converter {
        internal static bool CanConvertFrom(Type/*!*/ fromType, Type/*!*/ toType, NarrowingLevel level, bool implicitProtocolConversions) {
            ContractUtils.RequiresNotNull(fromType, "fromType");
            ContractUtils.RequiresNotNull(toType, "toType");

            //
            // narrowing level 0:
            //

            if (toType.IsAssignableFrom(fromType)) {
                return true;
            }

            // A COM object could be cast to any interface:
            if (Utils.IsComObjectType(fromType) && toType.IsInterface) {
                return true; 
            }

            if (HasImplicitNumericConversion(fromType, toType)) {
                return true;
            }

            if (CompilerHelpers.GetImplicitConverter(fromType, toType) != null) {
                return true;
            }

            //
            // narrowing level 1:
            //

            if (level < NarrowingLevel.One) {
                return false;
            }

            if (HasExplicitNumericConversion(fromType, toType)) {
                return true;
            }

            if (CompilerHelpers.GetExplicitConverter(fromType, toType) != null) {
                return true;
            }

            if (CompilerHelpers.HasTypeConverter(fromType, toType)) {
                return true;
            }

            if (fromType == typeof(char) && toType == typeof(string)) {
                return true;
            }

            if (toType == typeof(bool)) {
                return true;
            }

            //
            // narrowing level 2:
            //

            if (level < NarrowingLevel.Two) {
                return false;
            }

            // we can convert any object to any type for which we have a default protocol:
            if (implicitProtocolConversions && ProtocolConversionAction.HasDefaultConversion(toType)) {
                return true;
            }

            // any dynamic object is potentially convertible to any type:
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(fromType)) {
                return true;
            }

            return false;
        }

        internal static Expression ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType, RubyContext/*!*/ context, Expression/*!*/ contextExpression,
            bool implicitProtocolConversions) {

            return
                // narrowing level 0:
                ImplicitConvert(expr, expr.Type, toType) ??
                // narrowing level 1:
                ExplicitConvert(expr, expr.Type, toType) ??
                // narrowing level 2:
                Ast.Dynamic(ProtocolConversionAction.GetConversionAction(context, toType, implicitProtocolConversions), toType, expr);
        }

        internal static Expression ImplicitConvert(Expression/*!*/ expr, Type/*!*/ fromType, Type/*!*/ toType) {
            expr = AstUtils.Convert(expr, fromType);

            // TODO: COM to interface?

            if (toType.IsAssignableFrom(fromType)) {
                return AstUtils.Convert(expr, toType);
            }

            if (HasImplicitNumericConversion(fromType, toType)) {
                return Ast.Convert(expr, toType);
            }

            MethodInfo converter = CompilerHelpers.GetImplicitConverter(fromType, toType);
            if (converter != null) {
                return Ast.Call(null, converter, expr);
            }

            return null;
        }

        internal static Expression ExplicitConvert(Expression/*!*/ expr, Type/*!*/ fromType, Type/*!*/ toType) {
            expr = AstUtils.Convert(expr, fromType);

            if (HasExplicitNumericConversion(fromType, toType)) {
                // special cases to mimic Ruby behavior precisely:
                if (fromType == typeof(BigInteger)) {
                    if (toType == typeof(int)) {
                        return Methods.ConvertBignumToFixnum.OpCall(expr);
                    } else if (toType == typeof(double)) {
                        return Methods.ConvertBignumToFloat.OpCall(expr);
                    }
                } else if (fromType == typeof(double) && toType == typeof(int)) {
                    return Methods.ConvertDoubleToFixnum.OpCall(expr);
                }

                return Ast.ConvertChecked(expr, toType);
            }

            MethodInfo converter = CompilerHelpers.GetExplicitConverter(fromType, toType);
            if (converter != null) {
                return Ast.Call(null, converter, expr);
            }

            if (fromType == typeof(char) && toType == typeof(string)) {
                return Ast.Call(null, fromType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Static), expr);
            }

            if (toType == typeof(bool)) {
                Debug.Assert(fromType != typeof(bool));
                return fromType.IsValueType ? AstUtils.Constant(true) : Ast.NotEqual(expr, AstUtils.Constant(null));
            }

            // TODO:
            //if (TypeConverter...(fromType, toType)) {
            //    return true;
            //}

            return null;
        }

        internal static Candidate PreferConvert(Type t1, Type t2) {
            if (t1 == typeof(bool) && t2 == typeof(int)) return Candidate.Two;
            if (t1 == typeof(Decimal) && t2 == typeof(BigInteger)) return Candidate.Two;
            //if (t1 == typeof(int) && t2 == typeof(BigInteger)) return Candidate.Two;

            switch (Type.GetTypeCode(t1)) {
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }

                case TypeCode.Int16:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }

                case TypeCode.Int32:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }

                case TypeCode.Int64:
                    switch (Type.GetTypeCode(t2)) {
                        case TypeCode.UInt64:
                            return Candidate.Two;
                        default:
                            return Candidate.Equivalent;
                    }
            }
            return Candidate.Equivalent;
        }
    }
}
