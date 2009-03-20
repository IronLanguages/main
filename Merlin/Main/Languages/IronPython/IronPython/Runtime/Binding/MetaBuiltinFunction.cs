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
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    class MetaBuiltinFunction : MetaPythonObject, IPythonInvokable, IPythonOperable {
        public MetaBuiltinFunction(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, BuiltinFunction/*!*/ value)
            : base(expression, BindingRestrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ call, params DynamicMetaObject/*!*/[]/*!*/ args) {
            // TODO: Context should come from BuiltinFunction
            return InvokeWorker(call, BinderState.GetCodeContext(call), args);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder/*!*/ conversion) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "BuiltinFunc Convert " + conversion.Type);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "BuiltinFunc Convert");            

            if (conversion.Type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(conversion, conversion.Type, Restrict(typeof(BuiltinFunction)));
            }
            return conversion.FallbackConvert(this);
        }

        DynamicMetaObject IPythonOperable.BindOperation(PythonOperationBinder action, DynamicMetaObject[] args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "BuiltinFunc Operation " + action.Operation);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "BuiltinFunc Operation");
            switch (action.Operation) {
                case PythonOperationKind.CallSignatures:
                    return PythonProtocol.MakeCallSignatureOperation(this, Value.Targets);
            }

            return null;
        }

        #endregion

        #region IPythonInvokable Members

        public DynamicMetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, codeContext, args);
        }

        #endregion

        #region Invoke Implementation

        private DynamicMetaObject/*!*/ InvokeWorker(DynamicMetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[]/*!*/ args) {
            PerfTrack.NoteEvent(PerfTrack.Categories.Binding, "BuiltinFunc Invoke " + Value.DeclaringType.FullName + "." + Value.Name + " with " + args.Length + " args " + Value.IsUnbound);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "BuiltinFunction " + Value.Targets.Count + ", " + Value.Targets[0].GetParameters().Length);
            PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "BuiltinFunction " + BindingHelpers.GetCallSignature(call));

            if (this.NeedsDeferral()) {
                return call.Defer(ArrayUtils.Insert(this, args));
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].NeedsDeferral()) {
                    return call.Defer(ArrayUtils.Insert(this, args));
                }
            }

            if (Value.IsUnbound) {
                return MakeSelflessCall(call, codeContext, args);
            } else {
                return MakeSelfCall(call, codeContext, args);
            }
        }

        private DynamicMetaObject/*!*/ MakeSelflessCall(DynamicMetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[]/*!*/ args) {
            // just check if it's the same built-in function.  Because built-in functions are
            // immutable the identity check will suffice.  Because built-in functions are uncollectible
            // anyway we don't use the typical InstanceRestriction.
            BindingRestrictions selfRestrict = BindingRestrictions.GetExpressionRestriction(Ast.Equal(Expression, AstUtils.Constant(Value))).Merge(Restrictions);

            return Value.MakeBuiltinFunctionCall(
                call,
                codeContext,
                this,
                args,
                false,  // no self
                selfRestrict,
                (newArgs) => {
                    BindingTarget target;
                    var binder = BinderState.GetBinderState(call).Binder;
                    DynamicMetaObject res = binder.CallMethod(
                        new ParameterBinderWithCodeContext(binder, codeContext),
                        Value.Targets,
                        newArgs,
                        BindingHelpers.GetCallSignature(call),
                        selfRestrict,
                        PythonNarrowing.None,
                        Value.IsBinaryOperator ?
                                PythonNarrowing.BinaryOperator :
                                NarrowingLevel.All,
                        Value.Name,
                        out target
                    );

                    return new BuiltinFunction.BindingResult(target, res);
                }
            );
        }

        private DynamicMetaObject/*!*/ MakeSelfCall(DynamicMetaObjectBinder/*!*/ call, Expression/*!*/ codeContext, DynamicMetaObject/*!*/[]/*!*/ args) {
            BindingRestrictions selfRestrict = Restrictions.Merge(
                BindingRestrictionsHelpers.GetRuntimeTypeRestriction(
                    Expression,
                    typeof(BuiltinFunction)
                )
            ).Merge(
                BindingRestrictions.GetExpressionRestriction(
                    Value.MakeBoundFunctionTest(
                        AstUtils.Convert(Expression, typeof(BuiltinFunction))
                    )
                )
            );

            Expression instance = Ast.Property(
                AstUtils.Convert(
                    Expression,
                    typeof(BuiltinFunction)
                ),
                typeof(BuiltinFunction).GetProperty("__self__")
            );

              DynamicMetaObject self = GetInstance(instance, CompilerHelpers.GetType(Value.__self__));
                return Value.MakeBuiltinFunctionCall(
                call,
                codeContext,
                this,
                ArrayUtils.Insert(self, args),
                true,   // has self
                selfRestrict,
                (newArgs) => {
                    CallSignature signature = BindingHelpers.GetCallSignature(call);
                    DynamicMetaObject res;
                    BinderState state = BinderState.GetBinderState(call);
                    BindingTarget target;
                    var mc = new ParameterBinderWithCodeContext(state.Binder, codeContext);
                    if (Value.IsReversedOperator) {
                        res = state.Binder.CallMethod(
                            mc,
                            Value.Targets,
                            newArgs,
                            GetReversedSignature(signature),
                            self.Restrictions,
                            NarrowingLevel.None,
                            Value.IsBinaryOperator ?
                                PythonNarrowing.BinaryOperator :
                                NarrowingLevel.All,
                            Value.Name,
                            out target
                        );
                    } else {
                        res = state.Binder.CallInstanceMethod(
                            mc,
                            Value.Targets,
                            self,
                            args,
                            signature,
                            self.Restrictions,
                            NarrowingLevel.None,
                            Value.IsBinaryOperator ?
                                PythonNarrowing.BinaryOperator :
                                NarrowingLevel.All,
                            Value.Name,
                            out target
                        );
                    }

                    return new BuiltinFunction.BindingResult(target, res);
                }
            );
        }

        private DynamicMetaObject/*!*/ GetInstance(Expression/*!*/ instance, Type/*!*/ testType) {
            Assert.NotNull(instance, testType);
            object instanceValue = Value.__self__;

            BindingRestrictions restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(instance, testType);
            // cast the instance to the correct type
            if (CompilerHelpers.IsStrongBox(instanceValue)) {
                instance = ReadStrongBoxValue(instance);
                instanceValue = ((IStrongBox)instanceValue).Value;
            } else if (!testType.IsEnum) {
                // We need to deal w/ wierd types like MarshalByRefObject.  
                // We could have an MBRO whos DeclaringType is completely different.  
                // Therefore we special case it here and cast to the declaring type

                Type selfType = CompilerHelpers.GetType(Value.__self__);
                selfType = CompilerHelpers.GetVisibleType(selfType);

                if (selfType == typeof(object) && Value.DeclaringType.IsInterface) {
                    selfType = Value.DeclaringType;
                }

                if (Value.DeclaringType.IsInterface && selfType.IsValueType) {
                    // explicit interface implementation dispatch on a value type, don't
                    // unbox the value type before the dispatch.
                    instance = AstUtils.Convert(instance, Value.DeclaringType);
                } else if (selfType.IsValueType) {
                    // We might be calling a a mutating method (like
                    // Rectangle.Intersect). If so, we want it to mutate
                    // the boxed value directly
                    instance = Ast.Unbox(instance, selfType);
                } else {
#if SILVERLIGHT
                    instance = AstUtils.Convert(instance, selfType);
#else
                    Type convType = selfType == typeof(MarshalByRefObject) ? CompilerHelpers.GetVisibleType(Value.DeclaringType) : selfType;

                    instance = AstUtils.Convert(instance, convType);
#endif
                }
            } else {
                // we don't want to cast the enum to its real type, it will unbox it 
                // and turn it into its underlying type.  We presumably want to call 
                // a method on the Enum class though - so we cast to Enum instead.
                instance = AstUtils.Convert(instance, typeof(Enum));
            }
            return new DynamicMetaObject(
                instance,
                restrictions,
                instanceValue
            );
        }

        private MemberExpression/*!*/ ReadStrongBoxValue(Expression instance) {
            return Ast.Field(
                AstUtils.Convert(instance, Value.__self__.GetType()),
                Value.__self__.GetType().GetField("Value")
            );
        }

        internal static CallSignature GetReversedSignature(CallSignature signature) {
            return new CallSignature(ArrayUtils.Append(signature.GetArgumentInfos(), new Argument(ArgumentType.Simple)));
        }

        #endregion

        #region Helpers

        public new BuiltinFunction/*!*/ Value {
            get {
                return (BuiltinFunction)base.Value;
            }
        }

        #endregion
    }
}
