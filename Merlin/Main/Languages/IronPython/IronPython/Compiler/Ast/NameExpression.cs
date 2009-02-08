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
using System.Dynamic;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class NameExpression : Expression {
        private readonly SymbolId _name;
        private PythonReference _reference;
        private bool _assigned;                  // definitely assigned

        public NameExpression(SymbolId name) {
            _name = name;
        }

        public SymbolId Name {
            get { return _name; }
        }

        internal PythonReference Reference {
            get { return _reference; }
            set { _reference = value; }
        }

        internal bool Assigned {
            get { return _assigned; }
            set { _assigned = value; }
        }

        public override string ToString() {
            return base.ToString() + ":" + SymbolTable.IdToString(_name);
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Expression read = _reference.Variable;
            if (read == null) {
                read = AstUtils.Read(_name);
            }

            if (!_assigned && !(read is Microsoft.Scripting.Ast.GlobalVariableExpression)) {
                read = Ast.Call(
                    AstGenerator.GetHelperMethod("CheckUninitialized"),
                    read,
                    AstUtils.Constant(_name)
                );
            }

            return read;
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, Operators op) {
            MSAst.Expression variable = _reference.Variable;
            MSAst.Expression assignment;

            Type vt = variable != null ? variable.Type : typeof(object);

            if (op != Operators.None) {
                right = Binders.Operation(
                    ag.BinderState,
                    vt,
                    StandardOperators.FromOperator(op),
                    Transform(ag, vt),
                    right
                );
            }

            if (variable != null) {
                assignment = AstUtils.Assign(variable, AstGenerator.ConvertIfNeeded(right, variable.Type));
            } else {
                assignment = AstUtils.Assign(_name, right);
            }

            SourceSpan aspan = span.IsValid ? new SourceSpan(Span.Start, span.End) : SourceSpan.None;
            return ag.AddDebugInfoAndVoid(assignment, aspan);
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            MSAst.Expression variable = _reference.Variable;
            if (variable != null && !ag.Block.Global) {
                // keep the variable alive until we hit the del statement to
                // better match CPython's lifetimes
                MSAst.Expression del = Ast.Block(
                    Ast.Call(                                   
                        typeof(GC).GetMethod("KeepAlive"),
                        variable
                    ),
                    ag.AddDebugInfo(AstUtils.Delete(variable), Span)
                );
                    
                if (!_assigned) {
                    del = Ast.Block(
                        Transform(ag, variable.Type),
                        del,
                        Ast.Empty()
                    );
                }
                return del;
            } else {
                return ag.AddDebugInfo(AstUtils.Delete(_name), Span);
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }

        internal override bool CanThrow {
            get {
                return !Assigned;
            }
        }
    }
}
