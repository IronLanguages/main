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
        internal readonly int _compilationThreshold;
        internal readonly int _numberOfLocals;
        internal readonly int _maxStackDepth;
        internal readonly bool[] _localIsBoxed;

        private readonly InstructionArray _instructions;
        internal readonly object[] _objects;

        internal readonly LambdaExpression _lambda;
        private readonly ExceptionHandler[] _handlers;
        internal readonly DebugInfo[] _debugInfos;
        private readonly bool _onlyFaultHandlers;

        internal Interpreter(LambdaExpression lambda, bool[] localIsBoxed, int maxStackDepth,
            InstructionArray instructions, ExceptionHandler[] handlers, DebugInfo[] debugInfos, int compilationThreshold) {

            _lambda = lambda;
            _numberOfLocals = localIsBoxed.Length;
            if (Array.IndexOf(localIsBoxed, true) != -1) {
                _localIsBoxed = localIsBoxed;
            } else {
                _localIsBoxed = null;
            }
                
            _maxStackDepth = maxStackDepth;
            _instructions = instructions;
            _objects = instructions.Objects;
            _handlers = handlers;
            _debugInfos = debugInfos;

            _onlyFaultHandlers = true;
            foreach (var handler in handlers) {
                if (!handler.IsFinallyOrFault) {
                    _onlyFaultHandlers = false;
                    break;
                }
            }

            _compilationThreshold = compilationThreshold;
        }

        public void Run(InterpretedFrame frame) {
            if (_onlyFaultHandlers) {
                bool fault = true;
                try {
                    RunInstructions(frame);
                    fault = false;
                    return;
                } finally {
                    if (fault) {
                        frame.FaultingInstruction = frame.InstructionIndex;
                        HandleFault(frame);
                    }
                }
            } else {
                while (true) {
                    try {
                        RunInstructions(frame);
                        return;
                    } catch (Exception exc) {
                        frame.FaultingInstruction = frame.InstructionIndex;
                        ExceptionHandler handler = HandleCatch(frame, exc);

                        if (handler == null) {
                            throw;
                        }

                        // stay in the current catch so that ThreadAbortException is not rethrown by CLR:
                        var abort = exc as ThreadAbortException;
                        if (abort != null) {
                            _anyAbortException = abort;
                            frame.CurrentAbortHandler = handler;
                            Run(frame);
                            return;
                        } 
                    }
                }
            }
        }

        internal void RunBlock(InterpretedFrame frame, int endIndex) {
            while (true) {
                try {
                    RunInstructions(frame, endIndex);
                    return;
                } catch (Exception exc) {
                    endIndex = _instructions.Length;
                    frame.FaultingInstruction = frame.InstructionIndex;

                    ExceptionHandler handler = HandleCatch(frame, exc);
                    if (handler == null) {
                        throw;
                    }

                    // stay in the current catch so that ThreadAbortException is not rethrown by CLR:
                    var abort = exc as ThreadAbortException;
                    if (abort != null) {
                        _anyAbortException = abort;
                        frame.CurrentAbortHandler = handler;
                        RunBlock(frame, endIndex);
                        return;
                    } 
                }
            }
        }

        // To get to the current AbortReason object on Thread.CurrentThread 
        // we need to use ExceptionState property of any ThreadAbortException instance.
        private static ThreadAbortException _anyAbortException;

        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetOffset) {
            var abortHandler = frame.CurrentAbortHandler;
            if (abortHandler != null && !abortHandler.IsInside(frame.InstructionIndex + targetOffset)) {
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

        private ExceptionHandler GetBestHandler(InterpretedFrame frame, Type exceptionType) {
            ExceptionHandler best = null;
            foreach (var handler in _handlers) {
                if (handler.Matches(exceptionType, frame.InstructionIndex)) {
                    if (handler.IsBetterThan(best)) {
                        best = handler;
                    }
                }
            }
            return best;
        }

        private ExceptionHandler HandleCatch(InterpretedFrame frame, Exception exception) {
            Type exceptionType = exception.GetType();
            var handler = GetBestHandler(frame, exceptionType);
            if (handler == null) {
                return null;
            }

            frame.StackIndex = _numberOfLocals + handler.HandlerStackDepth;
            if (handler.IsFinallyOrFault) {
                frame.InstructionIndex = handler.StartHandlerIndex;

                RunBlock(frame, handler.EndHandlerIndex);
                if (frame.InstructionIndex == handler.EndHandlerIndex) {
                    frame.InstructionIndex -= 1; // push back into the right range

                    return HandleCatch(frame, exception);
                } else {
                    return handler;
                }
            } else {
                if (handler.PushException) {
                    frame.Push(exception);
                }

                frame.InstructionIndex = handler.StartHandlerIndex;
                return handler;
            }
        }

        private void HandleFault(InterpretedFrame frame) {
            var handler = GetBestHandler(frame, null);
            if (handler == null) return;
            frame.StackIndex = _numberOfLocals + handler.HandlerStackDepth;
            bool wasFault = true;
            try {
                frame.InstructionIndex = handler.StartHandlerIndex;
                RunInstructions(frame, handler.EndHandlerIndex);
                wasFault = false;
            } finally {
                if (wasFault) {
                    frame.FaultingInstruction = frame.InstructionIndex;
                }

                // This assumes that finally faults are propogated always
                //
                // Go back one so we are scoped correctly
                frame.InstructionIndex = handler.EndHandlerIndex - 1;
                HandleFault(frame);
            }
        }

        internal void RunInstructions(InterpretedFrame frame, int endInstruction) {
            var instructions = _instructions.Instructions;
            int index = frame.InstructionIndex;
            while (index < endInstruction) {
                index += instructions[index].Run(frame);
                frame.InstructionIndex = index;
            }
        }

        private void RunInstructions(InterpretedFrame frame) {
            var instructions = _instructions.Instructions;
            int index = frame.InstructionIndex;
            while (index < instructions.Length) {
                index += instructions[index].Run(frame);
                frame.InstructionIndex = index;
            }
        }
    }
}
