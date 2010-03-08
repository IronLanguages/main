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
using System.Reflection;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    [RubyClass("Regexp", Extends = typeof(RubyRegex), Inherits = typeof(Object)), Includes(typeof(Enumerable))]
    public static class RegexpOps {
        #region Helpers

        internal static bool NormalizeGroupIndex(ref int index, int groupCount) {
            // Normalize index against # Groups in Match
            if (index < 0) {
                index += groupCount;
                // Cannot refer to zero using negative indices 
                if (index == 0) {
                    return false;
                }
            }

            if (index < 0 || index > groupCount) {
                return false;
            }

            return true;
        }

        #endregion

        #region constructors, compile

        [RubyConstructor]
        public static RubyRegex/*!*/ Create(RubyClass/*!*/ self, 
            [NotNull]RubyRegex/*!*/ other) {
            
            return new RubyRegex(other);
        }

        [RubyConstructor]
        public static RubyRegex/*!*/ Create(RubyClass/*!*/ self,
            [NotNull]RubyRegex/*!*/ other, int options, [Optional]object encoding) {
            return Create(self, other, (object)options, encoding);
        }

        [RubyConstructor]
        public static RubyRegex/*!*/ Create(RubyClass/*!*/ self,
            [NotNull]RubyRegex/*!*/ other, [DefaultParameterValue(null)]object options, [Optional]object encoding) {

            ReportParametersIgnoredWarning(self.Context, encoding);
            return new RubyRegex(other);
        }
                
        [RubyConstructor]
        public static RubyRegex/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, int options, [DefaultProtocol, Optional]MutableString encoding) {

            return new RubyRegex(pattern, MakeOptions(options, encoding));
        }

        [RubyConstructor]
        public static RubyRegex/*!*/ Create(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [Optional]bool ignoreCase, [DefaultProtocol, Optional]MutableString encoding) {

            return new RubyRegex(pattern, MakeOptions(ignoreCase, encoding));
        }

        [RubyMethod("compile", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Compile() {
            return new RuleGenerator(RuleGenerators.InstanceConstructor);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyRegex/*!*/ Reinitialize(RubyRegex/*!*/ self, [NotNull]RubyRegex/*!*/ other) {
            self.Set(other.Pattern, other.Options);
            return self;
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyRegex/*!*/ Reinitialize(RubyContext/*!*/ context, RubyRegex/*!*/ self,
            [NotNull]RubyRegex/*!*/ regex, int options, [Optional]object encoding) {

            ReportParametersIgnoredWarning(context, encoding);
            return Reinitialize(self, regex);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyRegex/*!*/ Reinitialize(RubyContext/*!*/ context, RubyRegex/*!*/ self,
            [NotNull]RubyRegex/*!*/ regex, [DefaultParameterValue(null)]object ignoreCase, [Optional]object encoding) {

            ReportParametersIgnoredWarning(context, encoding);
            return Reinitialize(self, regex);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyRegex/*!*/ Reinitialize(RubyRegex/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, int options, [DefaultProtocol, Optional]MutableString encoding) {

            self.Set(pattern, MakeOptions(options, encoding));
            return self;
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyRegex/*!*/ Reinitialize(RubyRegex/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [Optional]bool ignoreCase, [DefaultProtocol, Optional]MutableString encoding) {

            self.Set(pattern, MakeOptions(ignoreCase, encoding));
            return self;
        }

        private static void ReportParametersIgnoredWarning(RubyContext/*!*/ context, object encoding) {
            context.ReportWarning((encoding != Missing.Value) ? "flags and encoding ignored" : "flags ignored");
        }

        internal static RubyRegexOptions MakeOptions(bool ignoreCase, MutableString encoding) {
            return (ignoreCase ? RubyRegexOptions.IgnoreCase : RubyRegexOptions.NONE) | StringToRegexEncoding(encoding);
        }

        internal static RubyRegexOptions MakeOptions(int options, MutableString encoding) {
            return (RubyRegexOptions)options | StringToRegexEncoding(encoding);
        }

        internal static RubyRegexOptions StringToRegexEncoding(MutableString encoding) {
            if (MutableString.IsNullOrEmpty(encoding)) {
                return RubyRegexOptions.NONE;
            }

            switch (encoding.GetChar(0)) {
                case 'N':
                case 'n': return RubyRegexOptions.FIXED; 
                case 'E':
                case 'e': return RubyRegexOptions.EUC; 
                case 'S':
                case 's': return RubyRegexOptions.SJIS;
                case 'U':
                case 'u': return RubyRegexOptions.UTF8;
            }

            return RubyRegexOptions.NONE; 
        }
        
        #endregion

        [RubyConstant]
        public const int IGNORECASE = (int)RubyRegexOptions.IgnoreCase;

        [RubyConstant]
        public const int EXTENDED = (int)RubyRegexOptions.Extended;

        [RubyConstant]
        public const int MULTILINE = (int)RubyRegexOptions.Multiline;

        /// <summary>
        /// Returns "(?{enabled-options}-{disabled-options}:{pattern-with-forward-slash-escaped})".
        /// Doesn't escape forward slashes that are already escaped.
        /// </summary>
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyRegex/*!*/ self) {
            // Ruby: doesn't wrap if there is a single embedded expression that evaluates to non-nil:
            // puts(/#{nil}#{/a/}#{nil}/) 
            // We don't do that.

            return self.ToMutableString();
        }

        /// <summary>
        /// Returns "/{pattern-with-forward-slash-escaped}/"
        /// Doesn't escape forward slashes that are already escaped.
        /// </summary>
        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyRegex/*!*/ self) {
            return self.Inspect();
        }

        [RubyMethod("options")]
        public static int GetOptions(RubyRegex/*!*/ self) {
            return (int)self.Options;
        }

        [RubyMethod("kcode")]
        public static MutableString GetKCode(RubyRegex/*!*/ self) {
            switch (self.Options & RubyRegexOptions.EncodingMask) {
                case RubyRegexOptions.NONE: return null;
                case RubyRegexOptions.EUC: return MutableString.CreateAscii("euc");
                case RubyRegexOptions.FIXED: return MutableString.CreateAscii("none");
                case RubyRegexOptions.UTF8: return MutableString.CreateAscii("utf8");
                case RubyRegexOptions.SJIS: return MutableString.CreateAscii("sjis");
                default: throw Assert.Unreachable;
            }
        }

        [RubyMethod("encoding", Compatibility = RubyCompatibility.Ruby19)]
        public static RubyEncoding/*!*/ GetEncoding(RubyRegex/*!*/ self) {
            return self.Encoding;
        }

        [RubyMethod("casefold?")]
        public static bool IsCaseInsensitive(RubyRegex/*!*/ self) {
            return (self.Options & RubyRegexOptions.IgnoreCase) != 0;
        }

        [RubyMethod("match")]
        public static MatchData Match(RubyScope/*!*/ scope, RubyRegex/*!*/ self, [DefaultProtocol]MutableString str) {
            return RubyRegex.SetCurrentMatchData(scope, self, str);
        }

        [RubyMethod("hash")]
        public static int GetHash(RubyRegex/*!*/ self) {
            return self.GetHashCode();
        }

        [RubyMethod("=="), RubyMethod("eql?")]
        public static bool Equals(RubyRegex/*!*/ self, object other) {
            return false;
        }

        [RubyMethod("=="), RubyMethod("eql?")]
        public static bool Equals(RubyContext/*!*/ context, RubyRegex/*!*/ self, [NotNull]RubyRegex/*!*/ other) {
            return self.Equals(other);
        }

        [RubyMethod("=~")]
        public static object MatchIndex(RubyScope/*!*/ scope, RubyRegex/*!*/ self, [DefaultProtocol]MutableString/*!*/ str) {
            MatchData match = RubyRegex.SetCurrentMatchData(scope, self, str);
            return (match != null) ? ScriptingRuntimeHelpers.Int32ToObject(match.Index) : null;
        }

        [RubyMethod("===")]
        public static bool CaseCompare(ConversionStorage<MutableString>/*!*/ stringTryCast, RubyScope/*!*/ scope, RubyRegex/*!*/ self, object obj) {
            MutableString str = Protocols.TryCastToString(stringTryCast, obj);
            return str != null && Match(scope, self, str) != null;
        }

        [RubyMethod("~")]
        public static object ImplicitMatch(ConversionStorage<MutableString>/*!*/ stringCast, RubyScope/*!*/ scope, RubyRegex/*!*/ self) {
            return MatchIndex(scope, self, Protocols.CastToString(stringCast, scope.GetInnerMostClosureScope().LastInputLine));
        }

        [RubyMethod("source")]
        public static MutableString/*!*/ Source(RubyRegex/*!*/ self) {
            return self.Pattern.Clone();
        }

        [RubyMethod("escape", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("quote", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Escape(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ str) {
            return RubyRegex.Escape(str).TaintBy(str);
        }

        [RubyMethod("last_match", RubyMethodAttributes.PublicSingleton)]
        public static MatchData LastMatch(RubyScope/*!*/ scope, RubyClass/*!*/ self) {
            return scope.GetInnerMostClosureScope().CurrentMatch;
        }

        [RubyMethod("last_match", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ LastMatch(RubyScope/*!*/ scope, RubyClass/*!*/ self, [DefaultProtocol]int groupIndex) {
            return scope.GetInnerMostClosureScope().CurrentMatch.GetGroupValue(groupIndex);
        }

        [RubyMethod("union", RubyMethodAttributes.PublicSingleton)]
        public static RubyRegex/*!*/ Union(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, params object[]/*!*/ strings) {

            if (strings.Length == 0) {
                return new RubyRegex(MutableString.CreateAscii("(?!)"), RubyRegexOptions.NONE);
            }

            MutableString result = MutableString.CreateMutable(RubyEncoding.Binary);
            for (int i = 0; i < strings.Length; i++) {
                if (i > 0) {
                    result.Append('|');
                }

                RubyRegex regex = strings[i] as RubyRegex;
                if (regex != null) {
                    if (strings.Length == 1) {
                        return regex;
                    }

                    regex.AppendTo(result);
                } else {
                    result.Append(RubyRegex.Escape(Protocols.CastToString(stringCast, strings[i])));
                }
            }

            // TODO:
            //RubyClass regexClass = RubyUtils.GetExecutionContext(context).GetClass(typeof(RubyRegex));
            //return NewCallSite3.Invoke(context, regexClass, result, null, null);
            return new RubyRegex(result, RubyRegexOptions.NONE);
        }
    }
}
