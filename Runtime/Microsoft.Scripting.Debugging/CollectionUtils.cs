/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Scripting.Debugging {
    internal static class CollectionUtils {
        internal static T[] RemoveLast<T>(this T[] array) {
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, result.Length);
            return result;
        }

        internal static bool ListEquals<T>(this ICollection<T> first, ICollection<T> second) {
            if (first.Count != second.Count) {
                return false;
            }
            var cmp = EqualityComparer<T>.Default;
            var f = first.GetEnumerator();
            var s = second.GetEnumerator();
            while (f.MoveNext()) {
                s.MoveNext();

                if (!cmp.Equals(f.Current, s.Current)) {
                    return false;
                }
            }
            return true;
        }

        internal static int ListHashCode<T>(this IEnumerable<T> list) {
            var cmp = EqualityComparer<T>.Default;
            int h = 6551;
            foreach (T t in list) {
                h ^= (h << 5) ^ cmp.GetHashCode(t);
            }
            return h;
        }
    }

    // Compares two ICollection<T>'s using element equality
    internal sealed class ListEqualityComparer<T> : EqualityComparer<ICollection<T>> {
        internal static readonly ListEqualityComparer<T> Instance = new ListEqualityComparer<T>();

        private ListEqualityComparer() { }

        // EqualityComparer<T> handles null and object identity for us
        public override bool Equals(ICollection<T> x, ICollection<T> y) {
            return x.ListEquals(y);
        }

        public override int GetHashCode(ICollection<T> obj) {
            return obj.ListHashCode();
        }
    }
}
