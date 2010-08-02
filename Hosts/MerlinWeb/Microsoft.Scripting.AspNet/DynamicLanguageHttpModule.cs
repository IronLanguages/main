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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.AspNet.Util;

namespace Microsoft.Scripting.AspNet {
    internal class DynamicLanguageHttpModule : IHttpModule, IBuildProvider {
        private const string globalFileName = "global";

        private static GlobalAsaxBuildResult s_buildResult;
        private static string s_globalVirtualPathWithoutExtension;
        private static string s_globalVirtualPath;
        private static bool s_globalFileCompiled;
        private static int s_globalFileVersion = 1;

        private static bool s_firstRequest = true;

        private static int s_moduleCount = 0;

        private static string s_merlinWebHeader;

        private ScriptScope _globalScope;

        private int _globalFileCompiledVersion = 0;

        private Dictionary<string, EventHandlerWrapper> _handlers;

        static DynamicLanguageHttpModule() {

            s_globalVirtualPathWithoutExtension = VirtualPathUtility.ToAbsolute("~/" + globalFileName);

            foreach (string extension in EngineHelper.FileExtensions) {
                // Register for global.<ext> file change notifications
                FileChangeNotifier.Register(Path.Combine(HttpRuntime.AppDomainAppPath, globalFileName + extension),
                    OnFileChanged);
            }

            // Create string with the special response header
            string mwName = typeof(DynamicLanguageHttpModule).Assembly.FullName.Split(',')[0];
            Version mwVersion  = typeof(DynamicLanguageHttpModule).Assembly.GetName().Version;
            Version dlrVersion = typeof(ScriptEngine).Assembly.GetName().Version;
            s_merlinWebHeader = string.Format("{0} v{1}.{2}(CTP); DLR v{3}.{4}",
                mwName, mwVersion.Major, mwVersion.Minor, dlrVersion.Major, dlrVersion.Minor);
        }

        public virtual void Init(HttpApplication app) {

            // Keep track of the number of modules
            Interlocked.Increment(ref s_moduleCount);

            // Register our BeginRequest handler first so we can make sure everything is up to date
            // before trying to invoke user code
            app.BeginRequest += new EventHandler(OnBeginRequest);

            // Register for all the events on HttpApplication, and keep track of them in a dictionary
            _handlers = new Dictionary<string, EventHandlerWrapper>();

            EventInfo[] eventInfos = typeof(HttpApplication).GetEvents();
            foreach (EventInfo eventInfo in eventInfos) {
                EventHandlerWrapper eventHandlerWrapper = new EventHandlerWrapper(this);
                EventHandler handler = eventHandlerWrapper.Handler;
                try {
                    eventInfo.AddEventHandler(app, handler);
                } catch (TargetInvocationException tiException) {
                    if (tiException.InnerException is PlatformNotSupportedException) {
                        // Ignore the event if we failed to add the handler.  This can happen with IIS7
                        // which has new events that only work in Integrated Pipeline mode
                        continue;
                    }

                    throw;
                }

                _handlers[eventInfo.Name] = eventHandlerWrapper;
            }
        }

        public virtual void Dispose() {
            Interlocked.Decrement(ref s_moduleCount);

            // If it's the last module, the app is shutting down.  Call Application_OnEnd (if any)
            if (s_moduleCount == 0) {
                if (s_buildResult != null) {
                    s_buildResult.CallOnEndMethod();
                }
            }
        }

        #region IBuildProvider Members
        ScriptEngine IBuildProvider.GetScriptEngine() {
            Debug.Assert(s_globalVirtualPath != null);
            return EngineHelper.GetScriptEngineByExtension(VirtualPathUtility.GetExtension(s_globalVirtualPath));
        }

        string IBuildProvider.GetScriptCode() {
            // Return the full content of the 'global' file
            return Util.Misc.GetStringFromVirtualPath(s_globalVirtualPath);
        }

        BuildResult IBuildProvider.CreateBuildResult(CompiledCode compiledCode, string scriptVirtualPath) {            
            return new GlobalAsaxBuildResult(((IBuildProvider)this).GetScriptEngine(), compiledCode, scriptVirtualPath);
        }
        #endregion

        private void OnBeginRequest(object sender, EventArgs eventArgs) {
            EnsureGlobalFileCompiled();

            if (_globalScope == null && s_buildResult != null && s_buildResult.CompiledCode != null) {
                _globalScope = EngineHelper.ScriptRuntime.CreateScope();

                EngineHelper.ExecuteCode(_globalScope, s_buildResult.CompiledCode, s_globalVirtualPath);

                s_buildResult.InitMethods(typeof(HttpApplication), _globalScope);
            }

            
            // If a 'global' file changed, we need to hook up its event handlers
            if (_globalFileCompiledVersion < s_globalFileVersion) {
                HookupScriptHandlers();
                _globalFileCompiledVersion = s_globalFileVersion;
            }

            // Turn off the first request flag
            s_firstRequest = false;

            // Add special marker response header
            if (s_merlinWebHeader != null) {
                ((HttpApplication)sender).Response.AddHeader("X-DLR-Version", s_merlinWebHeader);
            }
        }

