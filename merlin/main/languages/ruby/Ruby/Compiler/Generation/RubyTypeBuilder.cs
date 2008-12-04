/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Generation {
    internal class RubyTypeBuilder : IFeatureBuilder {

        #region ITypeFeature

        sealed class TypeFeature : ITypeFeature {
            public bool CanInherit {
                get { return true; }
            }

            public bool IsImplementedBy(Type/*!*/ type) {
                return typeof(IRubyObject).IsAssignableFrom(type);
            }

            public IFeatureBuilder/*!*/ MakeBuilder(TypeBuilder/*!*/ tb) {
                return new RubyTypeBuilder(tb);
            }

            public override int GetHashCode() {
                return typeof(TypeFeature).GetHashCode();
            }

            public override bool Equals(object obj) {
                return Object.ReferenceEquals(obj, _feature);
            }
        }

        static readonly TypeFeature/*!*/ _feature = new TypeFeature();

        public static ITypeFeature/*!*/ Feature {
            get { return _feature; }
        }

        #endregion

        protected readonly TypeBuilder/*!*/ _tb;
        protected readonly FieldBuilder/*!*/ _classField;
        protected readonly FieldBuilder/*!*/ _instanceDataField;

        internal RubyTypeBuilder(TypeBuilder/*!*/ tb) {
            _tb = tb;
            _classField = _tb.DefineField("_class", typeof(RubyClass), FieldAttributes.Private | FieldAttributes.InitOnly);
            _instanceDataField = _tb.DefineField("_instanceData", typeof(RubyInstanceData), FieldAttributes.Private);
        }

        public void Implement(ClsTypeEmitter/*!*/ emitter) {
            DefineConstructors();
            DefineRubyObjectImplementation();
            DefineSerializer();

            RubyTypeEmitter re = (emitter as RubyTypeEmitter);
            Assert.NotNull(re);
            re.ClassField = _classField;

            DefineDynamicObjectImplementation();

#if !SILVERLIGHT // ICustomTypeDescriptor
            DefineCustomTypeDescriptor();
#endif

            // we need to get the right execution context
#if OBSOLETE
            // TODO: remove the need for these methods to be special cased
            EmitOverrideEquals(typeGen);
            EmitOverrideGetHashCode(typeGen);
#endif
        }

#if !SILVERLIGHT
        private static readonly Type/*!*/[]/*!*/ _deserializerSignature = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
#endif
        private static readonly Type/*!*/[]/*!*/ _classArgSignature = new Type[] { typeof(RubyClass) };

        private static bool IsAvailable(MethodBase method) {
            return method != null && !method.IsPrivate && !method.IsFamilyAndAssembly;
        }

        private void DefineConstructors() {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            ConstructorInfo emptyCtor = _tb.BaseType.GetConstructor(bindingFlags, null, Type.EmptyTypes, null);
            ConstructorInfo classArgCtor = null;

            bool deserializerFound = false;
            foreach (var baseCtor in _tb.BaseType.GetConstructors(bindingFlags)) {
                if (!baseCtor.IsPublic && !baseCtor.IsFamily) {
                    continue;
                }

                int oldLength = baseCtor.GetParameters().Length;
                List<Type> newParams = new List<Type>(oldLength + 1);
                foreach (var param in baseCtor.GetParameters()) {
                    newParams.Add(param.ParameterType);
                }

                int offset = 1;
                bool isDeserializer = false;
                if (oldLength > 0 && newParams[0] == typeof(RubyClass)) {
                    // Build a simple pass-through constructor
                    offset = 0;
#if !SILVERLIGHT
                } else if (oldLength == 2 && newParams[0] == typeof(SerializationInfo) && newParams[1] == typeof(StreamingContext)) {
                    // Build a deserializer
                    deserializerFound = true;
                    isDeserializer = true;
                    offset = 0;
#endif
                } else {
                    // Special-case for Exception
                    if (_tb.IsSubclassOf(typeof(Exception)) && IsAvailable(emptyCtor)) {
                        if (oldLength == 0) {
                            // Skip this constructor; it would conflict with the one we're going to build next
                            continue;
                        } else if (oldLength == 1 && newParams[0] == typeof(string)) {
                            // Special case exceptions to improve interop. Ruby's default message for an exception is the name of the exception class.
                            BuildExceptionConstructor(baseCtor);
                        }
                    }

                    // Add RubyClass to the head of the parameter list
                    newParams.Insert(0, typeof(RubyClass));
                }

                // Build a new constructor based on this base class ctor
                ConstructorBuilder cb = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, newParams.ToArray());
                ILGen il = new ILGen(cb.GetILGenerator());
                il.EmitLoadArg(0);
                for (int i = 1; i < newParams.Count; i++) {
                    il.EmitLoadArg(i + offset);
                }
                il.Emit(OpCodes.Call, baseCtor);
                if (!isDeserializer) {
                    // ctor(RubyClass! class, ...) : ... { this._class = class; } 
                    il.EmitLoadArg(0);
                    il.EmitLoadArg(1);
                    il.EmitFieldSet(_classField);
                } else {
                    // ctor(SerializationInfo! info, StreamingContext! context) : base {
                    //   RubyOps.DeserializeObject(out this._instanceData, out this._class, info);
                    // }
                    il.EmitLoadArg(0);
                    il.EmitFieldAddress(_instanceDataField);
                    il.EmitLoadArg(0);
                    il.EmitFieldAddress(_classField);
                    il.EmitLoadArg(1);
                    il.EmitCall(typeof(RubyOps).GetMethod("DeserializeObject"));
                }
                il.Emit(OpCodes.Ret);

                if (oldLength == 0) {
                    classArgCtor = cb;
                }
            }
#if !SILVERLIGHT
            if (classArgCtor != null && !deserializerFound) {
                // We didn't previously find a deserialization constructor.  If we can, build one now.
                BuildDeserializationConstructor(classArgCtor);
            }
#endif
        }

        private void BuildExceptionConstructor(ConstructorInfo baseCtor) {
            // ctor(RubyClass! class) : base(RubyOps.GetDefaultExceptionMessage(class)) {
            //   this._class = class;
            // }
            ConstructorBuilder ctor = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, _classArgSignature);
            ILGen il = new ILGen(ctor.GetILGenerator());
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Call, Methods.GetDefaultExceptionMessage);
            il.Emit(OpCodes.Call, baseCtor);
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitFieldSet(_classField);
            il.Emit(OpCodes.Ret);
        }

