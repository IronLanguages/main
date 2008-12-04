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
using System.Reflection;
using System.Dynamic.Utils;

namespace System.Dynamic {
    /// <summary>
    /// Provides a simple class that can be inherited from to create an object with dynamic behavior
    /// at runtime.  Subclasses can override the various binder methods (GetMember, SetMember, Call, etc...)
    /// to provide custom behavior that will be invoked at runtime.  
    /// 
    /// If a method is not overridden then the Dynamic object does not directly support that behavior and 
    /// the call site will determine how the binder should be performed.
    /// </summary>
    public class DynamicObject : IDynamicObject {

        /// <summary>
        /// Enables derived types to create a new instance of Dynamic.  Dynamic instances cannot be
        /// directly instantiated because they have no implementation of dynamic behavior.
        /// </summary>
        protected DynamicObject() {
        }

        #region Public Virtual APIs

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of getting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryGetMember(GetMemberBinder binder, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of setting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual bool TrySetMember(SetMemberBinder binder, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of deleting a member.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual bool TryDeleteMember(DeleteMemberBinder binder) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of calling a member
        /// in the expando.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of converting the
        /// Dynamic object to another type.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryConvert(ConvertBinder binder, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of creating an instance
        /// of the Dynamic object.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of invoking the
        /// Dynamic object.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a binary operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a unary operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a get index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryGetIndex(GetIndexBinder binder, object[] args, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a set index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing a delete index operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on member "a.b (op)=c" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryBinaryOperationOnMember(BinaryOperationOnMemberBinder binder, object value, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on index "a[i,j,k] (op)= c" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryBinaryOperationOnIndex(BinaryOperationOnIndexBinder binder, object[] indexes, object value, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on member "a.b (op)" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryUnaryOperationOnMember(UnaryOperationOnMemberBinder binder, out object result) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of
        /// performing an operation on index "a[i,j,k] (op)" operation.
        /// 
        /// When not overridden the call site requesting the binder determines the behavior.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public virtual bool TryUnaryOperationOnIndex(UnaryOperationOnIndexBinder binder, object[] indexes, out object result) {
            throw new NotSupportedException();
        }

        #endregion

        #region MetaDynamic

        private sealed class MetaDynamic : MetaObject {

            internal MetaDynamic(Expression expression, DynamicObject value)
                : base(expression, Restrictions.Empty, value) {
            }

            public override MetaObject BindGetMember(GetMemberBinder binder) {
                if (IsOverridden("TryGetMember")) {
                    return CallMethodWithResult("TryGetMember", binder, NoArgs, (e) => binder.FallbackGetMember(this, e));
                }

                return base.BindGetMember(binder);
            }

            public override MetaObject BindSetMember(SetMemberBinder binder, MetaObject value) {
                if (IsOverridden("TrySetMember")) {
                    return CallMethodReturnLast("TrySetMember", binder, GetArgs(value), (e) => binder.FallbackSetMember(this, value, e));
                }

                return base.BindSetMember(binder, value);
            }

            public override MetaObject BindDeleteMember(DeleteMemberBinder binder) {
                if (IsOverridden("TryDeleteMember")) {
                    return CallMethodNoResult("TryDeleteMember", binder, NoArgs, (e) => binder.FallbackDeleteMember(this, e));
                }

                return base.BindDeleteMember(binder);
            }

            public override MetaObject BindConvert(ConvertBinder binder) {
                if (IsOverridden("TryConvert")) {
                    return CallMethodWithResult("TryConvert", binder, NoArgs, (e) => binder.FallbackConvert(this, e));
                }

                return base.BindConvert(binder);
            }

            public override MetaObject BindInvokeMember(InvokeMemberBinder binder, MetaObject[] args) {
                if (IsOverridden("TryInvokeMember")) {
                    return CallMethodWithResult("TryInvokeMember", binder, GetArgArray(args), (e) => binder.FallbackInvokeMember(this, args, e));
                }

                return base.BindInvokeMember(binder, args);
            }


            public override MetaObject BindCreateInstance(CreateInstanceBinder binder, MetaObject[] args) {
                if (IsOverridden("TryCreateInstance")) {
                    return CallMethodWithResult("TryCreateInstance", binder, GetArgArray(args), (e) => binder.FallbackCreateInstance(this, args, e));
                }

                return base.BindCreateInstance(binder, args);
            }

            public override MetaObject BindInvoke(InvokeBinder binder, MetaObject[] args) {
                if (IsOverridden("TryInvoke")) {
                    return CallMethodWithResult("TryInvoke", binder, GetArgArray(args), (e) => binder.FallbackInvoke(this, args, e));
                }

                return base.BindInvoke(binder, args);
            }

            public override MetaObject BindBinaryOperation(BinaryOperationBinder binder, MetaObject arg) {
                if (IsOverridden("TryBinaryOperation")) {
                    return CallMethodWithResult("TryBinaryOperation", binder, GetArgs(arg), (e) => binder.FallbackBinaryOperation(this, arg, e));
                }

                return base.BindBinaryOperation(binder, arg);
            }

            public override MetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                if (IsOverridden("TryUnaryOperation")) {
                    return CallMethodWithResult("TryUnaryOperation", binder, NoArgs, (e) => binder.FallbackUnaryOperation(this, e));
                }

                return base.BindUnaryOperation(binder);
            }

            public override MetaObject BindGetIndex(GetIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("TryGetIndex")) {
                    return CallMethodWithResult("TryGetIndex", binder, GetArgArray(indexes), (e) => binder.FallbackGetIndex(this, indexes, e));
                }

                return base.BindGetIndex(binder, indexes);
            }

