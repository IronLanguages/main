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
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif


namespace IronPython.Compiler.Ast {

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
        private readonly string _name;
        protected readonly ParameterKind _kind;
        protected Expression _defaultValue;

        private PythonVariable _variable;

        public Parameter(string name)
            : this(name, ParameterKind.Normal) {
        }

        public Parameter(string name, ParameterKind kind) {
            _name = name;
            _kind = kind;
        }

        /// <summary>
        /// Parameter name
        /// </summary>
        public string Name {
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

        internal MSAst.Expression Transform(AstGenerator inner, bool needsWrapperMethod, bool needsLocalsDictionary, List<MSAst.Expression> init) {
            string name = Name;
            if (_variable.AccessedInNestedScope || needsLocalsDictionary) {
                ClosureExpression closureVar;
                if (needsWrapperMethod) {
                    closureVar = inner.LiftedVariable(Variable, name, _variable.AccessedInNestedScope);
                } else {
                    closureVar = inner.LiftedParameter(Variable, name);
                }
                inner.SetLocalLiftedVariable(_variable, closureVar);
                init.Add(closureVar.Create());

                return closureVar;                
            } else {
                MSAst.Expression parameter;
                if (needsWrapperMethod) {
                    parameter = inner.Variable(typeof(object), name);
                } else {
                    parameter = inner.Parameter(typeof(object), name);
                }
                inner.Globals.SetParameter(_variable, parameter);
                return parameter;
            }
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
            : base("." + position, ParameterKind.Normal) {
            _tuple = tuple;
        }

        public TupleExpression Tuple {
            get { return _tuple; }
        }

        internal override void Init(AstGenerator inner, List<MSAst.Expression> init) {
            init.Add(
                _tuple.TransformSet(
                    inner,
                    Span,
                    inner.Globals.GetVariable(inner, Variable),
                    PythonOperationKind.None
                )
            );
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
