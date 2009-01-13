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
using System.Collections.Generic;
using System.Text;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Math;
using IronRuby.Builtins;
using System.Diagnostics;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime {
    public struct IntegerValue : IEquatable<IntegerValue> {
        private int _fixnum;
        private BigInteger _bignum;

        public int Fixnum { 
            get {
                Debug.Assert(IsFixnum);
                return _fixnum;
            } 
        }

        public BigInteger Bignum { 
            get {
                Debug.Assert(!IsFixnum);
                return _bignum;
            }
        }

        public bool IsFixnum { 
            get { return ReferenceEquals(_bignum, null); } 
        }

        public IntegerValue(int value) {
            _fixnum = value;
            _bignum = null;
        }

        public IntegerValue(BigInteger/*!*/ value) {
            _fixnum = 0;
            _bignum = value;
        }

        public static implicit operator IntegerValue(int value) {
            return new IntegerValue(value);
        }

        public static implicit operator IntegerValue(BigInteger value) {
            return new IntegerValue(value);
        }

        public object/*!*/ ToObject() {
            return (object)_bignum ?? ScriptingRuntimeHelpers.Int32ToObject(_fixnum);
        }

        public bool Equals(IntegerValue other) {
            if (_fixnum != other.Fixnum) return false;
            if (ReferenceEquals(_bignum, null)) return ReferenceEquals(other._bignum, null);
            return _bignum.Equals(other._bignum);
        }
    }
}
