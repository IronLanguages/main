/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;

namespace Microsoft.Scripting.Library {
    public class CollectingErrorSink : ErrorSink {
        public List<ErrorResult> Errors;
        public List<ErrorResult> Warnings;

        public CollectingErrorSink() {
            Errors = new List<ErrorResult>();
            Warnings = new List<ErrorResult>();
        }

        public override void Add(SourceUnit source, string message, SourceSpan span, int errorCode, Severity severity) {
            span = Add(message, span, severity);
        }

        private SourceSpan Add(string message, SourceSpan span, Severity severity) {
            if (severity == Severity.Warning) {
                Warnings.Add(new ErrorResult(message, span));
            } else if (severity == Severity.FatalError || severity == Severity.Error) {
                Errors.Add(new ErrorResult(message, span));
            }
            return span;
        }

        public override void Add(string message, string path, string code, string line, SourceSpan span, int errorCode, Severity severity) {
            Add(message, span, severity);
        }
    }

}
