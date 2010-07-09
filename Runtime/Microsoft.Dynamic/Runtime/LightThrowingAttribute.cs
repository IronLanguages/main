/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Marks a method which may return a light exception.  Such
    /// methods need to have their return value checked and the exception
    /// will need to be thrown if the caller is not light exception aware.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LightThrowingAttribute : Attribute {
    }
}
