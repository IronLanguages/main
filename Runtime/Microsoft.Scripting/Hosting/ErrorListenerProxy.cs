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

using System.Dynamic;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// Bridges ErrorSink and ErrorListener. 
    /// Errors reported by language compilers to ErrorSink are forwarded to the ErrorListener provided by the host.
    /// </summary>
    /// <remarks>
    /// This proxy is created in the scenario when the compiler is processing a single SourceUnit.
    /// Therefore it could maintain one to one mapping from SourceUnit to ScriptSource.
    /// In a case, which shouldn't happen, that the compiler reports an error in a different SourceUnit we just create 
    /// a new instance of the ScriptSource each time. 
    /// 
    /// TODO: Consider compilation of multiple source units and creating a hashtable mapping SourceUnits to ScriptSources
    /// within the context of compilation unit.
    /// </remarks>
    internal sealed class ErrorListenerProxySink : ErrorSink {
        private readonly ErrorListener _listener;
        private readonly ScriptSource _source;

        public ErrorListenerProxySink(ScriptSource source, ErrorListener listener) {
            _listener = listener;
            _source = source;
        }

        public override void Add(SourceUnit sourceUnit, string message, SourceSpan span, int errorCode, Severity severity) {
            if (_listener != null) {

                ScriptSource scriptSource;
                if (sourceUnit != _source.SourceUnit) {
                    scriptSource = new ScriptSource(_source.Engine.Runtime.GetEngine(sourceUnit.LanguageContext), sourceUnit);
                } else {
                    scriptSource = _source;
                }

                _listener.ErrorReported(scriptSource, message, span, errorCode, severity);
            } else if (severity == Severity.FatalError || severity == Severity.Error) {
                throw new SyntaxErrorException(message, sourceUnit, span, errorCode, severity);
            }
        }
    }
}
