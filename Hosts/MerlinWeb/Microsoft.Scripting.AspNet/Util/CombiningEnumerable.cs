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

#if UNUSED
// Combines two enumerable lists into one without making a copy
namespace Microsoft.Scripting.AspNet.Util {
    class CombiningEnumerable<T> : IEnumerable<T> {

        private IEnumerable<T> _list1, _list2;

        public CombiningEnumerable(IEnumerable<T> list1, IEnumerable<T> list2) {
            _list1 = list1;
            _list2 = list2;
        }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator() {
            return new CombiningEnumerator<T>(_list1.GetEnumerator(), _list2.GetEnumerator());
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        private class CombiningEnumerator<T> : IEnumerator<T> {

            private IEnumerator<T> _e1, _e2;
            private bool _useE1 = true;

            public CombiningEnumerator(IEnumerator<T> e1, IEnumerator<T> e2) {
                _e1 = e1;
                _e2 = e2;
            }

            #region IEnumerator<T> Members
            public T Current {
                get { return _useE1 ? _e1.Current : _e2.Current; }
            }
            #endregion

            #region IDisposable Members
            public void Dispose() {
                _e1.Dispose();
                _e2.Dispose();
            }
            #endregion

            #region IEnumerator Members
            object System.Collections.IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                if (_useE1) {
                    if (_e1.MoveNext())
                        return true;

                    _useE1 = false;
                }

                return _e2.MoveNext();
            }

            public void Reset() {
                _e1.Reset();
                _e2.Reset();
                _useE1 = true;
            }
            #endregion
        }
    }
}
#endif
