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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// lazily creates nested namespaces and types from a .NET namespace.  This
    /// just uses dir to reflect over the members and returns more Namespace objects 
    /// which correspond with the appropriate member type.
    /// </summary>
    internal class LazyDotNetDict : IDictionary<string, ISet<Namespace>> {
        private readonly Dictionary<string, ISet<Namespace>> _variables;
        private readonly object[] _objects;
        private readonly ProjectState _projectState;
        private readonly bool _showClr;
        private Dictionary<object, IList<string>> _nonClrAttributes;
        private bool _gotAll;

        public LazyDotNetDict(object obj, ProjectState projectState, bool showClr)
            : this(new[] { obj }, projectState, showClr) {
        }

        public LazyDotNetDict(IEnumerable<object> objects, ProjectState projectState, bool showClr) {
            _objects = objects.ToArray();
            _variables = new Dictionary<string, ISet<Namespace>>();
            _projectState = projectState;
            _showClr = showClr;            
        }

        public ISet<Namespace> GetValue(string index, ISet<Namespace> defaultValue) {
            return GetClr(index, _showClr, defaultValue);
        }

        public bool IsVisible(object module, string index, bool showClr) {
            if (showClr) {
                return true;
            }

            if (_nonClrAttributes == null) {
                _nonClrAttributes = new Dictionary<object, IList<string>>();
            }

            IList<string> members;
            if (!_nonClrAttributes.TryGetValue(module, out members)) {
                _nonClrAttributes[module] = members = Utils.DirHelper(module, false);
            }

            return members.IndexOf(index) != -1;
        }

        private object GetOne(object module, string index, bool showClr) {
            if (IsVisible(module, index, showClr)) {
                PythonType pyType = (module as PythonType);
                if (pyType != null) {
                    foreach (var baseType in pyType.mro()) {
                        PythonType curType = (baseType as PythonType);                        
                        if (curType != null) {
                            IDictionary<object, object> dict = new DictProxy(curType);                            
                            object bresult;
                            if (dict.TryGetValue(index, out bresult)) {
                                return bresult;
                            }
                        }
                    }
                }

                var tracker = module as NamespaceTracker;
                if (tracker != null) {
                    object value = NamespaceTrackerOps.GetCustomMember(_projectState.CodeContext, tracker, index);
                    if (value != OperationFailed.Value) {
                        return value;
                    } else {
                        return this;
                    }
                }

                object result;
                if (_projectState.TryGetMember(module, index, showClr, out result)) {
                    return result;
                }
            }
            return this; // sentinel indicating failure
        }

        internal ISet<Namespace> GetClr(string index, bool showClr, ISet<Namespace> defaultValue) {
            ISet<Namespace> result;
            if (_variables.TryGetValue(index, out result)) {      
                return result ?? defaultValue;
            }

            var attrs = new List<object>();
            foreach (var module in _objects) {
                try {
                    var attr = GetOne(module, index, showClr);
                    if (attr != this) {
                        attrs.Add(attr);
                    }
                } catch {
                    // TODO: Remove when Python bug is fixed
                }
            }

            if (attrs.Count > 0) {
                var ns = _projectState.GetNamespaceFromObjects(attrs);
                result = _variables[index] = ns.SelfSet;
                return result;
            } else {
                _variables[index] = null;
            }

            return defaultValue;
        }

        public object[] Objects {
            get { return _objects; }
        }

        #region IDictionary<string,Namespace> Members

        public void Add(string key, ISet<Namespace> value) {
            throw new InvalidOperationException();
        }

        public bool ContainsKey(string key) {
            throw new NotImplementedException();
        }

        public ICollection<string> Keys {
            get {
                EnsureAll();
                List<string> res = new List<string>(_variables.Count);
                foreach (var v in _variables) {
                    if (v.Value != null) {
                        res.Add(v.Key);
                    }
                }
                return res;
            }
        }

        private void EnsureAll() {
            if (!_gotAll) {
                HashSet<string> result = new HashSet<string>();
                if (_objects.Length == 1 && (_objects[0] is PythonType)) {
                    // fast path for when we're looking up in a type, this is about twice as fast
                    // as going through the normal code path.
                    PythonType pyType = (PythonType)_objects[0];
                    foreach (var baseType in pyType.mro()) {
                        PythonType curType = (baseType as PythonType);
                        if (curType != null) {
                            var dict = new DictProxy(curType);
                            var enumerator = dict.iteritems(_showClr ? _projectState.CodeContextCls : _projectState.CodeContext);
                            while (enumerator.MoveNext()) {
                                PythonTuple value = (PythonTuple)enumerator.Current;
                                string key = (string)value[0];

                                if (_variables.ContainsKey(key)) {
                                    continue;
                                }

                                _variables[key] = _projectState.GetNamespaceFromObjects(value[1]).SelfSet;
                            }
                        }
                    }               
                } else {
                    foreach (var module in _objects) {
                        foreach (var name in Utils.DirHelper(module, _showClr)) {
                            GetClr(name, _showClr, null);
                        }
                    }
                }
                _gotAll = true;
            }
        }

        public bool Remove(string key) {
            throw new InvalidOperationException();
        }

        public bool TryGetValue(string key, out ISet<Namespace> value) {
            EnsureAll();
            return _variables.TryGetValue(key, out value) && value != null;
        }

        public ICollection<ISet<Namespace>> Values {
            get { throw new NotImplementedException(); }
        }

        public ISet<Namespace> this[string index] {
            get {
                ISet<Namespace> result = GetValue(index, null);
                if (result != null) {
                    return result;
                }
                if (Keys.Contains(index)) {
                    // work around bug where IronPython includes property methods
                    // in types overloaded by generic arity (e.g. 
                    // System.Linq.Expressions.Expression)
                    return EmptySet<Namespace>.Instance;
                }
                throw new KeyNotFoundException(String.Format("Key {0} not found", index));
            }
            set {
                _variables[index] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,ISet<Namespace>>> Members

        public void Add(KeyValuePair<string, ISet<Namespace>> item) {
            throw new InvalidOperationException();
        }

        public void Clear() {
            throw new InvalidOperationException();
        }

        public bool Contains(KeyValuePair<string, ISet<Namespace>> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, ISet<Namespace>>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count {
            get { return Keys.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove(KeyValuePair<string, ISet<Namespace>> item) {
            throw new InvalidOperationException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,ISet<Namespace>>> Members

        public IEnumerator<KeyValuePair<string, ISet<Namespace>>> GetEnumerator() {
            EnsureAll();
            foreach (var v in _variables) {
                if (v.Value != null) {
                    yield return v;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        public ProjectState ProjectState {
            get {
                return _projectState;
            }
        }
    }
}
