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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// Supports detecting the remote runtime being killed, and starting up a new one.
    /// 
    /// Threading model:
    /// 
    /// ConsoleRestartManager creates a separate thread on which to create and execute the consoles. 
    /// There are usually atleast three threads involved:
    /// 
    /// 1. Main app thread: Instantiates ConsoleRestartManager and accesses its APIs. This thread has to stay 
    ///    responsive to user input and so the ConsoleRestartManager APIs cannot be long-running or blocking.
    ///    Since the remote runtime process can terminate asynchronously, the current RemoteConsoleHost can 
    ///    change at any time (if auto-restart is enabled). The app should typically not care which instance of 
    ///    RemoteConsoleHost is currently being used. The flowchart of this thread is:
    ///        Create ConsoleRestartManager
    ///        ConsoleRestartManager.Start
    ///        Loop:
    ///            Respond to user input | Send user input to console for execution | BreakExecution | RestartConsole | GetMemberNames
    ///        ConsoleRestartManager.Terminate
    ///    TODO: Currently, BreakExecution and GetMemberNames are called by the main thread synchronously.
    ///    Since they execute code in the remote runtime, they could take arbitrarily long. We should change
    ///    this so that the main app thread can never be blocked indefinitely.
    ///
    /// 2. Console thread: Dedicated thread for creating RemoteConsoleHosts and executing code (which could
    ///    take a long time or block indefinitely).
    ///        Wait for ConsoleRestartManager.Start to be called
    ///        Loop:
    ///            Create RemoteConsoleHost
    ///            Wait for signal for:
    ///                 Execute code | RestartConsole | Process.Exited
    ///
    /// 3. CompletionPort async callbacks:
    ///        Process.Exited | Process.OutputDataReceived | Process.ErrorDataReceived
    /// 
    /// 4. Finalizer thred
    ///    Some objects may have a Finalize method (which possibly calls Dispose). Not many (if any) types
    ///    should have a Finalize method.
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")] // TODO: This is public only because the test (RemoteConsole.py) needs it to be so. The test should be rewritten
    public abstract class ConsoleRestartManager {
        private RemoteConsoleHost _remoteConsoleHost;
        private Thread _consoleThread;
        private bool _exitOnNormalExit;
        private bool _terminating;

        /// <summary>
        /// Accessing _remoteConsoleHost from a thread other than console thread can result in race.
        /// If _remoteConsoleHost is accessed while holding _accessLock, it is guaranteed to be
        /// null or non-disposed.
        /// </summary>
        private object _accessLock = new object();

        /// <summary>
        /// This is created on the "creating thread", and goes on standby. Start needs to be called for activation.
        /// </summary>
        /// <param name="exitOnNormalExit">A host might want one of two behaviors:
        /// 1. Keep the REPL loop alive indefinitely, even when a specific instance of the RemoteConsoleHost terminates normally
        /// 2. Close the REPL loop when an instance of the RemoteConsoleHost terminates normally, and restart the loop
        ///    only if the instance terminates abnormally.</param>
        public ConsoleRestartManager(bool exitOnNormalExit) {
            _exitOnNormalExit = exitOnNormalExit;
            _consoleThread = new Thread(Run);
            _consoleThread.Name = "Console thread";
        }

        protected object AccessLock { get { return _accessLock; } }

        public Thread ConsoleThread { get { return _consoleThread; } }

        protected RemoteConsoleHost CurrentConsoleHost { get { return _remoteConsoleHost; } }

        public abstract RemoteConsoleHost CreateRemoteConsoleHost();

        /// <summary>
        /// Needs to be called for activation.
        /// </summary>
        public void Start() {
            Debug.Assert(Thread.CurrentThread != _consoleThread);

            if (_consoleThread.IsAlive) {
                throw new InvalidOperationException("Console thread is already running.");
            }
            _consoleThread.Start();
        }

        private void Run() {
#if DEBUG
            try {
                RunWorker();
            } catch (Exception e) {
                Debug.Assert(false, "Unhandled exception on console thread:\n\n" + e.ToString());
            }
#else
            RunWorker();
#endif
        }

        private void RunWorker() {
            Debug.Assert(Thread.CurrentThread == _consoleThread);

            while (true) {
                RemoteConsoleHost remoteConsoleHost = CreateRemoteConsoleHost();

                // Reading _terminating and setting of _remoteConsoleHost should be done atomically. 
                // Terminate() does the reverse operation (setting _terminating reading _remoteConsoleHost) atomically
                lock (_accessLock) {
                    if (_terminating) {
                        return;
                    }

                    _remoteConsoleHost = remoteConsoleHost;
                }

                try {
                    try {
                        int exitCode = remoteConsoleHost.Run(new string[0]);

                        if (_exitOnNormalExit && exitCode == 0) {
                            return;
                        }
                    } catch (RemoteRuntimeStartupException) {
                    }
                } finally {
                    lock (_accessLock) {
                        remoteConsoleHost.Dispose();
                        _remoteConsoleHost = null;
                    }
                }
            }
        }

        // TODO: We have to catch all exceptions as we are executing user code in the remote runtime, and we cannot control what 
        // exception it may throw. This could be fixed if we built our own remoting channel which returned an error code
        // instead of propagating exceptions back from the remote runtime.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public IList<string> GetMemberNames(string expression) {
            Debug.Assert(Thread.CurrentThread != _consoleThread);

            lock (_accessLock) {
                if (_remoteConsoleHost == null) {
                    return null;
                }

                ScriptEngine engine = _remoteConsoleHost.Engine;
                try {
                    ScriptScope scope = _remoteConsoleHost.ScriptScope;
                    ObjectOperations operations = engine.CreateOperations(scope);
                    ScriptSource src = engine.CreateScriptSourceFromString(expression, SourceCodeKind.Expression);
                    return operations.GetMemberNames(src.ExecuteAndWrap(scope));
                } catch {
                    return null;
                }
            }
        }

        public void BreakExecution() {
            Debug.Assert(Thread.CurrentThread != _consoleThread);

            lock (_accessLock) {
                if (_remoteConsoleHost == null) {
                    return;
                }

                try {
                    _remoteConsoleHost.AbortCommand();
                } catch (System.Runtime.Remoting.RemotingException) {
                    // The remote runtime may be terminated or non-responsive
                }
            }
        }

        public void RestartConsole() {
            Debug.Assert(Thread.CurrentThread != _consoleThread);

            lock (_accessLock) {
                if (_remoteConsoleHost == null) {
                    return;
                }

                _remoteConsoleHost.Terminate(0);
            }
        }

        /// <summary>
        /// Request (from another thread) the console REPL loop to terminate
        /// </summary>
        public void Terminate() {
            Debug.Assert(Thread.CurrentThread != _consoleThread);

            lock (_accessLock) {
                _terminating = true;
                _remoteConsoleHost.Terminate(0);
            }
            
            _consoleThread.Join();
        }
    }
}

#endif