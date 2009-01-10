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
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using MSA = System.Linq.Expressions;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using System.Collections.ObjectModel;

namespace IronRuby.Builtins {

    // TODO: freezing
#if DEBUG
    [DebuggerDisplay("{DebugName}")]
#endif
    public partial class RubyModule : IDuplicable {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly RubyModule[]/*!*/ EmptyArray = new RubyModule[0];

        private enum State {
            Uninitialized,
            Initializing,
            Initialized
        }

        private static int _globalVersion = 0;

        // TODO: more versions: constants, methods, etc.
        private int _version;

        // TODO: debug only
        private int _updateCounter; // number of updates of this module

        private readonly RubyContext/*!*/ _context;

        // non-null after primitive classes initialized:
        private RubyClass _singletonClass;

        // null means lookup the execution context's global namespace 
        private Dictionary<string, object> _constants;

        private Dictionary<string, RubyMemberInfo> _methods;
        private Dictionary<string, object> _classVariables;

        /// <summary>
        /// The entire list of modules included in this one.  Newly-added mixins are at the front of the array.
        /// When adding a module that itself contains other modules, Ruby tries to maintain the ordering of the
        /// contained modules so that method resolution is reasonably consistent.
        /// </summary>
        private RubyModule[]/*!*/ _mixins;

        private string _name;
        private State _state;
        private Action<RubyModule> _initializer;

        // namespace constants:
        private IAttributesCollection _clrConstants;

        // need to know if this is an interface module
        private readonly TypeTracker _tracker;

        /// <summary>
        /// A list of dependent modules. Forms a DAG.
        /// TODO: Use weak references.
        /// </summary>
        private List<RubyModule>/*!*/ _dependentModules = new List<RubyModule>();

#if DEBUG
        private string _debugName;
        public string DebugName { get { return _debugName ?? _name ?? "<anonymous>"; } set { _debugName = value; } }
#endif
        public TypeTracker Tracker {
            get { return _tracker; }
        }

        public RubyClass/*!*/ SingletonClass {
            get {
                Debug.Assert(_singletonClass != null);
                return _singletonClass;
            }
            // friend: RubyContext:InitializePrimitives
            internal set {
                Assert.NotNull(value);
                _singletonClass = value;
            }
        }

        public bool IsDummySingletonClass {
            get { return ReferenceEquals(_singletonClass, this); }
        }

        // TODO: We hold on RubyModule in rules. That keeps the module alive until the rule dies. 
        // We can allocate version in a separate object and let rule point to that object.
        [Emitted]
        public int Version {
            get { return _version; }
        }

        internal static PropertyInfo/*!*/ VersionProperty {
            get { return typeof(RubyModule).GetProperty("Version"); }
        }

        public string Name {
            get { return _name; }
            internal set { _name = value; }
        }

        public string GetName(RubyContext/*!*/ context) {
            return context == _context ? _name : _name + "@" + _context.RuntimeId;
        }

        public virtual bool IsSingletonClass {
            get { return false; }
        }

        public RubyContext/*!*/ Context {
            [Emitted]
            get { return _context; }
        }

        public virtual bool IsClass {
            get { return false; }
        }

