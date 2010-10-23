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
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    using AstExpressions = Microsoft.Scripting.Ast.ExpressionCollectionBuilder;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

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

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackCreateInstance(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                return InvokeMember.FallbackInvokeMember(this, "new", CallInfo, target, args, errorSuggestion, null);                
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

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
                DynamicMetaObject errorSuggestion) {

                // Used in combination with GetMember to compose InvokeMember operation.
                // Gets here only if the target is not a callable meta-object. 

                var metaBuilder = new MetaObjectBuilder(this, target, args);
                var callArgs = new CallArguments(_context, target, args, CallInfo);

                metaBuilder.AddTypeRestriction(target.GetLimitType(), target.Expression);

                RubyOverloadResolver.NormalizeArguments(metaBuilder, callArgs, 0, 0);
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

            internal Invoke(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public Type/*!*/ ResultType {
                get { return typeof(object); }
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, 
                DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindInvoke(this, target, InplaceConvertComArguments(args), out result)) {
                    return result;
                }
#endif
                return InvokeMember.FallbackInvokeMember(this, "call", CallInfo, target, args, errorSuggestion, null);
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
            
            // GetMember on original name is cached on context, the alternative name binder is stored here:
            private readonly InvokeMember _unmangled;
            private readonly string _originalName;

            internal InvokeMember(RubyContext/*!*/ context, string/*!*/ name, CallInfo/*!*/ callInfo, string originalName)
                : base(name, false, callInfo) {
                Assert.NotNull(context);
                _context = context;
                _originalName = originalName;

                if (originalName == null) {
                    string unmangled = RubyUtils.TryUnmangleMethodName(Name);
                    if (unmangled != null) {
                        _unmangled = new InvokeMember(_context, unmangled, CallInfo, Name);
                    }
                }
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackInvokeMember(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindInvokeMember(this, target, InplaceConvertComArguments(args), out result)) {
                    return result;
                }
#endif

                return FallbackInvokeMember(this, _originalName ?? Name, CallInfo, target, args, errorSuggestion, _unmangled);
            }

            internal static DynamicMetaObject FallbackInvokeMember(IInteropBinder/*!*/ binder, string/*!*/ methodName, CallInfo/*!*/ callInfo,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args, DynamicMetaObject errorSuggestion, InvokeMember alternateBinder) {

                var metaBuilder = new MetaObjectBuilder(binder, target, args);
                var callArgs = new CallArguments(binder.Context, target, args, callInfo);

                //
                // If we are called with no errorSuggestion we attempt to bind the alternate name since the current binding failed 
                // (unless we successfully bind to a COM object method or Ruby/CLR method defined on the target meta-object).
                // If we already have an errorSuggestion we use it as it represents a valid binding and no alternate name lookups are thus necessary.
                //
                // For example, DynamicObject InvokeMember calls our FallbackInvokeMember 4 times:
                //
                // 1) binder.fallback(..., errorSuggestion: null)
                //    -> DynamicObject.BindInvokeMember(altBinder)
                //       2) altBinder.fallback(..., errorSuggestion: null) 
                //          -> [[ error ]]
                //       
                //       3) altBinder.fallback(..., errorSuggestion: [[
                //                                                     TryInvokeMember(altName, out result)   
                //                                                       ? result                           
                //                                                       : TryGetMember(altName, out result) 
                //                                                           ? altBinder.FallbackInvoke(result)
                //                                                           : [[ error ]]
                //                                                   ]])
                //          -> errorSuggestion
                //
                // 4) binder.fallback(..., errorSuggestion: [[
                //                                            TryInvokeMember(name, out result)   
                //                                              ? result                           
                //                                              : TryGetMember(name, out result) 
                //                                                  ? binder.FallbackInvoke(result)
                //                                                    TryInvokeMember(altName, out result)   
                //                                                      ? result                           
                //                                                      : TryGetMember(altName, out result) 
                //                                                          ? altBinder.FallbackInvoke(result)
                //                                                          : [[ error ]]
                //
                //                                          ]])
                // -> errorSuggestion
                //
                bool tryAlternateBinding = alternateBinder != null && errorSuggestion == null;

                if (!RubyCallAction.BuildCall(metaBuilder, methodName, callArgs, errorSuggestion == null && !tryAlternateBinding, true)) {
                    Debug.Assert(errorSuggestion != null || tryAlternateBinding);
                    if (tryAlternateBinding) {
                        metaBuilder.SetMetaResult(target.BindInvokeMember(alternateBinder, args), true);
                    } else {
                        // method wasn't found so we didn't do any operation with arguments that would require restrictions converted to conditions:
                        metaBuilder.SetMetaResult(errorSuggestion, false);
                    }
                }

                return metaBuilder.CreateMetaObject((DynamicMetaObjectBinder)binder);
            }

            public override DynamicMetaObject/*!*/ FallbackInvoke(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args,
                DynamicMetaObject errorSuggestion) {

                AstExpressions exprs = new AstExpressions();
                exprs.Add(target.Expression);
                exprs.Add(args.ToExpressions());

                return new DynamicMetaObject(
                    AstUtils.LightDynamic(_context.MetaBinderFactory.InteropReturn(CallInfo), typeof(object), exprs),
                    target.Restrictions.Merge(BindingRestrictions.Combine(args))
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

                var callArgs = new CallArguments(context, target, args, RubyCallSignature.Interop(callInfo.ArgumentCount));
                var metaBuilder = new MetaObjectBuilder(target, args);

                if (!RubyCallAction.BuildCall(metaBuilder, methodName, callArgs, false, false)) {
                    // TODO: error suggestion?
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

        internal sealed class GetMember : GetMemberBinder, IInteropBinder, IInvokeOnGetBinder {
            private static readonly CallInfo _CallInfo = new CallInfo(0);
            private readonly RubyContext/*!*/ _context;

            // GetMember on original name is cached on context, the alternative name binder is stored here:
            private readonly GetMember _unmangled;
            private readonly string _originalName;

            internal GetMember(RubyContext/*!*/ context, string/*!*/ name, string originalName)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
                _originalName = originalName;

                if (originalName == null) {
                    string unmangled = RubyUtils.TryUnmangleMethodName(name);
                    if (unmangled != null) {
                        _unmangled = new GetMember(_context, unmangled, originalName);
                    }
                }
            }

            public RubyContext Context {
                get { return _context; }
            }

            bool IInvokeOnGetBinder.InvokeOnGet {
                get { return false; }
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetMember(this, target, out result)) {
                    return result;
                }
#endif

                var metaBuilder = new MetaObjectBuilder(target);
                var callArgs = new CallArguments(_context, target, DynamicMetaObject.EmptyMetaObjects, _CallInfo);
                
                // See InvokeMember binder for explanation.
                bool tryAlternateBinding = _unmangled != null && errorSuggestion == null;

                if (!RubyCallAction.BuildAccess(metaBuilder, _originalName ?? Name, callArgs, errorSuggestion == null && !tryAlternateBinding, true)) {
                    Debug.Assert(errorSuggestion != null || tryAlternateBinding);
                    if (tryAlternateBinding) {
                        metaBuilder.SetMetaResult(target.BindGetMember(_unmangled), true);
                    } else {
                        // method wasn't found so we didn't do any operation with arguments that would require restrictions converted to conditions:
                        metaBuilder.SetMetaResult(errorSuggestion, false);
                    }
                }

                return metaBuilder.CreateMetaObject(this);
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, GetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                Func<DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var callArgs = new CallArguments(context, target, DynamicMetaObject.EmptyMetaObjects, RubyCallSignature.Interop(0));
                var metaBuilder = new MetaObjectBuilder(target);

                if (!RubyCallAction.BuildAccess(metaBuilder, binder.Name, callArgs, false, false)) {
                    // TODO: error suggestion?
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

        /// <summary>
        /// GetMember with a fallback that returns OperationFailed singleton.
        /// No name mangling is performed.
        /// </summary>
        internal sealed class TryGetMemberExact : GetMemberBinder, IInvokeOnGetBinder {
            private readonly RubyContext/*!*/ _context;

            internal TryGetMemberExact(RubyContext/*!*/ context, string/*!*/ name)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
            }

            bool IInvokeOnGetBinder.InvokeOnGet {
                get { return false; }
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetMember(this, target, out result)) {
                    return result;
                }
#endif

                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Constant(OperationFailed.Value, typeof(object)),
                    target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions
                );
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.TryGetMemberExact({0})", Name);
            }
        }

        internal sealed class SetMember : DynamicMetaObjectBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;
            private readonly string/*!*/ _name;
            private readonly TryGetMemberExact/*!*/ _tryGetMember;
            private readonly SetMemberExact/*!*/ _setMember;
            private readonly SetMemberExact _setMemberUnmangled;
            private readonly TryGetMemberExact/*!*/ _tryGetMemberUnmangled;

            internal SetMember(RubyContext/*!*/ context, string/*!*/ name) {
                Assert.NotNull(context, name);

                _name = name;
                _context = context;
                _tryGetMember = context.MetaBinderFactory.InteropTryGetMemberExact(name);
                _setMember = context.MetaBinderFactory.InteropSetMemberExact(name);

                string unmanagled = RubyUtils.TryUnmangleMethodName(name);
                if (unmanagled != null) {
                    _setMemberUnmangled = context.MetaBinderFactory.InteropSetMemberExact(unmanagled);
                    _tryGetMemberUnmangled = context.MetaBinderFactory.InteropTryGetMemberExact(unmanagled);
                }
            }

            public RubyContext Context {
                get { return _context; }
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
                Debug.Assert(args.Length == 1);

                if (_setMemberUnmangled == null) {
                    // no unmangled name, just do the set member binding
                    return _setMember.Bind(target, args);
                }

                //
                // Consider this case:
                // x = {"Foo" -> 1}.
                // x.foo += 1
                // Without name mangling this would result to x being {"Foo" -> 1, "foo" -> 2} while the expected result is {"Foo" -> 2}.
                //
                // Hence if the object doesn't contain the member but contains an unmangled member we set the unmangled one:
                //
                return new DynamicMetaObject(
                    Expression.Condition(
                        Expression.AndAlso(
                            Expression.Equal(
                                AstUtils.LightDynamic(_tryGetMember, typeof(object), target.Expression),
                                Expression.Constant(OperationFailed.Value)
                            ),
                            Expression.NotEqual(
                                AstUtils.LightDynamic(_tryGetMemberUnmangled, typeof(object), target.Expression),
                                Expression.Constant(OperationFailed.Value)
                            )
                        ),
                        AstUtils.LightDynamic(_setMemberUnmangled, typeof(object), target.Expression, args[0].Expression),
                        AstUtils.LightDynamic(_setMember, typeof(object), target.Expression, args[0].Expression)
                    ),
                    target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions
                );
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, SetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target,
                DynamicMetaObject/*!*/ value, Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var args = new[] { value };
                var callArgs = new CallArguments(context, target, args, RubyCallSignature.Interop(1));
                var metaBuilder = new MetaObjectBuilder(target, args);

                if (!RubyCallAction.BuildCall(metaBuilder, binder.Name + "=", callArgs, false, false)) {
                    metaBuilder.SetMetaResult(fallback(target, value), false);
                }

                return metaBuilder.CreateMetaObject(binder);
            }

            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.SetMember({0}){1}",
                    _name,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }           
        }

        internal sealed class SetMemberExact : SetMemberBinder {
            private readonly RubyContext/*!*/ _context;

            internal SetMemberExact(RubyContext/*!*/ context, string/*!*/ name)
                : base(name, false) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackSetMember(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ value,
                DynamicMetaObject errorSuggestion) {

#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindSetMember(this, target, ConvertComArgument(value), out result)) {
                    return result;
                }
#endif

                return errorSuggestion ?? new DynamicMetaObject(
                    Expression.Throw(
                        Expression.New(
                            typeof(MissingMemberException).GetConstructor(new[] { typeof(string) }),
                            Expression.Constant(String.Format("unknown member: {0}", Name))
                        ),
                        typeof(object)
                    ),
                    target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions
                );
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.SetMemberExact({0})", Name);
            }
        }

        internal sealed class GetIndex : GetIndexBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;
            public RubyContext Context { get { return _context; } }

            internal GetIndex(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackGetIndex(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes, 
                DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindGetIndex(this, target, InplaceConvertComArguments(indexes), out result)) {
                    return result;
                }
#endif

                return InvokeMember.FallbackInvokeMember(this, "[]", CallInfo, target, indexes, errorSuggestion, null);
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, GetIndexBinder/*!*/ binder, 
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes,
                Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);
                
                var callArgs = new CallArguments(context, target, indexes, RubyCallSignature.Interop(indexes.Length));
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
            public RubyContext Context { get { return _context; } }

            internal SetIndex(RubyContext/*!*/ context, CallInfo/*!*/ callInfo)
                : base(callInfo) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ FallbackSetIndex(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes,
                DynamicMetaObject/*!*/ value, DynamicMetaObject errorSuggestion) {

#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryBindSetIndex(this, target, InplaceConvertComArguments(indexes), ConvertComArgument(value), out result)) {
                    return result;
                }
