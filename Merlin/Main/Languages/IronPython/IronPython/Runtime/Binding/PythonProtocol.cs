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
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Provides binding logic which is implemented to follow various Python protocols.  This includes
    /// things such as calling __call__ to perform calls, calling __nonzero__/__len__ to convert to
    /// bool, calling __add__/__radd__ to do addition, etc...  
    /// 
    /// This logic gets shared between both the IDynamicMetaObjectProvider implementation for Python objects as well
    /// as the Python sites.  This ensures the logic we follow for our builtin types and user defined
    /// types is identical and properly conforming to the various protocols.
    /// </summary>
    static partial class PythonProtocol {

        #region Conversions

        /// <summary>
        /// Gets a MetaObject which converts the provided object to a bool using __nonzero__ or __len__
        /// protocol methods.  This code is shared between both our fallback for a site and our MetaObject
        /// for user defined objects.
        /// </summary>
        internal static DynamicMetaObject ConvertToBool(ConvertBinder/*!*/ conversion, DynamicMetaObject/*!*/ self) {
            Assert.NotNull(conversion, self);

            SlotOrFunction sf = SlotOrFunction.GetSlotOrFunction(
                BinderState.GetBinderState(conversion),
                Symbols.NonZero,
                self);

            if (sf.Success) {
                if (sf.Target.Expression.Type != typeof(bool)) {
                    return new DynamicMetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("ThrowingConvertToNonZero"),
                            sf.Target.Expression
                        ),
                        sf.Target.Restrictions
                    );
                }

                return sf.Target;
            }

            sf = SlotOrFunction.GetSlotOrFunction(
                BinderState.GetBinderState(conversion),
                Symbols.Length,
                self);

            if (sf.Success) {
                return new DynamicMetaObject(
                    GetConvertByLengthBody(
                        BinderState.GetBinderState(conversion),
                        sf.Target.Expression
                    ),
                    sf.Target.Restrictions
                );
            }

            return null;
        }

        /// <summary>
        /// Used for conversions to bool
        /// </summary>
        private static Expression/*!*/ GetConvertByLengthBody(BinderState/*!*/ state, Expression/*!*/ call) {
            Assert.NotNull(state, call);

            Expression callAsInt = call;
            if (call.Type != typeof(int)) {
                callAsInt = Ast.Dynamic(
                    state.Convert(typeof(int), ConversionResultKind.ExplicitCast),
                    typeof(int),
                    call
                );
            }

            return Ast.NotEqual(callAsInt, Ast.Constant(0));
        }

        internal static DynamicMetaObject ConvertToIEnumerable(ConvertBinder/*!*/ conversion, DynamicMetaObject/*!*/ metaUserObject) {
            PythonType pt = MetaPythonObject.GetPythonType(metaUserObject);
            CodeContext context = BinderState.GetBinderState(conversion).Context;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {
                return MakeIterRule(metaUserObject, "CreatePythonEnumerable");
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {
                return MakeIterRule(metaUserObject, "CreateItemEnumerable");
            }

            return null;
        }

        internal static DynamicMetaObject ConvertToIEnumerator(ConvertBinder/*!*/ conversion, DynamicMetaObject/*!*/ metaUserObject) {
            PythonType pt = MetaPythonObject.GetPythonType(metaUserObject);
            CodeContext context = BinderState.GetBinderState(conversion).Context;
            PythonTypeSlot pts;

            if (pt.TryResolveSlot(context, Symbols.Iterator, out pts)) {
                return MakeIterRule(metaUserObject, "CreatePythonEnumerator");
            } else if (pt.TryResolveSlot(context, Symbols.GetItem, out pts)) {
                return MakeIterRule(metaUserObject, "CreateItemEnumerator");
            }

            return null;
        }

        private static DynamicMetaObject/*!*/ MakeIterRule(DynamicMetaObject/*!*/ self, string methodName) {            
            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod(methodName),
                    AstUtils.Convert(self.Expression, typeof(object))
                ),
                self.Restrictions
            );
        }

        #endregion

        #region Calls

        internal static DynamicMetaObject Call(DynamicMetaObjectBinder/*!*/ call, DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ args) {
            Assert.NotNull(call, args);
            Assert.NotNullItems(args);

            if (target.NeedsDeferral()) {
                return call.Defer(ArrayUtils.Insert(target, args));
            }

            foreach (DynamicMetaObject mo in args) {
                if (mo.NeedsDeferral()) {
                    RestrictTypes(args);

                    return call.Defer(
                        ArrayUtils.Insert(target, args)
                    );
                }
            }

            DynamicMetaObject self = target.Restrict(target.GetLimitType());

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(null, target);
            PythonType pt = DynamicHelpers.GetPythonType(target.Value);
            Expression body = GetCallError(self);
            BinderState state = BinderState.GetBinderState(call);

            // look for __call__, if it's present dispatch to it.  Otherwise fall back to the
            // default binder
            PythonTypeSlot callSlot;
            if (!typeof(Delegate).IsAssignableFrom(target.GetLimitType()) &&
                pt.TryResolveSlot(state.Context, Symbols.Call, out callSlot)) {
                Expression[] callArgs = ArrayUtils.Insert(
                    BinderState.GetCodeContext(call),
                    callSlot.MakeGetExpression(
                        state.Binder,
                        BinderState.GetCodeContext(call),
                        self.Expression,
                        GetPythonType(self),
                        AstUtils.Convert(body, typeof(object))
                    ), 
                    DynamicUtils.GetExpressions(args)
                );

                body = Ast.Dynamic(
                    BinderState.GetBinderState(call).Invoke(
                        BindingHelpers.GetCallSignature(call)
                    ),
                    typeof(object),
                    callArgs
                );

                return BindingHelpers.AddDynamicTestAndDefer(
                    call,
                    new DynamicMetaObject(body, self.Restrictions.Merge(BindingRestrictions.Combine(args))),
                    args,
                    valInfo
                );
            }

            return null;
        }

        private static Expression/*!*/ GetPythonType(DynamicMetaObject/*!*/ self) {
            Assert.NotNull(self);

            PythonType pt = DynamicHelpers.GetPythonType(self.Value);
            if (pt.IsSystemType) {
                return Ast.Constant(pt);
            }

            return Ast.Property(
                Ast.Convert(self.Expression, typeof(IPythonObject)),
                TypeInfo._IPythonObject.PythonType
            );
        }

        private static Expression/*!*/ GetCallError(DynamicMetaObject/*!*/ self) {
            Assert.NotNull(self);

            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("UncallableError"),
                    AstUtils.Convert(self.Expression, typeof(object))
                )
            );
        }
        
        #endregion
    }
}
