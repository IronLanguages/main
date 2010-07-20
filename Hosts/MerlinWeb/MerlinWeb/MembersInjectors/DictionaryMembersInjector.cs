using System.Collections;
using System.Runtime.CompilerServices;
using System.Web.UI;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;

[assembly: ExtensionType(typeof(StateBag), typeof(DictionaryMembersInjector))]
namespace Microsoft.Web.Scripting.MembersInjectors {
    public static class DictionaryMembersInjector {
        [SpecialName]
        public static object GetMemberNames(IDictionary dict, string name) {
            return dict[name];
        }
    }
}
