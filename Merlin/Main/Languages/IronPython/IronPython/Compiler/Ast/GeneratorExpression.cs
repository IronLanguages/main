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
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting.Actions;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {

    public class GeneratorExpression : Expression {
        private readonly FunctionDefinition _function;
        private readonly Expression _iterable;

        public GeneratorExpression(FunctionDefinition function, Expression iterable) {
            _function = function;
            _iterable = iterable;
        }

        internal override MSAst.Expression Transform(AstGenerator ag, Type type) {
            MSAst.Expression func = _function.TransformToFunctionExpression(ag);

            Debug.Assert(func.Type == typeof(PythonFunction));
            // Generator expressions always return functions.  We could do even better here when all PythonFunction's are in the same class.

            return Binders.Invoke(
                ag.BinderState,
                typeof(object),
                new CallSignature(1),
                func,
                ag.TransformAsObject(_iterable)
            );
        }

        public FunctionDefinition Function {
            get {
                return _function;
            }
        }

        public Expression Iterable {
            get {
                return _iterable;
            }
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                _function.Walk(walker);
                _iterable.Walk(walker);
            }
            walker.PostWalk(this);
        }
    }
}
