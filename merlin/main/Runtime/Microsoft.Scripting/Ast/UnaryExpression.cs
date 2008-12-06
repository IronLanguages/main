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
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public static partial class Utils {
        public static Expression Convert(Expression expression, Type type) {
            ContractUtils.RequiresNotNull(expression, "expression");

            if (expression.Type == type) {
                return expression;
            }

            if (expression.Type == typeof(void)) {
                return Expression.Block(expression, Expression.Default(type));
            }

            return Expression.Convert(expression, type);
        }
    }
}
