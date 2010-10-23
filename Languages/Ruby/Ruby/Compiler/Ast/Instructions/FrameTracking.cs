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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using IronRuby.Runtime;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System;

namespace IronRuby.Compiler.Ast {
    
    internal sealed class EnterInterpretedFrameExpression : ReducibleEmptyExpression, IInstructionProvider {
        internal static readonly MSA.Expression Instance = new EnterInterpretedFrameExpression();

        public void AddInstructions(LightCompiler compiler) {
            compiler.Instructions.Emit(_Instruction.Instance);
        }
        
        public sealed override Type/*!*/ Type {
            get { return typeof(InterpretedFrame); }
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override MSA.Expression/*!*/ Reduce() {
            return MSA.Expression.Constant(null, typeof(InterpretedFrame));
        }

        protected override MSA.Expression VisitChildren(MSA.ExpressionVisitor visitor) {
            return this;
        }

        private sealed class _Instruction : Instruction {
            internal static readonly Instruction Instance = new _Instruction();

            public override int ProducedStack { get { return 1; } }

            public override int Run(InterpretedFrame frame) {
                frame.Push(InterpretedFrame.CurrentFrame.Value);
                return +1;
            }

            public override string InstructionName {
                get { return "Ruby:EnterInterpretedFrame"; }
            }
        }
    }
}
