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
using System.Reflection.Emit;
using System.Dynamic.Utils;
using System.Diagnostics;

namespace System.Linq.Expressions.Compiler {

    internal sealed class LabelInfo {
        // The tree node representing this label
        internal readonly LabelTarget Node;

        // The IL label, will be mutated if Node is redefined
        internal Label Label { get; private set; }

        // The local that carries the label's value, if any
        internal readonly LocalBuilder Value;

        // The blocks where this label is defined. If it has more than one item,
        // the blocks can't be jumped to except from a child block
        private readonly Set<LabelBlockInfo> Definitions = new Set<LabelBlockInfo>();

        // Blocks that jump to this block
        private readonly List<LabelBlockInfo> References = new List<LabelBlockInfo>();

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
        private OpCode _opCode;

        private readonly ILGenerator _ilg;

        internal LabelInfo(ILGenerator il, LabelTarget node, bool canReturn) {
            _ilg = il;
            Node = node;
            Label = il.DefineLabel();
            _canReturn = canReturn;
            if (node != null && node.Type != typeof(void)) {
                Value = il.DeclareLocal(node.Type);
            }

            // Until we have more information, default to a leave instruction, which always works
            _opCode = OpCodes.Leave;
        }

        internal void Reference(LabelBlockInfo block) {
            References.Add(block);
            if (Definitions.Count > 0) {
                ValidateJump(block);
            }
        }

        // Returns true if the label was successfully defined
        // or false if the label is now ambiguous
        internal void Define(ILGenerator il, LabelBlockInfo block) {
            // Prevent the label from being shadowed, which enforces cleaner
            // trees. Also we depend on this for simplicity (keeping only one
            // active IL Label per LabelInfo)
            for (LabelBlockInfo j = block; j != null; j = j.Parent) {
                if (j.ContainsTarget(Node)) {
                    throw Error.LabelTargetAlreadyDefined(Node.Name);
                }
            }

            Definitions.Add(block);
            block.AddLabelInfo(Node, this);

            // Once defined, validate all jumps
            if (Definitions.Count == 1) {
                foreach (var r in References) {
                    ValidateJump(r);
                }
            } else {
                // Was just redefined, if we had any across block jumps, they're
                // now invalid
                if (_acrossBlockJump) {
                    throw Error.AmbiguousJump(Node.Name);
                }
                // For local jumps, we need a new IL label
                // This is okay because:
                //   1. no across block jumps have been made or will be made
                //   2. we don't allow the label to be shadowed
                Label = il.DefineLabel();
            }
        }

        private void ValidateJump(LabelBlockInfo reference) {
            // Assume we can do a ret/branch
            _opCode = _canReturn ? OpCodes.Ret : OpCodes.Br;

            // look for a simple jump out
            for (LabelBlockInfo j = reference; j != null; j = j.Parent) {
                if (Definitions.Contains(j)) {
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
            if (Definitions.Count > 1) {
                throw Error.AmbiguousJump(Node.Name);
            }

            // We didn't find an outward jump. Look for a jump across blocks
            LabelBlockInfo def = Definitions.First();
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
            if (References.Count > 0 && Definitions.Count == 0) {
                throw Error.LabelTargetUndefined(Node.Name);
            }
        }

        // Return directly if we can
        internal void EmitJump() {
            if (_opCode == OpCodes.Ret) {
                _ilg.Emit(OpCodes.Ret);
                return;
            }

            StoreValue();
            _ilg.Emit(_opCode, Label);
        }

        internal void StoreValue() {
            if (Value != null) {
                _ilg.Emit(OpCodes.Stloc, Value);
            }
        }

        // We always read the value from a local, because we don't know
        // if there will be a "leave" instruction targeting it ("branch"
        // preserves its stack, but "leave" empties the stack)
        internal void Mark() {
            _ilg.MarkLabel(Label);
            if (Value != null) {
                _ilg.Emit(OpCodes.Ldloc, Value);
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
