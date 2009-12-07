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
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace IronRuby.Compiler {
    internal struct AstBlock : IEnumerable<Expression> {
        private readonly ReadOnlyCollectionBuilder<Expression> _body;
        private readonly IEnumerable<ParameterExpression> _variables;

        public AstBlock(IEnumerable<ParameterExpression> variables) {
            _variables = variables;
            _body = new ReadOnlyCollectionBuilder<Expression>();
        }

        public IEnumerator<Expression> GetEnumerator() {
            return _body.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _body.GetEnumerator();
        }

        public void Add(Expression expression) {
            if (expression != null) {
                _body.Add(expression);
            }
        }

        public static implicit operator BlockExpression(AstBlock/*!*/ block) {
            if (block._variables == null) {
                return Expression.Block(block._body);
            } else {
                return Expression.Block(block._variables, block._body);
            }
        }
    }
}
