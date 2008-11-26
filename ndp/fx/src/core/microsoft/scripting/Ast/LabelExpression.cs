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
    /// <summary>
    /// Represents a label, which can be placed in any Expression context. If
    /// it is jumped to, it will get the value provided by the corresponding
    /// GotoExpression. Otherwise, it gets the value in DefaultValue. If the
    /// Type equals System.Void, no value should be provided
    /// </summary>
    public sealed class LabelExpression : Expression {
        private readonly Expression _defaultValue;
        private readonly LabelTarget _label;

        internal LabelExpression(LabelTarget label, Expression defaultValue) {
            _label = label;
            _defaultValue = defaultValue;
        }

        protected override Type GetExpressionType() {
            return _label.Type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Label;
        }

        new public LabelTarget Label {
            get { return _label; }
        }

        /// <summary>
        /// The value of the LabelExpression when the label is reached through
        /// normal control flow (e.g. is not jumped to)
        /// </summary>
        public Expression DefaultValue {
            get { return _defaultValue; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitLabel(this);
        }
    }

    public partial class Expression {
        public static LabelExpression Label(LabelTarget target) {
            return Label(target, null);
        }
        public static LabelExpression Label(LabelTarget target, Expression defaultValue) {
            ValidateGoto(target, ref defaultValue, "label", "defaultValue");
            return new LabelExpression(target, defaultValue);
        }
    }
}
