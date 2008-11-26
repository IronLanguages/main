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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Interpretation {

    /// <summary>
    /// An ILocalVariables implementation for the interpreter
    /// 
    /// TODO: This isn't quite correct, because it doesn't implement the
    /// LocalScopeExpression.IsClosure feature that only exposes variables that
    /// would otherwise be lifted. To implement it correctly would require a
    /// full variable binding pass, something the interpreter doesn't need
    /// today. The only thing that this breaks is Python's func_closure
    /// </summary>
    internal sealed class InterpreterVariables : IList<IStrongBox> {

        // TODO: InterpreterState should store values in strongly typed
        // StrongBox<T>, which gives us the correct cast error if the wrong
        // type is set at runtime.
        private sealed class InterpreterBox : IStrongBox {
            private readonly InterpreterState _state;
            private readonly Expression _variable;

            internal InterpreterBox(InterpreterState state, Expression variable) {
                _state = state;
                _variable = variable;
            }

            public object Value {
                get { return _state.GetValue(_variable); }
                set { _state.SetValue(_variable, value); }
            }
        }

        private readonly InterpreterState _state;
        private readonly ReadOnlyCollection<ParameterExpression> _vars;

        internal InterpreterVariables(InterpreterState state, RuntimeVariablesExpression node) {
            _state = state;
            _vars = node.Variables;
        }

        public int Count {
            get { return _vars.Count; }
        }

        public IStrongBox this[int index] {
            get {
                return new InterpreterBox(_state, _vars[index]);
            }
            set {
                throw CollectionReadOnly();
            }
        }

        public int IndexOf(IStrongBox item) {
            for (int i = 0, n = _vars.Count; i < n; i++) {
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
            int count = _vars.Count;
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
            for (int i = 0, n = _vars.Count; i < n; i++) {
                yield return this[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void IList<IStrongBox>.Insert(int index, IStrongBox item) {
            throw CollectionReadOnly();
        }

        void IList<IStrongBox>.RemoveAt(int index) {
            throw CollectionReadOnly();
        }

        void ICollection<IStrongBox>.Add(IStrongBox item) {
            throw CollectionReadOnly();
        }

        void ICollection<IStrongBox>.Clear() {
            throw CollectionReadOnly();
        }

        bool ICollection<IStrongBox>.Remove(IStrongBox item) {
            throw CollectionReadOnly();
        }

        private static Exception CollectionReadOnly() {
            throw new NotSupportedException("Collection is read-only.");
        }
    }
}
