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
using System.Globalization;
using System.IO;
using System.Text;

namespace IronRuby.StandardLibrary.Yaml {

    public class Scanner : IEnumerable<Token> {

        private class SimpleKey {
            internal readonly int TokenNumber;
            internal readonly bool Required;
            internal readonly int Index;
            internal readonly int Line;
            internal readonly int Column;

            internal SimpleKey(int tokenNumber, bool required, int index, int line, int column) {
                TokenNumber = tokenNumber;
                Required = required;
                Index = index;
                Line = line;
                Column = column;
            }
        }

        private bool _done = false;
        private int _flowLevel = 0;
        private int _tokensTaken = 0;
        private int _indent = -1;
        private bool _allowSimpleKey = true;
        private bool _eof = false;
        private int _column = 0;
        private bool _docStart = false;
        private readonly LinkedList<Token> _tokens = new LinkedList<Token>();
        private readonly Stack<int> _indents = new Stack<int>();
        private Dictionary<int, SimpleKey> _possibleSimpleKeys = new Dictionary<int, SimpleKey>();
        private readonly TextReader _reader;

        private char[] _buffer = new char[10];
        private int _count = 0;
        private int _pointer = 0;

        private bool _inFlowSequence = false;

        public Scanner(TextReader/*!*/ reader) {
            _reader = reader;
            FetchStreamStart();
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

        // This function was a no-op because NON_PRINTABLE was not set in the original code
        //private void checkPrintable(int start, int len) {
        //    for (int i = start; i < start + len; i++) {
        //        if (NON_PRINTABLE[_buffer[i]]) {
        //            throw new YamlException("At " + i + " we found: " + _buffer[i] + ". Special characters are not allowed");
        //        }
        //    }
        //}

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

        private void Forward() {
            Ensure(2, true);
            char ch1 = _buffer[_pointer++];
            if (ch1 == '\n' || (ch1 == '\r' && _buffer[_pointer] != '\n')) {
                _possibleSimpleKeys.Clear();
                _column = 0;
            } else {
                _column++;
            }
        }

        private void Forward(int length) {
            Ensure(length + 1, true);
            for (int i = 0; i < length; i++) {
                char ch = _buffer[_pointer];
                _pointer++;
                if (ch == '\n' || (ch == '\r' && _buffer[_pointer] != '\n')) {
                    _possibleSimpleKeys.Clear();
                    _column = 0;
                } else {
                    _column++;
                }
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
            Ensure(4, false);
            return (_buffer[_pointer]) == '-' &&
                (_buffer[_pointer + 1]) == '-' &&
                (_buffer[_pointer + 2]) == '-' &&
                (_buffer[_pointer + 3] != 0) &&
                !(_count <= (_pointer + 4) ||
                  ((_buffer[_pointer + 3] == '\n') &&
                   (_buffer[_pointer + 4] == 0))) &&
                (NULL_BL_T_LINEBR(_buffer[_pointer + 3]));
        }

        private bool IsStart() {
            Ensure(4, false);
            return (_buffer[_pointer]) == '.' &&
                (_buffer[_pointer + 1]) == '.' &&
                (_buffer[_pointer + 2]) == '.' &&
                (NULL_BL_T_LINEBR(_buffer[_pointer + 3]));
        }

        private bool IsEndOrStart() {
            Ensure(4, false);
            return (((_buffer[_pointer]) == '-' &&
                     (_buffer[_pointer + 1]) == '-' &&
                     (_buffer[_pointer + 2]) == '-') ||
                    ((_buffer[_pointer]) == '.' &&
                     (_buffer[_pointer + 1]) == '.' &&
                     (_buffer[_pointer + 2]) == '.')) &&
                     (NULL_BL_T_LINEBR(_buffer[_pointer + 3]));
        }

        private Token FetchMoreTokens() {
            ScanToNextToken();
            UnwindIndent(_column);
            char ch = Peek();
            bool colz = _column == 0;
            switch (ch) {
                case '\0': return FetchStreamEnd();
                case '\'': return FetchSingle();
                case '"': return FetchDouble();
                case '?': if (_flowLevel != 0 || NULL_BL_T_LINEBR(Peek(1))) { return FetchKey(); } break;
                case ':':
                    if ( !_inFlowSequence && ( _flowLevel != 0 || NULL_BL_T_LINEBR(Peek(1)) ) ) {
                        return FetchValue();
                    }
                    break;
                case '%': if (colz) { return FetchDirective(); } break;
                case '-':
                    if ((colz || _docStart) && IsEnding()) {
                        return FetchDocumentStart();
                    } else if (NULL_BL_T_LINEBR(Peek(1))) {
                        return FetchBlockEntry();
                    }
                    break;
                case '.':
                    if (colz && IsStart()) {
                        return FetchDocumentEnd();
                    }
                    break;
                case '[': return FetchFlowSequenceStart();
                case '{': return FetchFlowMappingStart();
                case ']': return FetchFlowSequenceEnd();
                case '}': return FetchFlowMappingEnd();
                case ',': return fetchFlowEntry();
                case '*': return FetchAlias();
                case '&': return FetchAnchor();
                case '!': return FetchTag();
                case '|': if (_flowLevel == 0) { return FetchLiteral(); } break;
                case '>': if (_flowLevel == 0) { return FetchFolded(); } break;
            }

            //TODO: this is probably incorrect...
            char c2 = _buffer[_pointer];
            if (NOT_USEFUL_CHAR(c2) ||
               (Ensure(1, false) && (c2 == '-' || c2 == '?' || c2 == ':') &&
                !NULL_BL_T_LINEBR(_buffer[_pointer + 1]))) {
                return FetchPlain();
            }

            throw new ScannerException("while scanning for the next token: found character " + ch + " (" + (int)ch + ") that cannot start any token");
        }

        private Token FetchStreamStart() {
            _docStart = true;
            return AddToken(StreamStartToken.Instance);
        }

        private Token FetchStreamEnd() {
            UnwindIndent(-1);
            _allowSimpleKey = false;
            _possibleSimpleKeys = new Dictionary<int, SimpleKey>();
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
                    if (_flowLevel == 0) {
                        _allowSimpleKey = true;
                    }
                } else {
                    break;
                }
            }
        }

