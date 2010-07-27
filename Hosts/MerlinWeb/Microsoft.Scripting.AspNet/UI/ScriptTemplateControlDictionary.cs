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

using System.Web;
using System.Dynamic;
using System.Collections.Generic;

namespace Microsoft.Scripting.AspNet.UI {
    // Dictionary used by ScriptTemplateControl objects
    internal class ScriptTemplateControlDictionary : DynamicObject {

        private object _templateControl;
        private ScriptTemplateControl _scriptTemplateControl;
        private readonly Dictionary<string, object> _dict = new Dictionary<string, object>();

        internal ScriptTemplateControlDictionary(object templateControl, ScriptTemplateControl scriptTemplateControl) {
            _templateControl = templateControl;
            _scriptTemplateControl = scriptTemplateControl;

            // Inject some known objects
            InjectTokens();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return TryGetValue(binder.Name, out result);   
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            _dict[binder.Name] = value;
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) {
            return _dict.Remove(binder.Name);
        }

        internal bool TryGetValue(string key, out object result) {
            // First, try the module globals
            if (TryGetValueFromModuleGlobals(key, out result))
                return true;

            // Then check if it can be handled via databinding
            if (TryGetValueFromDataBinding(key, out result))
                return true;

            // Finally, try resolving it as a something on the TemplateControl (or possibly
            // the FindControl injector)
            return _scriptTemplateControl.ScriptEngine.Operations.TryGetMember(_templateControl, key, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames() {
            foreach (var v in _dict) {
                yield return v.Key;
            }
        }

        private void InjectTokens() {
            // Support the special token "page" to refer to the associated Page/UserControl/Master
            InjectToken("page", _templateControl);

            // Make the profile available to match compiled pages (where it gets codegend as a property)
            InjectToken("Profile", HttpContext.Current.Profile);
        }

        internal void InjectToken(string key, object value) {
            _dict[key] = value;
        }

        internal bool RemoveToken(string key) {
            return _dict.Remove(key);
        }

        internal bool TryGetValueFromModuleGlobals(string key, out object value) {
            return _dict.TryGetValue(key, out value);
        }

        private bool TryGetValueFromDataBinding(string key, out object value) {

            // Support the special token 'Container' for the binding container, for consistency with compiled apps
            if (_scriptTemplateControl.BindingContainer != null && key== "Container") {
                value = _scriptTemplateControl.BindingContainer;
                return true;
            }

            // Try the data item, if any
            object dataItem = _scriptTemplateControl.GetDataItem();
            if (dataItem != null) {
                if (_scriptTemplateControl.ScriptEngine.Operations.TryGetMember(dataItem, key, out value))
                    return true;

            // The following would support databinding against dictionary items
#if MAYBE
                IDictionary dict = dataItem as IDictionary;
                if (dict != null) {
                    string keyString = SymbolTable.IdToString(key);
                    if (dict.Contains(keyString)) {
                        value = dict[keyString];
                        return true;
                    }
                }
#endif
            }

            value = null;
            return false;
        }

    }
}
