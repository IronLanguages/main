using System;
using System.Collections.Generic;
using System.Text;

namespace IronRuby.Compiler {
    public class ParserStack<TState, TValue, TLocation> {
        // Most Ruby files are around 30, few are around 100
        private const int InitialSize = 50;
        
        // It's actually faster to keep these as separate arrays rather then wrap entries into a struct.
        private TState[]/*!*/ _states = new TState[InitialSize];
        private TLocation[]/*!*/ _locations = new TLocation[InitialSize];
        private TValue[]/*!*/ _values = new TValue[InitialSize];
        
        private int _top = 0;

        public void Push(TState state, TValue value, TLocation location) {
            int top = _top;
            if (top == _states.Length) {
                var newStates = new TState[top * 2];
                var newValues = new TValue[top * 2];
                var newLocations = new TLocation[top * 2];
                Array.Copy(_states, newStates, top);
                Array.Copy(_values, newValues, top);
                Array.Copy(_locations, newLocations, top);
                _states = newStates;
                _values = newValues;
                _locations = newLocations;
            }
            _states[top] = state;
            _values[top] = value;
            _locations[top] = location;
            _top = top + 1;
        }

        public void Pop() {
            _top--;
        }

        public void Pop(int depth) {
            _top -= depth;
        }

        public TState PeekState(int depth) {
            return _states[_top - depth];
        }

        public TLocation PeekLocation(int depth) {
            return _locations[_top - depth];
        }

        public TValue PeekValue(int depth) {
            return _values[_top - depth];
        }

        public bool IsEmpty {
            get { return _top == 0; }
        }

        // debug dump only:
        public IEnumerable<TState>/*!*/ GetStates() {
            return _states;
        }
    }
}
