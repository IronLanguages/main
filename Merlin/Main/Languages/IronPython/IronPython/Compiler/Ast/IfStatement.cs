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

using System.Dynamic;
using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class IfStatement : Statement {
        private readonly IfStatementTest[] _tests;
        private readonly Statement _else;

        public IfStatement(IfStatementTest[] tests, Statement else_) {
            _tests = tests;
            _else = else_;
        }

        public IfStatementTest[] Tests {
            get { return _tests; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            MSAst.Expression result;

            if (_else != null) {
                result = ag.Transform(_else);
            } else {
                result = AstUtils.Empty();
            }

            // Now build from the inside out
            int i = _tests.Length;
            while (i-- > 0) {
                IfStatementTest ist = _tests[i];

                result = ag.AddDebugInfo(
                    Ast.Condition(
                        ag.TransformAndDynamicConvert(ist.Test, typeof(bool)), 
                        ag.Transform(ist.Body), 
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

        internal override bool CanThrow {
            get {
                return this._tests[0].Test.CanThrow;
            }
        }
    }
}
