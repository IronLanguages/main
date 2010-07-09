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
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Expression which produces a light exception value.  This should be constructed
    /// with the expression which creates the exception and this method will then call
    /// a helper method which wraps the exception in our internal light exception class.
    /// </summary>
    class LightThrowExpression : Expression {
        private readonly Expression _exception;
        private static MethodInfo _throw = new Func<Exception, object>(LightExceptions.Throw).Method;

        public LightThrowExpression(Expression exception) {
            _exception = exception;
        }

        public override Expression Reduce() {
            return Expression.Call(_throw, _exception);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            var exception = visitor.Visit(_exception);
            if (exception != _exception) {
                return new LightThrowExpression(exception);
            }

            return this;
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public override ExpressionType NodeType {
            get {
                return ExpressionType.Extension;
            }
        }

        public override Type Type {
            get { return typeof(object); }
        }
    }
}
