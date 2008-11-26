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

using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {

    public partial class Initializer : Expression {
        //	BEGIN { body }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")] // TODO
        private readonly LexicalScope/*!*/ _definedScope;
        
        // TODO:
        private readonly List<Expression> _statements;

        public List<Expression> Statements {
            get { return _statements; }
        }
        
        public Initializer(LexicalScope/*!*/ definedScope, List<Expression> statements, SourceSpan location)
            : base(location) {
            Assert.NotNull(definedScope);

            _definedScope = definedScope;
            _statements = statements;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            throw new NotImplementedException();
        }
    }
}
