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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using Microsoft.Contracts;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Represents a language context.  Typically there is at most 1 context 
    /// associated with each language, but some languages may use more than one context
    /// to identify code that should be treated differently.  Contexts are used during
    /// member and operator lookup.
    /// </summary>
    [Serializable]
    public struct ContextId : IEquatable<ContextId> {
        private int _id;
        private static Dictionary<object, ContextId> _contexts = new Dictionary<object,ContextId>();
        private static int _maxId = 1;

        public static readonly ContextId Empty = new ContextId();

        internal ContextId(int id) {
            _id = id;
        }

        /// <summary>
        /// Registers a language within the system with the specified name.
        /// </summary>
        public static ContextId RegisterContext(object identifier) {
            lock(_contexts) {
                ContextId res;
                if (_contexts.TryGetValue(identifier, out res)) {
                    throw Error.LanguageRegistered();
                }

                ContextId id = new ContextId();
                id._id = _maxId++;

                return id;
            }
        }

        /// <summary>
        /// Looks up the context ID for the specified context identifier
        /// </summary>
        public static ContextId LookupContext(object identifier) {
            ContextId res;
            lock (_contexts) {
                if (_contexts.TryGetValue(identifier, out res)) {
                    return res;
                }
            }

            return ContextId.Empty;
        }

        public int Id {
            get {
                return _id;
            }
        }

        #region IEquatable<ContextId> Members

        [StateIndependent]
        public bool Equals(ContextId other) {
            return this._id == other._id;
        }

        #endregion

        #region Object overrides

        public override int GetHashCode() {
            return _id;
        }

        public override bool Equals(object obj) {
            if (!(obj is ContextId)) return false;

            ContextId other = (ContextId)obj;
            return other._id == _id;
        }

        #endregion

        public static bool operator ==(ContextId self, ContextId other) {
            return self.Equals(other);
        }

        public static bool operator !=(ContextId self, ContextId other) {
            return !self.Equals(other);
        }
    }
}
