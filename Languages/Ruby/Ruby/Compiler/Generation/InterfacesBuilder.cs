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
    public class InterfacesBuilder : IFeatureBuilder {
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
