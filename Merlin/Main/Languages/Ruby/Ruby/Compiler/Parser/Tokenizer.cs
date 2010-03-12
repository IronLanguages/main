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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Ast;
using System.Text.RegularExpressions;
using IronRuby.Runtime;

namespace IronRuby.Compiler {
    internal enum LexicalState {
        EXPR_BEG,			// ignore newline, +/- is a sign.
        EXPR_END,			// newline significant, +/- is a operator.
        EXPR_ARG,			// newline significant, +/- is a operator.
        EXPR_CMDARG,		// newline significant, +/- is a operator.
        EXPR_ENDARG,		// newline significant, +/- is a operator.
        EXPR_MID,			// newline significant, +/- is a operator.
        EXPR_FNAME,			// ignore newline, no reserved words.
        EXPR_DOT,			// right after `.' or `::', no reserved words.
        EXPR_CLASS,			// immediate after `class', no here document.
    };

    public class Tokenizer : TokenizerService {
        private const int InitialBufferSize = 80;

        public bool ForceBinaryMultiByte { get; set; }

        public bool AllowNonAsciiIdentifiers {
            get { return _multiByteIdentifier < Int32.MaxValue; }
            set { _multiByteIdentifier = AllowMultiByteIdentifier(value); }
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
        private bool _verbatim;
        private int _multiByteIdentifier = Int32.MaxValue;

        private SourceUnit _sourceUnit;
        private ErrorSink/*!*/ _errorSink;
        private BignumParser _bigIntParser;
        private ILexicalVariableResolver/*!*/ _localVariableResolver;
        
        #region State

        private LexicalState _lexicalState;
        private bool _commaStart = true;
        private StringTokenizer _currentString = null;
        private int _cmdArgStack = 0;
        private int _condStack = 0;

        // Non-zero => End of the last heredoc that finished reading content.
        // While non-zero the current stream position doesn't correspond the current line and line index 
        // (the stream is ahead, we are reading from a buffer restored by the last heredoc).
        private int _heredocEndLine;
        private int _heredocEndLineIndex = -1;

        #endregion

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
        private int _dataOffset = -1;
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

            _input = reader;
            _initialLocation = initialLocation;
            _currentLine = _initialLocation.Line;
            _currentLineIndex = _initialLocation.Index;
            _tokenSpan = new SourceSpan(initialLocation, initialLocation);

            SetState(LexicalState.EXPR_BEG);

            _tokenValue = new TokenValue();
            _eofReached = false;
            _unterminatedToken = false;

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
                Parser.TerminalToString((int)token),
                _tokenValue.ToString(),
                _lexicalState);
#endif
        }

        #endregion

        #region Parser API, State Operations

        internal LexicalState LexicalState {
            get { return _lexicalState; }
        }
        
        private bool IS_ARG() {
            return _lexicalState == LexicalState.EXPR_ARG || _lexicalState == LexicalState.EXPR_CMDARG;
        }

        internal void SetState(LexicalState state) {
            _lexicalState = state;
        }

        internal int CMDARG
        {
            get { return _cmdArgStack; }
            set { _cmdArgStack = value; }
        }
        
        internal void CMDARG_PUSH(int n) { 
            BITSTACK_PUSH(ref _cmdArgStack, n); 
        }

        internal int CMDARG_POP() { 
            return BITSTACK_POP(ref _cmdArgStack); 
        }

        internal void CMDARG_LEXPOP() { 
            BITSTACK_LEXPOP(ref _cmdArgStack); 
        }

        internal bool CMDARG_P() { 
            return BITSTACK_SET_P(_cmdArgStack); 
        }

        // Push(n)
        private void BITSTACK_PUSH(ref int stack, int n) {
            stack = (stack << 1) | ((n) & 1);
        }

        // Pop()
        private int BITSTACK_POP(ref int stack) {
            return (stack >>= 1);
        }

        // x = Pop(), Top |= x
        private void BITSTACK_LEXPOP(ref int stack) {
            stack = (stack >> 1) | (stack & 1);
        }

        // Peek() != 0
        private bool BITSTACK_SET_P(int stack) {
            return (stack & 1) != 0;
        }

        internal void COND_PUSH(int n) {
            BITSTACK_PUSH(ref _condStack, n);
        }

        internal int COND_POP() {
            return BITSTACK_POP(ref _condStack);
        }

        internal void COND_LEXPOP() {
            // this case cannot happen, since it would require closing parenthesis not matching opening
            // ( ... while [[ ... ) ... do ]]

            // was: BITSTACK_LEXPOP(ref cond_stack);
            COND_POP();
        }

        internal bool COND_P() {
            return BITSTACK_SET_P(_condStack);
        }

        // Stores the current string tokenizer into the StringEmbeddedVariableBegin token.
        // It is restored later via call to StringEmbeddedVariableEnd. 
        private Tokens StringEmbeddedVariableBegin() {
            _tokenValue.SetStringTokenizer(_currentString);
            _currentString = null;
            SetState(LexicalState.EXPR_BEG);
            return Tokens.StringEmbeddedVariableBegin;
        }

        // Stores the current string tokenizer into the StringEmbeddedCodeBegin token.
        // It is restored later via call to StringEmbeddedCodeEnd. 
        private Tokens StringEmbeddedCodeBegin() {
            _tokenValue.SetStringTokenizer(_currentString);
            _currentString = null;
            SetState(LexicalState.EXPR_BEG);
            COND_PUSH(0);
            CMDARG_PUSH(0);
            return Tokens.StringEmbeddedCodeBegin;
        }

        // called from parser at the end of the embedded variable
        internal void StringEmbeddedVariableEnd(StringTokenizer stringTokenizer) {
            _currentString = stringTokenizer;
        }

