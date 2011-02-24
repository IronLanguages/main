/***** BEGIN LICENSE BLOCK *****
 * Version: CPL 1.0
 *
 * The contents of this file are subject to the Common Public
 * License Version 1.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.eclipse.org/legal/cpl-v10.html
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * Copyright (C) 2007 Ola Bini <ola@ologix.com>
 * Copyright (c) Microsoft Corporation.
 * 
 ***** END LICENSE BLOCK *****/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using IronRuby.Compiler;

namespace IronRuby.StandardLibrary.Yaml {

    public class Scanner : IEnumerable<Token> {

        private class SimpleKey {
            internal readonly int TokenNumber;
            internal readonly bool Required;
            internal readonly int Column;

            internal SimpleKey(int tokenNumber, bool required, int column) {
                TokenNumber = tokenNumber;
                Required = required;
                Column = column;
            }
        }

        private bool _done = false;
        private readonly Stack<Token> _collectionTokens = new Stack<Token>();
        private int _tokensTaken = 0;
        private int _indent = -1;
        private bool _allowSimpleKey = true;
        private bool _eof = false;
        private int _column = 0;
        private int _line = 0;
        private bool _docStart = false;
        private readonly LinkedList<Token> _tokens = new LinkedList<Token>();
        private readonly Stack<int> _indents = new Stack<int>();
        private readonly Dictionary<int, SimpleKey> _possibleSimpleKeys = new Dictionary<int, SimpleKey>();
        private readonly TextReader/*!*/ _reader;
        private readonly Encoding/*!*/ _encoding;

        private char[] _buffer = new char[10];
        private int _count = 0;
        private int _pointer = 0;

        public Scanner(TextReader/*!*/ reader, Encoding/*!*/ encoding) {
            _encoding = encoding;
            _reader = reader;
            FetchStreamStart();
        }

        private void ReportError(string/*!*/ message, params object[]/*!*/ args) {
            throw new ScannerException(
                String.Format(CultureInfo.InvariantCulture, message, args) + 
                String.Format(CultureInfo.InvariantCulture, " (line {0}, column {1})", _line + 1, _column + 1)
            );
        }

        private void ReportUnexpectedCharacter(string/*!*/ context, char expected) {
            ReportUnexpectedCharacter(context, "`" + expected + "'");
        }

        private void ReportUnexpectedCharacter(string/*!*/ context, string/*!*/ expected) {
            ReportError("while scanning {0}: expected {1}", context, expected);
        }

        private void ReportUnexpectedCharacter(string/*!*/ context, char expected, char unexpected) {
            ReportUnexpectedCharacter(context, "`" + expected + "'", unexpected);
        }

        private void ReportUnexpectedCharacter(string/*!*/ context, string/*!*/ expected, char unexpected) {
            ReportError("while scanning {0}: expected {1}, but found `{2}' ({3})", context, expected, unexpected, (int)unexpected);
        }

        private int FlowLevel {
            get { return _collectionTokens.Count; }
        }

        public int Line {
            get { return _line; }
        }

        public int Column {
            get { return _column; }
        }
        
        public Encoding/*!*/ Encoding {
            get { return _encoding; }
        }

        private void Update(int length, bool reset) {
            if (!_eof && reset) {
                // remove from [0, _pointer)
                int newCount = _count - _pointer;
                Array.Copy(_buffer, _pointer, _buffer, 0, newCount);
                _pointer = 0;
                _count = newCount;
            }

            int desiredCount = _pointer + length + 1;
            EnsureCapacity(desiredCount);

            while (_count < desiredCount) {
                int charsRead = 0;
                if (!_eof) {
                    try {
                        charsRead = _reader.Read(_buffer, _count, desiredCount - _count);
                    } catch (IOException e) {
                        throw new YamlException(e.Message, e);
                    }
                    if (charsRead == 0) {
                        _eof = true;
                    }
                }
                if (_eof) {
                    EnsureCapacity(_count + 1);
                    // TODO: scanner should not be using \0 as an end of stream marker
                    _buffer[_count++] = '\0';
                    return;
                } else {
                    //checkPrintable(_count, charsRead);
                    _count += charsRead;
                }
            }
        }

        private void EnsureCapacity(int min) {
            if (_buffer.Length < min) {
                int newSize = Math.Max(_buffer.Length * 2, min);
                char[] newBuffer = new char[newSize];
                if (_count > 0) {
                    Array.Copy(_buffer, 0, newBuffer, 0, _count);
                }
                _buffer = newBuffer;
            }
        }

        private bool Ensure(int len, bool reset) {
            if (_pointer + len >= _count) {
                Update(len, reset);
            }
            return true;
        }

