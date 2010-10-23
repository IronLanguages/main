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

using System.Runtime.CompilerServices;
using System.Xml.XPath;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.AspNet.MembersInjectors;

// REVIEW: this is currently broken since injectors don't work on interfaces (Bug 222072)
[assembly: ExtensionType(typeof(IXPathNavigable), typeof(XPathNavigableMembersInjector))]
namespace Microsoft.Scripting.AspNet.MembersInjectors {
    public static class XPathNavigableMembersInjector {
        [SpecialName]
        public static object GetBoundMember(IXPathNavigable xPathNavigable, string name) {
            XPathNavigator navigator = xPathNavigable.CreateNavigator();
            return navigator.GetAttribute(name, navigator.NamespaceURI);
        }
    }
}
