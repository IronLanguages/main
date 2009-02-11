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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    partial class MetaPythonType : MetaPythonObject, IPythonGetable {

        #region MetaObject Overrides

        public override DynamicMetaObject/*!*/ BindGetMember(GetMemberBinder/*!*/ member) {
            return GetMemberWorker(member, BinderState.GetCodeContext(member));            
        }

        private ValidationInfo GetTypeTest() {
            int version = Value.Version;

            return new ValidationInfo(
                Ast.Call(
                    typeof(PythonOps).GetMethod("CheckSpecificTypeVersion"),
                    AstUtils.Convert(Expression, typeof(PythonType)),
                    Ast.Constant(version)
                ),
                new BindingHelpers.PythonTypeValidator(Value, version).Validate
            );
        }

        public override DynamicMetaObject/*!*/ BindSetMember(SetMemberBinder/*!*/ member, DynamicMetaObject/*!*/ value) {
            BinderState state = BinderState.GetBinderState(member);

            if (Value.IsSystemType) {
                MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);
                MemberGroup mg = state.Binder.GetMember(OldSetMemberAction.Make(state.Binder, member.Name), Value.UnderlyingSystemType, member.Name);

                // filter protected member access against .NET types, these can only be accessed from derived types...
                foreach (MemberTracker mt in mg) {
                    if (IsProtectedSetter(mt)) {
                        return new DynamicMetaObject(
                            BindingHelpers.TypeErrorForProtectedMember(Value.UnderlyingSystemType, member.Name),
                            Restrictions.Merge(value.Restrictions).Merge(BindingRestrictions.GetInstanceRestriction(Expression, Value))
                        );
                    }
                }

                // have the default binder perform it's operation against a TypeTracker and then
                // replace the test w/ our own.
                return new DynamicMetaObject(
                    state.Binder.SetMember(
                        member.Name,
                        new DynamicMetaObject(
                            Ast.Constant(tt),
                            BindingRestrictions.Empty,
                            tt
                        ),
                        value,
                        Ast.Constant(state.Context)
                    ).Expression,
                    Restrictions.Merge(value.Restrictions).Merge(BindingRestrictions.GetInstanceRestriction(Expression, Value))
                );
            }

            return MakeSetMember(member, value);
        }

        public override DynamicMetaObject/*!*/ BindDeleteMember(DeleteMemberBinder/*!*/ member) {
            if (Value.IsSystemType) {
                BinderState state = BinderState.GetBinderState(member);

                MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);

                // have the default binder perform it's operation against a TypeTracker and then
                // replace the test w/ our own.
                return new DynamicMetaObject(
                    state.Binder.DeleteMember(
                        member.Name,
                        new DynamicMetaObject(
                            Ast.Constant(tt),
                            BindingRestrictions.Empty,
                            tt
                        )
                    ).Expression,
                    BindingRestrictions.GetInstanceRestriction(Expression, Value).Merge(Restrictions)
                );
            }

            return MakeDeleteMember(member);
        }

        #endregion

        #region IPythonGetable Members

        public DynamicMetaObject/*!*/ GetMember(PythonGetMemberBinder/*!*/ member, Expression/*!*/ codeContext) {
            return GetMemberWorker(member, codeContext);
        }

        #endregion

        #region Gets

        private DynamicMetaObject/*!*/ GetMemberWorker(DynamicMetaObjectBinder/*!*/ member, Expression codeContext) {
            switch (GetGetMemberName(member)) {
                case "__dict__":
                case "__class__":
                case "__bases__":
                case "__name__":
                    DynamicMetaObject self = Restrict(this.GetRuntimeType());
                    ValidationInfo valInfo = MakeMetaTypeTest(self.Expression);

                    return BindingHelpers.AddDynamicTestAndDefer(
                        member,
                        new DynamicMetaObject(
                            MakeMetaTypeRule(member, GetMemberFallback(member, codeContext).Expression),
                            self.Restrictions
                        ),
                        new DynamicMetaObject[] { this },
                        valInfo
                    );
                default:
                    if (!Value.IsSystemType) {
                        ValidationInfo typeTest = GetTypeTest();

                        return BindingHelpers.AddDynamicTestAndDefer(
                            member,
                            MakeTypeGetMember(member, codeContext),
                            new DynamicMetaObject[] { this },
                            typeTest
                        );
                    }

                    return MakeTypeGetMember(member, codeContext);
            }
        }
        
        private ValidationInfo MakeMetaTypeTest(Expression self) {

            PythonType metaType = DynamicHelpers.GetPythonType(Value);
            if (!metaType.IsSystemType) {
                int version = metaType.Version;

                return new ValidationInfo(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("CheckTypeVersion"),
                        self,
                        Ast.Constant(version)
                    ),
                    new BindingHelpers.PythonTypeValidator(metaType, version).Validate
                );
            }

            return ValidationInfo.Empty;
        }

        private DynamicMetaObject/*!*/ MakeTypeGetMember(DynamicMetaObjectBinder/*!*/ member, Expression codeContext) {
            // normal attribute, need to check the type version
            DynamicMetaObject self = new DynamicMetaObject(
                AstUtils.Convert(Expression, Value.GetType()),
                Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(Expression, Value)),
                Value
            );

            BinderState state = BinderState.GetBinderState(member);

            // have the default binder perform it's operation against a TypeTracker and then
            // replace the test w/ our own.
            DynamicMetaObject result = GetFallbackGet(member, state, codeContext);

            for (int i = Value.ResolutionOrder.Count - 1; i >= 0; i--) {
                PythonType pt = Value.ResolutionOrder[i];

                PythonTypeSlot pts;

                if (pt.IsSystemType) {
                    // built-in type, see if we can bind to any .NET members and then quit the search 
                    // because this includes all subtypes.
                    result = new DynamicMetaObject(
                        MakeSystemTypeGetExpression(pt, member, result.Expression),
                        self.Restrictions // don't merge w/ result - we've already restricted to instance.
                    );
                } else if (pt.IsOldClass) {
                    // mixed new-style/old-style class, search the one slot in it's MRO for the member
                    ParameterExpression tmp = Ast.Variable(typeof(object), "tmp");
                    result = new DynamicMetaObject(
                        Ast.Block(
                            new ParameterExpression[] { tmp },
                            Ast.Condition(
                                Ast.Call(
                                    typeof(PythonOps).GetMethod("OldClassTryLookupOneSlot"),
                                    Ast.Constant(pt.OldClass),
                                    AstUtils.Constant(SymbolTable.StringToId(GetGetMemberName(member))),
                                    tmp
                                ),
                                tmp,
                                AstUtils.Convert(result.Expression, typeof(object))
                            )
                        ),
                        self.Restrictions // don't merge w/ result - we've already restricted to instance.
                    );

                } else if (pt.TryLookupSlot(state.Context, SymbolTable.StringToId(GetGetMemberName(member)), out pts)) {
                    // user defined new style class, see if we have a slot.
                    ParameterExpression tmp = Ast.Variable(typeof(object), "tmp");

                    result = new DynamicMetaObject(
                        Ast.Block(
                            new ParameterExpression[] { tmp },
                            Ast.Condition(
                                Ast.Call(
                                    TypeInfo._PythonOps.SlotTryGetBoundValue,
                                    Ast.Constant(BinderState.GetBinderState(member).Context),
                                    Ast.Constant(pts, typeof(PythonTypeSlot)),
                                    Ast.Constant(null),
                                    Ast.Constant(Value),
                                    tmp
                                ),
                                tmp,
                                AstUtils.Convert(
                                    result.Expression,
                                    typeof(object)
                                )
                            )
                        ),
                        self.Restrictions   // don't merge w/ result - we've already restricted to instance.
                    );
                }
            }

            return result;
        }

        private DynamicMetaObject/*!*/ GetFallbackGet(DynamicMetaObjectBinder/*!*/ member, BinderState/*!*/ state, Expression codeContext) {
            MemberTracker tt = MemberTracker.FromMemberInfo(Value.UnderlyingSystemType);

            string memberName = GetGetMemberName(member);
            DynamicMetaObject res = new DynamicMetaObject(
                state.Binder.GetMember(
                    memberName,
                    new DynamicMetaObject(
                        Ast.Constant(tt),
                        BindingRestrictions.Empty,
                        tt
                    ),
                    Ast.Constant(state.Context),
                    BindingHelpers.IsNoThrow(member)

                ).Expression,
                BindingRestrictions.GetInstanceRestriction(Expression, Value).Merge(Restrictions)
            );

            if (codeContext != null && Value.IsHiddenMember(memberName)) {
                res = BindingHelpers.FilterShowCls(
                    codeContext,
                    member,
                    res,
                    Ast.Throw(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                            Ast.Constant(Value.Name),
                            AstUtils.Constant(SymbolTable.StringToId(memberName))
                        )
                    )
                );
            }

            return res;
        }

        private Expression MakeSystemTypeGetExpression(PythonType/*!*/ pt, DynamicMetaObjectBinder/*!*/ member, Expression/*!*/ error) {
            BinderState state = BinderState.GetBinderState(member);

            PythonTypeSlot pts;

            CodeContext clsContext = PythonContext.GetContext(state.Context).DefaultClsBinderState.Context;
            if (state.Binder.TryResolveSlot(clsContext, pt, Value, SymbolTable.StringToId(GetGetMemberName(member)), out pts)) {
                Expression success = pts.MakeGetExpression(
                    state.Binder,
                    BinderState.GetCodeContext(member),
                    null,
                    AstUtils.Convert(AstUtils.WeakConstant(Value), typeof(PythonType)),
                    error
                );

                return AddClsCheck(member, pts, success, error);
            }

            // need to lookup on type
            return MakeMetaTypeRule(member, error);
        }

        private Expression/*!*/ AddClsCheck(DynamicMetaObjectBinder/*!*/ member, PythonTypeSlot/*!*/ slot, Expression/*!*/ success, Expression/*!*/ error) {
            BinderState state = BinderState.GetBinderState(member);

            if (Value.IsPythonType && !slot.IsAlwaysVisible) {
                Type resType = BindingHelpers.GetCompatibleType(success.Type, error.Type);

                success = Ast.Condition(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("IsClsVisible"),
                        Ast.Constant(state.Context)
                    ),
                    AstUtils.Convert(success, resType),
                    AstUtils.Convert(error, resType)
                );
            }
            return success;
        }

        private Expression MakeMetaTypeRule(DynamicMetaObjectBinder/*!*/ member, Expression error) {
            BinderState state = BinderState.GetBinderState(member);

            string name = GetGetMemberName(member);
            OldGetMemberAction gma = OldGetMemberAction.Make(state.Binder, name);
            MemberGroup mg = state.Binder.GetMember(gma, typeof(PythonType), name);
            PythonType metaType = DynamicHelpers.GetPythonType(Value);
            PythonTypeSlot pts;

            foreach (PythonType pt in metaType.ResolutionOrder) {
                if (pt.IsSystemType) {
                    // need to lookup on type
                    mg = state.Binder.GetMember(gma, typeof(PythonType), name);

                    if (mg.Count > 0) {
                        return GetBoundTrackerOrError(member, mg, error);
                    }
                } else if (pt.OldClass != null) {
                    // mixed new-style/old-style class, just call our version of __getattribute__
                    // and let it sort it out at runtime.  
                    // TODO: IfError support
                    return Ast.Call(
                        AstUtils.Convert(
                            Expression,
                            typeof(PythonType)
                        ),
                        typeof(PythonType).GetMethod("__getattribute__"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        Ast.Constant(name)
                    );
                } else if (pt.TryLookupSlot(BinderState.GetBinderState(member).Context, SymbolTable.StringToId(GetGetMemberName(member)), out pts)) {
                    // user defined new style class, see if we have a slot.
                    ParameterExpression tmp = Ast.Variable(typeof(object), "slotRes");
                    return Ast.Block(
                        new ParameterExpression[] { tmp },
                        Ast.Condition(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                                Ast.Constant(BinderState.GetBinderState(member).Context),
                                Ast.Constant(pts, typeof(PythonTypeSlot)),
                                Expression,
                                Ast.Constant(metaType),
                                tmp
                            ),
                            tmp,
                            AstUtils.Convert(error, typeof(object))
                        )
                    );
                }
            }

            // the member doesn't exist anywhere in the type hierarchy, see if
            // we define __getattr__ on our meta type.
            if (metaType.TryResolveSlot(BinderState.GetBinderState(member).Context, Symbols.GetBoundAttr, out pts)) {
                ParameterExpression tmp = Ast.Variable(typeof(object), "res");
                return Ast.Block(
                    new ParameterExpression[] { tmp },
                    Ast.Condition(
                        Ast.Call(
                            typeof(PythonOps).GetMethod("SlotTryGetBoundValue"),
                            Ast.Constant(BinderState.GetBinderState(member).Context),
                            Ast.Constant(pts, typeof(PythonTypeSlot)),
                            Expression,
                            Ast.Constant(metaType),
                            tmp
                        ),
                        Ast.Dynamic(
                            new PythonInvokeBinder(
                                BinderState.GetBinderState(member),
                                new CallSignature(1)
                            ),
                            typeof(object),
                            BinderState.GetCodeContext(member),
                            tmp,
                            Ast.Constant(name)
                        ),
                        AstUtils.Convert(
                            error,
                            typeof(object)
                        )
                    )
                );
            }

            return error;/* ?? Ast.Throw(
                Ast.Call(
                    typeof(PythonOps).GetMethod("AttributeErrorForMissingAttribute", new Type[] { typeof(string), typeof(SymbolId) }),
                    Ast.Constant(DynamicHelpers.GetPythonType(_type).Name),
                    Ast.Constant(SymbolTable.StringToId(_name))
                )
            );*/
        }

        private Expression/*!*/ GetBoundTrackerOrError(DynamicMetaObjectBinder/*!*/ member, MemberGroup/*!*/ mg, Expression error) {
            BinderState state = BinderState.GetBinderState(member);
            MemberTracker tracker = GetTracker(member, mg);
            Expression target = null;

            if (tracker != null) {
                tracker = tracker.BindToInstance(AstUtils.Convert(Expression, typeof(PythonType)));
                target = tracker.GetValue(Ast.Constant(state.Context), state.Binder, Value.UnderlyingSystemType);
            }

            return target ?? error /*?? Ast.Throw(MakeAmbiguousMatchError(mg))*/;
        }