            public override MetaObject BindSetIndex(SetIndexBinder binder, MetaObject[] indexes, MetaObject value) {
                if (IsOverridden("TrySetIndex")) {
                    return CallMethodReturnLast("TrySetIndex", binder, GetArgArray(indexes, value), (e) => binder.FallbackSetIndex(this, indexes, value, e));
                }

                return base.BindSetIndex(binder, indexes, value);
            }

            public override MetaObject BindDeleteIndex(DeleteIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("TryDeleteIndex")) {
                    return CallMethodNoResult("TryDeleteIndex", binder, GetArgArray(indexes), (e) => binder.FallbackDeleteIndex(this, indexes, e));
                }

                return base.BindDeleteIndex(binder, indexes);
            }

            public override MetaObject BindBinaryOperationOnMember(BinaryOperationOnMemberBinder binder, MetaObject value) {
                if (IsOverridden("TryBinaryOperationOnMember")) {
                    return CallMethodWithResult("TryBinaryOperationOnMember", binder, GetArgs(value), (e) => binder.FallbackBinaryOperationOnMember(this, value, e));
                }

                return base.BindBinaryOperationOnMember(binder, value);
            }

            public override MetaObject BindBinaryOperationOnIndex(BinaryOperationOnIndexBinder binder, MetaObject[] indexes, MetaObject value) {
                if (IsOverridden("TryBinaryOperationOnIndex")) {
                    return CallMethodWithResult("TryBinaryOperationOnIndex", binder, GetArgArray(indexes, value), (e) => binder.FallbackBinaryOperationOnIndex(this, indexes, value, e));
                }

                return base.BindBinaryOperationOnIndex(binder, indexes, value);
            }


            public override MetaObject BindUnaryOperationOnMember(UnaryOperationOnMemberBinder binder) {
                if (IsOverridden("TryUnaryOperationOnMember")) {
                    return CallMethodWithResult("TryUnaryOperationOnMember", binder, NoArgs, (e) => binder.FallbackUnaryOperationOnMember(this, e));
                }

                return base.BindUnaryOperationOnMember(binder);
            }

            public override MetaObject BindUnaryOperationOnIndex(UnaryOperationOnIndexBinder binder, MetaObject[] indexes) {
                if (IsOverridden("TryUnaryOperationOnIndex")) {
                    return CallMethodWithResult("TryUnaryOperationOnIndex", binder, GetArgArray(indexes), (e) => binder.FallbackUnaryOperationOnIndex(this, indexes, e));
                }

                return base.BindUnaryOperationOnIndex(binder, indexes);
            }

            private delegate MetaObject Fallback(MetaObject errorSuggestion);

            private readonly static Expression[] NoArgs = EmptyArray<Expression>.Instance;

            private static Expression[] GetArgs(params MetaObject[] args) {
                Expression[] paramArgs = MetaObject.GetExpressions(args);

                for (int i = 0; i < paramArgs.Length; i++) {
                    paramArgs[i] = Helpers.Convert(args[i].Expression, typeof(object));
                }

                return paramArgs;
            }

            private static Expression[] GetArgArray(MetaObject[] args) {
                return new[] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
            }

            private static Expression[] GetArgArray(MetaObject[] args, MetaObject value) {
                return new[] {
                    Expression.NewArrayInit(typeof(object), GetArgs(args)),
                    Helpers.Convert(value.Expression, typeof(object))
                };
            }

