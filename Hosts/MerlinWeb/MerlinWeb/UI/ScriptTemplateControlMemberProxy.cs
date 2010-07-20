using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Web.Scripting.UI {
    // This implementation merges attributes from two sources:
    // 1. Real object properties (via IStrongBox)
    // 2. Module globals (essentially, instance fields)
    // This class is not directly exposed to user code, instead it's used as the backing for ScriptPage/ScriptMaster/ScriptUserControl.
    internal class ScriptTemplateControlMemberProxy {
        private ScriptTemplateControlDictionary _moduleGlobals;
        private object _self;

        internal ScriptTemplateControlMemberProxy(object self, ScriptTemplateControlDictionary moduleGlobals) {
            _self = self;
            _moduleGlobals = moduleGlobals;
        }

        public object GetBoundMember(string name) {
            object value;
            if (_moduleGlobals != null) {
                if (_moduleGlobals.TryGetValueFromModuleGlobals(name, out value)) {
                    return value;
                }
            }

            return OperationFailed.Value;
        }

        public void SetMemberAfter(string name, object value) {
            _moduleGlobals.InjectToken(name, value);
        }

        public bool DeleteMember(string name) {
            return _moduleGlobals.RemoveToken(name);
        }

        // REVIEW: we should implement IMemberList and return our names
    }
}