        // called from parser at the end of the embedded code
        internal void StringEmbeddedCodeEnd(StringTokenizer terminator) {
            _currentString = terminator;
            COND_LEXPOP();
            CMDARG_LEXPOP();
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

        internal void ReportWarning(ErrorInfo info) {
            Report(info.GetMessage(), info.Code, GetCurrentSpan(), Severity.Warning);
        }

        internal void ReportWarning(ErrorInfo info, params object[] args) {
            Report(info.GetMessage(args), info.Code, GetCurrentSpan(), Severity.Warning);
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
                if (_heredocEndLine > 0) {
                    _currentLine = _heredocEndLine + 1;
                    _currentLineIndex = _heredocEndLineIndex;
                    _heredocEndLine = 0;
                    _heredocEndLineIndex = -1;
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

        private bool is_bol() {
            return _bufferPos == 0;
        }

        private bool was_bol() {
            return _bufferPos == 1;
        }

        private bool LineContentEquals(string str, bool skipWhitespace) {
            int p = 0;
            int n;

            if (skipWhitespace) {
                while (p < _lineLength && IsWhiteSpace(_lineBuffer[p])) {
                    p++;
                }
            }

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
        
        private void MarkTokenStart() {
            _currentTokenStart = GetCurrentLocation();
            _currentTokenStartIndex = _bufferPos;
        }

        private SourceLocation GetCurrentLocation() {
            if (_lineBuffer != null) {
                return new SourceLocation(_currentLineIndex + _bufferPos, _currentLine, _bufferPos + 1);
            } else {
                return _initialLocation;
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

            if (_currentString != null) {
                // TODO:
                RefillBuffer();

                Tokens token = _currentString.Tokenize(this);
                if (token == Tokens.StringEnd || token == Tokens.RegexpEnd) {
                    _currentString = null;
                    _lexicalState = LexicalState.EXPR_END;
                }
                _tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
                DumpToken(token);
                return token;
            }

            bool whitespaceSeen = false;
            bool cmdState = _commaStart;
            _commaStart = false;

            while (true) {
                // TODO:
                RefillBuffer();

                Tokens token = Tokenize(whitespaceSeen, cmdState);
            
                _tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
                DumpToken(token);
                
                // ignored tokens:
                switch (token) {
                    case Tokens.MultiLineComment:
                    case Tokens.SingleLineComment:
                        break;

                    case Tokens.Whitespace:
                        whitespaceSeen = true;
                        break;

                    case Tokens.EndOfLine: // not considered whitespace
                        break;

                    case Tokens.EndOfFile:
                        _eofReached = true;
                        return token;

                    default:
                        return token;
                }

                if (_verbatim) {
                    return token;
                }
            }
        }

        private Tokens Tokenize(bool whitespaceSeen, bool cmdState) {
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
                    return MarkSingleLineTokenEnd(ReadStar(whitespaceSeen));

                case '!':
                    return MarkSingleLineTokenEnd(ReadBang());

                case '=': 
                    if (ReadMultiLineComment()) {
                        MarkMultiLineTokenEnd();
                        return Tokens.MultiLineComment;
                    }

                    return MarkSingleLineTokenEnd(ReadEquals());

                case '<':
                    return TokenizeLessThan(whitespaceSeen);

                case '>':
                    return MarkSingleLineTokenEnd(ReadGreaterThan());

                case '"':
                    return MarkSingleLineTokenEnd(ReadDoubleQuote());

                case '\'':
                    return MarkSingleLineTokenEnd(ReadSingleQuote());

                case '`':
                    return MarkSingleLineTokenEnd(ReadBacktick(cmdState));

                case '?':
                    return TokenizeQuestionmark();

                case '&':
                    return MarkSingleLineTokenEnd(ReadAmpersand(whitespaceSeen));

                case '|':
                    return MarkSingleLineTokenEnd(ReadPipe());

                case '+':
                    return MarkSingleLineTokenEnd(ReadPlus(whitespaceSeen));

                case '-':
                    return MarkSingleLineTokenEnd(ReadMinus(whitespaceSeen));

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
                    return MarkSingleLineTokenEnd(ReadColon(whitespaceSeen));

                case '/':
                    return MarkSingleLineTokenEnd(ReadSlash(whitespaceSeen));

                case '^':
                    return MarkSingleLineTokenEnd(ReadCaret());

                case ';':
                    _commaStart = true;
                    _lexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return (Tokens)';';

                case ',':
                    _lexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return (Tokens)',';

                case '~':
                    return MarkSingleLineTokenEnd(ReadTilde());

                case '(':
                    _commaStart = true;
                    return MarkSingleLineTokenEnd(ReadLeftParenthesis(whitespaceSeen));

                case '[':
                    return MarkSingleLineTokenEnd(ReadLeftBracket(whitespaceSeen));

                case '{':
                    return MarkSingleLineTokenEnd(ReadLeftBrace());

                case ')':
                case ']':
                case '}':
                    COND_LEXPOP();
                    CMDARG_LEXPOP();
                    _lexicalState = LexicalState.EXPR_END;
                    MarkSingleLineTokenEnd();
                    return (Tokens)c;

                case '%':
                    return TokenizePercent(whitespaceSeen);

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
                    return MarkSingleLineTokenEnd(ReadIdentifier(c, cmdState));

                default:
                    if (!IsIdentifierInitial(c, _multiByteIdentifier)) {
                        // UTF-8 BOM detection:
                        if (_compatibility < RubyCompatibility.Ruby19 && _currentLineIndex == 0 && _bufferPos == 1 &&
                            (c == 0xEF && Peek() == 0xBB && Peek(1) == 0xBF)) {
                            ReportWarning(Errors.ByteOrderMarkIgnored);
                            // skip BOM and continue parsing as if it was whitespace:
                            Read();
                            Read();
                            MarkSingleLineTokenEnd();
                            return Tokens.Whitespace;
                        } else {
                            ReportError(Errors.InvalidCharacterInExpression, (char)c);
                            MarkSingleLineTokenEnd();
                            return Tokens.InvalidCharacter;
                        }
                    } else if (c == 0xfeff && _encoding == RubyEncoding.KCodeUTF8 && _currentLineIndex == 0 && _bufferPos == 1) {
                        ReportWarning(Errors.ByteOrderMarkIgnored);
                        // skip BOM and continue parsing as if it was whitespace:
                        MarkSingleLineTokenEnd();
                        return Tokens.Whitespace;
                    }

                    return MarkSingleLineTokenEnd(ReadIdentifier(c, cmdState));
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
            if (_lexicalState == LexicalState.EXPR_BEG ||
                _lexicalState == LexicalState.EXPR_FNAME ||
                _lexicalState == LexicalState.EXPR_DOT ||
                _lexicalState == LexicalState.EXPR_CLASS) {

                return Tokens.EndOfLine;
            }

            _commaStart = true;
            _lexicalState = LexicalState.EXPR_BEG;
            return (Tokens)'\n';
        }

        private Tokens TokenizeBackslash() {
            // escaped eoln is considered whitespace:
            if (TryReadEndOfLine()) {
                MarkMultiLineTokenEnd();
                return Tokens.Whitespace;
            }

            MarkSingleLineTokenEnd();
            return (Tokens)'\\';
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
        private Tokens ReadIdentifier(int firstCharacter, bool cmdState) {
            // the first character already read:
            int start = _bufferPos - 1;
            SkipVariableName();

            // reads token suffix (!, ?, =) and returns the the token kind based upon the suffix:
            Tokens result = ReadIdentifierSuffix(firstCharacter);

            // TODO: possible optimization: ~15% are keywords, ~15% are existing local variables -> we can save allocations
            string identifier = new String(_lineBuffer, start, _bufferPos - start);
            
            if (_lexicalState != LexicalState.EXPR_DOT) {
                if (_lexicalState == LexicalState.EXPR_FNAME) {
                    SetStringToken(identifier);
                }

                Tokens keyword = StringToKeyword(identifier);
                if (keyword != Tokens.None) {
                    return keyword;
                }
            }

            if (_lexicalState == LexicalState.EXPR_BEG ||
                _lexicalState == LexicalState.EXPR_MID ||
                _lexicalState == LexicalState.EXPR_DOT ||
                _lexicalState == LexicalState.EXPR_ARG ||
                _lexicalState == LexicalState.EXPR_CMDARG) {

                if (_localVariableResolver.IsLocalVariable(identifier)) {
                    _lexicalState = LexicalState.EXPR_END;
                } else if (cmdState) {
                    _lexicalState = LexicalState.EXPR_CMDARG;
                } else {
                    _lexicalState = LexicalState.EXPR_ARG;
                }
            } else {
                _lexicalState = LexicalState.EXPR_END;
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

            if (_lexicalState == LexicalState.EXPR_FNAME &&
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
                case "not": return ReturnKeyword(Tokens.Not, LexicalState.EXPR_BEG);
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
                    if (_compatibility >= RubyCompatibility.Ruby19) {
                        return ReturnKeyword(Tokens.Encoding, LexicalState.EXPR_END);
                    } else {
                        return Tokens.None;
                    }

                default: return Tokens.None;
            }
        }

        private Tokens ReturnKeyword(Tokens keyword, LexicalState state) {
            _lexicalState = state;
            return keyword;
        }

        private Tokens ReturnKeyword(Tokens keywordInExpression, Tokens keywordModifier, LexicalState state) {
            Debug.Assert(keywordInExpression != keywordModifier);

            if (_lexicalState == LexicalState.EXPR_BEG) {
                _lexicalState = state;
                return keywordInExpression;
            } else {
                _lexicalState = LexicalState.EXPR_BEG;
                return keywordModifier;
            }
        }

        private Tokens ReturnDoKeyword() {
            LexicalState oldState = _lexicalState;
            _lexicalState = LexicalState.EXPR_BEG;

            // if last conditional opening is a parenthesis:
            if (COND_P()) {
                return Tokens.LoopDo;
            }

            if (CMDARG_P() && oldState != LexicalState.EXPR_CMDARG) {
                return Tokens.BlockDo;
            }

            if (oldState == LexicalState.EXPR_ENDARG) {
                return Tokens.BlockDo;
            }

            return Tokens.Do;
        }      
  
        #endregion

        #region Comments

        // =begin 
        // ...
        // =end
        private bool ReadMultiLineComment() {
            if (was_bol() && PeekMultiLineCommentBegin()) {

                while (true) {
                    _bufferPos = _lineLength;

                    int c = Read();
                    if (c == -1) {
                        _unterminatedToken = true;
                        ReportError(Errors.UnterminatedEmbeddedDocument);
                        return true;
                    }

                    if (c != '=') {
                        continue;
                    }

                    if (PeekMultiLineCommentEnd()) {
                        break;
                    }
                }

                _bufferPos = _lineLength;
                return true;
            }

            return false;
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
            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            switch (Peek()) {
                case '=':
                    Skip('=');
                    return Read('=') ? Tokens.Eqq : Tokens.Eq;

                case '~':
                    Skip('~');
                    return Tokens.Match;

                case '>':
                    Skip('>');
                    return Tokens.Assoc;

                default:
                    return (Tokens)'=';
            }
        }

        // Operators: + +@
        // Assignments: +=
        // Literals: +[:number:]
        private Tokens ReadPlus(bool whitespaceSeen) {
            int c = Peek();
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                
                _lexicalState = LexicalState.EXPR_ARG;
                if (c == '@') {
                    Skip('@');
                    return Tokens.Uplus;
                }

                return (Tokens)'+';
            }

            if (c == '=') {
                Skip('=');
                SetAsciiStringToken(Symbols.Plus);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID ||
                (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c))) {

                if (IS_ARG()) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                }

                _lexicalState = LexicalState.EXPR_BEG;
                if (IsDecimalDigit(c)) {
                    Skip(c);
                    return ReadUnsignedNumber(c);
                }

                return Tokens.Uplus;
            }

            _lexicalState = LexicalState.EXPR_BEG;
            return (Tokens)'+';
        }

        // Brackets: (
        private Tokens ReadLeftParenthesis(bool whitespaceSeen) {
            Tokens result = (Tokens)'(';
            
            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                result = Tokens.LeftParen;
            } else if (whitespaceSeen) {
                if (_lexicalState == LexicalState.EXPR_CMDARG) {
                    result = Tokens.LparenArg;
                } else if (_lexicalState == LexicalState.EXPR_ARG) {
                    ReportWarning(Errors.WhitespaceBeforeArgumentParentheses);
                }
            }

            COND_PUSH(0);
            CMDARG_PUSH(0);
            _lexicalState = LexicalState.EXPR_BEG;
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
                _lexicalState = LexicalState.EXPR_END;
                return result;
            }

            return (Tokens)'@';
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
            _lexicalState = LexicalState.EXPR_END;

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
                    return (Tokens)'$';
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
        private Tokens TokenizePercent(bool whitespaceSeen) {
            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                return TokenizeQuotationStart();
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Mod);
                _lexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return Tokens.Assignment;
            }

            if (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c)) {
                return TokenizeQuotationStart();
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            MarkSingleLineTokenEnd();
            return (Tokens)'%';
        }

        // Brackets: {
        private Tokens ReadLeftBrace() {
            Tokens result;

            if (IS_ARG() || _lexicalState == LexicalState.EXPR_END) {
                result = (Tokens)'{';        // block (primary)
            } else if (_lexicalState == LexicalState.EXPR_ENDARG) {
                result = Tokens.LbraceArg;   // block (expr)
            } else {
                result = Tokens.Lbrace;      // hash
            }

            COND_PUSH(0);
            CMDARG_PUSH(0);
            _lexicalState = LexicalState.EXPR_BEG;
            return result;
        }

        // Brackets: [
        // Operators: [] []=
        private Tokens ReadLeftBracket(bool whitespaceSeen) {
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                _lexicalState = LexicalState.EXPR_ARG;
                
                return Read(']') ? (Read('=') ? Tokens.Aset : Tokens.Aref) : (Tokens)'[';
            }

            Tokens result;
            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                result = Tokens.Lbrack;
            } else if (IS_ARG() && whitespaceSeen) {
                result = Tokens.Lbrack;
            } else {
                result = (Tokens)'[';
            }

