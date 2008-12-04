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
using System.Dynamic;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Builtins {
    using Ast = System.Linq.Expressions.Expression;
    using IronRuby.Compiler;
    
    public partial class RubyMethod : IDynamicObject {
        public MetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, Restrictions.Empty, this);
        }

        internal sealed class Meta : MetaObject {
            public RubyMethod/*!*/ Method {
                get { return (RubyMethod)Value; }
            }

            public Meta(Expression/*!*/ expression, Restrictions/*!*/ restrictions, RubyMethod/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override MetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ action, MetaObject/*!*/[]/*!*/ args) {
                RubyCallSignature callSignature;
                if (RubyCallSignature.TryCreate(action.Arguments, out callSignature)) {
                    return action.FallbackInvoke(this, args);
                }

                var self = (RubyMethod)Value;

                var context = new MetaObject(
                    Methods.GetContextFromMethod.OpCall(AstUtils.Convert(Expression, typeof(RubyMethod))),
                    Restrictions.Empty,
                    RubyOps.GetContextFromMethod(self)
                );

                var metaBuilder = new MetaObjectBuilder();
                Method.SetRuleForCall(metaBuilder, new CallArguments(context, this, args, callSignature));
                return metaBuilder.CreateMetaObject(action, args);
            }

            public override MetaObject/*!*/ BindConvert(ConvertBinder/*!*/ action) {
                var result = RubyBinder.TryBindCovertToDelegate(action, this);
                if (result != null) {
                    return result;
                }

                return base.BindConvert(action);
            }
        }
    }
}
