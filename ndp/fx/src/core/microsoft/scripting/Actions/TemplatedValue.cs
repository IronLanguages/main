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

namespace System.Runtime.CompilerServices {
    internal interface ITemplatedValue {
        int Index { get; }
        object CopyWithNewValue(object value);
        object ObjectValue { get; }
    }

    public class TemplatedValue<T> : ITemplatedValue {
        private T _value;
        private int _index;

        // constructor is internal as this type should be used by DLR only
        internal TemplatedValue(T value, int index) {
            _index = index;
            _value = value;
        }

        internal static TemplatedValue<T> Make(object value, int index) {
            return new TemplatedValue<T>((T)value, index);
        }

        public T Value {
            get {
                return _value;
            }
        }

        #region ITemplatedValue Members

        int ITemplatedValue.Index {
            get {
                return _index;
            }
        }

        object ITemplatedValue.CopyWithNewValue(object value) {
            return new TemplatedValue<T>((T)value, _index);
        }

        object ITemplatedValue.ObjectValue {
            get {
                return _value;
            }
        }

        #endregion
    }
}
