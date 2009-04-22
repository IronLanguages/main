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

using System.Runtime.Serialization;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using System.Security.Permissions;
using IronRuby.Compiler.Generation;
using System.Diagnostics;

namespace IronRuby.Builtins {
    /// <summary>
    /// The type to represent user objects that inherit from Object
    /// 
    /// Note that for classes that inherit from some other class, RubyTypeDispenser gets used
    /// </summary>
    [DebuggerDisplay("{Inspect().ConvertToString()}")]
    public partial class RubyObject : IRubyObject, IRubyObjectState, IDuplicable, ISerializable {
        internal const string ClassPropertyName = "Class";

        private RubyInstanceData _instanceData;
        private readonly RubyClass/*!*/ _class;

        public RubyObject(RubyClass/*!*/ cls) {
            Assert.NotNull(cls);
            _class = cls;
        }

        public override string/*!*/ ToString() {
            // Translate ToString to to_s conversion for .NET callers.
            // 
            var site = _class.StringConversionSite;
            return site.Target(site, this).ToString();
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(this, obj)) {
                // Handle this directly here. Otherwise it can cause infinite recurion when running
                // script code below as the DLR code needs to call Equals for templating of rules
                return true;
            }

            var site = _class.EqlSite;
            return Protocols.IsTrue(site.Target(site, this, obj));
        }

        public override int GetHashCode() {
            var site = _class.HashSite;
            object hash = site.Target(site, this);
            if (!((hash is int)  || (hash is Microsoft.Scripting.Math.BigInteger))) {
                throw RubyExceptions.CreateUnexpectedTypeError(_class.Context, "hash", "Integer");
            }
            return hash.GetHashCode();
        }

        public MutableString/*!*/ ToMutableString() {
            return RubyUtils.FormatObject(_class.Name, GetInstanceData().ObjectId, ((IRubyObjectState)this).IsTainted);
        }

        public MutableString/*!*/ Inspect() {
            return _class.Context.Inspect(this);
        }

#if !SILVERLIGHT
        protected RubyObject(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.DeserializeObject(out _instanceData, out _class, info);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            RubyOps.SerializeObject(_instanceData, _class, info);
        }
#endif

        [Emitted]
        public RubyClass/*!*/ Class {
            get { return _class; }
        }

        public RubyInstanceData/*!*/ GetInstanceData() {
            return RubyOps.GetInstanceData(ref _instanceData);
        }

        public RubyInstanceData TryGetInstanceData() {
            return _instanceData;
        }

        private void CopyInstanceDataFrom(IRubyObject/*!*/ source, bool copyFrozenState) {
            // copy instance data, but not the state:
            var sourceData = source.TryGetInstanceData();
            if (sourceData != null) {
                _instanceData = new RubyInstanceData();
                sourceData.CopyInstanceVariablesTo(_instanceData);
            }

            // copy flags:
            SetTaint(this, IsTainted(source));
            if (copyFrozenState && IsFrozen(source)) {
                Freeze(this);
            }
        }

        protected virtual RubyObject/*!*/ CreateInstance() {
            return new RubyObject(_class);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            result.CopyInstanceDataFrom(this, copySingletonMembers);
            return result;
        }

        #region IRubyObjectState Members

        bool IRubyObjectState.IsFrozen {
            get { return _instanceData != null && _instanceData.Frozen; }
        }

        bool IRubyObjectState.IsTainted {
            get { return _instanceData != null && _instanceData.Tainted; }
            set { GetInstanceData().Tainted = value; }
        }

        void IRubyObjectState.Freeze() {
            GetInstanceData().Freeze();
        }

        public static bool IsFrozen(IRubyObject/*!*/ obj) {
            var instanceData = obj.TryGetInstanceData();
            return instanceData != null && instanceData.Frozen;
        }

        public static bool IsTainted(IRubyObject/*!*/ obj) {
            var instanceData = obj.TryGetInstanceData();
            return instanceData != null && instanceData.Tainted;
        }

        public static void Freeze(IRubyObject/*!*/ obj) {
            obj.GetInstanceData().Freeze();
        }

        public static void SetTaint(IRubyObject/*!*/ obj, bool value) {
            obj.GetInstanceData().Tainted = value;
        }

        #endregion
    }
}
