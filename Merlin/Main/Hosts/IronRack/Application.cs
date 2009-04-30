using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using IronRuby.Builtins;

namespace IronRuby.Rack {
    public class Application {

        private readonly object _app;
        private readonly string _appRoot;
        private readonly string _rackVersion;

        public object App {
            get { return _app; }
        }

        private const string AppRootOptionName = "AppRoot";
        private const string RackVersionOptionName = "RackVersion";

        public Application(HttpContext context) {
            _appRoot = GetRoot(context);
            _rackVersion = GetRackVersion();


            InitRack();
            _app = Rackup();
        }

        public RubyArray Call(HttpContext context) {
           return RubyEngine.ExecuteMethod<RubyArray>(_app, "call", Utils.CreateEnv(context));
        }

        private void InitRack() {
            RubyEngine.Init();
            RubyEngine.Require("rubygems");
            RubyEngine.Require("rack", _rackVersion);
            RubyEngine.AddLoadPath(_appRoot);
        }

        private object Rackup() {
            var fullPath = RubyEngine.FindFile("config.ru");
            if(fullPath != null) {
                var content = File.ReadAllText(fullPath, Encoding.UTF8);
                return RubyEngine.Execute("Rack::Builder.new { " + content + "}");
            }
            return null;
        }

        private static string GetRackVersion() {
            return ConfigurationManager.AppSettings[RackVersionOptionName] ?? "=1.0.0";
        }

        private static string GetRoot(HttpContext context) {
            var root = ConfigurationManager.AppSettings[AppRootOptionName];
            if (root == null) {
                root = context.Request.PhysicalApplicationPath;
            } else {
                if (!Path.IsPathRooted(root)) {
                    root = Path.Combine(context.Request.PhysicalApplicationPath, root);
                }
                if (!Directory.Exists(root)) {
                    throw new ConfigurationErrorsException(String.Format(
                        "Directory '{0}' specified by '{1}' setting in configuration doesn't exist",
                        root, AppRootOptionName));
                }
            }
            return root;
        }
    }
}
