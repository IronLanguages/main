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
    public partial class RubyRegex : IEquatable<RubyRegex>, IDuplicable {
        private GenericRegex/*!*/ _regex;

        #region Construction

        public RubyRegex() {
            _regex = StringRegex.Empty;
        }

        public RubyRegex(MutableString/*!*/ pattern, RubyRegexOptions options) {
            ContractUtils.RequiresNotNull(pattern, "pattern");
            _regex = pattern.ToRegularExpression(options);
        }

        public RubyRegex(string/*!*/ pattern, RubyRegexOptions options) {
            ContractUtils.RequiresNotNull(pattern, "pattern");
            _regex = new StringRegex(pattern, options);
        }

        public RubyRegex(byte[]/*!*/ pattern, RubyRegexOptions options) {
            ContractUtils.RequiresNotNull(pattern, "pattern");
            // TODO: _regex = new BinaryRegex(pattern, options);
            _regex = new StringRegex(BinaryEncoding.Obsolete.GetString(pattern, 0, pattern.Length), options);
        }

        public RubyRegex(Regex/*!*/ regex) {
            ContractUtils.RequiresNotNull(regex, "regex");
            _regex = new StringRegex(regex);
        }

        public RubyRegex(RubyRegex/*!*/ regex) {
            ContractUtils.RequiresNotNull(regex, "regex");
            _regex = regex._regex;
        }

        /// <summary>
        /// Creates a copy of the proc that has the same target, context, self object  as this instance.
        /// Doesn't copy instance data.
        /// Preserves the class of the Proc.
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

        public bool IsEmpty {
            get { return _regex.IsEmpty; }
        }

        public RubyRegexOptions Options {
            get { return _regex.Options; }
        }

        public MutableString/*!*/ GetPattern() {
            return _regex.GetPattern();
        }

#if DEBUG
        public string/*!*/ GetTransformedPattern() {
            return _regex.GetTransformedPattern();
        }
#endif
        public bool Equals(RubyRegex other) {
            return ReferenceEquals(this, other) || other != null && _regex.Equals(other._regex);
        }

        public override bool Equals(object other) {
            return _regex.Equals(other as RubyRegex);
        }

        public override int GetHashCode() {
            return _regex.GetHashCode();
        }

        #region Match, ReverseMatch, Matches, Split

        public Match/*!*/ Match(MutableString/*!*/ input) {
            return Match(input, 0);
        }

        public Match/*!*/ Match(MutableString/*!*/ input, int start) {
            ContractUtils.RequiresNotNull(input, "input");
            return Match(input, start, input.Length - start);
        }

        public Match/*!*/ Match(MutableString/*!*/ input, int start, int count) {
            ContractUtils.RequiresNotNull(input, "input");
            return _regex.Match(input, start, count);
        }

        public Match/*!*/ ReverseMatch(MutableString/*!*/ str, int start) {
            ContractUtils.RequiresNotNull(str, "str");
            return _regex.ReverseMatch(str, start);
        }
        
        public Match/*!*/ ReverseMatch(MutableString/*!*/ str, int start, int count) {
            ContractUtils.RequiresNotNull(str, "str");
            return _regex.ReverseMatch(str, start, count);            
        }

        public MatchCollection/*!*/ Matches(MutableString/*!*/ input) {
            return Matches(input, 0);
        }

        public MatchCollection/*!*/ Matches(MutableString/*!*/ input, int start) {
            ContractUtils.RequiresNotNull(input, "input");
            return _regex.Matches(input, start);
        }

        public MutableString[]/*!*/ Split(MutableString/*!*/ input) {
            return _regex.Split(input, 0, 0);
        }

        public MutableString[]/*!*/ Split(MutableString/*!*/ input, int count) {
            return _regex.Split(input, count, 0);
        }
        
        public MutableString[]/*!*/ Split(MutableString/*!*/ input, int count, int start) {
            return _regex.Split(input, count, start);
        }

        public static MatchData SetCurrentMatchData(RubyScope/*!*/ scope, RubyRegex/*!*/ regex, MutableString/*!*/ str) {
            var targetScope = scope.GetInnerMostClosureScope();
            
            if (str == null) {
                return targetScope.CurrentMatch = null;
            }

            Match match = regex.Match(str, 0);
            if (match.Success) {
                return targetScope.CurrentMatch = scope.RubyContext.TaintObjectBy(new MatchData(match, str), str);
            } else {
                return targetScope.CurrentMatch = null;
            }
        }

        #endregion

        /// <summary>
        /// Returns a new instance of MutableString that contains escaped content of the given string.
        /// </summary>
        public static MutableString/*!*/ Escape(MutableString/*!*/ str) {
            // TODO:
            return str.EscapeRegularExpression();
        }

        public void Set(MutableString/*!*/ pattern, RubyRegexOptions options) {
            _regex = pattern.ToRegularExpression(options);
        }

        public static RegexOptions ToClrOptions(RubyRegexOptions options) {
            RegexOptions result = RegexOptions.Multiline;

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
    }
}
