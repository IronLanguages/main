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
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using Ast = System.Linq.Expressions.Expression;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using Microsoft.Scripting.Generation;

namespace IronRuby.Builtins {
    public sealed partial class RubyClass : RubyModule, IDuplicable {
        /// <summary>
        /// Visibility context within which all methods are visible.
        /// </summary>
        public const RubyClass IgnoreVisibility = null;

        public const string/*!*/ ClassSingletonName = "__ClassSingleton";
        public const string/*!*/ ClassSingletonSingletonName = "__ClassSingletonSingleton";
        public const string/*!*/ MainSingletonName = "__MainSingleton";

        // interlocked
        private static int _globalVersion = 0;

        // Level in class hierarchy (0 == Object)
        private readonly int _level;
        private readonly RubyClass _superClass;

        // is this class a singleton class?
        private readonly bool _isSingletonClass;

        // true for classes that are defined in Ruby
        private readonly bool _isRubyClass;
        
        // an object that the class is a singleton of (reverse _singletonClass pointer):
        private readonly object _singletonClassOf;
        
        // null for singletons:
        private Type _underlyingSystemType; // interlocked

        // TODO: thread safe?
        private RubyGlobalScope _globalScope; 

        // if this class is a struct represents its layout:
        private readonly RubyStruct.Info _structInfo;

        // TODO: thread safe?
        // whether initialize_copy can be called (the class has just been duplicated):
        private bool _isUninitializedCopy; 

        // immutable:
        private readonly Delegate/*!*/[]/*!*/ _factories;

        #region Mutable state guarded by ClassHierarchyLock

        [Emitted]
        public VersionHandle Version;

        private readonly WeakReference/*!*/ _weakSelf;

        // We postpone setting dependency edges from included mixins and the super class to those module
        // until a member table of this module is initialized. True if the edges has been set up.
        // This allows to create modules without locking. 
        private bool _dependenciesInitialized;

        #endregion

        #region Dynamic Sites

        private CallSite<Func<CallSite, object, object>> _inspectSite;
        private CallSite<Func<CallSite, object, MutableString>> _stringConversionSite;
        private CallSite<Func<CallSite, object, object, object>> _eqlSite;
        private CallSite<Func<CallSite, object, object>> _hashSite;

        public CallSite<Func<CallSite, object, object>>/*!*/ InspectSite { 
            get { return RubyUtils.GetCallSite(ref _inspectSite, Context, "inspect", 0); } 
        }
        
        public CallSite<Func<CallSite, object, MutableString>>/*!*/ StringConversionSite {
            get { return RubyUtils.GetCallSite(ref _stringConversionSite, ConvertToSAction.Make(Context)); } 
        }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ EqlSite {
            get { return RubyUtils.GetCallSite(ref _eqlSite, Context, "==", 1); }
        }

        public CallSite<Func<CallSite, object, object>>/*!*/ HashSite {
            get { return RubyUtils.GetCallSite(ref _hashSite, Context, "hash", 0); }

        }
        
        // RubyClass, RubyClass -> object
        private CallSite<Func<CallSite, object, object, object>> _classInheritedCallbackSite;

        internal void ClassInheritedEvent(RubyClass/*!*/ subClass) {
            if (_classInheritedCallbackSite == null) {
                Interlocked.CompareExchange(
                    ref _classInheritedCallbackSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(Context, Symbols.Inherited, RubyCallSignature.WithImplicitSelf(1)
                    )),
                    null
                );
            }

            _classInheritedCallbackSite.Target(_classInheritedCallbackSite, this, subClass);
        }

        // object, SymbolId -> object
        private CallSite<Func<CallSite, object, object, object>> _singletonMethodAddedCallbackSite;
        private CallSite<Func<CallSite, object, object, object>> _singletonMethodRemovedCallbackSite;
        private CallSite<Func<CallSite, object, object, object>> _singletonMethodUndefinedCallbackSite;

        // Ruby 1.8: called after method is added, except for alias_method which calls it before
        // Ruby 1.9: called before method is added
        public override void MethodAdded(string/*!*/ name) {
            Assert.NotNull(name);

            // not called on singleton classes:
            if (IsSingletonClass) {
                Context.Send(ref _singletonMethodAddedCallbackSite, Symbols.SingletonMethodAdded, _singletonClassOf, name);
            } else {
                base.MethodAdded(name);
            }
        }

        internal override void MethodRemoved(string/*!*/ name) {
            Assert.NotNull(name);

            if (IsSingletonClass) {
                Context.Send(ref _singletonMethodRemovedCallbackSite, Symbols.SingletonMethodRemoved, _singletonClassOf, name);
            } else {
                base.MethodRemoved(name);
            }
        }

        internal override void MethodUndefined(string/*!*/ name) {
            Assert.NotNull(name);

            if (IsSingletonClass) {
                Context.Send(ref _singletonMethodUndefinedCallbackSite, Symbols.SingletonMethodUndefined, _singletonClassOf, name);
            } else {
                base.MethodUndefined(name);
            }
        }

        #endregion

        internal WeakReference/*!*/ WeakSelf {
            get { return _weakSelf; }
        }

        public RubyClass SuperClass {
            get { return _superClass; } 
        }

        public int Level {
            get { return _level; }
        }

        internal override RubyClass GetSuperClass() {
            return _superClass;
        }

        public override bool IsClass {
            get { return true; }
        }

        public override bool IsSingletonClass {
            get { return _isSingletonClass; }
        }

        public object SingletonClassOf {
            get { return _singletonClassOf; }
        }

        // A class defined in Ruby code (not libraries, CLR types)
        public bool IsRubyClass {
            get { return _isRubyClass; }
        }

        internal RubyStruct.Info StructInfo {
            get { return _structInfo; }
        }

        internal override RubyGlobalScope GlobalScope {
            get { return _globalScope; }
        }

        internal void SetGlobalScope(RubyGlobalScope/*!*/ value) {
            Assert.NotNull(value);
            _globalScope = value;
        }

