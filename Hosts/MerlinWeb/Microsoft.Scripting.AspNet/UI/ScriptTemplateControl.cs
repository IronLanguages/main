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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.AspNet.UI.Controls;
using Microsoft.Scripting.AspNet.Util;

namespace Microsoft.Scripting.AspNet.UI {

    public class ScriptTemplateControl : IBuildProvider {
        private TemplateControl _templateControl;
        private ScriptTemplateControlMemberProxy _attribs;
        private string _templateControlVirtualPath;
        private string _scriptVirtualPath;
        private bool _inlineScript;
        private TemplateControlBuildResult _buildResult;
        private ScriptScope _scope;
        private ScriptEngine _scriptEngine;
        private ScriptTemplateControlDictionary _scopeDictionary;
        private object _dataItem;
        private object _bindingContainer;
        private bool _dataBinding;
        private static ScriptEngine _defaultEngine;

        // Get the ScriptTemplateControl object from a TemplateControl
        internal static ScriptTemplateControl GetScriptTemplateControl(Control c) {
            TemplateControl templateControl = c.TemplateControl;
            IScriptTemplateControl iscriptTemplateControl = templateControl as IScriptTemplateControl;
            if (iscriptTemplateControl == null) {
                throw new Exception("The page '" + templateControl.AppRelativeVirtualPath +
                    "' doesn't have the expected script base type (i.e. ScriptPage, ScriptUserControl or ScriptMaster)");
            }

            return iscriptTemplateControl.ScriptTemplateControl;
        }

        internal ScriptTemplateControl(TemplateControl templateControl) {
            _templateControl = templateControl;
        }

        internal ScriptTemplateControlMemberProxy MemberProxy { get { return _attribs; } }

        public ScriptScope ScriptModule { get { return _scope; } }
        public ScriptEngine ScriptEngine { get { return _scriptEngine; } }

        internal void HookUpScript(string scriptLanguage) {

            if (scriptLanguage == null) {
                // TODO: Need to revise this, we need to use the InvariantContext here probably
                // but it can't actually do calls yet.
                _scriptEngine = _defaultEngine;
                return;
            }

            _scriptEngine = EngineHelper.GetScriptEngineByName(scriptLanguage);
            _defaultEngine = _defaultEngine ?? _scriptEngine;

            _templateControlVirtualPath = VirtualPathUtility.ToAbsolute(_templateControl.AppRelativeVirtualPath);

            // First, look for a code behind file named <pagename>.aspx.<ext>, where 'ext' is the extension
            // for the page's language
            _inlineScript = false;
            IList<string> extensions = _scriptEngine.Setup.FileExtensions;
            foreach (string extension in extensions) {
                _scriptVirtualPath = _templateControlVirtualPath + extension;
                if (HookUpScriptFile()) {
                    return;
                }
            }

            // Then, look for inline script
            _inlineScript = true;
            _scriptVirtualPath = _templateControlVirtualPath;
            HookUpScriptFile();
        }

        private bool HookUpScriptFile() {
            _buildResult = (TemplateControlBuildResult)EngineHelper.GetBuildResult(_scriptVirtualPath, this);

            // No script: nothing to do
            if (_buildResult == null || _buildResult.CompiledCode == null)
                return false;

            _scopeDictionary = new ScriptTemplateControlDictionary(_templateControl, this);
            _scope = EngineHelper.ScriptRuntime.CreateScope(_scopeDictionary);

            _attribs = new ScriptTemplateControlMemberProxy(_scopeDictionary);

            EngineHelper.ExecuteCode(_scope, _buildResult.CompiledCode, _buildResult.ScriptVirtualPath);

            _buildResult.InitMethods(_templateControl.GetType(), _scope);

            _buildResult.HookupEvents(this, _scope, _templateControl);

            return true;
        }

        internal void HookupControlEvent(Control control, string eventName, string handlerName, int line) {
            EventInfo eventInfo = control.GetType().GetEvent(eventName);
            if (eventInfo == null) {
                throw new Exception("Control '" + control.ID + "' doesn't have an event named '" +
                    eventName + "'");
            }

            object o = null;
            if (_scopeDictionary == null || !_scopeDictionary.TryGetValue(handlerName, out o)) {
                Misc.ThrowException("The page doesn't have an event handler named  '" + handlerName + "'",
                    null, _templateControl.AppRelativeVirtualPath, line);
            }
            DynamicFunction handlerFunction = new DynamicFunction(o);

            Delegate handler = EventHandlerWrapper.GetWrapper(this, handlerFunction, _scriptVirtualPath, eventInfo.EventHandlerType);
            eventInfo.AddEventHandler(control, handler);
        }

