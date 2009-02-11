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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronPython.Runtime.Binding;
using IronPython.Compiler;

namespace IronPython.Runtime {
    /// <summary>
    /// TODO: Unify w/ PythonLanguageFeatures
    /// </summary>
    [Flags]
    public enum ModuleOptions {
        None = 0,
        TrueDivision    = 0x0001,        
        ShowClsMethods  = 0x0002,
        Optimized       = 0x0004,
        Initialize      = 0x0008,
        WithStatement   = 0x0010,
        AbsoluteImports = 0x0020,
        NoBuiltins      = 0x0040,
        ModuleBuiltins  = 0x0080,
        ExecOrEvalCode  = 0x0100,
        SkipFirstLine   = 0x0200,
        PrintFunction   = 0x0400,
    }

    public class PythonModule : ScopeExtension {
        private PythonLanguageFeatures _features; 
        private bool _isPythonCreatedModule;
        private bool _showCls;
        private BinderState _binderState;

        internal PythonModule(Scope scope)
            : base(scope) {
        }
        
        /// <summary>
        /// Copy constructor.
        /// </summary>
        internal PythonModule(Scope/*!*/ scope, PythonModule/*!*/ module)
            : base(scope) {
            Assert.NotNull(module);

            _features = module.LanguageFeatures;
        }

        public bool TrueDivision {
            get {
                return (_features & PythonLanguageFeatures.TrueDivision) != 0;
            }
            set {
                if (value) {
                    _features |= PythonLanguageFeatures.TrueDivision;
                } else {
                    _features &= ~PythonLanguageFeatures.TrueDivision;
                }
            }
        }

        public bool AllowWithStatement {
            get {
                return (_features & PythonLanguageFeatures.AllowWithStatement) != 0;
            }
            set {
                if (value) {
                    _features |= PythonLanguageFeatures.AllowWithStatement;
                } else {
                    _features &= ~PythonLanguageFeatures.AllowWithStatement;
                }
            }

        }

        public bool AbsoluteImports {
            get {
                return (_features & PythonLanguageFeatures.AbsoluteImports) != 0;
            }
            set {
                if (value) {
                    _features |= PythonLanguageFeatures.AbsoluteImports;
                } else {
                    _features &= ~PythonLanguageFeatures.AbsoluteImports;
                }
            }
        }

        public bool PrintFunction {
            get {
                return (_features & PythonLanguageFeatures.PrintFunction) != 0;
            }
            set {
                if (value) {
                    _features |= PythonLanguageFeatures.PrintFunction;
                } else {
                    _features &= ~PythonLanguageFeatures.PrintFunction;
                }
            }
        }

        internal PythonLanguageFeatures LanguageFeatures {
            get {
                return _features;
            }
            set {
                _features = value;
            }
        }

        public bool IsPythonCreatedModule {
            get {
                return _isPythonCreatedModule;
            }
            set {
                _isPythonCreatedModule = value;
            }
        }

        public bool ShowCls {
            get {
                return _showCls;
            }
            set {
                _showCls = value;
            }
        }

        internal BinderState BinderState {
            get {
                return _binderState;
            }
            set {
                _binderState = value;
            }
        }

        protected override void ModuleReloading() {
            base.ModuleReloading();
            _features = PythonLanguageFeatures.Default;
            _showCls = false;
        }

        internal object GetName() {
            object result;
            Scope.TryLookupName(Symbols.Name, out result);
            return result;
        }

        internal object GetFile() {
            object result;
            Scope.TryLookupName(Symbols.File, out result);
            return result;
        }

        /// <summary>
        /// Event fired when a module changes.
        /// </summary>
        public event EventHandler<ModuleChangeEventArgs> ModuleChanged;

        /// <summary>
        /// Called by the base class to fire the module change event when the
        /// module has been modified.
        /// </summary>
        internal void OnModuleChange(ModuleChangeEventArgs e) {
            EventHandler<ModuleChangeEventArgs> handler = ModuleChanged;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
