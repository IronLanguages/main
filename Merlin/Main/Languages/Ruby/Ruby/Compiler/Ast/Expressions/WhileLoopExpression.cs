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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    // pre-test:
    //   while <expression> do <statements> end
    //   until <expression> do <statements> end
    // post-test:
    //   <statement> while <expression>             
    //   <statement> until <expression>
    //   <block-expression> while <expression>   
    //   <block-expression> until <expression>
    public partial class WhileLoopExpression : Expression {
        private readonly Expression _condition;
        private readonly Statements _statements;		// optional
        private readonly bool _isWhileLoop; // while or until
        private readonly bool _isPostTest;  // do-while or while-do 

        public Expression Condition {
            get { return _condition; }
        }

        public Statements Statements {
            get { return _statements; }
        }

        public bool IsWhileLoop {
            get { return _isWhileLoop; }
        }

        public bool IsPostTest {
            get { return _isPostTest; }
        }

        public WhileLoopExpression(Expression/*!*/ condition, bool isWhileLoop, bool isPostTest, Statements/*!*/ statements, SourceSpan location)
            : base(location) {

            ContractUtils.RequiresNotNull(condition, "condition");
            ContractUtils.RequiresNotNull(statements, "statements");

            _condition = condition;
            _isWhileLoop = isWhileLoop;
            _isPostTest = isPostTest;
            _statements = statements;
        }

        // see Ruby Language.doc/Runtime/Control Flow Implementation/While-Until
        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            MSA.Expression resultVariable = gen.CurrentScope.DefineHiddenVariable("#loop-result", typeof(object));
            MSA.Expression redoVariable = gen.CurrentScope.DefineHiddenVariable("#skip-condition", typeof(bool));
            MSA.ParameterExpression blockUnwinder = gen.CurrentScope.DefineHiddenVariable("#unwinder", typeof(BlockUnwinder));
            MSA.ParameterExpression evalUnwinder = gen.CurrentScope.DefineHiddenVariable("#unwinder", typeof(EvalUnwinder));
            
            bool isInnerLoop = gen.CurrentLoop != null;

            MSA.LabelTarget breakLabel = Ast.Label();
            MSA.LabelTarget continueLabel = Ast.Label();

            gen.EnterLoop(redoVariable, resultVariable, breakLabel, continueLabel);
            MSA.Expression transformedBody = gen.TransformStatements(_statements, ResultOperation.Ignore);
            MSA.Expression transformedCondition = AstFactory.IsTrue(_condition.TransformRead(gen));
            gen.LeaveLoop();

            MSA.Expression conditionPositiveStmt, conditionNegativeStmt;
            if (_isWhileLoop) {
                conditionPositiveStmt = Ast.Empty();
                conditionNegativeStmt = Ast.Break(breakLabel);
            } else {
                conditionPositiveStmt = Ast.Break(breakLabel);
                conditionNegativeStmt = Ast.Empty();
            }

            // make the loop first:
            MSA.Expression loop = Ast.Block(
                Ast.Assign(redoVariable, Ast.Constant(_isPostTest)),

                AstFactory.Infinite(breakLabel, continueLabel,
                    AstUtils.Try(

                        AstUtils.If(redoVariable, 
                            Ast.Assign(redoVariable, Ast.Constant(false))
                        ).ElseIf(transformedCondition,
                            conditionPositiveStmt
                        ).Else(
                            conditionNegativeStmt
                        ),

                        transformedBody,
                        Ast.Empty()

                    ).Catch(blockUnwinder, 
                        // redo = u.IsRedo
                        Ast.Assign(redoVariable, Ast.Field(blockUnwinder, BlockUnwinder.IsRedoField)),
                        Ast.Empty()

                    ).Filter(evalUnwinder, Ast.Equal(Ast.Field(evalUnwinder, EvalUnwinder.ReasonField), AstFactory.BlockReturnReasonBreak),
                        // result = unwinder.ReturnValue
                        Ast.Assign(resultVariable, Ast.Field(evalUnwinder, EvalUnwinder.ReturnValueField)),
                        Ast.Break(breakLabel)
                    )
                ),
                Ast.Empty()
            );

            // wrap it to try finally that updates RFC state:
            if (!isInnerLoop) {
                loop = AstUtils.Try(
                    Ast.Assign(Ast.Field(gen.CurrentRfcVariable, RuntimeFlowControl.InLoopField), Ast.Constant(true)),
                    loop
                ).Finally(
                    Ast.Assign(Ast.Field(gen.CurrentRfcVariable, RuntimeFlowControl.InLoopField), Ast.Constant(false))
                );
            }

            return AstFactory.Block(loop, resultVariable);
        }
    }
}
