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
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;

namespace Microsoft.Scripting.Debugging {
    /// <summary>
    /// Used to extract locals information from expressions.
    /// </summary>
    internal sealed class LambdaWalker : MSAst.ExpressionVisitor {
        private readonly List<MSAst.ParameterExpression> _locals;
        private readonly Dictionary<MSAst.ParameterExpression, object> _strongBoxedLocals;

        internal LambdaWalker() {
            _locals = new List<MSAst.ParameterExpression>();
            _strongBoxedLocals = new Dictionary<MSAst.ParameterExpression, object>();
        }

        internal List<MSAst.ParameterExpression> Locals {
            get { return _locals; }
        }

        internal Dictionary<MSAst.ParameterExpression, object> StrongBoxedLocals {
            get { return _strongBoxedLocals; }
        }

        protected override MSAst.Expression VisitBlock(MSAst.BlockExpression node) {
            // Record all variables declared within the block
            foreach (MSAst.ParameterExpression local in node.Variables) {
                _locals.Add(local);
            }

            return base.VisitBlock(node);
        }

        protected override MSAst.Expression VisitRuntimeVariables(MSAst.RuntimeVariablesExpression node) {
            // Record all strongbox'ed variables
            foreach (MSAst.ParameterExpression local in node.Variables) {
                _strongBoxedLocals.Add(local, null);
            }

            return base.VisitRuntimeVariables(node);
        }

        protected override MSAst.Expression VisitLambda<T>(MSAst.Expression<T> node) {
            // Explicitely don't walk nested lambdas.  They should already have been transformed
            return node;
        }
    }
}
