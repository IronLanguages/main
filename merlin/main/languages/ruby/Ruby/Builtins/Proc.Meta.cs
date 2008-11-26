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

using System.Linq.Expressions;
using System.Dynamic.Binders;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;

namespace IronRuby.Builtins {

    public partial class Proc : IDynamicObject {
        public MetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, Restrictions.Empty, this);
        }

        internal sealed class Meta : MetaObject {
            public Meta(Expression/*!*/ expression, Restrictions/*!*/ restrictions, Proc/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
                RubyCallSignature callSignature;
                if (RubyCallSignature.TryCreate(action.Arguments, out callSignature)) {
                    return action.FallbackInvoke(this, args);
                }

                var context = new MetaObject(
                    Methods.GetContextFromProc.OpCall(AstUtils.Convert(Expression, typeof(Proc))),
                    Restrictions.Empty,
                    RubyOps.GetContextFromProc((Proc)Value)
                );

                var metaBuilder = new MetaObjectBuilder();
                Proc.SetCallActionRule(metaBuilder, new CallArguments(context, this, args, callSignature), true);
                return metaBuilder.CreateMetaObject(action, args);
            }
        }
    }
}
