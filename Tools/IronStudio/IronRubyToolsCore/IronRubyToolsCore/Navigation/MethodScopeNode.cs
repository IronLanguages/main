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
using Microsoft.IronStudio.Navigation;
using Microsoft.Scripting;
using IronRuby.Compiler.Ast;

namespace Microsoft.IronRubyTools.Navigation {
    internal sealed class MethodScopeNode : IScopeNode {
        private readonly MethodDefinition _method;

        public MethodScopeNode(MethodDefinition method) {
            _method = method;
        }

        public MethodDefinition Definition {
            get {
                return _method;
            }
        }

        #region IScopeNode Members

        public bool IsFunction {
            get { return true; }
        }

        public string Name {
            get { return _method.Name; }
        }

        public string Description {
            get { return _method.Name; }
        }

        public SourceLocation Start {
            get { return _method.Location.Start; }
        }

        public SourceLocation End {
            get { return _method.Location.End; }
        }

        public IEnumerable<IScopeNode> NestedScopes {
            get { 
                // TODO:
                return AstScopeNode.EnumerateBody(_method.Body.Statements); 
            }
        }

        #endregion
    }
}
