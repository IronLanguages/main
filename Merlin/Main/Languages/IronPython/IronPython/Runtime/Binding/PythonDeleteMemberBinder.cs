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
using Microsoft.Scripting;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;

    class PythonDeleteMemberBinder : DeleteMemberBinder, IPythonSite, IExpressionSerializable {
        private readonly PythonContext/*!*/ _context;

        public PythonDeleteMemberBinder(PythonContext/*!*/ context, string/*!*/ name)
            : base(name, false) {
            _context = context;
        }

        public PythonDeleteMemberBinder(PythonContext/*!*/ context, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _context = context;
        }

        public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject self, DynamicMetaObject errorSuggestion) {
            if (self.NeedsDeferral()) {
                return Defer(self);
            }

            return Context.Binder.DeleteMember(Name, self, AstUtils.Constant(Context.SharedContext));
        }

        public PythonContext/*!*/ Context {
            get {
                return _context;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _context.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            PythonDeleteMemberBinder ob = obj as PythonDeleteMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._context.Binder == _context.Binder && base.Equals(obj);
        }

        public override string ToString() {
            return "Python DeleteMember " + Name;
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeDeleteAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Name)
            );
        }

        #endregion
    }
}

