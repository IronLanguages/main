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

using System.Collections;
using System.Collections.Generic;
using Microsoft.Contracts;
using System.Linq.Expressions;

namespace System.Dynamic.Utils {

    /// <summary>
    /// A stack implemented as a list. Allows both Push/Pop access and indexing into any member of the list.
    /// </summary>
    internal sealed class ListStack<T> : IEnumerable<T> {
        private readonly List<T> _list;
        private int _version;

        public ListStack() {
            _list = new List<T>();
        }

        public ListStack(int capacity) {
            _list = new List<T>(capacity);
        }

        public ListStack(IEnumerable<T> collection) {
            _list = new List<T>(collection);
        }

        public T this[int index] {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        /// <exception cref="InvalidOperationException">Stack is empty.</exception>
        public T Peek() {
            if (_list.Count == 0) throw new InvalidOperationException();
            return _list[_list.Count - 1];
        }

        /// <exception cref="InvalidOperationException">Stack is empty.</exception>
        public T Pop() {
            if (_list.Count == 0) throw new InvalidOperationException();
            T result = _list[_list.Count - 1];
            _version++;
            _list.RemoveAt(_list.Count - 1);
            return result;
        }

        public bool Contains(T t) {
            return _list.Contains(t);
        }

        public void Clear() {
            _version++;
            _list.Clear();
        }

        public void Push(T item) {
            _version++;
            _list.Add(item);
        }

        public int Count {
            get { return _list.Count; }
        }

        /// <summary>
        /// Enumerates from the top of the stack to the bottom.
        /// </summary>
        /// <exception cref="InvalidOperationException">Stack has been modified during enumeration.</exception>
        [Pure]
        public IEnumerator<T> GetEnumerator() {
            int version = _version;
            for (int i = _list.Count - 1; i >= 0; i--) {
                yield return _list[i];
                if (_version != version) {
                    throw Error.StackChangedWhileEnumerationg();
                }
            }
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
