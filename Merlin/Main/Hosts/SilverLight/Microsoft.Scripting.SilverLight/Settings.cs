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
using System.Windows.Browser;

namespace Microsoft.Scripting.Silverlight {
    internal static class Settings {
        #region Properties
        /// <summary>
        /// Returns the entry point file path.
        /// </summary>
        internal static string EntryPoint {
            get {
                return _entryPoint;
            }
            private set {
                _entryPoint = GetAndValidateEntryPoint(
                    value,
                    DynamicApplication.Current.LanguagesConfig,
                    DynamicApplication.Current.ScriptTags
                );
            }
        }
        private static string _entryPoint;

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

            // Keep reportErrors on until the end, then set it to this value:
            bool setReportErrorsWhenDone = false;

            string reportErrorsDiv;
            if (args.TryGetValue("reportErrors", out reportErrorsDiv)) {
                ErrorTargetID = reportErrorsDiv;
                setReportErrorsWhenDone = true;
            } else {
                // if reportErrors is unspecified, set to false
                setReportErrorsWhenDone = false;
            }

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

            string entryPoint;
            args.TryGetValue("start", out entryPoint);
            EntryPoint = entryPoint;

            ReportUnhandledErrors = setReportErrorsWhenDone;
        }

        /// <summary>
        /// Gets and validates the entry-point.
        /// Pre-conditions:  entryPoint is the path to validate
        ///                  langConfig is the list of languages used to detect
        ///                  the entry point. scriptTags track the <script> 
        ///                  blocks on the HTML page.
        /// Post-conditions: returns the name of the validated entry-point. The
        ///                  return value can be null if no valid entry-point 
        ///                  exists while there exists a inline script-tag.
        ///                  The language of the entry-point is marked as "used"
        ///                  in langConfig.
        /// </summary>
        private static string GetAndValidateEntryPoint
        (string entryPoint, DynamicLanguageConfig langConfig, DynamicScriptTags scriptTags) {
            if (entryPoint == null) {
                entryPoint = FindEntryPoint(langConfig);
            } else {
                var vfs = BrowserPAL.PAL.VirtualFilesystem;
                var stream = vfs.GetFile(entryPoint);
                if (stream == null) {
                    throw new ApplicationException(
                      string.Format(
"Application expected to have an entry point called {0}, but was not found (check the {1})", 
                        entryPoint, BrowserPAL.PAL.VirtualFilesystem.Name()));
                }
            }
            if(entryPoint != null) {
                var entryPointExt = Path.GetExtension(entryPoint);
                foreach (var lang in langConfig.Languages) {
                    foreach (var ext in lang.Extensions) {
                        if (entryPointExt == ext) {
                            langConfig.LanguagesUsed[lang.Names[0].ToLower()] = true;
                            break;
                        }
                    }
                }
            }
            return entryPoint;
        }

        /// <summary>
        /// Pre-conditions:  langConfig is the set of languages used to detect 
        ///                  the entry point.
        /// Post-conditions: Returns the entryPoint's path as a string.
        ///                  It may still be null if the entry-point was not 
        ///                  found. An exception is thrown if multiple entry 
        ///                  points are found.
        /// </summary>
        private static string FindEntryPoint(DynamicLanguageConfig langConfig) {
            string entryPoint = null;
            foreach (var lang in langConfig.Languages) {
                foreach (var ext in lang.Extensions) {
                    var file = Settings.DefaultEntryPoint + ext;
                    var tempStream = BrowserPAL.PAL.VirtualFilesystem.GetFile(file);
                    if (tempStream != null) {
                        if (entryPoint != null) {
                            throw new ApplicationException(string.Format(
"Application can only have one entry point, but found two: {0}, {1}", 
                              _entryPoint, file
                            ));
                        }
                        entryPoint = file;
                    }
                }
            }
            return entryPoint;
        }
    }
}
