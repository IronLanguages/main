using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Scripting.Hosting;
using IronRuby.Builtins;

namespace IronRuby.Rack {
    public static class RubyEngine {
        public static readonly ScriptEngine Engine = Ruby.CreateEngine();
        private static readonly ScriptScope scope = Engine.CreateScope();

        public static void Init() {
            // HACK Load gems from default MRI installation. This shouldn't be needed.
            Environment.SetEnvironmentVariable("GEM_PATH", @"C:\ruby\lib\ruby\gems\1.8");
        }

        public static object Require(string file) {
            return Require(file, null);
        }

        public static object Require(string file, string rackVersion) {
            var command = new MutableString();
            if(rackVersion != null) {
                command.Append(string.Format("gem '{0}', '{1}';", file, rackVersion));
            }
            command.Append(string.Format("require '{0}'", file));
            return Execute(command);
        }

        public static object ExecuteFile(string fileName) {
            return Engine.CreateScriptSourceFromString(FindFile(fileName)).Execute(scope);
        }

        public static object Execute(string code) {
            return Engine.CreateScriptSourceFromString(code).Execute(scope);
        }
        
        public static T ExecuteMethod<T>(object instance, string methodName, params object[] args)
        {
            return (T)Engine.Operations.InvokeMember(instance, methodName, args);
        }

        public static object AddLoadPath(string path) {
            return Engine.Execute(string.Format("$LOAD_PATH.unshift '{0}'", path));
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
