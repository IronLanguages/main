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
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.Configuration;
using Microsoft.Scripting.AspNet.UI.Controls;
using System.Web.Compilation;

namespace Microsoft.Scripting.AspNet.UI {
    public class NoCompileCodePageParserFilter : PageParserFilter {

        // Logic to check that the newer System.Web.dll is installed
        private static bool? s_hasCorrectSystemWeb;
        internal static void CheckCorrectSystemWebSupport() {
            // Only perform the check once (technically, it could happen multiple times in a race condition, and that's fine)
            if (s_hasCorrectSystemWeb == null) {
                MethodInfo m = typeof(PageParserFilter).GetMethod("GetNoCompileUserControlType");
                s_hasCorrectSystemWeb = (m != null);
            }

            if (!s_hasCorrectSystemWeb.Value) {
                throw new Exception("In order to run dynamic language pages, you need to install an updated version of ASP.NET. " +
                    "If running on Windows Vista, you will need to install 'Orcas' Beta 1 to get the update.");
            }
        }

        // Check whether we are being called because of a GenerateCodeCompileUnit call
        // REVIEW: it's extremely hacky to have to walk the stack to get that information, but
        // I'm not sure what alternative we have
        private static bool IsGenerateCodeCompileUnitCall() {
            StackTrace stackTrace = new StackTrace();
            foreach (StackFrame frame in stackTrace.GetFrames()) {
                if (frame.GetMethod().Name == "GenerateCodeCompileUnit") {
                    return true;
                }
            }
            return false;
        }

        private bool _isScriptPage = false;
        private string _script;
        private int _scriptLine;

        protected override void Initialize() {
            // Make sure we're running with a System.Web.dll that has the new API's we need
            CheckCorrectSystemWebSupport();
        }

        public override void PreprocessDirective(string directiveName, IDictionary attributes) {

            // If a script language is specified, we'll do our special handling
            string language = (string)attributes["language"];

            if (language != null && EngineHelper.IsDLRLanguage(language)) {

                // Remove the 'language' attribute to prevent ASP.NET from treating it as a CodeDom language
                attributes.Remove("language");

                // Instead, set a 'scriptlanguage', so ScriptPage will get the language at runtime (via its
                // 'ScriptLanguage' property.
                attributes["scriptlanguage"] = language;

                _isScriptPage = true;

                // If a codefile attribute is specified, make sure its name starts with the page's name.
                // REVIEW: do we really need this if we ignore it at runtime?  Maybe for designer purpose only
                string codeFile = (string)attributes["codefile"];

                if (codeFile != null) {
                    string fileName = VirtualPathUtility.GetFileName(VirtualPath);
                    if (!codeFile.StartsWith(fileName + ".", StringComparison.OrdinalIgnoreCase)) {
                        throw new Exception("The CodeFile's name must match the page's file name");
                    }

                    // Remove the codefile attribute since we don't want the parser to think we're
                    // dealing with a partial class
                    attributes.Remove("codefile");
                }

                // When handling a call to GenerateCodeCompileUnit, always pretend to be a compiled page,
                // otherwise no CodeCompileUnit is generated
                if (IsGenerateCodeCompileUnitCall()) {
                    attributes["compilationmode"] = "Always";
                }
            }
        }

        public override Type GetNoCompileUserControlType() {
            return typeof(Controls.UserControl);
        }

        public override void ParseComplete(ControlBuilder rootBuilder) {
            // If we found a script, keep track of it as page properties
            if (_script != null) {
                SetPageProperty(null /*filter*/, "inlinescript", _script);
                SetPageProperty(null /*filter*/, "inlinescriptline", _scriptLine.ToString());

            }
        }

        public override CompilationMode GetCompilationMode(CompilationMode current) {
            return current;
        }

        public override bool AllowCode {
            get {
                return true;
            }
        }

        public override bool AllowControl(Type controlType, ControlBuilder builder) {
            return true;
        }

        public override bool AllowBaseType(Type baseType) {
            return true;
        }

        public override bool AllowVirtualReference(string referenceVirtualPath, VirtualReferenceType referenceType) {
            return true;
        }

        // Is the passed in server include (<!-- #include -->) allowed
        public override bool AllowServerSideInclude(string includeVirtualPath) {
            return true;
        }

        public override int NumberOfControlsAllowed {
            get {
                // No limit
                return -1;
            }
        }

        public override int TotalNumberOfDependenciesAllowed {
            get {
                // No limit
                return -1;
            }
        }

        public override int NumberOfDirectDependenciesAllowed {
            get {
                // No limit
                return -1;
            }
        }

        public override bool ProcessCodeConstruct(CodeConstructType codeType, string code) {

            // Only do our special handling when dealing with a script page
            if (!_isScriptPage)
                return base.ProcessCodeConstruct(codeType, code);

            Type controlType = null;
            Hashtable attributes = new Hashtable();

            attributes["Code"] = code;
            attributes["Line"] = Line;

            switch (codeType) {
                case CodeConstructType.CodeSnippet:
                    controlType = typeof(SnippetControl);
                    break;
                case CodeConstructType.ExpressionSnippet:
                    controlType = typeof(ExpressionSnippetControl);
                    break;
                case CodeConstructType.DataBindingSnippet:
                    controlType = typeof(DataBindingIslandControl);
                    break;
                case CodeConstructType.ScriptTag:
                    if (_script != null) {
                        throw new Exception("The page should not contain more than one <script runat='server' tag");
                    }

                    _script = code;
                    _scriptLine = Line;
                    return true;
                default:
                    return false;
            }

            Debug.Assert(controlType != null);
            AddControl(controlType, attributes);

            return true;
        }

        public override bool ProcessDataBindingAttribute(string controlId, string attributeName, string code) {

            // Only do our special handling when dealing with a script page
            if (!_isScriptPage)
                return base.ProcessDataBindingAttribute(controlId, attributeName, code);

            if (String.IsNullOrEmpty(controlId)) {
                throw new Exception("The control must have an ID in order to use a databound attribute");
            }

            Hashtable attributes = new Hashtable();
            attributes["Code"] = code;
            attributes["TargetId"] = controlId;
            attributes["AttributeName"] = attributeName;
            attributes["Line"] = Line;

            AddControl(typeof(DataBindingControl), attributes);

            return true;
        }

        public override bool ProcessEventHookup(string controlId, string eventName, string handlerName) {

            // Only do our special handling when dealing with a script page
            if (!_isScriptPage)
                return base.ProcessEventHookup(controlId, eventName, handlerName);

            if (String.IsNullOrEmpty(controlId)) {
                throw new Exception("The control must have an ID in order to use event hookup");
            }

            Hashtable attributes = new Hashtable();
            attributes["TargetId"] = controlId;
            attributes["EventName"] = eventName;
            attributes["HandlerName"] = handlerName;
            attributes["Line"] = Line;

            AddControl(typeof(EventHookupControl), attributes);

            return true;
        }
    }
}

