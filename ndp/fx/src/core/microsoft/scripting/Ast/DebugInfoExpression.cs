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

        protected override Type GetExpressionType() {
            return _expression.Type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.DebugInfo;
        }

        public override bool CanReduce {
            get {
                return true;
            }
        }

        public int StartLine {
            get { return _startLine; }
        }

        public int StartColumn {
            get { return _startColumn; }
        }

        public int EndLine {
            get { return _endLine; }
        }

        public int EndColumn {
            get { return _endColumn; }
        }

        /// <summary>
        /// Information about the source file
        /// </summary>
        public SymbolDocumentInfo Document {
            get { return _document; }
        }

        /// <summary>
        /// The underlying expression to be evaluated
        /// </summary>
        public Expression Expression {
            get { return _expression; }
        }

        /// <summary>
        /// Returns the underlying expression
        /// </summary>
        public override Expression Reduce() {
            return _expression;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDebugInfo(this);
        }
    }

    public partial class Expression {
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
