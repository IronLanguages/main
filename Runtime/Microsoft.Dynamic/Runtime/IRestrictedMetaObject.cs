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
using System.Dynamic;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Indicates that a MetaObject is already representing a restricted type.  Useful
    /// when we're already restricted to a known type but this isn't captured in
    /// the type info (e.g. the type is not sealed).
    /// </summary>
    public interface IRestrictedMetaObject {
        DynamicMetaObject Restrict(Type type);
    }
}
