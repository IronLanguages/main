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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;

    public partial class DefaultBinder : ActionBinder {
        /// <summary>
        /// Builds a MetaObject for performing a member get.  Supports all built-in .NET members, the OperatorMethod 
        /// GetBoundMember, and StrongBox instances.
        /// </summary>
        /// <param name="name">
        /// The name of the member to retrieve.  This name is not processed by the DefaultBinder and
        /// is instead handed off to the GetMember API which can do name mangling, case insensitive lookups, etc...
        /// </param>
        /// <param name="target">
        /// The MetaObject from which the member is retrieved.
        /// </param>
        /// <param name="value">
        /// The value being assigned to the target member.
        /// </param>
        public DynamicMetaObject SetMember(string name, DynamicMetaObject target, DynamicMetaObject value) {
            return SetMember(name, target, value, AstUtils.Constant(null, typeof(CodeContext)));
        }

        /// <summary>
        /// Builds a MetaObject for performing a member get.  Supports all built-in .NET members, the OperatorMethod 
        /// GetBoundMember, and StrongBox instances.
        /// </summary>
        /// <param name="name">
        /// The name of the member to retrieve.  This name is not processed by the DefaultBinder and
        /// is instead handed off to the GetMember API which can do name mangling, case insensitive lookups, etc...
        /// </param>
        /// <param name="target">
        /// The MetaObject from which the member is retrieved.
        /// </param>
        /// <param name="value">
        /// The value being assigned to the target member.
        /// </param>
        /// <param name="codeContext">
        /// An expression which provides access to the CodeContext if its required for 
        /// accessing the member (e.g. for an extension property which takes CodeContext).  By default this
        /// a null CodeContext object is passed.
        /// </param>
        public DynamicMetaObject SetMember(string name, DynamicMetaObject target, DynamicMetaObject value, Expression codeContext) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(value, "value");
            ContractUtils.RequiresNotNull(codeContext, "codeContext");

            return MakeSetMemberTarget(
                new SetOrDeleteMemberInfo(name, codeContext),
                target,
                value
            );
        }

        private DynamicMetaObject MakeSetMemberTarget(SetOrDeleteMemberInfo memInfo, DynamicMetaObject target, DynamicMetaObject value) {
            Type type = target.GetLimitType().IsCOMObject ? target.Expression.Type : target.GetLimitType();
            Expression self = target.Expression;
            
            target = target.Restrict(target.GetLimitType());

            memInfo.Body.Restrictions = target.Restrictions;

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                type = ((TypeTracker)target.Value).Type;
                self = null;

                memInfo.Body.Restrictions = memInfo.Body.Restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)
                );
            }

            MakeSetMemberRule(memInfo, type, self, value);

            return memInfo.Body.GetMetaObject(target, value);
        }

        private void MakeSetMemberRule(SetOrDeleteMemberInfo memInfo, Type type, Expression self, DynamicMetaObject target) {
            if (MakeOperatorSetMemberBody(memInfo, self, target, type, "SetMember")) {
                return;
            }

            // needed for GetMember call until DynamicAction goes away
            OldDynamicAction act = OldSetMemberAction.Make(
                this,
                memInfo.Name
            );

            MemberGroup members = GetMember(act, type, memInfo.Name);

            // if lookup failed try the strong-box type if available.
            if (members.Count == 0 && typeof(IStrongBox).IsAssignableFrom(type)) {
                self = Ast.Field(AstUtils.Convert(self, type), type.GetField("Value"));
                type = type.GetGenericArguments()[0];

                members = GetMember(act, type, memInfo.Name);
            }

            Expression error;
            TrackerTypes memberTypes = GetMemberType(members, out error);
            if (error == null) {
                switch (memberTypes) {
                    case TrackerTypes.Method:
                    case TrackerTypes.TypeGroup:
                    case TrackerTypes.Type:
                    case TrackerTypes.Constructor:
                        memInfo.Body.FinishCondition(
                            MakeError(MakeReadOnlyMemberError(type, memInfo.Name))
                        );
                        break;
                    case TrackerTypes.Event:
                        memInfo.Body.FinishCondition(
                            MakeError(MakeEventValidation(members, self, target.Expression, memInfo.CodeContext))
                        );
                        break;
                    case TrackerTypes.Field:
                        MakeFieldRule(memInfo, self, target, type, members);
                        break;
                    case TrackerTypes.Property:
                        MakePropertyRule(memInfo, self, target, type, members);
                        break;
                    case TrackerTypes.Custom:
                        MakeGenericBody(memInfo, self, target, type, members[0]);
                        break;
                    case TrackerTypes.All:
                        // no match
                        if (MakeOperatorSetMemberBody(memInfo, self, target, type, "SetMemberAfter")) {
                            return;
                        }

                        memInfo.Body.FinishCondition(
                            MakeError(MakeMissingMemberError(type, memInfo.Name))
                        );
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            } else {
                memInfo.Body.FinishCondition(error);
            }
        }

        private void MakeGenericBody(SetOrDeleteMemberInfo memInfo, Expression instance, DynamicMetaObject target, Type type, MemberTracker tracker) {
            if (instance != null) {
                tracker = tracker.BindToInstance(instance);
            }

            Expression val = tracker.SetValue(memInfo.CodeContext, this, type, target.Expression);

            if (val != null) {
                memInfo.Body.FinishCondition(val);
            } else {
                memInfo.Body.FinishCondition(
                    MakeError(tracker.GetError(this))
                );
            }
        }

        private void MakePropertyRule(SetOrDeleteMemberInfo memInfo, Expression instance, DynamicMetaObject target, Type targetType, MemberGroup properties) {
            PropertyTracker info = (PropertyTracker)properties[0];

            MethodInfo setter = info.GetSetMethod(true);

            // Allow access to protected getters TODO: this should go, it supports IronPython semantics.
            if (setter != null && !setter.IsPublic && !(setter.IsFamily || setter.IsFamilyOrAssembly)) {
                if (!PrivateBinding) {
                    setter = null;
                }
            }

            if (setter != null) {
                setter = CompilerHelpers.GetCallableMethod(setter, PrivateBinding);

                if (info.IsStatic != (instance == null)) {
                    memInfo.Body.FinishCondition(
                        MakeError(
                            MakeStaticPropertyInstanceAccessError(
                                info,
                                true,
                                instance,
                                target.Expression
                            )
                        )
                    );
                } else if (info.IsStatic && info.DeclaringType != targetType) {
                    memInfo.Body.FinishCondition(
                        MakeError(
                            MakeStaticAssignFromDerivedTypeError(targetType, info, target.Expression, memInfo.CodeContext)
                        )
                    );
                } else if (setter.ContainsGenericParameters) {
                    memInfo.Body.FinishCondition(
                        MakeGenericPropertyExpression(memInfo)
                    );
                } else if (setter.IsPublic && !setter.DeclaringType.IsValueType) {
                    if (instance == null) {
                        memInfo.Body.FinishCondition(
                            AstUtils.SimpleCallHelper(
                                setter,
                                ConvertExpression(
                                    target.Expression,
                                    setter.GetParameters()[0].ParameterType,
                                    ConversionResultKind.ExplicitCast,
                                    memInfo.CodeContext
                                )
                            )
                        );
                    } else {
                        memInfo.Body.FinishCondition(
                            MakeReturnValue(
                                MakeCallExpression(memInfo.CodeContext, setter, instance, target.Expression),
                                target
                            )
                        );
                    }
                } else {
                    // TODO: Should be able to do better w/ value types.
                    memInfo.Body.FinishCondition(
                        MakeReturnValue(
                            Ast.Call(
                                AstUtils.Constant(((ReflectedPropertyTracker)info).Property), // TODO: Private binding on extension properties
                                typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object), typeof(object[]) }),
                                instance == null ? AstUtils.Constant(null) : AstUtils.Convert(instance, typeof(object)),
                                AstUtils.Convert(target.Expression, typeof(object)),
                                Ast.NewArrayInit(typeof(object))
                            ),
                            target
                        )
                    );
                }
            } else {
                memInfo.Body.FinishCondition(
                    MakeError(
                        MakeMissingMemberError(targetType, memInfo.Name)
                    )
                );
            }
        }

        private void MakeFieldRule(SetOrDeleteMemberInfo memInfo, Expression instance, DynamicMetaObject target, Type targetType, MemberGroup fields) {
            FieldTracker field = (FieldTracker)fields[0];

            // TODO: Tmp variable for target
            if (field.DeclaringType.IsGenericType && field.DeclaringType.GetGenericTypeDefinition() == typeof(StrongBox<>)) {
                // work around a CLR bug where we can't access generic fields from dynamic methods.
                Type[] generic = field.DeclaringType.GetGenericArguments();
                memInfo.Body.FinishCondition(
                    MakeReturnValue(
                        Ast.Assign(
                            Ast.Field(
                                AstUtils.Convert(instance, field.DeclaringType),
                                field.DeclaringType.GetField("Value")
                            ),
                            AstUtils.Convert(target.Expression, generic[0])
                        ),
                        target
                    )
                );
            } else if (field.IsInitOnly || field.IsLiteral) {
                memInfo.Body.FinishCondition(
                    MakeError(
                        MakeReadOnlyMemberError(targetType, memInfo.Name)
                    )
                );
            } else if (field.IsStatic && targetType != field.DeclaringType) {
                memInfo.Body.FinishCondition(
                    MakeError(
                        MakeStaticAssignFromDerivedTypeError(targetType, field, target.Expression, memInfo.CodeContext)
                    )
                );
            } else if (field.DeclaringType.IsValueType && !field.IsStatic) {
                memInfo.Body.FinishCondition(
                    Ast.Throw(
                        Ast.New(
                            typeof(ArgumentException).GetConstructor(new Type[] { typeof(string) }),
                            AstUtils.Constant("cannot assign to value types")
                        )
                    )
                );
            } else if (field.IsPublic && field.DeclaringType.IsVisible) {
                memInfo.Body.FinishCondition(
                    MakeReturnValue(
                        Ast.Assign(
                            Ast.Field(
                                field.IsStatic ?
                                    null :
                                    AstUtils.Convert(instance, field.DeclaringType),
                                field.Field
                            ),
                            ConvertExpression(target.Expression, field.FieldType, ConversionResultKind.ExplicitCast, memInfo.CodeContext)
                        ),
                        target
                    )
                );
            } else {
                memInfo.Body.FinishCondition(
                    MakeReturnValue(
                        Ast.Call(
                            AstUtils.Convert(AstUtils.Constant(field.Field), typeof(FieldInfo)),
                            typeof(FieldInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }),
                            field.IsStatic ?
                                AstUtils.Constant(null) :
                                (Expression)AstUtils.Convert(instance, typeof(object)),
                            AstUtils.Convert(target.Expression, typeof(object))
                        ),
                        target
                    )
                );
            }
        }

        private Expression MakeReturnValue(Expression expression, DynamicMetaObject target) {
            return Ast.Block(
                expression,
                target.Expression
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorSetMemberBody(SetOrDeleteMemberInfo memInfo, Expression self, DynamicMetaObject target, Type type, string name) {
            if (self != null) {
                MethodInfo setMem = GetMethod(type, name);
                if (setMem != null && setMem.IsSpecialName) {
                    ParameterExpression tmp = Ast.Variable(target.Expression.Type, "setValue");
                    memInfo.Body.AddVariable(tmp);

                    Expression call = MakeCallExpression(memInfo.CodeContext, setMem, AstUtils.Convert(self, type), AstUtils.Constant(memInfo.Name), tmp);

                    call = Ast.Block(Ast.Assign(tmp, target.Expression), call);

                    if (setMem.ReturnType == typeof(bool)) {
                        memInfo.Body.AddCondition(
                            call,
                            tmp
                        );
                    } else {
                        memInfo.Body.FinishCondition(Ast.Block(call, tmp));
                    }

                    return setMem.ReturnType != typeof(bool);
                }
            }

            return false;
        }

        private static Expression MakeGenericPropertyExpression(SetOrDeleteMemberInfo memInfo) {
            return Ast.New(
                typeof(MemberAccessException).GetConstructor(new Type[] { typeof(string) }),
                AstUtils.Constant(memInfo.Name)
            );
        }
    }
}
