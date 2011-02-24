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

namespace Microsoft.IronRubyTools.Navigation {
    internal sealed class ModuleScopeNode : IScopeNode {
        private readonly ModuleDefinition _definition;

        public ModuleScopeNode(ModuleDefinition definition) {
            _definition = definition;
        }

        #region IScopeNode Members

        public bool IsFunction {
            get { return false; }
        }

        public string Name {
            get { return "TODO: name"; }
        }

        public string Description {
            get { return "TODO: description"; }
        }

        public SourceLocation Start {
            get { return _definition.Location.Start; }
        }

        public SourceLocation End {
            get { return _definition.Location.End; }
        }

        public IEnumerable<IScopeNode> NestedScopes {
            get {
                // TODO:
                return AstScopeNode.EnumerateBody(_definition.Body.Statements);
            }
        }

        #endregion
    }
}
