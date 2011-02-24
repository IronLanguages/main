using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Scripting.Hosting;

namespace HostingTest {

    /// <summary>
    /// From copied source : 
    ///    "One instance loaded into remote script app-domain, one instance runs locally."
    ///
    /// Other possible test scenario : 
    ///     Creating more then one derived Host as well as target each
    ///     override-able (i.e., virtual) and abstract methods.
    ///
    /// </summary>
    public class ScriptHostBasicSubTest : ScriptHost {
        private readonly string/*!*/ _path;
        private ScriptScope _defaultScope;

        
        public ScriptHostBasicSubTest(string/*!*/ path) {
            Debug.Assert(path != null);
            _path = path;
        }

        protected override void RuntimeAttached() {
            _defaultScope = Runtime.CreateScope();
        }

        public string/*!*/ DerivedHostPath {
            get { return _path; }
        }
    }
}
