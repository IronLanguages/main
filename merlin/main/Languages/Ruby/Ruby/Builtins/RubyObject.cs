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

namespace IronRuby.Builtins {
    public partial class RubyObject : IRubyObject, IRubyObjectState, IDuplicable, ISerializable {
        internal const string ClassPropertyName = "Class";

        private RubyInstanceData _instanceData;
        private readonly RubyClass/*!*/ _class;

        public RubyObject(RubyClass/*!*/ cls) {
            Assert.NotNull(cls);
            _class = cls;
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
