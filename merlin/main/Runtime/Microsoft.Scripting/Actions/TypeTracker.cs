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

        public IList<object> GetMemberNames(CodeContext context) {
            Dictionary<string, string> members = new Dictionary<string, string>();
            foreach (MemberInfo mi in Type.GetMembers()) {
                if (mi.MemberType != MemberTypes.Constructor) {
                    members[mi.Name] = mi.Name;
                }
            }

            List<object> res = new List<object>();
            foreach (string key in members.Keys) {
                res.Add(key);
            }
            return res;
        }

        #endregion

        /// <summary>
        /// Enables implicit Type to TypeTracker conversions accross dynamic languages.
        /// 
        /// TODO: Should be explicit, but that breaks a JS test
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator Type(TypeTracker tracker) {
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
