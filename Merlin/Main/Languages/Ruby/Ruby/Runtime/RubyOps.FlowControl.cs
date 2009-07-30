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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;

namespace IronRuby.Runtime {

    public static partial class RubyOps {
        private static readonly object/*!*/ RetrySingleton = new object();
        
        #region Unwinders, RFC Flags

        [Emitted]
        public static RuntimeFlowControl/*!*/ CreateRfcForMethod(Proc proc) {
            var result = new RuntimeFlowControl();
            result._activeFlowControlScope = result;
            result.InitializeRfc(proc);
            return result;
        }

        // Ruby method exit filter:
        [Emitted]
        public static bool IsMethodUnwinderTargetFrame(RubyScope/*!*/ scope, Exception/*!*/ exception) {
            var unwinder = exception as MethodUnwinder;
            if (unwinder == null) {
                RubyExceptionData.GetInstance(exception).CaptureExceptionTrace(scope);
                return false;
            } else {
                return unwinder.TargetFrame == scope.FlowControlScope;
            }
        }

        [Emitted]
        public static object GetMethodUnwinderReturnValue(Exception/*!*/ exception) {
            return ((MethodUnwinder)exception).ReturnValue;
        }

        [Emitted]
        public static void LeaveMethodFrame(RuntimeFlowControl/*!*/ rfc) {
            rfc.LeaveMethod();
        }
        
        [Emitted]
        public static void LeaveBlockFrame(RubyBlockScope/*!*/ scope) {
            
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

        [Emitted]
        public static object BlockRetry(BlockParam/*!*/ blockFlowControl) {
            if (blockFlowControl.CallerKind == BlockCallerKind.Yield) {
                blockFlowControl.SetFlowControl(BlockReturnReason.Retry, null, blockFlowControl.Proc.Kind);
                return RetrySingleton;
            } else {
                throw new LocalJumpError("retry from proc-closure");
            }
        }
        
        [Emitted]
        public static object MethodRetry(RubyScope/*!*/ scope, Proc proc) {
            if (proc != null) {
                return RetrySingleton;
            } else {
                throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
            }
        }

        /// <param name="blockFlowControl">Optional: if called from block</param>
        /// <param name="proc">Optional: value of the proc parameter of the enclosing method.</param>
        [Emitted]
        public static void EvalRetry(RubyScope/*!*/ scope) {
            if (scope.InRescue) {
                throw new EvalUnwinder(BlockReturnReason.Retry, RetrySingleton);
            }

            RubyBlockScope blockScope;
            RubyMethodScope methodScope;
            scope.GetInnerMostBlockOrMethodScope(out blockScope, out methodScope);

            if (methodScope != null && methodScope.BlockParameter != null) {
                throw new EvalUnwinder(BlockReturnReason.Retry, RetrySingleton);
            } else if (blockScope != null) {
                if (blockScope.BlockFlowControl.CallerKind == BlockCallerKind.Yield) {
                    throw new EvalUnwinder(BlockReturnReason.Retry, null, blockScope.BlockFlowControl.Proc.Kind, RetrySingleton);
                }
                //if (blockScope.BlockFlowControl.IsMethod) {
                throw new LocalJumpError("retry from proc-closure");// TODO: RFC
            }

            throw new LocalJumpError("retry used out of rescue", scope.FlowControlScope);
        }

        #endregion

        #region Break Helpers

        [Emitted]
        public static object BlockBreak(BlockParam/*!*/ blockFlowControl, object returnValue) {
            return blockFlowControl.Break(returnValue);
        }

        [Emitted]
        public static void MethodBreak(object returnValue) {
            throw new LocalJumpError("unexpected break");
        }
        
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

        [Emitted]
        public static object BlockReturn(BlockParam/*!*/ blockFlowControl, object returnValue) {
            Assert.NotNull(blockFlowControl);
            Assert.NotNull(blockFlowControl.Proc);

            Proc proc = blockFlowControl.Proc;
            if (blockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda) {
                return returnValue;
            }

            RuntimeFlowControl owner = proc.LocalScope.FlowControlScope;
            if (owner.IsActiveMethod) {
                throw new MethodUnwinder(owner, returnValue);
            } 
            
            throw new LocalJumpError("unexpected return");
        }

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

        #endregion

        #region Yield Helpers

        [Emitted]
        public static bool BlockYield(RubyScope/*!*/ scope, BlockParam/*!*/ ownerBlockFlowControl, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(scope, ownerBlockFlowControl, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    BlockRetry(ownerBlockFlowControl);
                    return true;

                case BlockReturnReason.Break:
                    YieldBlockBreak(scope, ownerBlockFlowControl, yieldedBlockFlowControl, returnValue);
                    return true;
            }
            return false;
        }

        [Emitted]
        public static bool MethodYield(RubyScope/*!*/ scope, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            return MethodYieldRfc(scope.FlowControlScope, yieldedBlockFlowControl, returnValue);
        }

        public static bool MethodYieldRfc(RuntimeFlowControl rfc, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    return true;

                case BlockReturnReason.Break:
                    YieldMethodBreak(rfc, yieldedBlockFlowControl, returnValue);
                    return true;
            }
            return false;
        }

        [Emitted]
        public static bool EvalYield(RubyScope/*!*/ scope, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(scope, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    EvalRetry(scope);
                    return true;

                case BlockReturnReason.Break:
                    YieldEvalBreak(yieldedBlockFlowControl, returnValue);
                    return true;
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
                if (targetFrame == scope.FlowControlScope) {
                    // The current primary super-frame is the proc-converter, however we are still in the block frame that needs to be unwound.
                    // Sets the owner's BFC to exit the current block (recursively up to the primary frame).
                    ownerBlockFlowControl.SetFlowControl(BlockReturnReason.Break, targetFrame, yieldedBlockFlowControl.SourceProcKind);
                    return;
                } else {
                    throw new MethodUnwinder(targetFrame, returnValue);
                }
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
        /// Proc#call helper emitted in the rule. Handles break from the called block.
        /// </summary>
        [Emitted]
        public static bool MethodProcCall(BlockParam/*!*/ blockFlowControl, object returnValue) {
            Assert.NotNull(blockFlowControl);

            Debug.Assert(blockFlowControl.ReturnReason != BlockReturnReason.Retry, "Cannot retry a block invoked via call");

            if (blockFlowControl.ReturnReason == BlockReturnReason.Break) {
                // breaking thru call - a kind of the break originator is checked:
                if (blockFlowControl.SourceProcKind != ProcKind.Lambda) { 
                    YieldMethodBreak(null, blockFlowControl, returnValue);
                    Debug.Assert(false, "YieldBreak should throw");
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Method Call with Block Helpers

        [Emitted]
        public static bool IsRetrySingleton(object value) {
            return value == RetrySingleton;
        }

        [Emitted]
        public static object PropagateRetrySingleton(object other, object possibleRetrySingleton) {
            return IsRetrySingleton(possibleRetrySingleton) ? possibleRetrySingleton : other;
        }

        [Emitted]
        public static object GetRetrySingleton() {
            return RetrySingleton;
        }

        #endregion
    }
}
