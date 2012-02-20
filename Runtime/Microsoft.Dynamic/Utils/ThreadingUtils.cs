using System.Threading;

#if WIN8

// When compiled with Dev10 VS CSC reports errors if this is not defined
// error CS0656: Missing compiler required member 'System.Threading.Thread.get_ManagedThreadId'
// error CS0656: Missing compiler required member 'System.Threading.Thread.get_CurrentThread'
namespace System.Threading {
    internal class Thread {
        public int ManagedThreadId { get { throw new NotImplementedException(); } }
        public static Thread CurrentThread { get { throw new NotImplementedException(); } }
    }
}

#endif

namespace Microsoft.Scripting.Utils {
    public static class ThreadingUtils {
#if CLR2 || SILVERLIGHT || WP75 // TODO: WP7?
        public static int GetCurrentThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }
#else
        private static int id;
        private static System.Threading.ThreadLocal<int> threadIds = new System.Threading.ThreadLocal<int>(() => Interlocked.Increment(ref id));
        
        public static int GetCurrentThreadId() {
            return threadIds.Value;
        }
#endif
    }
}
