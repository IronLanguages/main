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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Scripting.Utils;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Microsoft.IronStudio.Project {
    /// <summary>
    /// Implements common functionality of starting a project or a file
    /// with or without debugging.
    /// </summary>
    public abstract class CommonStarter : IStarter {
        private readonly IServiceProvider/*!*/ _serviceProvider;
        private static Process _chironProcess;
        private static string _chironDir;
        private static int _chironPort;

        public CommonStarter(IServiceProvider/*!*/ serviceProvider) {
            ContractUtils.RequiresNotNull(serviceProvider, "serviceProvider");
            _serviceProvider = serviceProvider;
        }

        #region IStarter Members

        public virtual void StartProject(CommonProjectNode project, bool debug) {
            string startupFile = ResolveStartupFile(project);
            StartFile(project, startupFile, debug);
        }

        public virtual void StartFile(CommonProjectNode project, string/*!*/ file, bool debug) {
            string extension = Path.GetExtension(file);
            if (String.Equals(extension, ".html", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(extension, ".htm", StringComparison.OrdinalIgnoreCase)) {
                StartSilverlightApp(project, file, debug);
                return;
            }

            if (debug) {
                StartWithDebugger(project, file);
            } else {
                StartWithoutDebugger(project, file);
            }
        }

        private static Guid guidSilvelightDebug = new Guid("{032F4B8C-7045-4B24-ACCF-D08C9DA108FE}");

        public void StartSilverlightApp(CommonProjectNode project, string/*!*/ file, bool debug) {
            string webSiteRoot;
            if (project != null) {
                webSiteRoot = project.GetWorkingDirectory();
                file = Path.GetFullPath(Path.Combine(webSiteRoot, file));
            } else {
                file = Path.GetFullPath(file);
                webSiteRoot = Path.GetDirectoryName(file);
            }
            webSiteRoot = webSiteRoot.TrimEnd('\\');

            int port = EnsureChiron(webSiteRoot);
            
            string url = "http://localhost:" + port;
            if (file.StartsWith(webSiteRoot) && file.Length > webSiteRoot.Length && file[webSiteRoot.Length] == '\\') {
                url += file.Substring(webSiteRoot.Length).Replace('\\', '/');
            } else if (file.StartsWith("\\")) {
                url += file.Replace('\\', '/'); 
            } else{  
                url += '/' + file.Replace('\\', '/');
            }

            StartInBrowser(url, debug ? guidSilvelightDebug : (Guid?)null);
        }

        public void StartInBrowser(string url, Guid? debugEngine) {
            if (debugEngine.HasValue) {
                // launch via VS debugger, it'll take care of figuring out the browsers
                VsDebugTargetInfo dbgInfo = new VsDebugTargetInfo();
                dbgInfo.dlo = (DEBUG_LAUNCH_OPERATION)_DEBUG_LAUNCH_OPERATION3.DLO_LaunchBrowser;
                dbgInfo.bstrExe = url;
                dbgInfo.clsidCustom = debugEngine.Value;
                dbgInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd | (uint)__VSDBGLAUNCHFLAGS4.DBGLAUNCH_UseDefaultBrowser;
                dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);
                VsShellUtilities.LaunchDebugger(_serviceProvider, dbgInfo);
            } else {
                // run the users default browser
                var handler = GetBrowserHandlerProgId();
                var browserCmd = (string)Registry.ClassesRoot.OpenSubKey(handler).OpenSubKey("shell").OpenSubKey("open").OpenSubKey("command").GetValue("");

                if (browserCmd.IndexOf("%1") != -1) {
                    browserCmd = browserCmd.Replace("%1", url);
                } else {
                    browserCmd = browserCmd + " " + url;
                }
                bool inQuote = false;
                string cmdLine = null;
                for (int i = 0; i < browserCmd.Length; i++) {
                    if (browserCmd[i] == '"') {
                        inQuote = !inQuote;
                    }

                    if (browserCmd[i] == ' ' && !inQuote) {
                        cmdLine = browserCmd.Substring(0, i);
                        break;
                    }
                }
                if (cmdLine == null) {
                    cmdLine = browserCmd;
                }

                Process.Start(cmdLine, browserCmd.Substring(cmdLine.Length));
            }
        }

        private static string GetBrowserHandlerProgId() {
            try {
                return (string)Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Explorer").OpenSubKey("FileExts").OpenSubKey(".html").OpenSubKey("UserChoice").GetValue("Progid");
            } catch {
                return (string)Registry.ClassesRoot.OpenSubKey(".html").GetValue("");
            }
        }

        public virtual string InstallPath {
            get {
                return Path.GetDirectoryName(typeof(CommonStarter).Assembly.Location);
            }
        }

        public virtual string ChironPath {
            get {
                return Path.Combine(Path.GetDirectoryName(typeof(CommonStarter).Assembly.Location), "Chiron.exe");
            }
        }

        private int EnsureChiron(string/*!*/ webSiteRoot) {
            Debug.Assert(!webSiteRoot.EndsWith("\\"));

            if (_chironDir != webSiteRoot && _chironProcess != null && !_chironProcess.HasExited) {
                try {
                    _chironProcess.Kill();
                } catch {
                    // process already exited
                }
                _chironProcess = null;
            }

            if (_chironProcess == null || _chironProcess.HasExited) {                
                // start Chiron
                var chironPath = ChironPath;
                // Get a free port
                _chironPort = GetFreePort();

                // TODO: race condition - the port might be taked by the time Chiron attempts to open it
                // TODO: we should wait for Chiron before launching the browser

                string commandLine = "/w:" + _chironPort + " /notification /d:";

                if (webSiteRoot.IndexOf(' ') != -1) {
                    commandLine += "\"" + webSiteRoot + "\"";
                } else {
                    commandLine += webSiteRoot;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(chironPath, commandLine);
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                _chironDir = webSiteRoot;
                _chironProcess = Process.Start(startInfo);
            }

            return _chironPort;
        }

        private static int GetFreePort() {
            return Enumerable.Range(new Random().Next(1200, 2000), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        public virtual void StartFile(string/*!*/ file, bool debug) {
            StartFile(null, file, debug);
        }

        #endregion

        #region Abstracts

        /// <summary>
        /// Returns full path of the language specififc iterpreter executable file.
        /// </summary>
        public abstract string/*!*/ GetInterpreterExecutable(CommonProjectNode project);

        private string/*!*/ GetInterpreterExecutableInternal(CommonProjectNode project) {
            string result;
            if (project != null) {
                result = (project.ProjectMgr.GetProjectProperty(CommonConstants.InterpreterPath, true) ?? "").Trim();
                if (!String.IsNullOrEmpty(result)) {
                    if (!File.Exists(result)) {
                        throw new FileNotFoundException(String.Format("Interpreter specified in the project does not exist: '{0}'", result), result);
                    }
                    return result;
                }
            }
            
            result = GetInterpreterExecutable(project);
            if (result == null) {
                ContractUtils.RequiresNotNull(result, "result of GetInterpreterExecutable");
            }
            return result;
        }

        /// <summary>
        /// Creates language specific command line for starting the project without debigging.
        /// </summary>
        public abstract string CreateCommandLineNoDebug(CommonProjectNode project, string startupFile);
        /// <summary>
        /// Creates language specific command line for starting the project with debigging.
        /// </summary>
        public abstract string CreateCommandLineDebug(CommonProjectNode project, string startupFile);

        #endregion

        #region Protected members
        /// <summary>
        /// Default implementation of the "Start withput Debugging" command.
        /// </summary>
        protected virtual void StartWithoutDebugger(CommonProjectNode project, string startupFile) {
            Process.Start(CreateProcessStartInfoNoDebug(project, startupFile));
        }

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        protected virtual void StartWithDebugger(CommonProjectNode project, string startupFile) {
            VsDebugTargetInfo dbgInfo = new VsDebugTargetInfo();
            dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);
            IntPtr ptr = Marshal.AllocCoTaskMem((int)dbgInfo.cbSize);
            try {
                Marshal.StructureToPtr(dbgInfo, ptr, false);
                SetupDebugInfo(ref dbgInfo, project, startupFile);

                LaunchDebugger(_serviceProvider, dbgInfo);
            } finally {
                if (ptr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
        }

        private static void LaunchDebugger(IServiceProvider provider, VsDebugTargetInfo dbgInfo) {
            if (!Directory.Exists(UnquotePath(dbgInfo.bstrCurDir))) {
                MessageBox.Show(String.Format("Working directory \"{0}\" does not exist.", dbgInfo.bstrCurDir));
            } else if (!File.Exists(UnquotePath(dbgInfo.bstrExe))) {
                MessageBox.Show(String.Format("Interpreter \"{0}\" does not exist.", dbgInfo.bstrExe));
            } else {
                VsShellUtilities.LaunchDebugger(provider, dbgInfo);
            }
        }

        private static string UnquotePath(string p) {
            if (p.StartsWith("\"") && p.EndsWith("\"")) {
                return p.Substring(1, p.Length - 2);
            }
            return p;
        }

        //TODO: this method should be protected, but due to IPy bug #19649
        //we keep it temporary public.
        /// <summary>
        /// Sets up debugger information.
        /// </summary>
        public virtual void SetupDebugInfo(ref VsDebugTargetInfo dbgInfo, 
                CommonProjectNode project, string startupFile) {
            dbgInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            dbgInfo.bstrExe = GetInterpreterExecutableInternal(project);
            dbgInfo.bstrCurDir = project != null ?
                project.GetWorkingDirectory() :
                Path.GetDirectoryName(startupFile);
            dbgInfo.bstrArg = CreateCommandLineDebug(project, startupFile);
            dbgInfo.bstrRemoteMachine = null;
            dbgInfo.fSendStdoutToOutputWindow = 0;                  
            StringDictionary env = new StringDictionary();
            
            SetupEnvironment(project, env);
            if (env.Count > 0) {
                // add any inherited env vars
                var variables = Environment.GetEnvironmentVariables();
                foreach (var key in variables.Keys) {
                    string strKey = (string)key;
                    if (!env.ContainsKey(strKey)) {
                        env.Add(strKey, (string)variables[key]);
                    }
                }

                //Environemnt variables should be passed as a
                //null-terminated block of null-terminated strings. 
                //Each string is in the following form:name=value\0
                StringBuilder buf = new StringBuilder();
                foreach (DictionaryEntry entry in env) {
                    buf.AppendFormat("{0}={1}\0", entry.Key, entry.Value);
                }
                buf.Append("\0");
                dbgInfo.bstrEnv = buf.ToString();
            }
            //Set the managed debugger
            dbgInfo.clsidCustom = VSConstants.CLSID_ComPlusOnlyDebugEngine;
            dbgInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        /// <summary>
        /// Sets up environment variables before starting the project.
        /// </summary>
        protected virtual void SetupEnvironment(CommonProjectNode project, StringDictionary environment) {
            //Do nothing by default
        }

        /// <summary>
        /// Creates process info used to start the project with no debugging.
        /// </summary>
        protected virtual ProcessStartInfo CreateProcessStartInfoNoDebug(CommonProjectNode project, string startupFile) {
            string command = CreateCommandLineNoDebug(project, startupFile);

            string interpreter = GetInterpreterExecutableInternal(project);
            command = "/c \"\"" + interpreter + "\" " + command + " & pause\"";
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", command);

            startInfo.WorkingDirectory = project != null ?
                project.GetWorkingDirectory() :
                Path.GetDirectoryName(startupFile);

            //In order to update environment variables we have to set UseShellExecute to false
            startInfo.UseShellExecute = false;
            SetupEnvironment(project, startInfo.EnvironmentVariables);
            return startInfo;
        }

        #endregion

        #region Private methods

        private string ResolveStartupFile(CommonProjectNode project) {
            if (project == null) {
                throw new ArgumentNullException("project");
            }
            string startupFile = project.GetStartupFile();
            if (string.IsNullOrEmpty(startupFile)) {
                //TODO: need to start active file then
                throw new ApplicationException("No startup file is defined for the startup project.");
            }
            return startupFile;
        }

        #endregion
    }
}
