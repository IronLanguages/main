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
using System.Threading;
using Microsoft.IronStudio.Core;
using Microsoft.IronStudio.Library;
using Microsoft.Scripting;
using Microsoft.Scripting.Library;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.IronStudio.Intellisense {
    /// <summary>
    /// Provides an asynchronous queue for parsing source code.  Multiple items
    /// may be parsed simultaneously.  Text buffers are monitored for changes and
    /// the parser is called when the buffer should be re-parsed.
    /// </summary>
    public class ParseQueue {
        private readonly IParser _parser;
        private int _analysisPending;

        /// <summary>
        /// Creates a new parse queue which will parse using the provided parser.
        /// </summary>
        /// <param name="parser"></param>
        public ParseQueue(IParser parser) {
            _parser = parser;
        }

        /// <summary>
        /// Parses the specified text buffer.  Continues to monitor the parsed buffer and updates
        /// the parse tree asynchronously as the buffer changes.
        /// </summary>
        /// <param name="buffer"></param>
        public void EnqueueBuffer(ITextView textView) {
            ITextBuffer buffer = textView.TextBuffer;
            var curSnapshot = buffer.CurrentSnapshot;
            EnqueWorker(new[] { GetContentProvider(buffer, curSnapshot) });

            // only attach one parser to each buffer, we can get multiple enqueue's
            // for example if a document is already open when loading a project.
            if (!buffer.Properties.ContainsProperty(typeof(BufferParser))) {
                BufferParser parser = new BufferParser(this, buffer);
                buffer.ChangedLowPriority += parser.BufferChangedLowPriority;
                
                textView.Closed += (sender, args) => {
                    buffer.ChangedLowPriority -= parser.BufferChangedLowPriority;
                    buffer.Properties.RemoveProperty(typeof(BufferParser));
                };

                buffer.Properties.AddProperty(typeof(BufferParser), parser);
            }
        }

        /// <summary>
        /// Parses the specified file on disk.
        /// </summary>
        /// <param name="filename"></param>
        public void EnqueueFile(string filename) {
            EnqueWorker(new[] { new FileTextContentProvider(filename) });
        }

        /// <summary>
        /// Parses the specified list of files on disk.
        /// </summary>
        /// <param name="filenames"></param>
        public void EnqueueFiles(params string[] filenames) {
            EnqueueFiles((IEnumerable<string>)filenames);
        }

        /// <summary>
        /// Parses the specified list of files on disk.
        /// </summary>
        public void EnqueueFiles(IEnumerable<string> filenames) {
            List<TextContentProvider> providers = new List<TextContentProvider>();
            foreach (var filename in filenames) {
                providers.Add(new FileTextContentProvider(filename));
            }
            EnqueWorker(providers);
        }

        class BufferParser {
            private readonly ParseQueue _parseQueue;
            private readonly Timer _timer;
            private readonly ITextBuffer _buffer;
            private ITextSnapshot _snapshot;

            private bool _parsing, _requeue, _textChange;
            
            private const int ReparseDelay = 1000;      // delay in MS before we re-parse a buffer w/ non-line changes.

            public BufferParser(ParseQueue queue, ITextBuffer buffer) {
                _parseQueue = queue;
                _timer = new Timer(ReparseWorker, null, Timeout.Infinite, Timeout.Infinite);
                _buffer = buffer;
            }

            internal void ReparseWorker(object unused) {
                ITextSnapshot curSnapshot;
                lock (this) {
                    if (_parsing) {
                        return;
                    }

                    _parsing = true;
                    curSnapshot = _snapshot;
                }

                _parseQueue.DoParse(new[] { GetContentProvider(_buffer, _snapshot) });

                lock (this) {
                    _parsing = false;
                    if (_requeue) {
                        ThreadPool.QueueUserWorkItem(ReparseWorker);
                    }
                    _requeue = false;
                }
            }

            internal void BufferChangedLowPriority(object sender, TextContentChangedEventArgs e) {
                lock (this) {
                    // only immediately re-parse on line changes after we've seen a text change.                   
                    _snapshot = e.After;
                    
                    if (_parsing) {
                        // we are currently parsing, just reque when we complete
                        _requeue = true;
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    } else if (LineAndTextChanges(e)) {
                        // user pressed enter, we should reque immediately
                        ThreadPool.QueueUserWorkItem(ReparseWorker);
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    } else {
                        // parse if the user doesn't do anything for a while.
                        _textChange = IncludesTextChanges(e);
                        _timer.Change(ReparseDelay, Timeout.Infinite);
                    }
                }
            }

            /// <summary>
            /// Used to track if we have line + text changes, just text changes, or just line changes.
            /// 
            /// If we have text changes followed by a line change we want to immediately reparse.
            /// If we have just text changes we want to reparse in ReparseDelay ms from the last change.
            /// If we have just repeated line changes (e.g. someone's holding down enter) we don't want to
            ///     repeatedly reparse, instead we want to wait ReparseDelay ms.
            /// </summary>
            private bool LineAndTextChanges(TextContentChangedEventArgs e) {
                if (_textChange) {
                    _textChange = false;
                    return e.Changes.IncludesLineChanges;
                }

                bool mixedChanges = false;
                if (e.Changes.IncludesLineChanges) {
                    mixedChanges = IncludesTextChanges(e);
                }

                return mixedChanges;
            }

            /// <summary>
            /// Returns true if the change incldues text changes (not just line changes).
            /// </summary>
            private static bool IncludesTextChanges(TextContentChangedEventArgs e) {
                bool mixedChanges = false;
                foreach (var change in e.Changes) {
                    if (change.OldText != "" || change.NewText != Environment.NewLine) {
                        mixedChanges = true;
                        break;
                    }
                }
                return mixedChanges;
            }
        }

        private void EnqueWorker(IEnumerable<TextContentProvider> contents) {
            Interlocked.Increment(ref _analysisPending);

            ThreadPool.QueueUserWorkItem(
                dummy => {
                    DoParse(contents);
                }
            );
        }

        private void DoParse(IEnumerable<TextContentProvider> contents) {
            try {
                foreach (var content in contents) {
                    _parser.Parse(content);
                }
            } finally {
                Interlocked.Decrement(ref _analysisPending);
            }
        }

        private static SnapshotSpan[] GetSpansToAnalyze(ITextBuffer buffer, ITextSnapshot snapshot) {
            if (buffer.Properties.ContainsProperty(typeof(IMixedBuffer))) {
                var mixedBuffer = buffer.Properties.GetProperty<IMixedBuffer>(typeof(IMixedBuffer));
                return mixedBuffer.GetLanguageSpans(snapshot);
            }
            return null;
        }

        private static TextContentProvider GetContentProvider(ITextBuffer buffer, ITextSnapshot snapshot) {
            var spans = GetSpansToAnalyze(buffer, snapshot);
            if (spans == null) {
                return new SnapshotTextContentProvider(snapshot);            
            } else if (spans.Length == 1) {
                return new SnapshotSpanTextContentProvider(spans[0]);
            } else if (spans.Length == 0) {
                return new SnapshotSpanTextContentProvider(new SnapshotSpan(snapshot, new Span(0, 0)));
            } else {
                return new SnapshotMultipleSpanTextContentProvider(new NormalizedSnapshotSpanCollection(spans));
            }
        }

        public bool IsParsing {
            get {
                return _analysisPending > 0;
            }
        }
    }
}
