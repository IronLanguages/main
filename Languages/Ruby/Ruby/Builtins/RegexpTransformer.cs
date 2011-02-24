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
using System.Diagnostics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Compiler;
using IronRuby.Runtime;
using System.Collections.Generic;

namespace IronRuby.Builtins {
    /// <summary>
    /// Converts a Ruby regexp pattern to a CLR pattern
    /// </summary>
    internal sealed class RegexpTransformer {
        private readonly string/*!*/ _rubyPattern;
        private int _index;
        private StringBuilder/*!*/ _sb;
        private bool _hasGAnchor;

        internal static string Transform(string/*!*/ rubyPattern, RubyRegexOptions options, out bool hasGAnchor) {
            // TODO: surrogates (REXML uses this pattern)
            if (rubyPattern == "^[\t\n\r -\uD7FF\uE000-\uFFFD\uD800\uDC00-\uDBFF\uDFFF]*$") {
                hasGAnchor = false;
                return "^(?:[\t\n\r -\uD7FF\uE000-\uFFFD]|[\uD800-\uDBFF][\uDC00-\uDFFF])*$";
            }
            
            RegexpTransformer transformer = new RegexpTransformer(rubyPattern);
            var result = transformer.Transform();
            hasGAnchor = transformer._hasGAnchor;
            return result;
        }

        private RegexpTransformer(string/*!*/ rubyPattern) {
            _rubyPattern = rubyPattern;
        }

        #region Buffer Ops

        private int Peek() {
            return _index < _rubyPattern.Length ? _rubyPattern[_index] : -1;
        }

        private int Peek(int disp) {
            int i = _index + disp;
            return i < _rubyPattern.Length ? _rubyPattern[i] : -1;
        }

        private int Read() {
            return _index < _rubyPattern.Length ? _rubyPattern[_index++] : -1;
        }

        private void Back() {
            _index--;
            Debug.Assert(_index >= 0);
        }
        
        private void Skip(int n) {
            _index += n;
            Debug.Assert(_index <= _rubyPattern.Length);
        }

        private void Skip() {
            Skip(1);
        }

        private void Skip(char c) {
            Debug.Assert(Peek() == c);
            Skip();
        }

        private bool Read(int c) {
            if (Peek() == c) {
                Skip();
                return true;
            } else {
                return false;
            }
        }

        private void Append(char c) {
            _sb.Append(c);
        }

        private void AppendEscaped(int c) {
            AppendEscaped(_sb, c);
        }

        private static StringBuilder/*!*/ AppendEscaped(StringBuilder/*!*/ builder, string/*!*/ str) {
            for (int i = 0; i < str.Length; i++) {
                AppendEscaped(builder, str[i]);
            }
            return builder;
        }

        private static StringBuilder/*!*/ AppendEscaped(StringBuilder/*!*/ builder, int c) {
            if (IsMetaCharacter(c)) {
                builder.Append('\\');
            }
            builder.Append((char)c);
            return builder;
        }

        private static string/*!*/ Escape(int c) {
            return IsMetaCharacter(c) ? "\\" + (char)c : ((char)c).ToString();
        }

        private static bool IsMetaCharacter(int c) {
            switch (c) {
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
                case '\n':
                case '\r':
                case '\t':
                case ' ':
                    return true;
            }

            return false;
        }

        #endregion

        private Exception/*!*/ MakeError(string/*!*/ message) {
            return new RegexpError(message + ": " + _rubyPattern);
        }

        private string/*!*/ Transform() {
            _sb = new StringBuilder(_rubyPattern.Length);
            Parse(false);
            var result = _sb.ToString();
            _sb = null;
            return result;
        }

        private void Parse(bool isSubexpression) {
            int lastEntityIndex = 0;
            int c;
            while (true) {
                switch (c = Read()) {
                    case -1:
                        if (isSubexpression) {
                            throw MakeError("end pattern in group");
                        }
                        return;
                    
                    case '\\':
                        lastEntityIndex = _sb.Length;
                        ParseEscape();
                        break;

                    case '?':
                    case '*':
                    case '+':
                        Append((char)c);
                        ParsePostQuantifier(lastEntityIndex, true);
                        break;

                    case '{':
                        if (ParseConstrainedQuantifier()) {
                            ParsePostQuantifier(lastEntityIndex, false);
                        } else {
                            goto default;
                        }
                        break;

                    case '(':
                        lastEntityIndex = _sb.Length;
                        ParseGroup();
                        break;

                    case ')':
                        if (isSubexpression) {
                            return;
                        } else {
                            throw MakeError("unmatched close parenthesis");
                        }
                    
                    case '[':
                        lastEntityIndex = _sb.Length;
                        ParseCharacterGroup(false).AppendTo(_sb, true);
                        break;

                    case '|':
                        Append('|');
                        lastEntityIndex = _sb.Length;
                        break;

                    default:
                        lastEntityIndex = _sb.Length;
                        Append((char)c);
                        break;
                }
            }
        }

