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
using System.Reflection;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime {
    /// <summary>
    /// Enables lazy initialization of module dictionaries.
    /// </summary>
    class ModuleDictionaryStorage : StringDictionaryStorage {
        private Type/*!*/ _type;
        private bool _cleared;

        public ModuleDictionaryStorage(Type/*!*/ moduleType) {
            Debug.Assert(moduleType != null);

            _type = moduleType;
        }

        public override bool Remove(object key) {
            string strKey = key as string;
            if (strKey == null) {
                return base.Remove(key);
            }

            bool found = base.Remove(key);
            object value;
            if (TryGetLazyValue(strKey, out value)) {
                // hide the deleted value
                base.Add(key, Uninitialized.Instance);
                found = true;
            }

            return found;
        }

        protected virtual void LazyAdd(object name, object value) {
            Add(name, value);
        }

        public override bool Contains(object key) {
            object dummy;
            return TryGetValue(key, out dummy);
        }

        public override void Clear() {
            _cleared = true;
            base.Clear();
        }

        public override List<KeyValuePair<object, object>> GetItems() {
            List<KeyValuePair<object, object>> res = new List<KeyValuePair<object, object>>();

            foreach (KeyValuePair<object, object> kvp in base.GetItems()) {
                if (kvp.Value != Uninitialized.Instance) {
                    res.Add(kvp);
                }
            }

            MemberInfo[] members = _type.GetMembers();
            foreach (MemberInfo mi in members) {
                if (base.Contains(mi.Name)) continue;

                object value;
                if (TryGetLazyValue(mi.Name, out value)) {
                    res.Add(new KeyValuePair<object, object>(mi.Name, value));
                }
            }
            return res;
        }

        public override int Count {
            get {
                // need to ensure we're fully populated
                GetItems();
                return base.Count;
            }
        }

        private bool TryGetLazyValue(string name, out object value) {
            return TryGetLazyValue(name, true, out value);
        }

        private bool TryGetLazyValue(string name, bool publish, out object value) {
            if (!_cleared) {
                MemberInfo[] members = NonHiddenMembers(GetMember(name));
                if (members.Length > 0) {
                    // we only support fields, methods, and nested types in modules.
                    switch (members[0].MemberType) {
                        case MemberTypes.Field:
                            Debug.Assert(members.Length == 1);

                            value = ((FieldInfo)members[0]).GetValue(null);
                            if (publish) {
                                LazyAdd(name, value);
                            }
                            return true;
                        case MemberTypes.Method:
                            if (!((MethodInfo)members[0]).IsSpecialName) {
                                value = BuiltinFunction.MakeFunction(
                                    name,
                                    ArrayUtils.ConvertAll<MemberInfo, MethodInfo>(members, delegate(MemberInfo mi) { return (MethodInfo)mi; }),
                                    members[0].DeclaringType
                                    );

                                if (publish) {
                                    LazyAdd(name, value);
                                }
                                return true;
                            }
                            break;
                        case MemberTypes.Property:
                            Debug.Assert(members.Length == 1);

                            value = ((PropertyInfo)members[0]).GetValue(null, ArrayUtils.EmptyObjects);

                            if (publish) {
                                LazyAdd(name, value);
                            }

                            return true;
                        case MemberTypes.NestedType:
                            if (members.Length == 1) {
                                value = DynamicHelpers.GetPythonTypeFromType((Type)members[0]);
                            } else {
                                TypeTracker tt = (TypeTracker)MemberTracker.FromMemberInfo(members[0]);
                                for (int i = 1; i < members.Length; i++) {
                                    tt = TypeGroup.UpdateTypeEntity(tt, (TypeTracker)MemberTracker.FromMemberInfo(members[i]));
                                }

                                value = tt;
                            }

                            if (publish) {
                                LazyAdd(name, value);
                            }
                            return true;
                    }
                }
            }

            value = null;
            return false;
        }

        private static MemberInfo[] NonHiddenMembers(MemberInfo[] members) {
            List<MemberInfo> res = new List<MemberInfo>(members.Length);
            foreach (MemberInfo t in members) {
                if (t.IsDefined(typeof(PythonHiddenAttribute), false)) {
                    continue;
                }

                res.Add(t);
            }
            return res.ToArray();
        }

        private MemberInfo[] GetMember(string name) {
            return _type.GetMember(name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
        }

        public override bool TryGetValue(object key, out object value) {
            if (base.TryGetValue(key, out value)) {
                return value != Uninitialized.Instance;
            }

            string strKey = key as string;
            if(strKey != null) {
                return TryGetLazyValue(strKey, out value);
            }

            return false;
        }

        public virtual void Reload() {
            foreach (KeyValuePair<object, object> kvp in base.GetItems()) {
                if (kvp.Value == Uninitialized.Instance) {
                    // hiding a member
                    base.Remove(kvp.Key);
                } else {
                    // member exists, need to remove it from the base class
                    // in case it differs from the member we actually have.
                    string strKey = kvp.Key as string;
                    if (strKey != null && GetMember(strKey).Length > 0) {
                        base.Remove(kvp.Key);
                    }
                }
            }
        }
    }
}
