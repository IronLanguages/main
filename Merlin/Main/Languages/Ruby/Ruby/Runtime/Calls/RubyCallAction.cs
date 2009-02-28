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
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Actions;

using IronRuby.Builtins;
using IronRuby.Compiler;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using IronRuby.Compiler.Generation;
using System.Collections.Generic;
   
namespace IronRuby.Runtime.Calls {

    public class RubyCallAction : DynamicMetaObjectBinder, IEquatable<RubyCallAction>, IExpressionSerializable {
        private readonly RubyCallSignature _signature;
        private readonly string/*!*/ _methodName;

        public RubyCallSignature Signature {
            get { return _signature; }
        }

        public string/*!*/ MethodName {
            get { return _methodName; }
        }

        protected RubyCallAction(string/*!*/ methodName, RubyCallSignature signature) {
            Assert.NotNull(methodName);
            _methodName = methodName;
            _signature = signature;
        }

        public static RubyCallAction/*!*/ Make(string/*!*/ methodName, int argumentCount) {
            return Make(methodName, RubyCallSignature.Simple(argumentCount));
        }

        [Emitted]
        public static RubyCallAction/*!*/ Make(string/*!*/ methodName, RubyCallSignature signature) {
            return new RubyCallAction(methodName, signature);
        }

        #region Object Overrides, IEquatable

        public override int GetHashCode() {
            return _methodName.GetHashCode() ^ _signature.GetHashCode() ^ GetType().GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as RubyCallAction);
        }

        public override string/*!*/ ToString() {
            return _methodName + _signature.ToString();
        }

        public bool Equals(RubyCallAction other) {
            return other != null && _methodName == other._methodName && _signature.Equals(other._signature) && other.GetType() == GetType();
        }

        #endregion

        #region IExpressionSerializable Members

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                Methods.GetMethod(typeof(RubyCallAction), "Make", typeof(string), typeof(RubyCallSignature)),
                AstUtils.Constant(_methodName),
                _signature.CreateExpression()
            );
        }

        #endregion

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/[]/*!*/ args) {
            var mo = new MetaObjectBuilder();
            Bind(mo, _methodName, new CallArguments(context, args, _signature));
            return mo.CreateMetaObject(this, context, args);
        }

        /// <exception cref="MissingMethodException">The resolved method is Kernel#method_missing.</exception>
        internal static void Bind(MetaObjectBuilder/*!*/ metaBuilder, string/*!*/ methodName, CallArguments/*!*/ args) {
            RubyClass targetClass = args.RubyContext.GetImmediateClassOf(args.Target);
            MethodResolutionResult method;
            RubyMemberInfo methodMissing = null;

            using (targetClass.Context.ClassHierarchyLocker()) {
                metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.RubyContext, args.ContextExpression);

                // TODO: HasScope
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
                }
            }

            // Whenever the current self's class changes we need to invalidate the rule, if a protected method is being called.
            if (method.Info != null && method.Info.IsProtected && !args.Signature.HasImplicitSelf) {
                // We don't need to compare versions, just the class objects (super-class relationship cannot be changed).
                // Since we don't want to hold on a class object (to make it collectible) we compare references to the version boxes.
                metaBuilder.AddCondition(Ast.Equal(
                    Methods.GetSelfClassVersionHandle.OpCall(args.ScopeExpression), 
                    Ast.Constant(args.Scope.SelfImmediateClass.Version)
                ));
            }

            if (method.Found) {
                method.Info.BuildCall(metaBuilder, args, methodName);
            } else {
                args.InsertMethodName(methodName);
                BindToMethodMissing(metaBuilder, args, methodName, methodMissing, method.IncompatibleVisibility, false);
            }
        }

        internal static void BindToMethodMissing(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName,
            RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall) {
            // Assumption: args already contain method name.
            
            // TODO: better check for builtin method
            if (methodMissing == null ||
                methodMissing.DeclaringModule == methodMissing.Context.KernelModule && methodMissing is RubyLibraryMethodInfo) {

                if (isSuperCall) {
                    metaBuilder.SetError(Methods.MakeMissingSuperException.OpCall(AstUtils.Constant(methodName)));
                } else if (incompatibleVisibility == RubyMethodVisibility.Private) {
                    metaBuilder.SetError(Methods.MakePrivateMethodCalledError.OpCall(args.ContextExpression, args.TargetExpression, AstUtils.Constant(methodName)));
                } else if (incompatibleVisibility == RubyMethodVisibility.Protected) {
                    metaBuilder.SetError(Methods.MakeProtectedMethodCalledError.OpCall(args.ContextExpression, args.TargetExpression, AstUtils.Constant(methodName)));
                } else {
                    // TODO: fallback
                    methodMissing.BuildCall(metaBuilder, args, methodName);
                }
            } else {
                methodMissing.BuildCall(metaBuilder, args, methodName);
            }
        }
    }
}