        private void EnsureGlobalFileCompiled() {

            // This is done only once for all the HttpModule instances every time the file changes

            Debug.Assert(s_buildResult == null || s_globalFileCompiled);

            if (s_globalFileCompiled)
                return;

            lock (typeof(DynamicLanguageHttpModule)) {
                
                if (s_globalFileCompiled)
                    return;

                _globalScope = null;
                s_globalVirtualPath = null;

                string globalVirtualPath;
                foreach (string extension in EngineHelper.FileExtensions) {
                    globalVirtualPath = s_globalVirtualPathWithoutExtension + extension;

                    if (HostingEnvironment.VirtualPathProvider.FileExists(globalVirtualPath)) {

                        // Fail if we had already found a global file
                        if (s_globalVirtualPath != null) {
                            throw new Exception(String.Format("A web application can only have one global file. Found both '{0}' and '{1}'",
                                s_globalVirtualPath, globalVirtualPath));
                        }
                        s_globalVirtualPath = globalVirtualPath;
                    }
                }

                // If we found a global file, compile it
                if (s_globalVirtualPath != null) {
                    s_buildResult = (GlobalAsaxBuildResult)EngineHelper.GetBuildResult(s_globalVirtualPath, this);
                }

                // We set this even when there is no file to compile
                s_globalFileCompiled = true;
            }
        }

        private void HookupScriptHandlers() {

            // This is done once per HttpApplication/HttpModule instance every time
            // a global.<ext> file changes

            // If it's the first request in the domain, call Application_OnStart (if any)
            if (s_firstRequest && s_buildResult != null) {
                s_buildResult.CallOnStartMethod();
            }
            
            // Hook up all the events implemented in the 'global' file
            foreach (string handlerName in _handlers.Keys) {
                EventHandlerWrapper eventHandlerWrapper = _handlers[handlerName];

                DynamicFunction f = null;

                if (s_buildResult != null && s_buildResult.EventHandlers != null) {
                    s_buildResult.EventHandlers.TryGetValue(handlerName, out f);
                }

                eventHandlerWrapper.SetDynamicFunction(f, s_globalVirtualPath);
            }
        }

        private static void OnFileChanged(string path) {

            // The file changed, and needs to be reprocessed
            s_globalFileCompiled = false;
            s_buildResult = null;

            s_globalFileVersion++;
        }

        class GlobalAsaxBuildResult : TypeWithEventsBuildResult {
            private const string HandlerPrefix = "Application_";
            private Dictionary<string, DynamicFunction> _eventHandlers;
            private DynamicFunction _onStartMethod, _onEndMethod;
            private ScriptEngine _engine;

            internal GlobalAsaxBuildResult(ScriptEngine engine, CompiledCode compiledCode, string scriptVirtualPath)
                : base(compiledCode, scriptVirtualPath) {
                _engine = engine;
            }

            public Dictionary<string, DynamicFunction> EventHandlers { get { return _eventHandlers; } }

            internal override bool ProcessEventHandler(string handlerName, Type type, DynamicFunction f) {
                // Does it look like a handler?
                if (!handlerName.StartsWith(HandlerPrefix))
                    return false;

                // Handle the special pseudo-events
                if (String.Equals(handlerName, "Application_OnStart", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(handlerName, "Application_Start", StringComparison.OrdinalIgnoreCase)) {
                    _onStartMethod = f;
                    return true;
                } else if (String.Equals(handlerName, "Application_OnEnd", StringComparison.OrdinalIgnoreCase) ||
                         String.Equals(handlerName, "Application_End", StringComparison.OrdinalIgnoreCase)) {
                    _onEndMethod = f;
                    return true;
                } else if (String.Equals(handlerName, "Session_OnEnd", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(handlerName, "Session_End", StringComparison.OrdinalIgnoreCase)) {
                    // REVIEW: can we support Session_End?
                    throw new Exception("Session_End is not supported!");
                }

                string eventName = handlerName.Substring(HandlerPrefix.Length);
                
                // This will throw if the event doesn't exist
                EventHookupHelper.GetEventInfo(type, eventName, f, ScriptVirtualPath);

                if (_eventHandlers == null)
                    _eventHandlers = new Dictionary<string, DynamicFunction>();

                _eventHandlers[eventName] = f;

                return true;
            }

            internal void CallOnStartMethod() {
                CallFunction(_engine, _onStartMethod);
            }

            internal void CallOnEndMethod() {
                CallFunction(_engine, _onEndMethod);
            }

            private void CallFunction(ScriptEngine engine, DynamicFunction f) {
                if (f == null)
                    return;

                try {                    
                    f.Invoke(engine);
                } catch (Exception e) {
                    if (!EngineHelper.ProcessRuntimeException(engine, e, ScriptVirtualPath))
                        throw;
                }
            }
        }
    }
}
