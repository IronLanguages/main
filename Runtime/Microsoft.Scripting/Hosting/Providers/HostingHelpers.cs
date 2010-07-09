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
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting.Providers {

    /// <summary>
    /// Advanced APIs for HAPI providers. These methods should not be used by hosts. 
    /// They are provided for other hosting API implementers that would like to leverage existing HAPI and 
    /// extend it with language specific functionality, for example. 
    /// </summary>
    public static class HostingHelpers {
        /// <exception cref="ArgumentNullException"><paramref name="runtime"/> is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="runtime"/> is remote.</exception>
        public static ScriptDomainManager GetDomainManager(ScriptRuntime runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            return runtime.Manager;
        }

        /// <exception cref="ArgumentNullException"><paramref name="engine"/>e is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="engine"/> is remote.</exception>
        public static LanguageContext GetLanguageContext(ScriptEngine engine) {
            ContractUtils.RequiresNotNull(engine, "engine");
            return engine.LanguageContext;
        }

        /// <exception cref="ArgumentNullException"><paramref name="scriptSource"/> is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="scriptSource"/> is remote.</exception>
        public static SourceUnit GetSourceUnit(ScriptSource scriptSource) {
            ContractUtils.RequiresNotNull(scriptSource, "scriptSource");
            return scriptSource.SourceUnit;
        }

        /// <exception cref="ArgumentNullException"><paramref name="compiledCode"/> is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="compiledCode"/> is remote.</exception>
        public static ScriptCode GetScriptCode(CompiledCode compiledCode) {
            ContractUtils.RequiresNotNull(compiledCode, "compiledCode");
            return compiledCode.ScriptCode;
        }

        /// <exception cref="ArgumentNullException"><paramref name="io"/> is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="io"/> is remote.</exception>
        public static SharedIO GetSharedIO(ScriptIO io) {
            ContractUtils.RequiresNotNull(io, "io");
            return io.SharedIO;
        }

        /// <exception cref="ArgumentNullException"><paramref name="scriptScope"/> is a <c>null</c> reference.</exception>
        /// <exception cref="SerializationException"><paramref name="scriptScope"/> is remote.</exception>
        public static Scope GetScope(ScriptScope scriptScope) {
            ContractUtils.RequiresNotNull(scriptScope, "scriptScope");
            return scriptScope.Scope;
        }

        /// <exception cref="ArgumentNullException"><paramref name="engine"/> is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scope"/> is a <c>null</c> reference.</exception>
        /// <exception cref="ArgumentException"><paramref name="engine"/> is a transparent proxy.</exception>
        public static ScriptScope CreateScriptScope(ScriptEngine engine, Scope scope) {
            ContractUtils.RequiresNotNull(engine, "engine");
            ContractUtils.RequiresNotNull(scope, "scope");
#if !SILVERLIGHT
            ContractUtils.Requires(!RemotingServices.IsTransparentProxy(engine), "engine", "The engine cannot be a transparent proxy");
#endif
            return new ScriptScope(engine, scope);
        }

        /// <summary>
        /// Performs a callback in the ScriptEngine's app domain and returns the result.
        /// </summary>
        [Obsolete("You should implement a service via LanguageContext and call ScriptEngine.GetService")]
        public static TRet CallEngine<T, TRet>(ScriptEngine engine, Func<LanguageContext, T, TRet> f, T arg) {            
            return engine.Call(f, arg);
        }

        /// <summary>
        /// Creates a new DocumentationOperations object from the given DocumentationProvider.
        /// </summary>
        public static DocumentationOperations CreateDocumentationOperations(DocumentationProvider provider) {
            return new DocumentationOperations(provider);
        }
    }
}
