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
using System.Dynamic;

namespace System.Linq.Expressions {
    //CONFORMING
    /// <summary>
    /// Represents an expression that has a constant value.
    /// </summary>
    public class ConstantExpression : Expression {
        internal static readonly ConstantExpression TrueLiteral = ConstantExpression.Make(true, typeof(bool));
        internal static readonly ConstantExpression FalseLiteral = ConstantExpression.Make(false, typeof(bool));
        internal static readonly ConstantExpression NullLiteral = ConstantExpression.Make(null, typeof(object));
        internal static readonly ConstantExpression EmptyStringLiteral = ConstantExpression.Make(String.Empty, typeof(string));
        internal static readonly ConstantExpression[] IntCache = new ConstantExpression[100];

        // Possible optimization: we could have a Constant<T> subclass that
        // stores the unboxed value.
        private readonly object _value;

        internal ConstantExpression(object value) {
            _value = value;
        }

        internal static ConstantExpression Make(object value, Type type) {
            if ((value == null && type == typeof(object)) || (value != null && value.GetType() == type)) {
                return new ConstantExpression(value);
            } else {
                return new TypedConstantExpression(value, type);
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <returns>The <see cref="Type"/> that represents the static type of the expression.</returns>
        protected override Type GetExpressionType() {
            if(_value == null) {
                return typeof(object);
            }
            return _value.GetType();
        }

        /// <summary>
        /// Returns the node type of this Expression. Extension nodes should return
        /// ExpressionType.Extension when overriding this method.
        /// </summary>
        /// <returns>The <see cref="ExpressionType"/> of the expression.</returns>
        protected override ExpressionType GetNodeKind() {
            return ExpressionType.Constant;
        }
        /// <summary>
        /// Gets the value of the constant expression.
        /// </summary>
        public object Value {
            get { return _value; }
        }

        internal override Expression Accept(ExpressionVisitor visitor) {
            return visitor.VisitConstant(this);
        }
    }

    internal class TypedConstantExpression : ConstantExpression {
        private readonly Type _type;

        internal TypedConstantExpression(object value, Type type)
            : base(value) {
            _type = type;
        }

        protected override Type GetExpressionType() {
            return _type;
        }
    }

    public partial class Expression {
        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> that has the <see cref="P:ConstantExpression.Value"/> property set to the specified boolean value. .
        /// </summary>
        /// <param name="value">An <see cref="System.Boolean"/> to set the <see cref="P:ConstantExpression.Value"/> property equal to.</param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Constant"/> and the <see cref="P:ConstantExpression.Value"/> property set to the specified value.
        /// </returns>
        public static ConstantExpression Constant(bool value) {
             return value ? ConstantExpression.TrueLiteral : ConstantExpression.FalseLiteral;
        }
        
        //CONFORMING
        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> that has the <see cref="P:ConstantExpression.Value"/> property set to the specified value. .
        /// </summary>
        /// <param name="value">An <see cref="System.Object"/> to set the <see cref="P:ConstantExpression.Value"/> property equal to.</param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Constant"/> and the <see cref="P:Expression.Value"/> property set to the specified value.
        /// </returns>
        public static ConstantExpression Constant(object value) {
            if (value == null) {
                return ConstantExpression.NullLiteral;
            }

            Type t = value.GetType();
            if (!t.IsEnum) {
                switch (Type.GetTypeCode(t)) {
                    case TypeCode.Boolean:
                        return Constant((bool)value);
                    case TypeCode.Int32:
                        int x = (int)value;
                        int cacheIndex = x + 2;
                        if (cacheIndex >= 0 && cacheIndex < ConstantExpression.IntCache.Length) {
                            ConstantExpression res;
                            if ((res = ConstantExpression.IntCache[cacheIndex]) == null) {
                                ConstantExpression.IntCache[cacheIndex] = res = ConstantExpression.Make(x, typeof(int));
                            }
                            return res;
                        }
                        break;
                    case TypeCode.String:
                        if (String.IsNullOrEmpty((string)value)) {
                            return ConstantExpression.EmptyStringLiteral;
                        }
                        break;
                }
            }

            return ConstantExpression.Make(value, value == null ? typeof(object) : value.GetType());
        }

        //CONFORMING
        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> that has the <see cref="P:ConstantExpression.Value"/> 
        /// and <see cref="P:ConstantExpression.Type"/> properties set to the specified values. .
        /// </summary>
        /// <param name="value">An <see cref="System.Object"/> to set the <see cref="P:ConstantExpression.Value"/> property equal to.</param>
        /// <param name="type">A <see cref="System.Type"/> to set the <see cref="P:Expression.Type"/> property equal to.</param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> that has the <see cref="P:Expression.NodeType"/> property equal to 
        /// <see cref="F:ExpressionType.Constant"/> and the <see cref="P:ConstantExpression.Value"/> and 
        /// <see cref="P:Expression.Type"/> properties set to the specified values.
        /// </returns>
        public static ConstantExpression Constant(object value, Type type) {
            ContractUtils.RequiresNotNull(type, "type");
            if (value == null && type.IsValueType && !TypeUtils.IsNullableType(type)) {
                throw Error.ArgumentTypesMustMatch();
            }
            if (value != null && !type.IsAssignableFrom(value.GetType())) {
                throw Error.ArgumentTypesMustMatch();
            }
            return ConstantExpression.Make(value, type);
        }
    }
}
