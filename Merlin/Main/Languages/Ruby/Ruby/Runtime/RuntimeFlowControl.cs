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

    public sealed class RuntimeFlowControl {
        [Emitted]
        public bool InLoop;
        [Emitted]
        public bool InRescue;
        [Emitted]
        public bool InBlock;
        [Emitted]
        public bool IsActiveMethod;

        public RuntimeFlowControl() {
        }

        internal static FieldInfo/*!*/ InRescueField { get { return typeof(RuntimeFlowControl).GetField("InRescue"); } }
        internal static FieldInfo/*!*/ InLoopField { get { return typeof(RuntimeFlowControl).GetField("InLoop"); } }
        internal static FieldInfo/*!*/ IsActiveMethodField { get { return typeof(RuntimeFlowControl).GetField("IsActiveMethod"); } }
        internal static FieldInfo/*!*/ InBlockField { get { return typeof(RuntimeFlowControl).GetField("InBlock"); } }
    }

    public static partial class RubyOps {
        private static readonly object/*!*/ RetrySingleton = new object();
        
        #region User Method Prologue Helpers

        [Emitted]
        public static RuntimeFlowControl/*!*/ CreateRfcForMethod(Proc proc) {
            RuntimeFlowControl result = new RuntimeFlowControl();
            result.IsActiveMethod = true;

            if (proc != null && proc.Kind == ProcKind.Block) {
                proc.Kind = ProcKind.Proc;
                proc.Converter = result;
            }

            return result;
        }

        #endregion

        #region Retry Helpers

        [Emitted]
        public static object BlockRetry(BlockParam/*!*/ blockFlowControl) {
            if (blockFlowControl.CallerKind == BlockCallerKind.Yield) {
                blockFlowControl.SetFlowControl(BlockReturnReason.Retry, null, blockFlowControl.Proc.Kind);
                return RetrySingleton;
            } else {
                throw new LocalJumpError("retry from proc-clause");
            }
        }
        
        [Emitted]
        public static object MethodRetry(RuntimeFlowControl/*!*/ rfc, Proc proc) {
            if (proc != null) {
                return RetrySingleton;
            } else {
                throw new LocalJumpError("retry used out of rescue", rfc);
            }
        }

        /// <param name="blockFlowControl">Optional: if called from block</param>
        /// <param name="proc">Optional: value of the proc parameter of the enclosing method.</param>
        [Emitted]
        public static void EvalRetry(RuntimeFlowControl/*!*/ rfc) {
            
            // TODO: get from scope:
            BlockParam blockFlowControl = null;
            Proc proc = null;

            if (rfc.InBlock && blockFlowControl.CallerKind != BlockCallerKind.Yield) {
                throw new LocalJumpError("retry from proc-clause");
            }

            if (rfc.InRescue || rfc.InBlock || proc != null) {
                throw new EvalUnwinder(BlockReturnReason.Retry, null, blockFlowControl.Proc.Kind, RetrySingleton); 
            } else {
	            throw new LocalJumpError("retry used out of rescue", rfc);
            }
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
        public static void EvalBreak(RuntimeFlowControl/*!*/ rfc, object returnValue) {
            // TODO: get from scope:
            BlockParam blockFlowControl = null;
            
            if (rfc.InLoop || rfc.InBlock) {
                throw new EvalUnwinder(BlockReturnReason.Break, blockFlowControl.Proc.Converter, blockFlowControl.Proc.Kind, returnValue);
            } else {
                throw new LocalJumpError("unexpected break");
            }
        }

        #endregion

        #region Next Helpers

        [Emitted]
        public static void MethodNext(RuntimeFlowControl/*!*/ rfc, object returnValue) {
            throw new LocalJumpError("unexpected next", rfc);
        }

        [Emitted]
        public static void EvalNext(RuntimeFlowControl/*!*/ rfc, object returnValue) {
            if (rfc.InLoop || rfc.InBlock) {
                throw new BlockUnwinder(returnValue, false); // next
            } else {
                throw new LocalJumpError("unexpected next");
            }
        }

        #endregion

        #region Redo Helpers

        [Emitted]
        public static void MethodRedo(RuntimeFlowControl/*!*/ rfc) {
            throw new LocalJumpError("unexpected redo", rfc);
        }

        [Emitted]
        public static void EvalRedo(RuntimeFlowControl/*!*/ rfc) {
            if (rfc.InLoop || rfc.InBlock) {
                throw new BlockUnwinder(null, true); // redo
            } else {
                throw new LocalJumpError("unexpected redo");
            }
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

            if (proc.Owner.IsActiveMethod) {
                throw new MethodUnwinder(proc.Owner, returnValue);
            } 
            
            throw new LocalJumpError("unexpected return");
        }

        [Emitted]
        public static object EvalReturn(RuntimeFlowControl/*!*/ rfc, object returnValue) {
            // TODO: get from scope:
            Proc proc = null;
            BlockParam blockFlowControl = null;

            if (rfc.InBlock) {
                if (blockFlowControl.CallerKind == BlockCallerKind.Call && proc.Kind == ProcKind.Lambda) {
                    throw new BlockUnwinder(returnValue, false);
                }

                if (proc.Owner.IsActiveMethod) {
                    throw new MethodUnwinder(proc.Owner, returnValue);
                }

                throw new LocalJumpError("unexpected return");
            } else {
                // return from the current method:
                throw new MethodUnwinder(rfc, returnValue);
            }
        }

        #endregion

        #region Yield Helpers

        [Emitted]
        public static bool BlockYield(RuntimeFlowControl/*!*/ rfc, BlockParam/*!*/ ownerBlockFlowControl, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(rfc, ownerBlockFlowControl, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    BlockRetry(ownerBlockFlowControl);
                    return true;

                case BlockReturnReason.Break:
                    YieldBlockBreak(rfc, ownerBlockFlowControl, yieldedBlockFlowControl, returnValue);
                    return true;
            }
            return false;
        }

        [Emitted]
        public static bool MethodYield(RuntimeFlowControl rfc, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
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
        public static bool EvalYield(RuntimeFlowControl/*!*/ rfc, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(rfc, yieldedBlockFlowControl);

            switch (yieldedBlockFlowControl.ReturnReason) {
                case BlockReturnReason.Retry:
                    // the result that the caller returns should already be RetrySingleton:
                    EvalRetry(rfc);
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

        private static void YieldBlockBreak(RuntimeFlowControl rfc, BlockParam/*!*/ ownerBlockFlowControl, BlockParam/*!*/ yieldedBlockFlowControl, object returnValue) {
            Assert.NotNull(ownerBlockFlowControl, yieldedBlockFlowControl);

            // target proc-converter:
            RuntimeFlowControl targetFrame = yieldedBlockFlowControl.TargetFrame;
            Debug.Assert(targetFrame != null);

            if (targetFrame.IsActiveMethod) {
                if (targetFrame == rfc) {
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

        #region EH Helpers

        [Emitted]
        public static bool CanRescue(RuntimeFlowControl/*!*/ rfc, Exception/*!*/ e) {
            if (e is StackUnwinder) {
                return false;
            }

            LocalJumpError lje = e as LocalJumpError;
            if (lje != null && ReferenceEquals(lje.SkipFrame, rfc)) {
                return false;
            }

            return true;
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
