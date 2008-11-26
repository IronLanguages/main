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
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {
    [RubyClass("MatchData", Extends = typeof(MatchData), Inherits = typeof(Object))]
    [UndefineMethod("new", IsStatic = true)]
    public static class MatchDataOps {

        #region Private Instance Methods

        [RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
        public static MatchData/*!*/ InitializeCopy(MatchData/*!*/ self, [NotNull]MatchData/*!*/ other) {
            self.InitializeFrom(other);
            return self;
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("[]")]
        public static MutableString GetGroup(RubyContext/*!*/ context, MatchData/*!*/ self, [DefaultProtocol]int index) {
            index = IListOps.NormalizeIndex(self.Groups.Count, index);
            return self.GetGroupValue(context, index);
        }

        [RubyMethod("[]")]
        public static RubyArray GetGroup(RubyContext/*!*/ context, MatchData/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {
            if (!IListOps.NormalizeRange(self.Groups.Count, ref start, ref length)) {
                return null;
            }

            RubyArray result = new RubyArray();
            for (int i = 0; i < length; i++) {
                result.Add(self.GetGroupValue(context, start + i));
            }

            return result;
        }

        [RubyMethod("[]")]
        public static RubyArray GetGroup(RubyContext/*!*/ context, MatchData/*!*/ self, [NotNull]Range/*!*/ range) {
            int begin, count;
            if (!IListOps.NormalizeRange(context, self.Groups.Count, range, out begin, out count)) {
                return null;
            }
            return GetGroup(context, self, begin, count);
        }

        [RubyMethod("begin")]
        public static object Begin(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            var group = self.GetExistingGroup(groupIndex);
            return group.Success ? ScriptingRuntimeHelpers.Int32ToObject(group.Index) : null;
        }

        [RubyMethod("end")]
        public static object End(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            var group = self.GetExistingGroup(groupIndex);
            return group.Success ? ScriptingRuntimeHelpers.Int32ToObject(group.Index + group.Length) : null;
        }

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int Length(MatchData/*!*/ self) {
            return self.Groups.Count;
        }

        [RubyMethod("offset")]
        public static RubyArray/*!*/ Offset(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            var group = self.GetExistingGroup(groupIndex);
            RubyArray result = new RubyArray(2);
            if (group.Success) {
                result.Add(group.Index);
                result.Add(group.Index + group.Length);
            } else {
                result.Add(null);
                result.Add(null);
            }
            return result;
        }

        [RubyMethod("pre_match")]
        public static MutableString/*!*/ PreMatch(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return self.OriginalString.GetSlice(0, self.Index).TaintBy(self, context);
        }

        [RubyMethod("post_match")]
        public static MutableString/*!*/ PostMatch(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return self.OriginalString.GetSlice(self.Index + self.Length).TaintBy(self, context);
        }

        private static RubyArray/*!*/ ReturnMatchingGroups(RubyContext/*!*/ context, MatchData/*!*/ self, int groupIndex) {
            Debug.Assert(groupIndex >= 0);
            
            if (self.Groups.Count < groupIndex) {
                return new RubyArray();
            }

            RubyArray result = new RubyArray(self.Groups.Count - groupIndex);
            for (int i = groupIndex; i < self.Groups.Count; i++) {
                result.Add(self.GetGroupValue(context, i));
            }
            return result;
        }

        [RubyMethod("captures")]
        public static RubyArray/*!*/ Captures(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return ReturnMatchingGroups(context, self, 1);
        }

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return ReturnMatchingGroups(context, self, 0);
        }

        [RubyMethod("string")]
        public static MutableString/*!*/ ReturnFrozenString(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return MutableString.Create(self.OriginalString).TaintBy(self, context).Freeze();
        }

        [RubyMethod("select")]
        public static object Select(RubyContext/*!*/ context, [NotNull]BlockParam/*!*/ block, MatchData/*!*/ self) {
            RubyArray result = new RubyArray();
            for (int i = 0; i < self.Groups.Count; i++) {
                MutableString value = self.GetGroupValue(context, i);

                object blockResult;
                if (block.Yield(value, out blockResult)) {
                    return blockResult;
                }

                if (RubyOps.IsTrue(blockResult)) {
                    result.Add(value);
                }
            }
            return result;
        }

        [RubyMethod("values_at")]
        public static RubyArray/*!*/ ValuesAt(RubyContext/*!*/ context, MatchData/*!*/ self, [NotNull]params object[]/*!*/ indices) {
            RubyArray result = new RubyArray();
            for (int i = 0; i < indices.Length; i++) {
                result.Add(GetGroup(context, self, Protocols.CastToFixnum(context, indices[i])));
            }
            return result;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return RubyUtils.ObjectToMutableString(context, self);
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return MutableString.Create(self.Value).TaintBy(self, context);
        }

        #endregion
    }
}
