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
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    using Ast = System.Linq.Expressions.Expression;

    internal sealed class RubyInvokeMemberBinder : InvokeMemberBinder {
        private readonly RubyContext/*!*/ _context;

        public RubyInvokeMemberBinder(RubyContext/*!*/ context, string/*!*/ name, ArgumentInfo[]/*!*/ arguments)
            : base(name, false, arguments) {
            _context = context;
        }

        public override object CacheIdentity {
            get { return this; }
        }

        public override MetaObject/*!*/ FallbackInvoke(MetaObject/*!*/ self, MetaObject/*!*/[]/*!*/ args, MetaObject/*!*/ onBindingError) {
            var result = TryBind(_context, this, self, args);
            if (result != null) {
                return result;
            }

            // TODO: return ((DefaultBinder)_context.Binder).GetMember(Name, self, Ast.Null(typeof(CodeContext)), true);
            throw new NotImplementedException();
        }

        public override MetaObject/*!*/ FallbackInvokeMember(MetaObject/*!*/ self, MetaObject/*!*/[]/*!*/ args, MetaObject/*!*/ onBindingError) {
            var result = TryBind(_context, this, self, args);
            if (result != null) {
                return result;
            }

            // TODO: return ((DefaultBinder)_context.Binder).GetMember(Name, self, Ast.Null(typeof(CodeContext)), true);
            throw new NotImplementedException();
        }

        public static MetaObject TryBind(RubyContext/*!*/ context, InvokeMemberBinder/*!*/ binder, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            Assert.NotNull(context, target);

            var metaBuilder = new MetaObjectBuilder();
            
            RubyCallAction.Bind(metaBuilder, binder.Name,
                new CallArguments(
                    new MetaObject(Ast.Constant(context), Restrictions.Empty, context),
                    target, 
                    args, 
                    RubyCallSignature.Simple(binder.Arguments.Count)
                )
            );

            // TODO: we should return null if we fail, we need to throw exception for now:
            return metaBuilder.CreateMetaObject(binder, MetaObject.EmptyMetaObjects);
        }
    }
}