        protected override bool CanIncludeClrInterface {
            get { return !IsSingletonClass && (_underlyingSystemType == null); }
        }

        public override Type/*!*/ GetUnderlyingSystemType() {
            if (_isSingletonClass) {
                throw new InvalidOperationException("Singleton class doesn't have underlying system type.");
            }

            if (_underlyingSystemType == null) {
                Interlocked.Exchange(ref _underlyingSystemType, 
                    RubyTypeDispenser.GetOrCreateType(
                        _superClass.GetUnderlyingSystemType(), 
                        GetClrInterfaces(),
                        _superClass != null && (_superClass.Restrictions & ModuleRestrictions.NoOverrides) != 0
                    )
                );
            }

            Debug.Assert(_underlyingSystemType != null);
            return _underlyingSystemType;
        }

        // default allocator:
        public RubyClass(RubyClass/*!*/ rubyClass)
            : this(rubyClass.Context, null, null, null, null, null, null, rubyClass.Context.ObjectClass, null, null, null, true, false, ModuleRestrictions.None) {
            
            // all modules need a singleton (see RubyContext.CreateModule):
            InitializeDummySingletonClass(rubyClass, null);
        }
        
        // friend: RubyContext
        // tracker: non-null => show members declared on the tracker
        internal RubyClass(RubyContext/*!*/ context, string name, Type type, object singletonClassOf,
            Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer, Delegate/*!*/[] factories, RubyClass superClass, 
            RubyModule/*!*/[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo,
            bool isRubyClass, bool isSingletonClass, ModuleRestrictions restrictions)
            : base(context, name, methodsInitializer, constantsInitializer, expandedMixins, null, tracker, restrictions) {

            Debug.Assert((superClass == null) == (type == typeof(object)), "All classes have a superclass, except for Object");
            Debug.Assert(superClass != null || structInfo == null, "Object is not a struct");
            Debug.Assert(!isRubyClass || tracker == null, "Ruby class cannot have a tracker");
            Debug.Assert(singletonClassOf != null || !isSingletonClass, "Singleton classes don't have a type");
            Debug.Assert(superClass != this);

            _underlyingSystemType = type;
            _superClass = superClass;
            _isSingletonClass = isSingletonClass;
            _isRubyClass = isRubyClass;
            _singletonClassOf = singletonClassOf;
            _factories = factories ?? Utils.EmptyDelegates;

            if (superClass != null) {
                _level = superClass.Level + 1;
                _structInfo = structInfo ?? superClass._structInfo;
            } else {
                _level = 0;
            }

            _weakSelf = new WeakReference(this);
            Version = new VersionHandle(Interlocked.Increment(ref _globalVersion));
            Version.SetName(name);
        }

        #region Versioning

        internal override void IncrementVersion() {
            Version.Value = Interlocked.Increment(ref _globalVersion);
        }

        internal override void InitializeDependencies() {
            Context.RequiresClassHierarchyLock();
            if (!_dependenciesInitialized) {
                _dependenciesInitialized = true;

                // super -> class dependency:
                if (_superClass != null) {
                    _superClass.AddDependentClass(this);
                }

                // mixin -> class dependency:
                AddAsDependencyOf(Mixins);
            }
        }

        internal void AddAsDependencyOf(IList<RubyModule/*!*/>/*!*/ modules) {
            Context.RequiresClassHierarchyLock();

            for (int i = 0; i < modules.Count; i++) {
                modules[i].AddDependentClass(this);
            }
        }

        internal override void MixinsUpdated(RubyModule/*!*/[]/*!*/ oldMixins, RubyModule/*!*/[]/*!*/ newMixins) {
            Context.RequiresClassHierarchyLock();

            // visit newly inserted modules in reverse method resolution order:
            int j = oldMixins.Length - 1;
            for (int i = newMixins.Length - 1; i >= 0; i--) {
                var mixin = newMixins[i];
                if (j < 0 || mixin != oldMixins[j]) {
                    // new module:
                    mixin.AddDependentClass(this);

                    InitializeNewMixin(mixin);
                    Debug.Assert(!mixin.MethodInitializationNeeded || MethodInitializationNeeded);

                    if (!mixin.MethodInitializationNeeded) {
                        foreach (var entry in mixin.GetMethods()) {
                            // Skip mixins that are below the current mixin in MRO:
                            PrepareMethodUpdate(entry.Key, entry.Value, i + 1);
                        }
                    }
                } else {
                    // original module:
                    j--;
                }
            }
            Debug.Assert(j == -1);
        }

        internal override void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method) {
            PrepareMethodUpdate(methodName, method, 0);
        }

