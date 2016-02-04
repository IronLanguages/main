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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using IronRuby.Builtins;

namespace IronRubyRack {

    public class IronRubyApplication {

        private readonly object _app;

        private string _appRoot;
        private string _rackVersion;
        private string _gemPath;
        private string _rackEnv;
        private string _publicDir;
        private RubyArray _actualRackVersion;
        private string _searchPaths;


        private const string AppRootOptionName = "AppRoot";
        private const string RackVersionOptionName = "RackVersion";
        private const string GemPathOptionName = "GemPath";
        private const string RackEnvOptionName = "RackEnv";
        private const string PublicDirOptionName = "PublicDir";
        private const string SearchPathsOptionName = "SearchPaths";

        // TODO: also include overload which takes a RubyObject and uses it as
        // the Rack application (to truely support rackup).
        public IronRubyApplication(string appPath) {
            PhysicalAppPath = appPath;

        InitSearchPaths();
            InitRack();
            _app = Rackup();
        }

        public RubyArray Call(Hash env) {
            return IronRubyEngine.ExecuteMethod<RubyArray>(App, "call", env);
        }

        private void InitSearchPaths()
        {
            var paths = SearchPaths
                .Split(';')
                .Select(p => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p))
                .ToList();
            IronRubyEngine.Engine.SetSearchPaths(paths);
        }

        private void InitRack() {
            IronRubyEngine.SetToplevelBinding();

            if (GemPath != null && GemPath.Length > 0) {
                Utils.Log("=> Setting GEM_PATH: " + IronRubyEngine.Context.Inspect(GemPath));
                Environment.SetEnvironmentVariable("GEM_PATH", GemPath);
            }

            if (RackEnv != null) {
                Utils.Log("=> Setting RACK_ENV: " + IronRubyEngine.Context.Inspect(RackEnv));
                Environment.SetEnvironmentVariable("RACK_ENV", RackEnv);
            }

            Utils.Log("=> Loading RubyGems");
            IronRubyEngine.Require("rubygems");

            Utils.Log("=> Loading Rack " + RackVersion);
            IronRubyEngine.Require("rack", RackVersion);
            Utils.Log(string.Format("=> Loaded rack-{0}", Utils.Join(ActualRackVersion, ".")));

            Utils.Log("=> Application root: " + IronRubyEngine.Context.Inspect(AppRoot));
            IronRubyEngine.AddLoadPath(AppRoot);
        }

        private object Rackup() {
            Utils.Log("=> Loading Rack application");
            var fullPath = Path.Combine(_appRoot, "config.ru");
            if (File.Exists(fullPath)) {
                return IronRubyEngine.ExecuteMethod<RubyArray>(
                    IronRubyEngine.Execute("Rack::Builder"),
                    "parse_file", MutableString.CreateAscii(fullPath))[0];
            }
            return null;
        }

        public object App {
            get { return _app; }
        }


        private string PhysicalAppPath { get; set; }

        internal string AppRoot {
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
                if (_gemPath == null) {
                    var gemPathFromConfig = ConfigurationManager.AppSettings[GemPathOptionName];
                    _gemPath = gemPathFromConfig == null ? "" : GetFullPath(gemPathFromConfig, PhysicalAppPath);
                }
                return _gemPath;
            }
        }

        public string RackEnv {
            get {
                if (_rackEnv == null)
                    _rackEnv = ConfigurationManager.AppSettings[RackEnvOptionName] ?? "deployment";
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
                    _actualRackVersion = IronRubyEngine.Execute<RubyArray>("Rack::VERSION");
                return _actualRackVersion;
            }
        }

        public string PublicDir {
            get {
                if (_publicDir == null) {
                    _publicDir = ConfigurationManager.AppSettings[PublicDirOptionName];
                    if (_publicDir == null || _publicDir.Contains("..")) {
                        _publicDir = "public";
                    }
                }
                return _publicDir;
            }
            set {
                if (_publicDir == null || _publicDir.Contains("..")) {
                    throw new FormatException("PublicDir must not be null or contain any \"..\" sequences");
                }
            }
        }

        public string SearchPaths
        {
            get
            {
                if(_searchPaths == null) {
                    _searchPaths = ConfigurationManager.AppSettings[SearchPathsOptionName] ?? "";
                }
                return _searchPaths;
            }
        }

        private static string GetFullPath(string path, string root) {
            return Path.GetFullPath(Path.Combine(root, path));
        }
    }
}
