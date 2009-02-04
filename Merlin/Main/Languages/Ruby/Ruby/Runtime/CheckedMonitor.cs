/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IronRuby.Runtime {
    /// <summary>
    /// Queryable recursive lock.
    /// </summary>
    internal sealed class CheckedMonitor {
        private int _locked;

        public void Enter() {
            Monitor.Enter(this);
            _locked++;
        }

        public void Exit() {
            _locked--;
            Monitor.Exit(this);
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

            public CheckedMonitorLocker(CheckedMonitor/*!*/ monitor) {
                _monitor = monitor;
                monitor.Enter();
            }

            public void Dispose() {
                _monitor.Exit();
            }
        }

        private struct CheckedMonitorUnlocker : IDisposable {
            private readonly CheckedMonitor/*!*/ _monitor;

            public CheckedMonitorUnlocker(CheckedMonitor/*!*/ monitor) {
                _monitor = monitor;
                monitor.Exit();
            }

            public void Dispose() {
                _monitor.Enter();
            }
        }
    }
}
