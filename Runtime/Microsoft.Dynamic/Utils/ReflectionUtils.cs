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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using System.Runtime.InteropServices;

#if !SILVERLIGHT
using Microsoft.Scripting.Metadata;
#endif

namespace Microsoft.Scripting.Utils {
    // CF doesn't support DefaultParameterValue attribute. Define our own, but not in System.Runtime.InteropServices namespace as that would 
    // make C# compiler emit the parameter's default value metadata not the attribute itself. The default value metadata are not accessible on CF.
#if SILVERLIGHT && CLR2 
    /// <summary>
    /// The Default Parameter Value Attribute.
    /// </summary>
    public sealed class DefaultParameterValueAttribute : Attribute
    {
        private readonly object _value;

        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="value">The value.</param>
        public DefaultParameterValueAttribute(object value)
        {
            _value = value;
        }
    }
#endif

    public static class ReflectionUtils {
        #region Signature and Type Formatting

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        public const char GenericArityDelimiter = '`';

        public static StringBuilder FormatSignature(StringBuilder result, MethodBase method) {
            return FormatSignature(result, method, (t) => t.FullName);
        }

        public static StringBuilder FormatSignature(StringBuilder result, MethodBase method, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");

            MethodInfo methodInfo = method as MethodInfo;
            if (methodInfo != null) {
                FormatTypeName(result, methodInfo.ReturnType, nameDispenser);
                result.Append(' ');
            }

            MethodBuilder builder = method as MethodBuilder;
            if (builder != null) {
                result.Append(builder.Signature);
                return result;
            }

            ConstructorBuilder cb = method as ConstructorBuilder;
            if (cb != null) {
                result.Append(cb.Signature);
                return result;
            }

            FormatTypeName(result, method.DeclaringType, nameDispenser);
            result.Append("::");
            result.Append(method.Name);

            if (!method.IsConstructor) {
                FormatTypeArgs(result, method.GetGenericArguments(), nameDispenser);
            }

            result.Append("(");

            if (!method.ContainsGenericParameters) {
                ParameterInfo[] ps = method.GetParameters();
                for (int i = 0; i < ps.Length; i++) {
                    if (i > 0) result.Append(", ");
                    FormatTypeName(result, ps[i].ParameterType, nameDispenser);
                    if (!System.String.IsNullOrEmpty(ps[i].Name)) {
                        result.Append(" ");
                        result.Append(ps[i].Name);
                    }
                }
            } else {
                result.Append("?");
            }

            result.Append(")");
            return result;
        }

        public static StringBuilder FormatTypeName(StringBuilder result, Type type) {
            return FormatTypeName(result, type, (t) => t.FullName);
        }

        public static StringBuilder FormatTypeName(StringBuilder result, Type type, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");
            
            if (type.IsGenericType) {
                Type genType = type.GetGenericTypeDefinition();
                string genericName = nameDispenser(genType).Replace('+', '.');
                int tickIndex = genericName.IndexOf('`');
                result.Append(tickIndex != -1 ? genericName.Substring(0, tickIndex) : genericName);

                Type[] typeArgs = type.GetGenericArguments();
                if (type.IsGenericTypeDefinition) {
                    result.Append('<');
                    result.Append(',', typeArgs.Length - 1);
                    result.Append('>');
                } else {
                    FormatTypeArgs(result, typeArgs, nameDispenser);
                }
            } else if (type.IsGenericParameter) {
                result.Append(type.Name);
            } else {
                // cut namespace off:
                result.Append(nameDispenser(type).Replace('+', '.'));
            }
            return result;
        }

        public static StringBuilder FormatTypeArgs(StringBuilder result, Type[] types) {
            return FormatTypeArgs(result, types, (t) => t.FullName);
        }

        public static StringBuilder FormatTypeArgs(StringBuilder result, Type[] types, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNullItems(types, "types");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");
            
            if (types.Length > 0) {
                result.Append("<");

                for (int i = 0; i < types.Length; i++) {
                    if (i > 0) result.Append(", ");
                    FormatTypeName(result, types[i], nameDispenser);
                }

                result.Append(">");
            }
            return result;
        }

