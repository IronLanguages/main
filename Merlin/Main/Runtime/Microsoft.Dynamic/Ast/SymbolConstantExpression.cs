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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Reflection;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Ast {

    /// <summary>
    /// Represents a SymbolId constant
    /// This node is reducible, and also rewritten by GlobalOptimizedRewriter
    /// 
    /// TODO: this node exists so GlobalOptimizedRewriter can recognize and
    /// rewrite a strongly typed node. Once that functionality is gone it
    /// should go away.
    /// When this type goes away, change the return type of 
    /// Microsoft.Scripting.Ast.Utils.Constant(object) from Expression to ConstantExpression.
    /// </summary>
    internal sealed class SymbolConstantExpression : Expression {
        private readonly SymbolId _value;

        internal SymbolConstantExpression(SymbolId value) {

            _value = value;
        }

        public override bool CanReduce {
            get { return true; }
        }

        public sealed override Type Type {
            get { return typeof(SymbolId); }
        }

        public sealed override ExpressionType NodeType {
            get { return ExpressionType.Extension; }
        }
        
        public SymbolId Value {
            get { return _value; }
        }

        private static readonly Expression _SymbolIdEmpty = Expression.Field(null, typeof(SymbolId).GetField("Empty"));
        private static readonly Expression _SymbolIdInvalid = Expression.Field(null, typeof(SymbolId).GetField("Invalid"));
        private static readonly ConstructorInfo _SymbolIdCtor = typeof(SymbolId).GetConstructor(new[] { typeof(int) });

        public override Expression Reduce() {
            return GetExpression(_value);
        }

        internal static Expression GetExpression(SymbolId value) {
            if (value == SymbolId.Empty) {
                return _SymbolIdEmpty;
            } else if (value == SymbolId.Invalid) {
                return _SymbolIdInvalid;
            } else {
                return Expression.New(_SymbolIdCtor, AstUtils.Constant(value.Id));
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) {
            return this;
        }
    }
}
