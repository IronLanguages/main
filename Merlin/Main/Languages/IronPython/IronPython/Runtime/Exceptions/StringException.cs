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
using System.Runtime.Serialization;

namespace IronPython.Runtime.Exceptions {
    /// <summary>
    /// Wrapper exception used for when the user wants to raise a string as an exception.
    /// </summary>
    [Serializable]
    public sealed class StringException : Exception, IPythonException {
        object value;

        public StringException() { }

        public StringException(string message)
            : base(message) {
            value = message;
        }

        public StringException(string name, object value)
            : base(name) {
            this.value = value;
        }

        public StringException(string message, Exception innerException)
            : base(message, innerException) {
        }

#if !SILVERLIGHT // SerializationInfo
        private StringException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            value = info.GetValue("value", typeof(object));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("value", value);

            base.GetObjectData(info, context);
        }
#endif

        public override string ToString() {
            return base.Message;
        }

        public object Value {
            get {
                return value;
            }
        }

        #region IPythonException Members

        public object ToPythonException() {
            return this;
        }

        #endregion
    }
}
