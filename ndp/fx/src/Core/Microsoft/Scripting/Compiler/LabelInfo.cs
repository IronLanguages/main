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

using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Reflection.Emit;

namespace System.Linq.Expressions.Compiler {

    /// <summary>
    /// Contains compiler state corresponding to a LabelTarget
    /// </summary>
    internal sealed class LabelInfo {
        // The tree node representing this label
        private readonly LabelTarget _node;

        // The IL label, will be mutated if Node is redefined
        private Label _label;
        private bool _labelDefined;

        internal Label Label {
            get {
                EnsureLabelAndValue();
                return _label;
            }
        }

        // The local that carries the label's value, if any
        private LocalBuilder _value;

        // The blocks where this label is defined. If it has more than one item,
        // the blocks can't be jumped to except from a child block
        private readonly Set<LabelBlockInfo> _definitions = new Set<LabelBlockInfo>();

        // Blocks that jump to this block
        private readonly List<LabelBlockInfo> _references = new List<LabelBlockInfo>();

        // True if this label is the last thing in this block
        // (meaning we can emit a direct return)
        private readonly bool _canReturn;

        // True if at least one jump is across blocks
        // If we have any jump across blocks to this label, then the
        // LabelTarget can only be defined in one place
        private bool _acrossBlockJump;

        // Until we have more information, default to a leave instruction,
        // which always works. Note: leave spills the stack, so we need to
        // ensure that StackSpiller has guarenteed us an empty stack at this
        // point. Otherwise Leave and Branch are not equivalent
        private OpCode _opCode = OpCodes.Leave;

        private readonly ILGenerator _ilg;

        internal LabelInfo(ILGenerator il, LabelTarget node, bool canReturn) {
            _ilg = il;
            _node = node;
            _canReturn = canReturn;
        }

        internal void Reference(LabelBlockInfo block) {
            _references.Add(block);
            if (_definitions.Count > 0) {
                ValidateJump(block);
            }
        }

        // Returns true if the label was successfully defined
        // or false if the label is now ambiguous
        internal void Define(LabelBlockInfo block) {
            // Prevent the label from being shadowed, which enforces cleaner
            // trees. Also we depend on this for simplicity (keeping only one
            // active IL Label per LabelInfo)
            for (LabelBlockInfo j = block; j != null; j = j.Parent) {
                if (j.ContainsTarget(_node)) {
                    throw Error.LabelTargetAlreadyDefined(_node.Name);
                }
            }

            _definitions.Add(block);
            block.AddLabelInfo(_node, this);

            // Once defined, validate all jumps
            if (_definitions.Count == 1) {
                foreach (var r in _references) {
                    ValidateJump(r);
                }
            } else {
                // Was just redefined, if we had any across block jumps, they're
                // now invalid
                if (_acrossBlockJump) {
                    throw Error.AmbiguousJump(_node.Name);
                }
                // For local jumps, we need a new IL label
                // This is okay because:
                //   1. no across block jumps have been made or will be made
                //   2. we don't allow the label to be shadowed
                _labelDefined = false;
            }
        }

        private void ValidateJump(LabelBlockInfo reference) {
            // Assume we can do a ret/branch
            _opCode = _canReturn ? OpCodes.Ret : OpCodes.Br;

            // look for a simple jump out
            for (LabelBlockInfo j = reference; j != null; j = j.Parent) {
                if (_definitions.Contains(j)) {
                    // found it, jump is valid!
                    return;
                }
                if (j.Kind == LabelBlockKind.Finally ||
                    j.Kind == LabelBlockKind.Filter) {
                    break;
                }
                if (j.Kind == LabelBlockKind.Try ||
                    j.Kind == LabelBlockKind.Catch) {
                    _opCode = OpCodes.Leave;
                }
            }

            _acrossBlockJump = true;
            if (_definitions.Count > 1) {
                throw Error.AmbiguousJump(_node.Name);
            }

            // We didn't find an outward jump. Look for a jump across blocks
            LabelBlockInfo def = _definitions.First();
            LabelBlockInfo common = Helpers.CommonNode(def, reference, b => b.Parent);

            // Assume we can do a ret/branch
            _opCode = _canReturn ? OpCodes.Ret : OpCodes.Br;

            // Validate that we aren't jumping across a finally
            for (LabelBlockInfo j = reference; j != common; j = j.Parent) {
                if (j.Kind == LabelBlockKind.Finally) {
                    throw Error.ControlCannotLeaveFinally();
                }
                if (j.Kind == LabelBlockKind.Filter) {
                    throw Error.ControlCannotLeaveFilterTest();
                }
                if (j.Kind == LabelBlockKind.Try ||
                    j.Kind == LabelBlockKind.Catch) {
                    _opCode = OpCodes.Leave;
                }
            }

            // Valdiate that we aren't jumping into a catch or an expression
            for (LabelBlockInfo j = def; j != common; j = j.Parent) {
                if (j.Kind != LabelBlockKind.Block) {
                    if (j.Kind == LabelBlockKind.Expression) {
                        throw Error.ControlCannotEnterExpression();
                    } else {
                        throw Error.ControlCannotEnterTry();
                    }
                }
            }
        }

