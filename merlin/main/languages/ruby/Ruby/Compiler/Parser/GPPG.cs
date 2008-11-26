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
using System.Diagnostics;
using System.IO;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Threading;

namespace IronRuby.Compiler {

    #region State

    [Serializable]
    public sealed class State {
#if DEBUG
        private int _id;
        public int Id { get { return _id; } set { _id = value; } }
#endif

        // State x Terminal -> ERROR + SHIFT(State) + REDUCE(State) + ACCEPT
        //
        // SHIFT > 0
        // ERROR == 0
        // REDUCE < 0
        // ACCEPT == -1
        private readonly Dictionary<int, int> _actions;

        // State x NonTerminal -> State
        private readonly Dictionary<int, int> _gotos;   

        // ParseAction - default action if terminal not in _actions dict
        private readonly int _defaultAction;		  

        public int DefaultAction {
            get { return _defaultAction; }
        }

        public Dictionary<int, int> GotoStates {
            get { return _gotos; }
        }

        public Dictionary<int, int> Actions {
            get { return _actions; }
        }

        public State(Dictionary<int, int> actions, Dictionary<int, int> gotos, int defaultAction) {
            _actions = actions;
            _gotos = gotos;
            _defaultAction = defaultAction;
        }
    }

    #endregion

    #region Stack

    public class ParserStack<T> {
        private T[]/*!*/ _array = new T[1];
        private int _top = 0;

        public void Push(T value) {
            if (_top >= _array.Length) {
                T[] newarray = new T[_array.Length * 2];
                System.Array.Copy(_array, newarray, _top);
                _array = newarray;
            }
            _array[_top++] = value;
        }

        public T Pop() {
            return _array[--_top];
        }

        public void Pop(int depth) {
            _top -= depth;
        }
        
        public T Peek(int depth) {
            return _array[_top - depth];
        }

        public bool IsEmpty() {
            return _top == 0;
        }

        public IEnumerable<T>/*!*/ GetEnumerator() {
            return _array;
        }
    }

    #endregion

    #region ParserTables

    public sealed class ParserTables {
        public State[] States;
        public byte[] RuleLhsNonTerminals;
        public byte[] RuleRhsLengths;
        public int ErrorToken;
        public int EofToken;

#if DEBUG // Metadata
        internal string[] NonTerminalNames;

        // concatenated symbols of rule RHSs; 
        // symbol < 0 represents a non-terminal
        // symbol >= 0 represents a terminal
        internal short[] RuleRhsSymbols;

        // rule index -> index in RuleRhsSymbols array (calculated):
        internal ushort[] RuleRhsSymbolIndexes;
#endif
    }

    #endregion

    #region IParserLogger

    internal interface IParserLogger {
        void BeforeReduction(int ruleId);
        void BeforeShift(int stateId, int tokenId, bool isErrorShift);
        void BeforeGoto(int stateId, int ruleId);
        void StateEntered();
        void NextToken(int tokenId);
    }

    #endregion

    #region ShiftReduceParser

