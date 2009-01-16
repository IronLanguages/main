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

using System.Reflection.Emit;
using System.Diagnostics;

namespace System.Linq.Expressions.Compiler {
    partial class LambdaCompiler {
        private void EmitBlockExpression(Expression expr) {
            // emit body
            Emit((BlockExpression)expr, EmitAs.Default);
        }

        private void Emit(BlockExpression node, EmitAs emitAs) {
            EnterScope(node);

            int count = node.ExpressionCount;
            for (int index = 0; index < count - 1; index++) {
                EmitExpressionAsVoid(node.GetExpression(index));
            }

            // if the type of Block it means this is not a Comma
            // so we will force the last expression to emit as void.
            if (emitAs == EmitAs.Void || node.Type == typeof(void)) {
                EmitExpressionAsVoid(node.GetExpression(count - 1));
            } else {
                EmitExpression(node.GetExpression(count - 1));
            }

            ExitScope(node);
        }

        private void EnterScope(BlockExpression node) {
            if (node.Variables.Count > 0 &&
                (_scope.MergedScopes == null || !_scope.MergedScopes.Contains(node))) {

                CompilerScope scope;
                if (!_tree.Scopes.TryGetValue(node, out scope)) {
                    //
                    // Very often, we want to compile nodes as reductions
                    // rather than as IL, but usually they need to allocate
                    // some IL locals. To support this, we allow emitting a
                    // BlockExpression that was not bound by VariableBinder.
                    // This works as long as the variables are only used
                    // locally -- i.e. not closed over.
                    //
                    // User-created blocks will never hit this case; only our
                    // internally reduced nodes will.
                    //
                    scope = new CompilerScope(node) { NeedsClosure = _scope.NeedsClosure };
                }

                _scope = scope.Enter(this, _scope);
                Debug.Assert(_scope.Node == node);
            }
        }

        private void ExitScope(BlockExpression node) {
            if (_scope.Node == node) {
                _scope = _scope.Exit();
            }
        }

        private void EmitDefaultExpression(Expression expr) {
            var node = (DefaultExpression)expr;
            if (node.Type != typeof(void)) {
                // emit default(T)
                _ilg.EmitDefault(node.Type);
            }
        }

        private void EmitLoopExpression(Expression expr) {
            LoopExpression node = (LoopExpression)expr;

            PushLabelBlock(LabelBlockKind.Block);
            LabelInfo breakTarget = DefineLabel(node.BreakLabel);
            LabelInfo continueTarget = DefineLabel(node.ContinueLabel);

            continueTarget.MarkWithEmptyStack();

            EmitExpressionAsVoid(node.Body);

            _ilg.Emit(OpCodes.Br, continueTarget.Label);

            PopLabelBlock(LabelBlockKind.Block);

            breakTarget.MarkWithEmptyStack();
        }

        #region SwitchStatement

        private void EmitSwitchExpression(Expression expr) {
            SwitchExpression node = (SwitchExpression)expr;

            LabelInfo breakTarget = DefineLabel(node.BreakLabel);

            Label defaultTarget = breakTarget.Label;
            Label[] labels = new Label[node.SwitchCases.Count];

            // Create all labels
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                labels[i] = _ilg.DefineLabel();

                // Default case.
                if (node.SwitchCases[i].IsDefault) {
                    // Set the default target
                    defaultTarget = labels[i];
                }
            }

            // Emit the test value
            EmitExpression(node.Test);

            // Check if jmp table can be emitted
            if (!TryEmitJumpTable(node, labels, defaultTarget)) {
                // There might be scenario(s) where the jmp table is not emitted
                // Emit the switch as conditional branches then
                EmitConditionalBranches(node, labels);
            }

            // If "default" present, execute default code, else exit the switch            
            _ilg.Emit(OpCodes.Br, defaultTarget);

            // Emit the bodies
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                // First put the corresponding labels
                _ilg.MarkLabel(labels[i]);
                // And then emit the Body!!
                EmitExpressionAsVoid(node.SwitchCases[i].Body);
            }

