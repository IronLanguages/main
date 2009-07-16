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
using System.Runtime.CompilerServices;
using System.Text;

namespace IronRuby.Runtime {
    public class ReferenceEqualityComparer : IEqualityComparer<object> {
        private ReferenceEqualityComparer() {}

        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        public new bool Equals(object x, object y) {
            return Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
