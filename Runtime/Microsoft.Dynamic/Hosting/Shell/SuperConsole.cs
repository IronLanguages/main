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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using System.Text;

namespace Microsoft.Scripting.Hosting.Shell {
#if !SILVERLIGHT // SuperConsole (ConsoleColor)
    public sealed class SuperConsole : BasicConsole {

        #region Nested types: History, SuperConsoleOptions, Cursor

        /// <summary>
        /// Class managing the command history.
        /// </summary>
        class History {
            protected List<string> _list = new List<string>();
            private int _current;
            private bool _increment;         // increment on Next()

            public string Current {
                get {
                    return _current >= 0 && _current < _list.Count ? _list[_current] : String.Empty;
                }
            }

            public void Add(string line, bool setCurrentAsLast) {
                if (line != null && line.Length > 0) {
                    int oldCount = _list.Count;
                    _list.Add(line);
                    if (setCurrentAsLast || _current == oldCount) {
                        _current = _list.Count;
                    } else {
                        _current++;
                    }
                    // Do not increment on the immediately following Next()
                    _increment = false;
                }
            }

            public string Previous() {
                if (_current > 0) {
                    _current--;
                    _increment = true;
                }
                return Current;
            }

            public string Next() {
                if (_current + 1 < _list.Count) {
                    if (_increment) _current++;
                    _increment = true;
                }
                return Current;
            }
        }

        /// <summary>
        /// List of available options
        /// </summary>
        class SuperConsoleOptions {
            private List<string> _list = new List<string>();
            private int _current;
            private string _root;

            public int Count {
                get {
                    return _list.Count;
                }
            }

            private string Current {
                get {
                    return _current >= 0 && _current < _list.Count ? _list[_current] : String.Empty;
                }
            }

            public void Clear() {
                _list.Clear();
                _current = -1;
            }

            public void Add(string line) {
                if (line != null && line.Length > 0) {
                    _list.Add(line);
                }
            }

            public string Previous() {
                if (_list.Count > 0) {
                    _current = ((_current - 1) + _list.Count) % _list.Count;
                }
                return Current;
            }

            public string Next() {
                if (_list.Count > 0) {
                    _current = (_current + 1) % _list.Count;
                }
                return Current;
            }

            public string Root {
                get {
                    return _root;
                }
                set {
                    _root = value;
                }
            }
        }

        /// <summary>
        /// Cursor position management
        /// </summary>
        struct Cursor {
            /// <summary>
            /// Beginning position of the cursor - top coordinate.
            /// </summary>
            private int _anchorTop;
            /// <summary>
            /// Beginning position of the cursor - left coordinate.
            /// </summary>
            private int _anchorLeft;

            public void Anchor() {
                _anchorTop = Console.CursorTop;
                _anchorLeft = Console.CursorLeft;
            }

            public void Reset() {
                Console.CursorTop = _anchorTop;
                Console.CursorLeft = _anchorLeft;
            }

            public void Place(int index) {
                Console.CursorLeft = (_anchorLeft + index) % Console.BufferWidth;
                int cursorTop = _anchorTop + (_anchorLeft + index) / Console.BufferWidth;
                if (cursorTop >= Console.BufferHeight) {
                    _anchorTop -= cursorTop - Console.BufferHeight + 1;
                    cursorTop = Console.BufferHeight - 1;
                }
                Console.CursorTop = cursorTop;
            }

            public static void Move(int delta) {
                int position = Console.CursorTop * Console.BufferWidth + Console.CursorLeft + delta;

                Console.CursorLeft = position % Console.BufferWidth;
                Console.CursorTop = position / Console.BufferWidth;
            }
        }

        #endregion

        /// <summary>
        /// The console input buffer.
        /// </summary>
        private StringBuilder _input = new StringBuilder();

        /// <summary>
        /// Current position - index into the input buffer
        /// </summary>
        private int _current;

        /// <summary>
        /// The number of white-spaces displayed for the auto-indenation of the current line
        /// </summary>
        private int _autoIndentSize;

        /// <summary>
        /// Length of the output currently rendered on screen.
        /// </summary>
        private int _rendered;

        /// <summary>
        /// Command history
        /// </summary>
        private History _history = new History();

        /// <summary>
        /// Tab options available in current context
        /// </summary>
        private SuperConsoleOptions _options = new SuperConsoleOptions();

        /// <summary>
        /// Cursort anchor - position of cursor when the routine was called
        /// </summary>
        private Cursor _cursor;

        /// <summary>
        /// The command line that this console is attached to.
        /// </summary>
        private CommandLine _commandLine;