            private static ConstantExpression Constant(MetaObjectBinder binder) {
                Type t = binder.GetType();
                while (!t.IsVisible) {
                    t = t.BaseType;
                }
                return Expression.Constant(binder, t);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic that returns a result
            /// </summary>
            private MetaObject CallMethodWithResult(string methodName, MetaObjectBinder binder, Expression[] args, Fallback fallback) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                MetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                // {
                //   object result;
                //   TryGetMember(payload, out result) ? result : fallbackResult
                // }
                //
                var result = Expression.Parameter(typeof(object), null);

                var callArgs = new Expression[args.Length + 2];
                Array.Copy(args, 0, callArgs, 1, args.Length);
                callArgs[0] = Constant(binder);
                callArgs[callArgs.Length - 1] = result;

                var callDynamic = new MetaObject(
                    Expression.Block(
                        new[] { result },
                        Expression.Condition(
                            Expression.Call(
                                GetLimitedSelf(),
                                typeof(DynamicObject).GetMethod(methodName),
                                callArgs
                            ),
                            result,
                            Helpers.Convert(fallbackResult.Expression, typeof(object))
                        )
                    ),
                    GetRestrictions().Merge(fallbackResult.Restrictions)
                );
                
                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }


            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic, but uses one of the arguments for
            /// the result.
            /// </summary>
            private MetaObject CallMethodReturnLast(string methodName, MetaObjectBinder binder, Expression[] args, Fallback fallback) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                MetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                // {
                //   object result;
                //   TrySetMember(payload, result = value) ? result : fallbackResult
                // }
                //

                var result = Expression.Parameter(typeof(object), null);
                var callArgs = args.AddFirst(Constant(binder));
                callArgs[args.Length] = Expression.Assign(result, callArgs[args.Length]);

                var callDynamic = new MetaObject(
                    Expression.Block(
                        new[] { result },
                        Expression.Condition(
                            Expression.Call(
                                GetLimitedSelf(),
                                typeof(DynamicObject).GetMethod(methodName),
                                callArgs
                            ),
                            result,
                            Helpers.Convert(fallbackResult.Expression, typeof(object))
                        )
                    ),
                    GetRestrictions().Merge(fallbackResult.Restrictions)
                );

                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }


            /// <summary>
            /// Helper method for generating a MetaObject which calls a
            /// specific method on Dynamic, but uses one of the arguments for
            /// the result.
            /// </summary>
            private MetaObject CallMethodNoResult(string methodName, MetaObjectBinder binder, Expression[] args, Fallback fallback) {
                //
                // First, call fallback to do default binding
                // This produces either an error or a call to a .NET member
                //
                MetaObject fallbackResult = fallback(null);

                //
                // Build a new expression like:
                //   TryDeleteMember(payload) ? null : fallbackResult
                //
                var callDynamic = new MetaObject(
                    Expression.Condition(
                        Expression.Call(
                            GetLimitedSelf(),
                            typeof(DynamicObject).GetMethod(methodName),
                            args.AddFirst(Constant(binder))
                        ),
                        Expression.Constant(null),
                        Helpers.Convert(fallbackResult.Expression, typeof(object))
                    ),
                    GetRestrictions().Merge(fallbackResult.Restrictions)
                );

                //
                // Now, call fallback again using our new MO as the error
                // When we do this, one of two things can happen:
                //   1. Binding will succeed, and it will ignore our call to
                //      the dynamic method, OR
                //   2. Binding will fail, and it will use the MO we created
                //      above.
                //
                return fallback(callDynamic);
            }

            /// <summary>
            /// Checks if the derived type has overridden the specified method.  If there is no
            /// implementation for the method provided then Dynamic falls back to the base class
            /// behavior which lets the call site determine how the binder is performed.
            /// </summary>
            private bool IsOverridden(string method) {
                var methods = Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo mi in methods) {
                    if (mi.DeclaringType != typeof(DynamicObject) && mi.GetBaseDefinition().DeclaringType == typeof(DynamicObject)) {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private Restrictions GetRestrictions() {
                Debug.Assert(Restrictions == Restrictions.Empty, "We don't merge, restrictions are always empty");

                return Restrictions.GetTypeRestriction(Expression, LimitType);
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                return Helpers.Convert(
                    Expression,
                    LimitType
                );
            }

            private new DynamicObject Value {
                get {
                    return (DynamicObject)base.Value;
                }
            }
        }

        #endregion

        #region IDynamicObject Members

        /// <summary>
        /// The provided MetaObject will dispatch to the Dynamic virtual methods.
        /// The object can be encapsulated inside of another MetaObject to
        /// provide custom behavior for individual actions.
        /// </summary>
        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return new MetaDynamic(parameter, this);
        }

        #endregion
    }
}