            _lexicalState = LexicalState.EXPR_BEG;
            COND_PUSH(0);
            CMDARG_PUSH(0);
            return result;
        }

        // Operators: ~ ~@
        private Tokens ReadTilde() {
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                // ~@
                Read('@');
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            return (Tokens)'~';
        }

        // Assignments: ^=
        // Operators: ^
        private Tokens ReadCaret() {
            if (Read('=')) {
                SetAsciiStringToken(Symbols.Xor);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            return (Tokens)'^';
        }

        // Operators: /
        // Assignments: /=
        // Literals: /... (regex start)
        private Tokens ReadSlash(bool whitespaceSeen) {
            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                _currentString = new StringContentTokenizer(StringType.RegularExpression | StringType.ExpandsEmbedded, '/');
                _tokenValue.SetStringTokenizer(_currentString);
                return Tokens.RegexpBegin;
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Divide);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (IS_ARG() && whitespaceSeen) {
                if (!IsWhiteSpace(c)) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                    _currentString = new StringContentTokenizer(StringType.RegularExpression | StringType.ExpandsEmbedded, '/');
                    _tokenValue.SetStringTokenizer(_currentString);
                    return Tokens.RegexpBegin;
                }
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            return (Tokens)'/';
        }

        // Operators: :: : 
        // Literals: :... (symbol start)
        private Tokens ReadColon(bool whitespaceSeen) {
            int c = Peek();
            if (c == ':') {
                Skip(c);
                if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID ||
                    _lexicalState == LexicalState.EXPR_CLASS || (IS_ARG() && whitespaceSeen)) {
                    
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.LeadingDoubleColon;
                }

                _lexicalState = LexicalState.EXPR_DOT;
                return Tokens.SeparatingDoubleColon;
            }

            if (_lexicalState == LexicalState.EXPR_END || _lexicalState == LexicalState.EXPR_ENDARG || IsWhiteSpace(c)) {
                _lexicalState = LexicalState.EXPR_BEG;
                return (Tokens)':';
            }

            switch (c) {
                case '\'':
                    Skip(c);
                    _currentString = new StringContentTokenizer(StringType.Symbol, '\'');
                    break;

                case '"':
                    Skip(c);
                    _currentString = new StringContentTokenizer(StringType.Symbol | StringType.ExpandsEmbedded, '"');
                    break;

                default:
                    Debug.Assert(_currentString == null);
                    break;
            }

            _lexicalState = LexicalState.EXPR_FNAME;
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.SymbolBegin;
        }

        // Assignments: **= *= 
        // Operators: ** * splat
        private Tokens ReadStar(bool whitespaceSeen) {
            Tokens result;

            int c = Peek();
            if (c == '*') {
                Skip(c);
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.Power);
                    _lexicalState = LexicalState.EXPR_BEG;
                    
                    return Tokens.Assignment;
                }

                result = Tokens.Pow;
            } else if (c == '=') {
                Skip(c);

                SetAsciiStringToken(Symbols.Multiply);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            } else if (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c)) {
                ReportWarning(Errors.StarInterpretedAsSplatArgument);
                result = Tokens.Star;
            } else if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                result = Tokens.Star;
            } else {
                result = (Tokens)'*';
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG;
                    break;
            }

            return result;
        }

        // Operators: ! != !~
        private Tokens ReadBang() {
            _lexicalState = LexicalState.EXPR_BEG;
            
            int c = Peek();
            if (c == '=') {
                Skip(c);
                return Tokens.Neq;
            } else if (c == '~') {
                Skip(c);
                return Tokens.Nmatch;
            }

            return (Tokens)'!';
        }

        // String: <<HEREDOC_LABEL
        // Assignment: <<=
        // Operators: << <= <=> <
        private Tokens TokenizeLessThan(bool whitespaceSeen) {
            int c = Read();

            if (c == '<' &&
                _lexicalState != LexicalState.EXPR_END &&
                _lexicalState != LexicalState.EXPR_DOT &&
                _lexicalState != LexicalState.EXPR_ENDARG &&
                _lexicalState != LexicalState.EXPR_CLASS && 
                (!IS_ARG() || whitespaceSeen)) {

                Tokens token = TokenizeHeredocLabel();
                if (token != Tokens.None) {
                    return token;
                }
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            if (c == '=') {
                if (Read('>')) {
                    MarkSingleLineTokenEnd();
                    return Tokens.Cmp;
                }
                MarkSingleLineTokenEnd();
                return Tokens.Leq;
            }

            if (c == '<') {
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.LeftShift);
                    _lexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return Tokens.Assignment;
                }
                MarkSingleLineTokenEnd();
                return Tokens.Lshft;
            }

            Back(c);
            MarkSingleLineTokenEnd();
            return (Tokens)'<';
        }

        // Assignment: >>=
        // Operators: > >= >>
        private Tokens ReadGreaterThan() {
            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG; 
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG; 
                    break;
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                return Tokens.Geq;
            }

            if (c == '>') {
                Skip(c);
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.RightShift);
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.Assignment;
                }
                return Tokens.Rshft;
            }

            return (Tokens)'>';
        }

        // String: `...
        // Operator: `
        private Tokens ReadBacktick(bool cmdState) {
            if (_lexicalState == LexicalState.EXPR_FNAME) {
                _lexicalState = LexicalState.EXPR_END;
                return (Tokens)'`';
            }

            if (_lexicalState == LexicalState.EXPR_DOT) {
                _lexicalState = (cmdState) ? LexicalState.EXPR_CMDARG : LexicalState.EXPR_ARG;
                return (Tokens)'`';
            }

            _currentString = new StringContentTokenizer(StringType.ExpandsEmbedded, '`');
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.ShellStringBegin;
        }

        // Operators: ? (conditional)
        // Literals: ?[:char:] ?{escape}
        // Errors: ?[:EOF:]
        private Tokens TokenizeQuestionmark() {
            if (_lexicalState == LexicalState.EXPR_END || _lexicalState == LexicalState.EXPR_ENDARG) {
                _lexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return (Tokens)'?';
            }

            // ?[:EOF:]
            int c = Peek();
            if (c == -1) {
                _unterminatedToken = true;
                MarkSingleLineTokenEnd();
                ReportError(Errors.IncompleteCharacter);
                return Tokens.EndOfFile;
            }

            // TODO: ?x, ?\u1234, ?\u{123456} -> string in 1.9
            // ?[:whitespace:]
            if (IsWhiteSpace(c)) {
                if (!IS_ARG()) {
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
                _lexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return (Tokens)'?';
            } 
            
            // ?{identifier}
            if ((IsLetterOrDigit(c) || c == '_') && IsIdentifier(Peek(1))) {
                _lexicalState = LexicalState.EXPR_BEG;
                MarkSingleLineTokenEnd();
                return (Tokens)'?';
            }

            Skip(c);
            
            // ?\{escape}
            if (c == '\\') {
                // TODO: ?\xx, ?\u1234, ?\u{123456} -> string in 1.9
                c = ReadEscape();

                // \M-{eoln} eats the eoln:
                MarkMultiLineTokenEnd();
            } else {
                MarkSingleLineTokenEnd();
            }

            // TODO: ?x -> string in 1.9
            c &= 0xff;
            _lexicalState = LexicalState.EXPR_END;
            _tokenValue.SetInteger(c);

            return Tokens.Integer;
        }

        // Operators: & &&
        // Assignments: &=
        private Tokens ReadAmpersand(bool whitespaceSeen) {
            int c = Peek();
            
            if (c == '&') {
                Skip(c);
                _lexicalState = LexicalState.EXPR_BEG;
                
                if (Read('=')) {
                    SetAsciiStringToken(Symbols.And);
                    return Tokens.Assignment;
                }

                return Tokens.LogicalAnd;
            } 
            
            if (c == '=') {
                Skip(c);
                _lexicalState = LexicalState.EXPR_BEG;
                SetAsciiStringToken(Symbols.BitwiseAnd);
                return Tokens.Assignment;
            }

            Tokens result;
            if (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c)) {
                // we are in command argument and there is a whitespace between ampersand: "foo &bar"
                ReportWarning(Errors.AmpersandInterpretedAsProcArgument);
                result = Tokens.Ampersand;
            } else if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                result = Tokens.Ampersand;
            } else {
                result = (Tokens)'&';
            }

            switch (_lexicalState) {
                case LexicalState.EXPR_FNAME:
                case LexicalState.EXPR_DOT:
                    _lexicalState = LexicalState.EXPR_ARG;
                    break;

                default:
                    _lexicalState = LexicalState.EXPR_BEG;
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
                _lexicalState = LexicalState.EXPR_BEG;

                if (Read('=')) {
                    SetAsciiStringToken(Symbols.Or);
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.Assignment;
                }
                return Tokens.LogicalOr;
            }

            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.BitwiseOr);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                _lexicalState = LexicalState.EXPR_ARG;
            } else {
                _lexicalState = LexicalState.EXPR_BEG;
            }

            return (Tokens)'|';
        }

        // Operators: . .. ...
        // Errors: .[:digit:]
        private Tokens ReadDot() {
            _lexicalState = LexicalState.EXPR_BEG;
            
            int c = Peek();
            if (c == '.') {
                Skip(c);
                return Read('.') ? Tokens.Dot3 : Tokens.Dot2;
            }

            if (IsDecimalDigit(c)) {
                ReportError(Errors.NoFloatingLiteral);
            }

            _lexicalState = LexicalState.EXPR_DOT;
            return (Tokens)'.';
        }

        // Operators: - -@
        // Assignments: -=
        // Literals: -... (negative number sign)
        private Tokens ReadMinus(bool whitespaceSeen) {
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                _lexicalState = LexicalState.EXPR_ARG;
                return Read('@') ? Tokens.Uminus : (Tokens)'-';
            }

            int c = Peek();
            if (c == '=') {
                Skip(c);
                SetAsciiStringToken(Symbols.Minus);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID ||
                (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c))) {

                if (IS_ARG()) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                }

                _lexicalState = LexicalState.EXPR_BEG;
                return IsDecimalDigit(c) ? Tokens.UminusNum : Tokens.Uminus;
            }

            _lexicalState = LexicalState.EXPR_BEG;
            return (Tokens)'-';
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

            // encoding is ignored in 1.9:
            if (_compatibility < RubyCompatibility.Ruby19) {
                return options | encoding;
            } else {
                return options;
            }
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
                    content.Append('\\');
                    AppendEscapedHexEscape(content);
                    break;

                case 'M':
                    if (!Read('-')) {
                        InvalidEscapeCharacter();
                        break;
                    }

                    content.Append('\\', 'M', '-');

                    // escaped:
                    AppendRegularExpressionCompositeEscape(content, term);
                    break;                    

                case 'C':
                    if (!Read('-')) {
                        InvalidEscapeCharacter();
                        break;
                    }

                    content.Append('\\', 'C', '-');

                    AppendRegularExpressionCompositeEscape(content, term);
                    break;

                case 'c':
                    content.Append('\\', 'c');
                    AppendRegularExpressionCompositeEscape(content, term);
                    break;
                    
                case -1:
                    InvalidEscapeCharacter();
                    break;

                default:
                    if (IsOctalDigit(c)) {
                        content.Append('\\');
                        AppendEscapedOctalEscape(content);
                        break;
                    }

                    if (c != '\\' || c != term) {
                        content.Append('\\');
                    }

                    // ReadEscape is not called if the backslash is followed by an eoln:
                    Debug.Assert(c != '\n' && (c != '\r' || Peek() != '\n'));
                    content.Append((char)c);
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
                content.Append((char)c);
            }
        }

        private void AppendEscapedOctalEscape(MutableStringBuilder/*!*/ content) {
            int start = _bufferPos - 1;
            ReadOctalEscape(0);

            Debug.Assert(IsOctalDigit(_lineBuffer[start])); // first digit
            content.Append(_lineBuffer, start, _bufferPos - start);
        }

        private void AppendEscapedHexEscape(MutableStringBuilder/*!*/ content) {
            int start = _bufferPos - 1;
            ReadHexEscape();

            Debug.Assert(_lineBuffer[start] == 'x');
            content.Append(_lineBuffer, start, _bufferPos - start);
        }

        private void AppendEscapedUnicode(MutableStringBuilder/*!*/ content) {
            int start = _bufferPos - 1;

            if (Peek() == '{') {
                ReadUnicodeCodePoint();
            } else {
                ReadUnicodeEscape();
            }

            Debug.Assert(_lineBuffer[start] == 'u');
            content.Append(_lineBuffer, start, _bufferPos - start);
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
        private int ReadUnicodeEscape() {
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
        private int ReadUnicodeCodePoint() {
            int c = Read();
            Debug.Assert(c == '{');

            int codepoint = 0;
            int i = 0;
            while (true) {
                c = Peek();
                if (i == 6) {
                    break;
                }

                int digit = ToDigit(c);
                if (digit >= 16) {
                    break;
                }

                codepoint = (codepoint << 4) | digit;
                i++;
                Skip(c);
            }

            if (c == '}') {
                Skip(c);
            } else {
                InvalidEscapeCharacter();
            }
            
            if (codepoint > 0x10ffff) {
                ReportError(Errors.TooLargeUnicodeCodePoint);
            }

            return codepoint;
        }

        // Reads up to 6 hex characters, treats them as a exadecimal code-point value and appends the result to the buffer.
        private void AppendUnicodeCodePoint(MutableStringBuilder/*!*/ content, StringType stringType) {
            int codepoint = ReadUnicodeCodePoint();

            if (codepoint < 0x10000) {
                // code-points [0xd800 .. 0xdffff] are not treated as invalid
                AppendCharacter(content, codepoint, stringType);
            } else {
                codepoint -= 0x10000;
                content.Append((char)((codepoint / 0x400) + 0xd800), (char)((codepoint % 0x400) + 0xdc00));
            }
        }

        public static int ToCodePoint(int highSurrogate, int lowSurrogate) {
            return (highSurrogate - 0xd800) * 0x400 + (lowSurrogate - 0xdc00) + 0x10000;
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
            _currentString = new StringContentTokenizer(StringType.ExpandsEmbedded, '"');
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.StringBegin;
        }

        // String: '...
        private Tokens ReadSingleQuote() {
            _currentString = new StringContentTokenizer(StringType.Default, '\'');
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.StringBegin;
        }

        // returns last character read
        private int ReadStringContent(MutableStringBuilder/*!*/ content, StringType stringType, int terminator, int openingParenthesis, 
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
                } else if (((stringType & StringType.ExpandsEmbedded) != 0) && c == '#' && _bufferPos < _lineLength) {
                    int c2 = _lineBuffer[_bufferPos];
                    if (c2 == '$' || c2 == '@' || c2 == '{') {
                        SeekRelative(-eolnWidth);
                        return c;
                    }
                } else if ((stringType & StringType.Words) != 0 && IsWhiteSpace(c)) {
                    SeekRelative(-eolnWidth);
                    return c;
                } else if (c == '\\') {
                    c = ReadNormalizeEndOfLine(out eolnWidth);

                    if (c == '\n') {
                        if ((stringType & StringType.Words) == 0) {
                            if ((stringType & StringType.ExpandsEmbedded) != 0) {
                                continue;
                            }
                            content.Append('\\');
                        }
                    } else if (c == '\\') {
                        if ((stringType & StringType.RegularExpression) != 0) {
                            content.Append('\\');
                        }
                    } else if ((stringType & StringType.RegularExpression) != 0) {
                        // \uFFFF, \u{codepoint}
                        if (c == 'u' && _compatibility >= RubyCompatibility.Ruby19) {
                            content.Append('\\');
                            AppendEscapedUnicode(content);
                        } else {
                            SeekRelative(-eolnWidth);
                            AppendEscapedRegexEscape(content, terminator);
                        }
                        continue;
                    } else if ((stringType & StringType.ExpandsEmbedded) != 0) {
                        if (c == 'u' && _compatibility >= RubyCompatibility.Ruby19) {
                            // TODO: if the string contains ascii characters only => it is ok and the encoding of the string will be UTF8
                            if (_encoding != RubyEncoding.UTF8) {
                                ReportError(Errors.EncodingsMixed, RubyEncoding.UTF8.Name, _encoding.Name);
                                content.Append('\\');
                                content.Append('u');
                                continue;
                            }

                            // \uFFFF, \u{codepoint}
                            if (Peek() == '{') {
                                AppendUnicodeCodePoint(content, stringType);
                                continue;
                            } else {
                                c = ReadUnicodeEscape();
                            }
                        } else {
                            // other escapes:
                            SeekRelative(-eolnWidth);
                            c = ReadEscape();
                            Debug.Assert(c <= 0xff);
                            AppendByte(content, (byte)c, stringType);
                            continue;
                        }
                    } else if ((stringType & StringType.Words) != 0 && IsWhiteSpace(c)) {
                        /* ignore backslashed spaces in %w */
                    } else if (c != terminator && !(openingParenthesis != 0 && c == openingParenthesis)) {
                        content.Append('\\');
                    }
                }

                AppendCharacter(content, c, stringType);
            }
        }

        private void AppendCharacter(MutableStringBuilder/*!*/ content, int c, StringType stringType) {
            if (c == 0 && (stringType & StringType.Symbol) != 0) {
                ReportError(Errors.NullCharacterInSymbol);
            } else {
                content.Append((char)c);
            }
        }

        private void AppendByte(MutableStringBuilder/*!*/ content, byte b, StringType stringType) {
            if (b == 0 && (stringType & StringType.Symbol) != 0) {
                ReportError(Errors.NullCharacterInSymbol);
            } else {
                content.Append(b);
            }
        }

        //
        // returns tokens: 
        // - StringEnd/RegexEnd           ... string/regex closed
        // - (Tokens)' '                  ... space in word list
        // - StringEmbeddedVariableBegin  ... #$, #@ (start of an embedded global/instance variable)
        // - StringEmbeddedCodeBegin      ... #{ (start of an embedded expression)
        // - StringContent                ... string data
        //
        internal Tokens TokenizeString(StringContentTokenizer/*!*/ info) {
            StringType stringKind = info.Properties;
            bool whitespaceSeen = false;

            // final separator in the list of words (see grammar):
            if (stringKind == StringType.FinalWordSeparator) {
                MarkTokenStart();
                MarkSingleLineTokenEnd();
                return Tokens.StringEnd;
            }

            MarkTokenStart();

            int eolnWidth;
            int c = ReadNormalizeEndOfLine(out eolnWidth);

            // unterminated string (error recovery is slightly different from MRI):
            if (c == -1) {
                ReportError(Errors.UnterminatedString);
                _unterminatedToken = true;
                MarkSingleLineTokenEnd();
                return Tokens.StringEnd;
            }

            bool isMultiline = c == '\n';

            // skip whitespace in word list:
            if ((stringKind & StringType.Words) != 0 && IsWhiteSpace(c)) {
                isMultiline |= SkipWhitespace();
                c = Read(); 
                whitespaceSeen = true;
            }

            // end of the top-level string:
            if (c == info.TerminatingCharacter && info.NestingLevel == 0) {
                
                // end of words:
                if ((stringKind & StringType.Words) != 0) {
                    // final separator in the list of words (see grammar):
                    info.Properties = StringType.FinalWordSeparator;
                    MarkTokenEnd(isMultiline);
                    return Tokens.WordSeparator;
                }

                // end of regex:
                if ((stringKind & StringType.RegularExpression) != 0) {
                    _tokenValue.SetRegexOptions(ReadRegexOptions());
                    MarkTokenEnd(isMultiline);
                    return Tokens.RegexpEnd;
                }
                
                // end of string/symbol:
                MarkTokenEnd(isMultiline);
                return Tokens.StringEnd;
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
            if ((stringKind & StringType.ExpandsEmbedded) != 0 && c == '#') {
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
                content.Append('#');
            } else {
                content = new MutableStringBuilder(_encoding);
                SeekRelative(-eolnWidth);
            }

            int nestingLevel = info.NestingLevel;
            ReadStringContent(content, stringKind, info.TerminatingCharacter, info.OpeningParenthesis, ref nestingLevel);
            info.NestingLevel = nestingLevel;

            _tokenValue.SetStringContent(content);
            MarkMultiLineTokenEnd();
            return Tokens.StringContent;
        }

        #endregion

        #region Heredoc

        private Tokens TokenizeHeredocLabel() {
            int term;
            StringType stringType = StringType.Default;

            int prefixWidth;
            int c = ReadNormalizeEndOfLine(out prefixWidth);
            if (c == '-') {
                c = ReadNormalizeEndOfLine(out prefixWidth);
                prefixWidth++;
                stringType = StringType.IndentedHeredoc;
            }

            string label;
            if (c == '\'' || c == '"' || c == '`') {
                if (c != '\'') {
                    stringType |= StringType.ExpandsEmbedded;
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
                stringType |= StringType.ExpandsEmbedded;
                
                int start = _bufferPos - 1;
                SkipVariableName();
                label = new String(_lineBuffer, start, _bufferPos - start);
            } else {
                SeekRelative(-prefixWidth);
                return Tokens.None;
            }

            // note that if we allow \n in the label we must change this to multi-line token!
            MarkSingleLineTokenEnd();
            
            // skip the rest of the line (the content is stored in heredoc string terminal and tokenized upon restore)
            int resume = _bufferPos;
            _bufferPos = _lineLength;
            _currentString = new HeredocTokenizer(stringType, label, resume, _lineBuffer, _lineLength, _currentLine, _currentLineIndex);
            _lineBuffer = new char[InitialBufferSize];
            _tokenValue.SetStringTokenizer(_currentString);

            return term == '`' ? Tokens.ShellStringBegin : Tokens.StringBegin;
        }

        private void HeredocRestore(HeredocTokenizer/*!*/ here) {
            _lineBuffer = here.ResumeLine;
            _lineLength = here.ResumeLineLength;
            _bufferPos = here.ResumePosition;
            _heredocEndLine = _currentLine;
            _heredocEndLineIndex = _currentLineIndex;
            _currentLine = here.FirstLine;
            _currentLineIndex = here.FirstLineIndex;
        }

        internal Tokens TokenizeHeredoc(HeredocTokenizer/*!*/ heredoc) {
            StringType stringKind = heredoc.Properties;
            bool isIndented = (stringKind & StringType.IndentedHeredoc) != 0;

            MarkTokenStart();

            if (Peek() == -1) {
                ReportError(Errors.UnterminatedHereDoc, heredoc.Label);
                MarkSingleLineTokenEnd();
                HeredocRestore(heredoc);
                _unterminatedToken = true;
                return Tokens.StringEnd;
            }

            // label reached - it becomes a string-end token:
            // (note that label is single line, MRI allows multiline, but such label is never matched)
            if (is_bol() && LineContentEquals(heredoc.Label, isIndented)) {
                // seek to the end of the line:
                SeekRelative(heredoc.Label.Length);

                MarkSingleLineTokenEnd();
                HeredocRestore(heredoc);

                // Zero-width token end immediately follows the heredoc opening label.
                // Prevents parser confusion when merging locations.
                //
                // [<<END][zero-width string end] ... other tokens ...
                // ... heredoc content tokens ...
                // END
                //
                MarkTokenStart();
                MarkSingleLineTokenEnd();
                return Tokens.StringEnd;
            }

            if ((stringKind & StringType.ExpandsEmbedded) == 0) {

                StringBuilder str = ReadNonexpandingHeredocContent(heredoc);

                // do not restore buffer, the next token query will invoke 'if (EOF)' or 'if (line contains label)' above:
                SetStringToken(str.ToString());
                MarkMultiLineTokenEnd();
                return Tokens.StringContent;
            }

            return TokenizeExpandingHeredocContent(heredoc);
        }

        private StringBuilder/*!*/ ReadNonexpandingHeredocContent(HeredocTokenizer/*!*/ heredoc) {
            bool isIndented = (heredoc.Properties & StringType.IndentedHeredoc) != 0;
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

        private Tokens TokenizeExpandingHeredocContent(HeredocTokenizer/*!*/ heredoc) {
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
                content.Append('#');
            } else {
                content = new MutableStringBuilder(_encoding);
            }

            bool isIndented = (heredoc.Properties & StringType.IndentedHeredoc) != 0;
            
            do {
                // read string content upto the end of the line:
                int tmp = 0;
                c = ReadStringContent(content, heredoc.Properties, '\n', 0, ref tmp);
                
                // stop reading on end-of-file or just before an embedded expression: #$, #$, #{
                if (c != '\n') {
                    break;
                }

                // adds \n
                content.Append((char)ReadNormalizeEndOfLine());

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
            StringType type;
            Tokens token;
            int terminator;

            // c is the character following %
            // note that it could be eoln in which case it needs to be normalized:
            int c = ReadNormalizeEndOfLine();
            switch (c) {
                case 'Q':
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.StringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'q':
                    type = StringType.Default;
                    token = Tokens.StringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'W':
                    type = StringType.Words | StringType.ExpandsEmbedded;
                    token = Tokens.WordsBegin;
                    // if the terminator is a whitespace the end will never be matched and syntax error will be reported
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'w':
                    type = StringType.Words;
                    token = Tokens.VerbatimWordsBegin;
                    // if the terminator is a whitespace the end will never be matched and syntax error will be reported
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'x':
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.ShellStringBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 'r':
                    type = StringType.RegularExpression | StringType.ExpandsEmbedded;
                    token = Tokens.RegexpBegin;
                    terminator = ReadNormalizeEndOfLine();
                    break;

                case 's':
                    type = StringType.Symbol;
                    token = Tokens.SymbolBegin;
                    terminator = ReadNormalizeEndOfLine();
                    _lexicalState = LexicalState.EXPR_FNAME;
                    break;

                default:
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.StringBegin;
                    terminator = c;
                    break;
            }

            int parenthesis = terminator;
            switch (terminator) {
                case -1:
                    _unterminatedToken = true;
                    MarkSingleLineTokenEnd();
                    ReportError(Errors.UnterminatedQuotedString);
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
                        return (Tokens)'%';
                    }

                    parenthesis = 0;
                    break;
            }

            bool isMultiline = terminator == '\n';

            if ((type & StringType.Words) != 0) {
                isMultiline |= SkipWhitespace();
            }

            if (isMultiline) {
                MarkMultiLineTokenEnd();
            } else {
                MarkSingleLineTokenEnd();
            }
            
            _currentString = new StringContentTokenizer(type, (char)terminator, (char)parenthesis);
            _tokenValue.SetStringTokenizer(_currentString);
            return token;
        }

        #endregion

        #region Numbers

        public sealed class BignumParser : UnsignedBigIntegerParser {
            private char[] _buffer;
            private int _position;

            public int Position { get { return _position; } set { _position = value; } }
            public char[] Buffer { get { return _buffer; } set { _buffer = value; } } // TODO: remove

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
            _lexicalState = LexicalState.EXPR_END;
           
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

        private static int AllowMultiByteIdentifier(bool allowMultiByteIdentifier) {
            // MRI 1.9 consideres all characters greater than 0x007f as identifiers.
            // Surrogate pairs are composed to a single character that is also considered an identifier.
            return allowMultiByteIdentifier ? 0x07f : Int32.MaxValue;
        }

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

        public static bool IsConstantName(string name, bool allowMultiByteCharacters) {
            int multiByteIdentifier = AllowMultiByteIdentifier(allowMultiByteCharacters);
            return !String.IsNullOrEmpty(name) 
                && IsUpperLetter(name[0])
                && IsVariableName(name, 1, 1, multiByteIdentifier)
                && IsIdentifier(name[name.Length - 1], multiByteIdentifier);
        }

        public static bool IsVariableName(string name, bool allowMultiByteCharacters) {
            int multiByteIdentifier = AllowMultiByteIdentifier(allowMultiByteCharacters);
            return !String.IsNullOrEmpty(name)
                && IsIdentifierInitial(name[0], multiByteIdentifier)
                && IsVariableName(name, 1, 0, multiByteIdentifier);
        }

        public static bool IsMethodName(string name, bool allowMultiByteCharacters) {
            int multiByteIdentifier = AllowMultiByteIdentifier(allowMultiByteCharacters);
            return !String.IsNullOrEmpty(name)
                && IsIdentifierInitial(name[0], multiByteIdentifier)
                && IsVariableName(name, 1, 1, multiByteIdentifier)
                && IsMethodNameSuffix(name[name.Length - 1], multiByteIdentifier);
        }

        public static bool IsInstanceVariableName(string name, bool allowMultiByteCharacters) {
            return name != null && name.Length >= 2
                && name[0] == '@'
                && IsVariableName(name, 1, 0, AllowMultiByteIdentifier(allowMultiByteCharacters));
        }

        public static bool IsClassVariableName(string name, bool allowMultiByteCharacters) {
            return name != null && name.Length >= 3
                && name[0] == '@'
                && name[1] == '@'
                && IsVariableName(name, 2, 0, AllowMultiByteIdentifier(allowMultiByteCharacters));
        }

        public static bool IsGlobalVariableName(string name, bool allowMultiByteCharacters) {
            return name != null && name.Length >= 2
                && name[0] == '$'
                && IsVariableName(name, 1, 0, AllowMultiByteIdentifier(allowMultiByteCharacters));
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
            get { return null; }
        }

        public override ErrorSink/*!*/ ErrorSink {
            get { return _errorSink; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _errorSink = value;
            }
        }

        public override bool IsRestartable {
            get { return false; }
        }

        public override SourceLocation CurrentPosition {
            get { return _tokenSpan.End; } // TODO: ???
        }

        public override bool SkipToken() {
            return GetNextToken() != Tokens.EndOfFile;
        }

        public override TokenInfo ReadToken() {

            TokenInfo result = new TokenInfo();

            Tokens token = GetNextToken();
            result.SourceSpan = TokenSpan;

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
                    result.Category = TokenCategory.Keyword;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case Tokens.Uplus:
                case Tokens.Uminus:
                case Tokens.UminusNum:
                case Tokens.Pow:
                case Tokens.Cmp:
                case Tokens.Eq:
                case Tokens.Eqq:
                case Tokens.Neq:
                case Tokens.Geq:
                case Tokens.Leq:
                case Tokens.LogicalAnd:
                case Tokens.LogicalOr:
                case Tokens.Match:     // =~
                case Tokens.Nmatch:    // !~
                case Tokens.Dot2:      // ..
                case Tokens.Dot3:      // ...
                case Tokens.Aref:      // []
                case Tokens.Aset:      // []=
                case Tokens.Lshft:
                case Tokens.Rshft:
                case Tokens.Assoc:
                case Tokens.Star:
                case Tokens.Ampersand:
                case Tokens.Assignment:
                    result.Category = TokenCategory.Operator;
                    break;

                case Tokens.SeparatingDoubleColon:
                case Tokens.LeadingDoubleColon:
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case Tokens.Lbrack:      // [
                case Tokens.Lbrace:      // {
                case (Tokens)'{':
                case Tokens.LbraceArg:  // <whitespacce>{
                case (Tokens)'}':
                case (Tokens)']':
                case (Tokens)'|':
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case Tokens.LeftParen:  // (
                case Tokens.LparenArg:  // <whitespace>(
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
                    break;

                case (Tokens)')':
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
                    break;

                case (Tokens)',':
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.ParameterNext;
                    break;

                case (Tokens)'.':
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case Tokens.StringEnd:
                    result.Category = TokenCategory.StringLiteral;
                    break;

                case Tokens.StringEmbeddedVariableBegin: // # in string followed by @ or $
                case Tokens.StringEmbeddedCodeBegin: // # in string followed by {
                    result.Category = TokenCategory.Delimiter;
                    break;

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
                    result.Category = TokenCategory.NumericLiteral;
                    break;

                case Tokens.StringContent:
                case Tokens.StringBegin:
                case Tokens.ShellStringBegin:
                case Tokens.SymbolBegin:
                case Tokens.WordsBegin:
                case Tokens.VerbatimWordsBegin:
                case Tokens.RegexpBegin:
                case Tokens.RegexpEnd:
                    // TODO: distingush various kinds of string content (regex, string, heredoc)
                    result.Category = TokenCategory.StringLiteral;
                    break;

                case (Tokens)'#':
                    result.Category = TokenCategory.LineComment;
                    break;

                case Tokens.EndOfFile:
                    result.Category = TokenCategory.EndOfStream;
                    break;

                case (Tokens)'\n':
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
                case Tokens.InvalidCharacter:
                    result.Category = TokenCategory.Error;
                    break;

                default:
                    result.Category = TokenCategory.None;
                    break;
            }

            return result;
        }

        #endregion
    }
}
