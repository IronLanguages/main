/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using System.Dynamic;
using IronRuby.Runtime;

namespace IronRuby.Compiler {
    internal enum TopScopeFactoryKind {
        /// <summary>
        /// Simple scope without DLR Scope binding.
        /// Used by Execute("code") w/o scope.
        /// </summary>
        Default,

        /// <summary>
        /// Main script scope w/o DLR Scope binding.
        /// The factory sets TOPLEVEL_BINDING and DATA constants.
        /// Used by ExecuteProgram. 
        /// </summary>
        Main,

        /// <summary>
        /// Hosted scope, i.e. scope with DLR Scope binding.
        /// Used by Execute("code", scope).
        /// </summary>
        GlobalScopeBound,

        /// <summary>
        /// Top scope is passed by parameter to the top-level lambda, it is not scope is created.
        /// Used by eval("code"), eval("code", binding).
        /// </summary>
        None,

        /// <summary>
        /// Creates a module scope with parent scope passed into the top-level lambda.
        /// Used by module_eval("code")/instance_eval("code").
        /// </summary>
        Module,

        /// <summary>
        /// File executed via load(true).
        /// </summary>
        WrappedFile,
    }

    [Serializable]
    public sealed class RubyCompilerOptions : CompilerOptions {
        /// <summary>
        /// Embedded code. The code being compiled is embedded into an already compiled code.
        /// </summary>
        internal bool IsEval { get; set; }
        
        internal TopScopeFactoryKind FactoryKind { get; set; }
        
        private SourceLocation _initialLocation = SourceLocation.MinValue;

        internal SourceLocation InitialLocation {
            get { return _initialLocation; }
            set { _initialLocation = value; }
        }

        /// <summary>
        /// Method name used by super in eval.
        /// </summary>
        internal string TopLevelMethodName { get; set; }

        internal List<string> LocalNames { get; set; }
        internal RubyCompatibility Compatibility { get; set; }

        public RubyCompilerOptions() {
        }
        
        // copies relevant options from runtime options:
        public RubyCompilerOptions(RubyOptions/*!*/ runtimeOptions) {
            Compatibility = runtimeOptions.Compatibility;
        }
    }
}
