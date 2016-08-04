#if DOTNET
namespace System.IO {
    internal static class StreamExtensions {
        public static void Close(this Stream stream) {
            stream.Dispose();
        }
    }
}
#endif


namespace System.Reflection {
    internal static class RuntimeReflectionExtensions {
#if !FEATURE_TYPE_INFO
        public static InterfaceMapping GetRuntimeInterfaceMap(this Type typeInfo, Type interfaceType) {
            return typeInfo.GetInterfaceMap(interfaceType);
        }
#endif

#if !FEATURE_GET_METHOD_INFO
        public static MethodInfo GetMethodInfo(this Delegate d) {
            return d.Method;
        }
#endif
    }
}