/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.IronPythonTools.Navigation;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Project;
using Microsoft.Win32;

namespace Microsoft.IronPythonTools.Project {

    /// <summary>
    /// Provides Python specific functionality for starting a project 
    /// or a file with or without debugging.
    /// </summary>
    public class PythonStarter : CommonStarter, IPythonStarter {
        private static string _ipyExe, _ipywExe;

        public PythonStarter(IServiceProvider serviceProvider) : base(serviceProvider) {}

        public override string InstallPath {
            get {
                return PythonRuntimeHost.GetPythonInstallDir() ?? Path.GetDirectoryName(typeof(PythonStarter).Assembly.Location);
            }
        }

        public override string ChironPath {
            get {
                string result;
#if DEBUG
                result = Environment.GetEnvironmentVariable("DLR_ROOT");
                if (result != null) {
                    result = Path.Combine(result, @"Bin\Debug\Chiron.exe");
                    if (File.Exists(result)) {
                        return result;
                    }
                }
#endif
                result = PythonRuntimeHost.GetPythonInstallDir();
                if (result != null) {
                    result = Path.Combine(result, @"Silverlight\bin\Chiron.exe");
                    if (File.Exists(result)) {
                        return result;
                    }
                }

                return base.ChironPath;
            }
        }

        private string InterpreterExecutable {
            get {
                if (_ipyExe == null) {
                    //ipy.exe is installed along with package assembly
                    _ipyExe = Path.Combine(
                        InstallPath,
                        "ipy.exe");
                }
                return _ipyExe;
            }
        }

        private string WindowsInterpreterExecutable {
            get {
                if (_ipywExe == null) {
                    //ipy.exe is installed along with package assembly
                    _ipywExe = Path.Combine(
                        InstallPath,
                        "ipyw.exe");
                }
                return _ipywExe;
            }
        }

        protected override void SetupEnvironment(CommonProjectNode project, StringDictionary environment) {
            if (project != null) {
                //IronPython passes search path via IRONPYTHONPATH environment variable
                string searchPath = project.GetProjectProperty(CommonConstants.SearchPath, true);
                if (!string.IsNullOrEmpty(searchPath)) {
                    environment[PythonConstants.IronPythonPath] = searchPath;
                }
            }
        }

        public override string CreateCommandLineNoDebug(CommonProjectNode project, string startupFile) {
            string cmdLineArgs = null;
            if (project != null) {
                cmdLineArgs = project.GetProjectProperty(CommonConstants.CommandLineArguments, true);
            }

            return String.Format("{0} \"{1}\" {2}", GetOptions(project), startupFile, cmdLineArgs);
        }

        public override string GetInterpreterExecutable(CommonProjectNode project) {
            bool isWindows = Convert.ToBoolean(project.GetProjectProperty(CommonConstants.IsWindowsApplication, true));
            return isWindows ? WindowsInterpreterExecutable : InterpreterExecutable;
        }

        public override string CreateCommandLineDebug(CommonProjectNode project, string startupFile) {
            string cmdLineArgs = null;
            if (project != null) {
                cmdLineArgs = project.GetProjectProperty(CommonConstants.CommandLineArguments, true);
            }
            return String.Format("-X:Debug {0} \"{1}\" {2}", GetOptions(project), startupFile, cmdLineArgs);
        }

        private string GetOptions(CommonProjectNode project) {
            if (project != null) {
                var debugStdLib = project.GetProjectProperty(PythonConstants.DebugStandardLibrary, false);
                bool debugStdLibResult;
                if (!bool.TryParse(debugStdLib, out debugStdLibResult) || !debugStdLibResult) {
                    var res = "-X:NoDebug \"" + System.Text.RegularExpressions.Regex.Escape(Path.Combine(InstallPath, "Lib\\")) + ".*\"";
                    
                    return res;
                }
            }

            return "";
        }
    }
}
