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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;
using System.Diagnostics;

namespace IronRuby.Compiler.Ast {

    // #{<statement>; ... ;<statement>}
    // (<statement>; ... ;<statement>)
    public partial class BlockExpression : Expression {
        internal static readonly BlockExpression Empty = new BlockExpression();

        private readonly Statements/*!*/ _statements;

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        private BlockExpression() 
            : base(SourceSpan.None) {
            _statements = EmptyStatements;            
        }
        
        internal BlockExpression(Statements/*!*/ statements, SourceSpan location)
            : base(location) {
            Assert.NotNull(statements);
            Debug.Assert(statements.Count > 1);

            _statements = statements;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return gen.TransformStatementsToExpression(_statements);
        }

        internal override MSA.Expression/*!*/ TransformReadBoolean(AstGenerator/*!*/ gen, bool positive) {
            return gen.TransformStatementsToBooleanExpression(_statements, positive);
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            // do not mark a sequence point wrapping the entire block:
            return TransformRead(gen);
        }
    }
}
