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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

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

namespace IronRuby.Compiler.Generation {

    public abstract class ClsTypeEmitter {
        public const string VtableNamesField = "#VTableNames#";
        public const string BaseMethodPrefix = "#base#";
        public const string FieldGetterPrefix = "#field_get#", FieldSetterPrefix = "#field_set#";

        private ILGen _cctor;
        private readonly TypeBuilder _tb;
        private readonly Type _baseType;
        private int _site;
        private readonly List<Expression> _dynamicSiteFactories;

        protected ClsTypeEmitter(TypeBuilder tb) {
            _tb = tb;
            _baseType = tb.BaseType;
            _dynamicSiteFactories = new List<Expression>();
        }

        private static bool CanOverrideMethod(MethodInfo/*!*/ mi) {
#if !SILVERLIGHT
            return true;
#else
            // can only override the method if it is not SecurityCritical
            return mi.GetCustomAttributes(typeof(System.Security.SecurityCriticalAttribute), false).Length == 0;
#endif
        }

        protected abstract MethodInfo EventHelper();
        protected abstract MethodInfo MissingInvokeMethodException();

        protected abstract void EmitImplicitContext(ILGen il);
        protected abstract void EmitMakeCallAction(string name, int nargs, bool isList);
        protected abstract FieldInfo GetConversionSiteField(Type toType);
        protected abstract MethodInfo GetGenericConversionSiteFactory(Type toType);
        protected abstract void EmitClassObjectFromInstance(ILGen il);
        
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
                if (!CompilerHelpers.IsProtected(fi)) {
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
            }
        }

        /// <summary>
        /// Overrides methods - this includes all accessible virtual methods as well as protected non-virtual members
        /// including statics and non-statics.
        /// </summary>
        internal void OverrideMethods(Type type) {
            // if we have conflicting virtual's due to new slots only override the methods on the most derived class.
            var added = new Dictionary<Key<string, MethodSignatureInfo>, MethodInfo>();

            string defaultGetter, defaultSetter;
            object[] attrs = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
            if (attrs.Length == 1) {
                string indexer = ((DefaultMemberAttribute)attrs[0]).MemberName;
                defaultGetter = "get_" + indexer;
                defaultSetter = "set_" + indexer;
            } else {
                defaultGetter = defaultSetter = null;
            }

            MethodInfo overridden;
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (MethodInfo mi in methods) {
                var key = Key.Create(mi.Name, new MethodSignatureInfo(mi));

                if (!added.TryGetValue(key, out overridden)) {
                    added[key] = mi;
                    continue;
                }

                if (overridden.DeclaringType.IsAssignableFrom(mi.DeclaringType)) {
                    added[key] = mi;
                }
            }

            foreach (MethodInfo mi in added.Values) {
                if (mi.DeclaringType == typeof(object) && mi.Name == "Finalize") {
                    continue;
                }

                if (mi.IsPublic || mi.IsProtected()) {
                    if (mi.IsVirtual && !mi.IsFinal) {
                        if (CanOverrideMethod(mi)) {
                            // override non-sealed virtual methods
                            string name = mi.Name;
                            if (mi.IsSpecialName) {
                                if (name == defaultGetter) {
                                    name = "[]";
                                } else if (name == defaultSetter) {
                                    name = "[]=";
                                } else if (name.StartsWith("get_", StringComparison.Ordinal)) {
                                    name = name.Substring(4);
                                } else if (name.StartsWith("set_", StringComparison.Ordinal)) {
                                    name = name.Substring(4) + "=";
                                }
                            }

                            CreateVTableMethodOverride(mi, name);

                            // define a stub for calling base virtual methods from dynamic methods:
                            CreateSuperCallHelper(mi);
                        }
                    } else if (mi.IsProtected()) {
                        // define a stub for calling protected methods from dynamic methods:
                        CreateSuperCallHelper(mi);
                    }
                }
            }
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

        private MethodBuilder CreateVTableGetterOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);

            EmitVirtualSiteCall(il, mi, name);

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

