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
using System.Collections.ObjectModel;
using System.Dynamic.Binders;
using System.Linq.Expressions;
using System.Diagnostics;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime.Calls;
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Simple helper for building up method call actions.
    /// </summary>
    internal class CallBuilder {
        private readonly AstGenerator _gen;

        private readonly List<MSA.Expression>/*!*/ _args = new List<MSA.Expression>();
        
        // TODO:
        public MSA.Expression Instance;
        public MSA.Expression SplattedArgument;
        public MSA.Expression Block;
        public MSA.Expression RhsArgument;

        internal CallBuilder(AstGenerator gen) {
            _gen = gen;
        }

        public void Add(MSA.Expression/*!*/ expression) {
            _args.Add(expression);
        }

        private RubyCallSignature MakeCallSignature(bool hasImplicitSelf) {
            return new RubyCallSignature(true, hasImplicitSelf, _args.Count, SplattedArgument != null, Block != null, RhsArgument != null);
        }

        public MSA.DynamicExpression/*!*/ MakeCallAction(string/*!*/ name, bool hasImplicitSelf) {
            return MakeCallAction(name, _gen.Binder, MakeCallSignature(hasImplicitSelf), GetExpressions());
        }

        public static MSA.DynamicExpression/*!*/ MakeCallAction(string/*!*/ name, ActionBinder/*!*/ binder, RubyCallSignature signature, 
            params MSA.Expression[]/*!*/ args) {
            RubyCallAction call = RubyCallAction.Make(name, signature);
            switch (args.Length) {
                case 0: return Ast.Dynamic(call, typeof(object), AstFactory.EmptyExpressions);
                case 1: return Ast.Dynamic(call, typeof(object), args[0]);
                case 2: return Ast.Dynamic(call, typeof(object), args[0], args[1]);
                case 3: return Ast.Dynamic(call, typeof(object), args[0], args[1], args[2]);
                case 4: return Ast.Dynamic(call, typeof(object), args[0], args[1], args[2], args[3]);
                default:
                    return Ast.Dynamic(
                        call,
                        typeof(object),
                        new ReadOnlyCollection<MSA.Expression>(args)
                    );
            }
        }

        public MSA.Expression/*!*/ MakeSuperCallAction(int lexicalScopeId) {
            return Ast.Dynamic(
                SuperCallAction.Make(MakeCallSignature(true), lexicalScopeId),
                typeof(object),
                GetExpressions()
            );
        }

        private MSA.Expression/*!*/[]/*!*/ GetExpressions() {
            var result = new List<MSA.Expression>();
            result.Add(_gen.CurrentScopeVariable);
            result.Add(Instance);
            
            if (Block != null) {
                result.Add(Block);
            }

            for (int i = 0; i < _args.Count; i++) {
                result.Add(_args[i]);
            }

            if (SplattedArgument != null) {
                result.Add(SplattedArgument);
            }

            if (RhsArgument != null) {
                result.Add(RhsArgument);
            }

            return result.ToArray();
        }
    }
}
