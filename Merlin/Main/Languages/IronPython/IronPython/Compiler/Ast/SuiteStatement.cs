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

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;
    
    public sealed class SuiteStatement : Statement {
        private readonly Statement[] _statements;

        public SuiteStatement(Statement[] statements) {
            Assert.NotNull(statements);
            _statements = statements;
        }

        public IList<Statement> Statements {
            get { return _statements; }
        } 

        internal override MSAst.Expression Transform(AstGenerator ag) {
            if (_statements.Length == 0) {
                return AstGenerator.EmptyBlock;
            }

            var stmts = ag.Transform(_statements);

            foreach (MSAst.Expression stmt in stmts) {
                if (stmt == null) {
                    // error was encountered and added to sync, 
                    // don't return any of the block
                    return null;
                }
            }
            return Ast.Block(stmts);
        }
       
        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_statements != null) {
                    foreach (Statement s in _statements) {
                        s.Walk(walker);
                    }
                }
            }
            walker.PostWalk(this);
        }

        public override string Documentation {
            get {                
                if (_statements.Length > 0) {
                    return _statements[0].Documentation;
                }
                return null;
            }
        }

        internal override bool CanThrow {
            get {
                foreach (Statement stmt in _statements) {
                    if (stmt.CanThrow) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
