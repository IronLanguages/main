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

namespace Microsoft.Scripting.Utils {
    public static class ReflectionUtils {

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        public const char GenericArityDelimiter = '`';

#if SILVERLIGHT
        public static bool IsNested(Type t) {
            return t.DeclaringType != null;
        }
#else
        public static bool IsNested(Type t) { return t.IsNested; }
#endif

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
            Type[] result = new Type[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++) {
                result[i] = parameterInfos[i].ParameterType;
            }
            return result;
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


    }
}
