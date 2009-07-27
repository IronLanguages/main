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
using IronRuby.Runtime.Calls;
using System.Diagnostics;

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
        protected readonly FieldBuilder/*!*/ _immediateClassField;
        protected readonly FieldBuilder/*!*/ _instanceDataField;

        internal RubyTypeBuilder(TypeBuilder/*!*/ tb) {
            _tb = tb;
            _immediateClassField = _tb.DefineField(RubyObject.ImmediateClassFieldName, typeof(RubyClass), FieldAttributes.Private);
            _instanceDataField = _tb.DefineField(RubyObject.InstanceDataFieldName, typeof(RubyInstanceData), FieldAttributes.Private);
        }

        public void Implement(ClsTypeEmitter/*!*/ emitter) {
            DefineConstructors();
            DefineRubyObjectImplementation();
            DefineSerializer();

            RubyTypeEmitter re = (emitter as RubyTypeEmitter);
            Assert.NotNull(re);
            re.ImmediateClassField = _immediateClassField;

            DefineDynamicObjectImplementation();

#if !SILVERLIGHT // ICustomTypeDescriptor
            DefineCustomTypeDescriptor();
#endif

        }

#if !SILVERLIGHT
        private static readonly Type/*!*/[]/*!*/ _deserializerSignature = new Type[] { typeof(SerializationInfo), typeof(StreamingContext) };
