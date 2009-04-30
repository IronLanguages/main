using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using IronRuby.Builtins;

namespace IronRuby.Rack.Handler {
    public class IIS {

        public readonly Application App;
        
        private static IIS _current;
        public  static IIS  Current { get { return _current; } }

        private IIS(Application app) {
            App = app;
        }

        public static void Run(Application app) {
            Run(app, new Dictionary<string, string>());
        }

        public static void Run(Application app, IDictionary<string, string> options) {
            // TODO: make sure IIS is running

            if (_current == null) {
                 _current = new IIS(app);
            }
        }

        public void Handle(HttpContext context) {

            RubyArray response = App.Call(context);

            //
            // The response is always an Array, structured as follows:
            //

            // 0 - (int)  status
            context.Response.StatusCode = (int) response[0];

            // 1 - (Hash) headers
            foreach (var header in ((Hash) response[1])) {
                context.Response.Headers[header.Key.ToString()] = header.Value.ToString();
            }

            // 2 - body (TODO must respond to "each" and only yield string values)
            var s = RubyEngine.Engine.CreateScope(); s.SetVariable("body", response[2]);
            var body = (MutableString) RubyEngine.Engine.CreateScriptSourceFromString("body.body.first").Execute(s);
            context.Response.BinaryWrite(body.ToByteArray());

            // TODO cookies are handled in other ways, so this shouldn't be here.
            var cookies = new Dictionary<object, object>();
            foreach (var cookie in cookies) {
                context.Response.Cookies.Set(new HttpCookie(cookie.Key.ToString(), cookie.Value.ToString()));
            }
        }

    }
}
