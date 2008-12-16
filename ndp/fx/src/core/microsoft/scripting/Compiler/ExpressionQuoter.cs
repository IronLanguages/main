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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;
using System.Dynamic.Utils;

namespace System.Runtime.CompilerServices {
    public partial class RuntimeOps {
        [Obsolete("do not call this method", true)]
        public static Expression Quote(Expression expression, object hoistedLocals, object[] locals) {
            Debug.Assert(hoistedLocals != null && locals != null);
            var quoter = new ExpressionQuoter((HoistedLocals)hoistedLocals, locals);
            return quoter.Visit(expression);
        }

        [Obsolete("do not call this method", true)]
        public static IList<IStrongBox> MergeRuntimeVariables(IList<IStrongBox> first, IList<IStrongBox> second, int[] indexes) {
            return new MergedRuntimeVariables(first, second, indexes);
        }

        // Modifies a quoted Expression instance by changing hoisted variables and
        // parameters into hoisted local references. The variable's StrongBox is
        // burned as a constant, and all hoisted variables/parameters are rewritten
        // as indexing expressions.
        //
        // The behavior of Quote is indended to be like C# and VB expression quoting
        private sealed class ExpressionQuoter : ExpressionVisitor {
            private readonly HoistedLocals _scope;
            private readonly object[] _locals;

            // A stack of variables that are defined in nested scopes. We search
            // this first when resolving a variable in case a nested scope shadows
            // one of our variable instances.
            private readonly Stack<Set<ParameterExpression>> _hiddenVars = new Stack<Set<ParameterExpression>>();

            internal ExpressionQuoter(HoistedLocals scope, object[] locals) {
                _scope = scope;
                _locals = locals;
            }

            protected internal override Expression VisitLambda<T>(Expression<T> node) {
                _hiddenVars.Push(new Set<ParameterExpression>(node.Parameters));
                Expression b = Visit(node.Body);
                _hiddenVars.Pop();
                if (b == node.Body) {
                    return node;
                }
                return Expression.Lambda<T>(b, node.Name, node.Parameters);
            }

            protected internal override Expression VisitBlock(BlockExpression node) {
                _hiddenVars.Push(new Set<ParameterExpression>(node.Variables));
                var b = Visit(node.Expressions);
                _hiddenVars.Pop();
                if (b == node.Expressions) {
                    return node;
                }
                return Expression.Block(node.Variables, b);
            }

            protected override CatchBlock VisitCatchBlock(CatchBlock node) {
                _hiddenVars.Push(new Set<ParameterExpression>(new[] { node.Variable }));
                Expression b = Visit(node.Body);
                Expression f = Visit(node.Filter);
                _hiddenVars.Pop();
                if (b == node.Body && f == node.Filter) {
                    return node;
                }
                return Expression.MakeCatchBlock(node.Test, node.Variable, b, f);
            }

            protected internal override Expression VisitRuntimeVariables(RuntimeVariablesExpression node) {
                int count = node.Variables.Count;
                var boxes = new List<IStrongBox>();
                var vars = new List<ParameterExpression>();
                var indexes = new int[count];
                for (int i = 0; i < count; i++) {
                    IStrongBox box = GetBox(node.Variables[i]);
                    if (box == null) {
                        indexes[i] = vars.Count;
                        vars.Add(node.Variables[i]);
                    } else {
                        indexes[i] = -1 - boxes.Count;
                        boxes.Add(box);
                    }
                }

                // No variables were rewritten. Just return the original node
                if (boxes.Count == 0) {
                    return node;
                }

                var boxesConst = Expression.Constant(new ReadOnlyCollection<IStrongBox>(boxes.ToArray()));
                // All of them were rewritten. Just return the array as a constant
                if (vars.Count == 0) {
                    return boxesConst;
                }

                // Otherwise, we need to return an object that merges them
                return Expression.Call(
                    typeof(RuntimeOps).GetMethod("MergeRuntimeVariables"),
                    Expression.RuntimeVariables(new ReadOnlyCollection<ParameterExpression>(vars.ToArray())),
                    boxesConst,
                    Expression.Constant(indexes)
                );
            }

            protected internal override Expression VisitParameter(ParameterExpression node) {
                IStrongBox box = GetBox(node);
                if (box == null) {
                    return node;
                }
                return Expression.Field(Expression.Constant(box), "Value");
            }

            private IStrongBox GetBox(ParameterExpression variable) {
                // Skip variables that are shadowed by a nested scope/lambda
                foreach (Set<ParameterExpression> hidden in _hiddenVars) {
                    if (hidden.Contains(variable)) {
                        return null;
                    }
                }

                HoistedLocals scope = _scope;
                object[] locals = _locals;
                while (true) {
                    int hoistIndex;
                    if (scope.Indexes.TryGetValue(variable, out hoistIndex)) {
                        return (IStrongBox)locals[hoistIndex];
                    }
                    scope = scope.Parent;
                    if (scope == null) {
                        break;
                    }
                    locals = HoistedLocals.GetParent(locals);
                }

                // Unbound variable: return null, so the original node is preserved
                return null;
            }
        }


        /// <summary>
        /// Provides a list of variables, supporing read/write of the values
        /// Exposed via RuntimeVariablesExpression
        /// </summary>
        private sealed class MergedRuntimeVariables : IList<IStrongBox> {
            private readonly IList<IStrongBox> _first;
            private readonly IList<IStrongBox> _second;

            // For reach item, the index into the first or second list
            // Positive values mean the first array, negative means the second
            private readonly int[] _indexes;

            internal MergedRuntimeVariables(IList<IStrongBox> first, IList<IStrongBox> second, int[] indexes) {
                _first = first;
                _second = second;
                _indexes = indexes;
            }

            public int Count {
                get { return _indexes.Length; }
            }

            public IStrongBox this[int index] {
                get {
                    index = _indexes[index];
                    return (index >= 0) ? _first[index] : _second[-1 - index];
                }
                set {
                    throw Error.CollectionReadOnly();
                }
            }

            public int IndexOf(IStrongBox item) {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    if (this[i] == item) {
                        return i;
                    }
                }
                return -1;
            }

            public bool Contains(IStrongBox item) {
                return IndexOf(item) >= 0;
            }

            public void CopyTo(IStrongBox[] array, int arrayIndex) {
                ContractUtils.RequiresNotNull(array, "array");
                int count = _indexes.Length;
                if (arrayIndex < 0 || arrayIndex + count > array.Length) {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                for (int i = 0; i < count; i++) {
                    array[arrayIndex++] = this[i];
                }
            }

            bool ICollection<IStrongBox>.IsReadOnly {
                get { return true; }
            }

            public IEnumerator<IStrongBox> GetEnumerator() {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    yield return this[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            void IList<IStrongBox>.Insert(int index, IStrongBox item) {
                throw Error.CollectionReadOnly();
            }

            void IList<IStrongBox>.RemoveAt(int index) {
                throw Error.CollectionReadOnly();
            }

            void ICollection<IStrongBox>.Add(IStrongBox item) {
                throw Error.CollectionReadOnly();
            }

            void ICollection<IStrongBox>.Clear() {
                throw Error.CollectionReadOnly();
            }

            bool ICollection<IStrongBox>.Remove(IStrongBox item) {
                throw Error.CollectionReadOnly();
            }
        }
    }
}
