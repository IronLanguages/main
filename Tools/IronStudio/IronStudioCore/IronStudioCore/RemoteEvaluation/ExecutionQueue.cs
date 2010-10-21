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
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Threading;

namespace Microsoft.IronStudio.RemoteEvaluation {
    class ExecutionQueue {
        private readonly Thread _executionThread;
        private bool _shouldShutDown;
        private AutoResetEvent _event = new AutoResetEvent(false);
        private List<ExecutionQueueItem> _items = new List<ExecutionQueueItem>();
        private bool _running, _aborting;
        private Action<Action> _commandDispatcher;

        public ExecutionQueue(ApartmentState state) {
            _executionThread = new Thread(ExecutionThread);
            _executionThread.IsBackground = true;
            _executionThread.Name = "ExecutionThread";
            _executionThread.SetApartmentState(state);
            _executionThread.Start();
            _commandDispatcher = (action) => action();
        }

        private void ExecutionThread() {
            while (!_shouldShutDown) {
                ExecutionQueueItem curItem = null;
                lock (_items) {
                    if (_items.Count > 0) {
                        curItem = _items[0];
                        _items.RemoveAt(0);
                    }
                }

                if (curItem != null) {
                    try {
                        _running = true;
                        try {
                            _commandDispatcher(() => curItem.Process());
                        } finally {
                            _running = false;
                        }

                        while (_aborting) {
                            // wait for abort thread to complete
                            ;
                        }
                    } catch (ThreadAbortException) {
                        Thread.ResetAbort();
                    }
                    curItem.Complete();
                } else {
                    _event.WaitOne();
                }
            }
        }

        public void Process(ExecutionQueueItem item) {
            if (Running) {
                // Remote side called us, we called back, and now they're calling back again.  Process this request synchronously so we don't hang
                _commandDispatcher(() => {
                    item.Process();
                });
            } else {
                Enqueue(item);
                item.Wait();
            }
        }

        public ObjectHandle CommandDispatcher {
            get {
                return new ObjectHandle(_commandDispatcher);
            }
            set {
                _commandDispatcher = (Action<Action>)value.Unwrap();
            }
        }

        public bool Running {
            get {
                return _running;
            }
        }

        public void Shutdown() {
            lock (_items) {
                _items.Clear();
            }

            _shouldShutDown = true;
            _event.Set();
        }

        public void Abort() {
            lock (_items) {
                _items.Clear();
            }

            _aborting = true;
            if (_running) {
                _executionThread.Abort();
            }
            _aborting = false;
        }

        public void Enqueue(ExecutionQueueItem item) {
            lock (_items) {
                _items.Add(item);
                _event.Set();
            }
        }
    }
}
