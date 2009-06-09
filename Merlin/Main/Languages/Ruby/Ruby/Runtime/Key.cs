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

namespace IronRuby.Runtime {
    public static class Key {
        public static Key<T0/*!*/, T1/*!*/>/*!*/ Create<T0, T1>(T0/*!*/ first, T1/*!*/ second) {
            return new Key<T0, T1>(first, second);
        }
    }

    public sealed class Key<T0, T1> : IEquatable<Key<T0, T1>> {
        public readonly T0/*!*/ First;
        public readonly T1/*!*/ Second;

        public Key(T0/*!*/ first, T1/*!*/ second) {
            First = first;
            Second = second;
        }

        public override int GetHashCode() {
            return First.GetHashCode() ^ Second.GetHashCode();
        }

        public override bool Equals(object obj) {
            var other = obj as Key<T0, T1>;
            return other != null && Equals(other);
        }

        public bool Equals(Key<T0, T1>/*!*/ other) {
            return (object)this == (object)other || First.Equals(other.First) && Second.Equals(other.Second);
        }

        public static bool operator ==(Key<T0, T1>/*!*/ s, Key<T0, T1>/*!*/ t) {
            return s.Equals(t);
        }

        public static bool operator !=(Key<T0, T1>/*!*/ s, Key<T0, T1>/*!*/ t) {
            return !s.Equals(t);
        }
    }
}
