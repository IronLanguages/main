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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Dynamic;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Collections.Generic;

namespace IronPython.Runtime.Binding {
    
    /// <summary>
    /// Common helpers used by the various binding logic.
    /// </summary>
    static class BindingHelpers {
        /// <summary>
        /// Trys to get the BuiltinFunction for the given name on the type of the provided MetaObject.  
        /// 
        /// Succeeds if the MetaObject is a BuiltinFunction or BuiltinMethodDescriptor.
        /// </summary>
        internal static bool TryGetStaticFunction(BinderState/*!*/ state, SymbolId op, DynamicMetaObject/*!*/ mo, out BuiltinFunction function) {
            PythonType type = MetaPythonObject.GetPythonType(mo);
            function = null;
            if (op != SymbolId.Empty) {
                PythonTypeSlot xSlot;
                object val;
                if (type.TryResolveSlot(state.Context, op, out xSlot) &&
                    xSlot.TryGetValue(state.Context, null, type, out val)) {
                    function = TryConvertToBuiltinFunction(val);
                    if (function == null) return false;
                }
            }
            return true;
        }

        internal static bool IsNoThrow(DynamicMetaObjectBinder action) {
            PythonGetMemberBinder gmb = action as PythonGetMemberBinder;
            if (gmb != null) {
                return gmb.IsNoThrow;
            }

            return false;
        }

        internal static DynamicMetaObject/*!*/ FilterShowCls(Expression/*!*/ codeContext, DynamicMetaObjectBinder/*!*/ action, DynamicMetaObject/*!*/ res, Expression/*!*/ failure) {
            if (action is IPythonSite) {
                Type resType = BindingHelpers.GetCompatibleType(res.Expression.Type, failure.Type);

                return new DynamicMetaObject(
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("IsClsVisible"),
                            codeContext
                        ),
                        AstUtils.Convert(res.Expression, resType),
                        AstUtils.Convert(failure, resType)

                    ),
                    res.Restrictions
                );
            }

