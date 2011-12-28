/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Microsoft.IronRubyTools.Navigation;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Project;
using System.Text;
using IronRuby.Runtime;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;
using Microsoft.Scripting.Utils;
using System.Net.Sockets;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Microsoft.IronRubyTools.Project {
    /// <summary>
    /// Provides Ruby specific functionality for starting a project or a file with or without debugging.
    /// </summary>
    public sealed class RubyStarter : CommonStarter, IRubyStarter {
        public RubyStarter(IServiceProvider/*!*/ serviceProvider) 
            : base(serviceProvider) {
        }

        private string/*!*/ CreateCommandLine(CommonProjectNode project, string/*!*/ startupFile, bool debug) {
            var commandLine = new StringBuilder();

            bool disableDebugging = project != null && Convert.ToBoolean(project.GetProjectProperty(RubyConstants.ProjectProperties.DisableDebugging, true));
            if (debug && !disableDebugging) {
                commandLine.Append(" -D");
            }

            string searchPath = (project != null) ? project.GetProjectProperty(CommonConstants.SearchPath, true) : null;
            if (!String.IsNullOrEmpty(searchPath)) {
                foreach (string path in searchPath.Split(Path.PathSeparator)) {
                    try {
                        // the path is relative to the .rbproj file, not to the ir.exe file:
                        var fullPath = RubyUtils.CanonicalizePath(Path.GetFullPath(path));

                        commandLine.Append(" -I");
                        if (fullPath.IndexOf(' ') >= 0) {
                            commandLine.Append('"').Append(fullPath).Append('"');
                        } else {
                            commandLine.Append(path);
                        }
                    } catch {
                        // ignore
                    }
                }
            }

            commandLine.Append(' ').Append('"').Append(startupFile).Append('"');

            string args = null;
            string launcher = (project != null) ? project.GetProjectProperty(RubyConstants.ProjectProperties.Launcher, true) : null;
            if (launcher == RubyConstants.ProjectProperties.Launcher_Spec) {
                string testDir = Path.GetFullPath(Path.Combine(project.GetWorkingDirectory(), "test"));
                if (Directory.Exists(testDir)) {
                    args = String.Join(" ", 
                        Directory.GetFiles(testDir, "*_spec.rb").ConvertAll((path) => path.IndexOf(' ') >= 0 ? '"' + path + '"' : path)
                    );
                }
            } else {
                args = (project != null) ? project.GetProjectProperty(CommonConstants.CommandLineArguments, true) : null;
            }

            if (!String.IsNullOrEmpty(args)) {
                commandLine.Append(' ');
                commandLine.Append(args);
            }

            return commandLine.ToString();
        }

        public override string GetInterpreterExecutable(CommonProjectNode project) {
            return project != null && Convert.ToBoolean(project.GetProjectProperty(CommonConstants.IsWindowsApplication, true)) ?
                IronRubyToolsPackage.Instance.IronRubyWindowsExecutable :
                IronRubyToolsPackage.Instance.IronRubyExecutable;
        }


        public override void StartProject(CommonProjectNode project, bool debug) {
            IronRubyToolsPackage.Instance.RequireIronRubyInstalled(allowCancel: false);
            
            string launcher = (project != null) ? project.GetProjectProperty(RubyConstants.ProjectProperties.Launcher, true) : null;
            string file;
            switch (launcher) {
                case RubyConstants.ProjectProperties.Launcher_Rack:
                    file = Path.Combine(IronRubyToolsPackage.Instance.IronRubyToolsPath, "Rackup.rb");
                    break;

                case RubyConstants.ProjectProperties.Launcher_Spec:
                    file = Path.Combine(IronRubyToolsPackage.Instance.IronRubyToolsPath, "Spec.rb");
                    break;

                default:
                    file = project.GetStartupFile();
                    if (String.IsNullOrEmpty(file)) {
                        //TODO: need to start active file then
                        throw new ApplicationException("No startup file is defined for the startup project.");
                    }
                    break;
            }

            StartFile(project, file, debug);
        }

        public override void StartFile(CommonProjectNode project, string/*!*/ file, bool debug) {
            string appType = (project != null) ? project.GetProjectProperty(RubyConstants.ProjectProperties.RubyApplicationType, true) : null;
            if (appType == RubyConstants.ProjectProperties.RubyApplicationType_WebApp) {
                string host = "localhost";
                int port = Convert.ToInt32(project.GetProjectProperty(RubyConstants.ProjectProperties.DefaultPort, true));
                if (LaunchWebServer(project, file, host, port)) {
                    StartInBrowser("http://" + host + ":" + port, null);
                } else {
                    throw new ApplicationException(String.Format(
                        "Unable to start a web server by running file '{0}'. No response on http://{1}:{2}.", 
                        file, host, port
                    ));
                }
            } else {
                base.StartFile(project, file, debug);
            }
        }

        public override string CreateCommandLineNoDebug(CommonProjectNode project, string/*!*/ startupFile) {
            return CreateCommandLine(project, startupFile, false);
        }

        public override string CreateCommandLineDebug(CommonProjectNode project, string/*!*/ startupFile) {
            return CreateCommandLine(project, startupFile, true);
        }

        private bool LaunchWebServer(CommonProjectNode/*!*/ project, string/*!*/ file, string/*!*/ host, int port) {
            bool serverLaunched = false;
            
            Socket socket;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            int time = 0;
            while (time < 50000) {
                try {
                    socket.Connect(host, port);
                    socket.Disconnect(false);
                    return true;
                } catch (SocketException e) {
                    if (e.SocketErrorCode != SocketError.ConnectionRefused) {
                        throw;
                    }
                    if (!serverLaunched) {
                        Process.Start(CreateProcessStartInfoNoDebug(project, file));
                        serverLaunched = true;
                    }
                    Thread.Sleep(500);
                    time += 500;
                }
            }

            return false;
        }

        internal static bool ExecuteScriptFile(string/*!*/ scriptFile, string/*!*/ workingDirectory,
            IVsOutputWindowPane/*!*/ output, BackgroundWorker/*!*/ worker, ManualResetEvent/*!*/ cancelled) {

            if (scriptFile.IndexOf(' ') >= 0) {
                scriptFile = '"' + scriptFile + '"';
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(IronRubyToolsPackage.Instance.IronRubyExecutable, scriptFile);
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.CreateNoWindow = true;
            return ExecuteProcess(startInfo, output, worker, cancelled) == 0;
        }

        private static int ExecuteProcess(ProcessStartInfo/*!*/ startInfo,
            IVsOutputWindowPane/*!*/ output, BackgroundWorker/*!*/ worker, ManualResetEvent/*!*/ cancelled) {

            Assert.NotNull(startInfo, output);
            Process process;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process = Process.Start(startInfo);

            WaitHandle stdErr = RedirectOutput(process, process.StandardError, output, worker);
            WaitHandle stdOut = RedirectOutput(process, process.StandardOutput, output, worker);

            bool result = Wait(process, cancelled);
            stdErr.WaitOne();
            stdOut.WaitOne();
            return result ? process.ExitCode : -1;
        }

        private static bool Wait(Process/*!*/ process, WaitHandle/*!*/ cancelled) {
            switch (WaitHandle.WaitAny(new[] { process.GetWaitHandle(), cancelled })) {
                case 0: 
                    return true;

                case 1:
                    // cancelled:
                    process.Kill();
                    return false;

                default:
                    throw Assert.Unreachable;
            }
        }

        private static void StreamOutput(Process/*!*/ process, StreamReader/*!*/ from, IVsOutputWindowPane/*!*/ output, BackgroundWorker worker) {
            while (!process.HasExited) {
                string line;
                while ((line = from.ReadLine()) != null) {
                    output.OutputStringThreadSafe(line);
                    output.OutputStringThreadSafe("\n");
                    worker.ReportProgress(5);
                }
            }
        }

        private static WaitHandle/*!*/ RedirectOutput(Process/*!*/ process, StreamReader/*!*/ from, IVsOutputWindowPane/*!*/ output, BackgroundWorker worker) {
            var done = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(new WaitCallback((_) => {
                StreamOutput(process, from, output, worker);
                done.Set();
            }), null);

            return done;
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
                result = IronRubyToolsPackage.Instance.IronRubyBinPath;
                if (result != null) {
                    result = Path.Combine(result, @"..\Silverlight\bin\Chiron.exe");
                    if (File.Exists(result)) {
                        return result;
                    }
                }

                return base.ChironPath;
            }
        }
    }
}
