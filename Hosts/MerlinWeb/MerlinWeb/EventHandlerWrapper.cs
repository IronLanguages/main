using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Web;
using Microsoft.Scripting;
using Microsoft.Web.Scripting.Util;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Web.Scripting {
    class EventHandlerWrapper {
        private static MethodInfo s_handlerMethodInfo;
        private object _self;
        private DynamicFunction _f;
        private string _virtualPath;
        private IBuildProvider _provider;

        public static Delegate GetWrapper(IBuildProvider provider, object self, DynamicFunction f,
            string virtualPath, Type delegateType) {

            // Get the MethodInfo only once
            if (s_handlerMethodInfo == null) {
                s_handlerMethodInfo = typeof(EventHandlerWrapper).GetMethod("Handler");
            }

            EventHandlerWrapper wrapper = new EventHandlerWrapper(provider, self, f, virtualPath);

            // Create a delegate of the required type
            return Delegate.CreateDelegate(delegateType, wrapper, s_handlerMethodInfo);
        }

        public EventHandlerWrapper(IBuildProvider provider) { _provider = provider;  }

        public EventHandlerWrapper(IBuildProvider provider, object self, DynamicFunction f, string virtualPath) {
            _self = self;
            _f = f;
            _virtualPath = virtualPath;
            _provider = provider;
        }

        public void SetDynamicFunction(DynamicFunction f, string virtualPath) {
            _f = f;
            _virtualPath = virtualPath;
        }

        public void Handler(object sender, object eventArgs) {

            // No function: nothing to do
            if (_f == null)
                return;
            
            EngineHelper.CallMethod(_provider.GetScriptEngine(), _f, _virtualPath, new object[] { sender, eventArgs });
        }
    }
}