        public SuperConsole(CommandLine commandLine, bool colorful)
            : base(colorful) {
            ContractUtils.RequiresNotNull(commandLine, "commandLine");
            _commandLine = commandLine;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool GetOptions() {
            _options.Clear();

            int len;
            for (len = _input.Length; len > 0; len--) {
                char c = _input[len - 1];
                if (Char.IsLetterOrDigit(c)) {
                    continue;
                } else if (c == '.' || c == '_') {
                    continue;
                } else {
                    break;
                }
            }

            string name = _input.ToString(len, _input.Length - len);
            if (name.Trim().Length > 0) {
                int lastDot = name.LastIndexOf('.');
                string attr, pref, root;
                if (lastDot < 0) {
                    attr = String.Empty;
                    pref = name;
                    root = _input.ToString(0, len);
                } else {
                    attr = name.Substring(0, lastDot);
                    pref = name.Substring(lastDot + 1);
                    root = _input.ToString(0, len + lastDot + 1);
                }

                try {
                    IList<string> result;
                    if (String.IsNullOrEmpty(attr)) {
                        result = _commandLine.GetGlobals(name);
                    } else {
                        result = _commandLine.GetMemberNames(attr);
                    }

                    _options.Root = root;
                    foreach (string option in result) {
                        if (option.StartsWith(pref, StringComparison.CurrentCultureIgnoreCase)) {
                            _options.Add(option);
                        }
                    }
                } catch {
                    _options.Clear();
                }
                return true;
            } else {
                return false;
            }
        }

        private void SetInput(string line) {
            _input.Length = 0;
            _input.Append(line);

            _current = _input.Length;

            Render();
        }

        private void Initialize() {
            _cursor.Anchor();
            _input.Length = 0;
            _current = 0;
            _rendered = 0;
        }

        // Check if the user is backspacing the auto-indentation. In that case, we go back all the way to
        // the previous indentation level.
        // Return true if we did backspace the auto-indenation.
        private bool BackspaceAutoIndentation() {
            if (_input.Length == 0 || _input.Length > _autoIndentSize) return false;

            // Is the auto-indenation all white space, or has the user since edited the auto-indentation?
            for (int i = 0; i < _input.Length; i++) {
                if (_input[i] != ' ') return false;
            }

            // Calculate the previous indentation level
            //!!! int newLength = ((input.Length - 1) / ConsoleOptions.AutoIndentSize) * ConsoleOptions.AutoIndentSize;            
            int newLength = _input.Length - 4;

            int backspaceSize = _input.Length - newLength;
            _input.Remove(newLength, backspaceSize);
            _current -= backspaceSize;
            Render();
            return true;
        }

        private void OnBackspace() {
            if (BackspaceAutoIndentation()) return;

            if (_input.Length > 0 && _current > 0) {
                _input.Remove(_current - 1, 1);
                _current--;
                Render();
            }
        }

        private void OnDelete() {
            if (_input.Length > 0 && _current < _input.Length) {
                _input.Remove(_current, 1);
                Render();
            }
        }

        private void Insert(ConsoleKeyInfo key) {
            char c;
            if (key.Key == ConsoleKey.F6) {
                Debug.Assert(FinalLineText.Length == 1);

                c = FinalLineText[0];
            } else {
                c = key.KeyChar;
            }
            Insert(c);
        }

        private void Insert(char c) {
            if (_current == _input.Length) {
                if (Char.IsControl(c)) {
                    string s = MapCharacter(c);
                    _current++;
                    _input.Append(c);
                    Output.Write(s);
                    _rendered += s.Length;
                } else {
                    _current++;
                    _input.Append(c);
                    Output.Write(c);
                    _rendered++;
                }
            } else {
                _input.Insert(_current, c);
                _current++;
                Render();
            }
        }

        private static string MapCharacter(char c) {
            if (c == 13) return "\r\n";
            if (c <= 26) return "^" + ((char)(c + 'A' - 1)).ToString();

            return "^?";
        }

        private static int GetCharacterSize(char c) {
            if (Char.IsControl(c)) {
                return MapCharacter(c).Length;
            } else {
                return 1;
            }
        }

        private void Render() {
            _cursor.Reset();
            StringBuilder output = new StringBuilder();
            int position = -1;
            for (int i = 0; i < _input.Length; i++) {
                if (i == _current) {
                    position = output.Length;
                }
                char c = _input[i];
                if (Char.IsControl(c)) {
                    output.Append(MapCharacter(c));
                } else {
                    output.Append(c);
                }
            }

            if (_current == _input.Length) {
                position = output.Length;
            }

            string text = output.ToString();
            Output.Write(text);

            if (text.Length < _rendered) {
                Output.Write(new String(' ', _rendered - text.Length));
            }
            _rendered = text.Length;
            _cursor.Place(position);
        }

        private void MoveLeft(ConsoleModifiers keyModifiers) {
            if ((keyModifiers & ConsoleModifiers.Control) != 0) {
                // move back to the start of the previous word
                if (_input.Length > 0 && _current != 0) {
                    bool nonLetter = IsSeperator(_input[_current - 1]);
                    while (_current > 0 && (_current - 1 < _input.Length)) {
                        MoveLeft();

                        if (IsSeperator(_input[_current]) != nonLetter) {
                            if (!nonLetter) {
                                MoveRight();
                                break;
                            }

                            nonLetter = false;
                        }
                    }
                }
            } else {
                MoveLeft();
            }
        }

        private static bool IsSeperator(char ch) {
            return !Char.IsLetter(ch);
        }

        private void MoveRight(ConsoleModifiers keyModifiers) {
            if ((keyModifiers & ConsoleModifiers.Control) != 0) {
                // move to the next word
                if (_input.Length != 0 && _current < _input.Length) {
                    bool nonLetter = IsSeperator(_input[_current]);
                    while (_current < _input.Length) {
                        MoveRight();

                        if (_current == _input.Length) break;
                        if (IsSeperator(_input[_current]) != nonLetter) {
                            if (nonLetter)
                                break;

                            nonLetter = true;
                        }
                    }
                }
            } else {
                MoveRight();
            }
        }

        private void MoveRight() {
            if (_current < _input.Length) {
                char c = _input[_current];
                _current++;
                Cursor.Move(GetCharacterSize(c));
            }
        }

        private void MoveLeft() {
            if (_current > 0 && (_current - 1 < _input.Length)) {
                _current--;
                char c = _input[_current];
                Cursor.Move(-GetCharacterSize(c));
            }
        }

        private const int TabSize = 4;
        private void InsertTab() {
            for (int i = TabSize - (_current % TabSize); i > 0; i--) {
                Insert(' ');
            }
        }

        private void MoveHome() {
            _current = 0;
            _cursor.Reset();
        }

        private void MoveEnd() {
            _current = _input.Length;
            _cursor.Place(_rendered);
        }

        public override string ReadLine(int autoIndentSize) {
            Initialize();

            _autoIndentSize = autoIndentSize;
            for (int i = 0; i < _autoIndentSize; i++)
                Insert(' ');

            bool inputChanged = false;
            bool optionsObsolete = false;

            for (; ; ) {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key) {
                    case ConsoleKey.Backspace:
                        OnBackspace();
                        inputChanged = optionsObsolete = true;
                        break;
                    case ConsoleKey.Delete:
                        OnDelete();
                        inputChanged = optionsObsolete = true;
                        break;
                    case ConsoleKey.Enter:
                        return OnEnter(inputChanged);
                    case ConsoleKey.Tab: {
                            bool prefix = false;
                            if (optionsObsolete) {
                                prefix = GetOptions();
                                optionsObsolete = false;
                            }

                            // Displays the next option in the option list,
                            // or beeps if no options available for current input prefix.
                            // If no input prefix, simply print tab.
                            DisplayNextOption(key, prefix);
                            inputChanged = true;
                            break;
                        }
                    case ConsoleKey.UpArrow:
                        SetInput(_history.Previous());
                        optionsObsolete = true;
                        inputChanged = false;
                        break;
                    case ConsoleKey.DownArrow:
                        SetInput(_history.Next());
                        optionsObsolete = true;
                        inputChanged = false;
                        break;
                    case ConsoleKey.RightArrow:
                        MoveRight(key.Modifiers);
                        optionsObsolete = true;
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveLeft(key.Modifiers);
                        optionsObsolete = true;
                        break;
                    case ConsoleKey.Escape:
                        SetInput(String.Empty);
                        inputChanged = optionsObsolete = true;
                        break;
                    case ConsoleKey.Home:
                        MoveHome();
                        optionsObsolete = true;
                        break;
                    case ConsoleKey.End:
                        MoveEnd();
                        optionsObsolete = true;
                        break;
                    case ConsoleKey.LeftWindows:
                    case ConsoleKey.RightWindows:
                        // ignore these
                        continue;

                    default:
                        if (key.KeyChar == '\x0D') goto case ConsoleKey.Enter;      // Ctrl-M
                        if (key.KeyChar == '\x08') goto case ConsoleKey.Backspace;  // Ctrl-H
                        Insert(key);
                        inputChanged = optionsObsolete = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Displays the next option in the option list,
        /// or beeps if no options available for current input prefix.
        /// If no input prefix, simply print tab.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="prefix"></param>
        private void DisplayNextOption(ConsoleKeyInfo key, bool prefix) {
            if (_options.Count > 0) {
                string part = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? _options.Previous() : _options.Next();
                SetInput(_options.Root + part);
            } else {
                if (prefix) {
                    Console.Beep();
                } else {
                    InsertTab();
                }
            }
        }

        /// <summary>
        /// Handle the enter key. Adds the current input (if not empty) to the history.
        /// </summary>
        /// <param name="inputChanged"></param>
        /// <returns>The input string.</returns>
        private string OnEnter(bool inputChanged) {
            Output.Write("\n");
            string line = _input.ToString();
            if (line == FinalLineText) return null;
            if (line.Length > 0) {
                _history.Add(line, inputChanged);
            }
            return line;
        }

        string FinalLineText {
            get {
                return Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04";
            }
        }
    }
#else
    public sealed class SuperConsole : BasicConsole {
        public SuperConsole(CommandLine  commandLine, bool isColorful)
            : base(isColorful) {
        }
    }
#endif
}
