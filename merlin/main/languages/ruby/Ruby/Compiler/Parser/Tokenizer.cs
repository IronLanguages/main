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

namespace IronRuby.Compiler {
    public enum LexicalState {
        EXPR_BEG,			/* ignore newline, +/- is a sign. */
        EXPR_END,			/* newline significant, +/- is a operator. */
        EXPR_ARG,			/* newline significant, +/- is a operator. */
        EXPR_CMDARG,		/* newline significant, +/- is a operator. */
        EXPR_ENDARG,		/* newline significant, +/- is a operator. */
        EXPR_MID,			/* newline significant, +/- is a operator. */
        EXPR_FNAME,			/* ignore newline, no reserved words. */
        EXPR_DOT,			/* right after `.' or `::', no reserved words. */
        EXPR_CLASS,			/* immediate after `class', no here document. */
    };

    public class Tokenizer : TokenizerService {
        public sealed class BignumParser : UnsignedBigIntegerParser {
            private char[] _buffer; // TODO: TokenizerBuffer
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

        #region DLR

        private const int DefaultBufferCapacity = 1024;

        // tokenizer properties:
        private SourceUnit _sourceUnit;

        //private TokenizerBuffer _buffer;
        private ErrorSink/*!*/ _errorSink;
        private TokenValue _tokenValue;
        private bool _verbatim;
        private bool _eofReached;

        // __END__
        private int _dataOffset;

        private RubyCompatibility _compatibility;

        /// <summary>
        /// Can be <c>null</c> - unbound tokenizer.
        /// </summary>
        private Parser _parser;

        public override object CurrentState {
            get { return null; }
        }

        public SourceUnit SourceUnit {
            get { return _sourceUnit; }
        }

        public int DataOffset {
            get { return _dataOffset; } 
        }

        public override ErrorSink/*!*/ ErrorSink {
            get { return _errorSink; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _errorSink = value;
            }
        }

        public RubyCompatibility Compatibility {
            get { return _compatibility; }
            set { _compatibility = value; }
        }

        public override bool IsRestartable {
            get { return false; }
        }

        internal bool IsEndOfFile {
            get { 
                return _eofReached; // TODO: _buffer.Peek() == TokenizerBuffer.EOF;
            }
        }

        public override SourceLocation CurrentPosition {
            get { return _tokenSpan.End; } // TODO: ???
        }

        public SourceSpan TokenSpan {
            get {
                return _tokenSpan;
            }
        }

        // TODO: internal (tests)
        public TokenValue TokenValue {
            get {
                return _tokenValue;
            }
        }

        // TODO: internal (tests)
        public LexicalState LexicalState {
            get {
                return _lexicalState;
            }
        }
        
        public Tokenizer() 
            : this(true) {
        }

        public Tokenizer(bool verbatim) 
            : this(verbatim, ErrorSink.Null) {
        }

        public Tokenizer(bool verbatim, ErrorSink/*!*/ errorSink) {
            ContractUtils.RequiresNotNull(errorSink, "errorSink");

            _bigIntParser = new BignumParser();
            _errorSink = errorSink;
            _sourceUnit = null;
            _parser = null;
            _verbatim = verbatim;
            _compatibility = RubyCompatibility.Default;
//            _buffer = null;
            _initialLocation = SourceLocation.Invalid;
            _tokenSpan = SourceSpan.Invalid;
            _tokenValue = new TokenValue();
            _bufferPos = 0;
            
            // TODO:
            _input = null;
        }

        internal Tokenizer(Parser/*!*/ parser)
            : this(false) {
            _parser = parser;
        }

        public void Initialize(SourceUnit sourceUnit) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            Initialize(null, sourceUnit.GetReader(), sourceUnit, SourceLocation.MinValue, DefaultBufferCapacity);
        }

        public override void Initialize(object state, TextReader/*!*/ reader, SourceUnit/*!*/ sourceUnit, SourceLocation initialLocation) {
            Initialize(state, reader, sourceUnit, initialLocation, DefaultBufferCapacity);
        }

        public void Initialize(object state, TextReader/*!*/ reader, SourceUnit sourceUnit, SourceLocation initialLocation, int bufferCapacity) {
            ContractUtils.RequiresNotNull(reader, "reader");

            //if (state != null) {
            //    if (!(state is State)) throw new ArgumentException();
            //    _state = new State((State)state);
            //} else {
            //    _state = new State(null);
            //}

            _sourceUnit = sourceUnit;

            //_buffer = new TokenizerBuffer(sourceReader, initialLocation, bufferCapacity, true);
            _input = reader;
            _initialLocation = initialLocation;
            _tokenSpan = new SourceSpan(initialLocation, initialLocation);
            _tokenValue = new TokenValue();
            
            _eofReached = false;

            UnterminatedToken = false;

            DumpBeginningOfUnit();
        }

        /// <summary>
        /// Asks parser whether a given identifier is a local variable. 
        /// Unbound tokenizer considers any identifier a local variable.
        /// </summary>
        private bool IsLocalVariable(string/*!*/ identifier) {
            return _parser == null || _parser.CurrentScope.ResolveVariable(identifier) != null;
        }

        private string GetTokenString() {
            return _tokenString != null ? _tokenString.ToString() : "";
        }

        #endregion

        private readonly BignumParser/*!*/ _bigIntParser;
        private LexicalState _lexicalState;

        private bool _commaStart = true;


        private StringTokenizer _currentString = null;

        private TextReader _input;

        private StringBuilder yytext;

        // Entire line that is currently being tokenized.
        // Ends with \r, \n, or \r\n.
        private char[] _lineBuffer;

        // characters belonging to the token currently being read:
        private StringBuilder _tokenString = null;
        
        // index in the current buffer/line:
        private int _bufferPos = 0;
        
        // current line no:
        private int _currentLine;
        private int _currentLineIndex;

        // non-zero => end line of the current heredoc
        private int _heredocEndLine;
        private int _heredocEndLineIndex = -1;
        private SourceLocation _initialLocation;
        
        private int _cmdArgStack = 0;
        private int _condStack = 0;

        internal bool UnterminatedToken;

        // token positions set during tokenization (TODO: to be replaced by tokenizer buffer):
        private SourceLocation _currentTokenStart;
        private SourceLocation _currentTokenEnd;

        // last token span:
        private SourceSpan _tokenSpan;

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
            Log("{0,-25} {1,-25} {2,-25} {3,-20} {4}",
                Parser.TerminalToString((int)token),
                "\"" + Parser.EscapeString(GetTokenString()) + "\"",
                _tokenValue.ToString(),
                "",                 // TODO: FIX RUBY
                _lexicalState);
#endif
        }

        #endregion

        #region Parser Callbacks, State Operations

        private bool IS_ARG() {
            return _lexicalState == LexicalState.EXPR_ARG || _lexicalState == LexicalState.EXPR_CMDARG;
        }

