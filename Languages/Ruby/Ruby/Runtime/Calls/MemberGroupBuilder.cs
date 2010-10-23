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
using System.Reflection;
using IronRuby.Builtins;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls {
    internal class MemberGroupBuilder {
        private readonly bool _specialNameOnly;

        private Dictionary<ValueArray<Type>, ClrOverloadInfo> _allMethods;

        internal MemberGroupBuilder(bool specialNameOnly) {
            _specialNameOnly = specialNameOnly;
        }

        private sealed class ClrOverloadInfo {
            public MethodBase Overload { get; set; }
            public RubyMethodGroupInfo Owner { get; set; }
        }

        /// <summary>
        /// There are basically 4 cases:
        /// 1) CLR method of the given name is not defined in the specified type.
        ///    Do nothing, the method will be found as we traverse the hierarhy towards the Kernel module.
        /// 2) Otherwise
        ///    1) There is no RubyMemberInfo of given <c>name</c> present in the (type..Kernel] ancestors.
        ///       We need to search all types in (type..Object] for CLR method overloads.
        ///    2) There is a RubyMemberInfo in a class, say C, in (type..Kernel]. 
        ///       We need to get CLR methods from (type..C) in addition to the members in the type.
        ///        1) C.HidesInheritedOverloads == true
        ///           All overloads of the method we look for are in [type..C).
        ///        2) C.HidesInheritedOverloads == false
        ///           All overloads of the method we look for are in [type..C) and in the RubyMemberInfo.
        /// </summary>
        internal bool TryGetClrMethod(RubyClass/*!*/ cls, Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, string/*!*/ clrName, out RubyMemberInfo method) {

            // declared only:
            MemberInfo[] initialMembers = GetDeclaredClrMethods(type, bindingFlags, clrName);
            int initialVisibleMemberCount = GetVisibleMethodCount(initialMembers);
            if (initialVisibleMemberCount == 0) {
                // case [1]
                method = null;
                return false;
            }

            // inherited overloads:
            List<RubyClass> ancestors = new List<RubyClass>();
            RubyMemberInfo inheritedRubyMember = null;
            bool skipHidden = false;

            cls.ForEachAncestor((module) => {
                if (module != cls) {
                    if (module.TryGetDefinedMethod(name, ref skipHidden, out inheritedRubyMember) && !inheritedRubyMember.IsSuperForwarder) {
                        return true;
                    }

                    // Skip classes that have no tracker, e.g. Fixnum(tracker) <: Integer(null) <: Numeric(null) <: Object(tracker).
                    // Skip interfaces, their methods are not callable => do not include them into a method group.
                    // Skip all classes once hidden sentinel is encountered (no CLR overloads are visible since then).
                    if (!skipHidden && module.TypeTracker != null && module.IsClass) {
                        ancestors.Add((RubyClass)module);
                    }
                }

                // continue:
                return false;
            });

            _allMethods = null;
            if (inheritedRubyMember != null) {
                // case [2.2.2]: add CLR methods from the Ruby member:
                var inheritedGroup = inheritedRubyMember as RubyMethodGroupInfo;
                if (inheritedGroup != null) {
                    AddMethodsOverwriteExisting(inheritedGroup.MethodBases, inheritedGroup.OverloadOwners);
                }
            }

            // populate classes in (type..Kernel] or (type..C) with method groups:
            for (int i = ancestors.Count - 1; i >= 0; i--) {
                var declared = GetDeclaredClrMethods(ancestors[i].TypeTracker.Type, bindingFlags, clrName);
                if (declared.Length != 0 && AddMethodsOverwriteExisting(declared, null)) {
                    // There is no cached method that needs to be invalidated.
                    //
                    // Proof:
                    // Suppose the group being created here overridden an existing method that is cached in a dynamic site invoked on some target class.
                    // Then either the target class is above all ancestors[i] or below some. If it is above then the new group doesn't 
                    // invalidate validity of the site. If it is below then the method resolution for the cached method would create
                    // and store to method tables all method groups in between the target class and the owner of the cached method, including the 
                    // one that contain overloads of ancestors[i]. But no module below inheritedRubyMember contains a method group of the name 
                    // being currently resolved.
                    ancestors[i].AddMethodNoCacheInvalidation(name, MakeAllMethodsGroup(ancestors[i]));
                }
            }

            if (_allMethods != null) {
                // add members declared in self:
                AddMethodsOverwriteExisting(initialMembers, null);

                // return the group, it will be stored in the method table by the caller:
                method = MakeAllMethodsGroup(cls);
            } else {
                method = MakeSingleOwnerGroup(cls, initialMembers, initialVisibleMemberCount);
            }

            return true;
        }

        private MemberInfo[]/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name) {
            // GetMember uses prefix matching if the name ends with '*', add another * to match the original name:
            if (name.LastCharacter() == '*') {
                name += "*";
            }
            return type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
        }

        // Returns the number of methods newly added to the dictionary.
        private bool AddMethodsOverwriteExisting(
            MemberInfo/*!*/[]/*!*/ newOverloads, RubyMethodGroupInfo/*!*/[] overloadOwners) {

            bool anyChange = false;
            for (int i = 0; i < newOverloads.Length; i++) {
                var method = (MethodBase)newOverloads[i];
                if (IsVisible(method)) {
                    var paramTypes = new ValueArray<Type>(ReflectionUtils.GetParameterTypes(method.GetParameters()));
                    if (_allMethods == null) {
                        _allMethods = new Dictionary<ValueArray<Type>, ClrOverloadInfo>();
                    }

                    _allMethods[paramTypes] = new ClrOverloadInfo {
                        Overload = method,
                        Owner = (overloadOwners != null) ? overloadOwners[i] : null
                    };

                    anyChange = true;
                }
            }
            return anyChange;
        }

        private bool IsVisible(MethodBase/*!*/ method) {
            return !method.IsPrivate && (method.IsSpecialName || !_specialNameOnly);
        }

        private int GetVisibleMethodCount(MemberInfo[]/*!*/ members) {
            int count = 0;
            foreach (MethodBase method in members) {
                if (IsVisible(method)) {
                    count++;
                }
            }
            return count;
        }

        private RubyMethodGroupInfo/*!*/ MakeAllMethodsGroup(RubyClass/*!*/ cls) {
            var overloads = new MethodBase[_allMethods.Count];
            var overloadOwners = new RubyMethodGroupInfo[overloads.Length];
            int i = 0;
            foreach (var entry in _allMethods.Values) {
                overloads[i] = entry.Overload;
                overloadOwners[i] = entry.Owner;
                i++;
            }

            var result = new RubyMethodGroupInfo(overloads, cls, overloadOwners, cls.IsSingletonClass);

            // update ownership of overloads owned by the new group:
            foreach (var entry in _allMethods.Values) {
                if (entry.Owner != null) {
                    entry.Owner.CachedInGroup(result);
                } else {
                    entry.Owner = result;
                }
            }

            return result;
        }

        private  RubyMethodGroupInfo/*!*/ MakeSingleOwnerGroup(RubyClass/*!*/ cls, MemberInfo[]/*!*/ members, int visibleMemberCount) {
            var allMethods = new MethodBase[visibleMemberCount];
            for (int i = 0, j = 0; i < members.Length; i++) {
                var method = (MethodBase)members[i];
                if (IsVisible(method)) {
                    allMethods[j++] = method;
                }
            }

            return new RubyMethodGroupInfo(allMethods, cls, null, cls.IsSingletonClass);
        }
    }
}