        private char Peek() {
            Ensure(1, false);
            return _buffer[_pointer];
        }

        private char Peek(int index) {
            Ensure(index + 1, false);
            return _buffer[_pointer + index];
        }

        private void Advance() {
            char c = _buffer[_pointer++];
            if (c == '\n' || (c == '\r' && _buffer[_pointer] != '\n')) {
                _possibleSimpleKeys.Clear();
                _column = 0;
                _line++;
            } else {
                _column++;
            }
        }

        private void Forward() {
            Ensure(2, true);
            Advance();
        }

        private void Forward(int length) {
            Ensure(length + 1, true);
            for (int i = 0; i < length; i++) {
                Advance();
            }
        }

        public Token PeekToken() {
            return PeekToken(0);
        }

        public Token GetToken() {
            while (NeedMoreTokens()) {
                FetchMoreTokens();
            }
            if (_tokens.Count > 0) {
                _tokensTaken++;
                Token t = _tokens.First.Value;
                _tokens.RemoveFirst();
//                 Console.WriteLine(t);
                return t;
            }
            return null;
        }


        private Token PeekToken(int index) {
            while (NeedMoreTokens(index + 1)) {
                FetchMoreTokens();
            }
            return (_tokens.Count > 0) ? _tokens.First.Value : null;
        }

        private bool NeedMoreTokens() {
            return NeedMoreTokens(1);
        }

        private bool NeedMoreTokens(int needed) {
            return !_done && (_tokens.Count < needed || NextPossibleSimpleKey() == _tokensTaken);
        }

        private Token AddToken(Token t) {
            _tokens.AddLast(t);
            return t;
        }

        private bool IsEnding() {
            return Peek() == '-' && Peek(1) == '-' && Peek(2) == '-' && IsWhitespace(Peek(3));
        }

        private bool IsStart() {
            return Peek() == '.' && Peek(1) == '.' && Peek(2) == '.' && IsWhitespace(Peek(3));
        }

        private Token FetchMoreTokens() {
            while (true) {
                ScanToNextToken();
                UnwindIndent(_column);
                char c = Peek();
                char d;
                bool atLineStart = _column == 0;
                bool whitespaceFollows;
                string s;

                switch (c) {
                    case '\0': 
                        return FetchStreamEnd();

                    case '\'': 
                        return FetchSingle();

                    case '"': 
                        return FetchDouble();

                    case '?':
                        whitespaceFollows = IsWhitespace(Peek(1));
                        if (FlowLevel != 0 || whitespaceFollows) {
                            return FetchKey();
                        } else if (!whitespaceFollows) {
                            return FetchPlain();
                        }
                        break;

                    case ':':
                        whitespaceFollows = IsWhitespace(Peek(1));
                        if (FlowLevel != 0 || whitespaceFollows) {
                            // key: value not allowed in a sequence [...]:
                            if (FlowLevel == 0 || _collectionTokens.Peek() != FlowSequenceStartToken.Instance) {
                                return FetchValue();
                            }
                        }

                        if (!whitespaceFollows) {
                            return FetchPlain();
                        }
                        break;

                    case '%': 
                        if (atLineStart) { 
                            return FetchDirective();
                        } 
                        break;

                    case '-':
                        if ((atLineStart || _docStart) && IsEnding()) {
                            return FetchDocumentStart();
                        } else if (IsWhitespace(Peek(1))) {
                            return FetchBlockEntry();
                        } else {
                            return FetchPlain();
                        } 

                    case '.':
                        if (atLineStart && IsStart()) {
                            return FetchDocumentEnd();
                        } else {
                            return FetchPlain();
                        }

                    case '[': 
                        return FetchFlowCollectionStart(FlowSequenceStartToken.Instance);

                    case '{': 
                        return FetchFlowCollectionStart(FlowMappingStartToken.Instance);

                    case ']': 
                        return FetchFlowCollectionEnd(FlowSequenceEndToken.Instance);

                    case '}': 
                        return FetchFlowCollectionEnd(FlowMappingEndToken.Instance);

                    case ',':
                        d = Peek(1);
                        if (d == ' ' || d == '\n') {
                            return FetchFlowEntry();
                        } else {
                            return FetchPlain();
                        }

                    case '*':
                    case '&':
                        s = PeekIdentifier(1);
                        return s.Length == 0 ? FetchPlain() : FetchAnchor(s, c == '*');

                    case '!': 
                        return FetchTag();

                    case '|': 
                        if (FlowLevel == 0) { 
                            return FetchLiteral(); 
                        } 
                        break;

                    case '>':
                        if (!IsWhitespace(Peek(1))) {
                            return FetchPlain();
                        } else if (FlowLevel == 0) {
                            return FetchFolded();
                        }
                        break;

                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                    case '#':
                        Debug.Assert(false);
                        break;

                    default:
                        return FetchPlain();
                }

                ReportError("unexpected `{0}' ({1})", c, (int)c);
                Forward();
            }
        }

