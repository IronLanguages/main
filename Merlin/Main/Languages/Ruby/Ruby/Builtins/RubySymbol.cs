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
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    [Serializable]
    public class RubySymbol : IComparable, IComparable<RubySymbol>, IEquatable<RubySymbol>, IRubyObjectState {
        internal static int MinId = 1;
        private readonly int _id;
        private readonly int _runtimeId;
        private readonly MutableString/*!*/ _string;

        internal RubySymbol(MutableString/*!*/ str, int id, int runtimeId) {
            Assert.NotNull(str);
            Debug.Assert(str.IsFrozen);
            Debug.Assert(str.GetType() == typeof(MutableString));

            _string = str;
            _id = id;
            _runtimeId = runtimeId;
        }

        #region Equals, GetHashCode, CompareTo

        public override bool Equals(object obj) {
            return (object)this == obj;
        }

        public override int GetHashCode() {
            // TODO: the same value, different encoding => different hash? different symbols?
            return _string.GetHashCode();
        }

        public bool Equals(RubySymbol other) {
            return Equals((object)other);
        }

        public int CompareTo(RubySymbol other) {
            return _string.CompareTo(other._string);
        }

        int IComparable.CompareTo(object other) {
            var sym = other as RubySymbol;
            return (sym != null) ? _string.CompareTo(sym._string) : -1;
        }

        #endregion

        #region Id, Encoding, Length, ToString

        public int RuntimeId {
            get { return _runtimeId; }
        }

        public int Id {
            get { return _id; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _string.Encoding; }
        }

        internal MutableString/*!*/ MutableString {
            get { return _string; }
        }

        public override string/*!*/ ToString() {
            return _string.ToString();
        }

        public MutableString/*!*/ ToMutableString() {
            return _string.Clone();
        }

        public bool IsEmpty {
            get { return _string.IsEmpty; }
        }

        public int GetByteCount() {
            return _string.GetByteCount();
        }

        public int GetCharCount() {
            return _string.GetCharCount();
        }

        public static explicit operator string(RubySymbol/*!*/ self) {
            return self._string.ToString();
        }

        #endregion

        #region IRubyObjectState Members

        public bool IsFrozen {
            get { return false; }
        }

        public bool IsTainted {
            get { return false; }
            set { /* nop */ }
        }

        public void Freeze() {
            // nop
        }

        #endregion

        #region String Operations

        public bool EndsWith(char value) {
            return _string.GetLastChar() == value;
        }

        public MutableString/*!*/ GetSlice(int start) {
            return _string.GetSlice(start);
        }

        #endregion

        
    }
}
