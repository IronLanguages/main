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
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using System.Text;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Actions.Calls;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    
    /// <summary>
    /// Provides binding semantics for a language.  This include conversions as well as support
    /// for producing rules for actions.  These optimized rules are used for calling methods, 
    /// performing operators, and getting members using the ActionBinder's conversion semantics.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public abstract partial class DefaultBinder : ActionBinder {
        protected DefaultBinder(ScriptDomainManager manager)
            : base(manager) {
        }

        /// <summary>
        /// Produces a rule for the specified Action for the given arguments.
        /// 
        /// The default implementation can produce rules for standard .NET types.  Languages should
        /// override this and provide any custom behavior they need and fallback to the default
        /// implementation if no custom behavior is required.
        /// </summary>
        protected override void MakeRule(OldDynamicAction action, object[] args, RuleBuilder rule) {
            ContractUtils.RequiresNotNull(action, "action");
            ContractUtils.RequiresNotNull(args, "args");

            object[] extracted;
            CodeContext callerContext = ExtractCodeContext(args, out extracted);

            ContractUtils.RequiresNotNull(callerContext, "callerContext");

            switch (action.Kind) {
                case DynamicActionKind.GetMember:
                    new GetMemberBinderHelper(callerContext, (OldGetMemberAction)action, extracted, rule).MakeNewRule();
                    return;
                case DynamicActionKind.SetMember:
                    new SetMemberBinderHelper(callerContext, (OldSetMemberAction)action, extracted, rule).MakeNewRule();
                    return;
                case DynamicActionKind.DeleteMember:
                    new DeleteMemberBinderHelper(callerContext, (OldDeleteMemberAction)action, extracted, rule).MakeRule();
                    return;
                case DynamicActionKind.ConvertTo:
                    new ConvertToBinderHelper(callerContext, (OldConvertToAction)action, extracted, rule).MakeRule();
                    return;
                default:
                    throw new NotImplementedException(action.ToString());
            }
        }

        protected static CodeContext ExtractCodeContext(object[] args, out object[] extracted) {
            CodeContext cc;
            if (args.Length > 0 && (cc = args[0] as CodeContext) != null) {
                extracted = ArrayUtils.ShiftLeft(args, 1);
            } else {
                cc = null;
                extracted = args;
            }
            return cc;
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual ErrorInfo MakeUndeletableMemberError(Type type, string name) {
            return MakeReadOnlyMemberError(type, name);
        }

        /// <summary>
        /// Called when the user is accessing a protected or private member on a get.
        /// 
        /// The default implementation allows access to the fields or properties using reflection.
        /// </summary>
        public virtual ErrorInfo MakeNonPublicMemberGetError(Expression codeContext, MemberTracker member, Type type, Expression instance) {
            switch (member.MemberType) {
                case TrackerTypes.Field:
                    FieldTracker ft = (FieldTracker)member;

                    return ErrorInfo.FromValueNoError(
                        Ast.Call(
                            AstUtils.Convert(AstUtils.Constant(ft.Field), typeof(FieldInfo)),
                            typeof(FieldInfo).GetMethod("GetValue"),
                            AstUtils.Convert(instance, typeof(object))
                        )
                    );
                case TrackerTypes.Property:
                    PropertyTracker pt = (PropertyTracker)member;

                    return ErrorInfo.FromValueNoError(
                        MemberTracker.FromMemberInfo(pt.GetGetMethod(true)).Call(codeContext, this, instance)
                    );
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Provides a way for the binder to provide a custom error message when lookup fails.  Just
        /// doing this for the time being until we get a more robust error return mechanism.
        /// </summary>
        public virtual ErrorInfo MakeReadOnlyMemberError(Type type, string name) {
            return ErrorInfo.FromException(
                Expression.New(
                    typeof(MissingMemberException).GetConstructor(new Type[] { typeof(string) }),
                    AstUtils.Constant(name)
                )
            );
        }

        public virtual ErrorInfo MakeEventValidation(MemberGroup members, Expression eventObject, Expression value, Expression codeContext) {
            EventTracker ev = (EventTracker)members[0];

            // handles in place addition of events - this validates the user did the right thing.
            return ErrorInfo.FromValueNoError(
                Expression.Call(
                    typeof(BinderOps).GetMethod("SetEvent"),
                    AstUtils.Constant(ev),
                    value
                )
            );
        }

        public static Expression MakeError(ErrorInfo error) {
            switch (error.Kind) {
                case ErrorInfoKind.Error:
                    // error meta objecT?
                    return error.Expression;
                case ErrorInfoKind.Exception:
                    return Expression.Throw(error.Expression);
                case ErrorInfoKind.Success:
                    return error.Expression;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static DynamicMetaObject MakeError(ErrorInfo error, BindingRestrictions restrictions) {
            return new DynamicMetaObject(MakeError(error), restrictions);
        }

        protected TrackerTypes GetMemberType(MemberGroup members, out Expression error) {
            error = null;
            TrackerTypes memberType = TrackerTypes.All;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                if (mi.MemberType != memberType) {
                    if (memberType != TrackerTypes.All) {
                        error = MakeAmbiguousMatchError(members);
                        return TrackerTypes.All;
                    }
                    memberType = mi.MemberType;
                }
            }
            return memberType;
        }

        private static Expression MakeAmbiguousMatchError(MemberGroup members) {
            StringBuilder sb = new StringBuilder();
            foreach (MemberTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.Throw(
                Ast.New(
                    typeof(AmbiguousMatchException).GetConstructor(new Type[] { typeof(string) }),
                    AstUtils.Constant(sb.ToString())
                )
            );
        }

        internal MethodInfo GetMethod(Type type, string name) {
            // declaring type takes precedence
            MethodInfo mi = type.GetMethod(name);
            if (mi != null) {
                return mi;
            }

            // then search extension types.
            Type curType = type;
            do {
                IList<Type> extTypes = GetExtensionTypes(curType);
                foreach (Type t in extTypes) {
                    MethodInfo next = t.GetMethod(name);
                    if (next != null) {
                        if (mi != null) {
                            throw new AmbiguousMatchException(String.Format("Found multiple members for {0} on type {1}", name, curType));
                        }

                        mi = next;
                    }
                }

                if (mi != null) {
                    return mi;
                }

                curType = curType.BaseType;
            } while (curType != null);

            return null;
        }
    }
}

