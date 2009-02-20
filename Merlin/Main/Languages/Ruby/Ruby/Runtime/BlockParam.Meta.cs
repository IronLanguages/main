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
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;

using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Compiler;
    
namespace IronRuby.Runtime {
    public sealed partial class BlockParam : IDynamicMetaObjectProvider {
        public DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Meta(parameter, BindingRestrictions.Empty, this);
        }

        internal sealed class Meta : DynamicMetaObject {
            private BlockParam BlockParam {
                get { return (BlockParam)Value; }
            }

            public Meta(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, BlockParam/*!*/ value)
                : base(expression, restrictions, value) {
                ContractUtils.RequiresNotNull(value, "value");
            }

            public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
                RubyCallSignature callSignature;
                if (RubyCallSignature.TryCreate(action.CallInfo, out callSignature)) {
                    return action.FallbackInvoke(this, args);
                }

                var metaBuilder = new MetaObjectBuilder();

                var context = new DynamicMetaObject(
                    Methods.GetContextFromBlockParam.OpCall(AstUtils.Convert(Expression, typeof(BlockParam))),
                    BindingRestrictions.Empty,
                    RubyOps.GetContextFromBlockParam((BlockParam)Value)
                );

                BlockParam.SetCallActionRule(metaBuilder, new CallArguments(context, this, args, callSignature));
                return metaBuilder.CreateMetaObject(action, args);
            }
        }
    }
}
