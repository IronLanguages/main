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
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using System.Linq.Expressions;
using System.Diagnostics;
using IronRuby.Compiler;
using System.Reflection;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    internal static class InteropBinder {
        internal sealed class CreateInstance : CreateInstanceBinder {
            private readonly RubyContext/*!*/ _context;

            internal CreateInstance(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public override DynamicMetaObject/*!*/ FallbackCreateInstance(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                return CreateErrorMetaObject(target, args, errorSuggestion);
            }
        }

        internal sealed class Invoke : InvokeBinder {
            private readonly RubyContext/*!*/ _context;

            internal Invoke(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
                DynamicMetaObject errorSuggestion) {

                return CreateErrorMetaObject(target, args, errorSuggestion);
            }

            public static DynamicMetaObject/*!*/ Bind(RubyContext/*!*/ context, InvokeBinder/*!*/ binder,
                RubyMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, Action<MetaObjectBuilder, CallArguments>/*!*/ buildInvoke) {

                RubyCallSignature callSignature;
                if (RubyCallSignature.TryCreate(binder.CallInfo, out callSignature)) {
                    return binder.FallbackInvoke(target, args);
                }

                var metaBuilder = new MetaObjectBuilder();
                buildInvoke(metaBuilder, new CallArguments(target.CreateMetaContext(), target, args, callSignature));
                return metaBuilder.CreateMetaObject(binder);
            }
        }

        internal sealed class InvokeMember : InvokeMemberBinder {
            private readonly RubyContext/*!*/ _context;

            internal InvokeMember(RubyContext/*!*/ context, string/*!*/ name, CallInfo/*!*/ callInfo)
                : base(name, false, callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackInvokeMember(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                var metaBuilder = new MetaObjectBuilder();
                var callArgs = new CallArguments(_context, target, args, CallInfo);

                if (!RubyCallAction.Build(metaBuilder, Name, callArgs, errorSuggestion == null)) {
                    Debug.Assert(errorSuggestion != null);
                    // method wasn't found so we didn't do any operation with arguments that would require restrictions converted to conditions:
                    metaBuilder.SetMetaResult(errorSuggestion, false);
                }

                return metaBuilder.CreateMetaObject(this);
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                var exprs = RubyBinder.ToExpressions(args, -1);
                exprs[0] = target.Expression;

                return new DynamicMetaObject(
                    Expression.Dynamic(new Invoke(_context, CallInfo), typeof(object), exprs), 
                    target.Restrictions.Merge(BindingRestrictions.Combine(args)) // TODO: this should be done everywhere!
                );
            }

            #endregion

            #region DLR -> Ruby

            internal delegate DynamicMetaObject/*!*/ Fallback(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args);

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, CreateInstanceBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/[]/*!*/ args, Fallback/*!*/ fallback) {
                return Bind(context, "new", binder.CallInfo, binder, target, args, fallback);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, InvokeMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/[]/*!*/ args, Fallback/*!*/ fallback) {
                return Bind(context, binder.Name, binder.CallInfo, binder, target, args, fallback);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, string/*!*/ methodName, CallInfo/*!*/ callInfo, 
                DynamicMetaObjectBinder/*!*/ binder, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, Fallback/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var metaBuilder = new MetaObjectBuilder();
                var callArgs = new CallArguments(context, target, args, RubyCallSignature.Simple(callInfo.ArgumentCount));

                if (!RubyCallAction.Build(metaBuilder, methodName, callArgs, false)) {
                    metaBuilder.SetMetaResult(fallback(target, args), false);
                }
                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion
        }

        internal sealed class GetMember : GetMemberBinder {
            private readonly RubyContext/*!*/ _context;

            internal GetMember(RubyContext/*!*/ context, string/*!*/ name)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
                throw new NotImplementedException("TODO");
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, GetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target, 
                Func<DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var metaBuilder = new MetaObjectBuilder();
                var callArgs = new CallArguments(context, target, DynamicMetaObject.EmptyMetaObjects, RubyCallSignature.Simple(0));
                
                RubyMemberInfo methodMissing;
                var method = RubyCallAction.Resolve(metaBuilder, binder.Name, callArgs, out methodMissing);

                if (method.Found) {
                    // create a bound member:
                    metaBuilder.Result = AstUtils.Constant(new RubyMethod(target.Value, method.Info, binder.Name));
                } else {
                    // TODO: support methodMissing
                    // we need to create a wrapper over methodMissing that has curried the method name and calls to methodMissing method:
                    return fallback(target);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion
        }

        internal sealed class Convert : ConvertBinder {
            private readonly RubyContext/*!*/ _context;

            public Convert(RubyContext/*!*/ context, Type/*!*/ type, bool isExplicit)
                : base(type, isExplicit) {
                Assert.NotNull(context);
                _context = context;
            }

            public override DynamicMetaObject/*!*/ FallbackConvert(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
                return CreateErrorMetaObject(target, DynamicMetaObject.EmptyMetaObjects, errorSuggestion);
            }
        }


        // TODO: remove
        internal static DynamicMetaObject/*!*/ CreateErrorMetaObject(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
            DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? DynamicMetaObject.CreateThrow(target, args, typeof(NotImplementedException), ArrayUtils.EmptyObjects);
        }

        // TODO: convert binder
        internal static DynamicMetaObject TryBindCovertToDelegate(RubyMetaObject/*!*/ metaTarget, ConvertBinder/*!*/ binder, MethodInfo/*!*/ delegateFactory) {
            var metaBuilder = new MetaObjectBuilder();
            return TryBuildConversionToDelegate(metaBuilder, metaTarget, binder.Type, delegateFactory) ? metaBuilder.CreateMetaObject(binder) : null;
        }

        internal static bool TryBuildConversionToDelegate(MetaObjectBuilder/*!*/ metaBuilder, RubyMetaObject/*!*/ metaTarget, Type/*!*/ delegateType, MethodInfo/*!*/ delegateFactory) {
            MethodInfo invoke;
            if (!typeof(Delegate).IsAssignableFrom(delegateType) || (invoke = delegateType.GetMethod("Invoke")) == null) {
                return false;
            }

            var type = metaTarget.Value.GetType();
            metaBuilder.AddTypeRestriction(type, metaTarget.Expression);
            metaBuilder.Result = delegateFactory.OpCall(AstUtils.Constant(delegateType), Ast.Convert(metaTarget.Expression, type));
            return true;
        }
    }
}
