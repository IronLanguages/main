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
        public static MutableString GetGroup(MatchData/*!*/ self, [DefaultProtocol]int index) {
            index = IListOps.NormalizeIndex(self.GroupCount, index);
            return self.GetGroupValue(index);
        }

        [RubyMethod("[]")]
        public static RubyArray GetGroup(MatchData/*!*/ self, [DefaultProtocol]int start, [DefaultProtocol]int length) {
            if (!IListOps.NormalizeRange(self.GroupCount, ref start, ref length)) {
                return null;
            }

            RubyArray result = new RubyArray();
            for (int i = 0; i < length; i++) {
                result.Add(self.GetGroupValue(start + i));
            }

            return result;
        }

        [RubyMethod("[]")]
        public static RubyArray GetGroup(ConversionStorage<int>/*!*/ fixnumCast, MatchData/*!*/ self, [NotNull]Range/*!*/ range) {
            int begin, count;
            if (!IListOps.NormalizeRange(fixnumCast, self.GroupCount, range, out begin, out count)) {
                return null;
            }
            return GetGroup(self, begin, count);
        }

        [RubyMethod("begin")]
        public static object Begin(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            self.RequireExistingGroup(groupIndex);
            return self.GroupSuccess(groupIndex) ? ScriptingRuntimeHelpers.Int32ToObject(self.GetGroupStart(groupIndex)) : null;
        }

        [RubyMethod("end")]
        public static object End(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            self.RequireExistingGroup(groupIndex);
            return self.GroupSuccess(groupIndex) ? ScriptingRuntimeHelpers.Int32ToObject(self.GetGroupEnd(groupIndex)) : null;
        }

        [RubyMethod("length")]
        [RubyMethod("size")]
        public static int Length(MatchData/*!*/ self) {
            return self.GroupCount;
        }

        [RubyMethod("offset")]
        public static RubyArray/*!*/ Offset(MatchData/*!*/ self, [DefaultProtocol]int groupIndex) {
            self.RequireExistingGroup(groupIndex);
            RubyArray result = new RubyArray(2);
            if (self.GroupSuccess(groupIndex)) {
                result.Add(self.GetGroupStart(groupIndex));
                result.Add(self.GetGroupEnd(groupIndex));
            } else {
                result.Add(null);
                result.Add(null);
            }
            return result;
        }

        [RubyMethod("pre_match")]
        public static MutableString/*!*/ PreMatch(MatchData/*!*/ self) {
            return self.GetPreMatch();
        }

        [RubyMethod("post_match")]
        public static MutableString/*!*/ PostMatch(MatchData/*!*/ self) {
            return self.GetPostMatch();
        }

        private static RubyArray/*!*/ ReturnMatchingGroups(MatchData/*!*/ self, int groupIndex) {
            Debug.Assert(groupIndex >= 0);
            
            if (self.GroupCount < groupIndex) {
                return new RubyArray();
            }

            RubyArray result = new RubyArray(self.GroupCount - groupIndex);
            for (int i = groupIndex; i < self.GroupCount; i++) {
                result.Add(self.GetGroupValue(i));
            }
            return result;
        }

        [RubyMethod("captures")]
        public static RubyArray/*!*/ Captures(MatchData/*!*/ self) {
            return ReturnMatchingGroups(self, 1);
        }

        [RubyMethod("to_a")]
        public static RubyArray/*!*/ ToArray(MatchData/*!*/ self) {
            return ReturnMatchingGroups(self, 0);
        }

        [RubyMethod("string")]
        public static MutableString/*!*/ ReturnFrozenString(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return MutableString.Create(self.OriginalString).TaintBy(self, context).Freeze();
        }

        [RubyMethod("select")]
        public static object Select([NotNull]BlockParam/*!*/ block, MatchData/*!*/ self) {
            RubyArray result = new RubyArray();
            for (int i = 0; i < self.GroupCount; i++) {
                MutableString value = self.GetGroupValue(i);

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
        public static RubyArray/*!*/ ValuesAt(ConversionStorage<int>/*!*/ conversionStorage, 
            MatchData/*!*/ self, [DefaultProtocol]params int[]/*!*/ indices) {

            RubyArray result = new RubyArray();
            for (int i = 0; i < indices.Length; i++) {
                result.Add(GetGroup(self, indices[i]));
            }
            return result;
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, MatchData/*!*/ self) {
            return RubyUtils.ObjectToMutableString(context, self);
        }

        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(MatchData/*!*/ self) {
            return self.GetValue();
        }

        #endregion
    }
}
