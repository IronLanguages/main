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

namespace IronRuby.Builtins {

    public sealed partial class RubyClass : RubyModule, IDuplicable {
        public const string/*!*/ ClassSingletonName = "__ClassSingleton";
        public const string/*!*/ ClassSingletonSingletonName = "__ClassSingletonSingleton";
        public const string/*!*/ MainSingletonName = "__MainSingleton";

        private readonly RubyClass _superClass;

        // is this class a singleton class?
        private readonly bool _isSingletonClass;

        // an object that the class is a singleton of (reverse _singletonClass pointer):
        private readonly object _singletonClassOf;
        
        // null for singletons:
        private Type _underlyingSystemType;
        private readonly bool _isRubyClass;
        private RubyGlobalScope _globalScope;

        // if this class is a struct represents its layout:
        private RubyStruct.Info _structInfo;

        // whether initialize_copy can be called (the class has just been duplicated):
        private bool _isUninitializedCopy;

        private Delegate[] _factories;
        
        public RubyClass SuperClass {
            get { return _superClass; } 
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
            set { _structInfo = value; }
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

        public Type/*!*/ GetUnderlyingSystemType() {
            if (_isSingletonClass) {
                throw new InvalidOperationException("Singleton class doesn't have underlying system type.");
            }

            if (_underlyingSystemType == null) {
                Interlocked.Exchange(ref _underlyingSystemType, RubyTypeDispenser.GetOrCreateType(_superClass.GetUnderlyingSystemType(), GetClrInterfaces()));
            }

            Debug.Assert(_underlyingSystemType != null);
            return _underlyingSystemType;
        }

        private PropertyInfo/*!*/ UnderlyingSystemTypeProperty {
            get { return typeof(RubyClass).GetProperty("UnderlyingSystemType"); }
        }

        public Delegate[] Factories {
            get { return _factories; }
            set { _factories = value; }
        }

        // default allocator:
        public RubyClass(RubyClass/*!*/ rubyClass)
            : this(rubyClass.Context, null, null, null, null, rubyClass.Context.ObjectClass, null, true, false) {
            
            // all modules need a singleton (see RubyContext.CreateModule):
            rubyClass.Context.CreateDummySingletonClassFor(this, rubyClass, null);
        }
        
        // friend: RubyContext
        // tracker: non-null => show members declared on the tracker
        internal RubyClass(RubyContext/*!*/ context, string name, Type type, object singletonClassOf, Action<RubyModule> initializer,
            RubyClass superClass, TypeTracker tracker, bool isRubyClass, bool isSingletonClass)
            : base(context, name, initializer, null, tracker) {

            Debug.Assert((superClass == null) == (type == typeof(object)), "All classes have a superclass, except for Object");
            Debug.Assert(!isRubyClass || tracker == null, "Ruby class cannot have a tracker");
            Debug.Assert(singletonClassOf != null || !isSingletonClass, "Singleton classes don't have a type");
            Debug.Assert(superClass != this);

            _underlyingSystemType = type;
            _superClass = superClass;
            _isSingletonClass = isSingletonClass;
            _isRubyClass = isRubyClass;
            _singletonClassOf = singletonClassOf;
            
            if (superClass != null) {
                superClass.AddDependentModule(this);
                _structInfo = superClass._structInfo; 
            }
        }

        internal override void EnsureInitialized() {
            base.EnsureInitialized();
            if (_superClass != null) {
                _superClass.EnsureInitialized();
            }
        }

        object IDuplicable.Duplicate(RubyContext/*!*/ context, bool copySingletonMembers) {
            if (IsSingletonClass) {
                throw RubyExceptions.CreateTypeError("can't copy singleton class");
            }

            RubyClass result = Duplicate(null);

            result._isUninitializedCopy = true;
            return result; 
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
        /// </summary>
        internal RubyClass/*!*/ Duplicate(object singletonClassOf) {
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

            RubyClass result = Context.CreateClass(Name, type, singletonClassOf, null, null,
                _superClass ?? Context.ObjectClass, null, isRubyClass, IsSingletonClass
            );

            result._structInfo = _structInfo; 

            // copy factories so that the class can be instantiated:
            result._factories = (_factories != null) ? ArrayUtils.Copy(_factories) : _factories;

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
            RubyContext ec = scope.RubyContext;
            RubyModule owner = scope.GetInnerMostModule();
            
            // MRI is inconsistent here, it triggers "inherited" event after the body of the method is evaluated.
            // In all other cases the order is event first, body next.
            RubyClass newClass = ec.DefineClass(owner, null, superClass ?? ec.ObjectClass);
            return (body != null) ? RubyUtils.EvaluateInModule(newClass, body, newClass) : newClass;
        }

        public override string/*!*/ ToString() {
            return Name;
        }

        protected override bool ForEachAncestor(Func<RubyModule, bool>/*!*/ action) {
            // walk up the class hierarchy:
            for (RubyClass c = this; c != null; c = c._superClass) {
                if (c.ForEachDeclaredAncestor(action)) return true;
            }
            return false;
        }

        internal override bool IsSuperClassAncestor(RubyModule/*!*/ module) {
            return _superClass != null && _superClass.HasAncestor(module);
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

        public override RubyMemberInfo ResolveMethodFallbackToObject(string/*!*/ name, bool includeMethod) {
            // Note: all classes include Object in ancestors, so we don't need to search there.
            return ResolveMethod(name, includeMethod);
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

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            bindingFlags |= (_isSingletonClass) ? BindingFlags.Static : BindingFlags.Instance;

            if (name.EndsWith("=")) {
                string propertyName = name.Substring(0, name.Length - 1);
                
                // property setter:
                if (TryGetClrMethod(type, bindingFlags, "set_" + propertyName, out method)) return true;
                unmangled = RubyUtils.TryUnmangleName(propertyName);
                if (unmangled != null && TryGetClrMethod(type, bindingFlags, "set_" + unmangled, out method)) return true;

                // writeable field:
                if (TryGetClrField(type, bindingFlags, propertyName, true, out method)) return true;
                if (unmangled != null && TryGetClrField(type, bindingFlags, unmangled, true, out method)) return true;
            } else {
                // method:
                if (TryGetClrMethod(type, bindingFlags, name, out method)) return true;
                unmangled = RubyUtils.TryUnmangleName(name);
                if (unmangled != null && TryGetClrMethod(type, bindingFlags, unmangled, out method)) return true;

                // getter:
                if (TryGetClrMethod(type, bindingFlags, "get_" + name, out method)) return true;
                if (unmangled != null && TryGetClrMethod(type, bindingFlags, "get_" + unmangled, out method)) return true;

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

        private bool TryGetClrMethod(Type/*!*/ type, BindingFlags bindingFlags, string/*!*/ name, out RubyMemberInfo method) {
            Assert.NotNull(type, name);

            MemberInfo[] members = type.GetMember(name, MemberTypes.Method, bindingFlags | BindingFlags.InvokeMethod);
            if (members.Length > 0) {
                method = new RubyMethodGroupInfo(SelectNonPrivateMethods(members), this, _isSingletonClass);
                return true;
            } else {
                method = null;
                return false;
            }
        }

        private IList<MethodBase>/*!*/ SelectNonPrivateMethods(MemberInfo[]/*!*/ members) {
            var result = new List<MethodBase>(members.Length);
            for (int i = 0; i < members.Length; i++) {
                var method = (MethodBase)members[i];
                if (!method.IsPrivate) {
                    result.Add(method);
                }
            }
            return result;
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
                method = new RubyEventInfo(eventInfo, RubyMemberFlags.Public, this);
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
                metaBuilder.Result = MakeAllocatorCall(args, () => Ast.Constant(Name));
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
            
            RubyMemberInfo initializer = ResolveMethod(Symbols.Initialize, true).InvalidateSitesOnOverride();
            RubyMethodInfo overriddenInitializer = initializer as RubyMethodInfo;

            // check the version of this class to ensure the initializer won't be changed without rebuilding the site:
            metaBuilder.AddCondition(
                Ast.Equal(Ast.Property(Ast.Constant(this), RubyModule.VersionProperty), Ast.Constant(this.Version))
            );

            // Initializer is overridden => initializer is invoked on an uninitialized instance.
            // Is user class (defined in Ruby code) => construct it as if it had initializer that calls super immediately
            // (we need to "inherit" factories/constructors from the base class (e.g. class S < String; self; end.new('foo')).
            if (overriddenInitializer != null || (_isRubyClass && _structInfo == null)) {
                metaBuilder.Result = MakeAllocatorCall(args, () => Ast.Constant(Name));

                if (overriddenInitializer != null || (_isRubyClass && initializer != null && !initializer.IsEmpty)) {
                    BuildOverriddenInitializerCall(metaBuilder, args, initializer);
                }
            } else if (type.IsSubclassOf(typeof(Delegate))) {
                metaBuilder.Result = MakeDelegateConstructorCall(type, args);
            } else {
                MethodBase[] constructionOverloads;

                bool includeSelf;
                if (_structInfo != null) {
                    constructionOverloads = new MethodBase[] { Methods.CreateStructInstance };
                    includeSelf = true;
                } else if (_factories != null) {
                    constructionOverloads = (MethodBase[])ReflectionUtils.GetMethodInfos(_factories);
                    includeSelf = true;
                } else {
                    constructionOverloads = type.GetConstructors();
                    if (constructionOverloads.Length == 0) {
                        throw RubyExceptions.CreateTypeError(String.Format("allocator undefined for {0}", Name));
                    }
                    includeSelf = false;
                }

                RubyMethodGroupInfo.BuildCallNoFlow(metaBuilder, args, methodName, constructionOverloads, includeSelf, false);

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
                    RubyCallAction.Make("initialize", 
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

        public Expression/*!*/ MakeAllocatorCall(CallArguments/*!*/ args, Func<Expression>/*!*/ defaultExceptionMessage) {
            Type type = GetUnderlyingSystemType();

            if (_structInfo != null) {
                return Methods.AllocateStructInstance.OpCall(AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
            }

            if (type.IsSubclassOf(typeof(Delegate))) {
                return MakeDelegateConstructorCall(type, args);
            }

            ConstructorInfo ctor;
            if (IsException()) {
                if ((ctor = type.GetConstructor(new[] { typeof(string) })) != null) {
                    return Ast.New(ctor, defaultExceptionMessage());
                } else if ((ctor = type.GetConstructor(new[] { typeof(string), typeof(Exception) })) != null) {
                    return Ast.New(ctor, defaultExceptionMessage(), Ast.Constant(null));
                }
            }

            if ((ctor = type.GetConstructor(new[] { typeof(RubyClass) })) != null) {
                return Ast.New(ctor, AstUtils.Convert(args.TargetExpression, typeof(RubyClass)));
            }
            
            if ((ctor = type.GetConstructor(new[] { typeof(RubyContext) })) != null) {
                return Ast.New(ctor, args.ContextExpression);
            }
 
            if ((ctor = type.GetConstructor(Type.EmptyTypes)) != null) {
                return Ast.New(ctor);
            } 

            throw RubyExceptions.CreateTypeError(String.Format("allocator undefined for {0}", Name));
        }

        private Expression/*!*/ MakeDelegateConstructorCall(Type/*!*/ type, CallArguments/*!*/ args) {
            if (args.Signature.HasBlock) {
                return Methods.CreateDelegateFromProc.OpCall(
                    Ast.Constant(type),
                    AstUtils.Convert(args.GetBlockExpression(), typeof(Proc))
                );
            } else {
                // TODO:
                throw new NotImplementedError("no block given");
            }
        }

        #endregion
    }
}
