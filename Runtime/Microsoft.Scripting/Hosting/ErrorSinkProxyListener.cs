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

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Bridges ErrorListener and ErrorSink. It provides the reverse functionality as ErrorSinkProxyListener
    /// </summary>
    public sealed class ErrorSinkProxyListener : ErrorListener {
        private ErrorSink _errorSink;

        public ErrorSinkProxyListener(ErrorSink errorSink) {
            _errorSink = errorSink;
        }

        public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity) {
            // Note that we cannot use "source.SourceUnit" since "source" may be a proxy object, and we will not be able to marshall 
            // "source.SourceUnit" to the current AppDomain

            string code = null;
            string line = null;
            try {
                code = source.GetCode();
                line = source.GetCodeLine(span.Start.Line);
            } catch (System.IO.IOException) {
                // could not get source code.
            }

            _errorSink.Add(message, source.Path, code, line, span, errorCode, severity);
        }
    }
}