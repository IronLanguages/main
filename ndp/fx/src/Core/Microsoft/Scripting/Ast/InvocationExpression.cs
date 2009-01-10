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
using System.Dynamic;
using System.Dynamic.Utils;
using System.Text;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents an expression that applies a delegate or lambda expression to a list of argument expressions.
    /// </summary>
    public sealed class InvocationExpression : Expression, IArgumentProvider {
        private IList<Expression> _arguments;
        private readonly Expression _lambda;
        private readonly Type _returnType;

        internal InvocationExpression(Expression lambda, IList<Expression> arguments, Type returnType) {
            _lambda = lambda;
            _arguments = arguments;
            _returnType = returnType;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            return _returnType;
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Invoke;
        }

        /// <summary>
        /// Gets the delegate or lambda expression to be applied.
        /// </summary>
        public Expression Expression {
            get { return _lambda; }
        }

        /// <summary>
        /// Gets the arguments that the delegate is applied to.
        /// </summary>
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

    public partial class Expression {

        ///<summary>Creates an <see cref="T:System.Linq.Expressions.InvocationExpression" />.</summary>
        ///<returns>An <see cref="T:System.Linq.Expressions.InvocationExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Invoke" /> and the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> and <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> properties set to the specified values.</returns>
        ///<param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> equal to.</param>
        ///<param name="arguments">An array of <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> collection.</param>
        ///<exception cref="T:System.ArgumentNullException">
        ///<paramref name="expression" /> is null.</exception>
        ///<exception cref="T:System.ArgumentException">
        ///<paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:System.Linq.Expressions.Expression`1" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        ///<exception cref="T:System.InvalidOperationException">
        ///<paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments) {
            return Invoke(expression, arguments.ToReadOnly());
        }

        ///<summary>Creates an <see cref="T:System.Linq.Expressions.InvocationExpression" />.</summary>
        ///<returns>An <see cref="T:System.Linq.Expressions.InvocationExpression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.Invoke" /> and the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> and <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> properties set to the specified values.</returns>
        ///<param name="expression">An <see cref="T:System.Linq.Expressions.Expression" /> to set the <see cref="P:System.Linq.Expressions.InvocationExpression.Expression" /> equal to.</param>
        ///<param name="arguments">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains <see cref="T:System.Linq.Expressions.Expression" /> objects to use to populate the <see cref="P:System.Linq.Expressions.InvocationExpression.Arguments" /> collection.</param>
        ///<exception cref="T:System.ArgumentNullException">
        ///<paramref name="expression" /> is null.</exception>
        ///<exception cref="T:System.ArgumentException">
        ///<paramref name="expression" />.Type does not represent a delegate type or an <see cref="T:System.Linq.Expressions.Expression`1" />.-or-The <see cref="P:System.Linq.Expressions.Expression.Type" /> property of an element of <paramref name="arguments" /> is not assignable to the type of the corresponding parameter of the delegate represented by <paramref name="expression" />.</exception>
        ///<exception cref="T:System.InvalidOperationException">
        ///<paramref name="arguments" /> does not contain the same number of elements as the list of parameters for the delegate represented by <paramref name="expression" />.</exception>
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
