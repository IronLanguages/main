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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Utils {
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
                string name = type.FullName;
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

        #endregion

        #region Delegates

        /// <summary>
        /// Creates an open delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(methodInfo, "methodInfo");

            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType);
            } else {
                return Delegate.CreateDelegate(delegateType, methodInfo);
            }
        }

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(MethodInfo methodInfo, Type delegateType, object target) {
            ContractUtils.RequiresNotNull(methodInfo, "methodInfo");
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        public static void GetDelegateSignature(Type delegateType, out ParameterInfo[] parameterInfos, out ParameterInfo returnInfo) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ContractUtils.Requires(invokeMethod != null, "delegateType", Strings.InvalidDelegate);

            parameterInfos = invokeMethod.GetParameters();
            returnInfo = invokeMethod.ReturnParameter;
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
            return (pi.Attributes & (ParameterAttributes.Optional | ParameterAttributes.HasDefault)) == 0;
        }

        public static bool HasDefaultValue(this ParameterInfo pi) {
            return (pi.Attributes & ParameterAttributes.HasDefault) != 0;
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

        #endregion

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
    }
}
