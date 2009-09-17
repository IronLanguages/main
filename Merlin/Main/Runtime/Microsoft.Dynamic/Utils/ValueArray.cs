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
using Microsoft.Contracts;

namespace Microsoft.Scripting.Utils {

    /// <summary>
    /// Represents an array that has value equality.
    /// </summary>
    public class ValueArray<T> : IEquatable<ValueArray<T>> {
        private readonly T[] _array;

        public ValueArray(T[] array) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNullItems(array, "array");
            _array = array;
        }

        #region IEquatable<ValueArray<T>> Members

        [StateIndependent]
        public bool Equals(ValueArray<T> other) {
            if (other == null) return false;
            return _array.ValueEquals(other._array);
        }

        #endregion

        [Confined]
        public override bool Equals(object obj) {
            return Equals(obj as ValueArray<T>);
        }

        [Confined]
        public override int GetHashCode() {
            int val = 6551;

            for (int i = 0; i < _array.Length; i++) {
                val ^= _array[i].GetHashCode();
            }
            return val;
        }
    }
}
