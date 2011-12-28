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

#if FEATURE_CORE_DLR
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    /// <summary>
    /// lhs = compound-rhs
    /// compound-lhs = rhs
    /// compound-lhs = compound-rhs
    /// </summary>
    public partial class ParallelAssignmentExpression : AssignmentExpression {
        // x1,x2,...,*y,z1,...,zn
        private readonly CompoundLeftValue/*!*/ _lhs;
        
        // [*]?a1,[*]?a2, ... [*]?an
        private readonly Expression/*!*/[]/*!*/ _rhs;

        public CompoundLeftValue/*!*/ Left {
            get { return _lhs; }
        }

        public Expression/*!*/[]/*!*/ Right {
            get { return _rhs; }
        }

        public ParallelAssignmentExpression(CompoundLeftValue/*!*/ lhs, Expression/*!*/[]/*!*/ rhs, SourceSpan location)
            : base(null, location) {
            Assert.NotNull(lhs, rhs);

            _lhs = lhs;
            _rhs = rhs;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);
            return _lhs.TransformWrite(gen, new Arguments(_rhs));
        }
    }
}