        public bool IsInterface {
            get { return _tracker != null && _tracker.Type.IsInterface; }
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
        protected RubyModule(RubyClass/*!*/ rubyClass, string name)
            : this(rubyClass.Context, name, null, null, null) {

            // all modules need a singleton (see RubyContext.CreateModule):
            rubyClass.Context.CreateDummySingletonClassFor(this, rubyClass, null);
        }

        internal RubyModule(RubyContext/*!*/ context, string name, Action<RubyModule> initializer,
            IAttributesCollection clrConstants, TypeTracker tracker) {
            Assert.NotNull(context);

            _context = context;
            _name = name;
            _state = State.Uninitialized;
            _mixins = EmptyArray;
            _initializer = initializer;
            _version = Interlocked.Increment(ref _globalVersion);
            _clrConstants = clrConstants;
            _tracker = tracker;
        }

        #region Initialization, Versioning

        private void InitializeMembers() {
            Debug.Assert(_state == State.Uninitialized);
            Debug.Assert(_constants == null && _methods == null, "Tables are null until initialized");

            Debug.Assert(_context.ObjectClass != null, "ObjectClass should already be initialized");

            if (DeclaresGlobalConstants) {
                _constants = null;
            } else {
                _constants = new Dictionary<string, object>();
            }

            _methods = new Dictionary<string, RubyMemberInfo>();

            Utils.Log(_name ?? "<anonymous>", "INITED");
            
            _state = State.Initializing;
            try {
                if (_initializer != null) {
                    _initializer(this);
                }
            } finally {
                _initializer = null;
                _state = State.Initialized;
            }
        }

        internal virtual void EnsureInitialized() {
            lock(this) { // TODO: There is more work to be done to improve module initialization thread-safety and performance
                if (_state == State.Uninitialized) {
                    for (int i = 0; i < _mixins.Length; i++) {
                        _mixins[i].EnsureInitialized();
                    }

                    if (_state == State.Uninitialized) {
                        InitializeMembers();
                    }
                }
            }
        }

        private void EnsureInitializedClassVariables() {
            if (_classVariables == null) {
                _classVariables = new Dictionary<string, object>();
            }
        }

        internal void AddDependentModule(RubyModule/*!*/ dependentModule) {
            Assert.NotNull(dependentModule);
            if (!_dependentModules.Contains(dependentModule)) {
                _dependentModules.Add(dependentModule);
            }
        }

        internal void SetDependency(IList<RubyModule>/*!*/ modules) {
            for (int i = 0; i < modules.Count; i++) {
                modules[i].AddDependentModule(this);
            }
        }

        internal void Updated(string/*!*/ reason) {
            int affectedModules = 0;
            int counter = Updated(ref affectedModules);

            if (affectedModules > 1) {
                Utils.Log(String.Format("{0,-50} {1,-30} affected={2,-5} total={3,-5}", Name, reason, affectedModules, counter), "UPDATED");
            }
        }

        // TODO: optimize
        private int Updated(ref int affectedModules) {
            _version = Interlocked.Increment(ref _globalVersion);

            for (int i = 0; i < _dependentModules.Count; i++) {
                _dependentModules[i].Updated(ref affectedModules);
            }

            // debug:
            affectedModules++;
            return Interlocked.Increment(ref _updateCounter);
        }

        internal void InitializeMembersFrom(RubyModule/*!*/ module) {
            ContractUtils.RequiresNotNull(module, "module");

#if !SILVERLIGHT // missing Clone on Delegate
            if (module.DeclaresGlobalConstants || module._clrConstants != null && _constants == null) {
#endif
                EnsureInitialized();
                module.EnsureInitialized();
#if !SILVERLIGHT
            } else {
                _state = module._state;
                _initializer = (module._initializer != null) ? (Action<RubyModule>)module._initializer.Clone() : null;
            }
#endif

            if (module.DeclaresGlobalConstants) {
                Debug.Assert(module._constants == null && module._clrConstants == null);
                Debug.Assert(_constants != null);
                _constants.Clear();
                foreach (KeyValuePair<SymbolId, object> constant in _context.TopGlobalScope.Items) {
                    _constants.Add(SymbolTable.IdToString(constant.Key), constant.Value);
                }
            } else {
                _constants = (module._constants != null) ? new Dictionary<string, object>(module._constants) : null;

                // copy namespace members:
                if (module._clrConstants != null) {
                    Debug.Assert(_constants != null);
                    foreach (KeyValuePair<SymbolId, object> constant in module._clrConstants.SymbolAttributes) {
                        _constants.Add(SymbolTable.IdToString(constant.Key), constant.Value);
                    }
                }
            }

            _methods = (module._methods != null) ? new Dictionary<string, RubyMemberInfo>(module._methods) : null;
            _classVariables = (module._classVariables != null) ? new Dictionary<string, object>(module._classVariables) : null;
            _mixins = ArrayUtils.Copy(module._mixins);

            // dependentModules - skip
            // tracker - skip, .NET members not copied

            Updated("InitializeFrom");
        }

        public void InitializeModuleCopy(RubyModule/*!*/ module) {
            if (_context.IsObjectFrozen(this)) {
                throw RubyExceptions.CreateTypeError("can't modify frozen Module");
            }
            
            InitializeMembersFrom(module);
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            RubyModule result = CreateInstance(null);

            // singleton members are copied here, not in InitializeCopy:
            if (copySingletonMembers && !IsSingletonClass) {
                result.SingletonClass.InitializeMembersFrom(SingletonClass);
            }

            // copy instance variables, and frozen, taint flags:
            _context.CopyInstanceData(this, result, false, true, false);
            return result;
        }

        // creates an empty Module or its subclass:
        protected virtual RubyModule/*!*/ CreateInstance(string name) {
            return _context.CreateModule(name, null, null, null, null);
        }

        public static RubyModule/*!*/ CreateInstance(RubyClass/*!*/ rubyClass, string name) {
            return rubyClass == rubyClass.Context.ModuleClass ? 
                new RubyModule(rubyClass, name) : 
                new RubyModule.Subclass(rubyClass, name);
        }

        #endregion

        #region Factories

        // Ruby constructor:
        public static object CreateAnonymousModule(RubyScope/*!*/ scope, BlockParam body, RubyClass/*!*/ self) {
            RubyModule newModule = CreateInstance(self, null);
            return (body != null) ? RubyUtils.EvaluateInModule(newModule, body, newModule) : newModule;
        }

        public RubyClass/*!*/ CreateSingletonClass() {
            //^ ensures result.IsSingletonClass && !result.IsDummySingletonClass

            Debug.Assert(!IsDummySingletonClass);

            if (_singletonClass.IsDummySingletonClass) {
                _context.AppendDummySingleton(_singletonClass);
            }

            return _singletonClass;
        }

        #endregion

        #region Ancestors

        // Return true from action to terminate enumeration.
        public bool ForEachAncestor(bool inherited, Func<RubyModule, bool>/*!*/ action) {
            if (inherited) {
                return ForEachAncestor(action);
            } else {
                return ForEachDeclaredAncestor(action);
            }
        }

        protected virtual bool ForEachAncestor(Func<RubyModule, bool>/*!*/ action) {
            return ForEachDeclaredAncestor(action);
        }

        protected bool ForEachDeclaredAncestor(Func<RubyModule, bool>/*!*/ action) {
            // this module:
            if (action(this)) return true;

            // mixins:
            // (need to be walked in reverse order--last one wins)
            foreach (RubyModule m in _mixins) {
                if (action(m)) return true;
            }

            return false;
        }

        #endregion

        #region Constants

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

                _loaded = true;
                return autoloadScope.Context.Loader.LoadFile(autoloadScope.Scope, null, _path, LoadFlags.LoadOnce | LoadFlags.AppendExtensions);
            }
        }

