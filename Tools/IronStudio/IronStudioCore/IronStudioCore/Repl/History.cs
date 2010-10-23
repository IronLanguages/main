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
using System.Collections.ObjectModel;

namespace Microsoft.IronStudio.Core.Repl {
    internal class History {
        internal class HistoryEntry {
            internal string Text;
            internal bool Command;
            internal int Duration;
            internal bool Failed;
        }

        internal class HistoryEventArgs : EventArgs {
            private readonly HistoryEntry _historyEntry;

            internal HistoryEventArgs(HistoryEntry entry) {
                _historyEntry = entry;
            }

            internal HistoryEntry HistoryEntry {
                get { return _historyEntry; }
            }
        }

        private readonly int _maxLength;
        private int _pos;
        private bool _live;
        private readonly ObservableCollection<HistoryEntry> _history;

        internal History()
            : this(50) {
        }

        internal History(int maxLength) {
            _maxLength = maxLength;
            _pos = -1;
            _live = false;
            _history = new ObservableCollection<HistoryEntry>();
        }

        internal int MaxLength {
            get { return _maxLength; }
        }

        internal int Length {
            get { return _history.Count; }
        }

        internal IEnumerable<HistoryEntry> Items {
            get { return _history; }
        }

        internal HistoryEntry Last {
            get {
                if (_history.Count > 0) {
                    return _history[_history.Count - 1];
                } else {
                    return null;
                }
            }
        }

        internal int Position {
            get {
                if (_pos >= 0) {
                    return _pos;
                } else {
                    return Length;
                }
            }
        }

        internal void Add(string text) {
            var entry = new HistoryEntry { Text = text };
            _live = false;
            if (Length == 0 || Last.Text != text) {
                _history.Add(entry);
                var handler = ItemAdded;
                if (handler != null) {
                    handler(this, new HistoryEventArgs(entry));
                }
            }
            if (_history[InternalPosition].Text != text) {
                _pos = -1;
            }
            if (Length > MaxLength) {
                _history.RemoveAt(0);
                if (_pos > 0) {
                    _pos--;
                }
            }
        }

        private int InternalPosition {
            get {
                if (_pos == -1) {
                    return Length - 1;
                } else {
                    return _pos;
                }
            }
        }

        private string GetHistoryText(int pos) {
            if (pos < 0) {
                pos += Length;
            }
            return _history[pos].Text;
        }

        internal string GetNextText() {
            _live = true;
            if (Length <= 0 || _pos < 0 || _pos == Length - 1) {
                return null;
            }
            _pos++;
            return GetHistoryText(_pos);
        }

        internal string GetPreviousText() {
            bool wasLive = _live;
            _live = true;
            if (Length == 0 || (Length > 1 && _pos == 0)) {
                return null;
            }
            if (_pos == -1) {
                _pos = Length - 1;
            } else if (!wasLive) {
                // Handles up up up enter up
                // Do nothing
            } else {
                _pos--;
            }
            return GetHistoryText(_pos);
        }

        private static readonly Func<string, string, bool>[] _matchFns = new Func<string, string, bool>[] {
            (x, y) => x.ToLowerInvariant().IndexOf(y.ToLowerInvariant()) >= 0,
            (x, y) => x.ToLowerInvariant().IndexOf(y.ToLowerInvariant()) == 0,
            (x, y) => x.IndexOf(y) >= 0,
            (x, y) => x.IndexOf(y) == 0
        };

        private string FindMatch(string mask, bool caseSensitive, Func<string> moveFn) {
            var matchFn = _matchFns[2 * (caseSensitive ? 1 : 0) + 1];
            var startPos = _pos;
            while (true) {
                string next = moveFn();
                if (next == null) {
                    _pos = startPos;
                    return null;
                }
                if (matchFn(next, mask)) {
                    return next;
                }
            }
        }

        internal string FindNext(string mask) {
            return FindMatch(mask, false, GetNextText);
        }

        internal string FindPrevious(string mask) {
            return FindMatch(mask, false, GetPreviousText);
        }

        internal event EventHandler<HistoryEventArgs> ItemAdded;
    }
}
