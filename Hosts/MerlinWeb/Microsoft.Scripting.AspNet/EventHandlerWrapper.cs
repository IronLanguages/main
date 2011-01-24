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
using System.Diagnostics;
using System.Web;
using Microsoft.Scripting;
using Microsoft.Scripting.AspNet.Util;
using Microsoft.Scripting.Hosting;

namespace Microsoft.Scripting.AspNet {
    class EventHandlerWrapper {
        private static MethodInfo _handlerMethodInfo;
        private DynamicFunction _f;
        private string _virtualPath;
        private IBuildProvider _provider;

        public static Delegate GetWrapper(IBuildProvider provider, DynamicFunction f,
            string virtualPath, Type delegateType) {

            // Get the MethodInfo only once
            if (_handlerMethodInfo == null) {
                _handlerMethodInfo = typeof(EventHandlerWrapper).GetMethod("Handler");
            }

            EventHandlerWrapper wrapper = new EventHandlerWrapper(provider, f, virtualPath);

            // Create a delegate of the required type
            return Delegate.CreateDelegate(delegateType, wrapper, _handlerMethodInfo);
        }

        public EventHandlerWrapper(IBuildProvider provider) { _provider = provider;  }

        public EventHandlerWrapper(IBuildProvider provider, DynamicFunction f, string virtualPath) {
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
