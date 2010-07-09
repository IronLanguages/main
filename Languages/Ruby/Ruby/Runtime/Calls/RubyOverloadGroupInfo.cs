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

using System.Diagnostics;
using System.Reflection;
using IronRuby.Builtins;
using Microsoft.Scripting.Actions.Calls;

namespace IronRuby.Runtime.Calls {
    /// <summary>
    /// A group of CLR method overloads that might be declared in multiple types and partially hidden by Ruby method definitions.
    /// </summary>
    internal sealed class RubyOverloadGroupInfo : RubyMethodGroupInfo {
        // A method group that owns each overload or null if all overloads are owned by this group.
        // A null member also marks an overload owned by this group.
        private readonly RubyOverloadGroupInfo[] _overloadOwners; // immutable

        #region Mutable state guarded by ClassHierarchyLock

        // Maximum over levels of all classes that are caching overloads from this group. -1 if there is no such class.
        private int _maxCachedOverloadLevel = -1;

        #endregion

        internal RubyOverloadGroupInfo(OverloadInfo/*!*/[]/*!*/ methods, RubyModule/*!*/ declaringModule,
            RubyOverloadGroupInfo/*!*/[] overloadOwners, bool isStatic)
            : base(methods, declaringModule, isStatic) {
            Debug.Assert(overloadOwners == null || methods.Length == overloadOwners.Length);

            _overloadOwners = overloadOwners;
        }

        internal override bool IsRubyMember {
            get { return false; }
        }

        internal RubyOverloadGroupInfo[] OverloadOwners {
            get { return _overloadOwners; }
        }

        internal int MaxCachedOverloadLevel {
            get { return _maxCachedOverloadLevel; }
        }

        // Called on this group whenever other group includes some overloads from this group.
        // Updates maxCachedOverloadLevel - the max. class hierarchy level which caches an overload owned by this group.
        internal void CachedInGroup(RubyMethodGroupInfo/*!*/ group) {
            Context.RequiresClassHierarchyLock();

            int groupLevel = ((RubyClass)group.DeclaringModule).Level;
            if (_maxCachedOverloadLevel < groupLevel) {
                _maxCachedOverloadLevel = groupLevel;
            }
        }

        // Called whenever this group is used in a dynamic site. 
        // We need to mark "invalidate sites on override" on all owners of the overloads stored in this group so that
        // whenever any of them are overridden the sites are invalidated.
        //
        // A - MG{f(T1)}
        // ^
        // B                 2) def f
        // ^ 
        // C - MG{f(T1), f(T2)}
        // ^
        // D                 1) D.new.f()
        //
        internal override void SetInvalidateSitesOnOverride() {
            Context.RequiresClassHierarchyLock();

            SetInvalidateSitesOnOverride(this);

            // Do not invalidate recursively. Only method groups that are listed need invalidation. 
            if (_overloadOwners != null) {
                foreach (var overloadOwner in _overloadOwners) {
                    if (overloadOwner != null) {
                        SetInvalidateSitesOnOverride(overloadOwner);
                    }
                }
            }
        }
    }
}
