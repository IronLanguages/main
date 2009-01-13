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
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;

namespace IronRuby.Runtime.Calls {
    public sealed class RubyBinder : DefaultBinder {
        internal RubyBinder(ScriptDomainManager/*!*/ manager)
            : base(manager) {
        }

        protected override int PrepareParametersBinding(ParameterInfo/*!*/[]/*!*/ parameterInfos, List<ArgBuilder>/*!*/ arguments,
            List<ParameterWrapper>/*!*/ parameters, ref int index) {
            
            var i = 0;

            while (i < parameterInfos.Length
                && parameterInfos[i].ParameterType.IsSubclassOf(typeof(SiteLocalStorage))) {

                arguments.Add(new SiteLocalStorageBuilder(parameterInfos[i]));
                i++;
            }

            if (i < parameterInfos.Length) {
                var parameterInfo = parameterInfos[i];

                if (parameterInfo.ParameterType == typeof(RubyScope)) {
                    arguments.Add(new RubyScopeArgBuilder(parameterInfo));
                    i++;
                } else if (parameterInfo.ParameterType == typeof(RubyContext)) {
                    arguments.Add(new RubyContextArgBuilder(parameterInfo));
                    i++;
                }
            }

            // If the method overload doesn't have a BlockParam parameter, we inject MissingBlockParam parameter and arg builder.
            // The parameter is treated as a regular explicit mandatory parameter.
            //
            // The argument builder provides no value for the actual argument expression, which makes the default binder to skip it
            // when emitting a tree for the actual method call (this is necessary since the method doesn't in fact have the parameter).
            // 
            // By injecting the missing block parameter we achieve that all overloads have either BlockParam, [NotNull]BlockParam or 
            // MissingBlockParam parameter. MissingBlockParam and BlockParam are convertible to each other. Default binder prefers 
            // those overloads where no conversion needs to happen, which ensures the desired semantics:
            //
            //                                        conversions with desired priority (the less number the higher priority)
            // Parameters:                call w/o block      call with non-null block       call with null block
            // (implicit, MBP, ... )      MBP -> MBP (1)            BP -> MBP (3)               BP -> MBP (2)
            // (implicit, BP,  ... )      MBP -> BP  (2)            BP -> BP  (2)               BP -> BP  (1)
            // (implicit, BP!, ... )          N/A                   BP -> BP! (1)                  N/A    
            //
            if (i >= parameterInfos.Length || parameterInfos[i].ParameterType != typeof(BlockParam)) {
                arguments.Add(new MissingBlockArgBuilder(index++));
                parameters.Add(new ParameterWrapper(this, typeof(MissingBlockParam), SymbolId.Empty, false));
            }

            return i;
        }

        internal static void GetParameterCount(ParameterInfo/*!*/[]/*!*/ parameterInfos, out int mandatory, out int optional, out bool acceptsBlock) {
            acceptsBlock = false;
            mandatory = 0;
            optional = 0;
            foreach (ParameterInfo parameterInfo in parameterInfos) {
                if (IsHiddenParameter(parameterInfo)) {
                    continue;
                } else if (parameterInfo.ParameterType == typeof(BlockParam)) {
                    acceptsBlock = true;
                } else if (CompilerHelpers.IsParamArray(parameterInfo)) {
                    // TODO: indicate splat args separately?
                    optional++;
                } else if (CompilerHelpers.IsOutParameter(parameterInfo)) {
                    // Python allows passing of optional "clr.Reference" to capture out parameters
                    // Ruby should allow similar
                    optional++;
                } else if (CompilerHelpers.IsMandatoryParameter(parameterInfo)) {
                    mandatory++;
                } else {
                    optional++;
                }
            }
        }

        internal static bool IsHiddenParameter(ParameterInfo/*!*/ parameterInfo) {
            return parameterInfo.ParameterType == typeof(RubyScope)
                || parameterInfo.ParameterType == typeof(RubyContext)
                || parameterInfo.ParameterType.IsSubclassOf(typeof(SiteLocalStorage));
        }

        internal sealed class RubyContextArgBuilder : ArgBuilder {
            public RubyContextArgBuilder(ParameterInfo/*!*/ info) 
                : base(info) {
            }

            public override int Priority {
                get { return -1; }
            }

            protected override Expression ToExpression(ParameterBinder/*!*/ parameterBinder, IList<Expression>/*!*/ parameters, bool[]/*!*/ hasBeenUsed) {
                return ((RubyParameterBinder)parameterBinder).ContextExpression;
            }
        }

        internal sealed class RubyScopeArgBuilder : ArgBuilder {
            public RubyScopeArgBuilder(ParameterInfo/*!*/ info)
                : base(info) {
            }

             public override int Priority {
                get { return -1; }
            }

