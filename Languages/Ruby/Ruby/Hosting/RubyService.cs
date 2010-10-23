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
using System.Security.Permissions;
using IronRuby.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Hosting {
    public sealed class RubyService
#if !SILVERLIGHT
 : MarshalByRefObject
#endif
 {
        private readonly ScriptEngine/*!*/ _engine;
        private readonly RubyContext/*!*/ _context;

        internal RubyService(RubyContext/*!*/ context, ScriptEngine/*!*/ engine) {
            Assert.NotNull(context, engine);
            _context = context;
            _engine = engine;
        }

        /// <summary>
        /// Loads a given script file using the semantics of Kernel#require method.
        /// </summary>
        /// <param name="engine">The Ruby engine.</param>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>Whether the file has already been required.</returns>
        /// <remarks>
        /// If a relative path is given the current search paths are used to locate the script file.
        /// The current search paths could be read and modified via <see cref="ScriptEngine.GetSearchPaths"/> and <see cref="ScriptEngine.SetSearchPaths"/>,
        /// respectively.
        /// </remarks>
        public bool RequireFile(string/*!*/ path) {
            ContractUtils.RequiresNotNull(path, "path");
            return RequireFile(path, (Scope)null);
        }

        /// <summary>
        /// Loads a given script file using the semantics of Kernel#require method.
        /// The script is executed within the context of a given <see cref="ScriptScope"/>.
        /// </summary>
        /// <param name="engine">The Ruby engine.</param>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="scope">The scope to use for the script execution.</param>
        /// <returns>Whether the file has already been required.</returns>
        /// <remarks>
        /// If a relative path is given the current search paths are used to locate the script file.
        /// The current search paths could be read and modified via <see cref="ScriptEngine.GetSearchPaths"/> and <see cref="ScriptEngine.SetSearchPaths"/>,
        /// respectively.
        /// </remarks>
        public bool RequireFile(string/*!*/ path, ScriptScope/*!*/ scope) {
            ContractUtils.RequiresNotNull(path, "path");
            ContractUtils.RequiresNotNull(scope, "scope");
            return RequireFile(path, HostingHelpers.GetScope(scope));
        }

        private bool RequireFile(string/*!*/ path, Scope scope) {
            return _context.Loader.LoadFile(scope, null, _context.EncodePath(path), LoadFlags.Require);
        }

#if !SILVERLIGHT
        public override object InitializeLifetimeService() {
            // track the engines lifetime
            return _engine.InitializeLifetimeService();
        }
#endif
    }
}