        // {n,m}
        // {n,}
        // {,m}
        // {n}
        private bool ParseConstrainedQuantifier() {
            Debug.Assert(_rubyPattern[_index - 1] == '{');

            int c;
            int m = -1;

            int i = 0;
            while (true) {
                c = Peek(i++);
                if (c == ',') {
                    if (m != -1) {
                        // not a quantifier
                        return false;
                    }
                    m = i;                    
                } else if (c == '}') {
                    break;
                } else if (!Tokenizer.IsDecimalDigit(c)) {
                    return false;
                }
            }

            _sb.Append(_rubyPattern, _index - 1, i + 1);
            _index += i;
            return true;
        }

        private void ParsePostQuantifier(int lastEntityIndex, bool possessive) {
            int c = Peek();

            if (c == '+') {
                // nested possessive quantifiers not directly supported by Regex:
                Skip();
                _sb.Insert(lastEntityIndex, possessive ? "(?>" : "(?:");
                Append(')');
                if (!possessive) {
                    Append('+');
                }
            } else if (c == '?') {
                Skip();
                Append('?');
            }
        }

        //
        // (?#...)            comment
        // (?imx-imx)         option on/off
        // (?imx-imx:subexp)  option on/off for subexp
        // (?:subexp)         not captured group
        // (?=subexp)         look-ahead
        // (?!subexp)         negative look-ahead
        // (?<=subexp)        look-behind
        // (?<!subexp)        negative look-behind
        // (?>subexp)         atomic group
        //                    don't backtrack in subexp.
        // 
        // (?<name>subexp)
        // (?'name'subexp)
        //
        // (subexp)           captured group
        //
        private void ParseGroup() {
            Debug.Assert(_rubyPattern[_index - 1] == '(');
            if (Read('?')) {
                int c = Read();
                if (c == '#') {
                    while (true) {
                        c = Read();
                        if (c == -1) {
                            throw MakeError("end pattern in group");
                        }
                        if (c == ')') {
                            break;
                        }
                    }
                    return;
                }

                Append('(');
                Append('?');

                switch (c) {
                    case '-':
                    case 'i':
                    case 'm':
                    case 'x':
                        while (true) {
                            if (c == 'm') {
                                // Map (?m) to (?s) ie. RegexOptions.SingleLine
                                Append('s');
                            } else if (c == 'i' || c == 'x' || c == '-') {
                                Append((char)c);
                            } else if (c == ':') {
                                Append(':');
                                break;
                            } else if (c == ')' || c == -1) {
                                Back();
                                break;
                            } else {
                                throw MakeError("undefined group option");
                            }
                            c = Read();
                        }
                        break;

                    case ':':
                        // non-captured group
                        Append(':');
                        break;

                    case '=':
                        // positive lookahead
                        Append('=');
                        break;

                    case '!':
                        // negative lookahead
                        Append('!');
                        break;

                    case '>':
                        // greedy subexpression
                        Append('>');
                        break;

                    case '<':
                        Append('<');
                        c = Read();
                        if (c == '=' || c == '!') {
                            // positive/negative lookbehind assertion
                            Append((char)c);
                        } else {
                            ParseGroupName(c, '>');
                        }
                        break;

                    case '\'':
                        Append('\'');
                        ParseGroupName(Read(), '\'');
                        break;

                    default:
                        throw MakeError("undefined group option");
                }
            } else {
                Append('(');
            }
            Parse(true);
            Append(')');
        }

        private void ParseGroupName(int c, int terminator) {
            if (c == terminator || c == -1) {
                throw MakeError("group name is empty");
            }
            while (true) {
                Append((char)c); 
                c = Read();
                if (c == terminator || c == ')') {
                    Append((char)c);
                    break;
                } else if (c == -1) {
                    throw MakeError("unterminated group name");
                }
            }
        }

        #region Escapes

        private void ParseEscape() {
            int c = Read();
            if (c == -1) {
                throw MakeError("too short escape sequence");
            }
            ParseEscape(c);
        }
        
        // escape outside of character group
        private void ParseEscape(int escape) {
            switch (escape) {
                case 'A':   // beginning a string
                case 'b':   // word boundary
                case 'B':   // not a word boundary
                case 'Z':   // end of string or a new line
                case 'z':   // end of string
                    Append('\\');
                    Append((char)escape);
                    break;

                // \g<n>
                // \g'n'
                // \g<-n>         
                // \g'-n'
                // \g<name>
                // \g'name'
                case 'g':
                    // TODO: this can be implemented by copying the pattern with removed groups
                    throw MakeError("\\g not supported");
                
                case 'k':
                    ParseBackreference();
                    break;

                case 'G':   // start position
                    _hasGAnchor = true;
                    Append('\\');
                    Append((char)escape);
                    break;

                case 'u':
                    if (Peek() == '{') {
                        // \u{1234 12345 123}
                        foreach (var codepoint in ParseUnicodeEscapeList()) {
                            AppendEscaped(codepoint);
                        }
                    } else {
                        // \u1234
                        AppendEscaped(ParseUnicodeEscape());
                    }
                    break;
                    
                default:
                    // \digit is a backreference to an indexed group or an octal escape
                    // In any case .NET Regex will decide for us, we don't need to distinguish between these cases.
                    if (Tokenizer.IsDecimalDigit(escape)) {
                        Append('\\');
                        Append((char)escape);
                        break;
                    }

                    ParseCharacterEscape(escape).AppendTo(_sb, false);
                    break;
            }
        }

