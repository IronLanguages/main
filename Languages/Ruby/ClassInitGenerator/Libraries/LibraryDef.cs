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
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Utils;

internal class LibraryDef {

    private bool Builtins { get { return _namespace == typeof(RubyClass).Namespace; } }

    public static readonly string/*!*/ TypeAction = "Action";
    public static readonly string/*!*/ TypeFunction = "Func";
    public static readonly string/*!*/ TypeDelegate = TypeName(typeof(Delegate));
    public static readonly string/*!*/ TypeRubyModule = TypeName(typeof(RubyModule));
    public static readonly string/*!*/ TypeRubyClass = TypeName(typeof(RubyClass));
    public static readonly string/*!*/ TypeActionOfRubyModule = TypeName(typeof(Action<RubyModule>));
    public static readonly string/*!*/ TypeLibraryInitializer = TypeName(typeof(LibraryInitializer));
    public static readonly string/*!*/ TypeRubyLibraryAttribute = TypeName(typeof(RubyLibraryAttribute));

    // TODO: store defs into LibraryDef
    private IDictionary<Type, ModuleDef> _moduleDefs = new SortedDictionary<Type, ModuleDef>(new TypeComparer()); // sorted to improve diff quality
    private IDictionary<Type, ModuleDef> _traits = new Dictionary<Type, ModuleDef>();
    private Dictionary<Type, string> _moduleRefs = new Dictionary<Type, string>();
    private Dictionary<Type, string> _classRefs = new Dictionary<Type, string>();

    public readonly string/*!*/ _namespace;
    public readonly string/*!*/ _initializerName;
    public bool AnyErrors;
    private IndentedTextWriter _output;

    #region Definitions

    public LibraryDef(string/*!*/ ns) {
        _namespace = ns;
        _initializerName = LibraryInitializer.GetTypeName(ns);
    }

    private class TypeRef {
        public ModuleDef Definition;
        public Type Type;
        public string RefName;

        public TypeRef(Type type) {
            Type = type;
            RefName = null;
            Definition = null;
        }

        public TypeRef(ModuleDef def, Type type, string refName) {
            Type = type;
            RefName = refName;
            Definition = def;
        }
    }

    private struct MixinRef {
        public readonly TypeRef/*!*/ Module;
        public readonly bool Copy;

        public MixinRef(TypeRef/*!*/ module, bool copy) {
            Module = module;
            Copy = copy;
        }
    }

    private enum ModuleKind {
        Module,
        Singleton,
        Class
    }

    private enum HiddenMethod {
        ClrInvisible,
        Undefined
    }

    private class ModuleDef {
        // Full Ruby name (e.g. "IronRuby::Clr::String")
        public string/*!*/ QualifiedName;

        // Name of the constant defined within declaring module (e.g. "String")
        public string/*!*/ SimpleName;

        // Full Ruby name with '::' replaced by '__' (e.g. "IronRuby__Clr__String").
        public string/*!*/ Id;

        // The trait only extends an existing CLR type, it doesn't define/reopen a Ruby module.
        public bool IsExtension;

        // Defines an Exception class.
        public bool IsException;

        // Aliases for the module (constants defined in the declaring type).
        public List<string>/*!*/ Aliases = new List<string>();

        // The type declaring Ruby methods:
        public Type/*!*/ Trait;

        // Type type being extended by trait (same as Trait for non-extension classes):
        public Type/*!*/ Extends;

        // If non-null, the declaring Ruby module is specified here and is different from the declaring CLR type.
        // Target declaration must be within the same library.
        public Type DefineIn;

        public IDictionary<string, MethodDef>/*!*/ InstanceMethods = new SortedDictionary<string, MethodDef>();
        public IDictionary<string, MethodDef>/*!*/ ClassMethods = new SortedDictionary<string, MethodDef>();
        public IDictionary<string, ConstantDef>/*!*/ Constants = new SortedDictionary<string, ConstantDef>();

        public IDictionary<string, HiddenMethod>/*!*/ HiddenInstanceMethods = new SortedDictionary<string, HiddenMethod>();
        public IDictionary<string, HiddenMethod>/*!*/ HiddenClassMethods = new SortedDictionary<string, HiddenMethod>();

        // { new_name -> old_name }
        public IDictionary<string, string>/*!*/ ClassMethodAliases = new SortedDictionary<string, string>();
        public IDictionary<string, string>/*!*/ InstanceMethodAliases = new SortedDictionary<string, string>();

        public List<MethodInfo>/*!*/ Factories = new List<MethodInfo>();

        public List<MixinRef>/*!*/ Mixins = new List<MixinRef>();

        public ModuleKind Kind;
        public bool HasCopyInclusions;
        
        public TypeRef Super;              // non-null for all classes except for object

        // Non-null for modules nested in other Ruby modules.
        // Doesn't influence dependency, constants for nested types are defined after all classes have been defined.
        public ModuleDef DeclaringModule;
        public string DeclaringTypeRef;

        // Whether the module is defined on Object, so we need to set a constant on Object.
        public bool IsGlobal;

        // Variable name where definition is stored.
        public string Reference;
        public string Definition;


        public string BuildConfig;
        public RubyCompatibility Compatibility;
        public ModuleRestrictions Restrictions;
        
        private int _dependencyOrder;

        /// <summary>
        /// Indicates the order that things should be generated so mixins/subclasses are always definied before
        /// they are referenced. Each module gets the value of the max of all the types it refers to
        /// plus one.
        /// </summary>
        public int DependencyOrder {
            get {
                if (_dependencyOrder == 0) {
                    if (Super != null && Super.Definition != null) {
                        _dependencyOrder = System.Math.Max(Super.Definition.DependencyOrder, _dependencyOrder);
                    }

                    foreach (MixinRef mixin in Mixins) {
                        if (mixin.Module.Definition != null) {
                            _dependencyOrder = System.Math.Max(mixin.Module.Definition.DependencyOrder, _dependencyOrder);
                        }
                    }
                    _dependencyOrder += 1;
                }
                return _dependencyOrder;
            }
        }

        public bool IsPrimitive {
            get {
                return !IsExtension && (
                       QualifiedName == RubyClass.MainSingletonName
                    || Extends == typeof(BasicObject)
                    || Extends == typeof(Kernel)
                    || Extends == typeof(Object)
                    || Extends == typeof(RubyClass)
                    || Extends == typeof(RubyModule)
                );
            }
        }

