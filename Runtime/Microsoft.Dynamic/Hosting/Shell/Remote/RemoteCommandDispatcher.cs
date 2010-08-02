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
using System.Threading;
using System.Security.Permissions;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// This allows the RemoteConsoleHost to abort a long-running operation. The RemoteConsoleHost itself
    /// does not know which ThreadPool thread might be processing the remote call, and so it needs
    /// cooperation from the remote runtime server.
    /// </summary>
    public class RemoteCommandDispatcher : MarshalByRefObject, ICommandDispatcher {
        /// <summary>
        /// Since OnOutputDataReceived is sent async, it can arrive late. The remote console
        /// cannot know if all output from the current command has been received. So
        /// RemoteCommandDispatcher writes out a marker to indicate the end of the output
        /// </summary>
        internal const string OutputCompleteMarker = "{7FF032BB-DB03-4255-89DE-641CA195E5FA}";

        private ScriptScope _scriptScope;
        private Thread _executingThread;

        public RemoteCommandDispatcher(ScriptScope scope) {
            _scriptScope = scope;
        }

        public ScriptScope ScriptScope { get { return _scriptScope; } }

        public object Execute(CompiledCode compiledCode, ScriptScope scope) {
            Debug.Assert(_executingThread == null);
            _executingThread = Thread.CurrentThread;

            try {
                object result = compiledCode.Execute(scope);

                Console.WriteLine(RemoteCommandDispatcher.OutputCompleteMarker);

                return result;
            } catch (ThreadAbortException tae) {
                KeyboardInterruptException pki = tae.ExceptionState as KeyboardInterruptException;
                if (pki != null) {
                    // Most exceptions get propagated back to the client. However, ThreadAbortException is handled
                    // differently by the remoting infrastructure, and gets wrapped in a RemotingException
                    // ("An error occurred while processing the request on the server"). So we filter it out
                    // and raise the KeyboardInterruptException
                    Thread.ResetAbort();
                    throw pki;
                } else {
                    throw;
                }
            } finally {
                _executingThread = null;
            }
        }

        /// <summary>
        /// Aborts the current active call to Execute by doing Thread.Abort
        /// </summary>
        /// <returns>true if a Thread.Abort was actually called. false if there is no active call to Execute</returns>
        public bool AbortCommand() {
            Thread executingThread = _executingThread;
            if (executingThread == null) {
                return false;
            }

            executingThread.Abort(new KeyboardInterruptException(""));
            return true;
        }

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}

#endif