        private Token FetchStreamStart() {
            _docStart = true;
            return AddToken(StreamStartToken.Instance);
        }

        private Token FetchStreamEnd() {
            UnwindIndent(-1);
            _allowSimpleKey = false;
            _possibleSimpleKeys.Clear();
            _done = true;
            _docStart = false;
            return AddToken(StreamEndToken.Instance);
        }

        private void ScanToNextToken() {
            while (true) {
                while (Peek() == ' ' || Peek() == '\t') {
                    Forward();
                }
                if (Peek() == '#') {
                    Forward();
                    while (!NULL_OR_LINEBR(Peek())) {
                        Forward();
                    }
                }
                if (ScanLineBreak().Length != 0) {
                    if (FlowLevel == 0) {
                        _allowSimpleKey = true;
                    }
                } else {
                    break;
                }
            }
        }

        private string/*!*/ ScanLineBreak() {
            char c = Peek();
            if (c == '\n') {
                Forward();
                return "\n";
            } else if (c == '\r' && Peek(1) == '\n') {
                Forward(2);
                return "\n";
            } else {
                return "";
            }
        }

        private void UnwindIndent(int col) {
            if (FlowLevel != 0) {
                return;
            }

            while (_indent > col) {
                _indent = _indents.Pop();
                _tokens.AddLast(BlockEndToken.Instance);
            }
        }

        private Token FetchDocumentStart() {
            _docStart = false;
            return FetchDocumentIndicator(DocumentStartToken.Instance);
        }

        private Token FetchDocumentIndicator(Token tok) {
            UnwindIndent(-1);
            RemovePossibleSimpleKey();
            _allowSimpleKey = false;
            Forward(3);
            return AddToken(tok);
        }

        private Token FetchBlockEntry() {
            _docStart = false;
            if (FlowLevel == 0) {
                if (!_allowSimpleKey) {
                    ReportError("sequence entries are not allowed here");
                }
                if (AddIndent(_column)) {
                    _tokens.AddLast(BlockSequenceStartToken.Instance);
                }
            }
            _allowSimpleKey = true;
            RemovePossibleSimpleKey();
            Forward();
            return AddToken(BlockEntryToken.Instance);
        }

        private bool AddIndent(int col) {
            if (_indent < col) {
                _indents.Push(_indent);
                _indent = col;
                return true;
            }
            return false;
        }

        private Token FetchTag() {
            _docStart = false;
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(ScanTag());
        }

        private void RemovePossibleSimpleKey() {
            SimpleKey key;
            if (_possibleSimpleKeys.TryGetValue(FlowLevel, out key)) {
                _possibleSimpleKeys.Remove(FlowLevel);
                if (key.Required) {
                    ReportUnexpectedCharacter("simple key", ':');
                }
            }
        }

        private void SavePossibleSimpleKey() {
            if (_allowSimpleKey) {
                RemovePossibleSimpleKey();
                _possibleSimpleKeys.Add(FlowLevel, new SimpleKey(_tokensTaken + _tokens.Count, (FlowLevel == 0) && _indent == _column, _column));
            }
        }

        private Token ScanTag() {
            char ch = Peek(1);
            string handle = null;
            string suffix = null;
            if (ch == '<') {
                Forward(2);
                suffix = ScanTagUri("tag");
                if (Peek() != '>') {
                    ReportError("tag", '>', Peek());
                }
                Forward();
            } else if (IsWhitespace(ch)) {
                suffix = "!";
                Forward();
            } else {
                int length = 1;
                bool useHandle = false;
                while (!IsWhitespace(ch)) {
                    if (ch == '!') {
                        useHandle = true;
                        break;
                    }
                    length++;
                    ch = Peek(length);
                }
                handle = "!";
                if (useHandle) {
                    handle = ScanTagHandle("tag");
                } else {
                    handle = "!";
                    Forward();
                }
                suffix = ScanTagUri("tag");
            }
            if (!IsWhitespaceButTab(Peek())) {
                ReportUnexpectedCharacter("tag", ' ', Peek());
            }
            return new TagToken(handle, suffix);
        }

