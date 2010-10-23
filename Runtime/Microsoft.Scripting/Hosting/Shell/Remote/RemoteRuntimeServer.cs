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
using System.Runtime.Remoting.Lifetime;

namespace Microsoft.Scripting.Hosting.Shell.Remote {
    /// <summary>
    /// The remote runtime server uses this class to publish an initialized ScriptEngine and ScriptRuntime 
    /// over a remoting channel.
    /// </summary>
    public static class RemoteRuntimeServer {
        internal const string CommandDispatcherUri = "CommandDispatcherUri";
        internal const string RemoteRuntimeArg = "-X:RemoteRuntimeChannel";

        private static TimeSpan GetSevenDays() {
            return new TimeSpan(7, 0, 0, 0); // days,hours,mins,secs 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")] // TODO: Microsoft.Scripting does not need to be APTCA
        internal static IpcChannel CreateChannel(string channelName, string portName) {
            // The Hosting API classes require TypeFilterLevel.Full to be remoted
            BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            System.Collections.IDictionary properties = new System.Collections.Hashtable();
            properties["name"] = channelName;
            properties["portName"] = portName;
            // exclusiveAddressUse corresponds to the FILE_FLAG_FIRST_PIPE_INSTANCE flag of CreateNamedPipe. 
            // Setting it to true seems to cause "Failed to create an IPC Port: Access is denied." occasionally.
            // TODO: Setting this to false is secure only if we use ACLs as well.
            properties["exclusiveAddressUse"] = false;

            // Create the channel.  
            IpcChannel channel = new IpcChannel(properties, null, serverProv);
            return channel;
        }

        /// <summary>
        /// Publish objects so that the host can use it, and then block indefinitely (until the input stream is open).
        /// 
        /// Note that we should publish only one object, and then have other objects be accessible from it. Publishing
        /// multiple objects can cause problems if the client does a call like "remoteProxy1(remoteProxy2)" as remoting
        /// will not be able to know if the server object for both the proxies is on the same server.
        /// </summary>
        /// <param name="remoteRuntimeChannelName">The IPC channel that the remote console expects to use to communicate with the ScriptEngine</param>
        /// <param name="scope">A intialized ScriptScope that is ready to start processing script commands</param>
        internal static void StartServer(string remoteRuntimeChannelName, ScriptScope scope) {
            Debug.Assert(ChannelServices.GetChannel(remoteRuntimeChannelName) == null);

            IpcChannel channel = CreateChannel("ipc", remoteRuntimeChannelName);

            LifetimeServices.LeaseTime = GetSevenDays();
            LifetimeServices.LeaseManagerPollTime = GetSevenDays();
            LifetimeServices.RenewOnCallTime = GetSevenDays();
            LifetimeServices.SponsorshipTimeout = GetSevenDays();

            ChannelServices.RegisterChannel(channel, false);

            try {
                RemoteCommandDispatcher remoteCommandDispatcher = new RemoteCommandDispatcher(scope);
                RemotingServices.Marshal(remoteCommandDispatcher, CommandDispatcherUri);

                // Let the remote console know that the startup output (if any) is complete. We use this instead of
                // a named event as we want all the startup output to reach the remote console before it proceeds.
                Console.WriteLine(RemoteCommandDispatcher.OutputCompleteMarker);

                // Block on Console.In. This is used to determine when the host process exits, since ReadLine will return null then.
                string input = System.Console.ReadLine();
                Debug.Assert(input == null);
            } finally {
                ChannelServices.UnregisterChannel(channel);
            }
        }
    }
}

#endif