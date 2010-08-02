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
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.AspNet.MembersInjectors;
using Microsoft.Scripting.AspNet.Util;

namespace Microsoft.Scripting.AspNet {

    internal interface IBuildProvider {
        ScriptEngine GetScriptEngine();
        string GetScriptCode();
        BuildResult CreateBuildResult(CompiledCode compiledCode, string scriptVirtualPath);
    }

    internal static class EngineHelper {
        private const string ScriptFolderName = "App_Script";
        internal static string s_scriptFolder;
        private static bool s_appScriptDirChanged;

        private static ScriptRuntime s_scriptEnvironment;
        private static string[] s_fileExtensions;
        private static string[] s_languageIds;

        static EngineHelper() {
            // Add the App_Script folder to the path to allow script files to be imported from there
            string appPath = System.Web.HttpRuntime.AppDomainAppPath;
            s_scriptFolder = Path.Combine(appPath, ScriptFolderName);

            var setup = ScriptRuntimeSetup.ReadConfiguration();

            // Set the host type
            setup.HostType = typeof(WebScriptHost);

            setup.Options["SearchPaths"] = new string[] { s_scriptFolder };

            s_scriptEnvironment = new ScriptRuntime(setup);
            s_scriptEnvironment.LoadAssembly(typeof(ControlMembersInjector).Assembly);      // for our member injectors

            
            // Register for notifications when something in App_Script changes
            FileChangeNotifier.Register(s_scriptFolder, OnAppScriptFileChanged);

            List<string> fileExtensions = new List<string>();
            foreach (var language in s_scriptEnvironment.Setup.LanguageSetups) {
                fileExtensions.AddRange(language.FileExtensions);
            }
            s_fileExtensions = Array.ConvertAll(fileExtensions.ToArray(), e => e.ToLower());

            List<string> simpleNames = new List<string>();
            foreach (var language in s_scriptEnvironment.Setup.LanguageSetups) {
                simpleNames.AddRange(language.Names);
            }
            s_languageIds = Array.ConvertAll(simpleNames.ToArray(), n => n.ToLower());
        }

        internal static ScriptRuntime ScriptRuntime { get { return s_scriptEnvironment; } }

        internal static string[] FileExtensions { get { return s_fileExtensions; } }

        internal static bool IsDLRLanguage(string language) {
            return Array.IndexOf(s_languageIds, language.ToLower()) > -1;
        }

        internal static bool IsDLRLanguageExtension(string extension) {
            return Array.IndexOf(s_fileExtensions, extension.ToLower()) > -1;
        }

        internal static ScriptEngine GetScriptEngineByExtension(string extension) {
            return s_scriptEnvironment.GetEngineByFileExtension(extension);
        }

        internal static ScriptEngine GetScriptEngineByName(string language) {
            return s_scriptEnvironment.GetEngine(language);
        }

        internal static BuildResult GetBuildResult(string virtualPath, IBuildProvider buildProvider) {

            // If any script files in App_Scripts changed, they need to be reloaded
            ReloadChangedScriptFiles();

            virtualPath = VirtualPathUtility.ToAbsolute(virtualPath);
            string cacheKey = virtualPath.ToLowerInvariant();

            // Look up the cache before and after taking the lock
            BuildResult result = (BuildResult)HttpRuntime.Cache[cacheKey];
            if (result != null)
                return result;

            ScriptEngine scriptEngine = buildProvider.GetScriptEngine();

            lock (typeof(EngineHelper)) {
                result = (BuildResult)HttpRuntime.Cache[cacheKey];
                if (result != null)
                    return result;

                DateTime utcStart = DateTime.UtcNow;

                CompiledCode compiledCode = null;
                string scriptCode = buildProvider.GetScriptCode();

                if (scriptCode != null) {
                    // We pass the physical path for debugging purpose
                    string physicalPath = HostingEnvironment.MapPath(virtualPath);
                    ScriptSource source = scriptEngine.CreateScriptSourceFromString(scriptCode, physicalPath, SourceCodeKind.File);

                    try {
                        compiledCode = source.Compile();
                    } catch (SyntaxErrorException e) {
                        EngineHelper.ThrowSyntaxErrorException(e);
                    }
                }

                // Note that we cache the result even if there is no script, to avoid having to check
                // again later.

                result = buildProvider.CreateBuildResult(compiledCode, virtualPath);

                CacheDependency cacheDependency = HostingEnvironment.VirtualPathProvider.GetCacheDependency(
                    virtualPath, new Util.SingleObjectCollection(virtualPath), utcStart);

                // Cache the result with a 5 minute sliding expiration
                HttpRuntime.Cache.Insert(cacheKey, result, cacheDependency,
                    Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5));
            }

            return result;
        }

        internal static void ExecuteCode(ScriptScope scope, CompiledCode compiledCode, string virtualPath) {
            try {
                compiledCode.Execute(scope);
            } catch (SyntaxErrorException e) {
                EngineHelper.ThrowSyntaxErrorException(e);
            } catch (Exception e) {
                if (!EngineHelper.ProcessRuntimeException(compiledCode.Engine, e, virtualPath))
                    throw;
            }
        }

        internal static CompiledCode CompileExpression(string expr, 
            ScriptEngine scriptEngine, string scriptVirtualPath, int lineOffset) {

            try {
                return scriptEngine.CreateScriptSourceFromString(expr, SourceCodeKind.Expression).Compile();
            } catch (SyntaxErrorException e) {
                ThrowSyntaxErrorException(e, scriptVirtualPath, lineOffset + e.Line);
            }
            return null;
        }

