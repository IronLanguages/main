/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public sealed class BlockBuilder : ExpressionCollectionBuilder<Expression> {
        public BlockBuilder() {
        }

        /// <summary>
        /// Returns <c>null</c> if no expression was added into the builder.
        /// If only a single expression was added returns it.
        /// Otherwise returns a <see cref="BlockExpression"/> containing the expressions added to the builder.
        /// </summary>
        public Expression ToExpression() {
            switch (Count) {
                case 0: return null;
                case 1: return Expression0;
                case 2: return Expression.Block(Expression0, Expression1);
                case 3: return Expression.Block(Expression0, Expression1, Expression2);
                case 4: return Expression.Block(Expression0, Expression1, Expression2, Expression3);
                default: return Expression.Block(Expressions);
            }
        }

        public static implicit operator Expression(BlockBuilder/*!*/ block) {
            return block.ToExpression();
        }
    }
}
