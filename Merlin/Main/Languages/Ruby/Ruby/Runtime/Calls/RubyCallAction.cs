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
using System.Linq.Expressions;
using System.Dynamic;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

using IronRuby.Builtins;
using IronRuby.Compiler;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime.Calls {

    public class RubyCallAction : RubyMetaBinder, IExpressionSerializable {
        private readonly RubyCallSignature _signature;
        private readonly string/*!*/ _methodName;

        public override RubyCallSignature Signature {
            get { return _signature; }
        }

        public string/*!*/ MethodName {
            get { return _methodName; }
        }

        internal protected RubyCallAction(RubyContext context, string/*!*/ methodName, RubyCallSignature signature) 
            : base(context) {
            Assert.NotNull(methodName);
            _methodName = methodName;
            _signature = signature;
        }

        /// <summary>
        /// Creates a runtime-bound call site binder.
        /// </summary>
        public static RubyCallAction/*!*/ Make(RubyContext/*!*/ context, string/*!*/ methodName, int argumentCount) {
            return Make(context, methodName, RubyCallSignature.Simple(argumentCount));
        }

        /// <summary>
        /// Creates a runtime-bound call site binder.
        /// </summary>
        public static RubyCallAction/*!*/ Make(RubyContext/*!*/ context, string/*!*/ methodName, RubyCallSignature signature) {
            ContractUtils.RequiresNotNull(context, "context");
            ContractUtils.RequiresNotNull(methodName, "methodName");
            return context.MetaBinderFactory.Call(methodName, signature);
        }

        /// <summary>
        /// Creates a call site binder that can be used from multiple runtimes. The site it binds for can be called from multiple runtimes.
        /// </summary>
        [Emitted]
        public static RubyCallAction/*!*/ MakeShared(string/*!*/ methodName, RubyCallSignature signature) {
            // TODO: reduce usage of these sites to minimum
            return RubyMetaBinderFactory.Shared.Call(methodName, signature);
        }

        public override string/*!*/ ToString() {
            return _methodName + _signature.ToString() + (Context != null ? " @" + Context.RuntimeId.ToString() : null);
        }

        #region IExpressionSerializable Members

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                Methods.GetMethod(typeof(RubyCallAction), "MakeShared", typeof(string), typeof(RubyCallSignature)),
                AstUtils.Constant(_methodName),
                _signature.CreateExpression()
            );
        }

        #endregion

        protected override void Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            Build(metaBuilder, _methodName, args, true);
        }

        internal static bool Build(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args, bool defaultFallback) {
            RubyMemberInfo methodMissing;
            var method = Resolve(metaBuilder, methodName, args, out methodMissing);

            if (method.Found) {
                method.Info.BuildCall(metaBuilder, args, methodName);
                return true;
            } else {
                return BindToMethodMissing(metaBuilder, args, methodName, methodMissing, method.IncompatibleVisibility, false, defaultFallback);
            }
        }

        internal static MethodResolutionResult Resolve(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args,
            out RubyMemberInfo methodMissing) {

            MethodResolutionResult method;
            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            using (targetClass.Context.ClassHierarchyLocker()) {
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext);

                // TODO: All sites should have either implicit-self or has-scope flag set?
                var visibilityContext = args.Signature.HasImplicitSelf || !args.Signature.HasScope ? RubyClass.IgnoreVisibility : args.Scope.SelfImmediateClass;
                method = targetClass.ResolveMethodForSiteNoLock(methodName, visibilityContext);
                if (!method.Found) {
                    if (args.Signature.IsTryCall) {
                        // TODO: this shouldn't throw. We need to fix caching of non-existing methods.
                        throw new MissingMethodException();
                        // metaBuilder.Result = AstUtils.Constant(Fields.RubyOps_MethodNotFound);
                    } else {
                        methodMissing = targetClass.ResolveMethodMissingForSite(methodName, method.IncompatibleVisibility);
                    }
                } else {
                    methodMissing = null;
                }
            }

            // Whenever the current self's class changes we need to invalidate the rule, if a protected method is being called.
            if (method.Info != null && method.Info.IsProtected && !args.Signature.HasImplicitSelf) {
                // We don't need to compare versions, just the class objects (super-class relationship cannot be changed).
                // Since we don't want to hold on a class object (to make it collectible) we compare references to the version boxes.
                metaBuilder.AddCondition(Ast.Equal(
                    Methods.GetSelfClassVersionHandle.OpCall(AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope))),
                    Ast.Constant(args.Scope.SelfImmediateClass.Version)
                ));
            }

            return method;
        }

        internal static bool BindToMethodMissing(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback) {
            // Assumption: args already contain method name.
            
            // TODO: better check for builtin method
            if (methodMissing == null ||
                methodMissing.DeclaringModule == methodMissing.Context.KernelModule && methodMissing is RubyLibraryMethodInfo) {

                if (isSuperCall) {
                    metaBuilder.SetError(Methods.MakeMissingSuperException.OpCall(AstUtils.Constant(methodName)));
                } else if (incompatibleVisibility == RubyMethodVisibility.Private) {
                    metaBuilder.SetError(Methods.MakePrivateMethodCalledError.OpCall(
                        AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)), args.TargetExpression, AstUtils.Constant(methodName))
                    );
                } else if (incompatibleVisibility == RubyMethodVisibility.Protected) {
                    metaBuilder.SetError(Methods.MakeProtectedMethodCalledError.OpCall(
                        AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)), args.TargetExpression, AstUtils.Constant(methodName))
                    );
                } else if (defaultFallback) {
                    args.InsertMethodName(methodName);
                    methodMissing.BuildCall(metaBuilder, args, methodName);
                } else {
                    return false;
                }
            } else {
                args.InsertMethodName(methodName);
                methodMissing.BuildCall(metaBuilder, args, methodName);
            }

            return true;
        }

        private DynamicMetaObjectBinder/*!*/ GetInteropBinder(RubyContext/*!*/ context, CallInfo/*!*/ callInfo) {
            switch (_methodName) {
                case "new":
                    return new InteropBinder.CreateInstance(context, callInfo);

                case "call":
                    return new InteropBinder.Invoke(context, callInfo);

                case "to_s":
                    return new InteropBinder.Convert(context, typeof(string), true);

                case "to_str":
                    return new InteropBinder.Convert(context, typeof(string), false);

                // TODO: other ops

                default:
                    if (_methodName.EndsWith("=")) {
                        // TODO: SetMemberBinder
                        throw new NotSupportedException();
                    } else {
                        return new InteropBinder.InvokeMember(context, _methodName, callInfo);
                    }
            }
        }

        protected override DynamicMetaObject/*!*/ InteropBind(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            // TODO: pass block as the last parameter (before RHS arg?):
            var normalizedArgs = RubyMethodGroupBase.NormalizeArguments(metaBuilder, args, SelfCallConvention.NoSelf, false, false);
            var callInfo = new CallInfo(normalizedArgs.Length);

            var interopBinder = GetInteropBinder(args.RubyContext, callInfo);
            var result = interopBinder.Bind(args.MetaTarget, normalizedArgs);
            metaBuilder.SetMetaResult(result, args);
            return metaBuilder.CreateMetaObject(interopBinder);
        }

        
    }
}