            breakTarget.MarkWithEmptyStack();
        }

        private const int MaxJumpTableSize = 65536;
        private const double MaxJumpTableSparsity = 10;

        // Emits the switch as if stmts
        private void EmitConditionalBranches(SwitchExpression node, Label[] labels) {
            LocalBuilder testValueSlot = GetLocal(typeof(int));
            _ilg.Emit(OpCodes.Stloc, testValueSlot);

            // For all the "cases" create their conditional branches
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                // Not default case emit the condition
                if (!node.SwitchCases[i].IsDefault) {
                    // Test for equality of case value and the test expression
                    _ilg.EmitInt(node.SwitchCases[i].Value);
                    _ilg.Emit(OpCodes.Ldloc, testValueSlot);
                    _ilg.Emit(OpCodes.Beq, labels[i]);
                }
            }

            FreeLocal(testValueSlot);
        }

        // Tries to emit switch as a jmp table
        private bool TryEmitJumpTable(SwitchExpression node, Label[] labels, Label defaultTarget) {
            if (node.SwitchCases.Count > MaxJumpTableSize) {
                return false;
            }

            int min = Int32.MaxValue;
            int max = Int32.MinValue;

            // Find the min and max of the values
            for (int i = 0; i < node.SwitchCases.Count; ++i) {
                // Not the default case.
                if (!node.SwitchCases[i].IsDefault) {
                    int val = node.SwitchCases[i].Value;
                    if (min > val) min = val;
                    if (max < val) max = val;
                }
            }

            long delta = (long)max - (long)min;
            if (delta > MaxJumpTableSize) {
                return false;
            }

            // Value distribution is too sparse, don't emit jump table.
            if (delta > node.SwitchCases.Count + MaxJumpTableSparsity) {
                return false;
            }

            // The actual jmp table of switch
            int len = (int)delta + 1;
            Label[] jmpLabels = new Label[len];

            // Initialize all labels to the default
            for (int i = 0; i < len; i++) {
                jmpLabels[i] = defaultTarget;
            }

            // Replace with the actual label target for all cases
            for (int i = 0; i < node.SwitchCases.Count; i++) {
                SwitchCase sc = node.SwitchCases[i];
                if (!sc.IsDefault) {
                    jmpLabels[sc.Value - min] = labels[i];
                }
            }

            // Emit the normalized index and then switch based on that
            if (min != 0) {
                _ilg.EmitInt(min);
                _ilg.Emit(OpCodes.Sub);
            }
            _ilg.Emit(OpCodes.Switch, jmpLabels);
            return true;
        }

        #endregion

        private void CheckRethrow() {
            // Rethrow is only valid inside a catch.
            for (LabelBlockInfo j = _labelBlock; j != null; j = j.Parent) {
                if (j.Kind == LabelBlockKind.Catch) {
                    return;
                } else if (j.Kind == LabelBlockKind.Finally) {
                    // Rethrow from inside finally is not verifiable
                    break;
                }
            }
            throw Error.RethrowRequiresCatch();
        }

        #region TryStatement

        private void CheckTry() {
            // Try inside a filter is not verifiable
            for (LabelBlockInfo j = _labelBlock; j != null; j = j.Parent) {
                if (j.Kind == LabelBlockKind.Filter) {
                    throw Error.TryNotAllowedInFilter();
                }
            }
        }

        private void EmitSaveExceptionOrPop(CatchBlock cb) {
            if (cb.Variable != null) {
                // If the variable is present, store the exception
                // in the variable.
                _scope.EmitSet(cb.Variable);
            } else {
                // Otherwise, pop it off the stack.
                _ilg.Emit(OpCodes.Pop);
            }
        }

        private void EmitTryExpression(Expression expr) {
            var node = (TryExpression)expr;

            CheckTry();

            //******************************************************************
            // 1. ENTERING TRY
            //******************************************************************

            PushLabelBlock(LabelBlockKind.Try);
            _ilg.BeginExceptionBlock();

            //******************************************************************
            // 2. Emit the try statement body
            //******************************************************************

            EmitExpression(node.Body);

            Type tryType = expr.Type;
            LocalBuilder value = null;
            if (tryType != typeof(void)) {
                //store the value of the try body
                value = GetLocal(tryType);
                _ilg.Emit(OpCodes.Stloc, value);
            }
            //******************************************************************
            // 3. Emit the catch blocks
            //******************************************************************

            foreach (CatchBlock cb in node.Handlers) {
                PushLabelBlock(LabelBlockKind.Catch);

                // Begin the strongly typed exception block
                EmitCatchStart(cb);

                //
                // Emit the catch block body
                //
                EmitExpression(cb.Body);
                if (tryType != typeof(void)) {
                    //store the value of the catch block body
                    _ilg.Emit(OpCodes.Stloc, value);
                }

                PopLabelBlock(LabelBlockKind.Catch);
            }

            //******************************************************************
            // 4. Emit the finally block
            //******************************************************************

            if (node.Finally != null || node.Fault != null) {
                PushLabelBlock(LabelBlockKind.Finally);

                if (node.Finally != null) {
                    _ilg.BeginFinallyBlock();
                } else {
                    _ilg.BeginFaultBlock();
                }

                // Emit the body
                EmitExpressionAsVoid(node.Finally ?? node.Fault);

                _ilg.EndExceptionBlock();
                PopLabelBlock(LabelBlockKind.Finally);
            } else {
                _ilg.EndExceptionBlock();
            }

            if (tryType != typeof(void)) {
                _ilg.Emit(OpCodes.Ldloc, value);
                FreeLocal(value);
            }
            PopLabelBlock(LabelBlockKind.Try);
        }

        /// <summary>
        /// Emits the start of a catch block.  The exception value that is provided by the
        /// CLR is stored in the variable specified by the catch block or popped if no
        /// variable is provided.
        /// </summary>
        private void EmitCatchStart(CatchBlock cb) {
            if (cb.Filter == null) {
                _ilg.BeginCatchBlock(cb.Test);
                EmitSaveExceptionOrPop(cb);
                return;
            }

            // emit filter block. Filter blocks are untyped so we need to do
            // the type check ourselves.  
            _ilg.BeginExceptFilterBlock();

            Label endFilter = _ilg.DefineLabel();
            Label rightType = _ilg.DefineLabel();

            // skip if it's not our exception type, but save
            // the exception if it is so it's available to the
            // filter
            _ilg.Emit(OpCodes.Isinst, cb.Test);
            _ilg.Emit(OpCodes.Dup);
            _ilg.Emit(OpCodes.Brtrue, rightType);
            _ilg.Emit(OpCodes.Pop);
            _ilg.Emit(OpCodes.Ldc_I4_0);
            _ilg.Emit(OpCodes.Br, endFilter);

            // it's our type, save it and emit the filter.
            _ilg.MarkLabel(rightType);
            EmitSaveExceptionOrPop(cb);
            PushLabelBlock(LabelBlockKind.Filter);
            EmitExpression(cb.Filter);
            PopLabelBlock(LabelBlockKind.Filter);

            // begin the catch, clear the exception, we've 
            // already saved it
            _ilg.MarkLabel(endFilter);
            _ilg.BeginCatchBlock(null);
            _ilg.Emit(OpCodes.Pop);
        }

        #endregion
    }
}
