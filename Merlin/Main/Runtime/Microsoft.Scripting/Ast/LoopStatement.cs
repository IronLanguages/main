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
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Ast {
    /// <summary>
    /// Factory methods.
    /// </summary>
    public static partial class Utils {
        public static LoopExpression While(Expression test, Expression body, Expression @else) {
            return Loop(test, null, body, @else, null, null);
        }

        public static LoopExpression While(Expression test, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue) {
            return Loop(test, null, body, @else, @break, @continue);
        }

        [Obsolete("use While overload without SourceSpan")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "header")]
        public static LoopExpression While(Expression test, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, SourceLocation header, SourceSpan span) {
            return Loop(test, null, body, @else, @break, @continue);
        }

        public static LoopExpression Infinite(Expression body) {
            return Expression.Loop(body, null, null);
        }

        public static LoopExpression Infinite(Expression body, LabelTarget @break, LabelTarget @continue) {
            return Expression.Loop(body, @break, @continue);
        }

        public static LoopExpression Loop(Expression test, Expression increment, Expression body, Expression @else) {
            return Loop(test, increment, body, @else, null, null);
        }

        [Obsolete("use Loop overload without SourceSpan")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "span")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "header")]
        public static LoopExpression Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue, SourceLocation header, SourceSpan span) {
            return Loop(test, increment, body, @else, @break, @continue);
        }

        public static LoopExpression Loop(Expression test, Expression increment, Expression body, Expression @else, LabelTarget @break, LabelTarget @continue) {
            ContractUtils.RequiresNotNull(body, "body");
            if (test != null) {
                ContractUtils.Requires(test.Type == typeof(bool), "test", "Test must be boolean");
                if (@break == null) {
                    @break = Expression.Label();
                }
            }

            // for (;;) {
            //     if (test) {
            //     } else {
            //        else;
            //        break;
            //     }
            //     Body
            // continue:
            //     Increment;
            // }

            // If there is no test, 'else' will never execute and gets simply thrown away.
            return Expression.Loop(
                Expression.Block(
                    test != null
                        ? (Expression)Expression.Condition(
                            test,
                            Expression.Empty(),
                            Expression.Block(
                                @else != null ? @else : Expression.Empty(),
                                Expression.Break(@break)
                            )
                        )
                        : Expression.Empty(),
                    body,
                    @continue != null ? (Expression)Expression.Label(@continue) : Expression.Empty(),
                    increment != null ? increment : Expression.Empty()
                ),
                @break,
                null
            );
        }
    }
}
