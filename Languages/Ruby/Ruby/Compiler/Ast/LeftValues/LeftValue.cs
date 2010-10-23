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

using System.Collections.Generic;
using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast {

    public abstract class LeftValue : Expression {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        internal static new readonly LeftValue[] EmptyArray = new LeftValue[0];

        public LeftValue(SourceSpan location)
            : base(location) {
        }

        // Gets an expression that evaluates to the part of the left value that represents a holder (target) of the left value;
        // For example target.bar, target[key], ...
        // This target is passed to the TransformWrite by assignment expressions.
        // This is necessary to prevent redundant evaluation of the target expression in in-place assignment left op= right.
        // Returns null if the left value doesn't have target expression.
        internal abstract MSA.Expression TransformTargetRead(AstGenerator/*!*/ gen);
        
        internal sealed override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return TransformRead(gen, TransformTargetRead(gen), false);
        }
        
        internal MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            return TransformWrite(gen, TransformTargetRead(gen), rightValue);
        }

        internal abstract MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, MSA.Expression targetValue, bool tryRead);
        internal abstract MSA.Expression/*!*/ TransformWrite(AstGenerator/*!*/ gen, MSA.Expression targetValue, MSA.Expression/*!*/ rightValue);
    }
}
