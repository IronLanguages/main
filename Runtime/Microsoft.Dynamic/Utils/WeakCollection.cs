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

using System;
using System.Collections.Generic;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Simple class for tracking a list of items and enumerating over them.
    /// The items are stored in weak references; if the objects are collected,
    /// they will not be seen when enumerating.
    /// </summary>
    /// <typeparam name="T">The type of the collection element.</typeparam>
    internal sealed class WeakCollection<T> : IEnumerable<T> where T : class {
        private const int DefaultCapacity = 4; // default capacity of List<T>

        private WeakReference[] _items = new WeakReference[DefaultCapacity];
        private int _size;

        public void Add(T t) {
            EnsureCapacity(_size + 1);
            _items[_size++] = new WeakReference(t);
        }

        private void EnsureCapacity(int size) {
            if (size > _items.Length) {
                // Clear out dead entires first; we might not have to resize it
                Compact();
                if (size > _items.Length) {
                    // Need to expand the list
                    int newSize = _items.Length * 2;
                    if (newSize < size) {
                        newSize = size;
                    }
                    var newList = new WeakReference[newSize];
                    _items.CopyTo(newList, 0);
                    _items = newList;
                }
            }
        }

        private void Compact() {
            int newSize = 0;
            for (int i = 0; i < _size; i++) {
                if (_items[i].IsAlive) {
                    if (newSize < i) {
                        _items[newSize] = _items[i];
                    }
                    newSize++;
                }
            }
            for (int i = newSize; i < _size; i++) {
                _items[i] = null;
            }
            _size = newSize;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _size; i++) {
                T light = (T)_items[i].Target;
                if (light != null) {
                    yield return light;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
