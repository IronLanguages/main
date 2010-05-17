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
using System.Diagnostics;
using System.Text;

namespace IronRuby.Builtins {
    public partial class MatchData : IDuplicable, IRubyObjectState {
        private const int FrozenFlag = 1;
        private const int TaintedFlag = 2;

        private int _flags;
        private Match/*!*/ _match;
        private int[] _kIndices;
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

        private MatchData(Match/*!*/ match, MutableString/*!*/ originalString, int[] kIndices) {
            Debug.Assert(match.Success);
            
            _match = match;

            // TODO (opt): create groups instead?
            _originalString = originalString;

            _kIndices = kIndices;
            IsTainted = originalString.IsTainted;
        }

        public MatchData() {
            _originalString = MutableString.FrozenEmpty;
            _match = Match.Empty;
        }

        protected MatchData(MatchData/*!*/ data)
            : this(data._match, data._originalString, data._kIndices) {
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
            _kIndices = other._kIndices;
            _originalString = other._originalString;
        }

        internal static MatchData Create(Match/*!*/ match, MutableString/*!*/ input, bool freezeInput, string/*!*/ encodedInput, RubyEncoding kcoding, int kStart) {
            if (!match.Success) {
                return null;
            }
            
            int[] kIndices;
            if (kcoding != null) {
                // TODO (opt): minimize GetByteCount calls, remove ToCharArray:
                char[] kCodedChars = encodedInput.ToCharArray();
                Encoding encoding = kcoding.StrictEncoding;
                kIndices = new int[match.Groups.Count * 2];
                for (int i = 0; i < match.Groups.Count; i++) {
                    var group = match.Groups[i];
                    if (group.Success) {
                        // group start index:
                        kIndices[i * 2] = kStart + encoding.GetByteCount(kCodedChars, 0, group.Index);
                        // group length:
                        kIndices[i * 2 + 1] = encoding.GetByteCount(kCodedChars, group.Index, group.Length);
                    } else {
                        kIndices[i * 2] = -1;
                    }
                }
            } else {
                kIndices = null;
            }

            if (freezeInput) {
                input = input.Clone().Freeze();
            }
            return new MatchData(match, input, kIndices);
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
            return _kIndices != null ? _kIndices[groupIndex * 2] : _match.Groups[groupIndex].Index;
        }

        public int GetGroupLength(int groupIndex) {
            ContractUtils.Requires(groupIndex >= 0);
            return _kIndices != null ? _kIndices[groupIndex * 2 + 1] : _match.Groups[groupIndex].Length;
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
