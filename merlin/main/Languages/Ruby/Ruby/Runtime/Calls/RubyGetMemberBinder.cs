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

    internal sealed class RubyGetMemberBinder : GetMemberBinder {
        private readonly RubyContext/*!*/ _context;

        public RubyGetMemberBinder(RubyContext/*!*/ context, string/*!*/ name)
            : base(name, false) {
            _context = context;
        }

        public override object CacheIdentity {
            get { return this; }
        }

        public override DynamicMetaObject/*!*/ FallbackGetMember(DynamicMetaObject/*!*/ self, DynamicMetaObject/*!*/ onBindingError) {
            var result = TryBind(_context, this, self);
            if (result != null) {
                return result; 
            }

            // TODO: remove CodeContext
            return ((DefaultBinder)_context.Binder).GetMember(Name, self, Ast.Constant(null, typeof(CodeContext)), true);
        }

        public static DynamicMetaObject TryBind(RubyContext/*!*/ context, GetMemberBinder/*!*/ binder, DynamicMetaObject/*!*/ target) {
            Assert.NotNull(context, target);
            var metaBuilder = new MetaObjectBuilder();
            var contextExpression = Ast.Constant(context);

            metaBuilder.AddTargetTypeTest(target.Value, target.Expression, context, contextExpression);

            RubyMemberInfo method = context.ResolveMethod(target.Value, binder.Name, true).InvalidateSitesOnOverride();
            if (method != null && RubyModule.IsMethodVisible(method, false)) {
                // we need to create a bound member:
                metaBuilder.Result = Ast.Constant(new RubyMethod(target.Value, method, binder.Name));
            } else {
                // TODO:
                // We need to throw an exception if we don't find method_missing so that our version update optimization works: 
                // This limits interop with other languages. 
                //                   
                // class B           CLR type with method 'foo'
                // class C < B       Ruby class
                // x = C.new
                //
                // 1. x.GET("foo") from Ruby
                //    No method found or CLR method found -> fallback to Python
                //    Python might see its method foo or might just fallback to .NET, 
                //    in any case it will add rule [1] with restriction on type of C w/o Ruby version check.
                // 2. B.define_method("foo") 
                //    This doesn't update C due to the optimization (there is no overridden method foo in C).
                // 3. x.GET("foo") from Ruby
                //    This will not invoke the binder since the rule [1] is still valid.
                //
                object symbol = SymbolTable.StringToId(binder.Name);
                RubyCallAction.BindToMethodMissing(metaBuilder, binder.Name,
                    new CallArguments(
                        new DynamicMetaObject(contextExpression, BindingRestrictions.Empty, context),
                        new[] { 
                            target,
                            new DynamicMetaObject(Ast.Constant(symbol), BindingRestrictions.Empty, symbol) 
                        },
                        RubyCallSignature.Simple(1)
                    ), 
                    method != null
                );
            }

            // TODO: we should return null if we fail, we need to throw exception for now:
            return metaBuilder.CreateMetaObject(binder, DynamicMetaObject.EmptyMetaObjects);
        }
    }
}