        // \k<n>
        // \k'n'
        // \k<-n>         
        // \k'-n'
        // \k<m+n>
        // \k<m-n>
        // \k'm+n'
        // \k'm-n'
        // \k<name>
        // \k'name'
        // \k<name+n>
        // \k<name-n>
        // \k'name+n'
        // \k'name-n'
        private void ParseBackreference() {
            Debug.Assert(_rubyPattern[_index - 1] == 'k');
            
            int terminator;
            int c = Read();
            if (c == '<') {
                terminator = '>';                
            } else if (c == '\'') {
                terminator = '\'';
            } else {
                throw MakeError("invalid back reference");
            }

            Append('\\');
            Append('k');
            Append((char)c);

            // TODO: relative names: <name+n>, <m+n>, ..
            c = Read();
            if (c == terminator || c == -1) {
                throw MakeError("group name is empty");
            }
            while (true) {
                Append((char)c);
                c = Read();
                if (c == terminator) {
                    Append((char)c);
                    break;
                } else if (c == -1) {
                    throw MakeError("invalid group name");
                }
            }
        }

        //
        // group_escape ::= 
        //     '\' '\'
        //   | '\' 'c' character
        //   | '\' 'C' '-' character
        //   | '\' 'M' '-' character
        //   | '\' 'M' '-' '\' 'C' '-' character
        //   | '\' 'p' '{' character_name '}'
        //   | '\' 'P' '{' character_name '}'
        //   | '\' 'x' hex_digit{1-2}
        //   | '\' 'u' hex_digit{4}
        //   | '\' octal{1-3}
        //   | '\' 'h'                                                 # hex digit
        //   | '\' 'H'                                                 # non-hex digit
        //   | '\' 's'                                                 # whitespace 
        //   | '\' 'S'                                                 # non-whitespace 
        //   | '\' 'w'                                                 # word character
        //   | '\' 'W'                                                 # non-word character
        //   | '\' 'd'                                                 # decimal digit 
        //   | '\' 'D'                                                 # non-decimal digit 
        //   | '\' 'b'                                                 # \u0008
        //   | '\' 't'                                                 # \u0009
        //   | '\' 'n'                                                 # \u000A
        //   | '\' 'v'                                                 # \u000B
        //   | '\' 'f'                                                 # \u000C
        //   | '\' other-character                                           
        //
        private int ParseSingleByteCharacterEscape(int escape) {
            bool hasControlModifier = false, hasMetaModifier = false;
            return ParseSingleByteCharacterEscape(escape, ref hasControlModifier, ref hasMetaModifier);
        }

        private int ParseSingleByteCharacterEscape(int escape, ref bool hasControlModifier, ref bool hasMetaModifier) {
            int c;
            switch (escape) {
                case -1: 
                    throw MakeError("too short escape sequence");

                case '\\': return '\\';
                case 'n': return '\n';
                case 't': return '\t';
                case 'r': return '\r';
                case 'f': return '\f';
                case 'v': return '\v';
                case 'a': return '\a';
                case 'e': return 27;
                case 'b': return '\b';
                
                case 'M':
                    if (!Read('-')) {
                        throw MakeError("too short meta escape");
                    }
                    if (hasMetaModifier) {
                        throw MakeError("duplicate meta escape");
                    }

                    hasMetaModifier = true;
                    c = Read();
                    if (c == -1) {
                        throw MakeError("too short escape sequence");
                    }
                    if (c == '\\') {
                        c = ParseSingleByteCharacterEscape(Read(), ref hasControlModifier, ref hasMetaModifier);
                    }
                    
                    return (c & 0xff) | 0x80;

                case 'C':
                    if (!Read('-')) {
                        throw MakeError("too short control escape");
                    }
                    goto case 'c';

                case 'c':
                    c = Read();
                    if (c == -1) {
                        throw MakeError("too short escape sequence");
                    }
                    if (hasControlModifier) {
                        throw MakeError("duplicate control escape");
                    }

                    hasControlModifier = true;
                    if (c == '\\') {
                        c = ParseSingleByteCharacterEscape(Read(), ref hasControlModifier, ref hasMetaModifier);
                    }
                    
                    return c & 0x9f;

                case 'x':
                    // hexa
                    c = Peek();
                    int d1 = Tokenizer.ToDigit(c);
                    if (d1 > 15) {
                        throw MakeError("invalid hex escape");
                    }
                    Skip();

                    c = Peek();
                    int d2 = Tokenizer.ToDigit(c);
                    if (d2 > 15) {
                        return d1;
                    }
                    Skip();

                    return (d1 << 4) | d2;
                
                default:
                    // octal
                    int o1 = Tokenizer.ToDigit(escape);
                    if (o1 > 7) {
                        return -1;
                    }

                    int o2 = Tokenizer.ToDigit(Peek());
                    if (o2 > 7) {
                        return o1;
                    }
                    Skip();

                    int o3 = Tokenizer.ToDigit(Peek());
                    if (o3 > 7) {
                        return (o1 << 3) | o2;
                    }
                    Skip();

                    return (o1 << 6) | (o2 << 3) | o3;
            }
        }

