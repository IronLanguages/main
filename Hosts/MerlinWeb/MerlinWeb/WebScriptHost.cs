using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Diagnostics;
using System.Web;
using System.Web.Compilation;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Web.Scripting {
    public sealed class WebScriptHost : ScriptHost {
        public WebScriptHost() {
        }

        protected override void RuntimeAttached() {
            base.RuntimeAttached();

            LoadReferencedAssembliesIntoEnvironment(Runtime);
        }

        delegate Assembly[] GetReferencedAssembliesReturnArray();
        delegate ICollection GetReferencedAssembliesReturnCollection();

        private static void LoadReferencedAssembliesIntoEnvironment(ScriptRuntime environment) {
            // Make sure we're running with a System.Web.dll that has the new API's we need
            UI.NoCompileCodePageParserFilter.CheckCorrectSystemWebSupport();

            // Call BuildManager.GetReferencedAssemblies(), either the one that returns an array or an ICollection,
            // depending on what's available.
            // TODO: this is a temporary workaround for bug VSWhidbey 607089.  Once fix, we should always call
            // the one that returns an ICollection.
            ICollection referencedAssemblies = null;
            GetReferencedAssembliesReturnCollection getReferencedAssembliesCollection =
                (GetReferencedAssembliesReturnCollection)GetGetReferencedAssembliesDelegate(
                    typeof(GetReferencedAssembliesReturnCollection));
            if (getReferencedAssembliesCollection != null) {
                referencedAssemblies = getReferencedAssembliesCollection();
            } else {
                GetReferencedAssembliesReturnArray getReferencedAssembliesArray =
                    (GetReferencedAssembliesReturnArray)GetGetReferencedAssembliesDelegate(
                        typeof(GetReferencedAssembliesReturnArray));
                Debug.Assert(getReferencedAssembliesArray != null);
                if (getReferencedAssembliesArray != null) {
                    referencedAssemblies = getReferencedAssembliesArray();
                }
            }

            if (referencedAssemblies != null) {
                // Load all the BuildManager assemblies into the engine
                foreach (Assembly a in referencedAssemblies) {
                    environment.LoadAssembly(a);
                }
            }
        }

        private static Delegate GetGetReferencedAssembliesDelegate(Type delegateType) {
            return Delegate.CreateDelegate(delegateType, typeof(BuildManager),
                "GetReferencedAssemblies", false /*ignoreCase*/, false /*throwOnBindFailure*/);
        }

    }
}
