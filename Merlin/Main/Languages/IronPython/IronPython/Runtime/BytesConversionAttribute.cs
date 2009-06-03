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
using System.Text;

namespace IronPython {
    /// <summary>
    /// Marks that the argument is typed to accept a bytes or bytearray object.  This
    /// attribute disallows passing a Python list object and auto-applying our generic
    /// conversion.  It also enables conversion of a string to a IList of byte in
    /// IronPython 2.6.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BytesConversionAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class BytesConversionNoStringAttribute : Attribute {
    }
}
