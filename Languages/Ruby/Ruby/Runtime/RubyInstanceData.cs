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

using System.Collections.Generic;
using Microsoft.Scripting;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;
using System.Diagnostics;
using System;

namespace IronRuby.Runtime {
    /// <summary>
    /// Stores the per-instance data that all Ruby objects need (frozen?, tainted?, untrusted?, instance_variables, etc)
    /// Stored in a lookaside weak hashtable for types that don't implement IRubyObject (i.e. .NET types).
    /// </summary>
    public sealed class RubyInstanceData : IRubyObjectState {
        // TODO: compress
        
        private static int _CurrentObjectId = 42; // Last unique Id we gave out.
        private int _objectId;
        
        // The values are unused if the object itself implements IRubyObjectState.
        private bool _frozen, _tainted, _untrusted;
        
        private Dictionary<string, object> _instanceVars;
        private RubyClass _immediateClass;

        /// <summary>
        /// Null - uninitialized (lazy init'd) => object doesn't have an instance singleton.
        /// Class - object doesn't have an instance singleton
        /// Singleton - object has an instance singleton
        /// 
        /// Not used by implementations of IRubyObject.
        /// </summary>
        internal RubyClass ImmediateClass {
            get { return _immediateClass; }
            set { _immediateClass = value; }
        }

        // Updates the immediate class if it has not been initialized yet.
        internal void UpdateImmediateClass(RubyClass/*!*/ immediate) {
            Interlocked.CompareExchange(ref _immediateClass, immediate, null);
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

        #region Flags

        public bool IsTainted {
            get { return _tainted; }
            set {
                Mutate();
                _tainted = value;
            }
        }

        public bool IsUntrusted {
            get { return _untrusted; }
            set {
                Mutate();
                _untrusted = value; 
            }
        }

        public bool IsFrozen {
            get { return _frozen; }
        }

        public void Freeze() {
            _frozen = true;
        }

        private void Mutate() {
            if (_frozen) {
                throw RubyExceptions.CreateObjectFrozenError();
            }
        }

        #endregion

        #region Instance Variables

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
            // frozen state checked by caller
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
            // Frozen state checked by caller. 
            Dictionary<string, object> vars = GetInstanceVariables();
            lock (vars) {
                vars[name] = value;
            }
        }

        internal VariableDebugView[]/*!*/ GetInstanceVariablesDebugView(RubyContext/*!*/ context) {
            if (_instanceVars == null) {
                return new RubyInstanceData.VariableDebugView[0];
            }

            var result = new List<VariableDebugView>();
            lock (_instanceVars) {
                foreach (var var in _instanceVars) {
                    result.Add(new VariableDebugView(context, this, var.Key));
                }
            }

            result.Sort((var1, var2) => String.CompareOrdinal(var1._name, var2._name));
            return result.ToArray();
        }

        [DebuggerDisplay("{GetValue()}", Name = "{_name,nq}", Type = "{GetClassName(),nq}")]
        public sealed class VariableDebugView {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RubyContext/*!*/ _context;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly RubyInstanceData/*!*/ _data;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            internal readonly string/*!*/ _name;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object A {
                get { return GetValue(); }
            }
            
            [DebuggerDisplay("{B}", Name = "Raw Value", Type = "{GetClrType()}")]
            public object B {
                get { return GetValue(); }
                set { _data.SetInstanceVariable(_name, value); }
            }
            
            private object GetValue() {
                return _data.GetInstanceVariable(_name);
            }

            private Type GetClrType() {
                var value = GetValue();
                return value != null ? value.GetType() : null;
            }

            private string/*!*/ GetClassName() {
                return _context.GetClassDisplayName(GetValue());
            }

            internal VariableDebugView(RubyContext/*!*/ context, RubyInstanceData/*!*/ data, string/*!*/ name) {
                _context = context;
                _data = data;
                _name = name;
            }
        }

        #endregion
    }
}