    public abstract class ShiftReduceParser<TValue, TLocation>
        where TValue : struct {

        private static ParserTables _tables;
        private static readonly object _tablesLock = new object();

        protected TValue yyval;
        protected TLocation yyloc;

        // Experimental : last yylloc prior to call of yylex()
        private TLocation _lastTokenSpan;

        private int _nextToken;
        private State _currentState;

        private bool _recovering;
        private int _tokensSinceLastError;

        private ParserStack<State>/*!*/ _stateStack;
        private ParserStack<TValue>/*!*/ _valueStack;
        private ParserStack<TLocation>/*!*/ _locationStack;
        private int _errorToken;
        private int _eofToken;

        private State[] _states;
        private byte[] _ruleLhsNonTerminals;
        private byte[] _ruleRhsLengths;

#if DEBUG
        // test hooks:
        internal State CurrentState { get { return _currentState; } } 
        internal ParserStack<State>/*!*/ StateStack { get { return _stateStack; } }
        internal ParserStack<TValue>/*!*/ ValueStack { get { return _valueStack; } }
        internal ParserStack<TLocation>/*!*/ LocationStack { get { return _locationStack; } }
        internal State[] States { get { return _states; } }
        internal byte[] RuleLhsNonTerminals { get { return _ruleLhsNonTerminals; } }
        internal byte[] RuleRhsLengths { get { return _ruleRhsLengths; } }
        internal ParserTables Tables { get { return _tables; } } 
#endif

        public ShiftReduceParser() {
            _stateStack = new ParserStack<State>();
            _valueStack = new ParserStack<TValue>();
            _locationStack = new ParserStack<TLocation>();

            if (_tables == null) {
                lock (_tablesLock) {
                    if (_tables == null) {
                        ParserTables tables = new ParserTables();
                        Initialize(tables);
#if DEBUG
                        InitializeMetadata(tables);
                        InitializeRulesMetadata(tables);
#endif
                        Thread.MemoryBarrier();
                        _tables = tables;
                    }
                }
            }

            _states = _tables.States;
            _ruleLhsNonTerminals = _tables.RuleLhsNonTerminals;
            _ruleRhsLengths = _tables.RuleRhsLengths;
            _errorToken = _tables.ErrorToken;
            _eofToken = _tables.EofToken;
        }

        protected abstract void Initialize(ParserTables/*!*/ tables);
        protected abstract TLocation MergeLocations(TLocation start, TLocation end);

        protected abstract TValue TokenValue { get; }     // lexical value: set by scanner
        protected abstract TLocation TokenSpan { get; }   // location value: set by scanner
        protected abstract TLocation DefaultTokenSpan { get; }

        protected abstract int GetNextToken();
        protected abstract void ReportSyntaxError(string message);
        
        protected State[]/*!*/ BuildStates(short[]/*!*/ data) {
            Debug.Assert(data != null && data.Length > 0);

            // 
            // serialized structure:
            //
            // length, 
            // (
            //   (action_count: positive short, goto_count: positive short) | (action_count: negative short), 
            //   (key: short, value: short){action_count} | (defaultAction: short), 
            //   (key: short, value: short){goto_count} 
            // ){length}
            //
            // where action_count is 
            //   > 0  ... a number of items in actions hashtable 
            //   == 0 ... there is no action hashtable, but there is a single integer default action id
            //   < 0  ... there is no action hashtable and no goto table, the value is default action id
            // goto_count is a number of items in gotos hashtable,
            //   zero means there is no goto hashtable
            //

            int offset = 0;
            State[] states = new State[data[offset++]];

            for (int i = 0; i < states.Length; i++) {
                int actionCount = data[offset++];

                Dictionary<int, int> actions = null;
                Dictionary<int, int> gotos = null;
                int defaultAction = 0;

                if (actionCount >= 0) {
                    int gotoCount = data[offset++];
                    Debug.Assert(gotoCount >= 0);

                    if (actionCount > 0) {
                        actions = new Dictionary<int, int>(actionCount);
                        for (int j = 0; j < actionCount; j++) {
                            actions.Add(data[offset++], data[offset++]);
                        }
                    } else {
                        defaultAction = data[offset++];
                    }

                    if (gotoCount > 0) {
                        gotos = new Dictionary<int, int>(gotoCount);
                        for (int j = 0; j < gotoCount; j++) {
                            Debug.Assert(data[offset] < 0);
                            gotos.Add(-data[offset++], data[offset++]);
                        }
                    }
                } else {
                    defaultAction = actionCount;
                }

                states[i] = new State(actions, gotos, defaultAction);
#if DEBUG
                states[i].Id = i;
#endif
            }

            return states;
        }

        public bool Parse() {

            _nextToken = 0;
            _currentState = _states[0];
            _lastTokenSpan = TokenSpan;

            _stateStack.Push(_currentState);
            _valueStack.Push(yyval);
            _locationStack.Push(yyloc);

            while (true) {

                LogStateEntered();
                
                int action = _currentState.DefaultAction;

                if (_currentState.Actions != null) {
                    if (_nextToken == 0) {

                        // We save the last token span, so that the location span
                        // of production right hand sides that begin or end with a
                        // nullable production will be correct.
                        _lastTokenSpan = TokenSpan;
                        _nextToken = GetNextToken();
                    }

                    LogNextToken(_nextToken);

                    _currentState.Actions.TryGetValue(_nextToken, out action);
                }

                if (action > 0) {
                    LogBeforeShift(action, _nextToken, false);
                    Shift(action);
                } else if (action < 0) {
                    Reduce(-action - 1);

                    if (action == -1)	// accept
                        return true;
                } else if (action == 0) {
                    // error
                    if (!ErrorRecovery())
                        return false;
                }
            }
        }

        protected void Shift(int stateId) {
            _currentState = _states[stateId];

            _valueStack.Push(TokenValue);
            _stateStack.Push(_currentState);
            _locationStack.Push(TokenSpan);

            if (_recovering) {
                if (_nextToken != _errorToken) {
                    _tokensSinceLastError++;
                }

                if (_tokensSinceLastError > 5) {
                    _recovering = false;
                }
            }

            if (_nextToken != _eofToken) {
                _nextToken = 0;
            }
        }


        protected void Reduce(int ruleId) {
            LogBeforeReduction(ruleId);

            int rhsLength = _ruleRhsLengths[ruleId];

            //
            //  Default action "$$ = $1" for unit productions.
            //  Default action "@$ = @1.Merge(@N)" for location info.
            //
            if (rhsLength == 1) {
                yyval = _valueStack.Peek(1); // default action: $$ = $1;
                yyloc = _locationStack.Peek(1);
            } else {
                yyval = new TValue();
                if (rhsLength == 0) {
                    // The location span for an empty production will start with the
                    // beginning of the next lexeme, and end with the finish of the
                    // previous lexeme.  This gives the correct behaviour when this
                    // nonsense value is used in later Merge operations.
                    yyloc = MergeLocations(_lastTokenSpan, TokenSpan);
                } else {
                    TLocation at1 = GetLocation(rhsLength);
                    TLocation atN = GetLocation(1);
                    if (at1 != null && atN != null) {
                        yyloc = MergeLocations(at1, atN);
                    }
                }
            }

            DoAction(ruleId);

            _stateStack.Pop(rhsLength);
            _valueStack.Pop(rhsLength);
            _locationStack.Pop(rhsLength);

            _currentState = _stateStack.Peek(1);

            int gotoState;
            if (_currentState.GotoStates.TryGetValue(_ruleLhsNonTerminals[ruleId], out gotoState)) {
                LogBeforeGoto(gotoState, ruleId);
                _currentState = _states[gotoState];
            }
            
            _stateStack.Push(_currentState);
            _valueStack.Push(yyval);
            _locationStack.Push(yyloc);
        }


        protected abstract void DoAction(int action_nr);

        public bool ErrorRecovery() {
            bool discard;

            if (!_recovering) { // if not recovering from previous error
                ReportSyntaxError(GetSyntaxErrorMessage());
            }

            if (!FindErrorRecoveryState())
                return false;

            //
            //  The interim fix for the "looping in error recovery"
            //  artifact involved moving the setting of the recovering 
            //  bool until after invalid tokens have been discarded.
            //
            ShiftErrorToken();
            discard = DiscardInvalidTokens();
            _recovering = true;
            _tokensSinceLastError = 0;
            return discard;
        }

        private string GetSyntaxErrorMessage() {
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.AppendFormat("syntax error, unexpected {0}", Parser.TerminalToString(_nextToken));

            if (_currentState.Actions.Count < 7) {
                bool first = true;
                foreach (int terminal in _currentState.Actions.Keys) {
                    if (first) {
                        errorMsg.Append(", expecting ");
                    } else {
                        errorMsg.Append(", or ");
                    }

                    errorMsg.Append(Parser.TerminalToString(terminal));
                    first = false;
                }
            }
            return errorMsg.ToString();
        }

        public void ShiftErrorToken() {
            int oldNext = _nextToken;
            _nextToken = _errorToken;

            int state = _currentState.Actions[_nextToken];
            LogBeforeShift(state, _nextToken, true);
            Shift(state);

            _nextToken = oldNext;
        }


        public bool FindErrorRecoveryState() {
            // pop states until one found that accepts error token
            while (true) {

                // shift
                int action;
                if (_currentState.Actions != null && _currentState.Actions.TryGetValue(_errorToken, out action) && action > 0) {
                    return true;
                }

                // LogState("Error, popping state", _stateStack.Peek(1));

                _stateStack.Pop();
                _valueStack.Pop();
                _locationStack.Pop();

                if (_stateStack.IsEmpty()) {
                    // Log("Aborting: didn't find a state that accepts error token");
                    return false;
                } else {
                    _currentState = _stateStack.Peek(1);
                }
            }
        }


        public bool DiscardInvalidTokens() {

            int action = _currentState.DefaultAction;

            if (_currentState.Actions != null) {
                
                // Discard tokens until find one that works ...
                while (true) {
                    if (_nextToken == 0) {
                        _nextToken = GetNextToken();
                    }

                    LogNextToken(_nextToken);

                    if (_nextToken == _eofToken)
                        return false;

                    _currentState.Actions.TryGetValue(_nextToken, out action);

                    if (action != 0) {
                        return true;
                    }

                    // LogToken("Error, discarding token", _nextToken);
                    _nextToken = 0;
                }

            } else if (_recovering && _tokensSinceLastError == 0) {
                // 
                //  Boolean recovering is not set until after the first
                //  error token has been shifted.  Thus if we get back 
                //  here with recovering set and no tokens read we are
                //  looping on the same error recovery action.  This 
                //  happens if current_state.parser_table is null because
                //  the state has an LR(0) reduction, but not all
                //  lookahead tokens are valid.  This only occurs for
                //  error productions that *end* on "error".
                //
                //  This action discards tokens one at a time until
                //  the looping stops.  Another attack would be to always
                //  use the LALR(1) table if a production ends on "error"
                //
                // LogToken("Error, panic discard of {0}", _nextToken);
                _nextToken = 0;
                return true;
            } else {
                return true;
            }
        }

        protected TValue GetValue(int depth) {
            return _valueStack.Peek(depth);
        }

        protected TLocation GetLocation(int depth) {
            return _locationStack.Peek(depth);
        }

        protected void ClearInput() {
            // experimental in this version.
            _nextToken = 0;
        }

        protected void StopErrorRecovery() {
            _recovering = false;
        }

        #region Debug Logging

#if DEBUG
        private IParserLogger _logger;
#endif

        [Conditional("DEBUG")]
        internal void EnableLogging(IParserLogger/*!*/ logger) {
#if DEBUG
            Assert.NotNull(logger);
            _logger = logger;
#endif
        }

        [Conditional("DEBUG")]
        internal void DisableLogging() {
#if DEBUG
            _logger = null;
#endif
        }

        [Conditional("DEBUG")]
        private void LogStateEntered() {
#if DEBUG
            if (_logger != null) _logger.StateEntered();
#endif
        }

        [Conditional("DEBUG")]
        private void LogNextToken(int tokenId) {
#if DEBUG
            if (_logger != null) _logger.NextToken(tokenId);
#endif
        }

        [Conditional("DEBUG")]
        private void LogBeforeReduction(int ruleId) {
#if DEBUG
            if (_logger != null) _logger.BeforeReduction(ruleId);
#endif
        }

        [Conditional("DEBUG")]
        private void LogBeforeShift(int stateId, int tokenId, bool isErrorShift) {
#if DEBUG
            if (_logger != null) _logger.BeforeShift(stateId, tokenId, isErrorShift);
#endif
        }

        [Conditional("DEBUG")]
        private void LogBeforeGoto(int stateId, int ruleId) {
#if DEBUG
            if (_logger != null) _logger.BeforeGoto(stateId, ruleId);
#endif
        }

        #endregion

        #region Parser Reflection
        
#if DEBUG
        protected abstract void InitializeMetadata(ParserTables/*!*/ tables);

        private static void InitializeRulesMetadata(ParserTables/*!*/ tables) {
            ushort[] indexes = new ushort[tables.RuleRhsLengths.Length];
            ushort index = 0;
            for (int i = 0; i < indexes.Length; i++) {
                indexes[i] = index;
                index += tables.RuleRhsLengths[i];
            }
            tables.RuleRhsSymbolIndexes = indexes;
        }
        
        // SHIFT > 0
        // ERROR == 0
        // REDUCE < 0
        // ACCEPT == -1
        internal string ActionToString(int action) {
            if (action > 0) return "S(" + action + ")";
            if (action == 0) return "";
            if (action == -1) return "ACCEPT";
            return "R(" + (-action) + ")"; 
        }

        internal string NonTerminalToString(int nonTerminal) {
            Debug.Assert(nonTerminal > 0);
            return _tables.NonTerminalNames[nonTerminal];
        }

        // < 0 -> non-terminal
        // > 0 -> terminal
        internal string SymbolToString(int symbol) {
            return (symbol < 0) ? NonTerminalToString(-symbol) : Parser.TerminalToString(symbol);
        }

        internal string RuleToString(int ruleIndex) {
            Debug.Assert(ruleIndex >= 0);
            StringBuilder sb = new StringBuilder();
            sb.Append(NonTerminalToString(_tables.RuleLhsNonTerminals[ruleIndex]));
            sb.Append(" -> ");

            // index of the first RHS symbol:
            int rhsLength = _tables.RuleRhsLengths[ruleIndex];
            if (rhsLength > 0) {
                int first = _tables.RuleRhsSymbolIndexes[ruleIndex];
                for (int i = 0; i < rhsLength; i++) {
                    sb.Append(SymbolToString(_tables.RuleRhsSymbols[first + i]));
                    sb.Append(" ");
                }
            } else {
                sb.Append("<empty>");
            }

            return sb.ToString();
        }
#endif

        [Conditional("DEBUG")]
        public void DumpTables(TextWriter/*!*/ output) {
#if DEBUG
            Dictionary<int, bool> terminals = new Dictionary<int, bool>();
            Dictionary<int, bool> nonterminals = new Dictionary<int, bool>();

            int termCount = -1;
            int ntermCount = -1;
            for (int q = 0; q < _states.Length; q++) {
                State s = _states[q];
                if (s.Actions != null) {
                    foreach (int t in s.Actions.Keys) {
                        if (t > termCount) {
                            termCount = t;
                        }

                        terminals[t] = true;
                    }
                }

                if (s.GotoStates != null) {
                    foreach (int t in s.GotoStates.Keys) {
                        if (t > ntermCount) {
                            ntermCount = t;
                        }
                        nonterminals[t] = true;
                    }
                }
            }

            output.WriteLine("States x (Terms + NonTerms) = {0} x ({1} + {2})", _states.Length, termCount, ntermCount);

            output.Write("State,");
            output.Write("Default,");
            for (int t = 0; t < termCount; t++) {
                if (terminals.ContainsKey(t)) {
                    output.Write(Parser.TerminalToString(t));
                    output.Write(",");
                }
            }

            for (int t = 0; t < ntermCount; t++) {
                if (nonterminals.ContainsKey(t)) {
                    output.Write(t); // TODO
                    output.Write(",");
                }
            }

            for (int q = 0; q < _states.Length; q++) {
                State s = _states[q];
                output.Write(q);
                output.Write(",");
                if (s.Actions == null) {
                    output.Write(ActionToString(s.DefaultAction));
                }
                output.Write(",");

                for (int t = 0; t < termCount; t++) {
                    if (terminals.ContainsKey(t)) {
                        int action;
                        if (s.Actions != null) {
                            s.Actions.TryGetValue(t, out action);
                            output.Write(ActionToString(action));
                        }
                        output.Write(",");
                    }
                }

                for (int t = 0; t < ntermCount; t++) {
                    if (nonterminals.ContainsKey(t)) {
                        if (s.GotoStates != null) {
                            int state;
                            if (s.GotoStates.TryGetValue(t, out state)) {
                                output.Write(state);
                            }
                        }
                        output.Write(",");
                    }
                }
                output.WriteLine();
            }
#endif
        }

        #endregion
    }

    #endregion
}
