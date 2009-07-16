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
using System.Text;
using System.Windows;
using System.IO;

namespace Microsoft.Scripting.Silverlight {
    internal static class Settings {
        #region Properties
        /// <summary>
        /// Returns the entry point file. If it is null, look for a file with
        /// the default entry point name and a valid language extension.
        /// </summary>
        internal static string EntryPoint {
            get {
                if (_entryPoint != null)
                    return _entryPoint;
                Stream stream = null;
                if (_entryPoint == null) {
                    if (DynamicApplication.Current.Engine == null || DynamicApplication.Current.Engine.Runtime == null)
                        throw new ApplicationException("Can not discover entry point before the DLR is initialized");
                    foreach (var ext in DynamicApplication.Current.Engine.LanguageExtensions()) {
                        var file = Settings.DefaultEntryPoint + ext;
                        var tempStream = BrowserPAL.PAL.VirtualFilesystem.GetFile(file);
                        if (tempStream != null) {
                            if (_entryPoint != null)
                                throw new ApplicationException(string.Format("Application can only have one entry point, but found two: {0}, {1}", _entryPoint, file));
                            _entryPoint = file;
                            stream = tempStream;
                        }
                    }
                    if (_entryPoint == null || stream == null)
                        throw new ApplicationException(string.Format("Application must have an entry point called {0}.*, where * is the language's extension", DefaultEntryPoint));
                }
                return _entryPoint;
            }
            private set {
                _entryPoint = value;
            }
        }
        private static string _entryPoint;

        internal static string GetEntryPoint() {
            return Settings.EntryPoint;
        }

        /// <summary>
        /// Determines if we emit optimized code, and whether turn on debugging features
        /// </summary>
        internal static bool Debug { get; private set; }
        
        /// <summary>
        /// Indicates whether we report unhandled errors to the HTML page
        /// </summary>
        internal static bool ReportUnhandledErrors {
            get { return _reportErrors; }
            set {
                if (value != _reportErrors) {
                    _reportErrors = value;
                    if (_reportErrors) {
                        Application.Current.UnhandledException += DynamicApplication.Current.OnUnhandledException;
                    } else {
                        Application.Current.UnhandledException -= DynamicApplication.Current.OnUnhandledException;
                    }
                }
            }
        }
        private static bool _reportErrors;

        /// <summary>
        /// Indicates the HTML element where errors should be reported into.
        /// </summary>
        internal static string ErrorTargetID { get; private set; }

        /// <summary>
        /// Indicates whether or not CLR stack traces are shown in the error report
        /// </summary>
        internal static bool ExceptionDetail { get; private set; }

        /// <summary>
        /// Indicates whether or not a dynamic console gets added to the HTML page
        /// </summary>
        internal static bool ConsoleEnabled { get; private set; }

        /// <summary>
        /// Indicates where to look for script files outside of the XAP. 
        /// If null, it will only look in the XAP.
        /// </summary>
        internal static string DownloadScriptsFrom { get; private set; }

        /// <summary>
        /// Indicates whether or not script files are to be downloaded.
        /// </summary>
        internal static bool DownloadScripts {
            get { return DownloadScriptsFrom != null; }
            set {
                if(value) {
                    if (DownloadScriptsFrom == null)
                        DownloadScriptsFrom = "app";
                } else {
                    DownloadScriptsFrom = null;
                }
            }
        }

        /// <summary>
        /// The default entry point name
        /// </summary>
        internal static string DefaultEntryPoint { get { return _defaultEntryPoint; } }
        private static string _defaultEntryPoint = "app";

        /// <summary>
        /// The languages config file.
        /// </summary>
        internal static string LanguagesConfigFile { get { return _languagesConfigFile; } }
        private static string _languagesConfigFile = "languages.config";
        #endregion
        
        internal static void Parse(IDictionary<string, string> args) {
            // Turn error reporting on while we parse initParams.
            // (Otherwise, we would silently fail if the initParams has an error)
            ReportUnhandledErrors = true;

            string entryPoint;
            args.TryGetValue("start", out entryPoint);
            EntryPoint = entryPoint;

            string consoleEnabledStr;
            bool consoleEnabled = false;
            if (args.TryGetValue("console", out consoleEnabledStr)) {
                if (!bool.TryParse(consoleEnabledStr, out consoleEnabled)) {
                    throw new ArgumentException("You must set 'console' to 'true' or 'false', for example: initParams: \"..., console=true\"");
                }
            }
            ConsoleEnabled = consoleEnabled;

            string debugStr;
            bool debug = false;
            if (args.TryGetValue("debug", out debugStr)) {
                if (!bool.TryParse(debugStr, out debug)) {
                    throw new ArgumentException("You must set 'debug' to 'true' or 'false', for example: initParams: \"..., debug=true\"");
                }
            }
            Debug = debug;

            string exceptionDetailStr;
            bool exceptionDetail = false;
            if (args.TryGetValue("exceptionDetail", out exceptionDetailStr)) {
                if (!bool.TryParse(exceptionDetailStr, out exceptionDetail)) {
                    throw new ArgumentException("You must set 'exceptionDetail' to 'true' or 'false', for example: initParams: \"..., exceptionDetail=true\"");
                }
            }
            ExceptionDetail = exceptionDetail;

            string reportErrorsDiv;
            if (args.TryGetValue("reportErrors", out reportErrorsDiv)) {
                ErrorTargetID = reportErrorsDiv;
                ReportUnhandledErrors = true;
            } else {
                // if reportErrors is unspecified, set to false
                ReportUnhandledErrors = false;
            }

            string downloadScriptsStr;
            bool downloadScripts = false;
            if (args.TryGetValue("downloadScripts", out downloadScriptsStr)) {
                if (!bool.TryParse(downloadScriptsStr, out downloadScripts)) {
                    throw new ArgumentException("You must set 'downloadScripts' to 'true' or 'false', for example: initParams: \"..., downloadScripts=true\"");
                }
            }
            DownloadScripts = downloadScripts;

            string downloadScriptsFrom;
            if (args.TryGetValue("downloadScriptsFrom", out downloadScriptsFrom)) {
                DownloadScriptsFrom = downloadScriptsFrom;
            }
        }
    }
}
