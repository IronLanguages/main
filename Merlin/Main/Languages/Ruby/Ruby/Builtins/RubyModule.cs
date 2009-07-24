/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {
    [Flags]
    public enum ModuleRestrictions {
        None = 0,

        /// <summary>
        /// Module doesn't allow its methods to be overridden.
        /// Used for built-ins, except for Object.
        /// </summary>
        NoOverrides = 1,

        /// <summary>
        /// Module doesn't allow its methods to be called by mangled names.
        /// Used for built-ins.
        /// </summary>
        NoNameMangling = 2,

        /// <summary>
        /// Default restrictions for built-in modules.
        /// </summary>
        Builtin = NoOverrides | NoNameMangling,
    }

    [Flags]
    public enum RubyModuleAttributes {
        None = 0,

        NoOverrides = ModuleRestrictions.NoOverrides,
        NoNameMangling = ModuleRestrictions.NoNameMangling,
        RestrictionsMask = NoOverrides | NoNameMangling,

        IsSelfContained = 0x100,
    }

    [Flags]
    public enum MethodLookup {
        Default = 0,
        Virtual = 1,
        ReturnForwarder = 2,
        FallbackToObject = 4,
    }

#if DEBUG
    [DebuggerDisplay("{DebugName}")]
#endif
    public partial class RubyModule : IDuplicable, IRubyObject {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly RubyModule[]/*!*/ EmptyArray = new RubyModule[0];

        private enum State {
            Uninitialized,
            Initializing,
            Initialized
        }

        private readonly RubyContext/*!*/ _context;

        // the type this module represents or null:
        private readonly TypeTracker _typeTracker;

        // the namespace this module represents or null:
        private readonly NamespaceTracker _namespaceTracker;

        private readonly ModuleRestrictions _restrictions;
        
        // name of the module or null for anonymous modules:
        private string _name;

        // lazy interlocked init:
        private RubyClass _singletonClass;

        private RubyInstanceData _instanceData;

        #region Mutable state guarded by ClassHierarchyLock

        // List of dependent modules (forms a DAG).
        private List<WeakReference/*!*/> _dependentClasses = new List<WeakReference>();        
        
#if DEBUG
        private int _referringMethodRulesSinceLastUpdate;
        private int _debugId = Interlocked.Increment(ref _DebugId);
        private static int _DebugId;
        
        public string DebugName { 
            get {
                string name;
                if (IsSingletonClass) {
                    object s = ((RubyClass)this).SingletonClassOf;
                    RubyModule m = s as RubyModule;
                    if (m != null) {
                        name = m.DebugName;
                    } else {
                        name = RuntimeHelpers.GetHashCode(s).ToString("x");
                    }
                    name = "S(" + name + ")";
                } else {
                    name = _name;
                }
                return name + " #" + _debugId;
            }
        }
#endif

        private enum MemberTableState {
            Uninitialized = 0,
            Initializing = 1,
            Initialized = 2
        }

        // constant table:
        private MemberTableState _constantsState = MemberTableState.Uninitialized;
        private Action<RubyModule> _constantsInitializer;
        private Dictionary<string, object> _constants;  // null means lookup the execution context's global namespace 
        
        // method table:
        private MemberTableState _methodsState = MemberTableState.Uninitialized;
        private Dictionary<string, RubyMemberInfo> _methods;
        private Action<RubyModule> _methodsInitializer;

        // Set of names that method_missing defined on this module (if applicable) was resolved for and that are cached. Lazy init.
        internal Dictionary<string, bool> MissingMethodsCachedInSites { get; set; }

        // class variable table:
        private Dictionary<string, object> _classVariables;

        //
        // The entire list of modules included in this one. Newly-added mixins are at the front of the array.
        // When adding a module that itself contains other modules, Ruby tries to maintain the ordering of the
        // contained modules so that method resolution is reasonably consistent.
        //
        // MRO walk: this, _mixins[0], _mixins[1], ..., _mixins[n-1], super, ...
        private RubyModule[]/*!*/ _mixins;

        #endregion

        #region Dynamic Sites

        // RubyModule, SymbolId -> object
        private CallSite<Func<CallSite, object, object, object>> _constantMissingCallbackSite;
        private CallSite<Func<CallSite, object, object, object>> _methodAddedCallbackSite;
        private CallSite<Func<CallSite, object, object, object>> _methodRemovedCallbackSite;
        private CallSite<Func<CallSite, object, object, object>> _methodUndefinedCallbackSite;

        internal object ConstantMissing(string/*!*/ name) {
            return Context.Send(ref _constantMissingCallbackSite, "const_missing", this, name);
        }

        // Ruby 1.8: called after method is added, except for alias_method which calls it before
        // Ruby 1.9: called before method is added
        public virtual void MethodAdded(string/*!*/ name) {
            Assert.NotNull(name);
            Debug.Assert(!IsSingletonClass);

            Context.Send(ref _methodAddedCallbackSite, Symbols.MethodAdded, this, name);
        }

        internal virtual void MethodRemoved(string/*!*/ name) {
            Assert.NotNull(name);
            Debug.Assert(!IsSingletonClass);

            Context.Send(ref _methodRemovedCallbackSite, Symbols.MethodRemoved, this, name);
        }

        internal virtual void MethodUndefined(string/*!*/ name) {
            Assert.NotNull(name);
            Debug.Assert(!IsSingletonClass);

            Context.Send(ref _methodUndefinedCallbackSite, Symbols.MethodUndefined, this, name);
        }

        #endregion

        public TypeTracker TypeTracker {
            get { return _typeTracker; }
        }

        public NamespaceTracker NamespaceTracker {
            get { return _namespaceTracker; }
        }

        public bool IsInterface {
            get { return _typeTracker != null && _typeTracker.Type.IsInterface; }
        }

        public virtual Type/*!*/ GetUnderlyingSystemType() {
            if (IsInterface) {
                return _typeTracker.Type;
            } else {
                throw new InvalidOperationException();
            }
        }

        public RubyClass/*!*/ SingletonClass {
            get {
                Debug.Assert(_singletonClass != null);
                return _singletonClass;
            }
        }

        public bool IsDummySingletonClass {
            get { return ReferenceEquals(_singletonClass, this); }
        }

        public virtual bool IsSingletonClass {
            get { return false; }
        }

        public ModuleRestrictions Restrictions {
            get { return _restrictions; }
        }

        internal RubyModule[]/*!*/ Mixins {
            get { return _mixins; }
        }

        public string Name {
            get { return _name; }
            internal set { _name = value; }
        }

        public RubyContext/*!*/ Context {
            [Emitted]
            get { return _context; }
        }

        public virtual bool IsClass {
            get { return false; }
        }

        public bool IsComClass {
            get { return ReferenceEquals(this, Context.ComObjectClass); }
        }

        internal virtual RubyClass GetSuperClass() {
            return null;
        }

        private bool DeclaresGlobalConstants {
            get { return ReferenceEquals(this, _context.ObjectClass); }
        }

        internal virtual RubyGlobalScope GlobalScope {
            get { return null; }
        }

        // default allocator:
        public RubyModule(RubyClass/*!*/ rubyClass) 
            : this(rubyClass, null) {
        }

        // creates an empty module:
        protected RubyModule(RubyClass/*!*/ metaModuleClass, string name)
            : this(metaModuleClass.Context, name, null, null, null, null, null, ModuleRestrictions.None) {

            // all modules need a singleton (see RubyContext.CreateModule):
            InitializeDummySingletonClass(metaModuleClass, null);
        }

        internal RubyModule(RubyContext/*!*/ context, string name, Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[] expandedMixins, NamespaceTracker namespaceTracker, TypeTracker typeTracker, ModuleRestrictions restrictions) {

            Assert.NotNull(context);
            Debug.Assert(namespaceTracker == null || typeTracker == null);
            Debug.Assert(expandedMixins == null ||
                CollectionUtils.TrueForAll(expandedMixins, (m) => m != this && m != null && !m.IsClass && m.Context == context)
            );

            _context = context;
            _name = name;
            _methodsInitializer = methodsInitializer;
            _constantsInitializer = constantsInitializer;
            _namespaceTracker = namespaceTracker;
            _typeTracker = typeTracker;
            _mixins = expandedMixins ?? EmptyArray;
            _restrictions = restrictions;
        }

        #region Initialization (thread-safe)

        internal bool ConstantInitializationNeeded {
            get { return _constantsState == MemberTableState.Uninitialized; }
        }

        private void InitializeConstantTableNoLock() {
            if (!ConstantInitializationNeeded) return;

            if (!DeclaresGlobalConstants) {
                _constants = new Dictionary<string, object>();
            }
            _constantsState = MemberTableState.Initializing;

            try {
                if (_constantsInitializer != EmptyInitializer) {
                    if (_constantsInitializer != null) {
                        Utils.Log(_name ?? "<anonymous>", "CT_INIT");
                        // TODO: use lock-free operations in initializers
                        _constantsInitializer(this);
                    } else if (!IsInterface && _typeTracker != null) {
                        // Load types eagerly. We do this only for CLR types that have no constant initializer (not builtins) and 
                        // a constant access is performed (otherwise this method wouldn't be called).
                        // 
                        // Note: Interfaces cannot declare nested types in C#, we follow the suit here.
                        // We don't currently need this restriction but once we implement generic type overload inheritance properly
                        // we would need to deal with inheritance from interfaces, which might be too complex. 
                        // 
                        LoadNestedTypes();
                    }
                }
            } finally {
                _constantsInitializer = null;
                _constantsState = MemberTableState.Initialized;
            }
        }

        internal static readonly Action<RubyModule> EmptyInitializer = (_) => { };

        internal bool MethodInitializationNeeded {
            get { return _methodsState == MemberTableState.Uninitialized; }
        }

        private void InitializeMethodTableNoLock() {
            if (!MethodInitializationNeeded) return;

            InitializeDependencies();

            _methods = new Dictionary<string, RubyMemberInfo>();
            _methodsState = MemberTableState.Initializing;

            try {
                if (_methodsInitializer != null) {
                    Utils.Log(_name ?? "<anonymous>", "MT_INIT");
                    // TODO: use lock-free operations in initializers?
                    _methodsInitializer(this);
                }
            } finally {
                _methodsInitializer = null;
                _methodsState = MemberTableState.Initialized;
            }
        }

        internal void InitializeMethodsNoLock() {
            if (MethodInitializationNeeded) {
                InitializeMethodsNoLock(GetUninitializedAncestors(true));
            }
        }

        internal void InitializeMethodsNoLock(IList<RubyModule/*!*/>/*!*/ modules) {
            for (int i = modules.Count - 1; i >= 0; i--) {
                modules[i].InitializeMethodTableNoLock();
            }
        }

        internal void InitializeConstantsNoLock() {
            if (ConstantInitializationNeeded) {
                InitializeConstantsNoLock(GetUninitializedAncestors(false));
            }
        }

        internal void InitializeConstantsNoLock(IList<RubyModule/*!*/>/*!*/ modules) {
            for (int i = modules.Count - 1; i >= 0; i--) {
                modules[i].InitializeConstantTableNoLock();
            }
        }

        private List<RubyModule>/*!*/ GetUninitializedAncestors(bool methods) {
            var result = new List<RubyModule>();
            result.Add(this);
            result.AddRange(_mixins);
            var super = GetSuperClass();
            while (super != null && (methods ? super.MethodInitializationNeeded : super.ConstantInitializationNeeded)) {
                result.Add(super);
                result.AddRange(super._mixins);
                super = super.SuperClass;
            }
            return result;
        }

        private void InitializeClassVariableTable() {
            if (_classVariables == null) {
                Interlocked.CompareExchange(ref _classVariables, new Dictionary<string, object>(), null);
            }
        }

        private void LoadNestedTypes() {
            Context.RequiresClassHierarchyLock();
            Debug.Assert(_constants != null && _constants.Count == 0);

            // TODO: Inherited generic overloads. We need a custom TypeGroup to do it right - part of the type group might be removed

            // TODO: protected types
            var bindingFlags = BindingFlags.Public | BindingFlags.DeclaredOnly;
            if (Context.DomainManager.Configuration.PrivateBinding) {
                bindingFlags |= BindingFlags.NonPublic;
            }

            // if the constant is redefined/removed from the base class. This is similar to method overload inheritance.
            Type[] types = _typeTracker.Type.GetNestedTypes(bindingFlags);
            var trackers = new List<TypeTracker>();
            var names = new List<string>();
            foreach (var type in types) {
                TypeTracker tracker = (NestedTypeTracker)MemberTracker.FromMemberInfo(type);

                if (type.IsGenericType) {
                    var name = ReflectionUtils.GetNormalizedTypeName(type);
                    int index = names.IndexOf(name);
                    if (index != -1) {
                        trackers[index] = TypeGroup.UpdateTypeEntity(trackers[index], tracker);
                        names[index] = name;
                    } else {
                        trackers.Add(tracker);
                        names.Add(name);
                    }
                } else {
                    trackers.Add(tracker);
                    names.Add(type.Name);
                }
            }

            for (int i = 0; i < trackers.Count; i++) {
                var tracker = trackers[i];
                _constants[names[i]] = (tracker is TypeGroup) ? tracker : (object)Context.GetModule(tracker.Type); 
            }
        }

        internal void InitializeMembersFrom(RubyModule/*!*/ module) {
            Context.RequiresClassHierarchyLock();
            Mutate();

            Assert.NotNull(module);

#if !SILVERLIGHT // missing Clone on Delegate
            if (module.DeclaresGlobalConstants || module._namespaceTracker != null && _constants == null) {
#endif
                // initialize the module so that we can copy all constants from it:
                module.InitializeConstantsNoLock();

                // initialize all ancestors of self:
                InitializeConstantsNoLock();
#if !SILVERLIGHT
            } else {
                _constantsInitializer = (module._constantsInitializer != null) ? (Action<RubyModule>)module._constantsInitializer.Clone() : null;
                _constantsState = module._constantsState;
            }
#endif

            if (module.DeclaresGlobalConstants) {
                Debug.Assert(module._constants == null && module._namespaceTracker == null);
                Debug.Assert(_constants != null);
                _constants.Clear();
                foreach (KeyValuePair<SymbolId, object> constant in _context.TopGlobalScope.Items) {
                    _constants.Add(SymbolTable.IdToString(constant.Key), constant.Value);
                }
            } else {
                _constants = (module._constants != null) ? new Dictionary<string, object>(module._constants) : null;

                // copy namespace members:
                if (module._namespaceTracker != null) {
                    Debug.Assert(_constants != null);
                    foreach (KeyValuePair<SymbolId, object> constant in module._namespaceTracker.SymbolAttributes) {
                        _constants.Add(SymbolTable.IdToString(constant.Key), constant.Value);
                    }
                }
            }

#if SILVERLIGHT
             module.InitializeMethodsNoLock();
             InitializeMethodsNoLock();
#else
            _methodsInitializer = (module._methodsInitializer != null) ? (Action<RubyModule>)module._methodsInitializer.Clone() : null;
            _methodsState = module._methodsState;
#endif
            _methods = (module._methods != null) ? new Dictionary<string, RubyMemberInfo>(module._methods) : null;

            _classVariables = (module._classVariables != null) ? new Dictionary<string, object>(module._classVariables) : null;
            _mixins = ArrayUtils.Copy(module._mixins);

            // dependentModules - skip
            // tracker - skip, .NET members not copied
            
            // TODO:
            // - handle overloads cached in groups
            // - version updates
            Updated("InitializeFrom");
        }

        public void InitializeModuleCopy(RubyModule/*!*/ module) {
            if (_context.IsObjectFrozen(this)) {
                throw RubyExceptions.CreateTypeError("can't modify frozen Module");
            }

            using (Context.ClassHierarchyLocker()) {
                InitializeMembersFrom(module);
            }
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            // the meta-module class of this module is the singleton's super class:
            RubyModule result = new RubyModule(_singletonClass.SuperClass, null);
                
            // singleton members are copied here, not in InitializeCopy:
            if (copySingletonMembers && !IsSingletonClass) {
                using (Context.ClassHierarchyLocker()) {
                    result.SingletonClass.InitializeMembersFrom(SingletonClass);
                }
            }

            // copy instance variables:
            _context.CopyInstanceData(this, result, false);
            return result;
        }

        #endregion

        #region Versioning (thread-safe)

        [Conditional("DEBUG")]
        internal void OwnedMethodCachedInSite() {
            Context.RequiresClassHierarchyLock();
#if DEBUG
            _referringMethodRulesSinceLastUpdate++;
#endif
        }

        internal virtual void InitializeDependencies() {
            // nop
        }

        internal IEnumerable<RubyClass>/*!*/ GetDependentClasses() {
            int deadCount = 0;
            for (int i = 0; i < _dependentClasses.Count; i++) {
                object cls = _dependentClasses[i].Target;
                if (cls != null) {
                    yield return (RubyClass)cls;
                } else {
                    deadCount++;
                }
            }

            PruneDependencies(deadCount);
        }

        internal void AddDependentClass(RubyClass/*!*/ dependentClass) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(dependentClass);

            foreach (var cls in GetDependentClasses()) {
                if (ReferenceEquals(dependentClass, cls)) {
                    return;
                }
            }

            _dependentClasses.Add(dependentClass.WeakSelf);
        }

        internal void Updated(string/*!*/ reason) {
            Context.RequiresClassHierarchyLock();
         
            int affectedModules = 0;
            int affectedRules = 0;
            Updated(ref affectedModules, ref affectedRules);

            Utils.Log(String.Format("{0,-50} {1,-30} affected={2,-5} rules={3,-5}", Name, reason, affectedModules, affectedRules), "UPDATED");
        }

        private void Updated(ref int affectedModules, ref int affectedRules) {
            Context.RequiresClassHierarchyLock();

            IncrementVersion();
#if DEBUG
            affectedModules++;
            affectedRules += _referringMethodRulesSinceLastUpdate;
            _referringMethodRulesSinceLastUpdate = 0;
#endif

            // Updates dependent classes. If dependent classes haven't been initialized yet we don't need to follow them.
            // TODO (opt): stop updating if a module that defines a method of the same name is reached.
            foreach (var cls in GetDependentClasses()) {
                cls.Updated(ref affectedModules, ref affectedRules);
            } 
        }

        internal virtual void IncrementVersion() {
            // nop
        }

        private void PruneDependencies(int estimatedDeadCount) {
            Context.RequiresClassHierarchyLock();

            // deadCount is greated than 1/5 of total => remove dead (the threshold is arbitrary, might need tuning):
            if (estimatedDeadCount * 5 < _dependentClasses.Count) {
                return;
            }

            int i = 0, j = 0;
            while (i < _dependentClasses.Count) {
                if (!_dependentClasses[i].IsAlive) {
                    if (j != i) {
                        _dependentClasses[j] = _dependentClasses[i];
                    }
                    j++;
                }
                i++;
            }

            if (j < i) {
                _dependentClasses.RemoveRange(j, i - j);
                _dependentClasses.TrimExcess();
            }
        }

        // A version of a frozen module can still change if its super-classes/mixins change.
        private void Mutate() {
            if (IsFrozen) {
                throw RubyExceptions.CreateTypeError(String.Format("can't modify frozen {0}", IsClass ? "class" : "module"));
            }
        }

        #endregion

        #region IRubyObject Members

        // thread-safe:
        public RubyClass ImmediateClass {
            get {
                return _singletonClass;
            }
            set {
                // all modules have singleton classes initialized at the creation time, these cannot be changed:
                throw Assert.Unreachable;
            }
        }

        // thread-safe:
        public RubyInstanceData TryGetInstanceData() {
            return _instanceData;
        }

        // thread-safe:
        public RubyInstanceData GetInstanceData() {
            return RubyOps.GetInstanceData(ref _instanceData);
        }

        // thread-safe:
        public virtual bool IsFrozen {
            get { return IsModuleFrozen; }
        }

        // thread-safe: _instanceData cannot be unset
        internal bool IsModuleFrozen {
            get { return _instanceData != null && _instanceData.Frozen; }
        }

        // thread-safe:
        public void Freeze() {
            GetInstanceData().Freeze();
        }

        // thread-safe:
        public bool IsTainted {
            get { return GetInstanceData().Tainted; }
            set { GetInstanceData().Tainted = value; }
        }

        public int BaseGetHashCode() {
            return base.GetHashCode();
        }

        public bool BaseEquals(object other) {
            return base.Equals(other);
        }

        #endregion

        #region Factories (thread-safe)

        // Ruby constructor:
        public static object CreateAnonymousModule(RubyScope/*!*/ scope, BlockParam body, RubyClass/*!*/ self) {
            RubyModule newModule = new RubyModule(self, null);
            return (body != null) ? RubyUtils.EvaluateInModule(newModule, body, newModule) : newModule;
        }

        // thread safe:
        public RubyClass/*!*/ CreateSingletonClass() {
            Debug.Assert(!IsDummySingletonClass);

            var singleton = _singletonClass;
            if (singleton.IsDummySingletonClass) {
                RubyClass super = ((RubyModule)singleton.SingletonClassOf).IsClass ? Context.ClassClass.SingletonClass : Context.ModuleClass.SingletonClass;
                RubyClass newDummy = CreateDummySingletonClass(super, Context.SingletonSingletonTrait);
                
                // update singleton only if it still points to itself:
                Interlocked.CompareExchange(ref singleton._singletonClass, newDummy, singleton);
            }

            Debug.Assert(_singletonClass.IsSingletonClass && !_singletonClass.IsDummySingletonClass);
            return _singletonClass;
        }

        // thread safe:
        internal void InitializeDummySingletonClass(RubyClass/*!*/ superClass, Action<RubyModule> trait) {
            // if multiple threads are trying to set the singleton, the first one should win:
            var previous = Interlocked.CompareExchange(ref _singletonClass, CreateDummySingletonClass(superClass, trait), null);
            Debug.Assert(previous == null);
        }

        // thread safe:
        private RubyClass/*!*/ CreateDummySingletonClass(RubyClass/*!*/ superClass, Action<RubyModule> trait) {
            // Note that in MRI, member tables of dummy singleton are shared with the class the dummy is singleton for
            // This is obviously an implementation detail leaking to the language and we don't support that.

            // real class object and it's singleton share the tracker:
            TypeTracker tracker = (IsSingletonClass) ? null : _typeTracker;

            // Singleton should have the same restrictions as the module it is singleton for.
            // Reason: We need static methods of builtins (e.g. Object#Equals) not to be exposed under Ruby names (Object#equals).
            // We also want static methods of non-builtins to be visible under both CLR and Ruby names. 
            var result = new RubyClass(
                Context, null, null, this, trait, null, null, superClass, null, tracker, null, false, true, this.Restrictions
            );
#if DEBUG
            result.Version.SetName(result.DebugName);
#endif
            result._singletonClass = result;
            
            // MRI: 
            return result;
        }

        #endregion

        #region Ancestors (thread-safe)

        // Return true from action to terminate enumeration.
        public bool ForEachAncestor(bool inherited, Func<RubyModule/*!*/, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();
            
            if (inherited) {
                return ForEachAncestor(action);
            } else {
                return ForEachDeclaredAncestor(action);
            }
        }

        internal virtual bool ForEachAncestor(Func<RubyModule/*!*/, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            return ForEachDeclaredAncestor(action);
        }

        internal bool ForEachDeclaredAncestor(Func<RubyModule/*!*/, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            // this module:
            if (action(this)) return true;

            // mixins:
            foreach (RubyModule m in _mixins) {
                if (action(m)) return true;
            }

            return false;
        }

        #endregion

        #region Constants (thread-safe)

        // Value of constant that is to be auto-loaded on first use.
        private sealed class AutoloadedConstant {
            private readonly MutableString/*!*/ _path;
            private bool _loaded;

            // File already loaded? An auto-loaded constant can be referenced by mutliple classes (via duplication).
            // After the constant is accessed in one of them and the file is loaded access to the other duplicates doesn't trigger file load.
            public bool Loaded { get { return _loaded; } }
            public MutableString/*!*/ Path { get { return _path; } }

            public AutoloadedConstant(MutableString/*!*/ path) {
                Assert.NotNull(path);
                Debug.Assert(path.IsFrozen);
                _path = path;
            }

            public bool Load(RubyGlobalScope/*!*/ autoloadScope) {
                if (_loaded) {
                    return false;
                }

                 using (autoloadScope.Context.ClassHierarchyUnlocker()) {
                     _loaded = true;
                     return autoloadScope.Context.Loader.LoadFile(autoloadScope.Scope, null, _path, LoadFlags.LoadOnce | LoadFlags.AppendExtensions);
                 }
            }
        }

        // A singleton stored in constant table when CLR nested type or namespace member is removed.
        private static readonly object RemovedConstant = new object();

        public string/*!*/ MakeNestedModuleName(string nestedModuleSimpleName) {
            return (ReferenceEquals(this, _context.ObjectClass) || nestedModuleSimpleName == null) ?
                nestedModuleSimpleName :
                _name + "::" + nestedModuleSimpleName;
        }

        // not thread-safe
        public void ForEachConstant(bool inherited, Func<RubyModule/*!*/, string/*!*/, object, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {
                // notification that we entered the module (it could have no constant):
                if (action(module, null, Missing.Value)) return true;

                return module.EnumerateConstants(action);
            });
        }

        // thread-safe:
        public void SetConstant(string/*!*/ name, object value) {
            using (Context.ClassHierarchyLocker()) {
                SetConstantNoLock(name, value);
            }
        }

        // thread-safe:
        public void SetBuiltinConstant(string/*!*/ name, object value) {
            // TODO: hoist the lock?
            using (Context.ClassHierarchyLocker()) {
                SetConstantNoMutateNoLock(name, value);
            }
        }

        private void SetConstantNoLock(string/*!*/ name, object value) {
            Mutate();
            SetConstantNoMutateNoLock(name, value);
        }

        private void SetConstantNoMutateNoLock(string/*!*/ name, object value) {
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();

            if (DeclaresGlobalConstants) {
                _context.SetGlobalConstant(name, value);
            } else {
                _constants[name] = value;
            }

            // TODO: we don't do dynamic constant lookup, so there is no need to update the class
            // Updated("SetConstant");
        }

        /// <summary>
        /// Sets constant of this module. 
        /// Returns true if the constant is already defined in the module and it is not an autoloaded constant.
        /// </summary>
        /// <remarks>
        /// Thread safe.
        /// </remarks>
        public bool SetConstantChecked(string/*!*/ name, object value) {
            using (Context.ClassHierarchyLocker()) {
                object existing;
                var result = TryLookupConstantNoLock(false, false, null, name, out existing);
                SetConstantNoLock(name, value);
                return result == ConstantLookupResult.Found;
            }
        }
        
        // thread-safe:
        public void SetAutoloadedConstant(string/*!*/ name, MutableString/*!*/ path) {
            object dummy;
            if (!TryGetConstantNoAutoload(name, out dummy)) {
                SetConstant(name, new AutoloadedConstant(MutableString.Create(path).Freeze()));
            }
        }

        // thread-safe:
        public MutableString GetAutoloadedConstantPath(string/*!*/ name) {
            using (Context.ClassHierarchyLocker()) {
                object value;
                AutoloadedConstant autoloaded;
                return (TryGetConstantNoAutoloadCheck(name, out value)
                    && (autoloaded = value as AutoloadedConstant) != null
                    && !autoloaded.Loaded) ?
                    autoloaded.Path : null;
            }
        }

        /// <summary>
        /// Get constant defined in this module. Do not autoload. Value is null for autoloaded constant.
        /// </summary>
        /// <remarks>
        /// Thread safe.
        /// </remarks>
        public bool TryGetConstantNoAutoload(string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryGetConstantNoLock(null, name, out value);
            }
        }

        internal bool TryGetConstant(RubyContext/*!*/ callerContext, RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            return callerContext != Context ?
                TryGetConstant(autoloadScope, name, out value) :
                TryGetConstantNoLock(autoloadScope, name, out value);
        }

        /// <summary>
        /// Get constant defined in this module.
        /// </summary>
        public bool TryGetConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryGetConstantNoLock(autoloadScope, name, out value);
            }
        }

        /// <summary>
        /// Get constant defined in this module.
        /// </summary>
        public bool TryGetConstantNoLock(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();
            return TryLookupConstantNoLock(false, false, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        /// <summary>
        /// Get constant defined in this module or any of its ancestors. Do not autoload. Value is null for autoloaded constant.
        /// </summary>
        /// <remarks>
        /// Thread safe.
        /// </remarks>
        public bool TryResolveConstantNoAutoload(string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryResolveConstantNoLock(null, name, out value);
            }
        }

        internal bool TryResolveConstant(RubyContext/*!*/ callerContext, RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            return callerContext != Context ?
                TryResolveConstant(autoloadScope, name, out value) :
                TryResolveConstantNoLock(autoloadScope, name, out value);
        }

        /// <summary>
        /// Get constant defined in this module or any of its ancestors.
        /// </summary>
        public bool TryResolveConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryResolveConstantNoLock(autoloadScope, name, out value);
            }
        }        

        /// <summary>
        /// Get constant defined in this module or any of its ancestors.
        /// </summary>
        public bool TryResolveConstantNoLock(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();
            return TryLookupConstantNoLock(true, true, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        private enum ConstantLookupResult {
            NotFound = 0,
            Found = 1,
            FoundAutoload = 2,
        }

        private ConstantLookupResult TryLookupConstantNoLock(bool included, bool inherited, RubyGlobalScope autoloadScope, 
            string/*!*/ name, out object value) {

            Context.RequiresClassHierarchyLock();
            Debug.Assert(included || !inherited);

            if (autoloadScope != null && autoloadScope.Context != Context) {
                throw RubyExceptions.CreateTypeError(String.Format("Cannot autoload constatns to a foreign runtime #{0}", autoloadScope.Context.RuntimeId));
            }

            value = null;
            while (true) {
                object result;

                RubyModule owner = included ? 
                    TryResolveConstantNoAutoloadCheck(inherited, name, out result) :
                    (TryGetConstantNoAutoloadCheck(name, out result) ? this : null);

                if (owner == null) {
                    return ConstantLookupResult.NotFound;
                }

                var autoloaded = result as AutoloadedConstant;
                if (autoloaded == null) {
                    value = result;
                    return ConstantLookupResult.Found;
                }

                if (autoloadScope == null) {
                    return ConstantLookupResult.FoundAutoload;
                }

                // autoloaded constants are removed before the associated file is loaded:
                object _;
                owner.TryRemoveConstantNoLock(name, out _);
                               
                // load file and try lookup again (releases the class hierarchy lock when loading the file):
                if (!autoloaded.Load(autoloadScope)) {
                    return ConstantLookupResult.NotFound;
                }
            }
        }

        // Returns the owner of the constant or null if the constant is not found.
        private RubyModule TryResolveConstantNoAutoloadCheck(bool inherited, string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();

            object result = null; // C# closure doesn't capture "out" parameters
            RubyModule owner = null;
            if (ForEachAncestor(inherited, (module) => (owner = module).TryGetConstantNoAutoloadCheck(name, out result))) {
                value = result;
                return owner;
            } else {
                value = null;
                return null;
            }
        }

        // Returns the owner of the constant (this module) or null if the constant is not found.
        private bool TryGetConstantNoAutoloadCheck(string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();

            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _namespaceTracker == null);

                // call to the host => release the lock:
                using (Context.ClassHierarchyUnlocker()) {
                    if (_context.TryGetGlobalConstant(name, out value)) {
                        value = TrackerToModule(value);
                        return true;
                    }
                }

                return false;
            }

            return TryGetLocalConstant(name, out value);
        }

        private bool TryGetLocalConstant(string/*!*/ name, out object value) {
            Debug.Assert(!DeclaresGlobalConstants);
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();

            if (_constants.TryGetValue(name, out value)) {
                if (value == RemovedConstant) {
                    value = null;
                    return false;
                }
                return true;
            }

            if (_namespaceTracker != null) {
                if (_namespaceTracker.TryGetValue(SymbolTable.StringToId(name), out value)) {
                    value = TrackerToModule(value);
                    return true;
                }
            } 

            value = null;
            return false;
        }

        // thread-safe:
        public bool TryRemoveConstant(string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryRemoveConstantNoLock(name, out value);
            }
        }

        private bool TryRemoveConstantNoLock(string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();

            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _namespaceTracker == null);
                SymbolId symbol = SymbolTable.StringToId(name);

                using (Context.ClassHierarchyUnlocker()) {
                    return _context.TopGlobalScope.TryGetName(symbol, out value) 
                        && _context.TopGlobalScope.TryRemoveName(symbol);
                }
            }

            return TryRemoveLocalConstant(name, out value);
        }

        private bool TryRemoveLocalConstant(string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();
            Debug.Assert(!DeclaresGlobalConstants);

            InitializeConstantsNoLock();

            if (!TryGetLocalConstant(name, out value)) {
                return false;
            }

            if (_constants != null && _constants.Remove(name)) {
                return true;
            }

            Debug.Assert(_namespaceTracker != null);
            _constants[name] = RemovedConstant;
            return true;
        }

        // thread-safe:
        private object TrackerToModule(object value) {
            TypeGroup typeGroup = value as TypeGroup;
            if (typeGroup != null) {
                return value;
            }

            // TypeTracker retrieved from namespace tracker should behave like a RubyClass/RubyModule:
            TypeTracker typeTracker = value as TypeTracker;
            if (typeTracker != null) {
                return _context.GetModule(typeTracker.Type);
            }

            // NamespaceTracker retrieved from namespace tracker should behave like a RubyModule:
            NamespaceTracker namespaceTracker = value as NamespaceTracker;
            if (namespaceTracker != null) {
                return _context.GetModule(namespaceTracker);
            }

            return value;
        }

        // TODO: DLR interop
        public bool EnumerateConstants(Func<RubyModule, string, object, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();
            
            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _namespaceTracker == null);
                foreach (KeyValuePair<SymbolId, object> constant in _context.TopGlobalScope.Items) {
                    if (action(this, SymbolTable.IdToString(constant.Key), constant.Value)) return true;
                }

                return false;
            }

            foreach (KeyValuePair<string, object> constant in _constants) {
                if (action(this, constant.Key, constant.Value)) return true;
            }

            if (_namespaceTracker != null) {
                foreach (KeyValuePair<SymbolId, object> constant in _namespaceTracker.SymbolAttributes) {
                    if (action(this, SymbolTable.IdToString(constant.Key), constant.Value)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods (thread-safe)

        // not thread-safe:
        public void ForEachInstanceMethod(bool inherited, Func<RubyModule/*!*/, string/*!*/, RubyMemberInfo, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {

                // skip interfaces (methods declared on interfaces have already been looked for in the class);
                // if 'this' is an interface, we want to visit all interface methods:
                if (module.IsInterface && !this.IsInterface) return false;

                // notification that we entered the module (it could have no method):
                if (action(module, null, null)) return true;

                return module.EnumerateMethods(action);
            });
        }

        // thread-safe:
        public void AddMethodAlias(string/*!*/ newName, string/*!*/ oldName) {
            // MRI 1.8: if (newName == oldName) return;
            // MRI 1.9: no check

            RubyMemberInfo method;
            using (Context.ClassHierarchyLocker()) {
                // MRI: aliases a super-forwarder not the real method.
                method = ResolveMethodNoLock(oldName, VisibilityContext.AllVisible, MethodLookup.FallbackToObject | MethodLookup.ReturnForwarder).Info;
                if (method == null) {
                    throw RubyExceptions.CreateUndefinedMethodError(this, oldName);
                }

                // Alias preserves visibility and declaring module even though the alias is declared in a different module (e.g. subclass) =>
                // we can share method info (in fact, sharing is sound with Method#== semantics - it returns true on aliased methods).
                // 
                // CLR members: 
                // Detaches the member from its underlying type (by creating a copy).
                // Note: We need to copy overload group since otherwise it might mess up caching if the alias is defined in a sub-module and 
                // overloads of the same name that are not included in the overload group are inherited to this module.
                // EnumerateMethods also relies on overload groups only representing cached CLR members.
                if (!method.IsRubyMember) {
                    SetMethodNoEventNoLock(Context, newName, method.Copy(method.Flags, method.DeclaringModule));
                } else {
                    SetMethodNoEventNoLock(Context, newName, method);
                }
            }

            MethodAdded(newName);
        }

        // Module#define_method:
        public void SetDefinedMethodNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method, RubyMethodVisibility visibility) {
            // CLR members: Detaches the member from its underlying type (by creating a copy).
            // Note: Method#== returns false on defined methods and redefining the original method doesn't affect the new one:
            SetMethodNoEventNoLock(callerContext, name, method.Copy((RubyMemberFlags)visibility, this));
        }

        // Module#module_function/private/protected/public:
        public void SetVisibilityNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method, RubyMethodVisibility visibility) {
            Context.RequiresClassHierarchyLock();

            RubyMemberInfo existing;
            bool skipHidden = false;
            if (TryGetMethod(name, ref skipHidden, out existing)) {
                // CLR members: Detaches the member from its underlying type (by creating a copy).
                SetMethodNoEventNoLock(callerContext, name, method.Copy((RubyMemberFlags)visibility, this));
            } else {
                SetMethodNoEventNoLock(callerContext, name, new SuperForwarderInfo((RubyMemberFlags)visibility, method.DeclaringModule, name));
            }
        }

        // Module#module_function:
        public void SetModuleFunctionNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            // CLR members: Detaches the member from its underlying type (by creating a copy).
            // TODO: check for CLR instance members, it should be an error to call module_function on them:
            SingletonClass.SetMethodNoEventNoLock(callerContext, name, method.Copy(RubyMemberFlags.Public, SingletonClass));
        }

        // thread-safe:
        public void AddMethod(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Assert.NotNull(name, method);
            Mutate();
            SetMethodNoEvent(callerContext, name, method);
            MethodAdded(name);
        }

        // thread-safe:
        public void SetMethodNoEvent(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            using (Context.ClassHierarchyLocker()) {
                SetMethodNoEventNoLock(callerContext, name, method);
            }
        }

        public void SetMethodNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Mutate();
            SetMethodNoMutateNoEventNoLock(callerContext, name, method);
        }

        private void SetMethodNoMutateNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(name, method);

            if (callerContext != _context) {
                throw RubyExceptions.CreateTypeError(String.Format("Cannot define a method on a {0} `{1}' defined in a foreign runtime #{2}",
                    IsClass ? "class" : "module", _name, _context.RuntimeId));
            }

            PrepareMethodUpdate(name, method);

            InitializeMethodsNoLock();
            _methods[name] = method;
        }

        internal virtual void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method) {
            InitializeMethodsNoLock();

            // Prepare all classes where this module is included for a method update.
            // TODO (optimization): we might end up walking some classes multiple times, could we mark them somehow as visited?
            foreach (var dependency in _dependentClasses) {
                var cls = (RubyClass)dependency.Target;
                if (cls != null) {
                    cls.PrepareMethodUpdate(methodName, method, 0);
                }
            }
        }

        // Returns max level in which a group has been invalidated.
        internal int InvalidateGroupsInDependentClasses(string/*!*/ methodName, int maxLevel) {
            int result = -1;
            foreach (var cls in GetDependentClasses()) {
                result = Math.Max(result, cls.InvalidateGroupsInSubClasses(methodName, maxLevel));
            }

            return result;
        }

        internal bool TryGetDefinedMethod(string/*!*/ name, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();
            if (_methods == null) {
                method = null;
                return false;
            }
            return _methods.TryGetValue(name, out method);
        }

        internal bool TryGetDefinedMethod(string/*!*/ name, ref bool skipHidden, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();

            if (TryGetDefinedMethod(name, out method)) {
                if (method.IsHidden || skipHidden && !method.IsRemovable) {
                    skipHidden = true;
                    method = null;
                    return false;
                } else {
                    return true;
                }
            }
            return false;
        }

        internal IEnumerable<KeyValuePair<string/*!*/, RubyMemberInfo/*!*/>>/*!*/ GetMethods() {
            Context.RequiresClassHierarchyLock();
            return _methods;
        }
        
        /// <summary>
        /// Direct addition to the method table. Used only for core method table operations.
        /// Do not use unless absolutely sure there is no overriding method used in a dynamic site.
        /// </summary>
        internal void AddMethodNoCacheInvalidation(string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Context.RequiresClassHierarchyLock();
            Debug.Assert(_methods != null);
            _methods.Add(name, method);
        }

        /// <summary>
        /// Direct removal from the method table. Used only for core method table operations.
        /// </summary>
        internal bool RemoveMethodNoCacheInvalidation(string/*!*/ name) {
            Context.RequiresClassHierarchyLock();
            Debug.Assert(_methods != null);
            return _methods.Remove(name);
        }

        // thread-safe:
        public bool RemoveMethod(string/*!*/ name) {
            if (RemoveMethodNoEvent(name)) {
                MethodRemoved(name);
                return true;
            }
            return false;
        }

        // thread-safe:
        private bool RemoveMethodNoEvent(string/*!*/ name) {
            Mutate();
            using (Context.ClassHierarchyLocker()) {
                InitializeMethodsNoLock();

                RubyMemberInfo method;
                if (_methods.TryGetValue(name, out method)) {
                    if (method.IsHidden || method.IsUndefined) {
                        return false;
                    } else if (this == _context.ObjectClass && name == Symbols.Initialize) {
                        // We prohibit removing Object#initialize to simplify object construction logic (this is compatible with 1.9 behavior).
                        return false;
                    } else if (method.IsRemovable) {
                        // Method is used in a dynamic site or group => update version of all dependencies of this module.
                        if (method.InvalidateSitesOnOverride || method.InvalidateGroupsOnRemoval) {
                            Updated("RemoveMethod: " + name);
                        }
                        
                        if (method.InvalidateSitesOnOverride && name == Symbols.MethodMissing) {
                            method.DeclaringModule.MissingMethodsCachedInSites = null;
                        }

                        // Method hides CLR overloads => update method groups in all dependencies of this module.
                        // TODO (opt): Do not update the entire subtree, update subtrees of all invalidated groups.
                        // TODO (opt): We can calculate max-level but it requires maintanance whenever a method is overridden 
                        //             and whenever method group is lazily created (TryGetClrMethod).
                        if (method.InvalidateGroupsOnRemoval) {
                            InvalidateGroupsInDependentClasses(name, Int32.MaxValue);
                        }
                        
                        _methods.Remove(name);
                    } else {
                        SetMethodNoEventNoLock(Context, name, RubyMemberInfo.HiddenMethod);
                    }
                    return true;
                } else if (TryGetClrMember(name, false, out method)) {
                    Debug.Assert(!method.IsRemovable);
                    SetMethodNoEventNoLock(Context, name, RubyMemberInfo.HiddenMethod);
                    return true;
                } else {
                    return false;
                }
            }
        }

        // thread-safe:
        public void UndefineMethod(string/*!*/ name) {
            UndefineMethodNoEvent(name);
            MethodUndefined(name);
        }

        // thread-safe:
        public void UndefineMethodNoEvent(string/*!*/ name) {
            SetMethodNoEvent(Context, name, RubyMethodInfo.UndefinedMethod);
        }

        // thread-safe:
        public void HideMethod(string/*!*/ name) {
            SetMethodNoEvent(Context, name, RubyMethodInfo.HiddenMethod);
        }

        // thread-safe:
        public void DefineLibraryMethod(string/*!*/ name, int attributes, params Delegate[]/*!*/ overloads) {
            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.MemberFlagsMask);
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            RubyCompatibility compatibility = (RubyCompatibility)(attributes >> RubyMethodAttribute.CompatibilityEncodingShift);
            if (compatibility > Context.RubyOptions.Compatibility) {
                return;
            }
            SetLibraryMethod(name, new RubyLibraryMethodInfo(overloads, flags, this), skipEvent);
        }

        // thread-safe:
        public void DefineLibraryMethod(string/*!*/ name, int attributes, Delegate/*!*/ overload) {
            DefineLibraryMethod(name, attributes, new[] { overload });
        }

        // thread-safe:
        public void DefineLibraryMethod(string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2) {
            DefineLibraryMethod(name, attributes, new[] { overload1, overload2 });
        }

        // thread-safe:
        public void DefineLibraryMethod(string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2, Delegate/*!*/ overload3) {
            DefineLibraryMethod(name, attributes, new[] { overload1, overload2, overload3 });
        }

        // thread-safe:
        public void DefineLibraryMethod(string/*!*/ name, int attributes, Delegate/*!*/ overload1, Delegate/*!*/ overload2, Delegate/*!*/ overload3, Delegate/*!*/ overload4) {
            DefineLibraryMethod(name, attributes, new[] { overload1, overload2, overload3, overload4 });
        }

        // thread-safe:
        public void DefineRuleGenerator(string/*!*/ name, int attributes, RuleGenerator/*!*/ generator) {
            Assert.NotNull(generator);
            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.VisibilityMask);
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            SetLibraryMethod(name, new RubyCustomMethodInfo(generator, flags, this), skipEvent);
        }

        // thread-safe:
        public void SetLibraryMethod(string/*!*/ name, RubyMemberInfo/*!*/ method, bool noEvent) {
            // trigger event only for non-builtins:
            if (noEvent) {
                // TODO: hoist lock?
                using (Context.ClassHierarchyLocker()) {
                    SetMethodNoMutateNoEventNoLock(_context, name, method);
                }
            } else {
                AddMethod(_context, name, method);
            }
        }

        // thread-safe:
        public MethodResolutionResult ResolveMethodForSite(string/*!*/ name, VisibilityContext visibility) {
            using (Context.ClassHierarchyLocker()) {
                return ResolveMethodForSiteNoLock(name, visibility);
            }
        }

        // thread-safe:
        public MethodResolutionResult ResolveMethod(string/*!*/ name, VisibilityContext visibility) {
            using (Context.ClassHierarchyLocker()) {
                return ResolveMethodNoLock(name, visibility);
            }
        }

        public MethodResolutionResult ResolveMethodForSiteNoLock(string/*!*/ name, VisibilityContext visibility) {
            return ResolveMethodForSiteNoLock(name, visibility, MethodLookup.Default);
        }

        internal MethodResolutionResult ResolveMethodForSiteNoLock(string/*!*/ name, VisibilityContext visibility, MethodLookup options) {
            return ResolveMethodNoLock(name, visibility, options).InvalidateSitesOnOverride();
        }

        public MethodResolutionResult ResolveMethodNoLock(string/*!*/ name, VisibilityContext visibility) {
            return ResolveMethodNoLock(name, visibility, MethodLookup.Default);
        }

        public MethodResolutionResult ResolveMethodNoLock(string/*!*/ name, VisibilityContext visibility, MethodLookup options) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(name);

            InitializeMethodsNoLock();
            RubyMemberInfo info = null;
            RubyModule owner = null;
            bool skipHidden = false;
            bool foundCallerSelf = false;
            MethodResolutionResult result;

            if (ForEachAncestor((module) => {
                owner = module;
                foundCallerSelf |= module == visibility.Class;
                return module.TryGetMethod(name, ref skipHidden, (options & MethodLookup.Virtual) != 0, out info);
            })) {
                if (info == null || info.IsUndefined) {
                    result = MethodResolutionResult.NotFound;
                } else if (!IsMethodVisible(info, owner, visibility, foundCallerSelf)) {
                    result = new MethodResolutionResult(info, owner, false);
                } else if (info.IsSuperForwarder) {
                    if ((options & MethodLookup.ReturnForwarder) != 0) {
                        result = new MethodResolutionResult(info, owner, true);
                    } else {
                        // start again with owner's super ancestor and ignore visibility:
                        result = owner.ResolveSuperMethodNoLock(((SuperForwarderInfo)info).SuperName, owner);
                    }
                } else {
                    result = new MethodResolutionResult(info, owner, true);
                }
            } else {
                result = MethodResolutionResult.NotFound;
            }

            // Note: all classes include Object in ancestors, so we don't need to search it again:
            if (!result.Found && (options & MethodLookup.FallbackToObject) != 0 && !IsClass) {
                return _context.ObjectClass.ResolveMethodNoLock(name, visibility, options & ~MethodLookup.FallbackToObject);
            }

            return result;
        }

        private bool IsMethodVisible(RubyMemberInfo/*!*/ method, RubyModule/*!*/ owner, VisibilityContext visibility, bool foundCallerSelf) {
            // Visibility not constrained by a class:
            // - call with implicit self => all methods are visible.
            // - interop call => only public methods are visible.
            if (visibility.Class == null) {
                return visibility.IsVisible(method.Visibility);
            } 
            
            if (method.Visibility == RubyMethodVisibility.Protected) {
                // A protected method is visible if the caller's self immediate class is a descendant of the method owner.
                if (foundCallerSelf) {
                    return true;
                }
                // walk ancestors from caller's self class (visibilityContext)
                // until the method owner is found or this module is found (this module is a descendant of the owner):
                return visibility.Class.ForEachAncestor((module) => module == owner || module == this);
            } 

            return method.Visibility == RubyMethodVisibility.Public;
        }

        // skip one method in the method resolution order (MRO)
        public MethodResolutionResult ResolveSuperMethodNoLock(string/*!*/ name, RubyModule/*!*/ callerModule) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(name, callerModule);

            InitializeMethodsNoLock();

            RubyMemberInfo info = null;
            RubyModule owner = null;
            bool foundModule = false;
            bool skipHidden = false;

            // start searching for the method in the MRO parent of the declaringModule:
            if (ForEachAncestor((module) => {
                if (module == callerModule) {
                    foundModule = true;
                    return false;
                }

                owner = module;
                return foundModule && module.TryGetMethod(name, ref skipHidden, out info) && !info.IsSuperForwarder;
            }) && !info.IsUndefined) {
                return new MethodResolutionResult(info, owner, true);
            }

            return MethodResolutionResult.NotFound;
        }

        // thread-safe:
        public RubyMemberInfo GetMethod(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            using (Context.ClassHierarchyLocker()) {
                InitializeMethodsNoLock();

                RubyMemberInfo method;
                bool skipHidden = false;
                TryGetMethod(name, ref skipHidden, out method);
                return method;
            }
        }

        internal bool TryGetMethod(string/*!*/ name, ref bool skipHidden, out RubyMemberInfo method) {
            return TryGetMethod(name, ref skipHidden, false, out method);
        }

        internal bool TryGetMethod(string/*!*/ name, ref bool skipHidden, bool virtualLookup, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(name);
            Debug.Assert(_methods != null);

            // lookup Ruby method first:    
            if (TryGetDefinedMethod(name, ref skipHidden, out method)) {
                return true;
            }

            if (virtualLookup) {
                string mangled;
                if ((mangled = RubyUtils.TryMangleName(name)) != null && TryGetDefinedMethod(mangled, ref skipHidden, out method)
                    && method.IsRubyMember) {
                    return true;
                }

                // Special mappings:
                // Do not map to Kernel#hash/eql? to prevent recursion in case Object.GetHashCode/Equals is removed.
                if (this != Context.KernelModule) {
                    if (name == "GetHashCode" && TryGetDefinedMethod("hash", out method) && method.IsRubyMember) {
                        return true;
                    } else if (name == "Equals" && TryGetDefinedMethod("eql?", out method) && method.IsRubyMember) {
                        return true;
                    }
                }
            }

            return !skipHidden && TryGetClrMember(name, virtualLookup, out method);
        }

        private bool TryGetClrMember(string/*!*/ name, bool virtualLookup, out RubyMemberInfo method) {
            // Skip hidden CLR overloads.
            // Skip lookup on types that are not visible or interfaces.
            if (_typeTracker != null && !_typeTracker.Type.IsInterface) {
                // Note: Do not allow mangling for CLR virtual lookups - we want to match the overridden name exactly as is, 
                // so that it corresponds to the base method call the override stub performs.
                bool tryUnmangle = !virtualLookup && (_restrictions & ModuleRestrictions.NoNameMangling) == 0;

                if (TryGetClrMember(_typeTracker.Type, name, tryUnmangle, out method)) {
                    _methods.Add(name, method);
                    return true;
                }
            }

            method = null;
            return false;
        }

        protected virtual bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool tryUnmangle, out RubyMemberInfo method) {
            method = null;
            return false;
        }

        public bool EnumerateMethods(Func<RubyModule, string, RubyMemberInfo, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            InitializeMethodsNoLock();

            foreach (KeyValuePair<string, RubyMemberInfo> method in _methods) {
                // Exclude attached CLR members as they only represent cached CLR method calls and these methods are enumerated below.
                // Include undefined and CLR hidden members - the action uses them to hide the names.
                if (method.Value.IsRubyMember) {
                    if (action(this, method.Key, method.Value)) {
                        return true;
                    }
                }
            }

            // CLR members (do not include interface members - they are not callable methods, just metadata):
            if (_typeTracker != null && !_typeTracker.Type.IsInterface) {
                foreach (string name in EnumerateClrMembers(_typeTracker.Type)) {
                    if (action(this, name, RubyMemberInfo.InteropMember)) {
                        return true;
                    }
                }
            }

            return false;
        }

        protected virtual IEnumerable<string/*!*/>/*!*/ EnumerateClrMembers(Type/*!*/ type) {
            return ArrayUtils.EmptyStrings;
        }

        /// <summary>
        /// inherited == false, attributes & attr == Instance:
        ///   - get methods in the "self" module
        ///   - also include methods on singleton ancestor classes until a non-singleton class is reached
        /// inherited == false, attributes & attr == Singleton:
        ///   - get methods only in the "self" module if it's a singleton class
        ///   - do not visit mixins nor super classes 
        /// inherited == true, attributes & attr == Singleton:
        ///   - walk all ancestors until a non-singleton class is reached (do not include non-singleton's methods)
        /// inherited == true, attributes & attr == None:
        ///   - walk all ancestors until an Object is reached
        /// 
        /// Methods are filtered by visibility specified in attributes (mutliple visibilities could be specified).
        /// A name undefined in a module is not visible in that module and its ancestors.
        /// Method names are not duplicated in the result.
        /// </summary>
        /// <remarks>
        /// Not thread safe.
        /// </remarks>
        public void ForEachMember(bool inherited, RubyMethodAttributes attributes, IEnumerable<string> foreignMembers, 
            Action<string/*!*/, RubyModule/*!*/, RubyMemberInfo/*!*/>/*!*/ action) {

            Context.RequiresClassHierarchyLock();

            var visited = new Dictionary<string, RubyMemberInfo>();

            // We can look for instance methods, singleton methods or all methods.
            // The difference is when we stop searching.
            bool instanceMethods = (attributes & RubyMethodAttributes.Instance) != 0;
            bool singletonMethods = (attributes & RubyMethodAttributes.Singleton) != 0;

            // TODO: if we allow creating singletons for foreign objects we need to change this:
            if (foreignMembers != null) {
                foreach (var name in foreignMembers) {
                    action(name, this, RubyMethodInfo.InteropMember);
                    visited.Add(name, RubyMethodInfo.InteropMember);
                }
            }

            bool stop = false;
            ForEachInstanceMethod(true, delegate(RubyModule/*!*/ module, string name, RubyMemberInfo member) {

                if (member == null) {
                    // notification received before any method of the module

                    if (stop) {
                        return true;
                    }

                    if (instanceMethods) {
                        stop = !inherited && (!IsClass || module.IsClass && !module.IsSingletonClass);
                    } else if (singletonMethods) {
                        if (!inherited && module != this || module.IsClass && !module.IsSingletonClass) {
                            return true;
                        }
                    } else {
                        stop = !inherited;
                    }

                } else if (!visited.ContainsKey(name)) {
                    // yield the member only if it has the right visibility:
                    if (!member.IsUndefined && !member.IsHidden && (((RubyMethodAttributes)member.Visibility & attributes) != 0)) {
                        action(name, module, member);
                    }

                    // visit the member even if it doesn't have the right visibility so that any overridden member with the right visibility
                    // won't later be visited:
                    visited.Add(name, member);
                }

                return false;
            });
        }

        public void ForEachMember(bool inherited, RubyMethodAttributes attributes, Action<string/*!*/, RubyModule/*!*/, RubyMemberInfo/*!*/>/*!*/ action) {
            ForEachMember(inherited, attributes, null, action);
        }

        #endregion

        #region Class variables (TODO: thread-safety)

        public void ForEachClassVariable(bool inherited, Func<RubyModule, string, object, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {
                // notification that we entered the module (it could have no class variable):
                if (action(module, null, Missing.Value)) return true;

                return module.EnumerateClassVariables(action);
            });
        }

        public void SetClassVariable(string/*!*/ name, object value) {
            InitializeClassVariableTable();

            Mutate();
            _classVariables[name] = value;
        }

        public bool TryGetClassVariable(string/*!*/ name, out object value) {
            value = null;
            return _classVariables != null && _classVariables.TryGetValue(name, out value);
        }

        public bool RemoveClassVariable(string/*!*/ name) {
            return _classVariables != null && _classVariables.Remove(name);
        }

        public RubyModule TryResolveClassVariable(string/*!*/ name, out object value) {
            Assert.NotNull(name);

            RubyModule result = null;
            object constValue = null;

            using (Context.ClassHierarchyLocker()) {
                if (ForEachAncestor(delegate(RubyModule/*!*/ module) {
                    if (module._classVariables != null && module._classVariables.TryGetValue(name, out constValue)) {
                        result = module;
                        return true;
                    }

                    return false;
                })) {
                    value = constValue;
                    return result;
                }
            }

            value = null;
            return null;
        }

        private bool EnumerateClassVariables(Func<RubyModule, string, object, bool>/*!*/ action) {
            if (_classVariables != null) {
                foreach (KeyValuePair<string, object> variable in _classVariables) {
                    if (action(this, variable.Key, variable.Value)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Mixins (thread-safe)

        // thread-safe:
        public bool HasAncestor(RubyModule/*!*/ module) {
            using (Context.ClassHierarchyLocker()) {
                return HasAncestorNoLock(module);
            }
        }

        public bool HasAncestorNoLock(RubyModule/*!*/ module) {
            Context.RequiresClassHierarchyLock();
            return ForEachAncestor(true, (m) => m == module);
        }

        // thread-safe:
        public RubyModule[]/*!*/ GetMixins() {
            using (Context.ClassHierarchyLocker()) {
                return ArrayUtils.Copy(_mixins);
            }
        }

        // thread-safe:
        public void IncludeModules(params RubyModule[]/*!*/ modules) {
            using (Context.ClassHierarchyLocker()) {
                IncludeModulesNoLock(modules);
            }
        }
        
        internal void IncludeModulesNoLock(RubyModule[]/*!*/ modules) {
            Context.RequiresClassHierarchyLock();
            Mutate();

            RubyUtils.RequireMixins(this, modules);

            RubyModule[] expanded = ExpandMixinsNoLock(GetSuperClass(), _mixins, modules);

            foreach (RubyModule module in expanded) {
                if (module.IsInterface) {
                    // Can't include generic interfaces
                    if (module.TypeTracker.Type.ContainsGenericParameters) {
                        throw RubyExceptions.CreateTypeError(String.Format(
                            "{0}: cannot extend with open generic instantiation {1}. Only closed instantiations are supported.",
                            Name, module.Name
                            ));
                    }

                    if (!CanIncludeClrInterface) {
                        bool alreadyIncluded = false;
                        foreach (RubyModule includedModule in _mixins) {
                            if (includedModule == module) {
                                alreadyIncluded = true;
                                break;
                            }
                        }
                        if (!alreadyIncluded) {
                            throw new InvalidOperationException(String.Format(
                                "Interface {0} cannot be included in class {1} because its underlying type has already been created.",
                                module.Name, Name
                                ));
                        }
                    }
                }
            }

            MixinsUpdated(_mixins, _mixins = expanded);
        }

        internal void InitializeNewMixin(RubyModule/*!*/ mixin) {
            if (_methodsState != MemberTableState.Uninitialized) {
                mixin.InitializeMethodTableNoLock();
            }

            if (_constantsState != MemberTableState.Uninitialized) {
                mixin.InitializeConstantTableNoLock();
            }
        }

        internal virtual void MixinsUpdated(RubyModule/*!*/[]/*!*/ oldMixins, RubyModule/*!*/[]/*!*/ newMixins) {
            // nop
        }

        // Requires hierarchy lock
        internal static RubyModule[]/*!*/ ExpandMixinsNoLock(RubyClass superClass, RubyModule/*!*/[]/*!*/ modules) {
            return ExpandMixinsNoLock(superClass, EmptyArray, modules);
        }

        // Requires hierarchy lock
        private static RubyModule[]/*!*/ ExpandMixinsNoLock(RubyClass superClass, RubyModule/*!*/[]/*!*/ existing, IList<RubyModule/*!*/>/*!*/ added) {
            Assert.NotNull(existing);
            Assert.NotNull(added);
            
            List<RubyModule> expanded = new List<RubyModule>(existing);
            ExpandMixinsNoLock(superClass, expanded, 0, added, true);
            return expanded.ToArray();
        }

        // Requires hierarchy lock
        private static int ExpandMixinsNoLock(RubyClass superClass, List<RubyModule/*!*/>/*!*/ existing, int index, IList<RubyModule/*!*/>/*!*/ added, 
            bool recursive) {

            foreach (RubyModule module in added) {
                Assert.NotNull(module);

                int newIndex = existing.IndexOf(module);
                if (newIndex >= 0) {
                    // Module is already present in _mixins
                    // Update the insertion point so that we retain ordering of dependencies
                    // If we're still in the initial level of recursion, repeat for module's mixins
                    index = newIndex + 1;
                    if (recursive) {
                        index = ExpandMixinsNoLock(superClass, existing, index, module._mixins, false);
                    }
                } else {
                    // Module is not yet present in _mixins
                    // Recursively insert module dependencies at the insertion point, then insert module itself
                    newIndex = ExpandMixinsNoLock(superClass, existing, index, module._mixins, false);
                    
                    // insert module only if it is not an ancestor of the superclass:
                    if (superClass == null || !superClass.HasAncestorNoLock(module)) {
                        existing.Insert(index, module);
                        index = newIndex + 1;
                    } else {
                        index = newIndex;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Returns true if given module is an ancestor of the superclass of this class (provided that this is a class).
        /// </summary>
        internal virtual bool IsSuperClassAncestor(RubyModule/*!*/ module) {
            return false;
        }

        protected virtual bool CanIncludeClrInterface {
            get { return true; }
        }

        internal List<Type> GetClrInterfaces() {
            List<Type> interfaces = new List<Type>();
            using (Context.ClassHierarchyLocker()) {
                foreach (RubyModule m in _mixins) {
                    if (m.IsInterface && !interfaces.Contains(m.TypeTracker.Type)) {
                        interfaces.Add(m.TypeTracker.Type);
                    }
                }
            }
            return interfaces;
        }

        private void IncludeTraitNoLock(ref Action<RubyModule> initializer, MemberTableState tableState, Action<RubyModule>/*!*/ trait) {
            Assert.NotNull(trait);

            if (tableState == MemberTableState.Uninitialized) {
                if (initializer != null) {
                    initializer += trait;
                } else {
                    initializer = trait;
                }
            } else {
                // TODO: postpone? hold lock?
                using (Context.ClassHierarchyUnlocker()) {
                    trait(this);
                }
            }
        }

        internal void IncludeLibraryModule(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, Action<RubyModule> constantsInitializer, 
            RubyModule/*!*/[]/*!*/ mixins, bool builtin) {

            // Do not allow non-builtin library inclusions to a frozen module.
            // We need to ensure that initializers are not modified once a module is frozen.
            if (!builtin) {
                Mutate();
            }

            using (Context.ClassHierarchyLocker()) {
                if (instanceTrait != null) {
                    IncludeTraitNoLock(ref _methodsInitializer, _methodsState, instanceTrait);
                }

                if (constantsInitializer != null) {
                    IncludeTraitNoLock(ref _constantsInitializer, _constantsState, instanceTrait);
                }

                if (classTrait != null) {
                    SingletonClass.IncludeTraitNoLock(ref SingletonClass._methodsInitializer, SingletonClass._methodsState, classTrait);
                }
                    
                // updates the module version:
                IncludeModulesNoLock(mixins);
            }
        }

        #endregion

        #region Names

        public string GetName(RubyContext/*!*/ context) {
            return context == _context ? _name : _name + "@" + _context.RuntimeId;
        }

        public MutableString/*!*/ GetDisplayName(RubyContext/*!*/ context, bool showEmptyName) {
            if (IsSingletonClass) {
                RubyClass c = (RubyClass)this;
                object singletonOf;
                MutableString result = MutableString.CreateMutable();

                int nestings = 0;
                while (true) {
                    nestings++;
                    result.Append("#<Class:");

                    singletonOf = c.SingletonClassOf;

                    RubyModule module = singletonOf as RubyModule;

                    if (module == null) {
                        nestings++;
                        result.Append("#<");
                        result.Append(c.SuperClass.GetName(context));
                        result.Append(':');
                        RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(_context, singletonOf));
                        break;
                    }

                    if (!module.IsSingletonClass) {
                        result.Append(module.GetName(context));
                        break;
                    }

                    c = (RubyClass)module;
                }
                return result.Append('>', nestings);
            } else if (_name == null) {
                if (showEmptyName) {
                    return MutableString.FrozenEmpty;
                } else {
                    MutableString result = MutableString.CreateMutable();
                    result.Append("#<");
                    result.Append(_context.GetClassOf(this).GetName(context));
                    result.Append(':');
                    RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(_context, this));
                    result.Append('>');
                    return result;
                }
            } else {
                return MutableString.CreateMutable(GetName(context));
            }
        }

        #endregion      
    }
}