        public void SetState(LexicalState state) {
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

        private Tokens StringEmbeddedVariableBegin() {
            _tokenValue.SetStringTokenizer(_currentString);
            _currentString = null;
            SetState(LexicalState.EXPR_BEG);
            return Tokens.StringEmbeddedVariableBegin;
        }

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

        // reads a line including eoln (any of \r, \n, \r\n)
        // returns null if no characters read (end of sream)
        // doesn't return an empty string
        private char[] lex_getline() {
            var result = new char[80];
            int size = 0;

            int c;
            for (; ; ) {
                c = _input.Read();
                if (c == -1) {
                    if (size > 0) {
                        // append eoln at the end of each file
                        //sb.Append('\n');
                        //_currentLineLength++;
                        break;
                    }
                    return null;
                }

                if (size == result.Length) {
                    Array.Resize(ref result, result.Length * 2);
                }
                result[size++] = (char)c;

                if (c == '\n') break;
                if (c == '\r' && _input.Peek() != '\n') break;
            }

            Debug.Assert(size > 0);
            Array.Resize(ref result, size);
            return result;
        }

        // reads next character from the current line, if no characters available, reads a new line to the buffer
        // skips \r if followed by \n
        // appends read character to the token value
        private int nextc() {
            if (!RefillBuffer()) {
                return -1;
            }

            Debug.Assert(0 <= _bufferPos && _bufferPos < _lineBuffer.Length);

            int c = _lineBuffer[_bufferPos];
            _bufferPos++;

            // skip \r if followed by \n
            if (c == '\r' && _bufferPos < _lineBuffer.Length && _lineBuffer[_bufferPos] == '\n') {
                _bufferPos++;
                yytext.Append((char)c); // remembers skipped \r in yytext
                c = '\n';
            }

            yytext.Append((char)c);

            return c;
        }

        private int peekc() {
            if (!RefillBuffer()) {
                return -1;
            }

            Debug.Assert(0 <= _bufferPos && _bufferPos < _lineBuffer.Length);

            int c = _lineBuffer[_bufferPos];

            // skip \r if followed by \n
            if (c == '\r' && _bufferPos < _lineBuffer.Length && _lineBuffer[_bufferPos] == '\n') {
                _bufferPos++;
                c = '\n';
            }

            return c;
        }

        private int lookahead(int i) {
            if (_lineBuffer == null) {
                if (!RefillBuffer()) {
                    return -1;
                }
            }

            if (_bufferPos + i < _lineBuffer.Length) {
                return _lineBuffer[_bufferPos + i];
            }

            return '\n';
        }

        private bool RefillBuffer() {
            Debug.Assert(_lineBuffer == null || 0 <= _bufferPos && _bufferPos <= _lineBuffer.Length);

            if (_lineBuffer == null || _bufferPos == _lineBuffer.Length) {
                var lineContent = lex_getline();

                // end of stream:
                if (lineContent == null) {
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
                    if (_lineBuffer == null) {
                        _currentLine = _initialLocation.Line;
                        _currentLineIndex = _initialLocation.Index;
                    } else {
                        _currentLine++;
                        _currentLineIndex += _lineBuffer.Length;
                    }
                }
                _lineBuffer = lineContent;
                _bufferPos = 0;
            }

            return true;
        }

        private void pushback(int c) {
            int remove = 0;
            if (yytext.Length > 0) {
                if (yytext.Length > 1 && yytext[yytext.Length - 1] == '\n' && yytext[yytext.Length - 2] == '\r') {
                    remove = 2;
                } else {
                    remove = 1;
                }
                yytext.Remove(yytext.Length - remove, remove);
            }

            if (c == -1) {
                return;
            }

            if (_bufferPos > 0) {
                _bufferPos -= remove;
            }
        }

        private bool is_bol() {
            return _bufferPos == 0;
        }

        private bool was_bol() {
            return _bufferPos == 1;
        }

        private bool peek(char c) {
            return _bufferPos < _lineBuffer.Length && c == _lineBuffer[_bufferPos];
        }

        private string tok() {
            return _tokenString.ToString();
        }

        private int toklen() {
            return _tokenString.Length;
        }

        private int toklast() {
            return (_tokenString.Length > 0 ? _tokenString[_tokenString.Length - 1] : 0);
        }

        private void newtok() {
            if (_tokenString == null) {
                _tokenString = new StringBuilder();
            } else {
                _tokenString.Length = 0;
            }
        }

        private void tokadd(char c) {
            _tokenString.Append(c);
        }

        private bool LineContentEquals(string str, bool skipWhitespace) {
            int p = 0;
            int n;

            if (skipWhitespace) {
                while (p < _lineBuffer.Length && IsWhiteSpace(_lineBuffer[p])) {
                    p++;
                }
            }

            n = _lineBuffer.Length - (p + str.Length);
            if (n < 0 || (n > 0 && _lineBuffer[p + str.Length] != '\n' && _lineBuffer[p + str.Length] != '\r')) {
                return false;
            }

            if (str == new String(_lineBuffer, p, str.Length)) {
                return true;
            }
            return false;
        }

        #endregion

        private void MarkSingleLineTokenEnd() {
            _currentTokenEnd = GetCurrentLocation();
        }

        private void MarkMultiLineTokenEnd() {
            _currentTokenEnd = GetCurrentLocation();
        }

        private Tokens MarkSingleLineTokenEnd(Tokens token) {
            MarkSingleLineTokenEnd();
            return token;
        }

        private void MarkTokenStart() {
            _currentTokenStart = GetCurrentLocation();
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

        public Tokens GetNextToken() {
            //if (_buffer == null) throw new InvalidOperationException("Uninitialized");
            if (_input == null) throw new InvalidOperationException("Uninitialized");

#if DEBUG
            _tokenValue = new TokenValue();
#endif

            Tokens result = Tokenize();

            if (result == Tokens.EndOfFile) {
                _eofReached = true;
            }

            return result;
        }

        private Tokens Tokenize() {
            yytext = new StringBuilder();
            bool whitespaceSeen = false;
            
            if (_currentString != null) {
                Tokens token = _currentString.Tokenize(this);
                if (token == Tokens.StringEnd || token == Tokens.RegexpEnd) {
                    _currentString = null;
                    _lexicalState = LexicalState.EXPR_END;
                }
                _tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
                DumpToken(token);
                return token;
            }

            bool cmdState = _commaStart;
            _commaStart = false;

            while (true) {
                Tokens token = Tokenize(whitespaceSeen, cmdState);
            
                _tokenSpan = new SourceSpan(_currentTokenStart, _currentTokenEnd);
                DumpToken(token);
                
                // ignored tokens:
                switch (token) {
                    case Tokens.MultiLineComment:
                    case Tokens.SingleLineComment:
                        if (_verbatim) {
                            return token;
                        }
                        continue;

                    case Tokens.Whitespace:
                        whitespaceSeen = true;
                        continue;

                    case Tokens.EndOfLine: // not considered whitespace
                    case Tokens.InvalidCharacter:
                        continue;
                }

                return token;
            }
        }

        private Tokens Tokenize(bool whitespaceSeen, bool cmdState) {
            peekc();
            MarkTokenStart();
            int c = nextc();

            switch (c) {
                case '\0':		// null terminates the input
                    // if tokenizer is asked for the next token it returns EOF again:
                    pushback(c);
                    MarkSingleLineTokenEnd();
                    return Tokens.EndOfFile;

                //case '\004':		/* ^D */ fixme
                //case '\032':		/* ^Z */ fixme
                case -1:		// end of stream
                    MarkSingleLineTokenEnd();
                    return Tokens.EndOfFile;

                //case '\13': /* '\v' */ fixme
                
                // whitespace
                case ' ':
                case '\t':
                case '\f':
                case '\r':
                    do {
                        c = nextc();
                    } while (c == ' ' || c == '\t' || c == '\f' || c == '\r');

                    if (c != -1) {
                        pushback(c);
                    }

                    MarkSingleLineTokenEnd();
                    return Tokens.Whitespace;

                case '\\':
                    c = nextc();

                    // escaped eoln is considered whitespace:
                    if (c == '\n') {
                        MarkMultiLineTokenEnd();
                        return Tokens.Whitespace;
                    }

                    pushback(c);
                    MarkSingleLineTokenEnd();
                    return (Tokens)'\\';

                // single-line comment
                case '#':
                    while (true) {
                        c = nextc();

                        if (c == -1 || c == '\n') {
                            if (c != -1) {
                                pushback(c);
                            }

                            MarkSingleLineTokenEnd();
                            return Tokens.SingleLineComment;
                        }
                    }

                case '\n':
                    if (_lexicalState == LexicalState.EXPR_BEG || 
                        _lexicalState == LexicalState.EXPR_FNAME ||
                        _lexicalState == LexicalState.EXPR_DOT || 
                        _lexicalState == LexicalState.EXPR_CLASS) {

                        MarkMultiLineTokenEnd();
                        return Tokens.EndOfLine;
                    }

                    _commaStart = true;
                    _lexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return (Tokens)'\n';

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
                    return MarkSingleLineTokenEnd(ReadQuestionmark());

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
                    return MarkSingleLineTokenEnd(ReadPercent(whitespaceSeen));

                case '$': 
                    return MarkSingleLineTokenEnd(ReadGlobalVariable());

                case '@':
                    return MarkSingleLineTokenEnd(ReadInstanceOrClassVariable());

                case '_':
                    if (was_bol() && LineContentEquals("__END__", false)) {
                        // if tokenizer is asked for the next token it returns EOF again:
                        pushback(c);
                        MarkSingleLineTokenEnd();
                        _dataOffset = _currentLineIndex + _lineBuffer.Length;
                        return Tokens.EndOfFile;
                    }
                    return MarkSingleLineTokenEnd(ReadIdentifier(c, cmdState));

                default:
                    if (!IsIdentifierInitial(c)) {
                        ReportError(Errors.InvalidCharacterInExpression, (char)c);
                        MarkSingleLineTokenEnd();
                        return Tokens.InvalidCharacter;
                    }

                    return MarkSingleLineTokenEnd(ReadIdentifier(c, cmdState));
            }
        }

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

            string identifier = new String(_lineBuffer, start, _bufferPos - start);
            
            if (_lexicalState != LexicalState.EXPR_DOT) {
                if (_lexicalState == LexicalState.EXPR_FNAME) {
                    _tokenValue.SetSymbol(identifier);
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

                if (IsLocalVariable(identifier)) {
                    _lexicalState = LexicalState.EXPR_END;
                } else if (cmdState) {
                    _lexicalState = LexicalState.EXPR_CMDARG;
                } else {
                    _lexicalState = LexicalState.EXPR_ARG;
                }
            } else {
                _lexicalState = LexicalState.EXPR_END;
            }
            
            _tokenValue.SetSymbol(identifier);
            return result;
        }

        private Tokens ReadIdentifierSuffix(int firstCharacter) {
            int suffix = lookahead(+0);
            int c = lookahead(+1);
            if ((suffix == '!' || suffix == '?') && c != '=') {
                nextc();
                return Tokens.FunctionIdentifier;
            }

            if (_lexicalState == LexicalState.EXPR_FNAME &&
                suffix == '=' && c != '~' && c != '>' && (c != '=' || lookahead(+2) == '>')) {
                // include '=' into the token:
                nextc();
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
                    _bufferPos = _lineBuffer.Length;

                    int c = nextc();
                    if (c == -1) {
                        UnterminatedToken = true;
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

                _bufferPos = _lineBuffer.Length;
                return true;
            }

            return false;
        }

        private bool PeekMultiLineCommentBegin() {
            int minLength = _bufferPos + 5;
            return minLength <= _lineBuffer.Length && 
                _lineBuffer[_bufferPos + 0] == 'b' &&
                _lineBuffer[_bufferPos + 1] == 'e' &&
                _lineBuffer[_bufferPos + 2] == 'g' &&
                _lineBuffer[_bufferPos + 3] == 'i' &&
                _lineBuffer[_bufferPos + 4] == 'n' &&
                (minLength == _lineBuffer.Length || IsWhiteSpace(_lineBuffer[minLength]));
        }

        private bool PeekMultiLineCommentEnd() {
            int minLength = _bufferPos + 3;
            return minLength <= _lineBuffer.Length && 
                _lineBuffer[_bufferPos + 0] == 'e' &&
                _lineBuffer[_bufferPos + 1] == 'n' &&
                _lineBuffer[_bufferPos + 2] == 'd' &&
                (minLength == _lineBuffer.Length || IsWhiteSpace(_lineBuffer[minLength]));
        }

        #endregion
        
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

            int c = nextc();
            if (c == '=') {

                c = nextc();
                if (c == '=') {
                    return Tokens.Eqq;
                }

                pushback(c);
                return Tokens.Eq;
            }

            if (c == '~') {
                return Tokens.Match;
            } else if (c == '>') {
                return Tokens.Assoc;
            }

            pushback(c);
            return (Tokens)'=';
        }

        // Operators: + +@
        // Assignments: +=
        // Literals: +[:number:]
        private Tokens ReadPlus(bool whitespaceSeen) {
            int c = nextc();
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                
                _lexicalState = LexicalState.EXPR_ARG;
                if (c == '@') {
                    return Tokens.Uplus;
                }

                pushback(c);
                return (Tokens)'+';
            }

            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.Plus);
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
                    return ReadUnsignedNumber(c);
                } else {
                    pushback(c);
                }

