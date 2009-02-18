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

using System.Dynamic;
using System.Linq.Expressions;

namespace Microsoft.Scripting.Utils {
    public static class DynamicUtils {
        /// <summary>
        /// Returns the list of expressions represented by the <see cref="DynamicMetaObject"/> instances.
        /// </summary>
        /// <param name="objects">An array of <see cref="DynamicMetaObject"/> instances to extract expressions from.</param>
        /// <returns>The array of expressions.</returns>
        public static Expression[] GetExpressions(DynamicMetaObject[] objects) {
            ContractUtils.RequiresNotNull(objects, "objects");

            Expression[] res = new Expression[objects.Length];
            for (int i = 0; i < objects.Length; i++) {
                DynamicMetaObject mo = objects[i];
                res[i] = mo != null ? mo.Expression : null;
            }

            return res;
        }

        /// <summary>
        /// Creates an instance of <see cref="DynamicMetaObject"/> for a runtime value and the expression that represents it during the binding process.
        /// </summary>
        /// <param name="argValue">The runtime value to be represented by the <see cref="DynamicMetaObject"/>.</param>
        /// <param name="parameterExpression">An expression to represent this <see cref="DynamicMetaObject"/> during the binding process.</param>
        /// <returns>The new instance of <see cref="DynamicMetaObject"/>.</returns>
        public static DynamicMetaObject ObjectToMetaObject(object argValue, Expression parameterExpression) {
            IDynamicMetaObjectProvider ido = argValue as IDynamicMetaObjectProvider;
            if (ido != null) {
                return ido.GetMetaObject(parameterExpression);
            } else {
                return new DynamicMetaObject(parameterExpression, BindingRestrictions.Empty, argValue);
            }
        }
    }
}
