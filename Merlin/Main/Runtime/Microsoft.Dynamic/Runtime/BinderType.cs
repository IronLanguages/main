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

namespace Microsoft.Scripting.Runtime {
    public enum BinderType {
        /// <summary>
        /// The MethodBinder will perform normal method binding.
        /// </summary>
        Normal,
        /// <summary>
        /// The MethodBinder will return the languages definition of NotImplemented if the arguments are
        /// incompatible with the signature.
        /// </summary>
        BinaryOperator,
        ComparisonOperator,
        /// <summary>
        /// The MethodBinder will set properties/fields for unused keyword arguments on the instance 
        /// that gets returned from the method.
        /// </summary>
        Constructor
    }

}
