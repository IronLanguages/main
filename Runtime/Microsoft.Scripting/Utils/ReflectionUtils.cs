using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Microsoft.Scripting.Utils {
    internal static class ReflectionUtils {
#if WIN8
        public static MethodInfo WithSignature(this IEnumerable<MethodInfo> members, Type[] parameterTypes)
        {
            return members.Where(c =>
            {
                var ps = c.GetParameters();
                if (ps.Length != parameterTypes.Length)
                {
                    return false;
                }

                for (int i = 0; i < ps.Length; i++)
                {
                    if (parameterTypes[i] != ps[i].ParameterType)
                    {
                        return false;
                    }
                }

                return true;
            }).Single();
        }

        public static ConstructorInfo WithSignature(this IEnumerable<ConstructorInfo> members, Type[] parameterTypes)
        {
            return members.Where(c =>
            {
                var ps = c.GetParameters();
                if (ps.Length != parameterTypes.Length)
                {
                    return false;
                }

                for (int i = 0; i < ps.Length; i++)
                {
                    if (parameterTypes[i] != ps[i].ParameterType)
                    {
                        return false;
                    }
                }

                return true;
            }).Single();
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] parameterTypes) {
            return type.GetTypeInfo().DeclaredConstructors.WithSignature(parameterTypes);
        }

        public static MethodInfo GetMethod(this Type type, string name) {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] parameterTypes) {
            return type.GetTypeInfo().GetDeclaredMethods(name).WithSignature(parameterTypes);
        }

        public static PropertyInfo GetProperty(this Type type, string name) {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

        public static FieldInfo GetField(this Type type, string name) {
            return type.GetTypeInfo().GetDeclaredField(name);
        }
#else
        public static MethodInfo GetMethodInfo(this Delegate d) {
            return d.Method;
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name) {
            return type.GetMember(name).OfType<MethodInfo>();
        }
#endif
    }
}
