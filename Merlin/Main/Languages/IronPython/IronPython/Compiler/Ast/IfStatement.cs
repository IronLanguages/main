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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;

using IronPython.Runtime;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class IfStatement : Statement, IInstructionProvider {
        private readonly IfStatementTest[] _tests;
        private readonly Statement _else;

        public IfStatement(IfStatementTest[] tests, Statement else_) {
            _tests = tests;
            _else = else_;
        }

        public IList<IfStatementTest> Tests {
            get { return _tests; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        public override MSAst.Expression Reduce() {
            return ReduceWorker(true);
        }

        #region IInstructionProvider Members

        void IInstructionProvider.AddInstructions(LightCompiler compiler) {
            // optimizing bool conversions does no good in the light compiler
            compiler.Compile(ReduceWorker(false));
        }

        #endregion

        private MSAst.Expression ReduceWorker(bool optimizeDynamicConvert) {
            MSAst.Expression result;

            if (_else != null) {
                result = _else;
            } else {
                result = AstUtils.Empty();
            }

            // Now build from the inside out
            int i = _tests.Length;
            while (i-- > 0) {
                IfStatementTest ist = _tests[i];

                result = GlobalParent.AddDebugInfoAndVoid(
                    Ast.Condition(
                        optimizeDynamicConvert ?
                            TransformAndDynamicConvert(ist.Test, typeof(bool)) :
                            GlobalParent.Convert(typeof(bool), Microsoft.Scripting.Actions.ConversionResultKind.ExplicitCast, ist.Test),
                        TransformMaybeSingleLineSuite(ist.Body, ist.Test.Start),
                        result
                    ),
                    new SourceSpan(ist.Start, ist.Header)
                );
            }

            return result;
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_tests != null) {
                    foreach (IfStatementTest test in _tests) {
                        test.Walk(walker);
                    }
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
