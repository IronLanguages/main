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


#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Scripting.Ast {
    internal class LightExceptionConvertingExpression : Expression, ILightExceptionAwareExpression {
        private readonly Expression _expr;
        private readonly bool _supportsLightEx;

        internal LightExceptionConvertingExpression(Expression expr, bool supportsLightEx) {
            _expr = expr;
            _supportsLightEx = supportsLightEx;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }

        public override Type Type {
            get { return _expr.Type; }
        }

        public override Expression Reduce() {
            return new LightExceptionRewriter().Rewrite(_expr);
        }        

        #region ILightExceptionAwareExpression Members

        Expression ILightExceptionAwareExpression.ReduceForLightExceptions() {
            if (_supportsLightEx) {
                return _expr;
            }
            return Reduce();
        }

        #endregion

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            var expr = visitor.Visit(_expr);
            if (expr != _expr) {
                return new LightExceptionConvertingExpression(expr, _supportsLightEx);
            }
            return this;
        }
    }
}
