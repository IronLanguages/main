#if WIN8 || ANDROID

using System;
using System.Runtime.Serialization;

namespace System.ComponentModel {
    [Serializable]
    public class WarningException : SystemException {
        private readonly string helpUrl;
        private readonly string helpTopic;
        
        public string HelpUrl {
            get {
                return this.helpUrl;
            }
        }
        
        public string HelpTopic {
            get {
                return this.helpTopic;
            }
        }
        
        public WarningException()
            : this(null, null, null) {
        }
        
        public WarningException(string message)
            : this(message, null, null) {
        }
        
        public WarningException(string message, string helpUrl)
            : this(message, helpUrl, null) {
        }
        
        public WarningException(string message, Exception innerException)
            : base(message, innerException) {
        }
        
        public WarningException(string message, string helpUrl, string helpTopic)
            : base(message) {
            this.helpUrl = helpUrl;
            this.helpTopic = helpTopic;
        }

#if FEATURE_SERIALIZATION
        protected WarningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}
#endif