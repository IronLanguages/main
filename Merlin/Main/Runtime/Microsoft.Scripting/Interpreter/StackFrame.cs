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

using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Interpreter {
    public class StackFrame {
        public readonly object[] Data;
        public StrongBox<object>[] Closure;
        public int StackIndex;
        public StackFrame Parent;
        public int InstructionIndex;
        public int FaultingInstruction;  // the last instruction to cause a fault

        public StackFrame(int numberOfLocals, int maxStackDepth) {
            StackIndex = numberOfLocals;
            Data = new object[numberOfLocals + maxStackDepth];
        }

        public void Push(object value) {
            Data[StackIndex++] = value;
        }

        public object Pop() {
            return Data[--StackIndex];
        }

        public object Peek() {
            return Data[StackIndex - 1];
        }
    }
}