        internal static string ToValidTypeName(string str) {
            if (String.IsNullOrEmpty(str)) {
                return "_";
            }

            StringBuilder sb = new StringBuilder(str);
            for (int i = 0; i < str.Length; i++) {
                if (str[i] == '\0' || str[i] == '.' || str[i] == '*' || str[i] == '+' || str[i] == '[' || str[i] == ']' || str[i] == '\\') {
                    sb[i] = '_';
                }
            }
            return sb.ToString();
        }

        public static string GetNormalizedTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType) {
                return GetNormalizedTypeName(name);
            }
            return name;
        }

        public static string GetNormalizedTypeName(string typeName) {
            Debug.Assert(typeName.IndexOf(Type.Delimiter) == -1); // This is the simple name, not the full name
            int backtick = typeName.IndexOf(ReflectionUtils.GenericArityDelimiter);
            if (backtick != -1) return typeName.Substring(0, backtick);
            return typeName;
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Creates an open delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType) {
            return CreateDelegate(methodInfo, delegateType, null);
        }

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType, object target) {
            ContractUtils.RequiresNotNull(methodInfo, "methodInfo");
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            if (PlatformAdaptationLayer.IsCompactFramework) {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }

            return CreateDelegateInternal(methodInfo, delegateType, target);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Delegate CreateDelegateInternal(MethodInfo methodInfo, Type delegateType, object target) {
            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        public static bool IsDynamicMethod(MethodBase method) {
            return !PlatformAdaptationLayer.IsCompactFramework && IsDynamicMethodInternal(method);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsDynamicMethodInternal(MethodBase method) {
            return method is DynamicMethod;
        }

        public static void GetDelegateSignature(Type delegateType, out ParameterInfo[] parameterInfos, out ParameterInfo returnInfo) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ContractUtils.Requires(invokeMethod != null, "delegateType", Strings.InvalidDelegate);

            parameterInfos = invokeMethod.GetParameters();
            returnInfo = invokeMethod.ReturnParameter;
        }

        /// <summary>
        /// Gets a Func of CallSite, object * paramCnt, object delegate type
        /// that's suitable for use in a non-strongly typed call site.
        /// </summary>
        public static Type GetObjectCallSiteDelegateType(int paramCnt) {
            switch (paramCnt) {
                case 0: return typeof(Func<CallSite, object, object>);
                case 1: return typeof(Func<CallSite, object, object, object>);
                case 2: return typeof(Func<CallSite, object, object, object, object>);
                case 3: return typeof(Func<CallSite, object, object, object, object, object>);
                case 4: return typeof(Func<CallSite, object, object, object, object, object, object>);
                case 5: return typeof(Func<CallSite, object, object, object, object, object, object, object>);
                case 6: return typeof(Func<CallSite, object, object, object, object, object, object, object, object>);
                case 7: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object>);
                case 8: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object>);
                case 9: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>);
                case 10: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 11: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 12: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 13: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 14: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                default:
                    Type[] paramTypes = new Type[paramCnt + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[1] = typeof(object);
                    for (int i = 0; i < paramCnt; i++) {
                        paramTypes[i + 2] = typeof(object);
                    }
                    return Snippets.Shared.DefineDelegate("InvokeDelegate" + paramCnt, typeof(object), paramTypes);
            }
        }

        #endregion

        #region Methods and Parameters

        public static MethodInfo[] GetMethodInfos(Delegate[] delegates) {
            MethodInfo[] result = new MethodInfo[delegates.Length];
            for (int i = 0; i < delegates.Length; i++) result[i] = delegates[i].Method;
            return result;
        }

        public static MethodBase[] GetMethodInfos(MemberInfo[] members) {
            return ArrayUtils.ConvertAll<MemberInfo, MethodBase>(
                members,
                delegate(MemberInfo inp) { return (MethodBase)inp; });
        }

        public static Type[] GetParameterTypes(ParameterInfo[] parameterInfos) {
            return GetParameterTypes((IList<ParameterInfo>)parameterInfos);
        }

        public static Type[] GetParameterTypes(IList<ParameterInfo> parameterInfos) {
            Type[] result = new Type[parameterInfos.Count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = parameterInfos[i].ParameterType;
            }
            return result;
        }

        public static Type GetReturnType(this MethodBase mi) {
            return (mi.IsConstructor) ? mi.DeclaringType : ((MethodInfo)mi).ReturnType;
        }

        public static bool SignatureEquals(MethodInfo method, params Type[] requiredSignature) {
            ContractUtils.RequiresNotNull(method, "method");

            Type[] actualTypes = ReflectionUtils.GetParameterTypes(method.GetParameters());
            Debug.Assert(actualTypes.Length == requiredSignature.Length - 1);
            int i = 0;
            while (i < actualTypes.Length) {
                if (actualTypes[i] != requiredSignature[i]) return false;
                i++;
            }

            return method.ReturnType == requiredSignature[i];
        }

