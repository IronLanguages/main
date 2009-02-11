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

using IronPython.Runtime.Operations;


namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class PythonSetIndexBinder : SetIndexBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonSetIndexBinder(BinderState/*!*/ state, int argCount)
            : base(BindingHelpers.GetSimpleArgumentInfos(argCount)) {
            _state = state;
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (System.Dynamic.ComBinder.TryBindSetIndex(this, target, indexes, value, out com)) {
                return com;
            }
#endif
            
            DynamicMetaObject[] finalArgs = new DynamicMetaObject[indexes.Length + 2];
            finalArgs[0] = target;
            for (int i = 0; i < indexes.Length; i++) {
                finalArgs[i + 1] = indexes[i];
            }
            finalArgs[finalArgs.Length - 1] = value;

            return PythonProtocol.Index(this, PythonIndexType.SetItem, finalArgs);
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonSetIndexBinder ob = obj as PythonSetIndexBinder;
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
                typeof(PythonOps).GetMethod("MakeSetIndexAction"),
                BindingHelpers.CreateBinderStateExpression(),
                Expression.Constant(Arguments.Count)
            );
        }

        #endregion
    }
}
