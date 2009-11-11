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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {
    public sealed class InterpretedFrame {
        internal readonly Interpreter Interpreter;
        public InterpretedFrame Parent;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public readonly object[] Data;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public readonly StrongBox<object>[] Closure;

        public int StackIndex;
        public int InstructionIndex;
        public int FaultingInstruction;  // the last instruction to cause a fault

        // When a ThreadAbortException is raised from interpreted code this is the first frame that caught it.
        // No handlers within this handler re-abort the current thread when left.
        public ExceptionHandler CurrentAbortHandler;

        internal InterpretedFrame(Interpreter interpreter, StrongBox<object>[] closure) {
            Interpreter = interpreter;
            StackIndex = interpreter._numberOfLocals;
            Data = new object[interpreter._numberOfLocals + interpreter._maxStackDepth];
            Closure = closure;
        }

        internal void BoxLocals() {
            bool[] boxedLocals = Interpreter._localIsBoxed;
            if (boxedLocals != null) {
                for (int i = 0; i < boxedLocals.Length; i++) {
                    if (boxedLocals[i]) {
                        Data[i] = new StrongBox<object>(Data[i]);
                    }
                }
            }
        }

        public static bool IsInterpretedFrame(MethodBase method) {
            ContractUtils.RequiresNotNull(method, "method");
            return method.DeclaringType == typeof(Interpreter) && method.Name == "Run";
        }

        public DebugInfo GetDebugInfo(int instructionIndex) {
            return DebugInfo.GetMatchingDebugInfo(Interpreter._debugInfos, instructionIndex);
        }

        public LambdaExpression Lambda {
            get { return Interpreter._lambda; }
        }

        public void Push(object value) {
            Data[StackIndex++] = value;
        }

        public void Push(bool value) {
            Data[StackIndex++] = value ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public void Push(int value) {
            Data[StackIndex++] = ScriptingRuntimeHelpers.Int32ToObject(value);
        }

        public object Pop() {
            return Data[--StackIndex];
        }

        internal void SetStackDepth(int depth) {
            StackIndex = Interpreter._numberOfLocals + depth;
        }

        public object Peek() {
            return Data[StackIndex - 1];
        }

        public void Dup() {
            int i = StackIndex;
            Data[i] = Data[i - 1];
            StackIndex = i + 1;
        }

#if DEBUG
        internal string[] Trace {
            get {
                var trace = new List<string>();
                var frame = this;
                do {
                    trace.Add(frame.Lambda.Name);
                    frame = frame.Parent;
                } while (frame != null);
                return trace.ToArray();
            }
        }
#endif
    }
}
