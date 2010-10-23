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
using System.Dynamic;
using Microsoft.Scripting.Utils;
using System.Security.Permissions;
using System.Threading;

#if CLR2
using dynamic = System.Object;
#endif

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
        public dynamic Execute() {
            return _code.Run(DefaultScope.Scope);
        }

        /// <summary>
        /// Execute code within a given scope and returns the result.
        /// </summary>
        public dynamic Execute(ScriptScope scope) {
            ContractUtils.RequiresNotNull(scope, "scope");
            return _code.Run(scope.Scope);
        }

        /// <summary>
        /// Executes code in in a default scope and converts to a given type.
        /// </summary>
        public T Execute<T>() {
            return _engine.Operations.ConvertTo<T>((object)Execute());
        }

        /// <summary>
        /// Execute code within a given scope and converts result to a given type.
        /// </summary>
        public T Execute<T>(ScriptScope scope) {
            return _engine.Operations.ConvertTo<T>((object)Execute(scope));
        }


#if !SILVERLIGHT
        /// <summary>
        /// Executes the code in an empty scope.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// </summary>
        public ObjectHandle ExecuteAndWrap() {
            return new ObjectHandle((object)Execute());
        }

        /// <summary>
        /// Executes the code in the specified scope.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// </summary>
        public ObjectHandle ExecuteAndWrap(ScriptScope scope) {
            return new ObjectHandle((object)Execute(scope));
        }

        /// <summary>
        /// Executes the code in an empty scope.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// 
        /// If an exception is thrown the exception is caught and an ObjectHandle to
        /// the exception is provided.
        /// </summary>
        /// <remarks>
        /// Use this API to handle non-serializable exceptions (exceptions might not be serializable due to security restrictions) 
        /// or if an exception serialization loses information.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ObjectHandle ExecuteAndWrap(out ObjectHandle exception) {
            exception = null;
            try {
                return new ObjectHandle((object)Execute());
            } catch (Exception e) {
                exception = new ObjectHandle(e);
                return null;
            }
        }

        /// <summary>
        /// Executes the expression in the specified scope and return a result.
        /// Returns an ObjectHandle wrapping the resulting value of running the code.  
        /// 
        /// If an exception is thrown the exception is caught and an ObjectHandle to
        /// the exception is provided.
        /// </summary>
        /// <remarks>
        /// Use this API to handle non-serializable exceptions (exceptions might not be serializable due to security restrictions) 
        /// or if an exception serialization loses information.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ObjectHandle ExecuteAndWrap(ScriptScope scope, out ObjectHandle exception) {
            exception = null;
            try{
                return new ObjectHandle((object)Execute(scope));
            } catch (Exception e) {
                exception = new ObjectHandle(e);
                return null;
            }
        }

        // TODO: Figure out what is the right lifetime
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
    }
}
