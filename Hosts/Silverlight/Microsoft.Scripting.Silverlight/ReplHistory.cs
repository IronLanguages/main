using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Silverlight {

    public class ReplHistory {

        public class HistoryEntry {
            public string Text;
            public bool Command;
            public int Duration;
            public bool Failed;
        }

        public class HistoryEventArgs : EventArgs {
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

        public ReplHistory()
            : this(50) {
        }

        public ReplHistory(int maxLength) {
            _maxLength = maxLength;
            _pos = -1;
            _live = false;
            _history = new ObservableCollection<HistoryEntry>();
        }

        public int MaxLength {
            get { return _maxLength; }
        }

        public int Length {
            get { return _history.Count; }
        }

        public IEnumerable<HistoryEntry> Items {
            get { return _history; }
        }

        public HistoryEntry Last {
            get {
                if (_history.Count > 0) {
                    return _history[_history.Count - 1];
                } else {
                    return null;
                }
            }
        }

        public HistoryEntry First {
            get {
                if (_history.Count > 0) {
                    return _history[0];
                } else {
                    return null;
                }
            }
        }

        public int Position {
            get {
                if (_pos >= 0) {
                    return _pos;
                } else {
                    return Length;
                }
            }
        }

        public void Add(string text) {
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

        public string GetNextText() {
            _live = true;
            if (Length <= 0 || _pos < 0 || _pos == Length - 1) {
                return null;
            }
            _pos++;
            return GetHistoryText(_pos);
        }

        public string GetPreviousText() {
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
            (x, y) => x.ToLower().IndexOf(y.ToLower()) >= 0,
            (x, y) => x.ToLower().IndexOf(y.ToLower()) == 0,
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
