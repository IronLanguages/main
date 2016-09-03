using System.Threading;

namespace Microsoft.Scripting.Utils
{
    /// <summary>
    /// Provides a subset of methods from the System.Threading.Volatile class which is not avaiable in .NET 4.0.
    /// Implementation taken from http://referencesource.microsoft.com.
    /// </summary>
    public static class VolatileUtils
    {
        public static T Read<T>(ref T location) where T : class
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        public static void Write<T>(ref T location, T value) where T : class
        {
            Thread.MemoryBarrier();
            location = value;
        }
    }
}
