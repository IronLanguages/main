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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {

    [RubyClass(Extends = typeof(string), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(ClrString), typeof(Enumerable), typeof(Comparable))]
    [HideMethod("[]")]
    [HideMethod("==")]
    [HideMethod("insert")]
    [HideMethod("split")]
    [HideMethod("clone")]
    public static class ClrStringOps {
        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol]MutableString/*!*/ str) {
            return str.ToString();
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, char c, [DefaultParameterValue(1)]int repeatCount) {
            return new String(c, repeatCount);
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [NotNull]char[]/*!*/ chars) {
            return new String(chars);
        }

        [RubyConstructor]
        public static string/*!*/ Create(RubyClass/*!*/ self, [NotNull]char[]/*!*/ chars, int startIndex, int length) {
            return new String(chars, startIndex, length);
        }
    }

    /// <summary>
    /// Mixed into System::String and System::Char.
    /// </summary>
    [RubyModule("String", DefineIn = typeof(IronRubyOps.Clr))]
    public static class ClrString {
        #region %, *, +

        [RubyMethod("%")]
        public static string/*!*/ Format(StringFormatterSiteStorage/*!*/ storage, string/*!*/ self, [NotNull]IList/*!*/ args) {
            StringFormatter formatter = new StringFormatter(storage, self, RubyEncoding.UTF8, args);
            return formatter.Format().ToString();
        }

        [RubyMethod("%")]
        public static string/*!*/ Format(StringFormatterSiteStorage/*!*/ storage, ConversionStorage<IList>/*!*/ arrayTryCast,
            string/*!*/ self, object args) {
            return Format(storage, self, Protocols.TryCastToArray(arrayTryCast, args) ?? new[] { args });
        }

        [RubyMethod("*")]
        public static string/*!*/ Repeat(string/*!*/ self, [DefaultProtocol]int times) {
            if (times < 0) {
                throw RubyExceptions.CreateArgumentError("negative argument");
            }

            var result = new StringBuilder();
            for (int i = 0; i < times; i++) {
                result.Append(self);
            }

            return result.ToString();
        }

        [RubyMethod("+")]
        public static string/*!*/ Concatenate(string/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            return self + other.ToString();
        }

        #endregion

        #region <=>, ==, ===, eql?

        [RubyMethod("<=>")]
        public static int Compare(string/*!*/ self, [NotNull]string/*!*/ other) {
            return Math.Sign(String.CompareOrdinal(self, other));
        }

        [RubyMethod("<=>")]
        public static int Compare(string/*!*/ self, [NotNull]MutableString/*!*/ other) {
            return -Math.Sign(other.CompareTo(self));
        }

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, RespondToStorage/*!*/ respondToStorage, string/*!*/ self, object other) {
            return MutableStringOps.Compare(comparisonStorage, respondToStorage, self, other);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool StringEquals(string/*!*/ lhs, [NotNull]string/*!*/ rhs) {
            return lhs == rhs;
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool StringEquals(string/*!*/ lhs, [NotNull]MutableString/*!*/ rhs) {
            return rhs.Equals(lhs);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RespondToStorage/*!*/ respondToStorage, BinaryOpStorage/*!*/ equalsStorage, string/*!*/ self, object other) {
            return MutableStringOps.Equals(respondToStorage, equalsStorage, self, other);
        }

        [RubyMethod("eql?")]
        public static bool Eql(string/*!*/ lhs, [NotNull]string/*!*/ rhs) {
            return lhs == rhs;
        }

        [RubyMethod("eql?")]
        public static bool Eql(string/*!*/ lhs, [NotNull]MutableString/*!*/ rhs) {
            return rhs.Equals(lhs);
        }

        [RubyMethod("eql?")]
        public static bool Eql(string/*!*/ lhs, object rhs) {
            return false;
        }

        #endregion

        #region [], slice

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static object GetChar(string/*!*/ self, [DefaultProtocol]int index) {
            return MutableStringOps.InExclusiveRangeNormalized(self.Length, ref index) ? RubyUtils.CharToObject(self[index]) : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static string GetSubstring(string/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int count) {
            if (!MutableStringOps.NormalizeSubstringRange(self.Length, ref start, ref count)) {
                return (start == self.Length) ? self : null;
            }

            return self.Substring(start, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static string GetSubstring(ConversionStorage<int>/*!*/ fixnumCast, string/*!*/ self, [NotNull]Range/*!*/ range) {
            int begin, count;
            if (!MutableStringOps.NormalizeSubstringRange(fixnumCast, range, self.Length, out begin, out count)) {
                return null;
            }
            return (count < 0) ? self : GetSubstring(self, begin, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static string GetSubstring(string/*!*/ self, [NotNull]string/*!*/ searchStr) {
            return (self.IndexOf(searchStr, StringComparison.Ordinal) != -1) ? searchStr : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static string GetSubstring(RubyScope/*!*/ scope, string/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            if (regex.IsEmpty) {
                return String.Empty;
            }

            // TODO (opt): don't create a new mutable string:
            MatchData match = RegexpOps.Match(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
            if (match == null) {
                return null;
            }

            var result = match.GetValue();
            return result != null ? result.ToString() : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static string GetSubstring(RubyScope/*!*/ scope, string/*!*/ self, [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol]int occurrance) {
            if (regex.IsEmpty) {
                return String.Empty;
            }

            MatchData match = RegexpOps.Match(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
            if (match == null || !RegexpOps.NormalizeGroupIndex(ref occurrance, match.GroupCount)) {
                return null;
            }

            MutableString result = match.GetGroupValue(occurrance);
            return result != null ? result.ToString() : null;
        }

        #endregion

        #region TODO: casecmp, capitalize, downcase, swapcase, upcase

        #endregion

        #region TODO: center

        #endregion

        #region TODO: chomp, chop
        #endregion

        #region dump, inspect

        [RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Inspect(string/*!*/ self) {
            return MutableString.Create(
                MutableString.AppendUnicodeRepresentation(
                    new StringBuilder().Append('\''), self, MutableString.Escape.Special, '\'', -1
                ).Append('\'').ToString(),
                RubyEncoding.UTF8
            );
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Dump(string/*!*/ self) {
            return MutableString.Create(
                MutableString.AppendUnicodeRepresentation(
                    new StringBuilder().Append('\''), self, MutableString.Escape.Special | MutableString.Escape.NonAscii, '\'', -1
                ).Append('\'').ToString(), 
                RubyEncoding.UTF8
            );
        }

        #endregion

        #region TODO: each, each_byte, each_line

        #endregion

        #region empty?, size, length, encoding

        [RubyMethod("empty?")]
        public static bool IsEmpty(string/*!*/ self) {
            return self.Length == 0;
        }

        [RubyMethod("size")]
        public static int GetLength(string/*!*/ self) {
            return self.Length;
        }

        [RubyMethod("encoding")]
        public static RubyEncoding/*!*/ GetEncoding(string/*!*/ self) {
            return RubyEncoding.UTF8;
        }

        #endregion

        #region TODO: sub, gsub
        #endregion

        #region TODO: index, rindex


        #endregion

        #region TODO: delete
        #endregion

        #region TODO: count
        #endregion

        #region include?

        [RubyMethod("include?")]
        public static bool Include(string/*!*/ str, [DefaultProtocol, NotNull]string/*!*/ subString) {
            return str.IndexOf(subString, StringComparison.Ordinal) != -1;
        }

        #endregion

        #region insert

        [RubyMethod("insert")]
        public static string Insert(string/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol, NotNull]string/*!*/ value) {
            return self.Insert(MutableStringOps.NormalizeInsertIndex(start, self.Length), value);
        }

        #endregion

        #region =~, TODO: match

        [RubyMethod("=~")]
        public static object Match(RubyScope/*!*/ scope, string/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return RegexpOps.MatchIndex(scope, regex, MutableString.Create(self, RubyEncoding.UTF8));
        }

        [RubyMethod("=~")]
        public static object Match(string/*!*/ self, [NotNull]string/*!*/ str) {
            throw RubyExceptions.CreateTypeError("type mismatch: String given");
        }

        [RubyMethod("=~")]
        public static object Match(CallSiteStorage<Func<CallSite, RubyScope, object, string, object>>/*!*/ storage,
            RubyScope/*!*/ scope, string/*!*/ self, object obj) {
            var site = storage.GetCallSite("=~", new RubyCallSignature(1, RubyCallFlags.HasScope | RubyCallFlags.HasImplicitSelf));
            return site.Target(site, scope, obj, self);
        }

        #endregion

        #region TODO: scan
        #endregion

        #region TODO: succ
        #endregion

        #region split

        // TODO: return an array of CLR strings, not mutable strings

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, string/*!*/ self) {
            return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), (MutableString)null, 0);
        }

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, string/*!*/ self,
            [DefaultProtocol]string separator, [DefaultProtocol, Optional]int limit) {

            return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), MutableString.Create(separator, RubyEncoding.UTF8), limit);
        }

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, string/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regexp, [DefaultProtocol, Optional]int limit) {

            return MutableStringOps.Split(stringCast, MutableString.Create(self, RubyEncoding.UTF8), regexp, limit);
        }

        #endregion

        #region TODO: strip, lstrip, rstrip
        #endregion

        #region TODO: squeeze
        #endregion

        #region to_i, hex, oct

        [RubyMethod("to_i")]
        public static object/*!*/ ToInteger(string/*!*/ self, [DefaultProtocol, DefaultParameterValue(10)]int @base) {
            if (@base == 1 || @base < 0 || @base > 36) {
                throw RubyExceptions.CreateArgumentError("illegal radix {0}", @base);
            }
            return Tokenizer.ParseInteger(self, @base).ToObject();
        }

        [RubyMethod("hex")]
        public static object/*!*/ ToIntegerHex(string/*!*/ self) {
            return Tokenizer.ParseInteger(self, 16).ToObject();
        }

        [RubyMethod("oct")]
        public static object/*!*/ ToIntegerOctal(string/*!*/ self) {
            return Tokenizer.ParseInteger(self, 8).ToObject();
        }

        #endregion

        #region to_f, to_s, to_str, to_clr_string, to_sym, intern

        [RubyMethod("to_f")]
        public static double ToDouble(string/*!*/ self) {
            double result;
            bool complete;
            return Tokenizer.TryParseDouble(self, out result, out complete) ? result : 0.0;
        }

        [RubyMethod("to_str", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ ToStr(string/*!*/ self) {
            return MutableString.Create(self, RubyEncoding.UTF8);
        }

        [RubyMethod("to_clr_string", RubyMethodAttributes.PublicInstance)]
        public static string/*!*/ ToClrString(string/*!*/ self) {
            return self;
        }

        [RubyMethod("to_sym")]
        [RubyMethod("intern")]
        public static RubySymbol/*!*/ ToSymbol(RubyContext/*!*/ context, string/*!*/ self) {
            // Cannot convert a string that contains \0 to a symbol
            if (self.IndexOf('\0') >= 0) {
                throw RubyExceptions.CreateArgumentError("symbol string may not contain '\0'");
            }

            return context.CreateSymbol(self, RubyEncoding.UTF8);
        }

        #endregion

        #region TODO: upto
        #endregion

        #region replace, reverse

        // Ruby replace is a mutating operation, we fallback to System.String::Replace.

        [RubyMethod("reverse")]
        public static string/*!*/ GetReversed(string/*!*/ self) {
            // TODO: surrogates
            StringBuilder result = new StringBuilder(self.Length);
            result.Length = self.Length;
            for (int i = 0; i < self.Length; i++) {
                result[i] = self[self.Length - 1 - i];
            }

            return result.ToString();
        }

        #endregion

        #region TODO: tr, tr_s
        #endregion

        #region TODO: ljust
        #endregion

        #region TODO: rjust
        #endregion

        #region TODO: unpack
        #endregion

        #region TODO: sum
        #endregion

        #region TODO: Encodings (1.9)

        //ascii_only?
        //bytes
        //bytesize
        //chars
        //codepoints
        //each_byte
        //each_char
        //each_codepoint
        //encode
        //encoding
        //valid_esncoding?

        #endregion

        #region method_missing

        [RubyMethod("method_missing", RubyMethodAttributes.PrivateInstance)]
        [RubyStackTraceHidden]
        public static object MethodMissing(RubyScope/*!*/ scope, BlockParam block, string/*!*/ self, [NotNull]RubySymbol/*!*/ name, params object[]/*!*/ args) {
            if (name.EndsWith('=') || name.EndsWith('!')) {
                throw RubyExceptions.CreateTypeError("Mutating method `{0}' called for an immutable string (System::String)", name);
            }

            // TODO: forward to MutableString until we implement the methods here:
            return KernelOps.SendMessageOpt(scope, block, ToStr(self), name.ToString(), args);
        }

        #endregion
    }
}
