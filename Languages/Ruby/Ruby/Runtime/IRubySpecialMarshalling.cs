
namespace IronRuby.Runtime {
    
    /// <summary>Ruby classes which are backed by .NET and aren't primitives can implement this interface to
    /// do special marshalling without relying on the .NET serialization stuff.</summary>
    public interface IRubySpecialMarshalling {

        /// <summary>If TrySpecialUnmarshal returns true, it means that attrName was handled specially and should not be set as an instance variable or passed to ruby.</summary>
        bool TrySpecialUnmarshal(string attrName, object attrValue);
    }
}
