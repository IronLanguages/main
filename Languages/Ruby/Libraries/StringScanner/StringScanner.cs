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

using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Text.RegularExpressions;
using IronRuby.Builtins;
using IronRuby.Runtime;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace IronRuby.StandardLibrary.StringScanner {

    [RubyClass("StringScanner")]
    public sealed class StringScanner : RubyObject {
        private MutableString/*!*/ _scanString;
        private int _previousPosition;
        private int _currentPosition;
        private int _foundPosition;
        private MutableString _lastMatch;
        private MatchData _lastMatchingGroups;

        #region Construction

        public StringScanner(RubyClass/*!*/ rubyClass) 
            : base(rubyClass) {
            _scanString = MutableString.FrozenEmpty;
        }

#if !SILVERLIGHT
        public StringScanner(SerializationInfo/*!*/ info, StreamingContext context) 
            : base(info, context) {
            // TODO: deserialize
        }

        public override void GetObjectData(SerializationInfo/*!*/ info, StreamingContext context) {
            base.GetObjectData(info, context);
            // TODO: serialize
        }
#endif

        protected override RubyObject/*!*/ CreateInstance() {
            return new StringScanner(ImmediateClass.NominalClass);
        }

        private void InitializeFrom(StringScanner/*!*/ other) {
            _currentPosition = other._currentPosition;
            _foundPosition = other._foundPosition;
            _lastMatch = other._lastMatch;
            _lastMatchingGroups = other._lastMatchingGroups;
            _previousPosition = other._previousPosition;
            _scanString = other.ScanString;
        }

        [RubyConstructor]
        public static StringScanner/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ scan, [Optional]object ignored) {
            var result = new StringScanner(self);
            result.ScanString = scan;
            result.Reset();
            return result;
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static void Reinitialize(StringScanner/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ scan, [Optional]object ignored) {
            self.ScanString = scan;
            self.Reset();
        }

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static void InitializeFrom(StringScanner/*!*/ self, [DefaultProtocol, NotNull]StringScanner/*!*/ other) {
            self.InitializeFrom(other);
        }

        #endregion

        #region Singleton Methods

        /// <summary>
        /// This method is defined for backwards compatibility
        /// </summary>
        [RubyMethod("must_C_version", RubyMethodAttributes.PublicSingleton)]
        public static object MustCVersion(object self) {
            return self;
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("<<")]
        [RubyMethod("concat")]
        public static StringScanner Concat(StringScanner/*!*/ self, MutableString str) {
            self.ScanString.Append(str);
            return self;
        }

        [RubyMethod("[]")]
        public static MutableString GetMatchSubgroup(StringScanner/*!*/ self, int subgroup) {
            if (subgroup == 0 && self.LastMatch != null) {
                return MutableString.Create(self.LastMatch);
            }
            if (self.LastMatchingGroups == null) {
                return null;
            }
            if (subgroup < 0) {
                subgroup = self.LastMatchingGroups.GroupCount - subgroup;
            }
            if (subgroup >= self.LastMatchingGroups.GroupCount) {
                return null;
            }
            return self.LastMatchingGroups.GetGroupValue(subgroup);
        }

        [RubyMethod("beginning_of_line?")]
        [RubyMethod("bol?")]
        public static bool BeginningOfLine(StringScanner/*!*/ self) {
            return (self.CurrentPosition == 0) || (self.ScanString.GetChar(self.CurrentPosition - 1) == '\n');
        }

        [RubyMethod("check")]
        public static MutableString Check(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            return (ScanFull(self, pattern, false, true) as MutableString);
        }

        [RubyMethod("check_until")]
        public static MutableString CheckUntil(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            return (SearchFull(self, pattern, false, true) as MutableString);
        }

        [RubyMethod("empty?")]
        [RubyMethod("eos?")]
        public static bool EndOfLine(StringScanner/*!*/ self) {
            return self.CurrentPosition >= self.Length;
        }

        [RubyMethod("exist?")]
        public static int? Exist(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            if (!self.Match(pattern, false, false)) {
                return null;
            }
            return self.FoundPosition + self.LastMatch.Length;
        }

        [RubyMethod("get_byte")]
        [RubyMethod("getbyte")]
        public static MutableString GetByte(StringScanner/*!*/ self) {
            if (self.CurrentPosition >= self.Length) {
                return null;
            }
            self.PreviousPosition = self.CurrentPosition;
            self.FoundPosition = self.CurrentPosition;
            self.LastMatch = self.ScanString.GetSlice(self.CurrentPosition++, 1);
            return MutableString.Create(self.LastMatch);
        }

        [RubyMethod("getch")]
        public static MutableString GetChar(StringScanner/*!*/ self) {
            if (self.CurrentPosition >= self.Length) {
                return null;
            }
            self.PreviousPosition = self.CurrentPosition;
            self.FoundPosition = self.CurrentPosition;
            self.LastMatch = self.ScanString.GetSlice(self.CurrentPosition++, 1);
            return MutableString.Create(self.LastMatch);
        }

        [RubyMethod("inspect")]
        [RubyMethod("to_s")]
        public static MutableString ToString(StringScanner/*!*/ self) {
            return MutableString.Create(self.ToString(), self._scanString.Encoding);
        }

        [RubyMethod("match?")]
        public static int? Match(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            if (!self.Match(pattern, true, false)) {
                return null;
            }
            return self.LastMatch.GetLength();
        }

        [RubyMethod("matched")]
        public static MutableString Matched(StringScanner/*!*/ self) {
            if (self.LastMatch == null) {
                return null;
            }
            return MutableString.Create(self.LastMatch);
        }

        [RubyMethod("matched?")]
        public static bool WasMatched(StringScanner/*!*/ self) {
            return (self.LastMatch != null);
        }

        [RubyMethod("matched_size")]
        [RubyMethod("matchedsize")]
        public static int? MatchedSize(StringScanner/*!*/ self) {
            if (self.LastMatch == null) {
                return null;
            }
            return self.LastMatch.Length;
        }

        [RubyMethod("peek")]
        [RubyMethod("peep")]
        public static MutableString Peek(StringScanner/*!*/ self, int len) {
            if (len < 0) {
                throw RubyExceptions.CreateArgumentError("negative string size (or size too big)");
            }
            int maxlen = self.Length - self.CurrentPosition;
            if (len > maxlen) {
                len = maxlen;
            }
            if (self.CurrentPosition >= self.Length || len == 0) {
                return MutableString.CreateEmpty();
            }
            return self.ScanString.GetSlice(self.CurrentPosition, len);
        }

        [RubyMethod("pos")]
        [RubyMethod("pointer")]
        public static int GetCurrentPosition(StringScanner/*!*/ self) {
            return self.CurrentPosition;
        }

        [RubyMethod("pos=")]
        [RubyMethod("pointer=")]
        public static int SetCurrentPosition(StringScanner/*!*/ self, int newPosition) {
            int newPos = newPosition;
            if (newPos < 0) {
                newPos = self.Length - self.CurrentPosition;
            }
            if (newPos > self.Length) {
                throw RubyExceptions.CreateRangeError("index out of range");
            }
            self.CurrentPosition = newPos;
            return newPosition;
        }

        [RubyMethod("post_match")]
        public static MutableString PostMatch(StringScanner/*!*/ self) {
            if (self.LastMatch == null) {
                return null;
            }
            int position = self.FoundPosition + self.LastMatch.Length;
            int len = self.Length - position;
            if (len <= 0) {
                return MutableString.CreateEmpty();
            }
            return self.ScanString.GetSlice(position, len);
        }

        [RubyMethod("pre_match")]
        public static MutableString PreMatch(StringScanner/*!*/ self) {
            if (self.LastMatch == null) {
                return null;
            }
            return self.ScanString.GetSlice(0, self.FoundPosition);
        }

        [RubyMethod("reset")]
        public static StringScanner Reset(StringScanner/*!*/ self) {
            self.Reset();
            return self;
        }

        [RubyMethod("rest")]
        public static MutableString Rest(StringScanner/*!*/ self) {
            int len = self.Length - self.CurrentPosition;
            if (len <= 0) {
                return MutableString.CreateEmpty();
            }
            return self.ScanString.GetSlice(self.CurrentPosition, len);
        }

        [RubyMethod("rest?")]
        public static bool IsRestLeft(StringScanner/*!*/ self) {
            return self.CurrentPosition < self.Length;
        }

        [RubyMethod("rest_size")]
        [RubyMethod("restsize")]
        public static int RestSize(StringScanner/*!*/ self) {
            return (self.CurrentPosition < self.Length) ? (self.Length - self.CurrentPosition) : 0;
        }

        [RubyMethod("scan")]
        public static object/*!*/ Scan(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            return ScanFull(self, pattern, true, true);
        }

        [RubyMethod("scan_full")]
        public static object ScanFull(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern, bool advancePointer, bool returnString) {
            bool match = self.Match(pattern, true, advancePointer);
            if (match) {
                if (returnString) {
                    return MutableString.Create(self.LastMatch);
                } else {
                    return ScriptingRuntimeHelpers.Int32ToObject(self.LastMatch.Length);
                }
            }
            return null;
        }

        [RubyMethod("scan_until")]
        public static object ScanUntil(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            return SearchFull(self, pattern, true, true);
        }

        [RubyMethod("search_full")]
        public static object SearchFull(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern, bool advancePointer, bool returnString) {
            bool match = self.Match(pattern, false, advancePointer);
            if (match) {
                int length = self.LastMatch.Length + (self.FoundPosition - self.PreviousPosition);
                if (returnString) {
                    return self.ScanString.GetSlice(self.PreviousPosition, length);
                } else {
                    return ScriptingRuntimeHelpers.Int32ToObject(length);
                }
            }
            return null;
        }

        [RubyMethod("skip")]
        public static int? Skip(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            bool match = self.Match(pattern, true, true);
            if (!match) {
                return null;
            }
            return (self.CurrentPosition - self.PreviousPosition);
        }

        [RubyMethod("skip_until")]
        public static int? SkipUntil(StringScanner/*!*/ self, [NotNull]RubyRegex/*!*/ pattern) {
            bool match = self.Match(pattern, false, true);
            if (!match) {
                return null;
            }
            return (self.CurrentPosition - self.PreviousPosition);
        }

        [RubyMethod("string")]
        public static MutableString GetString(StringScanner/*!*/ self) {
            return self.ScanString;
        }

        [RubyMethod("string=")]
        public static MutableString SetString(RubyContext/*!*/ context, StringScanner/*!*/ self, [NotNull]MutableString/*!*/ str) {
            self.ScanString = (MutableString)KernelOps.Freeze(context, MutableString.Create(str));
            self.Reset();
            return str;
        }

        [RubyMethod("clear")]
        [RubyMethod("terminate")]
        public static StringScanner Clear(StringScanner/*!*/ self) {
            self.Reset();
            self.CurrentPosition = self.Length;
            return self;
        }

        [RubyMethod("unscan")]
        public static StringScanner Unscan(StringScanner/*!*/ self) {
            if (self.LastMatch == null) {
                // throw Exception StringScanner::Error
                throw RubyExceptions.CreateRangeError("unscan failed: previous match had failed");
            }
            int position = self.PreviousPosition;
            self.Reset();
            self.CurrentPosition = position;
            return self;
        }
        #endregion

        #region Helpers

        private bool Match(RubyRegex/*!*/ pattern, bool currentPositionOnly, bool advancePosition) {
            // TODO: repeated calls on the same ScanString can be optimized:
            MatchData match = pattern.Match(_scanString, _currentPosition, false);
            _lastMatch = null;
            _lastMatchingGroups = null;
            _foundPosition = 0;
            if (match == null) {
                return false;
            }
            if (currentPositionOnly && match.Index != _currentPosition) {
                return false;
            }
            int length = (match.Index - _currentPosition) + match.Length;
            _foundPosition = match.Index;
            _previousPosition = _currentPosition;
            _lastMatch = _scanString.GetSlice(_foundPosition, match.Length);
            _lastMatchingGroups = match;
            if (advancePosition) {
                _currentPosition += length;
            }
            return true;
        }

        #endregion 

        
        private int PreviousPosition {
            get { return _previousPosition; }
            set { _previousPosition = value; }
        }

        private int CurrentPosition {
            get { return _currentPosition; }
            set { _currentPosition = value; }
        }

        private int Length {
            get { return _scanString.Length; }
        }

        private MutableString/*!*/ ScanString {
            get { return _scanString; }
            set { _scanString = value; }
        }

        private int FoundPosition {
            get { return _foundPosition; }
            set { _foundPosition = value; }
        }

        private MutableString LastMatch {
            set { _lastMatch = value; }
            get { return _lastMatch; }
        }

        private MatchData LastMatchingGroups {
            get { return _lastMatchingGroups; }
        }

        private void Reset() {
            _previousPosition = 0;
            _currentPosition = 0;
            _foundPosition = 0;
            _lastMatch = null;
            _lastMatchingGroups = null;
        }

        // TODO: encodings
        public override string ToString() {
            // #<StringScanner 0/11 @ "test ...">
            byte[] scanstr = ScanString.ToByteArray();
            StringBuilder sb = new StringBuilder("#<StringScanner ");
            if (CurrentPosition >= Length || CurrentPosition < 0) {
                sb.Append("fin >");
                return sb.ToString();
            }

            sb.AppendFormat("{0}/{1}", CurrentPosition, scanstr.Length);
            if (CurrentPosition > 0) {
                sb.Append(" \"");
                int len = CurrentPosition;
                if (len > 5) {
                    len = 5;
                    sb.Append("...");
                }
                for (int i = CurrentPosition - len; i < CurrentPosition; i++) {
                    MutableString.AppendCharRepresentation(sb, scanstr[i], -1, 
                        MutableString.Escape.NonAscii | MutableString.Escape.Special, '"', -1
                    );
                }
                sb.Append('"');
            }
            sb.Append(" @ ");
            if (CurrentPosition < scanstr.Length) {
                int len = scanstr.Length - CurrentPosition;
                bool ellipsis = false;
                if (len > 5) {
                    len = 5;
                    ellipsis = true;
                }
                sb.Append('"');
                for (int i = CurrentPosition; i < CurrentPosition + len; i++) {
                    MutableString.AppendCharRepresentation(sb, scanstr[i], -1, 
                        MutableString.Escape.NonAscii | MutableString.Escape.Special, '"', -1
                    );
                }
                if (ellipsis) {
                    sb.Append("...");
                }
                sb.Append('"');
            }
            sb.Append('>');
            return sb.ToString();
        }
    }
}