        #region IBuildProvider Members
        ScriptEngine IBuildProvider.GetScriptEngine() {
            return _scriptEngine;
        }
        
        string IBuildProvider.GetScriptCode() {

            // If the file doesn't exist, there is no script code
            if (!HostingEnvironment.VirtualPathProvider.FileExists(_scriptVirtualPath)) {

                // If it's the aspx/ascx/master file itself that doesn't exist, we're
                // probably dealing with a precompiled app, which we don't support.
                // REVIEW: Find a better way of detecting a precompiled app, like PrecompiledApp.config
                //if (_inlineScript) {
                //    throw new Exception("Precompilation is not supported on dynamic language pages");
                //}

                return null;
            }

            if (_inlineScript) {
                // If it's inline, we need to extract the script from the page
                return GetScriptFromTemplateControl();
            } else {
                return Util.Misc.GetStringFromVirtualPath(_scriptVirtualPath);
            }
        }

        BuildResult IBuildProvider.CreateBuildResult(CompiledCode compiledCode, string scriptVirtualPath) {
            return new TemplateControlBuildResult(compiledCode, scriptVirtualPath);
        }
        #endregion

        private string GetScriptFromTemplateControl() {
            IScriptTemplateControl iscriptTemplateControl = (IScriptTemplateControl)_templateControl;
            string code = iscriptTemplateControl.InlineScript;
            if (code == null)
                return String.Empty;

            StringBuilder builder = new StringBuilder();

            // Append enough blank lines to reach the start line of the script block in the page
            for (int line = 1; line < iscriptTemplateControl.InlineScriptLine; line++) {
                builder.AppendLine();
            }

            builder.Append(code);

            return builder.ToString();
        }

        // Note that dataItem can be null, in which case we go against Page.GetDataItem()
        public void SetDataItem(object dataItem) {
            Debug.Assert(_dataItem == null && !_dataBinding);
            _dataItem = dataItem;
            _dataBinding = true;
        }

        public void ClearDataItem() {
            Debug.Assert(_dataBinding);
            _dataItem = null;
            _dataBinding = false;
        }

        internal object GetDataItem() {

            // Don't try anything if we're not databinding
            if (!_dataBinding)
                return null;

            // If we have our own data item, use it
            if (_dataItem != null)
                return _dataItem;

            // Otherwise, use the data item on the page stack
            return _templateControl.Page.GetDataItem();
        }

        internal object BindingContainer { get { return _bindingContainer; } }

        internal object EvaluateDataBindingExpression(Control c, string expr, int line) {

            // Try to get a data item
            _bindingContainer = c.BindingContainer;
            Debug.Assert(_bindingContainer != null);
            object dataItem = DataBinder.GetDataItem(_bindingContainer);

            // If we got one, make it available
            if (dataItem != null)
                SetDataItem(dataItem);
            try {
                return EvaluateExpression(expr, line);
            } finally {
                if (dataItem != null)
                    ClearDataItem();
                _bindingContainer = null;
            }
        }

        internal object EvaluateExpression(string expr, int line) {
            CompiledCode compiledExpression = (CompiledCode) _buildResult.CompiledSnippetExpressions[expr];

            if (compiledExpression == null) {
                lock (_buildResult.CompiledSnippetExpressions) {
                    compiledExpression = (CompiledCode)_buildResult.CompiledSnippetExpressions[expr];
                    if (compiledExpression == null) {
                        // Need to subtract one from the line since it's 1-based and we want an offset
                        compiledExpression = EngineHelper.CompileExpression(expr, ScriptEngine, _templateControlVirtualPath, line - 1);
                        _buildResult.CompiledSnippetExpressions[expr] = compiledExpression;
                    }
                }
            }

            return EngineHelper.EvaluateCompiledCode(compiledExpression, _scope,
                _templateControlVirtualPath, line);
        }

