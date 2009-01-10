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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace System.Dynamic.Utils {
    /// <summary>
    /// Provides a dictionary-like object used for caches which holds onto a maximum
    /// number of elements specified at construction time.
    /// 
    /// This class is not thread safe.
    /// </summary>
    internal class CacheDict<TKey, TValue> {
        private readonly Dictionary<TKey, KeyInfo> _dict = new Dictionary<TKey, KeyInfo>();
        private readonly LinkedList<TKey> _list = new LinkedList<TKey>();
        private readonly int _maxSize;

        internal CacheDict(int maxSize) {
            _maxSize = maxSize;
        }

        internal bool TryGetValue(TKey key, out TValue value) {
            KeyInfo storedValue;
            if (_dict.TryGetValue(key, out storedValue)) {
                LinkedListNode<TKey> node = storedValue.List;
                if (node.Previous != null) {
                    // move us to the head of the list...
                    _list.Remove(node);
                    _list.AddFirst(node);
                }

                value = storedValue.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        internal void Add(TKey key, TValue value) {
            if (_list.Count == _maxSize) {
                // we've reached capacity, remove the last used element...
                LinkedListNode<TKey> node = _list.Last;
                _list.RemoveLast();
                bool res = _dict.Remove(node.Value);
                Debug.Assert(res);
            }

            // add the new entry to the head of the list and into the dictionary
            LinkedListNode<TKey> listNode = new LinkedListNode<TKey>(key);
            _list.AddFirst(listNode);
            _dict[key] = new CacheDict<TKey, TValue>.KeyInfo(value, listNode);
        }

        internal TValue this[TKey key] {
            get {
                TValue res;
                if (TryGetValue(key, out res)) {
                    return res;
                }
                throw new KeyNotFoundException();
            }
            set {
                Add(key, value);
            }
        }

        private struct KeyInfo {
            internal readonly TValue Value;
            internal readonly LinkedListNode<TKey> List;

            internal KeyInfo(TValue value, LinkedListNode<TKey> list) {
                Value = value;
                List = list;
            }
        }
    }
}
