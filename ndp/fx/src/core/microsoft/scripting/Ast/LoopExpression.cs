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

using System.Dynamic.Utils;

namespace System.Linq.Expressions {
    public sealed class LoopExpression : Expression {
        private readonly Expression _body;
        private readonly LabelTarget _break;
        private readonly LabelTarget _continue;

        /// <summary>
        /// Null test means infinite loop.
        /// </summary>
        internal LoopExpression(Expression body, LabelTarget @break, LabelTarget @continue) {
            _body = body;
            _break = @break;
            _continue = @continue;
        }

        protected override Type GetExpressionType() {
            return typeof(void);
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Loop;
        }

        public Expression Body {
            get { return _body; }
        }

        public LabelTarget BreakLabel {
            get { return _break; }
        }

        public LabelTarget ContinueLabel {
            get { return _continue; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitLoop(this);
        }
    }

    /// <summary>
    /// Factory methods.
    /// TODO: review which of these overloads we actually need
    /// </summary>
    public partial class Expression {
        public static LoopExpression Loop(Expression body) {
            return Loop(body, null);
        }

        public static LoopExpression Loop(Expression body, LabelTarget @break) {
            return Loop(body, @break, null);
        }

        public static LoopExpression Loop(Expression body, LabelTarget @break, LabelTarget @continue) {
            RequiresCanRead(body, "body");
            // TODO: lift the restriction on break, and allow loops to have non-void type
            ContractUtils.Requires(@break == null || @break.Type == typeof(void), "break", Strings.LabelTypeMustBeVoid);
            ContractUtils.Requires(@continue == null || @continue.Type == typeof(void), "continue", Strings.LabelTypeMustBeVoid);
            return new LoopExpression(body, @break, @continue);
        }
    }
}
