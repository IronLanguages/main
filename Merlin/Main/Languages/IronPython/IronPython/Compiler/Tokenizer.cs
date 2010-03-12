/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
//#define DUMP_TOKENS

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif

namespace IronPython.Compiler {

    /// <summary>
    /// IronPython tokenizer
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tokenizer")]
    public sealed partial class Tokenizer : TokenizerService {
        private State _state;
        private readonly bool _verbatim;
        internal bool _dontImplyDedent;
        private bool _disableLineFeedLineSeparator;
        private SourceUnit _sourceUnit;
        private TokenizerBuffer _buffer;
        private ErrorSink _errors;
        private Severity _indentationInconsistencySeverity;
        private bool _endContinues, _printFunction, _unicodeLiterals;

        private const int EOF = -1;
        private const int MaxIndent = 80;
        private const int DefaultBufferCapacity = 1024;

        public Tokenizer() {
            _errors = ErrorSink.Null;
            _verbatim = true;
            _state = new State(null);
        }

        public Tokenizer(ErrorSink errorSink) {
            _errors = errorSink;
            _state = new State(null);
        }

        [Obsolete("Use the overload that takes a PythonCompilerOptions instead")]
        public Tokenizer(ErrorSink errorSink, bool verbatim)
            : this(errorSink, verbatim, true) {
        }

        [Obsolete("Use the overload that takes a PythonCompilerOptions instead")]
        public Tokenizer(ErrorSink errorSink, bool verbatim, bool dontImplyDedent) {
            ContractUtils.RequiresNotNull(errorSink, "errorSink");

            _errors = errorSink;
            _verbatim = verbatim;
            _state = new State(null);
            _dontImplyDedent = dontImplyDedent;
        }

        public Tokenizer(ErrorSink errorSink, PythonCompilerOptions options) {
            ContractUtils.RequiresNotNull(errorSink, "errorSink");
            ContractUtils.RequiresNotNull(options, "options");

            _errors = errorSink;
            _verbatim = options.Verbatim;
            _state = new State(null);
            _dontImplyDedent = options.DontImplyDedent;
            _printFunction = options.PrintFunction;
            _unicodeLiterals = options.UnicodeLiterals;
        }

        /// <summary>
        /// Used to support legacy CreateParser API.
        /// </summary>
        internal Tokenizer(ErrorSink errorSink, PythonCompilerOptions options, bool verbatim)
            : this(errorSink, options) {

            _verbatim = verbatim || options.Verbatim;
        }

        public override bool IsRestartable {
            get { return true; }
        }

        public override object CurrentState {
            get {
                return _state;
            }
        }

        public override SourceLocation CurrentPosition {
            get { return TokenStart; }
        }

        public SourceUnit SourceUnit {
            get {
                return _sourceUnit;
            }
        }

        public override ErrorSink ErrorSink {
            get { return _errors; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                _errors = value;
            }
        }

        public Severity IndentationInconsistencySeverity {
            get { return _indentationInconsistencySeverity; }
            set {
                _indentationInconsistencySeverity = value;

                if (value != Severity.Ignore && _state.IndentFormat == null) {
                    _state.IndentFormat = new StringBuilder[MaxIndent];
                }
            }
        }

        public bool IsEndOfFile {
            get {
                return _buffer.Peek() == EOF;
            }
        }

        public SourceLocation TokenStart {
            get {
                if (_sourceUnit == null) {
                    return _buffer.TokenStart;
                }
                return _sourceUnit.MakeLocation(_buffer.TokenStart);
            }
        }

        public SourceLocation TokenEnd {
            get {
                if (_sourceUnit == null) {
                    return _buffer.TokenEnd;
                }
                return _sourceUnit.MakeLocation(_buffer.TokenEnd);
            }
        }

        public SourceSpan TokenSpan {
            get {
                return new SourceSpan(TokenStart, TokenEnd);
            }
        }

        public void Initialize(SourceUnit sourceUnit) {
            ContractUtils.RequiresNotNull(sourceUnit, "sourceUnit");

            Initialize(null, sourceUnit.GetReader(), sourceUnit, SourceLocation.MinValue, DefaultBufferCapacity);
        }

        public override void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation) {
            Initialize(state, reader, sourceUnit, initialLocation, DefaultBufferCapacity);
        }

