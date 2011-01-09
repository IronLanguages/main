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
    public partial class RubyRegex : IEquatable<RubyRegex>, IDuplicable {
        // 1.9: correctly encoded, switched to characters 
        // 1.8: k-coded binary data, if _options specify encoding, or raw binary data otherwise.
        private MutableString/*!*/ _pattern;
        private RubyRegexOptions _options;
        private bool _hasGAnchor;

        private Regex _cachedRegex;

        // Ruby 1.8: match operations use KCODE encoding so we need to remember the one for which we have cached CLR Regex.
        private RubyRegexOptions _cachedKCode;

        #region Construction

        public RubyRegex() {
            _pattern = MutableString.CreateEmpty();
            _options = RubyRegexOptions.NONE;
        }

        public RubyRegex(MutableString/*!*/ pattern) 
            : this(pattern, RubyRegexOptions.NONE) {
        }

        public RubyRegex(MutableString/*!*/ pattern, RubyRegexOptions options) {
            Set(pattern, options);
        }

        public RubyRegex(RubyRegex/*!*/ regex) {
            ContractUtils.RequiresNotNull(regex, "regex");
            Set(regex.Pattern, regex.Options);
        }

        public void Set(MutableString/*!*/ pattern, RubyRegexOptions options) {
            ContractUtils.RequiresNotNull(pattern, "pattern");

            // RubyRegexOptions.Once is only used to determine how the Regexp object should be created and cached. 
            // It is not a property of the final object. /foo/ should compare equal with /foo/o.
            _options = options & ~RubyRegexOptions.Once;

            RubyEncoding encoding = RubyEncoding.GetRegexEncoding(options);
            if (encoding != null) {
                _pattern = MutableString.CreateBinary(pattern.ToByteArray(), encoding ?? RubyEncoding.Binary).Freeze();
            } else {
                _pattern = pattern.PrepareForCharacterRead().Clone().Freeze();
            }
            
            TransformPattern(encoding, options & RubyRegexOptions.EncodingMask);
        }

        /// <summary>
        /// Creates a copy of the proc that has the same target, context, self object as this instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Regexp.
        /// </summary>
        protected virtual RubyRegex/*!*/ Copy() {
            return new RubyRegex(this);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = Copy();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        #endregion

        #region Transformation to CLR Regex

        private Regex/*!*/ Transform(ref RubyEncoding encoding, MutableString/*!*/ input, int start, out string strInput) {
            ContractUtils.RequiresNotNull(input, "input");

            // TODO:

            // K-coding of the current operation (the current KCODE gets preference over the KCODE regex option):
            RubyRegexOptions kc = _options & RubyRegexOptions.EncodingMask;
            if (kc != 0) {
                encoding = _pattern.Encoding;
            } else {
                kc = RubyRegexOptions.NONE;
            }

            // Convert input to a string. Force k-coding if necessary.
            if (kc != 0) {
                // Handling multi-byte K-coded characters is not entirely correct here.
                // Three cases to be considered:
                // 1) Multi-byte character is explicitly contained in the pattern: /€*/
                // 2) Subsequent escapes form a complete character: /\342\202\254*/ or /\xe2\x82\xac*/
                // 3) Subsequent escapes form an incomplete character: /[\x7f-\xff]{1,3}/
                //
                // In the first two cases we want to "group" the byte triplet so that regex operators like *, +, ? and {n,m} operate on 
                // the entire character, not just the last byte. We could unescape the bytes and replace them with complete Unicode characters.
                // Then we could encode the input using the same K-coding and we would get a match. 
                // However, case 3) requires the opposite: to match the bytes we need to encode the input using binary encoding. 
                // Using this encoding makes *+? operators operate on the last byte (encoded as UTF16 character).
                // 
                // The right solution would require the regex engine to handle multi-byte escaped characters, which it doesn't.
                //
                // TODO:
                // A correct workaround would be to wrap the byte sequence that forms a character into a non-capturing group, 
                // for example transform /\342\202\254*/ to /(?:\342\202\254)*/ and use binary encoding on both input and pattern.
                // For now, we just detect if there are any non-ascii character escapes. If so we use a binary encoding accomodating case 3), 
                // but breaking cases 1 and 2. Otherwise we encode using k-coding to make case 1 match.
                if (HasEscapedNonAsciiBytes(_pattern)) {
                    encoding = RubyEncoding.Binary;
                    kc = 0;
                }
                
                strInput = ForceEncoding(input, encoding.Encoding, start);
            } else {
                _pattern.RequireCompatibleEncoding(input);
                input.PrepareForCharacterRead();
                strInput = input.ConvertToString();
            }

            return TransformPattern(encoding, kc);
        }

        private Regex/*!*/ TransformPattern(RubyEncoding encoding, RubyRegexOptions kc) {
            // We can reuse cached CLR regex if it was created for the same k-coding:
            if (_cachedRegex != null && kc == _cachedKCode) {
                return _cachedRegex;
            }

            string pattern;
            if (kc != 0 || encoding == RubyEncoding.Binary) {
                pattern = _pattern.ToString(encoding.Encoding);
            } else {
                pattern = _pattern.ConvertToString();
            }

            Regex result;
            try {
                result = new Regex(RegexpTransformer.Transform(pattern, _options, out _hasGAnchor), ToClrOptions(_options));
            } catch (Exception e) {
                throw new RegexpError(e.Message);
            }

            _cachedKCode = kc;
            _cachedRegex = result;
            return result;
        }

        /// <summary>
        /// Searches the pattern for hexadecimal and octal character escapes that represent a non-ASCII character.
        /// </summary>
        private static bool HasEscapedNonAsciiBytes(MutableString/*!*/ pattern) {
            int i = 0;
            int length = pattern.GetByteCount();
            while (i < length - 2) {
                int c = pattern.GetByte(i++);
                if (c == '\\') {
                    c = pattern.GetByte(i++);
                    if (c == 'x') {
                        // hexa escape:
                        int d1 = Tokenizer.ToDigit(PeekByte(pattern, length, i++));
                        if (d1 < 16) {
                            int d2 = Tokenizer.ToDigit(PeekByte(pattern, length, i++));
                            if (d2 < 16) {
                                return (d1 * 16 + d2 >= 0x80);
                            }
                        }
                    } else if (c >= '2' && c <= '7') {
                        // a backreference (\1..\9) or an octal escape:
                        int d = Tokenizer.ToDigit(PeekByte(pattern, length, i++));
                        if (d < 8) {
                            int value = Tokenizer.ToDigit(c) * 8 + d;
                            d = Tokenizer.ToDigit(PeekByte(pattern, length, i++));
                            if (d < 8) {
                                value = value * 8 + d;
                            }
                            return value >= 0x80;
                        }
                    }
                }
            }

            return false;
        }

        private static int PeekByte(MutableString/*!*/ str, int length, int i) {
            return (i < length) ? str.GetByte(i) : -1;
        }

        private static string/*!*/ ForceEncoding(MutableString/*!*/ input, Encoding/*!*/ encoding, int start) {
            int byteCount = input.GetByteCount();

            if (start < 0) {
                start += byteCount;
            }
            if (start < 0) {
                return null;
            }

            return (start <= byteCount) ? input.ToString(encoding, start, byteCount - start) : null;
        }

        #endregion

        public bool IsEmpty {
            get { return _pattern.IsEmpty; }
        }

        public RubyRegexOptions Options {
            get { return _options; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _pattern.Encoding; }
        }

        public MutableString/*!*/ Pattern {
            get { return _pattern; }
        }

        public bool Equals(RubyRegex other) {
            return ReferenceEquals(this, other) 
                || other != null && _options.Equals(other._options) && _pattern.Equals(other._pattern);
        }

        public override bool Equals(object other) {
            return Equals(other as RubyRegex);
        }

        public override int GetHashCode() {
            return _pattern.GetHashCode() ^ _options.GetHashCode();
        }

        public static RegexOptions ToClrOptions(RubyRegexOptions options) {
            RegexOptions result = RegexOptions.Multiline | RegexOptions.CultureInvariant;

#if DEBUG
            if (RubyOptions.CompileRegexps) {
#if SILVERLIGHT // RegexOptions.Compiled
                throw new NotSupportedException("RegexOptions.Compiled is not supported on Silverlight");
#else
                result |= RegexOptions.Compiled;
#endif
            }
#endif
            if ((options & RubyRegexOptions.IgnoreCase) != 0) {
                result |= RegexOptions.IgnoreCase;
            }

            if ((options & RubyRegexOptions.Extended) != 0) {
                result |= RegexOptions.IgnorePatternWhitespace;
            }

            if ((options & RubyRegexOptions.Multiline) != 0) {
                result |= RegexOptions.Singleline;
            }

            return result;
        }

        #region Match, LastMatch, Matches, Split

        public MatchData Match(MutableString/*!*/ input) {
            string str;
            RubyEncoding kcode = null;
            return MatchData.Create(Transform(ref kcode, input, 0, out str).Match(str), input, true, str);
        }

        /// <summary>
        /// Start is a number of bytes if kcode is given, otherwise it's a number of characters.
        /// </summary>
        public MatchData Match(MutableString/*!*/ input, int start, bool freezeInput) {
            string str;
            RubyEncoding kcode = null;
            Regex regex = Transform(ref kcode, input, start, out str);

            Match match;
            if (kcode != null) {
                if (str == null) {
                    return null;
                }
                match = regex.Match(str, 0);
            } else {
                if (start < 0) {
                    start += str.Length;
                }
                if (start < 0 || start > str.Length) {
                    return null;
                }
                match = regex.Match(str, start);
            }

            return MatchData.Create(match, input, freezeInput, str);
        }

        public MatchData LastMatch(MutableString/*!*/ input) {
            return LastMatch(input, Int32.MaxValue);
        }

        /// <summary>
        /// Finds the last match whose index is less than or equal to "start".
        /// Captures are ordered in the same way as with forward match. This is different from .NET reverse matching.
        /// Start is a number of bytes if kcode is given, otherwise it's a number of characters.
        /// </summary>
        public MatchData LastMatch(MutableString/*!*/ input, int start) {
            string str;
            RubyEncoding kcode = null;
            Regex regex = Transform(ref kcode, input, 0, out str);
            Debug.Assert(str != null);

            if (kcode != null) {
                int byteCount;
                byte[] bytes = input.GetByteArray(out byteCount);

                if (start < 0) {
                    start += byteCount;
                }

                // GetCharCount returns the number of whole characters:
                start = (start >= byteCount) ? str.Length : kcode.Encoding.GetCharCount(bytes, 0, start + 1) - 1;
            } else {
                if (start < 0) {
                    start += str.Length;
                }

                if (start > str.Length) {
                    start = str.Length;
                }
            }

            Match match;
            if (_hasGAnchor) {
                // This only makes some \G anchors work. It seems that CLR doesn't support \G if preceeded by some characters.
                // For example, this works in MRI but doesn't in CLR: "abcabczzz".rindex(/.+\G.+/, 3)
                match = regex.Match(str, start);
            } else {
                match = LastMatch(regex, str, start);
                if (match == null) {
                    return null;
                }
            }
            return MatchData.Create(match, input, true, str);
        }

        /// <summary>
        /// Binary searches "str" for the last match whose index is within the range [0, start].
        /// </summary>
        private static Match LastMatch(Regex/*!*/ regex, string/*!*/ input, int start) {
            Match result = null;
            int s = 0;
            int e = start;

            while (s <= e) {
                int m = (s + e) / 2;
                Match match = regex.Match(input, m);
                if (match.Success && match.Index <= e) {
                    result = match;
                    s = match.Index + 1;
                } else {
                    e = m - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of fresh MatchData objects.
        /// </summary>
        public IList<MatchData>/*!*/ Matches(MutableString/*!*/ input, bool inputMayMutate) {
            string str;
            RubyEncoding kcode = null;
            MatchCollection matches = Transform(ref kcode, input, 0, out str).Matches(str);

            var result = new MatchData[matches.Count];
            if (result.Length > 0 && inputMayMutate) {
                // clone and freeze the string once so that it can be shared by all the MatchData objects
                input = input.Clone().Freeze();
            }

            for (int i = 0; i < result.Length; i++) {
                result[i] = MatchData.Create(matches[i], input, false, str);
            }

            return result;
        }

        public IList<MatchData>/*!*/ Matches(MutableString/*!*/ input) {
            return Matches(input, true);
        }

        public MutableString[]/*!*/ Split(MutableString/*!*/ input) {
            string str;
            RubyEncoding kcode = null;
            return MutableString.MakeArray(Transform(ref kcode, input, 0, out str).Split(str), kcode ?? input.Encoding);
        }
        
        public MutableString[]/*!*/ Split(MutableString/*!*/ input, int count) {
            string str;
            RubyEncoding kcode = null;
            return MutableString.MakeArray(Transform(ref kcode, input, 0, out str).Split(str, count), kcode ?? input.Encoding);
        }

        public static MatchData SetCurrentMatchData(RubyScope/*!*/ scope, RubyRegex/*!*/ regex, MutableString str) {
            return scope.GetInnerMostClosureScope().CurrentMatch = (str != null) ? regex.Match(str) : null;
        }

        #endregion               

        #region ToString, Inspect

        public override string/*!*/ ToString() {
            return ToMutableString().ToString();
        }

        public MutableString/*!*/ ToMutableString() {
            return AppendTo(MutableString.CreateMutable(RubyEncoding.Binary));
        }

        public MutableString/*!*/ Inspect() {
            MutableString result = MutableString.CreateMutable(RubyEncoding.Binary);
            result.Append('/');
            AppendEscapeForwardSlash(result, _pattern);
            result.Append('/');
            AppendOptionString(result, true);
            return result;
        }

        public MutableString/*!*/ AppendTo(MutableString/*!*/ result) {
            Assert.NotNull(result);

            result.Append("(?");
            if (AppendOptionString(result, true) < 3) {
                result.Append('-');
            }
            AppendOptionString(result, false);
            result.Append(':');
            AppendEscapeForwardSlash(result, _pattern);
            result.Append(')');
            return result;
        }

        private int AppendOptionString(MutableString/*!*/ result, bool enabled) {
            int count = 0;
            var options = Options;

            if (((options & RubyRegexOptions.Multiline) != 0) == enabled) {
                result.Append('m');
                count++;
            }

            if (((options & RubyRegexOptions.IgnoreCase) != 0) == enabled) {
                result.Append('i');
                count++;
            }

            if (((options & RubyRegexOptions.Extended) != 0) == enabled) {
                result.Append('x');
                count++;
            }

            return count;
        }

        private static int SkipToUnescapedForwardSlash(MutableString/*!*/ pattern, int patternLength, int i) {
            while (i < patternLength) {
                i = pattern.IndexOf('/', i);
                if (i <= 0) {
                    return i;
                }

                if (pattern.GetChar(i - 1) != '\\') {
                    return i;
                }

                i++;
            }
            return -1;
        }

        private static MutableString/*!*/ AppendEscapeForwardSlash(MutableString/*!*/ result, MutableString/*!*/ pattern) {
            int first = 0;
            int patternLength = pattern.GetCharCount();
            int i = SkipToUnescapedForwardSlash(pattern, patternLength, 0);
            while (i >= 0) {
                Debug.Assert(i < patternLength);
                Debug.Assert(pattern.GetChar(i) == '/' && (i == 0 || pattern.GetChar(i - 1) != '\\'));

                result.Append(pattern, first, i - first);
                result.Append('\\');
                first = i; // include forward slash in the next append
                i = SkipToUnescapedForwardSlash(pattern, patternLength, i + 1);
            }

            result.Append(pattern, first, patternLength - first);
            return result;
        }

        #endregion

        #region Escape

        private const int EndOfPattern = -1;

        /// <summary>
        /// Returns a new instance of MutableString that contains escaped content of the given string.
        /// </summary>
        public static MutableString/*!*/ Escape(MutableString/*!*/ str) {
            return str.EscapeRegularExpression();
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

        #endregion
    }
}