        internal static CompiledCode CompileCodeDom(System.CodeDom.CodeMemberMethod code,
            ScriptEngine scriptEngine) {

            try {
                return scriptEngine.CreateScriptSource(code).Compile();
            } catch (SyntaxErrorException e) {
                ThrowSyntaxErrorException(e);
            }
            return null;
        }

        internal static object EvaluateCompiledCode(CompiledCode compiledExpression, ScriptScope scope,
            string defaultVirtualPath, int lineOffset) {

            try {
                return compiledExpression.Execute(scope);
            } catch (Exception e) {
                if (!ProcessRuntimeException(compiledExpression.Engine, e, defaultVirtualPath, lineOffset))
                    throw;
            }
            return null;
        }

        internal static void ExecuteCompiledCode(CompiledCode compiledExpression, ScriptScope module) {

            try {
                compiledExpression.Execute(module);
            } catch (Exception e) {
                if (!ProcessRuntimeException(compiledExpression.Engine, e, null))
                    throw;
            }
        }

        // Call a method
        internal static object CallMethod(ScriptEngine engine, DynamicFunction f, string defaultVirtualPath,
            params object[] args) {

            try {                
                return f.Invoke(engine, args);
            } catch (Exception e) {
                if (!ProcessRuntimeException(engine, e, defaultVirtualPath))
                    throw;
            }
            return null;
        }

        internal static bool ProcessRuntimeException(ScriptEngine engine, Exception e, string virtualPath) {
            return ProcessRuntimeException(engine, e, virtualPath, 0 /*lineOffset*/);
        }

        internal static bool ProcessRuntimeException(ScriptEngine engine, Exception e, string defaultVirtualPath, int lineOffset) {

            var frames = engine.GetService<ExceptionOperations>().GetStackFrames(e);
            if (frames.Count == 0)
                return false;

            DynamicStackFrame frame = frames[0];
            int line = frame.GetFileLineNumber();

            // Get the physical path of the file where the exception occured, and attempt to get a
            // virtual path from it
            string physicalPath = frame.GetFileName();
            string virtualPath = Misc.GetVirtualPathFromPhysicalPath(physicalPath);

            // If we couldn't get one, use the passed in one, and adjust the line number
            if (virtualPath == null) {
                virtualPath = defaultVirtualPath;
                line += lineOffset;
            }

            Misc.ThrowException(e.Message, e, virtualPath, line);
            return true;
        }

        internal static void ThrowSyntaxErrorException(Microsoft.Scripting.SyntaxErrorException e) {
            ThrowSyntaxErrorException(e, null, 1);
        }

        internal static void ThrowSyntaxErrorException(Microsoft.Scripting.SyntaxErrorException e,
            string defaultVirtualPath, int defaultLine) {

            // Try to get a virtual path
            string virtualPath = Misc.GetVirtualPathFromPhysicalPath(e.GetSymbolDocumentName());
            int line;

            // If we couldn't get one, use the passed in path
            if (virtualPath == null) {
                virtualPath = defaultVirtualPath;
                line = defaultLine;
            } else {
                line = e.Line;
            }

            Misc.ThrowException(null /*message*/, e, virtualPath, line);
        }

        private static void OnAppScriptFileChanged(string path) {
            // Remember the fact that something in App_Script changed
            s_appScriptDirChanged = true;
        }

        private static void ReloadChangedScriptFiles() {

            // Nothing to do if no files changed
            if (!s_appScriptDirChanged)
                return;
            
            // TODO: a new ScriptRuntime should be created instead
            foreach (string path in Directory.GetFiles(s_scriptFolder)) {
                string ext = Path.GetExtension(path);
                if (IsDLRLanguageExtension(ext)) {
                    try {
                        s_scriptEnvironment.ExecuteFile(path);
                    } catch (SyntaxErrorException e) {
                        EngineHelper.ThrowSyntaxErrorException(e);
                    }
                }
            }
        
            // Clear out the flag to mark that all the changes were processed
            s_appScriptDirChanged = false;
        }

    }

    // The base class of what we cache when we build a python file
    internal class BuildResult {
        private CompiledCode _compiledCode;
        private string _scriptVirtualPath;

        public BuildResult(CompiledCode compiledCode, string scriptVirtualPath) {
            _scriptVirtualPath = scriptVirtualPath;
            _compiledCode = compiledCode;
        }

        public CompiledCode CompiledCode { get { return _compiledCode; } }
        public string ScriptVirtualPath { get { return _scriptVirtualPath; } }
    }

    abstract class TypeWithEventsBuildResult : BuildResult {

        private bool _initMethodsCalled;

        internal TypeWithEventsBuildResult(CompiledCode compiledCode, string scriptVirtualPath)
            : base(compiledCode, scriptVirtualPath) { }

        internal void InitMethods(Type type, ScriptScope moduleGlobals) {
            if (!_initMethodsCalled) {
                lock (this) {
                    if (!_initMethodsCalled) {
                        InitMethodsInternal(type, moduleGlobals);
                        _initMethodsCalled = true;
                    }
                }
            }
        }

        private void InitMethodsInternal(Type type, ScriptScope moduleGlobals) {

            // If CompiledCode is null, there was no script to compile
            if (CompiledCode == null)
                return;

            foreach (KeyValuePair<string, object> pair in moduleGlobals.GetItems()) {

                // Wrap it as a dynamic object. It may not be something callable, and if it's not,
                // it will fail when we try to call it.                
                DynamicFunction f = new DynamicFunction(pair.Value);
                ProcessEventHandler(pair.Key, type, f);
            }
        }

        internal abstract bool ProcessEventHandler(string handlerName, Type type, DynamicFunction f);
    }


}
 
