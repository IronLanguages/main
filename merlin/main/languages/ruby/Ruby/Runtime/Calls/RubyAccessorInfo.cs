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

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;
    using IronRuby.Compiler;
    using System.Diagnostics;

    public abstract class RubyAttributeAccessorInfo : RubyMemberInfo {
        private readonly string/*!*/ _instanceVariableName;

        protected string/*!*/ InstanceVariableName { get { return _instanceVariableName; } }

        protected RubyAttributeAccessorInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ variableName)
            : base(flags, declaringModule) {
            Assert.NotEmpty(variableName);
            Debug.Assert(variableName.StartsWith("@"));
            _instanceVariableName = variableName;
        }
    }

    public sealed class RubyAttributeReaderInfo : RubyAttributeAccessorInfo {
        public RubyAttributeReaderInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ variableName)
            : base(flags, declaringModule, variableName) {
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            metaBuilder.Result = Methods.GetInstanceVariable.OpCall(
                args.ScopeExpression,
                AstFactory.Box(args.TargetExpression),
                AstUtils.Constant(InstanceVariableName)
            );
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyAttributeReaderInfo(flags, module, InstanceVariableName);
        }
    }

    public sealed class RubyAttributeWriterInfo : RubyAttributeAccessorInfo {
        public RubyAttributeWriterInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ name)
            : base(flags, declaringModule, name) {
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {

            var actualArgs = RubyMethodGroupInfo.MakeActualArgs(metaBuilder, args, true, false, false, false);

            metaBuilder.Result = Methods.SetInstanceVariable.OpCall(
                AstFactory.Box(actualArgs[0]),
                AstFactory.Box(actualArgs[1]),
                args.ScopeExpression,
                AstUtils.Constant(InstanceVariableName)
            );
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyAttributeWriterInfo(flags, module, InstanceVariableName);
        }
    }
}
