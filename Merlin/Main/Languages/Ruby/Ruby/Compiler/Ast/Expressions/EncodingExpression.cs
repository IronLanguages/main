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
using System.Dynamic;
using System.Diagnostics;
using System.Text;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting;

using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    #if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

    using Ast = Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class EncodingExpression : Expression {
        public EncodingExpression(SourceSpan location) 
            : base(location) {
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return AstUtils.Constant(gen.Encoding);
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return null;
        }
    }
}
