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
using System.Runtime.Remoting;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using System.Security.Permissions;
using System.Threading;

namespace Microsoft.Scripting.Hosting {

    /// <summary>
    /// Hosting API counterpart for <see cref="ScriptCode"/>.
    /// </summary>
    public sealed class CompiledCode
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        private readonly ScriptEngine _engine;
        private readonly ScriptCode _code;

        private ScriptScope _defaultScope;

        internal ScriptCode ScriptCode { get { return _code; } }

        internal CompiledCode(ScriptEngine engine, ScriptCode code) {
            Assert.NotNull(engine);
            Assert.NotNull(code);

            _engine = engine;
            _code = code;
        }

        /// <summary>
        /// Engine that compiled this code.
        /// </summary>
        public ScriptEngine Engine {
            get { return _engine; }
        }

        /// <summary>
        /// Default scope for this code.
        /// </summary>
        public ScriptScope DefaultScope {
            get {
                if (_defaultScope == null) {
                    Interlocked.CompareExchange(ref _defaultScope, new ScriptScope(_engine, _code.CreateScope()), null);
                }
                return _defaultScope; 
            }
        }

        /// <summary>
        /// Executes code in a default scope.
        /// </summary>
        public object Execute() {
            return _code.Run(DefaultScope.Scope);
        }

        /// <summary>
        /// Execute code within a given scope and returns the result.
        /// </summary>
        public object Execute(ScriptScope scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            return _code.Run(scope.Scope);
        }

        /// <summary>
        /// Executes code in in a default scope and converts to a given type.
        /// </summary>
        public T Execute<T>() {
            return _engine.Operations.ConvertTo<T>(Execute());
        }

        /// <summary>
        /// Execute code within a given scope and converts result to a given type.
        /// </summary>
        public T Execute<T>(ScriptScope scope) {
            return _engine.Operations.ConvertTo<T>(Execute(scope));
        }


#if !SILVERLIGHT
        public ObjectHandle ExecuteAndWrap() {
            return new ObjectHandle(Execute());
        }

        public ObjectHandle ExecuteAndWrap(ScriptScope scope) {
            return new ObjectHandle(Execute(scope));
        }

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
