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
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// Simple helper for building up method call actions.
    /// </summary>
    internal sealed class CallSiteBuilder : ExpressionCollectionBuilder<MSA.Expression> {
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

        public MSA.Expression/*!*/ MakeSuperCallAction(int lexicalScopeId, bool hasImplicitArguments) {
            RubyCallFlags flags = GetSignatureFlags() | RubyCallFlags.HasImplicitSelf;
            if (hasImplicitArguments) {
                flags |= RubyCallFlags.HasImplicitArguments;
            }

            return MakeCallSite(SuperCallAction.Make(_gen.Context, new RubyCallSignature(Count - HiddenArgumentCount, flags), lexicalScopeId));
        }

        public MSA.Expression/*!*/ MakeCallAction(string/*!*/ name, bool hasImplicitSelf) {
            RubyCallFlags flags = GetSignatureFlags();
            if (hasImplicitSelf) {
                flags |= RubyCallFlags.HasImplicitSelf;
            }

            return MakeCallSite(RubyCallAction.Make(_gen.Context, name, new RubyCallSignature(Count - HiddenArgumentCount, flags)));
        }

        internal MSA.Expression/*!*/ MakeCallSite(CallSiteBinder/*!*/ binder) {
            if (SplattedArgument != null) {
                Add(SplattedArgument);
            }

            if (RhsArgument != null) {
                Add(RhsArgument);
            }

            return AstUtils.LightDynamic(binder, this);
        }

        internal static MSA.Expression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target) {
            Debug.Assert(signature.HasScope);

            return AstUtils.LightDynamic(RubyCallAction.Make(context, name, signature), AstUtils.Convert(scope, typeof(RubyScope)), target);
        }

        internal static MSA.Expression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target, MSA.Expression/*!*/ arg0) {
            Debug.Assert(signature.HasScope);

            return AstUtils.LightDynamic(RubyCallAction.Make(context, name, signature), AstUtils.Convert(scope, typeof(RubyScope)), target, arg0);
        }

        internal static MSA.Expression/*!*/ InvokeMethod(RubyContext/*!*/ context, string/*!*/ name, RubyCallSignature signature,
            MSA.Expression/*!*/ scope, MSA.Expression/*!*/ target, MSA.Expression/*!*/ arg0, MSA.Expression/*!*/ arg1) {
            Debug.Assert(signature.HasScope);

            return AstUtils.LightDynamic(RubyCallAction.Make(context, name, signature), AstUtils.Convert(scope, typeof(RubyScope)), target, arg0, arg1);
        }
    }
}
