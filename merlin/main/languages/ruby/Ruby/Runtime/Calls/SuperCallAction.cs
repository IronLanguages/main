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
using System.Dynamic.Binders;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {

    public sealed class SuperCallAction : MetaObjectBinder, IExpressionSerializable, IEquatable<SuperCallAction> {
        private readonly RubyCallSignature _signature;
        private readonly int _lexicalScopeId;

        internal RubyCallSignature Signature {
            get { return _signature; }
        }

        private SuperCallAction(RubyCallSignature signature, int lexicalScopeId) {
            _signature = signature;
            _lexicalScopeId = lexicalScopeId;
        }

        public static SuperCallAction/*!*/ Make(RubyCallSignature signature, int lexicalScopeId) {
            return new SuperCallAction(signature, lexicalScopeId);
        }

        public static SuperCallAction/*!*/ Make(int argumentCount, int lexicalScopeId) {
            ContractUtils.Requires(argumentCount >= 0, "argumentCount");
            return new SuperCallAction(RubyCallSignature.WithScope(argumentCount), lexicalScopeId);
        }

        public override object/*!*/ CacheIdentity {
            get { return this; }
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

        public override MetaObject/*!*/ Bind(MetaObject/*!*/ context, MetaObject/*!*/[]/*!*/ args) {
            var mo = new MetaObjectBuilder();
            SetRule(mo, new CallArguments(context, args, _signature));
            return mo.CreateMetaObject(this, context, args);
        }

        internal void SetRule(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args) {
            RubyModule currentDeclaringModule;
            string currentMethodName;

            var scope = args.Scope;

            object target;
            scope.GetSuperCallTarget(out currentDeclaringModule, out currentMethodName, out target);

            var targetExpression = metaBuilder.GetTemporary(typeof(object), "#super-self");
            
            metaBuilder.AddCondition(
                Methods.IsSuperCallTarget.OpCall(
                    AstUtils.Convert(args.ScopeExpression, typeof(RubyScope)),
                    Ast.Constant(currentDeclaringModule),
                    AstUtils.Constant(currentMethodName),
                    targetExpression
                )
            );

            args.SetTarget(targetExpression, target);

            Debug.Assert(currentDeclaringModule != null);

            // target is stored in a local, therefore it cannot be part of the restrictions:
            metaBuilder.TreatRestrictionsAsConditions = true;
            metaBuilder.AddTargetTypeTest(target, targetExpression, scope.RubyContext, args.ContextExpression);
            metaBuilder.TreatRestrictionsAsConditions = false;

            RubyMemberInfo method = scope.RubyContext.ResolveSuperMethod(target, currentMethodName, currentDeclaringModule);

            // super calls don't go to method_missing
            if (method == null) {
                metaBuilder.SetError(Methods.MakeMissingSuperException.OpCall(Ast.Constant(currentMethodName)));
            } else {
                method.InvalidateSitesOnOverride = true;
                method.BuildSuperCall(metaBuilder, args, currentMethodName, currentDeclaringModule);
            }
        }

        #endregion

        #region IExpressionSerializable Members

        Expression/*!*/ IExpressionSerializable.CreateExpression() {
            return Expression.Call(
                typeof(SuperCallAction).GetMethod("Make", new Type[] { typeof(RubyCallSignature), typeof(int) }),
                _signature.CreateExpression(),
                Expression.Constant(_lexicalScopeId)
            );
        }

        #endregion
    }
}
