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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using System.Linq.Expressions;

namespace IronRuby.Compiler.Generation {

    public abstract partial class ClsTypeEmitter {
        class SpecialNames {
            private readonly Dictionary<string/*!*/, List<string/*!*/>/*!*/>/*!*/ _specialNames;

            internal SpecialNames() {
                _specialNames = new Dictionary<string, List<string>>();
            }

            internal void SetSpecialName(string/*!*/ specialName, List<string/*!*/>/*!*/ names) {
                _specialNames[specialName] = names;
            }

            internal void SetSpecialName(string/*!*/ name) {
                List<string> names = new List<string>(1);
                names.Add(name);
                _specialNames[name] = names;
            }

            internal IEnumerable<string> GetBaseName(MethodInfo/*!*/ mi) {
                string newName;
                if (mi.Name.StartsWith(BaseMethodPrefix)) {
                    newName = mi.Name.Substring(BaseMethodPrefix.Length);
                } else if (mi.Name.StartsWith(FieldGetterPrefix)) {
                    newName = mi.Name.Substring(FieldGetterPrefix.Length);
                } else if (mi.Name.StartsWith(FieldSetterPrefix)) {
                    newName = mi.Name.Substring(FieldSetterPrefix.Length);
                } else {
                    throw new InvalidOperationException();
                }

                Debug.Assert(_specialNames.ContainsKey(newName));

                return _specialNames[newName];
            }
        }

        public const string VtableNamesField = "#VTableNames#";
        public const string BaseMethodPrefix = "#base#";
        public const string FieldGetterPrefix = "#field_get#", FieldSetterPrefix = "#field_set#";

        private ILGen _cctor;
        private readonly TypeBuilder _tb;
        private readonly Type _baseType;
        private int _site;
        private readonly SpecialNames _specialNames;
        private readonly List<Expression> _dynamicSiteFactories;

        protected ClsTypeEmitter(TypeBuilder tb) {
            _tb = tb;
            _baseType = tb.BaseType;
            _specialNames = new SpecialNames();
            _dynamicSiteFactories = new List<Expression>();
        }

        private static bool ShouldOverrideVirtual(MethodInfo/*!*/ mi) {
            return true;
        }

        private static bool CanOverrideMethod(MethodInfo/*!*/ mi) {
#if !SILVERLIGHT
            return true;
#else
            // can only override the method if it is not SecurityCritical
            return mi.GetCustomAttributes(typeof(System.Security.SecurityCriticalAttribute), false).Length == 0;
#endif
        }

        protected abstract MethodInfo NonInheritedValueHelper();
        protected abstract MethodInfo NonInheritedMethodHelper();
        protected abstract MethodInfo EventHelper();
        protected abstract MethodInfo MissingInvokeMethodException();
        protected abstract MethodInfo ConvertToDelegate();

        protected abstract void EmitImplicitContext(ILGen il);
        protected abstract void EmitMakeCallAction(string name, int nargs, bool isList);
        protected abstract FieldInfo GetConversionSite(Type toType);
        protected abstract void EmitClassObjectFromInstance(ILGen il);
        protected abstract void EmitPropertyGet(ILGen il, MethodInfo mi, string name, LocalBuilder callTarget);
        protected abstract void EmitPropertySet(ILGen il, MethodInfo mi, string name, LocalBuilder callTarget);

        protected abstract bool TryGetName(Type clrType, MethodInfo mi, out string name);
        protected abstract bool TryGetName(Type clrType, EventInfo ei, MethodInfo mi, out string name);
        protected abstract bool TryGetName(Type clrType, PropertyInfo pi, MethodInfo mi, out string name);
        protected abstract Type/*!*/[]/*!*/ MakeSiteSignature(int nargs);
        protected abstract Type/*!*/ ContextType { get; }

        /// <summary>
        /// Gets the position for the parameter which we are overriding.
        /// </summary>
        /// <param name="pis"></param>
        /// <param name="overrideParams"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private int GetOriginalIndex(ParameterInfo/*!*/[]/*!*/ pis, ParameterInfo/*!*/[]/*!*/ overrideParams, int i) {
            if (pis.Length == 0 || pis[0].ParameterType != ContextType) {
                return i - (overrideParams.Length - pis.Length);
            }

            // context & cls are swapped, context comes first.
            if (i == 1) return -1;
            if (i == 0) return 0;

            return i - (overrideParams.Length - pis.Length);
        }

