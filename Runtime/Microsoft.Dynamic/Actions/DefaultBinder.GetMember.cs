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

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace Microsoft.Scripting.Actions {
    using Ast = Expression;
    
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
        /// <returns>
        /// Returns a DynamicMetaObject which represents the value that will be returned when the member is accessed.
        /// 
        /// The returned DynamicMetaObject may be strongly typed to a value type which needs boxing before being
        /// returned from a standard DLR GetMemberBinder.  The language is responsible for performing any boxing
        /// so that it has an opportunity to perform custom boxing.
        /// </returns>
        public DynamicMetaObject GetMember(string name, DynamicMetaObject target) {
            return GetMember(
                name,
                target,
                new DefaultOverloadResolverFactory(this),
                false,
                null
            );
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
        /// <param name="resolverFactory">
        /// Provides overload resolution and method binding for any calls which need to be performed for the GetMember.
        /// </param>
        /// <returns>
        /// Returns a DynamicMetaObject which represents the value that will be returned when the member is accessed.
        /// 
        /// The returned DynamicMetaObject may be strongly typed to a value type which needs boxing before being
        /// returned from a standard DLR GetMemberBinder.  The language is responsible for performing any boxing
        /// so that it has an opportunity to perform custom boxing.
        /// </returns>
        public DynamicMetaObject GetMember(string name, DynamicMetaObject target, OverloadResolverFactory resolverFactory) {
            return GetMember(
                name,
                target,
                resolverFactory,
                false,
                null
            );
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
        /// <param name="resolverFactory">
        /// An OverloadResolverFactory which can be used for performing overload resolution and method binding.
        /// </param>
        /// <param name="isNoThrow">
        /// True if the operation should return Operation.Failed on failure, false if it
        /// should return the exception produced by MakeMissingMemberError.
        /// </param>
        /// <param name="errorSuggestion">
        /// The meta object to be used if the get results in an error.
        /// </param>
        /// <returns>
        /// Returns a DynamicMetaObject which represents the value that will be returned when the member is accessed.
        /// 
        /// The returned DynamicMetaObject may be strongly typed to a value type which needs boxing before being
        /// returned from a standard DLR GetMemberBinder.  The language is responsible for performing any boxing
        /// so that it has an opportunity to perform custom boxing.
        /// </returns>
        public DynamicMetaObject GetMember(string name, DynamicMetaObject target, OverloadResolverFactory resolverFactory, bool isNoThrow, DynamicMetaObject errorSuggestion) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(resolverFactory, "resolverFactory");

            return MakeGetMemberTarget(
                new GetMemberInfo(
                    name,
                    resolverFactory,
                    isNoThrow,
                    errorSuggestion
                ),
                target
            );
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
        /// <param name="isNoThrow">
        /// True if the operation should return Operation.Failed on failure, false if it
        /// should return the exception produced by MakeMissingMemberError.
        /// </param>
        /// <param name="errorSuggestion">
        /// The meta object to be used if the get results in an error.
        /// </param>
        /// <returns>
        /// Returns a DynamicMetaObject which represents the value that will be returned when the member is accessed.
        /// 
        /// The returned DynamicMetaObject may be strongly typed to a value type which needs boxing before being
        /// returned from a standard DLR GetMemberBinder.  The language is responsible for performing any boxing
        /// so that it has an opportunity to perform custom boxing.
        /// </returns>
        public DynamicMetaObject GetMember(string name, DynamicMetaObject target, bool isNoThrow, DynamicMetaObject errorSuggestion) {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(target, "target");

            return MakeGetMemberTarget(
                new GetMemberInfo(
                    name,
                    new DefaultOverloadResolverFactory(this),
                    isNoThrow,
                    errorSuggestion
                ),
                target
            );
        }

        private DynamicMetaObject MakeGetMemberTarget(GetMemberInfo getMemInfo, DynamicMetaObject target) {
            Type targetType = target.GetLimitType();
            BindingRestrictions restrictions = target.Restrictions;
            DynamicMetaObject self = target;
            target = target.Restrict(target.GetLimitType());

            // Specially recognized types: TypeTracker, NamespaceTracker, and StrongBox.  
            // TODO: TypeTracker and NamespaceTracker should technically be IDO's.
            MemberGroup members = MemberGroup.EmptyGroup;
            if (typeof(TypeTracker).IsAssignableFrom(targetType)) {
                restrictions = restrictions.Merge(
                    BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value)
                );

                TypeGroup tg = target.Value as TypeGroup;
                Type nonGen;
                if (tg == null || tg.TryGetNonGenericType(out nonGen)) {
                    members = GetMember(MemberRequestKind.Get, ((TypeTracker)target.Value).Type, getMemInfo.Name);
                    if (members.Count > 0) {
                        // we have a member that's on the type associated w/ the tracker, return that...
                        targetType = ((TypeTracker)target.Value).Type;
                        self = null;
                    }
                }
            }

            if (members.Count == 0) {
                // Get the members
                members = GetMember(MemberRequestKind.Get, targetType, getMemInfo.Name);
            }

            if (members.Count == 0) {
                if (typeof(TypeTracker).IsAssignableFrom(targetType)) {
                    // Throws an exception if we don't have a non-generic type, and if we do report an error now.  This matches
                    // the rule version of the default binder but should probably be removed long term.
                    EnsureTrackerRepresentsNonGenericType((TypeTracker)target.Value);
                } else if (targetType.IsInterface()) {
                    // all interfaces have object members
                    targetType = typeof(object);
                    members = GetMember(MemberRequestKind.Get, targetType, getMemInfo.Name);
                }
            }

            DynamicMetaObject propSelf = self;
            // if lookup failed try the strong-box type if available.
            if (members.Count == 0 && typeof(IStrongBox).IsAssignableFrom(targetType) && propSelf != null) {
                // properties/fields need the direct value, methods hold onto the strong box.
                propSelf = new DynamicMetaObject(
                    Ast.Field(AstUtils.Convert(propSelf.Expression, targetType), targetType.GetInheritedFields("Value").First()),
                    propSelf.Restrictions,
                    ((IStrongBox)propSelf.Value).Value
                );

                targetType = targetType.GetGenericArguments()[0];

                members = GetMember(
                    MemberRequestKind.Get,
                    targetType,
                    getMemInfo.Name
                );
            }

            MakeBodyHelper(getMemInfo, self, propSelf, targetType, members);

            getMemInfo.Body.Restrictions = restrictions;
            return getMemInfo.Body.GetMetaObject(target);
        }

        private static Type EnsureTrackerRepresentsNonGenericType(TypeTracker tracker) {
            // might throw an exception
            return tracker.Type;
        }

        private void MakeBodyHelper(GetMemberInfo getMemInfo, DynamicMetaObject self, DynamicMetaObject propSelf, Type targetType, MemberGroup members) {
            if (self != null) {
                MakeOperatorGetMemberBody(getMemInfo, propSelf, targetType, "GetCustomMember");
            }

            Expression error;
            TrackerTypes memberType = GetMemberType(members, out error);

            if (error == null) {
                MakeSuccessfulMemberAccess(getMemInfo, self, propSelf, targetType, members, memberType);
            } else {
                getMemInfo.Body.FinishError(getMemInfo.ErrorSuggestion != null ? getMemInfo.ErrorSuggestion.Expression : error);
            }
        }

        private void MakeSuccessfulMemberAccess(GetMemberInfo getMemInfo, DynamicMetaObject self, DynamicMetaObject propSelf, Type selfType, MemberGroup members, TrackerTypes memberType) {
            switch (memberType) {
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type:
                    MakeTypeBody(getMemInfo, selfType, members);
                    break;
                case TrackerTypes.Method:
                    // turn into a MethodGroup                    
                    MakeGenericBodyWorker(getMemInfo, selfType, ReflectionCache.GetMethodGroup(getMemInfo.Name, members), self);
                    break;
                case TrackerTypes.Event:
                case TrackerTypes.Field:
                case TrackerTypes.Property:
                case TrackerTypes.Constructor:
                case TrackerTypes.Custom:
                    MakeGenericBody(getMemInfo, selfType, members, propSelf);
                    break;
                case TrackerTypes.All:
                    // no members were found
                    if (self != null) {
                        MakeOperatorGetMemberBody(getMemInfo, propSelf, selfType, "GetBoundMember");
                    }

                    MakeMissingMemberRuleForGet(getMemInfo, self, selfType);
                    break;
                default:
                    throw new InvalidOperationException(memberType.ToString());
            }
        }

        private void MakeGenericBody(GetMemberInfo getMemInfo, Type instanceType, MemberGroup members, DynamicMetaObject instance) {
            MemberTracker bestMember = members[0];
            if (members.Count > 1) {
                // if we were given multiple members pick the member closest to the type...                
                Type bestMemberDeclaringType = members[0].DeclaringType;

                for (int i = 1; i < members.Count; i++) {
                    MemberTracker mt = members[i];
                    if (!IsTrackerApplicableForType(instanceType, mt)) {
                        continue;
                    }

                    if (members[i].DeclaringType.IsSubclassOf(bestMemberDeclaringType) ||
                        !IsTrackerApplicableForType(instanceType, bestMember)) {
                        bestMember = members[i];
                        bestMemberDeclaringType = members[i].DeclaringType;
                    }
                }
            }

            MakeGenericBodyWorker(getMemInfo, instanceType, bestMember, instance);
        }

        private static bool IsTrackerApplicableForType(Type type, MemberTracker mt) {
            return mt.DeclaringType == type || type.IsSubclassOf(mt.DeclaringType);
        }

        private void MakeTypeBody(GetMemberInfo getMemInfo, Type instanceType, MemberGroup members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }

            getMemInfo.Body.FinishCondition(typeTracker.GetValue(getMemInfo.ResolutionFactory, this, instanceType));
        }

        private void MakeGenericBodyWorker(GetMemberInfo getMemInfo, Type instanceType, MemberTracker tracker, DynamicMetaObject instance) {
            if (instance != null) {
                tracker = tracker.BindToInstance(instance);
            }

            DynamicMetaObject val = tracker.GetValue(getMemInfo.ResolutionFactory, this, instanceType);

            if (val != null) {
                getMemInfo.Body.FinishCondition(val);
            } else {
                ErrorInfo ei = tracker.GetError(this, instanceType);
                if (ei.Kind != ErrorInfoKind.Success && getMemInfo.IsNoThrow) {
                    getMemInfo.Body.FinishError(MakeOperationFailed());
                } else {
                    getMemInfo.Body.FinishError(MakeError(ei, typeof(object)));
                }
            }
        }

        /// <summary> if a member-injector is defined-on or registered-for this type call it </summary>
        private void MakeOperatorGetMemberBody(GetMemberInfo getMemInfo, DynamicMetaObject instance, Type instanceType, string name) {
            MethodInfo getMem = GetMethod(instanceType, name);
            if (getMem != null) {
                ParameterExpression tmp = Ast.Variable(typeof(object), "getVal");
                getMemInfo.Body.AddVariable(tmp);

                getMemInfo.Body.AddCondition(
                    Ast.NotEqual(
                        Ast.Assign(
                            tmp,
                            MakeCallExpression(
                                getMemInfo.ResolutionFactory,
                                getMem,
                                new DynamicMetaObject(
                                    Expression.Convert(instance.Expression, instanceType),
                                    instance.Restrictions,
                                    instance.Value
                                ),
                                new DynamicMetaObject(
                                    Expression.Constant(getMemInfo.Name),
                                    BindingRestrictions.Empty,
                                    getMemInfo.Name
                                )
                            ).Expression
                        ),
                        Ast.Field(null, typeof(OperationFailed).GetDeclaredField("Value"))
                    ),
                    tmp
                );
            }
        }

        private void MakeMissingMemberRuleForGet(GetMemberInfo getMemInfo, DynamicMetaObject self, Type type) {
            if (getMemInfo.ErrorSuggestion != null) {
                getMemInfo.Body.FinishError(getMemInfo.ErrorSuggestion.Expression);
            } else if (getMemInfo.IsNoThrow) {
                getMemInfo.Body.FinishError(MakeOperationFailed());
            } else {
                getMemInfo.Body.FinishError(
                    MakeError(MakeMissingMemberError(type, self, getMemInfo.Name), typeof(object))
                );
            }
        }

        private static MemberExpression MakeOperationFailed() {
            return Ast.Field(null, typeof(OperationFailed).GetDeclaredField("Value"));
        }


        /// <summary>
        /// Helper class for flowing information about the GetMember request.
        /// </summary>
        private sealed class GetMemberInfo {
            public readonly string Name;
            public readonly OverloadResolverFactory ResolutionFactory;
            public readonly bool IsNoThrow;
            public readonly ConditionalBuilder Body = new ConditionalBuilder();
            public readonly DynamicMetaObject ErrorSuggestion;

            public GetMemberInfo(string name, OverloadResolverFactory resolutionFactory, bool noThrow, DynamicMetaObject errorSuggestion) {
                Name = name;
                ResolutionFactory = resolutionFactory;
                IsNoThrow = noThrow;
                ErrorSuggestion = errorSuggestion;
            }
        }
    }
}
