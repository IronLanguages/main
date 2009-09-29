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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;
using System.Threading;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting.Providers;
using IronPython.Runtime.Operations;

namespace IronPython.Hosting {
    /// <summary>
    /// Helper class for implementing the Python class.
    /// 
    /// This is exposed as a service through PythonEngine and the helper class
    /// uses this service to get the correct remoting semantics.
    /// </summary>
    internal sealed class PythonService 
#if !SILVERLIGHT
        : MarshalByRefObject 
#endif
    {
        private readonly ScriptEngine/*!*/ _engine;
        private readonly PythonContext/*!*/ _context;
        private ScriptScope _sys, _builtins, _clr;

        public PythonService(PythonContext/*!*/ context, ScriptEngine/*!*/ engine) {
            Assert.NotNull(context, engine);
            _context = context;
            _engine = engine;
        }

        public ScriptScope/*!*/ GetSystemState() {
            if (_sys == null) {
                Interlocked.CompareExchange(
                    ref _sys,
                    HostingHelpers.CreateScriptScope(_engine, _context.SystemState.Scope),
                    null
                );
            }

            return _sys;
        }

        public ScriptScope/*!*/ GetBuiltins() {
            if (_builtins == null) {
                Interlocked.CompareExchange(
                    ref _builtins,
                    HostingHelpers.CreateScriptScope(_engine, _context.BuiltinModuleInstance.Scope),
                    null
                );
            }

            return _builtins;
        }

        public ScriptScope/*!*/ GetClr() {
            if (_clr == null) {
                Interlocked.CompareExchange(
                    ref _clr,
                    HostingHelpers.CreateScriptScope(_engine, _context.ClrModule.Scope),
                    null
                );
            }

            return _clr;
        }

        public ScriptScope/*!*/ ImportModule(ScriptEngine/*!*/ engine, string/*!*/ name) {
            PythonModule module = Importer.ImportModule(_context.SharedClsContext, _context.SharedClsContext.GlobalDict, name, false, -1) as PythonModule;
            if (module != null) {
                return HostingHelpers.CreateScriptScope(engine, module.Scope);
            }

            throw PythonOps.ImportError("no module named {0}", name);
        }

#if !SILVERLIGHT
        public override object InitializeLifetimeService() {
            // track the engines lifetime
            return _engine.InitializeLifetimeService();
        }
#endif
    }
}
