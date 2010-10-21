/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Microsoft.PyAnalysis {
#if FALSE
    // No longer used but kept around just in case - now incorporated into Namespace
    class SetOfOne<T> : ISet<T> {
        private readonly T _value;

        public SetOfOne(T value) {            
            _value = value;
        }

        public T Value {
            get {
                return _value;
            }
        }

        #region ISet<T> Members

        public bool Add(T item) {
            throw new InvalidOperationException();
        }

        public void ExceptWith(IEnumerable<T> other) {
            throw new InvalidOperationException();
        }

        public void IntersectWith(IEnumerable<T> other) {
            throw new InvalidOperationException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other) {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other) {
            var enumerator = other.GetEnumerator();
            if (enumerator.MoveNext()) {
                if (Contains(enumerator.Current)) {
                    return !enumerator.MoveNext();
                }
            }
            return false;
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            throw new InvalidOperationException();
        }

        public void UnionWith(IEnumerable<T> other) {
            throw new InvalidOperationException();
        }

        #endregion

        #region ICollection<T> Members

        void ICollection<T>.Add(T item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new InvalidOperationException();
        }

        public bool Contains(T item) {
            return EqualityComparer<T>.Default.Equals(item, _value);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            array[arrayIndex] = _value;
        }

        public int Count {
            get { return 1; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(T item) {
            throw new InvalidOperationException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new SetOfOneEnumerator(_value);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { 
            yield return _value; 
        }

        #endregion

        class SetOfOneEnumerator : IEnumerator<T> {
            private readonly T _value;
            private bool _enumerated;

            public SetOfOneEnumerator(T value) {
                _value = value;
            }

            #region IEnumerator<T> Members

            T IEnumerator<T>.Current {
                get { return _value;  }
            }

            #endregion

            #region IDisposable Members

            void IDisposable.Dispose() {
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current {
                get { return _value; }
            }

            bool System.Collections.IEnumerator.MoveNext() {
                if (_enumerated) {
                    return false;
                }
                _enumerated = true;
                return true;
            }

            void System.Collections.IEnumerator.Reset() {
                _enumerated = false;
            }

            #endregion
        }
    }
#endif
}
