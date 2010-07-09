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
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Threading;

using Microsoft.Contracts;
using Microsoft.Scripting.Actions.Calls;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// MethodGroup's represent a unique collection of method's.  Typically this
    /// unique set is all the methods which are overloaded by the same name including
    /// methods with different arity.  These methods represent a single logically
    /// overloaded element of a .NET type.
    /// 
    /// The base DLR binders will produce MethodGroup's when provided with a MemberGroup
    /// which contains only methods.  The MethodGroup's will be unique instances per
    /// each unique group of methods.
    /// </summary>
    public class MethodGroup : MemberTracker {
        private MethodTracker[] _methods;
        private Dictionary<TypeList, MethodGroup> _boundGenerics;

        internal MethodGroup(params MethodTracker[] methods) {
            _methods = methods;
        }

        public override TrackerTypes MemberType {
            get { return TrackerTypes.MethodGroup; }
        }

        public override Type DeclaringType {
            get { return _methods[0].DeclaringType; }
        }

        public override string Name {
            get { return _methods[0].Name; }
        }

        public bool ContainsInstance {
            get {
                foreach (MethodTracker mt in _methods) {
                    if (!mt.IsStatic) return true;
                }
                return false;
            }
        }

        public bool ContainsStatic {
            get {
                foreach (MethodTracker mt in _methods) {
                    if (mt.IsStatic) return true;
                }
                return false;
            }
        }

        public IList<MethodTracker> Methods {
            get {
                return _methods;
            }
        }

        public MethodBase[] GetMethodBases() {
            MethodBase[] methods = new MethodBase[Methods.Count];
            for (int i = 0; i < Methods.Count; i++) {
                methods[i] = Methods[i].Method;
            }
            return methods;
        }

        public override DynamicMetaObject GetValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type) {
            return base.GetValue(resolverFactory, binder, type);
        }

        public override MemberTracker BindToInstance(DynamicMetaObject instance) {
            if (ContainsInstance) {
                return new BoundMemberTracker(this, instance);
            }

            return this;
        }

        protected internal override DynamicMetaObject GetBoundValue(OverloadResolverFactory resolverFactory, ActionBinder binder, Type type, DynamicMetaObject instance) {
            return binder.ReturnMemberTracker(type, BindToInstance(instance));
        }

        /// <summary>
        /// Returns a BuiltinFunction bound to the provided type arguments.  Returns null if the binding
        /// cannot be performed.
        /// </summary>
        public MethodGroup MakeGenericMethod(Type[] types) {
            TypeList tl = new TypeList(types);

            // check for cached method first...
            MethodGroup mg;
            if (_boundGenerics != null) {
                lock (_boundGenerics) {
                    if (_boundGenerics.TryGetValue(tl, out mg)) {
                        return mg;
                    }
                }
            }

            // Search for generic targets with the correct arity (number of type parameters).
            // Compatible targets must be MethodInfos by definition (constructors never take
            // type arguments).
            List<MethodTracker> targets = new List<MethodTracker>(Methods.Count);
            foreach (MethodTracker mt in Methods) {
                MethodInfo mi = mt.Method;
                if (mi.ContainsGenericParameters && mi.GetGenericArguments().Length == types.Length)
                    targets.Add((MethodTracker)MemberTracker.FromMemberInfo(mi.MakeGenericMethod(types)));
            }

            if (targets.Count == 0) {
                return null;
            }

            // Build a new MethodGroup that will contain targets with bound type arguments & cache it.
            mg = new MethodGroup(targets.ToArray());

            EnsureBoundGenericDict();

            lock (_boundGenerics) {
                _boundGenerics[tl] = mg;
            }

            return mg;
        }

        private void EnsureBoundGenericDict() {
            if (_boundGenerics == null) {
                Interlocked.CompareExchange<Dictionary<TypeList, MethodGroup>>(
                    ref _boundGenerics,
                    new Dictionary<TypeList, MethodGroup>(1),
                    null);
            }
        }

        private class TypeList {
            private Type[] _types;

            public TypeList(Type[] types) {
                Debug.Assert(types != null);
                _types = types;
            }

            [Confined]
            public override bool Equals(object obj) {
                TypeList tl = obj as TypeList;
                if (tl == null || _types.Length != tl._types.Length) return false;

                for (int i = 0; i < _types.Length; i++) {
                    if (_types[i] != tl._types[i]) return false;
                }
                return true;
            }

            [Confined]
            public override int GetHashCode() {
                int hc = 6551;
                foreach (Type t in _types) {
                    hc = (hc << 5) ^ t.GetHashCode();
                }
                return hc;
            }
        }
    }
}
