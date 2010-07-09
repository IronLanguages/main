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

namespace IronRuby.Runtime {
    public static class Key {
        public static Key<T0/*!*/, T1/*!*/>/*!*/ Create<T0, T1>(T0/*!*/ first, T1/*!*/ second) {
            return new Key<T0, T1>(first, second);
        }

        public static Key<T0/*!*/, T1/*!*/, T2/*!*/>/*!*/ Create<T0, T1, T2>(T0/*!*/ first, T1/*!*/ second, T2/*!*/ third) {
            return new Key<T0, T1, T2>(first, second, third);
        }
    }

    [Serializable]
    public class Key<T0, T1> : IEquatable<Key<T0, T1>> {
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

    [Serializable]
    public sealed class Key<T0, T1, T2> : Key<T0, T1>, IEquatable<Key<T0, T1, T2>> {
        public readonly T2/*!*/ Third;

        public Key(T0/*!*/ first, T1/*!*/ second, T2/*!*/ third) 
            : base(first, second) {
            Third = third;
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ Third.GetHashCode();
        }

        public override bool Equals(object obj) {
            var other = obj as Key<T0, T1, T2>;
            return other != null && Equals(other);
        }

        public bool Equals(Key<T0, T1, T2>/*!*/ other) {
            return (object)this == (object)other || base.Equals(other) && Third.Equals(other.Third);
        }

        public static bool operator ==(Key<T0, T1, T2>/*!*/ s, Key<T0, T1, T2>/*!*/ t) {
            return s.Equals(t);
        }

        public static bool operator !=(Key<T0, T1, T2>/*!*/ s, Key<T0, T1, T2>/*!*/ t) {
            return !s.Equals(t);
        }
    }
}
