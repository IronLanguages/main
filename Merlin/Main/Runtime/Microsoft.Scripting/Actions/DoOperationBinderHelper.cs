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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public class DoOperationBinderHelper : BinderHelper<OldDoOperationAction> {
        private object[] _args;                                     // arguments we were created with
        private Type[] _types;                                      // types of our arguments
        private readonly RuleBuilder _rule;                         // the rule we're building and returning

        public DoOperationBinderHelper(CodeContext context, OldDoOperationAction action, object[] args, RuleBuilder rule)
            : base(context, action) {
            _args = args;
            _types = CompilerHelpers.GetTypes(args);
            _rule = rule;
            _rule.MakeTest(_types);
        }

        public void MakeRule() {
            if (Action.Operation == Operators.GetItem || 
                Action.Operation == Operators.SetItem) {
                // try default member first, then look for special name methods.
                if (MakeDefaultMemberRule(Action.Operation)) {
                    return;
                }
            }

            ExpressionType? et = OperatorInfo.OperatorToExpressionType(Action.Operation);
            if (et == null) {
                throw new InvalidOperationException(String.Format("unknown ExpressionType: {0}", Action.Operation));
            }
            OperatorInfo info = OperatorInfo.GetOperatorInfo(et.Value);
            if (Action.IsComparision) {
                MakeComparisonRule(info);
            } else {
                MakeOperatorRule(info);
            }
        }

        #region Comparison operator

        private void MakeComparisonRule(OperatorInfo info) {
            // check the first type if it has an applicable method
            MethodInfo[] targets = GetApplicableMembers(info);            
            if (targets.Length > 0 && TryMakeBindingTarget(targets)) {
                return;
            }

            // then check the 2nd type.
            targets = GetApplicableMembers(_types[1], info);
            if (targets.Length > 0 && TryMakeBindingTarget(targets)) {
                return;
            }

            // try Compare: cmp(x,y) (>, <, >=, <=, ==, !=) 0
            if (TryNumericComparison(info)) {
                return;
            }

            // try inverting the operator & result (e.g. if looking for Equals try NotEquals, LessThan for GreaterThan)...
            Operators revOp = GetInvertedOperator(OperatorInfo.ExpressionTypeToOperator(info.Operator));
            OperatorInfo revInfo = OperatorInfo.GetOperatorInfo(OperatorInfo.OperatorToExpressionType(revOp).Value);
            Debug.Assert(revInfo != null);

            // try the 1st type's opposite function result negated 
            targets = GetApplicableMembers(revInfo);
            if (targets.Length > 0 && TryMakeInvertedBindingTarget(targets)) {
                return;
            }

            // then check the 2nd type.
            targets = GetApplicableMembers(_types[1], revInfo);
            if (targets.Length > 0 && TryMakeInvertedBindingTarget(targets)) {
                return;
            }

            // see if we're comparing to null w/ an object ref or a Nullable<T>
            if (TryMakeNullComparisonRule()) {
                return;
            }

            // see if this is a primitive type where we're comparing the two values.
            if (TryPrimitiveCompare()) {
                return;
            }

            SetErrorTarget(info);
        }

        private bool TryNumericComparison(OperatorInfo info) {
            MethodInfo[] targets = FilterNonMethods(_types[0], Binder.GetMember(Action, _types[0], "Compare"));
            if (targets.Length > 0) {
                MethodBinder mb = MethodBinder.MakeBinder(Binder, targets[0].Name, targets);
                BindingTarget target = mb.MakeBindingTarget(CallTypes.None, _types);
                if (target.Success) {
                    Expression call = Ast.Convert(target.MakeExpression(_rule, _rule.Parameters), typeof(int));
                    switch (info.Operator) {
                        case ExpressionType.GreaterThan: call = Ast.GreaterThan(call, AstUtils.Constant(0)); break;
                        case ExpressionType.LessThan: call = Ast.LessThan(call, AstUtils.Constant(0)); break;
                        case ExpressionType.GreaterThanOrEqual: call = Ast.GreaterThanOrEqual(call, AstUtils.Constant(0)); break;
                        case ExpressionType.LessThanOrEqual: call = Ast.LessThanOrEqual(call, AstUtils.Constant(0)); break;
                        case ExpressionType.Equal: call = Ast.Equal(call, AstUtils.Constant(0)); break;
                        case ExpressionType.NotEqual: call = Ast.NotEqual(call, AstUtils.Constant(0)); break;
                    }
                    _rule.Target = _rule.MakeReturn(Binder, call);
                    return true;
                }
            }
            return false;
        }

        private bool TryPrimitiveCompare() {
            if (TypeUtils.GetNonNullableType(_types[0]) == TypeUtils.GetNonNullableType(_types[1]) &&
                TypeUtils.IsNumeric(_types[0])) {
                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                switch (Action.Operation) {
                    case Operators.Equals:             expr = Ast.Equal(Param0, Param1); break;
                    case Operators.NotEquals:          expr = Ast.NotEqual(Param0, Param1); break;
                    case Operators.GreaterThan:        expr = Ast.GreaterThan(Param0, Param1); break;
                    case Operators.LessThan:           expr = Ast.LessThan(Param0, Param1); break;
                    case Operators.GreaterThanOrEqual: expr = Ast.GreaterThanOrEqual(Param0, Param1); break;
                    case Operators.LessThanOrEqual:    expr = Ast.LessThanOrEqual(Param0, Param1); break;
                    default: throw new InvalidOperationException();
                }
                _rule.Target = _rule.MakeReturn(Binder, expr); 
                return true;                
            }
            return false;
        }

        /// <summary>
        /// Produces a rule for comparing a value to null - supports comparing object references and nullable types.
        /// </summary>
        private bool TryMakeNullComparisonRule() {
            if (_types[0] == typeof(DynamicNull)) {
                if (!_types[1].IsValueType) {
                    _rule.Target = _rule.MakeReturn(Binder, Ast.Equal(_rule.Parameters[1], AstUtils.Constant(null)));
                } else if (_types[1].GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    _rule.Target = _rule.MakeReturn(Binder, Ast.Property(Param1, _types[1].GetProperty("HasValue")));
                } else {
                    return false;
                }
                return true;
            } else if (_types[1] == typeof(DynamicNull)) {
                if (!_types[0].IsValueType) {
                    _rule.Target = _rule.MakeReturn(Binder, Ast.Equal(_rule.Parameters[0], AstUtils.Constant(null)));
                } else if (_types[0].GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    _rule.Target = _rule.MakeReturn(Binder, Ast.Property(Param0, _types[1].GetProperty("HasValue")));
                } else {
                    return false;
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Operator Rule

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // TODO: fix
        private void MakeOperatorRule(OperatorInfo info) {
            MethodInfo[] targets = GetApplicableMembers(info);
            
            if (targets.Length > 0 && TryMakeBindingTarget(targets)) {
                return;
            }

            if (_types.Length > 1) {
                targets = GetApplicableMembers(_types[1], info);
                if (targets.Length > 0 && TryMakeBindingTarget(targets)) {
                    return;
                }
            }

            if (_types.Length == 2 &&
                TypeUtils.GetNonNullableType(_types[0]) == TypeUtils.GetNonNullableType(_types[1]) &&
                TypeUtils.IsArithmetic(_types[0])) {
                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                switch (info.Operator) {
                    case ExpressionType.Add: expr = Ast.Add(Param0, Param1); break;
                    case ExpressionType.Subtract: expr = Ast.Subtract(Param0, Param1); break;
                    case ExpressionType.Divide: expr = Ast.Divide(Param0, Param1); break;
                    case ExpressionType.Modulo: expr = Ast.Modulo(Param0, Param1); break;
                    case ExpressionType.Multiply: expr = Ast.Multiply(Param0, Param1); break;
                    case ExpressionType.LeftShift: expr = Ast.LeftShift(Param0, Param1); break;
                    case ExpressionType.RightShift: expr = Ast.RightShift(Param0, Param1); break;
                    case ExpressionType.And: expr = Ast.And(Param0, Param1); break;
                    case ExpressionType.Or: expr = Ast.Or(Param0, Param1); break;
                    case ExpressionType.ExclusiveOr: expr = Ast.ExclusiveOr(Param0, Param1); break;
                    default: throw new InvalidOperationException();
                }
                _rule.Target = _rule.MakeReturn(Binder, expr);
                return;
            } else if(_types.Length == 1 && TryMakeDefaultUnaryRule(info)) {
                return;
            }
            
            SetErrorTarget(info);
        }

        private bool TryMakeDefaultUnaryRule(OperatorInfo info) {
            switch (info.Operator) {
                case ExpressionType.IsTrue:
                    if (_types[0] == typeof(bool)) {
                        _rule.Target = _rule.MakeReturn(Binder, Param0);
                        return true;
                    }
                    break;
                case ExpressionType.Negate:
                    if (TypeUtils.IsArithmetic(_types[0])) {
                        _rule.Target = _rule.MakeReturn(Binder, Ast.Negate(Param0));
                        return true;
                    }
                    break;
                case ExpressionType.Not:
                    if (TypeUtils.IsIntegerOrBool(_types[0])) {
                        _rule.Target = _rule.MakeReturn(Binder, Ast.Not(Param0));
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion

        #region Indexer Rule

        private bool MakeDefaultMemberRule(Operators oper) {
            if (_types[0].IsArray) {
                if (Binder.CanConvertFrom(_types[1], typeof(int), false, NarrowingLevel.All)) {
                    if(oper == Operators.GetItem) {
                        _rule.Target = _rule.MakeReturn(Binder,
                            Ast.ArrayAccess(
                                Param0,
                                ConvertIfNeeded(Param1, typeof(int))
                            )
                        );
                    } else {
                        _rule.Target = _rule.MakeReturn(Binder,
                            Ast.Assign(
                                Ast.ArrayAccess(
                                    Param0,
                                    ConvertIfNeeded(Param1, typeof(int))
                                ),
                                ConvertIfNeeded(Param2, _types[0].GetElementType())
                            )
                        );
                    }
                    return true;
                }
            }

            MethodInfo[] defaults = GetMethodsFromDefaults(_types[0].GetDefaultMembers(), oper);
            if (defaults.Length != 0) {
                MethodBinder binder = MethodBinder.MakeBinder(Binder,
                    oper == Operators.GetItem ? "get_Item" : "set_Item",
                    defaults);

                BindingTarget target = binder.MakeBindingTarget(CallTypes.ImplicitInstance, _types);

                if (target.Success) {
                    if (oper == Operators.GetItem) {
                        _rule.Target = _rule.MakeReturn(Binder, target.MakeExpression(_rule, _rule.Parameters));
                    } else {

                        _rule.Target = _rule.MakeReturn(Binder,
                            Ast.Block(
                                target.MakeExpression(_rule, _rule.Parameters),
                                _rule.Parameters[2]
                            )
                        );
                    }
                } else {
                    _rule.Target = Binder.MakeInvalidParametersError(target).MakeErrorForRule(_rule, Binder);
                }
                return true;
            }
            
            return false;
        }

        private MethodInfo[] GetMethodsFromDefaults(MemberInfo[] defaults, Operators op) {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MemberInfo mi in defaults) {
                PropertyInfo pi = mi as PropertyInfo;

                if (pi != null) {
                    if (op == Operators.GetItem) {
                        MethodInfo method = pi.GetGetMethod(PrivateBinding);
                        if (method != null) methods.Add(method);
                    } else if (op == Operators.SetItem) {
                        MethodInfo method = pi.GetSetMethod(PrivateBinding);
                        if (method != null) methods.Add(method);
                    }
                }
            }

            // if we received methods from both declaring type & base types we need to filter them
            Dictionary<MethodSignatureInfo, MethodInfo> dict = new Dictionary<MethodSignatureInfo, MethodInfo>();
            foreach (MethodInfo mb in methods) {
                MethodSignatureInfo args = new MethodSignatureInfo(mb.IsStatic, mb.GetParameters());
                MethodInfo other;

                if (dict.TryGetValue(args, out other)) {
                    if (other.DeclaringType.IsAssignableFrom(mb.DeclaringType)) {
                        // derived type replaces...
                        dict[args] = mb;
                    } 
                } else {
                    dict[args] = mb;
                }
            }

            return new List<MethodInfo>(dict.Values).ToArray();
        }        

        #endregion

        #region Common helpers

        public Expression Param0 {
            get { return GetParameter(0); }
        }

        public Expression Param1 {
            get { return GetParameter(1); }
        }
        
        public Expression Param2 {
            get { return GetParameter(2); }
        }

        private Expression GetParameter(int index) {
            Expression expr = _rule.Parameters[index];
            if (_types[index].IsAssignableFrom(expr.Type)) return expr;
            return AstUtils.Convert(expr, _types[index]);
        }

        private bool TryMakeBindingTarget(MethodInfo[] targets) {
            MethodBinder mb = MethodBinder.MakeBinder(Binder, targets[0].Name, targets);
            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, _types);
            if (target.Success) {
                Expression call = target.MakeExpression(_rule, _rule.Parameters);
                _rule.Target = _rule.MakeReturn(Binder, call);
                return true;
            }
            return false;
        }

        private bool TryMakeInvertedBindingTarget(MethodInfo[] targets) {
            MethodBinder mb = MethodBinder.MakeBinder(Binder, targets[0].Name, targets);
            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, _types);
            if (target.Success) {
                Expression call = target.MakeExpression(_rule, _rule.Parameters);
                _rule.Target = _rule.MakeReturn(Binder, Ast.Not(call));
                return true;
            }
            return false;
        }

        private static Operators GetInvertedOperator(Operators op) {
            switch (op) {
                case Operators.LessThan: return Operators.GreaterThanOrEqual;
                case Operators.LessThanOrEqual: return Operators.GreaterThan;
                case Operators.GreaterThan: return Operators.LessThanOrEqual;
                case Operators.GreaterThanOrEqual: return Operators.LessThan;
                case Operators.Equals: return Operators.NotEquals;
                case Operators.NotEquals: return Operators.Equals;
                default: throw new InvalidOperationException();
            }
        }

        private Expression ConvertIfNeeded(Expression expression, Type type) {
            if (expression.Type != type) {
                return Expression.Dynamic(
                    OldConvertToAction.Make(Binder, type, ConversionResultKind.ExplicitCast),
                    type,
                    _rule.Context,
                    expression
                );
            }
            return expression;
        }

        private void SetErrorTarget(OperatorInfo info) {
            _rule.Target =
                _rule.MakeError(
                    AstUtils.ComplexCallHelper(
                        typeof(BinderOps).GetMethod("BadArgumentsForOperation"),
                        ArrayUtils.Insert((Expression)AstUtils.Constant(info.Operator), _rule.Parameters)
                    )
                );
        }

        private MethodInfo[] GetApplicableMembers(OperatorInfo info) {
            return GetApplicableMembers(CompilerHelpers.GetType(_args[0]), info);
        }

        private MethodInfo[] GetApplicableMembers(Type t, OperatorInfo info) {
            MemberGroup members = Binder.GetMember(Action, t, info.Name);
            if (members.Count == 0 && info.AlternateName != null) {
                members = Binder.GetMember(Action, t, info.AlternateName);
            }

            // filter down to just methods
            return FilterNonMethods(t, members);
        }

        private static MethodInfo[] FilterNonMethods(Type t, MemberGroup members) {
            List<MethodInfo> methods = new List<MethodInfo>(members.Count);
            foreach (MemberTracker mi in members) {
                if (mi.MemberType == TrackerTypes.Method) {
                    MethodInfo method = ((MethodTracker)mi).Method ;

                    // don't call object methods for None type, but if someone added
                    // methods to null we'd call those.
                    if (method.DeclaringType != typeof(object) || t != typeof(DynamicNull)) {
                        methods.Add(method);
                    }
                }
            }

            return methods.ToArray();
        }

        #endregion
    }
}
