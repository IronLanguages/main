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
using System.Linq.Expressions;
using System.Dynamic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;

namespace IronPython.Runtime.Binding {

    class MetaBuiltinMethodDescriptor : MetaPythonObject, IPythonInvokable {
        public MetaBuiltinMethodDescriptor(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, BuiltinMethodDescriptor/*!*/ value)
            : base(expression, BindingRestrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public DynamicMetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ call, params DynamicMetaObject/*!*/[]/*!*/ args) {
            // TODO: Context should come from BuiltinFunction
            return InvokeWorker(call, BinderState.GetCodeContext(call), args);
        }

        [Obsolete]
        public override DynamicMetaObject BindOperation(OperationBinder action, DynamicMetaObject[] args) {
            switch (action.Operation) {
                case StandardOperators.CallSignatures:
                    return PythonProtocol.MakeCallSignatureOperation(this, Value.Template.Targets);
            }

            return base.BindOperation(action, args);
        }

        #endregion

        #region Invoke Implementation

        private DynamicMetaObject/*!*/ InvokeWorker(DynamicMetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[] args) {
            CallSignature signature = BindingHelpers.GetCallSignature(call);
            BindingRestrictions selfRestrict = BindingRestrictions.GetInstanceRestriction(Expression, Value).Merge(Restrictions);

            selfRestrict = selfRestrict.Merge(
                BindingRestrictions.GetExpressionRestriction(
                    MakeFunctionTest(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("GetBuiltinMethodDescriptorTemplate"),
                            Ast.Convert(Expression, typeof(BuiltinMethodDescriptor))
                        )
                    )
                )
            );

            return Value.Template.MakeBuiltinFunctionCall(
                call,
                codeContext,
                this,
                args,
                false,  // no self
                true,
                selfRestrict,
                (newArgs) => {
                    BindingTarget target;

                    BinderState state = BinderState.GetBinderState(call);

                    DynamicMetaObject res = state.Binder.CallMethod(
                        new ParameterBinderWithCodeContext(state.Binder, codeContext),
                        Value.Template.Targets,
                        newArgs,
                        signature,
                        selfRestrict,
                        NarrowingLevel.None,
                        Value.Template.IsBinaryOperator ?
                            PythonNarrowing.BinaryOperator :
                            NarrowingLevel.All,
                        Value.Template.Name,
                        out target
                    );

                    return new BuiltinFunction.BindingResult(target, res);
                });            
        }

        internal Expression MakeFunctionTest(Expression functionTarget) {
            return Ast.Equal(
                functionTarget,
                Ast.Constant(Value.Template)
            );
        }

        #endregion

        #region Helpers

        public new BuiltinMethodDescriptor/*!*/ Value {
            get {
                return (BuiltinMethodDescriptor)base.Value;
            }
        }

        #endregion
    }
}