        private CharacterSet/*!*/ ParseCharacterEscape(int escape) {
            int result = ParseSingleByteCharacterEscape(escape);
            if (result != -1) {
                return new CharacterSet(Escape(result), true);
            }
                    
            switch (escape) {
                case 'h':
                case 'H': 
                    return MakePosixCharacterClass(PosixCharacterClass.XDigit, escape == 'h');
                    
                case 'p':
                case 'P':
                    return ParseCharacterCategoryName(escape);

                case 's':
                    return new CharacterSet(@"\s");

                case 'S':
                    return new CharacterSet(@"\S");

                case 'd':
                    return new CharacterSet(@"\d");

                case 'D':
                    return new CharacterSet(@"\D");

                case 'w':
                    return new CharacterSet(@"\w");

                case 'W':
                    return new CharacterSet(@"\W");

                default:
                    // ignore backslash unless needed
                    return new CharacterSet(Escape(escape), true);
            }
        }

        #endregion

        #region Unicode Codepoints

        // Peeks exactly 4 hexadecimal characters (\uFFFF).
        private int ParseUnicodeEscape() {
            int d4 = Tokenizer.ToDigit(Read());
            int d3 = Tokenizer.ToDigit(Read());
            int d2 = Tokenizer.ToDigit(Read());
            int d1 = Tokenizer.ToDigit(Read());

            if (d1 >= 16 || d2 >= 16 || d3 >= 16 || d4 >= 16) {
                throw MakeError("invalid Unicode escape");
            }

            int codepoint = (d4 << 12) | (d3 << 8) | (d2 << 4) | d1;
            if (codepoint >= 0xd800 && codepoint <= 0xdfff) {
                throw MakeError("invalid Unicode range");
            }
            return codepoint;
        }

        private IEnumerable<int>/*!*/ ParseUnicodeEscapeList() {
            Skip('{');
            while (true) {
                int codepoint = ParseUnicodeCodePoint();
                int c = Read();
                yield return codepoint;
                if (c == '}') {
                    break;
                }
                if (c != ' ') {
                    throw MakeError("invalid Unicode list");
                }
            }
        }

        // Parses [0-9A-F]{1,6}
        private int ParseUnicodeCodePoint() {
            int codepoint = 0;
            int i = 0;
            while (true) {
                int digit = Tokenizer.ToDigit(Peek());
                if (digit >= 16) {
                    break;
                }

                if (i < 7) {
                    codepoint = (codepoint << 4) | digit;
                }

                i++;
                Skip();
            }

            if (i == 0) {
                throw MakeError("invalid Unicode list");
            }
            return codepoint;
        }

        private string/*!*/ UnicodeCodePointToString(int codepoint) {
            var sb = new StringBuilder(2);
            AppendUnicodeCodePoint(sb, codepoint);
            return sb.ToString();
        }

        private void AppendUnicodeCodePoint(StringBuilder/*!*/ builder, int codepoint) {
            if (codepoint >= 0xd800 && codepoint <= 0xdfff || codepoint > 0x10ffff) {
                throw MakeError("invalid Unicode range");
            } else if (codepoint < 0x10000) {
                AppendEscaped(builder, codepoint);
            } else {
                codepoint -= 0x10000;
                Append((char)((codepoint / 0x400) + 0xd800));
                Append((char)((codepoint % 0x400) + 0xdc00));
            }
        }

        #endregion

        #region Chracter Groups

        // [include - [exclude]]
        // ^[include - [exclude]] == [p{All} - [include - [exclude]]
        private sealed class CharacterSet {
            public static readonly CharacterSet Empty = new CharacterSet();

            private readonly bool _negated;
            private readonly string/*!*/ _include;
            private readonly CharacterSet/*!*/ _exclude;
            private readonly bool _isSingleCharacter;

            public CharacterSet() {
                _include = "";
                _exclude = this;
                _isSingleCharacter = false;
            }
            
            public CharacterSet(string/*!*/ include)
                : this(false, include, Empty) {
            }

            public CharacterSet(string/*!*/ include, bool isSingleCharacter)
                : this(false, include, Empty) {
                _isSingleCharacter = isSingleCharacter;
            }

            public CharacterSet(bool negate, string/*!*/ include)
                : this(negate, include, Empty) {
            }

            public CharacterSet(string/*!*/ include, CharacterSet/*!*/ exclude)
                : this(false, include, exclude) {
            }

            public CharacterSet(bool negate, string/*!*/ include, CharacterSet/*!*/ exclude) {
                Assert.NotNull(include, exclude);
                _negated = negate;
                _include = include;
                _exclude = exclude;
            }

            public string/*!*/ Include {
                get { return _include; }
            }

            public bool IsEmpty {
                get { return _include.Length == 0 && !_negated; }
            }

            public bool IsSingleCharacter {
                get { return _isSingleCharacter; }
            }

            internal CharacterSet/*!*/ GetIncludedSet() {
                return new CharacterSet(_include, _isSingleCharacter);
            }

