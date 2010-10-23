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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime {

    public static partial class RubyOps {
        #region RFC Flags

        [Emitted]
        public static RuntimeFlowControl/*!*/ CreateRfcForMethod(Proc proc) {
            var result = new RuntimeFlowControl();
            result._activeFlowControlScope = result;
            result.InitializeRfc(proc);
            return result;
        }

        [Emitted]
        public static void EnterLoop(RubyScope/*!*/ scope) {
            scope.InLoop = true;
        }

        [Emitted]
        public static void LeaveLoop(RubyScope/*!*/ scope) {
            scope.InLoop = false;
        }

        [Emitted]
        public static void EnterRescue(RubyScope/*!*/ scope) {
            scope.InRescue = true;
        }

        [Emitted]
        public static void LeaveRescue(RubyScope/*!*/ scope) {
            scope.InRescue = false;
        }

        #endregion

        #region Retry Helpers

        /// <summary>
        /// Implements retry statement in a block.
        /// </summary>
        [Emitted]
        public static object BlockRetry(BlockParam/*!*/ blockFlowControl) {
            if (blockFlowControl.CallerKind == BlockCallerKind.Yield) {
                blockFlowControl.SetFlowControl(BlockReturnReason.Retry, null, blockFlowControl.Proc.Kind);
                return BlockReturnResult.Retry;
            } else {
                throw new LocalJumpError("retry from proc-closure");
            }
        }

        /// <summary>
        /// Implements retry statement in a method.
        /// </summary>
        [Emitted]
        public static object MethodRetry(RubyScope/*!*/ scope, Proc proc) {
            if (proc != null) {
                return BlockReturnResult.Retry;
            } else {
                // TODO: can this happen? 
                // If proc was null then the block argument passed to the call-with-block that returned RetrySingleton would be null and thus 
                // the call cannot yield to any block that retries.
                throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
            }
        }

        /// <summary>
        /// Implements retry statement in eval'd code.
        /// </summary>
        [Emitted]
        public static void EvalRetry(RubyScope/*!*/ scope) {
            if (scope.InRescue) {
                throw new EvalUnwinder(BlockReturnReason.Retry, BlockReturnResult.Retry);
            }

            RubyBlockScope blockScope;
            RubyMethodScope methodScope;
            scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);

            if (methodScope != null && methodScope.BlockParameter != null) {
                throw new EvalUnwinder(BlockReturnReason.Retry, BlockReturnResult.Retry);
            } else if (blockScope != null) {
                if (blockScope.BlockFlowControl.CallerKind == BlockCallerKind.Yield) {
                    throw new EvalUnwinder(BlockReturnReason.Retry, null, blockScope.BlockFlowControl.Proc.Kind, BlockReturnResult.Retry);
                }
                //if (blockScope.BlockFlowControl.IsMethod) {
                throw new LocalJumpError("retry from proc-closure");// TODO: RFC
            }

            throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
        }

        #endregion

        #region Break Helpers

        /// <summary>
        /// Implements break statement in a block.
        /// </summary>
        [Emitted]
        public static object BlockBreak(BlockParam/*!*/ blockFlowControl, object returnValue) {
            return blockFlowControl.Break(returnValue);
        }

        /// <summary>
        /// Implements break statement in a method.
        /// </summary>
        [Emitted]
        public static void MethodBreak(object returnValue) {
            throw new LocalJumpError("unexpected break");
        }

        /// <summary>
        /// Implements break statement in eval'd code.
        /// </summary>
        [Emitted]
        public static void EvalBreak(RubyScope/*!*/ scope, object returnValue) {
            if (scope.InLoop) {
                throw new EvalUnwinder(BlockReturnReason.Break, returnValue);
            }
            
            RubyBlockScope blockScope;
            RubyMethodScope methodScope;
            scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
            if (blockScope != null) {
                var proc = blockScope.BlockFlowControl.Proc;
                throw new EvalUnwinder(BlockReturnReason.Break, proc.Converter, proc.Kind, returnValue);
            } else {
                throw new LocalJumpError("unexpected break");
            }
        }

        #endregion

        #region Next, Redo Helpers

        [Emitted]
        public static void MethodNext(RubyScope/*!*/ scope, object returnValue) {
            throw new LocalJumpError("unexpected next", scope.FlowControlScope);
        }

        [Emitted]
        public static void MethodRedo(RubyScope/*!*/ scope) {
            throw new LocalJumpError("unexpected redo", scope.FlowControlScope);
        }

        [Emitted]
        public static void EvalNext(RubyScope/*!*/ scope, object returnValue) {
            EvalNextOrRedo(scope, returnValue, false);
        }

        [Emitted]
        public static void EvalRedo(RubyScope/*!*/ scope) {
            EvalNextOrRedo(scope, null, true);
        }

        private static void EvalNextOrRedo(RubyScope/*!*/ scope, object returnValue, bool isRedo) {
            if (scope.InLoop) {
                throw new BlockUnwinder(returnValue, isRedo); 
            }

            RubyBlockScope blockScope;
            RubyMethodScope methodScope;
            scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
            if (blockScope != null) {
                throw new BlockUnwinder(returnValue, isRedo); 
            }

            throw new LocalJumpError(String.Format("unexpected {0}", isRedo ? "redo" : "next"));
        }

        #endregion

        #region Return Helpers

        /// <summary>
        /// Implements return statement in a block.
        /// </summary>
        [Emitted]
        public static object BlockReturn(BlockParam/*!*/ blockFlowControl, object returnValue) {
            Proc proc = blockFlowControl.Proc;
            if (blockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda) {
                return returnValue;
            }

            RuntimeFlowControl owner = proc.LocalScope.FlowControlScope;
            if (owner.IsActiveMethod) {
                blockFlowControl.ReturnReason = BlockReturnReason.Return;
                return new BlockReturnResult(owner, returnValue);
            }
            
            throw new LocalJumpError("unexpected return");
        }

        /// <summary>
        /// Implements return statement in eval'd code.
        /// </summary>
        [Emitted]
        public static object EvalReturn(RubyScope/*!*/ scope, object returnValue) {
            RubyBlockScope blockScope;
            RubyMethodScope methodScope;
            scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);
            
            if (blockScope != null) {
                Proc proc = blockScope.BlockFlowControl.Proc;

                if (blockScope.BlockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda) {
                    throw new BlockUnwinder(returnValue, false);
                }

                RuntimeFlowControl owner = proc.LocalScope.FlowControlScope;
                if (owner.IsActiveMethod) {
                    throw new MethodUnwinder(owner, returnValue);
                }

                throw new LocalJumpError("unexpected return");
            } else {
                // return from the current method:
                throw new MethodUnwinder(scope.FlowControlScope, returnValue);
            }
        }

        // post-yield return ops:

        private static void YieldBlockReturn(BlockParam/*!*/ blockFlowControl, object returnValue) {
            if (blockFlowControl.CallerKind == BlockCallerKind.Yield) {
                blockFlowControl.SetFlowControl(BlockReturnReason.Return, null, blockFlowControl.Proc.Kind);
            } else {
                // if the block is called we can't continue fast stack unwinding:
                throw ((BlockReturnResult)returnValue).ToUnwinder();
            }
        }

        #endregion

        #region Yield Helpers

        /// <summary>
        /// Implements post-yield dispatch for yield in a block.
        /// </summary>
        /// <returns>True if the block should terminate returning the result of yield.</returns>
        [Emitted]
        public static bool BlockYield(RubyScope/*!*/ scope, BlockParam/*!*/ ownerBlockFlowControl, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(scope, ownerBlockFlowControl, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    BlockRetry(ownerBlockFlowControl);
                    return true;

                case BlockReturnReason.Return:
                    // the result that the caller returns should already be MethodUnwinder:
                    YieldBlockReturn(ownerBlockFlowControl, returnValue);
                    return true;

                case BlockReturnReason.Break:
                    YieldBlockBreak(scope, ownerBlockFlowControl, yieldedBlockFlowControl, returnValue);
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Implements post-yield dispatch for yield in a method.
        /// </summary>
        /// <returns>True if the method should terminate returning the result of yield.</returns>
        [Emitted]
        public static bool MethodYield(RubyScope/*!*/ scope, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            return MethodYieldRfc(scope.FlowControlScope, yieldedBlockFlowControl, returnValue);
        }

        public static bool MethodYieldRfc(RuntimeFlowControl rfc, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                case BlockReturnReason.Return:
                    // The result that the caller returns already is RetrySingleton/MethodUnwinder,
                    // the call-site is with a block (to which we are yielding) and will handle retry/return.
                    return true;

                case BlockReturnReason.Break:
                    YieldMethodBreak(rfc, yieldedBlockFlowControl, returnValue);
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Implements post-yield dispatch for yield in eval'd code.
        /// </summary>
        /// <returns>True if the current frame should terminate returning the result of yield.</returns>
        [Emitted]
        public static bool EvalYield(RubyScope/*!*/ scope, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(scope, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns is already RetrySingleton:
                    Debug.Assert(returnValue == BlockReturnResult.Retry);
                    EvalRetry(scope);
                    throw Assert.Unreachable;

                case BlockReturnReason.Return:
                    // The result that the caller returns is already MethodUnwinder.
                    // We can't continue fast unwind since the eval call-site doesn't propagate it (a call w/o block).
                    throw ((BlockReturnResult)returnValue).ToUnwinder();

                case BlockReturnReason.Break:
                    YieldEvalBreak(yieldedBlockFlowControl, returnValue);
                    throw Assert.Unreachable;
            }
            return false;
        }

        // post-yield break ops:

        private static void YieldMethodBreak(RuntimeFlowControl rfc, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(yieldedBlockFlowControl);

            // target proc-converter:
            RuntimeFlowControl targetFrame = yieldedBlockFlowControl.TargetFrame;
            Debug.Assert(targetFrame != null);

            if (targetFrame.IsActiveMethod) {
                // optimize break to the current frame:
                if (targetFrame == rfc) {
                    return;
                } else {
                    throw new MethodUnwinder(targetFrame, returnValue);
                }
            } else {
                throw new LocalJumpError("break from proc-closure");
            }
        }

        private static void YieldBlockBreak(RubyScope/*!*/ scope, BlockParam/*!*/ ownerBlockFlowControl, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(ownerBlockFlowControl, yieldedBlockFlowControl);

            // target proc-converter:
            RuntimeFlowControl targetFrame = yieldedBlockFlowControl.TargetFrame;
            Debug.Assert(targetFrame != null);

            if (targetFrame.IsActiveMethod) {
                // TODO: this optimization doesn't work (see test RubyBlocks14B)
                //if (targetFrame == scope.FlowControlScope) {
                //    // The current primary super-frame is the proc-converter, however we are still in the block frame that needs to be unwound.
                //    // Sets the owner's BFC to exit the current block (recursively up to the primary frame).
                //    ownerBlockFlowControl.SetFlowControl(BlockReturnReason.Break, targetFrame, yieldedBlockFlowControl.SourceProcKind);
                //    return;
                //} else {
                    throw new MethodUnwinder(targetFrame, returnValue);
                //}
            } else {
                throw new LocalJumpError("break from proc-closure");
            }
        }

        private static void YieldEvalBreak(BlockParam/*!*/ blockFlowControl, object returnValue) {
            if (blockFlowControl.TargetFrame.IsActiveMethod) {
                // do not "optimize" for current RFC, we need to unwind stack anyway
                throw new MethodUnwinder(blockFlowControl.TargetFrame, returnValue);
            } else {
                throw new LocalJumpError("break from proc-closure");
            }
        }

        #endregion

        #region Call Helpers

        /// <summary>
        /// Post-proc-call: Handles break and return from the called block.
        /// </summary>
        [Emitted]
        public static object MethodProcCall(BlockParam/*!*/ blockFlowControl, object returnValue) {
            switch (blockFlowControl.ReturnReason) {
                case BlockReturnReason.Break:
                    // breaking thru call - the kind of the break originator is checked:
                    if (blockFlowControl.SourceProcKind != ProcKind.Lambda) { 
                        YieldMethodBreak(null, blockFlowControl, returnValue);
                        Debug.Assert(false, "YieldBreak should throw");
                    }
                    return returnValue;

                case BlockReturnReason.Return:
                    throw ((BlockReturnResult)returnValue).ToUnwinder();

                case BlockReturnReason.Retry:
                    // Cannot retry a block invoked via call
                    throw Assert.Unreachable;
            }

            return returnValue;
        }

        #endregion

        #region Method Call with Block Helpers

        [Emitted]
        public static bool IsRetrySingleton(object value) {
            return value == BlockReturnResult.Retry;
        }

        [Emitted]
        public static object PropagateRetrySingleton(object other, object possibleRetrySingleton) {
            return IsRetrySingleton(possibleRetrySingleton) ? possibleRetrySingleton : other;
        }

        /// <summary>
        /// Post-call-with-block in a method: propagates MethodUnwinder.
        /// </summary>
        [Emitted]
        public static object MethodPropagateReturn(RubyScope/*!*/ scope, Proc block, BlockReturnResult/*!*/ unwinder) {
            if (unwinder.TargetFrame == scope) {
                return unwinder.ReturnValue;
            } else if (block != null) {
                return unwinder;
            } else {
                throw unwinder.ToUnwinder();
            }
        }

        /// <summary>
        /// Post-call-with-block in a block: propagates MethodUnwinder.
        /// </summary>
        [Emitted]
        public static object BlockPropagateReturn(BlockParam/*!*/ blockFlowControl, object returnValue) {
            blockFlowControl.ReturnReason = BlockReturnReason.Return;
            return returnValue;
        }

        /// <summary>
        /// Post-call-with-block in a eval'd code: propagates MethodUnwinder.
        /// </summary>
        [Emitted]
        public static object EvalPropagateReturn(object returnValue) {
            throw ((BlockReturnResult)returnValue).ToUnwinder();
        }

        #endregion
    }
}