                return Tokens.Uplus;
            }

            _lexicalState = LexicalState.EXPR_BEG;
            pushback(c);
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

            int c = nextc();
            if (c == '@') {
                c = nextc();
                result = Tokens.ClassVariable;
            } else {
                result = Tokens.InstanceVariable;
            }

            if (IsDecimalDigit(c)) {
                ReportError(result == Tokens.InstanceVariable ? Errors.InvalidInstanceVariableName : Errors.InvalidClassVariableName, (char)c);
            } else if (IsIdentifierInitial(c)) {
                SkipVariableName();
                _tokenValue.SetSymbol(GetSymbol(start, _bufferPos - start));
                _lexicalState = LexicalState.EXPR_END;
                return result;
            }

            pushback(c);
            if (result == Tokens.ClassVariable) {
                pushback('@');
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
            
            int c = nextc();
            switch (c) {
                case '_':
                    c = nextc();
                    if (IsIdentifier(c)) {
                        SkipVariableName();
                        _tokenValue.SetSymbol(GetSymbol(start, _bufferPos - start));
                        return Tokens.GlobalVariable;
                    }
                    pushback(c);
                    return GlobalVariableToken(Symbols.LastInputLine);

                // exceptions:
                case '!': return GlobalVariableToken(Symbols.CurrentException);
                case '@': return GlobalVariableToken(Symbols.CurrentExceptionBacktrace);

                // options:
                case '-':
                    int length = IsIdentifier(nextc()) ? 2 : 1;
                    _tokenValue.SetSymbol(GetSymbol(start, length));
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
                    _tokenValue.SetInteger(RegexMatchReference.MatchPrefix);
                    return Tokens.MatchReference;

                case '\'':		
                    _tokenValue.SetInteger(RegexMatchReference.MatchSuffix);
                    return Tokens.MatchReference;

                case '+':
                    _tokenValue.SetInteger(RegexMatchReference.MatchLastGroup);
                    return Tokens.MatchReference;

                case '0':
                    c = nextc();
                    if (IsIdentifier(c)) {
                        // $0[A-Za-z0-9_] are invalid:
                        SkipVariableName();
                        ReportError(Errors.InvalidGlobalVariableName, new String(_lineBuffer, start - 1, _bufferPos - start));
                        _tokenValue.SetSymbol(Symbols.ErrorVariable);
                        return Tokens.GlobalVariable;
                    }
                    pushback(c);

                    return GlobalVariableToken(Symbols.CommandLineProgramPath);

                default:
                    if (IsDecimalDigit(c)) {
                        return ReadMatchGroupReferenceVariable(c);
                    }
                    
                    if (IsLetter(c)) {
                        SkipVariableName();
                        _tokenValue.SetSymbol(GetSymbol(start, _bufferPos - start));
                        return Tokens.GlobalVariable;
                    }

                    pushback(c);
                    return (Tokens)'$';
            }
        }

        private string/*!*/ GetSymbol(int start, int length) {
            return new String(_lineBuffer, start, length);
        }

        private Tokens ReadMatchGroupReferenceVariable(int c) {
            int start = _bufferPos - 1;
            int value = c - '0';
            bool overflow = false;

            while (true) {
                c = nextc();

                if (!IsDecimalDigit(c)) {
                    pushback(c);
                    break;
                }

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
            _tokenValue.SetSymbol(symbol);
            return Tokens.GlobalVariable;
        }

        // Assignments: %=
        // Operators: % 
        // Literals: %{... (quotation start)
        private Tokens ReadPercent(bool whitespaceSeen) {
            int c = nextc();

            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                return ReadQuotationStart(c);
            }

            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.Mod);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c)) {
                return ReadQuotationStart(c);
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

            pushback(c);
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
                
                int c = nextc();
                if (c == ']') {
                    c = nextc();
                    if (c == '=') {
                        return Tokens.Aset;
                    }
                    pushback(c);
                    return Tokens.Aref;
                }

                pushback(c);
                return (Tokens)'[';
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
                int c = nextc();
                
                // @~ is treated as ~
                if (c != '@') {
                    pushback(c);
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

            return (Tokens)'~';
        }

        // Assignments: ^=
        // Operators: ^
        private Tokens ReadCaret() {
            int c = nextc();
            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.Xor);
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

            pushback(c);
            return (Tokens)'^';
        }

        // Operators: /
        // Assignments: /=
        // Literals: /... (regex start)
        private Tokens ReadSlash(bool whitespaceSeen) {
            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                _currentString = new StringContentTokenizer(StringType.RegularExpression | StringType.ExpandsEmbedded, '/');
                _tokenValue.SetStringTokenizer(_currentString);
                return Tokens.RegexpBeg;
            }

            int c = nextc();
            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.Divide);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            pushback(c);
            if (IS_ARG() && whitespaceSeen) {
                if (!IsWhiteSpace(c)) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                    _currentString = new StringContentTokenizer(StringType.RegularExpression | StringType.ExpandsEmbedded, '/');
                    _tokenValue.SetStringTokenizer(_currentString);
                    return Tokens.RegexpBeg;
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
            int c = nextc();
            if (c == ':') {
                if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID ||
                    _lexicalState == LexicalState.EXPR_CLASS || (IS_ARG() && whitespaceSeen)) {
                    
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.LeadingDoubleColon;
                }

                _lexicalState = LexicalState.EXPR_DOT;
                return Tokens.SeparatingDoubleColon;
            }

            if (_lexicalState == LexicalState.EXPR_END || _lexicalState == LexicalState.EXPR_ENDARG || IsWhiteSpace(c)) {
                pushback(c);
                _lexicalState = LexicalState.EXPR_BEG;
                return (Tokens)':';
            }

            switch (c) {
                case '\'':
                    _currentString = new StringContentTokenizer(StringType.Symbol, '\'');
                    break;

                case '"':
                    _currentString = new StringContentTokenizer(StringType.Symbol | StringType.ExpandsEmbedded, '"');
                    break;

                default:
                    Debug.Assert(_currentString == null);
                    pushback(c);
                    break;
            }

            _lexicalState = LexicalState.EXPR_FNAME;
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.Symbeg;
        }

        // Assignments: **= *= 
        // Operators: ** * splat
        private Tokens ReadStar(bool whitespaceSeen) {
            Tokens result;
            int c = nextc();

            if (c == '*') {
                c = nextc();
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.Power);
                    _lexicalState = LexicalState.EXPR_BEG;
                    
                    return Tokens.Assignment;
                }

                pushback(c);
                result = Tokens.Pow;
            } else {
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.Multiply);
                    _lexicalState = LexicalState.EXPR_BEG;

                    return Tokens.Assignment;
                }
                
                pushback(c);
                
                if (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c)) {
                    //yywarn("`*' interpreted as argument prefix");
                    result = Tokens.Star;
                } else if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID) {
                    result = Tokens.Star;
                } else {
                    result = (Tokens)'*';
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

            return result;
        }

        // Operators: ! != !~
        private Tokens ReadBang() {
            _lexicalState = LexicalState.EXPR_BEG;
            
            int c = nextc();
            if (c == '=') {
                return Tokens.Neq;
            } else if (c == '~') {
                return Tokens.Nmatch;
            }

            pushback(c);
            return (Tokens)'!';
        }

        // String: <<HEREDOC_LABEL
        // Assignment: <<=
        // Operators: << <= <=> <
        private Tokens TokenizeLessThan(bool whitespaceSeen) {
            int c = nextc();

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
                c = nextc();
                if (c == '>') {
                    MarkSingleLineTokenEnd();
                    return Tokens.Cmp;
                }
                pushback(c);
                MarkSingleLineTokenEnd();
                return Tokens.Leq;
            }

            if (c == '<') {
                c = nextc();
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.LeftShift);
                    _lexicalState = LexicalState.EXPR_BEG;
                    MarkSingleLineTokenEnd();
                    return Tokens.Assignment;
                }
                pushback(c);
                MarkSingleLineTokenEnd();
                return Tokens.Lshft;
            }

            pushback(c);
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

            int c = nextc();
            if (c == '=') {
                return Tokens.Geq;
            }

            if (c == '>') {
                c = nextc();
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.RightShift);
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.Assignment;
                }
                pushback(c);
                return Tokens.Rshft;
            }

            pushback(c);
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
        // Literals: ?[:char:] ?\[:escape:]
        // Errors: ?[:EOF:]
        private Tokens ReadQuestionmark() {
            if (_lexicalState == LexicalState.EXPR_END || _lexicalState == LexicalState.EXPR_ENDARG) {
                _lexicalState = LexicalState.EXPR_BEG;
                return (Tokens)'?';
            }

            // ?[:EOF:]
            int c = nextc();
            if (c == -1) {
                UnterminatedToken = true;
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
                        case '\r': c2 = 'r'; break;
                        case '\f': c2 = 'f'; break;
                    }

                    if (c2 != 0) {
                        ReportWarning(Errors.InvalidCharacterSyntax, (char)c2);
                    }
                }
                pushback(c);
                _lexicalState = LexicalState.EXPR_BEG;
                return (Tokens)'?';
            } 
            
            // ?[:identifier:]
            if ((IsLetterOrDigit(c) || c == '_') && _bufferPos < _lineBuffer.Length && IsIdentifier(_lineBuffer[_bufferPos])) {
                pushback(c);
                _lexicalState = LexicalState.EXPR_BEG;
                return (Tokens)'?';
            } 
            
            // ?\[:escape:]
            if (c == '\\') {
                // TODO: ?x, ?\u1234, ?\u{123456} -> string in 1.9
                c = ReadEscape();
            }

            // TODO: ?x, ?\u1234, ?\u{123456} -> string in 1.9
            c &= 0xff;
            _lexicalState = LexicalState.EXPR_END;
            _tokenValue.SetInteger(c);

            return Tokens.Integer;
        }

        // Operators: & &&
        // Assignments: &=
        private Tokens ReadAmpersand(bool whitespaceSeen) {
            int c = nextc();
            
            if (c == '&') {
                _lexicalState = LexicalState.EXPR_BEG;
                
                c = nextc();
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.And);
                    return Tokens.Assignment;
                }

                pushback(c);
                return Tokens.BitwiseAnd;
            } 
            
            if (c == '=') {
                _lexicalState = LexicalState.EXPR_BEG;
                _tokenValue.SetSymbol(Symbols.BitwiseAnd);
                return Tokens.Assignment;
            }

            pushback(c);

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
            int c = nextc();

            if (c == '|') {
                _lexicalState = LexicalState.EXPR_BEG;

                c = nextc();
                if (c == '=') {
                    _tokenValue.SetSymbol(Symbols.Or);
                    _lexicalState = LexicalState.EXPR_BEG;
                    return Tokens.Assignment;
                }

                pushback(c);
                return Tokens.BitwiseOr;
            }

            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.BitwiseOr);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                _lexicalState = LexicalState.EXPR_ARG;
            } else {
                _lexicalState = LexicalState.EXPR_BEG;
            }

            pushback(c);
            return (Tokens)'|';
        }

        // Operators: . .. ...
        // Errors: .[:digit:]
        private Tokens ReadDot() {
            _lexicalState = LexicalState.EXPR_BEG;
            
            int c = nextc();
            if (c == '.') {
                c = nextc();
                if (c == '.') {
                    return Tokens.Dot3;
                }
                pushback(c);
                return Tokens.Dot2;
            }

            pushback(c);
            if (IsDecimalDigit(c)) {
                ReportError(Errors.NoFloatingLiteral);
            }

            _lexicalState = LexicalState.EXPR_DOT;
            return (Tokens)'.';
        }

        // Operators: - @-
        // Assignments: -=
        // Literals: -... (negative number sign)
        private Tokens ReadMinus(bool whitespaceSeen) {
            int c = nextc();
            if (_lexicalState == LexicalState.EXPR_FNAME || _lexicalState == LexicalState.EXPR_DOT) {
                
                _lexicalState = LexicalState.EXPR_ARG;
                if (c == '@') {
                    return Tokens.Uminus;
                }

                pushback(c);
                return (Tokens)'-';
            }

            if (c == '=') {
                _tokenValue.SetSymbol(Symbols.Minus);
                _lexicalState = LexicalState.EXPR_BEG;
                return Tokens.Assignment;
            }

            if (_lexicalState == LexicalState.EXPR_BEG || _lexicalState == LexicalState.EXPR_MID ||
                (IS_ARG() && whitespaceSeen && !IsWhiteSpace(c))) {

                if (IS_ARG()) {
                    ReportWarning(Errors.AmbiguousFirstArgument);
                }

                _lexicalState = LexicalState.EXPR_BEG;
                pushback(c);
                if (IsDecimalDigit(c)) {
                    return Tokens.UminusNum;
                }

                return Tokens.Uminus;
            }

            _lexicalState = LexicalState.EXPR_BEG;
            pushback(c);
            return (Tokens)'-';
        }

        // Reads
        //   [:letter:]*
        // and converts it to RegEx options.
        private RubyRegexOptions ReadRegexOptions() {
            RubyRegexOptions encoding = 0;
            RubyRegexOptions options = 0;

            int c;
            while (IsLetter(c = nextc())) {
                switch (c) {
                    case 'i': options |= RubyRegexOptions.IgnoreCase; break;
                    case 'x': options |= RubyRegexOptions.Extended; break;
                    case 'm': options |= RubyRegexOptions.Multiline; break;
                    case 'o': 
                        // TODO: Once option not implemented.
                        break;

                    case 'n': encoding |= RubyRegexOptions.FIXED; break;
                    case 'e': encoding = RubyRegexOptions.EUC; break;
                    case 's': encoding = RubyRegexOptions.SJIS; break;
                    case 'u': encoding = RubyRegexOptions.UTF8; break;

                    default:
                        ReportError(Errors.UnknownRegexOption, (char)c);
                        break;
                }
            }
            pushback(c);

            return options | encoding;
        }

        #region Character Escapes

        // \\ \n \t \r \f \v \a \b \s 
        // \[:octal:] \x[:hexa:] \M-\[:escape:] \M-[:char:] \C-[:escape:] \C-[:char:] \c[:escape:] \c[:char:] \[:char:]
        private int ReadEscape() {
            int c = nextc();
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
                    c = nextc();
                    if (c != '-') {
                        pushback(c);
                        return InvalidEscapeCharacter();
                    }
                    
                    c = nextc();
                    if (c == '\\') {
                        return ReadEscape() | 0x80;
                    }

                    if (c == -1) {
                        return InvalidEscapeCharacter();                        
                    }

                    return (c & 0xff) | 0x80;

                case 'C':
                    c = nextc();
                    if (c != '-') {
                        pushback(c);
                        return InvalidEscapeCharacter();                        
                    }
                    goto case 'c';

                case 'c':
                    c = nextc();

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
                    return c;
            }
        }

        private int InvalidEscapeCharacter() {
            ReportError(Errors.InvalidEscapeCharacter);
            // return != 0 so that additional errors (\0 in a symbol) are not invoked
            return '?';
        }
        
        // Appends escaped regex escape sequence.
        private void AppendEscapedRegexEscape(int term) {
            int c = nextc();

            switch (c) {
                case '\n': 
                    break;

                case 'x':
                    tokadd('\\');
                    AppendEscapedHexEscape();
                    break;

                case 'M':
                    c = nextc();
                    if (c != '-') {
                        pushback(c);
                        InvalidEscapeCharacter();
                        break;
                    }

                    tokadd('\\'); 
                    tokadd('M'); 
                    tokadd('-');

                    // escaped:
                    AppendRegularExpressionCompositeEscape(term);
                    break;                    

                case 'C':
                    c = nextc();
                    if (c != '-') {
                        pushback(c);
                        InvalidEscapeCharacter();
                        break;
                    }

                    tokadd('\\'); 
                    tokadd('C'); 
                    tokadd('-');

                    AppendRegularExpressionCompositeEscape(term);
                    break;

                case 'c':
                    tokadd('\\'); 
                    tokadd('c');
                    AppendRegularExpressionCompositeEscape(term);
                    break;
                    
                case -1:
                    InvalidEscapeCharacter();
                    break;

                default:
                    if (IsOctalDigit(c)) {
                        tokadd('\\');
                        AppendEscapedOctalEscape();
                        break;
                    }

                    if (c != '\\' || c != term) {
                        tokadd('\\');
                    }
                    tokadd((char)c);
                    break;
            }
        }

        private void AppendRegularExpressionCompositeEscape(int term) {
            int c = nextc();
            if (c == '\\') {
                AppendEscapedRegexEscape(term);
            } else if (c == -1) {
                InvalidEscapeCharacter();
            } else {
                tokadd((char)c);
            }
        }

        private void AppendEscapedOctalEscape() {
            int start = _bufferPos - 1;
            ReadOctalEscape(0);

            Debug.Assert(IsOctalDigit(_lineBuffer[start])); // first digit
            _tokenString.Append(_lineBuffer, start, _bufferPos - start);
        }

        private void AppendEscapedHexEscape() {
            int start = _bufferPos - 1;
            ReadHexEscape();

            Debug.Assert(_lineBuffer[start] == 'x');
            _tokenString.Append(_lineBuffer, start, _bufferPos - start);
        }

        private void AppendEscapedUnicode() {
            int start = _bufferPos - 1;

            if (peekc() == '{') {
                ReadUnicodeCodePoint();
            } else {
                ReadUnicodeEscape();
            }

            Debug.Assert(_lineBuffer[start] == 'u');
            _tokenString.Append(_lineBuffer, start, _bufferPos - start);
        }

        // Reads octal number of at most 3 digits.
        // Reads at most 2 octal digits as the value of the first digit is in "value".
        private int ReadOctalEscape(int value) {
            if (IsOctalDigit(peekc())) {
                value = (value << 3) | (nextc() - '0');

                if (IsOctalDigit(peekc())) {
                    value = (value << 3) | (nextc() - '0');
                }
            }
            return value;
        }

        // Reads hexadecimal number of at most 2 digits. 
        private int ReadHexEscape() {
            int value = ToDigit(peekc());
            if (value < 16) {
                nextc();
                int digit = ToDigit(peekc());
                if (digit < 16) {
                    value = (value << 4) | digit;
                    nextc();
                }

                return value;
            } else {
                return InvalidEscapeCharacter();
            }
        }

        // Peeks exactly 4 hexadecimal characters (\uFFFF).
        private int ReadUnicodeEscape() {
            int d4 = ToDigit(lookahead(0));
            int d3 = ToDigit(lookahead(1));
            int d2 = ToDigit(lookahead(2));
            int d1 = ToDigit(lookahead(3));

            if (d1 >= 16 || d2 >= 16 || d3 >= 16 || d4 >= 16) {
                return InvalidEscapeCharacter();                
            }

            nextc();
            nextc();
            nextc();
            nextc();
            return (d4 << 12) | (d3 << 8) | (d2 << 4) | d1;
        }

        // Reads {at-most-six-hexa-digits}
        private int ReadUnicodeCodePoint() {
            int c = nextc();
            Debug.Assert(c == '{');

            int codepoint = 0;
            int i = 0;
            while (true) {
                c = nextc();
                if (i == 6) {
                    break;
                }

                int digit = ToDigit(c);
                if (digit >= 16) {
                    break;
                }

                codepoint = (codepoint << 4) | digit;
                i++;
            }

            if (c != '}') {
                pushback(c);
                InvalidEscapeCharacter();                
            }
            
            if (codepoint > 0x10ffff) {
                ReportError(Errors.TooLargeUnicodeCodePoint);
            }

            return codepoint;
        }

        // Reads up to 6 hex characters, treats them as a exadecimal code-point value and appends the result to the buffer.
        private void AppendUnicodeCodePoint(StringType stringType) {
            int codepoint = ReadUnicodeCodePoint();

            if (codepoint < 0x10000) {
                // code-points [0xd800 .. 0xdffff] are not treated as invalid
                AppendCharacter(codepoint, stringType);
            } else {
                codepoint -= 0x10000;
                _tokenString.Append((char)((codepoint / 0x400) + 0xd800));
                _tokenString.Append((char)((codepoint % 0x400) + 0xdc00));
            }
        }

        #endregion

        #region Strings

        // String: "...
        private Tokens ReadDoubleQuote() {
            _currentString = new StringContentTokenizer(StringType.ExpandsEmbedded, '"');
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.StringBeg;
        }

        // String: '...
        private Tokens ReadSingleQuote() {
            _currentString = new StringContentTokenizer(StringType.Default, '\'');
            _tokenValue.SetStringTokenizer(_currentString);
            return Tokens.StringBeg;
        }

        // returns last character read
        private int ReadStringContent(StringType stringType, int terminator, int openingParenthesis, 
            ref int nestingLevel, ref bool hasUnicodeEscape) {

            while (true) {
                int c = nextc();
                if (c == -1) {
                    return -1;
                }

                if (openingParenthesis != 0 && c == openingParenthesis) {
                    nestingLevel++;
                } else if (c == terminator) {
                    if (nestingLevel == 0) {
                        pushback(c);
                        return c;
                    }
                    nestingLevel--;
                } else if (((stringType & StringType.ExpandsEmbedded) != 0) && c == '#' && _bufferPos < _lineBuffer.Length) {
                    int c2 = _lineBuffer[_bufferPos];
                    if (c2 == '$' || c2 == '@' || c2 == '{') {
                        pushback(c);
                        return c;
                    }
                } else if ((stringType & StringType.Words) != 0 && IsWhiteSpace(c)) {
                    pushback(c);
                    return c;
                } else if (c == '\\') {
                    c = nextc();

                    if (c == '\n') {
                        if ((stringType & StringType.Words) == 0) {
                            if ((stringType & StringType.ExpandsEmbedded) != 0) {
                                continue;
                            }
                            tokadd('\\');
                        }
                    } else if (c == '\\') {
                        if ((stringType & StringType.RegularExpression) != 0) {
                            tokadd('\\');
                        }
                    } else if ((stringType & StringType.RegularExpression) != 0) {
                        // \uFFFF, \u{codepoint}
                        if (c == 'u' && _compatibility >= RubyCompatibility.Ruby19) {
                            hasUnicodeEscape = true;
                            tokadd('\\');
                            AppendEscapedUnicode();
                        } else {
                            pushback(c);
                            AppendEscapedRegexEscape(terminator);
                        }
                        continue;
                    } else if ((stringType & StringType.ExpandsEmbedded) != 0) {
                        // \uFFFF, \u{codepoint}
                        if (c == 'u' && _compatibility >= RubyCompatibility.Ruby19) {
                            hasUnicodeEscape = true;
                            if (peekc() == '{') {
                                AppendUnicodeCodePoint(stringType);
                                continue;
                            } else {
                                c = ReadUnicodeEscape();
                            }
                        } else {
                            pushback(c);
                            c = ReadEscape();
                        }
                    } else if ((stringType & StringType.Words) != 0 && IsWhiteSpace(c)) {
                        /* ignore backslashed spaces in %w */
                    } else if (c != terminator && !(openingParenthesis != 0 && c == openingParenthesis)) {
                        tokadd('\\');
                    }
                }

                AppendCharacter(c, stringType);
            }
        }

        private void AppendCharacter(int c, StringType stringType) {
            if (c == 0 && (stringType & StringType.Symbol) != 0) {
                ReportError(Errors.NullCharacterInSymbol);
            } else {
                _tokenString.Append((char)c);
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

            int c = peekc();
            MarkTokenStart();

            // unterminated string (error recovery is slightly different from MRI):
            if (c == -1) {
                ReportError(Errors.UnterminatedString);
                UnterminatedToken = true;
                MarkSingleLineTokenEnd();
                return Tokens.StringEnd;
            }

            c = nextc();
            
            // skip whitespace in word list:
            if ((stringKind & StringType.Words) != 0 && IsWhiteSpace(c)) {
                do { 
                    c = nextc(); 
                } while (IsWhiteSpace(c));

                whitespaceSeen = true;
            }

            // end of the top-level string:
            if (c == info.TerminatingCharacter && info.NestingLevel == 0) {
                
                // end of words:
                if ((stringKind & StringType.Words) != 0) {
                    // final separator in the list of words (see grammar):
                    info.Properties = StringType.FinalWordSeparator;
                    MarkMultiLineTokenEnd();
                    return Tokens.WordSeparator;
                }

                // end of regex:
                if ((stringKind & StringType.RegularExpression) != 0) {
                    _tokenValue.SetRegexOptions(ReadRegexOptions());
                    MarkSingleLineTokenEnd();
                    return Tokens.RegexpEnd;
                }
                
                // end of string/symbol:
                MarkSingleLineTokenEnd();
                return Tokens.StringEnd;
            }

            // word separator:
            if (whitespaceSeen) {
                pushback(c);
                MarkMultiLineTokenEnd();
                return Tokens.WordSeparator;
            }

            newtok();

            // start of #$variable, #@variable, #{expression} in a string:
            if ((stringKind & StringType.ExpandsEmbedded) != 0 && c == '#') {
                c = nextc();
                switch (c) {
                    case '$':
                    case '@':
                        pushback(c);
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedVariableBegin();

                    case '{':
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedCodeBegin();
                }
                tokadd('#');
            }

            pushback(c);
            bool hasUnicodeEscape = false;

            int nestingLevel = info.NestingLevel;
            ReadStringContent(stringKind, info.TerminatingCharacter, info.OpeningParenthesis, ref nestingLevel, ref hasUnicodeEscape);
            info.NestingLevel = nestingLevel;

            _tokenValue.SetString(tok(), hasUnicodeEscape);
            MarkMultiLineTokenEnd();
            return Tokens.StringContent;
        }

        #endregion

        #region Heredoc

        private Tokens TokenizeHeredocLabel() {
            int term;
            StringType stringType = StringType.Default;

            int c = nextc();
            if (c == '-') {
                c = nextc();
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
                    c = nextc();
                    if (c == -1) {
                        UnterminatedToken = true;
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
                    if (c == '\n') {
                        pushback(c);
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
                pushback(c);
                if ((stringType & StringType.IndentedHeredoc) != 0) {
                    pushback('-');
                }
                return Tokens.None;
            }

            // note that if we allow \n in the label we must change this to multi-line token!
            MarkSingleLineTokenEnd();
            
            // skip the rest of the line (the content is stored in heredoc string terminal and tokenized upon restore)
            int resume = _bufferPos;
            _bufferPos = _lineBuffer.Length;
            _currentString = new HeredocTokenizer(stringType, label, resume, _lineBuffer, _currentLine, _currentLineIndex);
            _tokenValue.SetStringTokenizer(_currentString);

            return term == '`' ? Tokens.ShellStringBegin : Tokens.StringBeg;
        }

        private void HeredocRestore(HeredocTokenizer/*!*/ here) {
            _lineBuffer = here.ResumeLine;
            _bufferPos = here.ResumePosition;
            _heredocEndLine = _currentLine;
            _heredocEndLineIndex = _currentLineIndex;
            _currentLine = here.FirstLine;
            _currentLineIndex = here.FirstLineIndex;
        }

        internal Tokens TokenizeHeredoc(HeredocTokenizer/*!*/ heredoc) {
            StringType stringKind = heredoc.Properties;
            bool isIndented = (stringKind & StringType.IndentedHeredoc) != 0;

            int c = peekc();
            MarkTokenStart();

            if (c == -1) {
                ReportError(Errors.UnterminatedHereDoc, heredoc.Label);
                MarkSingleLineTokenEnd();
                HeredocRestore(heredoc);
                UnterminatedToken = true;
                return Tokens.StringEnd;
            }

            // label reached - it becomes a string-end token:
            // (note that label is single line, MRI allows multiline, but such label is never matched)
            if (is_bol() && LineContentEquals(heredoc.Label, isIndented)) {
                
                // TODO: reads the entire label:
                do {
                    c = nextc();
                } while (c != '\n' && c != -1);
                pushback(c);

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
                _tokenValue.SetString(str.ToString(), false);
                MarkMultiLineTokenEnd();
                return Tokens.StringContent;
            }

            return TokenizeExpandingHeredocContent(heredoc);
            
            // obsolete:
            //MarkMultiLineTokenEnd();
            //HeredocRestore(heredoc);
            //_currentString = new StringTerminator(StringType.FinalWordSeparator, 0, 0);
            //_tokenValue.SetString(str);
            //return Tokens.StringContent;
        }

        private StringBuilder/*!*/ ReadNonexpandingHeredocContent(HeredocTokenizer/*!*/ heredoc) {
            bool isIndented = (heredoc.Properties & StringType.IndentedHeredoc) != 0;
            var result = new StringBuilder();

            // reads lines until the line contains heredoc label
            do {
                int end = _lineBuffer.Length;
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

                if (end < _lineBuffer.Length) {
                    result.Append('\n');
                }

                _bufferPos = _lineBuffer.Length;

                // peekc forces a new line load:
                if (peekc() == -1) {
                    // eof reached before end of heredoc:
                    return result;
                }

            } while (!LineContentEquals(heredoc.Label, isIndented));

            // return to the end of line, next token will be StringEnd spanning over the end-of-heredoc label:
            _bufferPos = 0;
            return result;
        }

        private Tokens TokenizeExpandingHeredocContent(HeredocTokenizer/*!*/ heredoc) {
            newtok();
            int c = nextc();

            if (c == '#') {
                c = nextc();

                switch (c) {
                    case '$':
                    case '@':
                        pushback(c);
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedVariableBegin();

                    case '{':
                        MarkSingleLineTokenEnd();
                        return StringEmbeddedCodeBegin();
                }

                tokadd('#');
            }

            bool isIndented = (heredoc.Properties & StringType.IndentedHeredoc) != 0;

            pushback(c);
            bool hasUnicodeEscape = false;
            
            do {
                // read string content upto the end of the line:
                int tmp = 0;
                c = ReadStringContent(heredoc.Properties, '\n', 0, ref tmp, ref hasUnicodeEscape);
                
                // stop reading on end-of-file or just before an embedded expression: #$, #$, #{
                if (c != '\n') {
                    break;
                }

                // adds \n
                tokadd((char)nextc());

                // first char on the next line:
                if (peekc() == -1) {
                    break;
                }

            } while (!LineContentEquals(heredoc.Label, isIndented));

            _tokenValue.SetString(tok(), hasUnicodeEscape);
            MarkMultiLineTokenEnd();
            return Tokens.StringContent;
        }

        #endregion

        // Quotation start: 
        //   %[QqWwxrs]?[^:alpha-numeric:]
        private Tokens ReadQuotationStart(int c) {
            StringType type;
            Tokens token;
            int terminator;

            // c is the character following %
            switch (c) {
                case 'Q':
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.StringBeg;
                    terminator = nextc();
                    break;

                case 'q':
                    type = StringType.Default;
                    token = Tokens.StringBeg;
                    terminator = nextc();
                    break;

                case 'W':
                    type = StringType.Words | StringType.ExpandsEmbedded;
                    token = Tokens.WordsBeg;
                    terminator = nextc();
                    SkipWhitespace();
                    break;

                case 'w':
                    type = StringType.Words;
                    token = Tokens.VerbatimWordsBegin;
                    terminator = nextc();
                    SkipWhitespace();
                    break;

                case 'x':
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.ShellStringBegin;
                    terminator = nextc();
                    break;

                case 'r':
                    type = StringType.RegularExpression | StringType.ExpandsEmbedded;
                    token = Tokens.RegexpBeg;
                    terminator = nextc();
                    break;

                case 's':
                    type = StringType.Symbol;
                    token = Tokens.Symbeg;
                    terminator = nextc();
                    _lexicalState = LexicalState.EXPR_FNAME;
                    break;

                default:
                    type = StringType.ExpandsEmbedded;
                    token = Tokens.StringBeg;
                    terminator = c;
                    break;
            }

            int parenthesis = terminator;
            switch (terminator) {
                case -1:
                    UnterminatedToken = true;
                    ReportError(Errors.UnterminatedQuotedString);
                    return Tokens.EndOfFile;

                case '(': terminator = ')'; break;
                case '{': terminator = '}'; break;
                case '[': terminator = ']'; break;
                case '<': terminator = '>'; break;

                default:
                    if (IsLetterOrDigit(terminator)) {
                        pushback(c);
                        ReportError(Errors.UnknownQuotedStringType);
                        return (Tokens)'%';
                    }
                    parenthesis = 0; 
                    break;
            }

            _currentString = new StringContentTokenizer(type, (char)terminator, (char)parenthesis);
            _tokenValue.SetStringTokenizer(_currentString);
            return token;
        }

        #region Numbers

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
                switch (peekc()) {
                    case 'x':
                    case 'X':
                        nextc();
                        return ReadInteger(16, NumericCharKind.None);

                    case 'b':
                    case 'B':
                        nextc();
                        return ReadInteger(2, NumericCharKind.None);

                    case 'o':
                    case 'O':
                        nextc();
                        return ReadInteger(8, NumericCharKind.None);

                    case 'd':
                    case 'D':
                        nextc();
                        return ReadInteger(10, NumericCharKind.None);

                    case 'e':
                    case 'E': {
                            // 0e[+-]...    
                            int sign;
                            int start = _bufferPos - 1;

                            nextc();
                            if (TryReadExponentSign(out sign)) {
                                return ReadDoubleExponent(start, sign);
                            }

                            pushback('e');
                            _tokenValue.SetInteger(0);
                            return Tokens.Integer;
                        }

                    case '.':
                        // 0.
                        nextc();
                        if (IsDecimalDigit(peekc())) {
                            return ReadDouble(_bufferPos - 2);
                        }
                        pushback('.');

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
                int c = nextc();
                int digit = ToDigit(c);

                if (digit < @base) {
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
                        prev = NumericCharKind.Underscore;
                        underscoreCount++;
                        continue;
                    } 
                    
                    if (c == '.' && IsDecimalDigit(peekc())) {
                        ReportWarning(Errors.NoFloatingLiteral);
                    }

                    pushback(c);
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
            long integer = 0;
            NumericCharKind prev = NumericCharKind.None;

            while (true) {
                int sign;

                if (IsDecimalDigit(c)) {
                    prev = NumericCharKind.Digit;
                    integer = integer * 10 + (c - '0');
                    if (integer > Int32.MaxValue) {
                        return ReadBigNumber(integer, 10, numberStartIndex, underscoreCount, true);
                    }

                } else if (prev == NumericCharKind.Underscore) {

                    ReportError(Errors.TrailingUnderscoreInNumber);
                    pushback(c);
                    _tokenValue.SetInteger((int)integer);
                    return Tokens.Integer;

                } else if ((c == 'e' || c == 'E') && TryReadExponentSign(out sign)) {

                    return ReadDoubleExponent(numberStartIndex, sign);

                } else if (c == '_') {

                    underscoreCount++;
                    prev = NumericCharKind.Underscore;

                } else {

                    if (c == '.' && IsDecimalDigit(peekc())) {
                        return ReadDouble(numberStartIndex);
                    }

                    pushback(c);
                    _tokenValue.SetInteger((int)integer);
                    return Tokens.Integer;
                }

                c = nextc();
            }
        }
        private bool TryReadExponentSign(out int sign) {
            int s = peekc();
            if (s == '-') {
                nextc();
                sign = -1;
            } else if (s == '+') {
                nextc();
                sign = +1;
            } else {
                sign = +1;
            }

            if (IsDecimalDigit(peekc())) {
                return true;
            }

            if (s == '-') {
                ReportError(Errors.TrailingMinusInNumber);
                pushback('-');
            } else if (s == '+') {
                ReportError(Errors.TrailingPlusInNumber);
                pushback('+');
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
                int c = nextc();
                int digit = ToDigit(c);

                if (digit < @base) {
                    prev = NumericCharKind.Digit;
                } else {

                    if (prev == NumericCharKind.Underscore) {
                        ReportError(Errors.TrailingUnderscoreInNumber);                        
                    } else if (c == '_') {
                        prev = NumericCharKind.Underscore;
                        underscoreCount++;
                        continue;
                    } else if (allowDouble) {
                        int sign;
                        if ((c == 'e' || c == 'E') && TryReadExponentSign(out sign)) {
                            return ReadDoubleExponent(numberStartIndex, sign);
                        } else if (c == '.') {
                            if (IsDecimalDigit(peekc())) {
                                return ReadDouble(numberStartIndex);
                            }
                        }
                    }

                    pushback(c);

                    // TODO: store only the digit count, the actual value will be parsed later:
                    // TODO: skip initial zeros
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
            Debug.Assert(IsDecimalDigit(peekc()));

            NumericCharKind prev = NumericCharKind.None;
            while (true) {
                int sign;
                int c = nextc();

                if (IsDecimalDigit(c)) {
                    prev = NumericCharKind.Digit;
                } else if ((c == 'e' || c == 'E') && TryReadExponentSign(out sign)) {
                    return ReadDoubleExponent(numberStartIndex, sign);
                } else {
                    if (prev == NumericCharKind.Underscore) {
                        ReportError(Errors.TrailingUnderscoreInNumber);                        
                    } else if (c == '_') {
                        prev = NumericCharKind.Underscore;
                        continue;
                    }

                    pushback(c);
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
                int c = nextc();

                if (IsDecimalDigit(c)) {
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
                        prev = NumericCharKind.Underscore;
                        continue;
                    }

                    pushback(c);

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

        private Tokens DecodeDouble(int first, int end) {
            double result;
            if (!TryDecodeDouble(_lineBuffer, first, end, out result)) {
                result = Double.PositiveInfinity;
            }

            _tokenValue.SetDouble(result);
            return Tokens.Float;
        }

        #endregion

        #region Characters

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

        public static bool IsIdentifier(int c) {
            return IsIdentifierInitial(c) || IsDecimalDigit(c);
        }

        public static bool IsIdentifierInitial(int c) {
            return IsLetter(c) || c == '_';
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

        private static bool IsMethodNameSuffix(int c) {
            return IsIdentifier(c) || c == '!' || c == '?' || c == '=';
        }

        // Reads [A-Za-z0-9_]*
        private void SkipVariableName() {
            int c;
            do { c = nextc(); } while (IsIdentifier(c));
            pushback(c);
        }

        private void SkipWhitespace() {
            int c;
            do { c = nextc(); } while (IsWhiteSpace(c));
            pushback(c);
        }

        #endregion

        #region Public API

        private static int nextc(char[]/*!*/ str, ref int i) {
            i++;
            return (i < str.Length) ? str[i] : -1;
        }

        // subsequent _ are not considered error
        public static double ParseDouble(char[]/*!*/ str) {
            double sign;
            int i = -1;

            int c;
            do { c = nextc(str, ref i); } while (IsWhiteSpace(c));
            
            if (c == '-') {
                c = nextc(str, ref i);
                if (c == '_') return 0.0;
                sign = -1;
            } else if (c == '+') {
                c = nextc(str, ref i);
                if (c == '_') return 0.0;
                sign = +1;
            } else {
                sign = +1;
            }

            int start = i;

            while (c == '_' || IsDecimalDigit(c)) {
                c = nextc(str, ref i);
            }

            if (c == '.') {
                c = nextc(str, ref i);
                while (c == '_' || IsDecimalDigit(c)) {
                    c = nextc(str, ref i);
                }
            }

            // just before the current character:
            int end = i;

            if (c == 'e' || c == 'E') {
                c = nextc(str, ref i);
                if (c == '+' || c == '-') {
                    c = nextc(str, ref i);
                }

                int expEnd = end;

                while (true) {
                    if (IsDecimalDigit(c)) {
                        expEnd = i + 1;
                    } else if (c != '_') {
                        break;
                    }
                    c = nextc(str, ref i);
                }

                end = expEnd;
            }

            double result;
            return TryDecodeDouble(str, start, end, out result) ? result * sign : 0.0;
        }

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

        public static bool IsConstantName(string name) {
            return !String.IsNullOrEmpty(name) 
                && IsUpperLetter(name[0])
                && IsVariableName(name, 1, 1) 
                && IsIdentifier(name[name.Length - 1]);
        }

        public static bool IsMethodName(string name) {
            return !String.IsNullOrEmpty(name)
                && IsIdentifierInitial(name[0])
                && IsVariableName(name, 1, 1)
                && IsMethodNameSuffix(name[name.Length - 1]);
        }

        public static bool IsInstanceVariableName(string name) {
            return name != null && name.Length >= 2
                && name[0] == '@'
                && IsVariableName(name, 1, 0);
        }

        public static bool IsClassVariableName(string name) {
            return name != null && name.Length >= 3
                && name[0] == '@'
                && name[1] == '@'
                && IsVariableName(name, 2, 0);
        }

        public static bool IsGlobalVariableName(string name) {
            return name != null && name.Length >= 2
                && name[0] == '$'
                && IsVariableName(name, 1, 0);
        }

        private static bool IsVariableName(string name, int trimStart, int trimEnd) {
            for (int i = trimStart; i < name.Length - trimEnd; i++) {
                if (!IsIdentifier(name[i])) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Tokenizer Service

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
                case Tokens.BitwiseAnd:
                case Tokens.BitwiseOr:
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
                case Tokens.StringBeg:
                case Tokens.ShellStringBegin:
                case Tokens.Symbeg:
                case Tokens.WordsBeg:
                case Tokens.VerbatimWordsBegin:
                case Tokens.RegexpBeg:
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
