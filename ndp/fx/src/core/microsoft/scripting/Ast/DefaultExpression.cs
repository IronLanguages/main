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
    // Represents default(T) in the tree
    public sealed class DefaultExpression : Expression {
        internal static readonly DefaultExpression VoidInstance = new DefaultExpression(typeof(void));

        private readonly Type _type;

        internal DefaultExpression(Type type) {
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }

        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Default;
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitDefault(this);
        }
    }

    public partial class Expression {
        public static DefaultExpression Empty() {
            return DefaultExpression.VoidInstance;
        }

        public static DefaultExpression Default(Type type) {
            if (type == typeof(void)) {
                return Empty();
            }
            return new DefaultExpression(type);
        }
    }
}