            return res;
        }

        /// <summary>
        /// Gets the best CallSignature from a MetaAction.
        /// 
        /// The MetaAction should be either a Python InvokeBinder, or a DLR InvokeAction or 
        /// CreateAction.  For Python we can use a full-fidelity 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static CallSignature GetCallSignature(DynamicMetaObjectBinder/*!*/ action) {
            // Python'so own InvokeBinder which has a real sig
            PythonInvokeBinder pib = action as PythonInvokeBinder;
            if (pib != null) {
                return pib.Signature;
            }

            // DLR Invoke which has a argument array
            InvokeBinder iac = action as InvokeBinder;
            if (iac != null) {
                return ArgumentArrayToSignature(iac.Arguments);
            }

            InvokeMemberBinder cla = action as InvokeMemberBinder;
            if (cla != null) {
                return ArgumentArrayToSignature(cla.Arguments);
            }
            
            // DLR Create action which we hand off to our call code, also
            // has an argument array.
            CreateInstanceBinder ca = action as CreateInstanceBinder;
            Debug.Assert(ca != null);

            return ArgumentArrayToSignature(ca.Arguments);
        }

        public static Expression/*!*/ Invoke(Expression codeContext, BinderState/*!*/ binder, Type/*!*/ resultType, CallSignature signature, params Expression/*!*/[]/*!*/ args) {
            return Ast.Dynamic(
                binder.Invoke(
                    signature
                ),
                resultType,
                ArrayUtils.Insert(codeContext, args)
            );
        }

        /// <summary>
        /// Transforms an invoke member into a Python GetMember/Invoke.  The caller should
        /// verify that the given attribute is not resolved against a normal .NET class
        /// before calling this.  If it is a normal .NET member then a fallback InvokeMember
        /// is preferred.
        /// </summary>
        internal static DynamicMetaObject/*!*/ GenericInvokeMember(InvokeMemberBinder/*!*/ action, ValidationInfo valInfo, DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ args) {
            if (target.NeedsDeferral()) {
                return action.Defer(args);
            }

            return AddDynamicTestAndDefer(action, 
                action.FallbackInvoke(
                    new DynamicMetaObject(
                        Binders.Get(
                            BinderState.GetCodeContext(action),
                            BinderState.GetBinderState(action),
                            typeof(object),
                            action.Name,
                            target.Expression
                        ),
                        BindingRestrictions.Empty
                    ),
                    args,
                    null
                ),
                args,
                valInfo
            );
        }

        internal static bool NeedsDeferral(DynamicMetaObject[] args) {
            foreach (DynamicMetaObject mo in args) {
                if (mo.NeedsDeferral()) {
                    return true;
                }
            }
            return false;
        }

        internal static ArgumentInfo[] GetSimpleArgumentInfos(int count) {
            ArgumentInfo[] res = new ArgumentInfo[count];
            for (int i = 0; i < count; i++) {
                res[i] = Ast.PositionalArg(i);
            }

            return res;
        }

        internal static CallSignature ArgumentArrayToSignature(IList<ArgumentInfo/*!*/>/*!*/ args) {
            Argument[] ai = new Argument[args.Count];

            for (int i = 0; i < ai.Length; i++) {
                switch (args[i].ArgumentType) {
                    case ArgumentKind.Named:
                        ai[i] = new Argument(
                            ArgumentType.Named,
                            SymbolTable.StringToId(((NamedArgumentInfo)args[i]).Name)
                        );
                        break;
                    case ArgumentKind.Positional:
                        ai[i] = new Argument(ArgumentType.Simple);
                        break;
                }
            }

            return new CallSignature(ai);
        }

        internal static Type/*!*/ GetCompatibleType(/*!*/Type t, Type/*!*/ otherType) {
            if (t != otherType) {
                if (t.IsSubclassOf(otherType)) {
                    // subclass
                    t = otherType;
                } else if (otherType.IsSubclassOf(t)) {
                    // keep t
                } else {
                    // incompatible, both go to object
                    t = typeof(object);
                }
            }
            return t;
        }

        /// <summary>
        /// Determines if the type associated with the first MetaObject is a subclass of the
        /// type associated with the second MetaObject.
        /// </summary>
        internal static bool IsSubclassOf(DynamicMetaObject/*!*/ xType, DynamicMetaObject/*!*/ yType) {
            PythonType x = MetaPythonObject.GetPythonType(xType);
            PythonType y = MetaPythonObject.GetPythonType(yType);
            return x.IsSubclassOf(y);
        }
        
        private static BuiltinFunction TryConvertToBuiltinFunction(object o) {
            BuiltinMethodDescriptor md = o as BuiltinMethodDescriptor;

            if (md != null) {
                return md.Template;
            }

            return o as BuiltinFunction;
        }

        internal static DynamicMetaObject/*!*/ AddDynamicTestAndDefer(DynamicMetaObjectBinder/*!*/ operation, DynamicMetaObject/*!*/ res, DynamicMetaObject/*!*/[] args, ValidationInfo typeTest, params ParameterExpression[] temps) {
            return AddDynamicTestAndDefer(operation, res, args, typeTest, null, temps);
        }

        internal static DynamicMetaObject/*!*/ AddDynamicTestAndDefer(DynamicMetaObjectBinder/*!*/ operation, DynamicMetaObject/*!*/ res, DynamicMetaObject/*!*/[] args, ValidationInfo typeTest, Type deferType, params ParameterExpression[] temps) {
            if (typeTest != null) {
                if (typeTest.Test != null) {
                    // add the test and a validator if persent
                    Expression defer = operation.Defer(args).Expression;
                    if (deferType != null) {
                        defer = AstUtils.Convert(defer, deferType);
                    }

                    Type bestType = BindingHelpers.GetCompatibleType(defer.Type, res.Expression.Type);

                    res = new DynamicMetaObject(
                        Ast.Condition(
                            typeTest.Test,
                            AstUtils.Convert(res.Expression, bestType),
                            AstUtils.Convert(defer, bestType)
                        ),
                        res.Restrictions // ,
                        //typeTest.Validator
                    );
                } else if (typeTest.Validator != null) {
                    // just add the validator
                    res = new DynamicMetaObject(res.Expression, res.Restrictions); // , typeTest.Validator
                }
            } 
            
            if (temps.Length > 0) {
                // finally add the scoped variables
                res = new DynamicMetaObject(
                    Ast.Block(temps, res.Expression),
                    res.Restrictions,
                    null
                );
            }

            return res;
        }
        
        internal static Expression MakeTypeTests(params DynamicMetaObject/*!*/[] args) {
            Expression typeTest = null;

            for (int i = 0; i < args.Length; i++) {
                if (args[i].HasValue) {
                    IPythonObject val = args[i].Value as IPythonObject;
                    if (val != null) {
                        Expression test = CheckTypeVersion(args[i].Expression, val.PythonType.Version);

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

        internal static MethodCallExpression/*!*/ CheckTypeVersion(Expression/*!*/ tested, int version) {
            return Ast.Call(
                typeof(PythonOps).GetMethod("CheckTypeVersion"),
                AstUtils.Convert(tested, typeof(object)),
                Ast.Constant(version)
            );
        }

        internal static ValidationInfo/*!*/ GetValidationInfo(Expression/*!*/ tested, PythonType type) {
            int version = type.Version;

            return new ValidationInfo(
                Ast.Call(
                    typeof(PythonOps).GetMethod("CheckTypeVersion"),
                    AstUtils.Convert(tested, typeof(object)),
                    Ast.Constant(version)
                ),
                new PythonTypeValidator(type, version).Validate
            );
        }

        public static ValidationInfo GetValidationInfo(DynamicMetaObject metaSelf, params DynamicMetaObject[] args) {
            Func<bool> validation = null;
            Expression typeTest = null;
            if (metaSelf != null) {
                IPythonObject self = metaSelf.Value as IPythonObject;
                if (self != null) {
                    PythonType pt = self.PythonType;
                    int version = pt.Version;

                    typeTest = BindingHelpers.CheckTypeVersion(metaSelf.Expression, version);
                    validation = ValidatorAnd(validation, new PythonTypeValidator(pt, version).Validate);
                }
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].HasValue) {
                    IPythonObject val = args[i].Value as IPythonObject;
                    if (val != null) {
                        Expression test = BindingHelpers.CheckTypeVersion(args[i].Expression, val.PythonType.Version);
                        PythonType pt = val.PythonType;
                        int version = pt.Version;

                        validation = ValidatorAnd(validation, new PythonTypeValidator(pt, version).Validate);
                        
                        if (typeTest != null) {
                            typeTest = Ast.AndAlso(typeTest, test);
                        } else {
                            typeTest = test;
                        }
                    }
                }
            }

            return new ValidationInfo(typeTest, validation);
        }

        private static Func<bool> ValidatorAnd(Func<bool> self, Func<bool> other) {
            if (self == null) {
                return other;
            } else if (other == null) {
                return self;
            }

            return delegate() {
                return self() && other();
            };
        }
        
        internal class PythonTypeValidator {
            /// <summary>
            /// Weak reference to the dynamic type. Since they can be collected,
            /// we need to be able to let that happen and then disable the rule.
            /// </summary>
            private WeakReference _pythonType;

            /// <summary>
            /// Expected version of the instance's dynamic type
            /// </summary>
            private int _version;

            public PythonTypeValidator(PythonType pythonType, int version) {
                this._pythonType = new WeakReference(pythonType);
                this._version = version;
            }

            public bool Validate() {
                PythonType dt = _pythonType.Target as PythonType;
                return dt != null && dt.Version == _version;
            }
        }

        /// <summary>
        /// Adds a try/finally which enforces recursion limits around the target method.
        /// </summary>
        internal static Expression AddRecursionCheck(Expression expr) {
            if (PythonFunction.EnforceRecursion) {
                ParameterExpression tmp = Ast.Variable(expr.Type, "callres");

                expr = 
                    Ast.Block(
                        new [] { tmp },
                        AstUtils.Try(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPushFrame")),
                            Ast.Assign(tmp, expr)
                        ).Finally(
                            Ast.Call(typeof(PythonOps).GetMethod("FunctionPopFrame"))
                        ),
                        tmp
                    );
            }
            return expr;
        }

        internal static Expression CreateBinderStateExpression() {
            return AstUtils.CodeContext();
        }

        /// <summary>
        /// Helper to do fallback for Invoke's so we can handle both StandardAction and Python's 
        /// InvokeBinder.
        /// </summary>
        internal static DynamicMetaObject/*!*/ InvokeFallback(DynamicMetaObjectBinder/*!*/ action, Expression codeContext, DynamicMetaObject target, DynamicMetaObject/*!*/[]/*!*/ args) {
            InvokeBinder act = action as InvokeBinder;
            if (act != null) {
                return act.FallbackInvoke(target, args);
            }

            PythonInvokeBinder invoke = action as PythonInvokeBinder;
            if (invoke != null) {
                return invoke.Fallback(codeContext, target, args);
            }

            // unreachable, we always have one of these binders
            throw new InvalidOperationException();
        }

        internal static Expression/*!*/ TypeErrorForProtectedMember(Type/*!*/ type, string/*!*/ name) {
            Debug.Assert(!typeof(IPythonObject).IsAssignableFrom(type));

            return Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("TypeErrorForProtectedMember"),
                    Ast.Constant(type),
                    Ast.Constant(name)
                )
            );
        }

        internal static DynamicMetaObject/*!*/ TypeErrorGenericMethod(Type/*!*/ type, string/*!*/ name, BindingRestrictions/*!*/ restrictions) {
            return new DynamicMetaObject(
                Ast.Throw(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("TypeErrorForGenericMethod"),
                        Ast.Constant(type),
                        Ast.Constant(name)
                    )
                ),
                restrictions
            );
        }

        internal static bool IsDataMember(object p) {
            if (p is PythonFunction || p is BuiltinFunction || p is PythonType || p is BuiltinMethodDescriptor || p is OldClass || p is staticmethod || p is classmethod || p is Method || p is Delegate) {
                return false;
            }

            return true;
        }
    }

    internal class ValidationInfo {
        public readonly Expression Test;
        public readonly Func<bool> Validator;
        public static readonly ValidationInfo Empty = new ValidationInfo(null, null);

        public ValidationInfo(Expression test, Func<bool> validator) {
            Test = test;
            Validator = validator;
        }
    }
}
