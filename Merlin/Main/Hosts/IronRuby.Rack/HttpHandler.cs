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
using System.Threading;

namespace IronRuby.Rack {

    internal sealed class HttpHandler : IHttpHandler {

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            var rack = new RackDispatcher();
            rack.Start();
            rack.Dispatch(context);
            rack.Finish();
        }
    }

    internal sealed class HttpAsyncHandler : IHttpAsyncHandler {

        private RackDispatcher _rack;

        public bool IsReusable {
            get { return true; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
        {
            _rack = new RackDispatcher();
            _rack.Start();
            var async = new RackAsyncDispatcher(cb, context, extraData);
            async.StartAsyncWork(_rack);
            return async;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            _rack.Finish();
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException();
        }
    }

    class RackAsyncDispatcher : IAsyncResult {

        private bool _completed;
        private Object _state;
        private AsyncCallback _callback;
        private HttpContext _context;

        bool IAsyncResult.IsCompleted { get { return _completed; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
        Object IAsyncResult.AsyncState { get { return _state; } }
        bool IAsyncResult.CompletedSynchronously { get { return false; } }

        internal RackAsyncDispatcher(AsyncCallback callback, HttpContext context, object state) {
            _callback = callback;
            _context = context;
            _state = state;
            _completed = false;
        }

        internal void StartAsyncWork(RackDispatcher rack) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTask), rack);
        }

        private void StartAsyncTask(object rack) {
            Utils.Log("=== Starting async processing at " + DateTime.Now.ToString());

            (rack as RackDispatcher).Dispatch(_context);

            _completed = true;
            _callback(this);
        }
    }

    class RackDispatcher {

        private readonly Stopwatch _watch = new Stopwatch();
        private bool _dispatchCompleted;

        internal void Start() {
            _dispatchCompleted = false;
            Utils.Log("");
            Utils.Log("=== Request started at " + DateTime.Now.ToString());
            _watch.Reset();
            _watch.Start();
        }

        internal void Finish() {
            if (_dispatchCompleted) {
                _watch.Stop();
                Utils.Log(">>> Request finished (" + _watch.ElapsedMilliseconds.ToString() + "ms)");
            }
        }

        internal void Dispatch(HttpContext context) {
            try {
                Handler.IIS.Current.Handle(
                    new Request(new HttpRequestWrapper(context.Request)),
                    new Response(new HttpResponseWrapper(context.Response))
                );
                _dispatchCompleted = true;
            } catch (Exception e) {
                Utils.ReportError(context, e);
            }
        }
    }
}
