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

using Microsoft.Scripting.Actions;

using Ast = System.Linq.Expressions.Expression;

namespace IronPython.Runtime.Binding {

    static class Binders {
        /// <summary>
        /// Backwards compatible Convert for the old sites that need to flow CodeContext
        /// </summary>
        public static Expression/*!*/ Convert(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ type, ConversionResultKind resultKind, Expression/*!*/ target) {
            return Ast.Dynamic(
                binder.Convert(type, resultKind),
                type,
                target
            );
        }


        public static Expression/*!*/ Get(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.Dynamic(
                binder.GetMember(name),
                resultType,
                target,
                codeContext
            );
        }

        public static Expression/*!*/ TryGet(Expression/*!*/ codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, string/*!*/ name, Expression/*!*/ target) {
            return Ast.Dynamic(
                binder.GetMember(
                    name,
                    true
                ),
                resultType,
                target,
                codeContext
            );
        }

        public static DynamicMetaObjectBinder/*!*/ BinaryOperationRetBool(BinderState/*!*/ state, PythonOperationKind operatorName) {
            return BinaryOperationRetType(state, operatorName, typeof(bool));
        }

        public static DynamicMetaObjectBinder/*!*/ BinaryOperationRetType(BinderState/*!*/ state, PythonOperationKind operatorName, Type retType) {
            return new ComboBinder(
                new BinderMappingInfo(
                    BinaryOperationBinder(state, operatorName),
                    ParameterMappingInfo.Parameter(0),
                    ParameterMappingInfo.Parameter(1)
                ),
                new BinderMappingInfo(
                    state.Convert(retType, ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        public static DynamicMetaObjectBinder UnaryOperationBinder(BinderState state, PythonOperationKind operatorName) {
            ExpressionType? et = GetExpressionTypeFromUnaryOperator(operatorName);
            
            if (et == null) {
                return state.Operation(
                    operatorName
                );
            }

            return state.UnaryOperation(et.Value);
        }

        private static ExpressionType? GetExpressionTypeFromUnaryOperator(PythonOperationKind operatorName) {
            switch (operatorName) {
                case PythonOperationKind.Positive: return ExpressionType.UnaryPlus;
                case PythonOperationKind.Negate: return ExpressionType.Negate;
                case PythonOperationKind.OnesComplement: return ExpressionType.OnesComplement;
                case PythonOperationKind.Not: return ExpressionType.IsFalse;
            }
            return null;
        }

        public static DynamicMetaObjectBinder BinaryOperationBinder(BinderState state, PythonOperationKind operatorName) {
            ExpressionType? et = GetExpressionTypeFromBinaryOperator(operatorName);

            if (et == null) {
                return state.Operation(
                    operatorName
                );
            }

            return state.BinaryOperation(et.Value);
        }

        private static ExpressionType? GetExpressionTypeFromBinaryOperator(PythonOperationKind operatorName) {
            switch (operatorName) {
                case PythonOperationKind.Add: return ExpressionType.Add;
                case PythonOperationKind.BitwiseAnd: return ExpressionType.And;
                case PythonOperationKind.Divide: return ExpressionType.Divide;
                case PythonOperationKind.ExclusiveOr: return ExpressionType.ExclusiveOr;
                case PythonOperationKind.Mod: return ExpressionType.Modulo;
                case PythonOperationKind.Multiply: return ExpressionType.Multiply;
                case PythonOperationKind.BitwiseOr: return ExpressionType.Or;
                case PythonOperationKind.Power: return ExpressionType.Power;
                case PythonOperationKind.RightShift: return ExpressionType.RightShift;
                case PythonOperationKind.LeftShift: return ExpressionType.LeftShift;
                case PythonOperationKind.Subtract: return ExpressionType.Subtract;

                case PythonOperationKind.InPlaceAdd: return ExpressionType.AddAssign;
                case PythonOperationKind.InPlaceBitwiseAnd: return ExpressionType.AndAssign;
                case PythonOperationKind.InPlaceDivide: return ExpressionType.DivideAssign;
                case PythonOperationKind.InPlaceExclusiveOr: return ExpressionType.ExclusiveOrAssign;
                case PythonOperationKind.InPlaceMod: return ExpressionType.ModuloAssign;
                case PythonOperationKind.InPlaceMultiply: return ExpressionType.MultiplyAssign;
                case PythonOperationKind.InPlaceBitwiseOr: return ExpressionType.OrAssign;
                case PythonOperationKind.InPlacePower: return ExpressionType.PowerAssign;
                case PythonOperationKind.InPlaceRightShift: return ExpressionType.RightShiftAssign;
                case PythonOperationKind.InPlaceLeftShift: return ExpressionType.LeftShiftAssign;
                case PythonOperationKind.InPlaceSubtract: return ExpressionType.SubtractAssign;

                case PythonOperationKind.Equal: return ExpressionType.Equal;
                case PythonOperationKind.GreaterThan: return ExpressionType.GreaterThan;
                case PythonOperationKind.GreaterThanOrEqual: return ExpressionType.GreaterThanOrEqual;
                case PythonOperationKind.LessThan: return ExpressionType.LessThan;
                case PythonOperationKind.LessThanOrEqual: return ExpressionType.LessThanOrEqual;
                case PythonOperationKind.NotEqual: return ExpressionType.NotEqual;
            }
            return null;
        }

        public static DynamicMetaObjectBinder/*!*/ InvokeAndConvert(BinderState/*!*/ state, int argCount, Type retType) {
            // +2 for the target object and CodeContext which InvokeBinder recevies
            ParameterMappingInfo[] args = new ParameterMappingInfo[argCount + 2];   
            for (int i = 0; i < argCount + 2; i++) {
                args[i] = ParameterMappingInfo.Parameter(i);
            }

            return new ComboBinder(
                new BinderMappingInfo(
                    state.Invoke(
                        new CallSignature(argCount)
                    ),
                    args
                ),
                new BinderMappingInfo(
                    state.Convert(retType, ConversionResultKind.ExplicitCast),
                    ParameterMappingInfo.Action(0)
                )
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional splatting.
        /// 
        /// The signature of the target site should be object(function), object[], retType
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static PythonInvokeBinder/*!*/ InvokeSplat(BinderState/*!*/ state) {
            return state.Invoke(
                new CallSignature(new Argument(ArgumentType.List))
            );
        }

        /// <summary>
        /// Creates a new InvokeBinder which will call with positional and keyword splatting.
        /// 
        /// The signature of the target site should be object(function), object[], dictionary, retType
        /// </summary>
        public static PythonInvokeBinder/*!*/ InvokeKeywords(BinderState/*!*/ state) {
            return state.Invoke(
                new CallSignature(new Argument(ArgumentType.List), new Argument(ArgumentType.Dictionary))
            );
        }


    }
}
