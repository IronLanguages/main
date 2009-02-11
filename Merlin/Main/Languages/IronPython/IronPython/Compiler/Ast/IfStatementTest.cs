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

using Microsoft.Scripting;

namespace IronPython.Compiler.Ast {
    public class IfStatementTest : Node {
        private SourceLocation _header;
        private readonly Expression _test;
        private Statement _body;

        public IfStatementTest(Expression test, Statement body) {
            _test = test;
            _body = body;
        }

        public SourceLocation Header {
            set { _header = value; }
            get { return _header; }
        }

        public Expression Test {
            get { return _test; }
        }

        public Statement Body {
            get { return _body; }
            set { _body = value; }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_test != null) {
                    _test.Walk(walker);
                }
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
