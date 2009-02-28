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
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime.Calls {

    public sealed class SuperCallAction : DynamicMetaObjectBinder, IExpressionSerializable, IEquatable<SuperCallAction> {
        private readonly RubyCallSignature _signature;
        private readonly int _lexicalScopeId;

        internal RubyCallSignature Signature {
            get { return _signature; }
        }

        private SuperCallAction(RubyCallSignature signature, int lexicalScopeId) {
            _signature = signature;
            _lexicalScopeId = lexicalScopeId;
        }

        [Emitted]
        public static SuperCallAction/*!*/ Make(RubyCallSignature signature, int lexicalScopeId) {
            return new SuperCallAction(signature, lexicalScopeId);
        }

        public static SuperCallAction/*!*/ Make(int argumentCount, int lexicalScopeId) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            return new SuperCallAction(RubyCallSignature.WithScope(argumentCount), lexicalScopeId);
        }

        #region Object Overrides, IEquatable

        public override bool Equals(object obj) {
            return Equals(obj as SuperCallAction);
        }

        public override int GetHashCode() {
            return _signature.GetHashCode() ^ _lexicalScopeId;
        }

        public override string/*!*/ ToString() {
            return "super" + _signature.ToString() + ":" + _lexicalScopeId;
        }

        public bool Equals(SuperCallAction other) {
            return other != null && _signature.Equals(other._signature) && _lexicalScopeId == other._lexicalScopeId;
        }

        #endregion

        #region Rule Generation

        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ context, DynamicMetaObject/*!*/[]/*!*/ args) {
            var mo = new MetaObjectBuilder();
            BuildSuperCall(mo, new CallArguments(context, args, _signature));
            return mo.CreateMetaObject(this, context, args);
        }

        internal void BuildSuperCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            RubyModule currentDeclaringModule;
            string currentMethodName;

            var scope = args.Scope;

            object target;
            scope.GetSuperCallTarget(out currentDeclaringModule, out currentMethodName, out target);

            var targetExpression = metaBuilder.GetTemporary(typeof(object), "#super-self");
            
            metaBuilder.AddCondition(
                Methods.IsSuperCallTarget.OpCall(
                    AstUtils.Convert(args.ScopeExpression, typeof(RubyScope)),
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
                metaBuilder.AddTargetTypeTest(target, targetClass, targetExpression, scope.RubyContext, args.ContextExpression);
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
                args.InsertMethodName(currentMethodName);
                RubyCallAction.BindToMethodMissing(metaBuilder, args, currentMethodName, methodMissing, RubyMethodVisibility.None, true);
            }
        }

        #endregion

        #region IExpressionSerializable Members

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                Methods.GetMethod(typeof(SuperCallAction), "Make", typeof(RubyCallSignature), typeof(int)),
                _signature.CreateExpression(),
                AstUtils.Constant(_lexicalScopeId)
            );
        }

        #endregion
    }
}
