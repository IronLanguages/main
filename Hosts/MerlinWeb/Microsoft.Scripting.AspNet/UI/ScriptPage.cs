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
using System.Runtime.CompilerServices;
using System.Web.SessionState;
using System.Web.UI;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.AspNet.MembersInjectors;

namespace Microsoft.Scripting.AspNet.UI {
    public class ScriptPage : Page, IScriptTemplateControl, IRequiresSessionState {
        private ScriptTemplateControl _scriptTemplateControl;

        protected override void OnInit(EventArgs e) {
            // Make sure we're initialized before calling the base so that any OnInit handler
            // is properly registered in time.
            // Don't do anything if we don't have a ScriptLanguage (i.e. it's not a dynamic page)
            if (ScriptLanguage != null) {
                EnsureScriptTemplateControl();

                // If there are no controls, the page is meant to be a simple handler, and we can skip
                // the base.OnInit call.  We need to do this to avoid breaking if there is a Theme, since
                // that requires a <head> tag in the page.
                // NOTE: an unfortunate side effect of this fix is that Page_Init is not called.  It should
                // be ok if the top level code model is used for handlers
                if (Controls.Count == 0)
                    return;
            }

            base.OnInit(e);
        }

        // This allows the language to be specified in the page directive (e.g. <%@ Page scriptlanguage="IronPython" %>)
        private string _scriptLanguage;
        public string ScriptLanguage {
            get { return _scriptLanguage; }
            set { _scriptLanguage = value; }
        }

        [SpecialName]
        public object GetBoundMember(string name) {
            object res = ControlMembersInjector.GetBoundMember(this, name);
            if (res != OperationFailed.Value) {
                return res;
            }

            return EnsureScriptTemplateControl().MemberProxy.GetBoundMember(name);
        }

        [SpecialName]
        public void SetMemberAfter(string name, object value) {
            EnsureScriptTemplateControl().MemberProxy.SetMemberAfter(name, value);
        }

        [SpecialName]
        public bool DeleteMember(string name) {
            return EnsureScriptTemplateControl().MemberProxy.DeleteMember(name);
        }
                
        ScriptTemplateControl EnsureScriptTemplateControl() {
            // Initialize it on demand
            if (_scriptTemplateControl == null) {
                _scriptTemplateControl = new ScriptTemplateControl(this);
                _scriptTemplateControl.HookUpScript(ScriptLanguage);
            }

            return _scriptTemplateControl;
        }

        #region IScriptTemplateControl Members
        public virtual ScriptTemplateControl ScriptTemplateControl {
            get { return this.EnsureScriptTemplateControl(); }
        }

        private string _inlineScript;
        public virtual string InlineScript {
            get { return _inlineScript; }
            set { _inlineScript = value; }
        }

        private int _inlineScriptLine;
        public virtual int InlineScriptLine {
            get { return _inlineScriptLine; }
            set { _inlineScriptLine = value; }
        }
        #endregion

        // TemplateControl.Eval is not virtual, so we need to use 'new' to override it
        public new object Eval(string expression) {

            object dataItem = EnsureScriptTemplateControl().GetDataItem();

            // If we don't have a data item, just call the standard Eval
            if (dataItem == null)
                return base.Eval(expression);

            return DataBinder.Eval(dataItem, expression);
        }

        // Like Eval, but return a string instead of object
        public string EvalS(string expression) {
            return Eval(expression).ToString();
        }

        // This is needed to expose the view state, since the base property is protected
        public new StateBag ViewState {
            get {
                return base.ViewState;
            }
        }
    }
}