#if CLR2 && !SILVERLIGHT
        private static Type _ExtensionAttributeType;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool IsExtension(this MemberInfo member) {
            var dlrExtension = typeof(ExtensionAttribute);
            if (member.IsDefined(dlrExtension, false)) {
                return true;
            }

#if CLR2 && !SILVERLIGHT
            if (_ExtensionAttributeType == null) {
                try {
                    _ExtensionAttributeType = Assembly.Load("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                        .GetType("System.Runtime.CompilerServices.ExtensionAttribute");
                } catch {
                    _ExtensionAttributeType = dlrExtension;
                }
            }

            if (_ExtensionAttributeType != dlrExtension) {
                return member.IsDefined(_ExtensionAttributeType, false);
            }
#endif
            return false;
        }

        public static bool IsOutParameter(this ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            return pi.ParameterType.IsByRef && (pi.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified parameter is mandatory, i.e. is not optional and doesn't have a default value.
        /// </summary>
        public static bool IsMandatory(this ParameterInfo pi) {
            return (pi.Attributes & ParameterAttributes.Optional) == 0 && !pi.HasDefaultValue();
        }

        public static bool HasDefaultValue(this ParameterInfo pi) {
#if SILVERLIGHT && CLR2 // CF doesn't support Optional nor DefaultParameterValue attributes
            return pi.IsDefined(typeof(DefaultParameterValueAttribute), false);
#else
            return (pi.Attributes & ParameterAttributes.HasDefault) != 0;
#endif
        }

        public static bool ProhibitsNull(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(NotNullAttribute), false);
        }

        public static bool ProhibitsNullItems(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(NotNullItemsAttribute), false);
        }

        public static bool IsParamArray(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static bool IsParamDictionary(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamDictionaryAttribute), false);
        }

        public static bool IsParamsMethod(MethodBase method) {
            return IsParamsMethod(method.GetParameters());
        }

        public static bool IsParamsMethod(ParameterInfo[] pis) {
            foreach (ParameterInfo pi in pis) {
                if (pi.IsParamArray() || pi.IsParamDictionary()) return true;
            }
            return false;
        }

        public static object GetDefaultValue(this ParameterInfo info) {
#if SILVERLIGHT && CLR2 // CF doesn't support Optional nor DefaultParameterValue attributes
            if (info.IsOptional) {
                return info.ParameterType == typeof(object) ? Missing.Value : ScriptingRuntimeHelpers.GetPrimitiveDefaultValue(info.ParameterType);
            } 

            var defaultValueAttribute = info.GetCustomAttributes(typeof(DefaultParameterValueAttribute), false);
            if (defaultValueAttribute.Length > 0) {
                return ((DefaultParameterValueAttribute)defaultValueAttribute[0]).Value;
            } 

            return null;
#else
            return info.DefaultValue;
#endif
        }

        #endregion

        #region Type Reflection

        /// <summary>
        /// Like Type.GetInterfaces, but only returns the interfaces implemented by this type
        /// and not its parents.
        /// </summary>
        public static List<Type> GetDeclaredInterfaces(Type type) {
            IList<Type> baseInterfaces = (type.BaseType != null) ? type.BaseType.GetInterfaces() : Type.EmptyTypes;
            List<Type> interfaces = new List<Type>();
            foreach (Type iface in type.GetInterfaces()) {
                if (!baseInterfaces.Contains(iface)) {
                    interfaces.Add(iface);
                }
            }
            return interfaces;
        }

        #endregion

        #region Type Builder

        private const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.Abstract | MethodAttributes.ReservedMask;

        public static MethodBuilder DefineMethodOverride(TypeBuilder tb, MethodAttributes extra, MethodInfo decl) {
            MethodAttributes finalAttrs = (decl.Attributes & ~MethodAttributesToEraseInOveride) | extra;
            if (!decl.DeclaringType.IsInterface) {
                finalAttrs &= ~MethodAttributes.NewSlot;
            }

            if ((extra & MethodAttributes.MemberAccessMask) != 0) {
                // remove existing member access, add new member access
                finalAttrs &= ~MethodAttributes.MemberAccessMask;
                finalAttrs |= extra;
            }

            MethodBuilder impl = tb.DefineMethod(decl.Name, finalAttrs, decl.CallingConvention);
            CopyMethodSignature(decl, impl, false);
            return impl;
        }

