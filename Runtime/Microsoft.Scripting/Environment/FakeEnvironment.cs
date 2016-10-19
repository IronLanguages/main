#if NETCOREAPP1_0

using System.Collections;
using System.Runtime.InteropServices;

namespace System
{
    public static class FakeEnvironment
    {
        private static Lazy<OperatingSystem> osVersion = new Lazy<OperatingSystem>(() =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new OperatingSystem(PlatformID.Win32NT, new Version(0, 0));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new OperatingSystem(PlatformID.Unix, new Version(0, 0));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new OperatingSystem(PlatformID.MacOSX, new Version(0, 0));

            return null;
        });

        public static OperatingSystem OSVersion => osVersion.Value;

        public static Version Version { get; } = new Version(0, 0);

        public static string NewLine => System.Environment.NewLine;

        public static string GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);

        public static void SetEnvironmentVariable(string variable, string value) => Environment.SetEnvironmentVariable(variable, value);

        public static IDictionary GetEnvironmentVariables() => Environment.GetEnvironmentVariables();

        public static bool Is64BitProcess => RuntimeInformation.OSArchitecture == Architecture.X64;
    }
}

#endif
