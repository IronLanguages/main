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

using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    public class Arg : Node {
        private readonly SymbolId _name;
        private readonly Expression _expression;

        public Arg(Expression expression) : this(SymbolId.Empty, expression) { }

        public Arg(SymbolId name, Expression expression) {
            _name = name;
            _expression = expression;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Expression Expression {
            get { return _expression; }
        } 

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal Argument Transform(AstGenerator ag, out MSAst.Expression expression) {
            expression = ag.Transform(_expression);

            if (_name == SymbolId.Empty) {
                return Argument.Simple;
            } else if (_name == Symbols.Star) {
                return new Argument(ArgumentType.List);
            } else if (_name == Symbols.StarStar) {
                return new Argument(ArgumentType.Dictionary);
            } else {
                return new Argument(_name);
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_expression != null) {
                    _expression.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
