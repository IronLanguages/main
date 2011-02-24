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

using Microsoft.Scripting.Utils;
using System.Text.RegularExpressions;
using IronRuby.Runtime;
using System;
using System.Diagnostics;
using System.Text;

namespace IronRuby.Builtins {
    public partial class MatchData : IDuplicable, IRubyObjectState {
        private const int FrozenFlag = 1;
        private const int TaintedFlag = 2;
        private const int UntrustedFlag = 4;

        private int _flags;
        private Match/*!*/ _match;
        private MutableString/*!*/ _originalString;

        public MutableString/*!*/ OriginalString { get { return _originalString; } }

        /// <summary>
        /// The encoding of the match data is always the same as encoding of the input string 
        /// (even if the regex has a different compatible encoding).
        /// </summary>
        public RubyEncoding/*!*/ Encoding { get { return _originalString.Encoding; } }

        public int GroupCount { 
            get { return _match.Groups.Count; } 
        }

        public int Index {
            get { return GetGroupStart(0); } 
        }

        public int Length {
            get { return GetGroupLength(0); } 
        }

        #region Construction

        private MatchData(Match/*!*/ match, MutableString/*!*/ originalString) {
            Debug.Assert(match.Success);
            
            _match = match;

            // TODO (opt): create groups instead?
            _originalString = originalString;

            IsTainted = originalString.IsTainted;
            IsUntrusted = originalString.IsUntrusted;
        }

        public MatchData() {
            _originalString = MutableString.FrozenEmpty;
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

        internal static MatchData Create(Match/*!*/ match, MutableString/*!*/ input, bool freezeInput, string/*!*/ encodedInput) {
            if (!match.Success) {
                return null;
            }

            if (freezeInput) {
                input = input.Clone().Freeze();
            }
            return new MatchData(match, input);
        }
        
        #endregion

        #region IRubyObjectState Members

        public bool IsFrozen {
            get { return (_flags & FrozenFlag) != 0; }
        }

        public bool IsTainted {
            get { return (_flags & TaintedFlag) != 0; }
            set { _flags = (_flags & ~TaintedFlag) | (value ? TaintedFlag : 0); }
        }

        public bool IsUntrusted {
            get { return (_flags & UntrustedFlag) != 0; }
            set { _flags = (_flags & ~UntrustedFlag) | (value ? UntrustedFlag : 0); }
        }

        public void Freeze() {
            _flags |= FrozenFlag;
        }

        #endregion

        public bool GroupSuccess(int index) {
            return _match.Groups[index].Success;
        }

        public MutableString GetValue() {
            return _match.Success ? _originalString.GetSlice(Index, Length).TaintBy(this) : null;
        }

        public MutableString GetGroupValue(int index) {
            // we don't need to check index range, Groups indexer returns an unsuccessful group if out of range:
            return GroupSuccess(index) ? _originalString.GetSlice(GetGroupStart(index), GetGroupLength(index)).TaintBy(this) : null;
        }

        public MutableString AppendGroupValue(int index, MutableString/*!*/ result) {
            // we don't need to check index range, Groups indexer returns an unsuccessful group if out of range:
            return GroupSuccess(index) ? result.Append(_originalString, GetGroupStart(index), GetGroupLength(index)).TaintBy(this) : null;
        }

        public int GetGroupStart(int groupIndex) {
            ContractUtils.Requires(groupIndex >= 0);
            return _match.Groups[groupIndex].Index;
        }

        public int GetGroupLength(int groupIndex) {
            ContractUtils.Requires(groupIndex >= 0);
            return _match.Groups[groupIndex].Length;
        }
        
        public int GetGroupEnd(int groupIndex) {
            ContractUtils.Requires(groupIndex >= 0);
            return GetGroupStart(groupIndex) + GetGroupLength(groupIndex);
        }

        public void RequireExistingGroup(int index) {
            if (index >= _match.Groups.Count || index < 0) {
                throw RubyExceptions.CreateIndexError("index {0} out of matches", index);
            }
        }

        public MutableString/*!*/ GetPreMatch() {
            return _originalString.GetSlice(0, GetGroupStart(0)).TaintBy(this);
        }
        
        public MutableString/*!*/ GetPostMatch() {
            return _originalString.GetSlice(GetGroupEnd(0)).TaintBy(this);
        }
    }
}