        private string ScanTagUri(string name) {
            StringBuilder chunks = new StringBuilder(10);

            int length = 0;
            char ch = Peek(length);
            while (STRANGE_CHAR(ch)) {
                if ('%' == ch) {
                    Ensure(length, false);
                    chunks.Append(_buffer, _pointer, length);
                    length = 0;
                    ScanUriEscapes(chunks, name);
                } else {
                    length++;
                }
                ch = Peek(length);
            }
            if (length != 0) {
                Ensure(length, false);
                chunks.Append(_buffer, _pointer, length);
                Forward(length);
            }
            if (chunks.Length == 0) {
                ReportUnexpectedCharacter(name, "URI", ch);
            }
            return chunks.ToString();
        }

        private string ScanTagHandle(string name) {
            char ch = Peek();
            if (ch != '!') {
                ReportUnexpectedCharacter(name, "!", ch);
            }
            int length = 1;
            ch = Peek(length);
            if (ch != ' ') {
                while (IsIdentifier(ch)) {
                    length++;
                    ch = Peek(length);
                }
                if ('!' != ch) {
                    Forward(length);
                    ReportUnexpectedCharacter(name, '!', ch);
                }
                length++;
            }
            Ensure(length, false);
            string value = new string(_buffer, _pointer, length);
            Forward(length);
            return value;
        }

        private void ScanUriEscapes(StringBuilder str, string name) {
            while (Peek() == '%') {
                Forward();
                try {
                    Ensure(2, false);
                    str.Append(int.Parse(new string(_buffer, _pointer, 2), NumberStyles.HexNumber));
                } catch (FormatException) {
                    ReportError(
                        "while scanning a {0}: expected URI escape sequence of 2 hexadecimal numbers, but found `{0}' ({1}) and `{2}' ({3})",
                        name, Peek(1), Peek(2)
                    );
                }
                Forward(2);
            }
        }

        private Token FetchPlain() {
            _docStart = false;
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(ScanPlain());
        }

        private Token ScanPlain() {
            StringBuilder chunks = new StringBuilder(7);
            string spaces = "";
            int ind = _indent + 1;

            while (Peek() != '#') {
                int length = FindEndOfPlain();
                if (length == 0) {
                    break;
                }
                _allowSimpleKey = false;
                chunks.Append(spaces);
                chunks.Append(_buffer, _pointer, length);
                Forward(length);
                spaces = ScanPlainSpaces(ind);
                if (spaces == null || (FlowLevel == 0 && _column < ind)) {
                    break;
                }
            }
            return new ScalarToken(chunks.ToString(), ScalarQuotingStyle.None);
        }

        private int FindEndOfPlain() {
            int i = 0;
            while (true) {
                switch (Peek(i)) {
                    case '\t':
                    case '\0':
                    case ' ':
                    case '\n':
                        return i;

                    case '\r':
                        if (Peek(i + 1) == '\n') {
                            return i;
                        }
                        break;

                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '?':
                        if (FlowLevel != 0) {
                            return i;
                        }
                        break;

                    case ',':
                        if (FlowLevel != 0) {
                            switch (Peek(i + 1)) {
                                case '\0':
                                case ' ':
                                case '\n':
                                    return i;

                                case '\r':
                                    if (Peek(i + 2) == '\n') {
                                        return i;
                                    }
                                    break;
                            }
                        }
                        break;

                    case ':':
                        switch (Peek(i + 1)) {
                            case '\0':
                            case ' ':
                            case '\n':
                                return i;

                            case '\r':
                                if (Peek(i + 2) == '\n') {
                                    return i;
                                }
                                break;

                            case '[':
                            case ']':
                            case '{':
                            case '}':
                                if (FlowLevel != 0) {
                                    return i;
                                }
                                break;
                        }
                        break;
                }

                i++;
            }
        }

        private int NextPossibleSimpleKey() {
            foreach (SimpleKey key in _possibleSimpleKeys.Values) {
                if (key.TokenNumber > 0) {
                    return key.TokenNumber;
                }
            }
            return -1;
        }

        private string ScanPlainSpaces(int indent) {
            StringBuilder chunks = new StringBuilder();
            
            int length = 0;
            char c;
            while ((c = Peek(length)) == ' ' || c == '\t') {
                length++;
            }

            char[] whitespaces = new char[length];
            for (int i = 0; i < length; i++) {
                whitespaces[i] = _buffer[_pointer + i];
            }
            Forward(length);

            string lineBreak = ScanLineBreak();
            if (lineBreak.Length > 0) {
                _allowSimpleKey = true;
                if (IsEnding() || IsStart()) {
                    return "";
                }

                StringBuilder breaks = new StringBuilder();
                while (true) {
                    c = Peek();
                    if (c == ' ' || c == '\t') {
                        Forward();
                    } else {
                        var br = ScanLineBreak();
                        if (br.Length == 0) {
                            break;
                        }

                        breaks.Append(br);
                        if (IsEnding() || IsStart()) {
                            return "";
                        }
                    }
                }

                if (lineBreak.Length != 1 || lineBreak[0] != '\n') {
                    chunks.Append(lineBreak);
                } else if (breaks.Length == 0) {
                    chunks.Append(" ");
                }
                chunks.Append(breaks);
            } else {
                chunks.Append(whitespaces);
            }
            return chunks.ToString();
        }

