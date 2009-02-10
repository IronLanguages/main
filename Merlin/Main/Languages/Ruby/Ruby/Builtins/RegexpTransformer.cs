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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Compiler;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    /// <summary>
    /// Converts a Ruby regexp pattern to a CLR pattern
    /// </summary>
    internal class RegexpTransformer {
        [Flags]
        enum PatternState {
            Normal,
            InEscapeSequence,
            InCharacterClass
        }

        string/*!*/ _rubyPattern;
        PatternState _state = PatternState.Normal;
        int _index;
        StringBuilder/*!*/ _sb;

        internal static string Transform(string/*!*/ rubyPattern, RubyRegexOptions options) {
            RegexpTransformer transformer = new RegexpTransformer(rubyPattern);
            return transformer.Transform();
        }

        private RegexpTransformer(string/*!*/ rubyPattern) {
            _rubyPattern = rubyPattern;
            _sb = new StringBuilder(rubyPattern.Length);
        }

        private bool InEscapeSequence { get { return (_state & PatternState.InEscapeSequence) != 0; } }
        private bool InCharacterClass { get { return (_state & PatternState.InCharacterClass) != 0; } }

        private bool HasMoreCharacters { get { return (_index + 1) < _rubyPattern.Length; } }

        private char CurrentCharacter {
            get {
                Debug.Assert(_index < _rubyPattern.Length);
                return _rubyPattern[_index];
            }
        }

        private char NextCharacter {
            get {
                Debug.Assert(HasMoreCharacters);
                return _rubyPattern[_index + 1];
            }
        }

        private void LeaveEscapeSequence() {
            _state &= ~PatternState.InEscapeSequence;
        }

        private void AppendEscapedChar(char c) {
            Debug.Assert(InEscapeSequence);
            Debug.Assert(_rubyPattern[_index - 1] == '\\');
            _sb.Append('\\');
            _sb.Append(c);
            LeaveEscapeSequence();
        }

        private void OnBackSlash() {
            if (InEscapeSequence) {
                AppendEscapedChar('\\');
            } else {
                _state |= PatternState.InEscapeSequence;
            }
        }

        private static readonly string[][] _PredefinedCharacterClasses = new string[][] {
                new string[] { "[:alnum:]",  @"A-Za-z0-9_" },
                new string[] { "[:alpha:]",  @"A-Za-z_" },
                new string[] { "[:ascii:]",  @"\00-\FF" }, // Oni
                new string[] { "[:blank:]",  @" \t" },
                new string[] { "[:cntrl:]",  @"\c0" }, // TODO
                new string[] { "[:digit:]",  @"0-9" },
                new string[] { "[:graph:]",  @"\g" }, // TODO
                new string[] { "[:lower:]",  @"a-z" },
                new string[] { "[:print:]",  @" " }, // TODO
                new string[] { "[:punct:]",  @",.?" }, // TODO
                new string[] { "[:space:]",  @" \t\f\n\r\v" },
                new string[] { "[:upper:]",  @"A-Z" },
                new string[] { "[:xdigit:]", @"0-9A-Fa-f" },
            };

        private bool CheckReplacePredefinedCharacterClass() {
            string remainingString = _rubyPattern.Substring(_index);
            foreach (string[] predefinedCharacterClass in _PredefinedCharacterClasses) {
                if (remainingString.StartsWith(predefinedCharacterClass[0])) {
                    _sb.Append(predefinedCharacterClass[1]);
                    _index += predefinedCharacterClass[0].Length - 1;
                    return true;
                }
            }

            return false;
        }

        private void OnOpenBracket() {
            if (InEscapeSequence) {
                AppendEscapedChar('[');
            } else if (InCharacterClass) {
                if (!CheckReplacePredefinedCharacterClass()) {
                    // TODO - Ruby 1.9 allows "/[a[b]]/". Not sure what this means. So we really will need to keep nesting count.
                    // We need the nesting count anyway for "/[a&&[b]]/"
                    _sb.Append('[');
                }
            } else {
                _sb.Append('[');
                _state |= PatternState.InCharacterClass;
            }
        }

        private void OnCloseBracket() {
            if (InEscapeSequence) {
                AppendEscapedChar(']');
            } else {
                _sb.Append(']');
                _state &= ~PatternState.InCharacterClass;
            }
        }

        private string/*!*/ Transform() {
            for (/**/; _index < _rubyPattern.Length; _index++) {
                char c = _rubyPattern[_index];
                switch (c) {
                    case '\\': OnBackSlash(); break;
                    case '[': OnOpenBracket(); break;
                    case ']': OnCloseBracket(); break;
                    default: OnChar(c); break;
                }
            }

            if (InEscapeSequence) {
                // error case
                _sb.Append('\\');
                LeaveEscapeSequence();
            }

            return _sb.ToString();
        }

        private static bool IsMetaCharacterWithDirectMapping(char c) {
            switch (c) {
                // metacharacters:
                case '$':
                case '^':
                case '|':
                case '[':
                case ']':
                case '(':
                case ')':
                case '\\':
                case '.':
                case '#':
                case '-':

                case '{':
                case '}':
                case '*':
                case '+':
                case '?':

                case '0': // octal
                case 't':
                case 'v':
                case 'n':
                case 'r':
                case 'f':
                case 'a':
                case 'e': // characters

                case 'b': // word boundary or backslash in character group
                case 'B': // not word boundary
                case 'A': // beginning of string
                case 'Z': // end of string, or before newline at the end
                case 'z': // end of string
                case 'G':

                case 'd':
                case 'D':
                case 's':
                case 'S':
                case 'w': // word character 
                case 'W': // non word char
                // TODO: may need non-Unicode adjustment

                case 'M': // meta characters
                // TODO: replace

                // Oniguruma + .NET - character classes, they don't match so some fixups would be needed
                // MRI: doesn't support, but is also an error since it is followed by {name}, which is illegal
                case 'p':
                case 'P':
                    return true;
            }

            return false;
        }

        // fixes escapes
        // - unescapes non-special characters
        private void OnChar(char c) {
            if (!InEscapeSequence) {
                _sb.Append(c);
                return;
            }

            switch (c) {
                case 'c':
                    AppendEscapedChar('c');
                    // \c# -> \cC
                    if (HasMoreCharacters && NextCharacter == '#') {
                        _index++;
                        _sb.Append('C');
                    }
                    break;

                case 'x':
                    AppendEscapedChar('x');

                    // error:
                    if (!HasMoreCharacters) {
                        break;
                    }

                    // Change single digit hex notation to double-digit (with a leading 0)
                    //   \xFxyz  -> \x0Fxyz
                    //   \xF     -> \x0F
                    char firstDigit = _rubyPattern[++_index];
                    char secondDigit = HasMoreCharacters ? _rubyPattern[_index + 1] : '\0';
                    if (!Tokenizer.IsHexadecimalDigit(secondDigit)) {
                        _sb.Append('0');
                    }
                    _sb.Append(firstDigit);
                    break;

                case 'h': // Oniguruma only: [0-9A-Fa-f]
                case 'H': // Oniguruma only: [^0-9A-Fa-f]
                case 'g': // Oniguruma only
                case 'k': // Oniguruma, .NET: named backreference, MRI not supported
                    // remove backslash
                    _sb.Append(c);
                    LeaveEscapeSequence();
                    break;

                default:
                    if (IsMetaCharacterWithDirectMapping(c)) {
                        AppendEscapedChar(c);
                        return;
                    }

                    if (Tokenizer.IsDecimalDigit(c)) {
                        // TODO:
                        // \([1-9][0-9]*) where there is no group of such number (replace by an empty string)
                        AppendEscapedChar(c);
                    } else {
                        // Ruby allows any character to be escaped whereas .NET throws invalid escape exception. So remove the backslash
                        _sb.Append(c);
                        LeaveEscapeSequence();
                    }
                    break;
            }
        }
    }
}
