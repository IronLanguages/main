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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Compiler.Ast;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler {
    internal enum LexicalState : byte {
        EXPR_BEG,			// ignore newline, +/- is a sign.
        EXPR_END,			// newline significant, +/- is an operator.
        EXPR_ARG,			// newline significant, +/- is an operator.
        EXPR_CMDARG,		// newline significant, +/- is an operator, an identifier is a command name, special behavior of do keyword and left parenthesis
        EXPR_ENDARG,		// newline significant, +/- is an operator, unbound braces.
        EXPR_MID,			// newline significant, +/- is an operator.
        EXPR_FNAME,			// ignore newline, no reserved words.
        EXPR_ENDFN,         // newline significant, +/- is an operator, unbound braces (1.9)
        EXPR_DOT,			// right after `.' or `::', no reserved words.
        EXPR_CLASS,			// immediate after `class', no here document.
        EXPR_VALUE,         // alike EXPR_BEG but label is disallowed (1.9)
    };

    public class Tokenizer : TokenizerService {
        private readonly ILexicalVariableResolver/*!*/ _localVariableResolver;
        private BignumParser _bigIntParser;

        private const int InitialBufferSize = 80;

        public bool ForceBinaryMultiByte { get; set; }

        public bool AllowNonAsciiIdentifiers {
            get { return _multiByteIdentifier < Int32.MaxValue; }
            set { _multiByteIdentifier = value ? AllowMultiByteIdentifier : Int32.MaxValue; }
        }

        internal RubyEncoding/*!*/ Encoding {
            get { return _encoding; }
            set {
                Assert.NotNull(_encoding);
                _encoding = value;
            }
        }

        public bool Verbatim {
            get { return _verbatim; }
            set { _verbatim = value; }
        }

        private RubyEncoding/*!*/ _encoding;
        private TextReader _input;
        private SourceLocation _initialLocation;
        private RubyCompatibility _compatibility;

        // true if running w/o parser: comments and whitespace tokens are yielded
        private bool _verbatim;

        private int _multiByteIdentifier = Int32.MaxValue;

        private SourceUnit _sourceUnit;
        private ErrorSink/*!*/ _errorSink;

        #region State

        internal struct NestedTokenSequence : IEquatable<NestedTokenSequence> {
            internal static readonly NestedTokenSequence[] EmptyArray = new NestedTokenSequence[0];

            public readonly TokenSequenceState State;
            public readonly int OpenedBracesInEmbeddedCode;

            public NestedTokenSequence(TokenSequenceState/*!*/ state, int openedBracesInEmbeddedCode) {
                State = state;
                OpenedBracesInEmbeddedCode = openedBracesInEmbeddedCode;
            }

            public override bool Equals(object other) {
                return other is NestedTokenSequence && Equals((NestedTokenSequence)other);
            }

            public bool Equals(NestedTokenSequence other) {
                return ReferenceEquals(State, other.State) || (State != null && State.Equals(other.State)
                    && OpenedBracesInEmbeddedCode == other.OpenedBracesInEmbeddedCode
                );
            }

            public override int GetHashCode() {
                return State.GetHashCode() ^ OpenedBracesInEmbeddedCode;
            }
        }

        internal sealed class State : IEquatable<State> {
            internal TokenSequenceState/*!*/ _currentSequence;
            internal NestedTokenSequence[]/*!*/ _nestedSequences;
            internal int _nestedSequenceCount;

            // Verbatim mode only: a queue of heredocs that were started on the current line.
            // The queued heredocs will be pushed to the _nestedSequences stack at the end of the line.
            internal List<VerbatimHeredocState> VerbatimHeredocQueue;

            internal LexicalState LexicalState;

            // The number of unmatched left braces since the start of last string embedded code.
            internal int OpenedBracesInEmbeddedCode;

            // True (1) if the following identifier is treated as a command name (sets LexicalState.CMDARG).
            internal byte _commandMode;

            // True (1) if the previous token is Tokens.Whitespace.
            internal byte _whitespaceSeen;

            // True (1) if the previous token is Tokens.StringEmbeddedVariableBegin.
            internal byte _inStringEmbeddedVariable;

            // Non-zero => End of the last heredoc that finished reading content.
            // While non-zero the current stream position doesn't correspond the current line and line index 
            // (the stream is ahead, we are reading from a buffer restored by the last heredoc).
            internal int HeredocEndLine;
            internal int HeredocEndLineIndex;

            internal State(State src) {
                if (src != null) {
                    LexicalState = src.LexicalState;
                    OpenedBracesInEmbeddedCode = src.OpenedBracesInEmbeddedCode;
                    _commandMode = src._commandMode;
                    _whitespaceSeen = src._whitespaceSeen;
                    _inStringEmbeddedVariable = src._inStringEmbeddedVariable;
                    HeredocEndLine = src.HeredocEndLine;
                    HeredocEndLineIndex = src.HeredocEndLineIndex;
                    _currentSequence = src._currentSequence;
                    _nestedSequenceCount = src._nestedSequenceCount;
                    if (_nestedSequenceCount > 0) {
                        _nestedSequences = new NestedTokenSequence[_nestedSequenceCount];
                        Array.Copy(src._nestedSequences, 0, _nestedSequences, 0, _nestedSequenceCount);
                    } else {
                        _nestedSequences = NestedTokenSequence.EmptyArray;
                    }
                } else {
                    LexicalState = LexicalState.EXPR_BEG;
                    _commandMode = 1;
                    HeredocEndLineIndex = -1;
                    _currentSequence = TokenSequenceState.None;
                    _nestedSequences = NestedTokenSequence.EmptyArray;
                }
            }

            public override bool Equals(object other) {
                return Equals(other as State);
            }

            public bool Equals(State other) {
                return ReferenceEquals(this, other) || (other != null
                    && _currentSequence == other._currentSequence
                    && Utils.ValueEquals(_nestedSequences, _nestedSequenceCount, other._nestedSequences, other._nestedSequenceCount)
                    && LexicalState == other.LexicalState
                    && OpenedBracesInEmbeddedCode == other.OpenedBracesInEmbeddedCode
                    && _commandMode == other._commandMode
                    && _whitespaceSeen == other._whitespaceSeen
                    && _inStringEmbeddedVariable == other._inStringEmbeddedVariable
                    && HeredocEndLine == other.HeredocEndLine
                    && HeredocEndLineIndex == other.HeredocEndLineIndex
                );
            }

            public override int GetHashCode() {
                return _currentSequence.GetHashCode()
                     ^ _nestedSequences.GetValueHashCode(0, _nestedSequenceCount)
                     ^ _nestedSequenceCount
                     ^ (int)LexicalState
                     ^ OpenedBracesInEmbeddedCode
                     ^ _commandMode
                     ^ _whitespaceSeen
                     ^ _inStringEmbeddedVariable
                     ^ HeredocEndLine
                     ^ HeredocEndLineIndex;
            }

            internal TokenSequenceState/*!*/ CurrentSequence {
                get { return _currentSequence; }
                set {
                    Assert.NotNull(value);
                    _currentSequence = value;
                }
            }

            private void PushNestedSequence(NestedTokenSequence sequence) {
                if (_nestedSequences.Length == _nestedSequenceCount) {
                    Array.Resize(ref _nestedSequences, _nestedSequences.Length * 2 + 1);
                }
                _nestedSequences[_nestedSequenceCount++] = sequence;
            }

            private NestedTokenSequence PopNestedSequence() {
                Debug.Assert(_nestedSequenceCount > 0);
                var result = _nestedSequences[_nestedSequenceCount - 1];
                _nestedSequences[_nestedSequenceCount - 1] = default(NestedTokenSequence);
                _nestedSequenceCount--;
                return result;
            }

            internal void StartStringEmbeddedCode() {
                PushNestedSequence(new NestedTokenSequence(_currentSequence, OpenedBracesInEmbeddedCode));
                OpenedBracesInEmbeddedCode = 0;
                _currentSequence = TokenSequenceState.None;
            }

            internal void EndStringEmbeddedCode() {
                var nestedSeq = PopNestedSequence();
                _currentSequence = nestedSeq.State;
                OpenedBracesInEmbeddedCode = nestedSeq.OpenedBracesInEmbeddedCode;
                Debug.Assert(OpenedBracesInEmbeddedCode >= 0);
            }

            internal void EnqueueVerbatimHeredoc(VerbatimHeredocState/*!*/ heredoc) {
                if (VerbatimHeredocQueue == null) {
                    VerbatimHeredocQueue = new List<VerbatimHeredocState>();
                }
                VerbatimHeredocQueue.Add(heredoc);
            }

            internal void DequeueVerbatimHeredocs() {
                Debug.Assert(VerbatimHeredocQueue != null && VerbatimHeredocQueue.Count > 0);
                // TODO:
                if (_currentSequence == TokenSequenceState.None) {
                    PushNestedSequence(new NestedTokenSequence(new CodeState(LexicalState, _commandMode, _whitespaceSeen), OpenedBracesInEmbeddedCode));
                } else {
                    PushNestedSequence(new NestedTokenSequence(_currentSequence, OpenedBracesInEmbeddedCode));
                }

                for (int i = VerbatimHeredocQueue.Count - 1; i > 0; i--) {
                    PushNestedSequence(new NestedTokenSequence(VerbatimHeredocQueue[i], OpenedBracesInEmbeddedCode));
                }
                _currentSequence = VerbatimHeredocQueue[0];
                VerbatimHeredocQueue = null;
            }

            internal void FinishVerbatimHeredoc() {
                var nestedSeq = PopNestedSequence();
                // TODO:
                CodeState codeState = nestedSeq.State as CodeState;
                if (codeState != null) {
                    LexicalState = codeState.LexicalState;
                    _commandMode = codeState.CommandMode;
                    _whitespaceSeen = codeState.WhitespaceSeen;
                    _currentSequence = TokenSequenceState.None;
                } else {
                    _currentSequence = nestedSeq.State;
                }
                OpenedBracesInEmbeddedCode = nestedSeq.OpenedBracesInEmbeddedCode;
                Debug.Assert(OpenedBracesInEmbeddedCode >= 0);
            }
        }

        private bool InStringEmbeddedCode { get { return _state._nestedSequenceCount > 0; } }
        private bool InStringEmbeddedVariable { get { return _state._inStringEmbeddedVariable == 1; } set { _state._inStringEmbeddedVariable = value ? (byte)1 : (byte)0; } }
        private bool WhitespaceSeen { get { return _state._whitespaceSeen == 1; } set { _state._whitespaceSeen = value ? (byte)1 : (byte)0; } }
        
        // TODO: (verbatim mode) is this ok to be set by parser?
        public bool CommandMode { get { return _state._commandMode == 1; } set { _state._commandMode = value ? (byte)1 : (byte)0; } }

        private State _state;

        #endregion

        // The number of unmatched left parentheses, brackets, braces and lambda parameter definitions (->).
        // Used to find the begining of a lambda body.
        // The value is insignificant in verbatim mode.
        internal int _openingCount;

        // The value of _openingCount at the begining of a lambda definition.
        // Zeroed at the start of lambda body definition.
        // The value is insignificant in verbatim mode.
        internal int _lambdaOpenings;

        // track command arguments state - used for DO/BLOCK_DO disambiguation; always 0 in verbatim mode:
        private int _commandArgsStateStack;

        // track conditional expression state - used for LOOP_DO/DO/BLOCK_DO disambiguation; always 0 in verbatim mode:
        private int _loopConditionStateStack;

        // Entire line that is currently being tokenized.
        // Includes \r, \n, \r\n if there was eoln in input.
        private char[] _lineBuffer;

        // Portion of _lineBuffer that contains valid data. 
        private int _lineLength;

        // index in the current buffer/line:
        private int _bufferPos;

        // current line no:
        private int _currentLine;
        private int _currentLineIndex;

        // out: whether the last token terminated
        private bool _unterminatedToken;
        private bool _eofReached;
        // out: offset data following __END__ token
        private int _dataOffset;
        // out: token value:
        private TokenValue _tokenValue;
        
        // token positions set during tokenization (TODO: to be replaced by tokenizer buffer):
        private SourceLocation _currentTokenStart;
        private SourceLocation _currentTokenEnd;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private int _currentTokenStartIndex;

        // last token span:
        private SourceSpan _tokenSpan;
        
        #region Initialization

        public Tokenizer() 
            : this(true) {
        }

        public Tokenizer(bool verbatim)
            : this(verbatim, DummyVariableResolver.AllMethodNames) {
        }

        public Tokenizer(bool verbatim, ILexicalVariableResolver/*!*/ localVariableResolver) {
            ContractUtils.RequiresNotNull(localVariableResolver, "localVariableResolver");
            
            _errorSink = ErrorSink.Null;
            _localVariableResolver = localVariableResolver;
            _verbatim = verbatim;
            _encoding = RubyEncoding.Binary;
        }

        public void Initialize(SourceUnit/*!*/ sourceUnit) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");
            Initialize(null, sourceUnit.GetReader(), sourceUnit, SourceLocation.MinValue);
        }

        public void Initialize(TextReader/*!*/ reader) {
            Initialize(null, reader, null, SourceLocation.MinValue);
        }

        public override void Initialize(object state, TextReader/*!*/ reader, SourceUnit sourceUnit, SourceLocation initialLocation) {
            ContractUtils.RequiresNotNull(reader, "reader");

            _sourceUnit = sourceUnit;
            _initialLocation = initialLocation;
            _currentLine = _initialLocation.Line;
            _currentLineIndex = _initialLocation.Index;
            _tokenSpan = new SourceSpan(initialLocation, initialLocation);

            _state = new State(state as State);
            _commandArgsStateStack = 0;
            _loopConditionStateStack = 0;
            _openingCount = 0;
            _input = reader;
            _lineBuffer = null;
            _lineLength = 0;
            _bufferPos = 0;
            _currentTokenStart = SourceLocation.Invalid;
            _currentTokenEnd = SourceLocation.Invalid;
            _currentTokenStartIndex = -1;

            _tokenValue = new TokenValue();
            _eofReached = false;
            _unterminatedToken = false;
            _dataOffset = -1;

            DumpBeginningOfUnit();
        }

        #endregion

        #region Debug Logging

#if DEBUG
        private int _logVerbosity; // 0 means logging disabled
        private TextWriter _log;
#endif
        [Conditional("DEBUG")]
        public void EnableLogging(int verbosity, TextWriter/*!*/ output) {
            Debug.Assert(verbosity > 0 && verbosity <= 2);
            Assert.NotNull(output);
#if DEBUG
            _logVerbosity = verbosity;
            _log = output;
#endif
        }

        [Conditional("DEBUG")]
        public void DisableLogging() {
#if DEBUG
            _logVerbosity = 0;
            _log = null;
#endif
        }

        [Conditional("DEBUG")]
        private void Log(string/*!*/ format, params object[] args) {
#if DEBUG
            if (_logVerbosity > 0) {
                _log.WriteLine(format, args);
            }
#endif
        }

        [Conditional("DEBUG")]
        private void DumpBeginningOfUnit() {
            Log("--- Source unit: '{0}' ---", _sourceUnit != null ? _sourceUnit.Path : "N/A");
        }

        [Conditional("DEBUG")]
        private void DumpToken(Tokens token) {
#if DEBUG
            Log("{0,-25} {1,-25} {2}",
                Parser.GetTerminalName((int)token),
                _tokenValue.ToString(),
                LexicalState);
#endif
        }

        #endregion

        #region Parser Driven State

        internal LexicalState LexicalState {
            get { return _state.LexicalState; }
            set { _state.LexicalState = value; }
        }

        private bool IsEndLexicalState {
            get {
                return _state.LexicalState == LexicalState.EXPR_END 
                    || _state.LexicalState == LexicalState.EXPR_ENDARG 
                    || _state.LexicalState == LexicalState.EXPR_ENDFN;    // 1.9
            }
        }

        private bool IsBeginLexicalState {
            get {
                return _state.LexicalState == LexicalState.EXPR_BEG
                    || _state.LexicalState == LexicalState.EXPR_MID
                    || _state.LexicalState == LexicalState.EXPR_CLASS
                    || _state.LexicalState == LexicalState.EXPR_VALUE;    // 1.9
            }
        }

        private bool InArgs {
            get { return _state.LexicalState == LexicalState.EXPR_ARG || _state.LexicalState == LexicalState.EXPR_CMDARG; }
        }

        private bool InArgsNoSpace(int c) {
            return InArgs && WhitespaceSeen && !IsWhiteSpace(c);
        }

        // Push(n)
        private void BitStackPush(ref int stack, int n) {
            stack = (stack << 1) | ((n) & 1);
        }

        // Pop()
        private int BitStackPop(ref int stack) {
            return (stack >>= 1);
        }

        // x = Pop(), Top |= x
        private void BitStackOrPop(ref int stack) {
            stack = (stack >> 1) | (stack & 1);
        }

        // Peek() != 0
        private bool BitStackPeek(int stack) {
            return (stack & 1) != 0;
        }

        private bool InCommandArgs {
            get { return BitStackPeek(_commandArgsStateStack); }
        }

        private bool InLoopCondition {
            get { return BitStackPeek(_loopConditionStateStack); }
        }

        internal int EnterCommandArguments() {
            int old = _commandArgsStateStack;
            BitStackPush(ref _commandArgsStateStack, 1);
            return old;
        }

        internal void LeaveCommandArguments(int state) {
            _commandArgsStateStack = state;
        }

        internal void EnterLoopCondition() {
            BitStackPush(ref _loopConditionStateStack, 1);
        }

        internal void LeaveLoopCondition() {
            BitStackPop(ref _loopConditionStateStack);
        }

        private void EnterParenthesisedExpression() {
            _openingCount++;
            BitStackPush(ref _loopConditionStateStack, 0);
            BitStackPush(ref _commandArgsStateStack, 0);
        }

        internal void LeaveParenthesisedExpression() {
            _openingCount--;
            PopParenthesisedExpressionStack();
        }

        internal void PopParenthesisedExpressionStack() {
            BitStackOrPop(ref _commandArgsStateStack);

            // was: BitStackOrPop(ref _loopConditionStateStack);
            // this case cannot happen, since it would require closing parenthesis not matching opening
            // ( ... while [[ ... ) ... do ]]
            BitStackPop(ref _loopConditionStateStack);
        }

        private Tokens StringEmbeddedVariableBegin() {
            InStringEmbeddedVariable = true;
            LexicalState = LexicalState.EXPR_BEG;
            return Tokens.StringEmbeddedVariableBegin;
        }

        private Tokens StringEmbeddedCodeBegin() {
            _state.StartStringEmbeddedCode();
            LexicalState = LexicalState.EXPR_BEG;
            EnterParenthesisedExpression();
            return Tokens.StringEmbeddedCodeBegin;
        }

        internal int EnterLambdaDefinition() {
            int old = _lambdaOpenings;
            _lambdaOpenings = ++_openingCount;
            return old;
        }

        internal void LeaveLambdaDefinition(int oldLambdaOpenings) {
            _lambdaOpenings = oldLambdaOpenings;
        }

        #endregion

        #region Error Reporting

        private void Report(string/*!*/ message, int errorCode, SourceSpan location, Severity severity) {
            Debug.Assert(severity != Severity.FatalError);
            _errorSink.Add(_sourceUnit, message, location, errorCode, severity);
        }

        internal void ReportError(ErrorInfo info) {
            Report(info.GetMessage(), info.Code, GetCurrentSpan(), Severity.Error);
        }

        internal void ReportError(ErrorInfo info, params object[] args) {
            Report(info.GetMessage(args), info.Code, GetCurrentSpan(), Severity.Error);
        }

        internal void ReportError(ErrorInfo info, SourceSpan location, params object[] args) {
            Report(info.GetMessage(args), info.Code, location, Severity.Error);
        }

        internal void ReportWarning(ErrorInfo info) {
            Report(info.GetMessage(), info.Code, GetCurrentSpan(), Severity.Warning);
        }

        internal void ReportWarning(ErrorInfo info, params object[] args) {
            Report(info.GetMessage(args), info.Code, GetCurrentSpan(), Severity.Warning);
        }

        // TODO:
        private void WarnBalanced(string p, string p_2) {
            // 1.9
        }

        #endregion

        #region Buffer Operations

        // Populates the line buffer by the next line. 
        // Returns false if no characters were read.
        private bool LoadLine() {
            int size = 0;

            if (_lineBuffer == null) {
                _lineBuffer = new char[InitialBufferSize];
            }

            while (true) {
                int c;
                try {
                    c = _input.Read();
                } catch (DecoderFallbackException e) {
                    ReportError(Errors.InvalidMultibyteCharacter, BitConverter.ToString(e.BytesUnknown).Replace('-', ' '), _encoding.Name);
                    c = -1;
                }

                if (c == -1) {
                    if (size > 0) {
                        if (size < _lineBuffer.Length) {
                            _lineBuffer[size] = '\0';
                        }
                        break;
                    } else {
                        return false;
                    }
                }

                if (size == _lineBuffer.Length) {
                    Array.Resize(ref _lineBuffer, size * 2);
                }
                _lineBuffer[size++] = (char)c;

                if (c == '\n') break;
                if (c == '\r' && _input.Peek() != '\n') break;
            }

            _lineLength = size;
            _bufferPos = 0;
            return true;
        }

        private int Read() {
            if (!RefillBuffer()) {
                return -1;
            }

            Debug.Assert(0 <= _bufferPos && _bufferPos < _lineLength);

            return _lineBuffer[_bufferPos++];
        }

        private bool Read(int c) {
            if (Peek() == c) {
                Skip();
                return true;
            } else {
                return false;
            }
        }

        private void Skip(int c) {
            Debug.Assert(c != -1 && _lineBuffer[_bufferPos] == c);
            _bufferPos += 1;
        }

        private void Skip() {
            _bufferPos += 1;
        }

        private void SeekRelative(int disp) {
            Debug.Assert(_bufferPos + disp >= 0);
            Debug.Assert(_bufferPos + disp <= _lineLength);
            _bufferPos += disp;
        }

        private void Back(int c) {
            if (c != -1) {
                Debug.Assert(_lineBuffer[_bufferPos - 1] == c);
                _bufferPos--;
            } else {
                Debug.Assert(_bufferPos == _lineLength);
            }
        }

        private int Peek() {
            return Peek(0);
        }

        private int Peek(int disp) {
            if (_lineBuffer == null) {
                if (!RefillBuffer()) {
                    return -1;
                }
            }

            if (_bufferPos + disp < _lineLength) {
                return _lineBuffer[_bufferPos + disp];
            }

            return -1;
        }

        private bool RefillBuffer() {
            Debug.Assert(_lineBuffer == null || 0 <= _bufferPos && _bufferPos <= _lineLength);

            if (_lineBuffer == null || _bufferPos == _lineLength) {
                bool wasBufferNull = _lineBuffer == null;
                int oldLineLength = _lineLength;

                // end of stream:
                if (!LoadLine()) {
                    return false;
                }

                // skips lines of heredoc content (only number, real bits has already been read):
                if (_state.HeredocEndLine > 0) {
                    _currentLine = _state.HeredocEndLine;
                    _currentLineIndex = _state.HeredocEndLineIndex;
                    _state.HeredocEndLine = 0;
                    _state.HeredocEndLineIndex = -1;
                } else {

                    // TODO: initial column
                    if (wasBufferNull) {
                        _currentLine = _initialLocation.Line;
                        _currentLineIndex = _initialLocation.Index;
                    } else {
                        _currentLine++;
                        _currentLineIndex += oldLineLength;
                    }
                }
            }

            return true;
        }

        private bool AtEndOfLine {
            get { return _bufferPos == _lineLength && _bufferPos > 0 && _lineBuffer[_bufferPos - 1] == '\n'; }
        }

        private bool is_bol() {
            return _bufferPos == 0;
        }

        private bool was_bol() {
            return _bufferPos == 1;
        }

        private bool LineContentEquals(string str, bool skipWhitespace) {
            int strStart;
            return LineContentEquals(str, skipWhitespace, out strStart);
        }

        private bool LineContentEquals(string str, bool skipWhitespace, out int strStart) {
            int p = 0;
            int n;

            if (skipWhitespace) {
                while (p < _lineLength && IsWhiteSpace(_lineBuffer[p])) {
                    p++;
                }
            }

            strStart = p;
            n = _lineLength - (p + str.Length);
            if (n < 0 || (n > 0 && _lineBuffer[p + str.Length] != '\n' && _lineBuffer[p + str.Length] != '\r')) {
                return false;
            }

            return StringEquals(str, _lineBuffer, p, _lineLength);
        }

        private static bool StringEquals(string/*!*/ str, char[]/*!*/ chars, int offset, int count) {
            if (str.Length > count - offset) {
                return false;
            }
            for (int i = 0; i < str.Length; i++) {
                if (str[i] != chars[offset + i]) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Token Spans

        internal void CaptureTokenSpan() {
            _tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
        }

        private void MarkTokenEnd(bool isMultiLine) {
            if (isMultiLine) {
                MarkMultiLineTokenEnd();
            } else {
                MarkSingleLineTokenEnd();
            }
        }
        
        private void MarkSingleLineTokenEnd() {
            Debug.Assert(_lineBuffer == null || Array.IndexOf(_lineBuffer, '\n', _currentTokenStartIndex, _bufferPos - _currentTokenStartIndex) == -1);
            _currentTokenEnd = GetCurrentLocation();
        }

        private void MarkMultiLineTokenEnd() {
            _currentTokenEnd = GetCurrentLocation();
        }

        private Tokens MarkSingleLineTokenEnd(Tokens token) {
            MarkSingleLineTokenEnd();
            return token;
        }

        private Tokens MarkMultiLineTokenEnd(Tokens token) {
            MarkMultiLineTokenEnd();
            return token;
        }
        
        internal void MarkTokenStart() {
            _currentTokenStart = GetCurrentLocation();
            _currentTokenStartIndex = _bufferPos;
        }

        private SourceLocation GetCurrentLocation() {
            if (_lineBuffer == null) {
                return _initialLocation;
            } else if (AtEndOfLine) {
                return new SourceLocation(_currentLineIndex + _bufferPos, _currentLine + 1, 1);
            } else {
                return new SourceLocation(
                    _currentLineIndex + _bufferPos, 
                    _currentLine, 
                    (_currentLine == _initialLocation.Line ? _initialLocation.Column : 1) + _bufferPos
                );
            }
        }

        private SourceSpan GetCurrentSpan() {
            SourceLocation loc = GetCurrentLocation();
            return new SourceSpan(loc, loc);
        }

        #endregion

        #region Main Tokenization

        public Tokens GetNextToken() {
            if (_input == null) {
                throw new InvalidOperationException("Uninitialized");
            }

            while (true) {
                // TODO:
                RefillBuffer();

                Tokens token = _state.CurrentSequence.TokenizeAndMark(this);
                DumpToken(token);

                // Do not set WhitespaceSeen and CommandMode states as they already were set by FinishVerbatimHeredoc.
                if (token == Tokens.VerbatimHeredocEnd) {
                    // no heredoc can start on the current line:
                    Debug.Assert(_state.VerbatimHeredocQueue == null); 
                    return token;
                }

                //
                // Skip whitespace and set/clear CommandMode for the next token.
                //

                WhitespaceSeen = token == Tokens.Whitespace;
                switch (token) {
                    case Tokens.MultiLineComment:
                    case Tokens.SingleLineComment:
                    case Tokens.EndOfLine:
                    case Tokens.Whitespace:
                        // whitespace doesn't affect CommandMode
                        // ignore the token unless in verbatim mode
                        if (_verbatim) {
                            break;
                        } else {
                            continue;
                        }
                        
                    case Tokens.EndOfFile:
                        _eofReached = true;
                        CommandMode = false;
                        break;

                    case Tokens.NewLine:
                    case Tokens.Semicolon:
                    case Tokens.Do:
                    case Tokens.LeftBlockArgBrace:
                    case Tokens.LeftBlockBrace:
                        CommandMode = true;
                        break;

                    // parser also sets CommandMode = true after 
                    // block_parameters  |...| <<<
                    // method_parameters (...) <<<

                    default:
                        CommandMode = false;
                        break;
                }

                if (_verbatim && _state.VerbatimHeredocQueue != null && AtEndOfLine) {
                    _state.DequeueVerbatimHeredocs();
                }

                return token;
            }
        }

        internal Tokens Tokenize() {
            MarkTokenStart();
            int c = Read();

            switch (c) {
                case '\0':		// null terminates the input
                    // if tokenizer is asked for the next token it returns EOF again:
                    Back('\0');
                    MarkSingleLineTokenEnd();
                    return Tokens.EndOfFile;

                case -1:		// end of stream
                    MarkSingleLineTokenEnd();
                    return Tokens.EndOfFile;

                // whitespace
                case ' ':
                case '\t':
                case '\f':
                    return MarkSingleLineTokenEnd(ReadNonEolnWhiteSpace());

                case '\n':
                    return MarkMultiLineTokenEnd(GetEndOfLineToken());

                case '\r':
                    if (Read('\n')) {
                        return MarkMultiLineTokenEnd(GetEndOfLineToken());
                    } else {
                        return MarkSingleLineTokenEnd(ReadNonEolnWhiteSpace());
                    }

                case '\\':
                    return TokenizeBackslash();

                case '#':
                    return MarkSingleLineTokenEnd(ReadSingleLineComment());

                case '*':
                    return MarkSingleLineTokenEnd(ReadStar());

                case '!':
                    return MarkSingleLineTokenEnd(ReadBang());

                case '=': 
                    if (was_bol() && PeekMultiLineCommentBegin()) {
                        return TokenizeMultiLineComment(true);
                    }

                    return MarkSingleLineTokenEnd(ReadEquals());

                case '<':
                    return TokenizeLessThan();

                case '>':
                    return MarkSingleLineTokenEnd(ReadGreaterThan());

                case '"':
                    return MarkSingleLineTokenEnd(ReadDoubleQuote());

                case '\'':
                    return MarkSingleLineTokenEnd(ReadSingleQuote());

                case '`':
                    return MarkSingleLineTokenEnd(ReadBacktick());

                case '?':
                    return TokenizeQuestionmark();

                case '&':
                    return MarkSingleLineTokenEnd(ReadAmpersand());

                case '|':
                    return MarkSingleLineTokenEnd(ReadPipe());

                case '+':
                    return MarkSingleLineTokenEnd(ReadPlus());

                case '-':
                    return MarkSingleLineTokenEnd(ReadMinus());

                case '.':
                    return MarkSingleLineTokenEnd(ReadDot());

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return MarkSingleLineTokenEnd(ReadUnsignedNumber(c));

                case ':':
                    return MarkSingleLineTokenEnd(ReadColon());

                case '/':
                    return MarkSingleLineTokenEnd(ReadSlash());

                case '^':
                    return MarkSingleLineTokenEnd(ReadCaret());

                case ';':
                    LexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return Tokens.Semicolon;

                case ',':
                    LexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return Tokens.Comma;

                case '~':
                    return MarkSingleLineTokenEnd(ReadTilde());

                case '(':
                    return MarkSingleLineTokenEnd(ReadLeftParenthesis());

                case '[':
                    return MarkSingleLineTokenEnd(ReadLeftBracket());

                case '{':
                    return MarkSingleLineTokenEnd(ReadLeftBrace());

                case ')':
                    return TokenizeClosing(Tokens.RightParenthesis);

                case ']':
                    return TokenizeClosing(Tokens.RightBracket);                    

                case '}':
                    return TokenizeClosingBrace();

                case '%':
                    return TokenizePercent();

                case '$': 
                    return MarkSingleLineTokenEnd(ReadGlobalVariable());

                case '@':
                    return MarkSingleLineTokenEnd(ReadInstanceOrClassVariable());

                case '_':
                    if (was_bol() && LineContentEquals("__END__", false)) {
                        // if tokenizer is asked for the next token it returns EOF again:
                        Back('_');
                        MarkSingleLineTokenEnd();
                        _dataOffset = _currentLineIndex + _lineLength;
                        return Tokens.EndOfFile;
                    }
                    return MarkSingleLineTokenEnd(ReadIdentifier(c));

                default:
                    if (!IsIdentifierInitial(c, _multiByteIdentifier)) {
                        ReportError(Errors.InvalidCharacterInExpression, (char)c);
                        MarkSingleLineTokenEnd();
                        return Tokens.InvalidCharacter;
                    }

                    return MarkSingleLineTokenEnd(ReadIdentifier(c));
            }
        }

        #endregion

        #region End-Of-Line

        private Tokens ReadNonEolnWhiteSpace() {
            while (true) {
                int c = Peek();
                if (c == ' ' || c == '\t' || c == '\f') {
                    Skip(c);
                    continue;
                }
                if (c == '\r' && Peek(1) != '\n') {
                    Skip(c);
                    continue;
                }
                break;
            }

            return Tokens.Whitespace;
        }

        private Tokens GetEndOfLineToken() {
            if (LexicalState == LexicalState.EXPR_BEG ||
                LexicalState == LexicalState.EXPR_FNAME ||
                LexicalState == LexicalState.EXPR_DOT ||
                LexicalState == LexicalState.EXPR_CLASS ||
                LexicalState == LexicalState.EXPR_VALUE) {

                return Tokens.EndOfLine;
            }

            // TODO: don't do this in verbatim mode in heredoc:
            if (!_verbatim || _state.VerbatimHeredocQueue == null) {
                // foo
                //   .bar
                RefillBuffer();
                int start = _bufferPos;
                int c;
                do {
                    c = Read();
                    if (c == '.') {
                        if (Peek() != '.') {
                            _bufferPos = start;
                            return Tokens.EndOfLine;
                        }
                        break;
                    }
                } while (IsWhiteSpace(c));
                _bufferPos = start;
            }

            LexicalState = LexicalState.EXPR_BEG;
            return Tokens.NewLine;    
        }

        private Tokens TokenizeBackslash() {
            // escaped eoln is considered whitespace:
            if (TryReadEndOfLine()) {
                MarkMultiLineTokenEnd();
                return Tokens.Whitespace;
            }

            MarkSingleLineTokenEnd();
            return Tokens.Backslash;
        }

        private Tokens ReadSingleLineComment() {
            while (true) {
                int c = Peek();

                if (c == -1 || c == '\n') {
                    return Tokens.SingleLineComment;
                }

                Skip(c);
            }
        }

        private bool TryReadEndOfLine() {
            int c = Peek();
            if (c == '\n') {
                Skip(c);
                return true;
            }

            if (c == '\r' && Peek(1) == '\n') {
                SeekRelative(2);
                return true;
            }

            return false;
        }

        private int ReadNormalizeEndOfLine() {
            int c = Read();
            
            if (c == '\r' && Peek() == '\n') {
                Skip('\n');
                return '\n';
            }

            return c;
        }

        private int ReadNormalizeEndOfLine(out int eolnWidth) {
            int c = Read();

            if (c == '\r' && Peek() == '\n') {
                Skip('\n');
                eolnWidth = 2;
                return '\n';
            }

            eolnWidth = 1;
            return c;
        }

        #endregion

        #region Identifiers and Keywords

        // Identifiers:
        //   [:alpha:_][:identifier:]+
        // Method names:
        //   [:alpha:_][:identifier:]+[?][^=]
        //   [:alpha:_][:identifier:]+[!][^=]
        //   [:alpha:_][:identifier:]+[=][^=~>]
        //   [:alpha:_][:identifier:]+[=] immediately followed by =>
        // Keywords
        private Tokens ReadIdentifier(int firstCharacter) {
            // the first character already read:
            int start = _bufferPos - 1;
            SkipVariableName();

            // reads token suffix (!, ?, =) and returns the the token kind based upon the suffix:
            Tokens result = ReadIdentifierSuffix(firstCharacter);

            // TODO: possible optimization: ~15% are keywords, ~15% are existing local variables -> we can save allocations
            string identifier = new String(_lineBuffer, start, _bufferPos - start);

            // 1.9 label:
            if (InArgs || LexicalState == LexicalState.EXPR_BEG && !CommandMode) {
                if (Peek(0) == ':' && Peek(1) != ':') {
                    Skip(':');
                    LexicalState = LexicalState.EXPR_BEG;
                    SetStringToken(identifier);
                    return Tokens.Label;
                }
            }

            // keyword:
            if (LexicalState != LexicalState.EXPR_DOT) {
                if (LexicalState == LexicalState.EXPR_FNAME) {
                    SetStringToken(identifier);
                }

                Tokens keyword = StringToKeyword(identifier);
                if (keyword != Tokens.None) {
                    return keyword;
                }
            }

            if (IsBeginLexicalState || InArgs || LexicalState == LexicalState.EXPR_DOT) {
                // TODO: test LexState == DOT!!
                if (LexicalState != LexicalState.EXPR_DOT && _localVariableResolver.IsLocalVariable(identifier)) {
                    LexicalState = LexicalState.EXPR_END;
                } else if (CommandMode) {
                    LexicalState = LexicalState.EXPR_CMDARG;
                } else {
                    LexicalState = LexicalState.EXPR_ARG;
                }
            } else if (LexicalState == LexicalState.EXPR_FNAME) {
                LexicalState = LexicalState.EXPR_ENDFN;
            } else {
                LexicalState = LexicalState.EXPR_END;
            }

            SetStringToken(identifier);
            return result;
        }

        private Tokens ReadIdentifierSuffix(int firstCharacter) {
            int suffix = Peek(0);
            int c = Peek(1);
            if ((suffix == '!' || suffix == '?') && c != '=') {
                Skip(suffix);
                return Tokens.FunctionIdentifier;
            }

            if (LexicalState == LexicalState.EXPR_FNAME &&
                suffix == '=' && c != '~' && c != '>' && (c != '=' || Peek(2) == '>')) {
                // include '=' into the token:
                Skip(suffix);
                // TODO: FunctionIdentifier might be better, seems to not matter because the rules that use it accept FtnIdf as well
                return Tokens.Identifier;  
            }

            // no suffix:
            return IsUpperLetter(firstCharacter) ? Tokens.ConstantIdentifier : Tokens.Identifier;
        }

        private Tokens StringToKeyword(string/*!*/ identifier) {
            switch (identifier) {
                case "if": return ReturnKeyword(Tokens.If, Tokens.IfMod, LexicalState.EXPR_BEG);
                case "in": return ReturnKeyword(Tokens.In, LexicalState.EXPR_BEG);
                case "do": return ReturnDoKeyword();
                case "or": return ReturnKeyword(Tokens.Or, LexicalState.EXPR_BEG);

                case "and": return ReturnKeyword(Tokens.And, LexicalState.EXPR_BEG);
                case "end": return ReturnKeyword(Tokens.End, LexicalState.EXPR_END);
                case "def": return ReturnKeyword(Tokens.Def, LexicalState.EXPR_FNAME);
                case "for": return ReturnKeyword(Tokens.For, LexicalState.EXPR_BEG);
                case "not": return ReturnKeyword(Tokens.Not, LexicalState.EXPR_ARG);
                case "nil": return ReturnKeyword(Tokens.Nil, LexicalState.EXPR_END);
                case "END": return ReturnKeyword(Tokens.UppercaseEnd, LexicalState.EXPR_END);

                case "else": return ReturnKeyword(Tokens.Else, LexicalState.EXPR_BEG);
                case "then": return ReturnKeyword(Tokens.Then, LexicalState.EXPR_BEG);
                case "case": return ReturnKeyword(Tokens.Case, LexicalState.EXPR_BEG);
                case "self": return ReturnKeyword(Tokens.Self, LexicalState.EXPR_END);
                case "true": return ReturnKeyword(Tokens.True, LexicalState.EXPR_END);
                case "next": return ReturnKeyword(Tokens.Next, LexicalState.EXPR_MID);
                case "when": return ReturnKeyword(Tokens.When, LexicalState.EXPR_BEG);
                case "redo": return ReturnKeyword(Tokens.Redo, LexicalState.EXPR_END);

                case "alias": return ReturnKeyword(Tokens.Alias, LexicalState.EXPR_FNAME);
                case "begin": return ReturnKeyword(Tokens.Begin, LexicalState.EXPR_BEG);
                case "break": return ReturnKeyword(Tokens.Break, LexicalState.EXPR_MID);
                case "BEGIN": return ReturnKeyword(Tokens.UppercaseBegin, LexicalState.EXPR_END);
                case "class": return ReturnKeyword(Tokens.Class, LexicalState.EXPR_CLASS);
                case "elsif": return ReturnKeyword(Tokens.Elsif, LexicalState.EXPR_BEG);
                case "false": return ReturnKeyword(Tokens.False, LexicalState.EXPR_END);
                case "retry": return ReturnKeyword(Tokens.Retry, LexicalState.EXPR_END);
                case "super": return ReturnKeyword(Tokens.Super, LexicalState.EXPR_ARG);
                case "until": return ReturnKeyword(Tokens.Until, Tokens.UntilMod, LexicalState.EXPR_BEG);
                case "undef": return ReturnKeyword(Tokens.Undef, LexicalState.EXPR_FNAME);
                case "while": return ReturnKeyword(Tokens.While, Tokens.WhileMod, LexicalState.EXPR_BEG);
                case "yield": return ReturnKeyword(Tokens.Yield, LexicalState.EXPR_ARG);

                case "ensure": return ReturnKeyword(Tokens.Ensure, LexicalState.EXPR_BEG);
                case "module": return ReturnKeyword(Tokens.Module, LexicalState.EXPR_BEG);
                case "rescue": return ReturnKeyword(Tokens.Rescue, Tokens.RescueMod, LexicalState.EXPR_MID);
                case "return": return ReturnKeyword(Tokens.Return, LexicalState.EXPR_MID);
                case "unless": return ReturnKeyword(Tokens.Unless, Tokens.UnlessMod, LexicalState.EXPR_BEG);

                case "defined?": return ReturnKeyword(Tokens.Defined, LexicalState.EXPR_ARG);
                case "__LINE__": return ReturnKeyword(Tokens.Line, LexicalState.EXPR_END);
                case "__FILE__": return ReturnKeyword(Tokens.File, LexicalState.EXPR_END);
                case "__ENCODING__":
                    return ReturnKeyword(Tokens.Encoding, LexicalState.EXPR_END);

                default: return Tokens.None;
            }
        }

        private Tokens ReturnKeyword(Tokens keyword, LexicalState state) {
            LexicalState = state;
            return keyword;
        }

        private Tokens ReturnKeyword(Tokens keywordInExpression, Tokens keywordModifier, LexicalState state) {
            Debug.Assert(keywordInExpression != keywordModifier);

            if (LexicalState == LexicalState.EXPR_BEG || LexicalState == LexicalState.EXPR_VALUE) {
                LexicalState = state;
                return keywordInExpression;
            } else {
                LexicalState = LexicalState.EXPR_BEG;
                return keywordModifier;
            }
        }

        private Tokens ReturnDoKeyword() {
            // 1.9 lambda
            if (_lambdaOpenings > 0 && _lambdaOpenings == _openingCount) {
                _lambdaOpenings = 0;
                _openingCount--;
                return Tokens.LambdaDo;
            }
            
            LexicalState oldState = LexicalState;
            LexicalState = LexicalState.EXPR_BEG;
            // if last conditional opening is a parenthesis:
            if (InLoopCondition) {
                return Tokens.LoopDo;
            }

            if (InCommandArgs && oldState != LexicalState.EXPR_CMDARG || oldState == LexicalState.EXPR_ENDARG || oldState == LexicalState.EXPR_BEG) {
                return Tokens.BlockDo;
            }

            return Tokens.Do;
        }      
  
        #endregion

        #region Comments

        // =begin 
        // ...
        // =end
        internal Tokens TokenizeMultiLineComment(bool started) {
            while (true) {
                // skip current line:
                if (started) {
                    _bufferPos = _lineLength;
                } else {
                    started = true;
                }

                // read the first character of the next line:
                int c = Read();
                if (c == -1) {
                    _unterminatedToken = true;
                    if (_verbatim) {
                        _state.CurrentSequence = MultiLineCommentState.Instance;
                        MarkMultiLineTokenEnd();
                        return _currentTokenStart.Index == _currentTokenEnd.Index ? Tokens.EndOfFile : Tokens.MultiLineComment;
                    } else {
                        ReportError(Errors.UnterminatedEmbeddedDocument);
                        break;
                    }
                }
                
                if (c != '=') {
                    continue;
                }

                if (PeekMultiLineCommentEnd()) {
                    break;
                }
            }

            _state.CurrentSequence = TokenSequenceState.None;
            _bufferPos = _lineLength;
            MarkMultiLineTokenEnd();
            return Tokens.MultiLineComment;
        }

        private bool PeekMultiLineCommentBegin() {
            int minLength = _bufferPos + 5;
            return minLength <= _lineLength && 
                _lineBuffer[_bufferPos + 0] == 'b' &&
                _lineBuffer[_bufferPos + 1] == 'e' &&
                _lineBuffer[_bufferPos + 2] == 'g' &&
                _lineBuffer[_bufferPos + 3] == 'i' &&
                _lineBuffer[_bufferPos + 4] == 'n' &&
                (minLength == _lineLength || IsWhiteSpace(_lineBuffer[minLength]));
        }

        private bool PeekMultiLineCommentEnd() {
            int minLength = _bufferPos + 3;
            return minLength <= _lineLength && 
                _lineBuffer[_bufferPos + 0] == 'e' &&
                _lineBuffer[_bufferPos + 1] == 'n' &&
                _lineBuffer[_bufferPos + 2] == 'd' &&
                (minLength == _lineLength || IsWhiteSpace(_lineBuffer[minLength]));
        }

        #endregion

        #region Tokens

        // Assignment: =
        // Operators: == === =~ =>
        private Tokens ReadEquals() {
            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            switch (Peek()) {
                case '=':
                    Skip('=');
                    return Read('=') ? Tokens.StrictEqual : Tokens.Equal;

                case '~':
                    Skip('~');
                    return Tokens.Match;

                case '>':
                    Skip('>');
                    return Tokens.DoubleArrow;

                default:
                    return Tokens.Assignment;
            }
        }

        // Operators: + +@
        // Assignments: +=
        // Literals: +[:number:]
        private Tokens ReadPlus() {
            int c = Peek();
            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                
                LexicalState = LexicalState.EXPR_ARG;
                if (c == '@') {
                    Skip('@');
                    return Tokens.UnaryPlus;
                }

                return Tokens.Plus;
            }

            if (c == '=') {
                Skip('=');
                SetAsciiStringToken(Symbols.Plus);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            }

            bool nospace = false;
            if (IsBeginLexicalState || (nospace = InArgsNoSpace(c))) {
                if (nospace) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                }

                LexicalState = LexicalState.EXPR_BEG;
                if (IsDecimalDigit(c)) {
                    Skip(c);
                    return ReadUnsignedNumber(c);
                }

                return Tokens.UnaryPlus;
            }

            WarnBalanced("+", "unary operator");
            LexicalState = LexicalState.EXPR_BEG;
            return Tokens.Plus;
        }

        // Parentheses: (
        private Tokens ReadLeftParenthesis() {
            Tokens result = Tokens.LeftParenthesis;
            
            if (IsBeginLexicalState) {
                result = Tokens.LeftExprParenthesis;
            } else if (InArgs && WhitespaceSeen) {
                result = Tokens.LeftArgParenthesis;
            }

            EnterParenthesisedExpression();
            LexicalState = LexicalState.EXPR_BEG;
            return result;
        }

        // Instance variables:
        //   @[:alpha:_][:identifier:]*
        // Class variables:
        //   @@[:alpha:_][:identifier:]*
        // At:
        //   @
        private Tokens ReadInstanceOrClassVariable() {
            Tokens result;

            // start right before @/@@, the resulting symbol starts with @/@@
            int start = _bufferPos - 1;

            int c = Peek(0);
            if (c == '@') {
                c = Peek(1);
                result = Tokens.ClassVariable;
            } else {
                result = Tokens.InstanceVariable;
            }

            // c follows @ or @@
            if (IsDecimalDigit(c)) {
                ReportError(result == Tokens.InstanceVariable ? Errors.InvalidInstanceVariableName : Errors.InvalidClassVariableName, (char)c);
            } else if (IsIdentifierInitial(c)) {
                if (result == Tokens.ClassVariable) {
                    Skip('@');
                }
                Skip(c);

                SkipVariableName();
                SetStringToken(start, _bufferPos - start);
                LexicalState = LexicalState.EXPR_END;
                return result;
            }

            return Tokens.At;
        }

        // Global variables: 
        //   $[_~*$?!@/\;,.=:<>"] 
        //   $-[:identifier:] 
        //   $[:identifier:]
        // Match references: 
        //   $[&`'+] 
        //   $[1-9][0-9]+
        // Dollar:
        //   $
        private Tokens ReadGlobalVariable() {
            LexicalState = LexicalState.EXPR_END;

            // start right after $, the resulting symbol doesn't contain $
            int start = _bufferPos;
            
            int c = Read();
            switch (c) {
                case '_':
                    if (IsIdentifier(Peek())) {
                        SkipVariableName();
                        SetStringToken(start, _bufferPos - start);
                        return Tokens.GlobalVariable;
                    }
                    return GlobalVariableToken(Symbols.LastInputLine);

                // exceptions:
                case '!': return GlobalVariableToken(Symbols.CurrentException);
                case '@': return GlobalVariableToken(Symbols.CurrentExceptionBacktrace);

                // options:
                case '-':
                    if (IsIdentifier(Peek())) {
                        Read();
                        SetStringToken(start, 2);
                    } else {
                        SetAsciiStringToken("-");
                    }
                    return Tokens.GlobalVariable;

                // others:
                case ',': return GlobalVariableToken(Symbols.ItemSeparator);
                case ';': return GlobalVariableToken(Symbols.StringSeparator);
                case '/': return GlobalVariableToken(Symbols.InputSeparator);
                case '\\': return GlobalVariableToken(Symbols.OutputSeparator);
                case '*': return GlobalVariableToken(Symbols.CommandLineArguments);
                case '$': return GlobalVariableToken(Symbols.CurrentProcessId);
                case '?': return GlobalVariableToken(Symbols.ChildProcessExitStatus);
                case '=': return GlobalVariableToken(Symbols.IgnoreCaseComparator);
                case ':': return GlobalVariableToken(Symbols.LoadPath);
                case '"': return GlobalVariableToken(Symbols.LoadedFiles);
                case '<': return GlobalVariableToken(Symbols.InputContent);
                case '>': return GlobalVariableToken(Symbols.OutputStream);
                case '.': return GlobalVariableToken(Symbols.LastInputLineNumber);

                // regex:
                case '~': 
                    return GlobalVariableToken(Symbols.MatchData);
                
                case '&':
                    _tokenValue.SetInteger(RegexMatchReference.EntireMatch);
                    return Tokens.MatchReference;

                case '`':
                    _tokenValue.SetInteger(RegexMatchReference.PreMatch);
                    return Tokens.MatchReference;

                case '\'':		
                    _tokenValue.SetInteger(RegexMatchReference.PostMatch);
                    return Tokens.MatchReference;

                case '+':
                    _tokenValue.SetInteger(RegexMatchReference.MatchLastGroup);
                    return Tokens.MatchReference;

                case '0':
                    if (IsIdentifier(Peek())) {
                        // $0[A-Za-z0-9_] are invalid:
                        SkipVariableName();
                        ReportError(Errors.InvalidGlobalVariableName, new String(_lineBuffer, start - 1, _bufferPos - start));
                        SetAsciiStringToken(Symbols.ErrorVariable);
                        return Tokens.GlobalVariable;
                    }

                    return GlobalVariableToken(Symbols.CommandLineProgramPath);

                default:
                    if (IsDecimalDigit(c)) {
                        return ReadMatchGroupReferenceVariable(c);
                    }

                    if (IsIdentifier(c)) {
                        SkipVariableName();
                        SetStringToken(start, _bufferPos - start);
                        return Tokens.GlobalVariable;
                    }

                    Back(c);
                    return Tokens.Dollar;
            }
        }

        private Tokens ReadMatchGroupReferenceVariable(int c) {
            int start = _bufferPos - 1;
            int value = c - '0';
            bool overflow = false;

            while (true) {
                c = Peek();

                if (!IsDecimalDigit(c)) {
                    break;
                }

                Skip(c);
                value = unchecked(value * 10 + (c - '0'));
                overflow |= (value < 0);
            }

            if (overflow) {
                ReportError(Errors.MatchGroupReferenceOverflow, new String(_lineBuffer, start, _bufferPos - start));
            }

            _tokenValue.SetInteger(value);
            return Tokens.MatchReference;
        }

        private Tokens GlobalVariableToken(string/*!*/ symbol) {
            SetAsciiStringToken(symbol);
            return Tokens.GlobalVariable;
        }

        // Assignments: %=
        // Operators: % 
        // Literals: %{... (quotation start)
        private Tokens TokenizePercent() {
            if (IsBeginLexicalState) {
                return TokenizeQuotationStart();
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Mod);
                LexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return Tokens.OpAssignment;
            }

            if (InArgsNoSpace(c)) {
                return TokenizeQuotationStart();
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            WarnBalanced("%%", "string literal");
            MarkSingleLineTokenEnd();
            return Tokens.Percent;
        }

        // Closing tokens: ), ], }
        private Tokens TokenizeClosing(Tokens token) {
            LeaveParenthesisedExpression();
            LexicalState = token == Tokens.RightParenthesis ? LexicalState.EXPR_ENDFN : LexicalState.EXPR_ENDARG; // 1.8: EXPR_END (all cases)
            MarkSingleLineTokenEnd();
            return token;
        }

        // Closing tokens: }, StringEmbeddedCodeEnd
        private Tokens TokenizeClosingBrace() {
            if (_state.OpenedBracesInEmbeddedCode > 0) {
                _state.OpenedBracesInEmbeddedCode--;
                return TokenizeClosing(Tokens.RightBrace);
            }
            
            if (InStringEmbeddedCode) {
                _state.EndStringEmbeddedCode();
                return TokenizeClosing(Tokens.StringEmbeddedCodeEnd);
            } 
            
            // unmatched brace:
            return TokenizeClosing(Tokens.RightBrace);
        }

        // Braces: {
        private Tokens ReadLeftBrace() {
            Tokens result;

            if (_lambdaOpenings > 0 && _lambdaOpenings == _openingCount) {
                result = Tokens.LeftLambdaBrace;     // 1.9 lambda body opening
                _lambdaOpenings = 0;
                _openingCount--;
            } else if (InArgs || LexicalState == LexicalState.EXPR_END || LexicalState == LexicalState.EXPR_ENDFN) {
                result = Tokens.LeftBlockBrace;      // block (primary)
            } else if (LexicalState == LexicalState.EXPR_ENDARG) {
                result = Tokens.LeftBlockArgBrace;   // block (expr)
            } else {
                result = Tokens.LeftBrace;           // hash
            }

            _state.OpenedBracesInEmbeddedCode++;
            EnterParenthesisedExpression();
            LexicalState = LexicalState.EXPR_BEG;
            return result;
        }

        // Brackets: [
        // Operators: [] []=
        private Tokens ReadLeftBracket() {
            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                LexicalState = LexicalState.EXPR_ARG;

                return Read(']') ? (Read('=') ? Tokens.ItemSetter : Tokens.ItemGetter) : Tokens.LeftIndexingBracket;
            }

            Tokens result;
            if (IsBeginLexicalState) {
                result = Tokens.LeftBracket;
            } else if (InArgs && WhitespaceSeen) {
                result = Tokens.LeftBracket;
            } else {
                result = Tokens.LeftIndexingBracket;
            }

            LexicalState = LexicalState.EXPR_BEG;
            EnterParenthesisedExpression();
            return result;
        }

        // Operators: ~ ~@
        private Tokens ReadTilde() {
            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                // ~@
                Read('@');
                LexicalState = LexicalState.EXPR_ARG;
            } else {
                LexicalState = LexicalState.EXPR_BEG; 
            }

            return Tokens.Tilde;
        }

        // Assignments: ^=
        // Operators: ^
        private Tokens ReadCaret() {
            if (Read('=')) {
                SetAsciiStringToken(Symbols.Xor);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            return Tokens.Caret;
        }

        // Operators: /
        // Assignments: /=
        // Literals: /... (regex start)
        private Tokens ReadSlash() {
            if (IsBeginLexicalState) {
                _state.CurrentSequence = new StringState(StringProperties.RegularExpression | StringProperties.ExpandsEmbedded, '/');
                return Tokens.RegexpBegin;
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Divide);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            }

            if (InArgsNoSpace(c)) {
                ReportWarning(Errors.AmbiguousFirstArgument);
                _state.CurrentSequence = new StringState(StringProperties.RegularExpression | StringProperties.ExpandsEmbedded, '/');
                return Tokens.RegexpBegin;
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            WarnBalanced("/", "regexp literal");
            return Tokens.Slash;
        }

        // Operators: :: : 
        // Literals: :... (symbol start)
        private Tokens ReadColon() {
            int c = Peek();
            if (c == ':') {
                Skip(c);
                if (IsBeginLexicalState || InArgs && WhitespaceSeen) {
                    LexicalState = LexicalState.EXPR_BEG;
                    return Tokens.LeadingDoubleColon;
                }

                LexicalState = LexicalState.EXPR_DOT;
                return Tokens.SeparatingDoubleColon;
            }

            if (IsEndLexicalState || IsWhiteSpace(c)) {
                WarnBalanced(":", "symbol literal");
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.Colon;
            }

            switch (c) {
                case '\'':
                    Skip(c);
                    _state.CurrentSequence = new StringState(StringProperties.Symbol, '\'');
                    break;

                case '"':
                    Skip(c);
                    _state.CurrentSequence = new StringState(StringProperties.Symbol | StringProperties.ExpandsEmbedded, '"');
                    break;

                default:
                    Debug.Assert(_state.CurrentSequence == TokenSequenceState.None);
                    break;
            }

            LexicalState = LexicalState.EXPR_FNAME;
            return Tokens.SymbolBegin;
        }

        // Assignments: **= *= 
        // Operators: ** * splat
        private Tokens ReadStar() {
            Tokens result;

            int c = Peek();
            if (c == '*') {
                Skip(c);
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.Power);
                    LexicalState = LexicalState.EXPR_BEG;
                    
                    return Tokens.OpAssignment;
                }

                result = Tokens.Pow;
            } else if (c == '=') {
                Skip(c);

                SetAsciiStringToken(Symbols.Multiply);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            } else if (InArgsNoSpace(c)) {
                ReportWarning(Errors.StarInterpretedAsSplatArgument);
                result = Tokens.Star;
            } else if (IsBeginLexicalState) {
                result = Tokens.Star;
            } else {
                WarnBalanced("*", "argument prefix");
                result = Tokens.Asterisk;
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG;
                    break;
            }

            return result;
        }

        // Operators: ! != !~ !@
        private Tokens ReadBang() {
            int c = Peek();
            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                LexicalState = LexicalState.EXPR_ARG;
                if (c == '@') {
                    Skip(c);
                    return Tokens.Bang;
                }
            } else {
                LexicalState = LexicalState.EXPR_BEG;
            }
            
            if (c == '=') {
                Skip(c);
                return Tokens.NotEqual;
            } 
            
            if (c == '~') {
                Skip(c);
                return Tokens.Nmatch;
            }

            return Tokens.Bang;
        }

        // String: <<HEREDOC_LABEL
        // Assignment: <<=
        // Operators: << <= <=> <
        private Tokens TokenizeLessThan() {
            int c = Read();

            if (c == '<' &&
                LexicalState != LexicalState.EXPR_DOT &&
                LexicalState != LexicalState.EXPR_CLASS && 
                !IsEndLexicalState &&
                (!InArgs || WhitespaceSeen)) {

                Tokens token = TokenizeHeredocLabel();
                if (token != Tokens.None) {
                    return token;
                }
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            if (c == '=') {
                if (Read('>')) {
                    MarkSingleLineTokenEnd();
                    return Tokens.Cmp;
                }
                MarkSingleLineTokenEnd();
                return Tokens.LessOrEqual;
            }

            if (c == '<') {
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.LeftShift);
                    LexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return Tokens.OpAssignment;
                }
                WarnBalanced("<<", "here document");
                MarkSingleLineTokenEnd();
                return Tokens.Lshft;
            }

            Back(c);
            MarkSingleLineTokenEnd();
            return Tokens.Less;
        }

        // Assignment: >>=
        // Operators: > >= >>
        private Tokens ReadGreaterThan() {
            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                return Tokens.GreaterOrEqual;
            }

            if (c == '>') {
                Skip(c);
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.RightShift);
                    LexicalState = LexicalState.EXPR_BEG;
                    return Tokens.OpAssignment;
                }
                return Tokens.Rshft;
            }

            return Tokens.Greater;
        }

        // String: `...
        // Operator: `
        private Tokens ReadBacktick() {
            if (LexicalState == LexicalState.EXPR_FNAME) {
                LexicalState = LexicalState.EXPR_ENDFN;   // 1.8 : EXPR_END
                return Tokens.Backtick;
            }

            if (LexicalState == LexicalState.EXPR_DOT) {
                // This used to check if we are in command. There seems to be no way how we could get there.
                // The lexical state is EXPR_BEG after CommandMode is set for the next non-whitespace token. Whitespace tokens don't change the state.
                // The lexical state is EXPR_DOT after Token.SeparatingDoubleColon and Token.Dot none of which change the command state.
                // LexicalState = (_commandMode) ? LexicalState.EXPR_CMDARG : LexicalState.EXPR_ARG;
                Debug.Assert(!CommandMode);
                LexicalState = LexicalState.EXPR_ARG;
                return Tokens.Backtick;
            }

            _state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded, '`');
            return Tokens.ShellStringBegin;
        }

        // Operators: ? (conditional)
        // Literals: ?[:char:] ?{escape}
        // Errors: ?[:EOF:]
        private Tokens TokenizeQuestionmark() {
            if (IsEndLexicalState) {
                LexicalState = LexicalState.EXPR_VALUE; // 1.8: EXPR_BEG
                MarkSingleLineTokenEnd();
                return Tokens.QuestionMark;
            }

            // ?[:EOF:]
            int c = Peek();
            if (c == -1) {
                _unterminatedToken = true;
                MarkSingleLineTokenEnd();
                ReportError(Errors.IncompleteCharacter);
                return Tokens.EndOfFile;
            }

            // ?[:whitespace:]
            if (IsWhiteSpace(c)) {
                if (!InArgs) {
                    int c2 = 0;
                    switch (c) {
                        case ' ': c2 = 's'; break;
                        case '\n': c2 = 'n'; break;
                        case '\t': c2 = 't'; break;
                        case '\v': c2 = 'v'; break;
                        case '\r': c2 = (Peek(1) == '\n') ? 'n' : 'r'; break;
                        case '\f': c2 = 'f'; break;
                    }

                    if (c2 != 0) {
                        ReportWarning(Errors.InvalidCharacterSyntax, (char)c2);
                    }
                }
                LexicalState = LexicalState.EXPR_VALUE; // 1.8: EXPR_BEG
                MarkSingleLineTokenEnd();
                return Tokens.QuestionMark;
            }
            
            // ?{identifier}
            if ((IsLetterOrDigit(c) || c == '_') && IsIdentifier(Peek(1))) {
                LexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return Tokens.QuestionMark;
            }

            Skip(c);

            object content;
            RubyEncoding encoding;

            // ?\{escape}
            if (c == '\\') {
                c = Peek();
                if (c == 'u') {
                    // \uFFFF, \u{xxxxxx}
                    Skip(c);
                    int codepoint = Peek() == '{' ? ReadUnicodeEscape6() : ReadUnicodeEscape4();
                    content = UnicodeCodePointToString(codepoint);
                    encoding = (codepoint <= 0x7f) ? _encoding : RubyEncoding.UTF8;
                    MarkSingleLineTokenEnd();
                } else {
                    // \xXX, \M-x, ...
                    c = ReadEscape();
                    if (c <= 0x7f) {
                        content = new String((char)c, 1);
                        encoding = _encoding;
                    } else {
                        Debug.Assert(c <= 0xff);
                        content = new[] { (byte)c };
                        encoding = (_encoding == RubyEncoding.Ascii) ? RubyEncoding.Binary : _encoding;
                    }

                    // \M-{eoln} eats the eoln:
                    MarkMultiLineTokenEnd();
                }
            } else {
                int d;
                if (IsHighSurrogate(c) && IsLowSurrogate(d = Peek())) {
                    Skip(d);
                    content = new String(new[] { (char)c, (char)d });
                } else {
                    content = new String((char)c, 1);
                }
                encoding = _encoding;
                MarkSingleLineTokenEnd();
            }

            LexicalState = LexicalState.EXPR_END;
            _tokenValue.StringContent = content;
            _tokenValue.Encoding = encoding;
            return Tokens.Character;
        }

        // Operators: & &&
        // Assignments: &=
        private Tokens ReadAmpersand() {
            int c = Peek();
            
            if (c == '&') {
                Skip(c);
                LexicalState = LexicalState.EXPR_BEG;
                
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.And);
                    return Tokens.OpAssignment;
                }

                return Tokens.LogicalAnd;
            } 
            
            if (c == '=') {
                Skip(c);
                LexicalState = LexicalState.EXPR_BEG;
                SetAsciiStringToken(Symbols.BitwiseAnd);
                return Tokens.OpAssignment;
            }

            Tokens result;
            if (InArgsNoSpace(c)) {
                // we are in command argument and there is a whitespace between ampersand: "foo &bar"
                ReportWarning(Errors.AmpersandInterpretedAsProcArgument);
                result = Tokens.BlockReference;
            } else if (IsBeginLexicalState) {
                result = Tokens.BlockReference;
            } else {
                WarnBalanced("&", "argument prefix");
                result = Tokens.Ampersand;
            }

            switch (LexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    LexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    LexicalState = LexicalState.EXPR_BEG;
                    break;
            }

            return result;
        }

        // Operators: | ||
        // Assignments: |= ||=
        private Tokens ReadPipe() {
            int c = Peek();

            if (c == '|') {
                Skip(c);
                LexicalState = LexicalState.EXPR_BEG;

                if (Read('=')) {
                    SetAsciiStringToken(Symbols.Or);
                    LexicalState = LexicalState.EXPR_BEG;
                    return Tokens.OpAssignment;
                }
                return Tokens.LogicalOr;
            }

            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.BitwiseOr);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            }

            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                LexicalState = LexicalState.EXPR_ARG;
            } else {
                LexicalState = LexicalState.EXPR_BEG;
            }

            return Tokens.Pipe;
        }

        // Operators: . .. ...
        // Errors: .[:digit:]
        private Tokens ReadDot() {
            LexicalState = LexicalState.EXPR_BEG;
            
            int c = Peek();
            if (c == '.') {
                Skip(c);
                return Read('.') ? Tokens.TripleDot : Tokens.DoubleDot;
            }

            if (IsDecimalDigit(c)) {
                ReportError(Errors.NoFloatingLiteral);
            }

            LexicalState = LexicalState.EXPR_DOT;
            return Tokens.Dot;
        }

        // Operators: - -@ ->
        // Assignments: -=
        // Literals: -... (negative number sign)
        private Tokens ReadMinus() {
            if (LexicalState == LexicalState.EXPR_FNAME || LexicalState == LexicalState.EXPR_DOT) {
                LexicalState = LexicalState.EXPR_ARG;
                return Read('@') ? Tokens.UnaryMinus : Tokens.Minus;
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Minus);
                LexicalState = LexicalState.EXPR_BEG;
                return Tokens.OpAssignment;
            }

            if (c == '>') {
                Skip(c);
                LexicalState = LexicalState.EXPR_ARG;
                return Tokens.Lambda;
            }

            bool nospace = false;
            if (IsBeginLexicalState || (nospace = InArgsNoSpace(c))) {
                if (nospace) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                }

                LexicalState = LexicalState.EXPR_BEG;
                return IsDecimalDigit(c) ? Tokens.NumberNegation : Tokens.UnaryMinus;
            }

            LexicalState = LexicalState.EXPR_BEG;
            WarnBalanced("-", "unary operator");
            return Tokens.Minus;
        }

        // Reads
        //   [:letter:]*
        // and converts it to RegEx options.
        private RubyRegexOptions ReadRegexOptions() {
            RubyRegexOptions encoding = 0;
            RubyRegexOptions options = 0;

            while (true) {
                int c = Peek();
                if (!IsLetter(c)) {
                    break;
                }

                Skip(c);
                switch (c) {
                    case 'i': options |= RubyRegexOptions.IgnoreCase; break;
                    case 'x': options |= RubyRegexOptions.Extended; break;
                    case 'm': options |= RubyRegexOptions.Multiline; break;
                    case 'o': options |= RubyRegexOptions.Once; break;

                    case 'n': encoding = RubyRegexOptions.FIXED; break;
                    case 'e': encoding = RubyRegexOptions.EUC; break;
                    case 's': encoding = RubyRegexOptions.SJIS; break;
                    case 'u': encoding = RubyRegexOptions.UTF8; break;

                    default:
                        ReportError(Errors.UnknownRegexOption, (char)c);
                        break;
                }
            }

            return options | encoding;
        }

        #endregion

        #region Character Escapes

        // \\ \n \t \r \f \v \a \b \s 
        // \[:octal:] \x[:hexa:] \M-\[:escape:] \M-[:char:] \C-[:escape:] \C-[:char:] \c[:escape:] \c[:char:] \[:char:]
        private int ReadEscape() {
            int c = Read();
            switch (c) {
                case '\\': return '\\';
                case 'n': return '\n';
                case 't': return '\t';
                case 'r': return '\r';
                case 'f': return '\f';
                case 'v': return '\v';
                case 'a': return '\a';
                case 'e': return 27;
                case 'b': return '\b';
                case 's': return ' ';

                case 'x': return ReadHexEscape();

                case 'M':
                    if (!Read('-')) {
                        return InvalidEscapeCharacter();
                    }

                    c = ReadNormalizeEndOfLine();
                    if (c == '\\') {
                        return ReadEscape() | 0x80;
                    }

                    if (c == -1) {
                        return InvalidEscapeCharacter();                        
                    }

                    return (c & 0xff) | 0x80;

                case 'C':
                    if (!Read('-')) {
                        return InvalidEscapeCharacter();                        
                    }
                    goto case 'c';

                case 'c':
                    c = ReadNormalizeEndOfLine();

                    if (c == '?') {
                        return 0177;
                    }
                
                    if (c == -1) {
                        return InvalidEscapeCharacter();                        
                    }    
                
                    if (c == '\\') {
                        c = ReadEscape();
                    }

                    return c & 0x9f;

                case -1:
                    return InvalidEscapeCharacter();

                default:
                    if (IsOctalDigit(c)) {
                        return ReadOctalEscape(c - '0');
                    }

                    // ReadEscape is not called if the backslash is followed by an eoln:
                    Debug.Assert(c != '\n' && (c != '\r' || Peek() != '\n'));
                    return c;
            }
        }

        private int InvalidEscapeCharacter() {
            ReportError(Errors.InvalidEscapeCharacter);
            // return != 0 so that additional errors (\0 in a symbol) are not invoked
            return '?';
        }
        
        // Appends escaped regex escape sequence.
        private void AppendEscapedRegexEscape(MutableStringBuilder/*!*/ content, int term) {
            int c = Read();

            switch (c) {
                case 'x':
                    content.AppendAscii('\\');
                    AppendEscapedHexEscape(content);
                    break;

                case 'M':
                    if (!Read('-')) {
                        InvalidEscapeCharacter();
                        break;
                    }

                    content.AppendAscii('\\');
                    content.AppendAscii('M');
                    content.AppendAscii('-');

                    // escaped:
                    AppendRegularExpressionCompositeEscape(content, term);
                    break;                    

                case 'C':
                    if (!Read('-')) {
                        InvalidEscapeCharacter();
                        break;
                    }

                    content.AppendAscii('\\');
                    content.AppendAscii('C');
                    content.AppendAscii('-');
                    
                    AppendRegularExpressionCompositeEscape(content, term);
                    break;

                case 'c':
                    content.AppendAscii('\\');
                    content.AppendAscii('c');
                    AppendRegularExpressionCompositeEscape(content, term);
                    break;
                    
                case -1:
                    InvalidEscapeCharacter();
                    break;

                default:
                    if (IsOctalDigit(c)) {
                        content.AppendAscii('\\');
                        AppendEscapedOctalEscape(content);
                        break;
                    }

                    if (c != '\\' || c != term) {
                        content.AppendAscii('\\');
                    }

                    // ReadEscape is not called if the backslash is followed by an eoln:
                    Debug.Assert(c != '\n' && (c != '\r' || Peek() != '\n'));
                    AppendCharacter(content, c);
                    break;
            }
        }

        private void AppendRegularExpressionCompositeEscape(MutableStringBuilder/*!*/ content, int term) {
            int c = ReadNormalizeEndOfLine();
            if (c == '\\') {
                AppendEscapedRegexEscape(content, term);
            } else if (c == -1) {
                InvalidEscapeCharacter();
            } else {
                AppendCharacter(content, c);
            }
        }

        private void AppendEscapedOctalEscape(MutableStringBuilder/*!*/ content) {
            int start = _bufferPos - 1;
            ReadOctalEscape(0);

            Debug.Assert(IsOctalDigit(_lineBuffer[start])); // first digit
            content.AppendAscii(_lineBuffer, start, _bufferPos - start);
        }

        private void AppendEscapedHexEscape(MutableStringBuilder/*!*/ content) {
            int start = _bufferPos - 1;
            ReadHexEscape();

            Debug.Assert(_lineBuffer[start] == 'x');
            content.AppendAscii(_lineBuffer, start, _bufferPos - start);
        }

        // returns true if any escaped codepoint is non-ascii
        private bool AppendEscapedUnicode(MutableStringBuilder/*!*/ content) {
            bool isNonAscii = false;
            int start = _bufferPos - 1;
            if (Peek() == '{') {
                // \u{codepoint codepoint ... codepoint}
                isNonAscii = AppendUnicodeCodePoints(null);
            } else {
                // \uFFFF
                isNonAscii = ReadUnicodeEscape4() >= 0x80;
            }
            Debug.Assert(_lineBuffer[start] == 'u');
            content.AppendAscii(_lineBuffer, start, _bufferPos - start);
            return isNonAscii;
        }

        // Reads octal number of at most 3 digits.
        // Reads at most 2 octal digits as the value of the first digit is in "value".
        private int ReadOctalEscape(int value) {
            int c;
            if (IsOctalDigit(c = Peek())) {
                Skip(c);
                value = (value << 3) | (c - '0');

                if (IsOctalDigit(c = Peek())) {
                    Skip(c);
                    value = (value << 3) | (c - '0');
                }
            }
            return value;
        }

        // Reads hexadecimal number of at most 2 digits. 
        private int ReadHexEscape() {
            int c;
            int value = ToDigit(c = Peek());
            if (value < 16) {
                Skip(c);
                int digit = ToDigit(c = Peek());
                if (digit < 16) {
                    Skip(c);
                    value = (value << 4) | digit;
                }

                return value;
            } else {
                return InvalidEscapeCharacter();
            }
        }

        // Peeks exactly 4 hexadecimal characters (\uFFFF).
        private int ReadUnicodeEscape4() {
            int d4 = ToDigit(Peek(0));
            int d3 = ToDigit(Peek(1));
            int d2 = ToDigit(Peek(2));
            int d1 = ToDigit(Peek(3));

            if (d1 >= 16 || d2 >= 16 || d3 >= 16 || d4 >= 16) {
                return InvalidEscapeCharacter();                
            }

            SeekRelative(4);
            return (d4 << 12) | (d3 << 8) | (d2 << 4) | d1;
        }

        // Reads {at-most-six-hexa-digits}
        private int ReadUnicodeEscape6() {
            int c = Read();
            Debug.Assert(c == '{');

            bool isEmpty;
            int codepoint = ReadUnicodeCodePoint(out isEmpty);

            if (Peek() == '}') {
                Skip();
            } else {
                ReportError(Errors.UntermintedUnicodeEscape);
                isEmpty = false;
            }

            if (isEmpty) {
                ReportError(Errors.InvalidUnicodeEscape);
            }

            return codepoint;
        }

        // Parses [0-9A-F]{1,6}
        private int ReadUnicodeCodePoint(out bool isEmpty) {
            int codepoint = 0;
            int i = 0;
            while (true) {
                int digit = ToDigit(Peek());
                if (digit >= 16) {
                    break;
                }

                if (i < 7) {
                    codepoint = (codepoint << 4) | digit;
                }

                i++;
                Skip();
            }

            if (codepoint > 0x10ffff) {
                ReportError(Errors.TooLargeUnicodeCodePoint);
                codepoint = (int)'?';
            }

            isEmpty = i == 0;
            return codepoint;
        }

        // returns true if any codepoint represents a non-ascii character
        private bool AppendUnicodeCodePoints(MutableStringBuilder content) {
            bool isAscii = true;
            bool isEmpty;
            Skip('{');
            while (true) {
                int codepoint = ReadUnicodeCodePoint(out isEmpty);
                if (content != null && !isEmpty) {
                    AppendUnicodeCodePoint(content, codepoint);
                }
                isAscii &= codepoint <= 0x7f;

                int c = Peek();
                if (c == '}') {
                    Skip();
                    break;
                }
                if (c != ' ') {
                    ReportError(Errors.UntermintedUnicodeEscape);
                    isEmpty = false;
                    break;
                }
                Skip();
            }
            if (isEmpty) {
                ReportError(Errors.InvalidUnicodeEscape);
            }
            return !isAscii;
        }

        // Reads \uFFFF or \u{FFFFFF} and appends the character to the string content:
        private void AppendUnicodeCodePoint(MutableStringBuilder/*!*/ content, int codepoint) {
            if (codepoint >= 0x80) {
                SwitchToUtf8(content);
            }
            content.AppendUnicodeCodepoint(codepoint);
        }

        private void SwitchToUtf8(MutableStringBuilder/*!*/ content) {
            if (content.IsAscii) {
                content.Encoding = RubyEncoding.UTF8;
            } else if (content.Encoding != RubyEncoding.UTF8) {
                ReportError(Errors.EncodingsMixed, content.Encoding, RubyEncoding.UTF8.Name);
            }
        }
        
        private void AppendByte(MutableStringBuilder/*!*/ content, int b) {
            if (b >= 0x80 && content.Encoding == RubyEncoding.Ascii) {
                content.Encoding = RubyEncoding.Binary;
            }
            content.Append((byte)b);
        }

        private void AppendCharacter(MutableStringBuilder/*!*/ content, int c) {
            // c is encoded via the current source encoding (__ENCODING__):
            if (c >= 0x80 && !content.IsAscii && content.Encoding != _encoding) {
                ReportError(Errors.EncodingsMixed, content.Encoding, _encoding);
            }
            content.Append((char)c);
        }

        /// <summary>
        /// Converts all codepoints in range [0, 0x10ffff] to a string.
        /// Undefined for codepoint greater than 0x10ffff.
        /// </summary>
        public static string/*!*/ UnicodeCodePointToString(int codepoint) {
            if (codepoint < 0x10000) {
                // code-points [0xd800 .. 0xdfff] are not treated as invalid
                return new String((char)codepoint, 1);
            } else {
                codepoint -= 0x10000;
                return new String(new char[] { (char)((codepoint / 0x400) + 0xd800), (char)((codepoint % 0x400) + 0xdc00) });
            }
        }

        public static int ToCodePoint(int highSurrogate, int lowSurrogate) {
            return (highSurrogate - 0xd800) * 0x400 + (lowSurrogate - 0xdc00) + 0x10000;
        }

        public static bool IsHighSurrogate(int c) {
            return unchecked((uint)c - 0xd800 <= (uint)0xdbff - 0xd800);
        }

        public static bool IsLowSurrogate(int c) {
            return unchecked((uint)c - 0xdc00 <= (uint)0xdfff - 0xdc00);
        }

        public static bool IsSurrogate(int c) {
            return unchecked((uint)c - 0xd800 <= (uint)0xdfff - 0xd800);
        }

        #endregion

        #region Strings

        internal void SetStringToken(string/*!*/ value) {
            _tokenValue.SetString(value);
        }

        internal void SetAsciiStringToken(string/*!*/ symbol) {
            _tokenValue.SetString(symbol);
        }

        internal void SetStringToken(int start, int length) {
            SetStringToken(new String(_lineBuffer, start, length));
        }

        // String: "...
        private Tokens ReadDoubleQuote() {
            _state.CurrentSequence = new StringState(StringProperties.ExpandsEmbedded, '"');
            return Tokens.StringBegin;
        }

        // String: '...
        private Tokens ReadSingleQuote() {
            _state.CurrentSequence = new StringState(StringProperties.Default, '\'');
            return Tokens.StringBegin;
        }

        // returns last character read
        private int ReadStringContent(MutableStringBuilder/*!*/ content, StringProperties stringType, int terminator, int openingParenthesis, 
            ref int nestingLevel) {

            while (true) {
                int eolnWidth;
                int c = ReadNormalizeEndOfLine(out eolnWidth);
                if (c == -1) {
                    return -1;
                }

                if (openingParenthesis != 0 && c == openingParenthesis) {
                    nestingLevel++;
                } else if (c == terminator) {
                    if (nestingLevel == 0) {
                        SeekRelative(-eolnWidth);
                        return c;
                    }
                    nestingLevel--;
                } else if (((stringType & StringProperties.ExpandsEmbedded) != 0) && c == '#' && _bufferPos < _lineLength) {
                    int c2 = _lineBuffer[_bufferPos];
                    if (c2 == '$' || c2 == '@' || c2 == '{') {
                        SeekRelative(-eolnWidth);
                        return c;
                    }
                } else if ((stringType & StringProperties.Words) != 0 && IsWhiteSpace(c)) {
                    SeekRelative(-eolnWidth);
                    return c;
                } else if (c == '\\') {
                    c = ReadNormalizeEndOfLine(out eolnWidth);

                    if (c == '\n') {
                        if ((stringType & StringProperties.Words) == 0) {
                            if ((stringType & StringProperties.ExpandsEmbedded) != 0) {
                                continue;
                            }
                            content.AppendAscii('\\');
                        }
                    } else if (c == '\\') {
                        if ((stringType & StringProperties.RegularExpression) != 0) {
                            content.AppendAscii('\\');
                        }
                    } else if ((stringType & StringProperties.RegularExpression) != 0) {
                        // \uFFFF, \u{codepoint}
                        if (c == 'u') {
                            content.AppendAscii('\\');
                            if (AppendEscapedUnicode(content)) {
                                SwitchToUtf8(content);
                            }
                        } else {
                            SeekRelative(-eolnWidth);
                            AppendEscapedRegexEscape(content, terminator);
                        }
                        continue;
                    } else if ((stringType & StringProperties.ExpandsEmbedded) != 0) {
                        if (c == 'u') {
                            if (Peek() == '{') {
                                // \u{codepoint codepoint ... codepoint}
                                if (AppendUnicodeCodePoints(content)) {
                                    SwitchToUtf8(content);
                                }
                            } else {
                                // \uFFFF
                                int codepoint = ReadUnicodeEscape4();
                                AppendUnicodeCodePoint(content, codepoint);
                                if (codepoint >= 0x80) {
                                    SwitchToUtf8(content);
                                }
                            }
                        } else {
                            // other escapes:
                            SeekRelative(-eolnWidth);
                            AppendByte(content, ReadEscape());
                        }
                        continue;
                    } else if ((stringType & StringProperties.Words) != 0 && IsWhiteSpace(c)) {
                        // ignore backslashed spaces in %w
                    } else if (c != terminator && !(openingParenthesis != 0 && c == openingParenthesis)) {
                        content.AppendAscii('\\');
                    }
                }

                AppendCharacter(content, c);
            }
        }

        //
        // returns tokens: 
        // - StringEnd/RegexEnd           ... string/regex closed
        // - WordSeparator                ... whitespace in a word list
        // - StringEmbeddedVariableBegin  ... #$, #@ (start of an embedded global/instance variable)
        // - StringEmbeddedCodeBegin      ... #{ (start of an embedded expression)
        // - StringContent                ... string data
        //
        internal Tokens TokenizeString(StringState/*!*/ info) {
            // global or instance variable in a string:
            if (InStringEmbeddedVariable) {
                InStringEmbeddedVariable = false;
                return Tokenize();
            }

            StringProperties properties = info.Properties;
            bool whitespaceSeen = false;

            MarkTokenStart();

            int eolnWidth;
            int c = ReadNormalizeEndOfLine(out eolnWidth);

            // unterminated string:
            if (c == -1) {
                _unterminatedToken = true;
                MarkSingleLineTokenEnd();
                if (_verbatim) {
                    return Tokens.EndOfFile;
                } else {
                    ReportError(Errors.UnterminatedString);
                    return FinishString(Tokens.StringEnd);
                }
            }

            bool isMultiline = c == '\n';

            // skip whitespace in word list:
            if ((properties & StringProperties.Words) != 0 && IsWhiteSpace(c)) {
                isMultiline |= SkipWhitespace();
                c = Read(); 
                whitespaceSeen = true;
            }

            // end of the top-level string:
            if (c == info.TerminatingCharacter && info.NestingLevel == 0) {
                
                // end of words:
                if ((properties & StringProperties.Words) != 0) {
                    MarkTokenEnd(isMultiline);
                    return FinishString(Tokens.StringEnd);
                }

                // end of regex:
                if ((properties & StringProperties.RegularExpression) != 0) {
                    _tokenValue.SetRegexOptions(ReadRegexOptions());
                    MarkTokenEnd(isMultiline);
                    return FinishString(Tokens.RegexpEnd);
                }
                
                // end of string/symbol:
                MarkTokenEnd(isMultiline);
                return FinishString(Tokens.StringEnd);
            }

            // word separator:
            if (whitespaceSeen) {
                Debug.Assert(!IsWhiteSpace(c));
                Back(c);
                MarkTokenEnd(isMultiline);
                return Tokens.WordSeparator;
            }

            MutableStringBuilder content;

            // start of #$variable, #@variable, #{expression} in a string:
            if ((properties & StringProperties.ExpandsEmbedded) != 0 && c == '#') {
                switch (Peek()) {
                    case '$':
                    case '@':
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedVariableBegin();

                    case '{':
                        Skip('{');
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedCodeBegin();
                }
                content = new MutableStringBuilder(_encoding);
                content.AppendAscii('#');
            } else {
                content = new MutableStringBuilder(_encoding);
                SeekRelative(-eolnWidth);
            }

            int nestingLevel = info.NestingLevel;
            ReadStringContent(content, properties, info.TerminatingCharacter, info.OpeningParenthesis, ref nestingLevel);
            _state.CurrentSequence = info.SetNesting(nestingLevel);

            _tokenValue.SetStringContent(content);
            MarkMultiLineTokenEnd();
            return Tokens.StringContent;
        }

        private Tokens FinishString(Tokens endToken) {
            _state.CurrentSequence = TokenSequenceState.None;
            LexicalState = LexicalState.EXPR_END;
            return endToken;
        }

        #endregion

        #region Heredoc

        private Tokens TokenizeHeredocLabel() {
            int term;
            StringProperties stringType = StringProperties.Default;

            int prefixWidth;
            int c = ReadNormalizeEndOfLine(out prefixWidth);
            if (c == '-') {
                c = ReadNormalizeEndOfLine(out prefixWidth);
                prefixWidth++;
                stringType = StringProperties.IndentedHeredoc;
            }

            string label;
            if (c == '\'' || c == '"' || c == '`') {
                if (c != '\'') {
                    stringType |= StringProperties.ExpandsEmbedded;
                }

                // do not include quotes:
                int start = _bufferPos;
                term = c;

                while (true) {
                    c = Read(); 
                    if (c == -1) {
                        _unterminatedToken = true;
                        ReportError(Errors.UnterminatedHereDocIdentifier);
                        c = term;
                        break;
                    }

                    if (c == term) {
                        break;
                    }

                    // MRI doesn't do this, it continues reading the label and includes \n into it.
                    // The label cannot be matched with the end label (only single-line comparison is done), so it's better to report error here
                    // Allowing \n in label requires the token to be multi-line.
                    // Note we can ignore \r followed by \n here since it will fail in the next iteration.
                    if (c == '\n') {
                        Back('\n');
                        ReportError(Errors.UnterminatedHereDocIdentifier);
                        c = term;
                        break;
                    }
                }

                label = new String(_lineBuffer, start, _bufferPos - start - 1);
            } else if (IsIdentifier(c)) {
                term = '"';
                stringType |= StringProperties.ExpandsEmbedded;
                
                int start = _bufferPos - 1;
                SkipVariableName();
                label = new String(_lineBuffer, start, _bufferPos - start);
            } else {
                SeekRelative(-prefixWidth);
                return Tokens.None;
            }

            // note that if we allow \n in the label we must change this to multi-line token!
            MarkSingleLineTokenEnd();
            
            if (_verbatim) {
                // enqueue a new verbatim heredoc state, it will be dequeued at the end of the current line:
                _state.EnqueueVerbatimHeredoc(new VerbatimHeredocState(stringType, label));
                return Tokens.VerbatimHeredocBegin;
            } else {
                // skip the rest of the line (the content is stored in heredoc string terminal and tokenized upon restore)
                int resume = _bufferPos;
                _bufferPos = _lineLength;
                _state.CurrentSequence = new HeredocState(stringType, label, resume, _lineBuffer, _lineLength, _currentLine, _currentLineIndex);
                _lineBuffer = new char[InitialBufferSize];
                return term == '`' ? Tokens.ShellStringBegin : Tokens.StringBegin;
            }
        }

        private void MarkHeredocEnd(HeredocStateBase/*!*/ heredoc, int labelStart) {
            if (labelStart < 0) {
                MarkTokenStart();
                MarkSingleLineTokenEnd();
            } else {
                SeekRelative(labelStart);
                MarkTokenStart();
                SeekRelative(heredoc.Label.Length);
                if (TryReadEndOfLine()) {
                    MarkMultiLineTokenEnd();
                } else {
                    MarkSingleLineTokenEnd();
                }
            }
        }

        internal Tokens FinishVerbatimHeredoc(VerbatimHeredocState/*!*/ heredoc, int labelStart) {
            Debug.Assert(_verbatim);
            MarkHeredocEnd(heredoc, labelStart);
            _state.FinishVerbatimHeredoc();
            CaptureTokenSpan();
            return Tokens.VerbatimHeredocEnd;
        }

        // 
        // The heredoc end label is the token that we return as StringEnd.
        // After marking the label the buffer is restored so that we can continue reading tokens following the heredoc opening label.
        //
        // [<<END]<content><heredoc-end-label>|... other tokens ...
        // ... heredoc content tokens ...
        // END
        //
        internal Tokens FinishHeredoc(HeredocState/*!*/ heredoc, int labelStart) {
            Debug.Assert(!_verbatim);

            MarkHeredocEnd(heredoc, labelStart);
            _state.HeredocEndLine = _currentTokenEnd.Line;
            _state.HeredocEndLineIndex = _currentTokenEnd.Index;
            _state.CurrentSequence = TokenSequenceState.None;
            LexicalState = LexicalState.EXPR_END;
            
            // restore buffer:
            _lineBuffer = heredoc.ResumeLine;
            _lineLength = heredoc.ResumeLineLength;
            _bufferPos = heredoc.ResumePosition;
            _currentLine = heredoc.FirstLine;
            _currentLineIndex = heredoc.FirstLineIndex;

            // We pretend the end token is zero-width and immediately follows the opening heredoc token.
            // This makes locations merging in the parser work w/o introducing an additional complexity.
            MarkTokenStart();
            MarkSingleLineTokenEnd();
            CaptureTokenSpan();
            return Tokens.StringEnd;
        }

        internal Tokens TokenizeAndMarkHeredoc(HeredocStateBase/*!*/ heredoc) {
            // global or instance variable in heredoc:
            if (InStringEmbeddedVariable) {
                InStringEmbeddedVariable = false;
                CaptureTokenSpan();
                return Tokenize();
            }
            
            StringProperties stringKind = heredoc.Properties;
            bool isIndented = (stringKind & StringProperties.IndentedHeredoc) != 0;

            if (Peek() == -1) {
                ReportError(Errors.UnterminatedHereDoc, heredoc.Label);
                _unterminatedToken = true;
                return heredoc.Finish(this, -1);
            }

            // label reached - it becomes a string-end token:
            // (note that label is single line, MRI allows multiline, but such label is never matched)
            int labelStart;
            if (is_bol() && LineContentEquals(heredoc.Label, isIndented, out labelStart)) {
                return heredoc.Finish(this, labelStart);
            }

            MarkTokenStart();

            if ((stringKind & StringProperties.ExpandsEmbedded) == 0) {

                StringBuilder str = ReadNonexpandingHeredocContent(heredoc);

                // do not restore buffer, the next token query will invoke 'if (EOF)' or 'if (line contains label)' above:
                SetStringToken(str.ToString());
                MarkMultiLineTokenEnd();
                CaptureTokenSpan();
                return Tokens.StringContent;
            }

            Tokens result = TokenizeExpandingHeredocContent(heredoc);
            CaptureTokenSpan();
            return result;
        }

        private StringBuilder/*!*/ ReadNonexpandingHeredocContent(HeredocStateBase/*!*/ heredoc) {
            bool isIndented = (heredoc.Properties & StringProperties.IndentedHeredoc) != 0;
            var result = new StringBuilder();

            // reads lines until the line contains heredoc label
            do {
                int end = _lineLength;
                if (end > 0) {
                    switch (_lineBuffer[end - 1]) {
                        case '\n':
                            if (--end == 0 || _lineBuffer[end - 1] != '\r') {
                                end++;
                                break;
                            }
                            --end;
                            break;

                        case '\r':
                            --end;
                            break;
                    }
                }

                result.Append(_lineBuffer, 0, end);

                if (end < _lineLength) {
                    result.Append('\n');
                }

                _bufferPos = _lineLength;

                // force new line load:
                RefillBuffer();

                if (Peek() == -1) {
                    // eof reached before end of heredoc:
                    return result;
                }

            } while (!LineContentEquals(heredoc.Label, isIndented));

            // return to the end of line, next token will be StringEnd spanning over the end-of-heredoc label:
            _bufferPos = 0;
            return result;
        }

        private Tokens TokenizeExpandingHeredocContent(HeredocStateBase/*!*/ heredoc) {
            MutableStringBuilder content;

            int c = Peek();
            if (c == '#') {
                Skip(c);
                
                switch (Peek()) {
                    case '$':
                    case '@':
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedVariableBegin();

                    case '{':
                        Skip('{');
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedCodeBegin();
                }

                content = new MutableStringBuilder(_encoding);
                content.AppendAscii('#');
            } else {
                content = new MutableStringBuilder(_encoding);
            }

            bool isIndented = (heredoc.Properties & StringProperties.IndentedHeredoc) != 0;
            
            do {
                // read string content upto the end of the line:
                int tmp = 0;
                c = ReadStringContent(content, heredoc.Properties, '\n', 0, ref tmp);
                
                // stop reading on end-of-file or just before an embedded expression: #$, #$, #{
                if (c != '\n') {
                    break;
                }

                // append \n
                content.AppendAscii((char)ReadNormalizeEndOfLine());

                // if we are in verbatim mode we need to yield to heredocs that were defined on the current line:
                if (c == '\n' && _verbatim && _state.VerbatimHeredocQueue != null) {
                    break;
                }

                // TODO:
                RefillBuffer();

                // first char on the next line:
                if (Peek() == -1) {
                    break;
                }

            } while (!LineContentEquals(heredoc.Label, isIndented));

            _tokenValue.SetStringContent(content);
            MarkMultiLineTokenEnd();
            return Tokens.StringContent;
        }

        #endregion

        #region String Quotations

        // Quotation start: 
        //   %[QqWwxrs]?[^:alpha-numeric:]
        private Tokens TokenizeQuotationStart() {
            StringProperties type;
            Tokens token;
            int terminator;

            // c is the character following %
            // note that it could be eoln in which case it needs to be normalized:
            int c = ReadNormalizeEndOfLine();
            switch (c) {
                case 'Q':
                    type = StringProperties.ExpandsEmbedded;
                    token = Tokens.StringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'q':
                    type = StringProperties.Default;
                    token = Tokens.StringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'W':
                    type = StringProperties.Words | StringProperties.ExpandsEmbedded;
                    token = Tokens.WordsBegin;
                    // if the terminator is a whitespace the end will never be matched and syntax error will be reported
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'w':
                    type = StringProperties.Words;
                    token = Tokens.VerbatimWordsBegin;
                    // if the terminator is a whitespace the end will never be matched and syntax error will be reported
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'x':
                    type = StringProperties.ExpandsEmbedded;
                    token = Tokens.ShellStringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'r':
                    type = StringProperties.RegularExpression | StringProperties.ExpandsEmbedded;
                    token = Tokens.RegexpBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 's':
                    type = StringProperties.Symbol;
                    token = Tokens.SymbolBegin;
                    terminator = ReadNormalizeEndOfLine();
                    LexicalState = LexicalState.EXPR_FNAME;
                    break;

                default:
                    type = StringProperties.ExpandsEmbedded;
                    token = Tokens.StringBegin;
                    terminator = c;
                    break;
            }

            int parenthesis = terminator;
            switch (terminator) {
                case -1:
                    _unterminatedToken = true;
                    MarkSingleLineTokenEnd();
                    ReportError(Errors.UnterminatedString);
                    return Tokens.EndOfFile;

                case '(': terminator = ')'; break;
                case '{': terminator = '}'; break;
                case '[': terminator = ']'; break;
                case '<': terminator = '>'; break;

                default:
                    if (IsLetterOrDigit(terminator)) {
                        Back(terminator);
                        MarkSingleLineTokenEnd();
                        ReportError(Errors.UnknownQuotedStringType);
                        return Tokens.Percent;
                    }

                    parenthesis = 0;
                    break;
            }

            bool isMultiline = terminator == '\n';

            if ((type & StringProperties.Words) != 0) {
                isMultiline |= SkipWhitespace();
            }

            if (isMultiline) {
                MarkMultiLineTokenEnd();
            } else {
                MarkSingleLineTokenEnd();
            }

            _state.CurrentSequence = new StringState(type, (char)terminator, (char)parenthesis, 0);
            return token;
        }

        #endregion

        #region Numbers

        public sealed class BignumParser : UnsignedBigIntegerParser {
            private char[] _buffer;
            private int _position;

            public int Position { get { return _position; } set { _position = value; } }
            public char[] Buffer { get { return _buffer; } set { _buffer = value; } }

            public BignumParser() {
            }

            protected override int ReadDigit() {
                Debug.Assert('0' < 'A' && 'A' < '_' && '_' < 'a');

                while (true) {
                    char c = _buffer[_position++];

                    if (c <= '9') {
                        Debug.Assert(c >= '0');
                        return c - '0';
                    } else if (c >= 'a') {
                        Debug.Assert(c <= 'z');
                        return c - 'a' + 10;
                    } else if (c != '_') {
                        Debug.Assert(c >= 'A' && c <= 'Z');
                        return c - 'A' + 10;
                    }
                }
            }
        }

        private enum NumericCharKind {
            None,
            Digit,
            Underscore
        }

        // INTEGER:
        // [1-9]([0-9_]*[1-9])?
        // 0([0-7_]*[0-7])?
        // 0[xX][0-9a-fA-F]([0-9a-fA-F_]*[0-9a-fA-F])?
        // 0[dD][0-9]([0-9_]*[0-9])?
        // 0[bB][01]([01_]*[01])?
        // 0[oO][0-7]([0-7_]*[0-7])?
        //
        // FLOAT:
        // (0|[1-9]([0-9_]*[0-9])?)[.][0-9_]*[0-9]([eE][+-]?[0-9]([0-9_]*[0-9])?)
        //
        // Takes the first decimal digit of the number.
        //
        private Tokens ReadUnsignedNumber(int c) {
            LexicalState = LexicalState.EXPR_END;
           
            if (c == '0') {
                switch (Peek()) {
                    case 'x':
                    case 'X':
                        Skip();
                        return ReadInteger(16, NumericCharKind.None);

                    case 'b':
                    case 'B':
                        Skip();
                        return ReadInteger(2, NumericCharKind.None);

                    case 'o':
                    case 'O':
                        Skip();
                        return ReadInteger(8, NumericCharKind.None);

                    case 'd':
                    case 'D':
                        Skip();
                        return ReadInteger(10, NumericCharKind.None);

                    case 'e':
                    case 'E': {
                            // 0e[+-]...    
                            int sign;
                            int start = _bufferPos - 1;

                            if (TryReadExponentSign(1, out sign)) {
                                return ReadDoubleExponent(start, sign);
                            }

                            _tokenValue.SetInteger(0);
                            return Tokens.Integer;
                        }

                    case '.':
                        // 0.
                        if (IsDecimalDigit(Peek(1))) {
                            Skip('.');
                            return ReadDouble(_bufferPos - 2);
                        }

                        _tokenValue.SetInteger(0);
                        return Tokens.Integer;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '_':
                        // the previous character is '0' digit:
                        return ReadInteger(8, NumericCharKind.Digit);

                    case '8':
                    case '9':
                        ReportError(Errors.IllegalOctalDigit);
                        // treat the number as decimal
                        return ReadInteger(10, NumericCharKind.Digit);

                    default:
                        _tokenValue.SetInteger(0);
                        return Tokens.Integer;
                }
            }

            return ReadDecimalNumber(c);
        }

        // OCTAL:   [0-7]([0-7_]*[0-7])?
        // HEXA:    [0-9a-fA-F]([0-9a-fA-F_]*[0-9a-fA-F])?
        // BINARY:  [01]([01_]*[01])?
        // DECIMAL: [0-9]([0-9_]*[0-9])?
        //
        // prev ... previous character: either '0' for octals or -1 for 0[xbdo]
        private Tokens ReadInteger(int @base, NumericCharKind prev) {
            Debug.Assert(prev == NumericCharKind.None || prev == NumericCharKind.Digit);
            Debug.Assert(@base <= 16);
            long integer = 0;
            int numberStartIndex = _bufferPos;
            int underscoreCount = 0;

            while (true) {
                int c = Peek();
                int digit = ToDigit(c);

                if (digit < @base) {
                    Skip(c);

                    integer = integer * @base + digit;
                    prev = NumericCharKind.Digit;
                    
                    if (integer > Int32.MaxValue) {
                        return ReadBigNumber(integer, @base, numberStartIndex, underscoreCount, false);
                    }

                } else {
                    if (prev != NumericCharKind.Digit) {
                        if (prev == NumericCharKind.Underscore) {
                            ReportError(Errors.TrailingUnderscoreInNumber);
                        } else {
                            ReportError(Errors.NumericLiteralWithoutDigits);
                        }
                    } else if (c == '_') {
                        Skip(c);
                        prev = NumericCharKind.Underscore;
                        underscoreCount++;
                        continue;
                    } 
                    
                    if (c == '.' && IsDecimalDigit(Peek(1))) {
                        ReportWarning(Errors.NoFloatingLiteral);
                    }

                    _tokenValue.SetInteger((int)integer);
                    return Tokens.Integer;
                }
            }
        }

        // INTEGER:
        // [1-9]([0-9_]*[1-9])?
        //
        // FLOAT:
        // [1-9]([0-9_]*[0-9])?[.][0-9_]*[0-9]([eE][+-]?[0-9]([0-9_]*[0-9])?)
        private Tokens ReadDecimalNumber(int c) {
            Debug.Assert(IsDecimalDigit(c) && c != '0');

            // the first character of the number already read:
            int numberStartIndex = _bufferPos - 1;

            int underscoreCount = 0;
            NumericCharKind prev = NumericCharKind.Digit;
            long integer = c - '0';

            while (true) {
                int sign;
                c = Peek();

                if (IsDecimalDigit(c)) {
                    Skip(c);
                    prev = NumericCharKind.Digit;
                    integer = integer * 10 + (c - '0');
                    if (integer > Int32.MaxValue) {
                        return ReadBigNumber(integer, 10, numberStartIndex, underscoreCount, true);
                    }

                } else if (prev == NumericCharKind.Underscore) {

                    ReportError(Errors.TrailingUnderscoreInNumber);
                    _tokenValue.SetInteger((int)integer);
                    return Tokens.Integer;

                } else if ((c == 'e' || c == 'E') && TryReadExponentSign(1, out sign)) {

                    return ReadDoubleExponent(numberStartIndex, sign);

                } else if (c == '_') {

                    Skip(c);
                    underscoreCount++;
                    prev = NumericCharKind.Underscore;

                } else {

                    if (c == '.' && IsDecimalDigit(Peek(1))) {
                        Skip('.');
                        return ReadDouble(numberStartIndex);
                    }

                    _tokenValue.SetInteger((int)integer);
                    return Tokens.Integer;
                }
            }
        }

        private bool TryReadExponentSign(int offset, out int sign) {
            int s = Peek(offset);
            if (s == '-') {
                offset++;
                sign = -1;
            } else if (s == '+') {
                offset++;
                sign = +1;
            } else {
                sign = +1;
            }

            if (IsDecimalDigit(Peek(offset))) {
                SeekRelative(offset);
                return true;
            }

            if (s == '-') {
                ReportError(Errors.TrailingMinusInNumber);
            } else if (s == '+') {
                ReportError(Errors.TrailingPlusInNumber);
            } else {
                ReportError(Errors.TrailingEInNumber);
            }

            return false;
        }

        // OCTAL:   [0-7]([0-7_]*[0-7])?
        // HEXA:    [0-9a-fA-F]([0-9a-fA-F_]*[0-9a-fA-F])?
        // BINARY:  [01]([01_]*[01])?
        // DECIMAL: [0-9]([0-9_]*[0-9])?
        // FLOAT:   [1-9]([0-9_]*[0-9])?[.][0-9_]*[0-9]([eE][+-]?[0-9]([0-9_]*[0-9])?)
        //
        // Previous digit caused an integer overflow.
        // numberStartIndex ... index of the first (most significant) digit
        // underscoreCount  ... number of underscores already read
        private Tokens ReadBigNumber(long value, int @base, int numberStartIndex, int underscoreCount, bool allowDouble) {
            Debug.Assert(!allowDouble || @base == 10, "Only decimal based doubles supported");
            Debug.Assert(@base <= 16);

            // the previous char is a digit:
            NumericCharKind prev = NumericCharKind.Digit; 

            while (true) {
                int c = Peek();
                int digit = ToDigit(c);

                if (digit < @base) {
                    prev = NumericCharKind.Digit;
                    Skip(c);
                } else {

                    if (prev == NumericCharKind.Underscore) {
                        ReportError(Errors.TrailingUnderscoreInNumber);                        
                    } else if (c == '_') {
                        Skip(c);
                        prev = NumericCharKind.Underscore;
                        underscoreCount++;
                        continue;
                    } else if (allowDouble) {
                        int sign;
                        if ((c == 'e' || c == 'E') && TryReadExponentSign(1, out sign)) {
                            return ReadDoubleExponent(numberStartIndex, sign);
                        } else if (c == '.') {
                            if (IsDecimalDigit(Peek(1))) {
                                Skip('.');
                                return ReadDouble(numberStartIndex);
                            }
                        }
                    }

                    // TODO: store only the digit count, the actual value will be parsed later:
                    // TODO: skip initial zeros
                    if (_bigIntParser == null) {
                        _bigIntParser = new BignumParser();
                    }

                    _bigIntParser.Position = numberStartIndex;
                    _bigIntParser.Buffer = _lineBuffer;

                    BigInteger result = _bigIntParser.Parse(_bufferPos - numberStartIndex - underscoreCount, @base);

                    Debug.Assert(value > 0, "Cannot be zero since we are parsing a number greater than Int32.MaxValue");

                    _tokenValue.SetBigInteger(result);
                    return Tokens.BigInteger;
                }
            }
        }

        // FLOAT - decimal and exponent
        // {value.}[0-9_]*[0-9])([eE][+-]?[0-9]([0-9_]*[0-9])?)
        private Tokens ReadDouble(int numberStartIndex) {
            Debug.Assert(IsDecimalDigit(Peek()));

            NumericCharKind prev = NumericCharKind.None;
            while (true) {
                int sign;
                int c = Peek();

                if (IsDecimalDigit(c)) {
                    prev = NumericCharKind.Digit;
                    Skip(c);
                } else if ((c == 'e' || c == 'E') && TryReadExponentSign(1, out sign)) {
                    return ReadDoubleExponent(numberStartIndex, sign);
                } else {
                    if (prev == NumericCharKind.Underscore) {
                        ReportError(Errors.TrailingUnderscoreInNumber);                        
                    } else if (c == '_') {
                        Skip(c);
                        prev = NumericCharKind.Underscore;
                        continue;
                    }

                    return DecodeDouble(numberStartIndex, _bufferPos);
                }
            }
        }

        // FLOAT - exponent
        // [+-]?[0-9]([0-9_]*[0-9])?
        private Tokens ReadDoubleExponent(int numberStartIndex, int sign) {
            int exponent = 0;
            NumericCharKind prev = NumericCharKind.None;
            while (true) {
                int c = Peek();

                if (IsDecimalDigit(c)) {
                    Skip(c);
                    prev = NumericCharKind.Digit;
                    
                    // greater exponents evaluate to infinity/zero, we need to keep parsing though:
                    if (exponent < 10000) {
                        exponent = exponent * 10 + (c - '0');
                    }
                } else {
                    if (prev != NumericCharKind.Digit) {
                        Debug.Assert(prev == NumericCharKind.Underscore);
                        ReportError(Errors.TrailingUnderscoreInNumber);                            
                    } else if (c == '_') {
                        Skip(c);
                        prev = NumericCharKind.Underscore;
                        continue;
                    }

                    exponent *= sign;

                    // some MRI arbitrary restrictions on the exponent:
                    if (exponent <= -1021 || exponent >= 1025) {
                        // TODO:
                        int start = _currentTokenStart.Column - 1;
                        ReportWarning(Errors.FloatOutOfRange, new String(_lineBuffer, start, _bufferPos - start).Replace("_", ""));
                    }

                    return DecodeDouble(numberStartIndex, _bufferPos);
                }
            }
        }

        private static bool TryDecodeDouble(char[]/*!*/ str, int first, int end, out double result) {
            StringBuilder sb = new StringBuilder(end - first);
            sb.Length = end - first;

            int j = 0;
            for (int i = first; i < end; i++) {
                if (str[i] != '_') {
                    sb[j++] = str[i];
                }
            }

            sb.Length = j;
            return Double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryDecodeDouble(string/*!*/ str, int first, int end, out double result) {
            StringBuilder sb = new StringBuilder(end - first);
            sb.Length = end - first;

            int j = 0;
            for (int i = first; i < end; i++) {
                if (str[i] != '_') {
                    sb[j++] = str[i];
                }
            }

            sb.Length = j;
            return Double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private Tokens DecodeDouble(int first, int end) {
            double result;
            if (!TryDecodeDouble(_lineBuffer, first, end, out result)) {
                result = Double.PositiveInfinity;
            }

            _tokenValue.SetDouble(result);
            return Tokens.Float;
        }

        #endregion

        #region Character Categories

        public static bool IsDecimalDigit(int c) {
            return unchecked((uint)c - '0' <= (uint)'9' - '0');
        }

        public static bool IsOctalDigit(int c) {
            return unchecked((uint)c - '0' <= (uint)'7' - '0');
        }

        public static bool IsHexadecimalDigit(int c) {
            unchecked {
                return IsDecimalDigit(c) ||
                    (uint)c - 'a' <= (uint)'f' - 'a' ||
                    (uint)c - 'A' <= (uint)'F' - 'A';
            }
        }

        public static int ToDigit(int c) {
            if (IsDecimalDigit(c)) {
                return c - '0';
            }

            if (IsLowerLetter(c)) {
                return c - 'a' + 10;
            }

            if (IsUpperLetter(c)) {
                return c - 'A' + 10;
            }

            return Int32.MaxValue;
        }

        // MRI 1.9 consideres all characters greater than 0x007f as identifiers.
        // Surrogate pairs are composed to a single character that is also considered an identifier.
        private const int AllowMultiByteIdentifier = 0x07f;

        private bool IsIdentifier(int c) {
            return IsIdentifier(c, _multiByteIdentifier);
        }

        public static bool IsIdentifier(int c, int multiByteIdentifier) {
            return IsIdentifierInitial(c, multiByteIdentifier) || IsDecimalDigit(c);
        }

        private bool IsIdentifierInitial(int c) {
            return IsIdentifierInitial(c, _multiByteIdentifier);
        }

        public static bool IsIdentifierInitial(int c, int multiByteIdentifier) {
            return IsLetter(c) || c == '_' || c > multiByteIdentifier;
        }

        public static bool IsLetter(int c) {
            return IsUpperLetter(c) || IsLowerLetter(c);
        }

        public static bool IsLetterOrDigit(int c) {
            return IsLetter(c) || IsDecimalDigit(c);
        }

        public static bool IsUpperLetter(int c) {
            return unchecked((uint)c - 'A' <= (uint)'Z' - 'A');
        }

        public static bool IsLowerLetter(int c) {
            return unchecked((uint)c - 'a' <= (uint)'z' - 'a');
        }

        public static bool IsWhiteSpace(int c) {
            return IsAsciiWhiteSpace(c);
        }

        public static bool IsAsciiWhiteSpace(int c) {
            return unchecked(((uint)c - 9 <= (uint)13 - 9) || c == 32);
        }

        private static bool IsMethodNameSuffix(int c, int multiByteIdentifier) {
            return IsIdentifier(c, multiByteIdentifier) || c == '!' || c == '?' || c == '=';
        }

        private void SkipVariableName() {
            while (true) {
                int c = Peek();
                if (IsIdentifier(c)) {
                    Skip();
                } else {
                    break;
                }
            }
        }

        // returns true if an end of line has been skipped:
        private bool SkipWhitespace() {
            bool eolnSkipped = false;
            while (true) {
                RefillBuffer();
                int c = Peek();
                if (c == '\n') {
                    eolnSkipped = true;
                    Skip();
                } else if (IsWhiteSpace(c)) {
                    Skip();
                } else {
                    return eolnSkipped;
                }
            }
        }

        #endregion

        #region Public API

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public int DataOffset {
            get { return _dataOffset; }
        }

        public RubyCompatibility Compatibility {
            get { return _compatibility; }
            set { _compatibility = value; }
        }

        public SourceSpan TokenSpan {
            get { return _tokenSpan; }
        }

        public TokenValue TokenValue {
            get { return _tokenValue; }
        }

        public bool EndOfFileReached {
            get { return _eofReached; }
        }

        public bool UnterminatedToken {
            get { return _unterminatedToken; }
        }

        #region ParseInteger

        private static int NextChar(string/*!*/ str, ref int i) {
            return i == str.Length ? -1 : str[i++];
        }

        public static IntegerValue ParseInteger(string/*!*/ str, int @base) {
            int i = 0;
            return ParseInteger(str, @base, ref i);
        }
        
        // @base == 0:
        //    [:whitespace:]*[+-]?(0x|0X|ob|0B|0d|0D|0o|0O)?([:base-digit:][_]?)*[:base-digit:].*
        // otherwise:
        //    [:whitespace:]*[+-]?([:base-digit:][_]?)*[:base-digit:].*
        public static IntegerValue ParseInteger(string/*!*/ str, int @base, ref int i) {
            ContractUtils.RequiresNotNull(str, "str");

            int c;
            do { c = NextChar(str, ref i); } while (IsWhiteSpace(c));

            int sign;
            if (c == '+') {
                sign = +1;
                c = NextChar(str, ref i);
            } else if (c == '-') {
                sign = -1;
                c = NextChar(str, ref i);
            } else {
                sign = +1;
            }

            if (c == '0') {
                c = NextChar(str, ref i);
                int newBase = 0;
                switch (c) {
                    case 'x':
                    case 'X': newBase = 16; break;
                    case 'b':
                    case 'B': newBase = 2; break;
                    case 'd':
                    case 'D': newBase = 10; break;
                    case 'o':
                    case 'O': newBase = 8; break;
                }

                if (newBase != 0) {
                    // no base specified -> set the base
                    // base specified -> skip prefix of that base
                    if (@base == 0 || newBase == @base) {
                        @base = newBase;
                        c = NextChar(str, ref i);
                    }
                } else if (@base == 0) {
                    @base = 8;
                }
            } else if (@base == 0) {
                @base = 10;
            }

            bool underAllowed = false;
            long value = 0;
            int digitCount = 0;
            int start = i - 1;
            while (true) {
                if (c != '_') {
                    int digit = ToDigit(c);
                    if (digit < @base) {
                        if (value <= Int32.MaxValue) {
                            value = value * @base + digit;
                        }
                        digitCount++;
                    } else {
                        break;
                    }
                    underAllowed = true;
                } else if (underAllowed) {
                    underAllowed = false;
                } else {
                    break;
                }
                c = NextChar(str, ref i);
            }

            if (digitCount == 0) {
                return 0;
            }
            
            if (value <= Int32.MaxValue) {
                value *= sign;
                if (value >= Int32.MinValue && value <= Int32.MaxValue) {
                    return (int)value;
                } else {
                    return BigInteger.Create(value);
                }
            } else {
                var parser = new BignumParser();
                parser.Position = start;
                parser.Buffer = str.ToCharArray();
                
                return parser.Parse(digitCount, @base) * sign;
            }
        }

        #endregion

        #region TryParseDouble

        private static int Read(string/*!*/ str, ref int i) {
            i++;
            return (i < str.Length) ? str[i] : -1;
        }

        // subsequent _ are not considered error
        public static bool TryParseDouble(string/*!*/ str, out double result, out bool complete) {
            double sign;
            int i = -1;

            int c;
            do { c = Read(str, ref i); } while (IsWhiteSpace(c));
            
            if (c == '-') {
                c = Read(str, ref i);
                if (c == '_') {
                    result = 0.0;
                    complete = false;
                    return false;
                }
                sign = -1;
            } else if (c == '+') {
                c = Read(str, ref i);
                if (c == '_') {
                    result = 0.0;
                    complete = false;
                    return false;
                }
                sign = +1;
            } else {
                sign = +1;
            }

            int start = i;

            while (c == '_' || IsDecimalDigit(c)) {
                c = Read(str, ref i);
            }

            if (c == '.') {
                c = Read(str, ref i);
                while (c == '_' || IsDecimalDigit(c)) {
                    c = Read(str, ref i);
                }
            }

            // just before the current character:
            int end = i;

            if (c == 'e' || c == 'E') {
                c = Read(str, ref i);
                if (c == '+' || c == '-') {
                    c = Read(str, ref i);
                }

                int expEnd = end;

                while (true) {
                    if (IsDecimalDigit(c)) {
                        expEnd = i + 1;
                    } else if (c != '_') {
                        break;
                    }
                    c = Read(str, ref i);
                }

                end = expEnd;
            }

            bool success = TryDecodeDouble(str, start, end, out result);
            result *= sign;
            complete = end == str.Length;
            return success;
        }

        #endregion

        #region TryParseEncodingHeader

        private const string EncodingHeaderPattern = @"^[#].*?coding\s*[:=]\s*(?<encoding>[a-z0-9_-]+)";

        // reads case insensitively, doesn't rewind the reader if the content doesn't match, doesn't read a line unless it starts with '#':
        // ([#][!].*(\r|\n|\r\n))?
        // [#].*?coding\s*[:=]\s*([a-z0-9_-]+).*(\r|\n|\r\n)
        internal static bool TryParseEncodingHeader(TextReader/*!*/ reader, out string encodingName) {
            Assert.NotNull(reader);

            encodingName = null;

            if (reader.Peek() != '#') {
                return false;
            }

            string line = reader.ReadLine();

            // skip shebang:
            if (line.Length > 1 && line[1] == '!') {
                if (reader.Peek() != '#') {
                    return false;
                }
                line = reader.ReadLine();
            }

            var regex = new Regex(EncodingHeaderPattern, RegexOptions.IgnoreCase);
            var match = regex.Match(line);
            if (match.Success) {
                encodingName = match.Groups["encoding"].Value;
                return encodingName.Length > 0;
            }

            return false;
        }

        #endregion

        #region Names

        public static bool IsConstantName(string name) {
            return !String.IsNullOrEmpty(name) 
                && IsUpperLetter(name[0])
                && IsVariableName(name, 1, 1, AllowMultiByteIdentifier)
                && IsIdentifier(name[name.Length - 1], AllowMultiByteIdentifier);
        }

        public static bool IsVariableName(string name) {
            return !String.IsNullOrEmpty(name)
                && IsIdentifierInitial(name[0], AllowMultiByteIdentifier)
                && IsVariableName(name, 1, 0, AllowMultiByteIdentifier);
        }

        public static bool IsMethodName(string name) {
            return !String.IsNullOrEmpty(name)
                && IsIdentifierInitial(name[0], AllowMultiByteIdentifier)
                && IsVariableName(name, 1, 1, AllowMultiByteIdentifier)
                && IsMethodNameSuffix(name[name.Length - 1], AllowMultiByteIdentifier);
        }

        public static bool IsInstanceVariableName(string name) {
            return name != null && name.Length >= 2
                && name[0] == '@'
                && IsVariableName(name, 1, 0, AllowMultiByteIdentifier);
        }

        public static bool IsClassVariableName(string name) {
            return name != null && name.Length >= 3
                && name[0] == '@'
                && name[1] == '@'
                && IsVariableName(name, 2, 0, AllowMultiByteIdentifier);
        }

        public static bool IsGlobalVariableName(string name) {
            return name != null && name.Length >= 2
                && name[0] == '$'
                && IsVariableName(name, 1, 0, AllowMultiByteIdentifier);
        }

        private static bool IsVariableName(string name, int trimStart, int trimEnd, int multiByteIdentifier) {
            for (int i = trimStart; i < name.Length - trimEnd; i++) {
                if (!IsIdentifier(name[i], multiByteIdentifier)) {
                    return false;
                }
            }

            return true;
        }

        public static bool IsOperatorName(string/*!*/ name) {
            if (name.Length <= 3) {
                switch (name) {
                    case "|":
                    case "^":
                    case "&":
                    case "<=>":
                    case "==":
                    case "===":
                    case "=~":
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                    case "<<":
                    case ">>":
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "%":
                    case "**":
                    case "~":
                    case "+@":
                    case "-@":
                    case "[]":
                    case "[]=":
                    case "`":
                        return true;
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Tokenizer Service

        public override object CurrentState {
            get { return _state; }
        }

        public override ErrorSink/*!*/ ErrorSink {
            get { return _errorSink; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _errorSink = value;
            }
        }

        public override bool IsRestartable {
            get { return true; }
        }

        public override SourceLocation CurrentPosition {
            get { return _tokenSpan.End; }
        }

        public override bool SkipToken() {
            return GetNextToken() != Tokens.EndOfFile;
        }

        public override TokenInfo ReadToken() {
            Tokens token = GetNextToken();
            TokenInfo result = GetTokenInfo(token);
            result.SourceSpan = TokenSpan;
            return result;
        }

        internal static TokenInfo GetTokenInfo(Tokens token) {
            TokenInfo result = new TokenInfo();
            switch (token) {
                case Tokens.Undef:
                case Tokens.Rescue:
                case Tokens.Ensure:
                case Tokens.If:
                case Tokens.Unless:
                case Tokens.Then:
                case Tokens.Elsif:
                case Tokens.Else:
                case Tokens.Case:
                case Tokens.When:
                case Tokens.While:
                case Tokens.Until:
                case Tokens.For:
                case Tokens.Break:
                case Tokens.Next:
                case Tokens.Redo:
                case Tokens.Retry:
                case Tokens.In:
                case Tokens.Return:
                case Tokens.Yield:
                case Tokens.Super:
                case Tokens.Self:
                case Tokens.Nil:
                case Tokens.True:
                case Tokens.False:
                case Tokens.And:
                case Tokens.Or:
                case Tokens.Not:
                case Tokens.IfMod:
                case Tokens.UnlessMod:
                case Tokens.WhileMod:
                case Tokens.UntilMod:
                case Tokens.RescueMod:
                case Tokens.Alias:
                case Tokens.Defined:
                case Tokens.Line:
                case Tokens.File:
                case Tokens.Encoding:
                    result.Category = TokenCategory.Keyword;
                    break;

                case Tokens.Def:
                case Tokens.Class:
                case Tokens.Module:
                case Tokens.End:
                case Tokens.Begin:
                case Tokens.UppercaseBegin:
                case Tokens.UppercaseEnd:
                case Tokens.Do:
                case Tokens.LoopDo:
                case Tokens.BlockDo:
                case Tokens.LambdaDo:
                    result.Category = TokenCategory.Keyword;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case Tokens.Plus:                  // +
                case Tokens.UnaryPlus:             // +@
                case Tokens.Minus:                 // -
                case Tokens.UnaryMinus:            // -@
                case Tokens.NumberNegation:        // -<number>
                case Tokens.Pow:                   // **
                case Tokens.Cmp:                   // <=>
                case Tokens.Equal:                 // ==
                case Tokens.StrictEqual:           // ===
                case Tokens.NotEqual:              // !=
                case Tokens.Greater:               // >
                case Tokens.GreaterOrEqual:        // >=
                case Tokens.Less:                  // <
                case Tokens.LessOrEqual:           // <=
                case Tokens.LogicalAnd:            // &&
                case Tokens.LogicalOr:             // ||
                case Tokens.Match:                 // =~
                case Tokens.Nmatch:                // !~
                case Tokens.DoubleDot:             // ..
                case Tokens.TripleDot:             // ...
                case Tokens.ItemGetter:            // []
                case Tokens.ItemSetter:            // []=
                case Tokens.Lshft:                 // <<
                case Tokens.Rshft:                 // >>
                case Tokens.DoubleArrow:           // =>
                case Tokens.Lambda:                // ->
                case Tokens.Star:                  // *<arg>
                case Tokens.Asterisk:              // <expr> * <expr>
                case Tokens.BlockReference:        // &<arg>
                case Tokens.Ampersand:             // <expr> & <expr>
                case Tokens.Percent:               // <expr> % <expr>
                case Tokens.Assignment:            // =
                case Tokens.OpAssignment:          // +=, -=, ...
                case Tokens.Caret:                 // ^
                case Tokens.Colon:                 // :
                case Tokens.QuestionMark:          // ?
                case Tokens.Bang:                  // !
                case Tokens.Slash:                 // /
                case Tokens.Tilde:                 // ~
                case Tokens.Backtick:              // `
                    result.Category = TokenCategory.Operator;
                    break;

                case Tokens.SeparatingDoubleColon: // <expr>::<expr>
                case Tokens.LeadingDoubleColon:    // ::<expr>
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case Tokens.LeftBracket:             // [
                case Tokens.LeftIndexingBracket:     // [
                case Tokens.RightBracket:            // ]
                case Tokens.LeftBrace:               // {
                case Tokens.LeftBlockArgBrace:       // {
                case Tokens.LeftBlockBrace:          // {
                case Tokens.LeftLambdaBrace:         // {
                case Tokens.RightBrace:              // }
                case Tokens.Pipe:                    // |
                case Tokens.StringEmbeddedCodeBegin: // #{ in string
                case Tokens.StringEmbeddedCodeEnd:   // } in string
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case Tokens.LeftParenthesis:       // (
                case Tokens.LeftArgParenthesis:    // ( in argument
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
                    break;

                case Tokens.LeftExprParenthesis:   // ( in expression
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case Tokens.RightParenthesis:      // )
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
                    break;

                case Tokens.Comma:                 // ,
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.ParameterNext;
                    break;

                case Tokens.Dot:                   // .
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case Tokens.Character:
                case Tokens.StringEnd:
                case Tokens.VerbatimHeredocBegin:
                case Tokens.VerbatimHeredocEnd:
                    result.Category = TokenCategory.StringLiteral;
                    break;

                case Tokens.StringEmbeddedVariableBegin: // # in string followed by @ or $
                case Tokens.Semicolon:                   // ;
                case Tokens.WordSeparator:               // <whitespace>
                    result.Category = TokenCategory.Delimiter;
                    break;

                case Tokens.SymbolBegin:
                case Tokens.Label:
                case Tokens.Identifier:
                case Tokens.FunctionIdentifier:
                case Tokens.GlobalVariable:
                case Tokens.InstanceVariable:
                case Tokens.ConstantIdentifier:
                case Tokens.ClassVariable:
                case Tokens.MatchReference:
                    result.Category = TokenCategory.Identifier;
                    break;

                case Tokens.Integer:
                case Tokens.Float:
                case Tokens.BigInteger:
                    result.Category = TokenCategory.NumericLiteral;
                    break;

                case Tokens.StringContent:
                case Tokens.StringBegin:
                case Tokens.ShellStringBegin:
                case Tokens.WordsBegin:
                case Tokens.VerbatimWordsBegin:
                case Tokens.RegexpBegin:
                case Tokens.RegexpEnd:
                    // TODO: distingush various kinds of string content (regex, string, heredoc, symbols, words)
                    result.Category = TokenCategory.StringLiteral;
                    break;

                case Tokens.Pound:
                    result.Category = TokenCategory.LineComment;
                    break;

                case Tokens.EndOfFile:
                    result.Category = TokenCategory.EndOfStream;
                    break;

                case Tokens.NewLine:
                case Tokens.EndOfLine:
                case Tokens.Whitespace:
                    result.Category = TokenCategory.WhiteSpace;
                    break;

                case Tokens.SingleLineComment:
                    result.Category = TokenCategory.LineComment;
                    break;

                case Tokens.MultiLineComment:
                    result.Category = TokenCategory.Comment;
                    break;

                case Tokens.Error:
                case Tokens.Backslash:
                case Tokens.At:
                case Tokens.Dollar:
                case Tokens.InvalidCharacter:
                    result.Category = TokenCategory.Error;
                    break;

                default:
                    throw Assert.Unreachable;
            }

            return result;
        }

        internal static string/*!*/ GetTokenDescription(Tokens token) {
            switch (token) {
                case Tokens.Undef:                       return "`undef'";
                case Tokens.Rescue:                      
                case Tokens.RescueMod:                   return "`rescue'";
                case Tokens.Ensure:                      return "`ensure'";
                case Tokens.If:                          
                case Tokens.IfMod:                       return "`if'";
                case Tokens.Unless:                      
                case Tokens.UnlessMod:                   return "`unless'";
                case Tokens.Then:                        return "`then'";
                case Tokens.Elsif:                       return "`elsif'";
                case Tokens.Else:                        return "`else'";
                case Tokens.Case:                        return "`case'";
                case Tokens.When:                        return "`when'";
                case Tokens.While:                       
                case Tokens.WhileMod:                    return "`while'";
                case Tokens.Until:                       
                case Tokens.UntilMod:                    return "`until'";
                case Tokens.For:                         return "`for'";
                case Tokens.Break:                       return "`break'";
                case Tokens.Next:                        return "`next'";
                case Tokens.Redo:                        return "`redo'";
                case Tokens.Retry:                       return "`retry'";
                case Tokens.In:                          return "`in'";
                case Tokens.Return:                      return "`return'";
                case Tokens.Yield:                       return "`yield'";
                case Tokens.Super:                       return "`super'";
                case Tokens.Self:                        return "`self'";
                case Tokens.Nil:                         return "`nil'";
                case Tokens.True:                        return "`true'";
                case Tokens.False:                       return "`false'";
                case Tokens.And:                         return "`and'";
                case Tokens.Or:                          return "`or'";
                case Tokens.Not:                         return "`not'";
                case Tokens.Alias:                       return "`alias'";
                case Tokens.Defined:                     return "`defined'";
                case Tokens.Line:                        return "`__LINE__'";
                case Tokens.File:                        return "`__FILE__'";
                case Tokens.Encoding:                    return "`__ENCODING__'";
                case Tokens.Def:                         return "`def'";
                case Tokens.Class:                       return "`class'";
                case Tokens.Module:                      return "`module'";
                case Tokens.End:                         return "`end'";
                case Tokens.Begin:                       return "`begin'";
                case Tokens.UppercaseBegin:              return "`BEGIN'";
                case Tokens.UppercaseEnd:                return "`END'";
                case Tokens.Do:                          
                case Tokens.LoopDo:                      
                case Tokens.BlockDo:                     
                case Tokens.LambdaDo:                    return "`do'";
                case Tokens.Plus:                        return "`+'";
                case Tokens.UnaryPlus:                   return "`+@'";
                case Tokens.Minus:                       return "`-'";
                case Tokens.UnaryMinus:                  return "`-@'";
                case Tokens.Pow:                         return "`**'";
                case Tokens.Cmp:                         return "`<=>'";
                case Tokens.Equal:                       return "`=='";
                case Tokens.StrictEqual:                 return "`==='";
                case Tokens.NotEqual:                    return "`!='";
                case Tokens.Greater:                     return "`>'";
                case Tokens.GreaterOrEqual:              return "`>='";
                case Tokens.Less:                        return "`<'";
                case Tokens.LessOrEqual:                 return "`<'";
                case Tokens.LogicalAnd:                  return "`&&'";
                case Tokens.LogicalOr:                   return "`||'";
                case Tokens.Match:                       return "`=~'";
                case Tokens.Nmatch:                      return "`!~'";
                case Tokens.DoubleDot:                   return "`..'";
                case Tokens.TripleDot:                   return "`...'";
                case Tokens.ItemGetter:                  return "`[]'";
                case Tokens.ItemSetter:                  return "`[]='";
                case Tokens.Lshft:                       return "`<<'";
                case Tokens.Rshft:                       return "`>>'";
                case Tokens.DoubleArrow:                 return "`=>'";
                case Tokens.Lambda:                      return "`->'";
                case Tokens.Star:                        
                case Tokens.Asterisk:                    return "`*'";
                case Tokens.BlockReference:              
                case Tokens.Ampersand:                   return "`&'";
                case Tokens.Percent:                     return "`%'";
                case Tokens.Assignment:                  return "`='";
                case Tokens.Caret:                       return "`^'";
                case Tokens.Colon:                       return "`:'";
                case Tokens.QuestionMark:                return "`?'";
                case Tokens.Bang:                        return "`!'";
                case Tokens.Slash:                       return "`/'";
                case Tokens.Tilde:                       return "`~'";
                case Tokens.Backtick:                    return "`";
                case Tokens.SeparatingDoubleColon:       
                case Tokens.LeadingDoubleColon:          return "`::'";
                case Tokens.LeftBracket:                 
                case Tokens.LeftIndexingBracket:         return "`['";
                case Tokens.RightBracket:                return "`]'";
                case Tokens.LeftBrace:                   
                case Tokens.LeftBlockArgBrace:           
                case Tokens.LeftBlockBrace:              
                case Tokens.LeftLambdaBrace:             return "`{'";
                case Tokens.RightBrace:                  return "`}'";
                case Tokens.Pipe:                        return "`|'";
                case Tokens.LeftParenthesis:             
                case Tokens.LeftArgParenthesis:          
                case Tokens.LeftExprParenthesis:         return "`('";
                case Tokens.RightParenthesis:            return "`)'";
                case Tokens.Comma:                       return "`,'";
                case Tokens.Dot:                         return "`.'";
                case Tokens.Semicolon:                   return "`;'";

                case Tokens.Character:                   return "character escape (?...)";
                case Tokens.NumberNegation:              return "negative number";
                case Tokens.OpAssignment:                return "assignment with operation";
                case Tokens.StringEmbeddedVariableBegin: return "`#@' or `#$'"; 
                case Tokens.StringEmbeddedCodeBegin:     return "`#{'";
                case Tokens.StringEmbeddedCodeEnd:       return "`}'";
                case Tokens.Label:                       return "label";
                case Tokens.Identifier:                  return "identifier";
                case Tokens.FunctionIdentifier:          return "function name";
                case Tokens.GlobalVariable:              return "global variable";
                case Tokens.InstanceVariable:            return "instance variable";
                case Tokens.ConstantIdentifier:          return "constant";
                case Tokens.ClassVariable:               return "class variable";
                case Tokens.MatchReference:              return "$&, $`, $', $+, or $1-9";
                case Tokens.Integer:                     return "integer";
                case Tokens.BigInteger:                  return "big integer";
                case Tokens.Float:                       return "float";
                case Tokens.StringBegin:                 return "quote";
                case Tokens.VerbatimHeredocBegin:        return "heredoc start";
                case Tokens.StringContent:               return "string content";
                case Tokens.StringEnd:                   return "string terminator";
                case Tokens.VerbatimHeredocEnd:          return "heredoc terminator";
                case Tokens.ShellStringBegin:            return "shell command (`...`)";
                case Tokens.SymbolBegin:                 return "symbol";
                case Tokens.WordsBegin:                  return "`%W'";
                case Tokens.VerbatimWordsBegin:          return "`%w'";
                case Tokens.WordSeparator:               return "word separator";
                case Tokens.RegexpBegin:                 return "regex start";
                case Tokens.RegexpEnd:                   return "regex end";
                case Tokens.Pound:                       return "`#'";
                case Tokens.EndOfFile:                   return "end of file";
                case Tokens.NewLine:                     
                case Tokens.EndOfLine:                   return "end of line";
                case Tokens.Whitespace:                  return "space";
                case Tokens.SingleLineComment:           return "# comment";
                case Tokens.MultiLineComment:            return "=begin ... =end";
                case Tokens.Error:                       return "error";
                case Tokens.Backslash:                   return "`\\'";
                case Tokens.At:                          return "`@'";
                case Tokens.Dollar:                      return "`$'";
                case Tokens.InvalidCharacter:            return "invalid character";
                default:
                    throw Assert.Unreachable;
            }
        }

        #endregion
    }
}