        private Token FetchSingle() {
            return FetchFlowScalar(ScalarQuotingStyle.Single);
        }

        private Token FetchDouble() {
            return FetchFlowScalar(ScalarQuotingStyle.Double);
        }

        private Token FetchFlowScalar(ScalarQuotingStyle style) {
            _docStart = false;
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(ScanFlowScalar(style));
        }

        private Token ScanFlowScalar(ScalarQuotingStyle style) {
            StringBuilder chunks = new StringBuilder();

            char quote = Peek();
            Forward();
            ScanFlowScalarNonSpaces(chunks, style == ScalarQuotingStyle.Double);
            while (Peek() != quote) {
                ScanFlowScalarSpaces(chunks);
                ScanFlowScalarNonSpaces(chunks, style == ScalarQuotingStyle.Double);
            }
            Forward();
            return new ScalarToken(chunks.ToString(), style);
        }

        private char ParseHexChar(int length) {
            Ensure(length, false);

            // TODO: how do we parse 32-bit escape sequences?
            string str = new string(_buffer, _pointer, length);
            char c;
            try {
                c = (char)uint.Parse(str, NumberStyles.HexNumber);
            } catch (Exception) {
                c = ' ';
                ReportError(
                    "while scanning a double-quoted scalar: expected escape sequence of {0} hexadecimal numbers, but found `{1}'",
                    length, str
                );
            }

            Forward(length);
            return c;
        }

        private void ScanFlowScalarNonSpaces(StringBuilder chunks, bool dbl) {
            for (; ; ) {
                int length = 0;
                while (!SPACES_AND_STUFF(Peek(length))) {
                    length++;
                }
                if (length != 0) {
                    Ensure(length, false);
                    chunks.Append(_buffer, _pointer, length);
                    Forward(length);
                }
                char ch = Peek();
                if (!dbl && ch == '\'' && Peek(1) == '\'') {
                    chunks.Append('\'');
                    Forward(2);
                } else if ((dbl && ch == '\'') || (!dbl && DOUBLE_ESC(ch))) {
                    chunks.Append(ch);
                    Forward();
                } else if (dbl && ch == '\\') {
                    Forward();
                    ch = Peek();
                    int result;
                    if ((result = ESCAPE_REPLACEMENT(ch)) >= 0) {
                        chunks.Append((char)result);
                        Forward();
                    } else if ((result = ESCAPE_CODES(ch)) >= 0) {
                        length = result;
                        Forward();
                        chunks.Append(ParseHexChar(length));
                    } else if (FULL_LINEBR(ch)) {
                        ScanLineBreak();
                        chunks.Append(ScanFlowScalarBreaks());
                    } else {
                        chunks.Append('\\');
                    }
                } else {
                    return;
                }
            }
        }

        private void ScanFlowScalarSpaces(StringBuilder chunks) {
            int length = 0;
            while (BLANK_T(Peek(length))) {
                length++;
            }
            Ensure(length, false);
            string whitespaces = new string(_buffer, _pointer, length);
            Forward(length);
            char ch = Peek();
            if (ch == '\0') {
                ReportError("while scanning a quoted scalar: found unexpected end of stream");
            } else if (FULL_LINEBR(ch)) {
                string lineBreak = ScanLineBreak();
                string breaks = ScanFlowScalarBreaks();
                if (!(lineBreak.Length == 1 && lineBreak[0] == '\n')) {
                    chunks.Append(lineBreak);
                } else if (breaks.Length == 0) {
                    chunks.Append(" ");
                }
                chunks.Append(breaks);
            } else {
                chunks.Append(whitespaces);
            }
        }

        private string ScanFlowScalarBreaks() {
            StringBuilder chunks = new StringBuilder();
            bool colz = true;
            for (; ; ) {
                if (colz && (IsEnding() || IsStart())) {
                    ReportError("while scanning a quoted scalar: found unexpected document separator");
                }
                while (BLANK_T(Peek())) {
                    Forward();
                }
                if (FULL_LINEBR(Peek())) {
                    chunks.Append(ScanLineBreak());
                    colz = true;
                } else if ('\\' == Peek() && BLANK_T(Peek(1))) {
                    Forward();
                    ScanFlowScalarSpaces(chunks);
                    colz = false;
                } else {
                    return chunks.ToString();
                }
            }
        }

