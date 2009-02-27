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
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;

    public class RubyLambdaMethodInfo : RubyMemberInfo {
        private readonly Proc/*!*/ _lambda;
        private readonly string/*!*/ _definitionName;

        internal RubyLambdaMethodInfo(Proc/*!*/ lambda, string/*!*/ definitionName, RubyMemberFlags flags, RubyModule/*!*/ declaringModule) 
            : base(flags, declaringModule) {
            Assert.NotNull(lambda, definitionName, declaringModule);
            _lambda = lambda;
            _definitionName = definitionName;
        }

        public Proc/*!*/ Lambda {
            get { return _lambda; }
        }

        public string/*!*/ DefinitionName {
            get { return _definitionName; }
        }

        internal override bool IsRemovable {
            get { return true; }
        }

        public override MemberInfo/*!*/[]/*!*/ GetMembers() {
            return new MemberInfo[] { _lambda.Dispatcher.Method.Method };
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyLambdaMethodInfo(_lambda, _definitionName, flags, module);
        }

        public override RubyMemberInfo TrySelectOverload(Type/*!*/[]/*!*/ parameterTypes) {
            return parameterTypes.Length == _lambda.Dispatcher.ParameterCount 
                && CollectionUtils.TrueForAll(parameterTypes, (type) => type == typeof(object)) ? this : null;
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            Debug.Assert(!args.Signature.HasBlock);

            Proc.SetProcCallRule(
                metaBuilder,
                AstUtils.Constant(_lambda),            // proc object
                args.TargetExpression,            // self
                AstUtils.Constant(this),               // this method for super and class_eval
                args
            );
        }
    }
}
