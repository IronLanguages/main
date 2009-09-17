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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System.Diagnostics;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {
    public partial class Utils {
        public static Expression DebugMarker(string marker) {
            ContractUtils.RequiresNotNull(marker, "marker");
#if DEBUG
            return CallDebugWriteLine(marker);
#else
            return Utils.Empty();
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
                AstUtils.Constant(marker)
            );
        }
#endif

        public static Expression AddDebugInfo(Expression expression, SymbolDocumentInfo document, SourceLocation start, SourceLocation end) {
            if (document == null || !start.IsValid || !end.IsValid) {
                return expression;
            }
            return AddDebugInfo(expression, document, start.Line, start.Column, end.Line, end.Column);
        }

        //The following method does not check the validaity of the span
        public static Expression AddDebugInfo(Expression expression, SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn) {
            if (expression == null) {
                throw new System.ArgumentNullException("expression");
            }

            var sequencePoint = Expression.DebugInfo(document,
                startLine, startColumn, endLine, endColumn);

            var clearance = Expression.ClearDebugInfo(document);
            //always attach a clearance
            if (expression.Type == typeof(void)) {
                return Expression.Block(
                    sequencePoint,
                    expression,
                    clearance
                );
            } else {
                //save the expression to a variable
                var p = Expression.Parameter(expression.Type, null);
                return Expression.Block(
                    new[] { p },
                    sequencePoint,
                    Expression.Assign(p, expression),
                    clearance,
                    p
                );
            }
        }

    }
}
