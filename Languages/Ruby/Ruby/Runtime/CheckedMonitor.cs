/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// TODO: use ReaderWriterLockSlim on CLR4?
    /// Queryable recursive lock.
    /// </summary>
    internal sealed class CheckedMonitor {
        private int _locked;

        internal void Enter(ref bool lockTaken) {
            try {
                MonitorUtils.Enter(this, ref lockTaken);
            } finally {
                if (lockTaken) {
                        _locked++;
                }
            }
        }

        internal void Exit(ref bool lockTaken) {
            try {
                MonitorUtils.Exit(this, ref lockTaken);
            } finally {
                if (!lockTaken) {
                    _locked--;
                }
            }
        }

        public bool IsLocked {
            get { return _locked > 0; }
        }

        public IDisposable/*!*/ CreateLocker() {
            return new CheckedMonitorLocker(this);
        }

        public IDisposable/*!*/ CreateUnlocker() {
            return new CheckedMonitorUnlocker(this);
        }

        private struct CheckedMonitorLocker : IDisposable {
            private readonly CheckedMonitor/*!*/ _monitor;
            private bool _lockTaken;

            public CheckedMonitorLocker(CheckedMonitor/*!*/ monitor) {
                _monitor = monitor;
                _lockTaken = false;
                monitor.Enter(ref _lockTaken);
            }

            public void Dispose() {
                _monitor.Exit(ref _lockTaken);
            }
        }

        private struct CheckedMonitorUnlocker : IDisposable {
            private readonly CheckedMonitor/*!*/ _monitor;
            private bool _lockTaken;

            public CheckedMonitorUnlocker(CheckedMonitor/*!*/ monitor) {
                _monitor = monitor;
                _lockTaken = true;
                monitor.Exit(ref _lockTaken);
            }

            public void Dispose() {
                _monitor.Enter(ref _lockTaken);
            }
        }
    }
}
