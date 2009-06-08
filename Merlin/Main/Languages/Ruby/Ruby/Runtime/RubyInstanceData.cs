/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using Microsoft.Scripting;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores the per-instance data that all Ruby objects need (frozen?, tainted?, instance_variables, etc)
    /// Stored in a lookaside weak hashtable for types that don't implement IRubyObject (i.e. .NET types).
    /// </summary>
    public sealed class RubyInstanceData {
        private static int _CurrentObjectId = 42; // Last unique Id we gave out.

        // These need to be seperate fields so we get atomic access to them
        // (which means no synchronization is needed for get/set operations)
        private int _objectId;
        private bool _frozen, _tainted;
        private Dictionary<string, object> _instanceVars;
        private RubyClass _immediateClass;
         
        internal bool Tainted {
            get { return _tainted; }
            set { _tainted = value; }
        }

        internal bool Frozen {
            get { return _frozen; }
        }

        internal void Freeze() {
            _frozen = true;
        }

        /// <summary>
        /// Null - uninitialized (lazy init'd) => object doesn't have an instance singleton.
        /// Class - object doesn't have an instance singleton
        /// Singleton - object has an instance singleton
        /// </summary>
        internal RubyClass ImmediateClass {
            get { return _immediateClass; }
            set { _immediateClass = value; }
        }

        internal RubyClass InstanceSingleton {
            get { return (_immediateClass != null && _immediateClass.IsSingletonClass) ? _immediateClass : null; }
        }
        
        /// <summary>
        /// WARNING: not all objects store their ID here.
        /// Use ObjectOps.GetObjectId instead.
        /// </summary>
        internal int ObjectId {
            get { return _objectId; }
        }

        internal RubyInstanceData(int id) {
            _objectId = id;
        }

        internal RubyInstanceData() {
            _objectId = Interlocked.Increment(ref _CurrentObjectId);
        }

        /// <summary>
        /// Gets the instance variables dictionary, initializing it if it was null.
        /// Only use this if you want to set something into the dictionary, otherwise
        /// just use the _instanceVars field
        /// </summary>
        private Dictionary<string, object>/*!*/ GetInstanceVariables() {
            if (_instanceVars == null) {
                var newValue = new Dictionary<string, object>();
                if (Interlocked.CompareExchange(ref _instanceVars, newValue, null) == null) {
                    return newValue;
                }
            }
            return _instanceVars;
        }

        #region instance variable support

        internal bool HasInstanceVariables {
            get {
                return _instanceVars != null;
            }
        }

        internal void CopyInstanceVariablesTo(RubyInstanceData/*!*/ dup) {
            if (_instanceVars == null) {
                return;
            }
            lock (_instanceVars) {
                Dictionary<string, object> dupVars = dup.GetInstanceVariables();
                foreach (var var in _instanceVars) {
                    dupVars.Add(var.Key, var.Value);
                }
            }
        }

        internal bool IsInstanceVariableDefined(string/*!*/ name) {
            if (_instanceVars == null) {
                return false;
            }
            lock (_instanceVars) {
                return _instanceVars.ContainsKey(name);
            }
        }

        internal string/*!*/[]/*!*/ GetInstanceVariableNames() {
            if (_instanceVars == null) {
                return ArrayUtils.EmptyStrings;
            }
            lock (_instanceVars) {
                string[] result = new string[_instanceVars.Count];
                _instanceVars.Keys.CopyTo(result, 0);
                return result;
            }
        }

        // Returns a copy of the current instance variable key-value pairs for this object
        internal List<KeyValuePair<string, object>>/*!*/ GetInstanceVariablePairs() {
            if (_instanceVars == null) {
                return new List<KeyValuePair<string, object>>();
            }
            lock (_instanceVars) {
                return new List<KeyValuePair<string, object>>(_instanceVars);
            }
        }

        internal bool TryGetInstanceVariable(string/*!*/ name, out object value) {
            if (_instanceVars == null) {
                value = null;
                return false;
            }
            lock (_instanceVars) {
                return _instanceVars.TryGetValue(name, out value);
            }
        }

        internal bool TryRemoveInstanceVariable(string/*!*/ name, out object value) {
            if (_instanceVars == null) {
                value = null;
                return false;
            }
            lock (_instanceVars) {
                if (!_instanceVars.TryGetValue(name, out value)) {
                    return false;
                }
                _instanceVars.Remove(name);
                return true;
            }
        }

        internal object GetInstanceVariable(string/*!*/ name) {
            object value;
            TryGetInstanceVariable(name, out value);
            return value;
        }

        internal void SetInstanceVariable(string/*!*/ name, object value) {
            Dictionary<string, object> vars = GetInstanceVariables();
            lock (vars) {
                vars[name] = value;
            }
        }

        #endregion
    }
}
