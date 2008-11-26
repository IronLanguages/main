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

using System.Scripting.Utils;

namespace System.Linq.Expressions {
    /// <summary>
    /// Represents a labeled statement
    /// break and continue statements will jump to the end of body
    /// </summary>
    public sealed class LabeledStatement : Expression {
        private readonly Expression _expression;
        private readonly LabelTarget _label;

        internal LabeledStatement(Annotations annotations, LabelTarget label, Expression expression)
            : base(ExpressionType.LabeledStatement, typeof(void), annotations) {
            _label = label;
            _expression = expression;
        }

        // TODO: Resolve Label and Expression.Label()
        new public LabelTarget Label {
            get { return _label; }
        }

        public Expression Statement {
            get { return _expression; }
        }

        internal override Expression Accept(ExpressionTreeVisitor visitor) {
            return visitor.VisitLabeled(this);
        }
    }

    public partial class Expression {
        public static LabeledStatement Labeled(LabelTarget label, Expression body) {
            return Labeled(label, body, Annotations.Empty);
        }

        public static LabeledStatement Labeled(LabelTarget label, Expression body, Annotations annotations) {
            ContractUtils.RequiresNotNull(label, "label");
            RequiresCanRead(body, "body");
            return new LabeledStatement(annotations, label, body);
        }
    }
}
