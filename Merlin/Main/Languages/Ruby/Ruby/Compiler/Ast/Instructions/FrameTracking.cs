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

using IronRuby.Runtime;
using Microsoft.Scripting.Interpreter;
using MSA = System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System;

namespace IronRuby.Compiler.Ast {
    
    internal sealed class EnterInterpretedFrameExpression : ReducibleEmptyExpression, IInstructionProvider {
        internal static readonly MSA.Expression Instance = new EnterInterpretedFrameExpression();

        public void AddInstructions(LightCompiler compiler) {
            compiler.AddInstruction(_Instruction.Instance);
        }

        protected override MSA.ExpressionType NodeTypeImpl() {
            return MSA.ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(InterpretedFrame);
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override MSA.Expression/*!*/ Reduce() {
            return MSA.Expression.Constant(null, typeof(InterpretedFrame));
        }

        protected override MSA.Expression VisitChildren(Func<MSA.Expression, MSA.Expression> visitor) {
            return this;
        }

        private sealed class _Instruction : Instruction {
            internal static readonly Instruction Instance = new _Instruction();

            public override int ProducedStack { get { return 1; } }

            public override int Run(InterpretedFrame frame) {
                frame.Parent = RubyExceptionData.CurrentInterpretedFrame.Update(frame);
                frame.Push(frame);
                return +1;
            }

            public override string InstructionName {
                get { return "Ruby:EnterInterpretedFrame"; }
            }
        }
    }

    internal sealed class LeaveInterpretedFrameExpression : ReducibleEmptyExpression, IInstructionProvider {
        internal static readonly MSA.Expression Instance = new LeaveInterpretedFrameExpression();

        public void AddInstructions(LightCompiler compiler) {
            compiler.AddInstruction(_Instruction.Instance);
        }

        private sealed class _Instruction : Instruction {
            internal static readonly Instruction Instance = new _Instruction();

            public override int Run(InterpretedFrame frame) {
                RubyExceptionData.CurrentInterpretedFrame.Value = frame.Parent;
                return +1;
            }

            public override string InstructionName {
                get { return "Ruby:LeaveInterpretedFrame"; }
            }
        }
    }
}
