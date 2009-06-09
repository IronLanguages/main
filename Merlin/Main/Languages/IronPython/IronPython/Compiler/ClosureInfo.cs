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

using Microsoft.Scripting;

using IronPython.Compiler.Ast;

using MSAst = System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler {
    /// <summary>
    /// Tracking for variables lifted into closure objects. 
    /// 
    /// The primary purpose of ClosureInfo is to ensure that a parent scope
    /// has created the variables for a child scope.  There are two cases where we need
    /// to do this:
    ///     1. The parent scope defines the variable.  In this case we use a DefinitionClosureInfo
    ///        and it will create the new ClosureCell.
    ///     2. There are 3 or more scopes.  The outer most scope defines a variable and the inner most
    ///        scope refers to it.  But there is an interleaving scope which does not refer to the variable
    ///        but needs to pass it down.  In this case we have a ReferenceClosureInfo which pulls the closure
    ///        cell from the defintion (or previous reference) scope and puts it into it's own tuple.
    /// </summary>
    abstract class ClosureInfo {
        public readonly bool IsClosedOver;

        public ClosureInfo(bool isClosedOver) {
            IsClosedOver = isClosedOver;
        }

        public abstract MSAst.Expression/*!*/ GetClosureCellExpression();
        public abstract PythonVariable/*!*/ PythonVariable {
            get;
        }
        public abstract SymbolId/*!*/ Name {
            get;
        }
    }

    /// <summary>
    /// The ClosureInfo used when a variable is defined in the current scope and referenced
    /// from a child scope.
    /// </summary>
    class DefinitionClosureInfo : ClosureInfo {
        public readonly ClosureExpression/*!*/ Variable;

        public DefinitionClosureInfo(ClosureExpression/*!*/ variable, bool isClosedOver)
            : base(isClosedOver) {
            Assert.NotNull(variable);

            Variable = variable;
        }

        public override MSAst.Expression/*!*/ GetClosureCellExpression() {
            return Variable.ClosureCell;
        }

        public override PythonVariable/*!*/ PythonVariable {
            get {
                return Variable.PythonVariable;
            }
        }

        public override SymbolId/*!*/ Name {
            get {
                return Variable.Name;
            }
        }
    }

    /// <summary>
    /// The variable info used when a variable is defined in a parent scope and referenced
    /// in a child scope but not referenced in this scope.
    /// </summary>
    class ReferenceClosureInfo : ClosureInfo {
        public readonly int Index;
        public readonly PythonVariable/*!*/ Variable;
        public readonly MSAst.Expression/*!*/ TupleExpr;

        public ReferenceClosureInfo(PythonVariable/*!*/ variable, int index, MSAst.Expression/*!*/ tupleExpr, bool accessedInThisScope)
            : base(accessedInThisScope) {
            Assert.NotNull(variable, tupleExpr);

            Index = index;
            Variable = variable;
            TupleExpr = tupleExpr;
        }

        public override MSAst.Expression/*!*/ GetClosureCellExpression() {
            return MSAst.Expression.Property(
                TupleExpr,
                String.Format("Item{0:D3}", Index)
            );
        }

        public override PythonVariable/*!*/ PythonVariable {
            get {
                return Variable;
            }
        }

        public override SymbolId Name {
            get { return Variable.Name; }
        }
    }
}
