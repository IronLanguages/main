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
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    internal interface IInteropBinder {
        RubyContext Context { get; }
    }

    internal static class InteropBinder {
        internal sealed class CreateInstance : CreateInstanceBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal CreateInstance(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackCreateInstance(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                // target is not instantiable meta-object
 
                // TODO:
                // if target.LimitType == System.Type
                //   new {target}(args)
                // else 

                // invoke "new" method:
                return new DynamicMetaObject(
                    Ast.Dynamic(new InvokeMember(_context, "new", CallInfo), typeof(object), target.Expression),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.CreateInstance({0}){1}",
                    CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class Return : InvokeBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal Return(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
                DynamicMetaObject errorSuggestion) {

                // Used in combination with GetMember to compose InvokeMember operation.
                // Gets here only if the target is not a callable meta-object. 

                var metaBuilder = new MetaObjectBuilder(this, target, args);
                var callArgs = new CallArguments(_context, target, args, CallInfo);

                metaBuilder.AddTypeRestriction(target.GetLimitType(), target.Expression);

                var normalizedArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, callArgs, 0, 0);
                if (!metaBuilder.Error) {
                    // no arguments => just return the target:
                    metaBuilder.Result = target.Expression;
                } else {
                    // any arguments found (expected none):
                    metaBuilder.SetMetaResult(errorSuggestion, false);
                }
                
                return metaBuilder.CreateMetaObject(this);
            }
        }

        /// <summary>
        /// Attempts to invoke the member, falls back to InvokeMember("call")
        /// </summary>
        internal sealed class Invoke : InvokeBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;
            private readonly string/*!*/ _fallbackMethod;

            internal Invoke(RubyContext/*!*/ context, string/*!*/ fallbackMethod, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context, fallbackMethod);
                _context = context;
                _fallbackMethod = fallbackMethod;
            }

            public Type/*!*/ ResultType {
                get { return typeof(object); }
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
                DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindInvoke(this, target, args, out result)) {
                    return result;
                }
#endif
                // target is not a callable meta-object
                
                // TODO: target could be a delegate 
                // if (target.LimitType <: Delegate) then
                //   invoke delegate
                // else:

                // invoke the fallback method:
                return new DynamicMetaObject(
                    Ast.Dynamic(new InvokeMember(_context, _fallbackMethod, CallInfo), typeof(object), target.Expression),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
                );
            }

            public static DynamicMetaObject/*!*/ Bind(InvokeBinder/*!*/ binder,
                RubyMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, Action<MetaObjectBuilder, CallArguments>/*!*/ buildInvoke) {

                RubyCallSignature callSignature;
                if (RubyCallSignature.TryCreate(binder.CallInfo, out callSignature)) {
                    return binder.FallbackInvoke(target, args);
                }

                var callArgs = new CallArguments(target.CreateMetaContext(), target, args, callSignature);
                var metaBuilder = new MetaObjectBuilder(target, args);

                buildInvoke(metaBuilder, callArgs);
                return metaBuilder.CreateMetaObject(binder);
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.Invoke({0}){1}",
                    CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal class InvokeMember : InvokeMemberBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal InvokeMember(RubyContext/*!*/ context, string/*!*/ name, CallInfo/*!*/ callInfo)
                : base(name, false, callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackInvokeMember(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindInvokeMember(this, target, args, out result)) {
                    return result;
                }
#endif

                return FallbackInvokeMember(this, Name, CallInfo, target, args, errorSuggestion);
            }

            internal static DynamicMetaObject FallbackInvokeMember(IInteropBinder/*!*/ binder, string/*!*/ methodName, CallInfo/*!*/ callInfo,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, DynamicMetaObject errorSuggestion) {

                var metaBuilder = new MetaObjectBuilder(binder, target, args);
                var callArgs = new CallArguments(binder.Context, target, args, callInfo);

                if (!RubyCallAction.BuildCall(metaBuilder, methodName, callArgs, errorSuggestion == null, true)) {
                    Debug.Assert(errorSuggestion != null);
                    // method wasn't found so we didn't do any operation with arguments that would require restrictions converted to conditions:
                    metaBuilder.SetMetaResult(errorSuggestion, false);
                }

                return metaBuilder.CreateMetaObject((DynamicMetaObjectBinder)binder);
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                var exprs = RubyBinder.ToExpressions(args, -1);
                exprs[0] = target.Expression;

                return new DynamicMetaObject(
                    Expression.Dynamic(new Return(_context, CallInfo), typeof(object), exprs),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args)).
                        // TODO: ???
                        Merge(BindingRestrictions.GetTypeRestriction(target.Expression, target.GetLimitType()))
                );
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, CreateInstanceBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/[]/*!*/ args, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject>/*!*/ fallback) {
                return Bind(context, "new", binder.CallInfo, binder, target, args, fallback);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, InvokeMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/[]/*!*/ args, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject>/*!*/ fallback) {
                return Bind(context, binder.Name, binder.CallInfo, binder, target, args, fallback);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, string/*!*/ methodName, CallInfo/*!*/ callInfo, 
                DynamicMetaObjectBinder/*!*/ binder, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var callArgs = new CallArguments(context, target, args, RubyCallSignature.WithImplicitSelf(callInfo.ArgumentCount));
                var metaBuilder = new MetaObjectBuilder(target, args);

                if (!RubyCallAction.BuildCall(metaBuilder, methodName, callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target, args), false);
                }
                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.InvokeMember({0}, {1}){2}",
                    Name,
                    CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class GetMember : GetMemberBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal GetMember(RubyContext/*!*/ context, string/*!*/ name)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindGetMember(this, target, out result)) {
                    return result;
                }
