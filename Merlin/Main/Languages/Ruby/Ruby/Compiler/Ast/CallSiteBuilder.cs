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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// Simple helper for building up method call actions.
    /// </summary>
    internal sealed class CallSiteBuilder : CollectionBuilder<MSA.Expression> {
        private readonly AstGenerator/*!*/ _gen;
        private readonly bool _hasBlock;

        public MSA.Expression SplattedArgument { get; set; }
        public MSA.Expression RhsArgument { get; set; }

        internal CallSiteBuilder(AstGenerator/*!*/ gen, MSA.Expression/*!*/ instance, MSA.Expression block) {
            Assert.NotNull(gen, instance);
            _hasBlock = block != null;
            _gen = gen;

            // scope variable can be typed to a subclass of RubyScope:
            Add(AstUtils.Convert(_gen.CurrentScopeVariable, typeof(RubyScope)));
            Add(instance);
            Add(block);
        }

        /// <summary>
        /// [scope, instance, block?]
        /// </summary>
        private int HiddenArgumentCount {
            get { return _hasBlock ? 3 : 2; }
        }

        private RubyCallFlags GetSignatureFlags() {
            var flags = RubyCallFlags.HasScope;

            if (_hasBlock) {
                flags |= RubyCallFlags.HasBlock;
            }

            if (SplattedArgument != null) {
                flags |= RubyCallFlags.HasSplattedArgument;
            }

            if (RhsArgument != null) {
                flags |= RubyCallFlags.HasRhsArgument;
            }

            return flags;
        }

        public MSA.DynamicExpression/*!*/ MakeSuperCallAction(int lexicalScopeId, bool hasImplicitArguments) {
            RubyCallFlags flags = GetSignatureFlags() | RubyCallFlags.HasImplicitSelf;
            if (hasImplicitArguments) {
                flags |= RubyCallFlags.HasImplicitArguments;
            }

            return MakeCallSite(SuperCallAction.Make(_gen.Context, new RubyCallSignature(Count - HiddenArgumentCount, flags), lexicalScopeId));
        }

        public MSA.DynamicExpression/*!*/ MakeCallAction(string/*!*/ name, bool hasImplicitSelf) {
            RubyCallFlags flags = GetSignatureFlags();
            if (hasImplicitSelf) {
                flags |= RubyCallFlags.HasImplicitSelf;
            }

            return MakeCallSite(RubyCallAction.Make(_gen.Context, name, new RubyCallSignature(Count - HiddenArgumentCount, flags)));
        }

        internal MSA.DynamicExpression/*!*/ MakeCallSite(CallSiteBinder/*!*/ binder) {
            if (SplattedArgument != null) {
                Add(SplattedArgument);
            }

            if (RhsArgument != null) {
                Add(RhsArgument);
            }

            switch (Count) {
                case 0:
                case 1: throw Assert.Unreachable;
                case 2: return Ast.Dynamic(binder, typeof(object), Item000, Item001);
                case 3: return Ast.Dynamic(binder, typeof(object), Item000, Item001, Item002);
                case 4: return Ast.Dynamic(binder, typeof(object), Item000, Item001, Item002, Item003);
                default: return Ast.Dynamic(binder, typeof(object), Items);
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
    }
}
