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

using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {

    public class WhileStatement : Statement {
        // Marks the end of the condition of the while loop
        private SourceLocation _header;
        private readonly Expression _test;
        private readonly Statement _body;
        private readonly Statement _else;

        public WhileStatement(Expression test, Statement body, Statement else_) {
            _test = test;
            _body = body;
            _else = else_;
        }

        public Expression Test {
            get { return _test;}
        }

        public Statement Body {
            get { return _body; }
        }

        public Statement ElseStatement {
            get { return _else; }
        }

        private SourceSpan Header {
            get { return new SourceSpan(Start, _header); }
        }

        public void SetLoc(SourceLocation start, SourceLocation header, SourceLocation end) {
            Start = start;
            _header = header;
            End = end;
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            // Only the body is "in the loop" for the purposes of break/continue
            // The "else" clause is outside
            MSAst.LabelTarget breakLabel, continueLabel;

            ConstantExpression constTest = _test as ConstantExpression;
            if (constTest != null && constTest.Value is int) {
                // while 0: / while 1:
                int val = (int)constTest.Value;
                if (val == 0) {
                    // completely optimize the loop away
                    if (_else == null) {
                        return MSAst.Expression.Empty();
                    } else {
                        return ag.Transform(_else);
                    }
                }

                MSAst.Expression test = MSAst.Expression.Constant(true);
                MSAst.Expression res = AstUtils.While(
                    test,
                    ag.TransformLoopBody(_body, SourceLocation.Invalid, out breakLabel, out continueLabel),
                    ag.Transform(_else),
                    breakLabel,
                    continueLabel
                );

                if (_test.Start.Line != _body.Start.Line) {
                    res = ag.AddDebugInfo(res, _test.Span);
                }

                return res;
            }

            return AstUtils.While(
                ag.AddDebugInfo(
                    ag.TransformAndDynamicConvert(_test, typeof(bool)),
                    Header
                ),
                ag.TransformLoopBody(_body, _test.Start, out breakLabel, out continueLabel), 
                ag.Transform(_else),
                breakLabel,
                continueLabel
            );
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
                if (_else != null) {
                    _else.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        internal override bool CanThrow {
            get {
                return _test.CanThrow;
            }
        }
    }
}
