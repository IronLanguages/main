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
using System.ComponentModel;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Hosting.Providers {

    /// <summary>
    /// Advanced APIs for HAPI providers. These methods should not be used by hosts. 
    /// They are provided for other hosting API implementers that would like to leverage existing HAPI and 
    /// extend it with language specific functionality, for example. 
    /// </summary>
    public static class HostingHelpers {
        public static ScriptDomainManager GetDomainManager(ScriptRuntime runtime) {
            ContractUtils.RequiresNotNull(runtime, "runtime");
            return runtime.Manager;
        }

        public static LanguageContext GetLanguageContext(ScriptEngine engine) {
            ContractUtils.RequiresNotNull(engine, "engine");
            return engine.LanguageContext;
        }

        public static SourceUnit GetSourceUnit(ScriptSource scriptSource) {
            ContractUtils.RequiresNotNull(scriptSource, "scriptSource");
            return scriptSource.SourceUnit;
        }

        public static ScriptCode GetScriptCode(CompiledCode compiledCode) {
            ContractUtils.RequiresNotNull(compiledCode, "compiledCode");
            return compiledCode.ScriptCode;
        }

        public static SharedIO GetSharedIO(ScriptIO io) {
            ContractUtils.RequiresNotNull(io, "io");
            return io.SharedIO;
        }

        public static Scope GetScope(ScriptScope scriptScope) {
            ContractUtils.RequiresNotNull(scriptScope, "scriptScope");
            return scriptScope.Scope;
        }

        public static ScriptScope CreateScriptScope(ScriptEngine engine, Scope scope) {
            return new ScriptScope(engine, scope);
        }

        /// <summary>
        /// Performs a callback in the ScriptEngine's app domain and returns the result.
        /// </summary>
        public static TRet CallEngine<T, TRet>(ScriptEngine engine, Func<LanguageContext, T, TRet> f, T arg) {
            return engine.Call(f, arg);
        }
    }
}
