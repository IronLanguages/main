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

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class PythonGetIndexBinder : GetIndexBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonGetIndexBinder(BinderState/*!*/ state, int argCount)
            : base(Expression.CallInfo(argCount)) {
            _state = state;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (System.Dynamic.ComBinder.TryBindGetIndex(this, target, indexes, out com)) {
                return com;
            }
#endif
            return PythonProtocol.Index(this, PythonIndexType.GetItem, ArrayUtils.Insert(target, indexes));
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonGetIndexBinder ob = obj as PythonGetIndexBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        #region IPythonSite Members

        public BinderState/*!*/ Binder {
            get { return _state; }
        }

        #endregion

        #region IExpressionSerializable Members

        public Expression/*!*/ CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeGetIndexAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant(CallInfo.ArgumentCount)
            );
        }

        #endregion
    }
}
