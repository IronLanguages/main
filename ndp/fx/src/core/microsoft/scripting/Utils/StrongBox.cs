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

    public sealed class StrongBox<T> : IStrongBox {
        /// <summary>
        /// Gets the strongly typed value associated with the StrongBox.  This is explicitly
        /// exposed as a field instead of a property to enable loading the address of the field.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public T Value;

        /// <summary>
        /// Creates a new StrongBox which can receive a value when used in a reference call.
        /// TODO: review this new API
        /// </summary>
        public StrongBox() {
        }

        /// <summary>
        /// Creates a new StrongBox with the specified value.
        /// </summary>
        /// <param name="value"></param>
        public StrongBox(T value) {
            Value = value;
        }

        object IStrongBox.Value {
            get {
                return Value;
            }
            set {
                Value = (T)value;
            }
        }
    }

    public interface IStrongBox {
        object Value { get; set; }
    }
}
