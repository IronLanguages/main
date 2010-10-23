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
using System.Linq;
using System.Text;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    class TypeUnion : IEnumerable<Namespace> {
        private HashSet<Namespace> _ns;
        private bool _isObject;
        private const int MaxUniqueNamespaces = 10;

        private static IEqualityComparer<Namespace> ObjectComparer = EqualityComparer<Namespace>.Default;
        public static IEqualityComparer<Namespace> UnionComparer = new UnionEqualityComparer();

        public bool Add(Namespace ns, ProjectState state) {
            if (_isObject) {
                return false;
            }
            if (_ns == null) {
                _ns = new HashSet<Namespace>(ObjectComparer);
            }

            if (_ns.Add(ns)) {
                if (_ns.Count > MaxUniqueNamespaces) {
                    if (_ns.Comparer == ObjectComparer) {
                        _ns = new HashSet<Namespace>(_ns, UnionComparer);
                    } else {
                        // TODO: We should warn here in debug builds so see if we can improve tracking
                        _ns = state._objectSet;
                        _isObject = true;
                    }
                }
                return true;
            }

            return false;
        }

        public int Count {
            get {
                if (_ns == null) {
                    return 0;
                }
                return _ns.Count;
            }
        }

        public bool Contains(Namespace ns) {
            if (_ns != null) {
                return _ns.Contains(ns);
            }
            return false;
        }

        public ISet<Namespace> ToSet() {
            if (Count == 0) {
                return EmptySet<Namespace>.Instance;
            }

            return new HashSet<Namespace>(this);
        }

        #region IEnumerable<Namespace> Members

        public IEnumerator<Namespace> GetEnumerator() {
            if (_ns == null) {
                return EmptySet();
            }
            return _ns.GetEnumerator();
        }

        private IEnumerator<Namespace> EmptySet() {
            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _ns.GetEnumerator();
        }

        #endregion

        class UnionEqualityComparer : IEqualityComparer<Namespace> {
            #region IEqualityComparer<Namespace> Members

            public bool Equals(Namespace x, Namespace y) {
                return x.UnionEquals(y);
            }

            public int GetHashCode(Namespace obj) {
                return obj.UnionHashCode();
            }

            #endregion
        }
    }
}
