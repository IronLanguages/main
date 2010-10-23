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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    // <leading-mandatory>, <optional>, *<array>, <trailing-mandatory>, &block
    public partial class Parameters : Node {
        internal static readonly Parameters/*!*/ Empty = new Parameters(null, 0, null, null, null, SourceSpan.None);

        // all mandatory parameters:
        private readonly LeftValue/*!*/[]/*!*/ _mandatory;

        // the number of leading mandatory parameters:
        private readonly int _leadingMandatoryCount;

        private readonly SimpleAssignmentExpression/*!*/[]/*!*/ _optional;
        private readonly LeftValue _unsplat;
        private readonly LocalVariable _block;

        public LeftValue/*!*/[]/*!*/ Mandatory {
            get { return _mandatory; }
        }

        public int LeadingMandatoryCount {
            get { return _leadingMandatoryCount; }
        }

        public SimpleAssignmentExpression/*!*/[]/*!*/ Optional {
            get { return _optional; }
        }

        // for-loop might use constant variable, instance variable, class variable or global variable:
        public LeftValue Unsplat {
            get { return _unsplat; }
        }

        public LocalVariable Block {
            get { return _block; }
        }

        public Parameters(LeftValue/*!*/[] mandatory, int leadingMandatoryCount, SimpleAssignmentExpression/*!*/[] optional, LeftValue unsplat, LocalVariable block, SourceSpan location)
            : base(location) {

            mandatory = mandatory ?? LeftValue.EmptyArray;
            optional = optional ?? SimpleAssignmentExpression.EmptyArray;

            Debug.Assert(leadingMandatoryCount >= 0 && leadingMandatoryCount <= mandatory.Length);
            Debug.Assert(leadingMandatoryCount == mandatory.Length || optional != null || unsplat != null);

            _mandatory = mandatory;
            _leadingMandatoryCount = leadingMandatoryCount;
            _optional = optional;
            _unsplat = unsplat;
            _block = block;
        }

        internal MSA.Expression/*!*/ TransformOptionalsInitialization(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);

            if (_optional.Length == 0) {
                return AstUtils.Empty();
            }

            MSA.Expression singleton = gen.CurrentScope.DefineHiddenVariable("#default", typeof(object));

            MSA.Expression result = AstUtils.Empty();
            for (int i = 0; i < _optional.Length; i++) {
                result = AstUtils.IfThen(
                    Ast.Equal(_optional[i].Left.TransformRead(gen), singleton),
                    Ast.Block(
                        result,
                        _optional[i].TransformRead(gen) // assignment
                    )
                );
            }
            
            return Ast.Block(
                Ast.Assign(singleton, Ast.Field(null, Fields.DefaultArgument)),
                result,
                AstUtils.Empty()
            );
        }

        internal void TransformForSuperCall(AstGenerator/*!*/ gen, CallSiteBuilder/*!*/ siteBuilder) {
            for (int i = 0; i < _leadingMandatoryCount; i++) {
                siteBuilder.Add(_mandatory[i].TransformRead(gen));
            }

            foreach (SimpleAssignmentExpression s in _optional) {
                siteBuilder.Add(s.Left.TransformRead(gen));
            }
            
            for (int i = _leadingMandatoryCount; i < _mandatory.Length; i++) {
                siteBuilder.Add(_mandatory[i].TransformRead(gen));
            }

            if (_unsplat != null) {
                siteBuilder.SplattedArgument = _unsplat.TransformRead(gen);
            }
        }

        internal BlockSignatureAttributes GetBlockSignatureAttributes() {
            var result = BlockSignatureAttributes.None;

            if (_unsplat != null) {
                result |= BlockSignatureAttributes.HasUnsplatParameter;
            }

            if (_block != null) {
                result |= BlockSignatureAttributes.HasProcParameter;
            }

            int arity = _mandatory.Length;
            if (_unsplat != null) {
                arity = -(arity + 1);
            } else if (arity > 0 && _mandatory[_mandatory.Length - 1] is Placeholder) {
                arity--;
            }

            return BlockDispatcher.MakeAttributes(result, arity);
        }
    }
}