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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Chiron {
    static class Chiron {

        private static bool _WebServer;
        private static int _Port = 2060;
        private static bool _LaunchBrowser;
        private static string _StartPage;
        private static string _Directory = Directory.GetCurrentDirectory();
        private static string _XapFile;
        private static bool _XapDlr;
        private static bool _SaveManifest;
        private static bool _NotifyIcon;
        private static bool _Silent;
        private static bool _NoLogo;
        private static bool _Help;

        private static string _Error;
        
        private static string[] _LocalPath = new string[] { };
        internal static string[] LocalPath {
            get { return _LocalPath; }
            set { _LocalPath = value; }
        }

        private static bool _AnyAddress;
        internal static bool AnyAddress {
            get {
                return _AnyAddress;
            }
            set {
                _AnyAddress = value;
            }
        }
        
        private static AppManifestTemplate _ManifestTemplate;
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

        private static Dictionary<string, LanguageInfo> _Languages;
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

        private static string _UrlPrefix;
        /// <summary>
        /// Optional URL prefix for language assemblies
        /// </summary>
        internal static string UrlPrefix {
            get {
                if (_UrlPrefix == null) {
                    UrlPrefix = ConfigurationManager.AppSettings["urlPrefix"];
                }
                return _UrlPrefix;
            }
            set {
                _UrlPrefix = value;
                if (_UrlPrefix == null) _UrlPrefix = "";
                else {
                    if (!_UrlPrefix.EndsWith("/")) _UrlPrefix += '/';
                    // validate
                    Uri uri = new Uri(_UrlPrefix, UriKind.RelativeOrAbsolute);
                    if (!uri.IsAbsoluteUri && !_UrlPrefix.StartsWith("/")) {
                        _UrlPrefix = null;
                        throw new ConfigurationErrorsException("urlPrefix must be an absolute URI or start with a /");
                    }
                }
            }
        }

        private static bool? _DetectLanguageFromXAP;
        internal static bool DetectLanguage {
            get {
                if (_DetectLanguageFromXAP == null) {
                    string value = ConfigurationManager.AppSettings["detectLanguage"] ?? "true";
                    _DetectLanguageFromXAP = bool.Parse(value);
                }
                return _DetectLanguageFromXAP.Value;
            }
            set {
                _DetectLanguageFromXAP = value;
            }
        }

        private static bool? _UseExtensions;
        internal static bool UseExtensions {
            get {
                if (_UseExtensions == null) {
                    string value = ConfigurationManager.AppSettings["useExtensions"] ?? "false";
                    _UseExtensions = bool.Parse(value);
                }
                return _UseExtensions.Value;
            }
            set {
                _UseExtensions = value;
            }
        }

        private static Dictionary<string, string> _MimeMap;
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

        private static string _LocalAssemblyPath;

        // Looks for a DLR/language assembly relative to Chiron.exe
        // The path is set in localAssemblyPath in Chiron.exe.config's appSettings section
        internal static string TryGetAssemblyPath(string name) {
            if (_LocalAssemblyPath == null) {

                _LocalAssemblyPath = ConfigurationManager.AppSettings["localAssemblyPath"] ?? "";
                if (!Path.IsPathRooted(_LocalAssemblyPath)) {
                    _LocalAssemblyPath = Path.Combine(ChironPath(), _LocalAssemblyPath);
                }
                if (!Directory.Exists(_LocalAssemblyPath)) {
                    // fallback to Chiron install location
                    _LocalAssemblyPath = ChironPath();
                }
            }

            string path = Path.Combine(_LocalAssemblyPath, Path.GetFileName(name));
            return File.Exists(path) ? path : null;
        }

        static void ParseOptions(string[] args) {
            foreach (string option in args) {
                if (option[0] != '-' && option[0] != '/') {
                    _Error = string.Format("Invalid option '{0}'", option);
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
                    case "w":
                    case "webserver":
                        if (!string.IsNullOrEmpty(val)) {
                            if (!int.TryParse(val, out _Port) || _Port <= 0) {
                                _Error = string.Format("Invalid port '{0}'", val);
                                return;
                            }
                        }
                        _WebServer = true;
                        break;
                    case "d":
                    case "dir":
                    case "directory":
                        try {
                            _Directory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                            if (!Directory.Exists(_Directory)) throw new InvalidDataException();
                        } catch {
                            _Error = string.Format("Invalid directory '{0}'", val);
                            return;
                        }
                        break;
                    case "x":
                    case "xap":
                    case "xapfile":
                    case "z":
                    case "zipdlr":
                        if (string.IsNullOrEmpty(val)) {
                            _Error = "missing xapfile name";
                            return;
                        }
                        try {
                            _XapFile = Path.GetFullPath(Path.Combine(System.IO.Directory.GetCurrentDirectory(), val));
                        } catch {
                            _Error = string.Format("Invalid xapfile '{0}'", val);
                            return;
                        }
                        if (opt.StartsWith("z")) {
                            _XapDlr = true;
                        }
                        break;
                    case "r":
                    case "refpath":
                        try {
                            _LocalAssemblyPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), val));
                            if (!Directory.Exists(_LocalAssemblyPath)) throw new InvalidDataException();
                        } catch {
                            _Error = string.Format("Invalid refpath '{0}'", val);
                            return;
                        }
                        break;
                    case "b":
                    case "browser":
                        if (!string.IsNullOrEmpty(val)) {
                            _StartPage = val.Replace(Path.DirectorySeparatorChar, '/');
                        }
                        _LaunchBrowser = true;
                        _WebServer = true;
                        break;
                    case "u":
                    case "urlPrefix":
                        try {
                            UrlPrefix = val;
                        } catch {
                            _Error = string.Format("Invalid urlPrefix '{0}'", val);
                            return;
                        }
                        break;
                    case "e":
                    case "useExtensions":
                        UseExtensions = true;
                        break;
                    case "nl":
                    case "noLanguageDetection":
                        DetectLanguage = false;
                        break;
                    case "a":
                    case "anyAddress":
                        _AnyAddress = true;
                        break;
                    case "p":
                    case "path":
                        ParseAndSetLocalPath(val);
                        break;
                    case "m":
                    case "manifest":
                        _SaveManifest = true;
                        break;
                    case "n":
                    case "nologo":
                        _NoLogo = true;
                        break;
                    case "notification":
                        _NotifyIcon = true;
                        break;
                    case "s":
                    case "silent":
                        _Silent = true;
                        _NoLogo = true;
                        break;
                    case "?":
                    case "h":
                    case "help":
                        _Help = true;
                        return;
                    default:
                        _Error = string.Format("Invalid option '{0}'", option);
                        return;
                }
            }

            if (_XapFile != null && _WebServer)
                _Error = "/x or /z cannot be used together with /w or /b";

            if (_SaveManifest && (_XapFile != null || _WebServer))
                _Error = "/m can only be used with /d, /s and /n";

            if (args.Length == 0)
                _Help = true;
        }

        static int Main(string[] args) {
            ParseOptions(args);

            if (!_NoLogo) {
                Console.WriteLine(
                  "Chiron - Silverlight Dynamic Language Development Utility. Version {0}", 
                  typeof(Chiron).Assembly.GetName().Version
                );
            }

            if (_Help) {
                Console.WriteLine(
@"Usage: Chiron [<options>]

Common usages:

  Chiron.exe /b
    Starts the web-server on port 2060, and opens the default browser
    to the root of the web-server. This is used for developing an
    application, as Chiron will rexap you application's directory for
    every request.
    
  Chiron.exe /d:app /z:app.xap
    Takes the contents of the app directory and generates an app.xap 
    from it, which embeds the DLR and language assemblies according to
    the settings in Chiron.exe.config. This is used for deployment,
    so you can take the generated app.xap, along with any other files,
    and host them on any web-server.

Options:

  Note: forward-slashes (/) in option names can be substituted for dashes (-).
        For example ""Chiron.exe -w"" instead of ""Chiron.exe /w"".

  /w[ebserver][:<port number>]
    Launches a development web server that automatically creates
    XAP files for dynamic language applications (runs /z for every
    request of a XAP file, but generates it in memory).
    Optionally specifies server port number (default: 2060)

  /b[rowser][:<start url>]
    Launches the default browser and starts the web server
    Implies /w, cannot be combined with /x or /z

  /z[ipdlr]:<file>
    Generates a XAP file, including dynamic language dependencies, and
    auto-generates AppManifest.xaml (equivalent of /m in memory), 
    if it does not exist.
    Does not start the web server, cannot be combined with /w or /b

  /m[anifest]
    Saves the generated AppManifest.xaml file to disk
    Use /d to set the directory containing the sources
    Can only be combined with /d, /n and /s

  /d[ir[ectory]]:<path>
    Specifies directory on disk (default: the current directory).
    Implies /w.

  /r[efpath]:<path>
    Path where assemblies are located. Defaults to the same directory
    where Chiron.exe exists.
    Overrides appSettings.localAssemblyPath in Chiron.exe.config.

  /p[ath]:<path1;path2;..;pathn>
    Semi-colon-separated directories to be included in the XAP file,
    in addition to what is specified by /d
    
  /u[rlprefix]:<relative or absolute uri>
    Appends a relative or absolute Uri to each language assembly or extension
    added to the AppManifest.xaml. If a relative Uri is provided, Chiron 
    will serve all files located in the /refpath at this Uri, relative to the
    root of the web-server.
    Overrides appSettings.urlPrefix in Chiron.exe.config.
  
  /nl /noLanguageDetection
    Without this flag, Chiron scans the current application directory for files
    with a valid language's file extension, and only makes those languages
    available to the XAP file. See /useExtensions for whether the languages
    assemblies or extension files are used.
    With this flag, no language-specific assemblies/extensions are added to the
    XAP, so the Silverlight application is responsible for parsing the
    languages.config file in the XAP and downloading the languages it needs.
    Overrides appSettings.detectLanguages in Chiron.exe.config.
  
  /e /useExtensions:true|false (default false)
    Toggles whether or not language and DLR assemblies are embedded in
    the XAP file, or whether their equivalent extension files are used.

  /a /anyAddress
    By default Chiron listens on just the loopback IP address, meaning it only
    works for local requests. Providing this flag will cause Chiron to
    listen on any IP address, making it an internet-facing webserver. This
    option should only be used during development when running the Silverlight
    app on one machine and testing on many machines; Chiron is not optimized as
    a production webserver.

  /x[ap[file]]:<file>
    Specifies XAP file to generate. Only XAPs a directory; does not
    generate a manifest or add dynamic language DLLs; see /z for that
    functionality.
    Does not start the web server, cannot be combined with /w or /b

  /notification
    Display notification icon
  
  /n[ologo]
    Suppresses display of the logo banner

  /s[ilent]
    Suppresses display of all output
");
            }
            else if (_Error != null) {
                return Error(1000, "options", _Error);
            }
            else if (!string.IsNullOrEmpty(_XapFile)) {
                try {
                    if (!_Silent)
                        Console.WriteLine("Generating XAP {0} from {1}", _XapFile, _Directory);

                    if (_XapDlr) {
                        XapBuilder.XapToDisk(_Directory, _XapFile);
                    } else {
                        ZipArchive xap = new ZipArchive(_XapFile, FileAccess.Write);
                        XapBuilder.AddPathDirectories(xap);
                        xap.CopyFromDirectory(_Directory, "");
                        xap.Close();
                    }
                }
                catch (Exception ex) {
                    return Error(1001, "xap", ex.Message);
                }
            }
            else if (_SaveManifest) {
                try {
                    string manifest = Path.Combine(_Directory, "AppManifest.xaml");
                    if (File.Exists(manifest)) {
                        return Error(3002, "manifest", "AppManifest.xaml already exists at path " + manifest);
                    }

                    // Generate the AppManifest.xaml file to disk, as we would if we were
                    // generating it in the XAP
                    XapBuilder.GenerateManifest(_Directory).Save(manifest);
                } catch (Exception ex) {
                    return Error(3001, "manifest", ex.Message);
                }
            }
            else {
                string uri = string.Format("http://localhost:{0}/", _Port);

                if (!_Silent)
                    Console.WriteLine("Chiron serving '{0}' as {1}", _Directory, uri);

                try {
                    HttpServer server = new HttpServer(_Port, _Directory);
                    server.Start();

                    if (_LaunchBrowser) {
                        if (_StartPage != null) {
                            uri += _StartPage;
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo(uri);
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = _Directory;

                        Process p = new Process();
                        p.StartInfo = startInfo;
                        p.Start();
                    }

                    if (_NotifyIcon) {
                        Thread notify = new Thread(
                            () => {
                                Application.EnableVisualStyles();
                                Application.SetCompatibleTextRenderingDefault(false);

                                var notification = new Notification(_Directory, _Port);
                                Application.Run();
                            }
                        );
                        notify.SetApartmentState(ApartmentState.STA);
                        notify.IsBackground = true;
                        notify.Start();
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
            if (!_Silent) {
                string toolname = Path.GetFileName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                Console.WriteLine("{0}: {1} error CH{2}: {3}", toolname, category, code, message);
            }

            return code;
        }

        internal static void ParseAndSetLocalPath(string pathString) {
            var __path = new List<string>();
            string[] paths = pathString.Split(';');
            foreach (string path in paths) {
                var fullPath = path;
                if (!Path.IsPathRooted(path))
                    fullPath = Path.Combine(_Directory, path);
                if (Directory.Exists(fullPath))
                    __path.Add(fullPath);
            }
            _LocalPath = __path.ToArray();
        }

        internal static void Log(int statusCode, string uri, int byteCount, string message) {
            if (!_Silent) {
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

        private static string ChironPath() {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }
    }
}