        private Token FetchValue() {
            _docStart = false;
            SimpleKey key;
            if (!_possibleSimpleKeys.TryGetValue(FlowLevel, out key)) {
                if (FlowLevel == 0 && !_allowSimpleKey) {
                    ReportError("mapping values are not allowed here");
                }
                _allowSimpleKey = FlowLevel == 0;
                RemovePossibleSimpleKey();
            } else {
                _possibleSimpleKeys.Remove(FlowLevel);

                // find the insertion point
                LinkedListNode<Token> node = _tokens.First;
                int skip = key.TokenNumber - _tokensTaken;
                while (skip > 0) {
                    node = node.Next;
                    skip--;
                }

                node = _tokens.AddBefore(node, KeyToken.Instance);
                if (FlowLevel == 0 && AddIndent(key.Column)) {
                    _tokens.AddBefore(node, BlockMappingStartToken.Instance);
                }
                _allowSimpleKey = false;
            }
            Forward();
            return AddToken(ValueToken.Instance);
        }

        private Token FetchFlowCollectionStart(Token tok) {
            _docStart = false;
            SavePossibleSimpleKey();
            _collectionTokens.Push(tok);
            _allowSimpleKey = true;
            Forward(1);
            return AddToken(tok);
        }


        private Token FetchDocumentEnd() {
            return FetchDocumentIndicator(DocumentEndToken.Instance);
        }

        private Token FetchFlowCollectionEnd(Token tok) {
            RemovePossibleSimpleKey();
            _collectionTokens.Pop();
            _allowSimpleKey = false;
            Forward(1);
            return AddToken(tok);
        }

        private Token FetchFlowEntry() {
            _allowSimpleKey = true;
            RemovePossibleSimpleKey();
            Forward(1);
            return AddToken(FlowEntryToken.Instance);
        }

        private Token FetchLiteral() {
            return FetchBlockScalar(ScalarQuotingStyle.Literal);
        }

        private Token FetchFolded() {
            return FetchBlockScalar(ScalarQuotingStyle.Folded);
        }

        private Token FetchBlockScalar(ScalarQuotingStyle style) {
            _docStart = false;
            _allowSimpleKey = true;
            RemovePossibleSimpleKey();
            return AddToken(ScanBlockScalar(style));
        }

        private Token ScanBlockScalar(ScalarQuotingStyle style) {
            bool folded = style == ScalarQuotingStyle.Folded;
            StringBuilder chunks = new StringBuilder();

            bool? chomping;
            int increment;
            if (!ScanBlockScalarIndicators(out chomping, out increment)) {
                return ScanPlain();
            }

            bool sameLine = ScanBlockScalarIgnoredLine();

            int minIndent = _indent + 1;
            if (minIndent < 0) {
                minIndent = 0;
            }

            int maxIndent = 0;
            int ind = 0;
            if (sameLine) {
                int length = 0;
                while (!NULL_OR_LINEBR(Peek(length))) {
                    length++;
                }
                Ensure(length, false);
                chunks.Append(_buffer, _pointer, length);
                Forward(length);
            }

            string breaks;
            if (increment == -1) {
                ScanBlockScalarIndentation(out breaks, out maxIndent);
                if (minIndent > maxIndent) {
                    ind = minIndent;
                } else {
                    ind = maxIndent;
                }
            } else {
                ind = minIndent + increment - 1;
                breaks = ScanBlockScalarBreaks(ind);
            }

            string lineBreak = "";
            while (_column == ind && Peek() != '\0') {
                chunks.Append(breaks);
                bool leadingNonSpace = !BLANK_T(Peek());
                int length = 0;
                while (!NULL_OR_LINEBR(Peek(length))) {
                    length++;
                }
                Ensure(length, false);
                chunks.Append(_buffer, _pointer, length);
                Forward(length);
                lineBreak = ScanLineBreak();
                breaks = ScanBlockScalarBreaks(ind);
                if (_column == ind && Peek() != '\0') {
                    if (folded && lineBreak.Length == 1 && lineBreak[0] == '\n' && leadingNonSpace && !BLANK_T(Peek())) {
                        if (breaks.Length == 0) {
                            chunks.Append(" ");
                        }
                    } else {
                        chunks.Append(lineBreak);
                    }
                } else {
                    break;
                }
            }

            if (chomping.GetValueOrDefault(true)) {
                chunks.Append(lineBreak);
            }
            if (chomping.GetValueOrDefault(false)) {
                chunks.Append(breaks);
            }

            return new ScalarToken(chunks.ToString(), style);
        }

