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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;

namespace IronRuby.Builtins {

    [RubyClass("String", Extends = typeof(MutableString), Inherits = typeof(Object))]
    [Includes(typeof(Enumerable), typeof(Comparable))]
    [HideMethod("clone")] // MutableString.Clone() would override Kernel#clone
    [HideMethod("version")] // TODO: MutableString.Version would override some spec's method
    public class MutableStringOps {

        [RubyConstructor]
        public static MutableString/*!*/ Create(RubyClass/*!*/ self) {
            return MutableString.CreateMutable();
        }
        
        [RubyConstructor]
        public static MutableString/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ value) {
            return MutableString.Create(value);
        }

        #region Helpers

        internal static int NormalizeIndex(MutableString/*!*/ str, int index) {
            return IListOps.NormalizeIndex(str.Length, index);
        }

        private static bool InExclusiveRangeNormalized(MutableString/*!*/ str, ref int index) {
            if (index < 0) {
                index = index + str.Length;
            }
            return index >= 0 && index < str.Length;
        }

        private static bool InInclusiveRangeNormalized(MutableString/*!*/ str, ref int index) {
            if (index < 0) {
                index = index + str.Length;
            }
            return index >= 0 && index <= str.Length;
        }

        // Parses interval strings that are of this form:
        //
        // abc         # abc
        // abc-efg-h   # abcdefgh
        // ^abc        # all characters in range 0-255 except abc
        // \x00-0xDD   # all characters in hex range
        public class IntervalParser {
            private readonly MutableString/*!*/ _range;
            private int _pos;
            private bool _rangeStarted;
            private int _startRange;

            public IntervalParser(MutableString/*!*/ range) {
                _range = range;
                _pos = 0;
                _rangeStarted = false;
            }

            public int PeekChar() {
                return _pos >= _range.Length ? -1 : _range.GetChar(_pos);
            }

            public int GetChar() {
                return _pos >= _range.Length ? -1 : _range.GetChar(_pos++);
            }

            public int NextToken() {
                int current = GetChar();

                if (current == '\\') {
                    int next = PeekChar();
                    switch (next) {
                        case 'x':
                            _pos++;
                            int digit1 = Tokenizer.ToDigit(GetChar());
                            int digit2 = Tokenizer.ToDigit(GetChar());

                            if (digit1 >= 16) {
                                throw RubyExceptions.CreateArgumentError("Invalid escape character syntax");
                            }

                            if (digit2 >= 16) {
                                current = digit1;
                            } else {
                                current = ((digit1 << 4) + digit2);
                            }
                            break;

                        case 't':
                            current = '\t';
                            break;

                        case 'n':
                            current = '\n';
                            break;

                        case 'r':
                            current = '\r';
                            break;

                        case 'v':
                            current = '\v';
                            break;

                        case '\\':
                            current = '\\';
                            break;

                        default:
                            break;
                    }
                }

                return current;
            }

            // TODO: refactor this and Parse()
            public MutableString/*!*/ ParseSequence() {
                _pos = 0;

                MutableString result = MutableString.CreateBinary();
                if (_range.Length == 0) {
                    return result;
                }

                bool negate = false;
                if (_range.GetChar(0) == '^') {
                    // Special case of ^
                    if (_range.Length == 1) {
                        result.Append('^');
                        return result;
                    }

                    negate = true;
                    _pos = 1;
                }

                BitArray array = new BitArray(256);
                array.Not();

                int c;
                while ((c = NextToken()) != -1) {
                    if (_rangeStarted) {
                        // _startRange - c. ignore ranges which are the reverse sequence
                        if (_startRange <= c) {
                            for (int i = _startRange; i <= c; ++i) {
                                if (negate) {
                                    array.Set(i, false);
                                } else {
                                    result.Append((byte)i);
                                }
                            }
                        }
                        _rangeStarted = false;
                    } else {
                        int p = PeekChar();
                        if (p == '-') {
                            // z- is treated as a literal 'z', '-'
                            if (_pos == _range.Length - 1) {
                                if (negate) {
                                    array.Set(c, false);
                                    array.Set('-', false);
                                } else {
                                    result.Append((byte)c);
                                    result.Append('-');
                                }
                                break;
                            }

                            _startRange = c;
                            if (_rangeStarted) {
                                if (negate) {
                                    array.Set('-', false);
                                } else {
                                    result.Append('-');
                                }
                                _rangeStarted = false;
                            } else {
                                _rangeStarted = true;
                            }
                            _pos++; // consume -
                        } else {
                            if (negate) {
                                array.Set(c, false);
                            } else {
                                result.Append((byte)c);
                            }
                        }
                    }
                }

                if (negate) {
                    for (int i = 0; i < 256; i++) {
                        if (array.Get(i)) {
                            result.Append((byte)i);
                        }
                    }
                }
                return result;
            }

            public BitArray/*!*/ Parse() {
                _pos = 0;

                BitArray result = new BitArray(256);
                if (_range.Length == 0) {
                    return result;
                }

                bool negate = false;
                if (_range.GetChar(0) == '^') {
                    // Special case of ^
                    if (_range.Length == 1) {
                        result.Set('^', true);
                        return result;
                    }

                    negate = true;
                    _pos = 1;
                    result.Not();
                }

                int c;
                while ((c = NextToken()) != -1) {
                    if (_rangeStarted) {
                        // _startRange - c. ignore ranges which are the reverse sequence
                        if (_startRange <= c) {
                            for (int i = _startRange; i <= c; ++i)
                                result.Set(i, !negate);
                        }
                        _rangeStarted = false;
                    } else {
                        int p = PeekChar();
                        if (p == '-') {
                            // z- is treated as a literal 'z', '-'
                            if (_pos == _range.Length - 1) {
                                result.Set(c, !negate);
                                result.Set('-', !negate);
                                break;
                            }

                            _startRange = c;
                            if (_rangeStarted) {
                                result.Set('-', !negate);
                                _rangeStarted = false;
                            } else {
                                _rangeStarted = true;
                            }
                            _pos++; // consume -
                        } else {
                            result.Set(c, !negate);
                        }
                    }
                }

                return result;
            }
        }

        public class RangeParser {

            private readonly MutableString[]/*!*/ _ranges;

            public RangeParser(params MutableString[]/*!*/ ranges) {
                ContractUtils.RequiresNotNull(ranges, "ranges");
                _ranges = ranges;
            }

            public BitArray Parse() {
                BitArray result = new IntervalParser(_ranges[0]).Parse();
                for (int i = 1; i < _ranges.Length; i++) {
                    result.And(new IntervalParser(_ranges[i]).Parse());
                }
                return result;
            }
        }

        #endregion


        #region initialize, initialize_copy

        // "initialize" not called when a factory/non-default ctor is called.
        // "initialize_copy" called from "dup" and "clone"
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static MutableString/*!*/ Reinitialize(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString other) {
            return Replace(self, other);
        }
        
        // Reinitialization. Not called when a factory/non-default ctor is called.
        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static MutableString/*!*/ Reinitialize(MutableString/*!*/ self) {
            return self;
        }

        #endregion


        #region %, *, +

        [RubyMethod("%")]
        public static MutableString/*!*/ Format(StringFormatterSiteStorage/*!*/ storage, RubyContext/*!*/ context, 
            MutableString/*!*/ self, object arg) {

            IList args = arg as IList ?? new object[] { arg };
            StringFormatter formatter = new StringFormatter(storage, context, self.ConvertToString(), args);
            return formatter.Format().TaintBy(self);
        }

        [RubyMethod("*")]
        public static MutableString/*!*/ Repeat(MutableString/*!*/ self, [DefaultProtocol]int times) {
            if (times < 0) {
                throw RubyExceptions.CreateArgumentError("negative argument");
            }

            MutableString result = self.CreateInstance().TaintBy(self);
            for (int i = 0; i < times; i++) {
                result.Append(self);
            }

            return result;
        }

        [RubyMethod("+")]
        public static MutableString/*!*/ Concatenate(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            // doesn't create a subclass:
            return MutableString.Create(self).Append(other).TaintBy(self).TaintBy(other);
        }

        #endregion

        #region <<, concat

        [RubyMethod("<<")]
        [RubyMethod("concat")]
        public static MutableString/*!*/ Append(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            return self.Append(other).TaintBy(other);
        }

        [RubyMethod("<<")]
        [RubyMethod("concat")]
        public static MutableString/*!*/ Append(MutableString/*!*/ self, int c) {
            if (c < 0 || c > 255) {
                throw RubyExceptions.CreateTypeConversionError("Fixnum", "String");
            }

            return self.Append((byte)c);
        }

        #endregion

        #region <=>, ==, ===

        [RubyMethod("<=>")]
        public static int Compare(MutableString/*!*/ self, [NotNull]MutableString/*!*/ other) {
            return Math.Sign(self.CompareTo(other));
        }

