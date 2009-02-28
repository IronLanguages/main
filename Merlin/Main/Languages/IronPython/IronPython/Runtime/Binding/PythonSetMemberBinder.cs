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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class PythonSetMemberBinder : SetMemberBinder, IPythonSite, IExpressionSerializable {
        private readonly BinderState/*!*/ _state;

        public PythonSetMemberBinder(BinderState/*!*/ binder, string/*!*/ name)
            : base(name, false) {
            _state = binder;
        }

        public PythonSetMemberBinder(BinderState/*!*/ binder, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _state = binder;
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject self, DynamicMetaObject value, DynamicMetaObject onBindingError) {
            if (self.NeedsDeferral()) {
                return Defer(self, value);
            }
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (System.Dynamic.ComBinder.TryBindSetMember(this, self, value, out com)) {
                return com;
            }
#endif
            return Binder.Binder.SetMember(Name, self, value, AstUtils.Constant(Binder.Context));
        }

        public BinderState/*!*/ Binder {
            get {
                return _state;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonSetMemberBinder ob = obj as PythonSetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder && base.Equals(obj);
        }

        public override string ToString() {
            return "Python SetMember " + Name;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeSetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Name)
            );
        }

        #endregion
    }
}

