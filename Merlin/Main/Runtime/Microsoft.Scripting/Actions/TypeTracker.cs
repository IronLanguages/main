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
using System.Reflection;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    public abstract class TypeTracker : MemberTracker, IMembersList {
        internal TypeTracker() {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public abstract Type Type {
            get;
        }

        public abstract bool IsGenericType {
            get;
        }

        public abstract bool IsPublic {
            get;
        }

        #region IMembersList Members

        public virtual IList<string> GetMemberNames() {
            Dictionary<string, string> members = new Dictionary<string, string>();
            CollectMembers(members, Type);

            return MembersToList(members);
        }

        internal static IList<string> MembersToList(Dictionary<string, string> members) {
            List<string> res = new List<string>();
            foreach (string key in members.Keys) {
                res.Add(key);
            }
            return res;
        }

        internal static void CollectMembers(Dictionary<string, string> members, Type t) {
            foreach (MemberInfo mi in t.GetMembers()) {
                if (mi.MemberType != MemberTypes.Constructor) {
                    members[mi.Name] = mi.Name;
                }
            }
        }

        #endregion

        /// <summary>
        /// Enables implicit Type to TypeTracker conversions accross dynamic languages.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static explicit operator Type(TypeTracker tracker) {
            TypeGroup tg = tracker as TypeGroup;
            if (tg != null) {
                Type res;
                if (!tg.TryGetNonGenericType(out res)) {
                    throw ScriptingRuntimeHelpers.SimpleTypeError("expected non-generic type, got generic-only type");
                }
                return res;
            }
            return tracker.Type;
        }
    }
}