#if !SILVERLIGHT
        private void BuildDeserializationConstructor(ConstructorInfo thisCtor) {
            // ctor(SerializationInfo! info, StreamingContext! context) : this((RubyClass)context.Context) {
            //   RubyOps.DeserializeObject(out this._instanceData, out this._class, info);
            // }
            ConstructorBuilder ctor = _tb.DefineConstructor(MethodAttributes.Family, CallingConventions.Standard, _deserializerSignature);
            ILGen il = new ILGen(ctor.GetILGenerator());
            il.EmitLoadArg(0);
            il.EmitLoadArgAddress(2);
            il.EmitCall(typeof(StreamingContext).GetProperty("Context").GetGetMethod());
            il.Emit(OpCodes.Castclass, typeof(RubyClass));
            il.Emit(OpCodes.Call, thisCtor);

            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_classField);
            il.EmitLoadArg(1);
            il.EmitCall(typeof(RubyOps).GetMethod("DeserializeObject"));
            il.Emit(OpCodes.Ret);
        }
#endif

        private void DefineRubyObjectImplementation() {
            _tb.AddInterfaceImplementation(typeof(IRubyObject));

            ILGen il;

            // RubyClass! IRubyObject.RubyClass { get { return this._class; } }
            il = DefineMethodOverride(_tb, typeof(IRubyObject).GetProperty(RubyObject.ClassPropertyName).GetGetMethod());
            il.EmitLoadArg(0);
            il.EmitFieldGet(_classField);
            il.Emit(OpCodes.Ret);

            // RubyInstanceData IRubyObject.TryGetInstanceData() { return this._instanceData; }
            il = DefineMethodOverride(_tb, typeof(IRubyObject).GetMethod("TryGetInstanceData"));
            il.EmitLoadArg(0);
            il.EmitFieldGet(_instanceDataField);
            il.Emit(OpCodes.Ret);

            // RubyInstanceData! IRubyObject.GetInstanceData() { return RubyOps.GetInstanceData(ref _instanceData); }
            il = DefineMethodOverride(_tb, typeof(IRubyObject).GetMethod("GetInstanceData"));
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitCall(typeof(RubyOps).GetMethod("GetInstanceData"));
            il.Emit(OpCodes.Ret);
        }

        private void DefineSerializer() {
#if !SILVERLIGHT
            ILGen il;
            _tb.AddInterfaceImplementation(typeof(ISerializable));

            //  void ISerializable.GetObjectData(SerializationInfo! info, StreamingContext! context) {
            //      base.GetObjectData(info, context);
            //      RubyOps.SerializeObject(_instanceData, _class, info);
            //  }

            MethodInfo baseSerializer;
            if (typeof(ISerializable).IsAssignableFrom(_tb.BaseType)) {
                InterfaceMapping map = _tb.BaseType.GetInterfaceMap(typeof(ISerializable));
                baseSerializer = map.TargetMethods[0];
            } else {
                baseSerializer = null;
            }

            il = DefinePrivateInterfaceMethodOverride(_tb, typeof(ISerializable).GetMethod("GetObjectData"));
            if (baseSerializer != null) {
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                il.EmitLoadArg(2);
                il.Emit(OpCodes.Call, baseSerializer); // Nonvirtual call
            }

            il.EmitLoadArg(0);
            il.EmitFieldGet(_instanceDataField);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_classField);
            il.EmitLoadArg(1);
            il.EmitCall(typeof(RubyOps).GetMethod("SerializeObject"));
            il.Emit(OpCodes.Ret);
