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
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class AssignmentStatement : Statement {
        // _left.Length is 1 for simple assignments like "x = 1"
        // _left.Length will be 3 for "x = y = z = 1"
        private readonly Expression[] _left;
        private readonly Expression _right;

        public AssignmentStatement(Expression[] left, Expression right) {
            _left = left;
            _right = right;
        }

        public IList<Expression> Left {
            get { return _left; }
        }

        public Expression Right {
            get { return _right; }
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            if (_left.Length == 1) {
                // Do not need temps for simple assignment
                return AssignOne(ag);
            } else {
                return AssignComplex(ag, ag.Transform(_right));
            }
        }

        private MSAst.Expression AssignComplex(AstGenerator ag, MSAst.Expression right) {
            // Python assignment semantics:
            // - only evaluate RHS once. 
            // - evaluates assignment from left to right
            // - does not evaluate getters.
            // 
            // So 
            //   a=b[c]=d=f() 
            // should be:
            //   $temp = f()
            //   a = $temp
            //   b[c] = $temp
            //   d = $temp

            List<MSAst.Expression> statements = new List<MSAst.Expression>();

            // 1. Create temp variable for the right value
            MSAst.ParameterExpression right_temp = ag.GetTemporary("assignment");

            // 2. right_temp = right
            statements.Add(
                ag.MakeAssignment(right_temp, right)
                );

            // Do left to right assignment
            foreach (Expression e in _left) {
                if (e == null) {
                    continue;
                }

                // 3. e = right_temp
                MSAst.Expression transformed = e.TransformSet(ag, Span, right_temp, Operators.None);
                if (transformed == null) {
                    throw PythonOps.SyntaxError(String.Format("can't assign to {0}", e.NodeName), ag.Context.SourceUnit, e.Span, -1);
                }

                statements.Add(transformed);
            }

            // 4. Create and return the resulting suite
            statements.Add(Ast.Empty());
            return ag.AddDebugInfo(
                Ast.Block(statements.ToArray()),
                Span
            );
        }

        private MSAst.Expression AssignOne(AstGenerator ag) {
            Debug.Assert(_left.Length == 1);

            SequenceExpression seLeft = _left[0] as SequenceExpression;
            SequenceExpression seRight = _right as SequenceExpression;

            if (seLeft != null && seRight != null && seLeft.Items.Length == seRight.Items.Length) {
                int cnt = seLeft.Items.Length;

                // a, b = 1, 2, or [a,b] = 1,2 - not something like a, b = range(2)
                // we can do a fast parallel assignment
                MSAst.ParameterExpression[] tmps = new MSAst.ParameterExpression[cnt];
                MSAst.Expression[] body = new MSAst.Expression[cnt * 2 + 1];

                // generate the body, the 1st n are the temporary assigns, the
                // last n are the assignments to the left hand side
                // 0: tmp0 = right[0]
                // ...
                // n-1: tmpn-1 = right[n-1]
                // n: right[0] = tmp0
                // ...
                // n+n-1: right[n-1] = tmpn-1

                // allocate the temps first before transforming so we don't pick up a bad temp...
                for (int i = 0; i < cnt; i++) {
                    MSAst.Expression tmpVal = ag.Transform(seRight.Items[i]);
                    tmps[i] = ag.GetTemporary("parallelAssign", tmpVal.Type);

                    body[i] = Ast.Assign(tmps[i], tmpVal);
                }

                // then transform which can allocate more temps
                for (int i = 0; i < cnt; i++) {
                    body[i + cnt] = seLeft.Items[i].TransformSet(ag, SourceSpan.None, tmps[i], Operators.None);
                }

                // finally free the temps.
                foreach (MSAst.ParameterExpression variable in tmps) {
                    ag.FreeTemp(variable);
                }

                // 4. Create and return the resulting suite
                body[cnt * 2] = Ast.Empty();
                return ag.AddDebugInfo(Ast.Block(body), Span);
            }

            return _left[0].TransformSet(ag, Span, ag.Transform(_right), Operators.None);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                foreach (Expression e in _left) {
                    e.Walk(walker);
                }
                _right.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
