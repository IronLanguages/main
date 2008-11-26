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

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Actions;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;
    using IronRuby.Compiler;

    internal class RubyEventInfo : RubyMemberInfo {
        private readonly EventInfo/*!*/ _eventInfo;

        public RubyEventInfo(EventInfo/*!*/ eventInfo, RubyMemberFlags flags, RubyModule/*!*/ declaringModule)
            : base(flags, declaringModule) {
            Assert.NotNull(eventInfo, declaringModule);
            _eventInfo = eventInfo;
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyEventInfo(_eventInfo, flags, module);
        }

        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            if (args.Signature.HasBlock) {
                metaBuilder.Result = Methods.HookupEvent.OpCall(
                    Ast.Convert(Ast.Constant(_eventInfo), typeof(EventInfo)),
                    args.TargetExpression,
                    Ast.Convert(args.GetBlockExpression(), typeof(Proc))
                );
            } else {
                // TODO: make error
                throw new NotImplementedError("no block given");
            }
        }

        private static void ReadAll(IList<ParameterExpression> variables, IList<Expression> expressions, int start) {
            for (int i = 0, j = start; i < variables.Count; i++, j++) {
                expressions[j] = variables[i];
            }
        }
    }
}
