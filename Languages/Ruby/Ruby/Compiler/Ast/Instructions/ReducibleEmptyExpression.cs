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

using System;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    internal abstract class ReducibleEmptyExpression : MSA.Expression {
        public sealed override MSA.ExpressionType NodeType {
            get { return MSA.ExpressionType.Extension; }
        }

        public override Type/*!*/ Type {
            get { return typeof(void); }
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override MSA.Expression/*!*/ Reduce() {
            return Ast.Empty();
        }

        protected override MSA.Expression VisitChildren(MSA.ExpressionVisitor visitor) {
            return this;
        }
    }
}
