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
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace Microsoft.Scripting.AspNet.UI.Controls {
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
