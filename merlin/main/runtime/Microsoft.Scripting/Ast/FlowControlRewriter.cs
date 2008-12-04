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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// The purpose of this rewriter is simple: ETs do not allow jumps (break, continue, return, goto)
    /// that would go through a finally/fault. So we replace them with code that instead stores a flag,
    /// and then jumps to the end of the finally/fault. At the end of the try-finally, we emit a switch
    /// that then jumps to the correct label.
    /// 
    /// A few things that make this more complicated:
    /// 
    ///   1. If a finally contains a jump out, then jumps in the try/catch need to be replaced as well.
    ///      It's to support cases like this:
    ///          # returns 234
    ///          def foo():
    ///              try: return 123
    ///              finally: return 234 
    ///      
    ///      We need to replace the "return 123" because after it jumps, we'll go to the finally, which
    ///      might decide to jump again, but once the IL finally exits, it ignores the finally jump and
    ///      keeps going with the original jump. The moral of the story is: if any jumps in finally are
    ///      rewritten, try/catch jumps must be also.
    ///      
    ///  2. To generate better code, we only have one state variable, so if we have to jump out of
    ///     multiple finallys we just keep jumping. It looks sort of like this:
    ///       foo:
    ///       try { ... } finally {
    ///           try { ... } finally {
    ///             ...
    ///             if (...) {
    ///                 // was: goto foo;
    ///                 $flow = 1; goto endInnerFinally; 
    ///             }
    ///             ...
    ///             endInnerFinally:
    ///           }
    ///           switch ($flow) {
    ///               case 1: goto endOuterFinally;
    ///           }
    ///           ...
    ///           endOuterFinally:
    ///       }
    ///       switch ($flow) {
    ///         case 1: $flow = 0; goto foo;
    ///       }
    ///       ...
    /// 
    /// TODO: the compiler could optimize the switch/goto pattern, but it doesn't yet
    /// </summary>
    internal sealed class FlowControlRewriter : ExpressionVisitor {

        private sealed class BlockInfo {
            // Is this block a finally?
            internal bool InFinally;
            // Does this block need flow control?
            internal bool HasFlow {
                get { return FlowLabel != null; }
            }

            // Labels defined in this block
            // So we can figure out if we can just jump directly or if we need help
            internal readonly Set<LabelTarget> LabelDefs = new Set<LabelTarget>();

            // These two properties tell us what we need to emit in the flow control
            // (if anything)
            internal Set<LabelTarget> NeedFlowLabels;

            // To emit a jump that we can't do in IL, we set the state variable
            // and then jump to FlowLabel. It's up to the code at FlowLabel to
            // handle the jump
            internal LabelTarget FlowLabel;
        }

        private struct LabelInfo {
            internal readonly int FlowState;
            internal readonly ParameterExpression Variable;

            internal LabelInfo(int index, Type varType) {
                FlowState = index;
                if (varType != typeof(void)) {
                    Variable = Expression.Variable(varType, null);
                } else {
                    Variable = null;
                }
            }
        }

        private readonly Dictionary<LabelTarget, LabelInfo> _labels = new Dictionary<LabelTarget, LabelInfo>();
        private readonly Stack<BlockInfo> _blocks = new Stack<BlockInfo>();
        private ParameterExpression _flowVariable;

        // Rewriter entry point
        internal Expression Reduce(Expression node) {
            _blocks.Push(new BlockInfo());
            node = Visit(node);

            if (_flowVariable != null) {
                var vars = new List<ParameterExpression>();
                vars.Add(_flowVariable);
                foreach (var info in _labels.Values) {
                    if (info.Variable != null) {
                        vars.Add(info.Variable);
                    }
                }
                node = Expression.Block(vars, node);
            }
            _blocks.Pop();
            return node;
        }

        private void EnsureFlow(BlockInfo block) {
            if (_flowVariable == null) {
                _flowVariable = Expression.Variable(typeof(int), "$flow");
            }
            if (!block.HasFlow) {
                block.FlowLabel = Expression.Label();
                block.NeedFlowLabels = new Set<LabelTarget>();
            }
        }

        private LabelInfo EnsureLabelInfo(LabelTarget target) {
            LabelInfo result;
            if (!_labels.TryGetValue(target, out result)) {
                _labels.Add(target, result = new LabelInfo(_labels.Count + 1, target.Type));
            }
            return result;
        }

        protected override Expression VisitExtension(Expression node) {
            var ffc = node as FinallyFlowControlExpression;
            if (ffc != null) {
                // Unwrap nested finally flow expressions
                // We can generate better code by walking all of them now
                return Visit(ffc.Body);
            }
            return base.VisitExtension(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node) {
            // don't recurse into nested lambdas
            return node;
        }

        protected override Expression VisitTry(TryExpression node) {
            // Visit finally/fault block first
            BlockInfo block = new BlockInfo { InFinally = true };
            _blocks.Push(block);
            Expression @finally = Visit(node.Finally);
            Expression fault = Visit(node.Fault);
            block.InFinally = false;

            LabelTarget finallyEnd = block.FlowLabel;
            if (finallyEnd != null) {
                if (@finally != null) {
                    @finally = Expression.Label(finallyEnd, @finally);
                }
                if (fault != null) {
                    fault = Expression.Label(finallyEnd, fault);
                }
                // Make a new target, which will be emitted after the try
                block.FlowLabel = Expression.Label();
            }

            Expression @try = Visit(node.Body);
            IList<CatchBlock> handlers = Visit(node.Handlers, VisitCatchBlock);
            _blocks.Pop();

            if (@try == node.Body &&
                handlers == node.Handlers &&
                @finally == node.Finally &&
                fault == node.Fault) {
                return node;
            }

            if (!block.HasFlow) {
                return Expression.MakeTry(@try, @finally, fault, handlers);
            }

            //  If there is a control flow in finally, emit outer:
            //  try {
            //      // try block body and all catch handling
            //  } catch (Exception all) {
            //      saved = all;
            //  } finally {
            //      finally_body
            //      if (saved != null) {
            //          throw saved;
            //      }
            //  }
            //  
            //  If we have a fault handler we turn this into the better:
            //  try {
            //      // try block body and all catch handling
            //  } catch (Exception all) {
            //      saved = all;
            //      fault_body
            //      throw saved
            //  }

            if (handlers.Count > 0) {
                @try = Expression.MakeTry(@try, null, null, handlers);
            }

            var all = Expression.Variable(typeof(Exception), "$exception");
            if (@finally != null) {
                handlers = new[] { Expression.Catch(all.Type, Expression.Default(node.Type)) };
                @finally = Expression.Block(
                    @finally,
                    Expression.Condition(
                        Expression.NotEqual(all, Expression.Constant(null, all.Type)),
                        Expression.Throw(all),
                        Expression.Empty()
                    )
                );
            } else {
                handlers = new[] {
                    Expression.Catch(
                        all.Type, 
                        Expression.Block(fault, Expression.Throw(all))
                    )
                };
                fault = null;
            }

            // Emit flow control
            return Expression.Block(
                new ParameterExpression[] { all },
                Expression.MakeTry(@try, @finally, fault, handlers),
                Expression.Label(block.FlowLabel),
                MakeFlowControlSwitch(block)
            );
        }

        private Expression MakeFlowControlSwitch(BlockInfo block) {
            var cases = block.NeedFlowLabels.Map(
                target => Expression.SwitchCase(_labels[target].FlowState, MakeFlowJump(target))
            );
            return Expression.Switch(_flowVariable, null, new ReadOnlyCollection<SwitchCase>(cases));
        }

        // Determine if we can break directly to the label, or if we need to dispatch again
        // If we're breaking directly, we reset the _flowVariable, otherwise we just jump to
        // the next FlowLabel
        private Expression MakeFlowJump(LabelTarget target) {
            foreach (var block in _blocks) {
                if (block.LabelDefs.Contains(target)) {
                    break;
                }
                if (block.InFinally || block.HasFlow) {
                    EnsureFlow(block);
                    block.NeedFlowLabels.Add(target);
                    // If we need to go through another finally, just jump
                    // to its flow label
                    return Expression.Goto(block.FlowLabel);
                }
            }
            // Got here without needing flow, reset the flag and emit the real goto
            return Expression.Block(
                Expression.Assign(_flowVariable, Expression.Constant(0)),
                Expression.Goto(target, _labels[target].Variable)
            );
        }

        protected override Expression VisitGoto(GotoExpression node) {
            foreach (var block in _blocks) {
                if (block.LabelDefs.Contains(node.Target)) {
                    break;
                }
                if (block.InFinally || block.HasFlow) {
                    EnsureFlow(block);
                    block.NeedFlowLabels.Add(node.Target);
                    LabelInfo info = EnsureLabelInfo(node.Target);

                    var assignFlow = Expression.Assign(_flowVariable, Expression.Constant(info.FlowState));
                    var gotoFlow = Expression.Goto(block.FlowLabel);
                    if (info.Variable == null) {
                        return Expression.Block(assignFlow, gotoFlow);
                    }

                    var saveValue = Expression.Assign(info.Variable, node.Value);
                    return Expression.Block(saveValue, assignFlow, gotoFlow);
                }
            }
            return base.VisitGoto(node);
        }

        protected override Expression VisitBlock(BlockExpression node) {
            // Grab all labels in the block and define them in the block's scope
            // Labels defined immediately in the block are valid for the whole block
            foreach (var e in node.Expressions) {
                var label = e as LabelExpression;
                if (label != null) {
                    VisitLabelTarget(label.Target);
                }
            }
            return base.VisitBlock(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node) {
            if (node != null) {
                EnsureLabelInfo(node);
                _blocks.Peek().LabelDefs.Add(node);
            }
            return node;
        }
    }
}
