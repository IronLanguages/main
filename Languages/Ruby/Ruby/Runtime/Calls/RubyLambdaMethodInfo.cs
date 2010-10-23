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
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;

    public class RubyLambdaMethodInfo : RubyMemberInfo {
        private static int _Id = 1;

        private readonly int _id;
        private readonly Proc/*!*/ _lambda;
        private readonly string/*!*/ _definitionName;

        internal RubyLambdaMethodInfo(Proc/*!*/ block, string/*!*/ definitionName, RubyMemberFlags flags, RubyModule/*!*/ declaringModule) 
            : base(flags, declaringModule) {
            Assert.NotNull(block, definitionName, declaringModule);
            _lambda = block.ToLambda(this);
            _definitionName = definitionName;
            _id = Interlocked.Increment(ref _Id);
        }

        public override int GetArity() {
            return _lambda.Dispatcher.Arity;
        }

        public Proc/*!*/ Lambda {
            get { return _lambda; }
        }

        internal int Id {
            get { return _id; }
        }

        public string/*!*/ DefinitionName {
            get { return _definitionName; }
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
            Proc.BuildCall(
                metaBuilder,
                AstUtils.Constant(_lambda),            // proc object
                args.TargetExpression,                 // self
                args
            );
        }
    }
}