#endif

                return InvokeMember.FallbackInvokeMember(this, "[]=", CallInfo, target, ArrayUtils.Append(indexes, value), errorSuggestion, null);
            }

            #endregion

            #region DLR -> Ruby

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, SetIndexBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ indexes, DynamicMetaObject/*!*/ value,
                Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {
                Debug.Assert(fallback != null);

                var args = ArrayUtils.Append(indexes, value);
                var callArgs = new CallArguments(context, target, args,
                    new RubyCallSignature(indexes.Length, RubyCallFlags.IsInteropCall | RubyCallFlags.HasRhsArgument)
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

        internal sealed class SetIndexedProperty : DynamicMetaObjectBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;
            public RubyContext Context { get { return _context; } }

            private readonly GetMember/*!*/ _getMember;
            private readonly SetIndex/*!*/ _setIndex;

            internal SetIndexedProperty(RubyContext/*!*/ context, string/*!*/ name, CallInfo/*!*/ callInfo) {
                Assert.NotNull(context);
                _context = context;
                _getMember = context.MetaBinderFactory.InteropGetMember(name);
                _setIndex = context.MetaBinderFactory.InteropSetIndex(callInfo);
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            #region Ruby -> DLR

            public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
                Debug.Assert(args.Length > 1);

                var exprs = new AstExpressions();
                exprs.Add(AstUtils.LightDynamic(_getMember, typeof(object), target.Expression));
                exprs.Add(args.ToExpressions());

                return new DynamicMetaObject(AstUtils.LightDynamic(_setIndex, typeof(object), exprs), BindingRestrictions.Empty);
            }
                
            #endregion

            public override string/*!*/ ToString() {
                return String.Format("Interop.SetIndexedProperty({0}, {1}){2}",
                    _getMember.Name, 
                    _setIndex.CallInfo.ArgumentCount,
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        internal sealed class BinaryOperation : BinaryOperationBinder, IInteropBinder {
            internal static readonly CallInfo _CallInfo = new CallInfo(1);
            private readonly RubyContext/*!*/ _context;
            public RubyContext Context { get { return _context; } }

            internal BinaryOperation(RubyContext/*!*/ context, ExpressionType operation)
                : base(operation) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackBinaryOperation(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ arg, DynamicMetaObject errorSuggestion) {
                string methodName = RubyUtils.MapOperator(Operation);
                Debug.Assert(methodName != null, "Binary operator not implemented");
                return InvokeMember.FallbackInvokeMember(this, methodName, _CallInfo, target, new[] { arg }, errorSuggestion, null);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, BinaryOperationBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/ arg, 
                Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {

                string methodName = RubyUtils.MapOperator(binder.Operation);
                Debug.Assert(methodName != null, "Binary operator not implemented");

                return InvokeMember.Bind(context, methodName, _CallInfo, binder, target, new[] { arg },
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
            internal static readonly CallInfo _CallInfo = new CallInfo(0);
            private static DynamicMetaObject[] _MetaArgumentOne;

            private readonly RubyContext/*!*/ _context;
            public RubyContext Context { get { return _context; } }

            internal UnaryOperation(RubyContext/*!*/ context, ExpressionType operation)
                : base(operation) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackUnaryOperation(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
                string methodName = RubyUtils.MapOperator(Operation);
                Debug.Assert(methodName != null, "Unary operator not implemented");

                return InvokeMember.FallbackInvokeMember(this, methodName, _CallInfo, target, DynamicMetaObject.EmptyMetaObjects, errorSuggestion, null);
            }

            public static DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, UnaryOperationBinder/*!*/ binder,
                DynamicMetaObject/*!*/ target, Func<DynamicMetaObject, DynamicMetaObject>/*!*/ fallback) {

                if (binder.Operation == ExpressionType.Decrement) {
                    return InvokeMember.Bind(context, "-", BinaryOperation._CallInfo, binder, target, MetaArgumentOne, (trgt, _) => fallback(trgt));
                }

                if (binder.Operation == ExpressionType.Increment) {
                    return InvokeMember.Bind(context, "+", BinaryOperation._CallInfo, binder, target, MetaArgumentOne, (trgt, _) => fallback(trgt));
                }

                string methodName = RubyUtils.MapOperator(binder.Operation);
                Debug.Assert(methodName != null, "Unary operator not implemented");

                return InvokeMember.Bind(context, methodName, _CallInfo, binder, target, DynamicMetaObject.EmptyMetaObjects,
                    (trgt, _) => fallback(trgt)
                );
            }

            private static DynamicMetaObject[] MetaArgumentOne {
                get {
                    return _MetaArgumentOne ?? (
                        _MetaArgumentOne = new[] { new DynamicMetaObject(
                            Expression.Constant(ScriptingRuntimeHelpers.Int32ToObject(1), typeof(int)), 
                            BindingRestrictions.Empty, 
                            ScriptingRuntimeHelpers.Int32ToObject(1)
                        )}
                    );
                }
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
            public RubyContext Context { get { return _context; } }

            internal Convert(RubyContext/*!*/ context, Type/*!*/ type, bool isExplicit)
                : base(type, isExplicit) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackConvert(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryConvert(this, target, out result)) {
                    return result;
                }
#endif
                var metaBuilder = new MetaObjectBuilder(this, target, DynamicMetaObject.EmptyMetaObjects);

                if (!GenericConversionAction.BuildConversion(metaBuilder, target, Ast.Constant(_context), Type, errorSuggestion == null)) {
                    Debug.Assert(errorSuggestion != null);
                    // no conversion applicable so we didn't do any operation with arguments that would require restrictions converted to conditions:
                    metaBuilder.SetMetaResult(errorSuggestion, false);
                }

                return metaBuilder.CreateMetaObject(this);
            }

            public override string/*!*/ ToString() {
                return String.Format("Interop.Convert({0}, {1}){2}", 
                    Type.Name, 
                    Explicit ? "explicit" : "implicit", 
                    (_context != null ? " @" + Context.RuntimeId.ToString() : null)
                );
            }
        }

        /// <summary>
        /// Tries convert to IList. If the covnersion is not implemented by the target meta-object wraps it into an object[].
        /// </summary>
        internal sealed class Splat : ConvertBinder, IInteropBinder {
            private readonly RubyContext/*!*/ _context;
            public RubyContext Context { get { return _context; } }

            internal Splat(RubyContext/*!*/ context)
                : base(typeof(IList), true) {
                Assert.NotNull(context);
                _context = context;
            }

            public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
                if (_context.Options.NoAdaptiveCompilation) {
                    return null;
                }

                var result = this.LightBind<T>(args, _context.Options.CompilationThreshold);
                CacheTarget(result);
                return result;
            }

            public override DynamicMetaObject/*!*/ FallbackConvert(DynamicMetaObject/*!*/ target, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject result;
                if (Microsoft.Scripting.ComInterop.ComBinder.TryConvert(this, target, out result)) {
                    return result;
                }
#endif
                return target.Clone(Ast.NewArrayInit(typeof(object), target.Expression));
            }

            public override string/*!*/ ToString() {
                return "Interop.Splat" + (_context != null ? " @" + Context.RuntimeId.ToString() : null);
            }
        }
        
        // TODO: convert binder
        internal static DynamicMetaObject TryBindCovertToDelegate(RubyMetaObject/*!*/ target, ConvertBinder/*!*/ binder, MethodInfo/*!*/ delegateFactory) {
            var metaBuilder = new MetaObjectBuilder(target);
            return TryBuildConversionToDelegate(metaBuilder, target, binder.Type, delegateFactory) ? metaBuilder.CreateMetaObject(binder) : null;
        }

        internal static bool TryBuildConversionToDelegate(MetaObjectBuilder/*!*/ metaBuilder, RubyMetaObject/*!*/ target, Type/*!*/ delegateType, MethodInfo/*!*/ delegateFactory) {
            if (!typeof(Delegate).IsAssignableFrom(delegateType) || delegateType.GetMethod("Invoke") == null) {
                return false;
            }

            var type = target.Value.GetType();
            metaBuilder.AddTypeRestriction(type, target.Expression);
            metaBuilder.Result = delegateFactory.OpCall(AstUtils.Constant(delegateType), Ast.Convert(target.Expression, type));
            return true;
        }

        internal static DynamicMetaObject/*!*/[]/*!*/ InplaceConvertComArguments(DynamicMetaObject/*!*/[]/*!*/ args) {
            for (int i = 0; i < args.Length; i++) {
                args[i] = ConvertComArgument(args[i]);
            }
            return args;
        }

        internal static DynamicMetaObject/*!*/ ConvertComArgument(DynamicMetaObject/*!*/ arg) {
            Expression expr = arg.Expression;
            BindingRestrictions restrictions;
            if (arg.Value != null) {
                Type type = arg.Value.GetType();
                if (type == typeof(BigInteger)) {
                    expr = Ast.Convert(AstUtils.Convert(arg.Expression, typeof(BigInteger)), typeof(double));
                } else if (type == typeof(MutableString)) {
                    // TODO: encoding?
                    expr = Ast.Convert(AstUtils.Convert(arg.Expression, typeof(MutableString)), typeof(string));
                } else if (type == typeof(RubySymbol)) {
                    // TODO: encoding?
                    expr = Ast.Convert(AstUtils.Convert(arg.Expression, typeof(RubySymbol)), typeof(string));
                }
                restrictions = BindingRestrictions.GetTypeRestriction(arg.Expression, type);
            } else {
                restrictions = BindingRestrictions.GetExpressionRestriction(Ast.Equal(arg.Expression, AstUtils.Constant(null)));
            }
            return arg.Clone(expr, restrictions);
        }
    }
}
