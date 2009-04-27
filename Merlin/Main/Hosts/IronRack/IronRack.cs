/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using IronRuby.Builtins;
using IronRuby.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using System.Diagnostics;
using IronRuby.Runtime;
using System.Collections.Generic;

namespace IronRuby.Rack {

    /// <summary>
    /// A IIS HttpHandler which delegates all requests to Rack.
    /// See http://rack.rubyforge.org/doc/SPEC.html for exactly 
    /// what this handler needs to delegate between IIS and Rack.
    /// </summary>
    internal sealed class Handler : IHttpHandler {

        private readonly ScriptEngine _engine;

        private readonly Stopwatch _watch = new Stopwatch();

        public Handler(ScriptEngine engine) {
            _engine = engine;
        }

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext/*!*/ context) {
            
            // TODO is Rack thread-safe? Can this lock go away?
            lock (this) {
                RubyArray response;

                _watch.Reset();
                _watch.Start();
                
                try {
                    
                    Utils.Log("Request Start: " + context.Request.Url);

                    // Prepare the webserver's environment
                    var scope = _engine.CreateScope();
                    scope.SetVariable("env", Utils.CreateEnv(_engine, context));

                    // run.rb should simply invoke the Rack application with the environment: "app.call(env)"
                    var source = _engine.CreateScriptSourceFromFile(Utils.FindFile("run.rb", _engine));

                    // Since this is invoking a Rack application, the result is always a Ruby Array.
                    response = source.Execute<RubyArray>(scope);

                } catch (Exception e) {
                    
                    Utils.Log(String.Format("Request Finished: {0}\nERROR: {1}", context.Request.Url, e));                    
                    Utils.ReportError(_engine, context, e);

                    context.Response.StatusCode = 200;
                    return;
                }

                _watch.Stop();

                Utils.Log(String.Format("Request Finished: {0}\nOK in {1}", context.Request.Url, _watch.Elapsed));
                
                //
                // The response is always an Array, structured as follows:
                //
                
                // 0 - (int)  status
                context.Response.StatusCode = (int) response[0];
                
                // 1 - (Hash) headers
                foreach (var header in ((Hash)response[1])) {
                    context.Response.Headers[header.Key.ToString()] = header.Value.ToString();
                }

                // 2 - body (TODO must respond to "each" and only yield string values)
                var s = _engine.CreateScope(); s.SetVariable("body", response[2]);
                var body = (MutableString) _engine.CreateScriptSourceFromString("body.body.first").Execute(s);
                context.Response.BinaryWrite(body.ToByteArray());

                // TODO cookies are handled in other ways, so this shouldn't be here.
                var cookies = new Dictionary<object, object>();
                foreach (var cookie in cookies) {
                    context.Response.Cookies.Set(new HttpCookie(cookie.Key.ToString(), cookie.Value.ToString()));
                }
            }
        }
    }

    public class HandlerFactory : IHttpHandlerFactory {
        private static readonly object _GlobalLock = new object();
        private static Handler _Handler;

        public IHttpHandler GetHandler(HttpContext/*!*/ context, string/*!*/ requestType, string/*!*/ url, string/*!*/ pathTranslated) {

            // TODO is this lock needed?
            if (_Handler == null) {
                lock (_GlobalLock) {
                    if (_Handler == null) {

                        Utils.InitializeLog();
                        Utils.Log("=> Booting IronRack"); 

                        var rubyEngine = Ruby.CreateEngine();

                        // HACK Load gems from default MRI installation. This shouldn't be needed.
                        Environment.SetEnvironmentVariable("GEM_PATH", @"C:\ruby\lib\ruby\gems\1.8");

                        try {
                            var stopWatch = new Stopwatch();
                            stopWatch.Start();

                            // ironrack.rb should load the config.ru file, resulting in an Rack application
                            rubyEngine.CreateScriptSourceFromFile(Utils.FindFile("ironrack.rb", rubyEngine)).Execute();

                            stopWatch.Stop();
                            Utils.Log("Rack application loaded");
                        } catch (Exception e) {
                            Utils.ReportError(rubyEngine, context, e);

                            context.Response.StatusCode = 200;
                            return null;
                        }

                        _Handler = new Handler(rubyEngine);
                    }
                }
            }

            return _Handler;
        }

        public void ReleaseHandler(IHttpHandler/*!*/ handler) {

        }
    }
}
