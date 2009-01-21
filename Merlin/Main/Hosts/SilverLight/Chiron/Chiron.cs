/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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
using System.Configuration;
using System.Xml;

namespace Chiron {
    static class Chiron {
        static int _port;
        static string _dir;
        static string _xapfile;
        static bool _webserver;
        static bool _browser;
        static bool _silent;
        static bool _nologo;
        static bool _help;
        static bool _zipdlr;
        static bool _saveManifest;
        static string _error;
        static string _startPage;

        // these properties are lazy loaded so we don't parse the configuration file unless necessary
        static AppManifestTemplate _ManifestTemplate;
        static Dictionary<string, LanguageInfo> _Languages;
        static string _UrlPrefix, _LocalAssemblyPath;
        static Dictionary<string, string> _MimeMap;

        static int Main(string[] args) {
            ParseOptions(args);

            if (!_nologo) {
                Console.WriteLine(
@"Microsoft(R) Silverlight(TM) Development Utility. Version {0}
Copyright (c) Microsoft Corporation.  All rights reserved.
", typeof(Chiron).Assembly.GetName().Version);
            }

            if (_help) {
                Console.WriteLine(
@"Usage: Chiron [<options>]

General Options:

  /d[irectory]:<path>
    Specifies directory on disk (default: the current directory)

  /x[ap]:<file>
    Specifies XAP file to generate
    Does not start the web server, cannot be combined with /w or /b

  /n[ologo]
    Suppresses display of the logo banner

  /s[ilent]
    Suppresses display of all output

Dynamic Language Options:

  /z[ipdlr]:<file>
    Like /x, but includes files needed for dynamic language apps
    Does not start the web server, cannot be combined with /w or /b

  /w[ebserver][:<port number>]
    Launches a development web server that automatically creates
    XAP files for dynamic language applications
    Optionally specifies server port number (default: 2060)

  /b[rowser][:<start url>]
    Launches the default browser and starts the web server
    Implies /w, cannot be combined with /x or /z

  /m[anifest]
    Saves the generated AppManifest.xaml file to disk
    Use /d to set the directory containing the sources
    Can only be combined with /d, /n and /s
");
            }
            else if (_error != null) {
                return Error(1000, "options", _error);
            }
            else if (!string.IsNullOrEmpty(_xapfile)) {
                try {
                    if (!_silent)
                        Console.WriteLine("Generating XAP {0} from {1}", _xapfile, _dir);

                    if (_zipdlr) {
                        XapBuilder.XapToDisk(_dir, _xapfile);
                    } else {
                        ZipArchive xap = new ZipArchive(_xapfile, FileAccess.Write);
                        xap.CopyFromDirectory(_dir, "");
                        xap.Close();
                    }
                }
                catch (Exception ex) {
                    return Error(1001, "xap", ex.Message);
                }
            }
            else if (_saveManifest) {
                try {
                    string manifest = Path.Combine(_dir, "AppManifest.xaml");
                    if (File.Exists(manifest)) {
                        return Error(3002, "manifest", "AppManifest.xaml already exists at path " + manifest);
                    }

                    // Generate the AppManifest.xaml file to disk, as we would if we were
                    // generating it in the XAP
                    XapBuilder.GenerateManifest(_dir).Save(manifest);
                } catch (Exception ex) {
                    return Error(3001, "manifest", ex.Message);
                }
            }
            else {
                string uri = string.Format("http://localhost:{0}/", _port);

                if (!_silent)
                    Console.WriteLine("Chiron serving '{0}' as {1}", _dir, uri);

                try {
                    HttpServer server = new HttpServer(_port, _dir);
                    server.Start();

                    if (_browser) {
                        if (_startPage != null) {
                            uri += _startPage;
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo(uri);
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = _dir;

                        Process p = new Process();
                        p.StartInfo = startInfo;
                        p.Start();
                    }

                    while (server.IsRunning) Thread.Sleep(500);
                } catch (Exception ex) {
                    return Error(2001, "server", ex.Message);
                }
            }

            return 0;
        }

        // Print out an error in a format suitable for msbuild. For example:
        // chiron.exe: XAP error CH1001: Access to the path 'app.xap' is denied.
        private static int Error(int code, string category, string message) {
            if (!_silent) {
                string toolname = Path.GetFileName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                Console.WriteLine("{0}: {1} error CH{2}: {3}", toolname, category, code, message);
            }

            return code;
        }

        static void ParseOptions(string[] args) {
            _port = 2060;
            _dir = Directory.GetCurrentDirectory();
            _xapfile = null;
            _zipdlr = false;
            _webserver = false;
            _browser = false;
            _silent = false;
            _nologo = false;
            _help = false;

            foreach (string option in args) {
                if (option[0] != '-' && option[0] != '/') {
                    _error = string.Format("Invalid option '{0}'", option);
                    return;
                }

                string opt = option.Substring(1);
                string val = string.Empty;
                int i = opt.IndexOf(':');
                if (i > 0) {
                    val = opt.Substring(i + 1);
                    opt = opt.Substring(0, i);
                }
                opt = opt.ToLowerInvariant();

                switch (opt) {
                case "w": case "webserver":
                    if (!string.IsNullOrEmpty(val)) {
                        if (!int.TryParse(val, out _port) || _port <= 0) {
                            _error = string.Format("Invalid port '{0}'", val);
                            return;
                        }
                    }
                    _webserver = true;
                    break;
                case "d": case "dir": case "directory":
                    try {
                        _dir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                        if (!Directory.Exists(_dir)) throw new InvalidDataException();
                    }
                    catch {
                        _error = string.Format("Invalid directory '{0}'", val);
                        return;
                    }
                    break;
                case "x": case "xap": case "xapfile":
                case "z": case "zipdlr":
                    if (string.IsNullOrEmpty(val)) {
                        _error = "missing xapfile name";
                        return;
                    }
                    try {
                        _xapfile = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                    }
                    catch {
                        _error = string.Format("Invalid xapfile '{0}'", val);
                        return;
                    }
                    if (opt.StartsWith("z")) {
                        _zipdlr = true;
                    }
                    break;
                case "r": case "refpath":
                    try {
                        _LocalAssemblyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                        if (!Directory.Exists(_LocalAssemblyPath)) throw new InvalidDataException();
                    } catch {
                        _error = string.Format("Invalid refpath '{0}'", val);
                        return;
                    }
                    break;
                case "b": case "browser":
                    if (!string.IsNullOrEmpty(val)) {
                        _startPage = val.Replace(Path.DirectorySeparatorChar, '/');
                    }
                    _browser = true;
                    _webserver = true;
                    break;
                case "m": case "manifest":
                    _saveManifest = true;
                    break;
                case "n": case "nologo":
                    _nologo = true;
                    break;
                case "s": case "silent":
                    _silent = true;
                    _nologo = true;
                    break;
                case "?": case "h": case "help":
                    _help = true;
                    return;
                default:
                    _error = string.Format("Invalid option '{0}'", option);
                    return;
                }
            }

            if (_xapfile != null && _webserver)
                _error = "/x or /z cannot be used together with /w or /b";

            if (_saveManifest && (_xapfile != null || _webserver))
                _error = "/m can only be used with /d, /s and /n";

            if (args.Length == 0)
                _help = true;
        }

        public static void Log(int statusCode, string uri, int byteCount, string message) {
            if (!_silent) {
                if (string.IsNullOrEmpty(message)) {
                    Console.WriteLine("{0,8:HH:mm:ss} {1,3} {2,9:n0} {3}",
                        DateTime.Now, statusCode, byteCount, uri);
                }
                else {
                    Console.WriteLine("{0,8:HH:mm:ss} {1,3} {2,9:n0} {3} [{4}]",
                        DateTime.Now, statusCode, byteCount, uri, message);
                }
            }
        }


        internal static AppManifestTemplate ManifestTemplate {
            get {
                if (_ManifestTemplate == null) {
                    _ManifestTemplate = (AppManifestTemplate)ConfigurationManager.GetSection("AppManifest.xaml");
                    if (_ManifestTemplate == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find AppManifest.xaml section");
                    }
                }
                return _ManifestTemplate;
            }
        }

        internal static Dictionary<string, LanguageInfo> Languages {
            get {
                if (_Languages == null) {
                    _Languages = (Dictionary<string, LanguageInfo>)ConfigurationManager.GetSection("Languages");
                    if (_Languages == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find Languages section");
                    }
                }
                return _Languages;
            }
        }

        /// <summary>
        /// Optional URL prefix for language assemblies
        /// </summary>
        internal static string UrlPrefix {
            get {
                if (_UrlPrefix == null) {
                    _UrlPrefix = ConfigurationManager.AppSettings["urlPrefix"];
                    if (_UrlPrefix == null) {
                        _UrlPrefix = "";
                    } else {
                        if (!_UrlPrefix.EndsWith("/"))
                            _UrlPrefix += '/';

                        // validate
                        Uri uri = new Uri(_UrlPrefix, UriKind.RelativeOrAbsolute);
                        if (!uri.IsAbsoluteUri && !_UrlPrefix.StartsWith("/"))
                            throw new ConfigurationErrorsException("urlPrefix must be an absolute URI or start with a /");
                    }
                }
                return _UrlPrefix;
            }
        }

        internal static Dictionary<string, string> MimeMap {
            get {
                if (_MimeMap == null) {
                    _MimeMap = (Dictionary<string, string>)ConfigurationManager.GetSection("MimeTypes");
                    if (_MimeMap == null) {
                        throw new ConfigurationErrorsException("Could not find application configuration file, or could not find MimeTypes section");
                    }
                }
                return _MimeMap;
            }
        }

        // Looks for a DLR/language assembly relative to Chiron.exe
        // The path is set in localAssemblyPath in Chiron.exe.config's appSettings section
        internal static string TryGetAssemblyPath(string name) {
            if (_LocalAssemblyPath == null) {
                string chironPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                _LocalAssemblyPath = ConfigurationManager.AppSettings["localAssemblyPath"] ?? "";
                if (!Path.IsPathRooted(_LocalAssemblyPath)) {
                    _LocalAssemblyPath = Path.Combine(chironPath, _LocalAssemblyPath);
                }
                if (!Directory.Exists(_LocalAssemblyPath)) {
                    // fallback to Chiron install location
                    _LocalAssemblyPath = chironPath;
                }
            }

            string path = Path.Combine(_LocalAssemblyPath, Path.GetFileName(name));
            return File.Exists(path) ? path : null;
        }
    }
}
