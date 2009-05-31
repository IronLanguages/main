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
using IronRuby.Runtime;
using System.Collections.Generic;
using System.Diagnostics;

namespace IronRuby.Rack {

    internal sealed class HttpHandler : IHttpHandler {
        
        private readonly Stopwatch _watch = new Stopwatch();

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            lock (this) {
                Utils.Log("");
                Utils.Log("=== Request started at " + DateTime.Now.ToString());
                _watch.Reset();
                _watch.Start();
                
                Handler.IIS.Current.Handle(new Request(new HttpRequestWrapper(context.Request)),
                                           new Response(new HttpResponseWrapper(context.Response)));
                
                _watch.Stop();
                Utils.Log(">>> Request finished (" + _watch.ElapsedMilliseconds.ToString() + "ms)");
            }
        }
    }
}
