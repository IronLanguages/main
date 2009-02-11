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
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Operations;    

namespace IronPython.Runtime.Types {
    sealed class PythonTypeUserDescriptorSlot : PythonTypeSlot {
        private object _value;

        public PythonTypeUserDescriptorSlot(object value) {
            _value = value;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            try {
                value = PythonOps.GetUserDescriptor(Value, instance, owner);
                return true;
            } catch (MissingMemberException) {
                value = null;
                return false;
            }
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            return PythonOps.TryDeleteUserDescriptor(Value, instance);
        }

        internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value) {
            return PythonOps.TrySetUserDescriptor(Value, instance, value);
        }

        internal override bool IsSetDescriptor(CodeContext context, PythonType owner) {
            object dummy;
            return PythonOps.TryGetBoundAttr(context, Value, Symbols.SetDescriptor, out dummy);
        }
        public object Value {
            get {
                return _value;
            }
        }

    }
}
