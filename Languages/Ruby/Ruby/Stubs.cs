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