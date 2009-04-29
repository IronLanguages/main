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
using System.Runtime.CompilerServices;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

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

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            switch (Operation) {
                case ExpressionType.Negate:
                    if (CompilerHelpers.GetType(args[0]) == typeof(int)) {
                        if (typeof(T) == typeof(Func<CallSite, object, object>)) {
                            return (T)(object)new Func<CallSite, object, object>(IntNegate);
                        }
                    }
                    break;
                case ExpressionType.IsFalse:
                    if (CompilerHelpers.GetType(args[0]) == typeof(string)) {
                        if (typeof(T) == typeof(Func<CallSite, object, bool>)) {
                            return (T)(object)new Func<CallSite, object, bool>(StringIsFalse);
                        }
                    } else if (CompilerHelpers.GetType(args[0]) == typeof(bool)) {
                        if (typeof(T) == typeof(Func<CallSite, object, bool>)) {
                            return (T)(object)new Func<CallSite, object, bool>(BoolIsFalse);
                        }
                    } else if (CompilerHelpers.GetType(args[0]) == typeof(List)) {
                        if (typeof(T) == typeof(Func<CallSite, object, bool>)) {
                            return (T)(object)new Func<CallSite, object, bool>(ListIsFalse);
                        }
                    } else if (CompilerHelpers.GetType(args[0]) == typeof(PythonTuple)) {
                        if (typeof(T) == typeof(Func<CallSite, object, bool>)) {
                            return (T)(object)new Func<CallSite, object, bool>(TupleIsFalse);
                        }
                    } else if (args[0] == null) {
                        if (typeof(T) == typeof(Func<CallSite, object, bool>)) {
                            return (T)(object)new Func<CallSite, object, bool>(NoneIsFalse);
                        }
                    } 
                    break;
            }

            return base.BindDelegate(site, args);
        }

        private object IntNegate(CallSite site, object value) {
            if (value is int) {
                return Int32Ops.Negate((int)value);
            }

            return ((CallSite<Func<CallSite, object, object>>)site).Update(site, value);
        }

        private bool StringIsFalse(CallSite site, object value) {
            string strVal = value as string;
            if (strVal != null) {
                return strVal.Length == 0;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
        }

        private bool ListIsFalse(CallSite site, object value) {
            if (value != null && value.GetType() == typeof(List)) {
                return ((List)value).Count == 0;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
        }

        private bool NoneIsFalse(CallSite site, object value) {
            if (value == null) {
                return true;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
        }

        private bool TupleIsFalse(CallSite site, object value) {
            if (value != null && value.GetType() == typeof(PythonTuple)) {
                return ((PythonTuple)value).Count == 0;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
        }

        private bool BoolIsFalse(CallSite site, object value) {
            if (value is bool) {
                return !(bool)value;
            }

            return ((CallSite<Func<CallSite, object, bool>>)site).Update(site, value);
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
                AstUtils.Constant(Operation)
            );
        }

        #endregion
    }
}
