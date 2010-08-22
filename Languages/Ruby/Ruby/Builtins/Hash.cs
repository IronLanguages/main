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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// TODO: ordered dictionary
    /// TODO: all operations should check frozen state!
    /// 
    /// Dictionary inherits from Object, mixes in Enumerable.
    /// Ruby hash is a Dictionary{object, object}, but it adds default value/proc
    /// </summary>
    public partial class Hash : Dictionary<object, object>, IRubyObjectState, IDuplicable {

        // The default value can be a Proc that we should *return*, and that is different
        // from the default value being a Proc that we should *call*, hence two variables
        private Proc _defaultProc;
        private object _defaultValue;

        private uint _flags;
        private const uint IsFrozenFlag = 1;
        private const uint IsTaintedFlag = 2;
        private const uint IsUntrustedFlag = 4;

        public Proc DefaultProc { 
            get { return _defaultProc; } 
            set {
                Mutate();
                _defaultProc = value;
            }
        }

        public object DefaultValue { 
            get { return _defaultValue; } 
            set {
                Mutate();
                _defaultValue = value;
            }
        }

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
        /// Creates a blank instance of a RubyArray or its subclass given the Ruby class object.
        /// </summary>
        public static Hash/*!*/ CreateInstance(RubyClass/*!*/ rubyClass) {
            return (rubyClass.GetUnderlyingSystemType() == typeof(Hash)) ? new Hash(rubyClass.Context) : new Hash.Subclass(rubyClass);
        }

        /// <summary>
        /// Creates an empty instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Hash.
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

        #region Flags

        public void RequireNotFrozen() {
            if ((_flags & IsFrozenFlag) != 0) {
                throw RubyExceptions.CreateObjectFrozenError();
            }
        }

        private void Mutate() {
            RequireNotFrozen();
        }

        public bool IsTainted {
            get {
                return (_flags & IsTaintedFlag) != 0;
            }
            set {
                Mutate();
                _flags = (_flags & ~IsTaintedFlag) | (value ? IsTaintedFlag : 0);
            }
        }

        public bool IsUntrusted {
            get {
                return (_flags & IsUntrustedFlag) != 0;
            }
            set {
                Mutate();
                _flags = (_flags & ~IsUntrustedFlag) | (value ? IsUntrustedFlag : 0);
            }
        }

        public bool IsFrozen {
            get {
                return (_flags & IsFrozenFlag) != 0;
            }
        }

        void IRubyObjectState.Freeze() {
            Freeze();
        }

        public Hash/*!*/ Freeze() {
            _flags |= IsFrozenFlag;
            return this;
        }

        #endregion
    }
}
