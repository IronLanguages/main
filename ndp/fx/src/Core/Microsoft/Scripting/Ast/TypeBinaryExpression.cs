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
using System.Dynamic.Utils;
using System.Text;
using System.Reflection;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents an operation between an expression and a type. 
    /// </summary>
    public sealed class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;
        private readonly ExpressionType _nodeKind;

        internal TypeBinaryExpression(Expression expression, Type typeOperand, ExpressionType nodeKind) {
            _expression = expression;
            _typeOperand = typeOperand;
            _nodeKind = nodeKind;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type TypeImpl() {
            return typeof(bool);
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType NodeTypeImpl() {
            return _nodeKind;
        }

        /// <summary>
        /// Gets the expression operand of a type test operation.
        /// </summary>
        public Expression Expression {
            get { return _expression; }
        }

        /// <summary>
        /// Gets the type operand of a type test operation.
        /// </summary>
        public Type TypeOperand {
            get { return _typeOperand; }
        }

        #region Reduce TypeEqual

        internal Expression ReduceTypeEqual() {
            Type cType = Expression.Type;

            // For value types (including Void, but not nullables), we can
            // determine the result now
            if (cType.IsValueType && !cType.IsNullableType()) {
                return Expression.Block(Expression, Expression.Constant(cType == _typeOperand));
            }

            // Can check the value right now for constants.
            if (Expression.NodeType == ExpressionType.Constant) {
                return ReduceConstantTypeEqual();
            }

            // If the operand type is a sealed reference type or a nullable
            // type, it will match if value is not null
            if (cType.IsSealed && (cType == _typeOperand)) {
                return Expression.NotEqual(Expression, Expression.Constant(null, Expression.Type));
            }

            // expression is a ByVal parameter. Can safely reevaluate.
            var parameter = Expression as ParameterExpression;
            if (parameter != null && !parameter.IsByRef) {
                return ByValParameterTypeEqual(parameter);
            }

            // Create a temp so we only evaluate the left side once
            parameter = Parameter(typeof(object), null);

            // Convert to object if necessary
            var expression = Expression;
            if (!TypeUtils.AreReferenceAssignable(typeof(object), expression.Type)) {
                expression = Expression.Convert(expression, typeof(object));
            }

            return Expression.Block(
                new[] { parameter },
                Expression.Assign(parameter, expression),
                ByValParameterTypeEqual(parameter)
            );
        }

        // helper that is used when re-eval of LHS is safe.
        private Expression ByValParameterTypeEqual(ParameterExpression value) {
            //when comparing the value with null, we don't want to invoke the
            //user overloaded operator if there is one. Instead, we want to do
            //reference equality comparison by converting the value to System.Object.
            return Expression.AndAlso(
                //TODO: If we can generate more optimal IL for reference equality comparison
                //by using conversion, we can replace the following line with 
                //Expression.NotEqual(Expression.Convert(value, typeof(object))
                new LogicalBinaryExpression(ExpressionType.NotEqual, value, Expression.Constant(null)),
                Expression.Equal(
                    Expression.Call(
                        value,
                        typeof(object).GetMethod("GetType")
                    ),
                    Expression.Constant(_typeOperand.GetNonNullableType(), typeof(Type))
                )
            );
        }

        private Expression ReduceConstantTypeEqual() {
            ConstantExpression ce = Expression as ConstantExpression;
            //TypeEqual(null, T) always returns false.
            if (ce.Value == null) {
                return Expression.Constant(false);
            } else {
                return Expression.Constant(_typeOperand.GetNonNullableType() == ce.Value.GetType());
            }
        }

        #endregion

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitTypeBinary(this);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="TypeBinaryExpression"/>.
        /// </summary>
        /// <param name="expression">An <see cref="Expression"/> to set the <see cref="Expression"/> property equal to.</param>
        /// <param name="type">A <see cref="Type"/> to set the <see cref="TypeBinaryExpression.TypeOperand"/> property equal to.</param>
        /// <returns>A <see cref="TypeBinaryExpression"/> for which the <see cref="NodeType"/> property is equal to <see cref="TypeIs"/> and for which the <see cref="Expression"/> and <see cref="TypeBinaryExpression.TypeOperand"/> properties are set to the specified values.</returns>
        public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeIs);
        }

        /// <summary>
        /// Creates a <see cref="TypeBinaryExpression"/> that compares run-time type identity.
        /// </summary>
        /// <param name="expression">An <see cref="Expression"/> to set the <see cref="Expression"/> property equal to.</param>
        /// <param name="type">A <see cref="Type"/> to set the <see cref="TypeBinaryExpression.TypeOperand"/> property equal to.</param>
        /// <returns>A <see cref="TypeBinaryExpression"/> for which the <see cref="NodeType"/> property is equal to <see cref="TypeEqual"/> and for which the <see cref="Expression"/> and <see cref="TypeBinaryExpression.TypeOperand"/> properties are set to the specified values.</returns>
        public static TypeBinaryExpression TypeEqual(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeEqual);
        }
    }
}
