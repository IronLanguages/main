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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {
    internal static class CollectionExtensions {
        /// <summary>
        /// Wraps the provided enumerable into a ReadOnlyCollection{T}
        /// 
        /// Copies all of the data into a new array, so the data can't be
        /// changed after creation. The exception is if the enumerable is
        /// already a ReadOnlyCollection{T}, in which case we just return it.
        /// </summary>
        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null) {
                return EmptyReadOnlyCollection<T>.Instance;
            }

            var roCollection = enumerable as ReadOnlyCollection<T>;
            if (roCollection != null) {
                return roCollection;
            }

            var collection = enumerable as ICollection<T>;
            if (collection != null) {
                int count = collection.Count;
                if (count == 0) {
                    return EmptyReadOnlyCollection<T>.Instance;
                }

                T[] array = new T[count];
                collection.CopyTo(array, 0);
                return new ReadOnlyCollection<T>(array);
            }

            // ToArray trims the excess space and speeds up access
            return new ReadOnlyCollection<T>(new List<T>(enumerable).ToArray());
        }
        internal static T[] ToArray<T>(this IEnumerable<T> enumerable) {
            var c = enumerable as ICollection<T>;
            if (c != null) {
                var result = new T[c.Count];
                c.CopyTo(result, 0);
                return result;
            }
            return new List<T>(enumerable).ToArray();
        }

        
        internal static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate) {
            foreach (T element in source) {
                if (predicate(element)) {
                    return true;
                }
            }
            return false;
        }

        internal static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) {
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new ArgumentException("Collection is empty", "source");
                TSource result = e.Current;
                while (e.MoveNext()) result = func(result, e.Current);
                return result;
            }
        }

        internal static T[] AddFirst<T>(this IList<T> list, T item) {
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return res;
        }

        internal static bool TrueForAll<T>(this IEnumerable<T> collection, Predicate<T> predicate) {
            ContractUtils.RequiresNotNull(collection, "collection");
            ContractUtils.RequiresNotNull(predicate, "predicate");

            foreach (T item in collection) {
                if (!predicate(item)) return false;
            }

            return true;
        }
    }


    internal static class EmptyReadOnlyCollection<T> {
        internal static ReadOnlyCollection<T> Instance = new ReadOnlyCollection<T>(new T[0]);
    }
    // TODO: Should we use this everywhere for empty arrays?
    // my thought is, probably more hassle than its worth
    internal static class EmptyArray<T> {
        internal static T[] Instance = new T[0];
    }
}
