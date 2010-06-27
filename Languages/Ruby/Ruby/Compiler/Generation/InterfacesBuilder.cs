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
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;

namespace IronRuby.Compiler.Generation {
    public class InterfacesBuilder : IFeatureBuilder {

        #region ITypeFeature

        sealed class TypeFeature : ITypeFeature {
            internal static readonly TypeFeature/*!*/ Empty = new TypeFeature(Type.EmptyTypes);

            private readonly Type/*!*/[]/*!*/ _interfaces;
            private readonly int _hash;

            internal TypeFeature(IList<Type/*!*/>/*!*/ interfaceTypes) {
                Assert.NotNull(interfaceTypes);

                List<Type> types = new List<Type>(interfaceTypes.Count);
                foreach (Type t in interfaceTypes) {
                    AddInterface(types, t);
                }
                types.Sort((a, b) => String.CompareOrdinal(a.FullName, b.FullName));
                _interfaces = types.ToArray();

                _hash = typeof(TypeFeature).GetHashCode();
                foreach (Type t in _interfaces) {
                    _hash ^= t.GetHashCode(); // 
                }
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
                return typeof(TypeFeature).GetHashCode();
            }

            public override bool Equals(object obj) {
                TypeFeature other = obj as TypeFeature;
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

        internal static ITypeFeature/*!*/ MakeFeature(IList<Type/*!*/>/*!*/ interfaceTypes) {
            if (interfaceTypes.Count == 0) {
                return TypeFeature.Empty;
            }
            return new TypeFeature(interfaceTypes);
        }

        #endregion

        private readonly TypeBuilder/*!*/ _tb;
        private readonly Type/*!*/[]/*!*/ _interfaces;

        internal InterfacesBuilder(TypeBuilder/*!*/ tb, Type/*!*/[]/*!*/ interfaces) {
            _tb = tb;
            _interfaces = interfaces;
        }

        public void Implement(ClsTypeEmitter/*!*/ emitter) {
            // TODO: Exclude interfaces already implemented in base class feature sets
            // TODO: Exclude IDynamicMetaObjectProvider, IRubyObject, etc. or handle specially
            Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
            foreach (Type interfaceType in _interfaces) {
                if (interfaceType != typeof(IRubyType) && 
                    interfaceType != typeof(IRubyObject) && 
#if !SILVERLIGHT
                    interfaceType != typeof(ICustomTypeDescriptor) &&
                    interfaceType != typeof(ISerializable) &&
#endif
                    interfaceType != typeof(IRubyDynamicMetaObjectProvider)) {
                    _tb.AddInterfaceImplementation(interfaceType);
                    ImplementInterface(emitter, interfaceType, doneTypes);
                }
            }
        }

        private void ImplementInterface(ClsTypeEmitter/*!*/ emitter, Type/*!*/ interfaceType, Dictionary<Type/*!*/, bool>/*!*/ doneTypes) {
            if (doneTypes.ContainsKey(interfaceType)) {
                return;
            }
            doneTypes.Add(interfaceType, true);
            emitter.OverrideMethods(interfaceType);

            foreach (Type t in interfaceType.GetInterfaces()) {
                ImplementInterface(emitter, t, doneTypes);
            }
        }
    }
}
