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
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;
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
            get { return _signature.ResolveOnly ? typeof(bool) : typeof(object); }
        }

        internal protected RubyCallAction(RubyContext context, string/*!*/ methodName, RubyCallSignature signature) 
            : base(context) {
            Assert.NotNull(methodName);
            
            // a virtual call cannot be a super call nor interop call:
            Debug.Assert(!signature.IsVirtualCall || !signature.IsSuperCall && !signature.IsInteropCall);

            // a super call must have implicit self:
            Debug.Assert(!signature.IsSuperCall || signature.HasImplicitSelf);

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

        protected override object BindPrecompiled(Type/*!*/ delegateType, object[]/*!*/ args) {
            if (Context == null || 
                Signature.ResolveOnly ||
                (Signature.Flags & ~(RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasScope | RubyCallFlags.HasBlock | RubyCallFlags.HasRhsArgument)) != 0) {
                return null;
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
                return null;
            }

            var dispatcher = method.Info.GetDispatcher(delegateType, Signature, target, version);
            if (dispatcher != null) {
                object result = dispatcher.CreateDelegate(MethodDispatcher.UntypedFuncs.Contains(delegateType));
                RubyBinder.DumpPrecompiledRule(this, dispatcher);
                return result;
            }

            return null;
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

                if (args.Signature.ResolveOnly) {
                    metaBuilder.Result = AstFactory.True;
                    return true;    
                }
                
                if (args.Signature.IsVirtualCall && !method.Info.IsRubyMember) {
                    metaBuilder.Result = Ast.Field(null, Fields.ForwardToBase);
                    return true;
                }

                method.Info.BuildCall(metaBuilder, args, methodName);
                return true;
            } else if (args.Signature.ResolveOnly) {
                metaBuilder.Result = AstFactory.False;
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

                if (args.Signature.IsSuperCall) {
                    Debug.Assert(!args.Signature.IsVirtualCall && args.Signature.HasImplicitSelf);
                    method = targetClass.ResolveSuperMethodNoLock(methodName, targetClass).InvalidateSitesOnOverride();
                } else {
                    var options = args.Signature.IsVirtualCall ? MethodLookup.Virtual : MethodLookup.Default;
                    method = targetClass.ResolveMethodForSiteNoLock(methodName, visibilityContext, options);
                }

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

        // Returns true if the call was bound (with success or failure), false if fallback should be performed.
        internal static bool BuildMethodMissingCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback) {

            switch (BindToKernelMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall)) {
                case MethodMissingBinding.Custom:
                    Debug.Assert(!metaBuilder.Error);
                    methodMissing.BuildMethodMissingCall(metaBuilder, args, methodName);
                    return true;

                case MethodMissingBinding.Error:
                    // method_missing is defined in Kernel, error has been reported:
                    return true;

                case MethodMissingBinding.Fallback:
                    // method_missing is defined in Kernel:
                    if (defaultFallback) {
                        metaBuilder.SetError(Methods.MakeMissingMethodError.OpCall(
                            args.MetaContext.Expression,
                            AstUtils.Convert(args.TargetExpression, typeof(object)),
                            Ast.Constant(methodName)
                        ));
                        return true;
                    }
                    return false;
            }
            throw Assert.Unreachable;
        }

        // Returns true if the call was bound (with success or failure), false if fallback should be performed.
        internal static bool BuildMethodMissingAccess(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback) {

            switch (BindToKernelMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall)) {
                case MethodMissingBinding.Custom:
                    // we pretend we found the member and return a method that calls method_missing:
                    Debug.Assert(!metaBuilder.Error);
                    metaBuilder.Result = Methods.CreateBoundMissingMember.OpCall(
                        AstUtils.Convert(args.TargetExpression, typeof(object)),
                        Ast.Constant(methodMissing, typeof(RubyMemberInfo)),
                        Ast.Constant(methodName)
                    );
                    return true;

                case MethodMissingBinding.Error:
                    // method_missing is defined in Kernel, error has been reported:
                    return true;

                case MethodMissingBinding.Fallback:
                    // method_missing is defined in Kernel:
                    if (defaultFallback) {
                        metaBuilder.SetError(Methods.MakeMissingMemberError.OpCall(Ast.Constant(methodName)));
                        return true;
                    }
                    return false;
            }
            throw Assert.Unreachable;
        }

        private enum MethodMissingBinding {
            Error,
            Fallback,
            Custom
        }

        private static MethodMissingBinding BindToKernelMethodMissing(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall) {

            // TODO: better specialization of method_missing methods
            if (methodMissing == null ||
                methodMissing.DeclaringModule == methodMissing.Context.BasicObjectClass && methodMissing is RubyLibraryMethodInfo) {

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
                    return MethodMissingBinding.Fallback;
                }

                return MethodMissingBinding.Error;
            }

            return MethodMissingBinding.Custom;
        }

        protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext/*!*/ context, IList<DynamicMetaObject/*!*/>/*!*/ args,
            out MethodInfo postConverter) {

            postConverter = null;
                    
            ExpressionType op;
            int opArity = RubyUtils.TryMapOperator(_methodName, out op);
            if (opArity == 1 + args.Count) {
                switch (opArity) {
                    case 1: return context.MetaBinderFactory.InteropUnaryOperation(op);
                    case 2: return context.MetaBinderFactory.InteropBinaryOperation(op);
                }
            }
       
            switch (_methodName) {
                case "new":
                    return context.MetaBinderFactory.InteropCreateInstance(new CallInfo(args.Count));

                case "call":
                    return context.MetaBinderFactory.InteropInvoke(new CallInfo(args.Count));

                case "to_s":
                    if (args.Count == 0) {
                        postConverter = Methods.ObjectToMutableString;
                        return context.MetaBinderFactory.InteropInvokeMember("ToString", new CallInfo(0));
                    }
                    goto default;

                case "to_str":
                    if (args.Count == 0) {
                        postConverter = Methods.StringToMutableString;
                        return context.MetaBinderFactory.InteropConvert(typeof(string), false);
                    }
                    goto default;

                case "[]":
                    // TODO: or invoke?
                    return context.MetaBinderFactory.InteropGetIndex(new CallInfo(args.Count));

                case "[]=":
                    return context.MetaBinderFactory.InteropSetIndex(new CallInfo(args.Count));

                default:
                    if (_methodName.LastCharacter() == '=') {
                        var baseName = _methodName.Substring(0, _methodName.Length - 1);
                        if (args.Count == 1) {
                            return context.MetaBinderFactory.InteropSetMember(baseName);
                        } else {
                            return context.MetaBinderFactory.InteropSetIndexedProperty(baseName, new CallInfo(args.Count));
                        }
                    } else {
                        return context.MetaBinderFactory.InteropInvokeMember(_methodName, new CallInfo(args.Count));
                    }
            }
        }
    }
}
