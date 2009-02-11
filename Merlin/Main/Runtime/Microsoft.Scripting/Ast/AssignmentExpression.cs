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
using System.Diagnostics;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Evaluates to the CodeContext that's currently in scope
    /// 
    /// TODO: this should go away as an intrinsic in favor of languages
    ///       tracking their own scope chain explicitly
    /// </summary>
    public sealed class AssignmentExtensionExpression : Expression {
        private readonly Expression _left, _right;

        internal AssignmentExtensionExpression(Expression left, Expression right) {
            Debug.Assert(left is GlobalVariableExpression);

            _left = left;
            _right = right;
        }

        protected override System.Type TypeImpl() {
            //Need to be consistent with AssignBinaryExpression
            return _left.Type;
        }

        protected override ExpressionType NodeTypeImpl() {
            return ExpressionType.Extension;
        }

        public Expression Value {
            get {
                return _right;
            }
        }

        public Expression Expression {
            get {
                return _left;
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            Expression left = visitor.Visit(_left);
            Expression right = visitor.Visit(_right);

            if (left != _left || right != _right) {
                return new AssignmentExtensionExpression(left, right);
            }

            return this;
        }
    }
   
    public static partial class Utils {
        public static Expression Assign(Expression left, Expression right) {
            GlobalVariableExpression assignable = left as GlobalVariableExpression;
            if (assignable != null) {
                return new AssignmentExtensionExpression(left, right);
            }

            return Expression.Assign(left, right);
        }
    }
}
