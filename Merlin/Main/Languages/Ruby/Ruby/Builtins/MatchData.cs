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

using Microsoft.Scripting.Utils;
using System.Text.RegularExpressions;
using IronRuby.Runtime;
using System;

namespace IronRuby.Builtins {
    // TODO: taint from OriginalString, copy original string?
    public partial class MatchData : IDuplicable {
        private Match/*!*/ _match;
        private MutableString/*!*/ _originalString;

        public Match/*!*/ Match { get { return _match; } }
        public MutableString/*!*/ OriginalString { get { return _originalString; } }
        public bool Success { get { return _match.Success; } }
        public GroupCollection Groups { get { return _match.Groups; } }
        public int Index { get { return _match.Index; } }
        public int Length { get { return _match.Length; } }
        public string Value { get { return _match.Value; } }

        #region Construction

        public MatchData(Match/*!*/ match, MutableString/*!*/ originalString) {
            ContractUtils.RequiresNotNull(match, "regex");
            ContractUtils.RequiresNotNull(originalString, "originalString");
            _match = match;
            _originalString = originalString;
        }

        public MatchData() {
            _originalString = MutableString.Empty;
            _match = Match.Empty;
        }

        protected MatchData(MatchData/*!*/ data)
            : this(data._match, data._originalString) {
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            var result = CreateInstance();
            context.CopyInstanceData(this, result, copySingletonMembers);
            return result;
        }

        protected virtual MatchData/*!*/ CreateInstance() {
            return new MatchData();
        }
        
        public void InitializeFrom(MatchData/*!*/ other) {
            _match = other._match;
            _originalString = other._originalString;
        }

        #endregion

        public MutableString GetGroupValue(RubyContext/*!*/ context, int index) {
            var group = Groups[index];
            return group.Success ? MutableString.Create(group.Value).TaintBy(this, context) : null;
        }

        public Group/*!*/ GetExistingGroup(int index) {
            if (index >= Groups.Count || index < 0) {
                throw RubyExceptions.CreateIndexError(String.Format("index {0} out of matches", index));
            }
            return Groups[index];
        }
    }
}