        public string/*!*/ MakeNestedModuleName(string nestedModuleSimpleName) {
            return (ReferenceEquals(this, _context.ObjectClass) || nestedModuleSimpleName == null) ?
                nestedModuleSimpleName :
                _name + "::" + nestedModuleSimpleName;
        }

        public void ForEachConstant(bool inherited, Func<RubyModule/*!*/, string/*!*/, object, bool>/*!*/ action) {
            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {
                // notification that we entered the module (it could have no constant):
                if (action(module, null, Missing.Value)) return true;

                return module.EnumerateConstants(action);
            });
        }

        public void SetConstant(string/*!*/ name, object value) {
            if (DeclaresGlobalConstants) {
                _context.TopGlobalScope.SetName(SymbolTable.StringToId(name), value);
            } else {
                EnsureInitialized();
                _constants[name] = value;
            }

            // TODO: we don't do dynamic constant lookup, so there is no need to update the class
            // Updated("SetConstant");
        }

        /// <summary>
        /// Sets constant of this module. 
        /// Returns true if the constant is already defined in the module and it is not an autoloaded constant.
        /// </summary>
        public bool SetConstantChecked(string/*!*/ name, object value) {
            object existing;
            var result = TryLookupConstant(false, false, null, name, out existing);
            SetConstant(name, value);
            return result == ConstantLookupResult.Found;
        }
        
        public void SetAutoloadedConstant(string/*!*/ name, MutableString/*!*/ path) {
            SetConstant(name, new AutoloadedConstant(MutableString.Create(path).Freeze()));
        }

        public MutableString GetAutoloadedConstantPath(string/*!*/ name) {
            object value;
            AutoloadedConstant autoloaded;
            return (TryGetConstantNoAutoloadCheck(name, out value) 
                && (autoloaded = value as AutoloadedConstant) != null 
                && !autoloaded.Loaded) ? 
                autoloaded.Path : null;
        }

        /// <summary>
        /// Get constant defined in this module. Do not autoload. Value is null for autoloaded constant.
        /// </summary>
        public bool TryGetConstantNoAutoload(string/*!*/ name, out object value) {
            return TryGetConstant(null, name, out value);
        }

        /// <summary>
        /// Get constant defined in this module.
        /// </summary>
        public bool TryGetConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            return TryLookupConstant(false, false, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        /// <summary>
        /// Get constant defined in this module or any of its ancestors. Do not autoload. Value is null for autoloaded constant.
        /// </summary>
        public bool TryResolveConstantNoAutoload(string/*!*/ name, out object value) {
            return TryResolveConstant(null, name, out value);
        }

        /// <summary>
        /// Get constant defined in this module or any of its ancestors.
        /// </summary>
        public bool TryResolveConstant(RubyGlobalScope autoloadScope, string/*!*/ name, out object value) {
            return TryLookupConstant(true, true, autoloadScope, name, out value) != ConstantLookupResult.NotFound;
        }

        private enum ConstantLookupResult {
            NotFound = 0,
            Found = 1,
            FoundAutoload = 2,
        }

        private ConstantLookupResult TryLookupConstant(bool included, bool inherited, RubyGlobalScope autoloadScope, 
            string/*!*/ name, out object value) {

            Debug.Assert(included || !inherited);

            value = null;
            while (true) {
                object result;

                bool found = included ? 
                    TryResolveConstantNoAutoloadCheck(inherited, name, out result) :
                    TryGetConstantNoAutoloadCheck(name, out result);

                if (!found) {
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
                RemoveConstant(name);

                // load file and try lookup again:
                if (!autoloaded.Load(autoloadScope)) {
                    return ConstantLookupResult.NotFound;
                }
            }
        }

        private bool TryResolveConstantNoAutoloadCheck(bool inherited, string/*!*/ name, out object value) {
            object result = null; // C# closures can't capture "out" parameters

            bool found = ForEachAncestor(inherited, delegate(RubyModule module) {
                return module.TryGetConstantNoAutoloadCheck(name, out result);
            });

            value = result;
            return found;
        }

        // TODO: DLR interop
        private bool TryGetConstantNoAutoloadCheck(string/*!*/ name, out object value) {
            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _clrConstants == null);

                if (_context.TopGlobalScope.TryLookupName(SymbolTable.StringToId(name), out value)) {
                    value = TrackerToModule(value);
                    return true;
                }

                return false;
            }

            EnsureInitialized();

            if (_constants != null && _constants.TryGetValue(name, out value)) {
                return true;
            }

            if (_clrConstants != null && _clrConstants.TryGetValue(SymbolTable.StringToId(name), out value)) {
                value = TrackerToModule(value);
                return true;
            }

            value = null;
            return false;
        }

        // TODO: DLR interop
        public bool RemoveConstant(string/*!*/ name) {
            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _clrConstants == null);
                return _context.TopGlobalScope.TryRemoveName(SymbolTable.StringToId(name));
            }

            EnsureInitialized();

            if (_constants != null && _constants.Remove(name)) {
                return true;
            }

            if (_clrConstants != null && _clrConstants.Remove(SymbolTable.StringToId(name))) {
                return true;
            }

            return false;
        }

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
            if (DeclaresGlobalConstants) {
                Debug.Assert(_constants == null && _clrConstants == null);
                foreach (KeyValuePair<SymbolId, object> constant in _context.TopGlobalScope.Items) {
                    if (action(this, SymbolTable.IdToString(constant.Key), constant.Value)) return true;
                }

                return false;
            }

            EnsureInitialized();

            foreach (KeyValuePair<string, object> constant in _constants) {
                if (action(this, constant.Key, constant.Value)) return true;
            }

            if (_clrConstants != null) {
                foreach (KeyValuePair<SymbolId, object> constant in _clrConstants.SymbolAttributes) {
                    if (action(this, SymbolTable.IdToString(constant.Key), constant.Value)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        public void ForEachInstanceMethod(bool inherited, Func<RubyModule/*!*/, string/*!*/, RubyMemberInfo, bool>/*!*/ action) {
            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {

                // skip interfaces (methods declared on interfaces have already been looked for in the class);
                // if 'this' is an interface, we want to visit all interface methods:
                if (module.IsInterface && !this.IsInterface) return false;

                // notification that we entered the module (it could have no method):
                if (action(module, null, null)) return true;

                return module.EnumerateMethods(action);
            });
        }

        public void AddMethodAlias(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            // Alias preserves visibility and declaring module even though the alias is declared in a different module (e.g. subclass) =>
            // we can share method info (in fact, sharing is sound with Method#== semantics - it returns true on aliased methods).
            AddMethod(callerContext, name, method);
        }

        // adds method alias via define_method:
        public void AddDefinedMethod(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            // copy method, Method#== returns false on defined methods:
            AddMethod(callerContext, name, method.Copy(method.Flags, this));
        }

        // adds instance and singleton methods of a module function:
        public void AddModuleFunction(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            AddMethod(callerContext, name, method.Copy(RubyMemberFlags.Private, this));
            SingletonClass.AddMethod(callerContext, name, method.Copy(RubyMemberFlags.Public, SingletonClass));
        }

        public void SetMethodVisibility(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method, RubyMethodVisibility visibility) {
            if (method.Visibility != visibility) {
                AddMethod(callerContext, name, method.Copy((RubyMemberFlags)visibility, this));
            }
        }

        public void AddMethod(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Assert.NotNull(name, method);

            SetMethodNoEvent(callerContext, name, method);
            _context.MethodAdded(this, name);
        }

        internal void SetMethodNoEvent(RubyContext/*!*/ callerContext, string/*!*/ name, RubyMemberInfo/*!*/ method) {
            Assert.NotNull(name, method);

            if (callerContext != _context) {
                throw RubyExceptions.CreateTypeError(String.Format("Cannot define a method on a {0} `{1}' defined in a foreign runtime #{2}",
                    IsClass ? "class" : "module", _name, _context.RuntimeId));
            }

            // Update only if the current method overrides/redefines another one in the inheritance hierarchy.
            // Method lookup failures are not cached in dynamic sites. 
            // Therefore a future invocation of the method will trigger resolution in binder that will find just added method.
            // If, on the other hand, a method is overridden there might be a dynamic site that caches a method call to that method.
            // In that case we need to update version to invalidate the site.

            RubyMemberInfo overriddenMethod = ResolveMethodUsedInDynamicSite(name);

            // TODO: cache this? (definition of method_missing could have updated all subclasses):
            if (overriddenMethod == null) {
                overriddenMethod = ResolveMethodUsedInDynamicSite(Symbols.MethodMissing);
                // TODO: better check for builtin method
                if (overriddenMethod != null && overriddenMethod.DeclaringModule == _context.KernelModule && overriddenMethod is RubyMethodGroupInfo) {
                    overriddenMethod = null;
                }
            }

            EnsureInitialized();
            _methods[name] = method;

            if (overriddenMethod != null) {
                // TODO: stop updating if a module that defines a method of the same name is reached:
                Updated("SetMethod: " + name);
            }
        }

        public bool RemoveMethod(string/*!*/ name) {
            if (RemoveMethodNoEvent(name)) {
                _context.MethodRemoved(this, name);
                return true;
            }
            return false;
        }

        internal bool RemoveMethodNoEvent(string/*!*/ name) {
            EnsureInitialized();
            bool result = _methods.Remove(name);
            Updated("RemoveMethodNoEvent");
            return result;
        }

        internal void UndefineMethodNoEvent(string/*!*/ name) {
            EnsureInitialized();
            _methods[name] = RubyMethodInfo.UndefinedMethod;
            Updated("UndefineMethodNoEvent");
        }

        public void UndefineMethod(string/*!*/ name) {
            UndefineMethodNoEvent(name);
            _context.MethodUndefined(this, name);
        }

        public void HideMethod(string/*!*/ name) {
            EnsureInitialized();
            _methods[name] = RubyMethodInfo.HiddenMethod;
            Updated("HideMethod");
        }

        public void UndefineLibraryMethod(string/*!*/ name) {
            UndefineMethod(name);
        }

        public void DefineLibraryMethod(string/*!*/ name, int attributes, params Delegate[]/*!*/ overloads) {
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            SetLibraryMethod(name, MakeMethodGroupInfo(attributes, overloads), skipEvent);
        }

        public void DefineRuleGenerator(string/*!*/ name, int attributes, RuleGenerator/*!*/ generator) {
            Assert.NotNull(generator);
            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.VisibilityMask);
            bool skipEvent = ((RubyMethodAttributes)attributes & RubyMethodAttributes.NoEvent) != 0;
            SetLibraryMethod(name, new RubyCustomMethodInfo(generator, flags, this), skipEvent);
        }

        public void SetLibraryMethod(string/*!*/ name, RubyMemberInfo/*!*/ method, bool noEvent) {
            // trigger event only for non-builtins:
            if (noEvent) {
                SetMethodNoEvent(_context, name, method);
            } else {
                AddMethod(_context, name, method);
            }
        }

        private RubyMethodGroupInfo/*!*/ MakeMethodGroupInfo(int attributes, params Delegate[]/*!*/ overloads) {
            Assert.NotNullItems(overloads);

            var flags = (RubyMemberFlags)(attributes & (int)RubyMethodAttributes.MemberFlagsMask);
            return new RubyMethodGroupInfo(overloads, flags, this);
        }

        // Looks only for those methods that were used. Doesn't need to initialize method tables (a used method is always stored in a table).
        internal RubyMemberInfo ResolveMethodUsedInDynamicSite(string/*!*/ name) {
            RubyMemberInfo result = null;

            if (ForEachAncestor(delegate(RubyModule module) {
                return module._methods != null && module.TryGetMethod(name, out result);
            })) {
                // includes private methods:
                return result != null && result != RubyMethodInfo.UndefinedMethod && result.InvalidateSitesOnOverride ? result : null;
            }

            return null;
        }

        public RubyMemberInfo ResolveMethod(string/*!*/ name, bool includePrivate) {
            Assert.NotNull(name);
            EnsureInitialized();

            RubyMemberInfo result = null;

            if (ForEachAncestor(delegate(RubyModule module) {
                return module.TryGetMethod(name, out result);
            })) {
                return result != null && result != RubyMethodInfo.UndefinedMethod && IsMethodVisible(result, includePrivate) ? result : null;
            }

            return null;
        }

        internal static bool IsMethodVisible(RubyMemberInfo/*!*/ method, bool includePrivate) {
            return (
                method.Visibility == RubyMethodVisibility.Public ||
                method.Visibility == RubyMethodVisibility.Protected ||
                method.Visibility == RubyMethodVisibility.Private && includePrivate
            );
            // TODO: protected
        }

        /// <summary>
        /// Resolve method and if it is not found in this module's ancestors, resolve in Object.
        /// </summary>
        public virtual RubyMemberInfo ResolveMethodFallbackToObject(string/*!*/ name, bool includePrivate) {
            return ResolveMethod(name, includePrivate) ?? _context.ObjectClass.ResolveMethod(name, includePrivate);
        }

        public RubyMemberInfo GetMethod(string/*!*/ name) {
            ContractUtils.RequiresNotNull(name, "name");
            EnsureInitialized();

            RubyMemberInfo method;
            TryGetMethod(name, out method);
            return method;
        }

        // skip one method in the method resolution order (MRO)
        public RubyMemberInfo ResolveSuperMethod(string/*!*/ name, RubyModule/*!*/ declaringModule) {
            Assert.NotNull(name, declaringModule);

            EnsureInitialized();

            RubyMemberInfo result = null;
            bool foundModule = false;

            // start searching for the method in the MRO parent of the declaringModule:
            if (ForEachAncestor(delegate(RubyModule module) {
                if (module == declaringModule) {
                    foundModule = true;
                    return false;
                }
                return foundModule && module.TryGetMethod(name, out result);
            })) {
                return (result != RubyMethodInfo.UndefinedMethod) ? result : null;
            }

            return null;
        }

        protected bool TryGetMethod(string/*!*/ name, out RubyMemberInfo method) {
            Assert.NotNull(name);
            Debug.Assert(_methods != null);

            // lookup Ruby method first:    
            if (_methods.TryGetValue(name, out method)) {

                // method is hidden, continue resolution, but skip CLR method lookup:
                if (ReferenceEquals(method, RubyMethodInfo.HiddenMethod)) {
                    method = null;
                    return false;
                }

                return true;
            }

            // skip lookup on types that are not visible or interfaces:
            if (_tracker != null && _tracker.Type.IsVisible && !_tracker.Type.IsInterface) {
                if (TryGetClrMember(_tracker.Type, name, out method)) {
                    // We can add the resolved method to the method table since CLR types are immutable.
                    // Doing so makes next lookup faster (prevents allocating new method groups each time).
                    _methods.Add(name, method);
                    return true;
                }
            }

            return false;
        }

        protected virtual bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, out RubyMemberInfo method) {
            method = null;
            return false;
        }

        public bool EnumerateMethods(Func<RubyModule, string, RubyMemberInfo, bool>/*!*/ action) {
            EnsureInitialized();

            foreach (KeyValuePair<string, RubyMemberInfo> method in _methods) {
                if (action(this, method.Key, method.Value)) return true;
            }

            if (_tracker != null) {
                // TODO: CLR methods but removed...
            }

            return false;
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
        public void ForEachMember(bool inherited, RubyMethodAttributes attributes, Action<string/*!*/, RubyMemberInfo/*!*/>/*!*/ action) {
            var visited = new Dictionary<string, bool>();

            // We can look for instance methods, singleton methods or all methods.
            // The difference is when we stop searching.
            bool instanceMethods = (attributes & RubyMethodAttributes.Instance) != 0;
            bool singletonMethods = (attributes & RubyMethodAttributes.Singleton) != 0;

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

                } else if (member.IsUndefined) {
                    visited.Add(name, true);
                } else if (((RubyMethodAttributes)member.Visibility & attributes) != 0 && !visited.ContainsKey(name)) {
                    action(name, member);
                    visited.Add(name, true);
                }

                return false;
            });
        }

        #endregion

        #region Class variables

        public void ForEachClassVariable(bool inherited, Func<RubyModule, string, object, bool>/*!*/ action) {
            ForEachAncestor(inherited, delegate(RubyModule/*!*/ module) {
                // notification that we entered the module (it could have no class variable):
                if (action(module, null, Missing.Value)) return true;

                return module.EnumerateClassVariables(action);
            });
        }

        public void SetClassVariable(string/*!*/ name, object value) {
            EnsureInitializedClassVariables();
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
            RubyModule result = null;
            object constValue = null;
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

        #region Mixins

        public bool HasAncestor(RubyModule/*!*/ module) {
            return ForEachAncestor(true, delegate(RubyModule m) {
                return m == module;
            });
        }

        public ReadOnlyCollection<RubyModule>/*!*/ GetMixins() {
            return new ReadOnlyCollection<RubyModule>(_mixins);
        }

        internal void SetMixins(IList<RubyModule>/*!*/ modules) {
            Debug.Assert(_mixins.Length == 0);
            Debug.Assert(modules != null && CollectionUtils.TrueForAll(modules, (m) => m != null && !m.IsClass && m.Context == _context));

            // do not initialize modules:
            _mixins = MakeNewMixins(EmptyArray, modules);
            SetDependency(_mixins);
            Updated("SetMixins");
        }

        public void IncludeModules(params RubyModule[]/*!*/ modules) {
            RubyUtils.RequireMixins(this, modules);

            RubyModule[] tmp = MakeNewMixins(_mixins, modules);

            foreach (RubyModule module in tmp) {
                if (module.IsInterface) {
                    // Can't include generic interfaces
                    if (module.Tracker.Type.ContainsGenericParameters) {
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

            SetDependency(tmp);

            // initialize added modules:
            if (_state == State.Initialized) {
                foreach (RubyModule module in tmp) {
                    module.EnsureInitialized();
                }
            }

            _mixins = tmp;

            Updated("IncludeModules");
        }

        /// <summary>
        /// Build a new list of mixins based on an original list and a list of added modules
        /// </summary>
        private RubyModule[]/*!*/ MakeNewMixins(RubyModule[]/*!*/ original, IList<RubyModule/*!*/>/*!*/ updates) {
            Assert.NotNull(original);
            Assert.NotNull(updates);

            List<RubyModule> tmp = new List<RubyModule>(original);
            AddMixins(tmp, 0, updates, true);
            return tmp.ToArray();
        }

        private int AddMixins(List<RubyModule/*!*/>/*!*/ list, int index, IList<RubyModule/*!*/>/*!*/ updates, bool recursive) {
            foreach (RubyModule module in updates) {
                Assert.NotNull(module);

                if (module == this) {
                    throw RubyExceptions.CreateArgumentError("cyclic include detected");
                }
                int newIndex = list.IndexOf(module);
                if (newIndex >= 0) {
                    // Module is already present in _mixins
                    // Update the insertion point so that we retain ordering of dependencies
                    // If we're still in the initial level of recursion, repeat for module's mixins
                    index = newIndex + 1;
                    if (recursive) {
                        index = AddMixins(list, index, module._mixins, false);
                    }
                } else {
                    // Module is not yet present in _mixins
                    // Recursively insert module dependencies at the insertion point, then insert module itself
                    newIndex = AddMixins(list, index, module._mixins, false);
                    
                    // insert module only if it is not an ancestor of the superclass:
                    if (!IsSuperClassAncestor(module)) {
                        list.Insert(index, module);
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

        protected List<Type> GetClrInterfaces() {
            List<Type> interfaces = new List<Type>();
            foreach (RubyModule m in _mixins) {
                if (m.IsInterface && !interfaces.Contains(m.Tracker.Type)) {
                    interfaces.Add(m.Tracker.Type);
                }
            }
            return interfaces;
        }

        internal void IncludeTrait(Action<RubyModule>/*!*/ trait) {
            Assert.NotNull(trait);

            if (_state == State.Uninitialized) {
                if (_initializer != null) {
                    _initializer += trait;
                } else {
                    _initializer = trait;
                }
            } else {
                // TODO: postpone
                trait(this);
            }
        }

        internal void IncludeLibraryModule(Action<RubyModule> instanceTrait, Action<RubyModule> classTrait, RubyModule[]/*!*/ mixins) {
            Assert.NotNull(mixins);

            IncludeModules(mixins);

            if (instanceTrait != null) {
                IncludeTrait(instanceTrait);
            }

            if (classTrait != null) {
                SingletonClass.IncludeTrait(classTrait);
            }
        }

        #endregion

        #region Utils

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
                    return MutableString.Empty;
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