        private string ScanBlockScalarBreaks(int indent) {
            StringBuilder chunks = new StringBuilder();
            while (_column < indent && Peek() == ' ') {
                Forward();
            }
            while (FULL_LINEBR(Peek())) {
                chunks.Append(ScanLineBreak());
                while (_column < indent && Peek() == ' ') {
                    Forward();
                }
            }
            return chunks.ToString();
        }

        private void ScanBlockScalarIndentation(out string breaks, out int maxIndent) {
            StringBuilder chunks = new StringBuilder();
            maxIndent = 0;
            while (BLANK_OR_LINEBR(Peek())) {
                if (Peek() != ' ') {
                    chunks.Append(ScanLineBreak());
                } else {
                    Forward();
                    if (_column > maxIndent) {
                        maxIndent = _column;
                    }
                }
            }
            breaks = chunks.ToString();
        }

        private bool ScanBlockScalarIndicators(out bool? chomping, out int increment) {
            chomping = null;
            increment = -1;

            int i = 1;
            char c = Peek(i);
            if (c == '-' || c == '+') {
                chomping = c == '+';
                i++;
                c = Peek(i);
                if (Tokenizer.IsDecimalDigit(c)) {
                    increment = c - '0';
                    if (increment == 0) {
                        ReportError("while scanning a block scalar: expected indentation indicator in_ the range 1-9, but found 0");
                    }
                    i++;
                }
            } else if (Tokenizer.IsDecimalDigit(c)) {
                increment = c - '0';
                if (increment == 0) {
                    ReportError("while scanning a block scalar: expected indentation indicator in_ the range 1-9, but found 0");
                }
                i++;
                c = Peek(i);
                if (c == '-' || c == '+') {
                    chomping = c == '+';
                    i++;
                }
            }

            if (!IsWhitespaceButTab(Peek(i))) {
                return false;
            }

            Forward(i);
            return true;
        }

        private bool ScanBlockScalarIgnoredLine() {
            bool same = true;
            while (Peek() == ' ') {
                Forward();
            }
            if (Peek() == '#') {
                while (!NULL_OR_LINEBR(Peek())) {
                    Forward();
                }
                same = false;
            }
            if (NULL_OR_LINEBR(Peek())) {
                ScanLineBreak();
                return false;
            }
            return same;
        }


        private Token FetchDirective() {
            UnwindIndent(-1);
            RemovePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(ScanDirective());
        }

        private Token FetchKey() {
            if (FlowLevel == 0) {
                if (!_allowSimpleKey) {
                    ReportError("mapping keys are not allowed here");
                }
                if (AddIndent(_column)) {
                    _tokens.AddLast(BlockMappingStartToken.Instance);
                }
            }
            _allowSimpleKey = FlowLevel == 0;
            RemovePossibleSimpleKey();
            Forward();
            return AddToken(KeyToken.Instance);
        }

        private Token/*!*/ FetchAnchor(string/*!*/ identifier, bool isAlias) {
            SavePossibleSimpleKey();
            _allowSimpleKey = false;

            Forward(1 + identifier.Length);
            return AddToken(isAlias ? (Token)new AliasToken(identifier) : new AnchorToken(identifier));
        }

        private string/*!*/ PeekIdentifier(int offset) {
            int start = offset;
            while (IsIdentifier(Peek(offset))) {
                offset++;
            }
            return (offset == start) ? String.Empty : new String(_buffer, _pointer + start, offset - start);
        }

        private Token ScanDirective() {
            Forward();
            string name = ScanDirectiveName();
            string[] value = null;
            if (name == "Yaml") {
                value = ScanYamlDirectiveValue();
            } else if (name == "TAG") {
                value = ScanTagDirectiveValue();
            } else {
                while (!NULL_OR_LINEBR(Peek())) {
                    Forward();
                }
            }
            ScanDirectiveIgnoredLine();
            return new DirectiveToken(name, value);
        }

        private string ScanDirectiveName() {
            int length = 0;
            char ch = Peek(length);
            bool zlen = true;
            while (IsIdentifier(ch)) {
                zlen = false;
                length++;
                ch = Peek(length);
            }
            if (zlen) {
                ReportUnexpectedCharacter("directive", "alphabetic or numeric character", ch);
            }
            string value = null;
            try {
                Ensure(length, false);
                value = new string(_buffer, _pointer, length);
            } catch (Exception) {
            }
            Forward(length);
            if (!IsWhitespaceButTab(Peek())) {
                ReportUnexpectedCharacter("directive", "expected alphabetic or numeric character", ch);
            }
            return value;
        }

