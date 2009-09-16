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
using System.Diagnostics;
using System.Dynamic;

using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public sealed class SuperCallAction : RubyMetaBinder {
        private readonly RubyCallSignature _signature;
        private readonly int _lexicalScopeId;

        internal SuperCallAction(RubyContext context, RubyCallSignature signature, int lexicalScopeId)
            : base(context) {
            _signature = signature;
            _lexicalScopeId = lexicalScopeId;
        }

        public static SuperCallAction/*!*/ Make(RubyContext/*!*/ context, RubyCallSignature signature, int lexicalScopeId) {
            ContractUtils.RequiresNotNull(context, "context");
            return context.MetaBinderFactory.SuperCall(lexicalScopeId, signature);
        }

        [Emitted]
        public static SuperCallAction/*!*/ MakeShared(RubyCallSignature signature, int lexicalScopeId) {
            return RubyMetaBinderFactory.Shared.SuperCall(lexicalScopeId, signature);
        }

        public override string/*!*/ ToString() {
            return "super" + _signature.ToString() + ":" + _lexicalScopeId + (Context != null ? " @" + Context.RuntimeId.ToString() : null);
        }

        public override RubyCallSignature Signature {
            get { return _signature; }
        }

        public override Type/*!*/ ReturnType {
            get { return typeof(object); }
        }

        #region Rule Generation

        protected override bool Build(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, bool defaultFallback) {
            RubyModule currentDeclaringModule;
            string currentMethodName;

            var scope = args.Scope;

            object target;
            scope.GetSuperCallTarget(out currentDeclaringModule, out currentMethodName, out target);

            var targetExpression = metaBuilder.GetTemporary(typeof(object), "#super-self");
            
            metaBuilder.AddCondition(
                Methods.IsSuperCallTarget.OpCall(
                    AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope)),
                    AstUtils.Constant(currentDeclaringModule),
                    AstUtils.Constant(currentMethodName),
                    targetExpression
                )
            );

            args.SetTarget(targetExpression, target);

            Debug.Assert(currentDeclaringModule != null);

            RubyMemberInfo method;
            RubyMemberInfo methodMissing = null;

            // we need to lock the hierarchy of the target class:
            var targetClass = scope.RubyContext.GetImmediateClassOf(target);
            using (targetClass.Context.ClassHierarchyLocker()) {
                // initialize all methods in ancestors:                
                targetClass.InitializeMethodsNoLock();

                // target is stored in a local, therefore it cannot be part of the restrictions:
                metaBuilder.TreatRestrictionsAsConditions = true;
                metaBuilder.AddTargetTypeTest(target, targetClass, targetExpression, args.MetaContext, 
                    new[] { Symbols.MethodMissing } // currentMethodName is resolved for super, which cannot be an instance singleton
                );
                metaBuilder.TreatRestrictionsAsConditions = false;

                method = targetClass.ResolveSuperMethodNoLock(currentMethodName, currentDeclaringModule).InvalidateSitesOnOverride().Info;
                if (method == null) {
                    // MRI: method_missing is called for the targetClass, not for the super:
                    methodMissing = targetClass.ResolveMethodMissingForSite(currentMethodName, RubyMethodVisibility.None);
                }
            }

            if (method != null) {
                method.BuildSuperCall(metaBuilder, args, currentMethodName, currentDeclaringModule);
            } else {
                return RubyCallAction.BuildMethodMissingCall(metaBuilder, args, currentMethodName, methodMissing, RubyMethodVisibility.None, true, defaultFallback);
            }

            return true;
        }

        #endregion

        public override Expression/*!*/ CreateExpression() {
            return Methods.GetMethod(GetType(), "MakeShared").OpCall(_signature.CreateExpression(), AstUtils.Constant(_lexicalScopeId));
        }
    }
}
