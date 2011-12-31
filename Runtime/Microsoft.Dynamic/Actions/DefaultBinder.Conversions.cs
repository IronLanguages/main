/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    public partial class DefaultBinder : ActionBinder {
        public DynamicMetaObject ConvertTo(Type toType, ConversionResultKind kind, DynamicMetaObject arg) {
            return ConvertTo(toType, kind, arg, new DefaultOverloadResolverFactory(this));
        }

        public DynamicMetaObject ConvertTo(Type toType, ConversionResultKind kind, DynamicMetaObject arg, OverloadResolverFactory resolverFactory) {
            return ConvertTo(toType, kind, arg, resolverFactory, null);
        }

        public DynamicMetaObject ConvertTo(Type toType, ConversionResultKind kind, DynamicMetaObject arg, OverloadResolverFactory resolverFactory, DynamicMetaObject errorSuggestion) {
            ContractUtils.RequiresNotNull(toType, "toType");
            ContractUtils.RequiresNotNull(arg, "arg");

            // try all the conversions - first look for conversions against the expression type,
            // these can be done w/o any additional tests.  Then look for conversions against the 
            // restricted type.
            BindingRestrictions typeRestrictions = arg.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg.Expression, arg.GetLimitType()));

            DynamicMetaObject res = 
                TryConvertToObject(toType, arg.Expression.Type, arg, typeRestrictions) ??
                TryAllConversions(resolverFactory, toType, kind, arg.Expression.Type, typeRestrictions, arg) ??
                TryAllConversions(resolverFactory, toType, kind, arg.GetLimitType(), typeRestrictions, arg) ??
                errorSuggestion ??
                MakeErrorTarget(toType, kind, typeRestrictions, arg);

            if ((kind == ConversionResultKind.ExplicitTry || kind == ConversionResultKind.ImplicitTry) && toType.IsValueType()) {
                res = new DynamicMetaObject(
                    AstUtils.Convert(
                        res.Expression,
                        typeof(object)
                    ),
                    res.Restrictions
                );
            }
            return res;
        }

        #region Conversion attempt helpers

        /// <summary>
        /// Checks if the conversion is to object and produces a target if it is.
        /// </summary>
        private static DynamicMetaObject TryConvertToObject(Type toType, Type knownType, DynamicMetaObject arg, BindingRestrictions restrictions) {
            if (toType == typeof(object)) {
                if (knownType.IsValueType()) {
                    return MakeBoxingTarget(arg, restrictions);
                } else {
                    return new DynamicMetaObject(arg.Expression, restrictions);
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if any conversions are available and if so builds the target for that conversion.
        /// </summary>
        private DynamicMetaObject TryAllConversions(OverloadResolverFactory factory, Type toType, ConversionResultKind kind, Type knownType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            return
                TryAssignableConversion(toType, knownType, restrictions, arg) ??                // known type -> known type
                TryExtensibleConversion(toType, knownType, restrictions, arg) ??                // Extensible<T> -> Extensible<T>.Value
                TryUserDefinedConversion(kind, toType, knownType, restrictions, arg) ??         // op_Implicit
                TryImplicitNumericConversion(toType, knownType, restrictions, arg) ??           // op_Implicit
                TryNullableConversion(factory, toType, kind, knownType, restrictions, arg) ??   // null -> Nullable<T> or T -> Nullable<T>
                TryNullConversion(toType, knownType, restrictions);                             // null -> reference type
        }

        /// <summary>
        /// Checks if the conversion can be handled by a simple cast.
        /// </summary>
        private static DynamicMetaObject TryAssignableConversion(Type toType, Type type, BindingRestrictions restrictions, DynamicMetaObject arg) {
            if (toType.IsAssignableFrom(type) ||
                (type == typeof(DynamicNull) && (toType.IsClass() || toType.IsInterface()))) {
                // MakeSimpleConversionTarget handles the ConversionResultKind check
                return MakeSimpleConversionTarget(toType, restrictions, arg);
            }

            return null;
        }

        /// <summary>
        /// Checks if the conversion can be handled by calling a user-defined conversion method.
        /// </summary>
        private DynamicMetaObject TryUserDefinedConversion(ConversionResultKind kind, Type toType, Type type, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type fromType = GetUnderlyingType(type);

            DynamicMetaObject res =
                   TryOneConversion(kind, toType, type, fromType, "op_Implicit", true, restrictions, arg) ??
                   TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, true, restrictions, arg);

            if (kind == ConversionResultKind.ExplicitCast ||
                kind == ConversionResultKind.ExplicitTry) {
                // finally try explicit conversions
                res = res ??
                    TryOneConversion(kind, toType, type, fromType, "op_Explicit", false, restrictions, arg) ??
                    TryOneConversion(kind, toType, type, fromType, "ConvertTo" + toType.Name, false, restrictions, arg);
            }

            return res;
        }

        /// <summary>
        /// Helper that checkes both types to see if either one defines the specified conversion
        /// method.
        /// </summary>
        private DynamicMetaObject TryOneConversion(ConversionResultKind kind, Type toType, Type type, Type fromType, string methodName, bool isImplicit, BindingRestrictions restrictions, DynamicMetaObject arg) {
            MemberGroup conversions = GetMember(MemberRequestKind.Convert, fromType, methodName);
            DynamicMetaObject res = TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, restrictions, arg);
            if (res != null) {
                return res;
            }

            // then on the type we're trying to convert to
            conversions = GetMember(MemberRequestKind.Convert, toType, methodName);
            return TryUserDefinedConversion(kind, toType, type, conversions, isImplicit, restrictions, arg);
        }

        /// <summary>
        /// Checks if any of the members of the MemberGroup provide the applicable conversion and 
        /// if so uses it to build a conversion rule.
        /// </summary>
        private static DynamicMetaObject TryUserDefinedConversion(ConversionResultKind kind, Type toType, Type type, MemberGroup conversions, bool isImplicit, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type checkType = GetUnderlyingType(type);

            foreach (MemberTracker mt in conversions) {
                if (mt.MemberType != TrackerTypes.Method) continue;

                MethodTracker method = (MethodTracker)mt;

                if (isImplicit && method.Method.IsDefined(typeof(ExplicitConversionMethodAttribute), true)) {
                    continue;
                }

                if (method.Method.ReturnType == toType) {   // TODO: IsAssignableFrom?  IsSubclass?
                    ParameterInfo[] pis = method.Method.GetParameters();

                    if (pis.Length == 1 && pis[0].ParameterType.IsAssignableFrom(checkType)) {
                        // we can use this method
                        if (type == checkType) {
                            return MakeConversionTarget(kind, method, type, isImplicit, restrictions, arg);
                        } else {
                            return MakeExtensibleConversionTarget(kind, method, type, isImplicit, restrictions, arg);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if the conversion is to applicable by extracting the value from Extensible of T.
        /// </summary>
        private static DynamicMetaObject TryExtensibleConversion(Type toType, Type type, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type extensibleType = typeof(Extensible<>).MakeGenericType(toType);
            if (extensibleType.IsAssignableFrom(type)) {
                return MakeExtensibleTarget(extensibleType, restrictions, arg);
            }
            return null;
        }

        /// <summary>
        /// Checks if there's an implicit numeric conversion for primitive data types.
        /// </summary>
        private static DynamicMetaObject TryImplicitNumericConversion(Type toType, Type type, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type checkType = type;
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                checkType = type.GetGenericArguments()[0];
            }

            if (TypeUtils.IsNumeric(toType) && TypeUtils.IsNumeric(checkType)) {
                // check for an explicit conversion
                int toX, toY, fromX, fromY;
                if (TypeUtils.GetNumericConversionOrder(toType.GetTypeCode(), out toX, out toY) &&
                    TypeUtils.GetNumericConversionOrder(checkType.GetTypeCode(), out fromX, out fromY)) {
                    if (TypeUtils.IsImplicitlyConvertible(fromX, fromY, toX, toY)) {
                        // MakeSimpleConversionTarget handles the ConversionResultKind check
                        if (type == checkType) {
                            return MakeSimpleConversionTarget(toType, restrictions, arg);
                        } else {
                            return MakeSimpleExtensibleConversionTarget(toType, restrictions, arg);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks if there's a conversion to/from Nullable of T.
        /// </summary>
        private DynamicMetaObject TryNullableConversion(OverloadResolverFactory factory, Type toType, ConversionResultKind kind, Type knownType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            if (toType.IsGenericType() && toType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                if (knownType == typeof(DynamicNull)) {
                    // null -> Nullable<T>
                    return MakeNullToNullableOfTTarget(toType, restrictions);
                } else if (knownType == toType.GetGenericArguments()[0]) {
                    return MakeTToNullableOfTTarget(toType, knownType, restrictions, arg);
                } else if (kind == ConversionResultKind.ExplicitCast || kind == ConversionResultKind.ExplicitTry) {
                    if (knownType != typeof(object)) {
                        // when doing an explicit cast we'll do things like int -> Nullable<float>
                        return MakeConvertingToTToNullableOfTTarget(factory, toType, kind, restrictions, arg);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks to see if there's a conversion of null to a reference type
        /// </summary>
        private static DynamicMetaObject TryNullConversion(Type toType, Type knownType, BindingRestrictions restrictions) {
            if (knownType == typeof(DynamicNull) && !toType.IsValueType()) {
                return MakeNullTarget(toType, restrictions);
            }
            return null;
        }

        #endregion

        #region Rule production helpers

        /// <summary>
        /// Helper to produce an error when a conversion cannot occur
        /// </summary>
        private DynamicMetaObject MakeErrorTarget(Type toType, ConversionResultKind kind, BindingRestrictions restrictions, DynamicMetaObject arg) {
            DynamicMetaObject target;

            switch (kind) {
                case ConversionResultKind.ImplicitCast:
                case ConversionResultKind.ExplicitCast:
                    target = MakeError(
                        MakeConversionError(toType, arg.Expression),
                        restrictions,
                        toType
                    );
                    break;
                case ConversionResultKind.ImplicitTry:
                case ConversionResultKind.ExplicitTry:
                    target = new DynamicMetaObject(
                        GetTryConvertReturnValue(toType),
                        restrictions
                    );
                    break;
                default:
                    throw new InvalidOperationException(kind.ToString());
            }

            return target;
        }

        /// <summary>
        /// Helper to produce a rule which just boxes a value type
        /// </summary>
        private static DynamicMetaObject MakeBoxingTarget(DynamicMetaObject arg, BindingRestrictions restrictions) {
            // MakeSimpleConversionTarget handles the ConversionResultKind check
            return MakeSimpleConversionTarget(typeof(object), restrictions, arg);
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the helper method to do the convert
        /// </summary>
        private static DynamicMetaObject MakeConversionTarget(ConversionResultKind kind, MethodTracker method, Type fromType, bool isImplicit, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Expression param = AstUtils.Convert(arg.Expression, fromType);

            return MakeConversionTargetWorker(kind, method, isImplicit, restrictions, param);
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the helper method to do the convert
        /// </summary>
        private static DynamicMetaObject MakeExtensibleConversionTarget(ConversionResultKind kind, MethodTracker method, Type fromType, bool isImplicit, BindingRestrictions restrictions, DynamicMetaObject arg) {
            return MakeConversionTargetWorker(kind, method, isImplicit, restrictions, GetExtensibleValue(fromType, arg));
        }

        /// <summary>
        /// Helper to produce a conversion rule by calling the method to do the convert.  This version takes the parameter
        /// to be passed to the conversion function and we call it w/ our own value or w/ our Extensible.Value.
        /// </summary>
        private static DynamicMetaObject MakeConversionTargetWorker(ConversionResultKind kind, MethodTracker method, bool isImplicit, BindingRestrictions restrictions, Expression param) {
            return new DynamicMetaObject(
                WrapForThrowingTry(
                    kind,
                    isImplicit,
                    AstUtils.SimpleCallHelper(
                        method.Method,
                        param
                    ),
                    method.Method.ReturnType
                ),
                restrictions
            );
        }

        /// <summary>
        /// Helper to wrap explicit conversion call into try/catch incase it throws an exception.  If
        /// it throws the default value is returned.
        /// </summary>
        private static Expression WrapForThrowingTry(ConversionResultKind kind, bool isImplicit, Expression ret, Type retType) {
            if (!isImplicit && kind == ConversionResultKind.ExplicitTry) {
                Expression convFailed = GetTryConvertReturnValue(retType);
                ParameterExpression tmp = Ast.Variable(convFailed.Type == typeof(object) ? typeof(object) : ret.Type, "tmp");
                ret = Ast.Block(
                        new ParameterExpression[] { tmp },
                        AstUtils.Try(
                            Ast.Assign(tmp, AstUtils.Convert(ret, tmp.Type))
                        ).Catch(
                            typeof(Exception),
                            Ast.Assign(tmp, convFailed)
                        ),
                        tmp
                     );
            }
            return ret;
        }

        /// <summary>
        /// Helper to produce a rule when no conversion is required (the strong type of the expression
        /// input matches the type we're converting to or has an implicit conversion at the IL level)
        /// </summary>
        private static DynamicMetaObject MakeSimpleConversionTarget(Type toType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            return new DynamicMetaObject(
                AstUtils.Convert(arg.Expression, CompilerHelpers.GetVisibleType(toType)),
                restrictions);

            /*
            if (toType.IsValueType && _rule.ReturnType == typeof(object) && Expression.Type == typeof(object)) {
                // boxed value type is being converted back to object.  We've done 
                // the type check, there's no need to unbox & rebox the value.  infact 
                // it breaks calls on instance methods so we need to avoid it.
                _rule.Target =
                    _rule.MakeReturn(
                        Binder,
                        Expression
                    );
            } 
             * */
        }

        /// <summary>
        /// Helper to produce a rule when no conversion is required from an extensible type's
        /// underlying storage to the type we're converting to.  The type of extensible type
        /// matches the type we're converting to or has an implicit conversion at the IL level.
        /// </summary>
        private static DynamicMetaObject MakeSimpleExtensibleConversionTarget(Type toType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type extType = typeof(Extensible<>).MakeGenericType(toType);

            return new DynamicMetaObject(
                AstUtils.Convert(
                    GetExtensibleValue(extType, arg),
                    toType
                ),
                restrictions
            );
        }

        /// <summary>
        /// Helper to extract the value from an Extensible of T
        /// </summary>
        private static DynamicMetaObject MakeExtensibleTarget(Type extensibleType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            return new DynamicMetaObject(
                Ast.Property(Ast.Convert(arg.Expression, extensibleType), extensibleType.GetInheritedProperties("Value").First()),
                restrictions
            );
        }

        /// <summary>
        /// Helper to convert a null value to nullable of T
        /// </summary>
        private static DynamicMetaObject MakeNullToNullableOfTTarget(Type toType, BindingRestrictions restrictions) {
            return new DynamicMetaObject(
                Ast.Call(typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance").MakeGenericMethod(toType)),
                restrictions
            );
        }

        /// <summary>
        /// Helper to produce the rule for converting T to Nullable of T
        /// </summary>
        private static DynamicMetaObject MakeTToNullableOfTTarget(Type toType, Type knownType, BindingRestrictions restrictions, DynamicMetaObject arg) {
            // T -> Nullable<T>
            return new DynamicMetaObject(
                Ast.New(
                    toType.GetConstructor(new Type[] { knownType }),
                    AstUtils.Convert(arg.Expression, knownType)
                ),
                restrictions
            );
        }

        /// <summary>
        /// Helper to produce the rule for converting T to Nullable of T
        /// </summary>
        private DynamicMetaObject MakeConvertingToTToNullableOfTTarget(OverloadResolverFactory resolverFactory, Type toType, ConversionResultKind kind, BindingRestrictions restrictions, DynamicMetaObject arg) {
            Type valueType = toType.GetGenericArguments()[0];

            // ConvertSelfToT -> Nullable<T>
            if (kind == ConversionResultKind.ExplicitCast) {
                // if the conversion to T fails we just throw
                Expression conversion = ConvertExpression(arg.Expression, valueType, kind, resolverFactory);

                return new DynamicMetaObject(
                    Ast.New(
                        toType.GetConstructor(new Type[] { valueType }),
                        conversion
                    ),
                    restrictions
                );
            } else {
                Expression conversion = ConvertExpression(arg.Expression, valueType, kind, resolverFactory);

                // if the conversion to T succeeds then produce the nullable<T>, otherwise return default(retType)
                ParameterExpression tmp = Ast.Variable(typeof(object), "tmp");
                return new DynamicMetaObject(
                    Ast.Block(
                        new ParameterExpression[] { tmp },
                        Ast.Condition(
                            Ast.NotEqual(
                                Ast.Assign(tmp, conversion),
                                AstUtils.Constant(null)
                            ),
                            Ast.New(
                                toType.GetConstructor(new Type[] { valueType }),
                                Ast.Convert(
                                    tmp,
                                    valueType
                                )
                            ),
                            GetTryConvertReturnValue(toType)
                        )
                    ),
                    restrictions
                );
            }
        }

        /// <summary>
        /// Returns a value which indicates failure when a OldConvertToAction of ImplicitTry or
        /// ExplicitTry.
        /// </summary>
        public static Expression GetTryConvertReturnValue(Type type) {
            Expression res;
            if (type.IsInterface() || type.IsClass()) {
                res = AstUtils.Constant(null, type);
            } else {
                res = AstUtils.Constant(null);
            }

            return res;
        }

        /// <summary>
        /// Helper to extract the Value of an Extensible of T from the
        /// expression being converted.
        /// </summary>
        private static Expression GetExtensibleValue(Type extType, DynamicMetaObject arg) {
            return Ast.Property(
                AstUtils.Convert(
                    arg.Expression,
                    extType
                ),
                extType.GetInheritedProperties("Value").First()
            );
        }

        /// <summary>
        /// Helper that checks if fromType is an Extensible of T or a subtype of 
        /// Extensible of T and if so returns the T.  Otherwise it returns fromType.
        /// 
        /// This is used to treat extensible types the same as their underlying types.
        /// </summary>
        private static Type GetUnderlyingType(Type fromType) {
            Type curType = fromType;
            do {
                if (curType.IsGenericType() && curType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                    fromType = curType.GetGenericArguments()[0];
                }
                curType = curType.GetBaseType();
            } while (curType != null);
            return fromType;
        }

        /// <summary>
        /// Creates a target which returns null for a reference type.
        /// </summary>
        private static DynamicMetaObject MakeNullTarget(Type toType, BindingRestrictions restrictions) {
            return new DynamicMetaObject(
                AstUtils.Constant(null, toType),
                restrictions
            );
        }

        #endregion
    }
}