        public void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation, int bufferCapacity) {
            Initialize(state, reader, sourceUnit, initialLocation, bufferCapacity, null);
        }

        public void Initialize(object state, TextReader reader, SourceUnit sourceUnit, SourceLocation initialLocation, int bufferCapacity, PythonCompilerOptions compilerOptions) {
            ContractUtils.RequiresNotNull(reader, "reader");

            if (state != null) {
                if (!(state is State)) throw new ArgumentException("bad state provided");
                _state = new State((State)state);
            } else {
                _state = new State(null);
            }

            if (compilerOptions != null && compilerOptions.InitialIndent != null) {
                _state.Indent = (int[])compilerOptions.InitialIndent.Clone();
            }

            _sourceUnit = sourceUnit;
            _disableLineFeedLineSeparator = reader is NoLineFeedSourceContentProvider.Reader;

            if (_buffer == null) {
                _buffer = new TokenizerBuffer(reader, initialLocation, bufferCapacity, !_disableLineFeedLineSeparator);
            } else {
                _buffer.Initialize(reader, initialLocation, bufferCapacity, !_disableLineFeedLineSeparator);
            }

            DumpBeginningOfUnit();
        }

        public override TokenInfo ReadToken() {
            if (_buffer == null) {
                throw new InvalidOperationException("Uninitialized");
            }

            TokenInfo result = new TokenInfo();
            Token token = GetNextToken();
            result.SourceSpan = TokenSpan;

            switch (token.Kind) {
                case TokenKind.EndOfFile:
                    result.Category = TokenCategory.EndOfStream;
                    break;

                case TokenKind.Comment:
                    result.Category = TokenCategory.Comment;
                    break;

                case TokenKind.Name:
                    result.Category = TokenCategory.Identifier;
                    break;

                case TokenKind.Error:
                    result.Category = TokenCategory.Error;
                    break;

                case TokenKind.Constant:
                    result.Category = (token.Value is string) ? TokenCategory.StringLiteral : TokenCategory.NumericLiteral;
                    break;

                case TokenKind.LeftParenthesis:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterStart;
                    break;

                case TokenKind.RightParenthesis:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces | TokenTriggers.ParameterEnd;
                    break;

                case TokenKind.LeftBracket:
                case TokenKind.LeftBrace:
                case TokenKind.RightBracket:
                case TokenKind.RightBrace:
                    result.Category = TokenCategory.Grouping;
                    result.Trigger = TokenTriggers.MatchBraces;
                    break;

                case TokenKind.Colon:
                    result.Category = TokenCategory.Delimiter;
                    break;

                case TokenKind.Semicolon:
                    result.Category = TokenCategory.Delimiter;
                    break;

                case TokenKind.Comma:
                    result.Category = TokenCategory.Delimiter;
                    result.Trigger = TokenTriggers.ParameterNext;
                    break;

                case TokenKind.Dot:
                    result.Category = TokenCategory.Operator;
                    result.Trigger = TokenTriggers.MemberSelect;
                    break;

                case TokenKind.NewLine:
                    result.Category = TokenCategory.WhiteSpace;
                    break;

                default:
                    if (token.Kind >= TokenKind.FirstKeyword && token.Kind <= TokenKind.LastKeyword) {
                        result.Category = TokenCategory.Keyword;
                        break;
                    }

                    result.Category = TokenCategory.Operator;
                    break;
            }

            return result;
        }

        internal bool TryGetTokenString(int len, out string tokenString) {
            if (len != _buffer.TokenLength) {
                tokenString = null;
                return false;
            }
            tokenString = _buffer.GetTokenString();
            return true;
        }

        internal string GetTokenString() {
            return _buffer.GetTokenString();
        }

        internal bool PrintFunction {
            get {
                return _printFunction;
            }
            set {
                _printFunction = value;
            }
        }

        internal bool UnicodeLiterals {
            get {
                return _unicodeLiterals;
            }
            set {
                _unicodeLiterals = value;
            }
        }

        private bool NextChar(int ch) {
            return _buffer.Read(ch);
        }

        private int NextChar() {
            return _buffer.Read();
        }

        public Token GetNextToken() {
            Debug.Assert(_buffer != null && _sourceUnit != null, "Uninitialized");
            Token result;

            if (_state.PendingDedents != 0) {
                if (_state.PendingDedents == -1) {
                    _state.PendingDedents = 0;
                    result = Tokens.IndentToken;
                } else {
                    _state.PendingDedents--;
                    result = Tokens.DedentToken;
                }
            } else {
                result = Next();
            }

            DumpToken(result);
            return result;
        }

        private Token Next() {
            bool at_beginning = _buffer.AtBeginning;

            if (_state.IncompleteString != null && _buffer.Peek() != EOF) {
                IncompleteStringToken prev = _state.IncompleteString;
                _state.IncompleteString = null;
                return ContinueString(prev.IsSingleTickQuote ? '\'' : '"', prev.IsRaw, prev.IsUnicode, false, prev.IsTripleQuoted, 0);
            }

            _buffer.DiscardToken();

            int ch = NextChar();

            while (true) {
                switch (ch) {
                    case EOF:
                        return ReadEof();
                    case '\f':
                        // Ignore form feeds
                        _buffer.DiscardToken();
                        ch = NextChar();
                        break;
                    case ' ':
                    case '\t':
                        ch = SkipWhiteSpace(at_beginning);
                        break;

                    case '#':
                        if (_verbatim)
                            return ReadSingleLineComment();

                        ch = SkipSingleLineComment();
                        break;

                    case '\\':
                        if (_buffer.ReadEolnOpt(NextChar()) > 0) {
                            // discard token '\\<eoln>':
                            _buffer.DiscardToken();

                            ch = NextChar();
                            if (ch == -1) {
                                _endContinues = true;
                            }
                            break;

                        } else {
                            _buffer.Back();
                            goto default;
                        }

                    case '\"':
                    case '\'':
                        _state.LastNewLine = false;
                        return ReadString((char)ch, false, false, false);

                    case 'u':
                    case 'U':
                        _state.LastNewLine = false;
                        return ReadNameOrUnicodeString();

                    case 'r':
                    case 'R':
                        _state.LastNewLine = false;
                        return ReadNameOrRawString();
                    case 'b':
                    case 'B':
                        _state.LastNewLine = false;
                        return ReadNameOrBytes();

                    case '_':
                        _state.LastNewLine = false;
                        return ReadName();

                    case '.':
                        _state.LastNewLine = false;
                        ch = _buffer.Peek();
                        if (ch >= '0' && ch <= '9')
                            return ReadFraction();

                        _buffer.MarkSingleLineTokenEnd();
                        return Tokens.DotToken;

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
                        _state.LastNewLine = false;
                        return ReadNumber(ch);

                    default:

                        if (_buffer.ReadEolnOpt(ch) > 0) {
                            // token marked by the callee:
                            if (ReadNewline()) {
                                if (_state.LastNewLine) {
                                    return Tokens.NLToken;
                                } else {
                                    _state.LastNewLine = true;
                                    return Tokens.NewLineToken;
                                }
                            }

                            // we're in a grouping, white space is ignored
                            _buffer.DiscardToken();
                            ch = NextChar();
                            break;
                        }

                        _state.LastNewLine = false;
                        Token res = NextOperator(ch);
                        if (res != null) {
                            _buffer.MarkSingleLineTokenEnd();
                            return res;
                        }

                        if (IsNameStart(ch)) return ReadName();

                        _buffer.MarkSingleLineTokenEnd();
                        return BadChar(ch);
                }
            }
        }

        private int SkipWhiteSpace(bool atBeginning) {
            int ch;
            do { ch = NextChar(); } while (ch == ' ' || ch == '\t');

            _buffer.Back();

            if (atBeginning && ch != '#' && ch != '\f' && ch != EOF && !_buffer.IsEoln(ch)) {
                _buffer.MarkSingleLineTokenEnd();
                ReportSyntaxError(_buffer.TokenSpan, Resources.InvalidSyntax, ErrorCodes.SyntaxError);
            }

            _buffer.DiscardToken();
            _buffer.SeekRelative(+1);
            return ch;
        }

        private int SkipSingleLineComment() {
            // do single-line comment:
            int ch = _buffer.ReadLine();
            _buffer.MarkSingleLineTokenEnd();

            // discard token '# ...':
            _buffer.DiscardToken();
            _buffer.SeekRelative(+1);

            return ch;
        }

        private Token ReadSingleLineComment() {
            // do single-line comment:
            _buffer.ReadLine();
            _buffer.MarkSingleLineTokenEnd();

            return new CommentToken(_buffer.GetTokenString());
        }

        private Token ReadNameOrUnicodeString() {
            if (NextChar('\"')) return ReadString('\"', false, true, false);
            if (NextChar('\'')) return ReadString('\'', false, true, false);
            if (NextChar('r') || NextChar('R')) {
                if (NextChar('\"')) return ReadString('\"', true, true, false);
                if (NextChar('\'')) return ReadString('\'', true, true, false);
                _buffer.Back();
            }
            return ReadName();
        }

        private Token ReadNameOrBytes() {
            if (NextChar('\"')) return ReadString('\"', false, false, true);
            if (NextChar('\'')) return ReadString('\'', false, false, true);
            if (NextChar('r') || NextChar('R')) {
                if (NextChar('\"')) return ReadString('\"', true, false, true);
                if (NextChar('\'')) return ReadString('\'', true, false, true);
                _buffer.Back();
            }
            return ReadName();
        }

        private Token ReadNameOrRawString() {
            if (NextChar('\"')) return ReadString('\"', true, false, false);
            if (NextChar('\'')) return ReadString('\'', true, false, false);
            return ReadName();
        }

        private Token ReadEof() {
            _buffer.MarkSingleLineTokenEnd();

            if (!_dontImplyDedent && _state.IndentLevel > 0) {
                // before we imply dedents we need to make sure the last thing we returned was
                // a new line.
                if (!_state.LastNewLine) {
                    _state.LastNewLine = true;
                    return Tokens.NewLineToken;
                }

                // and then go ahead and imply the dedents.
                SetIndent(0, null);
                _state.PendingDedents--;
                return Tokens.DedentToken;
            }

            return Tokens.EndOfFileToken;
        }

        private static ErrorToken BadChar(int ch) {
            return new ErrorToken(StringUtils.AddSlashes(((char)ch).ToString()));
        }

        private static bool IsNameStart(int ch) {
            return Char.IsLetter((char)ch) || ch == '_';
        }

        private static bool IsNamePart(int ch) {
            return Char.IsLetterOrDigit((char)ch) || ch == '_';
        }

        private Token ReadString(char quote, bool isRaw, bool isUni, bool isBytes) {
            int sadd = 0;
            bool isTriple = false;

            if (NextChar(quote)) {
                if (NextChar(quote)) {
                    isTriple = true; sadd += 3;
                } else {
                    _buffer.Back();
                    sadd++;
                }
            } else {
                sadd++;
            }

            if (isRaw) sadd++;
            if (isUni) sadd++;
            if (isBytes) sadd++;

            return ContinueString(quote, isRaw, isUni, isBytes, isTriple, sadd);
        }

        private Token ContinueString(char quote, bool isRaw, bool isUnicode, bool isBytes, bool isTriple, int startAdd) {
            bool complete = true;
            bool multi_line = false;
            int end_add = 0;
            int eol_size = 0;

            for (; ; ) {
                int ch = NextChar();

                if (ch == EOF) {

                    if (_verbatim) {
                        complete = !isTriple;
                        break;
                    }
                    _buffer.Back();

                    // CPython reports the multi-line string error as if it is a single line
                    // ending at the last char in the file.
                    if (isTriple) {
                        _buffer.Back();
                        _buffer.MarkTokenEnd(false);
                        ReportSyntaxError(new SourceSpan(_buffer.TokenEnd, _buffer.TokenEnd), Resources.EofInTripleQuotedString, ErrorCodes.SyntaxError | ErrorCodes.IncompleteToken);
                    } else {
                        _buffer.MarkTokenEnd(multi_line);
                    }
                    
                    UnexpectedEndOfString(isTriple, isTriple);
                    return new ErrorToken(Resources.EofInString);

                } else if (ch == quote) {

                    if (isTriple) {
                        if (NextChar(quote) && NextChar(quote)) {
                            end_add += 3;
                            break;
                        }
                    } else {
                        end_add++;
                        break;
                    }

                } else if (ch == '\\') {

                    ch = NextChar();

                    if (ch == EOF) {
                        _buffer.Back();

                        if (_verbatim) {
                            complete = false;
                            break;
                        }

                        _buffer.MarkTokenEnd(multi_line);
                        UnexpectedEndOfString(isTriple, isTriple);
                        return new ErrorToken(Resources.EofInString);

                    } else if ((eol_size = _buffer.ReadEolnOpt(ch)) > 0) {

                        // skip \<eoln> unless followed by EOF:
                        if (_buffer.Peek() == EOF) {

                            // backup over the eoln:
                            _buffer.SeekRelative(-eol_size);
                            _buffer.MarkTokenEnd(multi_line);

                            // incomplete string in the form "abc\

                            if (_verbatim && isTriple) {
                                // return the partial string in verbatim mode
                                string incompleteContents = _buffer.GetTokenSubstring(startAdd, _buffer.TokenLength - startAdd - end_add - 1);
                                incompleteContents = NormalizeMultiLineEndings(isTriple, multi_line, incompleteContents);
                                return MakeStringToken(quote, isRaw, isUnicode, isBytes, isTriple, false, incompleteContents);
                            } else {
                                UnexpectedEndOfString(isTriple, true);
                                return new ErrorToken(Resources.EofInString);
                            }
                        }

                        multi_line = true;

                    } else if (ch != quote && ch != '\\') {
                        _buffer.Back();
                    }

                } else if ((eol_size = _buffer.ReadEolnOpt(ch)) > 0) {
                    if (!isTriple) {

                        // backup over the eoln:
                        _buffer.SeekRelative(-eol_size);

                        _buffer.MarkTokenEnd(multi_line);
                        UnexpectedEndOfString(isTriple, false);
                        return new ErrorToken((quote == '"') ? Resources.NewLineInDoubleQuotedString : Resources.NewLineInSingleQuotedString);
                    }

                    multi_line = true;
                }
            }

            _buffer.MarkTokenEnd(multi_line);

            // TODO: do not create a string, parse in place
            string contents = _buffer.GetTokenSubstring(startAdd, _buffer.TokenLength - startAdd - end_add); //.Substring(_start + startAdd, end - _start - (startAdd + eadd));

            contents = NormalizeMultiLineEndings(isTriple, multi_line, contents);

            return MakeStringToken(quote, isRaw, isUnicode, isBytes, isTriple, complete, contents);
        }

        private string NormalizeMultiLineEndings(bool isTriple, bool multi_line, string contents) {
            // EOLN should be normalized to '\n' in triple-quoted strings:
            // TODO: do this better
            if (multi_line && isTriple && !_disableLineFeedLineSeparator) {
                contents = contents.Replace("\r\n", "\n").Replace("\r", "\n");
            }
            return contents;
        }

        private Token MakeStringToken(char quote, bool isRaw, bool isUnicode, bool isBytes, bool isTriple, bool complete, string contents) {
            if (!isBytes) {
                contents = LiteralParser.ParseString(contents, isRaw, isUnicode || UnicodeLiterals, complete);
                if (complete) {
                    if (isUnicode) {
                        return new UnicodeStringToken(contents);
                    }
                    return new ConstantValueToken(contents);
                } else {
                    _state.IncompleteString = new IncompleteStringToken(contents, quote == '\'', isRaw, isUnicode, isTriple);
                    return _state.IncompleteString;
                }
            } else {
                List<byte> data = LiteralParser.ParseBytes(contents, isRaw, complete);
                if (complete) {
                    if (data.Count == 0) {
                        return new ConstantValueToken(Bytes.Empty);
                    }

                    return new ConstantValueToken(new Bytes(data));
                } else {
                    _state.IncompleteString = new IncompleteStringToken(data, quote == '\'', isRaw, false, isTriple);
                    return _state.IncompleteString;
                }
            }
        }

        private void UnexpectedEndOfString(bool isTriple, bool isIncomplete) {
            string message = isTriple ? Resources.EofInTripleQuotedString : Resources.EolInSingleQuotedString;
            int error = isIncomplete ? ErrorCodes.SyntaxError | ErrorCodes.IncompleteToken : ErrorCodes.SyntaxError;

            ReportSyntaxError(_buffer.TokenSpan, message, error);
        }

        private Token ReadNumber(int start) {
            int b = 10;
            if (start == '0') {
                if (NextChar('x') || NextChar('X')) {
                    return ReadHexNumber();
                } else {
                    if (NextChar('b') || NextChar('B')) {
                        return ReadBinaryNumber();
                    } else if (NextChar('o') || NextChar('O')) {
                        return ReadOctalNumber();
                    }
                }
                b = 8;
            }

            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case '.':
                        return ReadFraction();

                    case 'e':
                    case 'E':
                        return ReadExponent();

                    case 'j':
                    case 'J':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    case 'l':
                    case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(_buffer.GetTokenString(), b));

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
                        break;

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseInteger(_buffer.GetTokenString(), b));
                }
            }
        }

        private Token ReadBinaryNumber() {
            int bits = 0;
            int iVal = 0;
            bool useBigInt = false;
            BigInteger bigInt = BigInteger.Zero;
            while (true) {
                int ch = NextChar();
                switch (ch) {
                    case '0':
                        if (iVal != 0) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    case '1':
                        bits++;
                        if (bits == 32) {
                            useBigInt = true;
                            bigInt = (BigInteger)iVal;
                        }

                        if (bits >= 32) {
                            bigInt = (bigInt << 1) | (ch - '0');
                        } else {
                            iVal = iVal << 1 | (ch - '0');
                        }
                        break;
                    case 'l':
                    case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        return new ConstantValueToken(useBigInt ? bigInt : (BigInteger)iVal);
                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        return new ConstantValueToken(useBigInt ? (object)bigInt : (object)iVal);
                }
            }
        }

        private Token ReadOctalNumber() {
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                        break;

                    case 'l':
                    case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(_buffer.GetTokenSubstring(2, _buffer.TokenLength - 2), 8));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseInteger(_buffer.GetTokenSubstring(2), 8));
                }
            }
        }

        private Token ReadHexNumber() {
            while (true) {
                int ch = NextChar();

                switch (ch) {
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
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        break;

                    case 'l':
                    case 'L':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseBigInteger(_buffer.GetTokenSubstring(2, _buffer.TokenLength - 3), 16));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseInteger(_buffer.GetTokenSubstring(2), 16));
                }
            }
        }

        private Token ReadFraction() {
            while (true) {
                int ch = NextChar();

                switch (ch) {
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
                        break;

                    case 'e':
                    case 'E':
                        return ReadExponent();

                    case 'j':
                    case 'J':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseFloat(_buffer.GetTokenString()));
                }
            }
        }

        private Token ReadExponent() {
            int ch = NextChar();

            if (ch == '-' || ch == '+') {
                ch = NextChar();
            }

            while (true) {
                switch (ch) {
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
                        ch = NextChar();
                        break;

                    case 'j':
                    case 'J':
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(LiteralParser.ParseImaginary(_buffer.GetTokenString()));

                    default:
                        _buffer.Back();
                        _buffer.MarkSingleLineTokenEnd();

                        // TODO: parse in place
                        return new ConstantValueToken(ParseFloat(_buffer.GetTokenString()));
                }
            }
        }

        private Token ReadName() {
            int ch;

            do { ch = NextChar(); } while (IsNamePart(ch));
            _buffer.Back();

            _buffer.MarkSingleLineTokenEnd();

            string name = _buffer.GetTokenString();
            if (name == "None") return Tokens.NoneToken;

            Token result;
            if (Tokens.Keywords.TryGetValue(name, out result)) {
                if (result != Tokens.KeywordPrintToken || !_printFunction) {
                    return result;
                }
            }

            return new NameToken(name);
        }

        public int GroupingLevel {
            get {
                return _state.ParenLevel + _state.BraceLevel + _state.BracketLevel;
            }
        }

        /// <summary>
        /// True if the last characters in the buffer are a backslash followed by a new line indicating
        /// that their is an incompletement statement which needs further input to complete.
        /// </summary>
        public bool EndContinues {
            get {
                return _endContinues;
            }
        }

        /// <summary>
        /// Returns whether the 
        /// </summary>
        private bool ReadNewline() {
            // Check whether we're currently scanning for inconsistent use of identation characters. If
            // we are we'll switch to using a slower version of this method with the extra checks embedded.
            if (IndentationInconsistencySeverity != Severity.Ignore)
                return ReadNewlineWithChecks();

            int spaces = 0;
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case ' ': spaces += 1; break;
                    case '\t': spaces += 8 - (spaces % 8); break;
                    case '\f': spaces = 0; break;

                    case '#':
                        if (_verbatim) {
                            _buffer.Back();
                            _buffer.MarkMultiLineTokenEnd();
                            return true;
                        } else {
                            ch = _buffer.ReadLine();
                            break;
                        }
                    default:
                        _buffer.Back();

                        if (GroupingLevel > 0) {
                            return false;
                        }

                        _buffer.MarkMultiLineTokenEnd();

                        // if there's a blank line then we don't want to mess w/ the
                        // indentation level - Python says that blank lines are ignored.
                        // And if we're the last blank line in a file we don't want to
                        // increase the new indentation level.
                        if (ch == EOF) {
                            if (spaces < _state.Indent[_state.IndentLevel]) {
                                if (_sourceUnit.Kind == SourceCodeKind.InteractiveCode ||
                                    _sourceUnit.Kind == SourceCodeKind.Statements) {
                                    SetIndent(spaces, null);
                                } else {
                                    DoDedent(spaces, _state.Indent[_state.IndentLevel]);
                                }
                            }
                        } else if (ch != '\n' && ch != '\r') {
                            SetIndent(spaces, null);
                        }

                        return true;
                }
            }
        }

        // This is another version of ReadNewline with nearly identical semantics. The difference is
        // that checks are made to see that indentation is used consistently. This logic is in a
        // duplicate method to avoid inflicting the overhead of the extra logic when we're not making
        // the checks.
        private bool ReadNewlineWithChecks() {
            // Keep track of the indentation format for the current line
            StringBuilder sb = new StringBuilder(80);

            int spaces = 0;
            while (true) {
                int ch = NextChar();

                switch (ch) {
                    case ' ': spaces += 1; sb.Append(' '); break;
                    case '\t': spaces += 8 - (spaces % 8); sb.Append('\t'); break;
                    case '\f': spaces = 0; sb.Append('\f'); break;

                    case '#':
                        if (_verbatim) {
                            _buffer.Back();
                            _buffer.MarkMultiLineTokenEnd();
                            return true;
                        } else {
                            ch = _buffer.ReadLine();
                            break;
                        }

                    default:
                        if (_buffer.ReadEolnOpt(ch) > 0) {
                            spaces = 0;
                            sb.Length = 0;
                            break;
                        }

                        _buffer.Back();

                        if (GroupingLevel > 0) {
                            return false;
                        }

                        _buffer.MarkMultiLineTokenEnd();

                        // We've captured a line of significant identation (i.e. not pure whitespace).
                        // Check that any of this indentation that's in common with the current indent
                        // level is constructed in exactly the same way (i.e. has the same mix of spaces
                        // and tabs etc.).
                        CheckIndent(sb);

                        // if there's a blank line then we don't want to mess w/ the
                        // indentation level - Python says that blank lines are ignored.
                        // And if we're the last blank line in a file we don't want to
                        // increase the new indentation level.
                        if (ch == EOF) {
                            if (spaces < _state.Indent[_state.IndentLevel]) {
                                if (_sourceUnit.Kind == SourceCodeKind.InteractiveCode ||
                                    _sourceUnit.Kind == SourceCodeKind.Statements) {
                                    SetIndent(spaces, sb);
                                } else {
                                    DoDedent(spaces, _state.Indent[_state.IndentLevel]);
                                }
                            }
                        } else if (ch != '\n' && ch != '\r') {
                            SetIndent(spaces, sb);
                        }


                        return true;
                }
            }
        }

        private void CheckIndent(StringBuilder sb) {
            if (_state.Indent[_state.IndentLevel] > 0) {
                StringBuilder previousIndent = _state.IndentFormat[_state.IndentLevel];
                int checkLength = previousIndent.Length < sb.Length ? previousIndent.Length : sb.Length;
                for (int i = 0; i < checkLength; i++) {
                    if (sb[i] != previousIndent[i]) {

                        SourceLocation eoln_token_end = _buffer.TokenEnd;

                        // We've hit a difference in the way we're indenting, report it.
                        _errors.Add(_sourceUnit, Resources.InconsistentWhitespace,
                            new SourceSpan(eoln_token_end, eoln_token_end), // TODO: we can report better span - starting at the beginning of the line
                            ErrorCodes.TabError, _indentationInconsistencySeverity
                        );

                        // We only report problems once per module, so switch back to the fast algorithm.
                        _indentationInconsistencySeverity = Severity.Ignore;
                    }
                }
            }
        }

        private void SetIndent(int spaces, StringBuilder chars) {
            int current = _state.Indent[_state.IndentLevel];
            if (spaces == current) {
                return;
            } else if (spaces > current) {
                _state.Indent[++_state.IndentLevel] = spaces;
                if (_state.IndentFormat != null)
                    _state.IndentFormat[_state.IndentLevel] = chars;
                _state.PendingDedents = -1;
                return;
            } else {
                current = DoDedent(spaces, current);

                if (spaces != current) {
                    ReportSyntaxError(
                        new SourceSpan(new SourceLocation(_buffer.TokenEnd.Index, _buffer.TokenEnd.Line, _buffer.TokenEnd.Column-1), 
                            _buffer.TokenEnd),
                        Resources.IndentationMismatch, ErrorCodes.IndentationError);
                }
            }
        }

        private int DoDedent(int spaces, int current) {
            while (spaces < current) {
                _state.IndentLevel -= 1;
                _state.PendingDedents += 1;
                current = _state.Indent[_state.IndentLevel];
            }
            return current;
        }

        private object ParseInteger(string s, int radix) {
            try {
                return LiteralParser.ParseInteger(s, radix);
            } catch (ArgumentException e) {
                ReportSyntaxError(_buffer.TokenSpan, e.Message, ErrorCodes.SyntaxError);
            }
            return 0;
        }

        private object ParseFloat(string s) {
            try {
                return LiteralParser.ParseFloat(s);
            } catch (Exception e) {
                ReportSyntaxError(_buffer.TokenSpan, e.Message, ErrorCodes.SyntaxError);
                return 0.0;
            }
        }

        internal static bool TryGetEncoding(Encoding defaultEncoding, string line, ref Encoding enc, out string encName) {
            // encoding is "# coding: <encoding name>
            // minimum length is 18
            encName = null;
            if (line.Length < 10) return false;
            if (line[0] != '#') return false;

            // we have magic comment line
            int codingIndex;
            if ((codingIndex = line.IndexOf("coding")) == -1) return false;
            if (line.Length <= (codingIndex + 6)) return false;
            if (line[codingIndex + 6] != ':' && line[codingIndex + 6] != '=') return false;

            // it contains coding: or coding=
            int encodingStart = codingIndex + 7;
            while (encodingStart < line.Length) {
                if (!Char.IsWhiteSpace(line[encodingStart])) break;

                encodingStart++;
            }

            // line is coding: [all white space]
            if (encodingStart == line.Length) return false;

            int encodingEnd = encodingStart;
            while (encodingEnd < line.Length) {
                if (Char.IsWhiteSpace(line[encodingEnd])) break;

                encodingEnd++;
            }

            // get the encoding string name
            encName = line.Substring(encodingStart, encodingEnd - encodingStart);

            // and we have the magic ending as well...
            if (StringOps.TryGetEncoding(encName, out enc)) {
#if !SILVERLIGHT
                enc.DecoderFallback = new NonStrictDecoderFallback();
#endif
                return true;
            }
            return false;
        }

        private void ReportSyntaxError(SourceSpan span, string message, int errorCode) {
            _errors.Add(_sourceUnit, message, span, errorCode, Severity.FatalError);
        }

        [Conditional("DUMP_TOKENS")]
        private void DumpBeginningOfUnit() {
            Console.WriteLine("--- Source unit: '{0}' ---", _sourceUnit.Path);
        }

        [Conditional("DUMP_TOKENS")]
        private static void DumpToken(Token token) {
            Console.WriteLine("{0} `{1}`", token.Kind, token.Image.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"));
        }

        // TODO: Make this private after two of these objects can be compared from Python code.
        [Serializable]
        public struct State : IEquatable<State> {
            // indentation state
            public int[] Indent;
            public int IndentLevel;
            public int PendingDedents;
            public bool LastNewLine;        // true if the last token we emitted was a new line.
            public IncompleteStringToken IncompleteString;

            // Indentation state used only when we're reporting on inconsistent identation format.
            public StringBuilder[] IndentFormat;

            // grouping state
            public int ParenLevel, BraceLevel, BracketLevel;

            public State(State state) {
                Indent = (int[])state.Indent.Clone();
                LastNewLine = state.LastNewLine;
                BracketLevel = state.BraceLevel;
                ParenLevel = state.ParenLevel;
                BraceLevel = state.BraceLevel;
                PendingDedents = state.PendingDedents;
                IndentLevel = state.IndentLevel;
                IndentFormat = (state.IndentFormat != null) ? (StringBuilder[])state.IndentFormat.Clone() : null;
                IncompleteString = state.IncompleteString;
            }

            public State(object dummy) {
                Indent = new int[MaxIndent]; // TODO
                LastNewLine = false;
                BracketLevel = ParenLevel = BraceLevel = PendingDedents = IndentLevel = 0;
                IndentFormat = null;
                IncompleteString = null;
            }

            public override bool Equals(object obj) {
                if (obj is State) {
                    State other = (State)obj;
                    return other == this;
                } else {
                    return false;
                }
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }

            public static bool operator ==(State left, State right) {
                if (left == null) return right == null;

                return left.BraceLevel == right.BraceLevel &&
                       left.BracketLevel == right.BracketLevel &&
                       ((left.IncompleteString == null && right.IncompleteString == null) ||
                       (left.IncompleteString != null && right.IncompleteString != null &&
                       left.IncompleteString.IsRaw == right.IncompleteString.IsRaw &&
                       left.IncompleteString.IsSingleTickQuote == right.IncompleteString.IsSingleTickQuote &&
                       left.IncompleteString.IsTripleQuoted == right.IncompleteString.IsTripleQuoted &&
                       left.IncompleteString.IsUnicode == right.IncompleteString.IsUnicode)) &&
                       left.IndentLevel == right.IndentLevel &&
                       left.ParenLevel == right.ParenLevel &&
                       left.PendingDedents == right.PendingDedents &&
                       left.LastNewLine == right.LastNewLine;
            }

            public static bool operator !=(State left, State right) {
                return !(left == right);
            }

            #region IEquatable<State> Members

            public bool Equals(State other) {
                return this.Equals(other);
            }

            #endregion
        }
    }
}
