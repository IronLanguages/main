/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Dynamic;
using System.Security.Permissions;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// The host can use this class to track for errors reported during script parsing and compilation.
    /// Hosting API counterpart for <see cref="ErrorSink"/>.
    /// </summary>
    public abstract class ErrorListener
#if !SILVERLIGHT
         : MarshalByRefObject
#endif
    {
        protected ErrorListener() {
        }

        internal void ReportError(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
            ErrorReported(source, message, span, errorCode, severity);
        }

        public abstract void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity);

#if !SILVERLIGHT
        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
