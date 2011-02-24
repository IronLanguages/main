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
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.AspNet.Util;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.AspNet {
    class EventHookupHelper {
        private string _handlerName;
        private EventInfo _eventInfo;
        private string _scriptVirtualPath;

        internal static EventHookupHelper Create(Type type, string eventName, string handlerName,
            DynamicFunction f, string scriptVirtualPath) {

            EventInfo eventInfo = GetEventInfo(type, eventName, f, scriptVirtualPath);
            if (eventInfo == null)
                return null;

            return new EventHookupHelper(handlerName, eventInfo, scriptVirtualPath);
        }

        internal static EventInfo GetEventInfo(Type type, string eventName,
            DynamicFunction f, string scriptVirtualPath) {

            EventInfo eventInfo = type.GetEvent(eventName);

            // If it doesn't match an event name, just ignore it
            if (eventInfo == null)
                return null;

            return eventInfo;
        }

        internal EventHookupHelper(string handlerName, EventInfo eventInfo,
            string scriptVirtualPath) {
            _handlerName = handlerName;
            _eventInfo = eventInfo;
            _scriptVirtualPath = scriptVirtualPath;
        }

        internal void HookupHandler(IBuildProvider provider, ScriptScope moduleGlobals, object target) {

            DynamicFunction scriptFunction = new DynamicFunction((object)moduleGlobals.GetVariable(_handlerName));

            Delegate handler = EventHandlerWrapper.GetWrapper(
                provider, scriptFunction, _scriptVirtualPath, typeof(EventHandler));
            _eventInfo.AddEventHandler(target, handler);
        }
    }
}
