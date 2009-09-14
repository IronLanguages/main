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

using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = Expression;

    public abstract class JumpStatement : Expression {
        private readonly Arguments _arguments;

        public Arguments Arguments {
            get { return _arguments; }
        }

        public JumpStatement(Arguments arguments, SourceSpan location)
            : base(location) {
            _arguments = arguments;
        }

        internal MSA.Expression/*!*/ TransformReturnValue(AstGenerator/*!*/ gen) {
            return Arguments.TransformToReturnValue(gen, _arguments); 
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Utils.Convert(Transform(gen), typeof(object));
        }

        internal override MSA.Expression TransformResult(AstGenerator/*!*/ gen, ResultOperation resultOperation) {
            // the code will jump, ignore the result variable:
            return Transform(gen);
        }
    }
}
