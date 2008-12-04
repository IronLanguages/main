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
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls {
    using IronRuby.Builtins;
    using Ast = System.Linq.Expressions.Expression;
    using System;
    using IronRuby.Compiler;

    internal sealed class RubyCreateInstanceBinder : CreateInstanceBinder {
        private readonly RubyContext/*!*/ _context;

        public RubyCreateInstanceBinder(RubyContext/*!*/ context, ArgumentInfo[]/*!*/ arguments)
            : base(arguments) {
            _context = context;
        }

        public override object CacheIdentity {
            get { return this; }
        }

        public override MetaObject/*!*/ FallbackCreateInstance(MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args, MetaObject errorSuggestion) {
            var result = TryBind(_context, this, target, args);
            if (result != null) {
                return result;
            }

            throw new NotImplementedException();
            // TODO:
            //return ((DefaultBinder)_context.Binder).Create(.GetMember(Name, self, Ast.Null(typeof(CodeContext)), true);
        }

        public static MetaObject TryBind(RubyContext/*!*/ context, CreateInstanceBinder/*!*/ binder, MetaObject/*!*/ target, MetaObject/*!*/[]/*!*/ args) {
            Assert.NotNull(context, binder, target, args);
            
            var metaBuilder = new MetaObjectBuilder();

            RubyCallAction.Bind(metaBuilder, "new", 
                new CallArguments(
                    new MetaObject(Ast.Constant(context), Restrictions.Empty, context),
                    target, 
                    args, 
                    RubyCallSignature.Simple(args.Length)
                )
            );

            // TODO: we should return null if we fail, we need to throw exception due to version update optimization:
            return metaBuilder.CreateMetaObject(binder, MetaObject.EmptyMetaObjects);
        }
    }
}
