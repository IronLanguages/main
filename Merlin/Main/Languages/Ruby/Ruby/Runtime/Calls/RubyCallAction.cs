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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public class RubyCallAction : RubyMetaBinder {
        private readonly RubyCallSignature _signature;
        private readonly string/*!*/ _methodName;

        public override RubyCallSignature Signature {
            get { return _signature; }
        }

        public string/*!*/ MethodName {
            get { return _methodName; }
        }

        public override Type/*!*/ ReturnType {
            get { return typeof(object); }
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

        public override Expression/*!*/ CreateExpression() {
            return Expression.Call(
                Methods.GetMethod(typeof(RubyCallAction), "MakeShared", typeof(string), typeof(RubyCallSignature)),
                AstUtils.Constant(_methodName),
                _signature.CreateExpression()
            );
        }

        #region Precompiled Rules

        public override T BindDelegate<T>(CallSite<T>/*!*/ site, object[]/*!*/ args) {
            if (Context == null || (Signature.Flags & ~(RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasScope | RubyCallFlags.HasBlock)) != 0) {
                return base.BindDelegate<T>(site, args);
            }

            RubyScope scope;
            object target;
            if (Signature.HasScope) {
                scope = (RubyScope)args[0];
                target = args[1];
            } else {
                scope = Context.EmptyScope;
                target = args[0];
            }

            int version;
            MethodResolutionResult method;
            RubyClass targetClass = Context.GetImmediateClassOf(target);
            using (targetClass.Context.ClassHierarchyLocker()) {
                version = targetClass.Version.Method;
                method = targetClass.ResolveMethodForSiteNoLock(_methodName, GetVisibilityContext(Signature, scope));
            }

            if (!method.Found || method.Info.IsProtected && !Signature.HasImplicitSelf) {
                return base.BindDelegate<T>(site, args);
            }

            var dispatcher = method.Info.GetDispatcher<T>(Signature, target, version);
            if (dispatcher != null) {
                T result = (T)dispatcher.CreateDelegate();
                CacheTarget(result);
                RubyBinder.DumpPrecompiledRule(this, dispatcher);
                return result;
            }

            return base.BindDelegate<T>(site, args);
        }

        #endregion

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            return BuildCall(metaBuilder, _methodName, args, defaultFallback, true);
        }

        // Returns true if the call was bound (with success or failure), false if fallback should be performed.
        internal static bool BuildCall(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args, 
            bool defaultFallback, bool callClrMethods) {

            RubyMemberInfo methodMissing;
            var method = Resolve(metaBuilder, methodName, args, out methodMissing);

            if (method.Found) {
                if (!callClrMethods && !method.Info.IsRubyMember) {
                    return false;
                }

                if (args.Signature.IsVirtualCall && !method.Info.IsRubyMember) {
                    metaBuilder.Result = Ast.Field(null, Fields.ForwardToBase);
                    return true;
                }

                method.Info.BuildCall(metaBuilder, args, methodName);
                return true;
            } else {
                return BuildMethodMissingCall(metaBuilder, args, methodName, methodMissing, method.IncompatibleVisibility, false, defaultFallback);
            }
        }

        // Returns true if the call was bound (with success or failure), false if fallback should be performed.
        internal static bool BuildAccess(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args, 
            bool defaultFallback, bool callClrMethods) {

            RubyMemberInfo methodMissing;
            var method = Resolve(metaBuilder, methodName, args, out methodMissing);

            if (method.Found) {
                if (!callClrMethods && !method.Info.IsRubyMember) {
                    return false;
                }

                if (method.Info.IsDataMember) {
                    method.Info.BuildCall(metaBuilder, args, methodName);
                } else {
                    metaBuilder.Result = Methods.CreateBoundMember.OpCall(
                        AstUtils.Convert(args.TargetExpression, typeof(object)),
                        Ast.Constant(method.Info, typeof(RubyMemberInfo)),
                        Ast.Constant(methodName)
                    );
                }
                return true;
            } else {
                // Ruby doesn't have "attribute_missing" so we will always use method_missing and return a bound method object:
                return BuildMethodMissingAccess(metaBuilder, args, methodName, methodMissing, method.IncompatibleVisibility, false, defaultFallback);
            }
        }

        private static VisibilityContext GetVisibilityContext(RubyCallSignature callSignature, RubyScope scope) {
            return callSignature.HasImplicitSelf || !callSignature.HasScope ? 
                new VisibilityContext(callSignature.IsInteropCall ? RubyMethodAttributes.Public : RubyMethodAttributes.VisibilityMask) :
                new VisibilityContext(scope.SelfImmediateClass);
        }

        internal static MethodResolutionResult Resolve(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args,
            out RubyMemberInfo methodMissing) {

            MethodResolutionResult method;
            var targetClass = args.TargetClass;
            var visibilityContext = GetVisibilityContext(args.Signature, args.Scope);
            using (targetClass.Context.ClassHierarchyLocker()) {
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext, 
                    new[] { methodName, Symbols.MethodMissing }
                );

                var options = args.Signature.IsVirtualCall ? MethodLookup.Virtual : MethodLookup.Default;
                method = targetClass.ResolveMethodForSiteNoLock(methodName, visibilityContext, options);
                if (!method.Found) {
                    methodMissing = targetClass.ResolveMethodMissingForSite(methodName, method.IncompatibleVisibility);
                } else {
                    methodMissing = null;
                }
            }

            // Whenever the current self's class changes we need to invalidate the rule, if a protected method is being called.
            if (method.Info != null && method.Info.IsProtected && visibilityContext.Class != null) {
                // We don't need to compare versions, just the class objects (super-class relationship cannot be changed).
                // Since we don't want to hold on a class object (to make it collectible) we compare references to the version handlers.
                metaBuilder.AddCondition(Ast.Equal(
                    Methods.GetSelfClassVersionHandle.OpCall(AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope))),
                    Ast.Constant(visibilityContext.Class.Version)
                ));
            }

            return method;
        }

        internal static bool BuildMethodMissingCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback) {

            if (BindToMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall) || defaultFallback) {
                if (!metaBuilder.Error) {
                    args.InsertMethodName(methodName);
                    methodMissing.BuildCall(metaBuilder, args, methodName);
                }
                return true;
            } else {
                return false;
            }
        }

        internal static bool BuildMethodMissingAccess(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback) {

            if (BindToMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall) || defaultFallback) {
                if (!metaBuilder.Error) {
                    metaBuilder.Result = Methods.CreateBoundMissingMember.OpCall(
                        AstUtils.Convert(args.TargetExpression, typeof(object)), 
                        Ast.Constant(methodMissing, typeof(RubyMemberInfo)), 
                        Ast.Constant(methodName)
                    );
                }
                return true;
            } else {
                return false;
            }
        }

        internal static bool BindToMethodMissing(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall) {

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
                } else {
                    return false;
                }
            }

            return true;
        }

        protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args,
            out MethodInfo postConverter) {

            switch (_methodName) {
                case "new":
                    postConverter = null;
                    return new InteropBinder.CreateInstance(context, new CallInfo(args.Count));

                case "call":
                    postConverter = null; 
                    return new InteropBinder.Invoke(context, "call", new CallInfo(args.Count));

                case "to_s":
                    postConverter = Methods.ObjectToMutableString;
                    return new InteropBinder.InvokeMember(context, "ToString", new CallInfo(args.Count));

                case "to_str":
                    postConverter = Methods.StringToMutableString;
                    return new InteropBinder.Convert(context, typeof(string), false);

                case "[]":
                    // TODO: or invoke?
                    postConverter = null;
                    return new InteropBinder.GetIndex(context, new CallInfo(args.Count));

                case "[]=":
                    postConverter = null;
                    return new InteropBinder.SetIndex(context, new CallInfo(args.Count));

                // BinaryOps:
                case "+": // ExpressionType.Add
                case "-": // ExpressionType.Subtract
                case "/": // ExpressionType.Divide
                case "*": // ExpressionType.Multiply
                case "%": // ExpressionType.Modulo
                case "==": // ExpressionType.Equal
                case "!=": // ExpressionType.NotEqual
                case ">": // ExpressionType.GreaterThan
                case ">=": // ExpressionType.GreaterThanOrEqual
                case "<":  // ExpressionType.LessThan
                case "<=": // ExpressionType.LessThanOrEqual

                case "**": // ExpressionType.Power
                case "<<": // ExpressionType.LeftShift
                case ">>": // ExpressionType.RightShift
                case "&": // ExpressionType.And
                case "|": // ExpressionType.Or
                case "^": // ExpressionType.ExclusiveOr;

                // UnaryOp:
                case "-@":
                case "+@":
                case "~":
                    postConverter = null;
                    return null;

                default:
                    postConverter = null;
                    if (_methodName.EndsWith("=")) {
                        return new InteropBinder.SetMember(context, _methodName.Substring(0, _methodName.Length - 1));
                    } else {
                        return new InteropBinder.InvokeMember(context, _methodName, new CallInfo(args.Count));
                    }
            }
        }
    }
}
