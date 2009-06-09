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
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    public sealed class SetMemberBinderHelper : MemberBinderHelper<OldSetMemberAction> {
        private bool _isStatic;

        public SetMemberBinderHelper(CodeContext context, OldSetMemberAction action, object[] args, RuleBuilder rule)
            : base(context, action, args, rule) {
        }

        public void MakeNewRule() {
            Type targetType = CompilerHelpers.GetType(Target);

            Rule.MakeTest(StrongBoxType ?? targetType);

            if (typeof(TypeTracker).IsAssignableFrom(targetType)) {
                targetType = ((TypeTracker)Target).Type;
                _isStatic = true;
                Rule.AddTest(Ast.Equal(Rule.Parameters[0], AstUtils.Constant(Arguments[0])));
            } 

            MakeSetMemberRule(targetType);
            Rule.Target = Body;
        }

        private void MakeSetMemberRule(Type type) {
            if (MakeOperatorSetMemberBody(type, "SetMember")) {
                return;
            }

            MemberGroup members = Binder.GetMember(Action, type, StringName);

            // if lookup failed try the strong-box type if available.
            if (members.Count == 0 && StrongBoxType != null) {
                type = StrongBoxType;
                StrongBoxType = null;

                members = Binder.GetMember(Action, type, StringName);
            }

            Expression error;
            TrackerTypes memberTypes = GetMemberType(members, out error);
            if (error == null) {
                switch (memberTypes) {
                    case TrackerTypes.Method:
                    case TrackerTypes.TypeGroup:
                    case TrackerTypes.Type:
                    case TrackerTypes.Constructor: MakeReadOnlyMemberError(type); break;
                    case TrackerTypes.Event: AddToBody(Binder.MakeEventValidation(Rule, members).MakeErrorForRule(Rule, Binder)); break;
                    case TrackerTypes.Field: MakeFieldRule(type, members); break;
                    case TrackerTypes.Property: MakePropertyRule(type, members); break;
                    case TrackerTypes.Custom:                        
                        MakeGenericBody(type, members[0]);
                        break;
                    case TrackerTypes.All:
                        // no match
                        if (MakeOperatorSetMemberBody(type, "SetMemberAfter")) {
                            return;
                        }
                        MakeMissingMemberError(type);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            } else {
                AddToBody(Rule.MakeError(error));
            }
        }

        private void MakeGenericBody(Type type, MemberTracker tracker) {
            if (!_isStatic) {
                tracker = tracker.BindToInstance(Instance);
            }

            Expression val = tracker.SetValue(Rule.Context, Binder, type, Rule.Parameters[1]);
            Expression newBody;
            if (val != null) {
                newBody = Rule.MakeReturn(Binder, val);
            } else {
                newBody = tracker.GetError(Binder).MakeErrorForRule(Rule, Binder);
            }

            AddToBody(newBody);
        }

        private void MakePropertyRule(Type targetType, MemberGroup properties) {
            PropertyTracker info = (PropertyTracker)properties[0];

            MethodInfo setter = info.GetSetMethod(true);

            // Allow access to protected getters TODO: this should go, it supports IronPython semantics.
            if (setter != null && !setter.IsPublic && !setter.IsProtected()) {
                if (!PrivateBinding) {
                    setter = null;
                }
            }

            if (setter != null) {
                setter = CompilerHelpers.GetCallableMethod(setter, Binder.PrivateBinding);

                if (info.IsStatic != _isStatic) {
                    AddToBody(Binder.MakeStaticPropertyInstanceAccessError(info, true, Rule.Parameters).MakeErrorForRule(Rule, Binder));
                } else if(info.IsStatic && info.DeclaringType != targetType) {
                    AddToBody(Binder.MakeStaticAssignFromDerivedTypeError(targetType, info, Rule.Parameters[1], Rule.Context).MakeErrorForRule(Rule, Binder));             
                } else if (setter.ContainsGenericParameters) {
                    AddToBody(Rule.MakeError(MakeGenericPropertyExpression()));
                } else if (setter.IsPublic && !setter.DeclaringType.IsValueType) {
                    if (_isStatic) {
                        AddToBody(
                            Rule.MakeReturn(
                                Binder, 
                                AstUtils.SimpleCallHelper(
                                    setter, 
                                    Binder.ConvertExpression(
                                        Rule.Parameters[1],
                                        setter.GetParameters()[0].ParameterType,
                                        ConversionResultKind.ExplicitCast, 
                                        Rule.Context
                                    )
                                )
                            )
                        );
                    } else {
                        AddToBody(Rule.MakeReturn(Binder, MakeReturnValue(Binder.MakeCallExpression(Rule.Context, setter, Rule.Parameters))));
                    }
                } else {
                    // TODO: Should be able to do better w/ value types.
                    AddToBody(
                        Rule.MakeReturn(
                            Binder,
                            MakeReturnValue(
                                Ast.Call(
                                    AstUtils.Constant(((ReflectedPropertyTracker)info).Property), // TODO: Private binding on extension properties
                                    typeof(PropertyInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object), typeof(object[]) }),
                                    AstUtils.Convert(Instance, typeof(object)),
                                    AstUtils.Convert(Rule.Parameters[1], typeof(object)),
                                    Ast.NewArrayInit(typeof(object))
                                )
                            )
                        )
                    );
                }
            } else {
                AddToBody(Binder.MakeMissingMemberError(targetType, StringName).MakeErrorForRule(Rule, Binder));
            }
        }

        private void MakeFieldRule(Type targetType, MemberGroup fields) {
            FieldTracker field = (FieldTracker)fields[0];

            if (field.DeclaringType.IsGenericType && field.DeclaringType.GetGenericTypeDefinition() == typeof(StrongBox<>)) {
                // work around a CLR bug where we can't access generic fields from dynamic methods.
                Type[] generic = field.DeclaringType.GetGenericArguments();
                AddToBody(
                    Rule.MakeReturn(Binder,
                        MakeReturnValue(
                            Ast.Assign(
                                Ast.Field(
                                    AstUtils.Convert(Instance, field.DeclaringType),
                                    field.DeclaringType.GetField("Value")
                                ),
                                AstUtils.Convert(Rule.Parameters[1], generic[0])
                            )
                        )
                    )
                );
            } else if (field.IsInitOnly || field.IsLiteral) {
                AddToBody(Binder.MakeReadOnlyMemberError(Rule, targetType, StringName));
            } else if (field.IsStatic && targetType != field.DeclaringType) {
                AddToBody(Binder.MakeStaticAssignFromDerivedTypeError(targetType, field, Rule.Parameters[1], Rule.Context).MakeErrorForRule(Rule, Binder));
            } else if (field.DeclaringType.IsValueType && !field.IsStatic) {
                AddToBody(Rule.MakeError(Ast.New(typeof(ArgumentException).GetConstructor(new Type[] { typeof(string) }), AstUtils.Constant("cannot assign to value types"))));
            } else if (field.IsPublic && field.DeclaringType.IsVisible) {
                AddToBody(
                    Rule.MakeReturn(
                        Binder,
                        MakeReturnValue(
                            Ast.Assign(
                                Ast.Field(
                                    field.IsStatic ?
                                        null :
                                        AstUtils.Convert(Rule.Parameters[0], field.DeclaringType),
                                    field.Field
                                ),                                
                                Binder.ConvertExpression(Rule.Parameters[1], field.FieldType, ConversionResultKind.ExplicitCast, Rule.Context)
                            )
                        )
                    )
                );
            } else {
                AddToBody(
                    Rule.MakeReturn(
                        Binder,
                        MakeReturnValue(
                            Ast.Call(
                                AstUtils.Convert(AstUtils.Constant(field.Field), typeof(FieldInfo)),
                                typeof(FieldInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }),
                                field.IsStatic ?
                                    AstUtils.Constant(null) :
                                    (Expression)AstUtils.Convert(Instance, typeof(object)),
                                AstUtils.Convert(Rule.Parameters[1], typeof(object))
                            )
                        )
                    )
                );
            }
        }

        private Expression MakeReturnValue(Expression expression) {
            return Ast.Block(
                expression,
                Rule.Parameters[1]
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorSetMemberBody(Type type, string name) {
            MethodInfo setMem = GetMethod(type, name);
            if (setMem != null && setMem.IsSpecialName) {
                Expression call = Binder.MakeCallExpression(Rule.Context, setMem, Rule.Parameters[0], AstUtils.Constant(StringName), Rule.Parameters[1]);
                Expression ret;

                if (setMem.ReturnType == typeof(bool)) {
                    ret = AstUtils.If(call, Rule.MakeReturn(Binder, AstUtils.Convert(Rule.Parameters[1], typeof(object))));
                } else {
                    ret = Rule.MakeReturn(Binder, Ast.Block(call, AstUtils.Convert(Rule.Parameters[1], typeof(object))));
                }
                AddToBody(ret);
                return setMem.ReturnType != typeof(bool);
            }

            return false;
        }
    }
}
