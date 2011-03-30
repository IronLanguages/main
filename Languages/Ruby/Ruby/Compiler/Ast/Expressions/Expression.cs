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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using IronRuby.Builtins;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    /// <summary>
    /// Represents expressions. Statements are considered special cases of expressions in AST class hierarchy.
    /// Unlike syntactic expression a syntactic statement cannot be assigned to a left value.
    /// However certain Ruby constructs (e.g. block-expression) allow to read the value of a statement. 
    /// Usually such value is null (e.g. undef, alias, while/until statements), 
    /// although some syntactic statements evaluate to a non-null value (e.g. if/unless-statements).
    /// </summary>
    public abstract class Expression : Node {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly Expression[]/*!*/ EmptyArray = new Expression[0];
        internal static readonly List<Expression>/*!*/ _EmptyList = new List<Expression>();
        internal static readonly Statements/*!*/ _EmptyStatements = new Statements();   

        internal static List<Expression>/*!*/ EmptyList {
            get {
                Debug.Assert(_EmptyList.Count == 0);
                return _EmptyList; 
            }
        }

        internal static Statements/*!*/ EmptyStatements {
            get {
                Debug.Assert(_EmptyStatements.Count == 0);
                return _EmptyStatements;
            }
        }
        
        protected Expression(SourceSpan location) 
            : base(location) {
        }

        /// <summary>
        /// Transform as expression (value is read).
        /// </summary>
        internal abstract MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen);

        /// <summary>
        /// Transform as expression (value is read) and mark sequence point.
        /// </summary>
        internal MSA.Expression/*!*/ TransformReadStep(AstGenerator/*!*/ gen) {
            return gen.AddDebugInfo(TransformRead(gen), Location);
        }
        
        /// <summary>
        /// Transform as expression whose value is read as Boolean.
        /// </summary>
        internal virtual MSA.Expression/*!*/ TransformReadBoolean(AstGenerator/*!*/ gen, bool positive) {
            return (positive ? Methods.IsTrue : Methods.IsFalse).OpCall(AstUtils.Box(TransformRead(gen))); 
        }
        
        /// <summary>
        /// Transform as expression whose value is read as Boolean and mark a sequence point.
        /// </summary>
        internal MSA.Expression/*!*/ TransformCondition(AstGenerator/*!*/ gen, bool positive) {
            return gen.AddDebugInfo(TransformReadBoolean(gen, positive), Location);
        }	
		

        /// <summary>
        /// Transform as statement (value is not read). Marks sequnce point.
        /// </summary>
        internal virtual MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return gen.AddDebugInfo(TransformRead(gen), Location);
        }

        /// <summary>
        /// Transform and handle the result according to the specified result operation. Marks sequence point.
        /// </summary>
        internal virtual MSA.Expression/*!*/ TransformResult(AstGenerator/*!*/ gen, ResultOperation resultOperation) {
            MSA.Expression resultExpression = TransformRead(gen);
            MSA.Expression statement;

            if (resultOperation.Variable != null) {
                statement = Ast.Assign(resultOperation.Variable, Ast.Convert(resultExpression, resultOperation.Variable.Type));
            } else {
                statement = gen.Return(resultExpression);
            }

            return gen.AddDebugInfo(statement, Location);
        }

        // Condition under which the expression is considered "defined?".
        // Returns null if there is no condition.
        internal virtual MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return null;
        }

        // the name that is returned when the expression is defined:
        internal virtual string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "expression";
        }

        internal virtual MSA.Expression/*!*/ TransformIsDefined(AstGenerator/*!*/ gen) {
            MSA.Expression condition = TransformDefinedCondition(gen);
            MSA.Expression result = Methods.CreateMutableStringL.OpCall(AstUtils.Constant(GetNodeName(gen)), Ast.Constant(RubyEncoding.Binary));
            return (condition != null) ? Ast.Condition(condition, result, AstFactory.NullOfMutableString) : result;
        }

        internal MSA.Expression/*!*/ TransformBooleanIsDefined(AstGenerator/*!*/ gen, bool positive) {
            var condition = TransformDefinedCondition(gen);
            if (condition == null) {
                return (positive) ? AstFactory.True : AstFactory.False;
            } else {
                return (positive) ? condition : Ast.Not(condition);
            }
        }

        // Called on an expression that is used as a condition. 
        // Returns an expression that represents a tree with some nodes converted to conditions.
        // Range expression with a non-literal-integer bound is converted to flip expression.
        internal virtual Expression/*!*/ ToCondition(LexicalScope/*!*/ currentScope) {
            return this;
        }
    }
}
