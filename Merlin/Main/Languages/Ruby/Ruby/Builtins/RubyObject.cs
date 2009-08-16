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

        public RubyObject(RubyClass/*!*/ cls) {
            Assert.NotNull(cls);
            Debug.Assert(!cls.IsSingletonClass);
            _immediateClass = cls;
        }

        public override string/*!*/ ToString() {
#if DEBUG && !SILVERLIGHT && !SYSTEM_CORE
            if (RubyBinder._DumpingExpression) {
                return BaseToMutableString(this).ToString();
            }
#endif
            var site = _immediateClass.ToStringSite;
            object toStringResult = site.Target(site, this);
            if (ReferenceEquals(toStringResult, RubyOps.ForwardToBase)) {
                return BaseToString();
            }

            string str = toStringResult as string;
            if (str != null) {
                return str;
            }

            var mstr = toStringResult as MutableString ?? RubyUtils.ObjectToMutableString(_immediateClass.Context, toStringResult);
            return mstr.ToString();
        }

        public string/*!*/ BaseToString() {
            return ToMutableString(this).ToString();
        }

        public static MutableString/*!*/ ToMutableString(IRubyObject/*!*/ self) {
            return RubyUtils.FormatObject(self.ImmediateClass.GetNonSingletonClass().Name, self.GetInstanceData().ObjectId, self.IsTainted);
        }

        public static MutableString/*!*/ BaseToMutableString(IRubyObject/*!*/ self) {
            if (self is RubyObject) {
                return ToMutableString(self);
            } else {
                return MutableString.CreateMutable(self.BaseToString(), RubyEncoding.UTF8);
            }
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

        public bool BaseEquals(object other) {
            return base.Equals(other);
        }

        public override int GetHashCode() {
            var site = _immediateClass.GetHashCodeSite;
            object hashResult = site.Target(site, this);
            if (ReferenceEquals(hashResult, RubyOps.ForwardToBase)) {
                return base.GetHashCode();
            }

            return Protocols.ToHashCode(hashResult);
        }

        public int BaseGetHashCode() {
            return base.GetHashCode();
        }

        public MutableString/*!*/ Inspect() {
            return _immediateClass.Context.Inspect(this);
        }

        protected virtual RubyObject/*!*/ CreateInstance() {
            return new RubyObject(_immediateClass.NominalClass);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

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
            get { return _instanceData != null && _instanceData.Frozen; }
        }

        public bool IsTainted {
            get { return _instanceData != null && _instanceData.Tainted; }
            set { GetInstanceData().Tainted = value; }
        }

        public void Freeze() {
            GetInstanceData().Freeze();
        }

        #endregion

#if !SILVERLIGHT
        protected RubyObject(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.DeserializeObject(out _instanceData, out _immediateClass, info);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.SerializeObject(_instanceData, _immediateClass, info);
        }
#endif
    }
}