            internal CharacterSet/*!*/ Complement() {
                return new CharacterSet(!_negated, _include, _exclude);
            }

            internal CharacterSet/*!*/ Subtract(CharacterSet/*!*/ set) {
                if (IsEmpty || set.IsEmpty) {
                    return this;
                }

                if (_negated) {
                    if (set._negated) {
                        // (^A) \ (^B) = ^A and B = B \ A
                        return set.Complement().Subtract(Complement());
                    } else {
                        // (^A) \ B == ^(A or B)
                        return Complement().Union(set).Complement();
                    }
                } else if (set._negated) {
                    // A \ ^(B) == A and B
                    return Intersect(set.Complement());
                }

                // (a \ B) \ C == a \ (B or C)
                return new CharacterSet(_include, _exclude.Union(set));
            }

            internal CharacterSet/*!*/ Union(CharacterSet/*!*/ set) {
                if (IsEmpty) {
                    return set;
                } else if (set.IsEmpty) {
                    return this;
                }

                if (_negated) {
                    if (set._negated) {
                        // ^A or ^B == ^(A and B)
                        return Complement().Intersect(set.Complement()).Complement();
                    } else {
                        // ^A or B == ^(A \ B)
                        return Complement().Subtract(set).Complement();
                    }
                } else if (set._negated) {
                    // A or ^B == ^(B \ A)
                    return set.Complement().Subtract(this).Complement();
                }

                // (a \ B) or (c \ D) == (a or c) \ ((D \ a) or (B \ c) or (B and D))
                //
                // Proof: 
                // (a \ B) or (c \ D) == 
                // (a and ^B) or (c and ^D) == 
                // (a or c) and (a or ^D) and (^B or c) and (^B or ^D) ==
                // (a or c) \ (^(a or ^D) or ^(^B or c) or ^(^B or ^D)) ==
                // (a or c) \ ((D \ a) or (B \ c) or (B and D))                QED
                return new CharacterSet(_include + set._include,
                    set._exclude.Subtract(GetIncludedSet()).
                        Union(this._exclude.Subtract(set.GetIncludedSet())).
                        Union(this._exclude.Intersect(set._exclude))
                );
            }

            internal CharacterSet/*!*/ Intersect(CharacterSet/*!*/ set) {
                if (IsEmpty || set.IsEmpty) {
                    return Empty;
                }

                if (_negated) {
                    if (set._negated) {
                        // ^A and ^B == ^(A or B)
                        return Complement().Union(set.Complement()).Complement();
                    } else {
                        // ^A and B = B \ A
                        return set.Subtract(Complement());
                    }
                } else if (set._negated) {
                    // A and ^B = A \ B
                    return Subtract(set.Complement());
                }

                // (a \ B) and (c \ D) == a \ ^(c \ (B or D))
                // 
                // Proof:
                // (a \ B) and (c \ D) == 
                // (a and ^B) and (c and ^D) ==
                // a \ ^(c and ^B and ^D) ==
                // a \ ^(c \ (B or D))          QED
                return new CharacterSet(_include, new CharacterSet(true, set._include, _exclude.Union(set._exclude)));
            }

            public StringBuilder/*!*/ AppendTo(StringBuilder/*!*/ sb, bool parenthesize) {
                if (IsEmpty) {
                    if (_negated) {
                        sb.Append("[\0-\uffff]");
                    } else {
                        sb.Append("[a-[a]]");
                    }
                } else if (IsSingleCharacter && !parenthesize) {
                    sb.Append(_include);
                } else {
                    if (_negated) {
                        sb.Append("[\0-\uffff-");
                    }
                    sb.Append('[');
                    sb.Append(_include);
                    if (!_exclude.IsEmpty) {
                        sb.Append('-');
                        _exclude.AppendTo(sb, true);
                    }
                    sb.Append(']');
                    if (_negated) {
                        sb.Append(']');
                    }
                }
                return sb;
            }

            public override string/*!*/ ToString() {
                return IsEmpty ? String.Empty : AppendTo(new StringBuilder(), false).ToString();
            }
        }

        //
        // group ::= '[' negation_opt intersection ']'
        //
        // negation_opt ::= '^' | <empty>
        //
        private CharacterSet/*!*/ ParseCharacterGroup(bool nested) {
            Debug.Assert(_rubyPattern[_index - 1] == '[');

            bool positive = !Read('^');

            // [:alnum:]
            // [^:alnum:]
            if (nested) {
                var posixClass = ParsePosixCharacterClass(positive);
                if (posixClass != null) {
                    return posixClass;
                }
            }

            var result = ParseCharacterGroupIntersections();
            if (!positive) {
                result = result.Complement();
            }
            Debug.Assert(Peek() == -1 || Peek() == ']');
            Read(']');
            return result;
        }

        // 
        // intersection ::= intersection '&' '&' union
        //                | union
        // 
        private CharacterSet/*!*/ ParseCharacterGroupIntersections() {
            CharacterSet result = null;
            int c;
            while ((c = Peek()) != -1 && c != ']') {
                // eats &&
                var set = ParseCharacterGroupUnion();

                // result = result and set
                result = (result != null) ? result.Intersect(set) : set;
            }

            if (result == null) {
                throw MakeError((c == -1) ? "premature end of char-class" : "empty char-class");
            }

            return result;
        }

