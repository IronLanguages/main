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

using System;
using System.IO;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;

namespace IronRubyRack {

    public static class IronRubyEngine {

        public static ScriptEngine Engine;
        private static ScriptScope _Scope;

        static IronRubyEngine() {
            Engine = Ruby.CreateEngine();
            _Scope = Engine.CreateScope();
        }

        private static RubyContext GetExecutionContext(ScriptEngine engine) {
            return Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetLanguageContext(engine) as RubyContext;
        }

        public static RubyContext Context {
            get {
                return GetExecutionContext(Engine);
            }
        }

        public static void SetToplevelBinding() {
            Execute("TOPLEVEL_BINDING = binding");
        }

        public static object Require(string file) {
            return Require(file, null);
        }

        public static object Require(string file, string rackVersion) {
            var command = "";
            if (!String.IsNullOrEmpty(rackVersion)) {
                command = String.Format("gem '{0}', '{1}';", file, rackVersion);
            }
            command += String.Format("require '{0}'", file);
            return Execute(command);
        }

        public static object ExecuteFile(string fileName) {
            return Engine.CreateScriptSourceFromString(FindFile(fileName)).Execute(_Scope);
        }

        public static object Execute(string code) {
            return Execute(code, _Scope);
        }

        public static object Execute(string code, ScriptScope aScope) {
            return Execute<object>(code, aScope);
        }

        public static T Execute<T>(string code) {
            return Execute<T>(code, _Scope);
        }

        public static T Execute<T>(string code, ScriptScope aScope) {
#if DEBUG
            Utils.Log(string.Format("[DEBUG] >>> {0}", code));
#endif
            return (T)Engine.Execute(code, aScope);
        }

        public static object ExecuteMethod(object instance, string methodName, params object[] args) {
            return ExecuteMethod<object>(instance, methodName, args);
        }

        public static T ExecuteMethod<T>(object instance, string methodName, params object[] args) {
            return (T)Engine.Operations.InvokeMember(instance, methodName, args);
        }

        public static object AddLoadPath(string path) {
            return Engine.Execute(
                string.Format("$LOAD_PATH.unshift '{0}'",
                    IronRuby.Runtime.RubyUtils.CanonicalizePath(MutableString.CreateAscii(path))));
        }

        public static void SetConstant(string name, object value) {
            Engine.Runtime.Globals.SetVariable(name, value);
        }

        public static string FindFile(string file) {
            foreach (var path in Engine.GetSearchPaths()) {
                var fullPath = TryGetFullPath(path, file);
                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }
            return null;
        }

        private static string TryGetFullPath(string/*!*/ dir, string/*!*/ file) {
            try {
                return Path.GetFullPath(Path.Combine(dir, file));
            } catch {
                return null;
            }
        }
    }
}
