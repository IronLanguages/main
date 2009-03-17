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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Threading;

using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types {

    /// <summary>
    /// Represents a PythonType.  Instances of PythonType are created via PythonTypeBuilder.  
    /// </summary>
#if !SILVERLIGHT
    [DebuggerDisplay("PythonType: {Name}")]
#endif
    [PythonType("type")]
    [Documentation(@"type(object) -> gets the type of the object
type(name, bases, dict) -> creates a new type instance with the given name, base classes, and members from the dictionary")]
    public class PythonType : IMembersList, IDynamicMetaObjectProvider, IWeakReferenceable, ICodeFormattable, IFastGettable {
        private Type/*!*/ _underlyingSystemType;            // the underlying CLI system type for this type
        private string _name;                               // the name of the type
        private Dictionary<SymbolId, PythonTypeSlot> _dict; // type-level slots & attributes
        private PythonTypeAttributes _attrs;                // attributes of the type
        private int _version = GetNextVersion();            // version of the type
        private List<WeakReference> _subtypes;              // all of the subtypes of the PythonType
        private PythonContext _pythonContext;               // the context the type was created from, or null for system types.
        internal Dictionary<string, UserGetBase> _cachedGets; // cached gets on user defined type instances
        internal Dictionary<string, UserGetBase> _cachedTryGets; // cached try gets on used defined type instances
        internal Dictionary<SetMemberKey, FastSetBase> _cachedSets; // cached sets on user defined instances
        internal Dictionary<string, FastGetBase> _cachedTypeGets; // cached gets on types (system and user types)

        // commonly calculatable
        private List<PythonType> _resolutionOrder;          // the search order for methods in the type
        private PythonType/*!*/[]/*!*/ _bases;              // the base classes of the type
        private BuiltinFunction _ctor;                      // the built-in function which allocates an instance - a .NET ctor

        // fields that frequently remain null
        private WeakRefTracker _weakrefTracker;             // storage for Python style weak references
        private WeakReference _weakRef;                     // single weak ref instance used for all user PythonTypes.
        private string[] _slots;                            // the slots when the class was created
        private OldClass _oldClass;                         // the associated OldClass or null for new-style types  
        private int _originalSlotCount;                     // the number of slots when the type was created
        private InstanceCreator _instanceCtor;              // creates instances
        private CallSite<Func<CallSite, CodeContext, object, object>> _dirSite;
        private CallSite<Func<CallSite, CodeContext, object, string, object>> _getAttributeSite;
        private CallSite<Func<CallSite, CodeContext, object, object, string, object, object>> _setAttrSite;
        private CallSite<Func<CallSite, object, int>> _hashSite;
        private CallSite<Func<CallSite, CodeContext, object, object>> _lenSite;
        private CallSite<Func<CallSite, object, object, bool>> _eqSite;
        private CallSite<Func<CallSite, object, object, int>> _compareSite;
        private Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>> _tryGetMemSite;
        private Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>> _tryGetMemSiteShowCls;

        private PythonTypeSlot _lenSlot;                    // cached length slot, cleared when the type is mutated

        [MultiRuntimeAware]
        private static int MasterVersion = 1;
        private static readonly CommonDictionaryStorage _pythonTypes = new CommonDictionaryStorage();
        internal static PythonType _pythonTypeType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));
        private static readonly WeakReference[] _emptyWeakRef = new WeakReference[0];

        /// <summary>
        /// Creates a new type for a user defined type.  The name, base classes (a tuple of type
        /// objects), and a dictionary of members is provided.
        /// </summary>
        public PythonType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection dict) {
            InitializeUserType(context, name, bases, dict);
        }

        internal PythonType() {
        }

        /// <summary>
        /// Creates a new PythonType object which is backed by the specified .NET type for
        /// storage.  The type is considered a system type which can not be modified
        /// by the user.
        /// </summary>
        /// <param name="underlyingSystemType"></param>
        internal PythonType(Type underlyingSystemType) {
            _underlyingSystemType = underlyingSystemType;

            InitializeSystemType();
        }

        /// <summary>
        /// Creates a new PythonType which is a subclass of the specified PythonType.
        /// 
        /// Used for runtime defined new-style classes which require multiple inheritance.  The
        /// primary example of this is the exception system.
        /// </summary>
        internal PythonType(PythonType baseType, string name) {
            _underlyingSystemType = baseType.UnderlyingSystemType;

            IsSystemType = baseType.IsSystemType;
            IsPythonType = baseType.IsPythonType;
            Name = name;
            _bases = new PythonType[] { baseType };
            ResolutionOrder = Mro.Calculate(this, _bases);
            _attrs |= PythonTypeAttributes.HasDictionary;
        }

        /// <summary>
        /// Creates a new PythonType which is a subclass of the specified PythonType.
        /// 
        /// Used for runtime defined new-style classes which require multiple inheritance.  The
        /// primary example of this is the exception system.
        /// </summary>
        internal PythonType(PythonContext context, PythonType baseType, string name, string module, string doc)
            : this(baseType, name) {
            EnsureDict();

            _dict[Symbols.Doc] = new PythonTypeValueSlot(doc);
            _dict[Symbols.Module] = new PythonTypeValueSlot(module);
            IsSystemType = false;
            IsPythonType = false;
            _pythonContext = context;
            _attrs |= PythonTypeAttributes.HasDictionary;
        }

        /// <summary>
        /// Creates a new PythonType object which represents an Old-style class.
        /// </summary>
        internal PythonType(OldClass oc) {
            EnsureDict();

            _underlyingSystemType = typeof(OldInstance);
            Name = oc.__name__;
            OldClass = oc;

            List<PythonType> ocs = new List<PythonType>(oc.BaseClasses.Count);
            foreach (OldClass klass in oc.BaseClasses) {
                ocs.Add(klass.TypeObject);
            }

            List<PythonType> mro = new List<PythonType>();
            mro.Add(this);

            _bases = ocs.ToArray(); 
            _resolutionOrder = mro;
            AddSlot(Symbols.Class, new PythonTypeValueSlot(this));
        }

        internal BuiltinFunction Ctor {
            get {
                EnsureConstructor();

                return _ctor;
            }
        }

        #region Public API
        
        public static object __new__(CodeContext/*!*/ context, PythonType cls, string name, PythonTuple bases, IAttributesCollection dict) {
            if (name == null) {
                throw PythonOps.TypeError("type() argument 1 must be string, not None");
            }
            if (bases == null) {
                throw PythonOps.TypeError("type() argument 2 must be tuple, not None");
            }
            if (dict == null) {
                throw PythonOps.TypeError("TypeError: type() argument 3 must be dict, not None");
            }

            EnsureModule(context, dict);

            PythonType meta = FindMetaClass(cls, bases);

            if (meta != TypeCache.OldInstance && meta != TypeCache.PythonType) {
                if (meta != cls) {
                    // the user has a custom __new__ which picked the wrong meta class, call the correct metaclass
                    return PythonCalls.Call(context, meta, name, bases, dict);
                }

                // we have the right user __new__, call our ctor method which will do the actual
                // creation.                   
                return meta.CreateInstance(context, name, bases, dict);
            }

            // no custom user type for __new__
            return new PythonType(context, name, bases, dict);
        }

        internal static PythonType FindMetaClass(PythonType cls, PythonTuple bases) {
            PythonType meta = cls;
            foreach (object dt in bases) {
                PythonType metaCls = DynamicHelpers.GetPythonType(dt);

                if (metaCls == TypeCache.OldClass) continue;

                if (meta.IsSubclassOf(metaCls)) continue;

                if (metaCls.IsSubclassOf(meta)) {
                    meta = metaCls;
                    continue;
                }
                throw PythonOps.TypeError("metaclass conflict {0} and {1}", metaCls.Name, meta.Name);
            }
            return meta;
        }

        public static object __new__(CodeContext/*!*/ context, object cls, object o) {
            return DynamicHelpers.GetPythonType(o);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static PythonTuple Get__bases__(CodeContext/*!*/ context, PythonType/*!*/ type) {
            return type.GetBasesTuple();
        }

        private PythonTuple GetBasesTuple() {
            object[] res = new object[BaseTypes.Count];
            IList<PythonType> bases = BaseTypes;
            for (int i = 0; i < bases.Count; i++) {
                PythonType baseType = bases[i];

                if (baseType.IsOldClass) {
                    res[i] = baseType.OldClass;
                } else {
                    res[i] = baseType;
                }
            }

            return PythonTuple.MakeTuple(res);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static PythonType Get__base__(CodeContext/*!*/ context, PythonType/*!*/ type) {
            foreach (object typeObj in Get__bases__(context, type)) {
                PythonType pt = typeObj as PythonType;
                if (pt != null) {
                    return pt;
                }
            }
            return null;
        }

        /// <summary>
        /// Used in copy_reg which is the only consumer of __flags__ in the standard library.
        /// 
        /// Set if the type is user defined
        /// </summary>
        private const int TypeFlagHeapType = 1 << 9;

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static int Get__flags__(CodeContext/*!*/ context, PythonType/*!*/ type) {
            if (type.IsSystemType) {
                return 0;
            }

            return TypeFlagHeapType;
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static void Set__bases__(CodeContext/*!*/ context, PythonType/*!*/ type, object value) {
            // validate we got a tuple...           
            PythonTuple t = value as PythonTuple;
            if (t == null) throw PythonOps.TypeError("expected tuple of types or old-classes, got '{0}'", PythonTypeOps.GetName(value));

            List<PythonType> ldt = new List<PythonType>();

            foreach (object o in t) {
                // gather all the type objects...
                PythonType adt = o as PythonType;
                if (adt == null) {
                    OldClass oc = o as OldClass;
                    if (oc == null) {
                        throw PythonOps.TypeError("expected tuple of types, got '{0}'", PythonTypeOps.GetName(o));
                    }

                    adt = oc.TypeObject;
                }

                ldt.Add(adt);
            }

            // Ensure that we are not switching the CLI type
            Type newType = NewTypeMaker.GetNewType(type.Name, t);
            if (type.UnderlyingSystemType != newType)
                throw PythonOps.TypeErrorForIncompatibleObjectLayout("__bases__ assignment", type, newType);

            // set bases & the new resolution order
            List<PythonType> mro = CalculateMro(type, ldt);

            type.BaseTypes = ldt;
            type._resolutionOrder = mro;
        }

        private static List<PythonType> CalculateMro(PythonType type, IList<PythonType> ldt) {
            return Mro.Calculate(type, ldt);
        }

        private static bool TryReplaceExtensibleWithBase(Type curType, out Type newType) {
            if (curType.IsGenericType &&
                curType.GetGenericTypeDefinition() == typeof(Extensible<>)) {
                newType = curType.GetGenericArguments()[0];
                return true;
            }
            newType = null;
            return false;
        }

        [SpecialName]
        public object Call(CodeContext context, params object[] args) {
            return PythonTypeOps.CallParams(context, this, args);
        }

        [SpecialName]
        public object Call(CodeContext context, [ParamDictionary]IAttributesCollection kwArgs, params object[] args) {
            return PythonTypeOps.CallWorker(context, this, kwArgs, args);
        }

        public int __cmp__([NotNull]PythonType other) {
            if (other != this) {
                int res = Name.CompareTo(other.Name);

                if (res == 0) {
                    long thisId = IdDispenser.GetId(this);
                    long otherId = IdDispenser.GetId(other);
                    if (thisId > otherId) {
                        return 1;
                    } else {
                        return -1;
                    }
                }
                return res;
            }
            return 0;
        }

        public void __delattr__(CodeContext/*!*/ context, string name) {
            DeleteCustomMember(context, SymbolTable.StringToId(name));
        }

        [SlotField]
        public static PythonTypeSlot __dict__ = new PythonTypeDictSlot(_pythonTypeType);

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static object Get__doc__(CodeContext/*!*/ context, PythonType self) {
            PythonTypeSlot pts;
            object res;
            if (self.TryLookupSlot(context, Symbols.Doc, out pts) &&
                pts.TryGetValue(context, null, self, out res)) {
                return res;
            } else if (self.IsSystemType) {
                return PythonTypeOps.GetDocumentation(self.UnderlyingSystemType);
            }

            return null;
        }

        public object __getattribute__(CodeContext/*!*/ context, string name) {
            object value;
            if (TryGetBoundCustomMember(context, SymbolTable.StringToId(name), out value)) {
                return value;
            }

            throw PythonOps.AttributeError("type object '{0}' has no attribute '{1}'", Name, name);
        }

        public PythonType this[params Type[] args] {
            get {
                if (UnderlyingSystemType == typeof(Array)) {
                    if (args.Length == 1) {
                        return DynamicHelpers.GetPythonTypeFromType(args[0].MakeArrayType());
                    }
                    throw PythonOps.TypeError("expected one argument to make array type, got {0}", args.Length);
                }

                if (!UnderlyingSystemType.IsGenericTypeDefinition) {
                    throw new InvalidOperationException("MakeGenericType on non-generic type");
                }

                return DynamicHelpers.GetPythonTypeFromType(UnderlyingSystemType.MakeGenericType(args));
            }
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static string Get__module__(CodeContext/*!*/ context, PythonType self) {
            return PythonTypeOps.GetModuleName(context, self.UnderlyingSystemType);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static PythonTuple Get__mro__(PythonType type) {
            return PythonTypeOps.MroToPython(type.ResolutionOrder);
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static string Get__name__(PythonType type) {
            return type.Name;
        }

        [SpecialName, PropertyMethod, WrapperDescriptor]
        public static void Set__name__(PythonType type, string name) {
            if (type.IsSystemType) {
                throw PythonOps.TypeError("can't set attributes of built-in/extension type '{0}'", type.Name);
            }

            type.Name = name;
        }

        public string/*!*/ __repr__(CodeContext/*!*/ context) {
            string name = Name;

            if (IsSystemType) {
                if (PythonTypeOps.IsRuntimeAssembly(UnderlyingSystemType.Assembly) || IsPythonType) {
                    string module = Get__module__(context, this);
                    if (module != "__builtin__") {
                        return string.Format("<type '{0}.{1}'>", module, Name);
                    }
                }
                return string.Format("<type '{0}'>", Name);
            } else {
                PythonTypeSlot dts;
                string module = "unknown";
                object modObj;
                if (TryLookupSlot(context, Symbols.Module, out dts) &&
                    dts.TryGetValue(context, this, this, out modObj)) {
                    module = modObj as string;
                }
                return string.Format("<class '{0}.{1}'>", module, name);
            }
        }

        public void __setattr__(CodeContext/*!*/ context, string name, object value) {
            SetCustomMember(context, SymbolTable.StringToId(name), value);
        }

        public List __subclasses__(CodeContext/*!*/ context) {
            List ret = new List();
            IList<WeakReference> subtypes = SubTypes;

            if (subtypes != null) {
                PythonContext pc = PythonContext.GetContext(context);

                foreach (WeakReference wr in subtypes) {
                    if (wr.IsAlive) {
                        PythonType pt = (PythonType)wr.Target;

                        if (pt.PythonContext == null || pt.PythonContext == pc) {
                            ret.AddNoLock(wr.Target);
                        }
                    }
                }
            }

            return ret;
        }

        public virtual List mro() {
            return new List(Get__mro__(this));
        }

        /// <summary>
        /// Returns true if the specified object is an instance of this type.
        /// </summary>
        public virtual bool __instancecheck__(object instance) {
            return SubclassImpl(DynamicHelpers.GetPythonType(instance));
        }

        public virtual bool __subclasscheck__(PythonType sub) {
            return SubclassImpl(sub);
        }

        private bool SubclassImpl(PythonType sub) {
            if (UnderlyingSystemType.IsInterface) {
                // interfaces aren't in bases, and therefore IsSubclassOf doesn't do this check.
                if (UnderlyingSystemType.IsAssignableFrom(sub.UnderlyingSystemType)) {
                    return true;
                }
            }

            return sub.IsSubclassOf(this);
        }

        public virtual bool __subclasscheck__(OldClass sub) {
            return IsSubclassOf(sub.TypeObject);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator Type(PythonType self) {
            return self.UnderlyingSystemType;
        }

        public static implicit operator TypeTracker(PythonType self) {
            return ReflectionCache.GetTypeTracker(self.UnderlyingSystemType);
        }

        #endregion

        #region Internal API

        internal int SlotCount {
            get {
                return _originalSlotCount;
            }
        }

        /// <summary>
        /// Gets the name of the dynamic type
        /// </summary>
        internal string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        internal int Version {
            get {
                return _version;
            }
        }

        internal bool IsNull {
            get {
                return UnderlyingSystemType == typeof(DynamicNull);
            }
        }

        /// <summary>
        /// Gets the resolution order used for attribute lookup
        /// </summary>
        internal IList<PythonType> ResolutionOrder {
            get {
                return _resolutionOrder;
            }
            set {
                lock (SyncRoot) {
                    _resolutionOrder = new List<PythonType>(value);
                }
            }
        }

        /// <summary>
        /// Gets the dynamic type that corresponds with the provided static type. 
        /// 
        /// Returns null if no type is available.  TODO: In the future this will
        /// always return a PythonType created by the DLR.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static PythonType/*!*/ GetPythonType(Type type) {
            object res;
            
            if (!_pythonTypes.TryGetValue(type, out res)) {
                lock (_pythonTypes) {
                    if (!_pythonTypes.TryGetValue(type, out res)) {
                        res = new PythonType(type);

                        _pythonTypes.Add(type, res);
                    }
                }
            }

            return (PythonType)res;
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext/*!*/ context) {
            EnsureInstanceCtor();

            return _instanceCtor.CreateInstance(context);
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext/*!*/ context, object arg0) {
            EnsureInstanceCtor();

            return _instanceCtor.CreateInstance(context, arg0);
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext/*!*/ context, object arg0, object arg1) {
            EnsureInstanceCtor();

            return _instanceCtor.CreateInstance(context, arg0, arg1);
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext/*!*/ context, object arg0, object arg1, object arg2) {
            EnsureInstanceCtor();

            return _instanceCtor.CreateInstance(context, arg0, arg1, arg2);
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext context, params object[] args) {
            Assert.NotNull(args);
            EnsureInstanceCtor();

            // unpack args for common cases so we don't generate code to do it...
            switch (args.Length) {
                case 0: return _instanceCtor.CreateInstance(context);
                case 1: return _instanceCtor.CreateInstance(context, args[0]);
                case 2: return _instanceCtor.CreateInstance(context, args[0], args[1]);
                case 3: return _instanceCtor.CreateInstance(context, args[0], args[1], args[2]);
                default: 
                    return _instanceCtor.CreateInstance(context, args);
            }
        }

        /// <summary>
        /// Allocates the storage for the instance running the .NET constructor.  This provides
        /// the creation functionality for __new__ implementations.
        /// </summary>
        internal object CreateInstance(CodeContext context, object[] args, string[] names) {
            Assert.NotNull(args, "args");
            Assert.NotNull(names, "names");

            EnsureInstanceCtor();

            return _instanceCtor.CreateInstance(context, args, names);
        }

        internal int Hash(object o) {
            EnsureHashSite();

            return _hashSite.Target(_hashSite, o);
        }

        internal bool TryGetLength(object o, out int length) {
            EnsureLengthSite();

            PythonTypeSlot lenSlot = _lenSlot;
            CodeContext ctx = Context.DefaultBinderState.Context;
            if (lenSlot == null && !PythonOps.TryResolveTypeSlot(ctx, this, Symbols.Length, out lenSlot)) {
                length = 0;
                return false;                
            }

            object func;
            if (!lenSlot.TryGetValue(ctx, o, this, out func)) {
                length = 0;
                return false;
            }

            object res = _lenSite.Target(_lenSite, ctx, func);
            if (!(res is int)) {
                throw PythonOps.ValueError("__len__ must return int");
            }

            length = (int)res;
            return true;
        }

        internal bool EqualRetBool(object self, object other) {
            if (_eqSite == null) {
                Interlocked.CompareExchange(
                    ref _eqSite,
                    Context.CreateComparisonSite(PythonOperationKind.Equal),
                    null
                );
            }

            return _eqSite.Target(_eqSite, self, other);
        }

        internal int Compare(object self, object other) {
            if (_compareSite == null) {
                Interlocked.CompareExchange(
                    ref _compareSite,
                    Context.MakeSortCompareSite(),
                    null
                );
            }

            return _compareSite.Target(_compareSite, self, other);
        }

        internal bool TryGetBoundAttr(CodeContext context, object o, SymbolId name, out object ret) {
            CallSite<Func<CallSite, object, CodeContext, object>> site = GetTryGetMemberSite(context, name);

            try {
                ret = site.Target(site, o, context);
                return ret != OperationFailed.Value;
            } catch (MissingMemberException) {
                ret = null;
                return false;
            }
        }

        private CallSite<Func<CallSite, object, CodeContext, object>> GetTryGetMemberSite(CodeContext context, SymbolId name) {
            CallSite<Func<CallSite, object, CodeContext, object>> site;
            if (PythonOps.IsClsVisible(context)) {
                if (_tryGetMemSiteShowCls == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemSiteShowCls,
                        new Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>>(),
                        null
                    );
                }

                lock (_tryGetMemSiteShowCls) {
                    if (!_tryGetMemSiteShowCls.TryGetValue(name, out site)) {
                        _tryGetMemSiteShowCls[name] = site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(
                            PythonContext.GetContext(context).DefaultClsBinderState.GetMember(
                                SymbolTable.IdToString(name),
                                true
                            )
                        );
                    }
                }
            } else {
                if (_tryGetMemSite == null) {
                    Interlocked.CompareExchange(
                        ref _tryGetMemSite,
                        new Dictionary<SymbolId, CallSite<Func<CallSite, object, CodeContext, object>>>(),
                        null
                    );
                }

                lock (_tryGetMemSite) {
                    if (!_tryGetMemSite.TryGetValue(name, out site)) {
                        _tryGetMemSite[name] = site = CallSite<Func<CallSite, object, CodeContext, object>>.Create(
                            PythonContext.GetContext(context).DefaultBinderState.GetMember(
                                SymbolTable.IdToString(name),
                                true
                            )
                        );
                    }
                }
            }
            return site;
        }

        internal CallSite<Func<CallSite, object, int>>  HashSite {
            get {
                EnsureHashSite();

                return _hashSite;
            }
        }

        private void EnsureHashSite() {
            if(_hashSite == null) {
                Interlocked.CompareExchange(
                    ref _hashSite,
                    CallSite<Func<CallSite, object, int>>.Create(
                        Context.DefaultBinderState.Operation(
                            PythonOperationKind.Hash
                        )
                    ),
                    null
                );
            }
        }

        private void EnsureLengthSite() {
            if (_lenSite == null) {
                Interlocked.CompareExchange(
                    ref _lenSite,
                    CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                        Context.DefaultBinderState.InvokeNone
                    ),
                    null
                );
            }
        }

        /// <summary>
        /// Gets the underlying system type that is backing this type.  All instances of this
        /// type are an instance of the underlying system type.
        /// </summary>
        internal Type/*!*/ UnderlyingSystemType {
            get {
                return _underlyingSystemType;
            }
        }

        /// <summary>
        /// Gets the extension type for this type.  The extension type provides
        /// a .NET type which can be inherited from to extend sealed classes
        /// or value types which Python allows inheritance from.
        /// </summary>
        internal Type/*!*/ ExtensionType {
            get {
                if (!_underlyingSystemType.IsEnum) {
                    switch (Type.GetTypeCode(_underlyingSystemType)) {
                        case TypeCode.String: return typeof(ExtensibleString);
                        case TypeCode.Int32: return typeof(Extensible<int>);
                        case TypeCode.Double: return typeof(Extensible<double>);
                        case TypeCode.Object:
                            if (_underlyingSystemType == typeof(BigInteger)) {
                                return typeof(Extensible<BigInteger>);
                            } else if (_underlyingSystemType == typeof(Complex64)) {
                                return typeof(ExtensibleComplex);
                            }
                            break;
                    }
                }
                return _underlyingSystemType;
            }
        }

        /// <summary>
        /// Gets the base types from which this type inherits.
        /// </summary>
        internal IList<PythonType>/*!*/ BaseTypes {
            get {
                return _bases;
            }
            set {
                // validate input...
                foreach (PythonType pt in value) {
                    if (pt == null) throw new ArgumentNullException("value", "a PythonType was null while assigning base classes");
                }

                // first update our sub-type list

                lock (_bases) {
                    foreach (PythonType dt in _bases) {
                        dt.RemoveSubType(this);
                    }

                    // set the new bases
                    List<PythonType> newBases = new List<PythonType>(value);

                    // add us as subtypes of our new bases
                    foreach (PythonType dt in newBases) {
                        dt.AddSubType(this);
                    }

                    UpdateVersion();
                    _bases = newBases.ToArray();
                }
            }
        }

        /// <summary>
        /// Returns true if this type is a subclass of other
        /// </summary>
        internal bool IsSubclassOf(PythonType other) {
            // check for a type match
            if (other == this) {
                return true;
            }

            //Python doesn't have value types inheriting from ValueType, but we fake this for interop
            if (other.UnderlyingSystemType == typeof(ValueType) && UnderlyingSystemType.IsValueType) {
                return true;
            }

            return IsSubclassWorker(other);
        }

        private bool IsSubclassWorker(PythonType other) {
            for (int i = 0; i < _bases.Length; i++) {
                PythonType baseClass = _bases[i];

                if (baseClass == other || baseClass.IsSubclassWorker(other)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True if the type is a system type.  A system type is a type which represents an
        /// underlying .NET type and not a subtype of one of these types.
        /// </summary>
        internal bool IsSystemType {
            get {
                return (_attrs & PythonTypeAttributes.SystemType) != 0;
            }
            set {
                if (value) _attrs |= PythonTypeAttributes.SystemType;
                else _attrs &= (~PythonTypeAttributes.SystemType);
            }
        }

        internal bool IsWeakReferencable {
            get {
                return (_attrs & PythonTypeAttributes.WeakReferencable) != 0;
            }
        }

        internal bool HasDictionary {
            get {
                return (_attrs & PythonTypeAttributes.HasDictionary) != 0;
            }
            set {
                if (value) _attrs |= PythonTypeAttributes.HasDictionary;
                else _attrs &= (~PythonTypeAttributes.HasDictionary);
            }
        }        

        internal void SetConstructor(BuiltinFunction ctor) {
            _ctor = ctor;
        }

        internal bool IsPythonType {
            get {
                return (_attrs & PythonTypeAttributes.IsPythonType) != 0;
            }
            set {
                if (value) {
                    _attrs |= PythonTypeAttributes.IsPythonType;
                } else {
                    _attrs &= ~PythonTypeAttributes.IsPythonType;
                }
            }
        }

        internal OldClass OldClass {
            get {
                return _oldClass;
            }
            set {
                _oldClass = value;
            }
        }

        internal bool IsOldClass {
            get {
                return _oldClass != null;
            }
        }

        internal PythonContext PythonContext {
            get {
                return _pythonContext;
            }
        }

        internal PythonContext/*!*/ Context {
            get {
                return _pythonContext ?? DefaultContext.DefaultPythonContext;
            }
        }

        internal object SyncRoot {
            get {
                // TODO: This is un-ideal, we should lock on something private.
                return this;
            }
        }

        internal bool IsHiddenMember(string name) {
            PythonTypeSlot dummySlot;
            return !TryResolveSlot(DefaultContext.Default, SymbolTable.StringToId(name), out dummySlot) &&
                    TryResolveSlot(DefaultContext.DefaultCLS, SymbolTable.StringToId(name), out dummySlot);
        }

        #endregion

        #region Type member access

        /// <summary>
        /// Looks up a slot on the dynamic type
        /// </summary>
        internal bool TryLookupSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            if (IsSystemType) {
                return PythonBinder.GetBinder(context).TryLookupSlot(context, this, name, out slot);
            }

            return _dict.TryGetValue(name, out slot);
        }

        /// <summary>
        /// Searches the resolution order for a slot matching by name
        /// </summary>
        internal bool TryResolveSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                // don't look at interfaces - users can inherit from them, but we resolve members
                // via methods implemented on types and defined by Python.
                if (dt.IsSystemType && !dt.UnderlyingSystemType.IsInterface) {
                    return PythonBinder.GetBinder(context).TryResolveSlot(context, dt, this, name, out slot);
                }

                if (dt.TryLookupSlot(context, name, out slot)) {
                    return true;
                }
            }

            if (UnderlyingSystemType.IsInterface) {
                return TypeCache.Object.TryResolveSlot(context, name, out slot);
            }
            

            slot = null;
            return false;
        }

        /// <summary>
        /// Searches the resolution order for a slot matching by name.
        /// 
        /// Includes searching for methods in old-style classes
        /// </summary>
        internal bool TryResolveMixedSlot(CodeContext context, SymbolId name, out PythonTypeSlot slot) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt.TryLookupSlot(context, name, out slot)) {
                    return true;
                }

                if (dt.OldClass != null) {
                    object ret;
                    if (dt.OldClass.TryLookupSlot(name, out ret)) {
                        slot = ret as PythonTypeSlot;
                        if (slot == null) {
                            slot = new PythonTypeUserDescriptorSlot(ret);
                        }
                        return true;
                    }
                }
            }

            slot = null;
            return false;
        }

        /// <summary>
        /// Internal helper to add a new slot to the type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="slot"></param>
        internal void AddSlot(SymbolId name, PythonTypeSlot slot) {
            Debug.Assert(!IsSystemType);

            _dict[name] = slot;
        }

        internal void SetCustomMember(CodeContext/*!*/ context, SymbolId name, object value) {
            Debug.Assert(context != null);

            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TrySetValue(context, null, this, value))
                    return;
            }

            if (PythonType._pythonTypeType.TryResolveSlot(context, name, out dts)) {
                if (dts.TrySetValue(context, this, PythonType._pythonTypeType, value))
                    return;
            }

            if (IsSystemType) {
                throw new MissingMemberException(String.Format("'{0}' object has no attribute '{1}'", Name, SymbolTable.IdToString(name)));
            }

            dts = value as PythonTypeSlot;
            if (dts != null) {
                _dict[name] = dts;
            } else if (IsSystemType) {
                _dict[name] = new PythonTypeValueSlot(value);
            } else {
                _dict[name] = new PythonTypeUserDescriptorSlot(value);
            }

            UpdateVersion();
        }

        internal bool DeleteCustomMember(CodeContext/*!*/ context, SymbolId name) {
            Debug.Assert(context != null);

            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryDeleteValue(context, null, this))
                    return true;
            }

            if (IsSystemType) {
                throw new MissingMemberException(String.Format("can't delete attributes of built-in/extension type '{0}'", Name, SymbolTable.IdToString(name)));
            }

            if (!_dict.Remove(name)) {
                throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.MemberDoesNotExist,
                    name.ToString()));
            }

            UpdateVersion();
            return true;
        }

        internal bool TryGetBoundCustomMember(CodeContext context, SymbolId name, out object value) {
            PythonTypeSlot dts;
            if (TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetValue(context, null, this, out value)) {
                    return true;
                }
            }

            // search the type
            PythonType myType = DynamicHelpers.GetPythonType(this);
            if (myType.TryResolveSlot(context, name, out dts)) {
                if (dts.TryGetValue(context, this, myType, out value)) {
                    return true;
                }
            }

            value = null;
            return false;
        }

        #region IFastGettable Members

        T IFastGettable.MakeGetBinding<T>(CallSite<T> site, PythonGetMemberBinder/*!*/ binder, CodeContext context, string name) {
            Debug.Assert(typeof(T) == typeof(Func<CallSite, object, CodeContext, object>));

            if (GetType() != typeof(PythonType)) {
                // we don't optimize classes derived from meta classes yet                
                return null;
            }

            switch (name) {
                case "__dict__":
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(FastGetSingleton.Instance.GetDictionary);
                case "__class__":
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(FastGetSingleton.Instance.GetClass);
                case "__bases__":
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(FastGetSingleton.Instance.GetBases);
                case "__name__":
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(FastGetSingleton.Instance.GetName);
            }


            int version = _version;
            PythonTypeSlot dts;
            SymbolId symName = SymbolTable.StringToId(name);
            if (TryResolveSlot(context, symName, out dts) && dts.GetAlwaysSucceeds) {
                return MakeFastGetDelegate<T>(binder, dts, _version, false);
            }

            // search the type
            PythonType myType = DynamicHelpers.GetPythonType(this);
            if (myType.TryResolveSlot(context, symName, out dts) && dts.GetAlwaysSucceeds) {
                return MakeFastGetDelegate<T>(binder, dts, _version, true);
            }

            return null;
        }

        private T MakeFastGetDelegate<T>(PythonGetMemberBinder binder, PythonTypeSlot slot, int version, bool fromMetaClass) where T : class {
            if (slot is ReflectedExtensionProperty) {
                // very tricky, these include WrapperDescriptors.
                return null;
            }

            if (IsSystemType) {
                if (slot.CanOptimizeGets) {
                    FastGetBase fastGet;
                    EnsureFastTypeGets();
                    lock (_cachedTypeGets) {
                        if (!_cachedTypeGets.TryGetValue(binder.Name, out fastGet)) {
                            _cachedTypeGets[binder.Name] = fastGet = new OptimizingFastGetSystemType(
                                    binder,
                                    this,
                                    slot,
                                    fromMetaClass
                                );
                        }

                        OptimizingFastGetSystemType sysGet = (OptimizingFastGetSystemType)fastGet;
                        if (sysGet.UseNonOptimizedSite) {
                            return (T)(object)new Func<CallSite, object, CodeContext, object>(sysGet.GetSlot);
                        }

                        return null;
                    }
                } else {
                    ReflectedField field = slot as ReflectedField;
                    if (field != null) {
                        return (T)(object)new Func<CallSite, object, CodeContext, object>(
                            new FastGetSystemTypeValue(binder, this, field._info.GetValue(null)).GetSlot
                        );
                    }
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(new FastGetSystemTypeSlot(binder, this, slot, fromMetaClass).GetSlot);
                }
            }

            if (slot.CanOptimizeGets) {
                EnsureFastTypeGets();
                lock (_cachedTypeGets) {
                    FastGetBase fastGet;
                    if (!_cachedTypeGets.TryGetValue(binder.Name, out fastGet) || ((OptimizingFastGetUserType)fastGet)._version != version) {
                        _cachedTypeGets[binder.Name] = fastGet = new OptimizingFastGetUserType(
                            binder,
                            this,
                            _version,
                            slot,
                            fromMetaClass
                        );
                    }

                    OptimizingFastGetUserType userGet = (OptimizingFastGetUserType)fastGet;
                    if (userGet.UseNonOptimizedSite) {
                        return (T)(object)new Func<CallSite, object, CodeContext, object>(userGet.GetSlot);
                    }

                    return null;
                }
            } else {
                ReflectedField field = slot as ReflectedField;                
                if (field != null) {
                    return (T)(object)new Func<CallSite, object, CodeContext, object>(
                        new FastGetUserTypeValue(binder, this, _version, field._info.GetValue(null)).GetSlot
                    );
                }

                return (T)(object)new Func<CallSite, object, CodeContext, object>(new FastGetUserTypeSlot(binder, this, _version, slot, fromMetaClass).GetSlot);
            }
        }

        private void EnsureFastTypeGets() {
            if (_cachedTypeGets == null) {
                Interlocked.CompareExchange(ref _cachedTypeGets,
                    new Dictionary<string, FastGetBase>(),
                    null);
            }
        }

        class FastGetUserTypeBase : FastGetBase {
            protected readonly WeakReference _type;
            internal readonly int _version;
            protected int _hitCount;            

            public FastGetUserTypeBase(PythonGetMemberBinder binder, PythonType type, int version)
                : base(binder) {
                if (type._weakRef == null) {
                    type._weakRef = new WeakReference(type);
                }
                _type = type._weakRef;
                _version = version;
            }

            public bool UseNonOptimizedSite {
                get {
                    return _hitCount < 100;
                }
            }
        }

        class FastGetUserTypeSlot : FastGetUserTypeBase {
            private readonly PythonTypeSlot _slot;
            private readonly bool _fromMetaClass;

            public FastGetUserTypeSlot(PythonGetMemberBinder binder, PythonType type, int version, PythonTypeSlot slot, bool fromMetaClass)
                : base(binder, type, version) {
                if (type._weakRef == null) {
                    type._weakRef = new WeakReference(type);
                }
                _fromMetaClass = fromMetaClass;
                _slot = slot;
            }
            
            public object GetSlot(CallSite site, object self, CodeContext context) {
                object res;
                if (self == _type.Target && ((PythonType)self).Version == _version && _slot.TryGetValue(context, _fromMetaClass ? self : null, (PythonType)self, out res)) {
                    return res;
                }

                return Update(site, self, context);
            }
        }

        class OptimizingFastGetUserType : FastGetUserTypeBase {
            private readonly PythonTypeSlot _slot;
            private readonly bool _fromMetaClass;

            public OptimizingFastGetUserType(PythonGetMemberBinder binder, PythonType type, int version, PythonTypeSlot slot, bool fromMetaClass)
                : base(binder, type, version) {
                if (type._weakRef == null) {
                    type._weakRef = new WeakReference(type);
                }
                _fromMetaClass = fromMetaClass;
                _slot = slot;
            }

            public object GetSlot(CallSite site, object self, CodeContext context) {
                object res;
                if (self == _type.Target && 
                    ((PythonType)self).Version == _version && 
                    UseNonOptimizedSite &&
                    _slot.TryGetValue(context, _fromMetaClass ? self : null, (PythonType)self, out res)) {
                    _hitCount++;
                    return res;
                }

                return Update(site, self, context);
            }
        }

        class FastGetUserTypeValue : FastGetUserTypeBase {
            private readonly object _value;

            public FastGetUserTypeValue(PythonGetMemberBinder binder, PythonType type, int version, object value)
                : base(binder, type, version) {
                _value = value;
            }

            public object GetSlot(CallSite site, object self, CodeContext context) {
                if (self == _type.Target && ((PythonType)self).Version == _version) {
                    return _value;
                }

                return Update(site, self, context);
            }
        }

        class FastGetSystemTypeBase : FastGetBase {
            protected readonly PythonType _type;
            internal int _hitCount;            

            public FastGetSystemTypeBase(PythonGetMemberBinder binder, PythonType type)
                : base(binder) {
                _type = type;
            }

            internal bool UseNonOptimizedSite {
                get {
                    return _hitCount < 100;
                }
            }
        }

        class FastGetSystemTypeSlot : FastGetSystemTypeBase {
            private readonly PythonTypeSlot _slot;
            private readonly PythonType _instance;

            public FastGetSystemTypeSlot(PythonGetMemberBinder binder, PythonType type, PythonTypeSlot slot, bool fromMetaClass)
                : base(binder, type) {
                _slot = slot;
                if (fromMetaClass) {
                    _instance = type;
                }
            }

            public object GetSlot(CallSite site, object self, CodeContext context) {
                object res;
                if (self == _type && _slot.TryGetValue(context, _instance, _type, out res)) {
                    return res;
                }

                return Update(site, self, context);
            }
        }

        class OptimizingFastGetSystemType : FastGetSystemTypeBase {
            private readonly PythonTypeSlot _slot;
            private readonly PythonType _instance;

            public OptimizingFastGetSystemType(PythonGetMemberBinder binder, PythonType type, PythonTypeSlot slot, bool fromMetaClass)
                : base(binder, type) {
                _slot = slot;
                if (fromMetaClass) {
                    _instance = type;
                }
            }

            public object GetSlot(CallSite site, object self, CodeContext context) {
                object res;
                if (self == _type && UseNonOptimizedSite && _slot.TryGetValue(context, _instance, _type, out res)) {
                    _hitCount++;
                    return res;
                }

                return Update(site, self, context);
            }
        }

        class FastGetSystemTypeValue : FastGetSystemTypeBase {
            private readonly object _value;

            public FastGetSystemTypeValue(PythonGetMemberBinder binder, PythonType type, object value)
                : base(binder, type) {
                _value = value;
            }

            public object GetSlot(CallSite site, object self, CodeContext context) {
                if (self == _type) {
                    return _value;
                }

                return Update(site, self, context);
            }
        }


        class FastGetSingleton {
            public static FastGetSingleton Instance = new FastGetSingleton();

            public object GetDictionary(CallSite site, object self, CodeContext context) {
                if (self.GetType() == typeof(PythonType)) {
                    return new DictProxy(((PythonType)self));
                }

                return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, self, context);
            }

            public object GetClass(CallSite site, object self, CodeContext context) {
                if (self.GetType() == typeof(PythonType)) {
                    return TypeCache.PythonType;
                }

                return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, self, context);
            }

            public object GetBases(CallSite site, object self, CodeContext context) {
                if (self.GetType() == typeof(PythonType)) {
                    return ((PythonType)self).GetBasesTuple();
                }

                return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, self, context);
            }

            public object GetName(CallSite site, object self, CodeContext context) {
                if (self.GetType() == typeof(PythonType)) {
                    return ((PythonType)self).Name;
                }

                return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, self, context);
            }
        }

        #endregion

        #endregion

        #region Instance member access

        internal object GetMember(CodeContext context, object instance, SymbolId name) {
            object res;
            if (TryGetMember(context, instance, name, out res)) {
                return res;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember,
                SymbolTable.IdToString(name)));
        }

        internal object GetBoundMember(CodeContext context, object instance, SymbolId name) {
            object value;
            if (TryGetBoundMember(context, instance, name, out value)) {
                return value;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture,
                IronPython.Resources.CantFindMember,
                SymbolTable.IdToString(name)));
        }

        internal void SetMember(CodeContext context, object instance, SymbolId name, object value) {
            if (TrySetMember(context, instance, name, value)) {
                return;
            }

            throw new MissingMemberException(
                String.Format(CultureInfo.CurrentCulture,
                    IronPython.Resources.Slot_CantSet,
                    name));
        }

        internal void DeleteMember(CodeContext context, object instance, SymbolId name) {
            if (TryDeleteMember(context, instance, name)) {
                return;
            }

            throw new MissingMemberException(String.Format(CultureInfo.CurrentCulture, "couldn't delete member {0}", name));
        }

        /// <summary>
        /// Gets a value from a dynamic type and any sub-types.  Values are stored in slots (which serve as a level of 
        /// indirection).  This searches the types resolution order and returns the first slot that
        /// contains the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetMember(CodeContext context, object instance, SymbolId name, out object value) {
            if (TryGetNonCustomMember(context, instance, name, out value)) {
                return true;
            }

            try {
                if (PythonTypeOps.TryInvokeBinaryOperator(context, instance, SymbolTable.IdToString(name), Symbols.GetBoundAttr, out value)) {
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            return false;
        }

        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.  Equivelent to object.__getattribute__
        /// but it doens't throw an exception.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetNonCustomMember(CodeContext context, object instance, SymbolId name, out object value) {
            PythonType pt;
            IPythonObject sdo;
            bool hasValue = false;
            value = null;

            // first see if we have the value in the instance dictionary...
            // TODO: Instance checks should also work on functions, 
            if ((pt = instance as PythonType) != null) {
                PythonTypeSlot pts;
                if (pt.TryLookupSlot(context, name, out pts)) {
                    hasValue = pts.TryGetValue(context, null, this, out value);
                }
            } else if ((sdo = instance as IPythonObject) != null) {
                IAttributesCollection iac = sdo.Dict;

                hasValue = iac != null && iac.TryGetValue(name, out value);
            } 

            // then check through all the descriptors.  If we have a data
            // descriptor it takes priority over the value we found in the
            // dictionary.  Otherwise only run a get descriptor if we don't
            // already have a value.
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                object newValue;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (!hasValue || slot.IsSetDescriptor(context, this)) {
                        if (slot.TryGetValue(context, instance, this, out newValue))
                            value = newValue;
                            return true;
                    }
                }
            }

            return hasValue;
        }

        /// <summary>
        /// Gets a value from a dynamic type and any sub-types.  Values are stored in slots (which serve as a level of 
        /// indirection).  This searches the types resolution order and returns the first slot that
        /// contains the value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            object getattr;
            if (TryResolveNonObjectSlot(context, instance, Symbols.GetAttribute, out getattr)) {
                value = InvokeGetAttributeMethod(context, name, getattr);
                return true;
            }

            return TryGetNonCustomBoundMember(context, instance, name, out value);
        }

        private object InvokeGetAttributeMethod(CodeContext context, SymbolId name, object getattr) {
            EnsureGetAttributeSite(context);

            return _getAttributeSite.Target(_getAttributeSite, context, getattr, SymbolTable.IdToString(name));
        }

        private void EnsureGetAttributeSite(CodeContext context) {
            if (_getAttributeSite == null) {
                Interlocked.CompareExchange(
                    ref _getAttributeSite,
                    CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(
                        PythonContext.GetContext(context).DefaultBinderState.InvokeOne
                    ),
                    null
                );
            }
        }

        /// <summary>
        /// Attempts to lookup a member w/o using the customizer.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        internal bool TryGetNonCustomBoundMember(CodeContext context, object instance, SymbolId name, out object value) {
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac != null && iac.TryGetValue(name, out value)) {
                    return true;
                }
            }

            if (TryResolveSlot(context, instance, name, out value)) {
                return true;
            }

            try {
                object getattr;
                if (TryResolveNonObjectSlot(context, instance, Symbols.GetBoundAttr, out getattr)) {
                    value = InvokeGetAttributeMethod(context, name, getattr);
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            value = null;
            return false;
        }

        private bool TryResolveSlot(CodeContext context, object instance, SymbolId name, out object value) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetValue(context, instance, this, out value))
                        return true;
                }
            }

            value = null;
            return false;
        }

        private bool TryResolveNonObjectSlot(CodeContext context, object instance, SymbolId name, out object value) {
            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt == TypeCache.Object) break;

                PythonTypeSlot slot;
                if (dt.TryLookupSlot(context, name, out slot)) {
                    if (slot.TryGetValue(context, instance, this, out value))
                        return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Sets a value on an instance.  If a slot is available in the most derived type the slot
        /// is set there, otherwise the value is stored directly in the instance.
        /// </summary>
        internal bool TrySetMember(CodeContext context, object instance, SymbolId name, object value) {
            object setattr;
            if (TryResolveNonObjectSlot(context, instance, Symbols.SetAttr, out setattr)) {
                if (_setAttrSite == null) {
                    Interlocked.CompareExchange(
                        ref _setAttrSite,
                        CallSite<Func<CallSite, CodeContext, object, object, string, object, object>>.Create(
                            PythonContext.GetContext(context).DefaultBinderState.Invoke(
                                new CallSignature(4)
                            )
                        ),
                        null
                    );
                }

                _setAttrSite.Target(_setAttrSite, context, setattr, instance, SymbolTable.IdToString(name), value);
                return true;                              
            }

            return TrySetNonCustomMember(context, instance, name, value);
        }

        /// <summary>
        /// Attempst to set a value w/o going through the customizer.
        /// 
        /// This enables languages to provide the "base" implementation for setting attributes
        /// so that the customizer can call back here.
        /// </summary>
        internal bool TrySetNonCustomMember(CodeContext context, object instance, SymbolId name, object value) {
            PythonTypeSlot slot;
            if (TryResolveSlot(context, name, out slot)) {
                if (slot.TrySetValue(context, instance, this, value)) {
                    return true;
                }
            }

            // set the attribute on the instance
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac == null && sdo.PythonType.HasDictionary) {
                    iac = PythonDictionary.MakeSymbolDictionary();

                    if ((iac = sdo.SetDict(iac)) == null) {
                        return false;
                    }
                }

                iac[name] = value;
                return true;
            }

            return false;
        }

        internal bool TryDeleteMember(CodeContext context, object instance, SymbolId name) {
            try {
                object delattr;
                if (TryResolveNonObjectSlot(context, instance, Symbols.DelAttr, out delattr)) {
                    InvokeGetAttributeMethod(context, name, delattr);
                    return true;
                }
            } catch (MissingMemberException) {
                //!!! when do we let the user see this exception?
            }

            return TryDeleteNonCustomMember(context, instance, name);
        }

        internal bool TryDeleteNonCustomMember(CodeContext context, object instance, SymbolId name) {
            PythonTypeSlot slot;
            if (TryResolveSlot(context, name, out slot)) {
                if (slot.TryDeleteValue(context, instance, this)) {
                    return true;
                }
            }

            // set the attribute on the instance
            IPythonObject sdo = instance as IPythonObject;
            if (sdo != null) {
                IAttributesCollection iac = sdo.Dict;
                if (iac == null && sdo.PythonType.HasDictionary) {
                    iac = PythonDictionary.MakeSymbolDictionary();

                    if ((iac = sdo.SetDict(iac)) == null) {
                        return false;
                    }
                }

                return iac.Remove(name);
            }

            return false;
        }

        #endregion

        #region Member lists

        /// <summary>
        /// Returns a list of all slot names for the type and any subtypes.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        internal List GetMemberNames(CodeContext context) {
            return GetMemberNames(context, null);
        }

        /// <summary>
        /// Returns a list of all slot names for the type, any subtypes, and the instance.
        /// </summary>
        /// <param name="context">The context that is doing the inquiry of InvariantContext.Instance.</param>
        /// <param name="self">the instance to get instance members from, or null.</param>
        internal List GetMemberNames(CodeContext context, object self) {
            List res = TryGetCustomDir(context, self);
            if (res != null) {
                return res;
            }

            Dictionary<string, string> keys = new Dictionary<string, string>();

            for (int i = 0; i < _resolutionOrder.Count; i++) {
                PythonType dt = _resolutionOrder[i];

                if (dt.IsSystemType) {
                    PythonBinder.GetBinder(context).ResolveMemberNames(context, dt, this, keys);
                } else {
                    AddUserTypeMembers(context, keys, dt);
                }
            }
            

            AddInstanceMembers(self, keys);

            return new List(keys.Keys);
        }

        private List TryGetCustomDir(CodeContext context, object self) {
            if (self != null) {
                object dir;
                if (TryResolveNonObjectSlot(context, self, SymbolTable.StringToId("__dir__"), out dir)) {
                    EnsureDirSite(context);

                    return new List(_dirSite.Target(_dirSite, context, dir));
                }
            }

            return null;
        }

        private void EnsureDirSite(CodeContext context) {
            if (_dirSite == null) {
                Interlocked.CompareExchange(
                    ref _dirSite,
                    CallSite<Func<CallSite, CodeContext, object, object>>.Create(
                        PythonContext.GetContext(context).DefaultBinderState.InvokeNone
                    ),
                    null);
            }
        }

        /// <summary>
        /// Adds members from a user defined type.
        /// </summary>
        private void AddUserTypeMembers(CodeContext context, Dictionary<string, string> keys, PythonType dt) {
            foreach (KeyValuePair<SymbolId, PythonTypeSlot> kvp in dt._dict) {
                if (keys.ContainsKey(SymbolTable.IdToString(kvp.Key))) continue;

                keys[SymbolTable.IdToString(kvp.Key)] = SymbolTable.IdToString(kvp.Key);
            }
        }

        /// <summary>
        /// Adds members from a user defined type instance
        /// </summary>
        private static void AddInstanceMembers(object self, Dictionary<string, string> keys) {
            IPythonObject dyno = self as IPythonObject;
            if (dyno != null) {
                IAttributesCollection iac = dyno.Dict;
                if (iac != null) {
                    lock (iac) {
                        foreach (SymbolId id in iac.SymbolAttributes.Keys) {
                            keys[SymbolTable.IdToString(id)] = SymbolTable.IdToString(id);
                        }
                    }
                }
            }
        }

        internal PythonDictionary GetMemberDictionary(CodeContext context) {
            return GetMemberDictionary(context, true);
        }

        internal PythonDictionary GetMemberDictionary(CodeContext context, bool excludeDict) {
            PythonDictionary iac = PythonDictionary.MakeSymbolDictionary();
            if (IsSystemType) {
                PythonBinder.GetBinder(context).LookupMembers(context, this, iac);
            } else {
                foreach (SymbolId x in _dict.Keys) {
                    if (excludeDict && x.ToString() == "__dict__") {
                        continue;
                    }

                    PythonTypeSlot dts;
                    if (TryLookupSlot(context, x, out dts)) {
                        //??? why check for DTVS?
                        object val;
                        if (dts.TryGetValue(context, null, this, out val)) {
                            if ((dts is PythonTypeValueSlot) || (dts is PythonTypeUserDescriptorSlot)) {
                                ((IAttributesCollection)iac)[x] = val;
                            } else {
                                ((IAttributesCollection)iac)[x] = dts;
                            }
                        }
                    }
                }
            }
            return iac;
        }

        internal IAttributesCollection GetMemberDictionary(CodeContext context, object self) {
            if (self != null) {
                IPythonObject sdo = self as IPythonObject;
                if (sdo != null) return sdo.Dict;

                return null;
            }
            return GetMemberDictionary(context);
        }

        #endregion

        #region User type initialization

        private void InitializeUserType(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection vars) {
            // we don't support overriding __mro__
            if (vars.ContainsKey(Symbols.MethodResolutionOrder))
                throw new NotImplementedException("Overriding __mro__ of built-in types is not implemented");

            // cannot override mro when inheriting from type
            if (vars.ContainsKey(SymbolTable.StringToId("mro"))) {
                foreach (object o in bases) {
                    PythonType dt = o as PythonType;
                    if (dt != null && dt.IsSubclassOf(TypeCache.PythonType)) {
                        throw new NotImplementedException("Overriding type.mro is not implemented");
                    }
                }
            }

            bases = ValidateBases(bases);

            _name = name;
            _bases = GetBasesAsList(bases).ToArray();
            _pythonContext = PythonContext.GetContext(context);
            _resolutionOrder = CalculateMro(this, _bases);

            bool hasSlots = false;
            foreach (PythonType pt in _bases) {
                // if we directly inherit from 2 types with slots then the indexes would
                // conflict so inheritance isn't allowed.
                int slotCount = pt.GetUsedSlotCount();
                
                if (slotCount != 0) {
                    if (hasSlots) {
                        throw PythonOps.TypeError("multiple bases have instance lay-out conflict");
                    }
                    hasSlots = true;
                }
                
                pt.AddSubType(this);
            }

            foreach (PythonType pt in _resolutionOrder) {
                // we need to calculate the number of slots from resolution
                // order to deal with multiple bases having __slots__ that
                // directly inherit from each other.
                _originalSlotCount += pt.GetUsedSlotCount();
            }


            EnsureDict();

            PopulateDictionary(context, name, bases, vars);

            // calculate the .NET type once so it can be used for things like super calls
            _underlyingSystemType = NewTypeMaker.GetNewType(name, bases);

            // then let the user intercept and rewrite the type - the user can't create
            // instances of this type yet.
            _underlyingSystemType = __clrtype__();
            
            // finally assign the ctors from the real type the user provided
            _ctor = BuiltinFunction.MakeMethod(Name, _underlyingSystemType.GetConstructors(), _underlyingSystemType, FunctionType.Function);
        }

        internal static List<string> GetSlots(IAttributesCollection dict) {
            List<string> res = null;
            object slots;
            if (dict != null && dict.TryGetValue(Symbols.Slots, out slots)) {
                res = SlotsToList(slots);
            }

            return res;
        }

        internal static List<string> SlotsToList(object slots) {
            List<string> res = new List<string>();
            ISequence seq = slots as ISequence;
            if (seq != null && !(seq is ExtensibleString)) {
                res = new List<string>(seq.__len__());
                for (int i = 0; i < seq.__len__(); i++) {
                    res.Add(GetSlotName(seq[i]));
                }

                res.Sort();
            } else {
                res = new List<string>(1);
                res.Add(GetSlotName(slots));
            }
            return res;
        }


        private static string GetSlotName(object o) {
            string value;
            if (!Converter.TryConvertToString(o, out value) || String.IsNullOrEmpty(value))
                throw PythonOps.TypeError("slots must be one string or a list of strings");

            for (int i = 0; i < value.Length; i++) {
                if ((value[i] >= 'a' && value[i] <= 'z') ||
                    (value[i] >= 'A' && value[i] <= 'Z') ||
                    (i != 0 && value[i] >= '0' && value[i] <= '9') ||
                    value[i] == '_') {
                    continue;
                }
                throw PythonOps.TypeError("__slots__ must be valid identifiers");
            }

            return value;
        }

        private int GetUsedSlotCount() {
            int slotCount = 0;
            if (_slots != null) {
                slotCount = _slots.Length;

                if (Array.IndexOf(_slots, "__weakref__") != -1) {
                    slotCount--;
                }

                if (Array.IndexOf(_slots, "__dict__") != -1) {
                    slotCount--;
                }
            }
            return slotCount;
        }

        private void PopulateDictionary(CodeContext/*!*/ context, string name, PythonTuple bases, IAttributesCollection vars) {
            PopulateSlot(Symbols.Doc, null);

            List<string> slots = GetSlots(vars);
            if (slots != null) {
                _slots = slots.ToArray();
                
                int index = _originalSlotCount;

                for (int i = 0; i < slots.Count; i++) {
                    string slotName = slots[i];
                    if (slotName.StartsWith("__") && !slotName.EndsWith("__")) {
                        slotName = "_" + name + slotName;
                    }

                    SymbolId id = SymbolTable.StringToId(slotName);

                    AddSlot(id, new ReflectedSlotProperty(slotName, name, i + index));
                }

                _originalSlotCount += slots.Count;
            }

            // check the slots to see if we're weak refable
            if (CheckForSlotWithDefault(context, bases, slots, "__weakref__", Symbols.WeakRef)) {
                _attrs |= PythonTypeAttributes.WeakReferencable;
                AddSlot(Symbols.WeakRef, new PythonTypeWeakRefSlot(this));
            }

            if (CheckForSlotWithDefault(context, bases, slots, "__dict__", Symbols.Dict)) {
                _attrs |= PythonTypeAttributes.HasDictionary;
                AddSlot(Symbols.Dict, new PythonTypeDictSlot(this));
            }

            object modName;
            if (context.Scope.TryLookupName(Symbols.Name, out modName)) {
                PopulateSlot(Symbols.Module, modName);
            }

            foreach (KeyValuePair<SymbolId, object> kvp in vars.SymbolAttributes) {
                PopulateSlot(kvp.Key, kvp.Value);
            }

            PythonTypeSlot val;
            if (_dict.TryGetValue(Symbols.NewInst, out val) && val is PythonFunction) {
                AddSlot(Symbols.NewInst, new staticmethod(val));
            }
        }

        private static bool CheckForSlotWithDefault(CodeContext context, PythonTuple bases, List<string> slots, string name, SymbolId siName) {
            bool hasSlot = true;
            if (slots != null && !slots.Contains(name)) {
                hasSlot = false;
                foreach (object pt in bases) {
                    PythonType dt = pt as PythonType;
                    PythonTypeSlot dummy;
                    if (dt != null && dt.TryLookupSlot(context, siName, out dummy)) {
                        hasSlot = true;
                    }
                }
            } else if (slots != null) {
                // check and see if we have 2
                if(bases.Count > 0) {
                    PythonType dt = bases[0] as PythonType;
                    PythonTypeSlot dummy;
                    if (dt != null && dt.TryLookupSlot(context, siName, out dummy)) {
                        throw PythonOps.TypeError(name + " slot disallowed: we already got one");
                    }
                }
            }
            return hasSlot;
        }

        public virtual Type __clrtype__() {
            return _underlyingSystemType;
        }

        private void PopulateSlot(SymbolId key, object value) {
            PythonTypeSlot pts = value as PythonTypeSlot;
            if (pts == null) {
                pts = new PythonTypeUserDescriptorSlot(value);
            }

            AddSlot(key, pts);
        }

        private static List<PythonType> GetBasesAsList(PythonTuple bases) {
            List<PythonType> newbs = new List<PythonType>();
            foreach (object typeObj in bases) {
                PythonType dt = typeObj as PythonType;
                if (dt == null) {
                    dt = ((OldClass)typeObj).TypeObject;
                }

                newbs.Add(dt);
            }

            return newbs;
        }

        private PythonTuple ValidateBases(PythonTuple bases) {
            PythonTuple newBases = PythonTypeOps.EnsureBaseType(bases);
            for (int i = 0; i < newBases.__len__(); i++) {
                for (int j = 0; j < newBases.__len__(); j++) {
                    if (i != j && newBases[i] == newBases[j]) {
                        OldClass oc = newBases[i] as OldClass;
                        if (oc != null) {
                            throw PythonOps.TypeError("duplicate base class {0}", oc.__name__);
                        } else {
                            throw PythonOps.TypeError("duplicate base class {0}", ((PythonType)newBases[i]).Name);
                        }
                    }
                }
            }
            return newBases;
        }

        private static void EnsureModule(CodeContext context, IAttributesCollection dict) {
            if (!dict.ContainsKey(Symbols.Module)) {
                object modName;
                if (context.Scope.TryLookupName(Symbols.Name, out modName)) {
                    dict[Symbols.Module] = modName;
                }
            }
        }
        
        #endregion

        #region System type initialization

        /// <summary>
        /// Initializes a PythonType that represents a standard .NET type.  The same .NET type
        /// can be shared with the Python type system.  For example object, string, int,
        /// etc... are all the same types.  
        /// </summary>
        private void InitializeSystemType() {
            IsSystemType = true;
            IsPythonType = PythonBinder.IsPythonType(_underlyingSystemType);
            _name = NameConverter.GetTypeName(_underlyingSystemType);
            AddSystemBases();
        }

        private void AddSystemBases() {
            List<PythonType> mro = new List<PythonType>();
            mro.Add(this);

            if (_underlyingSystemType.BaseType != null) {
                Type baseType;
                if (_underlyingSystemType == typeof(bool)) {
                    // bool inherits from int in python
                    baseType = typeof(int);
                } else if (_underlyingSystemType.BaseType == typeof(ValueType)) {
                    // hide ValueType, it doesn't exist in Python
                    baseType = typeof(object);
                } else {
                    baseType = _underlyingSystemType.BaseType;
                }
                _bases = new PythonType[] { GetPythonType(baseType) };

                Type curType = baseType;
                while (curType != null) {
                    Type newType;
                    if (TryReplaceExtensibleWithBase(curType, out newType)) {
                        mro.Add(DynamicHelpers.GetPythonTypeFromType(newType));
                    } else {
                        mro.Add(DynamicHelpers.GetPythonTypeFromType(curType));
                    }
                    curType = curType.BaseType;
                }

                if (!IsPythonType) {
                    AddSystemInterfaces(mro);
                }
            } else if (_underlyingSystemType.IsInterface) {
                // add interfaces to MRO & create bases list
                Type[] interfaces = _underlyingSystemType.GetInterfaces();
                PythonType[] bases = new PythonType[interfaces.Length];

                for (int i = 0; i < interfaces.Length; i++) {
                    Type iface = interfaces[i];
                    PythonType it = DynamicHelpers.GetPythonTypeFromType(iface);

                    mro.Add(it);
                    bases[i] = it;
                }
                _bases = bases;
            } else {
                _bases = new PythonType[0];
            }

            _resolutionOrder = mro;
        }

        private void AddSystemInterfaces(List<PythonType> mro) {
            if (_underlyingSystemType.IsArray) {
                return;
            } 

            Type[] interfaces = _underlyingSystemType.GetInterfaces();
            Dictionary<string, Type> methodMap = new Dictionary<string, Type>();
            bool hasExplicitIface = false;
            List<Type> nonCollidingInterfaces = new List<Type>(interfaces);
            
            foreach (Type iface in interfaces) {
                InterfaceMapping mapping = _underlyingSystemType.GetInterfaceMap(iface);
                
                // grab all the interface methods which would hide other members
                for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                    MethodInfo target = mapping.TargetMethods[i];

                    if (!target.IsPrivate) {
                        methodMap[target.Name] = null;
                    } else {
                        hasExplicitIface = true;
                    }
                }

                if (hasExplicitIface) {
                    for (int i = 0; i < mapping.TargetMethods.Length; i++) {
                        MethodInfo target = mapping.TargetMethods[i];
                        MethodInfo iTarget = mapping.InterfaceMethods[i];

                        // any methods which aren't explicit are picked up at the appropriate
                        // time earlier in the MRO so they can be ignored
                        if (target.IsPrivate) {
                            hasExplicitIface = true;

                            Type existing;
                            if (methodMap.TryGetValue(iTarget.Name, out existing)) {
                                if (existing != null) {
                                    // collision, multiple interfaces implement the same name, and
                                    // we're not hidden by another method.  remove both interfaces, 
                                    // but leave so future interfaces get removed
                                    nonCollidingInterfaces.Remove(iface);
                                    nonCollidingInterfaces.Remove(methodMap[iTarget.Name]);
                                    break;
                                }
                            } else {
                                // no collisions so far...
                                methodMap[iTarget.Name] = iface;
                            }
                        } 
                    }
                }
            }

            if (hasExplicitIface) {
                // add any non-colliding interfaces into the MRO
                foreach (Type t in nonCollidingInterfaces) {
                    Debug.Assert(t.IsInterface);

                    mro.Add(DynamicHelpers.GetPythonTypeFromType(t));
                }
            }
        }

        /// <summary>
        /// Creates a __new__ method for the type.  If the type defines interesting constructors
        /// then the __new__ method will call that.  Otherwise if it has only a single argless
        /// </summary>
        private void AddSystemConstructors() {
            if (typeof(Delegate).IsAssignableFrom(_underlyingSystemType)) {
                SetConstructor(
                    BuiltinFunction.MakeMethod(
                        _underlyingSystemType.Name,
                        typeof(DelegateOps).GetMethod("__new__"),
                        _underlyingSystemType,
                        FunctionType.Function | FunctionType.AlwaysVisible
                    )
                );
            } else if (!_underlyingSystemType.IsAbstract) {
                BuiltinFunction reflectedCtors = GetConstructors();
                if (reflectedCtors == null) {
                    return; // no ctors, no __new__
                }

                SetConstructor(reflectedCtors);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private BuiltinFunction GetConstructors() {
            Type type = _underlyingSystemType;
            string name = Name;

            return PythonTypeOps.GetConstructorFunction(type, name);
        }

        private void EnsureConstructor() {
            if (_ctor == null) {
                AddSystemConstructors();
                if (_ctor == null) {
                    throw PythonOps.TypeError(_underlyingSystemType.FullName + " does not define any public constructors.");
                }
            }
        }

        private void EnsureInstanceCtor() {
            if (_instanceCtor == null) {
                _instanceCtor = InstanceCreator.Make(this);
            }
        }

        #endregion

        #region Private implementation details

        internal void Initialize() {
            EnsureDict();
        }

        private void UpdateVersion() {
            foreach (WeakReference wr in SubTypes) {
                if (wr.IsAlive) {
                    ((PythonType)wr.Target).UpdateVersion();
                }
            }

            _lenSlot = null;
            _version = GetNextVersion();
        }

        /// <summary>
        /// This will return a unique integer for every version of every type in the system.
        /// This means that DynamicSite code can generate a check to see if it has the correct
        /// PythonType and version with a single integer compare.
        /// 
        /// TODO - This method and related code should fail gracefully on overflow.
        /// </summary>
        private static int GetNextVersion() {
            if (MasterVersion < 0) {
                throw new InvalidOperationException(IronPython.Resources.TooManyVersions);
            }
            return Interlocked.Increment(ref MasterVersion);
        }

        private void EnsureDict() {
            if (_dict == null) {
                Interlocked.CompareExchange<Dictionary<SymbolId, PythonTypeSlot>>(
                    ref _dict,
                    new Dictionary<SymbolId, PythonTypeSlot>(),
                    null);
            }
        }
      
        /// <summary>
        /// Internal helper function to add a subtype
        /// </summary>
        private void AddSubType(PythonType subtype) {
            if (_subtypes == null) {
                Interlocked.CompareExchange<List<WeakReference>>(ref _subtypes, new List<WeakReference>(), null);
            }

            lock (_subtypes) {
                _subtypes.Add(new WeakReference(subtype));
            }
        }

        private void RemoveSubType(PythonType subtype) {
            int i = 0;
            if (_subtypes != null) {
                lock (_subtypes) {
                    while (i < _subtypes.Count) {
                        if (!_subtypes[i].IsAlive || _subtypes[i].Target == subtype) {
                            _subtypes.RemoveAt(i);
                            continue;
                        }
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of weak references to all the subtypes of this class.  May return null
        /// if there are no subtypes of the class.
        /// </summary>
        private IList<WeakReference> SubTypes {
            get {
                if (_subtypes == null) return _emptyWeakRef;

                lock (_subtypes) return _subtypes.ToArray();
            }
        }

        [Flags]
        private enum PythonTypeAttributes {
            None = 0x00,
            Immutable = 0x01,
            SystemType = 0x02,
            IsPythonType = 0x04,
            WeakReferencable = 0x08,
            HasDictionary = 0x10,
        }

        #endregion

        #region IMembersList Members

        IList<object> IMembersList.GetMemberNames(CodeContext context) {
            IList<object> res = GetMemberNames(context);

            object[] arr = new object[res.Count];
            res.CopyTo(arr, 0);

            Array.Sort(arr);
            return arr;
        }

        #endregion        

        #region IWeakReferenceable Members

        WeakRefTracker IWeakReferenceable.GetWeakRef() {
            return _weakrefTracker;
        }

        bool IWeakReferenceable.SetWeakRef(WeakRefTracker value) {
            return Interlocked.CompareExchange<WeakRefTracker>(ref _weakrefTracker, value, null) == null;
        }

        void IWeakReferenceable.SetFinalizer(WeakRefTracker value) {
            _weakrefTracker = value;
        }

        #endregion

        #region IDynamicMetaObjectProvider Members

        [PythonHidden]
        public DynamicMetaObject/*!*/ GetMetaObject(Expression/*!*/ parameter) {
            return new Binding.MetaPythonType(parameter, BindingRestrictions.Empty, this);
        }

        #endregion
    }

    enum OptimizedGetKind {
        None,
        SlotDict,
        SlotOnly,
        UserSlot
    }

    class UserGetBase : FastGetBase {
        internal readonly int _version;
        internal Func<CallSite, object, CodeContext, object> _func;
        internal int _hitCount;

        public UserGetBase(PythonGetMemberBinder binder, int version)
            : base(binder) {
            _version = version;
        }

        internal bool ShouldUseNonOptimizedSite {
            get {
                return _hitCount < 100;
            }
        }
    }

    class GetAttributeDelegates : UserGetBase {
        private readonly string _name;
        private readonly PythonTypeSlot _getAttributeSlot;
        private readonly PythonTypeSlot _getAttrSlot;
        private readonly CallSite<Func<CallSite, object, CodeContext, object>> _site;
        private readonly SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>/*!*/ _storage;

        public GetAttributeDelegates(CallSite<Func<CallSite, object, CodeContext, object>>/*!*/ site, PythonGetMemberBinder/*!*/ binder, string/*!*/ name, int version, PythonTypeSlot/*!*/ getAttributeSlot, PythonTypeSlot/*!*/ getAttrSlot)
            : base(binder, version) {
            Assert.NotNull(site, binder, getAttributeSlot);

            _storage = new SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>>();
            _getAttributeSlot = getAttributeSlot;
            _getAttrSlot = getAttrSlot;
            _name = name;
            _site = site;
            _func = GetAttribute;
        }

        public object GetAttribute(CallSite site, object self, CodeContext context) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version) {
                if (Binder.IsNoThrow) {
                    return UserTypeOps.GetAttributeNoThrow(context, self, _name, _getAttributeSlot, _getAttrSlot, _storage);
                }

                return UserTypeOps.GetAttribute(context, self, _name, _getAttributeSlot, _getAttrSlot, _storage);
            }
            return Update(site, self, context);
        }
    }

    class GetMemberDelegates : UserGetBase {
        private readonly SymbolId _name;
        private readonly PythonTypeSlot _slot, _getattrSlot;
        private readonly SlotGetValue _slotFunc;

        public GetMemberDelegates(OptimizedGetKind getKind, PythonGetMemberBinder binder, SymbolId name, int version, PythonTypeSlot slot, PythonTypeSlot getattrSlot, SlotGetValue slotFunc)
            : base(binder, version) {
            _slot = slot;
            _name = name;
            _getattrSlot = getattrSlot;
            _slotFunc = slotFunc;
            switch (getKind) {
                case OptimizedGetKind.SlotDict: _func = SlotDict; break;
                case OptimizedGetKind.SlotOnly: _func = SlotOnly; break;
                case OptimizedGetKind.UserSlot: _func = UserSlot; break;
                default: throw new InvalidOperationException();
            }
        }

        public object SlotDict(CallSite site, object self, CodeContext context) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                _hitCount++;

                object res;
                if (ipo.Dict != null && ipo.Dict.TryGetValue(_name, out res)) {
                    return res;
                }

                if (_slot != null && _slot.TryGetValue(context, self, ipo.PythonType, out res)) {
                    return res;
                }

                if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, ipo.PythonType, out res)) {
                    return GetAttr(context, res);
                }

                return TypeError(ipo);
            }

            return Update(site, self, context);
        }

        public object SlotOnly(CallSite site, object self, CodeContext context) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                _hitCount++;

                object res;
                if (_slot != null && _slot.TryGetValue(context, self, ipo.PythonType, out res)) {
                    return res;
                }

                if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, ipo.PythonType, out res)) {
                    return GetAttr(context, res);
                }

                return TypeError(ipo);
            }

            return Update(site, self, context);
        }

        public object UserSlot(CallSite site, object self, CodeContext context) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                object res = _slotFunc(self);
                if (res != Uninitialized.Instance) {
                    return res;
                }

                if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, ipo.PythonType, out res)) {
                    return GetAttr(context, res);
                }

                return TypeError(ipo);
            }

            return Update(site, self, context);
        }

        private object GetAttr(CodeContext context, object res) {
            if (Binder.IsNoThrow) {
                try {
                    return PythonOps.CallWithContext(context, res, SymbolTable.IdToString(_name));
                } catch (MissingMemberException) {
                    return OperationFailed.Value;
                }
            } else {
                return PythonOps.CallWithContext(context, res, SymbolTable.IdToString(_name));
            }
        }

        private object TypeError(IPythonObject ipo) {
            if (Binder.IsNoThrow) {
                return OperationFailed.Value;
            }

            throw PythonOps.AttributeErrorForMissingAttribute(ipo.PythonType.Name, _name);
        }
    }

    enum OptimizedSetKind {
        None,
        SetAttr,
        UserSlot,
        SetDict,
        Error
    }
    
    class SetMemberDelegates<TValue> : FastSetBase<TValue> {
        private readonly SymbolId _name;
        private readonly PythonTypeSlot _slot;
        private readonly CallSite<Func<CallSite, object, TValue, object>> _site;
        private readonly SlotSetValue _slotFunc;
        private readonly CodeContext _context;
        private readonly OptimizedSetKind _kind;

        public SetMemberDelegates(CodeContext context, OptimizedSetKind kind, CallSite<Func<CallSite, object, TValue, object>> site, PythonSetMemberBinder binder, SymbolId name, int version, PythonTypeSlot slot, SlotSetValue slotFunc) 
            : base(binder, version) {
            _slot = slot;
            _name = name;
            _site = site;
            _slotFunc = slotFunc;
            _context = context;
            _kind = kind;
            switch (kind) {
                case OptimizedSetKind.SetAttr: _func = new Func<CallSite, object, TValue, object>(SetAttr); break;
                case OptimizedSetKind.UserSlot: _func = new Func<CallSite, object, TValue, object>(UserSlot); break;
                case OptimizedSetKind.SetDict: _func = new Func<CallSite, object, TValue, object>(SetDict); break;
                case OptimizedSetKind.Error: _func = new Func<CallSite, object, TValue, object>(Error); break;
            }
        }

        public OptimizedSetKind Kind {
            get {
                return _kind;
            }
        }

        public object SetAttr(CallSite site, object self, TValue value) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                _hitCount++;

                object res;
                if (_slot.TryGetValue(_context, self, ipo.PythonType, out res)) {
                    return PythonOps.CallWithContext(_context, res, SymbolTable.IdToString(_name), value);
                }

                return TypeError(ipo);
            }

            return Update(site, self, value);
        }

        public object SetDict(CallSite site, object self, TValue value) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                _hitCount++;

                UserTypeOps.SetDictionaryValue(ipo, _name, value);
                return null;
            }

            return Update(site, self, value);
        }

        public object Error(CallSite site, object self, TValue value) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version) {
                return TypeError(ipo);
            }

            return Update(site, self, value);
        }

        public object UserSlot(CallSite site, object self, TValue value) {
            IPythonObject ipo = self as IPythonObject;
            if (ipo != null && ipo.PythonType.Version == _version && ShouldUseNonOptimizedSite) {
                _hitCount++;

                _slotFunc(self, value);
                return null;
            }

            return Update(site, self, value);
        }

        private object TypeError(IPythonObject ipo) {
            throw PythonOps.AttributeErrorForMissingAttribute(ipo.PythonType.Name, _name);
        }
    }

    class SetMemberKey : IEquatable<SetMemberKey> {
        public readonly Type Type;
        public readonly string Name;

        public SetMemberKey(Type type, string name) {
            Type = type;
            Name = name;
        }

        #region IEquatable<SetMemberKey> Members

        public bool Equals(SetMemberKey other) {
            return Type == other.Type && Name == other.Name;
        }

        #endregion

        public override bool Equals(object obj) {
            SetMemberKey other = obj as SetMemberKey;
            if (other == null) {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode() {
            return Type.GetHashCode() ^ Name.GetHashCode();
        }
    }

}
