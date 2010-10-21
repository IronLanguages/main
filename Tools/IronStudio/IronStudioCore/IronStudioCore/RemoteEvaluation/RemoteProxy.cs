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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.IronStudio.RemoteEvaluation {
    class RemoteProxy : MarshalByRefObject {
        private AutoResetEvent _processEvent = new AutoResetEvent(false);
        private AutoResetEvent _shutdownEvent = new AutoResetEvent(false);
        private WaitHandle _processHandle;
        private AsyncAccess _abort;
        
        private static ExecutionQueue _queue;

        public RemoteProxy(ApartmentState state) {
            _abort = new AsyncAccess(this);
            _queue = new ExecutionQueue(state);
        }

        public ScriptRuntime CreateRuntime(ScriptRuntimeSetup setup) {
            return new ScriptRuntime(setup);
        }

        public void Shutdown() {
            _queue.Shutdown();
            _shutdownEvent.Set();
        }

        public ObjectHandle CommandDispatcher {
            get {
                return _queue.CommandDispatcher;
            }
            set {
                _queue.CommandDispatcher = value;
            }
        }

        /// <summary>
        /// Stops execution of the current work item in the queue and flushes the queue.
        /// </summary>
        internal void Abort() {
            _queue.Abort();
        }

        public void SetConsoleOut(TextWriter writer) {
            Console.SetOut(writer);
        }

        public void SetConsoleError(TextWriter writer) {
            Console.SetError(writer);
        }

        public void SetConsoleIn(TextReader reader) {
            Console.SetIn(reader);
        }

        public void SetParentProcess(int id) {
            var process = Process.GetProcessById(id);
            _processHandle = new ProcessWaitHandle(process.Handle);
            _processEvent.Set();
        }

        class ProcessWaitHandle : WaitHandle {
            public ProcessWaitHandle(IntPtr handle) {
                SafeWaitHandle = new SafeWaitHandle(handle, false);
            }
        }

        public override object InitializeLifetimeService() {
            return null;
        }

        public void Nop() {
        }

        #region Channel Creation

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")] // TODO: Microsoft.Scripting does not need to be APTCA
        internal static IpcChannel CreateChannel(string channelName, string portName, IServerFormatterSinkProvider sinkProvider) {
            // The Hosting API classes require TypeFilterLevel.Full to be remoted
            var serverProv = sinkProvider;
            System.Collections.IDictionary properties = new Hashtable();
            properties["name"] = channelName;
            properties["portName"] = portName;
            properties["includeVersions"] = false;
            // exclusiveAddressUse corresponds to the FILE_FLAG_FIRST_PIPE_INSTANCE flag of CreateNamedPipe. 
            // Setting it to true seems to cause "Failed to create an IPC Port: Access is denied." occasionally.
            // TODO: Setting this to false is secure only if we use ACLs as well.
            properties["exclusiveAddressUse"] = false;

            // Create the channel.  
            return new IpcChannel(properties, null, serverProv);
        }

        /// <summary>
        /// Publish objects so that the host can use it, and then block indefinitely (until the input stream is open).
        /// 
        /// Note that we should publish only one object, and then have other objects be accessible from it. Publishing
        /// multiple objects can cause problems if the client does a call like "remoteProxy1(remoteProxy2)" as remoting
        /// will not be able to know if the server object for both the proxies is on the same server.
        /// </summary>
        /// <param name="remoteRuntimeChannelName">The IPC channel that the remote console expects to use to communicate with the ScriptEngine</param>
        internal void StartServer() {
            string remoteRuntimeChannelName = Guid.NewGuid().ToString();

            Debug.Assert(ChannelServices.GetChannel(remoteRuntimeChannelName) == null);

            LifetimeServices.LeaseTime = GetSevenDays();
            LifetimeServices.LeaseManagerPollTime = GetSevenDays();
            LifetimeServices.RenewOnCallTime = GetSevenDays();
            LifetimeServices.SponsorshipTimeout = GetSevenDays();

            // create two channels, one for synchronous execution of user code, one for async requests to abort long-running user code.

            IpcChannel channel = RegisterIpcChannel("ipc", remoteRuntimeChannelName, new SinkProvider());
            var provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IpcChannel abortChannel = RegisterIpcChannel("ipcabort", remoteRuntimeChannelName + "ABORT", provider);
            
            var objRef = RemotingServices.Marshal(this, "RemoteProxy");
            var abortObjRef = RemotingServices.Marshal(_abort, "RemoteAbort");
            try {
                //RemoteCommandDispatcher remoteCommandDispatcher = new RemoteCommandDispatcher(scope);
                //RemotingServices.Marshal(remoteCommandDispatcher, CommandDispatcherUri);

                // Let the remote console know that the startup output (if any) is complete. We use this instead of
                // a named event as we want all the startup output to reach the remote console before it proceeds.
                foreach (var x in channel.GetUrlsForUri(objRef.URI)) {
                    Console.WriteLine("URI: {0}", x);
                }

                foreach (var x in abortChannel.GetUrlsForUri(abortObjRef.URI)) {
                    Console.WriteLine("ABORTURI: {0}", x);
                }

                // wait to get our parent process...
                _processEvent.WaitOne();

                // now wait for our parent to exit or for them to signal we should shutdown
                switch (WaitHandle.WaitAny(new[] { _shutdownEvent, _processHandle })) {
                    case 1:
                        // rip the process as quickly as possible when our parent unexpected dies.
                        Environment.FailFast("parent process died unexpectedly");
                        break;
                }
            } finally {
                ChannelServices.UnregisterChannel(channel);
            }
        }

        internal static IpcChannel RegisterIpcChannel(string name, string remoteRuntimeChannelName, IServerFormatterSinkProvider sinkProvider) {
            IpcChannel channel = CreateChannel(name, remoteRuntimeChannelName, sinkProvider);            

            ChannelServices.RegisterChannel(channel, false);
            return channel;
        }

        private static TimeSpan GetSevenDays() {
            return new TimeSpan(7, 0, 0, 0); // days,hours,mins,secs 
        }

        #endregion

        #region Remoting Sinks

        /// <summary>
        /// Provides a SinkProvider whos sink will delegate to the binary sink but will marshal
        /// all requests onto a well known thread.
        /// </summary>
        class SinkProvider : IServerFormatterSinkProvider, IServerChannelSinkProvider {
            private readonly BinaryServerFormatterSinkProvider _provider = new BinaryServerFormatterSinkProvider(
                new Dictionary<object, object> {
                    { "includeVersions", false }
                },
                null
            );

            public SinkProvider() {                
                _provider.TypeFilterLevel = TypeFilterLevel.Full;
            }

            #region IServerChannelSinkProvider Members

            public IServerChannelSink CreateSink(IChannelReceiver channel) {
                return new ServerChannelSink(_provider.CreateSink(channel));
            }

            public void GetChannelData(IChannelDataStore channelData) {
                _provider.GetChannelData(channelData);
            }

            public IServerChannelSinkProvider Next {
                get {
                    return _provider.Next;
                }
                set {
                    _provider.Next = value;
                }
            }

            #endregion
        }

        class ServerChannelSink : IServerChannelSink {
            private IServerChannelSink _iServerChannelSink;

            public ServerChannelSink(IServerChannelSink iServerChannelSink) {
                _iServerChannelSink = iServerChannelSink;
            }

            #region IServerChannelSink Members

            public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, System.IO.Stream stream) {
                _iServerChannelSink.AsyncProcessResponse(sinkStack, state, msg, headers, stream);
            }

            public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers) {
                return GetResponseStream(sinkStack, state, msg, headers);
            }

            public IServerChannelSink NextChannelSink {
                get { return _iServerChannelSink.NextChannelSink; }
            }

            public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream) {
                // marshal the request onto our dedicated thread for processing requests
                var msgInfo = new MessageInfo(_iServerChannelSink, sinkStack, requestMsg, requestHeaders, requestStream);
                _queue.Process(msgInfo);

                responseMsg = msgInfo.ResponseMsg;
                responseHeaders = msgInfo.ResponseHeaders;
                responseStream = msgInfo.ResponseStream;

                return msgInfo.ServerProcessing;
            }

            #endregion

            #region IChannelSinkBase Members

            public System.Collections.IDictionary Properties {
                get { return _iServerChannelSink.Properties; }
            }

            #endregion
        }

        class MessageInfo : ExecutionQueueItem {
            public readonly IServerChannelSink _serverSink;
            public readonly IServerChannelSinkStack _sinkStack;
            public readonly IMessage _requestMsg;
            public readonly ITransportHeaders _requestHeaders;
            public readonly Stream _requestStream;
            
            public IMessage ResponseMsg;
            public ITransportHeaders ResponseHeaders;
            public Stream ResponseStream;
            public ServerProcessing ServerProcessing;

            public MessageInfo(IServerChannelSink serverSink, IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream stream) {
                _serverSink = serverSink;
                _sinkStack = sinkStack;
                _requestMsg = requestMsg;
                _requestHeaders = requestHeaders;
                _requestStream = stream;
            }

            public override void Process() {
                ServerProcessing = _serverSink.ProcessMessage(_sinkStack, _requestMsg, _requestHeaders, _requestStream, out ResponseMsg, out ResponseHeaders, out ResponseStream);
            }
        }

        #endregion

        public void SetCurrentDirectory(string dir) {
            Environment.CurrentDirectory = dir;
        }
    }
}
