using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Microsoft.Web.Scripting.Util;

namespace Microsoft.Web.Scripting.UI.Controls {
    public class BaseCodeControl: Control {
        private string _code;
        private int _line;

        public string Code {
            get { return _code; }
            set { _code = value; }
        }

        public int Line {
            get { return _line; }
            set { _line = value; }
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // This ensures that the template control is properly initialized
            ScriptTemplateControl.GetScriptTemplateControl(this);
        }

        internal new Control FindControl(string id) {
            Control control = base.FindControl(id);
            if (control == null) {
                Misc.ThrowException("Can't find control '" + id + "'", null,
                    TemplateControl.AppRelativeVirtualPath, Line);
                return null;
            }

            return control;
        }
    }
}