#if CLR2 && !SILVERLIGHT
        // ParameterInfo.GetRequiredCustomModifiers is broken on method signatures that contain generic parameters on CLR < 2.0.50727.4918
        private static bool? _modopsSupported;
        private static bool ModopsSupported {
            get {
                if (_modopsSupported == null) {
                    Assembly mscorlib = typeof(object).Assembly;
                    var companyAttrs = mscorlib.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if (companyAttrs.Length != 1 || ((AssemblyCompanyAttribute)companyAttrs[0]).Company.IndexOf("Microsoft") == -1) {
                        _modopsSupported = true;
                    } else {
                        Version version = new Version(((AssemblyFileVersionAttribute)mscorlib.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)[0]).Version);
                        _modopsSupported = version.Revision >= 4918;
                    }
                }
                return _modopsSupported.Value;
            }
        }
#endif

        public static void CopyMethodSignature(MethodInfo from, MethodBuilder to, bool substituteDeclaringType) {
            ParameterInfo[] paramInfos = from.GetParameters();
            Type[] parameterTypes = new Type[paramInfos.Length];
            Type[][] parameterRequiredModifiers = null, parameterOptionalModifiers = null;
            Type[] returnRequiredModifiers = null, returnOptionalModifiers = null;

#if !SILVERLIGHT
#if CLR2
            bool copyModopts = !from.IsGenericMethodDefinition || ModopsSupported;
#else
            bool copyModopts = true;
#endif
            if (copyModopts) {    
                returnRequiredModifiers = from.ReturnParameter.GetRequiredCustomModifiers();
                returnOptionalModifiers = from.ReturnParameter.GetOptionalCustomModifiers();
            }
#endif

            for (int i = 0; i < paramInfos.Length; i++) {
                if (substituteDeclaringType && paramInfos[i].ParameterType == from.DeclaringType) {
                    parameterTypes[i] = to.DeclaringType;
                } else {
                    parameterTypes[i] = paramInfos[i].ParameterType;
                }

#if !SILVERLIGHT
                if (copyModopts) {
                    var mods = paramInfos[i].GetRequiredCustomModifiers();
                    if (mods.Length > 0) {
                        if (parameterRequiredModifiers == null) {
                            parameterRequiredModifiers = new Type[paramInfos.Length][];
                        }

                        parameterRequiredModifiers[i] = mods;
                    }

                    mods = paramInfos[i].GetOptionalCustomModifiers();
                    if (mods.Length > 0) {
                        if (parameterOptionalModifiers == null) {
                            parameterOptionalModifiers = new Type[paramInfos.Length][];
                        }

                        parameterOptionalModifiers[i] = mods;
                    }
                }
#endif
            }

            to.SetSignature(
                from.ReturnType, returnRequiredModifiers, returnOptionalModifiers,
                parameterTypes, parameterRequiredModifiers, parameterOptionalModifiers
            );

            CopyGenericMethodAttributes(from, to);

            for (int i = 0; i < paramInfos.Length; i++) {
                to.DefineParameter(i + 1, paramInfos[i].Attributes, paramInfos[i].Name);
            }
        }

        private static void CopyGenericMethodAttributes(MethodInfo from, MethodBuilder to) {
            if (from.IsGenericMethodDefinition) {
                Type[] args = from.GetGenericArguments();
                string[] names = new string[args.Length];
                for (int i = 0; i < args.Length; i++) {
                    names[i] = args[i].Name;
                }
                var builders = to.DefineGenericParameters(names);
                for (int i = 0; i < args.Length; i++) {
                    // Copy template parameter attributes
                    builders[i].SetGenericParameterAttributes(args[i].GenericParameterAttributes);

                    // Copy template parameter constraints
                    Type[] constraints = args[i].GetGenericParameterConstraints();
                    List<Type> interfaces = new List<Type>(constraints.Length);
                    foreach (Type constraint in constraints) {
                        if (constraint.IsInterface) {
                            interfaces.Add(constraint);
                        } else {
                            builders[i].SetBaseTypeConstraint(constraint);
                        }
                    }
                    if (interfaces.Count > 0) {
                        builders[i].SetInterfaceConstraints(interfaces.ToArray());
                    }
                }
            }
        }

        #endregion

        #region Extension Methods

        public static IEnumerable<MethodInfo> GetVisibleExtensionMethods(Assembly assembly) {
#if !CLR2 && !SILVERLIGHT
            if (!assembly.IsDynamic && AppDomain.CurrentDomain.IsFullyTrusted) {
                try {
                    return GetVisibleExtensionMethodsFast(assembly);
                } catch (SecurityException) {
                    // full-demand can still fail if there is a partial trust domain on the stack
                }
            }
#endif
            return GetVisibleExtensionMethodsSlow(assembly);
        }

