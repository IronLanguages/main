/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_REMOTING
using System.Runtime.Remoting;
#else
using MarshalByRefObject = System.Object;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// ScriptHost is collocated with ScriptRuntime in the same app-domain. 
    /// The host can implement a derived class to consume some notifications and/or 
    /// customize operations like TryGetSourceUnit,ResolveSourceUnit, etc.
    ///
    /// The areguments to the the constructor of the derived class are specified in ScriptRuntimeSetup 
    /// instance that enters ScriptRuntime initialization.
    /// 
    /// If the host is remote with respect to DLR (i.e. also with respect to ScriptHost)
    /// and needs to access objects living in its app-domain it can pass MarshalByRefObject 
    /// as an argument to its ScriptHost subclass constructor.
    /// </summary>
    public class ScriptHost : MarshalByRefObject {
        /// <summary>
        /// The runtime the host is attached to.
        /// </summary>
        private ScriptRuntime _runtime;
        
        public ScriptHost() {
        }

        // Called by ScriptRuntime when it is completely initialized. 
        // Notifies the host implementation that the runtime is available now.
        internal void SetRuntime(ScriptRuntime runtime) {
            Assert.NotNull(runtime);
            _runtime = runtime;

            RuntimeAttached();
        }

        public ScriptRuntime Runtime {
            get {
                if (_runtime == null) {
                    throw new InvalidOperationException("Host not initialized");
                }
                return _runtime;
            }
        }

        public virtual PlatformAdaptationLayer PlatformAdaptationLayer {
            get {
                return PlatformAdaptationLayer.Default;
            }
        }

        #region Notifications

        /// <summary>
        /// Invoked after the initialization of the associated Runtime is finished.
        /// The host can override this method to perform additional initialization of runtime (like loading assemblies etc.).
        /// </summary>
        protected virtual void RuntimeAttached() {
            // nop
        }

        /// <summary>
        /// Invoked after a new language is loaded into the Runtime.
        /// The host can override this method to perform additional initialization of language engines.
        /// </summary>
        internal protected virtual void EngineCreated(ScriptEngine engine) {
            // nop
        }

        #endregion

#if FEATURE_REMOTING
        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }

}