        [RubyMethod("<=>")]
        public static object Compare(BinaryOpStorage/*!*/ comparisonStorage, RespondToStorage/*!*/ respondToStorage, 
            RubyContext/*!*/ context, object/*!*/ self, object other) {
            // Self is object so that we can reuse this method.

            // We test to see if other responds to to_str AND <=>
            // Ruby never attempts to convert other to a string via to_str and call Compare ... which is strange -- feels like a BUG in Ruby

            if (Protocols.RespondTo(respondToStorage, context, other, "to_str") && Protocols.RespondTo(respondToStorage, context, other, "<=>")) {
                var site = comparisonStorage.GetCallSite("<=>");
                object result = Integer.TryUnaryMinus(site.Target(site, context, other, self));
                if (result == null) {
                    throw RubyExceptions.CreateTypeError(String.Format("{0} can't be coerced into Fixnum",
                        RubyUtils.GetClassName(context, result)));
                }

                return result;
            }

            return null;
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool StringEquals(MutableString/*!*/ lhs, [NotNull]MutableString/*!*/ rhs) {
            return lhs.Equals(rhs);
        }

        [RubyMethod("==")]
        [RubyMethod("===")]
        public static bool Equals(RespondToStorage/*!*/ respondToStorage, BinaryOpStorage/*!*/ equalsStorage,
            RubyContext/*!*/ context, object/*!*/ self, object other) {
            // Self is object so that we can reuse this method.

            if (!Protocols.RespondTo(respondToStorage, context, other, "to_str")) {
                return false;
            }

            var equals = equalsStorage.GetCallSite("==");
            return Protocols.IsTrue(equals.Target(equals, context, other, self));
        }

        #endregion


        #region slice!

        private static Group MatchRegexp(RubyScope/*!*/ scope, MutableString/*!*/ self, RubyRegex/*!*/ regex, int occurrance) {
            MatchData match = RegexpOps.Match(scope, regex, self); 
            if (match == null || !match.Success)
                return null;

            // Normalize index against # Groups in Match
            if (occurrance < 0) {
                occurrance += match.Groups.Count;
                // Cannot refer to zero using negative indices 
                if (occurrance == 0) {
                    return null;
                }
            }

            if (occurrance < 0 || occurrance > match.Groups.Count) {
                return null;
            }

            return match.Groups[occurrance].Success ? match.Groups[occurrance] : null;
        }

        [RubyMethod("slice!")]
        public static object RemoveCharInPlace(RubyContext/*!*/ context, MutableString/*!*/ self, 
            [DefaultProtocol]int index) {

            if (!InExclusiveRangeNormalized(self, ref index)) {
                return null;
            }

            // TODO: optimize if the value is not read:
            int result = self.GetByte(index);
            self.Remove(index, 1);
            return result;
        }

        [RubyMethod("slice!")]
        public static MutableString RemoveSubstringInPlace(MutableString/*!*/ self,
            [DefaultProtocol]int start, [DefaultProtocol]int length) {

            if (length < 0) {
                return null;
            }

            if (!InInclusiveRangeNormalized(self, ref start)) {
                return null;
            }

            if (start + length > self.Length) {
                length = self.Length - start;
            }

            MutableString result = self.CreateInstance().Append(self, start, length).TaintBy(self);
            self.Remove(start, length);
            return result;
        }

        [RubyMethod("slice!")]
        public static MutableString RemoveSubstringInPlace(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, 
            MutableString/*!*/ self, [NotNull]Range/*!*/ range) {
            int begin = Protocols.CastToFixnum(fixnumCast, context, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, context, range.End);

            if (!InInclusiveRangeNormalized(self, ref begin)) {
                return null;
            }

            end = NormalizeIndex(self, end);

            int count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return count < 0 ? self.CreateInstance() : RemoveSubstringInPlace(self, begin, count);
        }

        [RubyMethod("slice!")]
        public static MutableString RemoveSubstringInPlace(RubyScope/*!*/ scope, MutableString/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            if (regex.IsEmpty) {
                return self.Clone().TaintBy(regex, scope);
            }

            MatchData match = RegexpOps.Match(scope, regex, self);
            if (match == null || !match.Success) {
                return null;
            }

            return RemoveSubstringInPlace(self, match.Index, match.Length).TaintBy(regex, scope);
        }

        [RubyMethod("slice!")]
        public static MutableString RemoveSubstringInPlace(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol]int occurrance) {

            if (regex.IsEmpty) {
                return self.Clone().TaintBy(regex, scope);
            }

            Group group = MatchRegexp(scope, self, regex, occurrance);
            return group == null ? null : RemoveSubstringInPlace(self, group.Index, group.Length).TaintBy(regex, scope);
        }

        [RubyMethod("slice!")]
        public static MutableString RemoveSubstringInPlace(MutableString/*!*/ self, [NotNull]MutableString/*!*/ searchStr) {
            if (searchStr.IsEmpty) {
                return searchStr.Clone();
            }

            int index = self.IndexOf(searchStr);
            if (index < 0) {
                return null;
            }

            RemoveSubstringInPlace(self, index, searchStr.Length);
            return searchStr.Clone();
        }

        #endregion

        #region [], slice

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static object GetChar(MutableString/*!*/ self, [DefaultProtocol]int index) {
            return InExclusiveRangeNormalized(self, ref index) ? (object)(int)self.GetByte(index) : null;
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static MutableString GetSubstring(MutableString/*!*/ self, 
            [DefaultProtocol]int start, [DefaultProtocol]int length) {

            start = NormalizeIndex(self, start);

            if (start == self.Length) {
                return self.CreateInstance().TaintBy(self);
            }

            if (start < 0 || start > self.Length || length < 0) {
                return null;
            }

            if (start + length > self.Length) {
                length = self.Length - start;
            }

            return self.CreateInstance().Append(self, start, length).TaintBy(self);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static MutableString GetSubstring(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, MutableString/*!*/ self, 
            [NotNull]Range/*!*/ range) {
            int begin = Protocols.CastToFixnum(fixnumCast, context, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, context, range.End);

            begin = NormalizeIndex(self, begin);
            if (begin < 0 || begin > self.Length) {
                return null;
            }

            end = NormalizeIndex(self, end);

            int count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return (count < 0) ? self.CreateInstance().TaintBy(self) : GetSubstring(self, begin, count);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static MutableString GetSubstring(MutableString/*!*/ self, [NotNull]MutableString/*!*/ searchStr) {
            if (self.IndexOf(searchStr) != -1) {
                return searchStr.Clone();
            } else {
                return null;
            }
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static MutableString GetSubstring(RubyScope/*!*/ scope, MutableString/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            if (regex.IsEmpty) {
                return self.CreateInstance().TaintBy(self).TaintBy(regex, scope);
            }

            MatchData match = RegexpOps.Match(scope, regex, self);
            if (match == null) {
                return null;
            }

            string result = match.Value;
            return (result.Length == 0) ? null : self.CreateInstance().TaintBy(self).Append(result).TaintBy(regex, scope);
        }

        [RubyMethod("[]")]
        [RubyMethod("slice")]
        public static MutableString GetSubstring(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol]int occurrance) {
            if (regex.IsEmpty) {
                return self.CreateInstance().TaintBy(self).TaintBy(regex, scope);
            }

            Group group = MatchRegexp(scope, self, regex, occurrance);
            if (group == null || !group.Success) {
                return null;
            }

            return self.CreateInstance().Append(group.Value).TaintBy(self).TaintBy(regex, scope);
        }

        #endregion

        #region []=

        [RubyMethod("[]=")]
        public static MutableString/*!*/ ReplaceCharacter(MutableString/*!*/ self,
            [DefaultProtocol]int index, [DefaultProtocol, NotNull]MutableString/*!*/ value) {

            index = index < 0 ? index + self.Length : index;
            if (index < 0 || index >= self.Length) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of string", index));
            }

            if (value.IsEmpty) {
                self.Remove(index, 1).TaintBy(value);
                return MutableString.CreateMutable();
            }

            self.Replace(index, 1, value).TaintBy(value);
            return value;
        }

        [RubyMethod("[]=")]
        public static int SetCharacter(MutableString/*!*/ self, 
            [DefaultProtocol]int index, int value) {

            index = index < 0 ? index + self.Length : index;
            if (index < 0 || index >= self.Length) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of string", index));
            }

            self.SetByte(index, unchecked((byte)value));
            return value;
        }

        [RubyMethod("[]=")]
        public static MutableString/*!*/ ReplaceSubstring(MutableString/*!*/ self, 
            [DefaultProtocol]int start, [DefaultProtocol]int charsToOverwrite, [DefaultProtocol, NotNull]MutableString/*!*/ value) {
            
            if (charsToOverwrite < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("negative length {0}", charsToOverwrite));
            }

            if (System.Math.Abs(start) > self.Length) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of string", start));
            }

            start = start < 0 ? start + self.Length : start;

            if (charsToOverwrite <= value.Length) {
                int insertIndex = start + charsToOverwrite;
                int limit = charsToOverwrite;
                if (insertIndex > self.Length) {
                    limit -= insertIndex - self.Length;
                    insertIndex = self.Length;
                }

                self.Replace(start, limit, value);
            } else {
                self.Replace(start, value.Length, value);

                int pos = start + value.Length;
                int charsToRemove = charsToOverwrite - value.Length;
                int charsLeftInString = self.Length - pos;

                self.Remove(pos, System.Math.Min(charsToRemove, charsLeftInString));
            }

