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

using System.Diagnostics;
using System.Threading;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime {
    public static class DefaultContext {
        [MultiRuntimeAware]
        public static CodeContext _default;
        [MultiRuntimeAware]
        public static CodeContext _defaultCLS;
        [MultiRuntimeAware]
        public static PythonBinder _defaultBinder;
        
        public static ContextId Id {
            get {
                return Default.LanguageContext.ContextId;
            }
        }
        
        public static CodeContext Default {
            get {
                Debug.Assert(_default != null);
                return _default;
            }
        }

        public static PythonBinder DefaultPythonBinder {
            get {
                return _defaultBinder;
            }
        }

        public static PythonContext DefaultPythonContext {
            get {
                Debug.Assert(_default != null);
                return (PythonContext)_default.LanguageContext;
            }
        }

        public static CodeContext DefaultCLS {
            get {
                Debug.Assert(_defaultCLS != null);
                return _defaultCLS;
            }
        }

        internal static void CreateContexts(ScriptDomainManager manager, PythonContext/*!*/ context) {
            if (_default == null) {
                Interlocked.CompareExchange(ref _default, CreateDefaultContext(context), null);
                Interlocked.CompareExchange(ref _defaultBinder, new PythonBinder(manager, context, null), null);
            }
        }

        internal static void CreateClsContexts(ScriptDomainManager manager, PythonContext/*!*/ context) {
            if (_defaultCLS == null) {
                Interlocked.CompareExchange(ref _defaultCLS, CreateDefaultCLSContext(context), null);
            }
        }

        internal static CodeContext/*!*/ CreateDefaultContext(PythonContext/*!*/ context) {
            PythonModule module = new PythonModule(new Scope());
            module.Scope.SetExtension(context.ContextId, module);
            return new CodeContext(module.Scope, context);
        }


        internal static CodeContext/*!*/ CreateDefaultCLSContext(PythonContext/*!*/ context) {
            PythonModule globalMod = context.CreateModule(ModuleOptions.ShowClsMethods | ModuleOptions.NoBuiltins);
            return new CodeContext(globalMod.Scope, context);
        }
    }
}
