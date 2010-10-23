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

using System.Collections.Generic;

namespace IronRuby.Compiler.Ast {
    public partial class Walker {
        public void VisitOptionalList<T>(IEnumerable<T> list) where T : Node {
            if (list != null) {
                foreach (var item in list) {
                    item.Walk(this);
                }
            }
        }

        public void VisitList<T>(IEnumerable<T>/*!*/ list) where T : Node {
            foreach (var item in list) {
                item.Walk(this);
            }
        }

        public void Walk(Node/*!*/ node) {
            node.Walk(this);
        }

        #region Default Walk

        internal protected virtual void Walk(Arguments/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Expressions);
            }
            Exit(node);
        }

        internal protected virtual void Walk(SplattedArgument/*!*/ node) {
            if (Enter(node)) {
                node.Argument.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(BlockReference/*!*/ node) {
            if (Enter(node)) {
                node.Expression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(BlockDefinition/*!*/ node) {
            if (Enter(node)) {
                node.Parameters.Walk(this);
                VisitList(node.Body);
            }
            Exit(node);
        }

        internal protected virtual void Walk(Body/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Statements);
                VisitOptionalList(node.RescueClauses);
                VisitOptionalList(node.ElseStatements);
                VisitOptionalList(node.EnsureStatements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(Maplet/*!*/ node) {
            if (Enter(node)) {
                node.Key.Walk(this);
                node.Value.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(Parameters/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Mandatory);
                VisitList(node.Optional);

                if (node.Unsplat != null) {
                    node.Unsplat.Walk(this);
                }

                if (node.Block != null) {
                    node.Block.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(RegexMatchReference/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        // TODO:

        internal protected virtual void Walk(SourceUnitTree/*!*/ node) {
            if (Enter(node)) {
                VisitOptionalList(node.Initializers);
                VisitOptionalList(node.Statements);
            }

            Exit(node);
        }

        internal protected virtual void Walk(ParallelAssignmentExpression/*!*/ node) {
            if (Enter(node)) {
                node.Left.Walk(this);
            }
            Exit(node);
        }


        internal protected virtual void Walk(MemberAssignmentExpression/*!*/ node) {
            if (Enter(node)) {
                node.LeftTarget.Walk(this);
                node.Right.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(SimpleAssignmentExpression/*!*/ node) {
            if (Enter(node)) {
                node.Left.Walk(this);
                node.Right.Walk(this);
            }
            Exit(node);
        }


        internal protected virtual void Walk(ElseIfClause/*!*/ node) {
            if (Enter(node)) {
                if (node.Condition != null) {
                    node.Condition.Walk(this);
                }

                VisitOptionalList(node.Statements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(WhenClause/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Comparisons);
                VisitOptionalList(node.Statements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RescueClause/*!*/ node) {
            if (Enter(node)) {
                if (node.Types != null) {
                    VisitOptionalList(node.Types);
                }

                if (node.Target != null) {
                    node.Target.Walk(this);
                }

                if (node.Statements != null) {
                    VisitOptionalList(node.Statements);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(ClassDefinition/*!*/ node) {
            if (Enter(node)) {
                if (node.QualifiedName != null) {
                    node.QualifiedName.Walk(this);
                }

                if (node.SuperClass != null) {
                    node.SuperClass.Walk(this);
                }

                node.Body.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ModuleDefinition/*!*/ node) {
            if (Enter(node)) {
                if (node.QualifiedName != null) {
                    node.QualifiedName.Walk(this);
                }

                node.Body.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(SingletonDefinition/*!*/ node) {
            if (Enter(node)) {
                if (node.QualifiedName != null) {
                    node.QualifiedName.Walk(this);
                }

                node.Singleton.Walk(this);

                node.Body.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(MethodDefinition/*!*/ node) {
            if (Enter(node)) {
                if (node.Target != null) {
                    node.Target.Walk(this);
                }

                node.Parameters.Walk(this);

                node.Body.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(LambdaDefinition/*!*/ node) {
            if (Enter(node)) {
                node.Block.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(AndExpression/*!*/ node) {
            if (Enter(node)) {
                node.Left.Walk(this);
                node.Right.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(IsDefinedExpression/*!*/ node) {
            if (Enter(node)) {
                node.Expression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(BlockExpression/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Statements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ArrayConstructor/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Arguments.Expressions);
            }
            Exit(node);
        }

        internal protected virtual void Walk(CaseExpression/*!*/ node) {
            if (Enter(node)) {
                if (node.Value != null) {
                    node.Value.Walk(this);
                }

                VisitOptionalList(node.WhenClauses);
                VisitOptionalList(node.ElseStatements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ConditionalExpression/*!*/ node) {
            if (Enter(node)) {
                node.Condition.Walk(this);
                node.TrueExpression.Walk(this);
                node.FalseExpression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ConditionalJumpExpression/*!*/ node) {
            if (Enter(node)) {
                node.Condition.Walk(this);
                node.JumpStatement.Walk(this);
                if (node.Value != null) {
                    node.Value.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(ErrorExpression/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(ForLoopExpression/*!*/ node) {
            if (Enter(node)) {
                if (node.Block != null) {
                    node.Block.Walk(this);
                }

                if (node.List != null) {
                    node.List.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(HashConstructor/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Maplets);
            }
            Exit(node);
        }

        internal protected virtual void Walk(IfExpression/*!*/ node) {
            if (Enter(node)) {
                node.Condition.Walk(this);
                VisitOptionalList(node.Body);
                VisitOptionalList(node.ElseIfClauses);
            }
            Exit(node);
        }

        internal protected virtual void Walk(Literal/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(StringLiteral/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(SymbolLiteral/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(FileLiteral/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(EncodingExpression/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(MethodCall/*!*/ node) {
            if (Enter(node)) {
                if (node.Target != null) {
                    node.Target.Walk(this);
                }

                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(MatchExpression/*!*/ node) {
            if (Enter(node)) {
                node.Regex.Walk(this);
                node.Expression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(NotExpression/*!*/ node) {
            if (Enter(node)) {
                node.Expression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(OrExpression/*!*/ node) {
            if (Enter(node)) {
                node.Left.Walk(this);
                node.Right.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RangeExpression/*!*/ node) {
            if (Enter(node)) {
                node.Begin.Walk(this);
                node.End.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RangeCondition/*!*/ node) {
            if (Enter(node)) {
                node.Range.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RegularExpression/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Pattern);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RegularExpressionCondition/*!*/ node) {
            if (Enter(node)) {
                node.RegularExpression.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(RescueExpression/*!*/ node) {
            if (Enter(node)) {
                node.GuardedExpression.Walk(this);
                node.RescueClauseStatement.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(StringConstructor/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.Parts);
            }
            Exit(node);
        }

        internal protected virtual void Walk(SelfReference/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(UnlessExpression/*!*/ node) {
            if (Enter(node)) {
                node.Condition.Walk(this);

                VisitOptionalList(node.Statements);

                if (node.ElseClause != null) {
                    node.ElseClause.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(SuperCall/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(WhileLoopExpression/*!*/ node) {
            if (Enter(node)) {
                if (node.Condition != null) {
                    node.Condition.Walk(this);
                }

                VisitOptionalList(node.Statements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(YieldCall/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(BreakStatement/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(NextStatement/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(RedoStatement/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(RetryStatement/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(ReturnStatement/*!*/ node) {
            if (Enter(node)) {
                if (node.Arguments != null) {
                    VisitList(node.Arguments.Expressions);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(ArrayItemAccess/*!*/ node) {
            if (Enter(node)) {
                node.Array.Walk(this);
                VisitList(node.Arguments.Expressions);
                if (node.Block != null) {
                    node.Block.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(AttributeAccess/*!*/ node) {
            if (Enter(node)) {
                node.Qualifier.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ConstantVariable/*!*/ node) {
            if (Enter(node)) {
                if (node.Qualifier != null) {
                    node.Qualifier.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(ClassVariable/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(CompoundLeftValue/*!*/ node) {
            if (Enter(node)) {
                VisitList(node.LeftValues);
            }
            Exit(node);
        }

        internal protected virtual void Walk(GlobalVariable/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(InstanceVariable/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(LocalVariable/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(Placeholder/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(ConditionalStatement/*!*/ node) {
            if (Enter(node)) {
                node.Condition.Walk(this);
                node.Body.Walk(this);
                if (node.ElseStatement != null) {
                    node.ElseStatement.Walk(this);
                }
            }
            Exit(node);
        }

        internal protected virtual void Walk(AliasStatement/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        internal protected virtual void Walk(FileInitializerStatement/*!*/ node) {
            if (Enter(node)) {
                VisitOptionalList(node.Statements);
            }
            Exit(node);
        }

        internal protected virtual void Walk(ShutdownHandlerStatement/*!*/ node) {
            if (Enter(node)) {
                node.Block.Walk(this);
            }
            Exit(node);
        }

        internal protected virtual void Walk(UndefineStatement/*!*/ node) {
            Enter(node);
            Exit(node);
        }

        #endregion
    }
}
