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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// Simple helper for building up method call actions.
    /// </summary>
    internal class CallBuilder {
        private readonly AstGenerator/*!*/ _gen;

        private readonly List<MSA.Expression>/*!*/ _args = new List<MSA.Expression>();
        
        // TODO:
        public MSA.Expression Instance;
        public MSA.Expression SplattedArgument;
        public MSA.Expression Block;
        public MSA.Expression RhsArgument;

        internal CallBuilder(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);
            _gen = gen;
        }

        public void Add(MSA.Expression/*!*/ expression) {
            _args.Add(expression);
        }

        private RubyCallSignature MakeCallSignature(bool hasImplicitSelf) {
            return new RubyCallSignature(true, hasImplicitSelf, _args.Count, SplattedArgument != null, Block != null, RhsArgument != null);
        }

        public MSA.DynamicExpression/*!*/ MakeCallAction(string/*!*/ name, bool hasImplicitSelf) {
            return InvokeMethod(_gen.Context, name, MakeCallSignature(hasImplicitSelf), GetExpressions());
        }

        public MSA.Expression/*!*/ MakeSuperCallAction(int lexicalScopeId) {
            return Ast.Dynamic(
                SuperCallAction.Make(_gen.Context, MakeCallSignature(true), lexicalScopeId),
                typeof(object),
                GetExpressions()
            );
        }

        internal static MSA.DynamicExpression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            params MSA.Expression[]/*!*/ args) {

            Debug.Assert(args.Length >= 2);
            var scope = args[0];
            var target = args[1];

            switch (args.Length) {
                case 2: return InvokeMethod(context, name, signature, scope, target);
                case 3: return InvokeMethod(context, name, signature, scope, target, args[2]);
                case 4: return InvokeMethod(context, name, signature, scope, target, args[2], args[3]);
                default: return InvokeMethod(context, name, signature, scope, target, args);
            }
        }

        internal static MSA.DynamicExpression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target) {
            Debug.Assert(signature.HasScope);

            return Ast.Dynamic(RubyCallAction.Make(context, name, signature), typeof(object), AstUtils.Convert(scope, typeof(RubyScope)), target);
        }

        internal static MSA.DynamicExpression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target, MSA.Expression/*!*/ arg0) {
            Debug.Assert(signature.HasScope);

            return Ast.Dynamic(RubyCallAction.Make(context, name, signature), typeof(object), AstUtils.Convert(scope, typeof(RubyScope)), target, arg0);
        }

        internal static MSA.DynamicExpression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target, MSA.Expression/*!*/ arg0, MSA.Expression/*!*/ arg1) {
            Debug.Assert(signature.HasScope);

            return Ast.Dynamic(RubyCallAction.Make(context, name, signature), typeof(object), AstUtils.Convert(scope, typeof(RubyScope)), target, arg0, arg1);
        }

        internal static MSA.DynamicExpression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target, MSA.Expression/*!*/[]/*!*/ args) {
            Debug.Assert(signature.HasScope);
            Debug.Assert(args.Length >= 2);

            args[0] = Ast.Convert(args[0], typeof(RubyScope));
            return Ast.Dynamic(RubyCallAction.Make(context, name, signature), typeof(object), args);
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