        internal void ValidateFinish() {
            // Make sure that if this label was jumped to, it is also defined
            if (_references.Count > 0 && _definitions.Count == 0) {
                throw Error.LabelTargetUndefined(_node.Name);
            }
        }

        internal void EmitJump() {
            // Return directly if we can
            if (_opCode == OpCodes.Ret) {
                _ilg.Emit(OpCodes.Ret);
            } else {
                StoreValue();
                _ilg.Emit(_opCode, Label);
            }
        }

        private void StoreValue() {
            EnsureLabelAndValue();
            if (_value != null) {
                _ilg.Emit(OpCodes.Stloc, _value);
            }
        }

        internal void Mark() {
            if (_canReturn) {
                // Don't mark return labels unless they were actually jumped to
                // (returns are last so we know for sure if anyone jumped to it)
                if (!_labelDefined) {
                    // We don't even need to emit the "ret" because
                    // LambdaCompiler does that for us.
                    return;
                }

                // Otherwise, emit something like:
                // ret
                // <marked label>:
                // ldloc <value>
                _ilg.Emit(OpCodes.Ret);
            } else {

                // For the normal case, we emit:
                // stloc <value>
                // <marked label>:
                // ldloc <value>
                StoreValue();
            }
            MarkWithEmptyStack();
        }

        // Like Mark, but assumes the stack is empty
        internal void MarkWithEmptyStack() {
            _ilg.MarkLabel(Label);
            if (_value != null) {
                // We always read the value from a local, because we don't know
                // if there will be a "leave" instruction targeting it ("branch"
                // preserves its stack, but "leave" empties the stack)
                _ilg.Emit(OpCodes.Ldloc, _value);
            }
        }

        private void EnsureLabelAndValue() {
            if (!_labelDefined) {
                _labelDefined = true;
                _label = _ilg.DefineLabel();
                if (_node != null && _node.Type != typeof(void)) {
                    _value = _ilg.DeclareLocal(_node.Type);
                }
            }
        }
    }

    internal enum LabelBlockKind {
        Block,
        Expression,
        Try,
        Catch,
        Finally,
        Filter,
    }

    internal sealed class LabelBlockInfo {
        private Dictionary<LabelTarget, LabelInfo> Labels; // lazily allocated, we typically use this only once every 6th-7th block
        internal readonly LabelBlockKind Kind;
        internal readonly LabelBlockInfo Parent;

        internal LabelBlockInfo(LabelBlockInfo parent, LabelBlockKind kind) {
            Parent = parent;
            Kind = kind;
        }

        internal bool ContainsTarget(LabelTarget target) {
            if (Labels == null) {
                return false;
            }

            return Labels.ContainsKey(target);
        }

        internal bool TryGetLabelInfo(LabelTarget target, out LabelInfo info) {
            if (Labels == null) {
                info = null;
                return false;
            }

            return Labels.TryGetValue(target, out info);
        }

        internal void AddLabelInfo(LabelTarget target, LabelInfo info) {
            Debug.Assert(Kind == LabelBlockKind.Block);

            if (Labels == null) {
                Labels = new Dictionary<LabelTarget, LabelInfo>();
            }

            Labels.Add(target, info);
        }
    }
}
