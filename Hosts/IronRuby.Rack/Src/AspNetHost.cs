/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Web;
using System.Web.Routing;

namespace IronRubyRack {

    public class AspNetHandlerFactory : IHttpHandlerFactory {

        private static AspNetHandler _Handler;

        /// <summary>
        /// Sets up the environment. This is only run when the server starts up, not
        /// for each request, so as much as possible should be done here. Returns a
        /// HttpHandler capable of serving Rack requests.
        /// </summary>
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated) {
            if (_Handler == null) {
                var rack = new RackDispatcher(context);
                if (rack._failed)
                    return null;
                _Handler = new AspNetHandler(rack);
            }
            return _Handler;
        }

        public void ReleaseHandler(IHttpHandler/*!*/ handler) {
            if(_Handler != null && !_Handler.IsReusable) {
                _Handler = null;
            }
        }
    }

    public sealed class AspNetHandler : IHttpHandler {

        internal RackDispatcher _rack;

        public AspNetHandler(RackDispatcher rack) {
            _rack = rack;
        }

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            _rack.ProcessRequest(context);
        }
    }

    public class AspNetModule : IHttpModule {

        private RackDispatcher _rack;

        public void Init(HttpApplication app) {
            app.BeginRequest += (sender, args) => {
                var httpApp = (sender as HttpApplication);
                if (_rack == null) {
                    lock (this) {
                        if (_rack == null) {
                            _rack = new RackDispatcher(httpApp.Context);
                        }
                    }
                }
                _rack.ProcessRequest(httpApp.Context);
                httpApp.CompleteRequest();
            };
        }

        public void Dispose() {
            _rack = null;
        }
    }

    public class RackRouteHandler : IRouteHandler {

        private static IronRubyRack.RackDispatcher dispatcher;

        public IHttpHandler GetHttpHandler(RequestContext requestContext) {
            if (dispatcher == null)
                dispatcher = new IronRubyRack.RackDispatcher(System.Web.HttpContext.Current);
            return new IronRubyRack.AspNetHandler(dispatcher);
        }
    }
}
