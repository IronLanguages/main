using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;

[assembly:ExtensionType(typeof(NameValueCollection), typeof(NameValueCollectionMembersInjector ))]

namespace Microsoft.Web.Scripting.MembersInjectors {
    public static class NameValueCollectionMembersInjector {
        [SpecialName]
        public static object GetBoundMember(NameValueCollection coll, string name) {
            return ((object)coll[name]);
        }
    }
}
