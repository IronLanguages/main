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

using Microsoft.Contracts;

namespace Microsoft.Scripting.Runtime {
    public class Extensible<T> {
        private T _value;

        public Extensible() { }
        public Extensible(T value) { this._value = value; }

        public T Value {
            get { return _value; }
        }

        [Confined]
        public override bool Equals(object obj) {
            return _value.Equals(obj);
        }

        [Confined]
        public override int GetHashCode() {
            return _value.GetHashCode();
        }

        [Confined]
        public override string ToString() {
            return _value.ToString();
        }

        public static implicit operator T(Extensible<T> extensible) {
            return extensible.Value;
        }
    }

}
