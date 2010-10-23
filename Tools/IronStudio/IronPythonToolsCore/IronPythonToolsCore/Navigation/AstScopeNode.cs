/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.IronStudio.Navigation;
using Microsoft.PyAnalysis;
using Microsoft.Scripting;

namespace Microsoft.IronPythonTools.Navigation {
    class AstScopeNode : IScopeNode {
        private readonly PythonAst _ast;
        private readonly IPythonProjectEntry _projectEntry;
        
        public AstScopeNode(PythonAst pythonAst, IPythonProjectEntry projectEntry) {
            _ast = pythonAst;
            _projectEntry = projectEntry;
        }

        #region IScopeNode Members

        public bool IsFunction {
            get { return false; }
        }

        public string Name {
            get { return _ast.Name; }
        }

        public string Description {
            get { return _ast.Documentation; }
        }

        public SourceLocation Start {
            get { return _ast.Start; }
        }

        public SourceLocation End {
            get { return _ast.End; }
        }

        public IEnumerable<IScopeNode> NestedScopes {
            get {
                return EnumerateBody(_ast.Body);
            }
        }

        internal static IEnumerable<IScopeNode> EnumerateBody(Statement body) {
            SuiteStatement suite = body as SuiteStatement;
            if (suite != null) {
                foreach (Statement stmt in suite.Statements) {
                    ClassDefinition klass = stmt as ClassDefinition;
                    if (klass != null) {
                        yield return new ClassScopeNode(klass);
                    }

                    FunctionDefinition func = stmt as FunctionDefinition;
                    if (func != null) {
                        yield return new FunctionScopeNode(func);
                    }
                }
            }
        }

        #endregion
    }
}
