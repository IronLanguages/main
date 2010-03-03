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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Scripting.Interpreter {
    /// <summary>
    /// A simple forth-style stack machine for executing Expression trees
    /// without the need to compile to IL and then invoke the JIT.  This trades
    /// off much faster compilation time for a slower execution performance.
    /// For code that is only run a small number of times this can be a 
    /// sweet spot.
    /// 
    /// The core loop in the interpreter is the RunInstructions method.
    /// </summary>
    internal sealed class Interpreter {
        internal static readonly object NoValue = new object();
        internal const int RethrowOnReturn = Int32.MaxValue;

        // zero: sync compilation
        // negative: default
        internal readonly int _compilationThreshold;

        private readonly LocalVariables _locals;
        internal readonly int[] _boxedLocals;
        private readonly Dictionary<LabelTarget, BranchLabel> _labelMapping;

        private readonly InstructionArray _instructions;
        internal readonly object[] _objects;
        internal readonly RuntimeLabel[] _labels;

        internal readonly LambdaExpression _lambda;
        private readonly ExceptionHandler[] _handlers;
        internal readonly DebugInfo[] _debugInfos;

        internal Interpreter(LambdaExpression lambda, LocalVariables locals, Dictionary<LabelTarget, BranchLabel> labelMapping,
            InstructionArray instructions, ExceptionHandler[] handlers, DebugInfo[] debugInfos, int compilationThreshold) {

            _lambda = lambda;
            _locals = locals;
            _boxedLocals = locals.GetBoxed();
                
            _instructions = instructions;
            _objects = instructions.Objects;
            _labels = instructions.Labels;
            _labelMapping = labelMapping;

            _handlers = handlers;
            _debugInfos = debugInfos;
            _compilationThreshold = compilationThreshold;
        }

        internal bool CompileSynchronously {
            get { return _compilationThreshold <= 1; }
        }

        internal InstructionArray Instructions {
            get { return _instructions; }
        }

        internal LocalVariables Locals {
            get { return _locals; } 
        }

        internal Dictionary<LabelTarget, BranchLabel> LabelMapping {
            get { return _labelMapping; }
        }

        /// <summary>
        /// Runs instructions within the given frame.
        /// </summary>
        /// <remarks>
        /// Interpreted stack frames are linked via Parent reference so that each CLR frame of this method corresponds 
        /// to an interpreted stack frame in the chain. It is therefore possible to combine CLR stack traces with 
        /// interpreted stack traces by aligning interpreted frames to the frames of this method.
        /// Each group of subsequent frames of Run method corresponds to a single interpreted frame.
        /// </remarks>
        [SpecialName, MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(InterpretedFrame frame) {
            while (true) {
                try {
                    var instructions = _instructions.Instructions;
                    int index = frame.InstructionIndex;
                    while (index < instructions.Length) {
                        index += instructions[index].Run(frame);
                        frame.InstructionIndex = index;
                    }
                    return;
                } catch (Exception exception) {
                    frame.SaveTraceToException(exception);
                    frame.FaultingInstruction = frame.InstructionIndex;
                    ExceptionHandler handler;
                    frame.InstructionIndex += GotoHandler(frame, exception, out handler);

                    if (handler == null || handler.IsFault) {
                        // run finally/fault blocks:
                        Run(frame);

                        // a finally block can throw an exception caught by a handler, which cancels the previous exception:
                        if (frame.InstructionIndex == RethrowOnReturn) {
                            throw;
                        }
                        return;
                    }

                    // stay in the current catch so that ThreadAbortException is not rethrown by CLR:
                    var abort = exception as ThreadAbortException;
                    if (abort != null) {
                        _anyAbortException = abort;
                        frame.CurrentAbortHandler = handler;
                        Run(frame);
                        return;
                    }
                    exception = null;
                }
            }
        }

        // To get to the current AbortReason object on Thread.CurrentThread 
        // we need to use ExceptionState property of any ThreadAbortException instance.
        private static ThreadAbortException _anyAbortException;

        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetLabelIndex) {
            var abortHandler = frame.CurrentAbortHandler;
            if (abortHandler != null && !abortHandler.IsInside(frame.Interpreter._labels[targetLabelIndex].Index)) {
                frame.CurrentAbortHandler = null;

                var currentThread = Thread.CurrentThread;
                if ((currentThread.ThreadState & System.Threading.ThreadState.AbortRequested) != 0) {
                    Debug.Assert(_anyAbortException != null);
                    // The current abort reason needs to be preserved.
#if SILVERLIGHT
                    currentThread.Abort();
#else
                    currentThread.Abort(_anyAbortException.ExceptionState);
#endif
                }
            }
        }

        internal ExceptionHandler GetBestHandler(int instructionIndex, Type exceptionType) {
            ExceptionHandler best = null;
            foreach (var handler in _handlers) {
                if (handler.Matches(exceptionType, instructionIndex)) {
                    if (handler.IsBetterThan(best)) {
                        best = handler;
                    }
                }
            }
            return best;
        }

        private int ReturnAndRethrowLabelIndex {
            get {
                // the last label is "return and rethrow" label:
                Debug.Assert(_labels[_labels.Length - 1].Index == RethrowOnReturn);
                return _labels.Length - 1;
            }
        }

        internal int GotoHandler(InterpretedFrame frame, object exception, out ExceptionHandler handler) {
            handler = GetBestHandler(frame.InstructionIndex, exception.GetType());
            if (handler == null) {
                return frame.Goto(ReturnAndRethrowLabelIndex, Interpreter.NoValue);
            } else {
                return frame.Goto(handler.LabelIndex, exception);
            }
        }
    }
}
