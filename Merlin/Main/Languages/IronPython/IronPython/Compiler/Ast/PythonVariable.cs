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
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    internal class PythonVariable {
        private readonly SymbolId _name;
        private readonly Type/*!*/ _type;
        private readonly ScopeStatement/*!*/ _scope;

        private bool _deleted;              // del x
        private bool _readBeforeInitialized;
        private bool _accessedInNestedScope;

        private bool _fallback;             // If uninitialized, lookup in builtins

        private int _index;                 // Index for flow checker

        private VariableKind _kind;
        private MSAst.Expression _variable;

        public PythonVariable(SymbolId name, Type/*!*/ type, VariableKind kind, ScopeStatement/*!*/ scope) {
            Assert.NotNull(type, scope);
            _name = name;
            _type = type;
            _kind = kind;
            _scope = scope;
        }

        public SymbolId Name {
            get { return _name; }
        }

        public Type Type {
            get { return _type; }
        }

        public ScopeStatement Scope {
            get { return _scope; }
        }

        public VariableKind Kind {
            get { return _kind; }
            set { _kind = value; }
        }

        internal bool Deleted {
            get { return _deleted; }
            set { _deleted = value; }
        }

        internal int Index {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// True iff there is a path in control flow graph on which the variable is used before initialized (assigned or deleted).
        /// </summary>
        public bool ReadBeforeInitialized {
            get { return _readBeforeInitialized; }
            set { _readBeforeInitialized = value; }
        }

        /// <summary>
        /// True iff the variable is referred to from the inner scope.
        /// </summary>
        public bool AccessedInNestedScope {
            get { return _accessedInNestedScope; }
            set { _accessedInNestedScope = value; }
        }

        internal bool Fallback {
            get { return _fallback; }
            set { _fallback = value; }
        }

        public MSAst.Expression Variable {
            get {
                Debug.Assert(_variable != null);
                return _variable;
            }
        }

        internal void SetParameter(MSAst.ParameterExpression parameter) {
            Debug.Assert(_variable == null);
            _variable = parameter;
        }

        internal MSAst.Expression Transform(AstGenerator ag) {
            Debug.Assert(_kind != VariableKind.Parameter);

            string name = SymbolTable.IdToString(_name);
            switch (_kind) {
                case VariableKind.Global:
                    return _variable = Utils.GlobalVariable(_type, name);

                case VariableKind.Local:
                case VariableKind.HiddenLocal:
                case VariableKind.GlobalLocal:
                    if (_accessedInNestedScope) {
                        return _variable = ag.Block.ClosedOverVariable(_type, name);
                    } else {
                        return _variable = ag.Block.Variable(_type, name);
                    }

                case VariableKind.Temporary:
                    return _variable = ag.Block.HiddenVariable(_type, name);

                default: 
                    throw Assert.Unreachable;
            }
        }
    }
}