#endif
        private static readonly Type/*!*/[]/*!*/ _classArgSignature = new Type[] { typeof(RubyClass) };

        private static readonly Type/*!*/[]/*!*/ _exceptionMessageSignature = new Type[] { typeof(string) };

        private static bool IsAvailable(MethodBase/*!*/ method) {
            return method != null && !method.IsPrivate && !method.IsAssembly && !method.IsFamilyAndAssembly;
        }

        private enum SignatureAdjustment {
            // ordered by priority:
            None = 0,
            ConvertClassToContext = 1,
            InsertClass = 2
        }

        private sealed class ConstructorBuilderInfo {
            public ConstructorInfo BaseCtor;
            public ParameterInfo[] BaseParameters;
            public Type[] ParameterTypes;
            public int ContextArgIndex;
            public int ClassArgIndex;
            public int ClassParamIndex;
            public SignatureAdjustment Adjustment;
        }

        private void DefineConstructors() {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            ConstructorInfo defaultBaseCtor = _tb.BaseType.GetConstructor(bindingFlags, null, Type.EmptyTypes, null);

            var ctors = new List<ConstructorBuilderInfo>();

            foreach (var baseCtor in _tb.BaseType.GetConstructors(bindingFlags)) {
                if (!baseCtor.IsPublic && !baseCtor.IsProtected()) {
                    continue;
                }

                ParameterInfo[] baseParams = baseCtor.GetParameters();

#if !SILVERLIGHT
                if (baseParams.Length == 2 &&
                    baseParams[0].ParameterType == typeof(SerializationInfo) && baseParams[1].ParameterType == typeof(StreamingContext)) {
                    OverrideDeserializer(baseCtor);
                    continue;
                }
#endif
                AddConstructor(ctors, MakeConstructor(baseCtor, baseParams));
            }

            BuildConstructors(ctors);
        }

        private static void AddConstructor(List<ConstructorBuilderInfo>/*!*/ ctors, ConstructorBuilderInfo/*!*/ ctor) {
            int existing = ctors.FindIndex((c) => c.ParameterTypes.ValueEquals(ctor.ParameterTypes));
            if (existing != -1) {
                if (ctors[existing].Adjustment > ctor.Adjustment) {
                    ctors[existing] = ctor;
                }
            } else {
                ctors.Add(ctor);
            }
        }

        private ConstructorBuilderInfo/*!*/ MakeConstructor(ConstructorInfo/*!*/ baseCtor, ParameterInfo/*!*/[]/*!*/ baseParams) {
            int contextArgIndex = -1;
            int classArgIndex = -1;
            for (int i = 0; i < baseParams.Length; i++) {
                if (baseParams[i].ParameterType == typeof(RubyContext)) {
                    contextArgIndex = i;
                    break;
                } else if (baseParams[i].ParameterType == typeof(RubyClass)) {
                    classArgIndex = i;
                    break;
                }
            }

            int classParamIndex;
            SignatureAdjustment adjustment;
            if (classArgIndex == -1) {
                if (contextArgIndex == -1) {                    
                    adjustment = SignatureAdjustment.InsertClass;
                    classParamIndex = 0;
                } else {
                    adjustment = SignatureAdjustment.ConvertClassToContext;
                    classParamIndex = contextArgIndex;
                }
            } else {
                adjustment = SignatureAdjustment.None;
                classParamIndex = classArgIndex;
            }

            Debug.Assert(classParamIndex >= 0);

            Type[] paramTypes = new Type[(adjustment == SignatureAdjustment.InsertClass ? 1 : 0) + baseParams.Length];
            int paramIndex = 0, argIndex = 0;
            if (adjustment == SignatureAdjustment.InsertClass) {
                paramIndex++;
            }

            while (paramIndex < paramTypes.Length) {
                paramTypes[paramIndex++] = baseParams[argIndex++].ParameterType;
            }

            paramTypes[classParamIndex] = typeof(RubyClass);

            return new ConstructorBuilderInfo() {
                BaseCtor = baseCtor,
                BaseParameters = baseParams,
                ParameterTypes = paramTypes,
                ContextArgIndex = contextArgIndex,
                ClassArgIndex = classArgIndex,
                ClassParamIndex = classParamIndex,
                Adjustment = adjustment,
            };
        }

        private void BuildConstructors(IList<ConstructorBuilderInfo>/*!*/ ctors) {
            foreach (var ctor in ctors) {
                // ctor(... RubyClass! class ..., <visible params>) : base(<hidden params>, <visible params>) { _class = class; }
                // ctor(... RubyClass! class ..., <visible params>) : base(... RubyOps.GetContextFromClass(class) ..., <visible params>) { _class = class; }
                // ctor(RubyClass! class) : base(RubyOps.GetDefaultExceptionMessage(class)) { _class = class; }
                ConstructorBuilder cb = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctor.ParameterTypes);
                ILGen il = new ILGen(cb.GetILGenerator());

                int paramIndex = 0;
                int argIndex = 0;

                // We need to initialize before calling base ctor since the ctor can call virtual methods.
                // _immediateClass = immediateClass:
                il.EmitLoadArg(0);
                il.EmitLoadArg(1 + ctor.ClassParamIndex);
                il.EmitFieldSet(_immediateClassField);

                // base ctor call:
                il.EmitLoadArg(0);

                ConstructorInfo msgCtor;
                if (ctor.ParameterTypes.Length == 1 && ctor.Adjustment == SignatureAdjustment.InsertClass &&
                    _tb.IsSubclassOf(typeof(Exception)) && IsAvailable(msgCtor = _tb.BaseType.GetConstructor(_exceptionMessageSignature))) {

                    // a parameterless exception constructor should use Ruby default message:
                    il.EmitLoadArg(1);
                    il.EmitCall(Methods.GetDefaultExceptionMessage);
                    il.Emit(OpCodes.Call, msgCtor);
                } else {
                    if (ctor.Adjustment == SignatureAdjustment.InsertClass) {
                        paramIndex++;
                    }

                    while (paramIndex < ctor.ParameterTypes.Length) {
                        if (ctor.Adjustment == SignatureAdjustment.ConvertClassToContext && argIndex == ctor.ContextArgIndex) {
                            il.EmitLoadArg(1 + ctor.ClassParamIndex);
                            il.EmitCall(Methods.GetContextFromModule);
                        } else {
                            ClsTypeEmitter.DefineParameterCopy(cb, paramIndex, ctor.BaseParameters[argIndex]);
                            il.EmitLoadArg(1 + paramIndex);
                        }
                        argIndex++;
                        paramIndex++;
                    }
                    il.Emit(OpCodes.Call, ctor.BaseCtor);
                }

                il.Emit(OpCodes.Ret);
            }
        }

#if !SILVERLIGHT
        private void OverrideDeserializer(ConstructorInfo/*!*/ baseCtor) {
            // ctor(SerializationInfo! info, StreamingContext! context) : base(info, context) {
            //   RubyOps.DeserializeObject(out this._instanceData, out this._immediateClass, info);
            // }

            ConstructorBuilder cb = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, _deserializerSignature);
            ILGen il = new ILGen(cb.GetILGenerator());

            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitLoadArg(2);
            il.Emit(OpCodes.Call, baseCtor);

            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_immediateClassField);
            il.EmitLoadArg(1);
            il.EmitCall(Methods.DeserializeObject);
            il.Emit(OpCodes.Ret);
        }
