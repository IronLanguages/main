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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    //	when <expressions>, *<array>: <body>
    public partial class WhenClause : Node {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly WhenClause[] EmptyArray = new WhenClause[0];

        private readonly Expression/*!*/[]/*!*/ _comparisons;
        private readonly Statements _statements;              // optional

        public Statements Statements {
            get { return _statements; }
        }

        public Expression/*!*/[]/*!*/ Comparisons {
            get { return _comparisons; }
        }

        public WhenClause(Expression/*!*/[] comparisons, Statements statements, SourceSpan location)
            : base(location) {
            _comparisons = comparisons ?? Expression.EmptyArray;
            _statements = statements;
        }
    }
}
