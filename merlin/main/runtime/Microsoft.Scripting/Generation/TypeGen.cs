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
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Contracts;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation {
    public sealed class TypeGen {
        private readonly AssemblyGen _myAssembly;
        private readonly TypeBuilder _myType;

        private ILGen _initGen;                        // The IL generator for the .cctor()

        /// <summary>
        /// Gets the Compiler associated with the Type Initializer (cctor) creating it if necessary.
        /// </summary>
        public ILGen TypeInitializer {
            get {
                if (_initGen == null) {
                    _initGen = new ILGen(_myType.DefineTypeInitializer().GetILGenerator());
                }
                return _initGen;
            }
        }

        internal AssemblyGen AssemblyGen {
            get { return _myAssembly; }
        }

        public TypeBuilder TypeBuilder {
            get { return _myType; }
        }

        public TypeGen(AssemblyGen myAssembly, TypeBuilder myType) {
            Assert.NotNull(myAssembly, myType);

            _myAssembly = myAssembly;
            _myType = myType;
        }

        [Confined]
        public override string ToString() {
            return _myType.ToString();
        }

        public Type FinishType() {
            if (_initGen != null) _initGen.Emit(OpCodes.Ret);

            Type ret = _myType.CreateType();

            //Console.WriteLine("finished: " + ret.FullName);
            return ret;
        }

        public FieldBuilder AddStaticField(Type fieldType, string name) {
            return _myType.DefineField(name, fieldType, FieldAttributes.Public | FieldAttributes.Static);
        }

        public FieldBuilder AddStaticField(Type fieldType, FieldAttributes attributes, string name) {
            return _myType.DefineField(name, fieldType, attributes | FieldAttributes.Static);
        }

        public ILGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod) {
            ContractUtils.RequiresNotNull(baseMethod, "baseMethod");

            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            Type[] baseSignature = baseMethod.GetParameters().Map(p => p.ParameterType);
            MethodBuilder mb = _myType.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                baseSignature);

            TypeBuilder.DefineMethodOverride(mb, baseMethod);
            return new ILGen(mb.GetILGenerator());
        }

        private const MethodAttributes MethodAttributesToEraseInOveride =
            MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        public ILGen DefineMethodOverride(MethodInfo baseMethod) {
            MethodAttributes finalAttrs = baseMethod.Attributes & ~MethodAttributesToEraseInOveride;
            Type[] baseSignature = baseMethod.GetParameters().Map(p => p.ParameterType);
            MethodBuilder mb = _myType.DefineMethod(baseMethod.Name, finalAttrs, baseMethod.ReturnType, baseSignature);

            TypeBuilder.DefineMethodOverride(mb, baseMethod);
            return new ILGen(mb.GetILGenerator());
        }
    }
}
