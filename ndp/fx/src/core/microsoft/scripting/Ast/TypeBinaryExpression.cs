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

namespace System.Linq.Expressions {
    //CONFORMING
    public sealed class TypeBinaryExpression : Expression {
        private readonly Expression _expression;
        private readonly Type _typeOperand;
        private readonly ExpressionType _nodeKind;

        internal TypeBinaryExpression(Expression expression, Type typeOperand, ExpressionType nodeKind){
            _expression = expression;
            _typeOperand = typeOperand;
            _nodeKind = nodeKind;
        }

        protected override Type GetExpressionType() {
            return typeof(bool);
        }

        protected override ExpressionType GetNodeKind() {
            return _nodeKind;
        }

        public Expression Expression {
            get { return _expression; }
        }

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
            return Expression.Block(
                new[] { parameter },
                Expression.Assign(parameter, Helpers.Convert(Expression, typeof(object))),
                ByValParameterTypeEqual(parameter)
            );
        }

        // helper that is used when re-eval of LHS is safe.
        private Expression ByValParameterTypeEqual(ParameterExpression value) {
            return Expression.AndAlso(
                Expression.NotEqual(value, Expression.Constant(null)),
                Expression.Equal(
                    Expression.Call(
                        value,
                        typeof(object).GetMethod("GetType")
                    ),
                    Expression.Constant(_typeOperand)
                )
            );
        }

        private Expression ReduceConstantTypeEqual() {
            ConstantExpression ce = Expression as ConstantExpression;
            //TypeEqual(null, T) always returns false.
            if (ce.Value == null) {
                return Expression.Constant(false);
            } else if (_typeOperand.IsNullableType()) {
                return Expression.Constant(_typeOperand == ce.Type);
            } else {
                return Expression.Constant(_typeOperand == ce.Value.GetType());
            }
        }

        #endregion

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitTypeBinary(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// </summary>
    public partial class Expression {
        //CONFORMING
        public static TypeBinaryExpression TypeIs(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeIs);
        }

        /// <summary>
        /// Creates an Expression that compares run-time type identity. It is
        /// roughly equivalent to a tree that does this:
        ///     obj != null &amp;&amp; obj.GetType() == type
        ///     
        /// If you want to check for "null" use Expression.Equal
        /// </summary>
        /// <param name="expression">The operand.</param>
        /// <param name="type">The type to check for at run-time.</param>
        /// <returns>A new Expression that performs a type equality check.</returns>
        public static TypeBinaryExpression TypeEqual(Expression expression, Type type) {
            RequiresCanRead(expression, "expression");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.Requires(!type.IsByRef, "type", Strings.TypeMustNotBeByRef);

            return new TypeBinaryExpression(expression, type, ExpressionType.TypeEqual);
        }
    }
}
