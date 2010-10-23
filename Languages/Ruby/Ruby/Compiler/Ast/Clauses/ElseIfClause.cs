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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    public partial class ElseIfClause : Node {
        private readonly Statements/*!*/ _statements;

        /// <summary>
        /// Null means a simple else.
        /// </summary>
        private readonly Expression _condition;

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        public Expression Condition {
            get { return _condition; }
        }

        public ElseIfClause(Expression condition, Statements/*!*/ statements, SourceSpan location)
            : base(location) {
            Assert.NotNull(statements);
            _statements = statements;
            _condition = condition;
        }
    }
}
