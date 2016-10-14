#if NETCOREAPP1_0

namespace System {
    [Serializable]
    public class ExternalException : Exception {
        public ExternalException()
            : base("External exception") {
        }

        public ExternalException(string message)
            : base(message) {
        }
        
        public ExternalException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}

#endif
