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
using System.Threading;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// Customize the CommandLine for remote scenarios
    /// </summary>
    public class RemoteConsoleCommandLine : CommandLine {
        private RemoteConsoleCommandDispatcher _remoteConsoleCommandDispatcher;

        public RemoteConsoleCommandLine(ScriptScope scope, RemoteCommandDispatcher remoteCommandDispatcher, AutoResetEvent remoteOutputReceived) {
            _remoteConsoleCommandDispatcher = new RemoteConsoleCommandDispatcher(remoteCommandDispatcher, remoteOutputReceived);
            Debug.Assert(scope != null);
            ScriptScope = scope;
        }

        protected override ICommandDispatcher CreateCommandDispatcher() {
            return _remoteConsoleCommandDispatcher;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void UnhandledExceptionWorker(Exception e) {
            try {
                base.UnhandledException(e);
            } catch (Exception exceptionDuringHandling) {
                // All bets are off while accessing the remote runtime. So we catch all exceptions.
                // However, in most cases, we only expect to see RemotingException here.
                if (!(exceptionDuringHandling is RemotingException)) {
                    Console.WriteLine(String.Format("({0} thrown while trying to display unhandled exception)", exceptionDuringHandling.GetType()), Style.Error);
                }

                // The remote server may have shutdown. So just do something simple
                Console.WriteLine(e.ToString(), Style.Error);
            }
        }

        protected override void UnhandledException(Exception e) {
            UnhandledExceptionWorker(e);
        }

        /// <summary>
        /// CommandDispatcher to ensure synchronize output from the remote runtime
        /// </summary>
        class RemoteConsoleCommandDispatcher : ICommandDispatcher {
            private RemoteCommandDispatcher _remoteCommandDispatcher;
            private AutoResetEvent _remoteOutputReceived;

            internal RemoteConsoleCommandDispatcher(RemoteCommandDispatcher remoteCommandDispatcher, AutoResetEvent remoteOutputReceived) {
                _remoteCommandDispatcher = remoteCommandDispatcher;
                _remoteOutputReceived = remoteOutputReceived;
            }

            public object Execute(CompiledCode compiledCode, ScriptScope scope) {
                // Delegate the operation to the RemoteCommandDispatcher which will execute the code in the remote runtime
                object result = _remoteCommandDispatcher.Execute(compiledCode, scope);

                // Output is received async, and so we need explicit synchronization in the remote console
                _remoteOutputReceived.WaitOne();

                return result;
            }
        }
    }
}

#endif