            self.TaintBy(value);
            return value;
        }

        [RubyMethod("[]=")]
        public static MutableString/*!*/ ReplaceSubstring(ConversionStorage<int>/*!*/ fixnumCast, RubyContext/*!*/ context, MutableString/*!*/ self, 
            [NotNull]Range/*!*/ range, [DefaultProtocol, NotNull]MutableString/*!*/ value) {

            int begin = Protocols.CastToFixnum(fixnumCast, context, range.Begin);
            int end = Protocols.CastToFixnum(fixnumCast, context, range.End);

            begin = begin < 0 ? begin + self.Length : begin;

            if (begin < 0 || begin > self.Length) {
                throw RubyExceptions.CreateRangeError(String.Format("{0}..{1} out of range", begin, end));
            }

            end = end < 0 ? end + self.Length : end;

            int count = range.ExcludeEnd ? end - begin : end - begin + 1;
            return ReplaceSubstring(self, begin, count, value);
        }

        [RubyMethod("[]=")]
        public static MutableString ReplaceSubstring(MutableString/*!*/ self,
            [NotNull]MutableString/*!*/ substring, [DefaultProtocol, NotNull]MutableString/*!*/ value) {

            int index = self.IndexOf(substring);
            if (index == -1) {
                throw RubyExceptions.CreateIndexError("string not matched");
            }

            return ReplaceSubstring(self, index, substring.Length, value);
        }

        [RubyMethod("[]=")]
        public static MutableString ReplaceSubstring(MutableString/*!*/ self,
            [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol, NotNull]MutableString/*!*/ value) {
            
            Match match = regex.Match(self);
            if (!match.Success) {
                throw RubyExceptions.CreateIndexError("regexp not matched");
            }

            return ReplaceSubstring(self, match.Index, match.Length, value);
        }

        #endregion

        #region casecmp, capitalize, capitalize!, downcase, downcase!, swapcase, swapcase!, upcase, upcase!

        public static bool UpCaseChar(MutableString/*!*/ self, int index) {
            char current = self.GetChar(index);
            if (current >= 'a' && current <= 'z') {
                self.SetChar(index, Char.ToUpper(current));
                return true;
            }
            return false;
        }

        public static bool DownCaseChar(MutableString/*!*/ self, int index) {
            char current = self.GetChar(index);
            if (current >= 'A' && current <= 'Z') {
                self.SetChar(index, Char.ToLower(current));
                return true;
            }
            return false;
        }

        public static bool SwapCaseChar(MutableString/*!*/ self, int index) {
            char current = self.GetChar(index);
            if (current >= 'A' && current <= 'Z') {
                self.SetChar(index, Char.ToLower(current));
                return true;
            } else if (current >= 'a' && current <= 'z') {
                self.SetChar(index, Char.ToUpper(current));
                return true;
            }
            return false;
        }

        public static bool CapitalizeMutableString(MutableString/*!*/ str) {
            bool changed = false;
            if (str.Length > 0) {
                if (UpCaseChar(str, 0)) {
                    changed = true;
                }
                for (int i = 1; i < str.Length; ++i) {
                    if (DownCaseChar(str, i)) {
                        changed = true;
                    }
                }
            }
            return changed;
        }

        public static bool UpCaseMutableString(MutableString/*!*/ str) {
            bool changed = false;
            for (int i = 0; i < str.Length; ++i) {
                if (UpCaseChar(str, i)) {
                    changed = true;
                }
            }
            return changed;
        }

        public static bool DownCaseMutableString(MutableString/*!*/ str) {
            bool changed = false;
            for (int i = 0; i < str.Length; ++i) {
                if (DownCaseChar(str, i)) {
                    changed = true;
                }
            }
            return changed;
        }

        public static bool SwapCaseMutableString(MutableString/*!*/ str) {
            bool changed = false;
            for (int i = 0; i < str.Length; ++i) {
                if (SwapCaseChar(str, i)) {
                    changed = true;
                }
            }
            return changed;
        }

        [RubyMethod("casecmp")]
        public static int Casecmp(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            return Compare(DownCase(self), DownCase(other));
        }

        [RubyMethod("capitalize")]
        public static MutableString/*!*/ Capitalize(MutableString/*!*/ self) {
            MutableString result = self.Clone();
            CapitalizeMutableString(result);
            return result;
        }

        [RubyMethod("capitalize!")]
        public static MutableString CapitalizeInPlace(MutableString/*!*/ self) {
            return CapitalizeMutableString(self) ? self : null;
        }

        [RubyMethod("downcase")]
        public static MutableString/*!*/ DownCase(MutableString/*!*/ self) {
            MutableString result = self.Clone();
            DownCaseMutableString(result);
            return result;
        }

        [RubyMethod("downcase!")]
        public static MutableString DownCaseInPlace(MutableString/*!*/ self) {
            return DownCaseMutableString(self) ? self : null;
        }

        [RubyMethod("swapcase")]
        public static MutableString/*!*/ SwapCase(MutableString/*!*/ self) {
            MutableString result = self.Clone();
            SwapCaseMutableString(result);
            return result;
        }

        [RubyMethod("swapcase!")]
        public static MutableString SwapCaseInPlace(MutableString/*!*/ self) {
            return SwapCaseMutableString(self) ? self : null;
        }

        [RubyMethod("upcase")]
        public static MutableString/*!*/ UpCase(MutableString/*!*/ self) {
            MutableString result = self.Clone();
            UpCaseMutableString(result);
            return result;
        }

        [RubyMethod("upcase!")]
        public static MutableString UpCaseInPlace(MutableString/*!*/ self) {
            return UpCaseMutableString(self) ? self : null;
        }

        #endregion


        #region center

        private static readonly MutableString _DefaultPadding = MutableString.Create(" ").Freeze();

        [RubyMethod("center")]
        public static MutableString/*!*/ Center(MutableString/*!*/ self, 
            [DefaultProtocol]int length,
            [Optional, DefaultProtocol]MutableString padding) {

            if (padding != null && padding.Length == 0) {
                throw RubyExceptions.CreateArgumentError("zero width padding");
            }

            if (self.Length >= length) {
                return self;
            }

            if (padding == null) {
                padding = _DefaultPadding;
            }

            char[] charArray = new char[length];
            int n = (length - self.Length) / 2;

            for (int i = 0; i < n; i++) {
                charArray[i] = padding.GetChar(i % padding.Length);
            }

            for (int i = 0; i < self.Length; i++) {
                charArray[n + i] = self.GetChar(i);
            }

            int m = length - self.Length - n;
            for (int i = 0; i < m; i++) {
                charArray[n + self.Length + i] = padding.GetChar(i % padding.Length);
            }

            return self.CreateInstance().Append(new String(charArray)).TaintBy(self).TaintBy(padding); 
        }

        #endregion


        #region chomp, chomp!, chop, chop!

        private static bool EndsWith(MutableString/*!*/ str, MutableString/*!*/ terminator) {
            int offset = str.Length - terminator.Length;
            if (offset < 0) {
                return false;
            }

            if (str.IsBinary) {
                for (int i = 0; i < terminator.Length; i++) {
                    if (str.GetChar(offset + i) != terminator.GetChar(i)) {
                        return false;
                    }
                }
            } else {
                for (int i = 0; i < terminator.Length; i++) {
                    if (str.GetByte(offset + i) != terminator.GetByte(i)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static MutableString/*!*/ ChompTrailingCarriageReturns(MutableString/*!*/ str, bool removeCarriageReturnsToo) {
            int end = str.Length;
            while (true) {
                if (end > 1) {
                    if (str.GetChar(end - 1) == '\n') {
                        end -= str.GetChar(end - 2) == '\r' ? 2 : 1;
                    } else if (removeCarriageReturnsToo && str.GetChar(end - 1) == '\r') {
                        end -= 1;
                    }
                    else {
                        break;
                    }
                } else if (end > 0) {
                    if (str.GetChar(end - 1) == '\n' || str.GetChar(end - 1) == '\r') {
                        end -= 1;
                    }
                    break;
                } else {
                    break;
                }
            }
            return str.GetSlice(0, end);
        }

        private static MutableString InternalChomp(MutableString/*!*/ self, MutableString separator) {
            if (separator == null) {
                return self.Clone();
            }

            // Remove multiple trailing CR/LFs
            if (separator.Length == 0) {
                return ChompTrailingCarriageReturns(self, false).TaintBy(self);
            }

            // Remove single trailing CR/LFs
            MutableString result = self.Clone();
            int length = result.Length;
            if (separator.Length == 1 && separator.GetChar(0) == '\n') {
                if (length > 1 && result.GetChar(length - 2) == '\r' && result.GetChar(length - 1) == '\n') {
                    result.Remove(length - 2, 2);
                } else if (length > 0 && (self.GetChar(length - 1) == '\n' || result.GetChar(length - 1) == '\r')) {
                    result.Remove(length - 1, 1);
                }
            } else if (EndsWith(result, separator)) {
                result.Remove(length - separator.Length, separator.Length);
            }

            return result;
        }

        [RubyMethod("chomp")]
        public static MutableString/*!*/ Chomp(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return InternalChomp(self, context.InputSeparator);
        }

        [RubyMethod("chomp")]
        public static MutableString/*!*/ Chomp(MutableString/*!*/ self, [DefaultProtocol]MutableString separator) {
            return InternalChomp(self, separator);
        }

        [RubyMethod("chomp!")]
        public static MutableString/*!*/ ChompInPlace(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return ChompInPlace(self, context.InputSeparator);
        }

        [RubyMethod("chomp!")]
        public static MutableString ChompInPlace(MutableString/*!*/ self, [DefaultProtocol]MutableString separator) {
            MutableString result = InternalChomp(self, separator);

            if (result.Equals(self) || result == null) {
                return null;
            }

            self.Clear();
            self.Append(result);
            return self;
        }

        private static MutableString/*!*/ ChopInteral(MutableString/*!*/ self) {
            if (self.Length == 1 || self.GetChar(self.Length - 2) != '\r' || self.GetChar(self.Length - 1) != '\n') {
                self.Remove(self.Length - 1, 1);
            } else {
                self.Remove(self.Length - 2, 2);
            }
            return self;
        }

        [RubyMethod("chop!")]
        public static MutableString ChopInPlace(MutableString/*!*/ self) {
            if (self.Length == 0) return null;
            return ChopInteral(self);
        }

        [RubyMethod("chop")]
        public static MutableString/*!*/ Chop(MutableString/*!*/ self) {
            return (self.Length == 0) ? self.CreateInstance().TaintBy(self) : ChopInteral(self.Clone());
        }

        #endregion


        #region dump, inspect

        public static string/*!*/ GetQuotedStringRepresentation(MutableString/*!*/ self, RubyContext/*!*/ context, bool forceEscapes, char quote) {
            return self.AppendRepresentation(
                new StringBuilder().Append(quote), 
                context.RubyOptions.Compatibility == RubyCompatibility.Ruby18, 
                forceEscapes,
                quote
            ).Append(quote).ToString();
        }

        // encoding aware
        [RubyMethod("dump")]
        public static MutableString/*!*/ Dump(RubyContext/*!*/ context, MutableString/*!*/ self) {
            // Note that "self" could be a subclass of MutableString, and the return value should be
            // of the same type
            return self.CreateInstance().Append(GetQuotedStringRepresentation(self, context, true, '"')).TaintBy(self);
        }

        // encoding aware
        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, MutableString/*!*/ self) {
            // Note that "self" could be a subclass of MutableString, but the return value should 
            // always be just a MutableString
            return MutableString.Create(GetQuotedStringRepresentation(self, context, false, '"'), self.Encoding).TaintBy(self);
        }

        #endregion

        #region each, each_byte, each_line

        [RubyMethod("each_byte")]
        public static object EachByte(BlockParam block, MutableString/*!*/ self) {
            if (block == null && self.Length > 0) {
                throw RubyExceptions.NoBlockGiven();
            }

            int i = 0;
            while (i < self.Length) {
                object result;
                if (block.Yield((int)self.GetByte(i), out result)) {
                    return result;
                }
                i++;
            }
            return self;
        }

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object EachLine(RubyContext/*!*/ context, BlockParam block, MutableString/*!*/ self) {
            return EachLine(block, self, context.InputSeparator);
        }

        private static readonly MutableString _DefaultLineSeparator = MutableString.Create("\n").Freeze();
        private static readonly MutableString _DefaultDoubleLineSeparator = MutableString.Create("\n\n").Freeze();

        [RubyMethod("each")]
        [RubyMethod("each_line")]
        public static object EachLine(BlockParam block, MutableString/*!*/ self, [DefaultProtocol]MutableString/*!*/ separator) {
            if (separator == null) {
                separator = MutableString.Empty;
            }

            uint version = self.Version;

            MutableString paragraphSeparator;
            if (separator.IsEmpty) {
                separator = _DefaultLineSeparator;
                paragraphSeparator = _DefaultDoubleLineSeparator;
            } else {
                paragraphSeparator = null;
            }

            // TODO: this is slow, refactor when we redo MutableString
            MutableString str = self;
            int start = 0;

            // In "normal" mode just split the string at the end of each seperator occurrance.
            // In "paragraph" mode, split the string at the end of each occurrance of two or more
            // successive seperators.
            while (start < self.Length) {
                int end;
                if (paragraphSeparator == null) {
                    end = str.IndexOf(separator, start);
                    if (end >= 0) {
                        end += separator.Length;
                    } else {
                        end = str.Length;
                    }
                } else {
                    end = str.IndexOf(paragraphSeparator, start);
                    if (end >= 0) {
                        end += (2 * separator.Length);
                        while (str.IndexOf(separator, end) == end) {
                            end += separator.Length;
                        }
                    } else {
                        end = str.Length;
                    }
                }

                // Yield the current line
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                object result;
                MutableString line = self.CreateInstance().TaintBy(self).Append(str, start, end - start);
                if (block.Yield(line, out result)) {
                    return result;
                }

                start = end;
            }

            // Ensure that the underlying string has not been mutated during the iteration
            RequireNoVersionChange(version, self);
            return self;
        }

        #endregion

        #region empty?, size, length, encoding

        // encoding aware
        [RubyMethod("empty?")]
        public static bool IsEmpty(MutableString/*!*/ self) {
            return self.IsEmpty;
        }

        // encoding aware
        [RubyMethod("size")]
        [RubyMethod("length")]
        public static int GetLength(MutableString/*!*/ self) {
            return (self.Encoding.IsKCoding) ? self.GetByteCount() : self.GetCharCount();
        }

        // encoding aware
        [RubyMethod("encoding")]
        public static RubyEncoding/*!*/ GetEncoding(MutableString/*!*/ self) {
            return self.Encoding;
        }

        #endregion


        #region sub, gsub

        // returns true if block jumped
        // "result" will be null if there is no successful match
        private static bool BlockReplaceFirst(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, MutableString/*!*/ input, BlockParam/*!*/ block,
            RubyRegex/*!*/ pattern, out object blockResult, out MutableString result) {

            var matchScope = scope.GetInnerMostClosureScope();
            MatchData match = RegexpOps.Match(scope, pattern, input);
            if (match == null || !match.Success) {
                result = null;
                blockResult = null;
                matchScope.CurrentMatch = null;
                return false;
            }

            // copy upfront so that no modifications to the input string are included in the result:
            result = input.Clone();
            matchScope.CurrentMatch = match;

            if (block.Yield(MutableString.Create(match.Value), out blockResult)) {
                return true;
            }

            // resets the $~ scope variable to the last match (skipped if block jumped):
            matchScope.CurrentMatch = match;

            MutableString replacement = Protocols.ConvertToString(tosConversion, scope.RubyContext, blockResult);
            result.TaintBy(replacement);

            // Note - we don't interpolate special sequences like \1 in block return value
            result.Replace(match.Index, match.Length, replacement);

            blockResult = null;
            return false;
        }
        
        // returns true if block jumped
        // "result" will be null if there is no successful match
        private static bool BlockReplaceAll(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, MutableString/*!*/ input, BlockParam/*!*/ block,
            RubyRegex/*!*/ regex, out object blockResult, out MutableString result) {

            var matchScope = scope.GetInnerMostClosureScope();

            MatchCollection matches = regex.Matches(input);
            if (matches.Count == 0) {
                result = null;
                blockResult = null;
                matchScope.CurrentMatch = null;
                return false;
            }

            // create an empty result:
            result = input.CreateInstance().TaintBy(input);
            
            int offset = 0;
            foreach (Match match in matches) {
                MatchData currentMatch = new MatchData(match, input);
                matchScope.CurrentMatch = currentMatch;

                uint version = input.Version;
                if (block.Yield(MutableString.Create(match.Value), out blockResult)) {
                    return true;
                }
                if (input.Version != version) {
                    return false;
                }

                // resets the $~ scope variable to the last match (skipd if block jumped):
                matchScope.CurrentMatch = currentMatch;

                MutableString replacement = Protocols.ConvertToString(tosConversion, scope.RubyContext, blockResult);
                result.TaintBy(replacement);

                // prematch:
                result.Append(input, offset, match.Index - offset);

                // replacement (unlike ReplaceAll, don't interpolate special sequences like \1 in block return value):
                result.Append(replacement);

                offset = match.Index + match.Length;
            }

            // post-last-match:
            result.Append(input, offset, input.Length - offset);

            blockResult = null;
            return false;
        }

        private static void AppendBackslashes(int backslashCount, MutableString/*!*/ result, int minBackslashes) {
            for (int j = 0; j < ((backslashCount - 1) >> 1) + minBackslashes; j++) {
                result.Append('\\');
            }
        }

        private static void AppendReplacementExpression(MutableString input, GroupCollection/*!*/ groups, MutableString/*!*/ result, MutableString/*!*/ replacement) {
            int backslashCount = 0;
            for (int i = 0; i < replacement.Length; i++) {
                char c = replacement.GetChar(i);
                if (c == '\\')
                    backslashCount++;
                else if (backslashCount == 0)
                    result.Append(c);
                else {
                    AppendBackslashes(backslashCount, result, 0);
                    // Odd number of \'s + digit means insert replacement expression
                    if ((backslashCount & 1) == 1) {
                        if (Char.IsDigit(c)) {
                            AppendGroupByIndex(groups, c - '0', backslashCount, result);
                        } else if (c == '&') {
                            AppendGroupByIndex(groups, groups.Count - 1, backslashCount, result);
                        } else if (c == '`') {
                            // Replace with everything in the input string BEFORE the match
                            result.Append(input, 0, groups[0].Index);
                        } else if (c == '\'') {
                            // Replace with everything in the input string AFTER the match
                            int start = groups[0].Index + groups[0].Length;
                            result.Append(input, start, input.Length - start);
                        } else if (c == '+') {
                            // Replace last character in last successful match group
                            AppendLastCharOfLastMatchGroup(groups, result);
                        } else {
                            // unknown escaped replacement char, go ahead and replace untouched
                            result.Append('\\');
                            result.Append(c);
                        }
                    } else {
                        // Any other # of \'s or a non-digit character means insert literal \'s and character
                        AppendBackslashes(backslashCount, result, 1);
                        result.Append(c);
                    }
                    backslashCount = 0;
                }
            }
            AppendBackslashes(backslashCount, result, 1);
        }

        private static void AppendLastCharOfLastMatchGroup(GroupCollection groups, MutableString result) {
            int i = groups.Count - 1;
            // move to last successful match group
            while (i > 0 && !groups[i].Success) {
                i--;
            }

            if (i > 0 && groups[i].Value.Length > 0) {
                result.Append(groups[i].Value[groups[i].Length - 1]);
            }
        }

        private static void AppendGroupByIndex(GroupCollection/*!*/ groups, int index, int backslashCount, MutableString/*!*/ result) {
            if (groups[index].Success) {
                result.Append(groups[index].Value);
            }
        }

        private static void AppendReplacementExpression(MutableString/*!*/ input, MatchData/*!*/ match, MutableString/*!*/ result, MutableString/*!*/ replacement) {
            AppendReplacementExpression(input, match.Groups, result, replacement);
        }

        // TODO: we have two overloads right now because we haven't unified Matches to return a List<MatchData> yet ...
        private static void AppendReplacementExpression(MutableString/*!*/ input, Match/*!*/ match, MutableString/*!*/ result, MutableString/*!*/ replacement) {
            AppendReplacementExpression(input, match.Groups, result, replacement);
        }

        private static MutableString ReplaceFirst(RubyScope/*!*/ scope, MutableString/*!*/ input, MutableString/*!*/ replacement, RubyRegex/*!*/ pattern) {
            MatchData match = RegexpOps.Match(scope, pattern, input);
            if (match == null || !match.Success) {
                return null;
            }

            MutableString result = input.CreateInstance().TaintBy(input).TaintBy(replacement);
            
            // prematch:
            result.Append(input, 0, match.Index);

            AppendReplacementExpression(input, match, result, replacement);

            // postmatch:
            int offset = match.Index + match.Length;
            result.Append(input, offset, input.Length - offset);

            return result;
        }

        private static MutableString ReplaceAll(RubyScope/*!*/ scope, MutableString/*!*/ input, MutableString/*!*/ replacement, 
            RubyRegex/*!*/ regex) {
            var matchScope = scope.GetInnerMostClosureScope();
            
            // case of all
            MatchCollection matches = regex.Matches(input);
            if (matches.Count == 0) {
                matchScope.CurrentMatch = null;
                return null;
            }

            MutableString result = input.CreateInstance().TaintBy(input).TaintBy(replacement);

            int offset = 0;
            foreach (Match match in matches) {
                result.Append(input, offset, match.Index - offset);
                AppendReplacementExpression(input, match, result, replacement);
                offset = match.Index + match.Length;
            }

            result.Append(input, offset, input.Length - offset);

            matchScope.CurrentMatch = new MatchData(matches[matches.Count - 1], input);
            return result;
        }

        [RubyMethod("sub")]
        public static object BlockReplaceFirst(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ pattern) {

            object blockResult;
            MutableString result;
            return BlockReplaceFirst(tosConversion, scope, self, block, pattern, out blockResult, out result) ? blockResult : (result ?? self.Clone());
        }
        
        [RubyMethod("gsub")]
        public static object BlockReplaceAll(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self, 
            [NotNull]RubyRegex pattern) {

            object blockResult;
            MutableString result;
            uint version = self.Version;
            object r = BlockReplaceAll(tosConversion, scope, self, block, pattern, out blockResult, out result) ? blockResult : (result ?? self.Clone());

            RequireNoVersionChange(version, self);
            return r;
        }

        [RubyMethod("sub")]
        public static object BlockReplaceFirst(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self, 
            [NotNull]MutableString matchString) {

            object blockResult;
            MutableString result;
            var regex = new RubyRegex(Regex.Escape(matchString.ToString()), RubyRegexOptions.NONE);

            return BlockReplaceFirst(tosConversion, scope, self, block, regex, out blockResult, out result) ? blockResult : (result ?? self.Clone());
        }

        [RubyMethod("gsub")]
        public static object BlockReplaceAll(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self, 
            [NotNull]MutableString matchString) {

            object blockResult;
            MutableString result;
            var regex = new RubyRegex(Regex.Escape(matchString.ToString()), RubyRegexOptions.NONE);

            uint version = self.Version;
            object r = BlockReplaceAll(tosConversion, scope, self, block, regex, out blockResult, out result) ? blockResult : (result ?? self.Clone());
            RequireNoVersionChange(version, self);
            return r;
        }

        [RubyMethod("sub")]
        public static MutableString ReplaceFirst(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern, [DefaultProtocol, NotNull]MutableString/*!*/ replacement) {

            return ReplaceFirst(scope, self, replacement, pattern) ?? self.Clone();
        }

        [RubyMethod("gsub")]
        public static MutableString ReplaceAll(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern, [DefaultProtocol, NotNull]MutableString/*!*/ replacement) {

            return ReplaceAll(scope, self, replacement, pattern) ?? self.Clone();
        }

        #endregion


        #region sub!, gsub!

        private static object BlockReplaceInPlace(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, BlockParam/*!*/ block, MutableString/*!*/ self, 
            RubyRegex/*!*/ pattern, bool replaceAll) {

            object blockResult;

            uint version = self.Version;

            // prepare replacement in a builder:
            MutableString builder;
            if (replaceAll ?
                BlockReplaceAll(tosConversion, scope, self, block, pattern, out blockResult, out builder) :
                BlockReplaceFirst(tosConversion, scope, self, block, pattern, out blockResult, out builder)) {

                // block jumped:
                return blockResult;
            }

            // unsuccessful match:
            if (builder == null) {
                return null;
            }

            RequireNoVersionChange(version, self);

            if (self.IsFrozen) {
                throw new RuntimeError("string frozen");
            }

            // replace content of self with content of the builder:
            self.Replace(0, self.Length, builder);
            return self.TaintBy(builder);
        }

        private static MutableString ReplaceInPlace(RubyScope/*!*/ scope, MutableString/*!*/ self, RubyRegex/*!*/ pattern,
            MutableString/*!*/ replacement, bool replaceAll) {
            
            MutableString builder = replaceAll ?
                ReplaceAll(scope, self, replacement, pattern) :
                ReplaceFirst(scope, self, replacement, pattern);
            
            // unsuccessful match:
            if (builder == null) {
                return null;
            }

            self.Replace(0, self.Length, builder);
            return self.TaintBy(builder);
        }

        [RubyMethod("sub!")]
        public static object BlockReplaceFirstInPlace(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self,
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern) {

            return BlockReplaceInPlace(tosConversion, scope, block, self, pattern, false);
        }

        [RubyMethod("gsub!")]
        public static object BlockReplaceAllInPlace(ConversionStorage<MutableString>/*!*/ tosConversion, 
            RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self,
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern) {

            return BlockReplaceInPlace(tosConversion, scope, block, self, pattern, true);
        }

        [RubyMethod("sub!")]
        public static MutableString ReplaceFirstInPlace(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern, [DefaultProtocol, NotNull]MutableString/*!*/ replacement) {

            return ReplaceInPlace(scope, self, pattern, replacement, false);
        }

        [RubyMethod("gsub!")]
        public static MutableString ReplaceAllInPlace(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull]RubyRegex/*!*/ pattern, [DefaultProtocol, NotNull]MutableString/*!*/ replacement) {

            return ReplaceInPlace(scope, self, pattern, replacement, true);
        }

        #endregion


        #region index, rindex

        [RubyMethod("index")]
        public static object Index(MutableString/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ substring, [DefaultProtocol, Optional]int start) {

            if (!NormalizeStart(self, ref start)) {
                return null;
            }
            int result = self.IndexOf(substring, start);
            return (result != -1) ? ScriptingRuntimeHelpers.Int32ToObject(result) : null;
        }

        [RubyMethod("index")]
        public static object Index(MutableString/*!*/ self, 
            int character, [DefaultProtocol, Optional]int start) {

            if (!NormalizeStart(self, ref start) || character < 0 || character > 255) {
                return null;
            }
            int result = self.IndexOf((byte)character, start);
            return (result != -1) ? ScriptingRuntimeHelpers.Int32ToObject(result) : null;
        }

        [RubyMethod("index")]
        public static object Index(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol, Optional]int start) {

            if (!NormalizeStart(self, ref start)) {
                return null;
            }

            var matchScope = scope.GetInnerMostClosureScope();

            Match match = regex.Match(self, start);
            if (match.Success) {
                matchScope.CurrentMatch = new MatchData(match, self);
                return ScriptingRuntimeHelpers.Int32ToObject(match.Index);
            } else {
                matchScope.CurrentMatch = null;
                return null;
            }
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ substring) {
            return ReverseIndex(self, substring, self.Length);
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ substring, [DefaultProtocol]int start) {
            start = NormalizeIndex(self, start);

            if (start < 0) {
                return null;
            }

            if (substring.IsEmpty) {
                return ScriptingRuntimeHelpers.Int32ToObject((start >= self.Length) ? self.Length : start);
            }

            start += substring.Length - 1;

            if (start >= self.Length) {
                start = self.Length - 1;
            }

            int result = self.LastIndexOf(substring, start);
            return (result != -1) ? ScriptingRuntimeHelpers.Int32ToObject(result) : null;
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(MutableString/*!*/ self,
            int character, [DefaultProtocol, DefaultParameterValue(-1)]int start) {

            start = NormalizeIndex(self, start);
            if (start < 0 || character < 0 || character > 255) {
                return null;
            }

            if (start >= self.Length) {
                start = self.Length - 1;
            }
            
            int result = self.LastIndexOf((byte)character, start);
            return (result != -1) ? ScriptingRuntimeHelpers.Int32ToObject(result) : null;
        }

        // TODO: note that .NET regex semantics don't line up well in these cases - so some specs do fail. There are 4 failures in rindex that are due to regex differences.

        [RubyMethod("rindex")]
        public static object ReverseIndex(RubyScope/*!*/ scope, MutableString/*!*/ self,
            [NotNull]RubyRegex/*!*/ regex) {
            return ReverseIndex(scope, self, regex, self.Length);
        }

        [RubyMethod("rindex")]
        public static object ReverseIndex(RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regex, [DefaultProtocol]int start) {

            start = NormalizeIndex(self, start);
            if (start < 0) {
                return null;
            }
            start = (start > self.Length) ? self.Length : start;

            var matchScope = scope.GetInnerMostClosureScope();
            
            Match match = regex.ReverseMatch(self, start);
            if (match.Success) {
                matchScope.CurrentMatch = new MatchData(match, self);
                return ScriptingRuntimeHelpers.Int32ToObject(match.Index);
            } else {
                matchScope.CurrentMatch = null;
                return null;
            }
        }

        // Start in range ==> search range from the first character towards the end.
        //
        // [-length, 0)     ==> [0, length + start]
        // start < -length  ==> false
        // [0, length)      ==> [start, length)
        // start > length   ==> false
        //
        private static bool NormalizeStart(MutableString/*!*/ self, ref int start) {
            start = NormalizeIndex(self, start);
            if (start < 0 || start > self.Length) {
                return false;
            }
            return true;
        }

        #endregion


        #region delete, delete!

        private static MutableString/*!*/ InternalDelete(MutableString/*!*/ self, MutableString[]/*!*/ ranges) {
            BitArray map = new RangeParser(ranges).Parse();
            MutableString result = self.CreateInstance().TaintBy(self);
            for (int i = 0; i < self.Length; i++) {
                if (!map.Get(self.GetChar(i))) {
                    result.Append(self.GetChar(i));
                }
            }
            return result;
        }

        private static MutableString/*!*/ InternalDeleteInPlace(MutableString/*!*/ self, MutableString[]/*!*/ ranges) {
            MutableString result = InternalDelete(self, ranges);
            if (self.Equals(result)) {
                return null;
            }

            self.Clear();
            self.Append(result);
            return self;
        }

        [RubyMethod("delete")]
        public static MutableString/*!*/ Delete(RubyContext/*!*/ context, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ strs) {
            if (strs.Length == 0) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            return InternalDelete(self, strs);
        }

        [RubyMethod("delete!")]
        public static MutableString/*!*/ DeleteInPlace(RubyContext/*!*/ context, MutableString/*!*/ self,
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ strs) {
            if (strs.Length == 0) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            return InternalDeleteInPlace(self, strs);
        }

        #endregion

        #region count
        
        private static object InternalCount(MutableString/*!*/ self, MutableString[]/*!*/ ranges) {
            BitArray map = new RangeParser(ranges).Parse();
            int count = 0;
            for (int i = 0; i < self.Length; i++) {
                if (map.Get(self.GetChar(i)))
                    count++;
            }
            return ScriptingRuntimeHelpers.Int32ToObject(count);
        }

        [RubyMethod("count")]
        public static object Count(RubyContext/*!*/ context, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ strs) {
            if (strs.Length == 0) {
                throw RubyExceptions.CreateArgumentError("wrong number of arguments");
            }
            return InternalCount(self, strs);
        }

        #endregion

        #region include?

        [RubyMethod("include?")]
        public static bool Include(MutableString/*!*/ str, [DefaultProtocol, NotNull]MutableString/*!*/ subString) {
            return str.IndexOf(subString) != -1;
        }

        [RubyMethod("include?")]
        public static bool Include(MutableString/*!*/ str, int c) {
            return str.IndexOf((byte)(c % 256)) != -1;
        }

        #endregion


        #region insert

        [RubyMethod("insert")]
        public static MutableString Insert(MutableString/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol, NotNull]MutableString/*!*/ value) {
            int offset = start < 0 ? start + self.Length + 1 : start;
            if (offset > self.Length || offset < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of string", start));
            }

            return self.Insert(offset, value).TaintBy(value);
        }

        #endregion


        #region match

        [RubyMethod("=~")]
        public static object Match(RubyScope/*!*/ scope, MutableString/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            return RegexpOps.MatchIndex(scope, regex, self);
        }

        [RubyMethod("=~")]
        public static object Match(MutableString/*!*/ self, [NotNull]MutableString/*!*/ str) {
            throw RubyExceptions.CreateTypeError("type mismatch: String given");
        }

        [RubyMethod("=~")]
        public static object Match(CallSiteStorage<Func<CallSite, RubyContext, object, MutableString, object>>/*!*/ storage,
            RubyScope/*!*/ scope, MutableString/*!*/ self, object obj) {
            var site = storage.GetCallSite("=~", 1);
            return site.Target(site, scope.RubyContext, obj, self);
        }

        [RubyMethod("match")]
        public static object MatchRegexp(CallSiteStorage<Func<CallSite, RubyScope, RubyRegex, MutableString, object>>/*!*/ storage, 
            RubyScope/*!*/ scope, MutableString/*!*/ self, [NotNull]RubyRegex/*!*/ regex) {
            var site = storage.GetCallSite("match", new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasScope));
            return site.Target(site, scope, regex, self);
        }

        [RubyMethod("match")]
        public static object MatchObject(CallSiteStorage<Func<CallSite, RubyScope, RubyRegex, MutableString, object>>/*!*/ storage, 
            RubyScope/*!*/ scope, MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ pattern) {
            var site = storage.GetCallSite("match", new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf | RubyCallFlags.HasScope));
            return site.Target(site, scope, new RubyRegex(pattern, RubyRegexOptions.NONE), self);
        }
       
        #endregion


        #region scan

        [RubyMethod("scan")]
        public static RubyArray/*!*/ Scan(RubyScope/*!*/ scope, MutableString/*!*/ self, [DefaultProtocol, NotNull]RubyRegex/*!*/ regex) {
            MatchCollection matches = regex.Matches(self);

            var matchScope = scope.GetInnerMostClosureScope();
            
            RubyArray result = new RubyArray(matches.Count);
            if (matches.Count == 0) {
                matchScope.CurrentMatch = null;
                return result;
            } 

            foreach (Match match in matches) {
                result.Add(MatchToScanResult(scope, self, regex, match));
            }

            matchScope.CurrentMatch = new MatchData(matches[matches.Count - 1], self);
            return result;
        }

        [RubyMethod("scan")]
        public static object/*!*/ Scan(RubyScope/*!*/ scope, [NotNull]BlockParam/*!*/ block, MutableString/*!*/ self, [DefaultProtocol, NotNull]RubyRegex regex) {
            var matchScope = scope.GetInnerMostClosureScope();
            
            MatchCollection matches = regex.Matches(self);
            if (matches.Count == 0) {
                matchScope.CurrentMatch = null;
                return self;
            } 

            foreach (Match match in matches) {
                var currentMatch = new MatchData(match, self);

                matchScope.CurrentMatch = currentMatch;

                object blockResult;
                if (block.Yield(MatchToScanResult(scope, self, regex, match), out blockResult)) {
                    return blockResult;
                }

                // resets the $~ scope variable to the last match (skipd if block jumped):
                matchScope.CurrentMatch = currentMatch;
            }
            return self;
        }

        private static object MatchToScanResult(RubyScope/*!*/ scope, MutableString/*!*/ self, RubyRegex/*!*/ regex, Match/*!*/ match) {
            if (match.Groups.Count == 1) {
                return MutableString.Create(match.Value).TaintBy(self).TaintBy(regex, scope);
            } else {
                var result = new RubyArray(match.Groups.Count - 1);
                for (int i = 1; i < match.Groups.Count; i++) {
                    if (match.Groups[i].Success) {
                        result.Add(MutableString.Create(match.Groups[i].Value).TaintBy(self).TaintBy(regex, scope));
                    } else {
                        result.Add(null);
                    }
                }
                return result;
            }
        }

        #endregion


        #region succ, succ!

        public static int GetIndexOfRightmostAlphaNumericCharacter(MutableString/*!*/ str, int index) {
            for (int i = index; i >= 0; --i)
                if (Char.IsLetterOrDigit(str.GetChar(i)))
                    return i;

            return -1;
        }

        // TODO: remove recursion
        public static void IncrementAlphaNumericChar(MutableString/*!*/ str, int index) {
            char c = str.GetChar(index);
            if (c == 'z' || c == 'Z' || c == '9') {
                int nextIndex = GetIndexOfRightmostAlphaNumericCharacter(str, index - 1);
                if (c == 'z') {
                    str.SetChar(index, 'a');
                    if (nextIndex == -1)
                        str.Insert(index, "a");
                    else
                        IncrementAlphaNumericChar(str, nextIndex);
                } else if (c == 'Z') {
                    str.SetChar(index, 'A');
                    if (nextIndex == -1)
                        str.Insert(index, "A");
                    else
                        IncrementAlphaNumericChar(str, nextIndex);
                } else {
                    str.SetChar(index, '0');
                    if (nextIndex == -1)
                        str.Insert(index, "1");
                    else
                        IncrementAlphaNumericChar(str, nextIndex);
                }
            } else {
                IncrementChar(str, index);
            }
        }

        public static void IncrementChar(MutableString/*!*/ str, int index) {
            byte c = str.GetByte(index);
            if (c == 255) {
                str.SetByte(index, 0);
                if (index > 0) {
                    IncrementChar(str, index - 1);
                } else {
                    str.Insert(0, 1);
                }
            } else {
                str.SetByte(index, unchecked((byte)(c + 1)));
            }
        }

        [RubyMethod("succ!")]
        [RubyMethod("next!")]
        public static MutableString/*!*/ SuccInPlace(MutableString/*!*/ self) {
            if (self.IsEmpty) {
                return self;
            }

            int index = GetIndexOfRightmostAlphaNumericCharacter(self, self.Length - 1);
            if (index == -1) {
                IncrementChar(self, self.Length - 1);
            } else {
                IncrementAlphaNumericChar(self, index);
            }

            return self;
        }

        [RubyMethod("succ")]
        [RubyMethod("next")]
        public static MutableString/*!*/ Succ(MutableString/*!*/ self) {
            return SuccInPlace(self.Clone());
        }

        #endregion


        #region split

        private static RubyArray/*!*/ MakeRubyArray(MutableString/*!*/ self, MutableString[]/*!*/ elements) {
            return MakeRubyArray(self, elements, 0, elements.Length);
        }

        private static RubyArray/*!*/ MakeRubyArray(MutableString/*!*/ self, MutableString[]/*!*/ elements, int start, int count) {
            RubyArray result = new RubyArray(elements.Length);
            for (int i = 0; i < count; i++) {
                result.Add(self.CreateInstance().Append(elements[start + i]).TaintBy(self));
            }
            return result;
        }

        // The IndexOf and InternalSplit helper methods are necessary because Ruby semantics of these methods
        // differ from .NET semantics. IndexOf("") returns the next character, which is reflected in the 
        // InternalSplit method which also flows taint

        private static int IndexOf(MutableString/*!*/ str, MutableString/*!*/ separator, int index) {
            if (separator.Length > 0)
                return str.IndexOf(separator, index);
            else
                return index + 1;
        }

        private static RubyArray/*!*/ WhitespaceSplit(MutableString/*!*/ self, int maxComponents) {
            char[] separators = new char[] { ' ', '\n', '\r', '\t', '\v' };
            MutableString[] elements = self.Split(separators, (maxComponents < 0) ? Int32.MaxValue : maxComponents, StringSplitOptions.RemoveEmptyEntries);

            RubyArray result = new RubyArray(); 
            foreach (MutableString element in elements) {
                result.Add(self.CreateInstance().Append(element).TaintBy(self));
            }

            // Strange behavior to match Ruby semantics
            if (maxComponents < 0) {
                result.Add(self.CreateInstance().TaintBy(self));
            }

            return result;
        }

        private static RubyArray/*!*/ InternalSplit(MutableString/*!*/ self, MutableString separator, StringSplitOptions options, int maxComponents) {
            if (separator == null || separator.Length == 1 && separator.GetChar(0) == ' ') {
                return WhitespaceSplit(self, maxComponents);
            }

            if (maxComponents <= 0) {
                maxComponents = Int32.MaxValue;
            }

            RubyArray result = new RubyArray(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);
            bool keepEmpty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            int selfLength = self.Length;
            int i = 0;
            int next;
            while (maxComponents > 1 && i < selfLength && (next = IndexOf(self, separator, i)) != -1) {

                if (next > i || keepEmpty) {
                    result.Add(self.CreateInstance().Append(self, i, next - i).TaintBy(self));
                    maxComponents--;
                }

                i = next + separator.Length;
            }

            if (i < selfLength || keepEmpty) {
                result.Add(self.CreateInstance().Append(self, i, selfLength - i).TaintBy(self));
            }

            return result;
        }

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, RubyScope/*!*/ scope, MutableString/*!*/ self) {
            return Split(stringCast, scope, self, (MutableString)null, 0);
        }

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [DefaultProtocol]MutableString separator, [DefaultProtocol, Optional]int limit) {

            if (separator == null) {
                object defaultSeparator = scope.RubyContext.StringSeparator;
                RubyRegex regexSeparator = defaultSeparator as RubyRegex;
                if (regexSeparator != null) {
                    return Split(stringCast, scope, self, regexSeparator, limit);
                }
                
                if (defaultSeparator != null) {
                    separator = Protocols.CastToString(stringCast, scope.RubyContext, defaultSeparator);
                }
            }

            if (limit == 0) {
                // suppress trailing empty fields
                RubyArray array = InternalSplit(self, separator, StringSplitOptions.None, Int32.MaxValue);
                while (array.Count != 0 && ((MutableString)array[array.Count - 1]).Length == 0) {
                    array.RemoveAt(array.Count - 1);
                }
                return array;
            } else if (limit == 1) {
                // returns an array with original string
                RubyArray result = new RubyArray(1);
                result.Add(self);
                return result;
            } else {
                return InternalSplit(self, separator, StringSplitOptions.None, limit);
            }
        }

        [RubyMethod("split")]
        public static RubyArray/*!*/ Split(ConversionStorage<MutableString>/*!*/ stringCast, RubyScope/*!*/ scope, MutableString/*!*/ self, 
            [NotNull]RubyRegex/*!*/ regexp, [DefaultProtocol, Optional]int limit) {
            
            if (regexp.IsEmpty) {
                return Split(stringCast, scope, self, MutableString.Empty, limit);
            }

            if (limit == 0) {
                // suppress trailing empty fields
                RubyArray array = MakeRubyArray(self, regexp.Split(self));
                while (array.Count != 0 && ((MutableString)array[array.Count - 1]).Length == 0) {
                    array.RemoveAt(array.Count - 1);
                }
                return array;
            } else if (limit == 1) {
                // returns an array with original string
                RubyArray result = new RubyArray(1);
                result.Add(self);
                return result;
            } else if (limit < 0) {
                // does not suppress trailing fields when negative 
                return MakeRubyArray(self, regexp.Split(self));
            } else {
                // limit > 1 limits to N fields
                return MakeRubyArray(self, regexp.Split(self, limit));
            }
        }

        #endregion


        #region strip, strip!, lstrip, lstrip!, rstrip, rstrip!

        [RubyMethod("strip")]
        public static MutableString/*!*/ Strip(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return Strip(self, true, true);
        }

        [RubyMethod("lstrip")]
        public static MutableString/*!*/ StripLeft(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return Strip(self, true, false);
        }

        [RubyMethod("rstrip")]
        public static MutableString/*!*/ StripRight(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return Strip(self, false, true);
        }

        [RubyMethod("strip!")]
        public static MutableString StripInPlace(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return StripInPlace(self, true, true);
        }

        [RubyMethod("lstrip!")]
        public static MutableString StripLeftInPlace(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return StripInPlace(self, true, false);
        }

        [RubyMethod("rstrip!")]
        public static MutableString StripRightInPlace(RubyContext/*!*/ context, MutableString/*!*/ self) {
            return StripInPlace(self, false, true);
        }
        
        private static MutableString/*!*/ Strip(MutableString/*!*/ str, bool trimLeft, bool trimRight) {
            int left, right;
            GetTrimRange(str, trimLeft, trimRight, out left, out right);
            return str.GetSlice(left, right - left).TaintBy(str);
        }

        public static MutableString StripInPlace(MutableString/*!*/ self, bool trimLeft, bool trimRight) {
            int left, right;
            GetTrimRange(self, trimLeft, trimRight, out left, out right);
            int remaining = right - left;

            // nothing to trim:
            if (remaining == self.Length) {
                return null;
            }

            if (remaining == 0) {
                // all whitespace
                self.Clear();
            } else {
                self.Trim(left, remaining);
            }
            return self;
        }

        private static void GetTrimRange(MutableString/*!*/ str, bool left, bool right, out int leftIndex, out int rightIndex) {
            GetTrimRange(
                str.Length,
                !left ? (Func<int, bool>)null : (i) => Char.IsWhiteSpace(str.GetChar(i)),
                !right ? (Func<int, bool>)null : (i) => {
                    char c = str.GetChar(i);
                    return Char.IsWhiteSpace(c) || c == '\0';
                },
                out leftIndex, 
                out rightIndex
            );
        }

        // Returns indices of the first non-whitespace character (from left and from right).
        // ensures (leftIndex == rightIndex) ==> all characters are whitespace
        // leftIndex == 0, rightIndex == length if there is no whitespace to be trimmed.
        internal static void GetTrimRange(int length, Func<int, bool> trimLeft, Func<int, bool> trimRight, out int leftIndex, out int rightIndex) {
            int i;
            if (trimLeft != null) {
                i = 0;
                while (i < length) {
                    if (!trimLeft(i)) {
                        break;
                    }
                    i++;
                }
            } else {
                i = 0;
            }

            int j;
            if (trimRight != null) {
                j = length - 1;
                // we need to compare i-th character again as it could be treated as right whitespace but not as left whitespace:
                while (j >= i) {
                    if (!trimRight(j)) {
                        break;
                    }
                    j--;
                }

                // should point right after the non-whitespace character:
                j++;
            } else {
                j = length;
            }

            leftIndex = i;
            rightIndex = j;

            Debug.Assert(leftIndex >= 0 && leftIndex <= length);
            Debug.Assert(rightIndex >= 0 && rightIndex <= length);
        }

        #endregion


        #region squeeze, squeeze!

        [RubyMethod("squeeze")]
        public static MutableString/*!*/ Squeeze(RubyContext/*!*/ context, MutableString/*!*/ self, 
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ args) {
            MutableString result = self.Clone();
            SqueezeMutableString(result, args);
            return result;
        }

        [RubyMethod("squeeze!")]
        public static MutableString/*!*/ SqueezeInPlace(RubyContext/*!*/ context, MutableString/*!*/ self,
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ args) {
            return SqueezeMutableString(self, args);
        }

        private static MutableString SqueezeMutableString(MutableString/*!*/ str, MutableString[]/*!*/ ranges) {
            // if squeezeAll is true then there should be no ranges, and vice versa
            Assert.NotNull(str, ranges);

            // convert the args into a map of characters to be squeezed (same algorithm as count)
            BitArray map = null;
            if (ranges.Length > 0) {
                map = new RangeParser(ranges).Parse();
            }

            // Do the squeeze in place
            int j = 1, k = 1;
            while (j < str.Length) {
                if (str.GetChar(j) == str.GetChar(j-1) && (ranges.Length == 0 || map.Get(str.GetChar(j)))) {
                    j++;
                } else {
                    str.SetChar(k, str.GetChar(j));
                    j++; k++;
                }
            }
            if (j > k) {
                str.Remove(k, j - k);
            }

            // if not modified return null
            return j == k ? null : str;
        }

        #endregion


        #region to_i, hex, oct

        [RubyMethod("to_i")]
        public static object/*!*/ ToInteger(MutableString/*!*/ self, [DefaultProtocol, DefaultParameterValue(10)]int @base) {
            return ClrString.ToInteger(self.ConvertToString(), @base);
        }

        [RubyMethod("hex")]
        public static object/*!*/ ToIntegerHex(MutableString/*!*/ self) {
            return ClrString.ToIntegerHex(self.ConvertToString());
        }

        [RubyMethod("oct")]
        public static object/*!*/ ToIntegerOctal(MutableString/*!*/ self) {
            return ClrString.ToIntegerOctal(self.ConvertToString());
        }

        #endregion

        #region to_f, to_s, to_str, to_clr_string, to_sym, intern

        [RubyMethod("to_f")]
        public static double ToDouble(MutableString/*!*/ self) {
            return ClrString.ToDouble(self.ConvertToString());
        }

        [RubyMethod("to_s")]
        [RubyMethod("to_str")]
        public static MutableString/*!*/ ToS(MutableString/*!*/ self) {
            return self.GetType() == typeof(MutableString) ? self : MutableString.Create(self).TaintBy(self);
        }

        [RubyMethod("to_clr_string")]
        public static string/*!*/ ToClrString(MutableString/*!*/ str) {
            return str.ConvertToString();
        }

        
        [RubyMethod("to_sym")]
        [RubyMethod("intern")]
        public static SymbolId ToSymbol(MutableString/*!*/ self) {
            return ClrString.ToSymbol(self.ConvertToString());
        }

        #endregion


        #region upto

        [RubyMethod("upto")]
        public static object UpTo(
            ConversionStorage<MutableString>/*!*/ stringCast, 
            RespondToStorage/*!*/ respondToStorage,
            BinaryOpStorage/*!*/ comparisonStorage,
            BinaryOpStorage/*!*/ lessThanStorage,
            BinaryOpStorage/*!*/ greaterThanStorage,
            BinaryOpStorage/*!*/ equalsStorage,
            UnaryOpStorage/*!*/ succStorage,
            RubyContext/*!*/ context, BlockParam block, MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ endString) {

            RangeOps.Each(stringCast, respondToStorage, comparisonStorage, lessThanStorage, greaterThanStorage, equalsStorage, succStorage, context, 
                block, new Range(self, endString, false)
            );

            return self;
        }

        #endregion


        #region replace, reverse, reverse!

        [RubyMethod("replace")]
        public static MutableString/*!*/ Replace(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ other) {
            // Handle case where objects are the same identity
            if (ReferenceEquals(self, other)) {
                return self;
            }

            self.Clear();
            self.Append(other);
            return self.TaintBy(other);
        }

        [RubyMethod("reverse")]
        public static MutableString/*!*/ GetReversed(MutableString/*!*/ self) {
            return self.Clone().Reverse();
        }

        [RubyMethod("reverse!")]
        public static MutableString/*!*/ Reverse(MutableString/*!*/ self) {
            return self.Reverse();
        }

        #endregion


        #region tr, tr_s

        private static MutableString/*!*/ TrInternal(MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ from,
            [DefaultProtocol, NotNull]MutableString/*!*/ to, bool squeeze) {

            MutableString result = self.CreateInstance().TaintBy(self);
            IntervalParser parser = new IntervalParser(from);

            // TODO: a single pass to generate both?
            MutableString source = parser.ParseSequence();
            BitArray bitmap = parser.Parse();

            MutableString dest = new IntervalParser(to).ParseSequence();

            int lastChar = dest.GetLastChar();
            char? lastTranslated = null;
            for (int i = 0; i < self.Length; i++) {
                char c = self.GetChar(i);
                if (bitmap.Get(c)) {
                    char? thisTranslated = null;
                    int index = source.IndexOf(c);
                    if (index >= dest.Length) {
                        if (lastChar != -1) {
                            thisTranslated = (char)lastChar;
                        }
                    } else {
                        thisTranslated = dest.GetChar(index);
                    }
                    if (thisTranslated != null && (!squeeze || lastTranslated == null || lastTranslated.Value != thisTranslated)) {
                        result.Append(thisTranslated.Value);
                    }
                    lastTranslated = thisTranslated;
                } else {
                    result.Append(c);
                    lastTranslated = null;
                }
            }

            return result;
        }

        [RubyMethod("tr")]
        public static MutableString/*!*/ Tr(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ from, [DefaultProtocol, NotNull]MutableString/*!*/ to) {

            return TrInternal(self, from, to, false);
        }

        [RubyMethod("tr!")]
        public static MutableString/*!*/ TrInPlace(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ from, [DefaultProtocol, NotNull]MutableString/*!*/ to) {

            MutableString result = TrInternal(self, from, to, false);
            if (self.Equals(result)) {
                return null;
            }

            self.Clear();
            self.Append(result);
            return self;
        }

        [RubyMethod("tr_s")]
        public static MutableString/*!*/ TrSqueeze(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ from, [DefaultProtocol, NotNull]MutableString/*!*/ to) {

            return TrInternal(self, from, to, true);
        }

        [RubyMethod("tr_s!")]
        public static MutableString/*!*/ TrSqueezeInPlace(MutableString/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ from, [DefaultProtocol, NotNull]MutableString/*!*/ to) {

            MutableString result = TrInternal(self, from, to, true);
            if (self.Equals(result)) {
                return null;
            }

            self.Clear();
            self.Append(result);
            return self;
        }

        #endregion

        
        #region ljust

        [RubyMethod("ljust")]
        public static MutableString/*!*/ LeftJustify(MutableString/*!*/ self, [DefaultProtocol]int width) {
            // TODO: is this correct? Is it just a space or is this some configurable whitespace thing?
            return LeftJustify(self, width, _DefaultPadding);
        }

        [RubyMethod("ljust")]
        public static MutableString/*!*/ LeftJustify(MutableString/*!*/ self, 
            [DefaultProtocol]int width, [DefaultProtocol, NotNull]MutableString/*!*/ padding) {

            if (padding.Length == 0) {
                throw RubyExceptions.CreateArgumentError("zero width padding");
            }

            int count = width - self.Length;
            if (count <= 0) {
                return self;
            }

            int iterations = count / padding.Length;
            int remainder = count % padding.Length;
            MutableString result = self.Clone().TaintBy(padding);

            for (int i = 0; i < iterations; i++) {
                result.Append(padding);
            }

            result.Append(padding, 0, remainder);

            return result;
        }

        #endregion

        #region rjust

        [RubyMethod("rjust")]
        public static MutableString/*!*/ RightJustify(MutableString/*!*/ self, [DefaultProtocol]int width) {
            // TODO: is this correct? Is it just a space or is this some configurable whitespace thing?
            return RightJustify(self, width, _DefaultPadding);
        }

        [RubyMethod("rjust")]
        public static MutableString/*!*/ RightJustify(MutableString/*!*/ self, 
            [DefaultProtocol]int width, [DefaultProtocol, NotNull]MutableString/*!*/ padding) {

            if (padding.Length == 0) {
                throw RubyExceptions.CreateArgumentError("zero width padding");
            }

            int count = width - self.Length;
            if (count <= 0) {
                return self;
            }

            int iterations = count / padding.Length;
            int remainder = count % padding.Length;
            MutableString result = self.CreateInstance().TaintBy(self).TaintBy(padding);

            for (int i = 0; i < iterations; i++) {
                result.Append(padding);
            }

            result.Append(padding.GetSlice(0, remainder));
            result.Append(self);

            return result;
        }

        #endregion


        #region unpack

        private static bool HasCapacity(Stream/*!*/ s, int? n) {
            if (s.Length < (s.Position + n)) {
                s.Position = s.Length;
                return false;
            } else {
                return true;
            }
        }

        private static int CalculateCounts(Stream/*!*/ s, int? count, int size, out int leftover) {
            int remaining = (int)(s.Length - s.Position);
            int maxCount = remaining / size;
            if (!count.HasValue) {
                leftover = 0;
                return maxCount;
            } else if (count.Value <= maxCount) {
                leftover = 0;
                return count.Value;
            } else {
                leftover = count.Value - maxCount;
                return maxCount;
            }
        }

        [RubyMethod("unpack")]
        public static RubyArray/*!*/ Unpack(RubyContext/*!*/ context, MutableString/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ format) {
            RubyArray result = new RubyArray(1 + self.Length / 2);
            using (MutableStringStream stream = new MutableStringStream(self)) {
                BinaryReader reader = new BinaryReader(stream);
                foreach (ArrayOps.FormatDirective directive in ArrayOps.FormatDirective.Enumerate(format.ToString())) {
                    int count, maxCount;
                    byte[] buffer;
                    MutableString str;
                    int nilCount = 0;
                    switch (directive.Directive) {
                        case '@':
                            stream.Position = directive.Count.HasValue ? directive.Count.Value : stream.Position;
                            break;

                        case 'A':
                        case 'a':
                            maxCount = (int)(stream.Length - stream.Position);
                            count = directive.Count.HasValue ? directive.Count.Value : maxCount;
                            if (count > maxCount) {
                                count = maxCount;
                            }
                            buffer = reader.ReadBytes(count);
                            str = MutableString.CreateBinary(buffer);
                            if (directive.Directive == 'A') {
                                // TODO: efficiency?
                                for (int pos = count - 1; pos >= 0; pos--) {
                                    if (buffer[pos] != 0 && buffer[pos] != 0x20) {
                                        break;
                                    }
                                    str.Remove(pos, 1);
                                }
                            }
                            result.Add(str);
                            break;

                        case 'Z':
                            maxCount = (int)(stream.Length - stream.Position);
                            count = directive.Count.HasValue ? directive.Count.Value : maxCount;
                            if (count > maxCount) {
                                count = maxCount;
                            }
                            buffer = reader.ReadBytes(count);
                            str = MutableString.CreateBinary(buffer);
                            for (int pos = 0; pos < count; pos++) {
                                if (buffer[pos] == 0) {
                                    str.Remove(pos, count - pos);
                                    break;
                                }
                            }
                            result.Add(str);
                            break;

                        case 'c':
                            count = CalculateCounts(stream, directive.Count, sizeof(sbyte), out nilCount);
                            for (int j = 0; j < count; j++) {
                                result.Add((int)reader.ReadSByte());
                            }
                            break;

                        case 'C':
                            count = CalculateCounts(stream, directive.Count, sizeof(byte), out nilCount);
                            for (int j = 0; j < count; j++) {
                                result.Add((int)reader.ReadByte());
                            }
                            break;

                        case 'i':
                        case 'l':
                            count = CalculateCounts(stream, directive.Count, sizeof(int), out nilCount);
                            for (int j = 0; j < count; j++) {
                                result.Add((int)reader.ReadInt32());
                            }
                            break;

                        case 'I':
                        case 'L':
                            count = CalculateCounts(stream, directive.Count, sizeof(uint), out nilCount);
                            for (int j = 0; j < count; j++) {
                                uint value = reader.ReadUInt32();
                                if (value <= Int32.MaxValue) {
                                    result.Add((int)value);
                                } else {
                                    result.Add((BigInteger)value);
                                }
                            }
                            break;

                        case 'm':
                            // TODO: Recognize "==" as end of base 64 encoding
                            int len = (int)(stream.Length - stream.Position);
                            char[] base64 = reader.ReadChars(len);
                            byte[] data = Convert.FromBase64CharArray(base64, 0, len);
                            result.Add(MutableString.CreateBinary(data));
                            break;

                        case 's':
                            count = CalculateCounts(stream, directive.Count, sizeof(short), out nilCount);
                            for (int j = 0; j < count; j++) {
                                result.Add((int)reader.ReadInt16());
                            }
                            break;

                        case 'S':
                            count = CalculateCounts(stream, directive.Count, sizeof(ushort), out nilCount);
                            for (int j = 0; j < count; j++) {
                                result.Add((int)reader.ReadUInt16());
                            }
                            break;

                        case 'U':
                            maxCount = (int)(stream.Length - stream.Position);
                            count = directive.Count.HasValue ? directive.Count.Value : maxCount;
                            int readCount = directive.Count.HasValue ? Encoding.UTF8.GetMaxByteCount(count) : count;
                            if (readCount > maxCount) {
                                readCount = maxCount;
                            }
                            long startPosition = stream.Position;
                            char[] charData = Encoding.UTF8.GetChars(reader.ReadBytes(readCount));
                            if (charData.Length > count) {
                                int actualCount = Encoding.UTF8.GetByteCount(charData, 0, count);
                                stream.Position = startPosition + actualCount;
                            } else if (charData.Length < count) {
                                count = charData.Length;
                            }
                            for (int j = 0; j < count; j++) {
                                result.Add((int)charData[j]);
                            }
                            break;

                        case 'X':
                            int len3 = directive.Count.HasValue ? directive.Count.Value : 0;
                            if (len3 > stream.Position) {
                                throw RubyExceptions.CreateArgumentError("X outside of string");
                            }
                            stream.Position -= len3;
                            break;

                        case 'x':
                            int len4 = directive.Count.HasValue ? directive.Count.Value : 0;
                            stream.Position += len4;
                            break;

                        case 'h':
                        case 'H':
                            maxCount = (int)(stream.Length - stream.Position) * 2;
                            result.Add(ToHex(reader, Math.Min(directive.Count ?? maxCount, maxCount), directive.Directive == 'h'));
                            break;

                        default:
                            throw RubyExceptions.CreateArgumentError(
                                String.Format("Unknown format directive '{0}'", directive.Directive));
                    }
                    for (int i = 0; i < nilCount; i++) {
                        result.Add(null);
                    }
                }
            }
            return result;
        }

        private static MutableString/*!*/ ToHex(BinaryReader/*!*/ reader, int nibbleCount, bool swap) {
            int wholeChars = nibbleCount / 2;
            MutableString hex = MutableString.CreateMutable(nibbleCount, RubyEncoding.Binary);

            for (int i = 0; i < wholeChars; i++) {
                byte b = reader.ReadByte();
                char loNibble = (b & 0x0F).ToLowerHexDigit();
                char hiNibble = ((b & 0xF0) >> 4).ToLowerHexDigit();

                if (swap) {
                    hex.Append(loNibble);
                    hex.Append(hiNibble);
                } else {
                    hex.Append(hiNibble);
                    hex.Append(loNibble);
                }
            }

            // the last nibble:
            if ((nibbleCount & 1) != 0) {
                int b = reader.ReadByte();
                if (swap) {
                    hex.Append((b & 0x0F).ToLowerHexDigit());
                } else {
                    hex.Append(((b & 0xF0) >> 4).ToLowerHexDigit());
                }
            }

            return hex;
        }

        #endregion

        #region sum

        [RubyMethod("sum")]
        public static object GetChecksum(MutableString/*!*/ self, [DefaultProtocol, DefaultParameterValue(16)]int bitCount) {
            int length = self.GetByteCount();
            uint mask = (bitCount > 31) ? 0xffffffff : (1U << bitCount) - 1;
            uint sum = 0;
            for (int i = 0; i < length; i++) {
                byte b = self.GetByte(i);
                try {
                    checked { sum = (sum + b) & mask; }
                } catch (OverflowException) {
                    return GetBigChecksum(self, i, sum, bitCount);
                }
            }

            return (sum > Int32.MaxValue) ? (BigInteger)sum : (object)(int)sum;
        }

        private static BigInteger GetBigChecksum(MutableString/*!*/ self, int start, BigInteger/*!*/ sum, int bitCount) {
            BigInteger mask = (((BigInteger)1) << bitCount) - 1;

            int length = self.GetByteCount();
            for (int i = start; i < length; i++) {
                sum = (sum + self.GetByte(i)) & mask;
            }
            return sum;
        }

        #endregion

        #region Encodings (1.9)

        //ascii_only?
        //bytes
        //bytesize
        //chars
        //codepoints
        //each_byte
        //each_char
        //each_codepoint
        //encode
        //encode!
        //encoding
        //force_encoding
        //getbyte
        //setbyte
        //valid_encoding?

        #endregion

        private static void RequireNoVersionChange(uint previousVersion, MutableString self) {
            if (previousVersion != self.Version) {
                throw new RuntimeError("string modified");
            }
        }
    }
}
