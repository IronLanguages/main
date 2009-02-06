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
            result.Define(_labelBlock);
            return result;
        }

        private void PushLabelBlock(LabelScopeKind type) {
            _labelBlock = new LabelScopeInfo(_labelBlock, type);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
        private void PopLabelBlock(LabelScopeKind kind) {
            Debug.Assert(_labelBlock != null && _labelBlock.Kind == kind);
            _labelBlock = _labelBlock.Parent;
        }

        private void EmitLabelExpression(Expression expr) {
            var node = (LabelExpression)expr;
            Debug.Assert(node.Target != null);

            // If we're an immediate child of a block, our label will already
            // be defined. If not, we need to define our own block so this
            // label isn't exposed except to its own child expression.
            LabelInfo label = null;

            if (_labelBlock.Kind == LabelScopeKind.Block) {
                _labelBlock.TryGetLabelInfo(node.Target, out label);

                // We're in a block but didn't find our label, try switch
                if (label == null && _labelBlock.Parent.Kind == LabelScopeKind.Switch) {
                    _labelBlock.Parent.TryGetLabelInfo(node.Target, out label);
                }

                // if we're in a switch or block, we should've found the label
                Debug.Assert(label != null);
            }

            if (label == null) {
                label = DefineLabel(node.Target);
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
                    if (_labelBlock.Kind != LabelScopeKind.Expression) {
                        PushLabelBlock(LabelScopeKind.Expression);
                        return true;
                    }
                    return false;
                case ExpressionType.Label:
                    // LabelExpression is a bit special, if it's directly in a
                    // block it becomes associate with the block's scope. Same
                    // thing if it's in a switch case body.
                    if (_labelBlock.Kind == LabelScopeKind.Block) {
                        var label = ((LabelExpression)node).Target;
                        if (_labelBlock.ContainsTarget(label)) {
                            return false;
                        }
                        if (_labelBlock.Parent.Kind == LabelScopeKind.Switch &&
                            _labelBlock.Parent.ContainsTarget(label)) {
                            return false;
                        }
                    }
                    PushLabelBlock(LabelScopeKind.Statement);
                    return true;
                case ExpressionType.Block:
                    if (node is SpilledExpressionBlock) {
                        // treat it as an expression
                        goto default;
                    }

                    PushLabelBlock(LabelScopeKind.Block);
                    // Labels defined immediately in the block are valid for
                    // the whole block.
                    if (_labelBlock.Parent.Kind != LabelScopeKind.Switch) {
                        DefineBlockLabels(node);
                    }
                    return true;
                case ExpressionType.Switch:
                    PushLabelBlock(LabelScopeKind.Switch);
                    // Define labels inside of the switch cases so theyare in
                    // scope for the whole switch. This allows "goto case" and
                    // "goto default" to be considered as local jumps.
                    var @switch = (SwitchExpression)node;
                    foreach (SwitchCase c in @switch.Cases) {
                        DefineBlockLabels(c.Body);
                    }
                    DefineBlockLabels(@switch.DefaultBody);
                    return true;
                case ExpressionType.Assign:
                    // Assignment where left side is a variable/parameter is
                    // safe to jump into
                    var assign = (BinaryExpression)node;
                    if (assign.Left.NodeType == ExpressionType.Parameter) {
                        PushLabelBlock(LabelScopeKind.Statement);
                        return true;
                    }
                    // Otherwise go to the default case
                    goto default;

                // Should we allow all UnaryExpressions to be treated as
                // statements? We need to allow at least convert because it's
                // essential in tree conversion (especially Void conversions)
                case ExpressionType.Convert:
                case ExpressionType.DebugInfo:
                case ExpressionType.Conditional:
                case ExpressionType.Loop:
                case ExpressionType.Goto:
                    PushLabelBlock(LabelScopeKind.Statement);
                    return true;
            }
        }

        private void DefineBlockLabels(Expression node) {
            var block = node as BlockExpression;
            if (block == null || block is SpilledExpressionBlock) {
                return;
            }
            for (int i = 0, n = block.ExpressionCount; i < n; i++) {
                Expression e = block.GetExpression(i);

                var label = e as LabelExpression;
                if (label != null) {
                    DefineLabel(label.Target);
                }
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
                        var label = ((LabelExpression)lambdaBody).Target;
                        _labelInfo.Add(label, new LabelInfo(_ilg, label, true));
                        return;
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
