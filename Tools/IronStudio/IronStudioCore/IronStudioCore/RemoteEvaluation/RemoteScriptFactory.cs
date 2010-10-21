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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Microsoft.Scripting.Hosting;

namespace Microsoft.IronStudio.RemoteEvaluation {
    /// <summary>
    /// Provides creation of a remote ScriptRuntime for DLR based languages.
    /// 
    /// Creating an instance of the class will cause a new process to be started.  ScriptRuntimes
    /// can then be created in that new process by calling the CreateRuntime function.
    /// </summary>
    public sealed class RemoteScriptFactory : IDisposable {
        private readonly Process _remoteRuntimeProcess;
        private readonly AutoResetEvent _remoteOutputReceived = new AutoResetEvent(false);
        private readonly RemoteProxy _proxy;
        private readonly AsyncAccess _asyncAccess;        

        private static bool _registeredChannel;
        private static object _lock = new object();

        /// <summary>
        /// Creates a new RemoteScriptFactory.  A remote process will be created for
        /// execution of code.  The code will execute on a thread with the specified
        /// apartment state.
        /// </summary>
        public RemoteScriptFactory(ApartmentState aptState) {            
            Process process = null;
            try {
                RegisterChannel();

                process = new Process();
                process.StartInfo = GetProcessStartInfo(aptState);

                _remoteRuntimeProcess = process;

                if (!process.Start() || process.HasExited) {
                    throw new InvalidOperationException(String.Format("Failed to start remote REPL process: {0}", process.ExitCode));
                }

                // read in the channel names we will connect to.  There are two of them, one is
                // bound to a certain thread, the 2nd is async and enables aborting work items.
                string uri = process.StandardOutput.ReadLine();
                if (!uri.StartsWith("URI: ")) {
                    throw new InvalidOperationException("Didn't get URI, got " + uri);
                }

                string abortUri = process.StandardOutput.ReadLine();
                if (!abortUri.StartsWith("ABORTURI: ")) {
                    throw new InvalidOperationException("Didn't get ABORTURI, got " + abortUri);
                }

                // finally get our objects
                _proxy = (RemoteProxy)RemotingServices.Connect(typeof(RemoteProxy), uri.Substring(5));
                _proxy.SetParentProcess(Process.GetCurrentProcess().Id);

                _asyncAccess = (AsyncAccess)RemotingServices.Connect(typeof(AsyncAccess), abortUri.Substring(10));
            } finally {
                if (_proxy == null) {                    
                    if (process != null) {
                        TerminateProcess(process.Handle, 1);
                        process.Close();
                    }
                    GC.SuppressFinalize(this);
                }
            }
        }

        private static void RegisterChannel() {
            lock (_lock) {
                if (!_registeredChannel) {
                    var provider = new BinaryServerFormatterSinkProvider();
                    provider.TypeFilterLevel = TypeFilterLevel.Full;
                    var properties = new Hashtable();
                    properties["name"] = Guid.NewGuid().ToString();
                    properties["portName"] = Guid.NewGuid().ToString();

                    IpcChannel channel = new IpcChannel(properties, null, provider);
                    // Register a channel so that we can get communication back from the other side.
                    System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);
                    _registeredChannel = true;
                }
            }
        }

        ~RemoteScriptFactory() {
            Dispose();
        }

        #region Public APIs

        public static void RunServer(ApartmentState state) {
            new RemoteProxy(state).StartServer();
        }

        /// <summary>
        /// Creates a ScriptRuntime in the remote process.
        /// </summary>
        public ScriptRuntime CreateRuntime(ScriptRuntimeSetup setup) {
            return _proxy.CreateRuntime(setup);
        }

        /// <summary>
        /// Shuts down the remote process.
        /// </summary>
        public void Shutdown() {
            try {
                _proxy.Shutdown();
            } catch {
            }
        }

        /// <summary>
        /// Aborts all work in the remote process.  The current work item, if any,
        /// is aborted with a Thread.Abort.  All queued work items will be cleared
        /// and will not be executed.
        /// </summary>
        public void Abort() {
            try {
                _asyncAccess.Abort();
            } catch (RemotingException) {
            }
        }


        public ObjectHandle CommandDispatcher {
            get {
                return _asyncAccess.CommandDispatcher;
            }
            set {
                _asyncAccess.CommandDispatcher = value;
            }
        }
        /// <summary>
        /// Sets the TextWriter that is used for output in the remote process.
        /// </summary>
        public void SetConsoleOut(TextWriter writer) {
            _proxy.SetConsoleOut(writer);
        }

        /// <summary>
        /// Sets the TextReader that is used for input in the remote process.
        /// </summary>
        public void SetConsoleIn(TextReader reader) {
            _proxy.SetConsoleIn(reader);
        }

        /// <summary>
        /// Sets the TextWriter that is used for error output in the remote process.
        /// </summary>
        public void SetConsoleError(TextWriter writer) {
            _proxy.SetConsoleError(writer);
        }

        /// <summary>
        /// Disposes of the RemoteScriptFactory and closes the remote process.
        /// </summary>
        public void Dispose() {
            if (_proxy != null) {
                if (!_remoteRuntimeProcess.HasExited) {
                    try {
                        // shutting down can throw if the process successfully
                        // exits while we're in the Shutdown call
                        _proxy.Shutdown();
                    } catch (RemotingException) {
                    }
                }

                if (!_remoteRuntimeProcess.HasExited) {
                    TerminateProcess(_remoteRuntimeProcess.Handle, 1);
                }
            }
            GC.SuppressFinalize(this);
        }

        public bool IsDisconnected {
            get{
                try {
                    _proxy.Nop();
                } catch {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Internal Implementation Details

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int TerminateProcess(IntPtr processIdOrHandle, uint exitCode);

        private ProcessStartInfo GetProcessStartInfo(ApartmentState state) {
            const string exeName = "RemoteScriptFactory.exe";

            // The exe is located in the IronStudio install dir. The repl can't run though unless IronPython and IronRuby dlls are GAC'd.
            string processBasePath = Path.GetDirectoryName(typeof(RemoteScriptFactory).Assembly.Location); 

#if DEBUG
            // While developing the tooling use an exe located in Bin\Debug as we don't GAC any dlls.
            string devBinPath = Environment.GetEnvironmentVariable("DLR_ROOT");
            if (devBinPath != null) {
                devBinPath = Path.Combine(devBinPath, @"Bin\Debug");
                if (File.Exists(Path.Combine(devBinPath, exeName))) {
                    processBasePath = devBinPath;
                }
            }
#endif

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = Path.Combine(processBasePath, exeName);
            processInfo.CreateNoWindow = true;

            // Set UseShellExecute to false to enable redirection.
            processInfo.UseShellExecute = false;

            // Redirect the standard streams. The output streams will be read asynchronously using an event handler.
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.Arguments = state.ToString();

            return processInfo;
        }

        #endregion

        public void SetCurrentDirectory(string dir) {
            _proxy.SetCurrentDirectory(dir);
        }
    }
}
