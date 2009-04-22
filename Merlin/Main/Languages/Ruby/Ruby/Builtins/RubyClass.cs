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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Dynamic;
using System.Threading;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Ast = System.Linq.Expressions.Expression;
using AstFactory = IronRuby.Compiler.Ast.AstFactory;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using System.Runtime.CompilerServices;

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

        // TODO: internal setter
        [Emitted]
        public StrongBox<int> Version;

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

#if FALSE
        private Type _extensionType;

        /// <summary>
        /// Holds on to the type with extension members, if any
        /// Used by the DLR action binder & binder helpers.
        /// See RubyActionBinder.GetExtensionTypes
        /// </summary>
        public Type ExtensionType {
            get { return _extensionType; }
            internal set { _extensionType = value; }
        }
#endif

        public override Type/*!*/ GetUnderlyingSystemType() {
            if (_isSingletonClass) {
                throw new InvalidOperationException("Singleton class doesn't have underlying system type.");
            }

            if (_underlyingSystemType == null) {
                Interlocked.Exchange(ref _underlyingSystemType, RubyTypeDispenser.GetOrCreateType(_superClass.GetUnderlyingSystemType(), GetClrInterfaces()));
            }

            Debug.Assert(_underlyingSystemType != null);
            return _underlyingSystemType;
        }

        // default allocator:
        public RubyClass(RubyClass/*!*/ rubyClass)
            : this(rubyClass.Context, null, null, null, null, null, null, rubyClass.Context.ObjectClass, null, null, null, true, false) {
            
            // all modules need a singleton (see RubyContext.CreateModule):
            InitializeDummySingletonClass(rubyClass, null);
        }
        
        // friend: RubyContext
        // tracker: non-null => show members declared on the tracker
        internal RubyClass(RubyContext/*!*/ context, string name, Type type, object singletonClassOf,
            Action<RubyModule> methodsInitializer, Action<RubyModule> constantsInitializer, Delegate/*!*/[] factories, RubyClass superClass, 
            RubyModule/*!*/[] expandedMixins, TypeTracker tracker, RubyStruct.Info structInfo, 
            bool isRubyClass, bool isSingletonClass)
            : base(context, name, methodsInitializer, constantsInitializer, expandedMixins, null, tracker) {

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
            Version = new StrongBox<int>(Interlocked.Increment(ref _globalVersion));
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
                RubyMethodGroupInfo overriddenGroup = overriddenMethod as RubyMethodGroupInfo;
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
                // We can do so because such methods were nto used in any cache.
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
                var group = method as RubyMethodGroupInfo;
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
                _superClass ?? Context.ObjectClass, null, null, _structInfo, isRubyClass, IsSingletonClass
            );

            if (!IsSingletonClass) {
                // singleton members are copied here, not in InitializeCopy:
                result.SingletonClass.InitializeMembersFrom(SingletonClass);

                // copy instance variables, and frozen, taint flags:
                Context.CopyInstanceData(this, result, false, true, false);
            }
            
            // members initialized in InitializeClassFrom (invoked by "initialize_copy")
            return result;
        }

        // TODO: public due to partial trust
        // implements Class#new
        public static object CreateAnonymousClass(RubyScope/*!*/ scope, BlockParam body, RubyClass/*!*/ self, [Optional]RubyClass superClass) {
            RubyContext context = scope.RubyContext;
            RubyModule owner = scope.GetInnerMostModule();
            
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

        public RubyClass/*!*/ GetNonSingletonClass() {
            RubyClass result = this;
            while (result != null && result.IsSingletonClass) {
                result = result._superClass;
            }
            return result;
        }

        public override MethodResolutionResult ResolveMethodFallbackToObjectNoLock(string/*!*/ name, RubyClass visibilityContext) {
            // Note: all classes include Object in ancestors, so we don't need to search there.
            return ResolveMethodNoLock(name, visibilityContext);
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
        private static Dictionary<MemberLookupCacheEntry, bool> _clrFailedMemberLookupCache
            = new Dictionary<MemberLookupCacheEntry, bool>();

        private struct MemberLookupCacheEntry : IEquatable<MemberLookupCacheEntry> {
            private readonly Type/*!*/ _type;
            private readonly string/*!*/ _methodName;

            public MemberLookupCacheEntry(Type/*!*/ type, string/*!*/ methodName) {
                _type = type;
                _methodName = methodName;
            }

            public override int GetHashCode() {
                return _type.GetHashCode() ^ _methodName.GetHashCode();
            }

            public bool Equals(MemberLookupCacheEntry other) {
                return _type == other._type && _methodName == other._methodName;
            }
        }

        private static bool IsFailureCached(Type/*!*/ type, string/*!*/ methodName) {
            // check for cached lookup failure (if the cache is available):
            bool result = false;
            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                result = cache.ContainsKey(new MemberLookupCacheEntry(type, methodName));
                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }

#if DEBUG
            PerfTrack.NoteEvent(PerfTrack.Categories.Count, "CLR member lookup failure cache " + (result ? "hit" : "miss"));
#endif
            return result;
        }

        private static void CacheFailure(Type/*!*/ type, string/*!*/ methodName) {
            // store failure to the cache if the cache is not owned by another thread:
            var cache = Interlocked.Exchange(ref _clrFailedMemberLookupCache, null);
            if (cache != null) {
                cache[new MemberLookupCacheEntry(type, methodName)] = true;
                Interlocked.Exchange(ref _clrFailedMemberLookupCache, cache);
            }
        }

        protected override bool TryGetClrMember(Type/*!*/ type, string/*!*/ name, out RubyMemberInfo method) {
            string unmangled;

            if (IsFailureCached(type, name)) {
                method = null;
                return false;
            }

            // We look only for members directly declared on the type and handle method overloads inheritance manually.  
            BindingFlags basicBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            BindingFlags bindingFlags = basicBindingFlags | ((_isSingletonClass) ? BindingFlags.Static : BindingFlags.Instance);

            string operatorName;
            if (!_isSingletonClass && (operatorName = MapOperator(name)) != null) {
                // instance invocation of an operator:
                if (TryGetClrMethod(type, basicBindingFlags | BindingFlags.Static, true, name, operatorName, out method)) {
                    return true;
                }
            } else if (name == "[]" || name == "[]=") {
                object[] attrs = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if (attrs.Length == 1) {
                    // default indexer accessor:
                    bool isSetter = name.Length == 3;
                    if (TryGetClrProperty(type, bindingFlags, name, ((DefaultMemberAttribute)attrs[0]).MemberName, isSetter, out method)) {
                        return true;
                    }
                }
            } else if (name.LastCharacter() == '=') {
                string propertyName = name.Substring(0, name.Length - 1);
                
                // property setter:
                if (TryGetClrProperty(type, bindingFlags, name, propertyName, true, out method)) return true;
                unmangled = RubyUtils.TryUnmangleName(propertyName);
                if (unmangled != null && TryGetClrProperty(type, bindingFlags, name, unmangled, true, out method)) return true;

                // writeable field:
                if (TryGetClrField(type, bindingFlags, propertyName, true, out method)) return true;
                if (unmangled != null && TryGetClrField(type, bindingFlags, unmangled, true, out method)) return true;
            } else {
                // method:
                if (TryGetClrMethod(type, bindingFlags, false, name, name, out method)) return true;
                unmangled = RubyUtils.TryUnmangleName(name);
                if (unmangled != null && TryGetClrMethod(type, bindingFlags, false, name, unmangled, out method)) return true;

                // getter:
                if (TryGetClrProperty(type, bindingFlags, name, name, false, out method)) return true;
                if (unmangled != null && TryGetClrProperty(type, bindingFlags, name, unmangled, false, out method)) return true;

                // event:
                if (TryGetClrEvent(type, bindingFlags, name, out method)) return true;
                if (unmangled != null && TryGetClrEvent(type, bindingFlags, unmangled, out method)) return true;

                // field:
                if (TryGetClrField(type, bindingFlags, name, false, out method)) return true;
                if (unmangled != null && TryGetClrField(type, bindingFlags, unmangled, false, out method)) return true;
            }

            CacheFailure(type, name);

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
            public RubyMethodGroupInfo Owner { get; set; }
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
        private bool TryGetClrMethod(Type/*!*/ type, BindingFlags bindingFlags, bool specialNameOnly, string/*!*/ name, string/*!*/ clrName, 
            out RubyMemberInfo method) {

            // declared only:
            MemberInfo[] initialMembers = GetDeclaredClrMethods(type, bindingFlags, clrName);
            int initialVisibleMemberCount = GetVisibleMethodCount(initialMembers, specialNameOnly);
            if (initialVisibleMemberCount == 0) {
                // case [1]
                method = null;
                return false;
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
                    // Skip interfaces, their methods are not callable => do not include them into a method group.
                    // Skip all classes once hidden sentinel is encountered (no CLR overloads are visible since then).
                    if (!skipHidden && module.Tracker != null && module.IsClass) {
                        ancestors.Add((RubyClass)module);
                    }
                }

                // continue:
                return false;
            });

            Dictionary<ValueArray<Type>, ClrOverloadInfo> allMethods = null;
            if (inheritedRubyMember != null) {
                // case [2.2.2]: add CLR methods from the Ruby member:
                var inheritedGroup = inheritedRubyMember as RubyMethodGroupInfo;
                if (inheritedGroup != null) {
                    AddMethodsOverwriteExisting(ref allMethods, inheritedGroup.MethodBases, inheritedGroup.OverloadOwners, specialNameOnly);
                }
            }

            // populate classes in (type..Kernel] or (type..C) with method groups:
            for (int i = ancestors.Count - 1; i >= 0; i--) {
                var declared = GetDeclaredClrMethods(ancestors[i].Tracker.Type, bindingFlags, clrName);
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
                    ancestors[i].AddMethodNoCacheInvalidation(name, ancestors[i].MakeGroup(allMethods));
                }
            }

            if (allMethods != null) {
                // add members declared in self:
                AddMethodsOverwriteExisting(ref allMethods, initialMembers, null, specialNameOnly);

                // return the group, it will be stored in the method table by the caller:
                method = MakeGroup(allMethods);
            } else {
                method = MakeGroup(initialMembers, initialVisibleMemberCount, specialNameOnly);
            }

            return true;
        }

        private MemberInfo[]/*!*/ GetDeclaredClrMethods(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name) {
            // GetMember uses prefix matching if the name ends with '*', add another * to match the original name:
            if (name.LastCharacter() == '*') {
                name += "*";
            }
            return type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
        }

        // Returns the number of methods newly added to the dictionary.
        private static bool AddMethodsOverwriteExisting(ref Dictionary<ValueArray<Type>, ClrOverloadInfo> methods,
            MemberInfo/*!*/[]/*!*/ newOverloads, RubyMethodGroupInfo/*!*/[] overloadOwners, bool specialNameOnly) {

            bool anyChange = false;
            for (int i = 0; i < newOverloads.Length; i++) {
                var method = (MethodBase)newOverloads[i];
                if (IsVisible(method, specialNameOnly)) {
                    var paramTypes = new ValueArray<Type>(ReflectionUtils.GetParameterTypes(method.GetParameters()));
                    if (methods == null) {
                        methods = new Dictionary<ValueArray<Type>, ClrOverloadInfo>();
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

        private static bool IsVisible(MethodBase/*!*/ method, bool specialNameOnly) {
            return !method.IsPrivate && (method.IsSpecialName || !specialNameOnly);
        }

        private static int GetVisibleMethodCount(MemberInfo[]/*!*/ members, bool specialNameOnly) {
            int count = 0;
            foreach (MethodBase method in members) {
                if (IsVisible(method, specialNameOnly)) {
                    count++;
                }
            }
            return count;
        }

        private RubyMethodGroupInfo/*!*/ MakeGroup(Dictionary<ValueArray<Type>, ClrOverloadInfo>/*!*/ allMethods) {
            var overloads = new MethodBase[allMethods.Count];
            var overloadOwners = new RubyMethodGroupInfo[overloads.Length];
            int i = 0;
            foreach (var entry in allMethods.Values) {
                overloads[i] = entry.Overload;
                overloadOwners[i] = entry.Owner;
                i++;
            }

            var result = new RubyMethodGroupInfo(overloads, this, overloadOwners, _isSingletonClass);
            
            // update ownership of overloads owned by the new group:
            foreach (var entry in allMethods.Values) {
                if (entry.Owner != null) {
                    entry.Owner.CachedInGroup(result);
                } else {
                    entry.Owner = result;
                }
            }

            return result;
        }

        private RubyMethodGroupInfo/*!*/ MakeGroup(MemberInfo[]/*!*/ members, int visibleMemberCount, bool specialNameOnly) {
            var allMethods = new MethodBase[visibleMemberCount];
            for (int i = 0, j = 0; i < members.Length; i++) {
                var method = (MethodBase)members[i];
                if (IsVisible(method, specialNameOnly)) {
                    allMethods[j++] = method;
                }
            }

            return new RubyMethodGroupInfo(allMethods, this, null, _isSingletonClass);
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

        private bool TryGetClrProperty(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, string/*!*/ clrName, bool isWrite, out RubyMemberInfo method) {
            return TryGetClrMethod(type, bindingFlags, true, name, (isWrite ? "set_" : "get_") + clrName, out method);
        }

        private bool TryGetClrField(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, bool isWrite, out RubyMemberInfo method) {
            Assert.NotNull(type, name);

            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            if (fieldInfo != null && !fieldInfo.IsPrivate && (!isWrite || !fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)) {
                method = new RubyFieldInfo(fieldInfo, RubyMemberFlags.Public, this, isWrite);
                return true;
            }

            method = null;
            return false;
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
                BuildAllocatorCall(metaBuilder, args, () => AstUtils.Constant(Name));
            }
        }
        
        /// <summary>
        /// Implements Class#new feature.
        /// </summary>
        public void BuildObjectConstruction(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            BuildObjectConstructionNoFlow(metaBuilder, args, methodName);
            metaBuilder.BuildControlFlow(args);
        }

        public void BuildObjectConstructionNoFlow(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, string/*!*/ methodName) {
            Debug.Assert(!IsSingletonClass, "Cannot instantiate singletons");

            Type type = GetUnderlyingSystemType();

            RubyMemberInfo initializer;
            using (Context.ClassHierarchyLocker()) {
                metaBuilder.AddVersionTest(this);

                initializer = ResolveMethodForSiteNoLock(Symbols.Initialize, IgnoreVisibility).Info;

                // Initializer resolves to Object#initializer unless overridden in a derived class.
                // We ensure that this method cannot be removed.
                Debug.Assert(initializer != null);
            }

            // Ruby libraries: should initialize fully via factories/constructors.
            // C# "initialize" methods: ignored - we don't consider them initializers.
            RubyMethodInfo overriddenInitializer = initializer as RubyMethodInfo;

            // Initializer is overridden => initializer is invoked on an uninitialized instance.
            // Is user class (defined in Ruby code) => construct it as if it had initializer that calls super immediately
            // (we need to "inherit" factories/constructors from the base class (e.g. class S < String; self; end.new('foo')).
            if (overriddenInitializer != null || (_isRubyClass && _structInfo == null)) {
                BuildAllocatorCall(metaBuilder, args, () => AstUtils.Constant(Name));

                if (!metaBuilder.Error) {
                    if (overriddenInitializer != null || (_isRubyClass && initializer != null && !initializer.IsEmpty)) {
                        BuildOverriddenInitializerCall(metaBuilder, args, initializer);
                    }
                }
            } else if (typeof(Delegate).IsAssignableFrom(type)) {
                BuildDelegateConstructorCall(metaBuilder, args, type);
            } else {
                MethodBase[] constructionOverloads;
                SelfCallConvention callConvention;

                if (_structInfo != null) {
                    constructionOverloads = new MethodBase[] { Methods.CreateStructInstance };
                    callConvention = SelfCallConvention.SelfIsParameter;
                } else if (_factories.Length != 0) {
                    constructionOverloads = (MethodBase[])ReflectionUtils.GetMethodInfos(_factories);
                    callConvention = SelfCallConvention.SelfIsParameter;
                } else if (type.IsArray && type.GetArrayRank() == 1) {
                    constructionOverloads = ClrVectorFactories;
                    callConvention = SelfCallConvention.SelfIsParameter;
                } else {
                    // TODO: handle protected constructors
                    constructionOverloads = type.GetConstructors();
                    if (constructionOverloads.Length == 0) {
                        metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
                        return;
                    }
                    callConvention = SelfCallConvention.NoSelf;
                }

                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, constructionOverloads, callConvention);

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

        public void BuildAllocatorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Func<Expression>/*!*/ defaultExceptionMessage) {
            Type type = GetUnderlyingSystemType();

            if (_structInfo != null) {
                metaBuilder.Result = Methods.AllocateStructInstance.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
                return;
            }

            if (type.IsSubclassOf(typeof(Delegate))) {
                BuildDelegateConstructorCall(metaBuilder, args, type);
                return;
            }

            ConstructorInfo ctor;
            if (IsException()) {
                if ((ctor = type.GetConstructor(new[] { typeof(string) })) != null) {
                    metaBuilder.Result = Ast.New(ctor, defaultExceptionMessage());
                    return;
                } else if ((ctor = type.GetConstructor(new[] { typeof(string), typeof(Exception) })) != null) {
                    metaBuilder.Result = Ast.New(ctor, defaultExceptionMessage(), AstUtils.Constant(null));
                    return;
                }
            }

            if ((ctor = type.GetConstructor(new[] { typeof(RubyClass) })) != null) {
                metaBuilder.Result = Ast.New(ctor, AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
                return;
            }

            if ((ctor = type.GetConstructor(new[] { typeof(RubyContext) })) != null) {
                metaBuilder.Result = Ast.New(ctor, AstUtils.Convert(args.MetaContext.Expression, typeof(RubyContext)));
                return;
            }

            if ((ctor = type.GetConstructor(Type.EmptyTypes)) != null) {
                metaBuilder.Result = Ast.New(ctor);
                return;
            }

            metaBuilder.SetError(Methods.MakeAllocatorUndefinedError.OpCall(Ast.Convert(args.TargetExpression, typeof(RubyClass))));
        }

        private void BuildDelegateConstructorCall(MetaObjectBuilder/*!*/ metaBuilder, CallArguments/*!*/ args, Type/*!*/ type) {
            if (args.Signature.HasBlock) {
                if (args.ExplicitArgumentCount == 2) {
                    metaBuilder.Result = Methods.CreateDelegateFromProc.OpCall(
                        AstUtils.Constant(type),
                        AstUtils.Convert(args.GetBlockExpression(), typeof(Proc))
                    );
                } else {
                    metaBuilder.SetError(Methods.MakeWrongNumberOfArgumentsError.OpCall(
                        AstUtils.Constant(args.ExplicitArgumentCount - 1), AstUtils.Constant(0)
                    ));
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
