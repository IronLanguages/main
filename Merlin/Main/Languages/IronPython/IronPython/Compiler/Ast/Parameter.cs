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

using System.Collections.Generic;
using Microsoft.Scripting;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Microsoft.Scripting.Runtime;

    public enum ParameterKind {
        Normal,
        List,
        Dictionary,
    };

    /// <summary>
    /// Parameter base class
    /// </summary>
    public class Parameter : Node {
        /// <summary>
        /// Position of the parameter: 0-based index
        /// </summary>
        private readonly SymbolId _name;
        protected readonly ParameterKind _kind;
        protected Expression _defaultValue;

        private PythonVariable _variable;

        public Parameter(SymbolId name)
            : this(name, ParameterKind.Normal) {
        }

        public Parameter(SymbolId name, ParameterKind kind) {
            _name = name;
            _kind = kind;
        }

        /// <summary>
        /// Parameter name
        /// </summary>
        public SymbolId Name {
            get { return _name; }
        }

        public Expression DefaultValue {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public bool IsList {
            get {
                return _kind == ParameterKind.List;
            }
        }

        public bool IsDictionary {
            get {
                return _kind == ParameterKind.Dictionary;
            }
        }

        internal PythonVariable Variable {
            get { return _variable; }
            set { _variable = value; }
        }

        internal MSAst.Expression Transform(AstGenerator inner) {
            MSAst.ParameterExpression parameter;
            string name = SymbolTable.IdToString(Name);
            if (_variable.AccessedInNestedScope) {
                parameter = inner.Block.ClosedOverParameter(typeof(object), name);
            } else {
                parameter = inner.Block.Parameter(typeof(object), name);
            }
            _variable.SetParameter(parameter);
            return parameter;
        }

        internal virtual void Init(AstGenerator inner, List<MSAst.Expression> init) {
            // Regular parameter has no initialization
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_defaultValue != null) {
                    _defaultValue.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }

    public class SublistParameter : Parameter {
        private readonly TupleExpression _tuple;

        public SublistParameter(int position, TupleExpression tuple)
            : base(SymbolTable.StringToId("." + position * 2), ParameterKind.Normal) {
            _tuple = tuple;
        }

        public TupleExpression Tuple {
            get { return _tuple; }
        }

        internal override void Init(AstGenerator inner, List<MSAst.Expression> init) {
            MSAst.Expression stmt = _tuple.TransformSet(
                inner,
                Span,
                Variable.Variable,
                Operators.None
            );

            if (stmt != null) {
                init.Add(stmt);
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_tuple != null) {
                    _tuple.Walk(walker);
                }
                if (_defaultValue != null) {
                    _defaultValue.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }
    }
}
