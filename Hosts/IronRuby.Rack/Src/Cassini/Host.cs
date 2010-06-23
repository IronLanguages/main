/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Microsoft Public
 * License (Ms-PL). A copy of the license can be found in the license.htm file
 * included in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Security.Permissions;
using System.Security.Principal;

namespace Cassini {
    class Host : MarshalByRefObject, IRegisteredObject {
        Server _server;

        int _port;
        volatile int _pendingCallsCount;
        string _virtualPath;
        string _lowerCasedVirtualPath;
        string _lowerCasedVirtualPathWithTrailingSlash;
        string _physicalPath;
        string _installPath;
        string _physicalClientScriptPath;
        string _lowerCasedClientScriptPathWithTrailingSlash;

        public override object InitializeLifetimeService() {
            // never expire the license
            return null;
        }

        public Host() {
            HostingEnvironment.RegisterObject(this);
        }

        public void Configure(Server server, int port, string virtualPath, string physicalPath) {
            _server = server;

            _port = port;
            _installPath = null;
            _virtualPath = virtualPath;

            _lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(_virtualPath);
            _lowerCasedVirtualPathWithTrailingSlash = virtualPath.EndsWith("/", StringComparison.Ordinal) ? virtualPath : virtualPath + "/";
            _lowerCasedVirtualPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(_lowerCasedVirtualPathWithTrailingSlash);
            _physicalPath = physicalPath;
            _physicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + "\\";
            _lowerCasedClientScriptPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
        }

        public void ProcessRequest(Connection conn) {
            // Add a pending call to make sure our thread doesn't get killed
            AddPendingCall();

            try {
                Request request = new Request(_server, this, conn);
                request.Process();
            }
            finally {
                RemovePendingCall();
            }
        }

        void WaitForPendingCallsToFinish() {
            for (; ; ) {
                if (_pendingCallsCount <= 0)
                    break;

                Thread.Sleep(250);
            }
        }

        void AddPendingCall() {
#pragma warning disable 0420
            Interlocked.Increment(ref _pendingCallsCount);
#pragma warning restore 0420
        }

        void RemovePendingCall() {
#pragma warning disable 0420
            Interlocked.Decrement(ref _pendingCallsCount);
#pragma warning restore 0420
        }

        public void Shutdown() {
            HostingEnvironment.InitiateShutdown();
        }

        void IRegisteredObject.Stop(bool immediate) {
            // Unhook the Host so Server will process the requests in the new appdomain.
            if (_server != null) {
                _server.HostStopped();
            }

            // Make sure all the pending calls complete before this Object is unregistered.
            WaitForPendingCallsToFinish();

            HostingEnvironment.UnregisterObject(this);
        }

        public string InstallPath { get { return _installPath; } }
        public string NormalizedClientScriptPath { get { return _lowerCasedClientScriptPathWithTrailingSlash; } }
        public string NormalizedVirtualPath { get { return _lowerCasedVirtualPathWithTrailingSlash; } }
        public string PhysicalClientScriptPath { get { return _physicalClientScriptPath; } }
        public string PhysicalPath { get { return _physicalPath; } }
        public int Port { get { return _port; } }
        public string VirtualPath { get { return _virtualPath; } }

        public bool IsVirtualPathInApp(String path) {
            bool isClientScriptPath;
            return IsVirtualPathInApp(path, out isClientScriptPath);
        }

        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath) {
            isClientScriptPath = false;

            if (path == null) {
                return false;
            }

            if (_virtualPath == "/" && path.StartsWith("/", StringComparison.Ordinal)) {
                if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                    isClientScriptPath = true;
                return true;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            if (path.StartsWith(_lowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
                return true;

            if (path == _lowerCasedVirtualPath)
                return true;

            if (path.StartsWith(_lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal)) {
                isClientScriptPath = true;
                return true;
            }

            return false;
        }

        public bool IsVirtualPathAppPath(string path) {
            if (path == null) return false;
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            return (path == _lowerCasedVirtualPath || path == _lowerCasedVirtualPathWithTrailingSlash);
        }
    }
}
