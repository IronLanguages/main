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

using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    public partial class Utils {
        public static Expression DebugMarker(string marker) {
            ContractUtils.RequiresNotNull(marker, "marker");
#if DEBUG
            return CallDebugWriteLine(marker);
#else
            return Expression.Empty();
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "marker")]
        public static Expression DebugMark(Expression expression, string marker) {
            ContractUtils.RequiresNotNull(expression, "expression");
            ContractUtils.RequiresNotNull(marker, "marker");

#if DEBUG
            return Expression.Block(
                CallDebugWriteLine(marker),
                expression
            );
#else
            return expression;
#endif
        }

#if DEBUG
        private static MethodCallExpression CallDebugWriteLine(string marker) {
            return Expression.Call(
                typeof(Debug).GetMethod("WriteLine", new[] { typeof(string) }),
                Expression.Constant(marker)
            );
        }
#endif
    }
}
