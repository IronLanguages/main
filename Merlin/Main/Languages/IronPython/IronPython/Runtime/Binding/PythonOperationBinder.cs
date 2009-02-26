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

    class PythonOperationBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;
        private readonly PythonOperationKind _operation;

        public PythonOperationBinder(BinderState/*!*/ state, PythonOperationKind/*!*/ operation) {
            _state = state;
            _operation = operation;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
            IPythonOperable op = target as IPythonOperable;
            if (op != null) {
                DynamicMetaObject res = op.BindOperation(this, ArrayUtils.Insert(target, args));
                if (res != null) {
                    return res;
                }
            }

            return PythonProtocol.Operation(this, ArrayUtils.Insert(target, args));
        }

        public PythonOperationKind Operation {
            get {
                return _operation;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode() ^ _operation.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonOperationBinder ob = obj as PythonOperationBinder;
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
            return "Python " + Operation;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeOperationAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant((int)Operation)
            );
        }

        #endregion

        #region GetIndex/SetIndex adapters

        // TODO: remove when Python uses the SetIndexBinder for real
        class SetIndexAdapter : SetIndexBinder {
            private readonly PythonOperationBinder _opBinder;

            internal SetIndexAdapter(PythonOperationBinder opBinder)
                : base(new CallInfo(0)) {
                _opBinder = opBinder;
            }

            public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject com;
                if (System.Dynamic.ComBinder.TryBindSetIndex(this, target, indexes, value, out com)) {
                    return com;
                }
#endif
                return PythonProtocol.Operation(_opBinder, ArrayUtils.Append(ArrayUtils.Insert(target, indexes), value));
            }

            public override int GetHashCode() {
                return _opBinder.GetHashCode();
            }

            public override bool Equals(object obj) {
                return obj != null && obj.Equals(_opBinder);
            }
        }

        // TODO: remove when Python uses the GetIndexBinder for real
        class GetIndexAdapter : GetIndexBinder {
            private readonly PythonOperationBinder _opBinder;

            internal GetIndexAdapter(PythonOperationBinder opBinder)
                : base(new CallInfo(0)) {
                _opBinder = opBinder;
            }

            public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
                DynamicMetaObject com;
                if (System.Dynamic.ComBinder.TryBindGetIndex(this, target, indexes, out com)) {
                    return com;
                }
#endif
                return PythonProtocol.Operation(_opBinder, ArrayUtils.Insert(target, indexes));
            }

            public override int GetHashCode() {
                return _opBinder.GetHashCode();
            }

            public override bool Equals(object obj) {
                return obj != null && obj.Equals(_opBinder);
            }
        }

        #endregion
    }
}
