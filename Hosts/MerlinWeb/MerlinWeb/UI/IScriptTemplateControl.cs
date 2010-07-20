using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;

namespace Microsoft.Web.Scripting.UI {
    public interface IScriptTemplateControl {
        ScriptTemplateControl ScriptTemplateControl { get; }

        string InlineScript { get; set; }
        int InlineScriptLine { get; set; }
    }
}
