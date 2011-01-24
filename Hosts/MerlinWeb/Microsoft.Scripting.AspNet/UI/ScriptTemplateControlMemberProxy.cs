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

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.AspNet.UI {
    // This implementation merges attributes from two sources:
    // 1. Real object properties (via IStrongBox)
    // 2. Module globals (essentially, instance fields)
    // This class is not directly exposed to user code, instead it's used as the backing for ScriptPage/ScriptMaster/ScriptUserControl.
    internal class ScriptTemplateControlMemberProxy {
        private ScriptTemplateControlDictionary _moduleGlobals;

        internal ScriptTemplateControlMemberProxy(ScriptTemplateControlDictionary moduleGlobals) {
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