#endif
        }

        // we need to get the right execution context
#if OBSOLETE
        private static void EmitOverrideEquals(TypeGen typeGen) {
            Type baseType = typeGen.TypeBuilder.BaseType;
            MethodInfo baseMethod = baseType.GetMethod("Equals", new Type[] { typeof(object) });
            Compiler cg = typeGen.DefineMethodOverride(baseMethod);

            // Check if an "eql?" method exists on this class
            cg.EmitType(typeGen.TypeBuilder);
            cg.EmitString("eql?");
            cg.EmitCall(typeof(RubyOps).GetMethod("ResolveDeclaredInstanceMethod"));
            Label callBase = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse_S, callBase);

            // If so, call it
            cg.EmitThis();
            cg.EmitArgGet(0);
            cg.EmitCall(typeof(RubyOps).GetMethod("CallEql"));
            cg.EmitReturn();

            // Otherwise, call base class
            cg.MarkLabel(callBase);
            cg.EmitThis();
            cg.EmitArgGet(0);
            cg.Emit(OpCodes.Call, baseMethod); // base call must be non-virtual
            cg.EmitReturn();

            cg.Finish();
        }

        private static void EmitOverrideGetHashCode(TypeGen typeGen) {
            Type baseType = typeGen.TypeBuilder.BaseType;
            MethodInfo baseMethod = baseType.GetMethod("GetHashCode", Type.EmptyTypes);
            Compiler cg = typeGen.DefineMethodOverride(baseMethod);

            // Check if a "hash" method exists on this class
            cg.EmitType(typeGen.TypeBuilder);
            cg.EmitString("hash");
            cg.EmitCall(typeof(RubyOps).GetMethod("ResolveDeclaredInstanceMethod"));
            Label callBase = cg.DefineLabel();
            cg.Emit(OpCodes.Brfalse_S, callBase);

            // If so, call it
            cg.EmitThis();
            cg.EmitCall(typeof(RubyOps).GetMethod("CallHash"));
            cg.EmitReturn();

            // Otherwise, call base class
            cg.MarkLabel(callBase);
            cg.EmitThis();
            cg.Emit(OpCodes.Call, baseMethod); // base call must be non-virtual
            cg.EmitReturn();

            cg.Finish();
        }