        //
        // union ::= union term
        //         | term
        //
        // term ::= escape
        //        | group
        //        | posix_character_class
        //        | character '-' character
        //        | character
        //
        // posix_character_class ::= '[' negation_opt ':' posix_character_class_name ':' ']' 
        //
        private CharacterSet ParseCharacterGroupUnion() {
            CharacterSet result = CharacterSet.Empty;

            // \u{1 2 3} produces multiple characters, the first and the last might be range bounds:
            IEnumerator<int> codepoints = null;

            while (true) {
                bool mayStartRange;
                var set = ParseCharacter(ref codepoints, out mayStartRange);
                if (set == null) {
                    break;
                }

                if (codepoints == null && Read('-')) {
                    // [a-]
                    // [a-&&b]
                    bool mayEndRange;
                    var rangeEnd = ParseCharacter(ref codepoints, out mayEndRange);
                    if (rangeEnd == null) {
                        result = result.Union(set).Union(new CharacterSet(@"\-", true));
                        break;
                    }

                    // [a-b]-z
                    // \p{L}-z
                    if (!mayStartRange || !set.IsSingleCharacter) {
                        throw MakeError("char-class value at start of range");
                    }

                    // a-[a-z]
                    // a-\p{L}
                    if (!mayEndRange || !rangeEnd.IsSingleCharacter) {
                        throw MakeError("char-class value at end of range");
                    }

                    set = new CharacterSet(set.Include + "-" + rangeEnd.Include);
                }

                result = result.Union(set);
            }
            return result;
        }

        private CharacterSet ParseCharacter(ref IEnumerator<int> codepoints, out bool mayStartRange) {
            if (codepoints != null) {
                mayStartRange = true;
                int current = codepoints.Current;
                if (!codepoints.MoveNext()) {
                    codepoints = null;
                }
                return new CharacterSet(UnicodeCodePointToString(current), true);
            }

            int c;
            switch (c = Read()) {
                case -1:
                    throw MakeError("premature end of char-class");

                case ']':
                    Back();
                    mayStartRange = false;
                    return null;

                case '&':
                    if (Read('&')) {
                        mayStartRange = false;
                        return null;
                    }
                    goto default;

                case '\\':
                    int escape = Read();
                    if (escape == 'u') {
                        int codepoint;
                        if (Peek() == '{') {
                            codepoints = ParseUnicodeEscapeList().GetEnumerator();
                            if (!codepoints.MoveNext()) {
                                throw MakeError("invalid Unicode list");
                            }
                            codepoint = codepoints.Current;
                            if (!codepoints.MoveNext()) {
                                codepoints = null;
                            }
                        } else {
                            codepoint = ParseUnicodeEscape();
                        }
                        mayStartRange = true;
                        return new CharacterSet(UnicodeCodePointToString(codepoint), true);
                    } else {
                        mayStartRange = true;
                        return ParseCharacterEscape(escape);
                    }

                case '[':
                    mayStartRange = false;
                    return ParseCharacterGroup(true);

                case '-':
                    // warning: character class has '-' without escape
                    mayStartRange = true;
                    return new CharacterSet(@"\-", true);

                default:
                    mayStartRange = true;
                    return new CharacterSet(((char)c).ToString(), true);
            }
        }

        private enum PosixCharacterClass {
            Alnum,
            Alpha,
            Ascii,
            Blank,
            Cntrl,
            Digit,
            Graph,
            Lower,
            Print,
            Punct,
            Space,
            Upper,
            XDigit,
            Word,
        }

