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
    [Serializable]
    public sealed class RubyCompilerOptions : CompilerOptions {
        // TODO: replace bool's by flags/enum
        internal bool IsEval { get; set; }
        internal bool IsModuleEval { get; set; }
        internal bool IsIncluded { get; set; }
        internal bool IsWrapped { get; set; }

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
