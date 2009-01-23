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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {
    /**
     * A simple forth-style stack machine for executing Expression trees
     * without the need to compile to IL and then invoke the JIT.  This trades
     * off much faster compilation time for a slower execution performance.
     * For code that is only run a small number of times this can be a 
     * sweet spot.
     * 
     * The core loop in the interpreter is the RunInstructions method.
     */
    internal class Interpreter {
        private int _numberOfLocals, _maxStackDepth;
        private bool[] _localIsBoxed;
        private Instruction[] _instructions;
        private LambdaExpression _lambda;
        private ExceptionHandler[] _handlers;
        private DebugInfo[] _debugInfos;
        private bool _onlyFaultHandlers;

        private bool AnyBoxedLocals(bool[] localIsBoxed) {
            for (int i=0; i < localIsBoxed.Length; i++) {
                if (localIsBoxed[i]) return true;
            }
            return false;
        }

        internal Interpreter(LambdaExpression lambda, bool[] localIsBoxed, int maxStackDepth, 
                           Instruction[] instructions, 
                           ExceptionHandler[] handlers, DebugInfo[] debugInfos) {
            this._lambda = lambda;
            this._numberOfLocals = localIsBoxed.Length;
            if (AnyBoxedLocals(localIsBoxed)) {
                _localIsBoxed = localIsBoxed;
            } else {
                _localIsBoxed = null;
            }
                
            this._maxStackDepth = maxStackDepth;
            this._instructions = instructions;
            this._handlers = handlers;
            this._debugInfos = debugInfos;

            _onlyFaultHandlers = true;
            foreach (var handler in handlers) {
                if (!handler.IsFault) {
                    _onlyFaultHandlers = false;
                    break;
                }
            }
        }

        internal StackFrame MakeFrame() {
            return MakeFrame(LightLambda.EmptyClosure);
        }

        internal StackFrame MakeFrame(StrongBox<object>[] closure) {
            var ret = new StackFrame(_numberOfLocals, _maxStackDepth);
            ret.Closure = closure;
            
            return ret;
        }

        private void BoxLocals(StackFrame frame) {
            if (_localIsBoxed != null) {
                for (int i = 0; i < _localIsBoxed.Length; i++) {
                    if (_localIsBoxed[i]) {
                        frame.Data[i] = new StrongBox<object>(frame.Data[i]);
                    }
                }
            }
        }

        private static MethodInfo _runMethod = typeof(Interpreter).GetMethod("Run");
        public object Run(StackFrame frame) {
            BoxLocals(frame);
            if (_onlyFaultHandlers) {
                bool fault = true;
                try {
                    RunInstructions(_instructions, frame);
                    fault = false;
                    return frame.Pop();
                } finally {
                    if (fault) {
                        HandleFault(frame);
                    }
                }
            } else {
                while (true) {
                    try {
                        RunInstructions(_instructions, frame);
                        return frame.Pop();
                    } catch (Exception exc) {
                        ExceptionHelpers.AssociateDynamicStackFrames(exc);
                        if (!HandleCatch(frame, exc)) {
                            ExceptionHelpers.UpdateForRethrow(exc);
                            throw;
                        } else if (exc is System.Threading.ThreadAbortException) {
                            // we can't exit the catch block here or the CLR will forcibly rethrow
                            // the exception on us.
                            Run(frame);
                        }
                    }
                }
            }
        }

        private void UpdateStackTrace(StackFrame frame) {
            foreach (var info in _debugInfos) {
                if (info.Matches(frame.InstructionIndex)) {
                    ExceptionHelpers.UpdateStackTrace(null, _runMethod, _lambda.Name, info.FileName, info.StartLine);
                    return;
                }
            }
        }

        private ExceptionHandler GetBestHandler(StackFrame frame, Type exceptionType) {
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

        private bool HandleCatch(StackFrame frame, Exception exception) {
            UpdateStackTrace(frame);

            Type exceptionType = exception.GetType();
            var handler = GetBestHandler(frame, exceptionType);
            if (handler == null) return false;

            frame.StackIndex = _numberOfLocals;
            if (handler.IsFault) {
                frame.InstructionIndex = handler.JumpToIndex;

                //var savedFrames = ExceptionHelpers.DynamicStackFrames;
                //ExceptionHelpers.DynamicStackFrames = null;

                RunFaultHandlerWithCatch(frame, handler.EndHandlerIndex);
                if (frame.InstructionIndex == handler.EndHandlerIndex) {
                    //ExceptionHelpers.DynamicStackFrames = savedFrames;
                    frame.InstructionIndex -= 1; // push back into the right range

                    return HandleCatch(frame, exception);
                } else {
                    return true;
                }
            } else {
                if (handler.PushException) {
                    frame.Push(exception);
                }
                frame.InstructionIndex = handler.JumpToIndex;
                return true;
            }
        }

        private void RunFaultHandlerWithCatch(StackFrame frame, int endIndex) {
            while (true) {
                try {
                    RunInstructions(_instructions, frame, endIndex);
                    return;
                } catch (Exception exc) {
                    ExceptionHelpers.AssociateDynamicStackFrames(exc);
                    if (!HandleCatch(frame, exc)) {
                        ExceptionHelpers.UpdateForRethrow(exc);
                        throw;
                    }
                }
            }
        }

        private void HandleFault(StackFrame frame) {
            UpdateStackTrace(frame);

            var handler = GetBestHandler(frame, null);
            if (handler == null) return;
            frame.StackIndex = _numberOfLocals;
            try {
                frame.InstructionIndex = handler.JumpToIndex;
                RunInstructions(_instructions, frame, handler.EndHandlerIndex);
            } finally {
                // This assumes that finally faults are propogated always
                frame.InstructionIndex = handler.EndHandlerIndex;
                HandleFault(frame);
            }
        }

        private static void RunInstructions(Instruction[] instructions, StackFrame frame, int endInstruction) {
            int index = frame.InstructionIndex;
            while (index < endInstruction) {
                index += instructions[index].Run(frame);
                frame.InstructionIndex = index;
            }
        }

        private static void RunInstructions(Instruction[] instructions, StackFrame frame) {
            int index = frame.InstructionIndex;
            while (index < instructions.Length) {
                index += instructions[index].Run(frame);
                frame.InstructionIndex = index;
            }
        }
    }
}
