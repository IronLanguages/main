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
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Generation;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Builds a rule for a GetMemberAction.  Supports all built-in .NET members, the OperatorMethod 
    /// GetBoundMember, and StrongBox instances.
    /// 
    /// The RuleMaker sets up it's initial state grabbing the incoming type and the type we should look members up
    /// from.  It then does the lookup for the members using the current context's ActionBinder and then binds to
    /// the returned members if the match is non-ambiguous.  
    /// 
    /// The target of the rule is built up using a series of block statements as the body.  
    /// </summary>
    public class GetMemberBinderHelper : MemberBinderHelper<OldGetMemberAction> {
        private Expression _instance;               // the expression the specifies the instance or null for rule.Parameters[0]
        private bool _isStatic;

        public GetMemberBinderHelper(CodeContext context, OldGetMemberAction action, object[] args, RuleBuilder rule)
            : base(context, action, args, rule) {
        }

        public Expression MakeMemberRuleTarget(Type instanceType, params MemberInfo[] members) {
            // This should go away w/ abstract values when it'll be easier to compose rules.
            return MakeRuleBody(instanceType, members);
        }

        public void MakeNewRule() {
            Rule.MakeTest(StrongBoxType ?? CompilerHelpers.GetType(Target));
            Rule.Target = MakeGetMemberTarget();
        }

        public Expression MakeRuleBody(Type type, params MemberInfo[] members) {
            return MakeBodyHelper(type, new MemberGroup(members));
        }

        private Expression MakeGetMemberTarget() {
            Type type = CompilerHelpers.GetType(Target);
            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                type = ((TypeTracker)Target).Type;
                _isStatic = true;
                Rule.AddTest(Ast.Equal(Rule.Parameters[0], Ast.Constant(Arguments[0])));
            }

            if (typeof(NamespaceTracker).IsAssignableFrom(type)) {
                Rule.AddTest(Ast.Equal(Rule.Parameters[0], Ast.Constant(Arguments[0])));
            }

            MemberGroup members = Binder.GetMember(Action, type, StringName);

            // if lookup failed try the strong-box type if available.
            if (members.Count == 0 && StrongBoxType != null) {
                type = StrongBoxType;
                StrongBoxType = null;

                members = Binder.GetMember(Action, type, StringName);
            }

            return MakeBodyHelper(type, members);
        }

        private Expression MakeBodyHelper(Type type, MemberGroup members) {
            if (!_isStatic) {
                MakeOperatorGetMemberBody(type, "GetCustomMember");
            }

            Expression error;
            TrackerTypes memberType = GetMemberType(members, out error);
            if (error != null) {
                AddToBody(Rule.MakeError(error));
                return Body;
            }

            switch (memberType) {
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type: MakeTypeBody(type, members); break;
                case TrackerTypes.Method:
                    // turn into a MethodGroup                    
                    MakeGenericBodyWorker(type, ReflectionCache.GetMethodGroup(StringName, members), _instance ?? Rule.Parameters[0]);
                    break;
                case TrackerTypes.Event:
                case TrackerTypes.Field:                
                case TrackerTypes.Property:
                case TrackerTypes.Constructor:
                case TrackerTypes.Custom:
                    MakeGenericBody(type, members); 
                    break;
                case TrackerTypes.All:     
                    // no members were found
                    if (!_isStatic) {
                        MakeOperatorGetMemberBody(type, "GetBoundMember");
                    }

                    MakeMissingMemberRuleForGet(type);
                    break;
                default: throw new InvalidOperationException(memberType.ToString());
            }
            return Body;
        }

        private void MakeGenericBody(Type type, MemberGroup members) {
            MakeGenericBodyWorker(type, members[0], null);
        }

        private void MakeGenericBody(Type type, MemberGroup members, Expression instance) {
            MakeGenericBodyWorker(type, members[0], instance);
        }

        private void MakeGenericBodyWorker(Type type, MemberTracker tracker, Expression instance) {
            if (!_isStatic) {
                tracker = tracker.BindToInstance(instance ?? Instance);
            }

            Expression val = tracker.GetValue(Rule.Context, Binder, type);
            Expression newBody;
            if (val != null) {
                newBody = Rule.MakeReturn(Binder, val);
            } else {
                newBody = tracker.GetError(Binder).MakeErrorForRule(Rule, Binder);
            }

            AddToBody(newBody);
        }

        private void MakeTypeBody(Type type, MemberGroup members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }

            AddToBody(Rule.MakeReturn(Binder, typeTracker.GetValue(Rule.Context, Binder, type)));
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        protected void MakeOperatorGetMemberBody(Type type, string name) {
            MethodInfo getMem = GetMethod(type, name);
            if (getMem != null && getMem.IsSpecialName) {
                ParameterExpression tmp = Rule.GetTemporary(typeof(object), "getVal");
                AddToBody(
                    AstUtils.If(
                        Ast.NotEqual(
                            Ast.Assign(
                                tmp,
                                Binder.MakeCallExpression(Rule.Context, getMem, Instance, Ast.Constant(StringName))
                            ),
                            Ast.Field(null, typeof(OperationFailed).GetField("Value"))
                        ),
                        Rule.MakeReturn(Binder, tmp)
                    )
                );
            }
        }

        /// <summary> Gets the Expression that represents the instance we're looking up </summary>
        public new Expression Instance {
            get {
                if (_instance != null) return _instance;

                return base.Instance;
            }
            protected set {
                _instance = value;
            }
        }

        private MethodInfo[] GetCallableMethods(MemberGroup members) {
            MethodInfo[] methods = new MethodInfo[members.Count];

            for (int i = 0; i < members.Count; i++) {
                methods[i] = CompilerHelpers.GetCallableMethod(((MethodTracker)members[i]).Method, Binder.PrivateBinding);
            }
            return methods;
        }

        #region Error rules

        private void MakeIncorrectArgumentCountError() {
            AddToBody(
                Rule.MakeError(
                    MakeIncorrectArgumentExpression(0, 0)
                )
            );
        }

        private void MakeGenericPropertyError() {
            // TODO: Better exception
            AddToBody(
                Rule.MakeError(
                    MakeGenericPropertyExpression()
                )
            );
        }

        private void MakeMissingMemberRuleForGet(Type type) {
            if (!Action.IsNoThrow) {
                MakeMissingMemberError(type);
            } else {
                AddToBody(Rule.MakeReturn(Binder, Ast.Field(null, typeof(OperationFailed).GetField("Value"))));
                Rule.IsError = true;
            }
        }

        #endregion
    }
}
