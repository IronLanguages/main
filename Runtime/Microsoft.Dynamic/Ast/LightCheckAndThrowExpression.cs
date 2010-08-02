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
using System.Reflection;
using Microsoft.Scripting.Utils;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Provides a method call to a method which may return light exceptions. 
    /// 
    /// The call is to a method which supports light exceptions.  When reducing
    /// an additional check and throw is added.  When a block code of is re-written
    /// for light exceptions this instead reduces to not throw a .NET exception.
    /// </summary>
    internal class LightCheckAndThrowExpression : Expression, ILightExceptionAwareExpression {
        private readonly Expression _expr;

        internal LightCheckAndThrowExpression(Expression instance) {
            _expr = instance;
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
            return Utils.Convert(
                Expression.Call(LightExceptions._checkAndThrow, _expr),
                _expr.Type
            );
        }

        #region ILightExceptionAwareExpression Members

        Expression ILightExceptionAwareExpression.ReduceForLightExceptions() {
            return _expr;
        }

        #endregion

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            var instance = visitor.Visit(_expr);
            if (instance != _expr) {
                return new LightCheckAndThrowExpression(instance);
            }
            return this;
        }
    }
}
