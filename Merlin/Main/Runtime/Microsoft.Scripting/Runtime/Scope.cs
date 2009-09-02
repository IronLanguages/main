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
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Represents a host-provided variables for executable code.  The variables are
    /// typically backed by a host-provided dictionary. Languages can also associate per-language
    /// information with the context by using scope extensions.  This can be used for tracking
    /// state which is used across multiple executions, for providing custom forms of 
    /// storage (for example object keyed access), or other language specific semantics.
    /// 
    /// Scope objects are thread-safe as long as their underlying storage is thread safe.
    /// 
    /// Script hosts can choose to use thread safe or thread unsafe modules but must be sure
    /// to constrain the code they right to be single-threaded if using thread unsafe
    /// storage.
    /// </summary>
    public sealed class Scope {
        private ScopeExtension[] _extensions; // resizable
        private readonly IAttributesCollection _dict;

        /// <summary>
        /// Creates a new scope with a new empty thread-safe dictionary.  
        /// </summary>
        public Scope()
            : this(null) {
        }

        /// <summary>
        /// Creates a new scope with the provided dictionary.
        /// </summary>
        public Scope(IAttributesCollection dictionary) {
            _dict = dictionary ?? new SymbolDictionary();
            _extensions = ScopeExtension.EmptyArray;
        }

        /// <summary>
        /// Gets the ScopeExtension associated with the provided ContextId.
        /// </summary>
        public ScopeExtension GetExtension(ContextId languageContextId) {
            return (languageContextId.Id < _extensions.Length) ? _extensions[languageContextId.Id] : null;
        }

        /// <summary>
        /// Sets the ScopeExtension to the provided value for the given ContextId.  
        /// 
        /// The extension can only be set once.  The returned value is either the new ScopeExtension
        /// if no value was previously set or the previous value.
        /// </summary>
        public ScopeExtension SetExtension(ContextId languageContextId, ScopeExtension extension) {
            ContractUtils.RequiresNotNull(extension, "extension");

            lock (_extensions) {
                if (languageContextId.Id >= _extensions.Length) {
                    Array.Resize(ref _extensions, languageContextId.Id + 1);
                }

                return _extensions[languageContextId.Id] ?? (_extensions[languageContextId.Id] = extension);
            }
        }
       
        /// <summary>
        /// Returns the list of keys which are available to all languages.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")] // TODO: fix
        public IEnumerable<SymbolId> Keys {
            get {
                foreach (object name in _dict.Keys) {
                    string strName = name as string;
                    if (strName == null) continue;

                    yield return SymbolTable.StringToId(strName);
                }
            }
        }

        /// <summary>
        /// Returns the list of Keys and Items which are available to all languages.
        /// </summary>
        public IEnumerable<KeyValuePair<SymbolId, object>> Items {
            get {
                foreach (KeyValuePair<SymbolId, object> kvp in _dict.SymbolAttributes) {
                    yield return kvp;
                }
            }
        }

        /// <summary>
        /// Trys to lookup the provided name in the current scope.
        /// </summary>
        public bool TryGetVariable(SymbolId name, out object value) {
            return _dict.TryGetValue(name, out value);
        }
		
        /// <summary>
        /// Gets the name from the Scope.  If the name is not defined a MissingMemberException
        /// is thrown.
        /// </summary>
        /// <exception cref="MissingMemberException">The name is not defined in the scope.</exception>
		public object GetVariable(SymbolId name) {
            object value;
            if (!TryGetVariable(name, out value)) {
                throw Error.NameNotDefined(SymbolTable.IdToString(name));
            }
            return value;
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// </summary>
        public void SetVariable(SymbolId name, object value) {
            _dict[name] = value;
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// </summary>
        public void SetVariable(string name, object value) {
            _dict[SymbolTable.StringToId(name)] = value;
        }

        /// <summary>
        /// Removes all members from the Scope.
        /// </summary>
        public void Clear() {
            List<object> ids = new List<object>(_dict.Keys);
            foreach (object name in ids) {
                _dict.RemoveObjectKey(name);
            }
        }

        /// <summary>
        /// Returns true if the provided name is defined in the Scope.
        /// </summary>
        public bool ContainsVariable(SymbolId name) {
            return _dict.ContainsKey(name);
        }

        /// <summary>
        /// Returns true if the provided name is defined in the Scope.
        /// </summary>
        public bool ContainsVariable(string name) {
            return _dict.ContainsKey(SymbolTable.StringToId(name));
        }

        /// <summary>
        /// Attempts to remove the provided name from this scope.
        /// 
        /// Returns true if the name exists and is removed, false if the 
        /// name is not defined.
        /// </summary>
        public bool TryRemoveVariable(SymbolId name) {
            return _dict.Remove(name);
        }

        /// <summary>
        /// Returns the number of variables that are defined in the Scope.
        /// </summary>
        public int VariableCount {
            get {
                return _dict.Count;
            }
        }

        #region Object key access

        /// <summary>
        /// Attemps to remove the provided object name from this scope removing names visible
        /// to both the current context and all contexts.
        /// </summary>
        public bool TryRemoveObjectName(object name) {
            return _dict.RemoveObjectKey(name);
        }

        public bool TryGetObjectName(object name, out object value) {
            if (_dict.TryGetObjectValue(name, out value)) return true;

            value = null;
            return false;
        }

        /// <summary>
        /// Sets the name to the specified value for the current context.
        /// 
        /// The name is an arbitrary object.
        /// </summary>
        public void SetObjectName(object name, object value) {
            _dict.AddObjectKey(name, value);                
        }

        public IEnumerable<object> GetAllKeys() {
            foreach (object key in _dict.Keys) {
                yield return key;
            }
        }

        /// <summary>
        /// Returns the list of Keys and Values available to all languages in addition to those
        /// keys which are only available to the provided LanguageContext.
        /// 
        /// Keys marked with DontEnumerate flag will not be returned.
        /// </summary>
        public IEnumerable<KeyValuePair<object, object>> GetAllItems() {
            foreach (KeyValuePair<object, object> kvp in _dict) {
                yield return kvp;
            }
        }

        #endregion

        #region Obsolete

        [Obsolete("Use SetVariable instead")]
        public void SetName(SymbolId name, object value) {
            SetVariable(name, value);
        }

        [Obsolete("Use TryGetVariable instead")]
        public bool TryGetName(SymbolId name, out object value) {
            return TryGetVariable(name, out value);
        }

        [Obsolete("Use ContainsVariable instead")]
        public bool ContainsName(SymbolId name) {
            return ContainsVariable(name);
        }

        #endregion
    }
}
