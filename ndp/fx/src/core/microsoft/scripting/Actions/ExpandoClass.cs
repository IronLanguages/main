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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic.Utils;
using System.Text;

namespace System.Dynamic.Binders {
    /// <summary>
    /// Represents a dynamically assigned class.  Expando objects which share the same 
    /// members will share the same class.  Classes are dynamically assigned as the
    /// expando object gains members.
    /// </summary>
    internal class ExpandoClass {
        private readonly string[] _keys;                            // list of names associated with each element in the data array, sorted
        private readonly int _hashCode;                             // pre-calculated hash code of all the keys the class contains
        private Dictionary<int, List<WeakReference>> _transitions;  // cached transitions

        private const int EmptyHashCode = 6551;                     // hash code of the empty ExpandoClass.

        internal static ExpandoClass Empty = new ExpandoClass();                        // The empty Expando class - all Expando objects start off w/ this class.
        
        /// <summary>
        /// Constructs the empty ExpandoClass.  This is the class used when an
        /// empty Expando object is initially constructed.
        /// </summary>
        internal ExpandoClass() {
            _hashCode = EmptyHashCode;
            _keys = new string[0];
        }

        /// <summary>
        /// Constructs a new ExpandoClass that can hold onto the specified keys.  The
        /// keys must be sorted ordinally.  The hash code must be precalculated for 
        /// the keys.
        /// </summary>
        internal ExpandoClass(string[] keys, int hashCode) {
            _hashCode = hashCode;
            _keys = keys;
        }

        /// <summary>
        /// Finds or creates a new ExpandoClass given the existing set of keys
        /// in this ExpandoClass plus the new key to be added.
        /// </summary>
        internal ExpandoClass FindNewClass(string newKey, bool ignoreCase) {
            // just XOR the newKey hash code 
            int hashCode = _hashCode ^ newKey.GetHashCode();

            lock (this) {
                List<WeakReference> infos = GetTransitionList(hashCode);

                for(int i = 0; i<infos.Count; i++) {
                    ExpandoClass klass = infos[i].Target as ExpandoClass;
                    if (klass == null) {
                        infos.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (string.Equals(klass._keys[klass._keys.Length - 1], newKey, GetStringComparison(ignoreCase))) {
                        // the new key is the key we added in this transition
                        return klass;
                    }
                }

                // no applicable transition, create a new one
                string[] keys = new string[_keys.Length + 1];
                Array.Copy(_keys, keys, _keys.Length);
                keys[_keys.Length] = newKey;
                ExpandoClass ec = new ExpandoClass(keys, hashCode);

                infos.Add(new WeakReference(ec));
                return ec;
            }
        }

        private static StringComparison GetStringComparison(bool ignoreCase) {
            return ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        }

        /// <summary>
        /// Gets a new object array for storing the data that matches this
        /// ExpandoClass given the old ExpandoClass and the instances associated 
        /// data array.
        /// </summary>
        internal object[] GetNewKeys(object[] oldData) {
            if (oldData.Length >= _keys.Length) {
                // we have extra space in our buffer, just initialize it to Uninitialized.
                oldData[_keys.Length - 1] = ExpandoObject.Uninitialized;
                return oldData;
            } 

            // we've grown too much - we need a new object array
            object[] res = new object[GetAlignedSize(_keys.Length)];
            Array.Copy(oldData, res, oldData.Length);
            res[oldData.Length] = ExpandoObject.Uninitialized;
            return res;            
        }

        private static int GetAlignedSize(int len) {
            // the alignment of the array for storage of values (must be a power of two)
            const int DataArrayAlignment = 8;                   

            // round up and then mask off lower bits
            return (len + (DataArrayAlignment - 1)) & (~(DataArrayAlignment - 1));
        }
        
        /// <summary>
        /// Gets the lists of transitions that are valid from this ExpandoClass
        /// to an ExpandoClass whos keys hash to the apporopriate hash code.
        /// </summary>
        private List<WeakReference> GetTransitionList(int hashCode) {
            if (_transitions == null) {
                _transitions = new Dictionary<int, List<WeakReference>>();
            }

            List<WeakReference> infos;
            if (!_transitions.TryGetValue(hashCode, out infos)) {
                _transitions[hashCode] = infos = new List<WeakReference>();
            }

            return infos;
        }

        /// <summary>
        /// Gets the index at which the value should be stored for the specified name.
        /// </summary>
        internal int GetValueIndex(string name, bool caseInsensitive) {
            for (int i = 0; i < _keys.Length; i++) {
                if (string.Equals(
                    _keys[i],
                    name,
                    GetStringComparison(caseInsensitive))) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the name of the specified index.  Used for getting the name to 
        /// create a new expando class when all we have is the class and old index.
        /// </summary>
        internal string GetIndexName(int index) {
            return _keys[index];
        }

        /// <summary>
        /// Gets the names of the keys that can be stored in the Expando class.  The
        /// list is sorted ordinally.
        /// </summary>
        internal string[] Keys {
            get {
                return _keys;
            }
        }
    }
}
