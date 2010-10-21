/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using IronRuby.Compiler.Ast;
using Microsoft.IronStudio.Navigation;
using Microsoft.Scripting;
using Microsoft.IronRubyTools.Intellisense;

namespace Microsoft.IronRubyTools.Navigation {
    internal sealed class AstScopeNode : IScopeNode {
        private readonly SourceUnitTree _ast;
        private readonly ProjectEntry _projectEntry;
        
        public AstScopeNode(SourceUnitTree ast, ProjectEntry projectEntry) {
            _ast = ast;
            _projectEntry = projectEntry;
        }

        #region IScopeNode Members

        public bool IsFunction {
            get { return false; }
        }

        public string Name {
            get { return "TODO: source unit name"; } // _ast.Name;
        }

        public string Description {
            get { return "TODO: source unit description"; } //_ast.Documentation;
        }

        public SourceLocation Start {
            get { return _ast.Location.Start; }
        }

        public SourceLocation End {
            get { return _ast.Location.End; }
        }

        public IEnumerable<IScopeNode> NestedScopes {
            get {
                // TODO:
                return EnumerateBody(_ast.Statements);
            }
        }

        internal static IEnumerable<IScopeNode> EnumerateBody(Statements body) {
            // TODO:
            foreach (Expression expr in body) {
                ModuleDefinition moduleDef = expr as ModuleDefinition;
                if (moduleDef != null) {
                    yield return new ModuleScopeNode(moduleDef);
                }

                MethodDefinition methodDef = expr as MethodDefinition;
                if (methodDef != null) {
                    yield return new MethodScopeNode(methodDef);
                }
            }
        }

        #endregion
    }
}
