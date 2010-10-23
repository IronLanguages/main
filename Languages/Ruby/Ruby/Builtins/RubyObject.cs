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

using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    /// <summary>
    /// The type to represent user objects that inherit from Object
    /// 
    /// Note that for classes that inherit from some other class, RubyTypeDispenser gets used
    /// </summary>
    [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
    [DebuggerDisplay(RubyObject.DebuggerDisplayValue, Type = RubyObject.DebuggerDisplayType)]
    public partial class RubyObject : IRubyObject, IDuplicable, ISerializable {
        internal const string ImmediateClassFieldName = "_immediateClass"; 
        internal const string InstanceDataFieldName = "_instanceData";
        internal const string DebuggerDisplayValue = "{" + ImmediateClassFieldName + ".GetDebuggerDisplayValue(this),nq}";
        internal const string DebuggerDisplayType = "{" + ImmediateClassFieldName + ".GetDebuggerDisplayType(),nq}";

        private RubyInstanceData _instanceData;
        private RubyClass/*!*/ _immediateClass;

        /// <summary>
        /// Implements Object#new.
        /// </summary>
        public RubyObject(RubyClass/*!*/ cls) {
            Assert.NotNull(cls);
            Debug.Assert(!cls.IsSingletonClass);
            _immediateClass = cls;
        }

        /// <summary>
        /// Implements Object#new.
        /// </summary>
        public RubyObject(RubyClass/*!*/ cls, params object[] args) 
            : this(cls) {
            // MRI: args are ignored
        }

        protected virtual RubyObject/*!*/ CreateInstance() {
            return new RubyObject(_immediateClass.NominalClass);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        #region ToString, Equals, GetHashCode

        public override string/*!*/ ToString() {
#if DEBUG && !SILVERLIGHT && CLR2
            if (RubyBinder._DumpingExpression) {
                return RubyUtils.ObjectBaseToMutableString(this).ToString();
            }
#endif
            var site = _immediateClass.ToStringSite;
            object toStringResult = site.Target(site, this);
            if (ReferenceEquals(toStringResult, RubyOps.ForwardToBase)) {
                return ((IRubyObject)this).BaseToString();
            }

            string str = toStringResult as string;
            if (str != null) {
                return str;
            }

            var mstr = toStringResult as MutableString ?? RubyUtils.ObjectToMutableString(_immediateClass.Context, toStringResult);
            return mstr.ToString();
        }

        public override bool Equals(object other) {
            if (ReferenceEquals(this, other)) {
                // Handle this directly here. Otherwise it can cause infinite recurion when running
                // script code below as the DLR code needs to call Equals for templating of rules
                return true;
            }
            
            var site = _immediateClass.EqualsSite;
            object equalsResult = site.Target(site, this, other);
            if (equalsResult == RubyOps.ForwardToBase) {
                return base.Equals(other);
            }

            return RubyOps.IsTrue(equalsResult);
        }

        public override int GetHashCode() {
            var site = _immediateClass.GetHashCodeSite;
            object hashResult = site.Target(site, this);
            if (ReferenceEquals(hashResult, RubyOps.ForwardToBase)) {
                return base.GetHashCode();
            }

            return Protocols.ToHashCode(hashResult);
        }

        string/*!*/ IRubyObject.BaseToString() {
            return RubyOps.ObjectToString(this);
        }

        bool IRubyObject.BaseEquals(object other) {
            return base.Equals(other);
        }

        int IRubyObject.BaseGetHashCode() {
            return base.GetHashCode();
        }

        #endregion

        #region IRubyObject

        [Emitted]
        public RubyClass/*!*/ ImmediateClass {
            get { return _immediateClass; }
            set { _immediateClass = value; }
        }

        public RubyInstanceData/*!*/ GetInstanceData() {
            return RubyOps.GetInstanceData(ref _instanceData);
        }

        public RubyInstanceData TryGetInstanceData() {
            return _instanceData;
        }

        public bool IsFrozen {
            get { return _instanceData != null && _instanceData.IsFrozen; }
        }

        public bool IsTainted {
            get { return _instanceData != null && _instanceData.IsTainted; }
            set { GetInstanceData().IsTainted = value; }
        }

        public bool IsUntrusted {
            get { return _instanceData != null && _instanceData.IsUntrusted; }
            set { GetInstanceData().IsUntrusted = value; }
        }

        public void Freeze() {
            GetInstanceData().Freeze();
        }

        #endregion

        #region Serialization

#if !SILVERLIGHT // serialization
        protected RubyObject(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.DeserializeObject(out _instanceData, out _immediateClass, info);
        }

        public virtual void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.SerializeObject(_instanceData, _immediateClass, info);
        }
#endif

        #endregion
    }
}
