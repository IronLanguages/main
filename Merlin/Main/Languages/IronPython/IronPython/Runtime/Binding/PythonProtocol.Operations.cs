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
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Binding {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    static partial class PythonProtocol {
        private const string DisallowCoerce = "DisallowCoerce";

        public static DynamicMetaObject/*!*/ Operation(BinaryOperationBinder/*!*/ operation, DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion) {
            DynamicMetaObject[] args = new[] { target, arg };
            if (BindingHelpers.NeedsDeferral(args)) {
                return operation.Defer(target, arg);
            }

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(args);

            PythonOperationKind? opString = null;
            switch (operation.Operation) {
                case ExpressionType.Add: opString = PythonOperationKind.Add; break;
                case ExpressionType.And: opString = PythonOperationKind.BitwiseAnd; break;
                case ExpressionType.Divide: opString = PythonOperationKind.Divide; break;
                case ExpressionType.ExclusiveOr: opString = PythonOperationKind.ExclusiveOr; break;
                case ExpressionType.Modulo: opString = PythonOperationKind.Mod; break;
                case ExpressionType.Multiply: opString = PythonOperationKind.Multiply; break;
                case ExpressionType.Or: opString = PythonOperationKind.BitwiseOr; break;
                case ExpressionType.Power: opString = PythonOperationKind.Power; break;
                case ExpressionType.RightShift: opString = PythonOperationKind.RightShift; break;
                case ExpressionType.LeftShift: opString = PythonOperationKind.LeftShift; break;
                case ExpressionType.Subtract: opString = PythonOperationKind.Subtract; break;

                case ExpressionType.AddAssign: opString = PythonOperationKind.InPlaceAdd; break;
                case ExpressionType.AndAssign: opString = PythonOperationKind.InPlaceBitwiseAnd; break;
                case ExpressionType.DivideAssign: opString = PythonOperationKind.InPlaceDivide; break;
                case ExpressionType.ExclusiveOrAssign: opString = PythonOperationKind.InPlaceExclusiveOr; break;
                case ExpressionType.ModuloAssign: opString = PythonOperationKind.InPlaceMod; break;
                case ExpressionType.MultiplyAssign: opString = PythonOperationKind.InPlaceMultiply; break;
                case ExpressionType.OrAssign: opString = PythonOperationKind.InPlaceBitwiseOr; break;
                case ExpressionType.PowerAssign: opString = PythonOperationKind.InPlacePower; break;
                case ExpressionType.RightShiftAssign: opString = PythonOperationKind.InPlaceRightShift; break;
                case ExpressionType.LeftShiftAssign: opString = PythonOperationKind.InPlaceLeftShift; break;
                case ExpressionType.SubtractAssign: opString = PythonOperationKind.InPlaceSubtract; break;

                case ExpressionType.Equal: opString = PythonOperationKind.Equal; break;
                case ExpressionType.GreaterThan: opString = PythonOperationKind.GreaterThan; break;
                case ExpressionType.GreaterThanOrEqual: opString = PythonOperationKind.GreaterThanOrEqual; break;
                case ExpressionType.LessThan: opString = PythonOperationKind.LessThan; break;
                case ExpressionType.LessThanOrEqual: opString = PythonOperationKind.LessThanOrEqual; break;
                case ExpressionType.NotEqual: opString = PythonOperationKind.NotEqual; break;
            }
            
            DynamicMetaObject res = null;
            if (opString != null) {
                res = MakeBinaryOperation(operation, args, opString.Value, errorSuggestion);
            } else {
                res = operation.FallbackBinaryOperation(target, arg);
            }
            
            return BindingHelpers.AddDynamicTestAndDefer(operation, AddPythonBoxing(res), args, valInfo);
        }

        public static DynamicMetaObject/*!*/ Operation(UnaryOperationBinder/*!*/ operation, DynamicMetaObject arg) {
            DynamicMetaObject[] args = new[] { arg };
            if (arg.NeedsDeferral()) {
                return operation.Defer(arg);
            }

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(args);

            DynamicMetaObject res = null;

            switch (operation.Operation) {
                case ExpressionType.UnaryPlus: 
                    res = MakeUnaryOperation(operation, arg, Symbols.Positive);
                    break;
                case ExpressionType.Negate: 
                    res = MakeUnaryOperation(operation, arg, Symbols.OperatorNegate); 
                    break;
                case ExpressionType.OnesComplement: 
                    res = MakeUnaryOperation(operation, arg, Symbols.OperatorOnesComplement); 
                    break;
                case ExpressionType.IsFalse:
                    res = MakeUnaryNotOperation(operation, arg);
                    break;
                case ExpressionType.IsTrue:
                    res = PythonProtocol.ConvertToBool(operation, arg);
                    break;
                default:
                    res = TypeError(operation, "unknown operation: " + operation.ToString(), args);
                    break;
                
            }

            return BindingHelpers.AddDynamicTestAndDefer(operation, AddPythonBoxing(res), args, valInfo);
        }

        public static DynamicMetaObject/*!*/ Index(DynamicMetaObjectBinder/*!*/ operation, PythonIndexType index, params DynamicMetaObject[] args) {
            if (BindingHelpers.NeedsDeferral(args)) {
                return operation.Defer(args);
            }

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(args[0]);

            DynamicMetaObject res = AddPythonBoxing(MakeIndexerOperation(operation, index, args));            

            return BindingHelpers.AddDynamicTestAndDefer(operation, res, args, valInfo);
        }

        public static DynamicMetaObject/*!*/ Operation(PythonOperationBinder/*!*/ operation, params DynamicMetaObject/*!*/[]/*!*/ args) {
            if (BindingHelpers.NeedsDeferral(args)) {
                return operation.Defer(args);
            }

            ValidationInfo valInfo = BindingHelpers.GetValidationInfo(args);

            DynamicMetaObject res = AddPythonBoxing(MakeOperationRule(operation, args));

            return BindingHelpers.AddDynamicTestAndDefer(operation, res, args, valInfo);
        }

        private static DynamicMetaObject AddPythonBoxing(DynamicMetaObject res) {
            if (res.Expression.Type.IsValueType) {
                // Use Python boxing rules if we're return a value type
                res = new DynamicMetaObject(
                    AstUtils.Convert(res.Expression, typeof(object)),
                    res.Restrictions
                );
            }
            return res;
        }

        private static DynamicMetaObject/*!*/ MakeOperationRule(PythonOperationBinder/*!*/ operation, DynamicMetaObject/*!*/[]/*!*/ args) {
            switch (NormalizeOperator(operation.Operation)) {
                case PythonOperationKind.Documentation:
                    return MakeDocumentationOperation(operation, args);
                case PythonOperationKind.MemberNames:
                    return MakeMemberNamesOperation(operation, args);
                case PythonOperationKind.CallSignatures:
                    return MakeCallSignatureOperation(args[0], CompilerHelpers.GetMethodTargets(args[0].Value));
                case PythonOperationKind.IsCallable:
                    return MakeIscallableOperation(operation, args);
                case PythonOperationKind.Hash:
                    return MakeHashOperation(operation, args[0]);
                case PythonOperationKind.Not:
                    return MakeUnaryNotOperation(operation, args[0]);
                case PythonOperationKind.Contains:
                    return MakeContainsOperation(operation, args);
                case PythonOperationKind.AbsoluteValue:
                    return MakeUnaryOperation(operation, args[0], Symbols.AbsoluteValue);
                case PythonOperationKind.Compare:
                    return MakeSortComparisonRule(args, operation, operation.Operation);
                case PythonOperationKind.GetEnumeratorForIteration:
                    return MakeEnumeratorOperation(operation, args[0]);
                default:
                    return MakeBinaryOperation(operation, args, operation.Operation, null);
            }
        }

        private static DynamicMetaObject MakeBinaryOperation(DynamicMetaObjectBinder operation, DynamicMetaObject/*!*/[] args, PythonOperationKind opStr, DynamicMetaObject errorSuggestion) {
            if (IsComparision(opStr)) {
                return MakeComparisonOperation(args, operation, opStr);
            }

            return MakeSimpleOperation(args, operation, opStr, errorSuggestion);
        }

        #region Unary Operations

        /// <summary>
        /// Creates a rule for the contains operator.  This is exposed via "x in y" in 
        /// IronPython.  It is implemented by calling the __contains__ method on x and
        /// passing in y.  
        /// 
        /// If a type doesn't define __contains__ but does define __getitem__ then __getitem__ is 
        /// called repeatedly in order to see if the object is there.
        /// 
        /// For normal .NET enumerables we'll walk the iterator and see if it's present.
        /// </summary>
        private static DynamicMetaObject/*!*/ MakeContainsOperation(PythonOperationBinder/*!*/ operation, DynamicMetaObject/*!*/[]/*!*/ types) {
            DynamicMetaObject res;
            // the paramteres come in backwards from how we look up __contains__, flip them.
            Debug.Assert(types.Length == 2);
            ArrayUtils.SwapLastTwo(types);

            BinderState state = BinderState.GetBinderState(operation);
            SlotOrFunction sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.Contains, types);

            if (sf.Success) {
                // just a call to __contains__
                res = sf.Target;
            } else {
                RestrictTypes(types);

                ParameterExpression curIndex = Ast.Variable(typeof(int), "count");
                sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.GetItem, types[0], new DynamicMetaObject(curIndex, BindingRestrictions.Empty));
                if (sf.Success) {
                    // defines __getitem__, need to loop over the indexes and see if we match

                    ParameterExpression getItemRes = Ast.Variable(sf.ReturnType, "getItemRes");
                    ParameterExpression containsRes = Ast.Variable(typeof(bool), "containsRes");

                    LabelTarget target = Ast.Label();
                    res = new DynamicMetaObject(
                        Ast.Block(
                            new ParameterExpression[] { curIndex, getItemRes, containsRes },
                            Utils.Loop(
                                null,                                                     // test
                                Ast.Assign(curIndex, Ast.Add(curIndex, AstUtils.Constant(1))), // increment
                                Ast.Block(                                            // body
                        // getItemRes = param0.__getitem__(curIndex)
                                    Utils.Try(
                                        Ast.Convert(
                                            Ast.Assign(
                                                getItemRes,
                                                sf.Target.Expression
                                            ),
                                            typeof(void)
                                        )
                                    ).Catch(
                        // end of indexes, return false
                                        typeof(IndexOutOfRangeException),
                                        Ast.Break(target)
                                    ),
                        // if(getItemRes == param1) return true
                                    Utils.If(
                                        Ast.Dynamic(
                                            state.BinaryOperation(
                                                ExpressionType.Equal
                                            ),
                                            typeof(bool),
                                            types[1].Expression,
                                            getItemRes
                                        ),
                                        Ast.Assign(containsRes, AstUtils.Constant(true)),
                                        Ast.Break(target)
                                    ),
                                    AstUtils.Empty()
                                ),
                                null,                                               // loop else
                                target,                                             // break label target
                                null
                            ),
                            containsRes
                        ),
                        BindingRestrictions.Combine(types)
                    );
                } else {
                    sf = SlotOrFunction.GetSlotOrFunction(state, Symbols.Iterator, types[0]);
                    if (sf.Success) {
                        // iterate using __iter__
                        res = new DynamicMetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("ContainsFromEnumerable"),
                                AstUtils.Constant(state.Context),
                                Ast.Dynamic(
                                    state.Convert(
                                        typeof(IEnumerator),
                                        ConversionResultKind.ExplicitCast
                                    ),
                                    typeof(IEnumerator),
                                    sf.Target.Expression
                                ),
                                AstUtils.Convert(types[1].Expression, typeof(object))
                            ),
                            BindingRestrictions.Combine(types)
                        );
                    } else {
                        // non-iterable object
                        res = new DynamicMetaObject(
                            Ast.Throw(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("TypeErrorForNonIterableObject"),
                                    AstUtils.Convert(
                                        types[1].Expression,
                                        typeof(object)
                                    )
                                )
                            ),
                            BindingRestrictions.Combine(types)
                        );
                    }
                }
            }

            if (res.GetLimitType() != typeof(bool) && res.GetLimitType() != typeof(void)) {
                res = new DynamicMetaObject(
                    Ast.Dynamic(
                        state.Convert(
                            typeof(bool),
                            ConversionResultKind.ExplicitCast
                        ),
                        typeof(bool),
                        res.Expression
                    ),
                    res.Restrictions
                );
            }

            return res;
        }

        private static void RestrictTypes(DynamicMetaObject/*!*/[] types) {
            for (int i = 0; i < types.Length; i++) {
                types[i] = types[i].Restrict(types[i].GetLimitType());
            }
        }

        private static DynamicMetaObject/*!*/ MakeHashOperation(PythonOperationBinder/*!*/ operation, DynamicMetaObject/*!*/ self) {
            self = self.Restrict(self.GetLimitType());

            BinderState state = BinderState.GetBinderState(operation);
            SlotOrFunction func = SlotOrFunction.GetSlotOrFunction(state, Symbols.Hash, self);
            DynamicMetaObject res = func.Target;

            if (func.ReturnType != typeof(int)) {
                if (func.ReturnType == typeof(BigInteger)) {
                    // Python 2.5 defines the result of returning a long as hashing the long
                    res = new DynamicMetaObject(
                        HashBigInt(operation, res.Expression),
                        res.Restrictions
                    );
                } else if (func.ReturnType == typeof(object)) {
                    // need to get the integer value here...
                    ParameterExpression tempVar = Ast.Parameter(typeof(object), "hashTemp");

                    res = new DynamicMetaObject(
                            Expression.Block(
                                new [] { tempVar },
                                Expression.Assign(tempVar, res.Expression),
                                Expression.Condition(
                                    Expression.TypeIs(tempVar, typeof(int)),
                                    Expression.Convert(tempVar, typeof(int)),
                                    Expression.Condition(
                                        Expression.TypeIs(tempVar, typeof(BigInteger)),
                                        HashBigInt(operation, tempVar),
                                        HashConvertToInt(state, tempVar)
                                    )
                                )
                            ),
                            res.Restrictions
                        );
                } else {
                    // need to convert unknown value to object
                    res = new DynamicMetaObject(
                        HashConvertToInt(state, res.Expression),
                        res.Restrictions
                    );
                }
            }

            return res;
        }

        private static DynamicExpression/*!*/ HashBigInt(PythonOperationBinder/*!*/ operation, Expression/*!*/ expression) {
            return Ast.Dynamic(
                operation,
                typeof(int),
                expression
            );
        }

        private static DynamicExpression/*!*/ HashConvertToInt(BinderState/*!*/ state, Expression/*!*/ expression) {
            return Ast.Dynamic(
                state.Convert(
                    typeof(int),
                    ConversionResultKind.ExplicitCast
                ),
                typeof(int),
                expression
            );
        }

        private static DynamicMetaObject MakeUnaryOperation(DynamicMetaObjectBinder binder, DynamicMetaObject self, SymbolId symbol) {
            self = self.Restrict(self.GetLimitType());

            SlotOrFunction func = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(binder), symbol, self);

            if (!func.Success) {
                // we get the error message w/ {0} so that PythonBinderHelper.TypeError formats it correctly
                return TypeError(binder, MakeUnaryOpErrorMessage(symbol, "{0}"), self);
            }

            return func.Target;
        }

        private static DynamicMetaObject MakeEnumeratorOperation(PythonOperationBinder operation, DynamicMetaObject self) {
            if (self.GetLimitType() == typeof(string)) {
                self = self.Restrict(self.GetLimitType());

                return new DynamicMetaObject(
                    Expression.Call(
                        typeof(PythonOps).GetMethod("StringEnumerator"),
                        self.Expression
                    ),
                    self.Restrictions
                );
            } else if (self.GetLimitType() == typeof(PythonDictionary)) {
                self = self.Restrict(self.GetLimitType());

                return new DynamicMetaObject(
                    Expression.Call(
                        typeof(PythonOps).GetMethod("MakeDictionaryKeyEnumerator"),
                        self.Expression
                    ),
                    self.Restrictions
                );
            } else if (self.Value is IEnumerable ||
                       typeof(IEnumerable).IsAssignableFrom(self.GetLimitType())) {
                self = self.Restrict(self.GetLimitType());

                return new DynamicMetaObject(
                    Expression.Call(
                        Expression.Convert(
                            self.Expression,
                            typeof(IEnumerable)
                        ),
                        typeof(IEnumerable).GetMethod("GetEnumerator")
                    ),
                    self.Restrictions
                );

            } else if (self.Value is IEnumerator ||                                 // check for COM object (and fast check when we have values)
                       typeof(IEnumerator).IsAssignableFrom(self.GetLimitType())) { // check if we don't have a value
                DynamicMetaObject ieres = self.Restrict(self.GetLimitType());

#if !SILVERLIGHT
                if (ComOps.IsComObject(self.Value)) {
                    ieres = new DynamicMetaObject(
                         self.Expression,
                         ieres.Restrictions.Merge(
                            BindingRestrictions.GetExpressionRestriction(
                                Ast.TypeIs(self.Expression, typeof(IEnumerator))
                            )
                        )
                    );
                }
#endif

                return ieres;
            }

            ParameterExpression tmp = Ast.Parameter(typeof(IEnumerator), "enum");
            DynamicMetaObject res = self.BindConvert(new ConversionBinder(BinderState.GetBinderState(operation), typeof(IEnumerator), ConversionResultKind.ExplicitTry));
            return new DynamicMetaObject(                
                Expression.Block(
                    new[] { tmp },
                    Ast.Condition(
                        Ast.NotEqual(
                            Ast.Assign(tmp, res.Expression),
                            AstUtils.Constant(null)
                        ),
                        tmp,
                        Ast.Call(
                            typeof(PythonOps).GetMethod("ThrowTypeErrorForBadIteration"),
                            BinderState.GetCodeContext(operation),
                            self.Expression
                        )
                    )
                ),
                res.Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakeUnaryNotOperation(DynamicMetaObjectBinder/*!*/ operation, DynamicMetaObject/*!*/ self) {
            self = self.Restrict(self.GetLimitType());

            SlotOrFunction nonzero = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.NonZero, self);
            SlotOrFunction length = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.Length, self);

            Expression notExpr;

            if (!nonzero.Success && !length.Success) {
                // always False or True for None
                notExpr = (self.GetLimitType() == typeof(DynamicNull)) ? AstUtils.Constant(true) : AstUtils.Constant(false);
            } else {
                SlotOrFunction target = nonzero.Success ? nonzero : length;

                notExpr = target.Target.Expression;

                if (nonzero.Success) {
                    // call non-zero and negate it
                    if (notExpr.Type == typeof(bool)) {
                        notExpr = Ast.Equal(notExpr, AstUtils.Constant(false));
                    } else {
                        notExpr = Ast.Call(
                            typeof(PythonOps).GetMethod("Not"),
                            AstUtils.Convert(notExpr, typeof(object))
                        );
                    }
                } else {
                    // call len, compare w/ zero
                    if (notExpr.Type == typeof(int)) {
                        notExpr = Ast.Equal(notExpr, AstUtils.Constant(0));
                    } else {
                        notExpr = Ast.Dynamic(
                            BinderState.GetBinderState(operation).Operation(
                                PythonOperationKind.Compare
                            ),
                            typeof(int),
                            notExpr,
                            AstUtils.Constant(0)
                        );
                    }
                }
            }

            return new DynamicMetaObject(
                notExpr,
                self.Restrictions.Merge(nonzero.Target.Restrictions.Merge(length.Target.Restrictions))
            );
        }


        #endregion

        #region Reflective Operations

        private static DynamicMetaObject/*!*/ MakeDocumentationOperation(PythonOperationBinder/*!*/ operation, DynamicMetaObject/*!*/[]/*!*/ args) {
            BinderState state = BinderState.GetBinderState(operation);

            return new DynamicMetaObject(
                Binders.Get(
                    BinderState.GetCodeContext(operation),
                    state,
                    typeof(string),
                    "__doc__",
                    args[0].Expression
                ),
                args[0].Restrictions
            );
        }

        private static DynamicMetaObject/*!*/ MakeMemberNamesOperation(PythonOperationBinder/*!*/ operation, DynamicMetaObject[] args) {
            DynamicMetaObject self = args[0];
            CodeContext context;
            if (args.Length > 1 && args[0].GetLimitType() == typeof(CodeContext)) {
                self = args[1];
                context = (CodeContext)args[0].Value;
            } else {
                context = BinderState.GetBinderState(operation).Context;
            }

            if (typeof(IMembersList).IsAssignableFrom(self.GetLimitType())) {
                return BinderState.GetBinderState(operation).Binder.GetMemberNames(self, BinderState.GetCodeContext(operation));
            }

            PythonType pt = DynamicHelpers.GetPythonType(self.Value);
            List<string> strNames = GetMemberNames(context, pt, self.Value);

            if (pt.IsSystemType) {
                return new DynamicMetaObject(
                    AstUtils.Constant(strNames),
                    BindingRestrictions.GetInstanceRestriction(self.Expression, self.Value).Merge(self.Restrictions)
                );
            }

            return new DynamicMetaObject(
                AstUtils.Constant(strNames),
                BindingRestrictions.GetInstanceRestriction(self.Expression, self.Value).Merge(self.Restrictions)
            );
        }

        internal static DynamicMetaObject/*!*/ MakeCallSignatureOperation(DynamicMetaObject/*!*/ self, IList<MethodBase/*!*/>/*!*/ targets) {
            List<string> arrres = new List<string>();
            foreach (MethodBase mb in targets) {
                StringBuilder res = new StringBuilder();
                string comma = "";

                Type retType = CompilerHelpers.GetReturnType(mb);
                if (retType != typeof(void)) {
                    res.Append(DynamicHelpers.GetPythonTypeFromType(retType).Name);
                    res.Append(" ");
                }

                MethodInfo mi = mb as MethodInfo;
                if (mi != null) {
                    string name;
                    NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mb.DeclaringType), mi, out name);
                    res.Append(name);
                } else {
                    res.Append(DynamicHelpers.GetPythonTypeFromType(mb.DeclaringType).Name);
                }

                res.Append("(");
                if (!CompilerHelpers.IsStatic(mb)) {
                    res.Append("self");
                    comma = ", ";
                }

                foreach (ParameterInfo pi in mb.GetParameters()) {
                    if (pi.ParameterType == typeof(CodeContext)) continue;

                    res.Append(comma);
                    res.Append(DynamicHelpers.GetPythonTypeFromType(pi.ParameterType).Name + " " + pi.Name);
                    comma = ", ";
                }
                res.Append(")");
                arrres.Add(res.ToString());
            }

            return new DynamicMetaObject(
                AstUtils.Constant(arrres.ToArray()),
                self.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(self.Expression, self.Value))
            );
        }

        private static DynamicMetaObject/*!*/ MakeIscallableOperation(PythonOperationBinder/*!*/ operation, DynamicMetaObject/*!*/[]/*!*/ args) {
            // Certain non-python types (encountered during interop) are callable, but don't have 
            // a __call__ attribute. The default base binder also checks these, but since we're overriding
            // the base binder, we check them here.
            DynamicMetaObject self = args[0];
            
            // only applies when called from a Python site
            if (typeof(Delegate).IsAssignableFrom(self.GetLimitType()) ||
                typeof(MethodGroup).IsAssignableFrom(self.GetLimitType())) {
                return new DynamicMetaObject(
                    AstUtils.Constant(true),
                    self.Restrict(self.GetLimitType()).Restrictions
                );
            }

            BinderState state = BinderState.GetBinderState(operation);
            Expression isCallable = Ast.NotEqual(
                Binders.TryGet(
                    BinderState.GetCodeContext(operation),
                    state,
                    typeof(object),
                    "__call__",
                    self.Expression
                ),
                AstUtils.Constant(OperationFailed.Value)
            );

            return new DynamicMetaObject(
                isCallable,
                self.Restrict(self.GetLimitType()).Restrictions
            );
        }

        #endregion

        #region Common Binary Operations

        private static DynamicMetaObject/*!*/ MakeSimpleOperation(DynamicMetaObject/*!*/[]/*!*/ types, DynamicMetaObjectBinder/*!*/ binder, PythonOperationKind operation, DynamicMetaObject errorSuggestion) {
            RestrictTypes(types);

            SlotOrFunction fbinder;
            SlotOrFunction rbinder;
            PythonTypeSlot fSlot;
            PythonTypeSlot rSlot;
            GetOpreatorMethods(types, operation, BinderState.GetBinderState(binder), out fbinder, out rbinder, out fSlot, out rSlot);

            return MakeBinaryOperatorResult(types, binder, operation, fbinder, rbinder, fSlot, rSlot, errorSuggestion);
        }

        private static void GetOpreatorMethods(DynamicMetaObject/*!*/[]/*!*/ types, PythonOperationKind oper, BinderState state, out SlotOrFunction fbinder, out SlotOrFunction rbinder, out PythonTypeSlot fSlot, out PythonTypeSlot rSlot) {
            oper = NormalizeOperator(oper);
            oper &= ~PythonOperationKind.InPlace;

            SymbolId op, rop;
            if (!IsReverseOperator(oper)) {
                op = Symbols.OperatorToSymbol(oper);
                rop = Symbols.OperatorToReversedSymbol(oper);
            } else {
                // coming back after coercion, just try reverse operator.
                rop = Symbols.OperatorToSymbol(oper);
                op = Symbols.OperatorToReversedSymbol(oper);
            }

            fSlot = null;
            rSlot = null;
            PythonType fParent, rParent;

            if (oper == PythonOperationKind.Multiply && 
                IsSequence(types[0]) && 
                !PythonOps.IsNonExtensibleNumericType(types[1].GetLimitType())) {
                // class M:
                //      def __rmul__(self, other):
                //          print "CALLED"
                //          return 1
                //
                // print [1,2] * M()
                //
                // in CPython this results in a successful call to __rmul__ on the type ignoring the forward
                // multiplication.  But calling the __mul__ method directly does NOT return NotImplemented like
                // one might expect.  Therefore we explicitly convert the MetaObject argument into an Index
                // for binding purposes.  That allows this to work at multiplication time but not with
                // a direct call to __mul__.

                DynamicMetaObject[] newTypes = new DynamicMetaObject[2];
                newTypes[0] = types[0];
                newTypes[1] = new DynamicMetaObject(
                    Ast.New(
                        typeof(Index).GetConstructor(new Type[] { typeof(object) }),
                        AstUtils.Convert(types[1].Expression, typeof(object))
                    ),
                    BindingRestrictions.Empty
                );
                types = newTypes;
            }

            if (!SlotOrFunction.TryGetBinder(state, types, op, SymbolId.Empty, out fbinder, out fParent)) {
                foreach (PythonType pt in MetaPythonObject.GetPythonType(types[0]).ResolutionOrder) {
                    if (pt.TryLookupSlot(state.Context, op, out fSlot)) {
                        fParent = pt;
                        break;
                    }
                }
            }

            if (!SlotOrFunction.TryGetBinder(state, types, SymbolId.Empty, rop, out rbinder, out rParent)) {
                foreach (PythonType pt in MetaPythonObject.GetPythonType(types[1]).ResolutionOrder) {
                    if (pt.TryLookupSlot(state.Context, rop, out rSlot)) {
                        rParent = pt;
                        break;
                    }
                }
            }

            if (fParent != null && (rbinder.Success || rSlot != null) && rParent != fParent && rParent.IsSubclassOf(fParent)) {
                // Python says if x + subx and subx defines __r*__ we should call r*.
                fbinder = SlotOrFunction.Empty;
                fSlot = null;
            }

            if (!fbinder.Success && !rbinder.Success && fSlot == null && rSlot == null) {
                if (op == Symbols.OperatorTrueDivide || op == Symbols.OperatorReverseTrueDivide) {
                    // true div on a type which doesn't support it, go ahead and try normal divide
                    PythonOperationKind newOp = op == Symbols.OperatorTrueDivide ? PythonOperationKind.Divide : PythonOperationKind.ReverseDivide;

                    GetOpreatorMethods(types, newOp, state, out fbinder, out rbinder, out fSlot, out rSlot);
                }
            }
        }

        private static bool IsReverseOperator(PythonOperationKind oper) {
            return (oper & PythonOperationKind.Reversed) != 0;
        }

        private static bool IsSequence(DynamicMetaObject/*!*/ metaObject) {
            if (typeof(List).IsAssignableFrom(metaObject.GetLimitType()) ||
                typeof(PythonTuple).IsAssignableFrom(metaObject.GetLimitType()) ||
                typeof(String).IsAssignableFrom(metaObject.GetLimitType())) {
                return true;
            }
            return false;
        }

        private static DynamicMetaObject/*!*/ MakeBinaryOperatorResult(DynamicMetaObject/*!*/[]/*!*/ types, DynamicMetaObjectBinder/*!*/ operation, PythonOperationKind op, SlotOrFunction/*!*/ fCand, SlotOrFunction/*!*/ rCand, PythonTypeSlot fSlot, PythonTypeSlot rSlot, DynamicMetaObject errorSuggestion) {
            Assert.NotNull(operation, fCand, rCand);

            SlotOrFunction fTarget, rTarget;
            BinderState state = BinderState.GetBinderState(operation);

            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            if ((op & PythonOperationKind.InPlace) != 0) {
                // in place operator, see if there's a specific method that handles it.
                SlotOrFunction function = SlotOrFunction.GetSlotOrFunction(BinderState.GetBinderState(operation), Symbols.OperatorToSymbol(op), types);

                // we don't do a coerce for in place operators if the lhs implements __iop__
                if (!MakeOneCompareGeneric(function, false, types, MakeCompareReturn, bodyBuilder)) {
                    // the method handles it and always returns a useful value.
                    return bodyBuilder.GetMetaObject(types);
                }
            }

            if (!SlotOrFunction.GetCombinedTargets(fCand, rCand, out fTarget, out rTarget) &&
                fSlot == null &&
                rSlot == null &&
                !ShouldCoerce(state, op, types[0], types[1], false) &&
                !ShouldCoerce(state, op, types[1], types[0], false) &&
                bodyBuilder.NoConditions) {
                return MakeRuleForNoMatch(operation, op, errorSuggestion, types);
            }

            if (ShouldCoerce(state, op, types[0], types[1], false) && 
                (op != PythonOperationKind.Mod || !MetaPythonObject.GetPythonType(types[0]).IsSubclassOf(TypeCache.String))) {
                // need to try __coerce__ first.
                DoCoerce(state, bodyBuilder, op, types, false);
            }

            if (MakeOneTarget(BinderState.GetBinderState(operation), fTarget, fSlot, bodyBuilder, false, types)) {
                if (ShouldCoerce(state, op, types[1], types[0], false)) {
                    // need to try __coerce__ on the reverse first                    
                    DoCoerce(state, bodyBuilder, op, new DynamicMetaObject[] { types[1], types[0] }, true);
                }

                if (rSlot != null) {
                    MakeSlotCall(BinderState.GetBinderState(operation), types, bodyBuilder, rSlot, true);
                    bodyBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression);
                } else if (MakeOneTarget(BinderState.GetBinderState(operation), rTarget, rSlot, bodyBuilder, false, types)) {
                    // need to fallback to throwing or coercion
                    bodyBuilder.FinishCondition(MakeBinaryThrow(operation, op, types).Expression);
                }
            }

            return bodyBuilder.GetMetaObject(types);
        }

        private static void MakeCompareReturn(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ retValue, bool isReverse) {
            if (retCondition != null) {
                bodyBuilder.AddCondition(retCondition, retValue);
            } else {
                bodyBuilder.FinishCondition(retValue);
            }
        }

        /// <summary>
        /// Delegate for finishing the comparison.   This takes in a condition and a return value and needs to update the ConditionalBuilder
        /// with the appropriate resulting body.  The condition may be null.
        /// </summary>
        private delegate void ComparisonHelper(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ retValue, bool isReverse);

        /// <summary>
        /// Helper to handle a comparison operator call.  Checks to see if the call can
        /// return NotImplemented and allows the caller to modify the expression that
        /// is ultimately returned (e.g. to turn __cmp__ into a bool after a comparison)
        /// </summary>
        private static bool MakeOneCompareGeneric(SlotOrFunction/*!*/ target, bool reverse, DynamicMetaObject/*!*/[]/*!*/ types, ComparisonHelper returner, ConditionalBuilder/*!*/ bodyBuilder) {
            if (target == SlotOrFunction.Empty || !target.Success) return true;

            ParameterExpression tmp;

            if (target.ReturnType == typeof(bool)) {
                tmp = bodyBuilder.CompareRetBool;
            } else {
                tmp = Ast.Variable(target.ReturnType, "compareRetValue");
                bodyBuilder.AddVariable(tmp);
            }

            if (target.MaybeNotImplemented) {
                Expression call = target.Target.Expression;
                Expression assign = Ast.Assign(tmp, call);

                returner(
                    bodyBuilder,
                    Ast.NotEqual(
                        assign,
                        AstUtils.Constant(PythonOps.NotImplemented)
                    ),
                    tmp,
                    reverse);
                return true;
            } else {
                returner(
                    bodyBuilder,
                    null,
                    target.Target.Expression,
                    reverse
                );
                return false;
            }
        }

        private static bool MakeOneTarget(BinderState/*!*/ state, SlotOrFunction/*!*/ target, PythonTypeSlot slotTarget, ConditionalBuilder/*!*/ bodyBuilder, bool reverse, DynamicMetaObject/*!*/[]/*!*/ types) {
            if (target == SlotOrFunction.Empty && slotTarget == null) return true;

            if (slotTarget != null) {
                MakeSlotCall(state, types, bodyBuilder, slotTarget, reverse);
                return true;
            } else if (target.MaybeNotImplemented) {
                Debug.Assert(target.ReturnType == typeof(object));

                ParameterExpression tmp = Ast.Variable(typeof(object), "slot");
                bodyBuilder.AddVariable(tmp);

                bodyBuilder.AddCondition(
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            target.Target.Expression
                        ),
                        Ast.Property(null, typeof(PythonOps).GetProperty("NotImplemented"))
                    ),
                    tmp
                );

                return true;
            } else {
                bodyBuilder.FinishCondition(target.Target.Expression);
                return false;
            }
        }

        private static void MakeSlotCall(BinderState/*!*/ state, DynamicMetaObject/*!*/[]/*!*/ types, ConditionalBuilder/*!*/ bodyBuilder, PythonTypeSlot/*!*/ slotTarget, bool reverse) {
            Debug.Assert(slotTarget != null);

            Expression self, other;
            if (reverse) {
                self = types[1].Expression;
                other = types[0].Expression;
            } else {
                self = types[0].Expression;
                other = types[1].Expression;
            }

            MakeSlotCallWorker(state, slotTarget, self, bodyBuilder, other);
        }

        private static void MakeSlotCallWorker(BinderState/*!*/ state, PythonTypeSlot/*!*/ slotTarget, Expression/*!*/ self, ConditionalBuilder/*!*/ bodyBuilder, params Expression/*!*/[]/*!*/ args) {
            // Generate:
            // 
            // SlotTryGetValue(context, slot, selfType, out callable) && (tmp=callable(args)) != NotImplemented) ?
            //      tmp :
            //      RestOfOperation
            //
            ParameterExpression callable = Ast.Variable(typeof(object), "slot");
            ParameterExpression tmp = Ast.Variable(typeof(object), "slot");

            bodyBuilder.AddCondition(
                Ast.AndAlso(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("SlotTryGetValue"),
                        AstUtils.Constant(state.Context),
                        AstUtils.Convert(Utils.WeakConstant(slotTarget), typeof(PythonTypeSlot)),
                        AstUtils.Convert(self, typeof(object)),
                        Ast.Call(
                            typeof(DynamicHelpers).GetMethod("GetPythonType"),
                            AstUtils.Convert(self, typeof(object))
                        ),
                        callable
                    ),
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            Ast.Dynamic(
                                state.Invoke(
                                    new CallSignature(args.Length)
                                ),
                                typeof(object),
                                ArrayUtils.Insert(AstUtils.Constant(state.Context), (Expression)callable, args)
                            )
                        ),
                        Ast.Property(null, typeof(PythonOps).GetProperty("NotImplemented"))
                    )
                ),
                tmp
            );
            bodyBuilder.AddVariable(callable);
            bodyBuilder.AddVariable(tmp);
        }

        private static void DoCoerce(BinderState/*!*/ state, ConditionalBuilder/*!*/ bodyBuilder, PythonOperationKind op, DynamicMetaObject/*!*/[]/*!*/ types, bool reverse) {
            DoCoerce(state, bodyBuilder, op, types, reverse, delegate(Expression e) {
                return e;
            });
        }

        /// <summary>
        /// calls __coerce__ for old-style classes and performs the operation if the coercion is successful.
        /// </summary>
        private static void DoCoerce(BinderState/*!*/ state, ConditionalBuilder/*!*/ bodyBuilder, PythonOperationKind op, DynamicMetaObject/*!*/[]/*!*/ types, bool reverse, Func<Expression, Expression> returnTransform) {
            ParameterExpression coerceResult = Ast.Variable(typeof(object), "coerceResult");
            ParameterExpression coerceTuple = Ast.Variable(typeof(PythonTuple), "coerceTuple");

            if (!bodyBuilder.TestCoercionRecursionCheck) {
                // during coercion we need to enforce recursion limits if
                // they're enabled and the rule's test needs to reflect this.                
                bodyBuilder.Restrictions = bodyBuilder.Restrictions.Merge(
                    BindingRestrictions.GetExpressionRestriction(
                        Ast.Equal(
                            Ast.Call(typeof(PythonOps).GetMethod("ShouldEnforceRecursion")),
                            AstUtils.Constant(PythonFunction.EnforceRecursion)
                        )
                    )
                );

                bodyBuilder.TestCoercionRecursionCheck = true;
            }

            // tmp = self.__coerce__(other)
            // if tmp != null && tmp != NotImplemented && (tuple = PythonOps.ValidateCoerceResult(tmp)) != null:
            //      return operation(tuple[0], tuple[1])                        
            SlotOrFunction slot = SlotOrFunction.GetSlotOrFunction(state, Symbols.Coerce, types);

            if (slot.Success) {
                bodyBuilder.AddCondition(
                    Ast.AndAlso(
                        Ast.Not(
                            Ast.TypeIs(
                                Ast.Assign(
                                    coerceResult,
                                    slot.Target.Expression
                                ),
                                typeof(OldInstance)
                            )
                        ),
                        Ast.NotEqual(
                            Ast.Assign(
                                coerceTuple,
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("ValidateCoerceResult"),
                                    coerceResult
                                )
                            ),
                            AstUtils.Constant(null)
                        )
                    ),
                    BindingHelpers.AddRecursionCheck(
                        returnTransform(
                            Ast.Dynamic(
                                state.Operation(op | PythonOperationKind.DisableCoerce),
                                typeof(object),
                                reverse ? CoerceTwo(coerceTuple) : CoerceOne(coerceTuple),
                                reverse ? CoerceOne(coerceTuple) : CoerceTwo(coerceTuple)
                            )
                        )
                    )
                );
                bodyBuilder.AddVariable(coerceResult);
                bodyBuilder.AddVariable(coerceTuple);
            }
        }

        private static MethodCallExpression/*!*/ CoerceTwo(ParameterExpression/*!*/ coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultTwo"),
                coerceTuple
            );
        }

        private static MethodCallExpression/*!*/ CoerceOne(ParameterExpression/*!*/ coerceTuple) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("GetCoerceResultOne"),
                coerceTuple
            );
        }


        #endregion

        #region Comparison Operations

        private static DynamicMetaObject/*!*/ MakeComparisonOperation(DynamicMetaObject/*!*/[]/*!*/ types, DynamicMetaObjectBinder/*!*/ operation, PythonOperationKind opString) {
            RestrictTypes(types);

            PythonOperationKind op = NormalizeOperator(opString);

            BinderState state = BinderState.GetBinderState(operation);
            Debug.Assert(types.Length == 2);
            DynamicMetaObject xType = types[0], yType = types[1];
            SymbolId opSym = Symbols.OperatorToSymbol(op);
            SymbolId ropSym = Symbols.OperatorToReversedSymbol(op);
            // reverse
            DynamicMetaObject[] rTypes = new DynamicMetaObject[] { types[1], types[0] };

            SlotOrFunction fop, rop, cmp, rcmp;
            fop = SlotOrFunction.GetSlotOrFunction(state, opSym, types);
            rop = SlotOrFunction.GetSlotOrFunction(state, ropSym, rTypes);
            cmp = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, types);
            rcmp = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, rTypes);

            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            SlotOrFunction.GetCombinedTargets(fop, rop, out fop, out rop);
            SlotOrFunction.GetCombinedTargets(cmp, rcmp, out cmp, out rcmp);

            // first try __op__ or __rop__ and return the value
            if (MakeOneCompareGeneric(fop, false, types, MakeCompareReturn, bodyBuilder)) {
                if (MakeOneCompareGeneric(rop, true, types, MakeCompareReturn, bodyBuilder)) {

                    // then try __cmp__ or __rcmp__ and compare the resulting int appropriaetly
                    if (ShouldCoerce(state, opString, xType, yType, true)) {
                        DoCoerce(state, bodyBuilder, PythonOperationKind.Compare, types, false, delegate(Expression e) {
                            return GetCompareTest(op, e, false);
                        });
                    }

                    if (MakeOneCompareGeneric(
                        cmp,
                        false,
                        types,
                        delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse) {
                            MakeCompareTest(op, builder, retCond, expr, reverse);
                        },
                        bodyBuilder)) {

                        if (ShouldCoerce(state, opString, yType, xType, true)) {
                            DoCoerce(state, bodyBuilder, PythonOperationKind.Compare, rTypes, true, delegate(Expression e) {
                                return GetCompareTest(op, e, true);
                            });
                        }

                        if (MakeOneCompareGeneric(
                            rcmp,
                            true,
                            types,
                            delegate(ConditionalBuilder builder, Expression retCond, Expression expr, bool reverse) {
                                MakeCompareTest(op, builder, retCond, expr, reverse);
                            },
                            bodyBuilder)) {
                            bodyBuilder.FinishCondition(MakeFallbackCompare(op, types));
                        }
                    }
                }
            }

            return bodyBuilder.GetMetaObject(types);
        }

        /// <summary>
        /// Makes the comparison rule which returns an int (-1, 0, 1).  TODO: Better name?
        /// </summary>
        private static DynamicMetaObject/*!*/ MakeSortComparisonRule(DynamicMetaObject/*!*/[]/*!*/ types, DynamicMetaObjectBinder/*!*/ operation, PythonOperationKind op) {
            RestrictTypes(types); 
            
            DynamicMetaObject fastPath = FastPathCompare(types);
            if (fastPath != null) {
                return fastPath;
            }

            // Python compare semantics: 
            //      if the types are the same invoke __cmp__ first.
            //      If __cmp__ is not defined or the types are different:
            //          try rich comparisons (eq, lt, gt, etc...) 
            //      If the types are not the same and rich cmp didn't work finally try __cmp__
            //      If __cmp__ isn't defined return a comparison based upon the types.
            //
            // Along the way we try both forward and reverse versions (try types[0] and then
            // try types[1] reverse version).  For these comparisons __cmp__ and __eq__ are their
            // own reversals and __gt__ is the opposite of __lt__.

            // collect all the comparison methods, most likely we won't need them all.
            DynamicMetaObject[] rTypes = new DynamicMetaObject[] { types[1], types[0] };
            SlotOrFunction cfunc, rcfunc, eqfunc, reqfunc, ltfunc, gtfunc, rltfunc, rgtfunc;

            BinderState state = BinderState.GetBinderState(operation);
            cfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, types);
            rcfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.Cmp, rTypes);
            eqfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorEquals, types);
            reqfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorEquals, rTypes);
            ltfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorLessThan, types);
            gtfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorGreaterThan, types);
            rltfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorLessThan, rTypes);
            rgtfunc = SlotOrFunction.GetSlotOrFunction(state, Symbols.OperatorGreaterThan, rTypes);

            // inspect forward and reverse versions so we can pick one or both.
            SlotOrFunction cTarget, rcTarget, eqTarget, reqTarget, ltTarget, rgtTarget, gtTarget, rltTarget;
            SlotOrFunction.GetCombinedTargets(cfunc, rcfunc, out cTarget, out rcTarget);
            SlotOrFunction.GetCombinedTargets(eqfunc, reqfunc, out eqTarget, out reqTarget);
            SlotOrFunction.GetCombinedTargets(ltfunc, rgtfunc, out ltTarget, out rgtTarget);
            SlotOrFunction.GetCombinedTargets(gtfunc, rltfunc, out gtTarget, out rltTarget);

            PythonType xType = MetaPythonObject.GetPythonType(types[0]);
            PythonType yType = MetaPythonObject.GetPythonType(types[1]);

            // now build the rule from the targets.
            // bail if we're comparing to null and the rhs can't do anything special...
            if (xType.IsNull) {
                if (yType.IsNull) {
                    return new DynamicMetaObject(
                        AstUtils.Constant(0),
                        BindingRestrictions.Combine(types)
                    );
                } else if (yType.UnderlyingSystemType.IsPrimitive || yType.UnderlyingSystemType == typeof(Microsoft.Scripting.Math.BigInteger)) {
                    return new DynamicMetaObject(
                        AstUtils.Constant(-1),
                        BindingRestrictions.Combine(types)
                    );
                }
            }

            ConditionalBuilder bodyBuilder = new ConditionalBuilder(operation);

            bool tryRich = true, more = true;
            if (xType == yType && cTarget != SlotOrFunction.Empty) {
                // if the types are equal try __cmp__ first
                if (ShouldCoerce(state, op, types[0], types[1], true)) {
                    // need to try __coerce__ first.
                    DoCoerce(state, bodyBuilder, PythonOperationKind.Compare, types, false);
                }

                more = more && MakeOneCompareGeneric(cTarget, false, types, MakeCompareReverse, bodyBuilder);

                if (xType != TypeCache.OldInstance) {
                    // try __cmp__ backwards for new-style classes and don't fallback to
                    // rich comparisons if available
                    more = more && MakeOneCompareGeneric(rcTarget, true, types, MakeCompareReverse, bodyBuilder);
                    tryRich = false;
                }
            }

            if (tryRich && more) {
                // try the >, <, ==, !=, >=, <=.  These don't get short circuited using the more logic
                // because they don't give a definitive answer even if they return bool.  Only if they
                // return true do we know to return 0, -1, or 1.
                // try eq
                MakeOneCompareGeneric(eqTarget, false, types, MakeCompareToZero, bodyBuilder);
                MakeOneCompareGeneric(reqTarget, true, types, MakeCompareToZero, bodyBuilder);

                // try less than & reverse
                MakeOneCompareGeneric(ltTarget, false, types, MakeCompareToNegativeOne, bodyBuilder);
                MakeOneCompareGeneric(rgtTarget, true, types, MakeCompareToNegativeOne, bodyBuilder);

                // try greater than & reverse
                MakeOneCompareGeneric(gtTarget, false, types, MakeCompareToOne, bodyBuilder);
                MakeOneCompareGeneric(rltTarget, true, types, MakeCompareToOne, bodyBuilder);
            }

            if (xType != yType) {
                if (more && ShouldCoerce(state, op, types[0], types[1], true)) {
                    // need to try __coerce__ first.
                    DoCoerce(state, bodyBuilder, PythonOperationKind.Compare, types, false);
                }

                more = more && MakeOneCompareGeneric(cTarget, false, types, MakeCompareReverse, bodyBuilder);

                if (more && ShouldCoerce(state, op, types[1], types[0], true)) {
                    // try __coerce__ first
                    DoCoerce(state, bodyBuilder, PythonOperationKind.Compare, rTypes, true, delegate(Expression e) {
                        return ReverseCompareValue(e);
                    });
                }

                more = more && MakeOneCompareGeneric(rcTarget, true, types, MakeCompareReverse, bodyBuilder);
            }

            if (more) {
                // fall back to compare types
                bodyBuilder.FinishCondition(MakeFallbackCompare(op, types));
            }

            return bodyBuilder.GetMetaObject(types);
        }

        private static DynamicMetaObject FastPathCompare(DynamicMetaObject/*!*/[] types) {
            if (types[0].GetLimitType() == types[1].GetLimitType()) {
                // fast paths for comparing some types which don't define __cmp__
                if (types[0].GetLimitType() == typeof(List)) {
                    types[0] = types[0].Restrict(typeof(List));
                    types[1] = types[1].Restrict(typeof(List));

                    return new DynamicMetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareLists"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        BindingRestrictions.Combine(types)
                    );
                } else if (types[0].GetLimitType() == typeof(PythonTuple)) {
                    types[0] = types[0].Restrict(typeof(PythonTuple));
                    types[1] = types[1].Restrict(typeof(PythonTuple));

                    return new DynamicMetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareTuples"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        BindingRestrictions.Combine(types)
                    );
                } else if (types[0].GetLimitType() == typeof(double)) {
                    types[0] = types[0].Restrict(typeof(double));
                    types[1] = types[1].Restrict(typeof(double));

                    return new DynamicMetaObject(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("CompareFloats"),
                            types[0].Expression,
                            types[1].Expression
                        ),
                        BindingRestrictions.Combine(types)
                    );
                }
            }
            return null;
        }

        private static void MakeCompareToZero(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(0, expr, bodyBuilder, retCondition);
        }

        private static void MakeCompareToOne(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(1, expr, bodyBuilder, retCondition);
        }

        private static void MakeCompareToNegativeOne(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            MakeValueCheck(-1, expr, bodyBuilder, retCondition);
        }

        private static void MakeValueCheck(int val, Expression retValue, ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition) {
            if (retValue.Type != typeof(bool)) {
                retValue = Ast.Dynamic(
                    BinderState.GetBinderState(bodyBuilder.Action).Convert(
                        typeof(bool),
                        ConversionResultKind.ExplicitCast
                    ),
                    typeof(bool),
                    retValue
                );
            }
            if (retCondition != null) {
                retValue = Ast.AndAlso(retCondition, retValue);
            }

            bodyBuilder.AddCondition(
                retValue,
                AstUtils.Constant(val)
            );
        }

        private static BinaryExpression/*!*/ ReverseCompareValue(Expression/*!*/ retVal) {
            return Ast.Multiply(
                AstUtils.Convert(
                    retVal,
                    typeof(int)
                ),
                AstUtils.Constant(-1)
            );
        }

        private static void MakeCompareReverse(ConditionalBuilder/*!*/ bodyBuilder, Expression retCondition, Expression/*!*/ expr, bool reverse) {
            Expression res = expr;
            if (reverse) {
                res = ReverseCompareValue(expr);
            }

            MakeCompareReturn(bodyBuilder, retCondition, res, reverse);
        }

        private static void MakeCompareTest(PythonOperationKind op, ConditionalBuilder/*!*/ bodyBuilder, Expression retCond, Expression/*!*/ expr, bool reverse) {
            MakeCompareReturn(bodyBuilder, retCond, GetCompareTest(op, expr, reverse), reverse);
        }

        private static Expression/*!*/ MakeFallbackCompare(PythonOperationKind op, DynamicMetaObject[] types) {
            return Ast.Call(
                GetComparisonFallbackMethod(op),
                AstUtils.Convert(types[0].Expression, typeof(object)),
                AstUtils.Convert(types[1].Expression, typeof(object))
            );
        }

        private static Expression GetCompareTest(PythonOperationKind op, Expression expr, bool reverse) {
            if (expr.Type == typeof(int)) {
                // fast path, just do a compare in IL
                return GetCompareNode(op, reverse, expr);
            } else {
                return GetCompareExpression(
                    op,
                    reverse,
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CompareToZero"),
                        AstUtils.Convert(expr, typeof(object))
                    )
                );
            }
        }

        #endregion

        #region Index Operations

        /// <summary>
        /// Python has three protocols for slicing:
        ///    Simple Slicing x[i:j]
        ///    Extended slicing x[i,j,k,...]
        ///    Long Slice x[start:stop:step]
        /// 
        /// The first maps to __*slice__ (get, set, and del).  
        ///    This takes indexes - i, j - which specify the range of elements to be
        ///    returned.  In the slice variants both i, j must be numeric data types.  
        /// The 2nd and 3rd are both __*item__.  
        ///    This receives a single index which is either a Tuple or a Slice object (which 
        ///    encapsulates the start, stop, and step values) 
        /// 
        /// This is in addition to a simple indexing x[y].
        /// 
        /// For simple slicing and long slicing Python generates Operators.*Slice.  For
        /// the extended slicing and simple indexing Python generates a Operators.*Item
        /// action.
        /// 
        /// Extended slicing maps to the normal .NET multi-parameter input.  
        /// 
        /// So our job here is to first determine if we're to call a __*slice__ method or
        /// a __*item__ method.  
        /// </summary>
        private static DynamicMetaObject/*!*/ MakeIndexerOperation(DynamicMetaObjectBinder/*!*/ operation, PythonIndexType op, DynamicMetaObject/*!*/[]/*!*/ types) {
            SymbolId item, slice;
            DynamicMetaObject indexedType = types[0].Restrict(types[0].GetLimitType());
            BinderState state = BinderState.GetBinderState(operation);
            BuiltinFunction itemFunc = null;
            PythonTypeSlot itemSlot = null;
            bool callSlice = false;
            int mandatoryArgs;

            GetIndexOperators(op, out item, out slice, out mandatoryArgs);

            if (types.Length == mandatoryArgs + 1 && IsSlice(op) && HasOnlyNumericTypes(operation, types, op == PythonIndexType.SetSlice)) {
                // two slice indexes, all int arguments, need to call __*slice__ if it exists
                callSlice = BindingHelpers.TryGetStaticFunction(state, slice, indexedType, out itemFunc);
                if (itemFunc == null || !callSlice) {
                    callSlice = MetaPythonObject.GetPythonType(indexedType).TryResolveSlot(state.Context, slice, out itemSlot);
                }
            }

            if (!callSlice) {
                // 1 slice index (simple index) or multiple slice indexes or no __*slice__, call __*item__, 
                if (!BindingHelpers.TryGetStaticFunction(state, item, indexedType, out itemFunc)) {
                    MetaPythonObject.GetPythonType(indexedType).TryResolveSlot(state.Context, item, out itemSlot);
                }
            }

            // make the Callable object which does the actual call to the function or slot
            Callable callable = Callable.MakeCallable(state, op, itemFunc, itemSlot);
            if (callable == null) {
                DynamicMetaObject[] newTypes = (DynamicMetaObject[])types.Clone();
                newTypes[0] = indexedType;
                return TypeError(operation, "'{0}' object is unsubscriptable", newTypes);
            }

            // prepare the arguments and make the builder which will
            // call __*slice__ or __*item__
            DynamicMetaObject[] args;
            IndexBuilder builder;
            if (callSlice) {
                // we're going to call a __*slice__ method, we pass the args as is.
                Debug.Assert(IsSlice(op));

                builder = new SliceBuilder(types, callable);

                // slicing is dependent upon the types of the arguments (HasNumericTypes) 
                // so we must restrict them.
                args = ConvertArgs(types);
            } else {
                // we're going to call a __*item__ method.
                builder = new ItemBuilder(types, callable);
                if (IsSlice(op)) {
                    // we need to create a new Slice object.
                    args = GetItemSliceArguments(state, op, types);
                } else {
                    // no need to restrict the arguments.  We're not
                    // a slice and so restrictions are not necessary
                    // here because it's not dependent upon our types.
                    args = (DynamicMetaObject[])types.Clone();

                    // but we do need to restrict based upon the type
                    // of object we're calling on.
                    args[0] = types[0].Restrict(types[0].GetLimitType());
                }

            }

            return builder.MakeRule(state, args);
        }

        /// <summary>
        /// Helper to convert all of the arguments to their known types.
        /// </summary>
        private static DynamicMetaObject/*!*/[]/*!*/ ConvertArgs(DynamicMetaObject/*!*/[]/*!*/ types) {
            DynamicMetaObject[] res = new DynamicMetaObject[types.Length];
            for (int i = 0; i < types.Length; i++) {
                res[i] = types[i].Restrict(types[i].GetLimitType());
            }
            return res;
        }

        /// <summary>
        /// Gets the arguments that need to be provided to __*item__ when we need to pass a slice object.
        /// </summary>
        private static DynamicMetaObject/*!*/[]/*!*/ GetItemSliceArguments(BinderState state, PythonIndexType op, DynamicMetaObject/*!*/[]/*!*/ types) {
            DynamicMetaObject[] args;
            if (op == PythonIndexType.SetSlice) {
                args = new DynamicMetaObject[] { 
                    types[0].Restrict(types[0].GetLimitType()),
                    GetSetSlice(state, types), 
                    types[types.Length- 1].Restrict(types[types.Length - 1].GetLimitType())
                };
            } else {
                Debug.Assert(op == PythonIndexType.GetSlice || op == PythonIndexType.DeleteSlice);

                args = new DynamicMetaObject[] { 
                    types[0].Restrict(types[0].GetLimitType()),
                    GetGetOrDeleteSlice(state, types)
                };
            }
            return args;
        }

        /// <summary>
        /// Base class for calling indexers.  We have two subclasses that target built-in functions and user defined callable objects.
        /// 
        /// The Callable objects get handed off to ItemBuilder's which then call them with the appropriate arguments.
        /// </summary>
        abstract class Callable {
            private readonly BinderState/*!*/ _binder;
            private readonly PythonIndexType _op;

            protected Callable(BinderState/*!*/ binder, PythonIndexType op) {
                Assert.NotNull(binder);

                _binder = binder;
                _op = op;
            }

            /// <summary>
            /// Creates a new CallableObject.  If BuiltinFunction is available we'll create a BuiltinCallable otherwise
            /// we create a SlotCallable.
            /// </summary>
            public static Callable MakeCallable(BinderState/*!*/ binder, PythonIndexType op, BuiltinFunction itemFunc, PythonTypeSlot itemSlot) {
                if (itemFunc != null) {
                    // we'll call a builtin function to produce the rule
                    return new BuiltinCallable(binder, op, itemFunc);
                } else if (itemSlot != null) {
                    // we'll call a PythonTypeSlot to produce the rule
                    return new SlotCallable(binder, op, itemSlot);
                }

                return null;
            }

            /// <summary>
            /// Gets the arguments in a form that should be used for extended slicing.
            /// 
            /// Python defines that multiple tuple arguments received (x[1,2,3]) get 
            /// packed into a Tuple.  For most .NET methods we just want to expand
            /// this into the multiple index arguments.  For slots and old-instances
            /// we want to pass in the tuple
            /// </summary>
            public virtual DynamicMetaObject[] GetTupleArguments(DynamicMetaObject[] arguments) {
                if (IsSetter) {
                    if (arguments.Length == 3) {
                        // simple setter, no extended slicing, no need to pack arguments into tuple
                        return arguments;
                    }

                    // we want self, (tuple, of, args, ...), value
                    Expression[] tupleArgs = new Expression[arguments.Length - 2];
                    BindingRestrictions restrictions = BindingRestrictions.Empty;
                    for (int i = 1; i < arguments.Length - 1; i++) {
                        tupleArgs[i - 1] = AstUtils.Convert(arguments[i].Expression, typeof(object));
                        restrictions = restrictions.Merge(arguments[i].Restrictions);
                    }
                    return new DynamicMetaObject[] {
                        arguments[0],
                        new DynamicMetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTuple"),
                                Ast.NewArrayInit(typeof(object), tupleArgs)
                            ),
                            restrictions
                        ),
                        arguments[arguments.Length-1]
                    };
                } else if (arguments.Length == 2) {
                    // simple getter, no extended slicing, no need to pack arguments into tuple
                    return arguments;
                } else {
                    // we want self, (tuple, of, args, ...)
                    Expression[] tupleArgs = new Expression[arguments.Length - 1];
                    for (int i = 1; i < arguments.Length; i++) {
                        tupleArgs[i - 1] = AstUtils.Convert(arguments[i].Expression, typeof(object));
                    }
                    return new DynamicMetaObject[] {
                        arguments[0],
                        new DynamicMetaObject(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("MakeTuple"),
                                Ast.NewArrayInit(typeof(object), tupleArgs)
                            ),
                            BindingRestrictions.Combine(ArrayUtils.RemoveFirst(arguments))
                        )
                    };
                }
            }

            /// <summary>
            /// Adds the target of the call to the rule.
            /// </summary>
            public abstract DynamicMetaObject/*!*/ CompleteRuleTarget(DynamicMetaObject[] args, Func<DynamicMetaObject> customFailure);

            protected PythonBinder Binder {
                get { return _binder.Binder; }
            }

            protected BinderState BinderState {
                get { return _binder; }
            }

            protected PythonIndexType Operator {
                get { return _op; }
            }

            protected bool IsSetter {
                get { return _op == PythonIndexType.SetItem || _op == PythonIndexType.SetSlice; }
            }
        }

        /// <summary>
        /// Subclass of Callable for a built-in function.  This calls a .NET method performing
        /// the appropriate bindings.
        /// </summary>
        class BuiltinCallable : Callable {
            private readonly BuiltinFunction/*!*/ _bf;

            public BuiltinCallable(BinderState/*!*/ binder, PythonIndexType op, BuiltinFunction/*!*/ func)
                : base(binder, op) {
                Assert.NotNull(func);

                _bf = func;
            }

            public override DynamicMetaObject[] GetTupleArguments(DynamicMetaObject[] arguments) {
                if (arguments[0].GetLimitType() == typeof(OldInstance)) {
                    // old instances are special in that they take only a single parameter
                    // in their indexer but accept multiple parameters as tuples.
                    return base.GetTupleArguments(arguments);
                }
                return arguments;
            }

            public override DynamicMetaObject/*!*/ CompleteRuleTarget(DynamicMetaObject/*!*/[]/*!*/ args, Func<DynamicMetaObject> customFailure) {
                Assert.NotNull(args);
                Assert.NotNullItems(args);

                BindingTarget target;
                
                DynamicMetaObject res = Binder.CallInstanceMethod(
                    new ParameterBinderWithCodeContext(Binder, AstUtils.Constant(BinderState.Context)),
                    _bf.Targets,
                    args[0],
                    ArrayUtils.RemoveFirst(args),
                    new CallSignature(args.Length - 1),
                    BindingRestrictions.Combine(args),
                    PythonNarrowing.None,
                    PythonNarrowing.IndexOperator,
                    _bf.Name,
                    out target
                );

                if (target.Success) {
                    if (IsSetter) {
                        res = new DynamicMetaObject(
                            Ast.Block(res.Expression, args[args.Length - 1].Expression),
                            res.Restrictions
                        );
                    }
                } else if (customFailure == null || (res = customFailure()) == null) {
                    res = DefaultBinder.MakeError(Binder.MakeInvalidParametersError(target), BindingRestrictions.Combine(ConvertArgs(args)));
                }

                return res;
            }
        }

        /// <summary>
        /// Callable to a user-defined callable object.  This could be a Python function,
        /// a class defining __call__, etc...
        /// </summary>
        class SlotCallable : Callable {
            private PythonTypeSlot _slot;

            public SlotCallable(BinderState/*!*/ binder, PythonIndexType op, PythonTypeSlot slot)
                : base(binder, op) {
                _slot = slot;
            }

            public override DynamicMetaObject/*!*/ CompleteRuleTarget(DynamicMetaObject/*!*/[]/*!*/ args, Func<DynamicMetaObject> customFailure) {
                Expression callable = _slot.MakeGetExpression(
                    Binder,
                    AstUtils.Constant(BinderState.Context),
                    args[0].Expression,
                    Ast.Call(
                        typeof(DynamicHelpers).GetMethod("GetPythonType"),
                        AstUtils.Convert(args[0].Expression, typeof(object))
                    ),
                    Ast.Throw(Ast.New(typeof(InvalidOperationException)))
                );

                Expression[] exprArgs = new Expression[args.Length - 1];
                for (int i = 1; i < args.Length; i++) {
                    exprArgs[i - 1] = args[i].Expression;
                }

                Expression retVal = Ast.Dynamic(
                    BinderState.Invoke(
                        new CallSignature(exprArgs.Length)
                    ),
                    typeof(object),
                    ArrayUtils.Insert(AstUtils.Constant(BinderState.Context), (Expression)callable, exprArgs)
                );

                if (IsSetter) {
                    retVal = Ast.Block(retVal, args[args.Length - 1].Expression);
                }

                return new DynamicMetaObject(
                    retVal,
                    BindingRestrictions.Combine(args)
                );
            }
        }

        /// <summary>
        /// Base class for building a __*item__ or __*slice__ call.
        /// </summary>
        abstract class IndexBuilder {
            private readonly Callable/*!*/ _callable;
            private readonly DynamicMetaObject/*!*/[]/*!*/ _types;

            public IndexBuilder(DynamicMetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable) {
                _callable = callable;
                _types = types;
            }

            public abstract DynamicMetaObject/*!*/ MakeRule(BinderState/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ args);

            protected Callable/*!*/ Callable {
                get { return _callable; }
            }

            protected DynamicMetaObject/*!*/[]/*!*/ Types {
                get { return _types; }
            }

            protected PythonType/*!*/ GetTypeAt(int index) {
                return MetaPythonObject.GetPythonType(_types[index]);
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*slice__ methods
        /// </summary>
        class SliceBuilder : IndexBuilder {
            private ParameterExpression _lengthVar;        // Nullable<int>, assigned if we need to calculate the length of the object during the call.

            public SliceBuilder(DynamicMetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable)
                : base(types, callable) {
            }

            public override DynamicMetaObject/*!*/ MakeRule(BinderState/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ args) {
                // the semantics of simple slicing state that if the value
                // is less than 0 then the length is added to it.  The default
                // for unprovided parameters are 0 and maxint.  The callee
                // is responsible for ignoring out of range values but slicing
                // is responsible for doing this initial transformation.

                Debug.Assert(args.Length > 2);  // index 1 and 2 should be our slice indexes, we might have another arg if we're a setter
                args = ArrayUtils.Copy(args);
                for (int i = 1; i < 3; i++) {
                    args[i] = args[i].Restrict(args[i].GetLimitType());

                    if (args[i].GetLimitType() == typeof(MissingParameter)) {
                        switch (i) {
                            case 1: args[i] = new DynamicMetaObject(AstUtils.Constant(0), args[i].Restrictions); break;
                            case 2: args[i] = new DynamicMetaObject(AstUtils.Constant(Int32.MaxValue), args[i].Restrictions); break;
                        }
                    } else if (args[i].GetLimitType() == typeof(int)) {
                        args[i] = MakeIntTest(args[0], args[i]);
                    } else if (args[i].GetLimitType().IsSubclassOf(typeof(Extensible<int>))) {
                        args[i] = MakeIntTest(
                            args[0],
                            new DynamicMetaObject(
                                Ast.Property(
                                    args[i].Expression,
                                    args[i].GetLimitType().GetProperty("Value")
                                ),
                                args[i].Restrictions
                            )
                        );
                    } else if (args[i].GetLimitType() == typeof(BigInteger)) {
                        args[i] = MakeBigIntTest(args[0], args[i]);
                    } else if (args[i].GetLimitType().IsSubclassOf(typeof(Extensible<BigInteger>))) {
                        args[i] = MakeBigIntTest(args[0], new DynamicMetaObject(Ast.Property(args[i].Expression, args[i].GetLimitType().GetProperty("Value")), args[i].Restrictions));
                    } else if (args[i].GetLimitType() == typeof(bool)) {
                        args[i] = new DynamicMetaObject(
                            Ast.Condition(args[i].Expression, AstUtils.Constant(1), AstUtils.Constant(0)),
                            args[i].Restrictions
                        );
                    } else {
                        // this type defines __index__, otherwise we'd have an ItemBuilder constructing a slice
                        args[i] = MakeIntTest(args[0],
                            new DynamicMetaObject(
                                Ast.Dynamic(
                                    binder.Convert(
                                        typeof(int),
                                        ConversionResultKind.ExplicitCast
                                    ),
                                    typeof(int),
                                    Ast.Dynamic(
                                        binder.InvokeNone,
                                        typeof(object),
                                        AstUtils.Constant(binder.Context),
                                        Binders.Get(
                                            AstUtils.Constant(binder.Context),
                                            binder,
                                            typeof(object),
                                            "__index__",
                                            args[i].Expression
                                        )
                                    )
                                ),                               
                                args[i].Restrictions
                            )
                        );
                    }
                }

                if (_lengthVar != null) {
                    // we need the length which we should only calculate once, calculate and
                    // store it in a temporary.  Note we only calculate the length if we'll
                    DynamicMetaObject res = Callable.CompleteRuleTarget(args, null);

                    return new DynamicMetaObject(
                        Ast.Block(
                            new ParameterExpression[] { _lengthVar },
                            Ast.Assign(_lengthVar, AstUtils.Constant(null, _lengthVar.Type)),
                            res.Expression
                        ),
                        res.Restrictions
                    );
                }

                return Callable.CompleteRuleTarget(args, null);
            }

            private DynamicMetaObject/*!*/ MakeBigIntTest(DynamicMetaObject/*!*/ self, DynamicMetaObject/*!*/ bigInt) {
                EnsureLengthVariable();

                return new DynamicMetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("NormalizeBigInteger"),
                        self.Expression,
                        bigInt.Expression,
                        _lengthVar
                    ),
                    self.Restrictions.Merge(bigInt.Restrictions)
                );
            }

            private DynamicMetaObject/*!*/ MakeIntTest(DynamicMetaObject/*!*/ self, DynamicMetaObject/*!*/ intVal) {
                return new DynamicMetaObject(
                    Ast.Condition(
                        Ast.LessThan(intVal.Expression, AstUtils.Constant(0)),
                        Ast.Add(intVal.Expression, MakeGetLength(self)),
                        intVal.Expression
                    ),
                    self.Restrictions.Merge(intVal.Restrictions)
                );
            }

            private Expression/*!*/ MakeGetLength(DynamicMetaObject /*!*/ self) {
                EnsureLengthVariable();

                return Ast.Call(
                    typeof(PythonOps).GetMethod("GetLengthOnce"),
                    self.Expression,
                    _lengthVar
                );
            }

            private void EnsureLengthVariable() {
                if (_lengthVar == null) {
                    _lengthVar = Ast.Variable(typeof(Nullable<int>), "objLength");
                }
            }
        }

        /// <summary>
        /// Derived IndexBuilder for calling __*item__ methods.
        /// </summary>
        class ItemBuilder : IndexBuilder {
            public ItemBuilder(DynamicMetaObject/*!*/[]/*!*/ types, Callable/*!*/ callable)
                : base(types, callable) {
            }

            public override DynamicMetaObject/*!*/ MakeRule(BinderState/*!*/ binder, DynamicMetaObject/*!*/[]/*!*/ args) {
                DynamicMetaObject[] tupleArgs = Callable.GetTupleArguments(args);
                return Callable.CompleteRuleTarget(tupleArgs, delegate() {
                    PythonTypeSlot indexSlot;
                    if (args[1].GetLimitType() != typeof(Slice) && GetTypeAt(1).TryResolveSlot(binder.Context, Symbols.Index, out indexSlot)) {
                        args[1] = new DynamicMetaObject(
                            Ast.Dynamic(
                                binder.InvokeNone,
                                typeof(int),
                                AstUtils.Constant(binder.Context),
                                Binders.Get(
                                    AstUtils.Constant(binder.Context),
                                    binder,
                                    typeof(object),
                                    "__index__",
                                    args[1].Expression
                                )
                            ),
                            BindingRestrictions.Empty
                        );

                        return Callable.CompleteRuleTarget(tupleArgs, null);
                    }
                    return null;
                });
            }
        }

        private static bool HasOnlyNumericTypes(DynamicMetaObjectBinder/*!*/ action, DynamicMetaObject/*!*/[]/*!*/ types, bool skipLast) {
            bool onlyNumeric = true;
            BinderState state = BinderState.GetBinderState(action);

            for (int i = 1; i < (skipLast ? types.Length - 1 : types.Length); i++) {
                DynamicMetaObject obj = types[i];
                if (!IsIndexType(state, obj)) {
                    onlyNumeric = false;
                    break;
                }
            }
            return onlyNumeric;
        }

        private static bool IsIndexType(BinderState/*!*/ state, DynamicMetaObject/*!*/ obj) {
            bool numeric = true;
            if (obj.GetLimitType() != typeof(MissingParameter) &&
                !PythonOps.IsNumericType(obj.GetLimitType())) {

                PythonType curType = MetaPythonObject.GetPythonType(obj);
                PythonTypeSlot dummy;

                if (!curType.TryResolveSlot(state.Context, Symbols.Index, out dummy)) {
                    numeric = false;
                }
            }
            return numeric;
        }

        private static bool IsSlice(PythonIndexType op) {
            return op >= PythonIndexType.GetSlice;
        }

        /// <summary>
        /// Helper to get the symbols for __*item__ and __*slice__ based upon if we're doing
        /// a get/set/delete and the minimum number of arguments required for each of those.
        /// </summary>
        private static void GetIndexOperators(PythonIndexType op, out SymbolId item, out SymbolId slice, out int mandatoryArgs) {
            switch (op) {
                case PythonIndexType.GetItem:
                case PythonIndexType.GetSlice:
                    item = Symbols.GetItem;
                    slice = Symbols.GetSlice;
                    mandatoryArgs = 2;
                    return;
                case PythonIndexType.SetItem:
                case PythonIndexType.SetSlice:
                    item = Symbols.SetItem;
                    slice = Symbols.SetSlice;
                    mandatoryArgs = 3;
                    return;
                case PythonIndexType.DeleteItem:
                case PythonIndexType.DeleteSlice:
                    item = Symbols.DelItem;
                    slice = Symbols.DeleteSlice;
                    mandatoryArgs = 2;
                    return;
            }

            throw new InvalidOperationException();
        }

        private static DynamicMetaObject/*!*/ GetSetSlice(BinderState state, DynamicMetaObject/*!*/[]/*!*/ args) {
            DynamicMetaObject[] newArgs = (DynamicMetaObject[])args.Clone();
            for (int i = 1; i < newArgs.Length; i++) {
                if (!IsIndexType(state, newArgs[i])) {
                    newArgs[i] = newArgs[i].Restrict(newArgs[i].GetLimitType());
                }
            }

            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("MakeSlice"),
                    AstUtils.Convert(GetSetParameter(newArgs, 1), typeof(object)),
                    AstUtils.Convert(GetSetParameter(newArgs, 2), typeof(object)),
                    AstUtils.Convert(GetSetParameter(newArgs, 3), typeof(object))
                ),
                BindingRestrictions.Combine(newArgs)
            );
        }

        private static DynamicMetaObject/*!*/ GetGetOrDeleteSlice(BinderState state, DynamicMetaObject/*!*/[]/*!*/ args) {
            DynamicMetaObject[] newArgs = (DynamicMetaObject[])args.Clone();
            for (int i = 1; i < newArgs.Length; i++) {
                if (!IsIndexType(state, newArgs[i])) {
                    newArgs[i] = newArgs[i].Restrict(newArgs[i].GetLimitType());
                }
            }

            return new DynamicMetaObject(
                Ast.Call(
                    typeof(PythonOps).GetMethod("MakeSlice"),
                    AstUtils.Convert(GetGetOrDeleteParameter(newArgs, 1), typeof(object)),
                    AstUtils.Convert(GetGetOrDeleteParameter(newArgs, 2), typeof(object)),
                    AstUtils.Convert(GetGetOrDeleteParameter(newArgs, 3), typeof(object))
                ),
                BindingRestrictions.Combine(newArgs)
            );
        }

        private static Expression/*!*/ GetGetOrDeleteParameter(DynamicMetaObject/*!*/[]/*!*/ args, int index) {
            if (args.Length > index) {
                return CheckMissing(args[index].Expression);
            }
            return AstUtils.Constant(null);
        }

        private static Expression GetSetParameter(DynamicMetaObject[] args, int index) {
            if (args.Length > (index + 1)) {
                return CheckMissing(args[index].Expression);
            }

            return AstUtils.Constant(null);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Checks if a coercion check should be performed.  We perform coercion under the following
        /// situations:
        ///     1. Old instances performing a binary operator (excluding rich comparisons)
        ///     2. User-defined new instances calling __cmp__ but only if we wouldn't dispatch to a built-in __coerce__ on the parent type
        ///     
        /// This matches the behavior of CPython.
        /// </summary>
        /// <returns></returns>
        private static bool ShouldCoerce(BinderState/*!*/ state, PythonOperationKind operation, DynamicMetaObject/*!*/ x, DynamicMetaObject/*!*/ y, bool isCompare) {
            if ((operation & PythonOperationKind.DisableCoerce) != 0) {
                return false;
            }

            PythonType xType = MetaPythonObject.GetPythonType(x), yType = MetaPythonObject.GetPythonType(y);

            if (xType == TypeCache.OldInstance) return true;

            if (isCompare && !xType.IsSystemType && yType.IsSystemType) {
                if (yType == TypeCache.Int32 ||
                    yType == TypeCache.BigInteger ||
                    yType == TypeCache.Double ||
                    yType == TypeCache.Complex64) {

                    // only coerce new style types that define __coerce__ and
                    // only when comparing against built-in types which
                    // define __coerce__
                    PythonTypeSlot pts;
                    if (xType.TryResolveSlot(state.Context, Symbols.Coerce, out pts)) {
                        // don't call __coerce__ if it's declared on the base type
                        BuiltinMethodDescriptor bmd = pts as BuiltinMethodDescriptor;
                        if (bmd == null) return true;

                        if (bmd.__name__ != "__coerce__" &&
                            bmd.DeclaringType != typeof(int) &&
                            bmd.DeclaringType != typeof(BigInteger) &&
                            bmd.DeclaringType != typeof(double) &&
                            bmd.DeclaringType != typeof(Complex64)) {
                            return true;
                        }

                        foreach (PythonType pt in xType.ResolutionOrder) {
                            if (pt.UnderlyingSystemType == bmd.DeclaringType) {
                                // inherited __coerce__
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static PythonOperationKind DirectOperation(PythonOperationKind op) {
            if ((op & PythonOperationKind.InPlace) == 0) {
                throw new InvalidOperationException();
            }

            return op & ~PythonOperationKind.InPlace;
        }

        private static PythonOperationKind NormalizeOperator(PythonOperationKind op) {
            if ((op & PythonOperationKind.DisableCoerce) != 0) {
                op = op & ~PythonOperationKind.DisableCoerce;
            }
            return op;
        }

        private static bool IsComparisonOperator(PythonOperationKind op) {
            return (op & PythonOperationKind.Comparison) != 0;
        }

        private static bool IsComparision(PythonOperationKind op) {
            return IsComparisonOperator(NormalizeOperator(op));
        }

        private static Expression/*!*/ GetCompareNode(PythonOperationKind op, bool reverse, Expression expr) {
            op = NormalizeOperator(op);

            switch (reverse ? OperatorToReverseOperator(op) : op) {
                case PythonOperationKind.Equal: return Ast.Equal(expr, AstUtils.Constant(0));
                case PythonOperationKind.NotEqual: return Ast.NotEqual(expr, AstUtils.Constant(0));
                case PythonOperationKind.GreaterThan: return Ast.GreaterThan(expr, AstUtils.Constant(0));
                case PythonOperationKind.GreaterThanOrEqual: return Ast.GreaterThanOrEqual(expr, AstUtils.Constant(0));
                case PythonOperationKind.LessThan: return Ast.LessThan(expr, AstUtils.Constant(0));
                case PythonOperationKind.LessThanOrEqual: return Ast.LessThanOrEqual(expr, AstUtils.Constant(0));
                default: throw new InvalidOperationException();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static PythonOperationKind OperatorToReverseOperator(PythonOperationKind op) {
            switch (op) {
                case PythonOperationKind.LessThan: return PythonOperationKind.GreaterThan;
                case PythonOperationKind.LessThanOrEqual: return PythonOperationKind.GreaterThanOrEqual;
                case PythonOperationKind.GreaterThan: return PythonOperationKind.LessThan;
                case PythonOperationKind.GreaterThanOrEqual: return PythonOperationKind.LessThanOrEqual;
                case PythonOperationKind.Equal: return PythonOperationKind.Equal;
                case PythonOperationKind.NotEqual: return PythonOperationKind.NotEqual;
                case PythonOperationKind.DivMod: return PythonOperationKind.ReverseDivMod;
                case PythonOperationKind.ReverseDivMod: return PythonOperationKind.DivMod;
                default:
                    return op & ~PythonOperationKind.Reversed;
            }
        }
        private static Expression/*!*/ GetCompareExpression(PythonOperationKind op, bool reverse, Expression/*!*/ value) {
            op = NormalizeOperator(op);

            Debug.Assert(value.Type == typeof(int));

            Expression zero = AstUtils.Constant(0);
            switch (reverse ? OperatorToReverseOperator(op) : op) {
                case PythonOperationKind.Equal: return Ast.Equal(value, zero);
                case PythonOperationKind.NotEqual: return Ast.NotEqual(value, zero);
                case PythonOperationKind.GreaterThan: return Ast.GreaterThan(value, zero); ;
                case PythonOperationKind.GreaterThanOrEqual: return Ast.GreaterThanOrEqual(value, zero);
                case PythonOperationKind.LessThan: return Ast.LessThan(value, zero);
                case PythonOperationKind.LessThanOrEqual: return Ast.LessThanOrEqual(value, zero);
                default: throw new InvalidOperationException();
            }
        }

        private static MethodInfo/*!*/ GetComparisonFallbackMethod(PythonOperationKind op) {
            op = NormalizeOperator(op);

            string name;
            switch (op) {
                case PythonOperationKind.Equal: name = "CompareTypesEqual"; break;
                case PythonOperationKind.NotEqual: name = "CompareTypesNotEqual"; break;
                case PythonOperationKind.GreaterThan: name = "CompareTypesGreaterThan"; break;
                case PythonOperationKind.LessThan: name = "CompareTypesLessThan"; break;
                case PythonOperationKind.GreaterThanOrEqual: name = "CompareTypesGreaterThanOrEqual"; break;
                case PythonOperationKind.LessThanOrEqual: name = "CompareTypesLessThanOrEqual"; break;
                case PythonOperationKind.Compare: name = "CompareTypes"; break;
                default: throw new InvalidOperationException();
            }
            return typeof(PythonOps).GetMethod(name);
        }

        internal static Expression/*!*/ CheckMissing(Expression/*!*/ toCheck) {
            if (toCheck.Type == typeof(MissingParameter)) {
                return AstUtils.Constant(null);
            }
            if (toCheck.Type != typeof(object)) {
                return toCheck;
            }

            return Ast.Condition(
                Ast.TypeIs(toCheck, typeof(MissingParameter)),
                AstUtils.Constant(null),
                toCheck
            );
        }
        
        private static DynamicMetaObject/*!*/ MakeRuleForNoMatch(DynamicMetaObjectBinder/*!*/ operation, PythonOperationKind op, DynamicMetaObject errorSuggestion, params DynamicMetaObject/*!*/[]/*!*/ types) {
            // we get the error message w/ {0}, {1} so that TypeError formats it correctly
            return errorSuggestion ?? TypeError(
                   operation,
                   MakeBinaryOpErrorMessage(op, "{0}", "{1}"),
                   types);
        }

        internal static string/*!*/ MakeUnaryOpErrorMessage(SymbolId op, string/*!*/ xType) {
            if (op == Symbols.OperatorOnesComplement) {
                return string.Format("bad operand type for unary ~: '{0}'", xType);
            } else if (op == Symbols.AbsoluteValue) {
                return string.Format("bad operand type for abs(): '{0}'", xType);
            } else if (op == Symbols.Positive) {
                return string.Format("bad operand type for unary +: '{0}'", xType);
            } else if (op == Symbols.OperatorNegate) {
                return string.Format("bad operand type for unary -: '{0}'", xType);
            }

            // unreachable
            throw new InvalidOperationException();
        }


        internal static string/*!*/ MakeBinaryOpErrorMessage(PythonOperationKind op, string/*!*/ xType, string/*!*/ yType) {
            return string.Format("unsupported operand type(s) for {2}: '{0}' and '{1}'",
                                xType, yType, GetOperatorDisplay(op));
        }

        private static string/*!*/ GetOperatorDisplay(PythonOperationKind op) {
            op = NormalizeOperator(op);

            switch (op) {
                case PythonOperationKind.Add: return "+";
                case PythonOperationKind.Subtract: return "-";
                case PythonOperationKind.Power: return "**";
                case PythonOperationKind.Multiply: return "*";
                case PythonOperationKind.FloorDivide: return "/";
                case PythonOperationKind.Divide: return "/";
                case PythonOperationKind.TrueDivide: return "//";
                case PythonOperationKind.Mod: return "%";
                case PythonOperationKind.LeftShift: return "<<";
                case PythonOperationKind.RightShift: return ">>";
                case PythonOperationKind.BitwiseAnd: return "&";
                case PythonOperationKind.BitwiseOr: return "|";
                case PythonOperationKind.ExclusiveOr: return "^";
                case PythonOperationKind.LessThan: return "<";
                case PythonOperationKind.GreaterThan: return ">";
                case PythonOperationKind.LessThanOrEqual: return "<=";
                case PythonOperationKind.GreaterThanOrEqual: return ">=";
                case PythonOperationKind.Equal: return "==";
                case PythonOperationKind.NotEqual: return "!=";
                case PythonOperationKind.LessThanGreaterThan: return "<>";
                case PythonOperationKind.InPlaceAdd: return "+=";
                case PythonOperationKind.InPlaceSubtract: return "-=";
                case PythonOperationKind.InPlacePower: return "**=";
                case PythonOperationKind.InPlaceMultiply: return "*=";
                case PythonOperationKind.InPlaceFloorDivide: return "/=";
                case PythonOperationKind.InPlaceDivide: return "/=";
                case PythonOperationKind.InPlaceTrueDivide: return "//=";
                case PythonOperationKind.InPlaceMod: return "%=";
                case PythonOperationKind.InPlaceLeftShift: return "<<=";
                case PythonOperationKind.InPlaceRightShift: return ">>=";
                case PythonOperationKind.InPlaceBitwiseAnd: return "&=";
                case PythonOperationKind.InPlaceBitwiseOr: return "|=";
                case PythonOperationKind.InPlaceExclusiveOr: return "^=";
                case PythonOperationKind.ReverseAdd: return "+";
                case PythonOperationKind.ReverseSubtract: return "-";
                case PythonOperationKind.ReversePower: return "**";
                case PythonOperationKind.ReverseMultiply: return "*";
                case PythonOperationKind.ReverseFloorDivide: return "/";
                case PythonOperationKind.ReverseDivide: return "/";
                case PythonOperationKind.ReverseTrueDivide: return "//";
                case PythonOperationKind.ReverseMod: return "%";
                case PythonOperationKind.ReverseLeftShift: return "<<";
                case PythonOperationKind.ReverseRightShift: return ">>";
                case PythonOperationKind.ReverseBitwiseAnd: return "&";
                case PythonOperationKind.ReverseBitwiseOr: return "|";
                case PythonOperationKind.ReverseExclusiveOr: return "^";
                default: return op.ToString();
            }
        }

        private static DynamicMetaObject/*!*/ MakeBinaryThrow(DynamicMetaObjectBinder/*!*/ action, PythonOperationKind op, DynamicMetaObject/*!*/[]/*!*/ args) {
            if (action is IPythonSite) {
                // produce the custom Python error message
                return new DynamicMetaObject(
                    Ast.Throw(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("TypeErrorForBinaryOp"),
                            AstUtils.Constant(SymbolTable.IdToString(Symbols.OperatorToSymbol(NormalizeOperator(op)))),
                            AstUtils.Convert(args[0].Expression, typeof(object)),
                            AstUtils.Convert(args[1].Expression, typeof(object))
                        )
                    ),
                    BindingRestrictions.Combine(args)
                );
            }

            // let the site produce its own error
            return GenericFallback(action, args);
        }

        private static List<string/*!*/>/*!*/ GetMemberNames(CodeContext/*!*/ context, PythonType/*!*/ pt, object value) {
            List names = pt.GetMemberNames(context, value);
            List<string> strNames = new List<string>();
            foreach (object o in names) {
                string s = o as string;
                if (s != null) {
                    strNames.Add(s);
                }
            }
            return strNames;
        }

        #endregion

        /// <summary>
        /// Produces an error message for the provided message and type names.  The error message should contain
        /// string formatting characters ({0}, {1}, etc...) for each of the type names.
        /// </summary>
        public static DynamicMetaObject/*!*/ TypeError(DynamicMetaObjectBinder/*!*/ action, string message, params DynamicMetaObject[] types) {
            if (action is IPythonSite) {
                // produce our custom errors for Python...
                Expression[] formatArgs = new Expression[types.Length + 1];
                for (int i = 1; i < formatArgs.Length; i++) {
                    formatArgs[i] = AstUtils.Constant(MetaPythonObject.GetPythonType(types[i - 1]).Name);
                }
                formatArgs[0] = AstUtils.Constant(message);
                Type[] typeArgs = CompilerHelpers.MakeRepeatedArray<Type>(typeof(object), types.Length + 1);
                typeArgs[0] = typeof(string);

                Expression error = Ast.Throw(
                    Ast.Call(
                        typeof(ScriptingRuntimeHelpers).GetMethod("SimpleTypeError"),
                        AstUtils.ComplexCallHelper(
                            typeof(String).GetMethod("Format", typeArgs),
                            formatArgs
                        )
                    )
                );

                return new DynamicMetaObject(
                    error,
                    BindingRestrictions.Combine(types)
                );
            }

            return GenericFallback(action, types);
        }

        private static DynamicMetaObject GenericFallback(DynamicMetaObjectBinder action, DynamicMetaObject[] types) {
            if (action is GetIndexBinder) {
                return ((GetIndexBinder)action).FallbackGetIndex(types[0], ArrayUtils.RemoveFirst(types));
            } else if (action is SetIndexBinder) {
                return ((SetIndexBinder)action).FallbackSetIndex(types[0], ArrayUtils.RemoveLast(ArrayUtils.RemoveFirst(types)), types[types.Length - 1]);
            } else if (action is DeleteIndexBinder) {
                return ((DeleteIndexBinder)action).FallbackDeleteIndex(types[0], ArrayUtils.RemoveFirst(types));
            } else if (action is UnaryOperationBinder) {
                return ((UnaryOperationBinder)action).FallbackUnaryOperation(types[0]);
            } else if (action is BinaryOperationBinder) {
                return ((BinaryOperationBinder)action).FallbackBinaryOperation(types[0], types[1]);
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
