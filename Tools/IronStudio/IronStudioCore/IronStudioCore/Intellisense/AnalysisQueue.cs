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
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.IronStudio.Intellisense;

namespace Microsoft.IronStudio.Library.Intellisense {
    /// <summary>
    /// Provides a single threaded analysis queue.  Items can be enqueued into the
    /// analysis at various priorities.  
    /// </summary>
    public sealed class AnalysisQueue<TAnalyzedType> : IDisposable where TAnalyzedType : class {
        private readonly Thread _thread;
        private readonly AutoResetEvent _event;
        private readonly IAnalyzer<TAnalyzedType> _analyzer;
        private readonly object _queueLock = new object();
        private readonly List<TAnalyzedType>[] _queue;
        private volatile bool _unload;
        private bool _isAnalyzing;

        private const int PriorityCount = (int)AnalysisPriority.High + 1;

        public AnalysisQueue(IAnalyzer<TAnalyzedType> analyzer) {
            _event = new AutoResetEvent(false);
            _analyzer = analyzer;

            _queue = new List<TAnalyzedType>[PriorityCount];
            for(int i = 0; i<PriorityCount; i++) {
                _queue[i] = new List<TAnalyzedType>();
            }

            _thread = new Thread(Worker);
            _thread.Name = String.Format("Analysis Queue of {0}", typeof(TAnalyzedType).Name);
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Enqueue(TAnalyzedType item, AnalysisPriority priority) {
            int iPri = (int)priority;

            if (iPri < 0 || iPri > _queue.Length) {
                throw new ArgumentException("priority");
            }

            lock (_queueLock) {
                // see if we have the item in the queue anywhere...
                for (int i = 0; i < _queue.Length; i++) {
                    if (_queue[i].Remove(item)) {
                        AnalysisPriority oldPri = (AnalysisPriority)i;

                        if (oldPri > priority) {
                            // if it was at a higher priority then our current
                            // priority go ahead and raise the new entry to our
                            // old priority
                            priority = oldPri;
                        }

                        break;
                    }
                }

                // enqueue the work item
                if (priority == AnalysisPriority.High) {
                    // always try and process high pri items immediately
                    _queue[iPri].Insert(0, item);
                } else {
                    _queue[iPri].Add(item);
                }
                _event.Set();
            }            
        }

        public void Stop() {
            if (_thread != null) {
                _unload = true;
                _event.Set();
            }
        }

        public bool IsAnalyzing {
            get {
                return _isAnalyzing;
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose() {
            Stop(); 	        
        }

        #endregion

        private TAnalyzedType GetNextItem() {
            for (int i = PriorityCount - 1; i >= 0; i--) {
                if (_queue[i].Count > 0) {
                    var res = _queue[i][0];
                    _queue[i].RemoveAt(0);
                    return res;
                }
            }
            return null;
        }

        private void Worker() {
            while (!_unload) {
                TAnalyzedType workItem;

                lock (_queueLock) {
                    workItem = GetNextItem();
                }
                _isAnalyzing = true;
                if (workItem != null) {
                    _analyzer.Analyze(workItem);
                    _isAnalyzing = false;
                } else {
                    _event.WaitOne();
                }
            }
        }
    }
}