        public bool HasInstanceInitializer {
            get { 
                return InstanceMethods.Count > 0 || HiddenInstanceMethods.Count > 0 || InstanceMethodAliases.Count > 0
                    || HasCopyInclusions || IsPrimitive; 
            }
        }

        public bool HasConstantsInitializer {
            get { return Constants.Count > 0 || HasCopyInclusions; }
        }

        public bool HasClassInitializer {
            get { 
                return ClassMethods.Count > 0 || HiddenClassMethods.Count > 0 || ClassMethodAliases.Count > 0 
                    || HasCopyInclusions || IsPrimitive; 
            }
        }

        public const string/*!*/ BasicObjectClassRef = "Context.BasicObjectClass";
        public const string/*!*/ ObjectClassRef = "Context.ObjectClass";
        public const string/*!*/ KernelModuleRef = "Context.KernelModule";
        public const string/*!*/ ModuleClassRef = "Context.ModuleClass";
        public const string/*!*/ ClassClassRef = "Context.ClassClass";

        internal string GetReference(ref int defVariableId) {
            if (Reference == null) {
                if (Extends == typeof(BasicObject)) {
                    Reference = BasicObjectClassRef;
                } else if (Extends == typeof(Object)) {
                    Reference = ObjectClassRef;
                } else if (Extends == typeof(Kernel)) {
                    Reference = KernelModuleRef;
                } else if (Extends == typeof(RubyModule)) {
                    Reference = ModuleClassRef;
                } else if (Extends == typeof(RubyClass)) {
                    Reference = ClassClassRef;
                } else {
                    Definition = Reference = "def" + defVariableId++;
                }
            }

            return Reference;
        }

        public override string/*!*/ ToString() {
            return QualifiedName;
        }

        internal string/*!*/ GetInitializerDelegates() {
            return
                (HasInstanceInitializer ? "Load" + Id + "_Instance" : "null") + ", " +
                (HasClassInitializer ? "Load" + Id + "_Class" : "null") + ", " +
                (HasConstantsInitializer ? "Load" + Id + "_Constants" : "null");
        }

        internal string/*!*/ GetModuleRestrictions() {
            return "0x" + Restrictions.ToString("X");
        }
    }

    private class MethodDef {
        public string/*!*/ Name;
        public List<MethodInfo>/*!*/ Overloads = new List<MethodInfo>();
        public string BuildConfig;
        public RubyCompatibility Compatibility;
        public RubyMethodAttributes/*!*/ Attributes;

        public bool IsRuleGenerator {
            get {
                return Overloads.Count == 1 && Overloads[0].ReturnType == typeof(RuleGenerator) && Overloads[0].GetParameters().Length == 0;
            }
        }

        public override string/*!*/ ToString() {
            return Name;
        }
    }

    private class ConstantDef {
        public readonly string/*!*/ Name;
        public readonly MemberInfo/*!*/ Member;
        public readonly string/*!*/ BuildConfig;

        public ConstantDef(string/*!*/ name, MemberInfo/*!*/ member, string/*!*/ buildConfig) {
            Name = name;
            Member = member;
            BuildConfig = buildConfig;
        }
    }

    #endregion

    private void WriteRubyCompatibilityCheck(RubyCompatibility compatibility) {
        if (compatibility != RubyCompatibility.Default) {
            _output.WriteLine("if (Context.RubyOptions.Compatibility >= RubyCompatibility.{0}) {{", compatibility.ToString());
        }
    }

    private void WriteRubyCompatibilityCheckEnd(RubyCompatibility compatibility) {
        if (compatibility != RubyCompatibility.Default) {
            _output.WriteLine("}");
        }
    }

    #region Reflection

