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
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Actions {
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public class DeleteMemberBinderHelper : MemberBinderHelper<OldDeleteMemberAction> {
        private bool _isStatic;

        public DeleteMemberBinderHelper(CodeContext context, OldDeleteMemberAction action, object[] args, RuleBuilder rule)
            : base(context, action, args, rule) {
        }

        public void MakeRule() {
            Rule.MakeTest(StrongBoxType ?? CompilerHelpers.GetType(Target));
            Rule.Target = MakeDeleteMemberTarget();
        }

        private Expression MakeDeleteMemberTarget() {
            Type type = CompilerHelpers.GetType(Target);

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                type = ((TypeTracker)Target).Type;
                _isStatic = true;
                Rule.AddTest(Ast.Equal(Rule.Parameters[0], AstUtils.Constant(Arguments[0])));
            } 

            if (_isStatic || !MakeOperatorGetMemberBody(type, "DeleteMember")) {
                MemberGroup group = Binder.GetMember(Action, type, StringName);
                if (group.Count != 0) {
                    if (group[0].MemberType == TrackerTypes.Property) {
                        MethodInfo del = ((PropertyTracker)group[0]).GetDeleteMethod(PrivateBinding);
                        if (del != null) {
                            MakePropertyDeleteStatement(del);
                            return Body;
                        }
                    }

                    MakeUndeletableMemberError(GetDeclaringMemberType(group));
                } else {
                    MakeMissingMemberError(type);
                }
            }

            return Body;
        }

        private static Type GetDeclaringMemberType(MemberGroup group) {
            Type t = typeof(object);
            foreach (MemberTracker mt in group) {
                if (t.IsAssignableFrom(mt.DeclaringType)) {
                    t = mt.DeclaringType;
                }
            }
            return t;
        }

        private void MakePropertyDeleteStatement(MethodInfo delete) {
            AddToBody(
                Rule.MakeReturn(
                    Binder,
                    Binder.MakeCallExpression(Rule.Context, delete, Rule.Parameters[0])
                )
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorGetMemberBody(Type type, string name) {
            MethodInfo delMem = GetMethod(type, name);
            if (delMem != null && delMem.IsSpecialName) {
                Expression call = Binder.MakeCallExpression(Rule.Context, delMem, Rule.Parameters[0], AstUtils.Constant(StringName));
                Expression ret;

                if (delMem.ReturnType == typeof(bool)) {
                    ret = AstUtils.If(call, Rule.MakeReturn(Binder, AstUtils.Constant(null)));
                } else {
                    ret = Rule.MakeReturn(Binder, call);
                }
                AddToBody( ret);
                return delMem.ReturnType != typeof(bool);
            }
            return false;
        }
    }
}
