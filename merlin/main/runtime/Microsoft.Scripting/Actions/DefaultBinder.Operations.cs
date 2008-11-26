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
using System.Dynamic.Binders;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions.Calls;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {
        public MetaObject DoOperation(string operation, params MetaObject[] args) {
            return DoOperation(operation, Ast.Constant(null, typeof(CodeContext)), args);
        }

        public MetaObject DoOperation(string operation, Expression codeContext, params MetaObject[] args) {
            ContractUtils.RequiresNotNull(operation, "operation");
            ContractUtils.RequiresNotNull(codeContext, "codeContext");
            ContractUtils.RequiresNotNullItems(args, "args");

            return
                MakeDefaultMemberRule(operation, args) ??   // see if we have a default member and we're doing indexing
                MakeGeneralOperatorRule(operation, codeContext, args);   // Then try comparison / other operators
        }

        /// <summary>
        /// Creates the MetaObject for indexing directly into arrays or indexing into objects which have
        /// default members.  Returns null if we're not an indexing operation.
        /// </summary>
        private MetaObject MakeDefaultMemberRule(string oper, MetaObject[] args) {
            if (oper == StandardOperators.GetItem || oper == StandardOperators.SetItem) {
                if (args[0].LimitType.IsArray) {
                    return MakeArrayIndexRule(oper, args);
                }

                return MakeMethodIndexRule(oper, args);
            }

            return null;
        }

        /// <summary>
        /// Creates the meta object for the rest of the operations: comparisons and all other
        /// operators.  If the operation cannot be completed a MetaObject which indicates an
        /// error will be returned.
        /// </summary>
        private MetaObject MakeGeneralOperatorRule(string operation, Expression codeContext, MetaObject[] args) {
            OperatorInfo info = OperatorInfo.GetOperatorInfo(operation);
            MetaObject res;

            if (CompilerHelpers.IsComparisonOperator(operation)) {
                res = MakeComparisonRule(info, codeContext, args);
            } else {
                res = MakeOperatorRule(info, codeContext, args);
            }

            return res;
        }

        #region Comparison operator

        private MetaObject MakeComparisonRule(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            return
                TryComparisonMethod(info, codeContext, args[0], args) ??   // check the first type if it has an applicable method
                TryComparisonMethod(info, codeContext, args[0], args) ??   // then check the second type
                TryNumericComparison(info, args) ??           // try Compare: cmp(x,y) (>, <, >=, <=, ==, !=) 0
                TryInvertedComparison(info, args[0], args) ?? // try inverting the operator & result (e.g. if looking for Equals try NotEquals, LessThan for GreaterThan)...
                TryInvertedComparison(info, args[0], args) ?? // inverted binding on the 2nd type
                TryNullComparisonRule(args) ??                // see if we're comparing to null w/ an object ref or a Nullable<T>
                TryPrimitiveCompare(info, args) ??            // see if this is a primitive type where we're comparing the two values.
                MakeOperatorError(info, args);                // no comparisons are possible            
        }

        private MetaObject TryComparisonMethod(OperatorInfo info, Expression codeContext, MetaObject target, MetaObject[] args) {
            MethodInfo[] targets = GetApplicableMembers(target.LimitType, info);
            if (targets.Length > 0) {
                return TryMakeBindingTarget(targets, args, codeContext, Restrictions.Empty);
            }

            return null;
        }

        private static MetaObject MakeOperatorError(OperatorInfo info, MetaObject[] args) {
            return new MetaObject(
                Ast.Throw(
                    AstUtils.ComplexCallHelper(
                        typeof(BinderOps).GetMethod("BadArgumentsForOperation"),
                        ArrayUtils.Insert((Expression)Ast.Constant(info.Operator), MetaObject.GetExpressions(args))
                    )
                ),
                Restrictions.Combine(args)
            );
        }

        private MetaObject TryNumericComparison(OperatorInfo info, MetaObject[] args) {
            MethodInfo[] targets = FilterNonMethods(
                args[0].LimitType,
                GetMember(OldDoOperationAction.Make(this, info.Operator),
                args[0].LimitType,
                "Compare")
            );

            if (targets.Length > 0) {
                MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);
                BindingTarget target = mb.MakeBindingTarget(CallTypes.None, args);
                if (target.Success) {
                    Expression call = AstUtils.Convert(target.MakeExpression(), typeof(int));
                    switch (info.Operator) {
                        case Operators.GreaterThan: call = Ast.GreaterThan(call, Ast.Constant(0)); break;
                        case Operators.LessThan: call = Ast.LessThan(call, Ast.Constant(0)); break;
                        case Operators.GreaterThanOrEqual: call = Ast.GreaterThanOrEqual(call, Ast.Constant(0)); break;
                        case Operators.LessThanOrEqual: call = Ast.LessThanOrEqual(call, Ast.Constant(0)); break;
                        case Operators.Equals: call = Ast.Equal(call, Ast.Constant(0)); break;
                        case Operators.NotEquals: call = Ast.NotEqual(call, Ast.Constant(0)); break;
                        case Operators.Compare:
                            break;
                    }

                    return new MetaObject(
                        call,
                        Restrictions.Combine(target.RestrictedArguments)
                    );
                }
            }

            return null;
        }

        private MetaObject TryInvertedComparison(OperatorInfo info, MetaObject target, MetaObject[] args) {
            Operators revOp = GetInvertedOperator(info.Operator);
            OperatorInfo revInfo = OperatorInfo.GetOperatorInfo(revOp);
            Debug.Assert(revInfo != null);

            // try the 1st type's opposite function result negated 
            MethodBase[] targets = GetApplicableMembers(target.LimitType, revInfo);
            if (targets.Length > 0) {
                return TryMakeInvertedBindingTarget(targets, args);
            }

            return null;
        }

        /// <summary>
        /// Produces a rule for comparing a value to null - supports comparing object references and nullable types.
        /// </summary>
        private static MetaObject TryNullComparisonRule(MetaObject[] args) {
            Type otherType = args[0].LimitType;

            Restrictions restrictions = Restrictions.GetTypeRestriction(args[0].Expression, args[0].LimitType).Merge(Restrictions.Combine(args));

            if (args[0].LimitType == typeof(Null)) {
                if (!otherType.IsValueType) {
                    return new MetaObject(
                        Ast.Equal(args[0].Expression, Ast.Constant(null)),
                        restrictions
                    );
                } else if (otherType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return new MetaObject(
                        Ast.Property(args[0].Expression, otherType.GetProperty("HasValue")),
                        restrictions
                    );
                }
            } else if (otherType == typeof(Null)) {
                if (!args[0].LimitType.IsValueType) {
                    return new MetaObject(
                        Ast.Equal(args[0].Expression, Ast.Constant(null)),
                        restrictions
                    );
                } else if (args[0].LimitType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    return new MetaObject(
                        Ast.Property(args[0].Expression, otherType.GetProperty("HasValue")),
                        restrictions
                    );
                }
            }

            return null;
        }

        private static MetaObject TryPrimitiveCompare(OperatorInfo info, MetaObject[] args) {
            if (TypeUtils.GetNonNullableType(args[0].LimitType) == TypeUtils.GetNonNullableType(args[1].LimitType) &&
                TypeUtils.IsNumeric(args[0].LimitType)) {
                Expression arg0 = args[0].Expression;
                Expression arg1 = args[1].Expression;

                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                switch (info.Operator) {
                    case Operators.Equals: expr = Ast.Equal(arg0, arg1); break;
                    case Operators.NotEquals: expr = Ast.NotEqual(arg0, arg1); break;
                    case Operators.GreaterThan: expr = Ast.GreaterThan(arg0, arg1); break;
                    case Operators.LessThan: expr = Ast.LessThan(arg0, arg1); break;
                    case Operators.GreaterThanOrEqual: expr = Ast.GreaterThanOrEqual(arg0, arg1); break;
                    case Operators.LessThanOrEqual: expr = Ast.LessThanOrEqual(arg0, arg1); break;
                    default: throw new InvalidOperationException();
                }

                return new MetaObject(
                    expr,
                    Restrictions.GetTypeRestriction(arg0, args[0].LimitType).Merge(Restrictions.GetTypeRestriction(arg1, args[0].LimitType)).Merge(Restrictions.Combine(args))
                );
            }

            return null;
        }

        #endregion

        #region Operator Rule

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // TODO: fix
        private MetaObject MakeOperatorRule(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            return
                TryForwardOperator(info, codeContext, args) ??
                TryReverseOperator(info, codeContext, args) ??
                TryInplaceOperator(info, codeContext, args) ??
                TryPrimitiveOperator(info, args) ??
                TryMakeDefaultUnaryRule(info, codeContext, args) ??
                MakeOperatorError(info, args);
        }

        private static MetaObject TryPrimitiveOperator(OperatorInfo info, MetaObject[] args) {
            if (args.Length == 2 &&
                TypeUtils.GetNonNullableType(args[0].LimitType) == TypeUtils.GetNonNullableType(args[1].LimitType) &&
                TypeUtils.IsArithmetic(args[0].LimitType)) {
                // TODO: Nullable<PrimitveType> Support
                Expression expr;
                MetaObject self = args[0].Restrict(args[0].LimitType);
                MetaObject arg0 = args[1].Restrict(args[0].LimitType);

                switch (info.Operator) {
                    case Operators.Add: expr = Ast.Add(self.Expression, arg0.Expression); break;
                    case Operators.Subtract: expr = Ast.Subtract(self.Expression, arg0.Expression); break;
                    case Operators.Divide: expr = Ast.Divide(self.Expression, arg0.Expression); break;
                    case Operators.Mod: expr = Ast.Modulo(self.Expression, arg0.Expression); break;
                    case Operators.Multiply: expr = Ast.Multiply(self.Expression, arg0.Expression); break;
                    case Operators.LeftShift: expr = Ast.LeftShift(self.Expression, arg0.Expression); break;
                    case Operators.RightShift: expr = Ast.RightShift(self.Expression, arg0.Expression); break;
                    case Operators.BitwiseAnd: expr = Ast.And(self.Expression, arg0.Expression); break;
                    case Operators.BitwiseOr: expr = Ast.Or(self.Expression, arg0.Expression); break;
                    case Operators.ExclusiveOr: expr = Ast.ExclusiveOr(self.Expression, arg0.Expression); break;
                    default: throw new InvalidOperationException();
                }

                return new MetaObject(
                    expr,
                    self.Restrictions.Merge(arg0.Restrictions)
                );
            }

            return null;
        }

        private MetaObject TryForwardOperator(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            // we need a special conversion for the return type on MemberNames
            if (info.Operator != Operators.MemberNames) {
                MethodInfo[] targets = GetApplicableMembers(args[0].LimitType, info);
                Restrictions restrictions = Restrictions.Empty;
                if (targets.Length == 0) {
                    targets = GetFallbackMembers(args[0].LimitType, info, args, out restrictions);
                }

                if (targets.Length > 0) {
                    return TryMakeBindingTarget(targets, args, codeContext, restrictions);
                }
            }

            return null;
        }

        private MetaObject TryReverseOperator(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            // we need a special conversion for the return type on MemberNames
            if (info.Operator != Operators.MemberNames) {
                if (args.Length > 0) {
                    MethodInfo[] targets = GetApplicableMembers(args[0].LimitType, info);
                    if (targets.Length > 0) {
                        return TryMakeBindingTarget(targets, args, codeContext, Restrictions.Empty);
                    }
                }
            }

            return null;
        }

        private MetaObject TryInplaceOperator(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            Operators op = CompilerHelpers.InPlaceOperatorToOperator(info.Operator);

            if (op != Operators.None) {
                // recurse to try and get the non-inplace action...
                return MakeOperatorRule(OperatorInfo.GetOperatorInfo(op), codeContext, args);
            }

            return null;
        }

        private static MetaObject TryMakeDefaultUnaryRule(OperatorInfo info, Expression codeContext, MetaObject[] args) {
            if (args.Length == 1) {
                Restrictions restrictions = Restrictions.GetTypeRestriction(args[0].Expression, args[0].LimitType).Merge(Restrictions.Combine(args));
                switch (info.Operator) {
                    case Operators.IsTrue:
                        if (args[0].LimitType == typeof(bool)) {
                            return args[0];
                        }
                        break;
                    case Operators.Negate:
                        if (TypeUtils.IsArithmetic(args[0].LimitType)) {
                            return new MetaObject(
                                Ast.Negate(args[0].Expression),
                                restrictions
                            );
                        }
                        break;
                    case Operators.Not:
                        if (TypeUtils.IsIntegerOrBool(args[0].LimitType)) {
                            return new MetaObject(
                                Ast.Not(args[0].Expression),
                                restrictions
                            );
                        }
                        break;
                    case Operators.Documentation:
                        object[] attrs = args[0].LimitType.GetCustomAttributes(typeof(DocumentationAttribute), true);
                        string documentation = String.Empty;

                        if (attrs.Length > 0) {
                            documentation = ((DocumentationAttribute)attrs[0]).Documentation;
                        }

                        return new MetaObject(
                            Ast.Constant(documentation),
                            restrictions
                        );
                    case Operators.MemberNames:
                        if (typeof(IMembersList).IsAssignableFrom(args[0].LimitType)) {
                            return MakeIMembersListRule(codeContext, args[0]);
                        }

                        MemberInfo[] members = args[0].LimitType.GetMembers();
                        Dictionary<string, string> mems = new Dictionary<string, string>();
                        foreach (MemberInfo mi in members) {
                            mems[mi.Name] = mi.Name;
                        }

                        string[] res = new string[mems.Count];
                        mems.Keys.CopyTo(res, 0);

                        return new MetaObject(
                            Ast.Constant(res),
                            restrictions
                        );
                    case Operators.CallSignatures:
                        return MakeCallSignatureResult(CompilerHelpers.GetMethodTargets(args[0].LimitType), args[0]);
                    case Operators.IsCallable:
                        // IsCallable() is tightly tied to Call actions. So in general, we need the call-action providers to also
                        // provide IsCallable() status. 
                        // This is just a rough fallback. We could also attempt to simulate the default CallBinder logic to see
                        // if there are any applicable calls targets, but that would be complex (the callbinder wants the argument list, 
                        // which we don't have here), and still not correct. 
                        bool callable = false;
                        if (typeof(Delegate).IsAssignableFrom(args[0].LimitType) ||
                            typeof(MethodGroup).IsAssignableFrom(args[0].LimitType)) {
                            callable = true;
                        }

                        return new MetaObject(
                            Ast.Constant(callable),
                            restrictions
                        );
                }
            }
            return null;
        }

        private static MetaObject MakeIMembersListRule(Expression codeContext, MetaObject target) {
            return new MetaObject(
                Ast.Call(
                    typeof(BinderOps).GetMethod("GetStringMembers"),
                    Ast.Call(
                        AstUtils.Convert(target.Expression, typeof(IMembersList)),
                        typeof(IMembersList).GetMethod("GetMemberNames"),
                        codeContext
                    )
                ),
                Restrictions.GetTypeRestriction(target.Expression, target.LimitType).Merge(target.Restrictions)
            );
        }

        private static MetaObject MakeCallSignatureResult(MethodBase[] methods, MetaObject target) {
            List<string> arrres = new List<string>();

            if (methods != null) {
                foreach (MethodBase mb in methods) {
                    StringBuilder res = new StringBuilder();
                    string comma = "";
                    foreach (ParameterInfo param in mb.GetParameters()) {
                        if (param.ParameterType == typeof(CodeContext)) continue;

                        res.Append(comma);
                        res.Append(param.ParameterType.Name);
                        res.Append(" ");
                        res.Append(param.Name);
                        comma = ", ";
                    }
                    arrres.Add(res.ToString());
                }
            }

            return new MetaObject(
                Ast.Constant(arrres.ToArray()),
                Restrictions.GetTypeRestriction(target.Expression, target.LimitType).Merge(target.Restrictions)
            );
        }

        #endregion

        #region Indexer Rule

        private static Type GetArgType(MetaObject[] args, int index) {
            return args[index].HasValue ? args[index].LimitType : args[index].Expression.Type;
        }

        private MetaObject MakeMethodIndexRule(string oper, MetaObject[] args) {
            MethodInfo[] defaults = GetMethodsFromDefaults(args[0].LimitType.GetDefaultMembers(), oper);
            if (defaults.Length != 0) {
                MethodBinder binder = MethodBinder.MakeBinder(
                    this,
                    oper == StandardOperators.GetItem ? "get_Item" : "set_Item",
                    defaults);

                MetaObject[] selfWithArgs = args;
                ParameterExpression arg2 = null;

                if (oper == StandardOperators.SetItem) {
                    Debug.Assert(args.Length >= 2);

                    // need to save arg2 in a temp because it's also our result
                    arg2 = Ast.Variable(args[2].Expression.Type, "arg2Temp");

                    args[2] = new MetaObject(
                        Ast.Assign(arg2, args[2].Expression),
                        args[2].Restrictions
                    );
                }

                BindingTarget target = binder.MakeBindingTarget(CallTypes.ImplicitInstance, selfWithArgs);

                Restrictions restrictions = Restrictions.Combine(args);

                if (target.Success) {
                    if (oper == StandardOperators.GetItem) {
                        return new MetaObject(
                            target.MakeExpression(),
                            restrictions.Merge(Restrictions.Combine(target.RestrictedArguments))
                        );
                    } else {
                        return new MetaObject(
                            Ast.Block(
                                new ParameterExpression[] { arg2 },
                                target.MakeExpression(),
                                arg2
                            ),
                            restrictions.Merge(Restrictions.Combine(target.RestrictedArguments))
                        );
                    }
                }

                return MakeError(
                    MakeInvalidParametersError(target),
                    restrictions
                );
            }

            return null;
        }

        private MetaObject MakeArrayIndexRule(string oper, MetaObject[] args) {
            if (CanConvertFrom(GetArgType(args, 1), typeof(int), false, NarrowingLevel.All)) {
                Restrictions restrictions = Restrictions.GetTypeRestriction(args[0].Expression, args[0].LimitType).Merge(Restrictions.Combine(args));

                if (oper == StandardOperators.GetItem) {
                    return new MetaObject(
                        Ast.ArrayAccess(
                            args[0].Expression,
                            ConvertIfNeeded(args[1].Expression, typeof(int))
                        ),
                        restrictions
                    );
                } else {
                    return new MetaObject(
                        Ast.Assign(
                            Ast.ArrayAccess(
                                args[0].Expression,
                                ConvertIfNeeded(args[1].Expression, typeof(int))
                            ),
                            ConvertIfNeeded(args[2].Expression, args[0].LimitType.GetElementType())
                        ),
                        restrictions.Merge(args[1].Restrictions)
                    );
                }
            }

            return null;
        }

        private MethodInfo[] GetMethodsFromDefaults(MemberInfo[] defaults, string op) {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MemberInfo mi in defaults) {
                PropertyInfo pi = mi as PropertyInfo;

                if (pi != null) {
                    if (op == StandardOperators.GetItem) {
                        MethodInfo method = pi.GetGetMethod(PrivateBinding); 
                        if (method != null) methods.Add(method);
                    } else if (op == StandardOperators.SetItem) {
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

        private MetaObject TryMakeBindingTarget(MethodInfo[] targets, MetaObject[] args, Expression codeContext, Restrictions restrictions) {
            MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);

            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, args);
            if (target.Success) {
                return new MetaObject(
                    target.MakeExpression(new ParameterBinderWithCodeContext(this, codeContext)),
                    restrictions.Merge(Restrictions.Combine(target.RestrictedArguments))
                );
            }

            return null;
        }

        private MetaObject TryMakeInvertedBindingTarget(MethodBase[] targets, MetaObject[] args) {
            MethodBinder mb = MethodBinder.MakeBinder(this, targets[0].Name, targets);
            MetaObject[] selfArgs = args;
            BindingTarget target = mb.MakeBindingTarget(CallTypes.None, selfArgs);

            if (target.Success) {
                return new MetaObject(
                    Ast.Not(target.MakeExpression()),
                    Restrictions.Combine(target.RestrictedArguments)
                );
            }

            return null;
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
            Assert.NotNull(expression, type);

            if (expression.Type != type) {
                return ConvertExpression(expression, type, ConversionResultKind.ExplicitCast, Ast.Constant(null, typeof(CodeContext)));
            }
            return expression;
        }

        private MethodInfo[] GetApplicableMembers(Type t, OperatorInfo info) {
            Assert.NotNull(t, info);

            OldDoOperationAction act = OldDoOperationAction.Make(this, info.Operator);

            MemberGroup members = GetMember(act, t, info.Name);
            if (members.Count == 0 && info.AlternateName != null) {
                members = GetMember(act, t, info.AlternateName);
            }

            // filter down to just methods
            return FilterNonMethods(t, members);
        }

        /// <summary>
        /// Gets alternate members which are specially recognized by the DLR for specific types when
        /// all other member lookup fails.
        /// </summary>
        private static MethodInfo[] GetFallbackMembers(Type t, OperatorInfo info, MetaObject[] args, out Restrictions restrictions) {
            // if we have an event we need to make a strongly-typed event handler

            // TODO: Events, we need to look in the args and pull out the real values

            if (t == typeof(EventTracker)) {
                EventTracker et = ((EventTracker)args[0].Value);
                if (info.Operator == Operators.InPlaceAdd) {
                    restrictions = GetFallbackRestrictions(t, et, args[0]);
                    return new MethodInfo[] { typeof(BinderOps).GetMethod("EventTrackerInPlaceAdd").MakeGenericMethod(et.Event.EventHandlerType) };
                } else if (info.Operator == Operators.InPlaceSubtract) {
                    restrictions = GetFallbackRestrictions(t, et, args[0]);
                    return new MethodInfo[] { typeof(BinderOps).GetMethod("EventTrackerInPlaceRemove").MakeGenericMethod(et.Event.EventHandlerType) };
                }
            } else if (t == typeof(BoundMemberTracker)) {
                BoundMemberTracker bmt = ((BoundMemberTracker)args[0].Value);
                if (bmt.BoundTo.MemberType == TrackerTypes.Event) {
                    EventTracker et = ((EventTracker)bmt.BoundTo);

                    if (info.Operator == Operators.InPlaceAdd) {
                        restrictions = GetFallbackRestrictions(t, et, args[0]);
                        return new MethodInfo[] { typeof(BinderOps).GetMethod("BoundEventTrackerInPlaceAdd").MakeGenericMethod(et.Event.EventHandlerType) };
                    } else if (info.Operator == Operators.InPlaceSubtract) {
                        restrictions = GetFallbackRestrictions(t, et, args[0]);
                        return new MethodInfo[] { typeof(BinderOps).GetMethod("BoundEventTrackerInPlaceRemove").MakeGenericMethod(et.Event.EventHandlerType) };
                    }
                }
            }

            restrictions = Restrictions.Empty;
            return new MethodInfo[0];
        }

        private static Restrictions GetFallbackRestrictions(Type t, EventTracker et, MetaObject self) {
            if (t == typeof(EventTracker)) {
                //
                // Test Generated:
                //   BinderOps.GetEventHandlerType(((EventTracker)args[0]).Event) == et.Event.EventHandlerType
                //
                return Restrictions.GetExpressionRestriction(
                    Ast.Equal(
                        Ast.Call(
                            typeof(BinderOps).GetMethod("GetEventHandlerType"),
                            Ast.Property(
                                Ast.Convert(
                                    self.Expression,
                                    typeof(EventTracker)
                                ),
                                typeof(EventTracker).GetProperty("Event")
                            )
                        ),
                        Ast.Constant(et.Event.EventHandlerType)
                    )
                );
            } else if (t == typeof(BoundMemberTracker)) {
                //
                // Test Generated:
                //   BinderOps.GetEventHandlerType(((EventTracker)((BoundMemberTracker)args[0]).BountTo).Event) == et.Event.EventHandlerType
                //
                return Restrictions.GetExpressionRestriction(
                    Ast.Equal(
                        Ast.Call(
                            typeof(BinderOps).GetMethod("GetEventHandlerType"),
                            Ast.Property(
                                Ast.Convert(
                                    Ast.Property(
                                        Ast.Convert(
                                            self.Expression,
                                            typeof(BoundMemberTracker)
                                        ),
                                        typeof(BoundMemberTracker).GetProperty("BoundTo")
                                    ),
                                    typeof(EventTracker)
                                ),
                                typeof(EventTracker).GetProperty("Event")
                            )
                        ),
                        Ast.Constant(et.Event.EventHandlerType)
                    )
                );
            }

            return Restrictions.Empty;
        }

        private static MethodInfo[] FilterNonMethods(Type t, MemberGroup members) {
            Assert.NotNull(t, members);

            List<MethodInfo> methods = new List<MethodInfo>(members.Count);
            foreach (MemberTracker mi in members) {
                if (mi.MemberType == TrackerTypes.Method) {
                    MethodInfo method = ((MethodTracker)mi).Method;

                    // don't call object methods for None type, but if someone added
                    // methods to null we'd call those.
                    if (method.DeclaringType != typeof(object) || t != typeof(Null)) {
                        methods.Add(method);
                    }
                }
            }

            return methods.ToArray();
        }

        #endregion
    }
}
