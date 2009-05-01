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
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    internal abstract class ReducibleEmptyExpression : MSA.Expression {
        protected override MSA.ExpressionType NodeTypeImpl() {
            return MSA.ExpressionType.Extension;
        }

        protected override Type/*!*/ TypeImpl() {
            return typeof(void);
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override MSA.Expression/*!*/ Reduce() {
            return Ast.Empty();
        }

        protected override MSA.Expression VisitChildren(Func<MSA.Expression, MSA.Expression> visitor) {
            return this;
        }
    }
}
