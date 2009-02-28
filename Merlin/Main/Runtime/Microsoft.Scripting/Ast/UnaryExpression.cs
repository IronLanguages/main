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

using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        /// <summary>
        /// Converts an expression to a void type.
        /// </summary>
        /// <param name="expression">An <see cref="Expression"/> to convert to void. </param>
        /// <returns>An <see cref="Expression" /> that has the <see cref="P:System.Linq.Expressions.Expression.NodeType" /> property equal to <see cref="F:System.Linq.Expressions.ExpressionType.ConvertChecked" /> and the <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> and <see cref="P:System.Linq.Expressions.Expression.Type" /> property set to void.</returns>
        public static Expression Void(Expression expression) {
            ContractUtils.RequiresNotNull(expression, "expression");
            if (expression.Type == typeof(void)) {
                return expression;
            }
            return Expression.Block(typeof(void), expression);
        }

        public static Expression Convert(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");

            if (expression.Type == type) {
                return expression;
            }

            if (expression.Type == typeof(void)) {
                return Expression.Block(expression, Utils.Default(type));
            }

            if (type == typeof(void)) {
                return Void(expression);
            }

            // TODO: this is not the right level for this to be at. It should
            // be pushed into languages if they really want this behavior.
            if (type == typeof(object)) {
                if (expression.Type == typeof(int)) {
                    return Expression.Convert(expression, typeof(object), typeof(ScriptingRuntimeHelpers).GetMethod("Int32ToObject"));
                } else if (expression.Type == typeof(bool)) {
                    return Expression.Convert(expression, typeof(object), typeof(ScriptingRuntimeHelpers).GetMethod("BooleanToObject"));
                }
            }

            return Expression.Convert(expression, type);
        }
    }
}