            if (toType.IsGenericParameter && toType.DeclaringMethod != null) {
                MethodInfo siteFactory = GetGenericConversionSiteFactory(toType);
                Debug.Assert(siteFactory.GetParameters().Length == 0 && typeof(CallSite).IsAssignableFrom(siteFactory.ReturnType));

                // siteVar = GetConversionSite<T>()
                var siteVar = il.DeclareLocal(siteFactory.ReturnType);
                il.Emit(OpCodes.Call, siteFactory);
                il.Emit(OpCodes.Stloc, siteVar);

                // Emit the site invoke
                il.Emit(OpCodes.Ldloc, siteVar);
                FieldInfo target = siteVar.LocalType.GetField("Target");
                il.EmitFieldGet(target);
                il.Emit(OpCodes.Ldloc, siteVar);

                // Emit the context
                EmitContext(il, false);

                il.Emit(OpCodes.Ldloc, callTarget);

                il.EmitCall(target.FieldType, "Invoke");
            } else {
                var site = GetConversionSiteField(toType);

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
        }

        private MethodBuilder CreateVTableMethodOverride(MethodInfo mi, string name) {
            MethodBuilder impl;
            ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
            EmitVirtualSiteCall(il, mi, name);
            _tb.DefineMethodOverride(impl, mi);
            return impl;
        }

        public void EmitVirtualSiteCall(ILGen il, MethodInfo mi, string name) {
            Label baseCallLabel = il.DefineLabel();

            LocalBuilder resultVar = il.DeclareLocal(typeof(object));
            EmitClrCallStub(il, mi, name);
            il.Emit(OpCodes.Stloc, resultVar);

            il.Emit(OpCodes.Ldloc, resultVar);
            il.Emit(OpCodes.Ldsfld, Fields.ForwardToBase);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue, baseCallLabel);

            if (mi.ReturnType != typeof(void)) {
                il.Emit(OpCodes.Ldloc, resultVar);
                EmitConvertFromObject(il, mi.ReturnType);
            }
            il.Emit(OpCodes.Ret);

            il.MarkLabel(baseCallLabel);
            EmitBaseMethodDispatch(mi, il);
        }

        /// <summary>
        /// Creates a method for doing a base method dispatch.  This is used to support
        /// super(type, obj) calls.
        /// </summary>
        private MethodBuilder CreateSuperCallHelper(MethodInfo mi) {
            MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            if (mi.IsStatic) {
                attrs |= MethodAttributes.Static;
            }

            MethodBuilder method = _tb.DefineMethod(BaseMethodPrefix + mi.Name, attrs, mi.CallingConvention);
            ReflectionUtils.CopyMethodSignature(mi, method, true);

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
            impl = ReflectionUtils.DefineMethodOverride(_tb, extra, decl);
            return CreateILGen(impl.GetILGenerator());
        }

        // TODO: use in Python's OverrideConstructor:
        public static ParameterBuilder DefineParameterCopy(ConstructorBuilder builder, int paramIndex, ParameterInfo info) {
            var result = builder.DefineParameter(1 + paramIndex, info.Attributes, info.Name);
            CopyParameterAttributes(info, result);
            return result;
        }

        public static ParameterBuilder DefineParameterCopy(MethodBuilder builder, int paramIndex, ParameterInfo info) {
            var result = builder.DefineParameter(1 + paramIndex, info.Attributes, info.Name);
            CopyParameterAttributes(info, result);
            return result;
        }

        public static void CopyParameterAttributes(ParameterInfo from, ParameterBuilder to) {
            if (from.IsDefined(typeof(ParamArrayAttribute), false)) {
                to.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects)
                );
            } else if (from.IsDefined(typeof(ParamDictionaryAttribute), false)) {
                to.SetCustomAttribute(new CustomAttributeBuilder(
                    typeof(ParamDictionaryAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects)
                );
            }

            if ((from.Attributes & ParameterAttributes.HasDefault) != 0) {
                to.SetConstant(from.DefaultValue);
            }
        }

        /// <summary>
        /// Generates stub to receive the CLR call and then call the dynamic language code.
        /// This code is similar to that in DelegateSignatureInfo.cs in the Microsoft.Scripting.
        /// </summary>
        internal void EmitClrCallStub(ILGen/*!*/ il, MethodInfo/*!*/ mi, string/*!*/ name) {
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

            il.Emit(OpCodes.Ldarg_0);

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