        private void CallBaseConstructor(ConstructorInfo parentConstructor, ParameterInfo[] pis, ParameterInfo[] overrideParams, ILGen il) {
            il.EmitLoadArg(0);
#if DEBUG
            int lastIndex = -1;
#endif
            for (int i = 0; i < overrideParams.Length; i++) {
                int index = GetOriginalIndex(pis, overrideParams, i);

#if DEBUG
                // we insert a new parameter (the class) but the parameters should
                // still remain in the same order after the extra parameter is removed.
                if (index >= 0) {
                    Debug.Assert(index > lastIndex);
                    lastIndex = index;
                }
#endif
                if (index >= 0) {
                    il.EmitLoadArg(i + 1);
                }
            }
            il.Emit(OpCodes.Call, parentConstructor);
            il.Emit(OpCodes.Ret);
        }

        protected ILGen GetCCtor() {
            if (_cctor == null) {
                ConstructorBuilder cctor = _tb.DefineTypeInitializer();
                _cctor = CreateILGen(cctor.GetILGenerator());
            }
            return _cctor;
        }

        protected Type BaseType {
            get { return _baseType; }
        }

        private void ImplementProtectedFieldAccessors() {
            // For protected fields to be accessible from the derived type in Silverlight,
            // we need to create public helper methods that expose them. These methods are
            // used by the IOldDynamicObject implementation (in UserTypeOps.GetRuleHelper)

            FieldInfo[] fields = _baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fi in fields) {
                if (!fi.IsFamily && !fi.IsFamilyOrAssembly) {
                    continue;
                }

                List<string> fieldAccessorNames = new List<string>();

                PropertyBuilder pb = _tb.DefineProperty(fi.Name, PropertyAttributes.None, fi.FieldType, Type.EmptyTypes);
                MethodAttributes methodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
                if (fi.IsStatic) {
                    methodAttrs |= MethodAttributes.Static;
                }

                MethodBuilder method;
                method = _tb.DefineMethod(FieldGetterPrefix + fi.Name, methodAttrs,
                                          fi.FieldType, Type.EmptyTypes);
                ILGen il = CreateILGen(method.GetILGenerator());
                if (!fi.IsStatic) {
                    il.EmitLoadArg(0);
                }

                if (fi.IsLiteral) {
                    // literal fields need to be inlined directly in here... We use GetRawConstant
                    // which will work even in partial trust if the constant is protected.
                    object value = fi.GetRawConstantValue();
                    switch (Type.GetTypeCode(fi.FieldType)) {
                        case TypeCode.Boolean:
                            if ((bool)value) {
                                il.Emit(OpCodes.Ldc_I4_1);
                            } else {
                                il.Emit(OpCodes.Ldc_I4_0);
                            }
                            break;
                        case TypeCode.Byte: il.Emit(OpCodes.Ldc_I4, (byte)value); break;
                        case TypeCode.Char: il.Emit(OpCodes.Ldc_I4, (char)value); break;
                        case TypeCode.Double: il.Emit(OpCodes.Ldc_R8, (double)value); break;
                        case TypeCode.Int16: il.Emit(OpCodes.Ldc_I4, (short)value); break;
                        case TypeCode.Int32: il.Emit(OpCodes.Ldc_I4, (int)value); break;
                        case TypeCode.Int64: il.Emit(OpCodes.Ldc_I8, (long)value); break;
                        case TypeCode.SByte: il.Emit(OpCodes.Ldc_I4, (sbyte)value); break;
                        case TypeCode.Single: il.Emit(OpCodes.Ldc_R4, (float)value); break;
                        case TypeCode.String: il.Emit(OpCodes.Ldstr, (string)value); break;
                        case TypeCode.UInt16: il.Emit(OpCodes.Ldc_I4, (ushort)value); break;
                        case TypeCode.UInt32: il.Emit(OpCodes.Ldc_I4, (uint)value); break;
                        case TypeCode.UInt64: il.Emit(OpCodes.Ldc_I8, (ulong)value); break;
                    }
                } else {
                    il.EmitFieldGet(fi);
                }
                il.Emit(OpCodes.Ret);

                pb.SetGetMethod(method);
                fieldAccessorNames.Add(method.Name);

                if (!fi.IsLiteral && !fi.IsInitOnly) {
                    method = _tb.DefineMethod(FieldSetterPrefix + fi.Name, methodAttrs,
                                              null, new Type[] { fi.FieldType });
                    method.DefineParameter(1, ParameterAttributes.None, "value");
                    il = CreateILGen(method.GetILGenerator());
                    il.EmitLoadArg(0);
                    if (!fi.IsStatic) {
                        il.EmitLoadArg(1);
                    }
                    il.EmitFieldSet(fi);
                    il.Emit(OpCodes.Ret);
                    pb.SetSetMethod(method);

                    fieldAccessorNames.Add(method.Name);
                }

                _specialNames.SetSpecialName(fi.Name, fieldAccessorNames);
            }
        }

        /// <summary>
        /// Overrides methods - this includes all accessible virtual methods as well as protected non-virtual members
        /// including statics and non-statics.
        /// </summary>
        internal void OverrideMethods(Type type) {
            // if we have conflicting virtual's due to new slots only override the methods on the
            // most derived class.
            var added = new Dictionary<Key<string, MethodSignatureInfo>, MethodInfo>();

            MethodInfo overridden;
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (MethodInfo mi in methods) {
                var key = Key.Create(mi.Name, new MethodSignatureInfo(mi.IsStatic, mi.GetParameters()));

                if (!added.TryGetValue(key, out overridden)) {
                    added[key] = mi;
                    continue;
                }

                if (overridden.DeclaringType.IsAssignableFrom(mi.DeclaringType)) {
                    added[key] = mi;
                }
            }

            var overriddenProperties = new Dictionary<PropertyInfo, PropertyBuilder>();
            foreach (MethodInfo mi in added.Values) {
                if (!ShouldOverrideVirtual(mi) || !CanOverrideMethod(mi)) continue;

                if (mi.IsPublic || mi.IsFamily || mi.IsFamilyOrAssembly) {
                    if (mi.IsGenericMethodDefinition) continue;

                    if (mi.IsSpecialName) {
                        OverrideSpecialName(mi, overriddenProperties);
                    } else {
                        OverrideBaseMethod(mi);
                    }
                }
            }
        }

        private void OverrideSpecialName(MethodInfo mi, Dictionary<PropertyInfo, PropertyBuilder> overridden) {
            if (!mi.IsVirtual || mi.IsFinal) {
                if ((mi.IsFamily || mi.IsSpecialName) && (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))) {
                    // need to be able to call into protected getter/setter methods from derived types,
                    // even if these methods aren't virtual and we are in partial trust.
                    _specialNames.SetSpecialName(mi.Name);
                    MethodBuilder mb = CreateSuperCallHelper(mi);

                    foreach (PropertyInfo pi in mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                        if (pi.GetGetMethod(true) == mi || pi.GetSetMethod(true) == mi) {
                            AddPublicProperty(mi, overridden, mb, pi);
                            break;
                        }
                    }
                }
            } else if (!TryOverrideProperty(mi, overridden)) {
                string name;
                EventInfo[] eis = mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (EventInfo ei in eis) {
                    if (ei.GetAddMethod() == mi) {
                        if (!TryGetName(mi.DeclaringType, ei, mi, out name)) return;
                        CreateVTableEventOverride(mi, mi.Name);
                        return;
                    } else if (ei.GetRemoveMethod() == mi) {
                        if (!TryGetName(mi.DeclaringType, ei, mi, out name)) return;
                        CreateVTableEventOverride(mi, mi.Name);
                        return;
                    }
                }

                OverrideBaseMethod(mi);
            }
        }

        private bool TryOverrideProperty(MethodInfo mi, Dictionary<PropertyInfo, PropertyBuilder> overridden) {
            string name;
            PropertyInfo[] pis = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            _specialNames.SetSpecialName(mi.Name);
            MethodBuilder mb = null;
            PropertyInfo foundProperty = null;
            foreach (PropertyInfo pi in pis) {
                if (pi.GetIndexParameters().Length > 0) {
                    if (mi == pi.GetGetMethod(true)) {
                        mb = CreateVTableMethodOverride(mi, "__getitem__");
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                        foundProperty = pi;
                        break;
                    } else if (mi == pi.GetSetMethod(true)) {
                        mb = CreateVTableMethodOverride(mi, "__setitem__");
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                        foundProperty = pi;
                        break;
                    }
                } else if (mi == pi.GetGetMethod(true)) {
                    if (mi.Name != "get_PythonType") {
                        if (!TryGetName(mi.DeclaringType, pi, mi, out name)) {
                            return true;
                        }
                        mb = CreateVTableGetterOverride(mi, name);
                        if (!mi.IsAbstract) {
                            CreateSuperCallHelper(mi);
                        }
                    }
                    foundProperty = pi;
                    break;
                } else if (mi == pi.GetSetMethod(true)) {
                    if (!TryGetName(mi.DeclaringType, pi, mi, out name)) {
                        return true;
                    }
                    mb = CreateVTableSetterOverride(mi, name);
                    if (!mi.IsAbstract) {
                        CreateSuperCallHelper(mi);
                    }
                    foundProperty = pi;
                    break;
                }
            }

            if (foundProperty != null) {
                AddPublicProperty(mi, overridden, mb, foundProperty);
                return true;
            }
            return false;
        }

        private void AddPublicProperty(MethodInfo mi, Dictionary<PropertyInfo, PropertyBuilder> overridden, MethodBuilder mb, PropertyInfo foundProperty) {
            MethodInfo getter = foundProperty.GetGetMethod(true);
            MethodInfo setter = foundProperty.GetSetMethod(true);
            if (IsProtected(getter) || IsProtected(setter)) {
                PropertyBuilder builder;
                if (!overridden.TryGetValue(foundProperty, out builder)) {
                    ParameterInfo[] indexArgs = foundProperty.GetIndexParameters();
                    Type[] paramTypes = new Type[indexArgs.Length];
                    for (int i = 0; i < paramTypes.Length; i++) {
                        paramTypes[i] = indexArgs[i].ParameterType;
                    }

                    overridden[foundProperty] = builder = _tb.DefineProperty(foundProperty.Name, foundProperty.Attributes, foundProperty.PropertyType, paramTypes);
                }

                if (foundProperty.GetGetMethod(true) == mi) {
                    builder.SetGetMethod(mb);
                } else if (foundProperty.GetSetMethod(true) == mi) {
                    builder.SetSetMethod(mb);
                }
            }
        }

        private static bool IsProtected(MethodInfo mi) {
            if (mi != null) {
                return mi.IsFamilyOrAssembly || mi.IsFamily;
            }
            return false;
        }

        /// <summary>
        /// Loads all the incoming arguments and forwards them to mi which
        /// has the same signature and then returns the result
        /// </summary>
        private void EmitBaseMethodDispatch(MethodInfo mi, ILGen il) {
            if (!mi.IsAbstract) {
                int offset = 0;
                if (!mi.IsStatic) {
                    il.EmitLoadArg(0);
                    offset = 1;
                }
                ParameterInfo[] parameters = mi.GetParameters();
                for (int i = 0; i < parameters.Length; i++) {
                    il.EmitLoadArg(i + offset);
                }
                il.EmitCall(OpCodes.Call, mi, null); // base call must be non-virtual
                il.Emit(OpCodes.Ret);
            } else {
                il.EmitLoadArg(0);
                il.EmitString(mi.Name);
                il.EmitCall(MissingInvokeMethodException());
                il.Emit(OpCodes.Throw);
            }
        }

        private void OverrideBaseMethod(MethodInfo mi) {
            if ((!mi.IsVirtual || mi.IsFinal) && !mi.IsFamily) {
                return;
            }

            Type baseType;
            if (_baseType == mi.DeclaringType || _baseType.IsSubclassOf(mi.DeclaringType)) {
                baseType = _baseType;
            } else {
                // We must be inherting from an interface
                Debug.Assert(mi.DeclaringType.IsInterface);
                baseType = mi.DeclaringType;
            }

            string name = null;
            if (!TryGetName(baseType, mi, out name)) return;

            if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") return;

            _specialNames.SetSpecialName(mi.Name);

            if (!mi.IsStatic) {
                CreateVTableMethodOverride(mi, name);
            }
            if (!mi.IsAbstract) {
                CreateSuperCallHelper(mi);
            }
        }

        /// <summary>
        /// Emits code to check if the class has overridden this specific
        /// function.  For example:
        /// 
        /// MyDerivedType.SomeVirtualFunction = ...
        ///     or
        /// 
        /// class MyDerivedType(MyBaseType):
        ///     def SomeVirtualFunction(self, ...):
        /// 
        /// </summary>
        internal LocalBuilder EmitBaseClassCallCheckForProperties(ILGen il, MethodInfo baseMethod, string name) {
            Label instanceCall = il.DefineLabel();
            LocalBuilder callTarget = il.DeclareLocal(typeof(object));

            il.EmitLoadArg(0);
            EmitClassObjectFromInstance(il);
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Ldloca, callTarget);
            il.EmitCall(NonInheritedValueHelper());

            il.Emit(OpCodes.Brtrue, instanceCall);

            EmitBaseMethodDispatch(baseMethod, il);

            il.MarkLabel(instanceCall);

            return callTarget;
        }

        private MethodBuilder CreateVTableGetterOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            EmitPropertyGet(il, mi, name, callTarget);

            if (!il.TryEmitImplicitCast(typeof(object), mi.ReturnType)) {
                EmitConvertFromObject(il, mi.ReturnType);
            }
            il.Emit(OpCodes.Ret);
            _tb.DefineMethodOverride(impl, mi);
            return impl;
        }

        public FieldInfo AllocateDynamicSite(Type[] signature, Func<FieldInfo, Expression> factory) {
            FieldInfo site = _tb.DefineField("site$" + _site++, CompilerHelpers.MakeCallSiteType(signature), FieldAttributes.Private | FieldAttributes.Static);
            _dynamicSiteFactories.Add(factory(site));
            return site;
        }

        /// <summary>
        /// Emit code to convert object to a given type. This code is semantically equivalent
        /// to PythonBinder.EmitConvertFromObject, except this version accepts ILGen whereas
        /// PythonBinder accepts Compiler. The Binder will chagne soon and the two will merge.
        /// </summary>
        public void EmitConvertFromObject(ILGen il, Type toType) {
            if (toType == typeof(object)) {
                return;
            } else if (toType == typeof(void)) {
                il.Emit(OpCodes.Pop);
                return;
            }

            var callTarget = il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Stloc, callTarget);

            var site = GetConversionSite(toType);

            // Emit the site invoke
            il.EmitFieldGet(site);
            FieldInfo target = site.FieldType.GetField("Target");
            il.EmitFieldGet(target);
            il.EmitFieldGet(site);

            // Emit the context
            EmitContext(il, false);

            il.Emit(OpCodes.Ldloc, callTarget);

            il.EmitCall(target.FieldType, "Invoke");
        }

        private MethodBuilder CreateVTableSetterOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            EmitPropertySet(il, mi, name, callTarget);

            il.Emit(OpCodes.Ret);
            _tb.DefineMethodOverride(impl, mi);
            return impl;
        }

        private void CreateVTableEventOverride(MethodInfo mi, string name) {
            // override the add/remove method  
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(mi, out impl);

            LocalBuilder callTarget = EmitBaseClassCallCheckForProperties(il, mi, name);

            il.Emit(OpCodes.Ldloc, callTarget);
            il.EmitLoadArg(0);
            il.EmitLoadArg(0);
            EmitClassObjectFromInstance(il);
            il.EmitLoadArg(1);
            il.EmitBoxing(mi.GetParameters()[0].ParameterType);
            il.Emit(OpCodes.Ldstr, name);
            il.EmitCall(EventHelper());
            il.Emit(OpCodes.Ret);
            _tb.DefineMethodOverride(impl, mi);
        }

        private MethodBuilder CreateVTableMethodOverride(MethodInfo mi, string name) {
            ParameterInfo[] parameters = mi.GetParameters();
            MethodBuilder impl;
            ILGen il;
            if (mi.IsVirtual && !mi.IsFinal) {
                il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            } else {
                impl = _tb.DefineMethod(
                    mi.Name,
                    mi.IsVirtual ?
                        (mi.Attributes | MethodAttributes.NewSlot) :
                        ((mi.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public),
                    mi.ReturnType,
                    ReflectionUtils.GetParameterTypes(parameters));
                il = CreateILGen(impl.GetILGenerator());
            }
            //CompilerHelpers.GetArgumentNames(parameters));  TODO: Set names

            LocalBuilder callTarget = EmitNonInheritedMethodLookup(name, il);
            Label instanceCall = il.DefineLabel();
            il.Emit(OpCodes.Brtrue, instanceCall);

            // lookup failed, call the base class method (this returns or throws)
            EmitBaseMethodDispatch(mi, il);

            // lookup succeeded, call the user defined method & return
            il.MarkLabel(instanceCall);
            EmitClrCallStub(il, mi, callTarget, name);
            EmitConvertFromObject(il, mi.ReturnType);
            il.Emit(OpCodes.Ret);

            if (mi.IsVirtual && !mi.IsFinal) {
                _tb.DefineMethodOverride(impl, mi);
            }
            return impl;
        }

        /// <summary>
        /// Emits the call to lookup a member defined in the user's type.  Returns
        /// the local which stores the resulting value and leaves a value on the
        /// stack indicating the success of the lookup.
        /// </summary>
        private LocalBuilder EmitNonInheritedMethodLookup(string name, ILGen il) {
            LocalBuilder callTarget = il.DeclareLocal(typeof(object));

            // emit call to helper to do lookup
            il.EmitLoadArg(0);
            EmitClassObjectFromInstance(il);
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Ldloca, callTarget);
            il.EmitCall(NonInheritedMethodHelper());
            return callTarget;
        }

        /// <summary>
        /// Creates a method for doing a base method dispatch.  This is used to support
        /// super(type, obj) calls.
        /// </summary>
        private MethodBuilder CreateSuperCallHelper(MethodInfo mi) {
            ParameterInfo[] parms = mi.GetParameters();
            Type[] types = ReflectionUtils.GetParameterTypes(parms);
            Type miType = mi.DeclaringType;
            for (int i = 0; i < types.Length; i++) {
                if (types[i] == miType) {
                    types[i] = _tb;
                }
            }

            MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            if (mi.IsStatic) {
                attrs |= MethodAttributes.Static;
            }

            MethodBuilder method = _tb.DefineMethod(
                BaseMethodPrefix + mi.Name,
                attrs,
                mi.ReturnType, types
            );

            for (int i = 0; i < types.Length; i++) {
                method.DefineParameter(i + 1, ParameterAttributes.None, parms[i].Name);
            }

            EmitBaseMethodDispatch(mi, CreateILGen(method.GetILGenerator()));
            return method;
        }

        public Type FinishType() {
            if (_dynamicSiteFactories.Count > 0) {
                GetCCtor();
            }

            if (_cctor != null) {
                if (_dynamicSiteFactories.Count > 0) { 
                    MethodBuilder createSitesImpl = _tb.DefineMethod(
                        "<create_dynamic_sites>", MethodAttributes.Private | MethodAttributes.Static, typeof(void), Type.EmptyTypes
                    );

                    _dynamicSiteFactories.Add(Expression.Empty());
                    Expression.Lambda(Expression.Block(_dynamicSiteFactories)).CompileToMethod(createSitesImpl);
                    _cctor.EmitCall(createSitesImpl);

                    _dynamicSiteFactories.Clear();
                }

                _cctor.Emit(OpCodes.Ret);
            }
            Type result = _tb.CreateType();
            new OverrideBuilder(_baseType).AddBaseMethods(result, _specialNames);
            return result;
        }

        internal protected ILGen CreateILGen(ILGenerator il) {
            // TODO: Debugging support
            return new ILGen(il);
        }

        private ILGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod, out MethodBuilder builder) {
            MethodAttributes attrs = baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.Public);
            attrs |= MethodAttributes.NewSlot | MethodAttributes.Final;

            Type[] baseSignature = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
            builder = _tb.DefineMethod(
                baseMethod.DeclaringType.Name + "." + baseMethod.Name,
                attrs,
                baseMethod.ReturnType,
                baseSignature
            );
            return CreateILGen(builder.GetILGenerator());
        }

        protected const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        protected ILGen DefineMethodOverride(Type type, string name, out MethodInfo decl, out MethodBuilder impl) {
            return DefineMethodOverride(MethodAttributes.PrivateScope, type, name, out decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodAttributes extra, Type type, string name, out MethodInfo decl, out MethodBuilder impl) {
            decl = type.GetMethod(name);
            return DefineMethodOverride(extra, decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodInfo decl, out MethodBuilder impl) {
            return DefineMethodOverride(MethodAttributes.PrivateScope, decl, out impl);
        }

        protected ILGen DefineMethodOverride(MethodAttributes extra, MethodInfo decl, out MethodBuilder impl) {
            MethodAttributes finalAttrs = (decl.Attributes & ~MethodAttributesToEraseInOveride) | extra;
            if ((extra & MethodAttributes.MemberAccessMask) != 0) {
                // remove existing member access, add new member access
                finalAttrs &= ~MethodAttributes.MemberAccessMask;
                finalAttrs |= extra;
            }
            Type[] signature = ReflectionUtils.GetParameterTypes(decl.GetParameters());
            impl = _tb.DefineMethod(decl.Name, finalAttrs, decl.ReturnType, signature);
            return CreateILGen(impl.GetILGenerator());
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// This code is similar to that in DelegateSignatureInfo.cs in the Microsoft.Scripting.
        /// </summary>
        internal void EmitClrCallStub(ILGen/*!*/ il, MethodInfo/*!*/ mi, LocalBuilder/*!*/ callTarget, string/*!*/ name) {
            int firstArg = 0;
            bool list = false;              // The list calling convention
            bool context = false;           // Context is an argument

            ParameterInfo[] pis = mi.GetParameters();
            if (pis.Length > 0) {
                if (pis[0].ParameterType == ContextType) {
                    firstArg = 1;
                    context = true;
                }
                if (pis[pis.Length - 1].IsDefined(typeof(ParamArrayAttribute), false)) {
                    list = true;
                }
            }

            ParameterInfo[] args = pis;
            int nargs = args.Length - firstArg;

            // Create the action
            ILGen cctor = GetCCtor();
            EmitMakeCallAction(name, nargs, list);

            // Create the dynamic site
            Type siteType = CompilerHelpers.MakeCallSiteType(MakeSiteSignature(nargs));
            FieldBuilder site = _tb.DefineField("site$" + _site++, siteType, FieldAttributes.Private | FieldAttributes.Static);
            cctor.EmitCall(siteType.GetMethod("Create"));
            cctor.EmitFieldSet(site);

            //
            // Emit the site invoke
            //
            il.EmitFieldGet(site);
            FieldInfo target = siteType.GetField("Target");
            il.EmitFieldGet(target);
            il.EmitFieldGet(site);

            // Emit the code context
            EmitContext(il, context);

            il.Emit(OpCodes.Ldloc, callTarget);

            List<ReturnFixer> fixers = new List<ReturnFixer>(0);
            for (int i = firstArg; i < args.Length; i++) {
                ReturnFixer rf = ReturnFixer.EmitArgument(il, args[i], i + 1);
                if (rf != null) {
                    fixers.Add(rf);
                }
            }

            il.EmitCall(target.FieldType, "Invoke");

            foreach (ReturnFixer rf in fixers) {
                rf.FixReturn(il);
            }
        }

        private void EmitContext(ILGen/*!*/ il, bool context) {
            if (context) {
                il.EmitLoadArg(1);
            } else {
                EmitImplicitContext(il);
            }
        }
    }

    /// <summary>
    /// Same as the DLR ReturnFixer, but accepts lower level constructs,
    /// such as LocalBuilder, ParameterInfos and ILGen.
    /// </summary>
    sealed class ReturnFixer {
        private readonly ParameterInfo/*!*/ _parameter;
        private readonly LocalBuilder/*!*/ _reference;
        private readonly int _index;

        private ReturnFixer(LocalBuilder/*!*/ reference, ParameterInfo/*!*/ parameter, int index) {
            Debug.Assert(reference.LocalType.IsGenericType && reference.LocalType.GetGenericTypeDefinition() == typeof(StrongBox<>));
            Debug.Assert(parameter.ParameterType.IsByRef);

            _parameter = parameter;
            _reference = reference;
            _index = index;
        }

        public void FixReturn(ILGen/*!*/ il) {
            il.EmitLoadArg(_index);
            il.Emit(OpCodes.Ldloc, _reference);
            il.EmitFieldGet(_reference.LocalType.GetField("Value"));
            il.EmitStoreValueIndirect(_parameter.ParameterType.GetElementType());
        }

        public static ReturnFixer/*!*/ EmitArgument(ILGen/*!*/ il, ParameterInfo/*!*/ parameter, int index) {
            il.EmitLoadArg(index);
            if (parameter.ParameterType.IsByRef) {
                Type elementType = parameter.ParameterType.GetElementType();
                Type concreteType = typeof(StrongBox<>).MakeGenericType(elementType);
                LocalBuilder refSlot = il.DeclareLocal(concreteType);
                il.EmitLoadValueIndirect(elementType);
                il.EmitNew(concreteType, new Type[] { elementType });
                il.Emit(OpCodes.Stloc, refSlot);
                il.Emit(OpCodes.Ldloc, refSlot);
                return new ReturnFixer(refSlot, parameter, index);
            } else {
                il.EmitBoxing(parameter.ParameterType);
                return null;
            }
        }
    }
}