    public void ReflectTypes(Type[]/*!*/ allTypes) {

        foreach (Type trait in allTypes) {
            if (trait.Namespace == null || !trait.Namespace.StartsWith(_namespace)) {
                continue;
            }

            object[] attrs;

            try {
                attrs = trait.GetCustomAttributes(typeof(RubyModuleAttribute), false);
            } catch (Exception e) {
                LogError("{0}: Invalid attribute value on type: {1}", trait.FullName, e.Message);
                continue;
            }

            foreach (RubyModuleAttribute module in attrs) {
                ModuleDef def = new ModuleDef();

                RubySingletonAttribute singleton = module as RubySingletonAttribute;
                RubyClassAttribute cls = module as RubyClassAttribute;

                if (cls != null) {
                    def.Kind = ModuleKind.Class;
                    def.IsException = module is RubyExceptionAttribute;
                } else if (singleton != null) {
                    def.Kind = ModuleKind.Singleton;
                    if (module.Extends != null) {
                        LogError("{0}: Singleton cannot Extend a type", trait.FullName);
                        module.Extends = null;
                    }
                } else {
                    def.Kind = ModuleKind.Module;
                }

                if (trait == module.Extends) {
                    LogError("Module cannot extend itself: {0}", trait);
                    continue;
                }

                if (module.Extends != null && module.Name == null) {
                    // extends a CLR type or an existing Ruby library class/module:
                    def.IsExtension = true;
                    def.SimpleName = module.Extends.Name.Replace(ReflectionUtils.GenericArityDelimiter, '_');
                    def.QualifiedName = null;
                    def.DeclaringModule = null;
                    def.IsGlobal = false;
                } else {
                    def.IsExtension = false;
                    def.SimpleName = module.Name ?? trait.Name;
                    def.QualifiedName = null;   // to be filled in later
                    def.DeclaringModule = null; // to be corrected later for nested modules
                    def.IsGlobal = true;        // to be corrected later for nested modules
                }

                def.Trait = trait;
                def.Extends = (module.Extends != null) ? module.Extends : trait;
                def.DefineIn = module.DefineIn;
                def.BuildConfig = module.BuildConfig;
                def.Compatibility = module.Compatibility;
                def.Restrictions = module.GetRestrictions(Builtins);

                def.Super = null;
                if (cls != null && def.Extends != typeof(BasicObject) && !def.Extends.IsInterface) {
                    if (cls != null && cls.Inherits != null) {
                        def.Super = new TypeRef(cls.Inherits);
                    } else if (!def.IsExtension) {
                        def.Super = new TypeRef(def.Extends.BaseType);
                    }
                }

                def.HasCopyInclusions = false;
                foreach (IncludesAttribute includes in trait.GetCustomAttributes(typeof(IncludesAttribute), false)) {
                    foreach (Type type in includes.Types) {
                        def.Mixins.Add(new MixinRef(new TypeRef(type), includes.Copy));
                    }
                    def.HasCopyInclusions |= includes.Copy;
                }

                _moduleDefs.Add(def.Extends, def);
                _traits.Add(def.Trait, def);

                // added Ruby methods and constants:
                ReflectMethods(def);
                ReflectFieldConstants(def);
                ReflectAliases(def);

                // hidden CLR methods:
                foreach (HideMethodAttribute method in trait.GetCustomAttributes(typeof(HideMethodAttribute), false)) {
                    var dict = (method.IsStatic) ? def.HiddenClassMethods : def.HiddenInstanceMethods;
                    if (dict.ContainsKey(method.Name)) {
                        LogError("Method {0} is already hidden/removed", method.Name);
                    } else {
                        dict.Add(method.Name, HiddenMethod.ClrInvisible);
                    }
                }

                // undefined methods:
                foreach (UndefineMethodAttribute method in trait.GetCustomAttributes(typeof(UndefineMethodAttribute), false)) {
                    var dict = (method.IsStatic) ? def.HiddenClassMethods : def.HiddenInstanceMethods;
                    if (dict.ContainsKey(method.Name)) {
                        LogError("Method {0} is already hidden/removed", method.Name);
                    } else {
                        dict.Add(method.Name, HiddenMethod.Undefined);
                    }
                }

                // aliased methods:
                foreach (AliasMethodAttribute method in trait.GetCustomAttributes(typeof(AliasMethodAttribute), false)) {
                    var aliasDict = (method.IsStatic) ? def.ClassMethodAliases : def.InstanceMethodAliases;
                    var hiddenDict = (method.IsStatic) ? def.HiddenClassMethods : def.HiddenInstanceMethods;

                    if (hiddenDict.ContainsKey(method.NewName)) {
                        LogError("Cannot alias hidden/removed method {0}", method.NewName);
                    } else if (aliasDict.ContainsKey(method.NewName)) {
                        LogError("Duplicate method alias {0} {1}", method.NewName, method.OldName);
                    } else {
                        aliasDict.Add(method.NewName, method.OldName);
                    }
                }
            }
        }

        int defVariableId = 1;

        // declaring modules, build configurations:
        foreach (ModuleDef def in _moduleDefs.Values) {
            if (!def.IsExtension) {
                if (def.Extends.IsGenericTypeDefinition) {
                    LogError("Only extension modules or classes can be generic type definitions '{0}'", def.QualifiedName);
                }

                // finds the inner most Ruby module def containing this module def:
                ModuleDef declaringDef = GetDeclaringModuleDef(def);
                if (declaringDef != null) {
                    def.DeclaringModule = declaringDef;
                    def.IsGlobal = false;

                    // inherits build config:
                    if (declaringDef.BuildConfig != null) {
                        if (def.BuildConfig != null) {
                            def.BuildConfig = declaringDef.BuildConfig + " && " + def.BuildConfig;
                        } else {
                            def.BuildConfig = declaringDef.BuildConfig;
                        }
                    }

                    if (declaringDef.Compatibility != RubyCompatibility.Default) {
                        def.Compatibility = (RubyCompatibility)Math.Max((int)declaringDef.Compatibility, (int)def.Compatibility);
                    }

                    // we will need a reference for setting the constant:
                    def.DeclaringTypeRef = declaringDef.GetReference(ref defVariableId);
                    def.GetReference(ref defVariableId);
                }
            }

            if (def.Kind == ModuleKind.Singleton) {
                // we need to refer to the singleton object returned from singelton factory:
                def.GetReference(ref defVariableId);
            }
        }

        // qualified names, ids:
        foreach (ModuleDef def in _moduleDefs.Values) {
            SetQualifiedName(def);

            if (def.Kind == ModuleKind.Singleton) {
                def.Id = "__Singleton_" + def.QualifiedName;
            } else {
                def.Id = def.QualifiedName.Replace(':', '_');
            }
        }

        // wire-up supers and mixins:
        foreach (ModuleDef def in _moduleDefs.Values) {
            if (def.Super != null) {
                ModuleDef superDef;
                if (_moduleDefs.TryGetValue(def.Super.Type, out superDef)) {

                    // define inheritance relationship:
                    def.Super.Definition = superDef;
                    def.Super.RefName = superDef.GetReference(ref defVariableId);

                } else {

                    // define a class ref-variable for the type of the super class:
                    def.Super.RefName = MakeClassReference(def.Super.Type);

                }
            } else if (!def.IsExtension && def.Kind == ModuleKind.Class && def.Extends != typeof(BasicObject)) {
                LogError("Missing super type for type '{0}'", def.QualifiedName);
            }

            // wire-up mixins:
            foreach (MixinRef mixin in def.Mixins) {
                ModuleDef mixinDef;
                if (_moduleDefs.TryGetValue(mixin.Module.Type, out mixinDef)) {

                    // define mixin relationship:
                    mixin.Module.Definition = mixinDef;
                    if (!mixin.Copy) {
                        mixin.Module.RefName = mixinDef.GetReference(ref defVariableId);
                    }

                } else if (!mixin.Copy) {
                    // define a module ref-variable for the type of the mixin:
                    mixin.Module.RefName = MakeModuleReference(mixin.Module.Type);
                } else {
                    LogError("Cannot copy-include a mixin not defined in this library ('{0}' includes '{1}')", 
                        def.QualifiedName, mixin.Module.Type
                    );
                }
            }
        }

        // TODO: checks
        // - loops in copy-inclusion
    }

    private void SetQualifiedName(ModuleDef/*!*/ def) {
        if (def.QualifiedName == null) {
            if (def.IsExtension) {
                def.QualifiedName = RubyContext.GetQualifiedNameNoLock(def.Extends, null, true);
            } else if (def.DeclaringModule == null) {
                def.QualifiedName = def.SimpleName;
            } else {
                SetQualifiedName(def.DeclaringModule);
                def.QualifiedName = def.DeclaringModule.QualifiedName + "::" + def.SimpleName;
            }
        }
    }

