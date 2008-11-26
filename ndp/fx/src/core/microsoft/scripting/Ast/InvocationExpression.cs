/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Dynamic.Binders;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class InvocationExpression : Expression, IArgumentProvider {
        private IList<Expression> _arguments;
        private readonly Expression _lambda;
        private readonly Type _returnType;

        internal InvocationExpression(Expression lambda, IList<Expression> arguments, Type returnType) {

            _lambda = lambda;
            _arguments = arguments;
            _returnType = returnType;
        }

        protected override Type GetExpressionType() {
            return _returnType;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Invoke;
        }

        public Expression Expression {
            get { return _lambda; }
        }

        public ReadOnlyCollection<Expression> Arguments {
            get { return ReturnReadOnly(ref _arguments); }
        }

        Expression IArgumentProvider.GetArgument(int index) {
            return _arguments[index];
        }

        int IArgumentProvider.ArgumentCount {
            get {
                return _arguments.Count;
            }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitInvocation(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {

        //CONFORMING
        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) {
            return Invoke(expression, arguments.ToReadOnly());
        }

        //CONFORMING
        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments) {
            RequiresCanRead(expression, "expression");

            Type delegateType = expression.Type;
            if (delegateType == typeof(Delegate)) {
                throw Error.ExpressionTypeNotInvocable(delegateType);
            } else if (!typeof(Delegate).IsAssignableFrom(expression.Type)) {
                Type exprType = TypeUtils.FindGenericType(typeof(Expression<>), expression.Type);
                if (exprType == null) {
                    throw Error.ExpressionTypeNotInvocable(expression.Type);
                }
                delegateType = exprType.GetGenericArguments()[0];
            }

            var mi = delegateType.GetMethod("Invoke");
            var args = arguments.ToReadOnly();
            ValidateArgumentTypes(mi, ExpressionType.Invoke, ref args);
            return new InvocationExpression(expression, args, mi.ReturnType);
        }
    }
}
