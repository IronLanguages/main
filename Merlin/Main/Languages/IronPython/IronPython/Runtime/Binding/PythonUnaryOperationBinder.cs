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
using System.Linq.Expressions;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class PythonUnaryOperationBinder : UnaryOperationBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonUnaryOperationBinder(BinderState/*!*/ state, ExpressionType operation)
            : base(operation) {
            _state = state;
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            return PythonProtocol.Operation(this, target);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonUnaryOperationBinder ob = obj as PythonUnaryOperationBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override string ToString() {
            return "PythonUnary " + Operation;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeUnaryOperationAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant(Operation)
            );
        }

        #endregion
    }
}