#if FALSE
        private static Expression/*!*/ MakeErrorExpression(MemberGroup/*!*/ mg) {
            if (mg.Count == 1) {
                MemberTracker mt = mg[0];

                if (mt.DeclaringType.ContainsGenericParameters) {
                    return Ast.Throw(
                        Ast.New(
                            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }),
                            Ast.Constant(String.Format("Cannot access member {1} declared on type {0} because the type contains generic parameters.", mt.DeclaringType.Name, mt.Name))
                        )
                    );
                }
            }

            return Ast.Throw(MakeAmbiguousMatchError(mg));
        }

        private static Expression/*!*/ MakeAmbiguousMatchError(MemberGroup/*!*/ members) {
            StringBuilder sb = new StringBuilder();
            foreach (MethodTracker mi in members) {
                if (sb.Length != 0) sb.Append(", ");
                sb.Append(mi.MemberType);
                sb.Append(" : ");
                sb.Append(mi.ToString());
            }

            return Ast.New(typeof(AmbiguousMatchException).GetConstructor(
                new Type[] { typeof(string) }),
                Ast.Constant(sb.ToString())
            );
        }
#endif

        private MemberTracker GetTracker(DynamicMetaObjectBinder/*!*/ member, MemberGroup/*!*/ mg) {
            TrackerTypes mt = GetMemberTypes(mg);
            MemberTracker tracker;

            switch (mt) {
                case TrackerTypes.Method:
                    tracker = ReflectionCache.GetMethodGroup(GetGetMemberName(member), mg);
                    break;
                case TrackerTypes.TypeGroup:
                case TrackerTypes.Type:
                    tracker = GetTypeGroup(mg);
                    break;
                case TrackerTypes.Field:
                case TrackerTypes.Property:
                    tracker = null;
                    foreach (MemberTracker curTracker in mg) {
                        if (curTracker.DeclaringType == Value.UnderlyingSystemType) {
                            tracker = curTracker;
                        }
                    }
                    if (tracker == null) {
                        tracker = mg[0];
                    }
                    break;
                case TrackerTypes.Field | TrackerTypes.Property:
                    // occurs when we have a protected field w/ public property accessors
                    List<MemberTracker> newGroup = new List<MemberTracker>();
                    foreach (MemberTracker curTracker in mg) {
                        if (curTracker.MemberType != TrackerTypes.Field) {
                            newGroup.Add(curTracker);
                        }
                    }

                    return GetTracker(member, new MemberGroup(newGroup.ToArray()));
                case TrackerTypes.Event:
                case TrackerTypes.Namespace:
                case TrackerTypes.Custom:
                case TrackerTypes.Constructor:
                    tracker = mg[0];
                    break;
                default:
                    tracker = null;
                    break;
            }

            return tracker;
        }

        internal static TrackerTypes GetMemberTypes(MemberGroup members) {
            TrackerTypes memberType = TrackerTypes.None;
            for (int i = 0; i < members.Count; i++) {
                MemberTracker mi = members[i];
                memberType |= mi.MemberType;
            }

            return memberType;
        }

        private static TypeTracker/*!*/ GetTypeGroup(MemberGroup/*!*/ members) {
            TypeTracker typeTracker = (TypeTracker)members[0];
            for (int i = 1; i < members.Count; i++) {
                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
            }
            return typeTracker;
        }

        #endregion

        #region Sets

        private DynamicMetaObject/*!*/ MakeSetMember(SetMemberBinder/*!*/ member, DynamicMetaObject/*!*/ value) {
            DynamicMetaObject self = Restrict(typeof(PythonType));

            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new DynamicMetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeSetCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(SymbolTable.StringToId(member.Name)),
                        AstUtils.Convert(
                            value.Expression,
                            typeof(object)
                        )
                    ),
                    self.Restrictions.Merge(value.Restrictions)
                ),
                new DynamicMetaObject[] { this, value },
                TestUserType()
            );
        }

        private bool IsProtectedSetter(MemberTracker mt) {
            PropertyTracker pt = mt as PropertyTracker;
            if (pt != null) {
                MethodInfo mi = pt.GetSetMethod(true);
                if (mi != null && (mi.IsFamily || mi.IsFamilyOrAssembly)) {
                    return true;
                }
            }

            FieldTracker ft = mt as FieldTracker;
            if (ft != null) {
                return ft.Field.IsFamily || ft.Field.IsFamilyOrAssembly;
            }

            return false;
        }

        #endregion

        #region Deletes

        private DynamicMetaObject/*!*/ MakeDeleteMember(DeleteMemberBinder/*!*/ member) {
            DynamicMetaObject self = Restrict(typeof(PythonType));
            return BindingHelpers.AddDynamicTestAndDefer(
                member,
                new DynamicMetaObject(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("PythonTypeDeleteCustomMember"),
                        Ast.Constant(BinderState.GetBinderState(member).Context),
                        self.Expression,
                        AstUtils.Constant(SymbolTable.StringToId(member.Name))
                    ),
                    self.Restrictions
                ),
                new DynamicMetaObject[] { this },
                TestUserType()
            );
        }

        #endregion

        #region Helpers

        private ValidationInfo/*!*/ TestUserType() {
            return new ValidationInfo(
                Ast.Not(
                    Ast.Call(
                        typeof(PythonOps).GetMethod("IsPythonType"),
                        AstUtils.Convert(
                            Expression,
                            typeof(PythonType)
                        )
                    )
                ),
                null
            );
        }

        #endregion
    }
}
