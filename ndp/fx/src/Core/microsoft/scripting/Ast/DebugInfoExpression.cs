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
using System.Collections.Generic;
using System.Text;
using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Wraps an expression, emitting a sequence point around it
    /// 
    /// This allows the debugger to highlight the correct source code when
    /// debugging.
    /// </summary>
    public sealed class DebugInfoExpression : Expression {
        private readonly Expression _expression;
        private readonly int _startLine, _startColumn, _endLine, _endColumn;
        private readonly SymbolDocumentInfo _document;

        internal DebugInfoExpression(Expression body, SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn) {
            _expression = body;
            _document = document;
            _startLine = startLine;
            _startColumn = startColumn;
            _endLine = endLine;
            _endColumn = endColumn;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents. (Inherited from <see cref="Expression"/>.)
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            return _expression.Type;
        }

        /// <summary>
        /// Returns the node type of this <see cref="Expression" />. (Inherited from <see cref="Expression" />.)
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> that represents this expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.DebugInfo;
        }

        /// <summary>
        /// Gets a boolean value indicating whether this expression is reducible.
        /// </summary>
        public override bool CanReduce {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the start line of the code that was used to generate the wrapped expression.
        /// </summary>
        public int StartLine {
            get { return _startLine; }
        }

        /// <summary>
        /// Gets the start column of the code that was used to generate the wrapped expression.
        /// </summary>
        public int StartColumn {
            get { return _startColumn; }
        }

        /// <summary>
        /// Gets the end line of the code that was used to generate the wrapped expression.
        /// </summary>
        public int EndLine {
            get { return _endLine; }
        }

        /// <summary>
        /// Gets the end column of the code that was used to generate the wrapped expression.
        /// </summary>
        public int EndColumn {
            get { return _endColumn; }
        }

        /// <summary>
        /// Gets the <see cref="SymbolDocumentInfo"/> that represents the source file.
        /// </summary>
        public SymbolDocumentInfo Document {
            get { return _document; }
        }

        /// <summary>
        /// The underlying <see cref="Expression"/> that the <see cref="DebugInfoExpression"/> applies to.
        /// </summary>
        public Expression Expression {
            get { return _expression; }
        }

        /// <summary>
        /// Returns the underlying expression that this <see cref="DebugInfoExpression"/> applies to.
        /// </summary>
        /// <returns>The reduced expression.</returns>
        public override Expression Reduce() {
            return _expression;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDebugInfo(this);
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="DebugInfoExpression"/> That identifies the source code that was used to generate an <see cref="Expression"/>.
        /// </summary>
        /// <param name="body">The <see cref="Expression"/> that this <see cref="DebugInfoExpression"/> applies to.</param>
        /// <param name="document">The <see cref="SymbolDocumentInfo"/> that represents the source file.</param>
        /// <param name="startLine">The start line of the code that was used to generate the wrapped expression. Must be greater than 0.</param>
        /// <param name="startColumn">The start column of the code that was used to generate the wrapped expression. Must be greater than 0.</param>
        /// <param name="endLine">The end line of the code that was used to generate the wrapped expression. Must be greater or equal than the start line.</param>
        /// <param name="endColumn">The end column of the code that was used to generate the wrapped expression. If the end line is the same as the start line, it must be greater or equal than the start column. In any case, must be greater than 0.</param>
        /// <returns>An instance of <see cref="DebugInfoExpression"/>.</returns>
        public static DebugInfoExpression DebugInfo(Expression body, SymbolDocumentInfo document, int startLine, int startColumn, int endLine, int endColumn) {
            ContractUtils.RequiresNotNull(body, "body");
            ContractUtils.RequiresNotNull(document, "document");

            ValidateSpan(startLine, startColumn, endLine, endColumn);
            
            return new DebugInfoExpression(body, document, startLine, startColumn, endLine, endColumn);
        }

        private static void ValidateSpan(int startLine, int startColumn, int endLine, int endColumn) {
            if (startLine < 1) {
                throw Error.OutOfRange("startLine", 1);
            }
            if (startColumn < 1) {
                throw Error.OutOfRange("startColumn", 1);
            }
            if (endLine < 1) {
                throw Error.OutOfRange("endLine", 1);
            }
            if (endColumn < 1) {
                throw Error.OutOfRange("endColumn", 1);
            }
            if (startLine > endLine) {
                throw Error.StartEndMustBeOrdered();
            }
            if (startLine == endLine && startColumn > endColumn) {
                throw Error.StartEndMustBeOrdered();
            }
        }
    }
}
