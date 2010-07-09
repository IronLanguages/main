/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace IronRuby.Runtime {
    internal sealed class WeakList<T> : IEnumerable<T>
        where T : class {
        private readonly List<WeakReference/*!*/> _list = new List<WeakReference>();

        public WeakList() {
        }

        public IEnumerator<T> GetEnumerator() {
            int deadCount = 0;
            for (int i = 0; i < _list.Count; i++) {
                object item = _list[i].Target;
                if (item != null) {
                    yield return (T)item;
                } else {
                    deadCount++;
                }
            }

            // deadCount is greated than 1/5 of total => remove dead (the threshold is arbitrary, might need tuning):
            if (deadCount > 5 && deadCount > _list.Count / 5) {
                Prune();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }


        public void Add(WeakReference item) {
            _list.Add(item);
        }

        private void Prune() {
            int i = 0, j = 0;
            while (i < _list.Count) {
                if (_list[i].IsAlive) {
                    if (j != i) {
                        _list[j] = _list[i];
                    }
                    j++;
                }
                i++;
            }

            if (j < i) {
                _list.RemoveRange(j, i - j);
                _list.TrimExcess();
            }
        }
    }
}