    private ModuleDef GetDeclaringModuleDef(ModuleDef/*!*/ def) {
        ModuleDef declaringDef;
        if (def.DefineIn != null) {
            if (_traits.TryGetValue(def.DefineIn, out declaringDef)) {
                return declaringDef;
            }
            LogError("Declaring type specified by DeclareIn parameter '{0}' is not Ruby module definition or is in different library", def.DefineIn);
        }

        Type declaringType = def.Trait.DeclaringType;
        while (declaringType != null) {
            if (_traits.TryGetValue(declaringType, out declaringDef)) {
                return declaringDef;
            } else {
                declaringType = declaringType.DeclaringType;
            }
        }
        return null;
    }

    private string/*!*/ MakeModuleReference(Type/*!*/ typeRef) {
        string refVariable;

        if (!_moduleRefs.TryGetValue(typeRef, out refVariable)) {
            refVariable = "moduleRef" + _moduleRefs.Count;
            _moduleRefs.Add(typeRef, refVariable);
        }

        return refVariable;
    }

    private string/*!*/ MakeClassReference(Type/*!*/ typeRef) {
        string refVariable;

        if (!_classRefs.TryGetValue(typeRef, out refVariable)) {
            refVariable = "classRef" + _classRefs.Count;
            _classRefs.Add(typeRef, refVariable);
        }

        return refVariable;
    }

    private void ReflectMethods(ModuleDef/*!*/ moduleDef) {
        Debug.Assert(moduleDef.Trait != null);

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        foreach (MethodInfo method in moduleDef.Trait.GetMethods(flags)) {
            object[] attrs = method.GetCustomAttributes(typeof(RubyMethodAttribute), false);
            if (attrs.Length > 0) {
                if (!RequireStatic(method)) continue;
                foreach (RubyMethodAttribute attr in attrs) {
                    MethodDef def;

                    IDictionary<string, MethodDef> methods =
                        ((attr.MethodAttributes & RubyMethodAttributes.Instance) != 0) ? moduleDef.InstanceMethods : moduleDef.ClassMethods;

                    if (!methods.TryGetValue(attr.Name, out def)) {
                        def = new MethodDef();
                        def.Name = attr.Name;
                        def.Attributes = attr.MethodAttributes;

                        if (Builtins) {
                            def.Attributes |= RubyMethodAttributes.NoEvent;
                        }

                        def.BuildConfig = attr.BuildConfig;
                        def.Compatibility = attr.Compatibility;

                        methods.Add(attr.Name, def);
                    }
                    def.Overloads.Add(method);
                }
            }

            if (method.IsDefined(typeof(RubyConstructorAttribute), false)) {
                if (!RequireStatic(method)) continue;
                moduleDef.Factories.Add(method);
            }

            if (method.IsDefined(typeof(RubyConstantAttribute), false)) {
                if (!RequireStatic(method)) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableFrom(typeof(RubyModule)) ||
                    parameters[0].Attributes != ParameterAttributes.None) {
                    LogError("Method that defines a constant must take a single parameter of type RubyModule: '{0}'",
                        ReflectionUtils.FormatSignature(new StringBuilder(), method));
                }

                ReflectConstants(moduleDef, method);
            }
        }

