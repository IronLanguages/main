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

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Event args for when a ScriptScope has had its contents changed.  
    /// </summary>
    public class ModuleChangeEventArgs : EventArgs {
        private string _name;
        private ModuleChangeType _type;
        private object _value;

        /// <summary>
        /// Creates a new ModuleChangeEventArgs object with the specified name and type.
        /// </summary>
        public ModuleChangeEventArgs(string name, ModuleChangeType changeType) {
            _name = name;
            _type = changeType;
        }

        /// <summary>
        /// Creates a nwe ModuleChangeEventArgs with the specified name, type, and changed value.
        /// </summary>
        public ModuleChangeEventArgs(string name, ModuleChangeType changeType, object value) {
            _name = name;
            _type = changeType;
            _value = value;
        }

        /// <summary>
        /// Gets the name of the symbol that has changed.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Gets the way in which the symbol has changed: Set or Delete.
        /// </summary>
        public ModuleChangeType ChangeType {
            get {
                return _type;
            }
        }

        /// <summary>
        /// The the symbol has been set provides the new value.
        /// </summary>
        public object Value {
            get { return _value; }
        }
    }
}