        internal void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method, int mixinsToSkip) {
            Context.RequiresClassHierarchyLock();

            // Bump versions of dependent classes only if the current method overrides/redefines another one in the inheritance hierarchy.
            // Method lookup failures are not cached in dynamic sites. 
            // Therefore a future invocation of the method will trigger resolution in binder that will find just added method.
            // If, on the other hand, a method is overridden there might be a dynamic site that caches a method call to that method.
            // In that case we need to update version to invalidate the site.

            // Method table doesn't need to be initialized here. No site could be bound to this class or its subclass
            // without initializing this class. So there would be nothing to invalidate.
            if (MethodInitializationNeeded) {
                return;
            }

            Debug.Assert(mixinsToSkip <= Mixins.Length);
            int modulesToSkip, updatedLevel;
            if (mixinsToSkip > 0) {
                // skip this class when looking for overridden method:
                modulesToSkip = mixinsToSkip + 1;

                // update groups in this class as well:
                updatedLevel = _level - 1;
            } else {
                modulesToSkip = 0;
                updatedLevel = _level;
            }

            RubyMemberInfo overriddenMethod = ResolveOverriddenMethod(methodName, modulesToSkip);

            if (overriddenMethod == null) {
                overriddenMethod = ResolveOverriddenMethod(Symbols.MethodMissing, modulesToSkip);

                var missingMethods = (overriddenMethod != null) ? 
                    overriddenMethod.DeclaringModule.MissingMethodsCachedInSites : 
                    Context.KernelModule.MissingMethodsCachedInSites;

                if (missingMethods != null && missingMethods.ContainsKey(methodName)) {
                    Updated("SetMethod: " + methodName);
                }
            } else {
                if (overriddenMethod.InvalidateSitesOnOverride) {
                    Updated("SetMethod: " + methodName);
                }

                // If the overridden method is not a group the groups below were already updated.
                RubyOverloadGroupInfo overriddenGroup = overriddenMethod as RubyOverloadGroupInfo;
                if (overriddenGroup != null) {
                    // It suffice to compare the level of the overridden group.
                    // Reason: If there was any overload visible to a group below updatedLevel it would be visible to the overriddenGroup as well. 
                    if (overriddenGroup.MaxCachedOverloadLevel > updatedLevel) {
                        // Search the subtree of this class and remove all method groups that cache an overload owned by a super class.
                        if (mixinsToSkip > 0) {
                            InvalidateGroupsInSubClasses(methodName, overriddenGroup.MaxCachedOverloadLevel);
                        } else {
                            InvalidateGroupsInDependentClasses(methodName, overriddenGroup.MaxCachedOverloadLevel);
                        }

                        if (method.IsRemovable) {
                            method.InvalidateGroupsOnRemoval = true;
                        }
                    }
                } else {
                    // if the overridden method requires group invalidation on removal then the overridding method requires it too:
                    if (method.IsRemovable && overriddenMethod.InvalidateGroupsOnRemoval) {
                        method.InvalidateGroupsOnRemoval = true;
                    }
                }
            }
        }

        // Looks only for those methods that were used. Doesn't need to initialize method tables (a used method is always stored in a table).
        private RubyMemberInfo ResolveOverriddenMethod(string/*!*/ name, int modulesToSkip) {
            Context.RequiresClassHierarchyLock();

            RubyMemberInfo result = null;
            bool skipHidden = false;

            if (ForEachAncestor((module) => {
                if (modulesToSkip > 0) {
                    modulesToSkip--;
                    return false;
                }

                // Skips modules whose method tables are not initialized as well as CLR methods that are not yet loaded to method tables.
                // We can do so because such methods were not used in any cache.
                //
                // Skips super-forwarder since the forwarded super ancestor would be used in a site/group, not the forwarder itself.
                // If the forwarded ancestor is overridden the forwarder will forward to the override.
                //
                // Skips "hidden" sentinels and hidden non-removable methods.
                return module.TryGetDefinedMethod(name, ref skipHidden, out result) && !result.IsSuperForwarder;
            })) {
                // includes private methods:
                return result != null && !result.IsUndefined ? result : null;
            }

            return null;
        }

        // Returns max level in which a group has been invalidated.
        internal int InvalidateGroupsInSubClasses(string/*!*/ methodName, int maxLevel) {
            // don't recurse below maxLevel:
            if (_level > maxLevel) {
                return -1;
            }

            RubyMemberInfo method;
            if (TryGetDefinedMethod(methodName, out method)) {
                var group = method as RubyOverloadGroupInfo;
                if (group != null) {
                    // version of the class has already been increased:
                    RemoveMethodNoCacheInvalidation(methodName);
                    return Math.Max(_level, InvalidateGroupsInDependentClasses(methodName, maxLevel));
                } else {
                    // don't recurse here: the new method overrides a non-group
                    return -1;
                }
            } else {
                return InvalidateGroupsInDependentClasses(methodName, maxLevel);
            }
        }

        #endregion

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            if (IsSingletonClass) {
                throw RubyExceptions.CreateTypeError("can't copy singleton class");
            }

            using (Context.ClassHierarchyLocker()) {
                RubyClass result = Duplicate(null);
                result._isUninitializedCopy = true;
                return result;
            }
        }

        public void InitializeClassCopy(RubyClass/*!*/ rubyClass) {
            if (!_isUninitializedCopy) {
                throw RubyExceptions.CreateTypeError("already initialized class");
            }
            _isUninitializedCopy = false;

            InitializeModuleCopy(rubyClass);

            // MRI clears the name in initialize_copy:
            Name = null;
        }

        /// <summary>
        /// Duplicates this class object. When duplicating singleton class a new "singletonClassOf" reference needs to be provided.
        /// NOT thread safe.
        /// </summary>
        internal RubyClass/*!*/ Duplicate(object singletonClassOf) {
            Context.RequiresClassHierarchyLock();

            Type type;
            bool isRubyClass;

            // Notes:
            // - MRI duplicates Object class so that the result doesn't inherit from Object,
            //   which we don't do (we want our class hierarchy to be rooted).
            if (this != Context.ObjectClass) {
                isRubyClass = IsRubyClass;
                type = _underlyingSystemType;
            } else {
                isRubyClass = true;
                type = null;
            }

            RubyClass result = Context.CreateClass(Name, type, singletonClassOf, null, null, null, _factories, 
                _superClass ?? Context.ObjectClass, null, null, _structInfo, isRubyClass, IsSingletonClass, ModuleRestrictions.None
            );

            if (!IsSingletonClass) {
                // singleton members are copied here, not in InitializeCopy:
                result.SingletonClass.InitializeMembersFrom(SingletonClass);

                // copy instance variables and taint flag:
                Context.CopyInstanceData(this, result, true, false);
            }
            
            // members initialized in InitializeClassFrom (invoked by "initialize_copy")
            return result;
        }

        // implements Class#new
        public static object CreateAnonymousClass(RubyScope/*!*/ scope, BlockParam body, RubyClass/*!*/ self, [Optional]RubyClass superClass) {
            RubyContext context = scope.RubyContext;
            RubyModule owner = scope.GetInnerMostModuleForConstantLookup();
            
            // MRI is inconsistent here, it triggers "inherited" event after the body of the method is evaluated.
            // In all other cases the order is event first, body next.
            RubyClass newClass = context.DefineClass(owner, null, superClass ?? context.ObjectClass, null);
            return (body != null) ? RubyUtils.EvaluateInModule(newClass, body, newClass) : newClass;
        }

        public override string/*!*/ ToString() {
            return Name;
        }

        internal override bool ForEachAncestor(Func<RubyModule, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            // walk up the class hierarchy: 
            for (RubyClass c = this; c != null; c = c._superClass) {
                if (c.ForEachDeclaredAncestor(action)) return true;
            }
            return false;
        }

        public bool IsSubclassOf(RubyClass/*!*/ super) {
            Assert.NotNull(super);

            RubyClass c = this;
            do {
                if (c == super) return true;
                c = c.SuperClass;
            } while (c != null);

            return false;
        }

        public bool IsException() {
            return IsSubclassOf(Context.ExceptionClass);
        }

        public RubyClass/*!*/ NominalClass {
            get {
                return IsSingletonClass ? SuperClass : this;
            }
        }

        public RubyClass/*!*/ GetNonSingletonClass() {
            RubyClass result = this;
            while (result != null && result.IsSingletonClass) {
                result = result._superClass;
            }
            return result;
        }
        
        // thread-safe
        public override bool IsFrozen {
            get {
                // a class is frozen if it has been frozen itself or the root of the singleton hierarchy (module, class or object) has been frozen:
                if (!_isSingletonClass) {
                    return IsModuleFrozen;
                }

                RubyClass cls = this;
                while (true) {
                    RubyModule module = cls._singletonClassOf as RubyModule;
                    if (module != null) {
                        if (module.IsSingletonClass) {
                            cls = (RubyClass)module;
                        } else {
                            return module.IsModuleFrozen;
                        }
                    } else {
                        return Context.IsObjectFrozen(cls._singletonClassOf);
                    }
                }
            }
        }

        internal RubyMemberInfo ResolveMethodMissingForSite(string/*!*/ name, RubyMethodVisibility incompatibleVisibility) {
            Context.RequiresClassHierarchyLock();
            var methodMissing = ResolveMethodForSiteNoLock(Symbols.MethodMissing, null);
            if (incompatibleVisibility == RubyMethodVisibility.None) {
                methodMissing.InvalidateSitesOnMissingMethodAddition(name, Context);
            }
            return methodMissing.Info;
        }

        #region CLR Member Lookup

        /// <summary>
        /// Stores unsucessfull look-ups of CLR type members.
        /// Reflection is not efficient at caching look-ups and we do multiple of them (due to mangling) each time a method is being resolved.
        /// </summary>
        private static Dictionary<Key<Type, string>, bool> _clrFailedMemberLookupCache = new Dictionary<Key<Type, string>, bool>();

        private static bool IsFailureCached(Type/*!*/ type, string/*!*/ methodName) {
            // check for cached lookup failure (if the cache is available):
            bool result = false;
            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                result = cache.ContainsKey(Key.Create(type, methodName));
                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }

#if DEBUG
            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Ruby: CLR member lookup failure cache " + (result ? "hit" : "miss"));
#endif
            return result;
        }

        private static void CacheFailure(Type/*!*/ type, string/*!*/ methodName) {
            // store failure to the cache if the cache is not owned by another thread:
            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                cache[Key.Create(type, methodName)] = true;
                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }
        }

        // thread safe: doesn't need any lock since it only accesses immutable state
        public bool TryGetClrMember(string/*!*/ name, out RubyMemberInfo method) {
            // Get the first class in hierarchy that represents CLR type - worse case we end up with Object.
            // Ruby classes don't represent a CLR type and hence expose no CLR members.
            RubyClass cls = this;
            while (cls.TypeTracker == null) {
                cls = cls.SuperClass;
            }

            Debug.Assert(!cls.TypeTracker.Type.IsInterface);

            // Note: We don't cache failures as this API is not used so frequently (e.g. for regular method dispatch) that we would need caching.
            method = null;
            return cls.TryGetClrMember(cls.TypeTracker.Type, name, true, 0, out method);
        }

        // thread safe: doesn't need any lock since it only accesses immutable state
        public bool TryGetClrConstructor(out RubyMemberInfo method) {
            ConstructorInfo[] ctors;
            if (TypeTracker != null && !TypeTracker.Type.IsInterface && (ctors = TypeTracker.Type.GetConstructors()) != null && ctors.Length > 0) {
                method = new RubyMethodGroupInfo(ctors, this, true);
                return true;
            }

            method = null;
            return false;
        }

        protected override bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool tryUnmangle, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();

            if (IsFailureCached(type, name)) {
                method = null;
                return false;
            }

            if (TryGetClrMember(type, name, tryUnmangle, BindingFlags.DeclaredOnly, out method)) {
                return true;
            }

            CacheFailure(type, name);
            method = null;
            return false;
        }

        private bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool tryUnmangle, BindingFlags basicBindingFlags, out RubyMemberInfo method) {
            basicBindingFlags |= BindingFlags.Public | BindingFlags.NonPublic;

            // We look only for members directly declared on the type and handle method overloads inheritance manually.  
            BindingFlags bindingFlags = basicBindingFlags | ((_isSingletonClass) ? BindingFlags.Static : BindingFlags.Instance);

            // instance methods on Object are also available in static context:
            if (type == typeof(Object)) {
                bindingFlags |= BindingFlags.Instance;
            }

            string operatorName;
            if (!_isSingletonClass && (operatorName = MapOperator(name)) != null) {
                // instance invocation of an operator:
                if (TryGetClrMethod(type, basicBindingFlags | BindingFlags.Static, true, name, null, operatorName, null, out method)) {
                    return true;
                }
            } else if (name == "[]" || name == "[]=") {
                object[] attrs = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if (attrs.Length == 1) {
                    // default indexer accessor:
                    bool isSetter = name.Length == 3;
                    if (TryGetClrProperty(type, bindingFlags, isSetter, name, ((DefaultMemberAttribute)attrs[0]).MemberName, null, out method)) {
                        return true;
                    }
                }
            } else if (name.LastCharacter() == '=') {
                string propertyName = name.Substring(0, name.Length - 1);
                string altName = tryUnmangle ? RubyUtils.TryUnmangleName(propertyName) : null;
                
                // property setter:
                if (TryGetClrProperty(type, bindingFlags, true, name, propertyName, altName, out method)) return true;

                // writeable field:
                if (TryGetClrField(type, bindingFlags, true, propertyName, altName, out method)) return true;
            } else {
                string altName = tryUnmangle ? RubyUtils.TryUnmangleName(name) : null;

                // method:
                if (TryGetClrMethod(type, bindingFlags, false, name, null, name, altName, out method)) return true;
                
                // getter:
                if (TryGetClrProperty(type, bindingFlags, false, name, name, altName, out method)) return true;

                // event:
                if (TryGetClrEvent(type, bindingFlags, name, altName, out method)) return true;

                // field:
                if (TryGetClrField(type, bindingFlags, false, name, altName, out method)) return true;
            }

            method = null;
            return false;
        }

        private static string MapOperator(string/*!*/ name) {
            switch (name) {
                case "+": return "op_Addition";
                case "-": return "op_Subtraction";
                case "/": return "op_Division";
                case "*": return "op_Multiply";
                case "%": return "op_Modulus";
                case "==": return "op_Equality";
                case "!=": return "op_Inequality";
                case ">": return "op_GreaterThan";
                case ">=": return "op_GreaterThanOrEqual";
                case "<": return "op_LessThan";
                case "<=": return "op_LessThanOrEqual";
                case "-@": return "op_UnaryNegation";
                case "+@": return "op_UnaryPlus";

                // TODO:
                case "**": return "Power"; 
                case "<<": return "LeftShift";  
                case ">>": return "RightShift"; 
                case "&": return "BitwiseAnd";  
                case "|": return "BitwiseOr";   
                case "^": return "ExclusiveOr"; 
                case "<=>": return "Compare";
                case "~": return "OnesComplement";

                default:
                    return null;
            }
        }

        internal static string MapOperator(ExpressionType/*!*/ op) {
            switch (op) {
                case ExpressionType.Add: return "+";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Equal: return "==";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Negate: return "-@";
                case ExpressionType.UnaryPlus: return "+@";
                
                case ExpressionType.Power: return "**";
                case ExpressionType.LeftShift: return "<<";
                case ExpressionType.RightShift: return ">>";
                case ExpressionType.And: return "&";
                case ExpressionType.Or: return "|";
                case ExpressionType.ExclusiveOr: return "^";
                case ExpressionType.OnesComplement: return "~";

                default:
                    return null;
            }
        }

        internal static bool IsOperator(MethodBase/*!*/ method) {
            if (!method.IsStatic || !method.IsSpecialName) {
                return false;
            }

            switch (method.Name) {
                case "op_Addition": 
                case "op_Subtraction":
                case "op_Division": 
                case "op_Multiply": 
                case "op_Modulus":
                case "op_Equality": 
                case "op_Inequality": 
                case "op_GreaterThan":
                case "op_GreaterThanOrEqual":
                case "op_LessThan":
                case "op_LessThanOrEqual":
                case "op_UnaryNegation":
                case "op_UnaryPlus": 

                // TODO:
                case "Power":
                case "LeftShift": 
                case "RightShift":
                case "BitwiseAnd":
                case "BitwiseOr": 
                case "ExclusiveOr":
                case "Compare": 
                case "OnesComplement":
                    return true;

                default:
                    return false;
            }
        }

        private sealed class ClrOverloadInfo {
            public MethodBase Overload { get; set; }
            public RubyOverloadGroupInfo Owner { get; set; }
        }

        /// <summary>
        /// There are basically 4 cases:
        /// 1) CLR method of the given name is not defined in the specified type.
        ///    Do nothing, the method will be found as we traverse the hierarhy towards the Kernel module.
        /// 2) Otherwise
        ///    1) There is no RubyMemberInfo of given <c>name</c> present in the (type..Kernel] ancestors.
        ///       We need to search all types in (type..Object] for CLR method overloads.
        ///    2) There is a RubyMemberInfo in a class, say C, in (type..Kernel]. 
        ///       We need to get CLR methods from (type..C) in addition to the members in the type.
        ///        1) C.HidesInheritedOverloads == true
        ///           All overloads of the method we look for are in [type..C).
        ///        2) C.HidesInheritedOverloads == false
        ///           All overloads of the method we look for are in [type..C) and in the RubyMemberInfo.
        /// </summary>
        private bool TryGetClrMethod(Type/*!*/ type, BindingFlags bindingFlags, bool specialNameOnly, 
            string/*!*/ name, string clrNamePrefix, string/*!*/ clrName, string altClrName, out RubyMemberInfo method) {

            // declared only:
            MemberInfo[] initialMembers = GetDeclaredClrMethods(type, bindingFlags, clrNamePrefix, clrName, altClrName);
            int initialVisibleMemberCount = GetVisibleMethodCount(initialMembers, specialNameOnly);
            if (initialVisibleMemberCount == 0) {
                // case [1]
                //
                // Note: This failure might be cached (see CacheFailure) based on the type and name, 
                // therefore it must not depend on any other mutable state:
                method = null;
                return false;
            }

            // if all CLR inherited members are to be returned we are done:
            if ((bindingFlags & BindingFlags.DeclaredOnly) == 0) {
                method = MakeGroup(initialMembers, initialVisibleMemberCount, specialNameOnly, true);
                return true;
            }

            Context.RequiresClassHierarchyLock();

            // inherited overloads:
            List<RubyClass> ancestors = new List<RubyClass>();
            RubyMemberInfo inheritedRubyMember = null;
            bool skipHidden = false;

            ForEachAncestor((module) => {
                if (module != this) {
                    if (module.TryGetDefinedMethod(name, ref skipHidden, out inheritedRubyMember) && !inheritedRubyMember.IsSuperForwarder) {
                        return true;
                    }

                    // Skip classes that have no tracker, e.g. Fixnum(tracker) <: Integer(null) <: Numeric(null) <: Object(tracker).
                    // Skip interfaces, their methods are not callable => do not include them into a method group.
                    // Skip all classes once hidden sentinel is encountered (no CLR overloads are visible since then).
                    if (!skipHidden && module.TypeTracker != null && module.IsClass) {
                        ancestors.Add((RubyClass)module);
                    }
                }

                // continue:
                return false;
            });

            // (method clr name, parameter types) => (overload, owner)
            Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo> allMethods = null;

            if (inheritedRubyMember != null) {
                // case [2.2.2]: add CLR methods from the Ruby member:
                var inheritedGroup = inheritedRubyMember as RubyOverloadGroupInfo;

                if (inheritedGroup != null) {
                    AddMethodsOverwriteExisting(ref allMethods, inheritedGroup.MethodBases, inheritedGroup.OverloadOwners, specialNameOnly);
                }
            }

            // populate classes in (type..Kernel] or (type..C) with method groups:
            for (int i = ancestors.Count - 1; i >= 0; i--) {
                var declared = GetDeclaredClrMethods(ancestors[i].TypeTracker.Type, bindingFlags, clrNamePrefix, clrName, altClrName);
                if (declared.Length != 0 && AddMethodsOverwriteExisting(ref allMethods, declared, null, specialNameOnly)) {
                    // There is no cached method that needs to be invalidated.
                    //
                    // Proof:
                    // Suppose the group being created here overridden an existing method that is cached in a dynamic site invoked on some target class.
                    // Then either the target class is above all ancestors[i] or below some. If it is above then the new group doesn't 
                    // invalidate validity of the site. If it is below then the method resolution for the cached method would create
                    // and store to method tables all method groups in between the target class and the owner of the cached method, including the 
                    // one that contain overloads of ancestors[i]. But no module below inheritedRubyMember contains a method group of the name 
                    // being currently resolved.
                    ancestors[i].AddMethodNoCacheInvalidation(name, ancestors[i].MakeGroup(allMethods.Values));
                }
            }

            if (allMethods != null) {
                // add members declared in self:
                AddMethodsOverwriteExisting(ref allMethods, initialMembers, null, specialNameOnly);

                // return the group, it will be stored in the method table by the caller:
                method = MakeGroup(allMethods.Values);
            } else {
                method = MakeGroup(initialMembers, initialVisibleMemberCount, specialNameOnly, false);
            }

            return true;
        }

        private static MemberInfo/*!*/[]/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string prefix, string/*!*/ name, string altName) {
            MemberInfo[] result = GetDeclaredClrMethods(type, bindingFlags, prefix + name);
            if (altName == null) {
                return result;
            }

            MemberInfo[] altResult = GetDeclaredClrMethods(type, bindingFlags, prefix + altName);
            return ArrayUtils.AppendRange(result, altResult);
        }

        private static MemberInfo/*!*/[]/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name) {
            // GetMember uses prefix matching if the name ends with '*', add another * to match the original name:
            if (name.LastCharacter() == '*') {
                name += "*";
            }

            return type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
        }

        // Returns the number of methods newly added to the dictionary.
        private bool AddMethodsOverwriteExisting(ref Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo> methods,
            MemberInfo/*!*/[]/*!*/ newOverloads, RubyOverloadGroupInfo/*!*/[] overloadOwners, bool specialNameOnly) {

            bool anyChange = false;
            for (int i = 0; i < newOverloads.Length; i++) {
                var method = (MethodBase)newOverloads[i];
                if (IsVisible(method, specialNameOnly)) {
                    var paramTypes = Key.Create(method.Name, new ValueArray<Type>(ReflectionUtils.GetParameterTypes(method.GetParameters())));
                    if (methods == null) {
                        methods = new Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo>();
                    }

                    methods[paramTypes] = new ClrOverloadInfo {
                        Overload = method,
                        Owner = (overloadOwners != null) ? overloadOwners[i] : null
                    };

                    anyChange = true;
                }
            }
            return anyChange;
        }

        //
        // Filters out methods that are not visible regardless of what the caller is. 
        //   - Private and internal methods are only visible when PrivateBinding is on.
        // 
        // If the method might be visible for some callers it is included into the method group and possibly ignored by overload resolver.
        //   - Public methods on public types are always visible.
        //   - Public methods on internal types are visible if they implement an interface method.
        //   - Protected methods are visible if called from a subclass.
        //   - Private methods are visible if they explicitly implement an interface method.
        // 
        private bool IsVisible(MethodBase/*!*/ method, bool specialNameOnly) {
            if (specialNameOnly && !method.IsSpecialName) {
                return false;
            }
            
            if (Context.DomainManager.Configuration.PrivateBinding) {
                return true;
            }

            if (method.IsPrivate || method.IsAssembly || method.IsFamilyAndAssembly) {
                return false;
            }

            if (method.IsProtected()) {
                return method.DeclaringType != null && method.DeclaringType.IsVisible && !method.DeclaringType.IsSealed;
            }

            return true;
        }

        private int GetVisibleMethodCount(MemberInfo[]/*!*/ members, bool specialNameOnly) {
            int count = 0;
            foreach (MethodBase method in members) {
                if (IsVisible(method, specialNameOnly)) {
                    count++;
                }
            }
            return count;
        }

        private RubyOverloadGroupInfo/*!*/ MakeGroup(ICollection<ClrOverloadInfo>/*!*/ allMethods) {
            var overloads = new MethodBase[allMethods.Count];
            var overloadOwners = new RubyOverloadGroupInfo[overloads.Length];
            int i = 0;
            foreach (var entry in allMethods) {
                overloads[i] = entry.Overload;
                overloadOwners[i] = entry.Owner;
                i++;
            }

            var result = new RubyOverloadGroupInfo(overloads, this, overloadOwners, _isSingletonClass);
            
            // update ownership of overloads owned by the new group:
            foreach (var entry in allMethods) {
                if (entry.Owner != null) {
                    entry.Owner.CachedInGroup(result);
                } else {
                    entry.Owner = result;
                }
            }

            return result;
        }

        private RubyMethodGroupInfo/*!*/ MakeGroup(MemberInfo[]/*!*/ members, int visibleMemberCount, bool specialNameOnly, bool isDetached) {
            var allMethods = new MethodBase[visibleMemberCount];
            for (int i = 0, j = 0; i < members.Length; i++) {
                var method = (MethodBase)members[i];
                if (IsVisible(method, specialNameOnly)) {
                    allMethods[j++] = method;
                }
            }

            return isDetached ? 
                new RubyMethodGroupInfo(allMethods, this, _isSingletonClass) :
                new RubyOverloadGroupInfo(allMethods, this, null, _isSingletonClass);
        }

        // TODO: Indexers can be overloaded:
        //private bool TryGetClrProperty(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, bool isWrite, out RubyMemberInfo method) {
        //    Assert.NotNull(type, name);

        //    PropertyInfo propertyInfo = type.GetProperty(name, bindingFlags);
        //    if (propertyInfo != null) {
        //        MethodInfo accessor = isWrite ? propertyInfo.GetSetMethod(false) : propertyInfo.GetGetMethod(false);
        //        if (accessor != null) {
        //            // TODO: define RubyPropertyInfo class
        //            method = new RubyMethodGroupInfo(new MethodBase[] { accessor }, this, _isSingletonClass);
        //            return true;
        //        }
        //    }

        //    method = null;
        //    return false;
        //}

        private bool TryGetClrProperty(Type/*!*/ type, BindingFlags bindingFlags, bool isWrite, 
            string/*!*/ name, string/*!*/ clrName, string altClrName, out RubyMemberInfo method) {

            return TryGetClrMethod(type, bindingFlags, true, name, isWrite ? "set_" : "get_", clrName, altClrName, out method);
        }

        private bool TryGetClrField(Type/*!*/ type, BindingFlags bindingFlags, bool isWrite, string/*!*/ name, string altName, out RubyMemberInfo method) {
            return
                TryGetClrField(type, bindingFlags, isWrite, name, out method) ? true :
                altName != null && TryGetClrField(type, bindingFlags, isWrite, altName, out method);
        }

        private bool TryGetClrField(Type/*!*/ type, BindingFlags bindingFlags, bool isWrite, string/*!*/ name, out RubyMemberInfo method) {
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            if (fieldInfo != null && !fieldInfo.IsPrivate && (!isWrite || !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)) {
                method = new RubyFieldInfo(fieldInfo, RubyMemberFlags.Public, this, isWrite);
                return true;
            }

            method = null;
            return false;
        }

        private bool TryGetClrEvent(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, string altName, out RubyMemberInfo method) {
            return
                TryGetClrEvent(type, bindingFlags, name, out method) ? true :
                altName != null && TryGetClrEvent(type, bindingFlags, altName, out method);
        }

        private bool TryGetClrEvent(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, out RubyMemberInfo method) {
            Assert.NotNull(type, name);

            EventInfo eventInfo = type.GetEvent(name, bindingFlags);
            if (eventInfo != null) {
                method = new RubyEventInfo((EventTracker)MemberTracker.FromMemberInfo(eventInfo), RubyMemberFlags.Public, this);
                return true;
            }

            method = null;
            return false;
        }

        #endregion

        #region Dynamic operations

        /// <summary>
        /// Implements Class#allocate feature.
        /// </summary>  
        public void BuildObjectAllocation(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            // check for empty arguments (handles splat correctly):
            var argsBuilder = new ArgsBuilder(0, 0, 0, false);
            argsBuilder.AddCallArguments(metaBuilder, args);

            if (!metaBuilder.Error) {
                if (!BuildAllocatorCall(metaBuilder, args, () => AstUtils.Constant(Name))) {
                    metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
                }
            }
        }
        
        /// <summary>
        /// Implements Class#new feature.
        /// </summary>
        public void BuildObjectConstruction(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            BuildObjectConstructionNoFlow(metaBuilder, args, methodName);
            metaBuilder.BuildControlFlow(args);
        }

        /// <summary>
        /// Implements Class#clr_new feature.
        /// </summary>
        public void BuildClrObjectConstruction(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            ConstructorInfo[] ctors;
            if (TypeTracker == null) {
                metaBuilder.SetError(Methods.MakeNotClrTypeError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
            } else if ((ctors = TypeTracker.Type.GetConstructors()) == null || ctors.Length == 0) {
                metaBuilder.SetError(Methods.MakeConstructorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
            } else {
                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, ctors, SelfCallConvention.NoSelf, true);
            }
        }

        public void BuildObjectConstructionNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            Debug.Assert(!IsSingletonClass, "Cannot instantiate singletons");

            Type type = GetUnderlyingSystemType();

            RubyMemberInfo initializer;
            using (Context.ClassHierarchyLocker()) {
                // check version of the class so that we invalidate the rule whenever the initializer changes:
                metaBuilder.AddVersionTest(this);

                initializer = ResolveMethodForSiteNoLock(Symbols.Initialize, IgnoreVisibility).Info;

                // Initializer resolves to Object#initializer unless overridden in a derived class.
                // We ensure that this method cannot be removed.
                Debug.Assert(initializer != null);
            }

            bool hasRubyInitializer = initializer is RubyMethodInfo;
            bool hasLibraryInitializer = !hasRubyInitializer && initializer.DeclaringModule != Context.ObjectClass;

            if (hasRubyInitializer || hasLibraryInitializer && _isRubyClass) {
                // allocate and initialize:
                bool allocatorFound = BuildAllocatorCall(metaBuilder, args, () => AstUtils.Constant(Name));
                if (metaBuilder.Error) {
                    return;
                }

                if (!allocatorFound) {
                    metaBuilder.SetError(Methods.MakeMissingDefaultConstructorError.OpCall(
                        Ast.Convert(args.TargetExpression, typeof(RubyClass)),
                        Ast.Constant(initializer.DeclaringModule.Name)
                    ));
                    return;
                }

                if (!initializer.IsEmpty) {
                    BuildOverriddenInitializerCall(metaBuilder, args, initializer);
                }
            } else {
                // construct:
                MethodBase[] constructionOverloads;
                SelfCallConvention callConvention = SelfCallConvention.SelfIsParameter;
                bool implicitProtocolConversions = false;

                if (typeof(Delegate).IsAssignableFrom(type)) {
                    BuildDelegateConstructorCall(metaBuilder, args, type);
                    return;
                } else if (type.IsArray && type.GetArrayRank() == 1) {
                    constructionOverloads = ClrVectorFactories;
                } else if (_structInfo != null) {
                    constructionOverloads = new MethodBase[] { Methods.CreateStructInstance };
                } else if (_factories.Length != 0) {
                    constructionOverloads = (MethodBase[])ReflectionUtils.GetMethodInfos(_factories);
                } else {
                    // TODO: handle protected constructors
                    constructionOverloads = type.GetConstructors();
                    if (constructionOverloads.Length == 0) {
                        metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
                        return;
                    }
                    callConvention = SelfCallConvention.NoSelf;
                    implicitProtocolConversions = true;
                }

                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, constructionOverloads, callConvention, implicitProtocolConversions);

                // we need to handle break, which unwinds to a proc-converter that could be this method's frame:
                if (!metaBuilder.Error) {
                    metaBuilder.ControlFlowBuilder = RubyMethodGroupInfo.RuleControlFlowBuilder;
                }
            }
        }

        private static void BuildOverriddenInitializerCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, RubyMemberInfo/*!*/ initializer) {
            var instanceExpr = metaBuilder.Result;
            metaBuilder.Result = null;

            var instanceVariable = metaBuilder.GetTemporary(instanceExpr.Type, "#instance");

            // We know an exact type of the new instance and that there is no singleton for that instance.
            // We also have the exact method we need to call ("initialize" is a RubyMethodInfo).
            // => no tests are necessary:
            args.SetTarget(instanceVariable, null);

            if (initializer is RubyMethodInfo) {
                initializer.BuildCallNoFlow(metaBuilder, args, Symbols.Initialize);
            } else {
                // TODO: we need more refactoring of RubyMethodGroupInfo.BuildCall to be able to inline this:
                metaBuilder.Result = Ast.Dynamic(
                    RubyCallAction.Make(args.RubyContext, "initialize", 
                        new RubyCallSignature(args.Signature.ArgumentCount, args.Signature.Flags | RubyCallFlags.HasImplicitSelf)
                    ),
                    typeof(object),
                    args.GetCallSiteArguments(instanceVariable)
                );
            }

            if (!metaBuilder.Error) {
                // PropagateRetrySingleton(instance = new <type>(), instance.initialize(<args>))
                metaBuilder.Result = Methods.PropagateRetrySingleton.OpCall(
                    Ast.Assign(instanceVariable, instanceExpr),
                    metaBuilder.Result
                );

                // we need to handle break, which unwinds to a proc-converter that could be this method's frame:
                metaBuilder.ControlFlowBuilder = RubyMethodInfo.RuleControlFlowBuilder;
            }
        }

        public bool BuildAllocatorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Func<Expression>/*!*/ defaultExceptionMessage) {
            Type type = GetUnderlyingSystemType();

            if (_structInfo != null) {
                metaBuilder.Result = Methods.AllocateStructInstance.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
                return true;
            }

            ConstructorInfo ctor;
            if (IsException()) {
                if ((ctor = type.GetConstructor(new[] { typeof(string) })) != null) {
                    metaBuilder.Result = Ast.New(ctor, defaultExceptionMessage());
                    return true;
                } else if ((ctor = type.GetConstructor(new[] { typeof(string), typeof(Exception) })) != null) {
                    metaBuilder.Result = Ast.New(ctor, defaultExceptionMessage(), AstUtils.Constant(null));
                    return true;
                }
            }

            if ((ctor = type.GetConstructor(new[] { typeof(RubyClass) })) != null) {
                metaBuilder.Result = Ast.New(ctor, AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
                return true;
            }

            if ((ctor = type.GetConstructor(new[] { typeof(RubyContext) })) != null) {
                metaBuilder.Result = Ast.New(ctor, AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)));
                return true;
            }

            if ((ctor = type.GetConstructor(Type.EmptyTypes)) != null) {
                metaBuilder.Result = Ast.New(ctor);
                return true;
            }

            return false;
        }

        private void BuildDelegateConstructorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Type/*!*/ type) {
            if (args.Signature.HasBlock) {
                var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
                if (!metaBuilder.Error) {
                    metaBuilder.Result = Methods.CreateDelegateFromProc.OpCall(
                        AstUtils.Constant(type),
                        AstUtils.Convert(args.GetBlockExpression(), typeof(Proc))
                    );
                }
            } else {
                var actualArgs = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
                if (!metaBuilder.Error) {
                    var convertBinder = args.RubyContext.CreateConvertBinder(type, true);
                    var converted = convertBinder.Bind(actualArgs[0], DynamicMetaObject.EmptyMetaObjects);
                    metaBuilder.SetMetaResult(converted, args);
                }
            }
        }

        private MethodBase/*!*/[]/*!*/ ClrVectorFactories {
            get {
                if (_clrVectorFactories == null) {
                    _clrVectorFactories = new[] { Methods.CreateVector, Methods.CreateVectorWithValues };
                }
                return _clrVectorFactories;
            }
        }

        private static MethodBase[] _clrVectorFactories;

        #endregion
    }
}
