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

using System.Collections.Generic;

namespace Microsoft.Scripting.Ast {
    internal sealed class LambdaParameterRewriter : ExpressionVisitor {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        internal LambdaParameterRewriter(Dictionary<ParameterExpression, ParameterExpression> map) {
            _map = map;
        }

        // We don't need to worry about parameter shadowing, because we're
        // replacing the instances consistently everywhere
        protected override Expression VisitParameter(ParameterExpression node) {
            ParameterExpression result;
            if (_map.TryGetValue(node, out result)) {
                return result;
            }
            return node;
        }
    }
}
