
/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using System.Linq.Expressions;

namespace System.Dynamic.Utils {
    // Miscellaneous helpers that don't belong anywhere else
    internal static class Helpers {

        internal static Expression Convert(Expression expression, Type type) {
            if (expression.Type == type) {
                return expression;
            }
            if (expression.Type == typeof(void)) {
                return Expression.Block(expression, Expression.Default(type));
            }
            return Expression.Convert(expression, type);
        }

        /// <summary>
        /// Creates an array of size count with each element initialized to item
        /// </summary>
        internal static T[] RepeatedArray<T>(T item, int count) {
            T[] ret = new T[count];
            for (int i = 0; i < count; i++) ret[i] = item;
            return ret;
        }

        internal static T CommonNode<T>(T first, T second, Func<T, T> parent) where T : class {
            var cmp = EqualityComparer<T>.Default;
            if (cmp.Equals(first, second)) {
                return first;
            }
            var set = new System.Linq.Expressions.Compiler.Set<T>(cmp);
            for (T t = first; t != null; t = parent(t)) {
                set.Add(t);
            }
            for (T t = second; t != null; t = parent(t)) {
                if (set.Contains(t)) {
                    return t;
                }
            }
            return null;
        }

        internal static void IncrementCount<T>(T key, Dictionary<T, int> dict) {
            int count;
            dict.TryGetValue(key, out count);
            dict[key] = count + 1;
        }

        internal static string ToValidPath(string path) {
            return ToValidPath(path, false, true);
        }

        internal static string ToValidPath(string path, bool isMask) {
            return ToValidPath(path, isMask, true);
        }

        internal static string ToValidFileName(string path) {
            return ToValidPath(path, false, false);
        }

        private static string ToValidPath(string path, bool isMask, bool isPath) {
            Debug.Assert(!isMask || isPath);

            if (String.IsNullOrEmpty(path)) {
                return "_";
            }

            StringBuilder sb = new StringBuilder(path);

            if (isPath) {
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
            } else {
#if SILVERLIGHT
                foreach (char c in Path.GetInvalidPathChars()) {
                    sb.Replace(c, '_');
                }
                sb.Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('\\', '_').Replace('/', '_');
#else
                foreach (char c in Path.GetInvalidFileNameChars()) {
                    sb.Replace(c, '_');
                }
#endif
            }

            if (!isMask) {
                sb.Replace('*', '_').Replace('?', '_');
            }

            return sb.ToString();
        }
    }
}
