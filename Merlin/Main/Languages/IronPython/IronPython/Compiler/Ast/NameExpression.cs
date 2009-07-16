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
using IronPython.Runtime.Operations;

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
            MSAst.Expression read;
            if (_reference.PythonVariable == null) {
                read = Ast.Call(
                    typeof(PythonOps).GetMethod("LookupName"),
                    ag.LocalContext,
                    ag.Globals.GetSymbol(_name)
                );
            } else {
                read = ag.Globals.GetVariable(ag, _reference.PythonVariable);
            }

            if (!_assigned && !(read is IPythonGlobalExpression)) {
                read = Ast.Call(
                    AstGenerator.GetHelperMethod("CheckUninitialized"),
                    read,
                    ag.Globals.GetSymbol(_name)
                );
            }

            return read;
        }

        internal override MSAst.Expression TransformSet(AstGenerator ag, SourceSpan span, MSAst.Expression right, PythonOperationKind op) {
            MSAst.Expression assignment;

            if (op != PythonOperationKind.None) {
                right = ag.Operation(
                    typeof(object),
                    op,
                    Transform(ag, typeof(object)),
                    right
                );
            }

            if (_reference.PythonVariable != null) {
                assignment = ag.Globals.Assign(
                    ag.Globals.GetVariable(ag, _reference.PythonVariable), 
                    AstGenerator.ConvertIfNeeded(right, typeof(object))
                );
            } else {
                assignment = Ast.Call(
                    null,
                    typeof(PythonOps).GetMethod("SetName"),
                    new [] {
                        ag.LocalContext, 
                        ag.Globals.GetSymbol(_name),
                        AstUtils.Convert(right, typeof(object))
                        }
                );
            }

            SourceSpan aspan = span.IsValid ? new SourceSpan(Span.Start, span.End) : SourceSpan.None;
            return ag.AddDebugInfoAndVoid(assignment, aspan);
        }

        internal override string CheckAssign() {
            return null;
        }

        internal override string CheckDelete() {
            return null;
        }

        internal override MSAst.Expression TransformDelete(AstGenerator ag) {
            if (_reference.PythonVariable != null && !ag.IsGlobal) {
                MSAst.Expression variable = ag.Globals.GetVariable(ag, _reference.PythonVariable);
                // keep the variable alive until we hit the del statement to
                // better match CPython's lifetimes
                MSAst.Expression del = Ast.Block(
                    Ast.Call(
                        typeof(GC).GetMethod("KeepAlive"),
                        variable
                    ),
                    ag.Globals.Delete(variable)
                );

                if (!_assigned) {
                    del = Ast.Block(
                        Transform(ag, variable.Type),
                        del,
                        AstUtils.Empty()
                    );
                }
                return del;
            } else {
                return Ast.Call(
                    typeof(PythonOps).GetMethod("RemoveName"),
                    new[] {
                        ag.LocalContext,
                        ag.Globals.GetSymbol(_name)
                    }
                );
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
