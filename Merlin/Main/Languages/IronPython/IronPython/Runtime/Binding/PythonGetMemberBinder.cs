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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Runtime.Binding {

    class PythonGetMemberBinder : DynamicMetaObjectBinder, IPythonSite, IExpressionSerializable {
        private readonly PythonContext/*!*/ _context;
        private readonly GetMemberOptions _options;
        private readonly string _name;

        public PythonGetMemberBinder(PythonContext/*!*/ context, string/*!*/ name) {
            _context = context;
            _name = name;
        }

        public PythonGetMemberBinder(PythonContext/*!*/ context, string/*!*/ name, bool isNoThrow)
            : this(context, name) {
            _options = isNoThrow ? GetMemberOptions.IsNoThrow : GetMemberOptions.None;
        }

        #region MetaAction overrides

        /// <summary>
        /// Python's Invoke is a non-standard action.  Here we first try to bind through a Python
        /// internal interface (IPythonInvokable) which supports CallSigantures.  If that fails
        /// and we have an IDO then we translate to the DLR protocol through a nested dynamic site -
        /// this includes unsplatting any keyword / position arguments.  Finally if it's just a plain
        /// old .NET type we use the default binder which supports CallSignatures.
        /// </summary>
        public override DynamicMetaObject/*!*/ Bind(DynamicMetaObject/*!*/ target, DynamicMetaObject/*!*/[]/*!*/ args) {
            Debug.Assert(args.Length == 1);
            Debug.Assert(args[0].GetLimitType() == typeof(CodeContext));

            // we don't have CodeContext if an IDO falls back to us when we ask them to produce the Call
            DynamicMetaObject cc = args[0];
            IPythonGetable icc = target as IPythonGetable;

            if (icc != null) {
                // get the member using our interface which also supports CodeContext.
                return icc.GetMember(this, cc);
            } else if (target.Value is IDynamicMetaObjectProvider && !(target is MetaPythonObject)) {
                return GetForeignObject(target);
            }
#if !SILVERLIGHT
 else if (ComOps.IsComObject(target.Value)) {
                return GetForeignObject(target);
            }
#endif
            return Fallback(target, cc);
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args) {
            Debug.Assert(args[1].GetType() == typeof(CodeContext));

            IFastGettable fastGet = args[0] as IFastGettable;
            if (fastGet != null) {
                T res = fastGet.MakeGetBinding<T>(site, this, (CodeContext)args[1], Name);
                if (res != null) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.BindingFast, "IFastGettable");
                    return res;
                }

                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "IFastGettable");
                return base.BindDelegate<T>(site, args);
            }

            IPythonObject pyObj = args[0] as IPythonObject;
            if (pyObj != null && !(args[0] is IProxyObject)) {
                FastBindResult<T> res = UserTypeOps.MakeGetBinding<T>((CodeContext)args[1], site, pyObj, this);
                if (res.Target != null) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.BindingFast, "IPythonObject");
                    if (res.ShouldCache) {
                        CacheTarget(res.Target);
                    }
                    return res.Target;
                }

                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "IPythonObject Get");
                return base.BindDelegate<T>(site, args);
            }

            if (args[0] != null) {
                if (args[0].GetType() == typeof(Scope)) {
                    if (!IsNoThrow) {
                        return (T)(object)new Func<CallSite, object, CodeContext, object>(new ScopeDelegate(_name).Target);
                    } else {
                        return (T)(object)new Func<CallSite, object, CodeContext, object>(new ScopeDelegate(_name).NoThrowTarget);
                    }
                } else if (args[0].GetType() == typeof(NamespaceTracker)) {
                    switch (Name) {
                        case "__str__":
                        case "__repr__":
                            // need to return the built in method descriptor for these...
                            break;
                        case "__file__":
                            return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetFile);
                        case "__dict__":
                            return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetDict);
                        case "__name__":
                            return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).GetName);
                        default:
                            return (T)(object)new Func<CallSite, object, CodeContext, object>(new NamespaceTrackerDelegate(_name).Target);
                    }
                }
            }

            if (args[0] != null &&
#if !SILVERLIGHT
 !ComOps.IsComObject(args[0]) &&
#endif
 !(args[0] is IDynamicMetaObjectProvider)) {
                Type selfType = typeof(T).GetMethod("Invoke").GetParameters()[1].ParameterType;
                T res = null;
                if (selfType == typeof(object)) {
                    res = (T)(object)MakeGetMemberTarget<object>(Name, args[0]);
                } else if (selfType == typeof(List)) {
                    res = (T)(object)MakeGetMemberTarget<List>(Name, args[0]);
                } else if (selfType == typeof(string)) {
                    res = (T)(object)MakeGetMemberTarget<string>(Name, args[0]);
                }

                if (res != null) {
                    return (T)(object)res;
                }
                return base.BindDelegate<T>(site, args);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast " + IsNoThrow + " " + CompilerHelpers.GetType(args[0]));
            return base.BindDelegate<T>(site, args);
        }

        class FastErrorGet<TSelfType> : FastGetBase {
            private readonly Type _type;
            private readonly string _name;

            public FastErrorGet(Type type, string name) {
                _type = type;
                _name = name;
            }

            public override bool IsValid(PythonType type) {
                // only used for built-in types, we never become invalid.
                return true;
            }

            public object GetError(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    throw PythonOps.AttributeErrorForMissingAttribute(DynamicHelpers.GetPythonType(target).Name, SymbolTable.StringToId(_name));
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

            public object GetErrorNoThrow(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return OperationFailed.Value;
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

            public object GetAmbiguous(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    throw new AmbiguousMatchException(_name);
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

        }

        class BuiltinBase<TSelfType> : FastGetBase {
            public override bool IsValid(PythonType type) {
                // only used for built-in types, we never become invalid.
                return true;
            }
        }

        class FastMethodGet<TSelfType> : BuiltinBase<TSelfType> {
            private readonly Type _type;
            private readonly BuiltinMethodDescriptor _method;

            public FastMethodGet(Type type, BuiltinMethodDescriptor method) {
                _type = type;
                _method = method;
            }

            public object GetMethod(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return _method.UncheckedGetAttribute(target);
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }
        }

        class FastSlotGet<TSelfType> : BuiltinBase<TSelfType> {
            private readonly Type _type;
            private readonly PythonTypeSlot _slot;
            private readonly PythonType _owner;

            public FastSlotGet(Type type, PythonTypeSlot slot, PythonType owner) {
                _type = type;
                _slot = slot;
                _owner = owner;
            }

            public object GetRetSlot(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return _slot;
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

            public object GetBindSlot(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    object value;
                    _slot.TryGetValue(context, target, _owner, out value);
                    return value;
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }
        }

        class FastTypeGet<TSelfType> : BuiltinBase<TSelfType> {
            private readonly Type _type;
            private readonly object _pyType;

            public FastTypeGet(Type type, object pythonType) {
                _type = type;
                _pyType = pythonType;
            }

            public object GetTypeObject(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return _pyType;
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }
        }

        class FastPropertyGet<TSelfType> : BuiltinBase<TSelfType> {
            private readonly Type _type;
            private readonly Func<object, object> _propGetter;

            public FastPropertyGet(Type type, Func<object, object> propGetter) {
                _type = type;
                _propGetter = propGetter;
            }

            public object GetProperty(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return _propGetter(target);
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

            public object GetPropertyBool(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return ScriptingRuntimeHelpers.BooleanToObject((bool)_propGetter(target));
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }

            public object GetPropertyInt(CallSite site, TSelfType target, CodeContext context) {
                if (target != null && target.GetType() == _type) {
                    return ScriptingRuntimeHelpers.Int32ToObject((int)_propGetter(target));
                }

                return ((CallSite<Func<CallSite, TSelfType, CodeContext, object>>)site).Update(site, target, context);
            }
        }

        private Func<CallSite, TSelfType, CodeContext, object> MakeGetMemberTarget<TSelfType>(string name, object target) {
            Type type = CompilerHelpers.GetType(target);

            // needed for GetMember call until DynamicAction goes away
            OldDynamicAction act = OldGetMemberAction.Make(Context.Binder, name);

            if (typeof(TypeTracker).IsAssignableFrom(type)) {
                // no fast path for TypeTrackers
                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast TypeTracker");
                return null;
            }

            MemberGroup members = Context.Binder.GetMember(act, type, name);

            if (members.Count == 0 && type.IsInterface) {
                // all interfaces have object members
                type = typeof(object);
                members = Context.Binder.GetMember(act, type, name);
            }

            if (members.Count == 0 && typeof(IStrongBox).IsAssignableFrom(type)) {
                // no fast path for strong box access
                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast StrongBox");
                return null;
            }

            MethodInfo getMem = Context.Binder.GetMethod(type, "GetCustomMember");
            if (getMem != null && getMem.IsSpecialName) {
                // no fast path for custom member access
                PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast GetCustomMember " + type);
                return null;
            }

            Expression error;
            TrackerTypes memberType = Context.Binder.GetMemberType(members, out error);

            if (error == null) {
                PythonType argType = DynamicHelpers.GetPythonTypeFromType(type);
                bool isHidden = argType.IsHiddenMember(name);
                if (isHidden) {
                    PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast FilteredMember " + memberType);
                    return null;
                }

                switch (memberType) {
                    case TrackerTypes.TypeGroup:
                    case TrackerTypes.Type:
                        object typeObj;
                        if (members.Count == 1) {
                            typeObj = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)members[0]).Type);
                        } else {
                            TypeTracker typeTracker = (TypeTracker)members[0];
                            for (int i = 1; i < members.Count; i++) {
                                typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)members[i]);
                            }
                            typeObj = typeTracker;
                        }

                        return new FastTypeGet<TSelfType>(type, typeObj).GetTypeObject;
                    case TrackerTypes.Method:
                        PythonTypeSlot slot = PythonTypeOps.GetSlot(members, name, _context.DomainManager.Configuration.PrivateBinding);
                        if (slot is BuiltinMethodDescriptor) {
                            return new FastMethodGet<TSelfType>(type, (BuiltinMethodDescriptor)slot).GetMethod;
                        } else if (slot is BuiltinFunction) {
                            return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(type)).GetRetSlot;
                        }
                        return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(type)).GetBindSlot;
                    case TrackerTypes.Event:
                        if (members.Count == 1 && !((EventTracker)members[0]).IsStatic) {
                            slot = PythonTypeOps.GetSlot(members, name, _context.DomainManager.Configuration.PrivateBinding);
                            return new FastSlotGet<TSelfType>(type, slot, DynamicHelpers.GetPythonTypeFromType(((EventTracker)members[0]).DeclaringType)).GetBindSlot;
                        }
                        PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast Event " + members.Count + " " + ((EventTracker)members[0]).IsStatic);
                        return null;
                    case TrackerTypes.Property:
                        if (members.Count == 1) {
                            PropertyTracker pt = (PropertyTracker)members[0];
                            if (!pt.IsStatic && pt.GetIndexParameters().Length == 0) {
                                MethodInfo prop = pt.GetGetMethod();

                                if (prop != null && prop.GetParameters().Length == 0) {
                                    if (prop.ReturnType == typeof(bool)) {
                                        return new FastPropertyGet<TSelfType>(type, ReflectedCaller.Create(prop).Invoke).GetPropertyBool;
                                    } else if (prop.ReturnType == typeof(int)) {
                                        return new FastPropertyGet<TSelfType>(type, ReflectedCaller.Create(prop).Invoke).GetPropertyInt;
                                    } else {
                                        return new FastPropertyGet<TSelfType>(type, ReflectedCaller.Create(prop).Invoke).GetProperty;
                                    }
                                }
                            }
                        }
                        PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast Property " + members.Count + " " + ((PropertyTracker)members[0]).IsStatic);
                        return null;
                    case TrackerTypes.All:
                        getMem = Context.Binder.GetMethod(type, "GetBoundMember");
                        if (getMem != null && getMem.IsSpecialName) {
                            PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast GetBoundMember " + type);
                            return null;
                        }

                        if (IsNoThrow) {
                            return new FastErrorGet<TSelfType>(type, name).GetErrorNoThrow;
                        } else {
                            return new FastErrorGet<TSelfType>(type, name).GetError;
                        }
                    default:
                        PerfTrack.NoteEvent(PerfTrack.Categories.BindingSlow, "GetNoFast " + memberType);
                        return null;
                }
            } else {
                StringBuilder sb = new StringBuilder();
                foreach (MemberTracker mi in members) {
                    if (sb.Length != 0) sb.Append(", ");
                    sb.Append(mi.MemberType);
                    sb.Append(" : ");
                    sb.Append(mi.ToString());
                }

                return new FastErrorGet<TSelfType>(type, sb.ToString()).GetAmbiguous;
            }
        }

        class ScopeDelegate : FastGetBase {
            private readonly string _name;

            public ScopeDelegate(string name) {
                _name = name;
            }

            public object Target(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(Scope)) {
                    return ScopeOps.__getattribute__(context, (Scope)self, _name);
                }

                return Update(site, self, context);
            }

            public object NoThrowTarget(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(Scope)) {
                    return ScopeOps.GetAttributeNoThrow(context, (Scope)self, _name);
                }

                return Update(site, self, context);
            }

            public override bool IsValid(PythonType type) {
                return true;
            }
        }

        class NamespaceTrackerDelegate : FastGetBase {
            private readonly SymbolId _name;

            public NamespaceTrackerDelegate(string name) {
                _name = SymbolTable.StringToId(name);
            }

            public object Target(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(NamespaceTracker)) {
                    object res = NamespaceTrackerOps.GetCustomMember(context, (NamespaceTracker)self, _name);
                    if (res != OperationFailed.Value) {
                        return res;
                    }

                    throw PythonOps.AttributeErrorForMissingAttribute(self, _name);
                }

                return Update(site, self, context);
            }

            public object GetName(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(NamespaceTracker)) {
                    return NamespaceTrackerOps.Get__name__(context, (NamespaceTracker)self);
                }

                return Update(site, self, context);
            }

            public object GetFile(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(NamespaceTracker)) {
                    return NamespaceTrackerOps.Get__file__((NamespaceTracker)self);
                }

                return Update(site, self, context);
            }

            public object GetDict(CallSite site, object self, CodeContext context) {
                if (self != null && self.GetType() == typeof(NamespaceTracker)) {
                    return NamespaceTrackerOps.Get__dict__(context, (NamespaceTracker)self);
                }

                return Update(site, self, context);
            }

            public override bool IsValid(PythonType type) {
                return true;
            }
        }

        private DynamicMetaObject GetForeignObject(DynamicMetaObject self) {
            return new DynamicMetaObject(
                Expression.Dynamic(
                    _context.CompatGetMember(Name),
                    typeof(object),
                    self.Expression
                ),
                self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, self.GetLimitType()))
            );
        }

        #endregion

        public DynamicMetaObject/*!*/ Fallback(DynamicMetaObject/*!*/ self, DynamicMetaObject/*!*/ codeContext) {
            // Python always provides an extra arg to GetMember to flow the context.
            return FallbackWorker(self, codeContext, Name, _options, this, null);
        }

        internal static DynamicMetaObject FallbackWorker(DynamicMetaObject/*!*/ self, DynamicMetaObject/*!*/ codeContext, string name, GetMemberOptions options, DynamicMetaObjectBinder action, DynamicMetaObject errorSuggestion) {
            if (self.NeedsDeferral()) {
                return action.Defer(self);
            }

            PerfTrack.NoteEvent(PerfTrack.Categories.BindingTarget, "FallbackGet");

            bool isNoThrow = ((options & GetMemberOptions.IsNoThrow) != 0) ? true : false;
            Type limitType = self.GetLimitType();

            if (limitType == typeof(DynamicNull) || PythonBinder.IsPythonType(limitType)) {
                // look up in the PythonType so that we can 
                // get our custom method names (e.g. string.startswith)            
                PythonType argType = DynamicHelpers.GetPythonTypeFromType(limitType);

                // if the name is defined in the CLS context but not the normal context then
                // we will hide it.                
                if (argType.IsHiddenMember(name)) {
                    DynamicMetaObject baseRes = PythonContext.GetPythonContext(action).Binder.GetMember(
                        name,
                        self,
                        codeContext.Expression,
                        isNoThrow,
                        errorSuggestion
                    );
                    Expression failure = GetFailureExpression(limitType, name, isNoThrow, action);

                    return BindingHelpers.FilterShowCls(codeContext, action, baseRes, failure);
                }
            }

            if (self.GetLimitType() == typeof(OldInstance)) {
                if ((options & GetMemberOptions.IsNoThrow) != 0) {
                    return new DynamicMetaObject(
                        Ast.Field(
                            null,
                            typeof(OperationFailed).GetField("Value")
                        ),
                        self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(OldInstance)))
                    );
                } else {
                    return new DynamicMetaObject(
                        Ast.Throw(
                            Ast.Call(
                                typeof(PythonOps).GetMethod("AttributeError"),
                                AstUtils.Constant("{0} instance has no attribute '{1}'"),
                                Ast.NewArrayInit(
                                    typeof(object),
                                    AstUtils.Constant(((OldInstance)self.Value)._class._name),
                                    AstUtils.Constant(name)
                                )
                            )
                        ),
                        self.Restrictions.Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(self.Expression, typeof(OldInstance)))
                    );
                }
            }

            var res = PythonContext.GetPythonContext(action).Binder.GetMember(name, self, codeContext.Expression, isNoThrow, errorSuggestion);

            // Default binder can return something typed to boolean or int.
            // If that happens, we need to apply Python's boxing rules.
            if (res.Expression.Type == typeof(bool) || res.Expression.Type == typeof(int)) {
                res = new DynamicMetaObject(
                    AstUtils.Convert(res.Expression, typeof(object)),
                    res.Restrictions
                );
            }

            return res;
        }

        private static Expression/*!*/ GetFailureExpression(Type/*!*/ limitType, string name, bool isNoThrow, DynamicMetaObjectBinder action) {
            return isNoThrow ?
                Ast.Field(null, typeof(OperationFailed).GetField("Value")) :
                DefaultBinder.MakeError(
                    PythonContext.GetPythonContext(action).Binder.MakeMissingMemberError(
                        limitType,
                        name
                    ),
                    typeof(object)
                );
        }

        public string Name {
            get {
                return _name;
            }
        }

        public PythonContext/*!*/ Context {
            get {
                return _context;
            }
        }

        public bool IsNoThrow {
            get {
                return (_options & GetMemberOptions.IsNoThrow) != 0;
            }
        }

        public override int GetHashCode() {
            return _name.GetHashCode() ^ _context.Binder.GetHashCode() ^ ((int)_options);
        }

        public override bool Equals(object obj) {
            PythonGetMemberBinder ob = obj as PythonGetMemberBinder;
            if (ob == null) {
                return false;
            }

            return ob._context.Binder == _context.Binder &&
                ob._options == _options &&
                ob._name == _name;
        }

        public override string ToString() {
            return String.Format("Python GetMember {0} IsNoThrow: {1}", Name, _options);
        }

        #region IExpressionSerializable Members

        public Expression CreateExpression() {
            return Ast.Call(
                typeof(PythonOps).GetMethod("MakeGetAction"),
                BindingHelpers.CreateBinderStateExpression(),
                AstUtils.Constant(Name),
                AstUtils.Constant(IsNoThrow)
            );
        }

        #endregion
    }

    class CompatibilityGetMember : GetMemberBinder, IPythonSite {
        private readonly PythonContext/*!*/ _state;

        public CompatibilityGetMember(PythonContext/*!*/ binder, string/*!*/ name)
            : base(name, false) {
            _state = binder;
        }

        public CompatibilityGetMember(PythonContext/*!*/ binder, string/*!*/ name, bool ignoreCase)
            : base(name, ignoreCase) {
            _state = binder;
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject self, DynamicMetaObject errorSuggestion) {
#if !SILVERLIGHT
            DynamicMetaObject com;
            if (System.Dynamic.ComBinder.TryBindGetMember(this, self, out com, true)) {
                return com;
            }
#endif
            return PythonGetMemberBinder.FallbackWorker(self, PythonContext.GetCodeContextMOCls(this), Name, GetMemberOptions.None, this, errorSuggestion);
        }

        #region IPythonSite Members

        public PythonContext Context {
            get { return _state; }
        }

        #endregion

        public override int GetHashCode() {
            return base.GetHashCode() ^ _state.Binder.GetHashCode();
        }

        public override bool Equals(object obj) {
            CompatibilityGetMember ob = obj as CompatibilityGetMember;
            if (ob == null) {
                return false;
            }

            return ob._state.Binder == _state.Binder &&
                base.Equals(obj);
        }
    }

    [Flags]
    enum GetMemberOptions {
        None,
        IsNoThrow = 0x01,
        IsCaseInsensitive = 0x02
    }
}
