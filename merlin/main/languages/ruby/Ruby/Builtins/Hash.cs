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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// Dictionary inherits from Object, mixes in Enumerable.
    /// Ruby hash is a Dictionary{object, object}, but it adds default value/proc
    /// </summary>
    public partial class Hash : Dictionary<object, object>, IDuplicable {

        // The default value can be a Proc that we should *return*, and that is different
        // from the default value being a Proc that we should *call*, hence two variables
        private Proc _defaultProc;
        private object _defaultValue;

        public Proc DefaultProc { get { return _defaultProc; } set { _defaultProc = value; } }
        public object DefaultValue { get { return _defaultValue; } set { _defaultValue = value; } }

        #region Construction

        public Hash(RubyContext/*!*/ context)
            : base(context.EqualityComparer) {
        }

        public Hash(IEqualityComparer<object>/*!*/ comparer)
            : base(comparer) {
        }

        public Hash(EqualityComparer/*!*/ comparer, Proc defaultProc, object defaultValue)
            : base(comparer) {
            _defaultValue = defaultValue;
            _defaultProc = defaultProc;
        }

        public Hash(EqualityComparer/*!*/ comparer, int capacity)
            : base(capacity, comparer) {
        }

        public Hash(IDictionary<object, object>/*!*/ dictionary)
            : base(dictionary) {
        }
        
        public Hash(IDictionary<object, object>/*!*/ dictionary, EqualityComparer/*!*/ comparer) 
            : base(dictionary, comparer) {
        }

        public Hash(Hash/*!*/ hash)
            : base(hash, hash.Comparer) {
            _defaultProc = hash._defaultProc;
            _defaultValue = hash.DefaultValue;
        }

        /// <summary>
        /// Creates an empty instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Array.
        /// </summary>
        protected virtual Hash/*!*/ CreateInstance() {
            return new Hash(Comparer);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        #endregion
    }
}
