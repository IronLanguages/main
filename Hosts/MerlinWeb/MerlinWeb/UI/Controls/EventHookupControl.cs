using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace Microsoft.Web.Scripting.UI.Controls {
    // This control handles event handler hookup attributes.  e.g. onClick="SomeHandlerMethod"
    public class EventHookupControl : BaseCodeControl {
        private string _targetId;
        private string _eventName;
        private string _handlerName;

        public string TargetId {
            get { return _targetId; }
            set { _targetId = value; }
        }

        public string EventName {
            get { return _eventName; }
            set { _eventName = value; }
        }

        public string HandlerName {
            get { return _handlerName; }
            set { _handlerName = value; }
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            Control targetControl = FindControl(_targetId);
            ScriptTemplateControl scriptTemplateControl = ScriptTemplateControl.GetScriptTemplateControl(this);
            scriptTemplateControl.HookupControlEvent(targetControl, _eventName, _handlerName, Line);
        }
    }
}
