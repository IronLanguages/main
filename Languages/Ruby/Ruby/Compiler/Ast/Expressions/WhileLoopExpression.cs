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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;

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
            MSA.ParameterExpression unwinder;
            
            bool isInnerLoop = gen.CurrentLoop != null;

            MSA.LabelTarget breakLabel = Ast.Label();
            MSA.LabelTarget continueLabel = Ast.Label();

            gen.EnterLoop(redoVariable, resultVariable, breakLabel, continueLabel);
            MSA.Expression transformedBody = gen.TransformStatements(_statements, ResultOperation.Ignore);
            MSA.Expression transformedCondition = _condition.TransformCondition(gen, true);
            gen.LeaveLoop();

            MSA.Expression conditionPositiveStmt, conditionNegativeStmt;
            if (_isWhileLoop) {
                conditionPositiveStmt = AstUtils.Empty();
                conditionNegativeStmt = Ast.Break(breakLabel);
            } else {
                conditionPositiveStmt = Ast.Break(breakLabel);
                conditionNegativeStmt = AstUtils.Empty();
            }

            // make the loop first:
            MSA.Expression loop = new AstBlock {
                gen.ClearDebugInfo(),
                Ast.Assign(redoVariable, AstUtils.Constant(_isPostTest)),

                AstFactory.Infinite(breakLabel, continueLabel,
                    AstUtils.Try(

                        AstUtils.If(redoVariable, 
                            Ast.Assign(redoVariable, AstUtils.Constant(false))
                        ).ElseIf(transformedCondition,
                            conditionPositiveStmt
                        ).Else(
                            conditionNegativeStmt
                        ),

                        transformedBody,
                        AstUtils.Empty()

                    ).Catch(unwinder = Ast.Parameter(typeof(BlockUnwinder), "#u"), 
                        // redo = u.IsRedo
                        Ast.Assign(redoVariable, Ast.Field(unwinder, BlockUnwinder.IsRedoField)),
                        AstUtils.Empty()

                    ).Filter(unwinder = Ast.Parameter(typeof(EvalUnwinder), "#u"), 
                        Ast.Equal(Ast.Field(unwinder, EvalUnwinder.ReasonField), AstFactory.BlockReturnReasonBreak),

                        // result = unwinder.ReturnValue
                        Ast.Assign(resultVariable, Ast.Field(unwinder, EvalUnwinder.ReturnValueField)),
                        Ast.Break(breakLabel)
                    )
                ),
                gen.ClearDebugInfo(),
                AstUtils.Empty(),
            };

            // wrap it to try finally that updates RFC state:
            if (!isInnerLoop) {
                loop = AstUtils.Try(
                    Methods.EnterLoop.OpCall(gen.CurrentScopeVariable),
                    loop
                ).Finally(
                    Methods.LeaveLoop.OpCall(gen.CurrentScopeVariable)
                );
            }

            return Ast.Block(loop, resultVariable);
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            // do not mark a sequence point wrapping the entire node:
            return TransformRead(gen);
        }        
    }
}