            protected override Expression ToExpression(ParameterBinder/*!*/ parameterBinder, IList<Expression>/*!*/ parameters, bool[]/*!*/ hasBeenUsed) {
                 return ((RubyParameterBinder)parameterBinder).ScopeExpression;
            }
        }

        internal sealed class MissingBlockArgBuilder : SimpleArgBuilder {
            public MissingBlockArgBuilder(int index)
                : base(typeof(MissingBlockParam), index, false, false) {
            }

            public override int Priority {
                get { return -1; }
            }

            protected override SimpleArgBuilder/*!*/ Copy(int newIndex) {
                return new MissingBlockArgBuilder(newIndex);
            }

            protected override Expression ToExpression(ParameterBinder/*!*/ parameterBinder, IList<Expression>/*!*/ parameters, bool[]/*!*/ hasBeenUsed) {
                Debug.Assert(Index < parameters.Count);
                Debug.Assert(Index < hasBeenUsed.Length);
                Debug.Assert(parameters[Index] != null);
                hasBeenUsed[Index] = true;
                return null;
            }
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
                        Ast.Constant(toType)
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
                    AstUtils.Convert(Ast.Constant(toType), typeof(Type)),
                    typeof(Type).GetMethod("IsInstanceOfType"),
                    AstUtils.Convert(expr, typeof(object))
                );
            }

            Expression convertExpr = null;
            if (expr.Type == typeof(DynamicNull)) {
                convertExpr = Ast.Default(visType);
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
                        Ast.Constant(visType.TypeHandle)
                    ),
                    visType
                )
            );
        }

        public override bool CanConvertFrom(Type/*!*/ fromType, Type/*!*/ toType, bool toNotNullable, NarrowingLevel level) {
            return Converter.CanConvertFrom(fromType, toType, level);
        }

        public override bool CanConvertFrom(Type/*!*/ fromType, ParameterWrapper/*!*/ toParameter, NarrowingLevel level) {
            Type toType = toParameter.Type;

            if (base.CanConvertFrom(fromType, toParameter, level)) {
                return true;
            }

            // blocks:
            if (fromType == typeof(MissingBlockParam)) {
                return toType == typeof(BlockParam) && !toParameter.ProhibitNull;
            }

            if (fromType == typeof(BlockParam) && toType == typeof(MissingBlockParam)) {
                return true;
            }

            // protocol conversions:
            if (toParameter.ParameterInfo != null && toParameter.ParameterInfo.IsDefined(typeof(DefaultProtocolAttribute), false)) {
                // any type is potentially convertible, except for nil if [NotNull] is used:
                return fromType != typeof(DynamicNull) || !toParameter.ProhibitNull;
            }

            return false;
        }

        public override Candidate SelectBestConversionFor(Type/*!*/ actualType, ParameterWrapper/*!*/ candidateOne, ParameterWrapper/*!*/ candidateTwo, NarrowingLevel level) {
            Type typeOne = candidateOne.Type;
            Type typeTwo = candidateTwo.Type;

            if (actualType == typeof(DynamicNull)) {
                // if nil is passed as a block argument prefer BlockParam over missing block;
                if (typeOne == typeof(BlockParam) && typeTwo == typeof(MissingBlockParam)) {
                    return Candidate.One;
                }

                if (typeOne == typeof(MissingBlockParam) && typeTwo == typeof(BlockParam)) {
                    return Candidate.Two;
                }
            } else {
                if (actualType == typeOne && candidateOne.ProhibitNull) {
                    return Candidate.One;
                }

                if (actualType == typeTwo && candidateTwo.ProhibitNull) {
                    return Candidate.Two;
                }
            }

            if (actualType == typeOne) {
                return Candidate.One;
            }

            if (actualType == typeTwo) {
                return Candidate.Two;
            }


            return Candidate.Equivalent;
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

        internal static Expression/*!*/[]/*!*/ ToExpressions(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new Expression[args.Length - start];
            for (int i = 0; i < result.Length; i++) {
                result[i] = args[start + i].Expression;
            }
            return result;
        }

        internal static object/*!*/[]/*!*/ ToValues(DynamicMetaObject/*!*/[]/*!*/ args, int start) {
            var result = new object[args.Length - start];
            for (int i = 0; i < result.Length; i++) {
                result[i] = args[start + i].Value;
            }
            return result;
        }

        internal static DynamicMetaObject TryBindCovertToDelegate(ConvertBinder/*!*/ action, DynamicMetaObject/*!*/ target) {
            if (typeof(Delegate).IsAssignableFrom(action.Type)) {
                return new DynamicMetaObject(
                    Methods.CreateDelegateFromMethod.OpCall(
                        Ast.Constant(action.Type),
                        AstUtils.Convert(target.Expression, typeof(RubyMethod))
                    ),
                    target.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.Value.GetType()))
                );
            }
            return null;
        }

        #endregion
        
    }
}
