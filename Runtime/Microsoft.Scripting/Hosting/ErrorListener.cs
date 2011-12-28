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

using System;
using System.Dynamic;

#if FEATURE_REMOTING
using System.Runtime.Remoting;
#else
using MarshalByRefObject = System.Object;
#endif

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// The host can use this class to track for errors reported during script parsing and compilation.
    /// Hosting API counterpart for <see cref="ErrorSink"/>.
    /// </summary>
    public abstract class ErrorListener : MarshalByRefObject {
        protected ErrorListener() {
        }

        internal void ReportError(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
            ErrorReported(source, message, span, errorCode, severity);
        }

        public abstract void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity);

#if FEATURE_REMOTING
        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
