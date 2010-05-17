using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using IronRuby;
using IronRuby.Builtins;

namespace IronRuby.Rack {
    public class Application {

        private readonly HttpContext _context;
        private readonly object _app;

        private string _appRoot;
        private string _rackVersion;
        private string _gemPath;
        private string _rackEnv;
        private RubyArray _actualRackVersion;

        private const string AppRootOptionName = "AppRoot";
        private const string RackVersionOptionName = "RackVersion";
        private const string GemPathOptionName = "GemPath";
        private const string RackEnvOptionName = "RackEnv";

        public Application(HttpContext context) {
            _context = context;

            InitRack();
            _app = Rackup();
        }

        public RubyArray Call(Hash env) {
            return RubyEngine.ExecuteMethod<RubyArray>(App, "call", env);
        }

        private void InitRack() {
            RubyEngine.SetToplevelBinding();
            
            if (GemPath != null) {
                Utils.Log("=> Setting GEM_PATH: " + RubyEngine.Context.Inspect(GemPath));
                Environment.SetEnvironmentVariable("GEM_PATH", GemPath);
            }

            if (RackEnv != null) {
                Utils.Log("=> Setting RACK_ENV: " + RubyEngine.Context.Inspect(RackEnv));
                Environment.SetEnvironmentVariable("RACK_ENV", RackEnv);
            }

            Utils.Log("=> Loading RubyGems");
            RubyEngine.Require("rubygems");

            Utils.Log("=> Loading Rack " + RackVersion);
            RubyEngine.Require("rack", RackVersion);
            Utils.Log(string.Format("=> Loaded rack-{0}", Utils.Join(ActualRackVersion, ".")));

            Utils.Log("=> Application root: " + RubyEngine.Context.Inspect(AppRoot));
            RubyEngine.AddLoadPath(AppRoot);
        }

        private object Rackup() {
            Utils.Log("=> Loading Rack application");
            var fullPath = RubyEngine.FindFile("config.ru");
            if (fullPath != null) {
                var content = File.ReadAllText(fullPath, Encoding.UTF8);
                return RubyEngine.Execute("Rack::Builder.new { (\n" + content + "\n) }.to_app");
            }
            return null;
        }

        public object App {
            get { return _app; }
        }


        private string PhysicalAppPath {
            get {
                return _context != null ? _context.Request.PhysicalApplicationPath : null;
            }
        }

        private string AppRoot {
            get {
                if (_appRoot == null) {
                    var root = ConfigurationManager.AppSettings[AppRootOptionName];
                    if (root == null) {
                        root = PhysicalAppPath;
                    } else {
                        if (!Path.IsPathRooted(root)) {
                            root = GetFullPath(root, PhysicalAppPath);
                        }
                        if (!Directory.Exists(root)) {
                            throw new ConfigurationErrorsException(String.Format(
                                "Directory '{0}' specified by '{1}' setting in configuration doesn't exist",
                                root, AppRootOptionName));
                        }
                    }
                    _appRoot = root;
                }
                return _appRoot;
            }
        }

        private string GemPath {
            get {
                if (_gemPath == null) 
                    _gemPath = GetFullPath(ConfigurationManager.AppSettings[GemPathOptionName], PhysicalAppPath);
                return _gemPath;
            }
        }

        public string RackEnv {
            get {
                if (_rackEnv == null)
                    _rackEnv = ConfigurationManager.AppSettings[RackEnvOptionName];
                return _rackEnv;
            }
        }

        private string RackVersion {
            get {
                if (_rackVersion == null)
                    _rackVersion = ConfigurationManager.AppSettings[RackVersionOptionName] ?? ">=1.0.0";
                return _rackVersion;
            }
        }

        public RubyArray ActualRackVersion {
            get {
                if (_actualRackVersion == null)
                    _actualRackVersion = RubyEngine.Execute<RubyArray>("Rack::VERSION");
                return _actualRackVersion;
            }
        }

        private static string GetFullPath(string path, string root) {
            return Path.GetFullPath(Path.Combine(root, path));
        }
    }
}
