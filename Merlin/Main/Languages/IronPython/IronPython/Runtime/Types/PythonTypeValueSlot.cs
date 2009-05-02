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

using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types {
    internal class PythonTypeValueSlot : PythonTypeSlot, IValueSlot {
        private object _value;

        public PythonTypeValueSlot(object value) {
            _value = value;
        }

        internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value) {
            value = _value;
            return true;
        }

        internal override bool GetAlwaysSucceeds {
            get {
                return true;
            }
        }

        internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner) {
            if (instance == null) {
                //!!! remove ValueSlot from dictionary
            } 
            return false;            
        }

        public object Value {
            get {
                return _value;
            }
        }
    }

    interface IValueSlot {
        object Value {
            get;
        }
    }
}
