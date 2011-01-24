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
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;
using System.Web.Hosting;
using System.Web.UI;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.AspNet.Util;

namespace Microsoft.Scripting.AspNet.UI.Controls {

    // This control handles <% ... %> blocks
    public class SnippetControl : BaseCodeControl {

        private const string RenderMethodName = "__RenderMethod";

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Tell the parent to use a special render method instead of the normal logic.  Note that several
            // children can end up setting this on their parent, which is fine because the method handles
            // all the children, so it doesn't matter who sets the delegate
            Parent.SetRenderMethodDelegate(RenderMethod);
        }

        private void RenderMethod(HtmlTextWriter writer, Control container) {

            ScriptTemplateControl scriptTemplateControl = ScriptTemplateControl.GetScriptTemplateControl(container);

            // Get the compiled code for the render method logic
            CompiledCode compiledCode = scriptTemplateControl.GetSnippetRenderCode(container.UniqueID, this);

            // Execute it in our module
            EngineHelper.ExecuteCompiledCode(compiledCode, scriptTemplateControl.ScriptModule);

            // We should always find our render function in the module
            // REVIEW: we shouldn't have to do this, and should instead work with a lambda like mechanism (bug 218654)
            object f = scriptTemplateControl.ScriptModule.GetVariable(RenderMethodName);
            Debug.Assert(f != null);

            // Call the render method
            DynamicFunction renderFunction = new DynamicFunction(f);

            EngineHelper.CallMethod(scriptTemplateControl.ScriptEngine, renderFunction, null /*defaultVirtualPath*/,
                new SnippetRenderHelper(writer, container.Controls));
        }

        // Generate the CodeDom for our render method
        internal CodeMemberMethod GenerateRenderMemberMethod(string virtualPath) {

            Control container = Parent;

            string physicalPath = HostingEnvironment.MapPath(virtualPath);

            CodeMemberMethod renderMethod = new CodeMemberMethod();
            renderMethod.Name = RenderMethodName;
            renderMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "__srh"));

            // REVIEW: we need support for CodeArgumentReferenceExpression, as using a snippet is
            // not guanranteed to be language agnostic
            CodeExpression snippetRenderHelper = new CodeArgumentReferenceExpression("__srh");

            // Go through all the children to build the CodeDOM tree
            for (int controlIndex = 0; controlIndex < container.Controls.Count; controlIndex++) {
                Control c = container.Controls[controlIndex];

                if (!(c is SnippetControl || c is ExpressionSnippetControl)) {

                    // If it's a regular control, generate a call to render it based on its index

                    CodeExpression method = new CodeMethodInvokeExpression(snippetRenderHelper, "RenderControl",
                        new CodePrimitiveExpression(controlIndex));
                    renderMethod.Statements.Add(new CodeExpressionStatement(method));

                    continue;
                }

                BaseCodeControl codeControl = (BaseCodeControl)c;

                string code = codeControl.Code;
                CodeStatement stmt;

                if (codeControl is SnippetControl) {

                    // If it's a <% code %> block, just append the code as is

                    stmt = new CodeSnippetStatement(code);
                } else {

                    // If it's a <%= expr %> block, generate a call to render it

                    CodeExpression method = new CodeMethodInvokeExpression(snippetRenderHelper, "Render",
                        new CodeSnippetExpression(code));
                    stmt = new CodeExpressionStatement(method);
                }

                stmt.LinePragma = new CodeLinePragma(physicalPath, codeControl.Line);
                renderMethod.Statements.Add(stmt);
            }

            return renderMethod;
        }
    }

    // Class passed as parameter to the generated render methods.  The render method calls back into
    // this class to render code expressions and controls
    public class SnippetRenderHelper {
        private HtmlTextWriter _writer;
        private ControlCollection _controls;

        internal SnippetRenderHelper(HtmlTextWriter writer, ControlCollection controls) {
            _writer = writer;
            _controls = controls;
        }

        public void Render(object o) {
            _writer.Write(o);
        }

        public void RenderControl(int index) {
            _controls[index].RenderControl(_writer);
        }
    }

}
