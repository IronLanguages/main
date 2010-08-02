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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime.Conversions {
    using Ast = Expression;
    
    public interface IConvertibleRubyMetaObject : IConvertibleMetaObject {
        Convertibility IsConvertibleTo(Type type, bool isExplicit);
    }

    public struct Convertibility {
        public static readonly Convertibility NotConvertible = new Convertibility(false, null);
        public static readonly Convertibility AlwaysConvertible = new Convertibility(true, null);

        public readonly bool IsConvertible;
        public readonly Expression Assumption;

        public Convertibility(bool isConvertible, Expression assumption) {
            IsConvertible = isConvertible;
            Assumption = assumption;
        }
    }

    internal static partial class Converter {
        internal static Convertibility CanConvertFrom(DynamicMetaObject fromArg, Type/*!*/ fromType, Type/*!*/ toType, bool toNotNullable,
            NarrowingLevel level, bool explicitProtocolConversions, bool implicitProtocolConversions) {
            ContractUtils.RequiresNotNull(fromType, "fromType");
            ContractUtils.RequiresNotNull(toType, "toType");

            var metaConvertible = fromArg as IConvertibleMetaObject;
            var rubyMetaConvertible = fromArg as IConvertibleRubyMetaObject;

            //
            // narrowing level 0:
            //

            if (toType == fromType) {
                return Convertibility.AlwaysConvertible;
            }

            if (fromType == typeof(DynamicNull)) {
                if (toNotNullable) {
                    return Convertibility.NotConvertible;
                }

                if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return Convertibility.AlwaysConvertible;
                }

                if (!toType.IsValueType) {
                    // null convertible to any reference type:
                    return Convertibility.AlwaysConvertible;
                } else if (toType == typeof(bool)) {
                    return Convertibility.AlwaysConvertible;
                } else if (!ProtocolConversionAction.HasDefaultConversion(toType)) {
                    // null not convertible to a value type unless a protocol conversion is allowed:
                    return Convertibility.NotConvertible;
                }
            }

            // blocks:
            if (fromType == typeof(MissingBlockParam)) {
                return new Convertibility(toType == typeof(BlockParam) && !toNotNullable, null);
            }

            if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam)) {
                return Convertibility.AlwaysConvertible;
            }

            if (toType.IsAssignableFrom(fromType)) {
                return Convertibility.AlwaysConvertible;
            }

            if (HasImplicitNumericConversion(fromType, toType)) {
                return Convertibility.AlwaysConvertible;
            }

            if (CompilerHelpers.GetImplicitConverter(fromType, toType) != null) {
                return Convertibility.AlwaysConvertible;
            }

            if (rubyMetaConvertible != null) {
                return rubyMetaConvertible.IsConvertibleTo(toType, false);
            } else if (metaConvertible != null) {
                return new Convertibility(metaConvertible.CanConvertTo(toType, false), null);
            }

            //
            // narrowing level 1:
            //

            if (level < NarrowingLevel.One) {
                return Convertibility.NotConvertible;
            }

            if (explicitProtocolConversions && ProtocolConversionAction.HasDefaultConversion(toType)) {
                return Convertibility.AlwaysConvertible;
            }

            //
            // narrowing level 2:
            //

            if (level < NarrowingLevel.Two) {
                return Convertibility.NotConvertible;
            }

            if (HasExplicitNumericConversion(fromType, toType)) {
                return Convertibility.AlwaysConvertible;
            }

            if (CompilerHelpers.GetExplicitConverter(fromType, toType) != null) {
                return Convertibility.AlwaysConvertible;
            }

            if (CompilerHelpers.HasTypeConverter(fromType, toType)) {
                return Convertibility.AlwaysConvertible;
            }

            if (fromType == typeof(char) && toType == typeof(string)) {
                return Convertibility.AlwaysConvertible;
            }

            if (toType == typeof(bool)) {
                return Convertibility.AlwaysConvertible;
            }

            if (rubyMetaConvertible != null) {
                return rubyMetaConvertible.IsConvertibleTo(toType, true);
            } else if (metaConvertible != null) {
                return new Convertibility(metaConvertible.CanConvertTo(toType, true), null);
            }

            // 
            // narrowing level 3:
            // 

            if (level < NarrowingLevel.Three) {
                return Convertibility.NotConvertible;
            }

            if (implicitProtocolConversions && ProtocolConversionAction.HasDefaultConversion(toType)) {
                return Convertibility.AlwaysConvertible;
            }

            // A COM object can potentially be converted to the given interface, but might also be not so use this only as the last resort:
            if (TypeUtils.IsComObjectType(fromType) && toType.IsInterface) {
                return Convertibility.AlwaysConvertible;
            }

            return Convertibility.NotConvertible;
        }

        internal static Expression/*!*/ ConvertExpression(Expression/*!*/ expr, Type/*!*/ toType, RubyContext/*!*/ context, Expression/*!*/ contextExpression,
            bool implicitProtocolConversions) {

            return
                ImplicitConvert(expr, expr.Type, toType) ??
                ExplicitConvert(expr, expr.Type, toType) ??
                AstUtils.LightDynamic(ProtocolConversionAction.GetConversionAction(context, toType, implicitProtocolConversions), toType, expr);
        }

        internal static Expression ImplicitConvert(Expression/*!*/ expr, Type/*!*/ fromType, Type/*!*/ toType) {
            expr = AstUtils.Convert(expr, fromType);

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

                case TypeCode.Boolean:
                    if (t2 == typeof(int)) {
                        return Candidate.Two;
                    }
                    return Candidate.Equivalent;

                case TypeCode.Decimal:
                case TypeCode.Double:
                    if (t2 == typeof(BigInteger)) {
                        return Candidate.Two;
                    }
                    return Candidate.Equivalent;

                case TypeCode.Char:
                    if (t2 == typeof(string)) {
                        return Candidate.Two;
                    }
                    return Candidate.Equivalent;
            }
            return Candidate.Equivalent;
        }

        #region Runtime Conversions

        internal static Byte ToByte(int value) {
            if (value >= Byte.MinValue && value <= Byte.MaxValue) {
                return (Byte)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::Byte");
        }

        internal static SByte ToSByte(int value) {
            if (value >= SByte.MinValue && value <= SByte.MaxValue) {
                return (SByte)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::SByte");
        }

        internal static Int16 ToInt16(int value) {
            if (value >= Int16.MinValue && value <= Int16.MaxValue) {
                return (Int16)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::Int16");
        }

        internal static UInt16 ToUInt16(int value) {
            if (value >= UInt16.MinValue && value <= UInt16.MaxValue) {
                return (UInt16)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt16");
        }

        internal static UInt32 ToUInt32(int value) {
            if (value >= UInt32.MinValue) {
                return (UInt32)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt32");
        }

        internal static UInt32 ToUInt32(BigInteger value) {
            UInt32 result;
            if (value.AsUInt32(out result)) {
                return result;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt32");
        }

        internal static Int64 ToInt64(BigInteger value) {
            Int64 result;
            if (value.AsInt64(out result)) {
                return result;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::Int64");
        }

        internal static UInt64 ToUInt64(int value) {
            if (value >= 0) {
                return (UInt64)value;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt64");
        }

        internal static UInt64 ToUInt64(BigInteger value) {
            UInt64 result;
            if (value.AsUInt64(out result)) {
                return result;
            }
            throw RubyExceptions.CreateRangeError("number too big to convert into System::UInt64");
        }

        #endregion
    }
}
