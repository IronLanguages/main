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
using System.Web.UI;

namespace Microsoft.Scripting.AspNet.UI.Controls {
    // This control handles <%# ... %> island blocks (i.e. not as attribute values)
    public class DataBindingIslandControl : BaseCodeControl {
        public string Text {
            get {
                object o = ViewState["Text"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["Text"] = value;
            }
        }

        protected override void OnDataBinding(EventArgs e) {
            ScriptTemplateControl scriptTemplateControl = ScriptTemplateControl.GetScriptTemplateControl(this);
            object result = scriptTemplateControl.EvaluateDataBindingExpression(
                this, Code.Trim(), Line);
            Text = System.Convert.ToString(result);
        }

        protected override void Render(HtmlTextWriter writer) {
            writer.Write(Text);
        }
    }
}
