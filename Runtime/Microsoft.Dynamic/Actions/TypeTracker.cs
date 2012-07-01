/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

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
            var members = new HashSet<string>();
            GetMemberNames(Type, members);
            return members.ToArray();
        }

        internal static void GetMemberNames(Type type, HashSet<string> result) {
            foreach (Type ancestor in type.Ancestors()) {
                foreach (MemberInfo mi in ancestor.GetDeclaredMembers()) {
                    if (!(mi is ConstructorInfo)) {
                        result.Add(mi.Name);
                    }
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

        private static readonly Dictionary<Type, TypeTracker> _typeCache = new Dictionary<Type, TypeTracker>();

        public static TypeTracker GetTypeTracker(Type type) {
            TypeTracker res;

            lock (_typeCache) {
                if (!_typeCache.TryGetValue(type, out res)) {
                    _typeCache[type] = res = new NestedTypeTracker(type);
                }
            }

            return res;
        }
    }
}
