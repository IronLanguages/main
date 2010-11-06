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
using System.Linq;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Metadata {
    // TODO:
    public struct Sequence<T> : IEnumerable<T> {
        private const int ChunkSize = 8;

        private readonly Chunk _firstChunk;
        private Chunk _lastChunk;

        public Sequence(bool x) {
            _firstChunk = _lastChunk = new Chunk();
        }

        public void Add(T item) {
            _lastChunk = _lastChunk.Add(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return new Enumerator(_firstChunk);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private sealed class Chunk {
            private T _item1;
            private T _item2;
            private T _item3;
            private T _item4;
            private T _item5;
            private T _item6;
            private T _item7;
            private T _item8;
            internal int _count;
            internal Chunk _next;

            internal Chunk() {
            }

            internal T this[int index] {
                get {
                    ContractUtils.Assert(index < _count);
                    switch (index) {
                        case 0: return _item1;
                        case 1: return _item2;
                        case 2: return _item3;
                        case 3: return _item4;
                        case 4: return _item5;
                        case 5: return _item6;
                        case 6: return _item7;
                        case 7: return _item8;
                        default: return default(T);
                    }
                }
                set {
                    ContractUtils.Assert(index < _count);
                    switch (index) {
                        case 0: _item1 = value; return;
                        case 1: _item2 = value; return;
                        case 2: _item3 = value; return;
                        case 3: _item4 = value; return;
                        case 4: _item5 = value; return;
                        case 5: _item6 = value; return;
                        case 6: _item7 = value; return;
                        case 7: _item8 = value; return;
                    }
                }
            }

            internal Chunk Add(T item) {
                Chunk chunk;
                if (_count == ChunkSize) {
                    _next = chunk = new Chunk();
                } else {
                    chunk = this;
                }
                chunk[chunk._count++] = item;
                return chunk;
            }
        }

        private sealed class Enumerator : IEnumerator<T> {
            private readonly Chunk _initialChunk;
            private Chunk _currentChunk;
            private int _index;

            public Enumerator(Chunk sequence) {
                _initialChunk = sequence;
            }

            public bool MoveNext() {
                if (_currentChunk == null) {
                    ContractUtils.Assert(_index == 0);
                    _currentChunk = _initialChunk;
                }

                if (_index == _currentChunk._count) {
                    if (_currentChunk._next == null) {
                        return false;
                    }
                    _currentChunk = _currentChunk._next;
                    _index = 0;
                } else {
                    _index++;
                }
                return true;
            }

            public void Reset() {
                _currentChunk = null;
                _index = 0;
            }

            public T Current {
                get { return _currentChunk[_index]; }
            }

            public void Dispose() {
            }

            object System.Collections.IEnumerator.Current {
                get { return Current; }
            }
        }
    }
}
