using System.Runtime.CompilerServices;
using System.Xml.XPath;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;

// REVIEW: this is currently broken since injectors don't work on interfaces (Bug 222072)
[assembly: ExtensionType(typeof(IXPathNavigable), typeof(XPathNavigableMembersInjector))]
namespace Microsoft.Web.Scripting.MembersInjectors {
    public static class XPathNavigableMembersInjector {
        [SpecialName]
        public static object GetBoundMember(IXPathNavigable xPathNavigable, string name) {
            XPathNavigator navigator = xPathNavigable.CreateNavigator();
            return navigator.GetAttribute(name, navigator.NamespaceURI);
        }
    }
}
