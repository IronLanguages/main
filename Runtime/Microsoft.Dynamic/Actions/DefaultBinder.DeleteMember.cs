/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Dynamic;
using System.Reflection;

using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {

    public partial class DefaultBinder : ActionBinder {
        public DynamicMetaObject DeleteMember(string name, DynamicMetaObject target) {
            return DeleteMember(name, target, new DefaultOverloadResolverFactory(this));
        }

        public DynamicMetaObject DeleteMember(string name, DynamicMetaObject target, OverloadResolverFactory resolutionFactory) {
            return DeleteMember(name, target, resolutionFactory, null);
        }

        public DynamicMetaObject DeleteMember(string name, DynamicMetaObject target, OverloadResolverFactory resolutionFactory, DynamicMetaObject errorSuggestion) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");

            return MakeDeleteMemberTarget(
                new SetOrDeleteMemberInfo(
                    name,
                    resolutionFactory
                ),
                target.Restrict(target.GetLimitType()),
                errorSuggestion
            );
        }

        private DynamicMetaObject MakeDeleteMemberTarget(SetOrDeleteMemberInfo delInfo, DynamicMetaObject target, DynamicMetaObject errorSuggestion) {
            Type type = target.GetLimitType();
            BindingRestrictions restrictions = target.Restrictions;
            DynamicMetaObject self = target;

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)
                );

                type = ((TypeTracker)target.Value).Type;
                self = null;
            }

            delInfo.Body.Restrictions = restrictions;

            if (self == null || !MakeOperatorDeleteMemberBody(delInfo, self, type, "DeleteMember")) {
                MemberGroup group = GetMember(MemberRequestKind.Delete, type, delInfo.Name);
                if (group.Count != 0) {
                    if (group[0].MemberType == TrackerTypes.Property) {
                        MethodInfo del = ((PropertyTracker)group[0]).GetDeleteMethod(PrivateBinding);
                        if (del != null) {
                            MakePropertyDeleteStatement(delInfo, self, del);
                            return delInfo.Body.GetMetaObject(target);
                        }
                    }

                    delInfo.Body.FinishError(errorSuggestion ?? MakeError(MakeUndeletableMemberError(GetDeclaringMemberType(group), delInfo.Name), typeof(void)));
                } else {
                    delInfo.Body.FinishError(errorSuggestion ?? MakeError(MakeMissingMemberErrorForDelete(type, self, delInfo.Name), typeof(void)));
                }
            }

            return delInfo.Body.GetMetaObject(target);
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

        private void MakePropertyDeleteStatement(SetOrDeleteMemberInfo delInfo, DynamicMetaObject instance, MethodInfo delete) {
            delInfo.Body.FinishCondition(
                instance == null ? 
                    MakeCallExpression(delInfo.ResolutionFactory, delete) :
                    MakeCallExpression(delInfo.ResolutionFactory, delete, instance)
            );
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private bool MakeOperatorDeleteMemberBody(SetOrDeleteMemberInfo delInfo, DynamicMetaObject instance, Type type, string name) {
            MethodInfo delMem = GetMethod(type, name);

            if (delMem != null) {
                DynamicMetaObject call = MakeCallExpression(delInfo.ResolutionFactory, delMem, instance, new DynamicMetaObject(AstUtils.Constant(delInfo.Name), BindingRestrictions.Empty, delInfo.Name));

                if (delMem.ReturnType == typeof(bool)) {
                    delInfo.Body.AddCondition(
                        call.Expression,
                        AstUtils.Constant(null)
                    );
                } else {
                    delInfo.Body.FinishCondition(call);
                }

                return delMem.ReturnType != typeof(bool);
            }
            return false;
        }

        /// <summary>
        /// Helper class for flowing information about the GetMember request.
        /// </summary>
        private sealed class SetOrDeleteMemberInfo {
            public readonly string Name;
            public readonly OverloadResolverFactory ResolutionFactory;
            public readonly ConditionalBuilder Body = new ConditionalBuilder();

            public SetOrDeleteMemberInfo(string name, OverloadResolverFactory resolutionFactory) {
                Name = name;
                ResolutionFactory = resolutionFactory;
            }
        }
    }
}
