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
    public abstract class GenericRegex {
        private readonly RubyRegexOptions _options;

        public abstract bool IsEmpty { get; }
        public RubyRegexOptions Options { get { return _options; } }
        public abstract MutableString/*!*/ GetPattern();

        public abstract Match/*!*/ Match(MutableString/*!*/ input, int start, int count);
        public abstract Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start);
        public abstract Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start, int count);
        public abstract MatchCollection/*!*/ Matches(MutableString/*!*/ input, int start);
        public abstract MutableString[]/*!*/ Split(MutableString/*!*/ input, int count, int start);

        public bool Equals(GenericRegex/*!*/ other) {
            // TODO:
            return Options == other.Options && GetPattern().Equals(other.GetPattern());
        }

        public override int GetHashCode() {
            // TODO:
            return (int)Options ^ GetPattern().GetHashCode();
        }

        protected GenericRegex(RubyRegexOptions options) {
            _options = options;
        }
    }

    public class BinaryRegex : GenericRegex {
        private readonly byte[]/*!*/ _pattern;

        internal protected BinaryRegex(byte[]/*!*/ pattern, RubyRegexOptions options)
            : base(options) {
            Assert.NotNull(pattern);
            _pattern = ArrayUtils.Copy(pattern);
        }

        public override bool IsEmpty {
            get { return _pattern.Length == 0; }
        }

        public override Match/*!*/ Match(MutableString/*!*/ input, int start, int count) {
            throw new NotImplementedException();
        }

        public override Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start) {
            throw new NotImplementedException();
        }

        public override Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start, int count) {
            throw new NotImplementedException();
        }

        public override MatchCollection/*!*/ Matches(MutableString/*!*/ input, int start) {
            throw new NotImplementedException();
        }

        public override MutableString/*!*/ GetPattern() {
            return MutableString.CreateBinary(_pattern);
        }

        public override MutableString[]/*!*/ Split(MutableString/*!*/ input, int count, int start) {
            throw new NotImplementedException();
        }

        internal static byte[]/*!*/ Escape(byte[]/*!*/ pattern) {
            // TODO: encoding
            return BinaryEncoding.Obsolete.GetBytes(StringRegex.Escape(BinaryEncoding.Obsolete.GetString(pattern, 0, pattern.Length)));
        }
    }

    public class StringRegex : GenericRegex {
        internal static readonly StringRegex Empty = new StringRegex(new Regex(String.Empty, RubyRegex.ToClrOptions(RubyRegexOptions.NONE)));

        private readonly Regex/*!*/ _regex;
        private readonly string/*!*/ _pattern;

        internal protected StringRegex(string/*!*/ pattern, RubyRegexOptions options)
            : base(options) {
            Assert.NotNull(pattern);
            _pattern = pattern;

            string transformed = TransformPattern(pattern, options);
            try {
                _regex = new Regex(transformed, RubyRegex.ToClrOptions(options));
            } catch (ArgumentException e) {
                Utils.Log("-- original ---" + new String('-', 50), "REGEX_ERROR");
                Utils.Log(pattern, "REGEX_ERROR");
                Utils.Log("-- transformed " + new String('-', 50), "REGEX_ERROR");
                Utils.Log(transformed, "REGEX_ERROR");
                Utils.Log("---------------" + new String('-', 50), "REGEX_ERROR");
                throw new RegexpError(e.Message, e);
            }
        }

        internal protected StringRegex(Regex/*!*/ regex)
            : base(RubyRegexOptions.NONE) {
            Assert.NotNull(regex);
            _regex = regex;
            _pattern = regex.ToString();
        }

        public override bool IsEmpty {
            get { return _pattern.Length == 0; }
        }

        public override Match/*!*/ Match(MutableString/*!*/ input, int start, int count) {
            return _regex.Match(input.ConvertToString(), start, count);
        }

        public override Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start) {
            return new Regex(_pattern, _regex.Options | RegexOptions.RightToLeft).Match(input.ConvertToString(), start);
        }

        public override Match/*!*/ ReverseMatch(MutableString/*!*/ input, int start, int count) {
            return new Regex(_pattern, _regex.Options | RegexOptions.RightToLeft).Match(input.ConvertToString(), start, count);
        }

        public override MatchCollection Matches(MutableString/*!*/ input, int start) {
            return _regex.Matches(input.ConvertToString(), start);
        }

        public override MutableString/*!*/ GetPattern() {
            return MutableString.Create(_pattern);
        }

        public override MutableString[]/*!*/ Split(MutableString/*!*/ input, int count, int start) {
            return MutableString.MakeArray(_regex.Split(input.ConvertToString(), count, start));
        }

        private static int SkipNonSpecial(string/*!*/ pattern, int i, out char escaped) {
            while (i < pattern.Length) {
                char c = pattern[i];
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
                    case ' ':
                        escaped = c;
                        return i;

                    case '\t':
                        escaped = 't';
                        return i;

                    case '\n':
                        escaped = 'n';
                        return i;

                    case '\r':
                        escaped = 'r';
                        return i;

                    case '\f':
                        escaped = 'f';
                        return i;
                }
                i++;
            }

            escaped = '\0';
            return -1;
        }

        internal static string/*!*/ Escape(string/*!*/ pattern) {
            StringBuilder sb = EscapeToStringBuilder(pattern);
            return (sb != null) ? sb.ToString() : pattern;
        }

        internal static StringBuilder EscapeToStringBuilder(string/*!*/ pattern) {
            int first = 0;
            char escaped;
            int i = SkipNonSpecial(pattern, 0, out escaped);

            if (i == -1) {
                return null;
            }

            StringBuilder result = new StringBuilder(pattern.Length + 1);

            do {
                Debug.Assert(i < pattern.Length);
                // pattern[i] needs escape

                result.Append(pattern, first, i - first);
                result.Append('\\');
                result.Append(escaped);
                i++;

                Debug.Assert(i <= pattern.Length);

                first = i;
                i = SkipNonSpecial(pattern, i, out escaped);
            } while (i >= 0);

            result.Append(pattern, first, pattern.Length - first);
            return result;
        }

        private static int SkipWellEscaped(string/*!*/ pattern, int i) {
            while (i < pattern.Length - 1) {
                if (pattern[i] == '\\') {
                    switch (pattern[i + 1]) {
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

                        case 'C': // control characters
                        case 'M': // meta characters
                        // TODO: replace

                        // Oniguruma + .NET - character classes, they don't match so some fixups would be needed
                        // MRI: doesn't support, but is also an error since it is followed by {name}, which is illegal
                        case 'p':
                        case 'P':
                            // keep
                            break;

                        default:
                            return i;
                    }
                    i += 2;
                } else {
                    i += 1;
                }
            }
            return -1;
        }

        // fixes escapes
        // - unescapes non-special characters
        // - fixes \xF     -> \x0F
        internal static string/*!*/ TransformPattern(string/*!*/ pattern, RubyRegexOptions options) {

            int first = 0;
            int i = SkipWellEscaped(pattern, 0);

            // trailing backslash is an error in both MRI, .NET
            if (i == -1) {
                return pattern;
            }

            StringBuilder result = new StringBuilder(pattern.Length);

            do {
                Debug.Assert(i + 1 < pattern.Length);
                Debug.Assert(pattern[i] == '\\');

                result.Append(pattern, first, i - first);
                i++;

                char c = pattern[i++];
                switch (c) {
                    case 'x':
                        result.Append('\\');
                        result.Append('x');

                        // error:
                        if (i == pattern.Length) {
                            break;
                        }

                        // fix single digit:
                        c = pattern[i++];
                        if (i == pattern.Length || !Tokenizer.IsHexadecimalDigit(pattern[i])) {
                            result.Append('0');
                        }
                        result.Append(c);
                        break;

                    case 'h': // Oniguruma only: [0-9A-Fa-f]
                    case 'H': // Oniguruma only: [^0-9A-Fa-f]
                    case 'g': // Oniguruma only
                    case 'k': // Oniguruma, .NET: named backreference, MRI not supported
                    // remove backslash

                    default:
                        if (Tokenizer.IsDecimalDigit(c)) {
                            // TODO:
                            // \([1-9][0-9]*) where there is no group of such number (replace by an empty string)
                            result.Append('\\');
                        }

                        // .NET throws invalid escape exception, remove backslash:
                        result.Append(c);
                        break;
                }
                Debug.Assert(i <= pattern.Length);

                first = i;
                i = SkipWellEscaped(pattern, i);
            } while (i >= 0);

            result.Append(pattern, first, pattern.Length - first);
            return result.ToString();
        }
    }
}