        internal CompiledCode GetSnippetRenderCode(string cacheKey, SnippetControl snippetControl) {
            CompiledCode compiledSnippet = (CompiledCode)_buildResult.CompiledSnippets[cacheKey];

            if (compiledSnippet == null) {
                lock (_buildResult.CompiledSnippets) {
                    compiledSnippet = (CompiledCode)_buildResult.CompiledSnippets[cacheKey];
                    if (compiledSnippet == null) {
                        // It's not cached, so ask the control for the CodeDom tree and compile it
                        CodeMemberMethod codeDomMethod = snippetControl.GenerateRenderMemberMethod(_templateControlVirtualPath);
                        compiledSnippet = EngineHelper.CompileCodeDom(codeDomMethod, ScriptEngine);

                        // Cache it
                        _buildResult.CompiledSnippets[cacheKey] = compiledSnippet;
                    }
                }
            }

            return compiledSnippet;
        }

        internal void SetProperty(string propertyName, object value) {

            // Try to get a dynamic setter for this property
            DynamicFunction setterFunction = GetPropertySetter(propertyName);

            // If we couldn't find a setter, just set a field by that name
            if (setterFunction == null) {
                _scope.SetVariable(propertyName, value);
                return;
            }

            // Set the property value
            CallFunction(setterFunction, value);
        }

        private DynamicFunction GetPropertySetter(string propertyName) {

            // Prepend "Set" to get the method name
            // REVIEW: is this the right naming pattern?
            string setterName = "Set" + propertyName;

            object setterFunction;
            if (!_scope.TryGetVariable(setterName, out setterFunction))
                return null;

            return new DynamicFunction(setterFunction);
        }

        public DynamicFunction GetFunction(string name) {
            if (ScriptModule == null)
                return null;

            object val;
            if (!ScriptModule.TryGetVariable(name, out val))
                return null;

            return new DynamicFunction(val);
        }

        public object CallFunction(DynamicFunction f, params object[] args) {
            return EngineHelper.CallMethod(_scriptEngine, f, _scriptVirtualPath, args);
        }

        public object CallDataBindingFunction(DynamicFunction f, params object[] args) {
            return CallDataBindingFunction((object)null, f, args);
        }

        public object CallDataBindingFunction(object dataItem, DynamicFunction f, params object[] args) {
            // We need to inject the data item in the globals dictionary to make it available
            SetDataItem(dataItem);
            try {
                return CallFunction(f, args);
            } finally {
                ClearDataItem();
            }
        }

        class TemplateControlBuildResult : TypeWithEventsBuildResult {
            private const string HandlerPrefix = "Page_";
            private List<EventHookupHelper> _eventHandlers;
            private Hashtable _compiledSnippetExpressions; // <string, CompiledCode>
            private Hashtable _compiledSnippets; // <string, CompiledCode>

            internal TemplateControlBuildResult(CompiledCode compiledCode, string scriptVirtualPath)
                : base(compiledCode, scriptVirtualPath) { }

            // Dictionary of <%= ... %> expression blocks
            internal IDictionary CompiledSnippetExpressions {
                get {
                    if (_compiledSnippetExpressions == null)
                        _compiledSnippetExpressions = new Hashtable();
                    return _compiledSnippetExpressions;
                }
            }

            // Dictionary of <% ... %> code blocks
            internal IDictionary CompiledSnippets {
                get {
                    if (_compiledSnippets == null)
                        _compiledSnippets = new Hashtable();
                    return _compiledSnippets;
                }
            }

            internal override bool ProcessEventHandler(string handlerName, Type type, DynamicFunction f) {
                // Does it look like a handler?
                if (!handlerName.StartsWith(HandlerPrefix))
                    return false;

                string eventName = handlerName.Substring(HandlerPrefix.Length);

                EventHookupHelper helper = EventHookupHelper.Create(type, eventName,
                    handlerName, f, ScriptVirtualPath);
                if (helper == null)
                    return false;

                if (_eventHandlers == null)
                    _eventHandlers = new List<EventHookupHelper>();

                _eventHandlers.Add(helper);

                return true;
            }

            internal void HookupEvents(IBuildProvider provider, ScriptScope moduleGlobals, object o) {

                if (_eventHandlers == null)
                    return;

                foreach (EventHookupHelper helper in _eventHandlers) {
                    helper.HookupHandler(provider, moduleGlobals, o);
                }
            }
        }
    }
}
