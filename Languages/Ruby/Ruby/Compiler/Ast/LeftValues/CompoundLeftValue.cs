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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstExpressions = ReadOnlyCollectionBuilder<MSA.Expression>;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;

    public partial class CompoundLeftValue : LeftValue {
        // TODO: 1.9
        
        /// <summary>
        /// Empty LHS - used by blocks.
        /// </summary>
        internal static readonly CompoundLeftValue/*!*/ EmptyBlockSignature = new CompoundLeftValue(LeftValue.EmptyArray);

        // TODO: 1.9

        /// <summary>
        /// Unspecified LHS - used by blocks.
        /// </summary>
        internal static readonly CompoundLeftValue/*!*/ UnspecifiedBlockSignature = new CompoundLeftValue(LeftValue.EmptyArray);

        /// <summary>
        /// List of l-values, possibly compound.
        /// </summary>
        private readonly LeftValue/*!*/[]/*!*/ _leftValues;

        /// <summary>
        /// The index in _leftValues of *args l-value. 
        /// _leftValues.Length if there is none.
        /// </summary>
        private readonly int _unsplattedValueIndex;

        public LeftValue/*!*/[]/*!*/ LeftValues {
            get { return _leftValues; }
        }

        public int UnsplattedValueIndex {
            get { return _unsplattedValueIndex; }
        }

        public LeftValue UnsplattedValue {
            get { return HasUnsplattedValue ? _leftValues[_unsplattedValueIndex] : null; }
        }

        public bool HasUnsplattedValue {
            get { return _unsplattedValueIndex < _leftValues.Length; }
        }

        public CompoundLeftValue(LeftValue/*!*/[]/*!*/ leftValues)
            : this(leftValues, leftValues.Length) {
        }

        public CompoundLeftValue(LeftValue/*!*/[]/*!*/ leftValues, int unsplattedValueIndex) 
            : base(SourceSpan.None) {
            Assert.NotNullItems(leftValues);
            Debug.Assert(unsplattedValueIndex >= 0 && unsplattedValueIndex <= leftValues.Length);
            _leftValues = leftValues;
            _unsplattedValueIndex = unsplattedValueIndex;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression targetValue, bool tryRead) {
            // called in Parameters.TransformForSuperCall
            throw new NotSupportedException("TODO: reading compound l-value");
        }

        internal override MSA.Expression TransformTargetRead(AstGenerator/*!*/ gen) {
            return null;
        }

        internal override MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression targetValue, MSA.Expression/*!*/ rightValue) {
            // called when dealing with nested compound LHS, e.g. in a,(b,c),d = x,y,z this method implements (b,c) = y assignment
            Debug.Assert(targetValue == null);
            return TransformWrite(gen, rightValue, true);
        }

        internal MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, Arguments/*!*/ rightValues) {
            Debug.Assert(!rightValues.IsEmpty);
            Assert.NotEmpty(_leftValues);

            if (_leftValues.Length == 1) {
                // (...) = RHS is equivalent to ... = RHS:
                CompoundLeftValue compound = _leftValues[0] as CompoundLeftValue;
                if (compound != null) {
                    return compound.TransformWrite(gen, rightValues);
                }

                if (!HasUnsplattedValue) {
                    return _leftValues[0].TransformWrite(gen, rightValues.TransformToArray(gen));
                }
            }

            if (rightValues.Expressions.Length == 1) {
                return TransformWrite(gen, rightValues.Expressions[0].TransformRead(gen), true);
            } else {
                return TransformWrite(gen, rightValues.TransformToArray(gen), false);
            }
        }

        private MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression/*!*/ transformedRight, bool isSimpleRhs) {
            var writes = new AstBlock();
            
            MSA.Expression rightList = gen.CurrentScope.DefineHiddenVariable("#rhs", typeof(IList));
            MSA.Expression result;

            if (isSimpleRhs) {
                // 1.9 returns the RHS, not an unsplatted array, if there is just a single RHS:
                result = gen.CurrentScope.DefineHiddenVariable("#pr", transformedRight.Type);
                writes.Add(Ast.Assign(result, transformedRight)); 

                transformedRight = AstUtils.LightDynamic(ImplicitSplatAction.Make(gen.Context), typeof(IList), result);
            } else {
                result = rightList;
            }

            writes.Add(Ast.Assign(rightList, transformedRight));

            for (int i = 0; i < _unsplattedValueIndex; i++) {
                writes.Add(_leftValues[i].TransformWrite(gen, Methods.GetArrayItem.OpCall(rightList, AstUtils.Constant(i))));
            }

            if (HasUnsplattedValue) {
                MSA.Expression explicitCount = AstUtils.Constant(_leftValues.Length - 1);

                // remaining RHS values:
                MSA.Expression array = Methods.GetArrayRange.OpCall(rightList, AstUtils.Constant(_unsplattedValueIndex), explicitCount);
                writes.Add(_leftValues[_unsplattedValueIndex].TransformWrite(gen, array));

                for (int i = _unsplattedValueIndex + 1; i < _leftValues.Length; i++) {
                    writes.Add(_leftValues[i].TransformWrite(gen, Methods.GetTrailingArrayItem.OpCall(rightList, AstUtils.Constant(_leftValues.Length - i), explicitCount)));
                }
            }

            writes.Add(result);
            return writes;
        }

        public override string/*!*/ ToString() {
            var result = new StringBuilder();
            bool first = true;

            for (int i = 0; i < _leftValues.Length; i++) {
                if (!first) {
                    result.Append(',');
                } else {
                    first = false;
                }

                if (i == _unsplattedValueIndex) {
                    result.Append('*');
                    result.Append(_leftValues[i].ToString());
                } else {
                    var compound = _leftValues[i] as CompoundLeftValue;
                    if (compound != null) {
                        result.Append('(');
                    }

                    result.Append(_leftValues[i].ToString());

                    if (compound != null) {
                        result.Append(')');
                    }
                }
            }

            return result.ToString();
        }
    }
}
