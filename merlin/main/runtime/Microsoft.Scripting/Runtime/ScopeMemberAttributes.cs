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
    /// Common attributes used to control attributes of a Scope.
    /// </summary>
    [Flags]
    public enum ScopeMemberAttributes {
        /// <summary>
        /// The member has no Scope attributes.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// The member can only be read from and cannot be written to
        /// </summary>
        ReadOnly = 0x0001,
        /// <summary>
        /// The member can be read from or written to but cannot be deleted
        /// </summary>
        DontDelete = 0x0002,
        /// <summary>
        /// The member can be read or written but is not visible in the displayed list of members.
        /// </summary>
        DontEnumerate = 0x0004,
    }
}
