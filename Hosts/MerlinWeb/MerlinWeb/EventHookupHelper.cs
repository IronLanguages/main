using System;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Web.Scripting.Util;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Web.Scripting {
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

        internal void HookupHandler(IBuildProvider provider, ScriptScope moduleGlobals, object self, object target) {

            DynamicFunction scriptFunction = new DynamicFunction((object)moduleGlobals.GetVariable(_handlerName));

            Delegate handler = EventHandlerWrapper.GetWrapper(
                provider, self, scriptFunction, _scriptVirtualPath, typeof(EventHandler));
            _eventInfo.AddEventHandler(target, handler);
        }
    }
}
