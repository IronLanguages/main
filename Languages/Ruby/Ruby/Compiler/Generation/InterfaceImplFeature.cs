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
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler.Generation {
    internal sealed class InterfaceImplFeature : ITypeFeature {
        internal static readonly InterfaceImplFeature/*!*/ Empty = new InterfaceImplFeature(Type.EmptyTypes);

        private readonly Type/*!*/[]/*!*/ _interfaces;
        private readonly int _hash;

        internal InterfaceImplFeature(IList<Type/*!*/>/*!*/ interfaceTypes) {
            Assert.NotNull(interfaceTypes);

            List<Type> types = new List<Type>(interfaceTypes.Count);
            foreach (Type t in interfaceTypes) {
                AddInterface(types, t);
            }
            types.Sort((a, b) => String.CompareOrdinal(a.FullName, b.FullName));
            _interfaces = types.ToArray();

            _hash = typeof(InterfaceImplFeature).GetHashCode();
            foreach (Type t in _interfaces) {
                _hash ^= t.GetHashCode();
            }
        }

        internal static ITypeFeature/*!*/ Create(IList<Type/*!*/>/*!*/ interfaceTypes) {
            if (interfaceTypes.Count == 0) {
                return InterfaceImplFeature.Empty;
            }
            return new InterfaceImplFeature(interfaceTypes);
        }

        private static void AddInterface(List<Type/*!*/>/*!*/ types, Type/*!*/ type) {
            Assert.NotNull(type);
            Assert.Equals(true, type.IsInterface && !type.ContainsGenericParameters);

            for (int i = 0; i < types.Count; i++) {
                Type t = types[i];
                if (t == type || type.IsAssignableFrom(t)) {
                    // This interface is already included in the list
                    return;
                }
                if (t.IsAssignableFrom(type)) {
                    // We are going to supercede this interface
                    types.RemoveAt(i--);
                }
            }
            types.Add(type);
        }

        public bool CanInherit {
            get { return true; }
        }

        public bool IsImplementedBy(Type/*!*/ type) {
            // TODO: If the type's implementation of each interface is a delegating implementation, return true
            return (_interfaces.Length == 0);
        }

        public IFeatureBuilder/*!*/ MakeBuilder(TypeBuilder/*!*/ tb) {
            return new InterfacesBuilder(tb, _interfaces);
        }

        public override int GetHashCode() {
            return typeof(InterfaceImplFeature).GetHashCode();
        }

        public override bool Equals(object obj) {
            InterfaceImplFeature other = obj as InterfaceImplFeature;
            if (other == null) return false;
            if (_interfaces.Length != other._interfaces.Length) return false;

            for (int i = 0; i < _interfaces.Length; i++) {
                if (!_interfaces[i].Equals(other._interfaces[i])) {
                    return false;
                }
            }

            return true;
        }
    }
}
