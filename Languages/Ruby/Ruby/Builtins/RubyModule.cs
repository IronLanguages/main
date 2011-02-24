/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
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
using Microsoft.Scripting.Runtime;

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
        /// Module doesn't allow its methods to be called by mangled (FooBar -> foo_bar) or mapped ([] -> get_Item) names.
        /// Used for built-ins.
        /// </summary>
        NoNameMapping = 2,

        /// <summary>
        /// Module is not published in the runtime global scope.
        /// </summary>
        NotPublished = 4,

        /// <summary>
        /// Module doesn't expose the underlying CLR type. The behavior is the same as if it was written in Ruby.
        /// (clr_new and clr_ctor won't work on such class/module, CLR methods won't be visible, etc.).
        /// </summary>
        NoUnderlyingType = 8,

        /// <summary>
        /// By default a non-builtin library load fails if it defines a class whose name conflicts with an existing constant name.
        /// If this restriction is applied to a class it can reopen an existing Ruby class of the same name but it can't specify an underlying type.
        /// This is required so that the library load doesn't depend on whether or not any instances of the existing Ruby class already exist. 
        /// </summary>
        AllowReopening = 16 | NoUnderlyingType,

        /// <summary>
        /// Default restrictions for built-in modules.
        /// </summary>
        Builtin = NoOverrides | NoNameMapping | NotPublished,

        All = Builtin
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
    [DebuggerTypeProxy(typeof(RubyModule.DebugView))]
    [ReflectionCached]
    public partial class RubyModule : IDuplicable, IRubyObject {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly RubyModule[]/*!*/ EmptyArray = new RubyModule[0];

        // interlocked
        internal static int _globalMethodVersion = 0;
        internal static int _globalModuleId = 0;

        private enum State {
            Uninitialized,
            Initializing,
            Initialized
        }

        private readonly RubyContext/*!*/ _context;
        
        #region CLR Types and Namespaces

        // the namespace this module represents or null:
        private readonly NamespaceTracker _namespaceTracker;

        // The type this module represents or null. 
        // TODO: unify with _underlyingSystemType on classes
        private readonly TypeTracker _typeTracker;

        public TypeTracker TypeTracker {
            get { return _typeTracker; }
        }

        public NamespaceTracker NamespaceTracker {
            get { return _namespaceTracker; }
        }

        public bool IsInterface {
            get { return _typeTracker != null && _typeTracker.Type.IsInterface; }
        }

        public bool IsClrModule {
            get { return _typeTracker != null && IsModuleType(_typeTracker.Type); }
        }

        public virtual Type/*!*/ GetUnderlyingSystemType() {
            if (IsClrModule) {
                return _typeTracker.Type;
            } else {
                throw new InvalidOperationException();
            }
        }
        
        #endregion

        private readonly ModuleRestrictions _restrictions;
        private readonly WeakReference/*!*/ _weakSelf;
        
        // name of the module or null for anonymous modules:
        private string _name;

        // Lazy interlocked init'd.
        private RubyInstanceData _instanceData;
        
        #region Immediate/Singleton/Super Class

        // Lazy interlocked init'd.
        // Classes
        //   Null immediately after construction, initialized by RubyContext.CreateClass factory to a singleton class.
        //   This lazy initialization is needed to allow circular references among Kernel, Object, Module and Class.
        //   We create the singleton class eagerly in the factory since classes' singleton classes form a hierarchy parallel 
        //   to the main inheritance hierarachy of classes. If we didn't we would need to update super-references of all singleton subclasses 
        //   that were created after a singleton class is lazily created.
        // Modules
        //   Initialized to the class of the module and may later be changed to a singleton class (only if the singleton is needed).
        //   We don't create the singleton class eagerly to optimize rule generation. If we did, each (meta)module instance would receive its own immediate class
        //   different from other instances of the (meta)module. And thus the instances would use different method table versions eventhough they are the same.
        // Singleton Classes
        //   Self reference for dummy singletons (tha last singletons in the singleton chain).
        private RubyClass _immediateClass;

        /// <summary>
        /// A dummy singleton class is an immutable class that has no members and is used to terminate singleton class chain.
        /// </summary>
        /// <remarks>
        /// A method invoked on the last class in the singleton class chain before the dummy singleton is searched in the inheritance hierarchy of the dummy singleton:
        /// [dummy singleton, singleton(Class), singleton(Module), singleton(Object), Class, Module, Object].
        /// The ImmediateClass reference cannot be null (that would cause null-ref exception in rules), so we need to set it to the dummy.
        /// </remarks>
        public bool IsDummySingletonClass {
            get { return _immediateClass == this; }
        }

        public virtual bool IsSingletonClass {
            get { return false; }
        }

        public virtual bool IsClass {
            get { return false; }
        }

        public bool IsObjectClass {
            get { return ReferenceEquals(this, Context.ObjectClass); }
        }

        public bool IsBasicObjectClass {
            get { return ReferenceEquals(this, Context.BasicObjectClass); }
        }

        public bool IsComClass {
            get { return ReferenceEquals(this, Context.ComObjectClass); }
        }

        internal virtual RubyClass GetSuperClass() {
            return null;
        }

        // thread safe:
        internal void InitializeImmediateClass(RubyClass/*!*/ cls) {
            Debug.Assert(_immediateClass == null);
            _immediateClass = cls;
        }

        // thread safe:
        internal void InitializeImmediateClass(RubyClass/*!*/ singletonSuperClass, Action<RubyModule> trait) {
            Assert.NotNull(singletonSuperClass);

            RubyClass immediate;
            if (IsClass) {
                // class: eager singleton class construction:
                immediate = CreateSingletonClass(singletonSuperClass, trait);
                immediate.InitializeImmediateClass(_context.ClassClass.GetDummySingletonClass());
            } else if (trait != null) {
                // module: eager singleton class construction:
                immediate = CreateSingletonClass(singletonSuperClass, trait);
                immediate.InitializeImmediateClass(singletonSuperClass.GetDummySingletonClass());
            } else {
                // module: lazy singleton class construction:
                immediate = singletonSuperClass;
            }

            InitializeImmediateClass(immediate);
        }

        #endregion

        #region Mutable state guarded by ClassHierarchyLock

        [Emitted]
        public readonly VersionHandle Version;
        public readonly int Id;

        // List of dependent classes - subclasses of this class and classes to which this module is included to (forms a DAG).
        private WeakList<RubyClass> _dependentClasses;

#if DEBUG
        private int _referringMethodRulesSinceLastUpdate;
        
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
                return name + " #" + Id;
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
        private Dictionary<string, ConstantStorage> _constants;
        
        // method table:
        private MemberTableState _methodsState = MemberTableState.Uninitialized;
        private Dictionary<string, RubyMemberInfo> _methods;
        private Action<RubyModule> _methodsInitializer;

        // class variable table:
        private Dictionary<string, object> _classVariables;

        //
        // The entire list of modules included in this one. Newly-added mixins are at the front of the array.
        // When adding a module that itself contains other modules, Ruby tries to maintain the ordering of the
        // contained modules so that method resolution is reasonably consistent.
        //
        // MRO walk: this, _mixins[0], _mixins[1], ..., _mixins[n-1], super, ...
        private RubyModule[]/*!*/ _mixins;

        // A list of extension methods included into this type or null if none were included.
        // { method-name -> methods }
        internal Dictionary<string, List<ExtensionMethodInfo>> _extensionMethods;
        
        #endregion

        #region Dynamic Sites

        // RubyModule, symbol -> object
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
            get { return _context; }
        }

        internal virtual RubyGlobalScope GlobalScope {
            get { return null; }
        }

        internal WeakReference/*!*/ WeakSelf {
            get { return _weakSelf; }
        }

        internal Dictionary<string, List<ExtensionMethodInfo>> ExtensionMethods {
            get { return _extensionMethods; }
        }

        // default allocator:
        public RubyModule(RubyClass/*!*/ metaModuleClass) 
            : this(metaModuleClass, null) {
        }

        // creates an empty (meta)module:
        protected RubyModule(RubyClass/*!*/ metaModuleClass, string name)
            : this(metaModuleClass.Context, name, null, null, null, null, null, ModuleRestrictions.None) {
            
            // metaModuleClass represents a subclass of Module or its duplicate (Kernel#dup)
            InitializeImmediateClass(metaModuleClass, null);
        }

        internal RubyModule(RubyContext/*!*/ context, string name, Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer,
            RubyModule/*!*/[] expandedMixins, NamespaceTracker namespaceTracker, TypeTracker typeTracker, ModuleRestrictions restrictions) {

            Assert.NotNull(context);
            Debug.Assert(namespaceTracker == null || typeTracker == null || typeTracker.Type == typeof(object));
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
            _weakSelf = new WeakReference(this);

            Version = new VersionHandle(Interlocked.Increment(ref _globalMethodVersion));
            Version.SetName(name);
            Id = Interlocked.Increment(ref _globalModuleId);
        }

        #region Initialization (thread-safe)

        internal bool ConstantInitializationNeeded {
            get { return _constantsState == MemberTableState.Uninitialized; }
        }

        private void InitializeConstantTableNoLock() {
            if (!ConstantInitializationNeeded) return;

            _constants = new Dictionary<string, ConstantStorage>();
            _constantsState = MemberTableState.Initializing;

            try {
                if (_constantsInitializer != EmptyInitializer) {
                    if (_constantsInitializer != null) {
                        Utils.Log(_name ?? "<anonymous>", "CT_INIT");
                        // TODO: use lock-free operations in initializers
                        _constantsInitializer(this);
                    } else if (_typeTracker != null && !_typeTracker.Type.IsInterface) {
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

                var name = (type.IsGenericType) ? ReflectionUtils.GetNormalizedTypeName(type) : type.Name;
                int index = names.IndexOf(name);
                if (index != -1) {
                    trackers[index] = TypeGroup.UpdateTypeEntity(trackers[index], tracker);
                    names[index] = name;
                } else {
                    trackers.Add(tracker);
                    names.Add(name);
                }
            }

            for (int i = 0; i < trackers.Count; i++) {
                var tracker = trackers[i];
                ConstantStorage storage;
                if (tracker is TypeGroup) {
                    storage = new ConstantStorage(tracker, new WeakReference(tracker));
                } else {
                    var module = Context.GetModule(tracker.Type);
                    storage = new ConstantStorage(module, module.WeakSelf);
                }
                _constants[names[i]] = storage;
            }
        }

        internal void InitializeMembersFrom(RubyModule/*!*/ module) {
            Context.RequiresClassHierarchyLock();
            Mutate();

            Assert.NotNull(module);

            if (module._namespaceTracker != null && _constants == null) {
                // initialize the module so that we can copy all constants from it:
                module.InitializeConstantsNoLock();

                // initialize all ancestors of self:
                InitializeConstantsNoLock();
            } else {
                _constantsInitializer = Utils.CloneInvocationChain(module._constantsInitializer);
                _constantsState = module._constantsState;
            }

            _constants = (module._constants != null) ? new Dictionary<string, ConstantStorage>(module._constants) : null;

            // copy namespace members:
            if (module._namespaceTracker != null) {
                Debug.Assert(_constants != null);
                foreach (KeyValuePair<string, object> constant in module._namespaceTracker) {
                    _constants.Add(constant.Key, new ConstantStorage(constant.Value));
                }
            }

            _methodsInitializer = Utils.CloneInvocationChain(module._methodsInitializer);
            _methodsState = module._methodsState;
            if (module._methods != null) {
                _methods = new Dictionary<string, RubyMemberInfo>(module._methods.Count);
                foreach (var method in module._methods) {
                    _methods[method.Key] = method.Value.Copy(method.Value.Flags, this);
                }
            } else {
                _methods = null;
            }

            _classVariables = (module._classVariables != null) ? new Dictionary<string, object>(module._classVariables) : null;
            _mixins = ArrayUtils.Copy(module._mixins);

            // dependentModules - skip
            // tracker - skip, .NET members not copied
            
            // TODO:
            // - handle overloads cached in groups
            // - version updates
            MethodsUpdated("InitializeFrom");
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
            
            // capture the current immediate class (it can change any time if it not a singleton class)
            RubyClass immediate = _immediateClass;

            RubyModule result = new RubyModule(immediate.IsSingletonClass ? immediate.SuperClass : immediate, null);

            // singleton members are copied here, not in InitializeCopy:
            if (copySingletonMembers && immediate.IsSingletonClass) {
                var singletonClass = result.GetOrCreateSingletonClass();
                using (Context.ClassHierarchyLocker()) {
                    singletonClass.InitializeMembersFrom(immediate);
                }
            }
            
            // copy instance variables:
            _context.CopyInstanceData(this, result, false);
            return result;
        }

        #endregion

        #region Versioning (thread-safe)

        // A version of a frozen module can still change if its super-classes/mixins change.
        private void Mutate() {
            Debug.Assert(!IsDummySingletonClass);
            if (IsFrozen) {
                throw RubyExceptions.CreateRuntimeError(String.Format("can't modify frozen {0}", IsClass ? "class" : "module"));
            }
        }

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

        internal WeakList<RubyClass/*!*/>/*!*/ DependentClasses {
            get {
                Context.RequiresClassHierarchyLock();
                if (_dependentClasses == null) {
                    _dependentClasses = new WeakList<RubyClass>();
                }
                return _dependentClasses;
            }
        }

        internal void AddDependentClass(RubyClass/*!*/ dependentClass) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(dependentClass);

            foreach (var cls in DependentClasses) {
                if (ReferenceEquals(dependentClass, cls)) {
                    return;
                }
            }

            DependentClasses.Add(dependentClass.WeakSelf);
        }

        private void IncrementMethodVersion() {
            if (IsClass) {
                Version.Method = Interlocked.Increment(ref _globalMethodVersion);
            }
        }

        internal void MethodsUpdated(string/*!*/ reason) {
            Context.RequiresClassHierarchyLock();

            int affectedModules = 0;
            int affectedRules = 0;
            Func<RubyModule, bool> visitor = (module) => {
                module.IncrementMethodVersion();
#if DEBUG
                affectedModules++;
                affectedRules += module._referringMethodRulesSinceLastUpdate;
                module._referringMethodRulesSinceLastUpdate = 0;
#endif
                // TODO (opt?): stop updating if a class that defines a method of the same name is reached.
                return false;
            };

            visitor(this);
            ForEachRecursivelyDependentClass(visitor);

            Utils.Log(String.Format("{0,-50} {1,-30} affected={2,-5} rules={3,-5}", Name, reason, affectedModules, affectedRules), "UPDATED");
        }

        /// <summary>
        /// Calls given action on all modules that are directly or indirectly nested into this module.
        /// </summary>
        private bool ForEachRecursivelyDependentClass(Func<RubyModule, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            if (_dependentClasses != null) {
                foreach (var cls in _dependentClasses) {
                    if (action(cls)) {
                        return true;
                    }

                    if (cls.ForEachRecursivelyDependentClass(action)) {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region IRubyObject Members

        // thread-safe:
        public RubyClass ImmediateClass {
            get {
                return _immediateClass;
            }
            set {
                throw new InvalidOperationException("Cannot change the immediate class of a module");
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
            get { return _instanceData != null && _instanceData.IsFrozen; }
        }

        // thread-safe:
        public void Freeze() {
            GetInstanceData().Freeze();
        }

        // thread-safe:
        public bool IsTainted {
            get { return GetInstanceData().IsTainted; }
            set { GetInstanceData().IsTainted = value; }
        }

        // thread-safe:
        public bool IsUntrusted {
            get { return GetInstanceData().IsUntrusted; }
            set { GetInstanceData().IsUntrusted = value; }
        }

        int IRubyObject.BaseGetHashCode() {
            return base.GetHashCode();
        }

        bool IRubyObject.BaseEquals(object other) {
            return base.Equals(other);
        }

        string/*!*/ IRubyObject.BaseToString() {
            return base.ToString();
        }

        public override string/*!*/ ToString() {
            return _name ?? "<anonymous>";
        }

        #endregion

        #region Factories (thread-safe)

        // Ruby constructor:
        public static object CreateAnonymousModule(RubyScope/*!*/ scope, BlockParam body, RubyClass/*!*/ self) {
            RubyModule newModule = new RubyModule(self, null);
            return (body != null) ? RubyUtils.EvaluateInModule(newModule, body, null, newModule) : newModule;
        }

        // thread safe:
        public RubyClass/*!*/ GetOrCreateSingletonClass() {
            if (IsDummySingletonClass) {
                throw new InvalidOperationException("Dummy singleton class has no singleton class");
            }

            RubyClass immediate = _immediateClass;
            RubyClass singletonSuper;
            RubyClass singletonImmediate;

            if (!immediate.IsSingletonClass) {
                // finish module singleton initialization:
                Debug.Assert(!IsClass);
                singletonSuper = immediate;
                singletonImmediate = immediate.GetDummySingletonClass();
            } else if (immediate.IsDummySingletonClass) {
                // expanding singleton chain:
                singletonSuper = immediate.SuperClass;
                singletonImmediate = immediate;
            } else {
                return immediate;
            }

            var singleton = CreateSingletonClass(singletonSuper, null);
            singleton.InitializeImmediateClass(singletonImmediate);
            Interlocked.CompareExchange(ref _immediateClass, singleton, immediate);

            Debug.Assert(_immediateClass.IsSingletonClass && !_immediateClass.IsDummySingletonClass);
            return _immediateClass;
        }

        /// <summary>
        /// Create a new singleton class for this module. 
        /// Doesn't attach this module to it yet, the caller needs to do so.
        /// </summary>
        /// <remarks>Thread safe.</remarks>
        internal RubyClass/*!*/ CreateSingletonClass(RubyClass/*!*/ superClass, Action<RubyModule> trait) {
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
                     return autoloadScope.Context.Loader.LoadFile(autoloadScope.Scope, null, _path, LoadFlags.Require);
                 }
            }
        }

        public string/*!*/ MakeNestedModuleName(string nestedModuleSimpleName) {
            return (IsObjectClass || nestedModuleSimpleName == null) ?
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

        internal void Publish(string/*!*/ name) {
            RubyOps.ScopeSetMember(_context.TopGlobalScope, name, this);
        }

        private void SetConstantNoLock(string/*!*/ name, object value) {
            Mutate();
            SetConstantNoMutateNoLock(name, value);
        }

        internal void SetConstantNoMutateNoLock(string/*!*/ name, object value) {
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();
            _context.ConstantAccessVersion++;
            _constants[name] = new ConstantStorage(value);
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
                ConstantStorage existing;
                var result = TryLookupConstantNoLock(false, false, null, name, out existing);
                SetConstantNoLock(name, value);
                return result == ConstantLookupResult.Found;
            }
        }
        
        // thread-safe:
        public void SetAutoloadedConstant(string/*!*/ name, MutableString/*!*/ path) {
            ConstantStorage dummy;
            if (!TryGetConstant(null, name, out dummy)) {
                SetConstant(name, new AutoloadedConstant(MutableString.Create(path).Freeze()));
            }
        }

        // thread-safe:
        public MutableString GetAutoloadedConstantPath(string/*!*/ name) {
            using (Context.ClassHierarchyLocker()) {
                ConstantStorage storage;
                AutoloadedConstant autoloaded;
                return (TryGetConstantNoAutoloadCheck(name, out storage)
                    && (autoloaded = storage.Value as AutoloadedConstant) != null
                    && !autoloaded.Loaded) ?
                    autoloaded.Path : null;
            }
        }

        internal bool TryResolveConstant(RubyContext/*!*/ callerContext, RubyGlobalScope autoloadScope, string/*!*/ name, out ConstantStorage value) {
            return callerContext != Context ?
                TryResolveConstant(autoloadScope, name, out value) :
                TryResolveConstantNoLock(autoloadScope, name, out value);
        }

        public bool TryGetConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            ConstantStorage storage;
            var result = TryGetConstant(autoloadScope, name, out storage);
            value = storage.Value;
            return result;
        }

        /// <summary>
        /// Get constant defined in this module.
        /// </summary>
        internal bool TryGetConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out ConstantStorage value) {
            using (Context.ClassHierarchyLocker()) {
                return TryGetConstantNoLock(autoloadScope, name, out value);
            }
        }

        /// <summary>
        /// Get constant defined in this module.
        /// </summary>
        internal bool TryGetConstantNoLock(RubyGlobalScope autoloadScope, string/*!*/ name, out ConstantStorage value) {
            Context.RequiresClassHierarchyLock();
            return TryLookupConstantNoLock(false, false, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        /// <summary>
        /// Get constant defined in this module or any of its ancestors. 
        /// Autoloads if autoloadScope is not null.
        /// </summary>
        /// <remarks>
        /// Thread safe.
        /// </remarks>
        internal bool TryResolveConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out ConstantStorage value) {
            using (Context.ClassHierarchyLocker()) {
                return TryResolveConstantNoLock(autoloadScope, name, out value);
            }
        }        

        /// <summary>
        /// Get constant defined in this module or any of its ancestors.
        /// </summary>
        internal bool TryResolveConstantNoLock(RubyGlobalScope autoloadScope, string/*!*/ name, out ConstantStorage value) {
            Context.RequiresClassHierarchyLock();
            return TryLookupConstantNoLock(true, true, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        private enum ConstantLookupResult {
            NotFound = 0,
            Found = 1,
            FoundAutoload = 2,
        }

        private ConstantLookupResult TryLookupConstantNoLock(bool included, bool inherited, RubyGlobalScope autoloadScope,
            string/*!*/ name, out ConstantStorage value) {

            Context.RequiresClassHierarchyLock();
            Debug.Assert(included || !inherited);

            value = default(ConstantStorage);
            while (true) {
                ConstantStorage result;

                RubyModule owner = included ? 
                    TryResolveConstantNoAutoloadCheck(inherited, name, out result) :
                    (TryGetConstantNoAutoloadCheck(name, out result) ? this : null);

                if (owner == null) {
                    return ConstantLookupResult.NotFound;
                }

                var autoloaded = result.Value as AutoloadedConstant;
                if (autoloaded == null) {
                    value = result;
                    return ConstantLookupResult.Found;
                }

                if (autoloadScope == null) {
                    return ConstantLookupResult.FoundAutoload;
                }

                if (autoloadScope.Context != Context) {
                    throw RubyExceptions.CreateTypeError(String.Format("Cannot autoload constants to a foreign runtime #{0}", autoloadScope.Context.RuntimeId));
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
        private RubyModule TryResolveConstantNoAutoloadCheck(bool inherited, string/*!*/ name, out ConstantStorage value) {
            Context.RequiresClassHierarchyLock();

            var storage = default(ConstantStorage);
            RubyModule owner = null;
            if (ForEachAncestor(inherited, (module) => (owner = module).TryGetConstantNoAutoloadCheck(name, out storage))) {
                value = storage;
                return owner;
            } else {
                value = storage;
                return null;
            }
        }

        // Returns the owner of the constant (this module) or null if the constant is not found.
        internal bool TryGetConstantNoAutoloadCheck(string/*!*/ name, out ConstantStorage storage) {
            Context.RequiresClassHierarchyLock();

            if (name.Length == 0) {
                storage = default(ConstantStorage);
                return false;
            }

            InitializeConstantsNoLock();
            if (_constants.TryGetValue(name, out storage)) {
                if (storage.IsRemoved) {
                    storage = default(ConstantStorage);
                    return false;
                } else {
                    return true;
                }
            }

            if (_namespaceTracker != null) {
                object value;
                if (_namespaceTracker.TryGetValue(name, out value)) {
                    storage = new ConstantStorage( _context.TrackerToModule(value));
                    return true;
                }
            }

            storage = default(ConstantStorage);
            return false;
        }

        // Direct lookup into the constant table (if it exists).
        internal bool TryGetConstantNoAutoloadNoInit(string/*!*/ name, out ConstantStorage storage) {
            Context.RequiresClassHierarchyLock();
            storage = default(ConstantStorage);
            return _constants != null && _constants.TryGetValue(name, out storage) && !storage.IsRemoved;
        }

        // thread-safe:
        public bool TryRemoveConstant(string/*!*/ name, out object value) {
            using (Context.ClassHierarchyLocker()) {
                return TryRemoveConstantNoLock(name, out value);
            }
        }

        private bool TryRemoveConstantNoLock(string/*!*/ name, out object value) {
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();

            bool result;
            ConstantStorage storage;
            if (_constants.TryGetValue(name, out storage)) {
                if (storage.IsRemoved) {
                    value = null;
                    return false;
                } else {
                    value = storage.Value;
                    result = true;
                }
            } else {
                value = null;
                result = false;
            }

            object namespaceValue;
            if (_namespaceTracker != null && _namespaceTracker.TryGetValue(name, out namespaceValue)) {
                _constants[name] = ConstantStorage.Removed;
                _context.ConstantAccessVersion++;
                value = namespaceValue;
                result = true;
            } else if (result) {
                _constants.Remove(name);
                _context.ConstantAccessVersion++;
            }

            return result;
        }

        public bool EnumerateConstants(Func<RubyModule, string, object, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            InitializeConstantsNoLock();

            foreach (var constant in _constants) {
                var name = constant.Key;
                var storage = constant.Value;
                if (!storage.IsRemoved && action(this, name, storage.Value)) {
                    return true;
                }
            }

            if (_namespaceTracker != null) {
                foreach (KeyValuePair<string, object> constant in _namespaceTracker) {
                    string name = constant.Key;
                    // we check if we haven't already yielded the value so that we don't yield values hidden by a user defined constant:
                    if (!_constants.ContainsKey(name) && action(this, name, constant.Value)) {
                        return true;
                    }
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

                // Skip CLR modules (methods declared on CLR modules have already been looked for in the class).
                // If 'this' is a CLR module, we want to visit all mixed-in methods.
                if (module.IsClrModule && !this.IsClrModule) return false;

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
            Debug.Assert(!IsClass);

            // CLR members: Detaches the member from its underlying type (by creating a copy).
            // TODO: check for CLR instance members, it should be an error to call module_function on them:
            var singletonClass = GetOrCreateSingletonClass();
            singletonClass.SetMethodNoEventNoLock(callerContext, name, method.Copy(RubyMemberFlags.Public, singletonClass));
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

        internal void SetMethodNoMutateNoEventNoLock(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Context.RequiresClassHierarchyLock();
            Assert.NotNull(name, method);

            if (callerContext != _context) {
                throw RubyExceptions.CreateTypeError(String.Format("Cannot define a method on a {0} `{1}' defined in a foreign runtime #{2}",
                    IsClass ? "class" : "module", _name, _context.RuntimeId));
            }

            if (method.IsUndefined && name == Symbols.Initialize) {
                throw RubyExceptions.CreateTypeError("Cannot undefine `initialize' method");
            }

            PrepareMethodUpdate(name, method);

            InitializeMethodsNoLock();
            _methods[name] = method;
        }

        internal void AddExtensionMethodsNoLock(List<ExtensionMethodInfo>/*!*/ extensions) {
            Context.RequiresClassHierarchyLock();

            PrepareExtensionMethodsUpdate(extensions);

            if (_extensionMethods == null) {
                _extensionMethods = new Dictionary<string, List<ExtensionMethodInfo>>();
            }

            foreach (var extension in extensions) {
                List<ExtensionMethodInfo> overloads;
                if (!_extensionMethods.TryGetValue(extension.Method.Name, out overloads)) {
                    _extensionMethods.Add(extension.Method.Name, overloads = new List<ExtensionMethodInfo>());
                }

                // If the signature of the extension is the same as other overloads the overload resolver should prefer non extension method.
                overloads.Add(extension);
            }
        }

        // TODO: constraints
        private static bool IsPartiallyInstantiated(Type/*!*/ type) {
            if (type.IsGenericParameter) {
                return false;
            }

            if (type.IsArray) {
                return !type.GetElementType().IsGenericParameter;
            }

            foreach (var arg in type.GetGenericArguments()) {
                if (!arg.IsGenericParameter) {
                    Debug.Assert(arg.DeclaringMethod != null);
                    return true;
                }
            }

            return false;
        }

        internal virtual void PrepareExtensionMethodsUpdate(List<ExtensionMethodInfo>/*!*/ extensions) {
            if (_dependentClasses != null) {
                foreach (var cls in _dependentClasses) {
                    cls.PrepareExtensionMethodsUpdate(extensions);
                }
            }
        }

        internal virtual void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method) {
            InitializeMethodsNoLock();

            // Prepare all classes where this module is included for a method update.
            // TODO (optimization): we might end up walking some classes multiple times, could we mark them somehow as visited?
            if (_dependentClasses != null) {
                foreach (var cls in _dependentClasses) {
                    cls.PrepareMethodUpdate(methodName, method, 0);
                }
            }
        }

        // Returns max level in which a group has been invalidated.
        internal int InvalidateGroupsInDependentClasses(string/*!*/ methodName, int maxLevel) {
            int result = -1;
            if (_dependentClasses != null) {
                foreach (var cls in _dependentClasses) {
                    result = Math.Max(result, cls.InvalidateGroupsInSubClasses(methodName, maxLevel));
                }
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
                    } else if (IsBasicObjectClass && name == Symbols.Initialize) {
                        // We prohibit removing Object#initialize to simplify object construction logic.
                        return false;
                    } else if (method.IsRemovable) {
                        // Method is used in a dynamic site or group => update version of all dependencies of this module.
                        if (method.InvalidateSitesOnOverride || method.InvalidateGroupsOnRemoval) {
                            MethodsUpdated("RemoveMethod: " + name);
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

            // TODO: BasicObject
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
            Debug.Assert(_methods != null);

            // lookup Ruby method first:    
            if (TryGetDefinedMethod(name, ref skipHidden, out method)) {
                return true;
            }

            if (virtualLookup) {
                string mangled;
                // Note: property and default indexers getters and setters use FooBar, FooBar= and [], []= names, respectively, in virtual sites:
                if ((mangled = RubyUtils.TryMangleMethodName(name)) != null && TryGetDefinedMethod(mangled, ref skipHidden, out method)
                    && method.IsRubyMember) {
                    return true;
                }

                // Special mappings:
                // Do not map to Kernel#hash/eql?/to_s to prevent recursion in case Object.GetHashCode/Equals/ToString is removed.
                if (this != Context.KernelModule) {
                    if (name == "GetHashCode" && TryGetDefinedMethod("hash", out method) && method.IsRubyMember) {
                        return true;
                    } else if (name == "Equals" && TryGetDefinedMethod("eql?", out method) && method.IsRubyMember) {
                        return true;
                    } if (name == "ToString" && TryGetDefinedMethod("to_s", out method) && method.IsRubyMember) {
                        return true;
                    }
                }
            }

            return !skipHidden && TryGetClrMember(name, virtualLookup, out method);
        }

        private bool TryGetClrMember(string/*!*/ name, bool virtualLookup, out RubyMemberInfo method) {
            // Skip hidden CLR overloads.
            // Skip lookup on types that are not visible, that are interfaces or generic type definitions.
            if (_typeTracker != null && !IsModuleType(_typeTracker.Type)) {
                // Note: Do not allow mangling for CLR virtual lookups - we want to match the overridden name exactly as is, 
                // so that it corresponds to the base method call the override stub performs.
                bool mapNames = (Restrictions & ModuleRestrictions.NoNameMapping) == 0;
                bool unmangleNames = !virtualLookup && mapNames;

                if (TryGetClrMember(_typeTracker.Type, name, mapNames, unmangleNames, out method)) {
                    _methods.Add(name, method);
                    return true;
                }
            }

            method = null;
            return false;
        }

        protected virtual bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool mapNames, bool unmangleNames, out RubyMemberInfo method) {
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
            if (_typeTracker != null && !IsModuleType(_typeTracker.Type)) {
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

        public bool EnumerateClassVariables(Func<RubyModule, string, object, bool>/*!*/ action) {
            if (_classVariables != null) {
                foreach (KeyValuePair<string, object> variable in _classVariables) {
                    if (action(this, variable.Key, variable.Value)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Mixins (thread-safe)

        /// <summary>
        /// Returns true if the CLR type is treated as Ruby module (as opposed to a Ruby class)
        /// </summary>
        public static bool IsModuleType(Type/*!*/ type) {
            return type.IsInterface || type.IsGenericTypeDefinition;
        }

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
                if (module.IsInterface && !CanIncludeClrInterface) {
                    if (Array.IndexOf(_mixins, module) == -1) {
                        throw new InvalidOperationException(String.Format(
                            "Interface `{0}' cannot be included in class `{1}' because its underlying type has already been created",
                            module.Name, Name
                        ));
                    }
                } 
            }

            MixinsUpdated(_mixins, _mixins = expanded);
            _context.ConstantAccessVersion++;
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
        public static RubyModule[]/*!*/ ExpandMixinsNoLock(RubyClass superClass, RubyModule/*!*/[]/*!*/ modules) {
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

        internal List<Type> GetImplementedInterfaces() {
            List<Type> interfaces = new List<Type>();
            using (Context.ClassHierarchyLocker()) {
                foreach (RubyModule m in _mixins) {
                    if (m.IsInterface && !m.TypeTracker.Type.IsGenericTypeDefinition && !interfaces.Contains(m.TypeTracker.Type)) {
                        interfaces.Add(m.TypeTracker.Type);
                    }
                }
            }
            return interfaces;
        }

        private void IncludeTraitNoLock(ref Action<RubyModule> initializer, MemberTableState tableState, Action<RubyModule>/*!*/ trait) {
            Assert.NotNull(trait);

            if (tableState == MemberTableState.Uninitialized) {
                if (initializer != null && initializer != EmptyInitializer) {
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
                    IncludeTraitNoLock(ref _constantsInitializer, _constantsState, constantsInitializer);
                }

                if (classTrait != null) {
                    var singleton = GetOrCreateSingletonClass();
                    singleton.IncludeTraitNoLock(ref singleton._methodsInitializer, singleton._methodsState, classTrait);
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

        public MutableString GetDisplayName(RubyContext/*!*/ context, bool showEmptyName) {
            if (IsSingletonClass) {
                RubyClass c = (RubyClass)this;
                object singletonOf;
                MutableString result = MutableString.CreateMutable(context.GetIdentifierEncoding());

                int nestings = 0;
                while (true) {
                    nestings++;
                    result.Append("#<Class:");

                    singletonOf = c.SingletonClassOf;

                    RubyModule module = singletonOf as RubyModule;

                    if (module == null || !module.IsSingletonClass && module.Name == null) {
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
                    return null;
                } else {
                    MutableString result = MutableString.CreateMutable(context.GetIdentifierEncoding());
                    result.Append("#<");
                    result.Append(_context.GetClassOf(this).GetName(context));
                    result.Append(':');
                    RubyUtils.AppendFormatHexObjectId(result, RubyUtils.GetObjectId(_context, this));
                    result.Append('>');
                    return result;
                }
            } else {
                return MutableString.CreateMutable(GetName(context), context.GetIdentifierEncoding());
            }
        }

        #endregion      

        #region Debug View

        // see: RubyObject.DebuggerDisplayValue
        internal string/*!*/ GetDebuggerDisplayValue(object obj) {
            return Context.Inspect(obj).ConvertToString();
        }

        // see: RubyObject.DebuggerDisplayType
        internal string/*!*/ GetDebuggerDisplayType() {
            return Name;
        }

        internal sealed class DebugView {
            private readonly RubyModule/*!*/ _obj;

            public DebugView(RubyModule/*!*/ obj) {
                Assert.NotNull(obj);
                _obj = obj;
            }

            #region RubyObjectDebugView

            [DebuggerDisplay("{GetModuleName(A),nq}", Name = "{GetClassKind(),nq}", Type = "")]
            public object A {
                get { return _obj.ImmediateClass; }
            }

            [DebuggerDisplay("{B}", Name = "tainted?", Type = "")]
            public bool B {
                get { return _obj.IsTainted; }
                set { _obj.IsTainted = value; }
            }

            [DebuggerDisplay("{C}", Name = "untrusted?", Type = "")]
            public bool C {
                get { return _obj.IsUntrusted; }
                set { _obj.IsUntrusted = value; }
            }

            [DebuggerDisplay("{D}", Name = "frozen?", Type = "")]
            public bool D {
                get { return _obj.IsFrozen; }
                set { if (value) { _obj.Freeze(); } }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object E {
                get {
                    var instanceData = _obj.TryGetInstanceData();
                    if (instanceData == null) {
                        return new RubyInstanceData.VariableDebugView[0];
                    }

                    return instanceData.GetInstanceVariablesDebugView(_obj.ImmediateClass.Context);
                }
            }

            private string GetClassKind() {
                return _obj.ImmediateClass.IsSingletonClass ? "singleton class" : "class";
            }

            private static string GetModuleName(object module) {
                var m = (RubyModule)module;
                return m != null ? m.GetDisplayName(m.Context, false).ToString() : null;
            }

            #endregion

            [DebuggerDisplay("{GetModuleName(F),nq}", Name = "super", Type = "")]
            public object F {
                get { return _obj.GetSuperClass(); }
            }

            [DebuggerDisplay("", Name = "mixins", Type = "")]
            public object G {
                get { return _obj.GetMixins(); }
            }

            [DebuggerDisplay("", Name = "instance methods", Type = "")]
            public object H {
                get { return GetMethods(RubyMethodAttributes.Instance); }
            }

            [DebuggerDisplay("", Name = "singleton methods", Type = "")]
            public object I {
                get { return GetMethods(RubyMethodAttributes.Singleton); }
            }

            private Dictionary<string, RubyMemberInfo> GetMethods(RubyMethodAttributes attributes) {
                // TODO: custom view for methods, sorted
                var result = new Dictionary<string, RubyMemberInfo>();
                using (_obj.Context.ClassHierarchyLocker()) {
                    _obj.ForEachMember(false, attributes | RubyMethodAttributes.VisibilityMask, (name, _, info) => {
                        result[name] = info;
                    });
                }
                return result;
            }

            // TODO: class variables
        }

        #endregion
    }
}
