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

using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {
    using Microsoft.Scripting.Utils;
    #if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

    
    // x = expression rescue jump_statement
    // x = expression rescue expression
    public partial class RescueExpression : Expression {
        private readonly SourceSpan _rescueSpan;
        private readonly Expression/*!*/ _guardedExpression;
        private readonly Expression/*!*/ _rescueClauseStatement;

        public Expression/*!*/ GuardedExpression {
            get { return _guardedExpression; }
        }

        public Expression/*!*/ RescueClauseStatement {
            get { return _rescueClauseStatement; }
        }
        
        public RescueExpression(Expression/*!*/ guardedExpression, Expression/*!*/ rescueClauseStatement, SourceSpan rescueSpan, SourceSpan location)
            : base(location) {
            ContractUtils.RequiresNotNull(guardedExpression, "guardedExpression");
            ContractUtils.RequiresNotNull(rescueClauseStatement, "rescueClauseStatement");

            _guardedExpression = guardedExpression;
            _rescueClauseStatement = rescueClauseStatement;
            _rescueSpan = rescueSpan;
        }

        private Body/*!*/ ToBody(AstGenerator/*!*/ gen) {
            return new Body(
                new Statements(_guardedExpression),
                CollectionUtils.MakeList(new RescueClause(Expression.EmptyArray, null, new Statements(_rescueClauseStatement), _rescueSpan)),
            null, null, Location);
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return ToBody(gen).TransformResult(gen, ResultOperation.Ignore);
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator gen) {
            return ToBody(gen).TransformRead(gen);
        }
    }
}
