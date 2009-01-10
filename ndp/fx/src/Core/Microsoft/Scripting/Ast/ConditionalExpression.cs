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
using System.Text;

namespace System.Linq.Expressions {
    //CONFORMING
    /// <summary>
    /// Represents an expression that has a conditional operator.
    /// </summary>
    public class ConditionalExpression : Expression {
        private readonly Expression _test;
        private readonly Expression _true;

        internal ConditionalExpression(Expression test, Expression ifTrue) {
            _test = test;
            _true = ifTrue;
        }

        internal static ConditionalExpression Make(Expression test, Expression ifTrue, Expression ifFalse) {
            if (ifFalse == DefaultExpression.VoidInstance) {
                return new ConditionalExpression(test, ifTrue);
            } else {
                return new FullConditionalExpression(test, ifTrue, ifFalse);
            }
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Conditional;
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            return IfTrue.Type;
        }

        /// <summary>
        /// Gets the test of the conditional operation.
        /// </summary>
        public Expression Test {
            get { return _test; }
        }
        /// <summary>
        /// Gets the expression to execute if the test evaluates to true.
        /// </summary>
        public Expression IfTrue {
            get { return _true; }
        }
        /// <summary>
        /// Gets the expression to execute if the test evaluates to false.
        /// </summary>
        public Expression IfFalse {
            get { return GetFalse(); }
        }

        internal virtual Expression GetFalse() {
            return DefaultExpression.VoidInstance;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitConditional(this);
        }
    }

    internal class FullConditionalExpression : ConditionalExpression {
        private readonly Expression _false;

        internal FullConditionalExpression(Expression test, Expression ifTrue, Expression ifFalse)
            : base(test, ifTrue) {
            _false = ifFalse;
        }

        internal override Expression GetFalse() {
            return _false;
        }
    }

    public partial class Expression {
        //CONFORMING
        /// <summary>
        /// Creates a <see cref="ConditionalExpression"/>.
        /// </summary>
        /// <param name="test">An <see cref="Expression"/> to set the <see cref="P:ConditionalExpression.Test"/> property equal to.</param>
        /// <param name="ifTrue">An <see cref="Expression"/> to set the <see cref="P:ConditionalExpression.IfTrue"/> property equal to.</param>
        /// <param name="ifFalse">An <see cref="Expression"/> to set the <see cref="P:ConditionalExpression.IfFalse"/> property equal to.</param>
        /// <returns>A <see cref="ConditionalExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Conditional"/> and the <see cref="P:ConditionalExpression.Test"/>, <see cref="P:ConditionalExpression.IfTrue"/>, 
        /// and <see cref="P:ConditionalExpression.IfFalse"/> properties set to the specified values.</returns>
        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse) {
            RequiresCanRead(test, "test");
            RequiresCanRead(ifTrue, "ifTrue");
            RequiresCanRead(ifFalse, "ifFalse");

            if (test.Type != typeof(bool)) {
                throw Error.ArgumentMustBeBoolean();
            }
            if (ifTrue.Type != ifFalse.Type) {
                throw Error.ArgumentTypesMustMatch();
            }

            return ConditionalExpression.Make(test, ifTrue, ifFalse);
        }
    }
}
