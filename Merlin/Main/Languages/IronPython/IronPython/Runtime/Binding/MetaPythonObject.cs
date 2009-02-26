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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using IronPython.Runtime.Types;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    partial class MetaPythonObject : DynamicMetaObject {
        public MetaPythonObject(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions)
            : base(expression, restrictions) {
        }

        public MetaPythonObject(Expression/*!*/ expression, BindingRestrictions/*!*/ restrictions, object value)
            : base(expression, restrictions, value) {
        }

        internal static MethodCallExpression MakeTryGetTypeMember(BinderState/*!*/ binderState, PythonTypeSlot dts, Expression self, ParameterExpression tmp) {
            return MakeTryGetTypeMember(
                binderState,
                dts, 
                tmp,
                self,
                Ast.Property(
                    Ast.Convert(
                        self,
                        typeof(IPythonObject)),
                    TypeInfo._IPythonObject.PythonType
                )
            );
        }

        internal static MethodCallExpression MakeTryGetTypeMember(BinderState/*!*/ binderState, PythonTypeSlot dts, ParameterExpression tmp, Expression instance, Expression pythonType) {
            return Ast.Call(
                TypeInfo._PythonOps.SlotTryGetBoundValue,
                Ast.Constant(binderState.Context),
                AstUtils.Convert(Utils.WeakConstant(dts), typeof(PythonTypeSlot)),
                AstUtils.Convert(instance, typeof(object)),
                AstUtils.Convert(
                    pythonType,
                    typeof(PythonType)
                ),
                tmp
            );
        }

        public DynamicMetaObject Restrict(Type type) {
            return MetaObjectExtensions.Restrict(this, type);
        }

        public PythonType/*!*/ PythonType {
            get {
                return DynamicHelpers.GetPythonType(Value);
            }
        }

        public static PythonType/*!*/ GetPythonType(DynamicMetaObject/*!*/ value) {
            if (value.HasValue) {
                return DynamicHelpers.GetPythonType(value.Value);
            }

            return DynamicHelpers.GetPythonTypeFromType(value.GetLimitType());
        }

        public static Expression MakeTypeTests(DynamicMetaObject metaSelf, params DynamicMetaObject/*!*/[] args) {
            Expression typeTest = null;
            if (metaSelf != null) {
                IPythonObject self = metaSelf.Value as IPythonObject;
                if (self != null) {
                    typeTest = BindingHelpers.CheckTypeVersion(metaSelf.Expression, self.PythonType.Version);
                }
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].HasValue) {
                    IPythonObject val = args[i].Value as IPythonObject;
                    if (val != null) {
                        Expression test = BindingHelpers.CheckTypeVersion(args[i].Expression, val.PythonType.Version);

                        if (typeTest != null) {
                            typeTest = Ast.AndAlso(typeTest, test);
                        } else {
                            typeTest = test;
                        }
                    }
                }
            }

            return typeTest;
        }

        /// <summary>
        /// Creates a target which creates a new dynamic method which contains a single
        /// dynamic site that invokes the callable object.
        /// 
        /// TODO: This should be specialized for each callable object
        /// </summary>
        protected DynamicMetaObject/*!*/ MakeDelegateTarget(DynamicMetaObjectBinder/*!*/ action, Type/*!*/ toType, DynamicMetaObject/*!*/ arg) {
            Debug.Assert(arg != null);

            BinderState state = BinderState.GetBinderState(action);
            CodeContext context;
            if (state != null) {
                context = state.Context;
            } else {
                context = DefaultContext.Default;
            }
            
            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("GetDelegate"),
                    Ast.Constant(context),
                    arg.Expression,
                    Ast.Constant(toType)
                ),
                arg.Restrictions
            );
        }

        protected static DynamicMetaObject GetMemberFallback(DynamicMetaObject self, DynamicMetaObjectBinder member, Expression codeContext) {
            PythonGetMemberBinder gmb = member as PythonGetMemberBinder;
            if (gmb != null) {
                return gmb.Fallback(self, codeContext);
            }

            GetMemberBinder gma = (GetMemberBinder)member;

            return gma.FallbackGetMember(self);
        }

        protected static string GetGetMemberName(DynamicMetaObjectBinder member) {
            PythonGetMemberBinder gmb = member as PythonGetMemberBinder;
            if (gmb != null) {
                return gmb.Name;
            }

            InvokeMemberBinder invoke = member as InvokeMemberBinder;
            if (invoke != null) {
                return invoke.Name;
            }

            GetMemberBinder gma = (GetMemberBinder)member;

            return gma.Name;
        }

    }
}