#endif

        private void DefineDynamicObjectImplementation() {
            _tb.AddInterfaceImplementation(typeof(IDynamicObject));

            // MetaObject! IDynamicObject.GetMetaObject(Expression! parameter) {
            //   return RubyOps.GetMetaObject(this, parameter);
            // }

            MethodInfo decl = typeof(IDynamicObject).GetMethod("GetMetaObject");
            MethodBuilder impl = _tb.DefineMethod(
                decl.Name,
                decl.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.ReservedMask),
                decl.ReturnType,
                ReflectionUtils.GetParameterTypes(decl.GetParameters())
            );

            ILGen il = new ILGen(impl.GetILGenerator());
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitCall(Methods.GetMetaObject);
            il.Emit(OpCodes.Ret);

            _tb.DefineMethodOverride(impl, decl);
        }

        private static ILGen/*!*/ DefineMethodOverride(TypeBuilder/*!*/ tb, MethodInfo/*!*/ decl) {
            MethodBuilder impl;
            return DefineMethodOverride(tb, decl, out impl);
        }

        private static ILGen/*!*/ DefineMethodOverride(TypeBuilder/*!*/ tb, MethodInfo/*!*/ decl, out MethodBuilder/*!*/ impl) {
            impl = tb.DefineMethod(
                decl.Name,
                decl.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.ReservedMask),
                decl.ReturnType,
                ReflectionUtils.GetParameterTypes(decl.GetParameters())
            );

            tb.DefineMethodOverride(impl, decl);
            return new ILGen(impl.GetILGenerator());
        }

        private static ILGen/*!*/ DefinePrivateInterfaceMethodOverride(TypeBuilder/*!*/ tb, MethodInfo/*!*/ decl) {
            MethodBuilder impl;
            return DefinePrivateInterfaceMethodOverride(tb, decl, out impl);
        }

        private static ILGen/*!*/ DefinePrivateInterfaceMethodOverride(TypeBuilder/*!*/ tb, MethodInfo/*!*/ decl, out MethodBuilder/*!*/ impl) {
            string name = decl.DeclaringType.Name + "." + decl.Name;
            //MethodAttributes attributes = decl.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.ReservedMask | MethodAttributes.MemberAccessMask)
            //    | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Private;
            MethodAttributes attributes = decl.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attributes |= MethodAttributes.NewSlot | MethodAttributes.Final;
            impl = tb.DefineMethod(
                name,
                attributes,
                decl.ReturnType,
                ReflectionUtils.GetParameterTypes(decl.GetParameters())
            );

            tb.DefineMethodOverride(impl, decl);
            return new ILGen(impl.GetILGenerator());
        }

#if !SILVERLIGHT // ICustomTypeDescriptor
        private void DefineCustomTypeDescriptor() {
            _tb.AddInterfaceImplementation(typeof(ICustomTypeDescriptor));

            foreach (MethodInfo m in typeof(ICustomTypeDescriptor).GetMethods()) {
                ImplementCTDOverride(m);
            }
        }

        private void ImplementCTDOverride(MethodInfo m) {
            MethodBuilder builder;
            ILGen il = DefinePrivateInterfaceMethodOverride(_tb, m, out builder);
            il.EmitLoadArg(0);

            ParameterInfo[] pis = m.GetParameters();
            Type[] paramTypes = new Type[pis.Length + 1];
            paramTypes[0] = typeof(object);
            for (int i = 0; i < pis.Length; i++) {
                il.EmitLoadArg(i + 1);
                paramTypes[i + 1] = pis[i].ParameterType;
            }

            il.EmitCall(typeof(CustomTypeDescHelpers), m.Name, paramTypes);
            il.EmitBoxing(m.ReturnType);
            il.Emit(OpCodes.Ret);
        }
#endif
    }
}
