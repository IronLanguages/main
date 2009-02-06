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
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Linq.Expressions.Compiler;

namespace System.Runtime.CompilerServices {

    /// <summary>
    /// This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.
    /// Contains helper methods called from dynamically generated methods.
    /// </summary>
    public static partial class RuntimeOps {
        /// <summary>
        /// Creates an interface that can be used to modify closed over variables at runtime.
        /// </summary>
        /// <param name="data">The closure array.</param>
        /// <param name="indexes">An array of indicies into the closure array where variables are found.</param>
        /// <returns>An interface to access variables.</returns>
        [Obsolete("do not call this method", true)]
        public static IList<IStrongBox> CreateRuntimeVariables(object[] data, long[] indexes) {
            return new RuntimeVariableList(data, indexes);
        }

        /// <summary>
        /// Creates an interface that can be used to modify closed over variables at runtime.
        /// </summary>
        /// <returns>An interface to access variables.</returns>
        [Obsolete("do not call this method", true)]
        public static IList<IStrongBox> CreateRuntimeVariables() {
            return EmptyReadOnlyCollection<IStrongBox>.Instance;
        }

        /// <summary>
        /// Provides a list of variables, supporing read/write of the values
        /// Exposed via RuntimeVariablesExpression
        /// </summary>
        private sealed class RuntimeVariableList : IList<IStrongBox> {
            // The top level environment. It contains pointers to parent
            // environments, which are always in the first element
            private readonly object[] _data;

            // An array of (int, int) pairs, each representing how to find a
            // variable in the environment data struction.
            //
            // The first integer indicates the number of times to go up in the
            // closure chain, the second integer indicates the index into that
            // closure chain.
            private readonly long[] _indexes;

            internal RuntimeVariableList(object[] data, long[] indexes) {
                Debug.Assert(data != null);
                Debug.Assert(indexes != null);

                _data = data;
                _indexes = indexes;
            }

            public int Count {
                get { return _indexes.Length; }
            }

            public IStrongBox this[int index] {
                get {
                    // We lookup the closure using two ints:
                    // 1. The high dword is the number of parents to go up
                    // 2. The low dword is the index into that array
                    long closureKey = _indexes[index];

                    // walk up the parent chain to find the real environment
                    object[] result = _data;
                    for (int parents = (int)(closureKey >> 32); parents > 0; parents--) {
                        result = HoistedLocals.GetParent(result);
                    }

                    // Return the variable storage
                    return (IStrongBox)result[(int)closureKey];
                }
                set {
                    throw Error.CollectionReadOnly();
                }
            }

            public int IndexOf(IStrongBox item) {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    if (this[i] == item) {
                        return i;
                    }
                }
                return -1;
            }

            public bool Contains(IStrongBox item) {
                return IndexOf(item) >= 0;
            }

            public void CopyTo(IStrongBox[] array, int arrayIndex) {
                ContractUtils.RequiresNotNull(array, "array");
                int count = _indexes.Length;
                if (arrayIndex < 0 || arrayIndex + count > array.Length) {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                for (int i = 0; i < count; i++) {
                    array[arrayIndex++] = this[i];
                }
            }

            bool ICollection<IStrongBox>.IsReadOnly {
                get { return true; }
            }

            public IEnumerator<IStrongBox> GetEnumerator() {
                for (int i = 0, n = _indexes.Length; i < n; i++) {
                    yield return this[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            void IList<IStrongBox>.Insert(int index, IStrongBox item) {
                throw Error.CollectionReadOnly();
            }

            void IList<IStrongBox>.RemoveAt(int index) {
                throw Error.CollectionReadOnly();
            }

            void ICollection<IStrongBox>.Add(IStrongBox item) {
                throw Error.CollectionReadOnly();
            }

            void ICollection<IStrongBox>.Clear() {
                throw Error.CollectionReadOnly();
            }

            bool ICollection<IStrongBox>.Remove(IStrongBox item) {
                throw Error.CollectionReadOnly();
            }
        }
    }
}