        private string ScanLineBreak() {
            // Transforms:
            //   '\r\n'      :   '\n'
            //   '\r'        :   '\n'
            //   '\n'        :   '\n'
            //   '\x85'      :   '\n'
            //   default     :   ''
            char val = Peek();
            if (FULL_LINEBR(val)) {
                Ensure(2, false);
                if (_buffer[_pointer] == '\r' && _buffer[_pointer + 1] == '\n') {
                    Forward(2);
                } else {
                    Forward();
                }
                return "\n";
            } else {
                return "";
            }
        }

        private void UnwindIndent(int col) {
            if (_flowLevel != 0) {
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
            if (_flowLevel == 0) {
                if (!_allowSimpleKey) {
                    throw new ScannerException("sequence entries are not allowed here");
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
            if (_possibleSimpleKeys.TryGetValue(_flowLevel, out key)) {
                _possibleSimpleKeys.Remove(_flowLevel);
                if (key.Required) {
                    throw new ScannerException("while scanning a simple key: could not find expected ':'");
                }
            }
        }

        private void SavePossibleSimpleKey() {
            if (_allowSimpleKey) {
                RemovePossibleSimpleKey();
                _possibleSimpleKeys.Add(_flowLevel, new SimpleKey(_tokensTaken + _tokens.Count, (_flowLevel == 0) && _indent == _column, -1, -1, _column));
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
                    throw new ScannerException("while scanning a tag: expected '>', but found " + Peek() + "(" + (int)Peek() + ")");
                }
                Forward();
            } else if (NULL_BL_T_LINEBR(ch)) {
                suffix = "!";
                Forward();
            } else {
                int length = 1;
                bool useHandle = false;
                while (!NULL_BL_T_LINEBR(ch)) {
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
            if (!NULL_BL_LINEBR(Peek())) {
                throw new ScannerException("while scanning a tag: expected ' ', but found " + Peek() + "(" + (int)Peek() + ")");
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
                throw new ScannerException("while scanning a " + name + ": expected URI, but found " + ch + "(" + (int)ch + ")");
            }
            return chunks.ToString();
        }

        private string ScanTagHandle(string name) {
            char ch = Peek();
            if (ch != '!') {
                throw new ScannerException("while scanning a " + name + ": expected '!', but found " + ch + "(" + (int)ch + ")");
            }
            int length = 1;
            ch = Peek(length);
            if (ch != ' ') {
                while (ALPHA(ch)) {
                    length++;
                    ch = Peek(length);
                }
                if ('!' != ch) {
                    Forward(length);
                    throw new ScannerException("while scanning a " + name + ": expected '!', but found " + ch + "(" + ((int)ch) + ")");
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
                } catch (FormatException fe) {
                    throw new ScannerException("while scanning a " + name + ": expected URI escape sequence of 2 hexadecimal numbers, but found " + Peek(1) + "(" + ((int)Peek(1)) + ") and " + Peek(2) + "(" + ((int)Peek(2)) + ")", fe);
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

        private delegate bool CharTest(char c);

        private Token ScanPlain() {
            StringBuilder chunks = new StringBuilder(7);
            string spaces = "";
            int ind = _indent + 1;
            bool f_nzero;
            CharTest r_check, r_check2, r_check3;
            if (_flowLevel != 0) {
                f_nzero = true;
                r_check = R_FLOWNONZERO;
                r_check2 = ALL_FALSE;
                r_check3 = ALL_FALSE;
            } else {
                f_nzero = false;
                r_check = NULL_BL_T_LINEBR;
                r_check2 = R_FLOWZERO1;
                r_check3 = NULL_BL_T_LINEBR;
            }
            while (Peek() != '#') {
                int length = 0;
                for (int i = 0; ; i++) {
                    Ensure(i + 2, false);
                    if (r_check(_buffer[_pointer + i]) || (r_check2(_buffer[_pointer + i]) && r_check3(_buffer[_pointer + i + 1]))) {
                        length = i;
                        char ch = Peek(length);
                        if (!(f_nzero && ch == ':' && !S4(Peek(length + 1)))) {
                            break;
                        }
                    }
                }

                if (length == 0) {
                    break;
                }
                _allowSimpleKey = false;
                chunks.Append(spaces);
                Ensure(length, false);
                chunks.Append(_buffer, _pointer, length);
                Forward(length);
                spaces = ScanPlainSpaces(ind);
                if (spaces == null || (_flowLevel == 0 && _column < ind)) {
                    break;
                }
            }
            return new ScalarToken(chunks.ToString(), true);
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
            while (Peek(length) == ' ') {
                length++;
            }
            char[] whitespaces = new char[length];
            for (int i = 0; i < length; i++) {
                whitespaces[i] = ' ';
            }
            Forward(length);
            char ch = Peek();
            if (FULL_LINEBR(ch)) {
                string lineBreak = ScanLineBreak();
                _allowSimpleKey = true;
                if (IsEndOrStart()) {
                    return "";
                }
                StringBuilder breaks = new StringBuilder();
                while (BLANK_OR_LINEBR(Peek())) {
                    if (' ' == Peek()) {
                        Forward();
                    } else {
                        breaks.Append(ScanLineBreak());
                        if (IsEndOrStart()) {
                            return "";
                        }
                    }
                }
                if (!(lineBreak.Length == 1 && lineBreak[0] == '\n')) {
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
            return FetchFlowScalar('\'');
        }

        private Token FetchDouble() {
            return FetchFlowScalar('"');
        }

        private Token FetchFlowScalar(char style) {
            _docStart = false;
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(ScanFlowScalar(style));
        }

        private Token ScanFlowScalar(char style) {
            bool dbl = style == '"';
            StringBuilder chunks = new StringBuilder();

            char quote = Peek();
            Forward();
            ScanFlowScalarNonSpaces(chunks, dbl);
            while (Peek() != quote) {
                ScanFlowScalarSpaces(chunks);
                ScanFlowScalarNonSpaces(chunks, dbl);
            }
            Forward();
            return new ScalarToken(chunks.ToString(), false, style);
        }

        private char ParseHexChar(int length) {
            Ensure(length, false);

            // TODO: how do we parse 32-bit escape sequences?
            string str = new string(_buffer, _pointer, length);
            char c;
            try {
                c = (char)uint.Parse(str, NumberStyles.HexNumber);
            } catch (Exception e) {
                throw new ScannerException("while scanning a double-quoted scalar: expected escape sequence of " + length + " hexadecimal numbers, but found something else: " + str, e);
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
                throw new ScannerException("while scanning a quoted scalar: found unexpected end of stream");
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
                if (colz && IsEndOrStart()) {
                    throw new ScannerException("while scanning a quoted scalar: found unexpected document separator");
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
            if (!_possibleSimpleKeys.TryGetValue(_flowLevel, out key)) {
                if (_flowLevel == 0 && !_allowSimpleKey) {
                    throw new ScannerException("mapping values are not allowed here");
                }
                _allowSimpleKey = _flowLevel == 0;
                RemovePossibleSimpleKey();
            } else {
                _possibleSimpleKeys.Remove(_flowLevel);

                // find the insertion point
                LinkedListNode<Token> node = _tokens.First;
                int skip = key.TokenNumber - _tokensTaken;
                while (skip > 0) {
                    node = node.Next;
                    skip--;
                }

                node = _tokens.AddBefore(node, KeyToken.Instance);
                if (_flowLevel == 0 && AddIndent(key.Column)) {
                    _tokens.AddBefore(node, BlockMappingStartToken.Instance);
                }
                _allowSimpleKey = false;
            }
            Forward();
            return AddToken(ValueToken.Instance);
        }

        private Token FetchFlowSequenceStart() {
            _inFlowSequence = true;
            return FetchFlowCollectionStart(FlowSequenceStartToken.Instance);
        }

        private Token FetchFlowMappingStart() {
            return FetchFlowCollectionStart(FlowMappingStartToken.Instance);
        }

        private Token FetchFlowCollectionStart(Token tok) {
            _docStart = false;
            SavePossibleSimpleKey();
            _flowLevel++;
            _allowSimpleKey = true;
            Forward(1);
            return AddToken(tok);
        }


        private Token FetchDocumentEnd() {
            return FetchDocumentIndicator(DocumentEndToken.Instance);
        }

        private Token FetchFlowSequenceEnd() {
            _inFlowSequence = false;
            return FetchFlowCollectionEnd(FlowSequenceEndToken.Instance);
        }

        private Token FetchFlowMappingEnd() {
            return FetchFlowCollectionEnd(FlowMappingEndToken.Instance);
        }

        private Token FetchFlowCollectionEnd(Token tok) {
            RemovePossibleSimpleKey();
            _flowLevel--;
            _allowSimpleKey = false;
            Forward(1);
            return AddToken(tok);
        }

        private Token fetchFlowEntry() {
            _allowSimpleKey = true;
            RemovePossibleSimpleKey();
            Forward(1);
            return AddToken(FlowEntryToken.Instance);
        }

        private Token FetchLiteral() {
            return FetchBlockScalar('|');
        }

        private Token FetchFolded() {
            return FetchBlockScalar('>');
        }

        private Token FetchBlockScalar(char style) {
            _docStart = false;
            _allowSimpleKey = true;
            RemovePossibleSimpleKey();
            return AddToken(ScanBlockScalar(style));
        }

        private Token ScanBlockScalar(char style) {
            bool folded = style == '>';
            StringBuilder chunks = new StringBuilder();
            Forward();

            bool? chomping;
            int increment;
            ScanBlockScalarIndicators(out chomping, out increment);

            bool sameLine = ScanBlockScalarIgnoredLine();

            int minIndent = _indent + 1;
            if (minIndent < 0) {
                minIndent = 0;
            }

            int maxIndent = 0;
            int ind = 0;
            if (sameLine) {
                bool leadingNonSpace = !BLANK_T(Peek());
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

            return new ScalarToken(chunks.ToString(), false, style);
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

        private void ScanBlockScalarIndicators(out bool? chomping, out int increment) {
            chomping = null;
            increment = -1;

            char ch = Peek();
            if (ch == '-' || ch == '+') {
                chomping = ch == '+' ? true : false;
                Forward();
                ch = Peek();
                if (char.IsDigit(ch)) {
                    increment = ch - '0';
                    if (increment == 0) {
                        throw new ScannerException("while scanning a block scalar: expected indentation indicator in_ the range 1-9, but found 0");
                    }
                    Forward();
                }
            } else if (char.IsDigit(ch)) {
                increment = ch - '0';
                if (increment == 0) {
                    throw new ScannerException("while scanning a block scalar: expected indentation indicator in_ the range 1-9, but found 0");
                }
                Forward();
                ch = Peek();
                if (ch == '-' || ch == '+') {
                    chomping = ch == '+' ? true : false;
                    Forward();
                }
            }
            if (!NULL_BL_LINEBR(Peek())) {
                throw new ScannerException("while scanning a block scalar: expected chomping or indentation indicators, but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
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
            if (_flowLevel == 0) {
                if (!_allowSimpleKey) {
                    throw new ScannerException("mapping keys are not allowed here");
                }
                if (AddIndent(_column)) {
                    _tokens.AddLast(BlockMappingStartToken.Instance);
                }
            }
            _allowSimpleKey = _flowLevel == 0;
            RemovePossibleSimpleKey();
            Forward();
            return AddToken(KeyToken.Instance);
        }

        private Token FetchAlias() {
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(new AliasToken(ScanAnchor()));
        }

        private Token FetchAnchor() {
            SavePossibleSimpleKey();
            _allowSimpleKey = false;
            return AddToken(new AnchorToken(ScanAnchor()));
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
            while (ALPHA(ch)) {
                zlen = false;
                length++;
                ch = Peek(length);
            }
            if (zlen) {
                throw new ScannerException("while scanning a directive: expected alphabetic or numeric character, but found " + ch + "(" + ((int)ch) + ")");
            }
            string value = null;
            try {
                Ensure(length, false);
                value = new string(_buffer, _pointer, length);
            } catch (Exception) {
            }
            Forward(length);
            if (!NULL_BL_LINEBR(Peek())) {
                throw new ScannerException("while scanning a directive: expected alphabetic or numeric character, but found " + ch + "(" + ((int)ch) + ")");
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
                throw new ScannerException("while scanning a directive: expected a comment or a line break, but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            return ScanLineBreak();
        }

        private string ScanAnchor() {
            char indicator = Peek();
            string name = indicator == '*' ? "alias" : "anchor";
            Forward();
            int length = 0;
            while (ALPHA(Peek(length))) {
                length++;
            }
            if (length == 0) {
                throw new ScannerException("while scanning an " + name + ": expected alphabetic or numeric character, but found something else...");
            }
            string value = null;
            try {
                Ensure(length, false);
                value = new string(_buffer, _pointer, length);
            } catch (Exception) {
            }
            Forward(length);
            if (!NON_ALPHA_OR_NUM(Peek())) {
                throw new ScannerException("while scanning an " + name + ": expected alphabetic or numeric character, but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            return value;
        }

        private string[] ScanYamlDirectiveValue() {
            while (Peek() == ' ') {
                Forward();
            }
            string major = ScanYamlDirectiveNumber();
            if (Peek() != '.') {
                throw new ScannerException("while scanning a directive: expected a digit or '.', but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            Forward();
            string minor = ScanYamlDirectiveNumber();
            if (!NULL_BL_LINEBR(Peek())) {
                throw new ScannerException("while scanning a directive: expected a digit or ' ', but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            return new string[] { major, minor };
        }

        private string ScanYamlDirectiveNumber() {
            char ch = Peek();
            if (!char.IsDigit(ch)) {
                throw new ScannerException("while scanning a directive: expected a digit, but found " + ch + "(" + ((int)ch) + ")");
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
                throw new ScannerException("while scanning a directive: expected ' ', but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            return value;
        }

        private string ScanTagDirectivePrefix() {
            string value = ScanTagUri("directive");
            if (!NULL_BL_LINEBR(Peek())) {
                throw new ScannerException("while scanning a directive: expected ' ', but found " + Peek() + "(" + ((int)Peek()) + ")");
            }
            return value;
        }

        #region character classes

        private bool NULL_BL_T_LINEBR(char c) {
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

        private bool NULL_BL_LINEBR(char c) {
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

        private bool NON_ALPHA_OR_NUM(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '?':
                case ':':
                case ',':
                case ']':
                case '}':
                case '%':
                case '@':
                case '`':
                    return true;
            }
            return false;
        }

        private bool NOT_USEFUL_CHAR(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '-':
                case '?':
                case ':':
                case '[':
                case ']':
                case '{':
                case '#':
                case '&':
                case '*':
                case '!':
                case '|':
                case '\'':
                case '"':
                case '@':
                    return false;
            }
            return true;
        }

        private bool S4(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '[':
                case ']':
                case '{':
                case '}':
                    return true;
            }
            return false;
        }

        private bool ALL_FALSE(char c) {
            return false;
        }

        private bool R_FLOWZERO1(char c) {
            return c == ':';
        }

        private bool R_FLOWNONZERO(char c) {
            switch (c) {
                case '\0':
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case '[':
                case ']':
                case '{':
                case '}':
                case ',':
                case ':':
                case '?':
                    return true;
            }
            return false;
        }

        private bool ALPHA(char c) {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_';
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
