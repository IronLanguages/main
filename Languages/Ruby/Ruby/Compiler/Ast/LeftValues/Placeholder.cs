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
using Microsoft.Scripting.Utils;
using System;

namespace IronRuby.Compiler.Ast {

    /// <summary>
    /// Used as a null LHS without backing storage.
    /// E.g. 'a, = 1' is represented as 'ParallelAssignment(CoumpoundLeftValue(Variable, Placeholder), CompoundRightValue(Constant))
    /// </summary>
    public partial class Placeholder : Variable {
        public static readonly Placeholder/*!*/ Singleton = new Placeholder();

        private Placeholder()
            : base(String.Empty, SourceSpan.None) {
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            throw Assert.Unreachable;
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            Assert.NotNull(gen, rightValue);

            // no-op
            return rightValue;
        }

        public override string/*!*/ ToString() {
            return " ";
        }
    }
}