#endif
                throw new NotImplementedException("TODO");
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, GetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target, 
                Func<DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var callArgs = new CallArguments(context, target, DynamicMetaObject.EmptyMetaObjects, RubyCallSignature.WithImplicitSelf(0));
                var metaBuilder = new MetaObjectBuilder(target);

                if (!RubyCallAction.BuildAccess(metaBuilder, binder.Name, callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target), false);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.GetMember({0}){1}",
                    Name,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class SetMember : SetMemberBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal SetMember(RubyContext/*!*/ context, string/*!*/ name)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackSetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ value, 
                DynamicMetaObject errorSuggestion) {

#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindSetMember(this, target, value, out result)) {
                    return result;
                }
#endif
                throw new NotImplementedException("TODO");
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, SetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/ value, Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var args = new[] { value };
                var callArgs = new CallArguments(context, target, args, RubyCallSignature.WithImplicitSelf(1));
                var metaBuilder = new MetaObjectBuilder(target, args);

                if (!RubyCallAction.BuildCall(metaBuilder, binder.Name + "=", callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target, value), false);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.SetMember({0}){1}",
                    Name,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class GetIndex : GetIndexBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal GetIndex(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetIndex(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes, 
                DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindGetIndex(this, target, indexes, out result)) {
                    return result;
                }
#endif
                // TODO: CLR get index
                // TODO: invoke
                throw new NotImplementedException("TODO");
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, GetIndexBinder/*!*/ binder, 
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes,
                Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);
                
                var callArgs = new CallArguments(context, target, indexes, RubyCallSignature.WithImplicitSelf(indexes.Length));
                var metaBuilder = new MetaObjectBuilder(target, indexes);

                if (!RubyCallAction.BuildCall(metaBuilder, "[]", callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target, indexes), false);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.GetIndex({0}){1}",
                    CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class SetIndex : SetIndexBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            internal SetIndex(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackSetIndex(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes, 
                DynamicMetaObject/*!*/ value, DynamicMetaObject errorSuggestion) {

#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryBindSetIndex(this, target, indexes, value, out result)) {
                    return result;
                }
#endif
                throw new NotImplementedException("TODO");
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, SetIndexBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes, DynamicMetaObject/*!*/ value,
                Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var args = ArrayUtils.Append(indexes, value);
                var callArgs = new CallArguments(context, target, args,
                    new RubyCallSignature(indexes.Length, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasRhsArgument)
                );

                var metaBuilder = new MetaObjectBuilder(target, args);

                if (!RubyCallAction.BuildCall(metaBuilder, "[]=", callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target, indexes, value), false);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.SetIndex({0}){1}",
                    CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class BinaryOperation : BinaryOperationBinder, IInteropBinder {
            private static readonly CallInfo _CallInfo = new CallInfo(1);
            private readonly RubyContext/*!*/ _context;

            internal BinaryOperation(RubyContext/*!*/ context, ExpressionType operation)
                : base(operation) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackBinaryOperation(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ arg, DynamicMetaObject errorSuggestion) {
                return InvokeMember.FallbackInvokeMember(this, RubyClass.MapOperator(Operation), _CallInfo, target, new[] { arg }, errorSuggestion);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, BinaryOperationBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ arg, 
                Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {

                return InvokeMember.Bind(context, RubyClass.MapOperator(binder.Operation), _CallInfo, binder, target, new[] { arg },
                    (trgt, args) => fallback(trgt, args[0])
                );
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.BinaryOperation({0}){1}",
                    Operation,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class UnaryOperation : UnaryOperationBinder, IInteropBinder {
            private static readonly CallInfo _CallInfo = new CallInfo(0);
            private readonly RubyContext/*!*/ _context;

            internal UnaryOperation(RubyContext/*!*/ context, ExpressionType operation)
                : base(operation) {
                Assert.NotNull(context);
                _context = context;
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackUnaryOperation(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
                return InvokeMember.FallbackInvokeMember(this, RubyClass.MapOperator(Operation), _CallInfo, target, DynamicMetaObject.EmptyMetaObjects, errorSuggestion);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, UnaryOperationBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, Func<DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                return InvokeMember.Bind(context, RubyClass.MapOperator(binder.Operation), _CallInfo, binder, target, DynamicMetaObject.EmptyMetaObjects,
                    (trgt, _) => fallback(trgt)
                );
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.UnaryOperation({0}){1}",
                    Operation,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class Convert : ConvertBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;

            public Convert(RubyContext/*!*/ context, Type/*!*/ type, bool isExplicit)
                : base(type, isExplicit) {
                Assert.NotNull(context);
                _context = context;
            }

            public Type/*!*/ ResultType {
                get { return Type; }
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override DynamicMetaObject/*!*/ FallbackConvert(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (System.Dynamic.ComBinder.TryConvert(this, target, out result)) {
                    return result;
                }
#endif

                // TODO:
                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(Methods.MakeTypeConversionError.OpCall(
                        AstUtils.Constant(_context), AstUtils.Convert(target.Expression, typeof(object)), Ast.Constant(ReturnType)
                    ), ReturnType),
                    target.Restrictions
                );
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.Convert({0}, {1}){2}", 
                    Type.Name, 
                    Explicit ? "explicit" : "implicit", 
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }


        // TODO: remove
        internal static DynamicMetaObject/*!*/ CreateErrorMetaObject(this DynamicMetaObjectBinder binder, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
            DynamicMetaObject errorSuggestion) {
            return errorSuggestion ?? new DynamicMetaObject(
                Expression.Throw(Expression.New(typeof(NotImplementedException)), binder.ReturnType),
                target.Restrictions.Merge(BindingRestrictions.Combine(args))
            );
        }

        // TODO: convert binder
        internal static DynamicMetaObject TryBindCovertToDelegate(RubyMetaObject/*!*/ target, ConvertBinder/*!*/ binder, MethodInfo/*!*/ delegateFactory) {
            var metaBuilder = new MetaObjectBuilder(target);
            return TryBuildConversionToDelegate(metaBuilder, target, binder.Type, delegateFactory) ? metaBuilder.CreateMetaObject(binder) : null;
        }

        internal static bool TryBuildConversionToDelegate(MetaObjectBuilder/*!*/ metaBuilder, RubyMetaObject/*!*/ target, Type/*!*/ delegateType, MethodInfo/*!*/ delegateFactory) {
            MethodInfo invoke;
            if (!typeof(Delegate).IsAssignableFrom(delegateType) || (invoke = delegateType.GetMethod("Invoke")) == null) {
                return false;
            }

            var type = target.Value.GetType();
            metaBuilder.AddTypeRestriction(type, target.Expression);
            metaBuilder.Result = delegateFactory.OpCall(AstUtils.Constant(delegateType), Ast.Convert(target.Expression, type));
            return true;
        }
    }
}
