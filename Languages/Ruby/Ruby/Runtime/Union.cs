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
using System.Collections.Generic;
using System.Text;
using IronRuby.Compiler.Generation;
using IronRuby.Builtins;
using System.Diagnostics;

namespace IronRuby.Runtime {
    public struct Union<TFirst, TSecond> {
        private TFirst _first;
        private TSecond _second;

        public TFirst First { get { return _first; } }
        public TSecond Second { get { return _second; } }

        [Emitted]
        public Union(TFirst first, TSecond second) {
            _first = first;
            _second = second;
        }

        public static implicit operator Union<TFirst, TSecond>(TFirst value) {
            return new Union<TFirst, TSecond>(value, default(TSecond));
        }

        public static implicit operator Union<TFirst, TSecond>(TSecond value) {
            return new Union<TFirst, TSecond>(default(TFirst), value);
        }
    }

    public static class UnionSpecializations {
        public static bool IsFixnum(this Union<int, MutableString> union) {
            return ReferenceEquals(union.Second, null);
        }

        public static int Fixnum(this Union<int, MutableString> union) {
            Debug.Assert(union.IsFixnum());
            return union.First;
        }

        public static MutableString/*!*/ String(this Union<int, MutableString> union) {
            Debug.Assert(!union.IsFixnum());
            return union.Second;
        }

        public static bool IsFixnum(this Union<MutableString, int> union) {
            return ReferenceEquals(union.First, null);
        }

        public static int Fixnum(this Union<MutableString, int> union) {
            Debug.Assert(union.IsFixnum());
            return union.Second;
        }

        public static MutableString/*!*/ String(this Union<MutableString, int> union) {
            Debug.Assert(!union.IsFixnum());
            return union.First;
        }
    }
}
