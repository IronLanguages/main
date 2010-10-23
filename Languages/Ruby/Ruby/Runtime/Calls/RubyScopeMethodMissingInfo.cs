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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Compiler;

namespace IronRuby.Runtime.Calls {
    using Ast = Expression;
    using AstFactory = IronRuby.Compiler.Ast.AstFactory;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public sealed class RubyScopeMethodMissingInfo : RubyMemberInfo {
        internal RubyScopeMethodMissingInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule)
            : base(flags, declaringModule) {
        }

        protected internal override RubyMemberInfo/*!*/ Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new RubyScopeMethodMissingInfo(flags, module);
        }

        public override int GetArity() {
            return -1;
        }

        public override MemberInfo[]/*!*/ GetMembers() {
            return new MemberInfo[0];
        }

        #region Direct call (rarely used)

        // Only used if method_missing() is called directly on the main singleton.
        internal override void BuildCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var globalScope = args.TargetClass.GlobalScope;

            // TODO: this just calls super for now, so it doesn't look up the scope:
            metaBuilder.Result = AstUtils.LightDynamic(
                new RubyCallAction(globalScope.Context, Symbols.MethodMissing,
                    new RubyCallSignature(
                        args.Signature.ArgumentCount,
                        args.Signature.Flags | RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsSuperCall
                    )
                ),
                typeof(object),
                args.GetCallSiteArguments(args.TargetExpression)
            );
        }

        #endregion

        internal override void BuildMethodMissingCallNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ name) {
            var globalScope = args.TargetClass.GlobalScope;
            var context = globalScope.Context;

            if (name.LastCharacter() == '=') {
                var normalizedArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
                if (!metaBuilder.Error) {
                    var scopeVar = metaBuilder.GetTemporary(typeof(Scope), "#scope");

                    metaBuilder.AddInitialization(
                        Ast.Assign(scopeVar, Methods.GetGlobalScopeFromScope.OpCall(AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope))))
                    );

                    var interopSetter = context.MetaBinderFactory.InteropSetMember(name.Substring(0, name.Length - 1));

                    metaBuilder.SetMetaResult(
                        interopSetter.Bind(
                            new DynamicMetaObject(
                                scopeVar,
                                BindingRestrictions.Empty,
                                globalScope.Scope
                            ),
                            new[] { normalizedArgs[0] }
                        ),
                        true
                    );
                }
            } else {
                RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
                Expression errorExpr =  metaBuilder.Error ? Ast.Throw(metaBuilder.Result, typeof(object)) : null;

                var scopeVar = metaBuilder.GetTemporary(typeof(Scope), "#scope");
                var scopeLookupResultVar = metaBuilder.GetTemporary(typeof(object), "#result");
                
                metaBuilder.AddInitialization(
                    Ast.Assign(scopeVar, Methods.GetGlobalScopeFromScope.OpCall(AstUtils.Convert(args.MetaScope.Expression, typeof(RubyScope))))
                );

                Expression scopeLookupResultExpr = errorExpr ?? scopeLookupResultVar;
                Expression fallbackExp;

                if (name == "scope") {
                    fallbackExp = errorExpr ?? args.TargetExpression;
                } else {
                    // super(methodName, ...args...) - ignore argument error:
                    args.InsertMethodName(name);
                    fallbackExp = AstUtils.LightDynamic(
                        context.MetaBinderFactory.Call(Symbols.MethodMissing, 
                            new RubyCallSignature(
                                args.Signature.ArgumentCount + 1,
                                args.Signature.Flags | RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsSuperCall
                            )
                        ),
                        typeof(object),
                        args.GetCallSiteArguments(args.TargetExpression)
                    );
                }

                var scopeLookup = Ast.NotEqual(
                    Ast.Assign(scopeLookupResultVar, AstUtils.LightDynamic(context.MetaBinderFactory.InteropTryGetMemberExact(name), typeof(object), scopeVar)),
                    Expression.Constant(OperationFailed.Value)
                );

                string unmanagled = RubyUtils.TryUnmangleMethodName(name);
                if (unmanagled != null) {
                    scopeLookup = Ast.OrElse(
                        scopeLookup,
                        Ast.NotEqual(
                            Ast.Assign(scopeLookupResultVar, AstUtils.LightDynamic(context.MetaBinderFactory.InteropTryGetMemberExact(unmanagled), typeof(object), scopeVar)),
                            Expression.Constant(OperationFailed.Value)
                        )
                    );
                }

                metaBuilder.Result = Ast.Condition(
                    scopeLookup,
                    scopeLookupResultExpr,
                    fallbackExp
                );
            }
        }
    }
}

