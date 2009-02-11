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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;

using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    class MetaMethod : MetaPythonObject, IPythonInvokable {
        public MetaMethod(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, Method/*!*/ value)
            : base(expression, BindingRestrictions.Empty, value) {
            Assert.NotNull(value);
        }

        #region IPythonInvokable Members

        public DynamicMetaObject/*!*/ Invoke(PythonInvokeBinder/*!*/ pythonInvoke, Expression/*!*/ codeContext, DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(pythonInvoke, args);
        }

        #endregion

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindInvokeMember(InvokeMemberBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ args) {
            return BindingHelpers.GenericCall(action, this, args);
        }

        public override DynamicMetaObject/*!*/ BindInvoke(InvokeBinder/*!*/ callAction, params DynamicMetaObject/*!*/[]/*!*/ args) {
            return InvokeWorker(callAction, args);
        }

        public override DynamicMetaObject BindConvert(ConvertBinder action) {
            if (action.Type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateTarget(action, action.Type, Restrict(typeof(Method)));
            }

            return base.BindConvert(action);
        }

        #endregion

        #region Invoke Implementation

        private DynamicMetaObject InvokeWorker(DynamicMetaObjectBinder/*!*/ callAction, DynamicMetaObject/*!*/[] args) {
            CallSignature signature = BindingHelpers.GetCallSignature(callAction);
            DynamicMetaObject self = Restrict(typeof(Method));
            BindingRestrictions restrictions = self.Restrictions;

            DynamicMetaObject func = GetMetaFunction(self);
            DynamicMetaObject call;

            if (Value.im_self == null) {
                // restrict to null self (Method is immutable so this is an invariant test)
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetExpressionRestriction(
                        Ast.Equal(
                            GetSelfExpression(self),
                            Ast.Constant(null)
                        )
                    )
                );

                if (args.Length == 0) {
                    // this is an error, we pass null which will throw the normal error
                    call = new DynamicMetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("MethodCheckSelf"),
                            self.Expression,
                            Ast.Constant(null)
                        ),
                        restrictions
                    );
                } else {
                    // this may or may not be an error
                    call = new DynamicMetaObject(
                        Ast.Block(
                            MakeCheckSelf(signature, args),
                            Ast.Dynamic(
                                new PythonInvokeBinder(
                                    BinderState.GetBinderState(callAction),
                                    BindingHelpers.GetCallSignature(callAction)
                                ),
                                typeof(object),
                                ArrayUtils.Insert(BinderState.GetCodeContext(callAction), DynamicUtils.GetExpressions(ArrayUtils.Insert(func, args)))
                            )
                        ),
                        BindingRestrictions.Empty
                    );
                    /*call = func.Invoke(callAction, ArrayUtils.Insert(func, args));
                    call =  new MetaObject(
                        Ast.Comma(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MethodCheckSelf"),
                                self.Expression,
                                args[0].Expression
                            ),
                            call.Expression
                        ),
                        call.Restrictions                        
                    );*/
                }
            } else {
                // restrict to non-null self (Method is immutable so this is an invariant test)
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetExpressionRestriction(
                        Ast.NotEqual(
                            GetSelfExpression(self),
                            Ast.Constant(null)
                        )
                    )
                );

                DynamicMetaObject im_self = GetMetaSelf(self);
                DynamicMetaObject[] newArgs = ArrayUtils.Insert(func, im_self, args);
                CallSignature newSig = new CallSignature(ArrayUtils.Insert(new Argument(ArgumentType.Simple), signature.GetArgumentInfos()));


                call = new DynamicMetaObject(
                    Ast.Dynamic(
                        new PythonInvokeBinder(
                            BinderState.GetBinderState(callAction),
                            newSig
                        ),
                        typeof(object),
                        ArrayUtils.Insert(BinderState.GetCodeContext(callAction), DynamicUtils.GetExpressions(newArgs))
                    ),
                    BindingRestrictions.Empty
                );

                /*
                call = func.Invoke(
                    new CallBinder(
                        BinderState.GetBinderState(callAction),
                        newSig
                    ),
                    newArgs
                );*/
            }

            if (call.HasValue) {
                return new DynamicMetaObject(
                    call.Expression,
                    restrictions.Merge(call.Restrictions),
                    call.Value
                );
            } else {
                return new DynamicMetaObject(
                    call.Expression,
                    restrictions.Merge(call.Restrictions)
                );
            }
        }

        #endregion

        #region Helpers

        private DynamicMetaObject GetMetaSelf(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject func;

            IDynamicObject ido = Value.im_self as IDynamicObject;
            if (ido != null) {
                func = ido.GetMetaObject(GetSelfExpression(self));
            } else if (Value.im_self == null) {
                func = new DynamicMetaObject(
                    GetSelfExpression(self),
                    BindingRestrictions.Empty);
            } else {
                func = new DynamicMetaObject(
                    GetSelfExpression(self),
                    BindingRestrictions.Empty,
                    Value.im_self
                );
            }

            return func;
        }
        
        private DynamicMetaObject/*!*/ GetMetaFunction(DynamicMetaObject/*!*/ self) {
            DynamicMetaObject func;
            IDynamicObject ido = Value.im_func as IDynamicObject;
            if (ido != null) {
                func = ido.GetMetaObject(GetFunctionExpression(self));
            } else {
                func = new DynamicMetaObject(
                    GetFunctionExpression(self),
                    BindingRestrictions.Empty
                );
            }
            return func;
        }

        private static MemberExpression GetFunctionExpression(DynamicMetaObject self) {
            return Ast.Property(
                self.Expression,
                typeof(Method).GetProperty("im_func")
            );
        }

        private static MemberExpression GetSelfExpression(DynamicMetaObject self) {
            return Ast.Property(
                self.Expression,
                typeof(Method).GetProperty("im_self")
            );
        }

        public new Method/*!*/ Value {
            get {
                return (Method)base.Value;
            }
        }

        private Expression/*!*/ MakeCheckSelf(CallSignature signature, DynamicMetaObject/*!*/[]/*!*/ args) {
            ArgumentType firstArgKind = signature.GetArgumentKind(0);

            Expression res;
            if (firstArgKind == ArgumentType.Simple || firstArgKind == ArgumentType.Instance) {
                res = CheckSelf(AstUtils.Convert(Expression, typeof(Method)), args[0].Expression);
            } else if (firstArgKind != ArgumentType.List) {
                res = CheckSelf(AstUtils.Convert(Expression, typeof(Method)), Ast.Constant(null));
            } else {
                // list, check arg[0] and then return original list.  If not a list,
                // or we have no items, then check against null & throw.
                res = CheckSelf(
                    AstUtils.Convert(Expression, typeof(Method)),
                    Ast.Condition(
                        Ast.AndAlso(
                            Ast.TypeIs(args[0].Expression, typeof(IList<object>)),
                            Ast.NotEqual(
                                Ast.Property(
                                    Ast.Convert(args[0].Expression, typeof(ICollection)),
                                    typeof(ICollection).GetProperty("Count")
                                ),
                                Ast.Constant(0)
                            )
                        ),
                        Ast.Call(
                            Ast.Convert(args[0].Expression, typeof(IList<object>)),
                            typeof(IList<object>).GetMethod("get_Item"),
                            Ast.Constant(0)
                        ),
                        Ast.Constant(null)
                    )
                );
            }

            return res;
        }

        private Expression/*!*/ CheckSelf(Expression/*!*/ method, Expression/*!*/ inst) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MethodCheckSelf"),
                method,
                AstUtils.Convert(inst, typeof(object))
            );
        }

        #endregion
    }
}
