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
using System.Threading;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Interpreter {

#if !SILVERLIGHT && DEBUG
    [DebuggerTypeProxy(typeof(Instructions.DebugView))]
#endif
    internal class Instructions : List<Instruction> {
#if DEBUG
        private readonly LightCompiler _compiler;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        internal Instructions(LightCompiler compiler) {
#if DEBUG
            _compiler = compiler;
#endif
        }

        #region Debug View
#if !SILVERLIGHT && DEBUG
        internal sealed class DebugView {
            private readonly Instructions _instructions;

            public DebugView(Instructions instructions) {
                Assert.NotNull(instructions);
                _instructions = instructions;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionView[]/*!*/ A0 {
                get {
                    var result = new List<InstructionView>();
                    int index = 0;
                    int stackDepth = 0;
                    foreach (var instruction in _instructions) {
                        result.Add(new InstructionView(index, stackDepth, instruction, _instructions._compiler));
                        index++;
                        stackDepth += instruction.ProducedStack - instruction.ConsumedStack;
                    }
                    return result.ToArray();
                }
            }

            [DebuggerDisplay("{GetValue(),nq}", Name = "{GetName(),nq}")]
            internal struct InstructionView {
                private readonly int _index;
                private readonly int _stackDepth;
                private readonly Instruction _instruction;
                private readonly LightCompiler _compiler;

                internal string GetName() {
                    return _index.ToString() + (_stackDepth == 0 ? "" : " D(" + _stackDepth.ToString() + ")");
                }

                internal string GetValue() {
                    return _instruction.ToString(_compiler) + " " + (_instruction.ProducedStack - _instruction.ConsumedStack).ToString();

                }

                public InstructionView(int index, int stackDepth, Instruction instruction, LightCompiler compiler) {
                    _index = index;
                    _stackDepth = stackDepth;
                    _instruction = instruction;
                    _compiler = compiler;
                }
            }
        }
#endif
        #endregion
    }
}
