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

#if !SILVERLIGHT // Remoting

using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// ConsoleHost where the ScriptRuntime is hosted in a separate process (referred to as the remote runtime server)
    /// 
    /// The RemoteConsoleHost spawns the remote runtime server and specifies an IPC channel name to use to communicate
    /// with each other. The remote runtime server creates and initializes a ScriptRuntime and a ScriptEngine, and publishes
    /// it over the specified IPC channel at a well-known URI. Note that the RemoteConsoleHost cannot easily participate
    /// in the initialization of the ScriptEngine as classes like LanguageContext are not remotable.
    /// 
    /// The RemoteConsoleHost then starts the interactive loop and executes commands on the ScriptEngine over the remoting channel.
    /// The RemoteConsoleHost listens to stdout of the remote runtime server and echos it locally to the user.
    /// </summary>
    public abstract class RemoteConsoleHost : ConsoleHost, IDisposable {
        Process _remoteRuntimeProcess;
        internal RemoteCommandDispatcher _remoteCommandDispatcher;
        private string _channelName = RemoteConsoleHost.GetChannelName();
        private IpcChannel _clientChannel;
        private AutoResetEvent _remoteOutputReceived = new AutoResetEvent(false);
        private ScriptScope _scriptScope;

        #region Private methods

        private static string GetChannelName() {
            return "RemoteRuntime-" + Guid.NewGuid().ToString();
        }

        private ProcessStartInfo GetProcessStartInfo() {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Arguments = RemoteRuntimeServer.RemoteRuntimeArg + " " + _channelName;
            processInfo.CreateNoWindow = true;

            // Set UseShellExecute to false to enable redirection.
            processInfo.UseShellExecute = false;

            // Redirect the standard streams. The output streams will be read asynchronously using an event handler.
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            // The input stream can be ignored as the remote server process does not need to read any input
            processInfo.RedirectStandardInput = true;

            CustomizeRemoteRuntimeStartInfo(processInfo);
            Debug.Assert(processInfo.FileName != null);
            return processInfo;
        }

        private void StartRemoteRuntimeProcess() {
            Process process = new Process();

            process.StartInfo = GetProcessStartInfo();

            process.OutputDataReceived += new DataReceivedEventHandler(OnOutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(OnErrorDataReceived);

            process.Exited += new EventHandler(OnRemoteRuntimeExited);
            _remoteRuntimeProcess = process;

            process.Start();

            // Start the asynchronous read of the output streams.
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // wire up exited 
            process.EnableRaisingEvents = true;

            // Wait for the output marker to know when the startup output is complete
            _remoteOutputReceived.WaitOne();

            if (process.HasExited) {
                throw new RemoteRuntimeStartupException("Remote runtime terminated during startup with exitcode " + process.ExitCode);
            }
        }

        private T GetRemoteObject<T>(string uri) {
            T result = (T)Activator.GetObject(typeof(T), "ipc://" + _channelName + "/" + uri);

            // Ensure that the remote object is responsive by calling a virtual method (which will be executed remotely)
            Debug.Assert(result.ToString() != null);

            return result;
        }

        private void InitializeRemoteScriptEngine() {
            StartRemoteRuntimeProcess();

            _remoteCommandDispatcher = GetRemoteObject<RemoteCommandDispatcher>(RemoteRuntimeServer.CommandDispatcherUri);

            _scriptScope = _remoteCommandDispatcher.ScriptScope;
            Engine = _scriptScope.Engine;

            // Register a channel for the reverse direction, when the remote runtime process wants to fire events
            // or throw an exception
            string clientChannelName = _channelName.Replace("RemoteRuntime", "RemoteConsole");
            _clientChannel = RemoteRuntimeServer.CreateChannel(clientChannelName, clientChannelName);
            ChannelServices.RegisterChannel(_clientChannel, false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected virtual void OnRemoteRuntimeExited(object sender, EventArgs args) {
            Debug.Assert(((Process)sender).HasExited);
            Debug.Assert(sender == _remoteRuntimeProcess || _remoteRuntimeProcess == null);

            EventHandler remoteRuntimeExited = RemoteRuntimeExited;
            if (remoteRuntimeExited != null) {
                remoteRuntimeExited(sender, args);
            }

            // StartRemoteRuntimeProcess also blocks on this event. Signal it in case the 
            // remote runtime terminates during startup itself.
            _remoteOutputReceived.Set();

            // Nudge the ConsoleHost to exit the REPL loop
            Terminate(_remoteRuntimeProcess.ExitCode);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] // TODO: This is protected only for test code, which could be rewritten to not require this to be protected
        protected virtual void OnOutputDataReceived(object sender, DataReceivedEventArgs eventArgs) {
            if (String.IsNullOrEmpty(eventArgs.Data)) {
                return;
            }

            string output = eventArgs.Data as string;

            if (output.Contains(RemoteCommandDispatcher.OutputCompleteMarker)) {
                Debug.Assert(output == RemoteCommandDispatcher.OutputCompleteMarker);
                _remoteOutputReceived.Set();
            } else {
                ConsoleIO.WriteLine(output, Style.Out);
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs eventArgs) {
            if (!String.IsNullOrEmpty(eventArgs.Data)) {
                ConsoleIO.WriteLine((string)eventArgs.Data, Style.Error);
            }
        }

        #endregion

        public override void Terminate(int exitCode) {
            if (CommandLine == null) {
                // Terminate may be called during startup when CommandLine has not been initialized.
                // We could fix this by initializing CommandLine before starting the remote runtime process
                return;
            }

            base.Terminate(exitCode);
        }

        protected override CommandLine CreateCommandLine() {
            return new RemoteConsoleCommandLine(_scriptScope, _remoteCommandDispatcher, _remoteOutputReceived);
        }

        public ScriptScope ScriptScope { get { return CommandLine.ScriptScope; } }
        public Process RemoteRuntimeProcess { get { return _remoteRuntimeProcess; } }

        // TODO: We have to catch all exceptions as we are executing user code in the remote runtime, and we cannot control what 
        // exception it may throw. This could be fixed if we built our own remoting channel which returned an error code
        // instead of propagating exceptions back from the remote runtime.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void UnhandledException(ScriptEngine engine, Exception e) {
            ((RemoteConsoleCommandLine)CommandLine).UnhandledExceptionWorker(e);
        }
        /// <summary>
        /// Called if the remote runtime process exits by itself. ie. without the remote console killing it.
        /// </summary>
        internal event EventHandler RemoteRuntimeExited;

        /// <summary>
        /// Allows the console to customize the environment variables, working directory, etc.
        /// </summary>
        /// <param name="processInfo">At the least, processInfo.FileName should be initialized</param>
        public abstract void CustomizeRemoteRuntimeStartInfo(ProcessStartInfo processInfo);

        /// <summary>
        /// Aborts the current active call to Execute by doing Thread.Abort
        /// </summary>
        /// <returns>true if a Thread.Abort was actually called. false if there is no active call to Execute</returns>
        public bool AbortCommand() {
            return _remoteCommandDispatcher.AbortCommand();
        }

        public override int Run(string[] args) {
            var runtimeSetup = CreateRuntimeSetup();
            var options = new ConsoleHostOptions();
            ConsoleHostOptionsParser = new ConsoleHostOptionsParser(options, runtimeSetup);

            try {
                ParseHostOptions(args);
            } catch (InvalidOptionException e) {
                Console.Error.WriteLine("Invalid argument: " + e.Message);
                return ExitCode = 1;
            }

            // Create IConsole early (with default settings) in order to be able to display startup output
            ConsoleIO = CreateConsole(null, null, new ConsoleOptions());

            InitializeRemoteScriptEngine();
            Runtime = Engine.Runtime;

            ExecuteInternal();

            return ExitCode;
        }

        #region IDisposable Members

        public virtual void Dispose(bool disposing) {
            if (!disposing) {
                // Managed fields cannot be reliably accessed during finalization since they may already have been finalized
                return;
            }

            _remoteOutputReceived.Close();

            if (_clientChannel != null) {
                ChannelServices.UnregisterChannel(_clientChannel);
                _clientChannel = null;
            }

            if (_remoteRuntimeProcess != null) {
                _remoteRuntimeProcess.Exited -= OnRemoteRuntimeExited;

                // Closing stdin is a signal to the remote runtime to exit the process.
                _remoteRuntimeProcess.StandardInput.Close();
                _remoteRuntimeProcess.WaitForExit(5000);

                if (!_remoteRuntimeProcess.HasExited) {
                    _remoteRuntimeProcess.Kill();
                    _remoteRuntimeProcess.WaitForExit();
                }

                _remoteRuntimeProcess = null;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    [Serializable]
    public class RemoteRuntimeStartupException : Exception {
        public RemoteRuntimeStartupException() { }

        public RemoteRuntimeStartupException(string message)
            : base(message) {
        }

        public RemoteRuntimeStartupException(string message, Exception innerException)
            : base(message, innerException) {
        }

        protected RemoteRuntimeStartupException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}

#endif