#endif

        private void DefineRubyObjectImplementation() {
            _tb.AddInterfaceImplementation(typeof(IRubyObject));

            _tb.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DebuggerTypeProxyAttribute).GetConstructor(new[] { typeof(Type) }),
                new[] { typeof(RubyObjectDebugView) }
            ));

            _tb.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(DebuggerDisplayAttribute).GetConstructor(new[] { typeof(string) }),
                new[] { RubyObject.DebuggerDisplayValue },
                new[] { typeof(DebuggerDisplayAttribute).GetProperty("Type") },
                new[] { RubyObject.DebuggerDisplayType }
            ));

            ILGen il;

            // RubyClass! IRubyObject.ImmediateClass { get { return _immediateClassField; } }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_get_ImmediateClass);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_immediateClassField);
            il.Emit(OpCodes.Ret);

            // RubyClass! IRubyObject.ImmediateClass { set { _immediateClassField = value; } }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_set_ImmediateClass);
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitFieldSet(_immediateClassField);
            il.Emit(OpCodes.Ret);

            // RubyInstanceData IRubyObject.TryGetInstanceData() { return _instanceData; }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_TryGetInstanceData);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_instanceDataField);
            il.Emit(OpCodes.Ret);

            // RubyInstanceData! IRubyObject.GetInstanceData() { return RubyOps.GetInstanceData(ref _instanceData); }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_GetInstanceData);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitCall(Methods.GetInstanceData);
            il.Emit(OpCodes.Ret);

            // bool IRubyObject.IsFrozen { get { return RubyOps.IsObjectFrozen(_instanceData); } }
            il = DefineMethodOverride(_tb, Methods.IRubyObjectState_get_IsFrozen);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_instanceDataField);
            il.EmitCall(Methods.IsObjectFrozen);
            il.Emit(OpCodes.Ret);

            // void IRubyObject.Freeze { RubyOps.FreezeObject(ref _instanceData); }
            il = DefineMethodOverride(_tb, Methods.IRubyObjectState_Freeze);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitCall(Methods.FreezeObject);
            il.Emit(OpCodes.Ret);

            // bool IRubyObject.IsTainted { 
            //   get { return RubyOps.IsObjectTainted(_instanceData); }
            //   set { return RubyOps.SetObjectTaint(ref _instanceData, value); }
            // }
            il = DefineMethodOverride(_tb, Methods.IRubyObjectState_get_IsTainted);
            il.EmitLoadArg(0);
            il.EmitFieldGet(_instanceDataField);
            il.EmitCall(Methods.IsObjectTainted);
            il.Emit(OpCodes.Ret);

            il = DefineMethodOverride(_tb, Methods.IRubyObjectState_set_IsTainted);
            il.EmitLoadArg(0);
            il.EmitFieldAddress(_instanceDataField);
            il.EmitLoadArg(1);
            il.EmitCall(Methods.SetObjectTaint);
            il.Emit(OpCodes.Ret);

            // TODO: can we merge this with #base#GetHashCode/Equals/ToString?

            // int IRubyObject.BaseGetHashCode() { return base.GetHashCode(); }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_BaseGetHashCode);
            il.EmitLoadArg(0);
            il.EmitCall(_tb.BaseType.GetMethod("GetHashCode", Type.EmptyTypes));
            il.Emit(OpCodes.Ret);

            // int IRubyObject.BaseEquals(object other) { return base.Equals(other); }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_BaseEquals);
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitCall(_tb.BaseType.GetMethod("Equals", new[] { typeof(object) }));
            il.Emit(OpCodes.Ret);

            // string IRubyObject.BaseToString() { return base.ToString(); }
            il = DefineMethodOverride(_tb, Methods.IRubyObject_BaseToString);
            il.EmitLoadArg(0);
            il.EmitCall(_tb.BaseType.GetMethod("ToString", Type.EmptyTypes));
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
            il.EmitFieldGet(_immediateClassField);
            il.EmitLoadArg(1);
            il.EmitCall(Methods.SerializeObject);
            il.Emit(OpCodes.Ret);
#endif
        }

        private void DefineDynamicObjectImplementation() {
            _tb.AddInterfaceImplementation(typeof(IDynamicMetaObjectProvider));

            // MetaObject! IDynamicMetaObjectProvider.GetMetaObject(Expression! parameter) {
            //   return RubyOps.GetMetaObject(this, parameter);
            // }

            MethodInfo decl = typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject");
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
