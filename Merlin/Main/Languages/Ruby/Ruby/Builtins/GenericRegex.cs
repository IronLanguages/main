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
#if DEBUG
        public abstract string/*!*/ GetTransformedPattern();
#endif

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
            ContractUtils.Requires(RubyRegex.GetPersistedOptions(options) == options);
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

#if DEBUG
        public override string/*!*/ GetTransformedPattern() {
            throw new NotImplementedException();
        }
#endif
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

        private const int EndOfPattern = -1;

        private readonly Regex/*!*/ _regex;
        private readonly string/*!*/ _pattern;

        internal protected StringRegex(string/*!*/ pattern, RubyRegexOptions options)
            : base(options) {
            Assert.NotNull(pattern);
            _pattern = pattern;

            string transformed = RegexpTransformer.Transform(pattern, options);
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
            // TODO (encoding):
            return MutableString.Create(_pattern, RubyEncoding.UTF8);
        }

#if DEBUG
        public override string/*!*/ GetTransformedPattern() {
            return _regex.ToString();
        }
#endif
        public override MutableString[]/*!*/ Split(MutableString/*!*/ input, int count, int start) {
            return MutableString.MakeArray(_regex.Split(input.ConvertToString(), count, start), input.Encoding);
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
            return EndOfPattern;
        }

        internal static string/*!*/ Escape(string/*!*/ pattern) {
            StringBuilder sb = EscapeToStringBuilder(pattern);
            return (sb != null) ? sb.ToString() : pattern;
        }

        internal static StringBuilder EscapeToStringBuilder(string/*!*/ pattern) {
            int first = 0;
            char escaped;
            int i = SkipNonSpecial(pattern, 0, out escaped);

            if (i == EndOfPattern) {
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

        internal static string/*!*/ TransformPattern(string/*!*/ pattern, RubyRegexOptions options) {
            return RegexpTransformer.Transform(pattern, options);
        }
    }
}