        //
        //  \p{property-name}
        //  \p{^property-name}    (negative)
        //  \P{property-name}     (negative)
        //          
        // Property-name:
        //          
        //  + works on all encodings
        //    Alnum, Alpha, Blank, Cntrl, Digit, Graph, Lower,
        //    Print, Punct, Space, Upper, XDigit, Word, ASCII,
        //          
        //  + works on EUC_JP, Shift_JIS
        //    Hiragana, Katakana
        //          
        //  + works on UTF8, UTF16, UTF32
        //    Any, Assigned, C, Cc, Cf, Cn, Co, Cs, L, Ll, Lm, Lo, Lt, Lu,
        //    M, Mc, Me, Mn, N, Nd, Nl, No, P, Pc, Pd, Pe, Pf, Pi, Po, Ps,
        //    S, Sc, Sk, Sm, So, Z, Zl, Zp, Zs, 
        //    Arabic, Armenian, Bengali, Bopomofo, Braille, Buginese,
        //    Buhid, Canadian_Aboriginal, Cherokee, Common, Coptic,
        //    Cypriot, Cyrillic, Deseret, Devanagari, Ethiopic, Georgian,
        //    Glagolitic, Gothic, Greek, Gujarati, Gurmukhi, Han, Hangul,
        //    Hanunoo, Hebrew, Hiragana, Inherited, Kannada, Katakana,
        //    Kharoshthi, Khmer, Lao, Latin, Limbu, Linear_B, Malayalam,
        //    Mongolian, Myanmar, New_Tai_Lue, Ogham, Old_Italic, Old_Persian,
        //    Oriya, Osmanya, Runic, Shavian, Sinhala, Syloti_Nagri, Syriac,
        //    Tagalog, Tagbanwa, Tai_Le, Tamil, Telugu, Thaana, Thai, Tibetan,
        //    Tifinagh, Ugaritic, Yi
        //
        private CharacterSet/*!*/ ParseCharacterCategoryName(int escape) {
            bool positive = escape == 'p';

            int c = Peek();
            if (c != '{') {
                throw MakeError("invalid Unicode property");
            }
            Skip();

            // CLR doesn't support ^:
            if (Peek() == '^') {
                positive = !positive;
                Skip();
            }

            int start = _index;

            while ((c = Peek()) != '}' && c != -1) {
                Skip();
            }

            // trailing }
            if (c == -1) {
                throw MakeError("invalid Unicode property");
            }
            
            string name = _rubyPattern.Substring(start, _index - start);
            Skip();

            switch (name) {
                // CLR unsupported, any encoding:
                case "Alnum": return MakePosixCharacterClass(PosixCharacterClass.Alnum, positive); 
                case "Alpha": return MakePosixCharacterClass(PosixCharacterClass.Alpha, positive); 
                case "Blank": return MakePosixCharacterClass(PosixCharacterClass.Blank, positive); 
                case "Cntrl": return MakePosixCharacterClass(PosixCharacterClass.Cntrl, positive); 
                case "Digit": return MakePosixCharacterClass(PosixCharacterClass.Digit, positive); 
                case "Graph": return MakePosixCharacterClass(PosixCharacterClass.Graph, positive); 
                case "Lower": return MakePosixCharacterClass(PosixCharacterClass.Lower, positive); 
                case "Print": return MakePosixCharacterClass(PosixCharacterClass.Print, positive); 
                case "Punct": return MakePosixCharacterClass(PosixCharacterClass.Punct, positive); 
                case "Space": return MakePosixCharacterClass(PosixCharacterClass.Space, positive); 
                case "Upper": return MakePosixCharacterClass(PosixCharacterClass.Upper, positive); 
                case "XDigit": return MakePosixCharacterClass(PosixCharacterClass.XDigit, positive);
                case "ASCII": return MakePosixCharacterClass(PosixCharacterClass.Ascii, positive);
                case "Word": return MakePosixCharacterClass(PosixCharacterClass.Word, positive); 

                // CLR unsupported, Unicode only:
                case "Any":
                    // conjunction of any two disjunctive categories:
                    if (positive) {
                        return new CharacterSet(@"\P{L}\P{N}");
                    } else {
                        return new CharacterSet(@"\p{L}", new CharacterSet(@"\p{L}"));
                    }

                case "Assigned":
                    positive = !positive;
                    name = "Cn";
                    goto default;

                case "Arabic": 
                case "Armenian": 
                case "Bengali": 
                case "Bopomofo": 
                case "Braille": 
                case "Buginese":
                case "Buhid": 
                case "Cherokee": 
                case "Common": 
                case "Coptic":
                case "Cypriot": 
                case "Cyrillic": 
                case "Deseret": 
                case "Devanagari": 
                case "Ethiopic": 
                case "Georgian":
                case "Glagolitic": 
                case "Gothic": 
                case "Greek": 
                case "Gujarati": 
                case "Gurmukhi": 
                case "Han": 
                case "Hangul":
                case "Hanunoo": 
                case "Hebrew": 
                case "Hiragana": 
                case "Inherited":
                case "Kannada": 
                case "Katakana":
                case "Kharoshthi": 
                case "Khmer": 
                case "Lao": 
                case "Latin": 
                case "Limbu": 
                case "Linear_B":
                case "Malayalam":
                case "Mongolian": 
                case "Myanmar": 
                case "New_Tai_Lue":
                case "Ogham": 
                case "Old_Italic":
                case "Old_Persian":
                case "Oriya": 
                case "Osmanya": 
                case "Runic": 
                case "Shavian": 
                case "Sinhala": 
                case "Syloti_Nagri": 
                case "Syriac":
                case "Tagalog": 
                case "Tagbanwa": 
                case "TaiLe": 
                case "Tamil": 
                case "Telugu":
                case "Thaana": 
                case "Thai": 
                case "Tibetan":
                case "Tifinagh": 
                case "Ugaritic": 
                case "Yi":
                    // TODO: not all of the above are prefixed Is-
                    name = "Is" + name;
                    goto default;

                case "Canadian_Aboriginal":
                    name = "IsUnifiedCanadianAboriginalSyllabics";
                    goto default;

                default:
                    return new CharacterSet(@"\" + (positive ? 'p' : 'P') + "{" + name + "}");
            }
        }

        // [:xxx:] in character class
        // [^:xxx:] in character class
        private CharacterSet ParsePosixCharacterClass(bool positive) {
            int i = 0;
            if (Peek(i) == ':') {
                i++;
            } else {
                return null;
            }

            int start = _index + i;

            int c;
            while ((c = Peek(i)) != ':' && c != -1) {
                i++;
            }

            if (c == -1 || Peek(i + 1) != ']') {
                return null;
            }

            string name = _rubyPattern.Substring(start, _index + i - start);
            _index += i + 2;

            return MakePosixCharacterClass(ParsePosixClass(name), positive);
        }

        private PosixCharacterClass ParsePosixClass(string/*!*/ name) {
            switch (name) {
                case "alnum": return PosixCharacterClass.Alnum;
                case "alpha": return PosixCharacterClass.Alpha;
                case "ascii": return PosixCharacterClass.Ascii;
                case "blank": return PosixCharacterClass.Blank;
                case "cntrl": return PosixCharacterClass.Cntrl;
                case "digit": return PosixCharacterClass.Digit;
                case "graph": return PosixCharacterClass.Graph;
                case "lower": return PosixCharacterClass.Lower;
                case "print": return PosixCharacterClass.Print;
                case "punct": return PosixCharacterClass.Punct;
                case "space": return PosixCharacterClass.Space;
                case "upper": return PosixCharacterClass.Upper;
                case "xdigit": return PosixCharacterClass.XDigit;
                case "word": return PosixCharacterClass.Word;
                default: 
                    throw MakeError("invalid POSIX bracket type");
            }
        }

        private CharacterSet MakePosixCharacterClass(PosixCharacterClass charClass, bool positive) {
            switch (charClass) {
                case PosixCharacterClass.Alnum: 
                    if (positive) {
                        return new CharacterSet(@"\p{L}\p{N}\p{M}"); 
                    } else {
                        return new CharacterSet(@"\P{L}", new CharacterSet(@"\p{N}\p{M}")); 
                    }

                case PosixCharacterClass.Alpha:
                    if (positive) {
                        return new CharacterSet(@"\p{L}\p{M}"); 
                    } else {
                        return new CharacterSet(@"\P{L}", new CharacterSet(@"\p{M}")); 
                    }

                case PosixCharacterClass.Ascii:
                    if (positive) {
                        return new CharacterSet(@"\p{IsBasicLatin}"); 
                    } else {
                        return new CharacterSet(@"\P{IsBasicLatin}"); 
                    }

                case PosixCharacterClass.Blank:
                    if (positive) {
                        return new CharacterSet("\\p{Zs}\t"); 
                    } else {
                        return new CharacterSet(@"\P{Zs}", new CharacterSet("\t")); 
                    }
                    
                case PosixCharacterClass.Cntrl:
                    if (positive) {
                        return new CharacterSet(@"\p{Cc}"); 
                    } else {
                        return new CharacterSet(@"\P{Cc}"); 
                    }

                case PosixCharacterClass.Digit:
                    if (positive) {
                        return new CharacterSet(@"\p{Nd}"); 
                    } else {
                        return new CharacterSet(@"\P{Nd}"); 
                    }

                case PosixCharacterClass.Graph:
                    // TODO: there are some differences (Unicode version?)
                    if (positive) {
                        return new CharacterSet(@"\P{Z}", new CharacterSet(@"\p{C}")); 
                    } else {
                        return new CharacterSet(@"\p{Z}\p{C}"); 
                    }

                case PosixCharacterClass.Lower:
                    // TODO: there are some differences (Unicode version?)
                    if (positive) {
                        return new CharacterSet(@"\p{Ll}"); 
                    } else {
                        return new CharacterSet(@"\P{Ll}"); 
                    }

                case PosixCharacterClass.Print:
                    if (positive) {
                        return new CharacterSet(@"\P{C}");
                    } else {
                        return new CharacterSet(@"\p{C}");
                    }

                case PosixCharacterClass.Punct:
                    if (positive) {
                        return new CharacterSet(@"\p{P}"); 
                    } else {
                        return new CharacterSet(@"\P{P}"); 
                    }

                case PosixCharacterClass.Space:
                    if (positive) {
                        return new CharacterSet("\\p{Z}\u0085\u0009-\u000d"); 
                    } else {
                        return new CharacterSet(@"\P{Z}", new CharacterSet("\u0085\u0009-\u000d")); 
                    }

                case PosixCharacterClass.Upper:
                    // TODO: there are some differences (Unicode version?)
                    if (positive) {
                        return new CharacterSet(@"\p{Lu}"); 
                    } else {
                        return new CharacterSet(@"\P{Lu}"); 
                    }

                case PosixCharacterClass.XDigit:
                    if (positive) {
                        return new CharacterSet("a-fA-F0-9"); 
                    } else {
                        return new CharacterSet(true, "a-fA-F0-9");
                    }

                case PosixCharacterClass.Word:
                    // TODO: there are some differences (Unicode version?)
                    if (positive) {
                        return new CharacterSet(@"\p{L}\p{Nd}\p{Pc}\p{M}");
                    } else {
                        return new CharacterSet(@"\P{L}", new CharacterSet(@"\p{Nd}\p{Pc}\p{M}"));
                    }
            }

            throw Assert.Unreachable;
        }

        #endregion
    }
}
