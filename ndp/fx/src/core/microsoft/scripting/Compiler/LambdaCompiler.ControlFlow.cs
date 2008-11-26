/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Diagnostics;
using System.Reflection.Emit;
using System.Dynamic.Utils;

namespace System.Linq.Expressions.Compiler {

    // The part of the LambdaCompiler dealing with low level control flow
    // break, contiue, return, exceptions, etc
    partial class LambdaCompiler {

        private LabelInfo EnsureLabel(LabelTarget node) {
            LabelInfo result;
            if (!_labelInfo.TryGetValue(node, out result)) {
                _labelInfo.Add(node, result = new LabelInfo(_ilg, node, false));
            }
            return result;
        }

        private LabelInfo ReferenceLabel(LabelTarget node) {
            LabelInfo result = EnsureLabel(node);
            result.Reference(_labelBlock);
            return result;
        }

        private LabelInfo DefineLabel(LabelTarget node) {
            if (node == null) {
                return new LabelInfo(_ilg, null, false);
            }
            LabelInfo result = EnsureLabel(node);
            result.Define(_ilg, _labelBlock);
            return result;
        }

        private void PushLabelBlock(LabelBlockKind type) {
            _labelBlock = new LabelBlockInfo(_labelBlock, type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
        private void PopLabelBlock(LabelBlockKind kind) {
            Debug.Assert(_labelBlock != null && _labelBlock.Kind == kind);
            _labelBlock = _labelBlock.Parent;
        }

        private void EmitLabelExpression(Expression expr) {
            var node = (LabelExpression)expr;
            Debug.Assert(node.Label != null);

            // If we're an immediate child of a block, our label will already
            // be defined. If not, we need to define our own block so this
            // label isn't exposed except to its own child expression.
            LabelInfo label;
            if (!_labelBlock.TryGetLabelInfo(node.Label, out label)) {
                label = DefineLabel(node.Label);
            }

            if (node.DefaultValue != null) {
                EmitExpression(node.DefaultValue);
            }

            label.Mark();
        }

        private void EmitGotoExpression(Expression expr) {
            var node = (GotoExpression)expr;
            if (node.Value != null) {
                EmitExpression(node.Value);
            }

            ReferenceLabel(node.Target).EmitJump();           
        }

        private bool TryPushLabelBlock(Expression node) {
            // Anything that is "statement-like" -- e.g. has no associated
            // stack state can be jumped into, with the exception of try-blocks
            // We indicate this by a "Block"
            // 
            // Otherwise, we push an "Expression" to indicate that it can't be
            // jumped into
            switch (node.NodeType) {
                default:
                    if (_labelBlock.Kind != LabelBlockKind.Expression) {
                        PushLabelBlock(LabelBlockKind.Expression);
                        return true;
                    }
                    return false;
                case ExpressionType.Label:
                    // LabelExpression is a bit special, if it's directly in a block
                    // it becomes associate with the block's scope
                    if (_labelBlock.Kind != LabelBlockKind.Block ||
                        !_labelBlock.ContainsTarget(((LabelExpression)node).Label)) {
                        PushLabelBlock(LabelBlockKind.Block);
                        return true;
                    }
                    return false;
                case ExpressionType.Assign:
                    // Assignment where left side is a variable/parameter is
                    // safe to jump into
                    var assign = (BinaryExpression)node;
                    if (assign.Left.NodeType == ExpressionType.Parameter) {
                        PushLabelBlock(LabelBlockKind.Block);
                        return true;
                    }
                    // Otherwise go to the default case
                    goto default;                    
                case ExpressionType.DebugInfo:
                case ExpressionType.Conditional:
                case ExpressionType.Block:
                case ExpressionType.Switch:
                case ExpressionType.Loop:
                case ExpressionType.Goto:
                    PushLabelBlock(LabelBlockKind.Block);
                    return true;
            }
        }

        // See if this lambda has a return label
        // If so, we'll create it now and mark it as allowing the "ret" opcode
        // This allows us to generate better IL
        private void AddReturnLabel(Expression lambdaBody) {
            while (true) {
                switch (lambdaBody.NodeType) {
                    default:
                        // Didn't find return label
                        return;
                    case ExpressionType.Label:
                        // Found it!
                        var label = ((LabelExpression)lambdaBody).Label;
                        _labelInfo.Add(label, new LabelInfo(_ilg, label, true));
                        return;
                    case ExpressionType.DebugInfo:
                        // Look in the body
                        lambdaBody = ((DebugInfoExpression)lambdaBody).Expression;
                        continue;
                    case ExpressionType.Block:
                        // Look in the last expression of a block
                        var body = (BlockExpression)lambdaBody;                        
                        lambdaBody = body.GetExpression(body.ExpressionCount - 1);
                        continue;
                }
            }
        }
    }
}