        private string ScanDirectiveIgnoredLine() {
            while (Peek() == ' ') {
                Forward();
            }
            if (Peek() == '"') {
                while (!NULL_OR_LINEBR(Peek())) {
                    Forward();
                }
            }
            char ch = Peek();
            if (!NULL_OR_LINEBR(ch)) {
                ReportUnexpectedCharacter("directive", "a comment or a line break", Peek());
            }
            return ScanLineBreak();
        }

        private string[] ScanYamlDirectiveValue() {
            while (Peek() == ' ') {
                Forward();
            }
            string major = ScanYamlDirectiveNumber();
            if (Peek() != '.') {
                ReportUnexpectedCharacter("directive", "a digit or a dot", Peek());
            }
            Forward();
            string minor = ScanYamlDirectiveNumber();
            if (!IsWhitespaceButTab(Peek())) {
                ReportUnexpectedCharacter("directive", "a digit or a space", Peek());
            }
            return new string[] { major, minor };
        }

        private string ScanYamlDirectiveNumber() {
            char ch = Peek();
            if (!char.IsDigit(ch)) {
                ReportUnexpectedCharacter("directive", "a digit", ch);
            }
            int length = 0;
            StringBuilder sb = new StringBuilder();
            while (char.IsDigit(Peek(length))) {
                sb.Append(Peek(length));
                length++;
            }
            Forward(length);
            return sb.ToString();
        }

        private string[] ScanTagDirectiveValue() {
            while (Peek() == ' ') {
                Forward();
            }
            string handle = ScanTagDirectiveHandle();
            while (Peek() == ' ') {
                Forward();
            }
            string prefix = ScanTagDirectivePrefix();
            return new string[] { handle, prefix };
        }

        private string ScanTagDirectiveHandle() {
            string value = ScanTagHandle("directive");
            if (Peek() != ' ') {
                ReportUnexpectedCharacter("directive", ' ', Peek());
            }
            return value;
        }

        private string ScanTagDirectivePrefix() {
            string value = ScanTagUri("directive");
            if (!IsWhitespaceButTab(Peek())) {
                ReportUnexpectedCharacter("directive", ' ', Peek());
            }
            return value;
        }

        #region character classes

        private bool IsWhitespace(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return true;
            }
            return false;
        }

        private bool IsWhitespaceButTab(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\r':
                case '\n':
                    return true;
            }
            return false;
        }

        private bool FULL_LINEBR(char c) {
            return c == '\n' || c == '\r';
        }

        private bool NULL_OR_LINEBR(char c) {
            return c == '\n' || c == '\r' || c == '\0';
        }

        private bool BLANK_OR_LINEBR(char c) {
            return c == ' ' || c == '\n' || c == '\r';
        }

        private bool BLANK_T(char c) {
            return c == ' ' || c == '\t';
        }

        private bool DOUBLE_ESC(char c) {
            return c == '\\' || c == '"';
        }

        private bool SPACES_AND_STUFF(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '\\':
                case '\'':
                case '"':
                    return true;
            }
            return false;
        }

        private bool IsIdentifier(char c) {
            return Tokenizer.IsLetterOrDigit(c) || c == '-' || c == '_';
        }

        private bool STRANGE_CHAR(char c) {
            switch (c) {
                case '-':
                case '_':
                case '[':
                case ']':
                case '(':
                case ')':
                case '\'':
                case ';':
                case '/':
                case '?':
                case ':':
                case '@':
                case '&':
                case '=':
                case '+':
                case '$':
                case ',':
                case '.':
                case '!':
                case '~':
                case '*':
                case '%':
                case '^':
                case '#':
                    return true;
            }
            return char.IsLetterOrDigit(c);
        }

        private int ESCAPE_CODES(char c) {
            switch (c) {
                case 'x': return 2;
                case 'u': return 4;
                case 'U': return 8;
            }
            return -1;
        }

        private int ESCAPE_REPLACEMENT(char c) {
            switch (c) {
                case '0': return 0;
                case 'a': return 7;
                case 'b': return 8;
                case 't': return 9;
                case '\t': return 9;
                case 'n': return 10;
                case 'v': return 11;
                case 'f': return 12;
                case 'r': return 13;
                case 'e': return 27;
                case '"': return '"';
                case '\\': return '\\';
                case 'N': return 133;
                case '_': return 160;
            }            
            return -1;
        }

        #endregion

        #region IEnumerable<Token> Members

        public IEnumerator<Token> GetEnumerator() {
            while (PeekToken() != null) {
                yield return GetToken();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
