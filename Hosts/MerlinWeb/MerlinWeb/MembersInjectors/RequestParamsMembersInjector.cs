using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.Scripting.Runtime;
using Microsoft.Web.Scripting.MembersInjectors;

[assembly: ExtensionType(typeof(HttpRequest), typeof(RequestParamsMembersInjector))]
namespace Microsoft.Web.Scripting.MembersInjectors {
    public static class RequestParamsMembersInjector {
        [SpecialName]
        public static object GetBoundMember(HttpRequest request, string name) {            
            return ((object)request.Params[name]);
        }
    }
}
