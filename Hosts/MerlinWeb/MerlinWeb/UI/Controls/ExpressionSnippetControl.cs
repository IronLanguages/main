using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace Microsoft.Web.Scripting.UI.Controls {
    // This control handles <%= ... %> blocks
    public class ExpressionSnippetControl : BaseCodeControl {
        protected override void Render(HtmlTextWriter writer) {
            ScriptTemplateControl scriptTemplateControl = ScriptTemplateControl.GetScriptTemplateControl(this);
            object result = scriptTemplateControl.EvaluateExpression(Code.Trim(), Line);
            writer.Write(System.Convert.ToString(result));
        }
    }
}
