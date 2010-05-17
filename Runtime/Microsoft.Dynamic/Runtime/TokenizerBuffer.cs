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
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Runtime {
    public sealed class TokenizerBuffer {

        //private const char EndOfData = '\uFFFF';
        private const int FirstColumn = 1;
        public const int EndOfFile = -1;
        public const int InvalidCharacter = -2;

        // Whether to allow multiple forms of EOLN. 
        // If false only '\n' is treated as a line separator otherwise '\n', '\r\n' and '\r' are treated as separators.
        private bool _multiEolns;
        private TextReader _reader;

        private char[] _buffer;

        // Set to true when the buffer is resized. The contents of the buffer is shifted to the beginning at that point,
        // which discards the no longer used data in the buffer.
        private bool _bufferResized;

        // current position in the buffer, points to the next character read:
        private int _position;

        // location corresponding to the current token's first character:
        private SourceLocation _tokenStartLocation;

        // token end location; lazily calculated:
        private SourceLocation _tokenEndLocation;

        // index of the first/last+1 valid character in the buffer:
        private int _start;
        private int _end;

        // index of the last+1 character of the current token (token start is _start):
        private int _tokenEnd;

        public TextReader Reader {
            get {
                return _reader;
            }
        }
        
        public bool AtBeginning {
            get {
                return _position == 0 && !_bufferResized;
            }
        }

        public int TokenLength {
            get {
                Debug.Assert(_tokenEnd != -1, "Token end not marked");
                return _tokenEnd - _start;
            }
        }

        public int TokenRelativePosition {
            get {
                CheckInvariants();

                return _position - _start;
            }
        }

        public int Position {
            get {
                CheckInvariants();
                return _position;
            }
        }

        public SourceSpan TokenSpan {
            get {
                return new SourceSpan(TokenStart, TokenEnd);
            }
        }

        public SourceLocation TokenStart {
            get {
                return _tokenStartLocation;
            }
        }

        public SourceLocation TokenEnd {
            get {
                Debug.Assert(_tokenEnd != -1, "Token end not marked");
                return _tokenEndLocation;
            }
        }

        public TokenizerBuffer(TextReader reader, SourceLocation initialLocation, int initialCapacity, bool multiEolns) {
            Initialize(reader, initialLocation, initialCapacity, multiEolns);
        }

        public void Initialize(TextReader reader, SourceLocation initialLocation, int initialCapacity, bool multiEolns) {
            ContractUtils.RequiresNotNull(reader, "reader");
            ContractUtils.Requires(initialCapacity > 0, "initialCapacity");

            _reader = reader;

            if (_buffer == null || _buffer.Length < initialCapacity) {
                _buffer = new char[initialCapacity];
            }

            _tokenEnd = -1;
            _multiEolns = multiEolns;

            _tokenEndLocation = SourceLocation.Invalid;
            _tokenStartLocation = initialLocation;
            
            _start = _end = 0;
            _position = 0;

            CheckInvariants();
        }

        public int Read() {
            int result = Peek();
            _position++;
            return result;
        }

        public bool Read(int ch) {
            CheckInvariants();
            if (Peek() == ch) {
                _position++;
                CheckInvariants();
                return true;
            } else {
                return false;
            }
        }

        public bool Read(string str) {
            Debug.Assert(!String.IsNullOrEmpty(str));
            CheckInvariants();

            int old_pos = _position;
            
            // ensure sufficient data loaded:
            SeekRelative(str.Length - 1);
            if (Read() == EndOfFile) {
                Seek(old_pos);
                CheckInvariants();
                return false;
            }
            
            Debug.Assert(_position + str.Length <= _buffer.Length);

            int i = 0;
            while (i < str.Length && _buffer[i] == str[i]) i++;

            if (i != str.Length) {
                Seek(old_pos);
                CheckInvariants();
                return false;
            }

            CheckInvariants();
            return true;
        }

        public int Peek() {
            CheckInvariants();

            if (_position >= _end) {
                RefillBuffer();
                
                // eof:
                if (_position >= _end) {
                    CheckInvariants();
                    return EndOfFile;
                }
            }

            Debug.Assert(_position < _end);
            
            int result = _buffer[_position];
            CheckInvariants();
            return result;
        }

        private void RefillBuffer() {
            if (_end == _buffer.Length) {
                int new_size = System.Math.Max(System.Math.Max((_end - _start) * 2, _buffer.Length), _position);
                ResizeInternal(ref _buffer, new_size, _start, _end - _start);
                _end -= _start;
                _position -= _start;
                _start = 0;
                _bufferResized = true;
            }

            // make the buffer full:
            int count = _reader.Read(_buffer, _end, _buffer.Length - _end);
            _end += count;

            ClearInvalidChars();
        }

        public void Back() {
            SeekRelative(-1);
        }

        /// <summary>
        /// Sets the current position inside current token or one character behind it.
        /// </summary>
        public void Seek(int offset) {
            CheckInvariants();
            Debug.Assert(offset >= 0); 
            // no upper limit, we can seek beyond end in which case we are reading EOFs

            _position = _start + offset;

            CheckInvariants();
        }

        /// <summary>
        /// Sets the current position inside current token or one character behind it.
        /// A relative displacement with respect to the current position in the token is specified.
        /// </summary>
        public void SeekRelative(int disp) {
            CheckInvariants();
            Debug.Assert(disp >= _start - _position);
            // no upper limit, we can seek beyond end in which case we are reading EOFs

            _position += disp;

            CheckInvariants();
        }

        /// <summary>
        /// Marks token end. Enables to read the current token.
        /// </summary>
        public void MarkMultiLineTokenEnd() {
            CheckInvariants();

            _tokenEnd = System.Math.Min(_position, _end);
            _tokenEndLocation = (_multiEolns) ? GetTokenEndMultiEolns() : GetTokenEndSingleEoln();

            DumpToken();

            CheckInvariants();
        }

        public void MarkSingleLineTokenEnd() {
            CheckInvariants();

            _tokenEnd = System.Math.Min(_position, _end);
            int token_length = _tokenEnd - _start;
            
            _tokenEndLocation = new SourceLocation(
                _tokenStartLocation.Index + token_length,
                _tokenStartLocation.Line,
                _tokenStartLocation.Column + token_length
            );

            DumpToken();

            CheckInvariants();
        }

        public void MarkMultiLineTokenEnd(int disp) {
            SeekRelative(disp);
            MarkMultiLineTokenEnd();
        }

        public void MarkSingleLineTokenEnd(int disp) {
            SeekRelative(disp);
            MarkSingleLineTokenEnd();
        }

        public void MarkTokenEnd(bool isMultiLine) {
            if (isMultiLine)
                MarkMultiLineTokenEnd();
            else
                MarkSingleLineTokenEnd();
        }
        
        /// <summary>
        /// Marks token start. It means the buffer can drop the current token.
        /// Can be called even if no token has been read yet.
        /// </summary>
        public void DiscardToken() {
            CheckInvariants();

            // no token marked => mark it now:
            if (_tokenEnd == -1) MarkMultiLineTokenEnd();

            // the current token's end is the next token's start:
            _tokenStartLocation = _tokenEndLocation;
            _start = _tokenEnd;
            _tokenEnd = -1;
#if DEBUG
            _tokenEndLocation = SourceLocation.Invalid;
#endif
            CheckInvariants();
        }

        public char GetChar(int offset) {
            Debug.Assert(offset >= 0 && offset < _end);
            return _buffer[_start + offset];
        }

        public char GetCharRelative(int disp) {
            CheckInvariants();
            Debug.Assert(disp >= _start - _position);
            
            return _buffer[_position + disp];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetTokenString() {
            CheckInvariants();
            Debug.Assert(_tokenEnd != -1, "Token end not marked");

            return new String(_buffer, _start, _tokenEnd - _start);
        }

        public string GetTokenSubstring(int offset) {
            return GetTokenSubstring(offset, _tokenEnd - _start - offset);
        }
        
        public string GetTokenSubstring(int offset, int length) {
            CheckInvariants();
            Debug.Assert(_tokenEnd != -1, "Token end not marked");
            Debug.Assert(offset >= 0 && offset <= _tokenEnd - _start && length >= 0 && length <= _tokenEnd - _start - offset);

            return new String(_buffer, _start + offset, length);
        }

        private SourceLocation GetTokenEndSingleEoln() {
            int end_line = _tokenStartLocation.Line;
            int end_column = _tokenStartLocation.Column;

            for (int i = _start; i < _tokenEnd; i++) {
                if (_buffer[i] == '\n') {
                    end_column = FirstColumn;
                    end_line++;
                } else {
                    end_column++;
                }
            }
            return new SourceLocation(_tokenStartLocation.Index + _tokenEnd - _start, end_line, end_column);
        }

        private SourceLocation GetTokenEndMultiEolns() {
            int end_line = _tokenStartLocation.Line;
            int end_column = _tokenStartLocation.Column;
            
            int i = _start;
            while (i < _tokenEnd - 1) {
                if (_buffer[i] == '\n') {
                    end_column = FirstColumn;
                    end_line++;
                } else if (_buffer[i] == '\r') {
                    end_column = FirstColumn;
                    end_line++;

                    Debug.Assert(i + 1 < _buffer.Length);
                    if (_buffer[i + 1] == '\n') i++;
                } else
                    end_column++;

                i++;
            }

            if (i < _tokenEnd) {
                if (_buffer[i] == '\n') {
                    end_column = FirstColumn;
                    end_line++;
                } else if (_buffer[i] == '\r') {
                    end_column = FirstColumn;
                    end_line++;
                } else {
                    end_column++;
                }
            }

            return new SourceLocation(_tokenStartLocation.Index + _tokenEnd - _start, end_line, end_column);
        }

        public bool IsEoln(int current) {
            if (current == '\n') return true;

            if (current == '\r' && _multiEolns) {

                if (Peek() == '\n') {
                    return true;
                }

                return true;
            }

            return false;
        }

        public int ReadEolnOpt(int current) {
            if (current == '\n') return 1;

            if (current == '\r' && _multiEolns) {

                if (Peek() == '\n') {
                    SeekRelative(+1);
                    return 2;
                }

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Reads till the end of line and returns the character that stopped the reading.
        /// The returned character is not skipped.
        /// </summary>
        public int ReadLine() {
            int ch;
            do { ch = Read(); } while (ch != EndOfFile && !IsEoln(ch));
            Back();
            return ch;
        }

        /// <summary>
        /// Resizes an array to a speficied new size and copies a portion of the original array into its beginning.
        /// </summary>
        private static void ResizeInternal(ref char[] array, int newSize, int start, int count) {
            Debug.Assert(array != null && newSize > 0 && count >= 0 && newSize >= count && start >= 0);

            char[] result = (newSize != array.Length) ? new char[newSize] : array;

            Buffer.BlockCopy(array, start * sizeof(char), result, 0, count * sizeof(char));

            array = result;
        }

        [Conditional("DEBUG")]
        private void ClearInvalidChars() {
            for (int i = 0; i < _start; i++) _buffer[i] = '\0';
            for (int i = _end; i < _buffer.Length; i++) _buffer[i] = '\0';
        }

        [Conditional("DEBUG")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void CheckInvariants() {
            Debug.Assert(_buffer.Length >= 1);

            // _start == _end when discarding token and at beginning, when == 0
            Debug.Assert(_start >= 0 && _start <= _end); 

            Debug.Assert(_end >= 0 && _end <= _buffer.Length);
            
            // position beyond _end means we are reading EOFs:
            Debug.Assert(_position >= _start);
            Debug.Assert(_tokenEnd >= -1 && _tokenEnd <= _end);
        }

        [Conditional("DUMP_TOKENS")]
        private void DumpToken() {
            Console.WriteLine("--> `{0}` {1}", GetTokenString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t"), TokenSpan);
        }
    }    
}
