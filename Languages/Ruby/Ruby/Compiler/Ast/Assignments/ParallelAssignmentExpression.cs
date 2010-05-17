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

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    /// <summary>
    /// compound-lhs = compound-rhs
    /// </summary>
    public partial class ParallelAssignmentExpression : AssignmentExpression {
        private readonly CompoundLeftValue/*!*/ _lhs;
        private readonly CompoundRightValue/*!*/ _rhs;

        public CompoundLeftValue/*!*/ Left {
            get { return _lhs; }
        }

        public CompoundRightValue/*!*/ Right {
            get { return _rhs; }
        }

        public ParallelAssignmentExpression(CompoundLeftValue/*!*/ lhs, CompoundRightValue/*!*/ rhs, SourceSpan location)
            : base(null, location) {
            Assert.NotNull(lhs, rhs);

            _lhs = lhs;
            _rhs = rhs;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);
            return _lhs.TransformWrite(gen, _rhs);
        }
    }
}
