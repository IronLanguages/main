/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;
using System.Reflection;

namespace Microsoft.Scripting.Ast {
    using AstExpressions = ReadOnlyCollectionBuilder<Expression>;

    public class ExpressionCollectionBuilder<TExpression> : IEnumerable<TExpression>, ICollection<TExpression> {
        public TExpression Expression0 { get; private set; }
        public TExpression Expression1 { get; private set; }
        public TExpression Expression2 { get; private set; }
        public TExpression Expression3 { get; private set; }

        private int _count;
        private ReadOnlyCollectionBuilder<TExpression> _expressions;

        public ExpressionCollectionBuilder() {
        }

        public int Count {
            get { return _count; }
        }

        /// <summary>
        /// If the number of items added to the builder is greater than 4 returns a read-only collection builder containing all the items.
        /// Returns <c>null</c> otherwise.
        /// </summary>
        public ReadOnlyCollectionBuilder<TExpression> Expressions {
            get { return _expressions; }
        }

        public void Add(IEnumerable<TExpression> expressions) {
            if (expressions != null) {
                foreach (var expression in expressions) {
                    Add(expression);
                }
            }
        }

        public void Add(TExpression item) {
            if (item == null) {
                return;
            }

            switch (_count) {
                case 0: Expression0 = item; break;
                case 1: Expression1 = item; break;
                case 2: Expression2 = item; break;
                case 3: Expression3 = item; break;
                case 4:
                    _expressions = new ReadOnlyCollectionBuilder<TExpression> {
                        Expression0,
                        Expression1,
                        Expression2,
                        Expression3,
                        item
                    };
                    break;

                default:
                    Debug.Assert(_expressions != null);
                    _expressions.Add(item);
                    break;
            }

            _count++;
        }

        private IEnumerator<TExpression>/*!*/ GetItemEnumerator() {
            if (_count > 0) {
                yield return Expression0;
            }
            if (_count > 1) {
                yield return Expression1;
            }
            if (_count > 2) {
                yield return Expression2;
            }
            if (_count > 3) {
                yield return Expression3;
            }
        }

        public IEnumerator<TExpression>/*!*/ GetEnumerator() {
            if (_expressions != null) {
                return _expressions.GetEnumerator();
            } else {
                return GetItemEnumerator();
            }
        }

        IEnumerator/*!*/ IEnumerable.GetEnumerator() {
            return CollectionUtils.ToCovariant<TExpression, object>((IEnumerable<TExpression>)this).GetEnumerator();
        }

        #region ICollection<TExpression> Members

        public void Clear() {
            Expression0 = Expression1 = Expression2 = Expression3 = default(TExpression);
            _expressions = null;
            _count = 0;
        }

        public bool Contains(TExpression item) {
            return this.Any((e) => e.Equals(item));
        }

        public void CopyTo(TExpression[] array, int arrayIndex) {
            foreach (var expression in this) {
                array[arrayIndex++] = expression;
            }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(TExpression item) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ExpressionCollectionBuilder : ExpressionCollectionBuilder<Expression> {
        public Expression/*!*/ ToMethodCall(Expression instance, MethodInfo/*!*/ method) {
            switch (Count) {
                case 0: 
                    return Expression.Call(instance, method);

                case 1:
                    // we have no specialized subclass for instance method call expression with 1 arg:
                    return instance != null ? 
                        Expression.Call(instance, method, new AstExpressions { Expression0 }) : 
                        Expression.Call(method, Expression0);

                case 2: 
                    return Expression.Call(instance, method, Expression0, Expression1);

                case 3: 
                    return Expression.Call(instance, method, Expression0, Expression1, Expression2);

                case 4:
                    // we have no specialized subclass for instance method call expression with 4 args:
                    return instance != null ?
                        Expression.Call(instance, method, new AstExpressions { Expression0, Expression1, Expression2, Expression3 }) :
                        Expression.Call(method, Expression0, Expression1, Expression2, Expression3);

                default: 
                    return Expression.Call(instance, method, Expressions);
            }
        }
    }
}