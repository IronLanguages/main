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

using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.AspNet.MembersInjectors;

[assembly:ExtensionType(typeof(NameValueCollection), typeof(NameValueCollectionMembersInjector ))]

namespace Microsoft.Scripting.AspNet.MembersInjectors {
    public static class NameValueCollectionMembersInjector {
        [SpecialName]
        public static object GetBoundMember(NameValueCollection coll, string name) {
            return ((object)coll[name]);
        }
    }
}
