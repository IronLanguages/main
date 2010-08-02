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

using System.Dynamic;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {

    /// <summary>
    /// lhs = rhs
    /// lhs op= rhs
    /// </summary>
    public abstract class AssignmentExpression : Expression {
        // "&" "|", null, etc
        private string _operation;

        public string Operation {
            get { return _operation; }
            internal set { _operation = value; }
        }

        public AssignmentExpression(string operation, SourceSpan location)
            : base(location) {

            _operation = operation;
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "assignment";
        }
    }
}