#if !SILVERLIGHT
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<MethodInfo> GetVisibleExtensionMethodsFast(Assembly assembly) {
            // Security: link demand
            return MetadataServices.GetVisibleExtensionMethodInfos(assembly);
        }
#endif

        // TODO: make internal
        // TODO: handle type load exceptions
        public static IEnumerable<MethodInfo> GetVisibleExtensionMethodsSlow(Assembly assembly) {
            var ea = typeof(ExtensionAttribute);
            if (assembly.IsDefined(ea, false)) {
#if SILVERLIGHT
                foreach (Module module in assembly.GetModules()) {
#else
                foreach (Module module in assembly.GetModules(false)) {
#endif
                    foreach (Type type in module.GetTypes()) {
                        var tattrs = type.Attributes;
                        if (((tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.Public ||
                            (tattrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic) &&
                            (tattrs & TypeAttributes.Abstract) != 0 &&
                            (tattrs & TypeAttributes.Sealed) != 0 &&
                            type.IsDefined(ea, false)) {

                            foreach (MethodInfo method in type.GetMethods()) {
                                if (method.IsPublic && method.IsStatic && method.IsDefined(ea, false)) {
                                    yield return method;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Value is null if there are no extension methods in the assembly.
        private static Dictionary<Assembly, Dictionary<string, List<ExtensionMethodInfo>>> _extensionMethodsCache;

        /// <summary>
        /// Enumerates extension methods in given assembly. Groups the methods by declaring namespace.
        /// Uses a global cache if <paramref name="useCache"/> is true.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> GetVisibleExtensionMethodGroups(Assembly/*!*/ assembly, bool useCache) {
#if !CLR2
            useCache &= !assembly.IsDynamic;
#endif
            if (useCache) {
                if (_extensionMethodsCache == null) {
                    _extensionMethodsCache = new Dictionary<Assembly, Dictionary<string, List<ExtensionMethodInfo>>>();
                }

                lock (_extensionMethodsCache) {
                    Dictionary<string, List<ExtensionMethodInfo>> existing;
                    if (_extensionMethodsCache.TryGetValue(assembly, out existing)) {
                        return EnumerateExtensionMethods(existing);
                    }
                }
            }

            Dictionary<string, List<ExtensionMethodInfo>> result = null;
            foreach (MethodInfo method in ReflectionUtils.GetVisibleExtensionMethodsSlow(assembly)) {
                if (method.DeclaringType == null || method.DeclaringType.IsGenericTypeDefinition) {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 0) {
                    continue;
                }

                Type type = parameters[0].ParameterType;
                if (type.IsByRef || type.IsPointer) {
                    continue;
                }

                string ns = method.DeclaringType.Namespace ?? String.Empty;
                List<ExtensionMethodInfo> extensions = null;

                if (result == null) {
                    result = new Dictionary<string, List<ExtensionMethodInfo>>();
                }

                if (!result.TryGetValue(ns, out extensions)) {
                    result.Add(ns, extensions = new List<ExtensionMethodInfo>());
                }

                extensions.Add(new ExtensionMethodInfo(type, method));
            }

            if (useCache) {
                lock (_extensionMethodsCache) {
                    _extensionMethodsCache[assembly] = result;
                }
            }

            return EnumerateExtensionMethods(result);
        }

        // TODO: GetVisibleExtensionMethods(Hashset<string> namespaces, Type type, string methodName) : IEnumerable<MethodInfo> {}

        private static IEnumerable<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> EnumerateExtensionMethods(Dictionary<string, List<ExtensionMethodInfo>> dict) {
            if (dict != null) {
                foreach (var entry in dict) {
                    yield return new KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>(entry.Key, new ReadOnlyCollection<ExtensionMethodInfo>(entry.Value));
                }
            }
        }

        #endregion

        #region Generic Types

        internal static Dictionary<Type, Type> BindGenericParameters(Type/*!*/ openType, Type/*!*/ closedType, bool ignoreUnboundParameters) {
            var binding = new Dictionary<Type, Type>();
            BindGenericParameters(openType, closedType, (parameter, type) => {
                Type existing;
                if (binding.TryGetValue(parameter, out existing)) {
                    return type == existing;
                }

                binding[parameter] = type;

                return true;
            });

            return ConstraintsViolated(binding, ignoreUnboundParameters) ? null : binding;
        }

        /// <summary>
        /// Binds occurances of generic parameters in <paramref name="openType"/> against corresponding types in <paramref name="closedType"/>.
        /// Invokes <paramref name="binder"/>(parameter, type) for each such binding.
        /// Returns false if the <paramref name="openType"/> is structurally different from <paramref name="closedType"/> or if the binder returns false.
        /// </summary>
        internal static bool BindGenericParameters(Type/*!*/ openType, Type/*!*/ closedType, Func<Type, Type, bool>/*!*/ binder) {
            if (openType.IsGenericParameter) {
                return binder(openType, closedType);
            }

            if (openType.IsArray) {
                if (!closedType.IsArray) {
                    return false;
                }
                return BindGenericParameters(openType.GetElementType(), closedType.GetElementType(), binder);
            }

            if (!openType.IsGenericType || !closedType.IsGenericType) {
                return openType == closedType;
            }

            if (openType.GetGenericTypeDefinition() != closedType.GetGenericTypeDefinition()) {
                return false;
            }

            Type[] closedArgs = closedType.GetGenericArguments();
            Type[] openArgs = openType.GetGenericArguments();

            for (int i = 0; i < openArgs.Length; i++) {
                if (!BindGenericParameters(openArgs[i], closedArgs[i], binder)) {
                    return false;
                }
            }

            return true;
        }

        internal static bool ConstraintsViolated(Dictionary<Type, Type>/*!*/ binding, bool ignoreUnboundParameters) {
            foreach (var entry in binding) {
                if (ConstraintsViolated(entry.Key, entry.Value, binding, ignoreUnboundParameters)) {
                    return true;
                }
            }

            return false;
        }

        internal static bool ConstraintsViolated(Type/*!*/ genericParameter, Type/*!*/ closedType, Dictionary<Type, Type>/*!*/ binding, bool ignoreUnboundParameters) {
            if ((genericParameter.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0 && closedType.IsValueType) {
                // value type to parameter type constrained as class
                return true;
            }

            if ((genericParameter.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 &&
                (!closedType.IsValueType || (closedType.IsGenericType && closedType.GetGenericTypeDefinition() == typeof(Nullable<>)))) {
                // nullable<T> or class/interface to parameter type constrained as struct
                return true;
            }

            if ((genericParameter.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0 &&
                (!closedType.IsValueType && closedType.GetConstructor(Type.EmptyTypes) == null)) {
                // reference type w/o a default constructor to type constrianed as new()
                return true;
            }

            Type[] constraints = genericParameter.GetGenericParameterConstraints();
            for (int i = 0; i < constraints.Length; i++) {
                Type instantiation = InstantiateConstraint(constraints[i], binding);

                if (instantiation == null) {
                    if (ignoreUnboundParameters) {
                        continue;
                    } else {
                        return true;
                    }
                }

                if (!instantiation.IsAssignableFrom(closedType)) {
                    return true;
                }
            }

            return false;
        }

        internal static Type InstantiateConstraint(Type/*!*/ constraint, Dictionary<Type, Type>/*!*/ binding) {
            Debug.Assert(!constraint.IsArray && !constraint.IsByRef && !constraint.IsGenericTypeDefinition);
            if (!constraint.ContainsGenericParameters) {
                return constraint;
            }

            Type closedType;
            if (constraint.IsGenericParameter) {
                return binding.TryGetValue(constraint, out closedType) ? closedType : null;
            }

            Type[] args = constraint.GetGenericArguments();
            for (int i = 0; i < args.Length; i++) {
                if ((args[i] = InstantiateConstraint(args[i], binding)) == null) {
                    return null;
                }
            }

            return constraint.GetGenericTypeDefinition().MakeGenericType(args);
        }

        #endregion
    }

    public struct ExtensionMethodInfo : IEquatable<ExtensionMethodInfo> {
        private readonly Type/*!*/ _extendedType; // cached type of the first parameter
        private readonly MethodInfo/*!*/ _method;

        internal ExtensionMethodInfo(Type/*!*/ extendedType, MethodInfo/*!*/ method) {
            Assert.NotNull(extendedType, method);
            _extendedType = extendedType;
            _method = method;
        }

        public Type/*!*/ ExtendedType {
            get { return _extendedType; }
        }

        public MethodInfo/*!*/ Method {
            get { return _method; }
        }

        public override bool Equals(object obj) {
            return obj is ExtensionMethodInfo && Equals((ExtensionMethodInfo)obj);
        }

        public bool Equals(ExtensionMethodInfo other) {
            return _method.Equals(other._method);
        }

        public static bool operator ==(ExtensionMethodInfo self, ExtensionMethodInfo other) {
            return self.Equals(other);
        }

        public static bool operator !=(ExtensionMethodInfo self, ExtensionMethodInfo other) {
            return !self.Equals(other);
        }

        public override int GetHashCode() {
            return _method.GetHashCode();
        }
        
        /// <summary>
        /// Determines if a given type matches the type that the method extends. 
        /// The match might be non-trivial if the extended type is an open generic type with constraints.
        /// </summary>
        public bool IsExtensionOf(Type/*!*/ type) {
            ContractUtils.RequiresNotNull(type, "type");
#if CLR2 || SILVERLIGHT
            if (type == _extendedType) {
                return true;
            }
#else
            if (type.IsEquivalentTo(ExtendedType)) {
                return true;
            }
#endif
            if (!_extendedType.ContainsGenericParameters) {
                return false;
            }

            //
            // Ignores constraints that can't be instantiated given the information we have (type of the first parameter).
            //
            // For example, 
            // void Foo<S, T>(this S x, T y) where S : T;
            //
            // We make such methods available on all types. 
            // If they are not called with arguments that satisfy the constraint the overload resolver might fail.
            //
            return ReflectionUtils.BindGenericParameters(_extendedType, type, true) != null;
        }
    }
}