        VerifyMethods(moduleDef);
    }

    private void VerifyMethods(ModuleDef/*!*/ moduleDef) {
        foreach (var methodDef in moduleDef.InstanceMethods.Values) {
            VerifyMethod(moduleDef, methodDef.Overloads, methodDef, true);
        }

        foreach (var methodDef in moduleDef.ClassMethods.Values) {
            VerifyMethod(moduleDef, methodDef.Overloads, methodDef, false);
        }

        VerifyFactory(moduleDef, moduleDef.Factories);
    }

    private void VerifyFactory(ModuleDef/*!*/ moduleDef, IList<MethodInfo>/*!*/ overloads) {
        VerifyMethod(moduleDef, overloads, null, false);
    }

    private void VerifyMethod(ModuleDef/*!*/ moduleDef, IList<MethodInfo>/*!*/ overloads, MethodDef methodDef, bool isInstance) {
        foreach (var overload in overloads) {
            var parameterInfos = overload.GetParameters();

            if (overload.ReturnType == typeof(RuleGenerator)) {
                if (parameterInfos.Length != 0) {
                    LogMethodError("RuleGenerator must be parameterless", methodDef, overload);
                }

                if (overloads.Count != 1) {
                    LogMethodError("RuleGenerator must have no overloads", methodDef, overload);
                }
            } else {

                bool hasContext = false;
                bool hasBlock = false;
                bool hasSelf = false;
                int storageCount = 0;
                for (int i = 0; i < parameterInfos.Length; i++) {
                    if (parameterInfos[i].ParameterType.IsByRef) {
                        LogMethodError("has ref/out parameter", methodDef, overload);
                    }

                    var type = parameterInfos[i].ParameterType;

                    if (type.IsSubclassOf(typeof(RubyCallSiteStorage))) {
                        if (hasSelf || hasContext || hasBlock) {
                            LogMethodError("RubyCallSiteStorage must precede all other parameters", methodDef, overload);
                        }
                        storageCount++;
                    } else if (type == typeof(RubyContext) || type == typeof(RubyScope)) {
                        if (hasContext) {
                            LogMethodError("has multiple context parameters", methodDef, overload);
                        }

                        if (i - storageCount != 0) {
                            LogMethodError("Context parameter must be the first parameter following optional RubyCallSiteStorage", methodDef, overload);
                        }

                        hasContext = true;
                    } else if (type == typeof(BlockParam)) {
                        if (hasBlock) {
                            LogMethodError("has multiple block parameters.", methodDef, overload);
                        }

                        // TODO: sites

                        if (hasContext && i - storageCount != 1) {
                            LogMethodError("Block parameter must be the first parameter after context parameter", methodDef, overload);
                        }

                        if (!hasContext && i - storageCount != 0) {
                            LogMethodError("Block parameter must be the first parameter following optional RubyCallSiteStorage", methodDef, overload);
                        }

                        // TODO: we should detect a call to the BlockParam.Yield:
                        //if (overload.ReturnType != typeof(object)) {
                        //    LogMethodWarning("A method that yields to a block must return Object.", methodDef, overload);
                        //}

                        hasBlock = true;
                    } else if (!hasSelf) {
                        // self
                        if (isInstance) {
                            Debug.Assert(methodDef != null);

                            // TODO:
                            //if (parameterInfos[i].Name != "self") {
                            //    LogMethodWarning("Self parameter should be named 'self'.", methodDef, overload);
                            //}
                        } else {
                            Type requiredType;

                            switch (moduleDef.Kind) {
                                case ModuleKind.Module: requiredType = typeof(RubyModule); break;
                                case ModuleKind.Singleton:
                                case ModuleKind.Class: requiredType = typeof(RubyClass); break;
                                default: throw Assert.Unreachable;
                            }

                            if (!type.IsAssignableFrom(requiredType)) {
                                LogMethodError("Invalid type of self parameter: it must be assignable from '{0}'.", methodDef, overload, requiredType);
                            }

                            if (parameterInfos[i].Name != "self") {
                                LogMethodError("Self parameter should be named 'self'.", methodDef, overload);
                            }
                        }

                        hasSelf = true;
                    }
                }

                if (!hasSelf) {
                    LogMethodError("Missing self parameter", methodDef, overload);
                }
            }
        }
    }

    private void ReflectFieldConstants(ModuleDef/*!*/ moduleDef) {
        Debug.Assert(moduleDef.Trait != null);

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        foreach (FieldInfo field in moduleDef.Trait.GetFields(flags)) {
            ReflectConstants(moduleDef, field);
        }
    }

    private void ReflectConstants(ModuleDef/*!*/ moduleDef, MemberInfo/*!*/ member) {
        Debug.Assert(member is MethodInfo || member is FieldInfo);

        foreach (RubyConstantAttribute attr in member.GetCustomAttributes(typeof(RubyConstantAttribute), false)) {
            string name = attr.Name ?? member.Name;

            ConstantDef existing;
            if (moduleDef.Constants.TryGetValue(name, out existing)) {
                LogError("Constant '{0}' defined by multiple members: '{1}' and '{2}'", name, existing.Member.Name, member.Name);
                continue;
            }

            if (String.IsNullOrEmpty(name) || name[0] < 'A' && name[0] > 'Z') {
                LogError("Invalid constant name: '{0}' ({1}::{2})", name, existing.Member.DeclaringType.FullName, existing.Member.Name);
                continue;
            }

            Debug.Assert(attr.Compatibility == RubyCompatibility.Default);
            moduleDef.Constants.Add(name, new ConstantDef(name, member, attr.BuildConfig));
        }
    }

    private void ReflectAliases(ModuleDef/*!*/ moduleDef) {
        // TODO: check for duplicates (in general in the scope of dll)
        foreach (RubyConstantAttribute attr in moduleDef.Trait.GetCustomAttributes(typeof(RubyConstantAttribute), false)) {
            if (attr.Name == null) {
                LogError("Constant alias for module/class must have a name (type '{0}')", moduleDef.Trait.FullName);
            }

            if (moduleDef.Aliases.IndexOf(attr.Name) != -1) {
                LogError("Duplicate module/class alias '{0}' (type '{1}')", attr.Name, moduleDef.Trait.FullName);
            }

            moduleDef.Aliases.Add(attr.Name);
        }
    }

    #endregion

    private bool RequireStatic(MethodBase/*!*/ method) {
        if (!method.IsStatic) {
            Console.Error.WriteLine("Instance methods not supported (method '{0}.{1}')", TypeName(method.DeclaringType), method.Name);
            AnyErrors = true;
            return false;
        }
        return true;
    }

    private void LogError(string/*!*/ message, params object[] args) {
        Console.Error.Write("Error: ");
        Console.Error.WriteLine(message, args);
        AnyErrors = true;
    }

    private void LogWarning(string/*!*/ message, params object[] args) {
        Console.Error.Write("Warning: ");
        Console.Error.WriteLine(message, args);
    }

    private void LogMethodError(string/*!*/ message, MethodDef methodDef, MethodBase/*!*/ overload, params object[] args) {
        string methodName = (methodDef != null) ? "method \"" + methodDef.Name + '"' : "factory";
        Console.Error.WriteLine("Error: {0}: {1}", methodName, String.Format(message, args));
        Console.Error.WriteLine("       overload: {0}", ReflectionUtils.FormatSignature(new StringBuilder(), overload));
        AnyErrors = true;
    }

    private void LogMethodWarning(string/*!*/ message, MethodDef methodDef, MethodBase/*!*/ overload, params object[] args) {
        string methodName = (methodDef != null) ? "method \"" + methodDef.Name + '"' : "factory";
        Console.Error.WriteLine("Warning: {0}: {1}", methodName, String.Format(message, args));
        Console.Error.WriteLine("         overload: {0}", ReflectionUtils.FormatSignature(new StringBuilder(), overload));
    }

    #region Code Generation

    public void GenerateCode(IndentedTextWriter/*!*/ output) {
        _output = output;

        _output.WriteLine("namespace {0} {{", _namespace);
        _output.Indent++;
        _output.WriteLine("using System;");
        _output.WriteLine("using Microsoft.Scripting.Utils;");
        _output.WriteLine("using System.Runtime.InteropServices;");
        _output.WriteLine();

        _output.WriteLine("public sealed class {0} : {1} {{", _initializerName, TypeLibraryInitializer);
        _output.Indent++;

        _output.WriteLine("protected override void LoadModules() {");
        _output.Indent++;

        GenerateModuleRegistrations();

        _output.Indent--;
        _output.WriteLine("}");
        _output.WriteLine();

        GenerateTraitInitializations(_moduleDefs);
        GenerateExceptionFactories(_moduleDefs);

        _output.Indent--;
        _output.WriteLine("}");

        _output.Indent--;
        _output.WriteLine("}");
        _output.WriteLine();
    }

    private void GenerateModuleRegistrations() {

        // primitives:
        if (Builtins) {
            _output.WriteLine("Context.RegisterPrimitives(");
            _output.Indent++;

            _output.WriteLine("Load{0}_Instance,", RubyClass.MainSingletonName);

            _output.WriteLine(_moduleDefs[typeof(BasicObject)].GetInitializerDelegates() + ",");
            _output.WriteLine(_moduleDefs[typeof(Kernel)].GetInitializerDelegates() + ",");
            _output.WriteLine(_moduleDefs[typeof(Object)].GetInitializerDelegates() + ",");
            _output.WriteLine(_moduleDefs[typeof(RubyModule)].GetInitializerDelegates() + ",");
            _output.WriteLine(_moduleDefs[typeof(RubyClass)].GetInitializerDelegates());

            _output.Indent--;
            _output.WriteLine(");");
        }

        // generate references:
        foreach (KeyValuePair<Type, string> moduleRef in _moduleRefs) {
            _output.WriteLine("{0} {1} = GetModule(typeof({2}));", TypeRubyModule, moduleRef.Value, TypeName(moduleRef.Key));
        }

        foreach (KeyValuePair<Type, string> classRef in _classRefs) {
            _output.WriteLine("{0} {1} = GetClass(typeof({2}));", TypeRubyClass, classRef.Value, TypeName(classRef.Key));
        }

        _output.WriteLine();
        _output.WriteLine();

        // We need to generate these in proper dependency order
        // also, we want the sort to be stable to improve the
        // quality of the resulting diff of the generated code.
        ModuleDef[] worklist = new ModuleDef[_moduleDefs.Count];
        _moduleDefs.Values.CopyTo(worklist, 0);
        Array.Sort(worklist, delegate(ModuleDef x, ModuleDef y) {
            // if the dependency order is the same, fall back to the type name
            if (x.DependencyOrder == y.DependencyOrder) {
                return x.QualifiedName.CompareTo(y.QualifiedName);
            }
            return x.DependencyOrder - y.DependencyOrder;
        });

        // classes:
        foreach (ModuleDef def in worklist) {
            GenerateModuleRegistration(def);
        }

        // add constants for non-global nested modules, adds aliases:
        foreach (ModuleDef def in worklist) {
            if (def.IsGlobal && def.Aliases.Count > 0 || def.DeclaringTypeRef != null) {
                if (def.BuildConfig != null) {
                    _output.WriteLine("#if " + def.BuildConfig);
                }
                WriteRubyCompatibilityCheck(def.Compatibility);

                if (def.IsGlobal) {
                    GenerateAliases(def, ModuleDef.ObjectClassRef);
                } else if (def.DeclaringTypeRef != null) {
                    GenerateAliases(def, def.DeclaringTypeRef);
                    GenerateSetConstant(def.DeclaringTypeRef, def.SimpleName, def.Reference);
                }

                WriteRubyCompatibilityCheckEnd(def.Compatibility);
                if (def.BuildConfig != null) {
                    _output.WriteLine("#endif");
                }
            }
        }
    }

    private void GenerateSetConstant(string/*!*/ owner, string/*!*/ name, string/*!*/ expression) {
        _output.WriteLine("Set{3}Constant({0}, \"{1}\", {2});", owner, name, expression, Builtins ? "Builtin" : null);
    }

    private void GenerateAliases(ModuleDef/*!*/ def, string/*!*/ ownerRef) {
        foreach (string alias in def.Aliases) {
            GenerateSetConstant(ownerRef, alias, def.Reference);
        }
    }

    private void GenerateModuleRegistration(ModuleDef/*!*/ def) {
        if (def.IsPrimitive) {
            _output.WriteLine("// Skipped primitive: {0}", def.QualifiedName);
            return;
        }

        if (def.BuildConfig != null) {
            _output.WriteLine("#if " + def.BuildConfig);
        }
        WriteRubyCompatibilityCheck(def.Compatibility);


        switch (def.Kind) {
            case ModuleKind.Class:
                if (def.Definition != null) {
                    _output.Write("{0} {1} = ", TypeRubyClass, def.Definition);
                }

                if (Builtins && !def.IsExtension) {
                    if (def.QualifiedName == "NilClass") {
                        _output.Write("Context.NilClass = ");
                    } else if (def.QualifiedName == "TrueClass") {
                        _output.Write("Context.TrueClass = ");
                    } else if (def.QualifiedName == "FalseClass") {
                        _output.Write("Context.FalseClass = ");
                    } else if (def.QualifiedName == "Exception") {
                        _output.Write("Context.ExceptionClass = ");
                    } else if (def.QualifiedName == "StandardError") {
                        _output.Write("Context.StandardErrorClass = ");
                    }
                }

#if TODO
                string extensionType = "null";
                if (def.Trait != def.Extends) {
                    extensionType = string.Format("typeof({0})", TypeName(def.Trait));
                }
                pass:    extensionType,
#endif

                if (def.IsExtension) {
                    _output.Write("ExtendClass(typeof({0}), {1}, {2}, {3}, ",
                        TypeName(def.Extends),
                        def.GetModuleRestrictions(),
                        def.Super != null ? def.Super.RefName : "null",
                        def.GetInitializerDelegates()
                    );
                } else {
                    _output.Write("Define{0}Class(\"{1}\", typeof({2}), {3}, {4}, {5}, ",
                        def.IsGlobal ? "Global" : "",
                        def.QualifiedName,
                        TypeName(def.Extends),
                        def.GetModuleRestrictions(),
                        def.Super.RefName,
                        def.GetInitializerDelegates()
                    );
                }

                GenerateInclusions(def, true);

                if (def.Factories.Count > 0) {
                    _output.WriteLine(", ");
                    GenerateDelegatesListCreation(def.Factories);
                } else if (def.IsException) {
                    _output.WriteLine(", ");
                    GenerateExceptionFactoryDelegateList(def);
                }

                _output.WriteLine(");");
                break;

            case ModuleKind.Module:
                if (def.Definition != null) {
                    _output.Write("{0} {1} = ", TypeRubyModule, def.Definition);
                }

                if (def.IsExtension) {
                    _output.Write("ExtendModule(typeof({0}), {1}, {2}, ", 
                        TypeName(def.Extends),
                        def.GetModuleRestrictions(),
                        def.GetInitializerDelegates()
                    );
                } else {
                    _output.Write("Define{0}Module(\"{1}\", typeof({2}), {3}, {4}, ",
                        def.IsGlobal ? "Global" : "",
                        def.QualifiedName,
                        TypeName(def.Extends),
                        def.GetModuleRestrictions(),
                        def.GetInitializerDelegates()
                    );
                }

                GenerateInclusions(def, false);

                _output.WriteLine(");");
                break;

            case ModuleKind.Singleton:
                if (def.Definition != null) {
                    _output.Write("object {0} = ", def.Definition);
                }

                _output.Write("DefineSingleton({0}, ", def.GetInitializerDelegates());

                GenerateInclusions(def, false);

                _output.WriteLine(");");
                break;

            default:
                throw Assert.Unreachable;
        }

        WriteRubyCompatibilityCheckEnd(def.Compatibility);
        if (def.BuildConfig != null) {
            _output.WriteLine("#endif");
        }
    }

    private void GenerateInclusions(ModuleDef/*!*/ def, bool makeArray) {
        List<string> mixinRefs = new List<string>();
        AddMixinRefsRecursive(mixinRefs, def);

        if (mixinRefs.Count > 0) {
            if (makeArray) {
                _output.Write("new {0}[] {{", TypeRubyModule);
            }
            bool first = true;
            foreach (string mixinRef in mixinRefs) {
                if (first) {
                    first = false;
                } else {
                    _output.Write(", ");
                }
                _output.Write(mixinRef);
            }
            if (makeArray) {
                _output.Write("}");
            }
        } else {
            _output.Write("{0}.EmptyArray", TypeRubyModule);
        }
    }

    private void AddMixinRefsRecursive(List<string>/*!*/ mixinRefs, ModuleDef/*!*/ def) {
        foreach (MixinRef mixin in def.Mixins) {
            if (mixin.Copy) {
                AddMixinRefsRecursive(mixinRefs, mixin.Module.Definition);
            } else {
                mixinRefs.Add(mixin.Module.RefName);
            }
        }
    }

    private void GenerateTraitInitializations(IDictionary<Type, ModuleDef>/*!*/ traitDefs) {
        // Sort the items before we generate them to improve diff quality
        ModuleDef[] worklist = new ModuleDef[traitDefs.Count];
        traitDefs.Values.CopyTo(worklist, 0);
        Array.Sort(worklist, (x, y) => x.QualifiedName.CompareTo(y.QualifiedName));

        foreach (ModuleDef moduleDef in worklist) {
            GenerateTraitInitialization(moduleDef);
        }
    }

    private void GenerateTraitInitialization(ModuleDef/*!*/ moduleDef) {
        GenerateConstantsInitialization(moduleDef);
        GenerateTraitInitialization(moduleDef, true);
        GenerateTraitInitialization(moduleDef, false);
    }

    private void GenerateTraitInitialization(ModuleDef/*!*/ moduleDef, bool isInstance) {
        if (isInstance ? moduleDef.HasInstanceInitializer : moduleDef.HasClassInitializer) {
            if (moduleDef.BuildConfig != null) {
                _output.WriteLine("#if " + moduleDef.BuildConfig);
            }
            _output.WriteLine("private static void Load{0}_{2}({1}/*!*/ module) {{", moduleDef.Id, TypeRubyModule, isInstance ? "Instance" : "Class");
            _output.Indent++;

            GenerateIncludedTraitLoaders(moduleDef, isInstance);
            GenerateHiddenMethods(isInstance ? moduleDef.HiddenInstanceMethods : moduleDef.HiddenClassMethods);
            GenerateMethods(moduleDef.Trait, isInstance ? moduleDef.InstanceMethods : moduleDef.ClassMethods);
            GenerateMethodAliases(isInstance ? moduleDef.InstanceMethodAliases : moduleDef.ClassMethodAliases);

            _output.Indent--;
            _output.WriteLine("}");
            if (moduleDef.BuildConfig != null) {
                _output.WriteLine("#endif");
            }
            _output.WriteLine();
        }
    }

    private void GenerateConstantsInitialization(ModuleDef/*!*/ moduleDef) {
        if (moduleDef.HasConstantsInitializer) {
            if (moduleDef.BuildConfig != null) {
                _output.WriteLine("#if " + moduleDef.BuildConfig);
            }
            WriteRubyCompatibilityCheck(moduleDef.Compatibility);

            _output.WriteLine("private static void Load{0}_Constants({1}/*!*/ module) {{", moduleDef.Id, TypeRubyModule);
            _output.Indent++;

            GenerateIncludedConstantLoaders(moduleDef);
            GenerateConstants(moduleDef);

            _output.Indent--;
            _output.WriteLine("}");
            WriteRubyCompatibilityCheckEnd(moduleDef.Compatibility);
            if (moduleDef.BuildConfig != null) {
                _output.WriteLine("#endif");
            }
            _output.WriteLine();
        }
    }

    private void GenerateConstants(ModuleDef/*!*/ moduleDef) {
        // add constants on module
        foreach (var constantDef in moduleDef.Constants.Values) {
            if (constantDef.BuildConfig != null) {
                _output.WriteLine("#if " + constantDef.BuildConfig);
            }

            GenerateSetConstant("module",  constantDef.Name, String.Format("{0}.{1}{2}",
                TypeName(constantDef.Member.DeclaringType), 
                constantDef.Member.Name,
                constantDef.Member is MethodInfo ? "(module)" : null
            ));

            if (constantDef.BuildConfig != null) {
                _output.WriteLine("#endif");
            }
        }
        _output.WriteLine();
    }

    private void GenerateIncludedTraitLoaders(ModuleDef/*!*/ moduleDef, bool isInstance) {
        foreach (MixinRef mixin in moduleDef.Mixins) {
            ModuleDef def = mixin.Module.Definition;
            if (mixin.Copy && (isInstance ? def.HasInstanceInitializer : def.HasClassInitializer)) {
                _output.WriteLine("Load{0}_{1}(module);", mixin.Module.Definition.Id, isInstance ? "Instance" : "Class");
            }
        }
    }

    private void GenerateIncludedConstantLoaders(ModuleDef/*!*/ moduleDef) {
        foreach (MixinRef mixin in moduleDef.Mixins) {
            ModuleDef def = mixin.Module.Definition;
            if (mixin.Copy && def.HasConstantsInitializer) {
                _output.WriteLine("Load{0}_Constants(module);", mixin.Module.Definition.Id);
            }
        }
    }

    private void GenerateHiddenMethods(IDictionary<string, HiddenMethod>/*!*/ methods) {
        foreach (KeyValuePair<string, HiddenMethod> entry in methods) {
            if (entry.Value == HiddenMethod.Undefined) {
                _output.WriteLine("module.{0}(\"{1}\");",
                    Builtins ? "UndefineMethodNoEvent" : "UndefineMethod",
                    entry.Key
                );
            } else {
                _output.WriteLine("module.HideMethod(\"{0}\");", entry.Key);
            }
        }
    }

    private void GenerateMethodAliases(IDictionary<string, string>/*!*/ aliases) {
        foreach (var alias in aliases) {
            _output.WriteLine("module.AddMethodAlias(\"{0}\", \"{1}\");", alias.Key, alias.Value);
        }
    }

    private void GenerateMethods(Type/*!*/ type, IDictionary<string, MethodDef>/*!*/ methods) {
        foreach (MethodDef def in methods.Values) {
            if (def.BuildConfig != null) {
                _output.WriteLine("#if " + def.BuildConfig);
            }

            int attributes = (int)def.Attributes;
            if (def.Compatibility != RubyCompatibility.Default) {
                int encodedCompat = ((int)def.Compatibility) << RubyMethodAttribute.CompatibilityEncodingShift;
                Debug.Assert((encodedCompat & attributes) == 0);
                attributes |= encodedCompat;
            }

            if (def.IsRuleGenerator) {
                _output.WriteLine("DefineRuleGenerator(module, \"{0}\", 0x{1:x}, {2}.{3}());",
                    def.Name,
                    attributes,
                    TypeName(def.Overloads[0].DeclaringType),
                    def.Overloads[0].Name);
            } else {
                _output.Write("DefineLibraryMethod(module, \"{0}\", 0x{1:x}", def.Name, attributes);

                _output.WriteLine(", ");
                _output.Indent++;
                GenerateParameterAttributes(def.Overloads);
                _output.Indent--;
                _output.WriteLine(", ");
                GenerateDelegatesListCreation(def.Overloads);

                _output.WriteLine(");");
            }

            _output.WriteLine();

            if (def.BuildConfig != null) {
                _output.WriteLine("#endif");
            }
        }
    }

    private void GenerateDelegatesListCreation(IEnumerable<MethodInfo>/*!*/ methods) {
        _output.Indent++;
        bool first = true;
        foreach (MethodInfo method in methods) {
            if (first) {
                first = false;
            } else {
                _output.WriteLine(", ");
            }
            GenerateDelegateCreation(method);
        }
        _output.Indent--;
        _output.WriteLine();
    }

    private void GenerateParameterAttributes(ICollection<MethodInfo>/*!*/ methods) {
        if (methods.Count > LibraryInitializer.MaxOverloads) {
            _output.Write("new[] { ");
        }

        bool first = true;
        foreach (MethodInfo method in methods) {
            if (first) {
                first = false;
            } else {
                _output.Write(", ");
            }
            _output.Write("0x{0,8:x8}U", LibraryOverload.EncodeCustomAttributes(method));
        }

        if (methods.Count > LibraryInitializer.MaxOverloads) {
            _output.Write("}");
        }
    }

    private const string ExceptionFactoryPrefix = "ExceptionFactory__";

    private void GenerateExceptionFactoryDelegateList(ModuleDef/*!*/ moduleDef) {
        Debug.Assert(moduleDef.IsException);
        _output.Write("new {0}{1}({2}.{3}{4})",
            TypeFunction,
            TypeArgs(typeof(RubyClass), typeof(object), typeof(Exception)),
            _initializerName,
            ExceptionFactoryPrefix,
            moduleDef.Id
        );
    }

    private void GenerateExceptionFactories(IDictionary<Type, ModuleDef>/*!*/ moduleDefs) {
        // TODO: sort by name
        foreach (var moduleDef in moduleDefs.Values) {
            if (moduleDef.IsException) {
                if (moduleDef.BuildConfig != null) {
                    _output.WriteLine("#if " + moduleDef.BuildConfig);
                }
                WriteRubyCompatibilityCheck(moduleDef.Compatibility);

                // public static Exception/*!*/ Factory(RubyClass/*!*/ self, [DefaultParameterValue(null)]object message) {
                //     return InitializeException(new Exception(GetClrMessage(self, message)), message);
                // }

                _output.WriteLine("public static {0}/*!*/ {1}{2}({3}/*!*/ self, [{4}(null)]object message) {{",
                    TypeName(typeof(Exception)),
                    ExceptionFactoryPrefix,
                    moduleDef.Id,
                    TypeRubyClass,
                    TypeName(typeof(DefaultParameterValueAttribute))
                );
                _output.Indent++;

                _output.WriteLine("return {0}.{2}(new {1}({0}.{3}(self, message), ({4})null), message);",
                    TypeName(typeof(RubyExceptionData)),
                    TypeName(moduleDef.Extends),
                    new Func<Exception, object, Exception>(RubyExceptionData.InitializeException).Method.Name,
                    new Func<RubyClass, object, string>(RubyExceptionData.GetClrMessage).Method.Name,
                    TypeName(typeof(Exception))
                );

                _output.Indent--;
                _output.WriteLine("}");
                _output.WriteLine();

                WriteRubyCompatibilityCheckEnd(moduleDef.Compatibility);
                if (moduleDef.BuildConfig != null) {
                    _output.WriteLine("#endif");
                }
            }
        }
    }

    private void GenerateDelegateCreation(MethodInfo/*!*/ method) {
        ParameterInfo[] ps = method.GetParameters();
        Type[] paramTypes = Array.ConvertAll(ps, (p) => p.ParameterType);

        string delegateType;
        if (method.ReturnType != typeof(void)) {
            delegateType = TypeFunction;
            paramTypes = ArrayUtils.Append(paramTypes, method.ReturnType);
        } else {
            delegateType = TypeAction;
        }

        _output.Write("new {0}{1}({2}.{3})",
            delegateType,
            TypeArgs(paramTypes),
            TypeName(method.DeclaringType),
            method.Name
        );
    }

    #endregion

    #region Helpers

    private static string/*!*/ TypeName(Type/*!*/ type) {
        return ReflectionUtils.FormatTypeName(new StringBuilder(), type, Generator.TypeNameDispenser).ToString();
    }

    private static string/*!*/ TypeArgs(params Type/*!*/[]/*!*/ types) {
        return ReflectionUtils.FormatTypeArgs(new StringBuilder(), types, Generator.TypeNameDispenser).ToString();
    }

    private static string/*!*/ GenericTypeName(Type/*!*/ type) {
        if (type.IsGenericTypeDefinition) {
            return String.Concat(type.Namespace, ".", type.Name.Substring(0, type.Name.IndexOf('`')));
        } else {
            return type.Name;
        }
    }

    private class TypeComparer : IComparer<Type> {
        public int Compare(Type/*!*/ x, Type/*!*/ y) {
            return x.FullName.CompareTo(y.FullName);
        }
    }

    #endregion
}

