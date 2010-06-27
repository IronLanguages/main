using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web;
using System.IO;

namespace IronRubyRack
{
    public class RackDispatcher
    {
        internal static IronRubyApplication App { get; private set; }

        private readonly Stopwatch _watch = new Stopwatch();
        private bool _dispatchCompleted;
        internal bool _failed;

        public RackDispatcher(HttpContext context) {
            Utils.Log("");
            Utils.Log("=== Booting ironruby-rack at " + DateTime.Now.ToString());

            try {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                App = new IronRubyApplication(context.Request.PhysicalApplicationPath);
                Handler.AspNet.Run(App);

                stopWatch.Stop();
                Utils.Log("=> Rack application loaded (" + stopWatch.ElapsedMilliseconds + " ms)");
            } catch (Exception e) {
                _failed = true;
                Utils.ReportError(context, e);
            }
        }

        public void ProcessRequest(HttpContext context) {
            if (!_failed && !HandleFile(context))
                LogRequest(() => Dispatch(context));
        }

        private bool HandleFile(HttpContext/*!*/ context) {
            string fullPath = context.Request.PhysicalPath;
            if (App != null) {
                fullPath = string.Format(@"{0}\{1}{2}", App.AppRoot, App.PublicDir, context.Request.FilePath.Replace('/', '\\'));
                if (File.Exists(fullPath)) {
                    Utils.Log(String.Format("File found: {0} -> {1}", context.Request.Url, fullPath));
                    try {
                        context.Response.WriteFile(fullPath);
                        context.Response.StatusCode = 200;
                    } catch (Exception) {
                        context.Response.StatusCode = 500;
                    }
                    return true;
                }
            } 
            context.Response.StatusCode = 404;
            Utils.Log(String.Format("File not found: {0}", fullPath));
            return false;
        }

        internal void LogRequest(Action operation) {
            _dispatchCompleted = false;
            Utils.Log("");
            Utils.Log("=== Request started at " + DateTime.Now.ToString());
            _watch.Reset();
            _watch.Start();

            operation();

            if (_dispatchCompleted) {
                _watch.Stop();
                Utils.Log(">>> Request finished (" + _watch.ElapsedMilliseconds.ToString() + "ms)");
            }
        }

        internal void Dispatch(HttpContext context) {
            try {
                if (!_failed && Handler.AspNet.Current != null) {
                    Handler.AspNet.Current.Handle(
                        new AspNetRequest(context.Request),
                        new AspNetResponse(context.Response)
                    );
                }
                _dispatchCompleted = true;
            } catch (Exception e) {
                Utils.ReportError(context, e);
            }
        }
    }
}
