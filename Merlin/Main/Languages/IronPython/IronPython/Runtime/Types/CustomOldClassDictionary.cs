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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Threading;

namespace IronPython.Runtime.Types {
    [Serializable]
    internal sealed class CustomOldClassDictionaryStorage : StringDictionaryStorage {
        private int _keyVersion;
        private string[] _extraKeys;
        private object[] _values;

        public CustomOldClassDictionaryStorage(string[] extraKeys, int keyVersion) {
            _extraKeys = extraKeys;
            _keyVersion = keyVersion;
            _values = new object[extraKeys.Length];
            for (int i = 0; i < _values.Length; i++) {
                _values[i] = Uninitialized.Instance;
            }
        }

        public override void Add(object key, object value) {
            int ikey = FindKey(key);
            if (ikey != -1) {
                _values[ikey] = value;
                return;
            }

            base.Add(key, value);
        }

        public override void AddNoLock(object key, object value) {
            int ikey = FindKey(key);
            if (ikey != -1) {
                _values[ikey] = value;
                return;
            }

            base.AddNoLock(key, value);
        }

        public override bool Contains(object key) {
            int ikey = FindKey(key);
            if (ikey != -1) {
                return _values[ikey] != Uninitialized.Instance;
            }

            return base.Contains(key);
        }

        public override bool Remove(object key) {
            int ikey = FindKey(key);
            if (ikey != -1) {
                if (Interlocked.Exchange<object>(ref _values[ikey], Uninitialized.Instance) != Uninitialized.Instance) {
                    return true;
                }

                return false;
            }

            return base.Remove(key);
        }

        public override bool TryGetValue(object key, out object value) {
            int ikey = FindKey(key);
            if (ikey != -1) {
                value = _values[ikey];
                if (value != Uninitialized.Instance) {
                    return true;
                }

                value = null;
                return false;
            }

            return base.TryGetValue(key, out value);
        }

        public override int Count {
            get { 
                int count = base.Count;
                foreach (object o in _values) {
                    if (o != Uninitialized.Instance) {
                        count++;
                    }
                }

                return count;
            }
        }

        public override void Clear() {
            for (int i = 0; i < _values.Length; i++) {
                _values[i] = Uninitialized.Instance;
            }

            base.Clear();
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = base.GetItems();

            for (int i = 0; i < _extraKeys.Length; i++) {
                if (!String.IsNullOrEmpty(_extraKeys[i]) && _values[i] != Uninitialized.Instance) {
                    res.Add(new KeyValuePair<object, object>(_extraKeys[i], _values[i]));
                }
            }

            return res;
        }

        public int KeyVersion {
            get {
                return _keyVersion;
            }
        }

        public int FindKey(object key) {
            string strKey = key as string;
            if (strKey != null) {
                return FindKey(strKey);
            }

            return -1;
        }

        public int FindKey(string key) {
            for (int i = 0; i < _extraKeys.Length; i++) {
                if (_extraKeys[i] == key) {
                    return i;
                }
            }
            return -1;
        }

        public object GetValueHelper(int index, object oldInstance) {
            object ret = _values[index];
            if (ret != Uninitialized.Instance) return ret;
            //TODO this should go to a faster path since we know it's not in the dict
            return ((OldInstance)oldInstance).GetBoundMember(null, _extraKeys[index]);
        }

        public bool TryGetValueHelper(int index, object oldInstance, out object res) {
            res = _values[index];
            if (res != Uninitialized.Instance) {
                return true;
            }
            //TODO this should go to a faster path since we know it's not in the dict
            return ((OldInstance)oldInstance).TryGetBoundCustomMember(null, _extraKeys[index], out res);
        }

        public void SetExtraValue(int index, object value) {
            _values[index] = value;
        }
    }
}
