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

using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {

    public class WhileStatement : Statement, ILoopStatement {
        // Marks the end of the condition of the while loop
        private SourceLocation _header;
        private readonly Expression _test;
        private readonly Statement _body;
        private readonly Statement _else;
        private MSAst.LabelTarget _break, _continue;

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

        MSAst.LabelTarget ILoopStatement.BreakLabel {
            get {
                return _break;
            }
            set {
                _break = value;
            }
        }

        MSAst.LabelTarget ILoopStatement.ContinueLabel {
            get {
                return _continue;
            }
            set {
                _continue = value;
            }
        }

        public override MSAst.Expression Reduce() {
            // Only the body is "in the loop" for the purposes of break/continue
            // The "else" clause is outside

            ConstantExpression constTest = _test as ConstantExpression;
            if (constTest != null && constTest.Value is int) {
                // while 0: / while 1:
                int val = (int)constTest.Value;
                if (val == 0) {
                    // completely optimize the loop away
                    if (_else == null) {
                        return MSAst.Expression.Empty();
                    } else {
                        return _else;
                    }
                }

                MSAst.Expression test = MSAst.Expression.Constant(true);
                MSAst.Expression res = AstUtils.While(
                    test,
                    _body,
                    _else,
                    _break,
                    _continue
                );

                if (_test.Start.Line != _body.Start.Line) {
                    res = GlobalParent.AddDebugInfoAndVoid(res, _test.Span);
                }

                return res;
            }

            return AstUtils.While(
                GlobalParent.AddDebugInfo(
                    TransformAndDynamicConvert(_test, typeof(bool)),
                    Header
                ),
                _body, 
                _else,
                _break,
                _continue
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
    }
}
