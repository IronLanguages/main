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

#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Generation;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Builtins {
    using Ast = Expression;
    using Utils = IronRuby.Runtime.Utils;
	using AstUtils = Microsoft.Scripting.Ast.Utils;

    public sealed partial class RubyClass : RubyModule, IDuplicable {
        public const string/*!*/ MainSingletonName = "__MainSingleton";

        // Level in class hierarchy (0 == BasicObject)
        private readonly int _level;
        private readonly RubyClass _superClass;

        // Lazy interlocked init.
        // Created for classes that represent a subclass of Module class.
        private RubyClass _dummySingletonClass;

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

        // We postpone setting dependency edges from included mixins and the super class to those module
        // until a member table of this module is initialized. True if the edges has been set up.
        // This allows to create modules without locking. 
        private bool _dependenciesInitialized;

        //
        // Null if this class doesn't represent a CLR type, it represets a CLR type that implements IRubyObject or 
        // if it doesn't have any singleton-subclasses.
        // 
        // Otherwise represents a set of method names for which we need to emit singleton checks into rules.
        // A name is added to the set whenever a method of that name is defined on any singleton-subclass (or its included modules) of this class.
        // A name can be removed from this table only if no method of that name exists in any singleton-subclass (or its included modules) of this class.
        // TODO (opt): Currently we don't remove items from this dictionary. We would need to ref-count the methods.
        //
        internal Dictionary<string, bool> ClrSingletonMethods { get; set; }

        // Increased each time an extension method is defined on interfaces or generic definitions included in this class.
        internal int _extensionVersion;

        #endregion

        #region Dynamic Sites

        private CallSite<Func<CallSite, object, object>> _inspectSite;
        private CallSite<Func<CallSite, object, MutableString>> _inspectResultConversionSite;
        private CallSite<Func<CallSite, object, object, object>> _eqlSite;
        private CallSite<Func<CallSite, object, object>> _hashSite;
        private CallSite<Func<CallSite, object, object>> _toStringSite;
        private CallSite<Func<CallSite, object, object>> _toArraySplatSite;
        private CallSite<Func<CallSite, object, object, object>> _newSite;

        public CallSite<Func<CallSite, object, object>>/*!*/ InspectSite { 
            get { return RubyUtils.GetCallSite(ref _inspectSite, Context, "inspect", 0); } 
        }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ NewSite {
            get { return RubyUtils.GetCallSite(ref _newSite, Context, "new", 1); }
        }

        internal CallSite<Func<CallSite, object, object>>/*!*/ ToImplicitTrySplatSite {
            get { return RubyUtils.GetCallSite(ref _toArraySplatSite, ImplicitTrySplatAction.Make(Context)); }
        }
        
        public CallSite<Func<CallSite, object, MutableString>>/*!*/ InspectResultConversionSite {
            get { return RubyUtils.GetCallSite(ref _inspectResultConversionSite, ConvertToSAction.Make(Context)); } 
        }

        public CallSite<Func<CallSite, object, object, object>>/*!*/ EqualsSite {
            get { 
                return RubyUtils.GetCallSite(ref _eqlSite, Context, "Equals",
                    new RubyCallSignature(1, RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsVirtualCall)
                );
            }
        }

        internal CallSite<Func<CallSite, object, object>>/*!*/ GetHashCodeSite {
            get { 
                return RubyUtils.GetCallSite(ref _hashSite, Context, "GetHashCode", 
                    new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsVirtualCall)
                );
            }
        }

        public CallSite<Func<CallSite, object, object>>/*!*/ ToStringSite {
            get {
                return RubyUtils.GetCallSite(ref _toStringSite, Context, "ToString",
                    new RubyCallSignature(0, RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsVirtualCall)
                );
            }
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

        // object, symbol -> object
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

        internal void InitializeDummySingleton() {
            Debug.Assert(_dummySingletonClass == null);
            _dummySingletonClass = CreateDummySingleton();
        }

        internal RubyClass/*!*/ GetDummySingletonClass() {
            if (_dummySingletonClass == null) {
                Debug.Assert(IsSubclassOf(Context.ModuleClass));
                Interlocked.CompareExchange(ref _dummySingletonClass, CreateDummySingleton(), null);
            }
            return _dummySingletonClass;
        }

        private RubyClass/*!*/ CreateDummySingleton() {
            var result = new RubyClass(Context, null, null, this, null, null, null, this.ImmediateClass, null, null, null, false, true, ModuleRestrictions.None);
            result.InitializeImmediateClass(result);
            return result;
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
                throw RubyExceptions.CreateTypeError("A singleton class doesn't have underlying system type.");
            }

            if (_underlyingSystemType == null) {
                Interlocked.Exchange(ref _underlyingSystemType, 
                    RubyTypeDispenser.GetOrCreateType(
                        _superClass != null ? _superClass.GetUnderlyingSystemType() : typeof(BasicObject), 
                        GetImplementedInterfaces(),
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
            
            InitializeImmediateClass(rubyClass, null);
        }
        
        // friend: RubyContext
        // tracker: non-null => show members declared on the tracker
        internal RubyClass(RubyContext/*!*/ context, string name, Type type, object singletonClassOf,
            Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer, Delegate/*!*/[] factories, RubyClass superClass, 
            RubyModule/*!*/[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo,
            bool isRubyClass, bool isSingletonClass, ModuleRestrictions restrictions)
            : base(context, name, methodsInitializer, constantsInitializer, expandedMixins,
                type != typeof(object) ? null : context.Namespaces, tracker, restrictions) {

            Debug.Assert(context.Namespaces != null, "Namespaces should be initialized");
            Debug.Assert(superClass != null || structInfo == null, "BasicObject is not a struct");
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
        }

        #region Versioning

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

        /// <summary>
        /// Invalidates
        /// - failure cache on RubyClass via incrementing extension version
        /// - groups 
        /// - call sites via incrementing method version
        /// </summary>
        internal override void PrepareExtensionMethodsUpdate(List<ExtensionMethodInfo>/*!*/ extensions) {
            _extensionVersion++;

            // No groups cached, no overloads cached in call-sites.
            if (MethodInitializationNeeded) {
                return;
            }
            
            MethodsUpdated("ExtensionMethods");

            // Differences from preparing Ruby method for update:
            // - No override search. 
            //   - We need to invalidate groups regardless of whether the e.m. overrides a method or not (it needs to be added to all groups below).
            //   - We could optimize a bit if the definition overrides a method group thata hasn't been used in a call site.
            //     In that case we wouldn't need to invalidate any rules. This is rare and we would still need to invalidate groups. So we don't do that.
            // - No CLR singletons special casing - extension methods can't be added on singletons.
            foreach (var extension in extensions) {
                InvalidateGroupsInSubClasses(extension.Method.Name, Int32.MaxValue);
            }
        }

        internal override void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method) {
            PrepareMethodUpdate(methodName, method, 0);
        }

        internal void PrepareMethodUpdate(string/*!*/ methodName, RubyMemberInfo/*!*/ method, int mixinsToSkip) {
            Context.RequiresClassHierarchyLock();

            bool superClassUpdated = false;

            // A singleton subclass of a CLR type that doesn't implement IRubyObject:
            if (_isSingletonClass && !_superClass.IsSingletonClass && !_superClass.IsRubyClass && !(_singletonClassOf is IRubyObject)) {
                var ssm = _superClass.ClrSingletonMethods;
                if (ssm != null) {
                    if (!ssm.ContainsKey(methodName)) {
                        ssm[methodName] = true;
                        _superClass.MethodsUpdated("SetSingletonMethod: " + methodName);
                        superClassUpdated = true;
                    }
                } else {
                    _superClass.ClrSingletonMethods = ssm = new Dictionary<string, bool>();
                    ssm[methodName] = true;
                    _superClass.MethodsUpdated("SetSingletonMethod: " + methodName);
                    superClassUpdated = true;
                }
            }

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
                // super class and this class have already been updated, no need to update again:
                if (!superClassUpdated) {
                    var missingMethods = Context.MissingMethodsCachedInSites;
                    if (missingMethods != null && missingMethods.Contains(methodName)) {
                        MethodsUpdated("SetMethod: " + methodName);
                    }
                }
            } else {
                if (overriddenMethod.InvalidateSitesOnOverride && !superClassUpdated) {
                    MethodsUpdated("SetMethod: " + methodName);
                }

                // If the overridden method is not a group the groups below don't need to be updated since they don't include any overloads
                // from above the current method definition.
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
            if (IsBasicObjectClass) {
                throw RubyExceptions.CreateTypeError("can't copy the root class");
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

            Debug.Assert(_superClass != null, "BasicObject cannot be duplicated");

            RubyClass result = Context.CreateClass(Name, _underlyingSystemType, singletonClassOf, null, null, null, _factories,
                _superClass, null, null, _structInfo, IsRubyClass, IsSingletonClass, ModuleRestrictions.None
            );

            if (!IsSingletonClass) {
                // singleton members are copied here, not in InitializeCopy:
                result.ImmediateClass.InitializeMembersFrom(ImmediateClass);

                // copy instance variables and taint flag:
                Context.CopyInstanceData(this, result, false);
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
            return (body != null) ? RubyUtils.EvaluateInModule(newClass, body, new[] { newClass }, newClass) : newClass;
        }

        internal override bool ForEachAncestor(Func<RubyModule, bool>/*!*/ action) {
            Context.RequiresClassHierarchyLock();

            // walk up the class hierarchy: 
            for (RubyClass c = this; c != null; c = c._superClass) {
                if (c.ForEachDeclaredAncestor(action)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this class is equal to super or it is its descendant.
        /// </summary>
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
            var methodMissing = ResolveMethodForSiteNoLock(Symbols.MethodMissing, VisibilityContext.AllVisible);
            if (incompatibleVisibility == RubyMethodVisibility.None) {
                methodMissing.InvalidateSitesOnMissingMethodAddition(name, Context);
            }
            return methodMissing.Info;
        }

        #region CLR Member Lookup

        /// <summary>
        /// Stores unsucessfull look-ups of CLR type members.
        /// Reflection is not efficient at caching look-ups and we do multiple of them (due to mangling) each time a method is being resolved.
        /// { (declaring-type, method-name, isStatic) -> (type-extension-version) }
        /// </summary>
        private static Dictionary<Key<Type, string, bool>, int> _clrFailedMemberLookupCache = new Dictionary<Key<Type, string, bool>, int>();

        private static bool IsFailureCached(Type/*!*/ type, string/*!*/ methodName, bool isStatic, int extensionVersion) {
            // check for cached lookup failure (if the cache is available):
            bool result = false;
            var key = Key.Create(type, methodName, isStatic);

            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                int cachedExtensionVersion;
                if (cache.TryGetValue(key, out cachedExtensionVersion)) {
                    if (cachedExtensionVersion != extensionVersion) {
                        // can't use the cached failure if there are any new extension methods:
                        cache.Remove(key);
                        result = false;
                    } else {
                        result = true;
                    }
                } else {
                    result = false;
                }

                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }

#if DEBUG
            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "Ruby: CLR member lookup failure cache " + (result ? "hit" : "miss"));
#endif
            return result;
        }

        private static void CacheFailure(Type/*!*/ type, string/*!*/ methodName, bool isStatic, int extensionVersion) {
            // store failure to the cache if the cache is not owned by another thread:
            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                cache[Key.Create(type, methodName, isStatic)] = extensionVersion;
                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }
        }

        // thread safe: doesn't need any lock since it only accesses immutable state
        public bool TryGetClrMember(string/*!*/ name, Type asType, out RubyMemberInfo method) {
            Debug.Assert(!_isSingletonClass);

            // Get the first class in hierarchy that represents CLR type - worse case we end up with Object.
            // Ruby classes don't represent a CLR type and hence expose no CLR members.
            RubyClass cls = this;
            while (cls.TypeTracker == null) {
                cls = cls.SuperClass;
            }

            Type type = cls.TypeTracker.Type;
            Debug.Assert(!RubyModule.IsModuleType(type));

            // Note: We don't cache results as this API is not used so frequently (e.g. for regular method dispatch).
            
            if (asType != null && !asType.IsAssignableFrom(type)) {
                throw RubyExceptions.CreateNameError(String.Format("`{0}' does not inherit from `{1}'", cls.Name, Context.GetTypeName(asType, true)));
            }

            method = null;
            using (Context.ClassHierarchyLocker()) {
                return cls.TryGetClrMember(asType ?? type, name, true, true, 0, out method);
            }
        }

        // thread safe: doesn't need any lock since it only accesses immutable state
        public bool TryGetClrConstructor(out RubyMemberInfo method) {
            OverloadInfo[] ctors;
            if (TypeTracker != null && (ctors = GetConstructors(TypeTracker.Type)).Length > 0) {
                method = new RubyMethodGroupInfo(ctors, this, true);
                return true;
            }

            method = null;
            return false;
        }

        protected override bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool mapNames, bool unmangleNames, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();

            if (IsFailureCached(type, name, _isSingletonClass, _extensionVersion)) {
                method = null;
                return false;
            }

            if (TryGetClrMember(type, name, mapNames, unmangleNames, BindingFlags.DeclaredOnly, out method)) {
                return true;
            }

            CacheFailure(type, name, _isSingletonClass, _extensionVersion);
            method = null;
            return false;
        }

        /// <summary>
        /// Returns a fresh instance of RubyMemberInfo each time it is called. The caller needs to cache it if appropriate.
        /// May add or use method groups to/from super-clases if BindingFlags.DeclaredOnly is used.
        /// </summary>
        private bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, bool mapNames, bool unmangleNames, BindingFlags basicBindingFlags, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();

            basicBindingFlags |= BindingFlags.Public | BindingFlags.NonPublic;

            // We look only for members directly declared on the type and handle method overloads inheritance manually.  
            BindingFlags bindingFlags = basicBindingFlags | ((_isSingletonClass) ? BindingFlags.Static : BindingFlags.Instance);

            // instance methods on Object are also available in static context:
            if (type == typeof(Object)) {
                bindingFlags |= BindingFlags.Instance;
            }

            string operatorName;
            if (mapNames && !_isSingletonClass && (operatorName = RubyUtils.ToClrOperatorName(name)) != null) {
                // instance invocation of an operator:
                if (TryGetClrMethod(type, basicBindingFlags | BindingFlags.Static, true, name, null, operatorName, null, out method)) {
                    return true;
                }
            } else if (mapNames && (name == "[]" || name == "[]=")) {
                if (type.IsArray && !_isSingletonClass) {
                    bool isSetter = name.Length == 3;
                    TryGetClrMethod(type, bindingFlags, false, name, null, isSetter ? "Set" : "Get", null, out method);
                    Debug.Assert(method != null);
                    return true;
                } else {
                    object[] attrs = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                    if (attrs.Length == 1) {
                        // default indexer accessor:
                        bool isSetter = name.Length == 3;
                        if (TryGetClrProperty(type, bindingFlags, isSetter, name, ((DefaultMemberAttribute)attrs[0]).MemberName, null, out method)) {
                            return true;
                        }
                    }
                }
            } else if (name.LastCharacter() == '=') {
                string propertyName = name.Substring(0, name.Length - 1);
                string altName = unmangleNames ? RubyUtils.TryUnmangleMethodName(propertyName) : null;
                
                // property setter:
                if (TryGetClrProperty(type, bindingFlags, true, name, propertyName, altName, out method)) return true;

                // writeable field:
                if (TryGetClrField(type, bindingFlags, true, propertyName, altName, out method)) return true;
            } else {
                string altName = unmangleNames ? RubyUtils.TryUnmangleMethodName(name) : null;

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

        protected override IEnumerable<string/*!*/>/*!*/ EnumerateClrMembers(Type/*!*/ type) {
            var basicBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

            // default indexer:
            string defaultIndexerName = null;
            if (!_isSingletonClass) {
                object[] attrs = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if (attrs.Length == 1) {
                    defaultIndexerName = ((DefaultMemberAttribute)attrs[0]).MemberName;
                }
            }

            foreach (MethodInfo method in type.GetMethods(basicBindingFlags | BindingFlags.Static | BindingFlags.Instance)) {
                if (IsVisible(method.Attributes, method.DeclaringType, false)) {
                    if (method.IsSpecialName) {
                        var name = RubyUtils.MapOperator(method);
                        if (name != null) {
                            yield return name;
                        }

                        if (method.IsStatic == _isSingletonClass) {
                            if (method.Name.StartsWith("get_")) {
                                var propertyName = method.Name.Substring(4);
                                yield return propertyName;

                                if (propertyName == defaultIndexerName) {
                                    yield return "[]";
                                }
                            }

                            if (method.Name.StartsWith("set_")) {
                                var propertyName = method.Name.Substring(4);
                                yield return propertyName + "=";

                                if (propertyName == defaultIndexerName) {
                                    yield return "[]=";
                                }
                            }
                        }
                    } else if (method.IsStatic == _isSingletonClass) {
                        yield return method.Name;
                    }
                }
            }

            var bindingFlags = basicBindingFlags | (_isSingletonClass ? BindingFlags.Static : BindingFlags.Instance);

            foreach (FieldInfo field in type.GetFields(bindingFlags)) {
                if (IsVisible(field)) {
                    yield return field.Name;

                    if (IsWriteable(field)) {
                        yield return field.Name + "=";
                    }
                }
            }

            foreach (EventInfo evnt in type.GetEvents(bindingFlags)) {
                yield return evnt.Name;
            }

        }
        
        private sealed class ClrOverloadInfo {
            public OverloadInfo Overload { get; set; }
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
        /// <remarks>
        /// Doesn't include explicitly implemented interface methods. Including them would allow to call them directly (obj.foo) 
        /// if the overload resolution succeeds. However, the interface methods are probably implemented explicitly for a reason:
        /// 1) There is a conflict in signatures -> the overload resolution would probably fail.
        /// 2) The class was designed with an intention to not expose the implementations directly.
        /// </remarks>
        private bool TryGetClrMethod(Type/*!*/ type, BindingFlags bindingFlags, bool specialNameOnly, 
            string/*!*/ name, string clrNamePrefix, string/*!*/ clrName, string altClrName, out RubyMemberInfo method) {
            Context.RequiresClassHierarchyLock();

            // declared only:
            List<OverloadInfo> initialMembers = new List<OverloadInfo>(GetDeclaredClrMethods(type, bindingFlags, clrNamePrefix, clrName, altClrName, specialNameOnly));
            if (initialMembers.Count == 0) {
                // case [1]
                //
                // Note: This failure might be cached (see CacheFailure) based on the type and name, 
                // therefore it must not depend on any other mutable state:
                method = null;
                return false;
            }

            // If all CLR inherited members are to be returned we are done.
            // (creates a detached info; used by Kernel#clr_member)
            if ((bindingFlags & BindingFlags.DeclaredOnly) == 0) {
                method = MakeGroup(initialMembers, initialMembers.Count, specialNameOnly, true);
                return true;
            }

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
                    // Skip CLR modules, their methods are not callable => do not include them into a method group.
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
                } else if (inheritedRubyMember.IsRemovable) {
                    // The groups created below won't contain overloads defined above (if there are any).
                    // If this method is removed we need to invalidate them.
                    inheritedRubyMember.InvalidateGroupsOnRemoval = true;
                }
            }

            // populate classes in (type..Kernel] or (type..C) with method groups:
            for (int i = ancestors.Count - 1; i >= 0; i--) {
                var declared = ancestors[i].GetDeclaredClrMethods(ancestors[i].TypeTracker.Type, bindingFlags, clrNamePrefix, clrName, altClrName, specialNameOnly);
                if (AddMethodsOverwriteExisting(ref allMethods, declared, null, specialNameOnly)) {
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
                method = MakeGroup(initialMembers, initialMembers.Count, specialNameOnly, false);
            }

            return true;
        }

        private IEnumerable<OverloadInfo>/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string prefix, string/*!*/ name, string altName, bool specialNameOnly) {
            string memberName = prefix + name;
            string altMemberName = prefix + altName;

            MemberInfo[] methods = GetDeclaredClrMethods(type, bindingFlags, memberName);
            MemberInfo[] altMethods = (altName != null) ? GetDeclaredClrMethods(type, bindingFlags, altMemberName) : Utils.EmptyMemberInfos;

            foreach (MethodBase method in methods.Concat(altMethods)) {
                if (IsVisible(method.Attributes, method.DeclaringType, specialNameOnly)) {
                    yield return new ReflectionOverloadInfo(method);
                }
            }

            if ((bindingFlags & BindingFlags.Instance) != 0) {
                var extensions = GetClrExtensionMethods(type, memberName);
                var altExtensions = GetClrExtensionMethods(type, altMemberName);

                foreach (var extension in extensions.Concat(altExtensions)) {
                    // TODO: inherit ExtensionMethodInfo <: OverloadInfo?
                    yield return new ReflectionOverloadInfo(extension.Method);
                }
            }
        }

        private static MemberInfo/*!*/[]/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name) {
            // GetMember uses prefix matching if the name ends with '*', add another * to match the original name:
            if (name.LastCharacter() == '*') {
                name += "*";
            }

            return type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
        }

        private IEnumerable<ExtensionMethodInfo>/*!*/ GetClrExtensionMethods(Type/*!*/ type, string/*!*/ name) {
            List<ExtensionMethodInfo> extensions;
            if (_extensionMethods != null && _extensionMethods.TryGetValue(name, out extensions)) {
                foreach (var extension in extensions) {
                    // Don't check IsExtensionOf: the target type of an extension method stored in _extensionMethods is 
                    // an instantiation (not parameterized), a generic parameter T (type == Object), or T[] (type == Array). 
                    // If the method's parameter is a constrained generic parameter (T or T[]) this might yield methods that
                    // shouldn't be available on the current type. They are filtered out in overload resolution.
                    // TODO: these methods show up in obj.methods; we need to fix that
                    yield return extension;
                }
            }

            foreach (var mixin in Mixins) {
                if (mixin._extensionMethods != null && mixin._extensionMethods.TryGetValue(name, out extensions)) {
                    foreach (var extension in extensions) {
                        if (extension.IsExtensionOf(type)) {
                            yield return extension;
                        }
                    }
                }
            }
        }

        // Returns the number of methods newly added to the dictionary.
        private bool AddMethodsOverwriteExisting(ref Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo> methods,
            IEnumerable<OverloadInfo/*!*/>/*!*/ newOverloads, RubyOverloadGroupInfo/*!*/[] overloadOwners, bool specialNameOnly) {

            bool anyChange = false;
            int i = 0;
            foreach (var method in newOverloads) {
                if (IsVisible(method.Attributes, method.DeclaringType, specialNameOnly)) {
                    var paramTypes = Key.Create(method.Name, new ValueArray<Type>(ReflectionUtils.GetParameterTypes(method.Parameters)));
                    if (methods == null) {
                        methods = new Dictionary<Key<string, ValueArray<Type>>, ClrOverloadInfo>();
                    }

                    methods[paramTypes] = new ClrOverloadInfo {
                        Overload = method,
                        Owner = (overloadOwners != null) ? overloadOwners[i] : null
                    };

                    anyChange = true;
                }
                i++;
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
        private bool IsVisible(MethodAttributes/*!*/ attributes, Type declaringType, bool specialNameOnly) {
            if (specialNameOnly && (attributes & MethodAttributes.SpecialName) == 0) {
                return false;
            }
            
            if (Context.DomainManager.Configuration.PrivateBinding) {
                return true;
            }

            switch (attributes & MethodAttributes.MemberAccessMask) {
                case MethodAttributes.Private:
                case MethodAttributes.Assembly:
                case MethodAttributes.FamANDAssem:
                    return false;

                case MethodAttributes.Family:
                case MethodAttributes.FamORAssem:
                    return declaringType != null && declaringType.IsVisible && !declaringType.IsSealed;
            }

            return true;
        }

        private bool IsVisible(FieldInfo/*!*/ field) {
            if (Context.DomainManager.Configuration.PrivateBinding) {
                return true;
            }

            if (field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly) {
                return false;
            }

            if (field.IsProtected()) {
                return field.DeclaringType != null && field.DeclaringType.IsVisible && !field.DeclaringType.IsSealed;
            }

            return true;
        }

        private bool IsWriteable(FieldInfo/*!*/ field) {
            return !field.IsInitOnly && !field.IsLiteral;
        }

        private int GetVisibleMethodCount(IEnumerable<OverloadInfo/*!*/>/*!*/ members, bool specialNameOnly) {
            int count = 0;
            foreach (OverloadInfo method in members) {
                if (IsVisible(method.Attributes, method.DeclaringType, specialNameOnly)) {
                    count++;
                }
            }
            return count;
        }

        private RubyOverloadGroupInfo/*!*/ MakeGroup(ICollection<ClrOverloadInfo>/*!*/ allMethods) {
            var overloads = new OverloadInfo[allMethods.Count];
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

        private RubyMethodGroupInfo/*!*/ MakeGroup(IEnumerable<OverloadInfo/*!*/>/*!*/ members, int visibleMemberCount, bool specialNameOnly, bool isDetached) {
            var allMethods = new OverloadInfo[visibleMemberCount];
            int i = 0;
            foreach (var method in members) {
                if (IsVisible(method.Attributes, method.DeclaringType, specialNameOnly)) {
                    allMethods[i++] = method;
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
            if (fieldInfo != null && IsVisible(fieldInfo) && (!isWrite || IsWriteable(fieldInfo))) {
                // creates detached info if only declared members are requested (used by Kernel#clr_member):
                bool createDetached = (bindingFlags & BindingFlags.DeclaredOnly) != 0;
                method = new RubyFieldInfo(fieldInfo, RubyMemberFlags.Public, this, isWrite, createDetached);
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
                // creates detached info if only declared members are requested (used by Kernel#clr_member):
                bool createDetached = (bindingFlags & BindingFlags.DeclaredOnly) != 0;
                method = new RubyEventInfo((EventTracker)MemberTracker.FromMemberInfo(eventInfo), RubyMemberFlags.Public, this, createDetached);
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
            var argsBuilder = new ArgsBuilder(0, 0, 0, 0, false);
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
            OverloadInfo[] ctors;
            if (TypeTracker == null) {
                metaBuilder.SetError(Methods.MakeNotClrTypeError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
            } else if ((ctors = GetConstructors(TypeTracker.Type)).Length == 0) {
                metaBuilder.SetError(Methods.MakeConstructorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
            } else {
                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, ctors, SelfCallConvention.NoSelf, true);
            }
        }

        public void BuildObjectConstructionNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            if (IsSingletonClass) {
                metaBuilder.SetError(Methods.MakeVirtualClassInstantiatedError.OpCall());
                return;
            }

            Type type = GetUnderlyingSystemType();

            RubyMemberInfo initializer;
            using (Context.ClassHierarchyLocker()) {
                // check version of the class so that we invalidate the rule whenever the initializer changes:
                metaBuilder.AddVersionTest(this);

                initializer = ResolveMethodForSiteNoLock(Symbols.Initialize, VisibilityContext.AllVisible).Info;

                // Initializer resolves to BasicObject#initialize unless overridden in a derived class.
                // We ensure that initializer cannot be removed/undefined so that we don't ever fall back to method_missing (see RubyModule.RemoveMethodNoEvent).
                Debug.Assert(initializer != null);
            }

            bool isLibraryMethod = initializer is RubyLibraryMethodInfo;
            bool isRubyInitializer = initializer.IsRubyMember && !isLibraryMethod;
            bool isLibraryInitializer = isLibraryMethod && !initializer.DeclaringModule.IsObjectClass && !initializer.DeclaringModule.IsBasicObjectClass;

            if (isRubyInitializer || isLibraryInitializer && _isRubyClass) {
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
                OverloadInfo[] constructionOverloads;
                SelfCallConvention callConvention = SelfCallConvention.SelfIsParameter;
                bool implicitProtocolConversions = false;

                if (typeof(Delegate).IsAssignableFrom(type)) {
                    BuildDelegateConstructorCall(metaBuilder, args, type);
                    return;
                } else if (type.IsArray && type.GetArrayRank() == 1) {
                    constructionOverloads = GetClrVectorFactories();
                } else if (_structInfo != null) {
                    constructionOverloads = new OverloadInfo[] { new ReflectionOverloadInfo(Methods.CreateStructInstance) };
                } else if (_factories.Length != 0) {
                    constructionOverloads = ArrayUtils.ConvertAll(_factories, (d) => new ReflectionOverloadInfo(d.Method));
                } else {
                    // TODO: handle protected constructors
                    constructionOverloads = GetConstructors(type == typeof(object) ? typeof(RubyObject) : type);

                    if (type.IsValueType) {
                        if (constructionOverloads.Length == 0 || GetConstructor(type) == null) {
                            constructionOverloads = ArrayUtils.Append(constructionOverloads, new ReflectionOverloadInfo(Methods.CreateDefaultInstance));
                        }
                    } else if (constructionOverloads.Length == 0) {
                        metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
                        return;
                    }

                    callConvention = SelfCallConvention.NoSelf;
                    implicitProtocolConversions = true;
                }

                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, constructionOverloads, callConvention, implicitProtocolConversions);

                if (!metaBuilder.Error) {
                    metaBuilder.Result = MarkNewException(metaBuilder.Result);

                    // we need to handle break, which unwinds to a proc-converter that could be this method's frame:
                    if (args.Signature.HasBlock) {
                        metaBuilder.ControlFlowBuilder = RubyMethodGroupInfo.RuleControlFlowBuilder;
                    }
                }
            }
        }

        private OverloadInfo[]/*!*/ GetConstructors(Type/*!*/ type) {
            return ReflectionOverloadInfo.CreateArray(type.GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | (Context.DomainManager.Configuration.PrivateBinding ? BindingFlags.NonPublic : 0)
            ));
        }

        private ConstructorInfo GetConstructor(Type/*!*/ type, params Type[]/*!*/ parameterTypes) {
            return type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | (Context.DomainManager.Configuration.PrivateBinding ? BindingFlags.NonPublic : 0),
                null,
                parameterTypes,
                null
            );
        }

        private Expression/*!*/ MarkNewException(Expression/*!*/ expression) {
            // mark the exception as "Ruby created" so that "new" is not called again on its class when handled in rescue clause:
            return IsException() ? Methods.MarkException.OpCall(AstUtils.Convert(expression, typeof(Exception))) : expression;
        }

        private static void BuildOverriddenInitializerCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, RubyMemberInfo/*!*/ initializer) {
            var instanceExpr = metaBuilder.Result;
            metaBuilder.Result = null;

            var instanceVariable = metaBuilder.GetTemporary(instanceExpr.Type, "#instance");

            // We know an exact type of the new instance and that there is no singleton for that instance.
            // We also have the exact method we need to call ("initialize" is a RubyMethodInfo/RubyLambdaMethodInfo).
            // => no tests are necessary:
            args.SetTarget(instanceVariable, null);

            if (initializer is RubyMethodInfo || initializer is RubyLambdaMethodInfo) {
                initializer.BuildCallNoFlow(metaBuilder, args, Symbols.Initialize);
            } else {
                // TODO: we need more refactoring of RubyMethodGroupInfo.BuildCall to be able to inline this:
                metaBuilder.Result = AstUtils.LightDynamic(
                    RubyCallAction.Make(args.RubyContext, "initialize",
                        new RubyCallSignature(
                            args.Signature.ArgumentCount, 
                            (args.Signature.Flags & ~RubyCallFlags.IsInteropCall) | RubyCallFlags.HasImplicitSelf
                        )
                    ),
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
                if (args.Signature.HasBlock) {
                    metaBuilder.ControlFlowBuilder = RubyMethodInfo.RuleControlFlowBuilder;
                }
            }
        }

        public bool BuildAllocatorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Func<Expression>/*!*/ defaultExceptionMessage) {
            var newExpression = GetAllocatorNewExpression(args, defaultExceptionMessage);
            if (newExpression != null) {
                metaBuilder.Result = MarkNewException(newExpression);
                return true;
            } else {
                return false;
            }
        }

        private Expression GetAllocatorNewExpression(CallArguments/*!*/ args, Func<Expression>/*!*/ defaultExceptionMessage) {
            Type type = GetUnderlyingSystemType();

            if (type == typeof(object)) {
                type = typeof(RubyObject);
            }

            if (_structInfo != null) {
                return Methods.AllocateStructInstance.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
            }

            ConstructorInfo ctor;
            if (IsException()) {
                if ((ctor = GetConstructor(type, typeof(string))) != null) {
                    return Ast.New(ctor, defaultExceptionMessage());
                } else if ((ctor = GetConstructor(type, typeof(string), typeof(Exception))) != null) {
                    return Ast.New(ctor, defaultExceptionMessage(), AstUtils.Constant(null));
                }
            }

            if ((ctor = GetConstructor(type, typeof(RubyClass))) != null) {
                return Ast.New(ctor, AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
            }

            if ((ctor = GetConstructor(type, typeof(RubyContext))) != null) {
                return Ast.New(ctor, AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)));
            }

            if ((ctor = GetConstructor(type)) != null) {
                return Ast.New(ctor);
            }

            if (type.IsValueType && type != typeof(int) && type != typeof(double)) {
                return Ast.New(type);
            }

            return null;
        }

        private void BuildDelegateConstructorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Type/*!*/ type) {
            if (args.Signature.HasBlock) {
                RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
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

        private OverloadInfo/*!*/[]/*!*/ GetClrVectorFactories() {
            if (_clrVectorFactories == null) {
                Type elementType = GetUnderlyingSystemType().GetElementType();
                _clrVectorFactories = new[] { 
                    Methods.CreateVector.MakeGenericMethod(elementType), 
                    Methods.CreateVectorWithValues.MakeGenericMethod(elementType) 
                };
            }
            return ReflectionOverloadInfo.CreateArray(_clrVectorFactories);
        }

        // thread-safe (the latest write wins):
        private MethodBase[] _clrVectorFactories;

        #endregion
    }
}
