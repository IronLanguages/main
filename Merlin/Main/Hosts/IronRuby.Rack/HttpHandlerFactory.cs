using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web;

namespace IronRuby.Rack {

    /// <summary>
    /// Builds a HttpHandler capable of serving requests through Rack
    /// See http://rack.rubyforge.org/doc/SPEC.html for exactly 
    /// what the handler needs to delegate between IIS and Rack.
    /// </summary>
    public class HttpHandlerFactory : IHttpHandlerFactory {
        private static readonly object _GlobalLock = new object();
        private static HttpHandler _Handler;

        /// <summary>
        /// Sets up the environment. This is only run when the server starts up, not
        /// for each request, so as much as possible should be done here. Returns a
        /// HttpHandler capable of serving Rack requests.
        /// </summary>
        public IHttpHandler GetHandler(HttpContext/*!*/ context, string/*!*/ requestType, string/*!*/ url, string/*!*/ pathTranslated) {

            // TODO is this lock needed?
            if (_Handler == null) {
                lock (_GlobalLock) {
                    if (_Handler == null) {

                        Utils.InitializeLog();
                        Utils.Log("");
                        Utils.Log("=> Booting IronRack");

                        Application app;

                        try {
                            var stopWatch = new Stopwatch();
                            stopWatch.Start();

                            app = new Application(context);
                            Handler.IIS.Run(app);
                            _Handler = new HttpHandler();

                            stopWatch.Stop();
                            Utils.Log("=> Rack application loaded (" + stopWatch.ElapsedMilliseconds + " ms)");
                        } catch (Exception e) {
                            Utils.ReportError(context, e);

                            context.Response.StatusCode = 200;
                            return null;
                        }
                    }
                }
            }

            return _Handler;
        }

        public void ReleaseHandler(IHttpHandler/*!*/ handler) {

        }
    }
}
