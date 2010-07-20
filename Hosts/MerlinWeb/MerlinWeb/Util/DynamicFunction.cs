using Microsoft.Scripting.Hosting;


// Simple wrapper for a DLR object.  Currently, it's only used for making calls, but could be extended
// to other usage (e.g. getting atttributes)
namespace Microsoft.Web.Scripting.Util {
    public class DynamicFunction {
        private object _object;

        public DynamicFunction(object o) {
            _object = o;
        }

        public object Invoke(ScriptEngine engine, params object[] args) {
            return engine.Operations.Invoke(_object, args);
        }
    }
}
