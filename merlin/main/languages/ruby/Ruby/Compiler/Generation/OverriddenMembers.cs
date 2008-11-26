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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;
using System.Dynamic.Binders;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Generation {
    public abstract partial class ClsTypeEmitter {

        class OverriddenMembers {
            private static readonly Dictionary<Type, OverriddenMembers> _overrideData = new Dictionary<Type, OverriddenMembers>();

            private readonly Dictionary<string/*!*/, List<MethodInfo/*!*/>/*!*/>/*!*/ _methods = new Dictionary<string, List<MethodInfo>>();
            private readonly Dictionary<string/*!*/, List<ExtensionPropertyTracker/*!*/>/*!*/>/*!*/ _properties = new Dictionary<string, List<ExtensionPropertyTracker>>();

            internal static OverriddenMembers GetForType(Type/*!*/ type) {
                OverriddenMembers result;
                lock (_overrideData) {
                    if (!_overrideData.TryGetValue(type, out result)) {
                        _overrideData[type] = result = new OverriddenMembers();
                    }
                }
                return result;
            }

            internal void AddMethod(MethodInfo/*!*/ mi, string/*!*/ newName) {
                List<MethodInfo> methods;
                if (!_methods.TryGetValue(newName, out methods)) {
                    _methods[newName] = methods = new List<MethodInfo>();
                }

                methods.Add(mi);
            }

            internal ExtensionPropertyTracker AddPropertyInfo(string/*!*/ propName, MethodInfo get, MethodInfo set) {
                MethodInfo mi = get ?? set;

                List<ExtensionPropertyTracker> trackers;
                if (!_properties.TryGetValue(propName, out trackers)) {
                    _properties[propName] = trackers = new List<ExtensionPropertyTracker>();
                }

                ExtensionPropertyTracker res;
                for (int i = 0; i < trackers.Count; i++) {
                    if (trackers[i].DeclaringType == mi.DeclaringType) {
                        trackers[i] = res = new ExtensionPropertyTracker(
                            propName,
                            get ?? trackers[i].GetGetMethod(),
                            set ?? trackers[i].GetSetMethod(),
                            null,
                            mi.DeclaringType
                        );
                        return res;
                    }
                }

                trackers.Add(
                    res = new ExtensionPropertyTracker(
                        propName,
                        get,
                        set,
                        null,
                        mi.DeclaringType
                    )
                );

                return res;
            }

            internal List<MemberTracker/*!*/> GetMembers(string/*!*/ name) {
                List<MemberTracker> members = null;

                List<MethodInfo> methodList;
                if (_methods.TryGetValue(name, out methodList) && methodList.Count > 0) {
                    foreach (MethodInfo mi in methodList) {
                        if (members == null) members = new List<MemberTracker>();
                        members.Add(MemberTracker.FromMemberInfo(mi));
                    }
                }

                List<ExtensionPropertyTracker> propertyList;
                if (_properties.TryGetValue(name, out propertyList) && propertyList.Count > 0) {
                    foreach (ExtensionPropertyTracker tracker in propertyList) {
                        if (members == null) members = new List<MemberTracker>();
                        members.Add(tracker);
                    }
                }

                return members;
            }
        }

        class OverrideBuilder {
            private readonly Type/*!*/ _baseType;

            internal OverrideBuilder(Type baseType) {
                _baseType = baseType;
            }

            // TODO: if it's an indexer then we want to override get_Item/set_Item methods
            // which map to [] and []=

            internal void AddBaseMethods(Type finishedType, SpecialNames specialNames) {
                // "Adds" base methods to super type - this makes super(...).xyz to work - otherwise 
                // we'd return a function that did a virtual call resulting in a stack overflow.
                OverriddenMembers overrides = OverriddenMembers.GetForType(finishedType);

                foreach (MethodInfo mi in finishedType.GetMethods()) {
                    if (!ShouldOverrideVirtual(mi)) continue;

                    string methodName = mi.Name;
                    if (methodName.StartsWith(BaseMethodPrefix) || methodName.StartsWith(FieldGetterPrefix) || methodName.StartsWith(FieldSetterPrefix)) {
                        foreach (string newName in specialNames.GetBaseName(mi)) {
                            if (mi.IsSpecialName && (newName.StartsWith("get_") || newName.StartsWith("set_"))) {
                                StoreOverriddenProperty(overrides, mi, newName);
                            } else if (mi.IsSpecialName && (newName.StartsWith(FieldGetterPrefix) || newName.StartsWith(FieldSetterPrefix))) {
                                StoreOverriddenField(overrides, mi, newName);
                            } else {
                                StoreOverriddenMethod(overrides, mi, newName);
                            }
                        }
                    }
                }
            }

            private void StoreOverriddenProperty(OverriddenMembers overrides, MethodInfo mi, string newName) {
                string propName = newName.Substring(4); // get_ or set_
                foreach (PropertyInfo pi in _baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
                    if (pi.Name == propName) {
                        if (newName.StartsWith("get_")) {
                            overrides.AddMethod(mi, propName);
                        } else if (newName.StartsWith("set_")) {
                            overrides.AddMethod(mi, propName + "=");
                        }
                    }
                }
            }

            private void StoreOverriddenField(OverriddenMembers overrides, MethodInfo mi, string newName) {
                string fieldName = newName.Substring(FieldGetterPrefix.Length); // get_ or set_
                foreach (FieldInfo pi in _baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
                    if (pi.Name == fieldName) {
                        if (newName.StartsWith(FieldGetterPrefix)) {
                            overrides.AddMethod(mi, fieldName);
                        } else if (newName.StartsWith(FieldSetterPrefix)) {
                            overrides.AddMethod(mi, fieldName + "=");
                        }
                    }
                }
            }

            private void StoreOverriddenMethod(OverriddenMembers overrides, MethodInfo mi, string newName) {
                MemberInfo[] members = _baseType.GetMember(newName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                Debug.Assert(members.Length > 0, String.Format("{0} from {1}", newName, _baseType.Name));
                Type declType = members[0].DeclaringType;

                overrides.AddMethod(mi, newName);
            }
        }

        public static List<MemberTracker/*!*/> GetOverriddenMembersForType(Type/*!*/ type, string/*!*/ name) {
            return OverriddenMembers.GetForType(type).GetMembers(name);
        }
    }
}
