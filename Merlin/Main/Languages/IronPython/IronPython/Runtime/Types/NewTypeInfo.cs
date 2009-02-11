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

using System.Collections.Generic;

namespace IronPython.Runtime.Types {
    /// <summary>
    /// TypeInfo captures the minimal CLI information required by NewTypeMaker for a Python object
    /// that inherits from a CLI type.
    /// </summary>
    internal class NewTypeInfo {
        // The CLI base-type.
        private Type _baseType;

        private IList<Type> _interfaceTypes;
        private IList<string> _slots;
        private Nullable<int> _hash;

        public NewTypeInfo(Type baseType, IList<Type> interfaceTypes, IList<string> slots) {
            this._baseType = baseType;
            this._interfaceTypes = interfaceTypes;
            this._slots = slots;
        }

        public Type BaseType {
            get { return _baseType; }
        }

        public IList<string> Slots {
            get {
                return _slots;
            }
        }

        public IList<Type> InterfaceTypes {
            get { return _interfaceTypes; }
        }

        public override int GetHashCode() {
            if (_hash == null) {
                int hashCode = _baseType.GetHashCode();
                for (int i = 0; i < _interfaceTypes.Count; i++) {
                    hashCode ^= _interfaceTypes[i].GetHashCode();
                }

                if (_slots != null) {
                    if (_slots.Contains("__dict__")) {
                        hashCode ^= 6551;
                    }
                    if (_slots.Contains("__weakref__")) {
                        hashCode ^= 23;
                    }
                }

                _hash = hashCode;
            }

            return _hash.Value;
        }

        public override bool Equals(object obj) {
            NewTypeInfo other = obj as NewTypeInfo;
            if (other == null) return false;


            if (_baseType.Equals(other._baseType) &&
                _interfaceTypes.Count == other._interfaceTypes.Count &&
                ((_slots == null && other._slots == null) ||
                (_slots != null && other._slots != null))) {

                for (int i = 0; i < _interfaceTypes.Count; i++) {
                    if (!_interfaceTypes[i].Equals(other._interfaceTypes[i])) return false;
                }

                if (_slots != null) {
                    if (_slots.Contains("__dict__") != other._slots.Contains("__dict__")) {
                        return false;
                    }
                    if (_slots.Contains("__weakref__") != other._slots.Contains("__weakref__")) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }        
    }
}
