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
        /// <summary>
        /// Empty LHS - used by blocks.
        /// </summary>
        internal static readonly CompoundLeftValue/*!*/ EmptyBlockSignature = new CompoundLeftValue(LeftValue.EmptyList, null, SourceSpan.None);

        /// <summary>
        /// Unspecified LHS - used by blocks.
        /// </summary>
        internal static readonly CompoundLeftValue/*!*/ UnspecifiedBlockSignature = new CompoundLeftValue(LeftValue.EmptyList, null, SourceSpan.None);

        /// <summary>
        /// List of l-values, possibly compound.
        /// </summary>
        private readonly List<LeftValue>/*!*/ _leftValues;

        /// <summary>
        /// Residual l-value (l-value following the * sentinel).
        /// </summary>
        private readonly LeftValue _unsplattedValue;

        public List<LeftValue>/*!*/ LeftValues {
            get { return _leftValues; }
        }

        public LeftValue UnsplattedValue {
            get { return _unsplattedValue; }
        }

        public CompoundLeftValue(List<LeftValue>/*!*/ leftValues, LeftValue unsplattedValue, SourceSpan location)
            : base(location) {
            Assert.NotNull(leftValues);

            _leftValues = leftValues;
            _unsplattedValue = unsplattedValue;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression targetValue, bool tryRead) {
            throw Assert.Unreachable;
        }

        internal override MSA.Expression TransformTargetRead(AstGenerator/*!*/ gen) {
            return null;
        }

        internal override MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression targetValue, MSA.Expression/*!*/ rightValue) {
            Debug.Assert(targetValue == null);
            return TransformWrite(gen, new AstExpressions { rightValue }, null);
        }

        internal MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, CompoundRightValue/*!*/ rhs) {
            return TransformWrite(gen, gen.TranformExpressions(rhs.RightValues), (rhs.SplattedValue != null) ? rhs.SplattedValue.TransformRead(gen) : null);
        }

        private MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, AstExpressions/*!*/ rightValues, MSA.Expression splattedValue) {

            // We need to distinguish various special cases here.
            // Each of the bool variables defined below is true iff the corresponding special form of LHS/RHS occurs.
            // These flags drive the DLR AST being produced by this method.
            // For parallel assignment specification, see "Ruby Language.docx/Runtime/Parallel Assignment".

            // L(0,-) not applicable
            Debug.Assert(!(_leftValues.Count == 0 && _unsplattedValue == null));

            // L(1,-)?
            bool leftOneNone = _leftValues.Count == 1 && _unsplattedValue == null;

            // L(0,*)?
            bool leftNoneSplat = _leftValues.Count == 0 && _unsplattedValue != null;

            // R(0,*)?
            bool rightNoneSplat = rightValues.Count == 0 && splattedValue != null;

            // R(1,-)?
            bool rightOneNone = rightValues.Count == 1 && splattedValue == null;

            // R(1,*)?
            bool rightOneSplat = rightValues.Count == 1 && splattedValue != null;

            // R(0,-) not applicable
            Debug.Assert(!(rightValues.Count == 0 && splattedValue == null));

            MSA.Expression resultExpression;

            if (leftOneNone) {
                // L(1,-):

                // recurse right away (X) = RHS is equivalent to X = RHS:
                CompoundLeftValue compound = _leftValues[0] as CompoundLeftValue;
                if (compound != null) {
                    return compound.TransformWrite(gen, rightValues, splattedValue);
                }

                if (rightOneSplat) {
                    // R(1,*)
                    resultExpression = Methods.SplatPair.OpCall(
                        AstUtils.Box(rightValues[0]),
                        AstUtils.LightDynamic(SplatAction.Make(gen.Context), typeof(IList), splattedValue)
                    );
                } else {
                    // case 1: R(1,-)
                    // case 2: R(0,*) 
                    // case 3: otherwise
                    resultExpression = Arguments.TransformRead(gen, rightValues, splattedValue, true /* Splat */);
                }

                return _leftValues[0].TransformWrite(gen, resultExpression);
            }

            bool optimizeReads = true;

            if (rightOneNone && !leftNoneSplat) {
                // R(1,-) && !L(0,*)
                resultExpression = Methods.Unsplat.OpCall(
                    AstUtils.LightDynamic(ConvertToArraySplatAction.Make(gen.Context), rightValues[0])
                );
                optimizeReads = false;
            } else {
                // case 1: R(0,*) = L
                // case 2: otherwise
                resultExpression = Arguments.TransformRead(gen, rightValues, splattedValue, false /* Unsplat */);
                optimizeReads = !rightNoneSplat;
            }

            var writes = new AstBlock();

            MSA.Expression result = gen.CurrentScope.DefineHiddenVariable("#rhs", typeof(IList));
            writes.Add(Ast.Assign(result, resultExpression));

            MethodInfo itemGetter = Methods.IList_get_Item;
            for (int i = 0; i < _leftValues.Count; i++) {
                MSA.Expression rvalue;

                if (optimizeReads) {
                    if (i < rightValues.Count) {
                        // unchecked get item:
                        rvalue = Ast.Call(result, itemGetter, AstUtils.Constant(i));
                    } else if (splattedValue != null) {
                        // checked get item:
                        rvalue = Methods.GetArrayItem.OpCall(result, AstUtils.Constant(i));
                    } else {
                        // missing item:
                        rvalue = AstUtils.Constant(null);
                    }
                } else {
                    rvalue = Methods.GetArrayItem.OpCall(result, AstUtils.Constant(i));
                }

                writes.Add(_leftValues[i].TransformWrite(gen, rvalue));
            }

            // unsplatting the rest of rhs values into an array:
            if (_unsplattedValue != null) {
                // copies the rest of resulting array to the *LHS;
                // the resulting array contains splatted *RHS - no need for additional appending:
                MSA.Expression array = Methods.GetArraySuffix.OpCall(result, AstUtils.Constant(_leftValues.Count));

                // assign the array (possibly empty) to *LHS:
                writes.Add(_unsplattedValue.TransformWrite(gen, array));
            }

            writes.Add(result);
            return writes;
        }

        internal BlockSignatureAttributes GetBlockSignatureAttributes() {
            var result = BlockSignatureAttributes.None;
            
            CompoundLeftValue compound;
            
            if (_unsplattedValue != null) {
                result |= BlockSignatureAttributes.HasUnsplatParameter;
                compound = this;
            } else if (_leftValues.Count == 1 && (compound = _leftValues[0] as CompoundLeftValue) != null) {
                result |= BlockSignatureAttributes.HasSingleCompoundParameter;
            } else {
                compound = this;
            }

            int arity;

            if (this == UnspecifiedBlockSignature) {
                arity = -1;
            } else {
                arity = compound._leftValues.Count;
                if (compound._unsplattedValue != null) {
                    arity = -arity - 1;
                } else if (compound._leftValues.Count > 0 && compound._leftValues[compound._leftValues.Count - 1] is Placeholder) {
                    arity--;
                }
            }

            return BlockDispatcher.MakeAttributes(result, arity);
        }

        public override string/*!*/ ToString() {
            var result = new StringBuilder();
            bool first = true;
            
            for (int i = 0; i < _leftValues.Count; i++) {
                if (!first) {
                    result.Append(',');
                } else {
                    first = false;
                }
                
                var compound = _leftValues[i] as CompoundLeftValue;
                if (compound != null) {
                    result.Append('(');
                }

                result.Append(_leftValues[i].ToString());
                
                if (compound != null) {
                    result.Append(')');
                }
            }

            if (_unsplattedValue != null) {
                if (!first) {
                    result.Append(',');
                } else {
                    first = false;
                }
                result.Append('*');
                result.Append(_unsplattedValue.ToString());
            }

            return result.ToString();
        }
    }
}
