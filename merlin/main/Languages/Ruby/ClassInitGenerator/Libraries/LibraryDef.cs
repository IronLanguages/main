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
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System.Text;
using IronRuby;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Generation;

internal class LibraryDef {

    private bool Builtins { get { return _namespace == typeof(RubyClass).Namespace; } }

    public static readonly string/*!*/ TypeAction0 = TypeName(typeof(Action));
    public static readonly string/*!*/ TypeAction1 = GenericTypeName(typeof(Action<>));
    public static readonly string/*!*/ TypeActionN = GenericTypeName(typeof(Action<,>));
    public static readonly string/*!*/ TypeFunction = GenericTypeName(typeof(Func<>));
    public static readonly string/*!*/ TypeDelegate = TypeName(typeof(Delegate));
    public static readonly string/*!*/ TypeRubyModule = TypeName(typeof(RubyModule));
    public static readonly string/*!*/ TypeRubyClass = TypeName(typeof(RubyClass));
    public static readonly string/*!*/ TypeActionOfRubyModule = TypeName(typeof(Action<RubyModule>));
    public static readonly string/*!*/ TypeRubyExecutionContext = TypeName(typeof(RubyContext));
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
        public string/*!*/ QualifiedName;
        public string/*!*/ SimpleName;
        public string/*!*/ Id;
        public bool IsExtension;
        public bool IsException;
        public List<string>/*!*/ Aliases = new List<string>();

        public Type/*!*/ Trait;
        public IDictionary<string, MethodDef>/*!*/ InstanceMethods = new SortedDictionary<string, MethodDef>();
        public IDictionary<string, MethodDef>/*!*/ ClassMethods = new SortedDictionary<string, MethodDef>();
        public IDictionary<string, ConstantDef>/*!*/ Constants = new SortedDictionary<string, ConstantDef>();

        public IDictionary<string, HiddenMethod>/*!*/ HiddenInstanceMethods = new SortedDictionary<string, HiddenMethod>();
        public IDictionary<string, HiddenMethod>/*!*/ HiddenClassMethods = new SortedDictionary<string, HiddenMethod>();

        public List<MethodInfo>/*!*/ Factories = new List<MethodInfo>();

        public List<MixinRef>/*!*/ Mixins = new List<MixinRef>();

        // Type real type (same as Trait for non-extension classes):
        public Type/*!*/ Extends;

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
                    || QualifiedName == RubyClass.ClassSingletonName
                    || QualifiedName == RubyClass.ClassSingletonSingletonName
                    || Extends == typeof(Kernel)
                    || Extends == typeof(Object)
                    || Extends == typeof(RubyClass)
                    || Extends == typeof(RubyModule)
                );
            }
        }

        public bool HasInstanceInitializer {
            get {
                return InstanceMethods.Count > 0 || HiddenInstanceMethods.Count > 0 || HasCopyInclusions || IsPrimitive ||
                    Constants.Count > 0;
            }
        }

        public bool HasClassInitializer {
            get { return ClassMethods.Count > 0 || HiddenClassMethods.Count > 0 || HasCopyInclusions || IsPrimitive; }
        }

        public const string/*!*/ ObjectClassRef = "Context.ObjectClass";
        public const string/*!*/ KernelModuleRef = "Context.KernelModule";
        public const string/*!*/ ModuleClassRef = "Context.ModuleClass";
        public const string/*!*/ ClassClassRef = "Context.ClassClass";

        internal string GetReference(ref int defVariableId) {
            if (Reference == null) {
                if (Extends == typeof(Object)) {
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
    }

    private class MethodDef {
        public string/*!*/ Name;
        public List<MethodInfo>/*!*/ Overloads = new List<MethodInfo>();
        public string BuildConfig;
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
                def.BuildConfig = module.BuildConfig;

                def.Super = null;
                if (cls != null && def.Extends != typeof(object) && !def.Extends.IsInterface && !def.IsExtension) {
                    if (cls != null && cls.Inherits != null) {
                        def.Super = new TypeRef(cls.Inherits);
                    } else {
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

                if (cls != null && cls.MixinInterfaces) {
                    foreach (Type iface in def.Extends.GetInterfaces()) {
                        ModuleDef mixin;
                        if (_moduleDefs.TryGetValue(iface, out mixin)) {
                            def.Mixins.Add(new MixinRef(new TypeRef(mixin.Extends), false));
                        }
                    }
                }

                _moduleDefs.Add(def.Extends, def);
                _traits.Add(def.Trait, def);

                // added Ruby methods and constants:
                ReflectMethods(def);
                ReflectFieldConstants(def);
                ReflectAliases(def);

                // hidden CLR methods:
                foreach (HideMethodAttribute method in trait.GetCustomAttributes(typeof(HideMethodAttribute), false)) {
                    // TODO: warning, method already removed, method not found ...
                    if (method.IsStatic) {
                        def.HiddenClassMethods[method.Name] = HiddenMethod.ClrInvisible;
                    } else {
                        def.HiddenInstanceMethods[method.Name] = HiddenMethod.ClrInvisible;
                    }
                }

                // undefined methods:
                foreach (UndefineMethodAttribute method in trait.GetCustomAttributes(typeof(UndefineMethodAttribute), false)) {
                    // TODO: warning, method already removed, method not found ...
                    if (method.IsStatic) {
                        def.HiddenClassMethods[method.Name] = HiddenMethod.Undefined;
                    } else {
                        def.HiddenInstanceMethods[method.Name] = HiddenMethod.Undefined;
                    }
                }
            }
        }

        int defVariableId = 1;

        // qualified names, ids, declaring modules:
        foreach (ModuleDef def in _moduleDefs.Values) {
            if (!def.IsExtension) {
                def.QualifiedName = def.SimpleName;

                // finds the inner most Ruby module def containing this module def:
                Type declaringType = def.Trait.DeclaringType;
                while (declaringType != null) {
                    ModuleDef declaringDef;
                    if (_traits.TryGetValue(declaringType, out declaringDef)) {
                        def.QualifiedName = declaringDef.QualifiedName + "::" + def.SimpleName;
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

                        // we will need a reference for setting the constant:
                        def.DeclaringTypeRef = declaringDef.GetReference(ref defVariableId);
                        def.GetReference(ref defVariableId);
                        break;
                    } else {
                        declaringType = declaringType.DeclaringType;
                    }
                }
            } else {
                def.QualifiedName = RubyUtils.GetQualifiedName(def.Extends);
            }

            if (def.Kind == ModuleKind.Singleton) {
                // we need to refer to the singleton object returned from singelton factory:
                def.GetReference(ref defVariableId);

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
            } else if (!def.IsExtension && def.Kind == ModuleKind.Class && def.Extends != typeof(object)) {
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
                }
            }
        }

        // TODO: checks
        // - loops in copy-inclusion
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

                    if (type == typeof(CodeContext)) {
                        LogMethodError("CodeContext is obsolete use RubyContext instead.", methodDef, overload);
                    }

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SiteLocalStorage<>)) {
                        if (hasSelf || hasContext || hasBlock) {
                            LogMethodError("SiteLocalStorage must precede all other parameters", methodDef, overload);
                        }
                        storageCount++;
                    } else if (type == typeof(RubyContext) || type == typeof(RubyScope)) {
                        if (hasContext) {
                            LogMethodError("has multiple context parameters", methodDef, overload);
                        }

                        if (i - storageCount != 0) {
                            LogMethodError("Context parameter must be the first parameter following optional SiteLocalStorage", methodDef, overload);
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
                            LogMethodError("Block parameter must be the first parameter following optional SiteLocalStorage", methodDef, overload);
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

    private void LogMethodWarning(string/*!*/ message, MethodDef methodDef, MethodBase/*!*/ overload,params object[] args) {
        string methodName = (methodDef != null) ? "method \"" + methodDef.Name + '"' : "factory";
        Console.Error.WriteLine("Warning: {0}: {1}", methodName, String.Format(message, args));
        Console.Error.WriteLine("         overload: {0}", ReflectionUtils.FormatSignature(new StringBuilder(), overload));
    }

    #region Code Generation

    public void GenerateCode(IndentedTextWriter/*!*/ output) {
        _output = output;

        _output.WriteLine("namespace {0} {{", _namespace);
        _output.Indent++;

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

            _output.WriteLine("new {0}(Load{1}_Instance),", TypeActionOfRubyModule, RubyClass.ClassSingletonName);
            _output.WriteLine("new {0}(Load{1}_Instance),", TypeActionOfRubyModule, RubyClass.ClassSingletonSingletonName);
            _output.WriteLine("new {0}(Load{1}_Instance),", TypeActionOfRubyModule, RubyClass.MainSingletonName);

            _output.WriteLine("new {0}(LoadKernel_Instance),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadObject_Instance),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadModule_Instance),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadClass_Instance),", TypeActionOfRubyModule);

            _output.WriteLine("new {0}(LoadKernel_Class),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadObject_Class),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadModule_Class),", TypeActionOfRubyModule);
            _output.WriteLine("new {0}(LoadClass_Class)", TypeActionOfRubyModule);

            _output.Indent--;
            _output.WriteLine(");");
        }

        // generate references:
        foreach (KeyValuePair<Type, string> moduleRef in _moduleRefs) {
            _output.WriteLine("{0} {1} = GetModule(typeof({2}));", TypeRubyModule, moduleRef.Value, moduleRef.Key);
        }

        foreach (KeyValuePair<Type, string> classRef in _classRefs) {
            _output.WriteLine("{0} {1} = GetClass(typeof({2}));", TypeRubyClass, classRef.Value, classRef.Key);
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

                if (def.IsGlobal) {
                    GenerateAliases(def, ModuleDef.ObjectClassRef);
                } else if (def.DeclaringTypeRef != null) {
                    GenerateAliases(def, def.DeclaringTypeRef);
                    _output.WriteLine("{0}.SetConstant(\"{1}\", {2});", def.DeclaringTypeRef, def.SimpleName, def.Reference);
                }

                if (def.BuildConfig != null) {
                    _output.WriteLine("#endif");
                }
            }
        }
    }

    private void GenerateAliases(ModuleDef/*!*/ def, string/*!*/ ownerRef) {
        foreach (string alias in def.Aliases) {
            _output.WriteLine("{0}.SetConstant(\"{1}\", {2});", ownerRef, alias, def.Reference);
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
                    _output.Write("ExtendClass(typeof({0}), {1}, {2}, ",
                        TypeName(def.Extends),
                        (def.HasInstanceInitializer) ? String.Format("new {0}(Load{1}_Instance)", TypeActionOfRubyModule, def.Id) : "null",
                        (def.HasClassInitializer) ? String.Format("new {0}(Load{1}_Class)", TypeActionOfRubyModule, def.Id) : "null"
                    );
                } else {
                    _output.Write("Define{0}Class(\"{1}\", typeof({2}), {3}, {4}, {5}, {6}, ",
                        def.IsGlobal ? "Global" : "",
                        def.QualifiedName,
                        TypeName(def.Extends),
                        def.Extends == def.Trait ? "true" : "false", 
                        def.Super.RefName,
                        (def.HasInstanceInitializer) ? String.Format("new {0}(Load{1}_Instance)", TypeActionOfRubyModule, def.Id) : "null",
                        (def.HasClassInitializer) ? String.Format("new {0}(Load{1}_Class)", TypeActionOfRubyModule, def.Id) : "null"
                    );
                }

                GenerateInclusions(def);

                _output.Write(", ");

                if (def.Factories.Count > 0) {
                    GenerateDelegatesArrayCreation(def.Factories);
                } else if (def.IsException) {
                    GenerateExceptionFactoryDelegateArray(def);
                } else {
                    _output.Write("null");
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
                        (def.HasInstanceInitializer) ? String.Format("new {0}(Load{1}_Instance)", TypeActionOfRubyModule, def.Id) : "null",
                        (def.HasClassInitializer) ? String.Format("new {0}(Load{1}_Class)", TypeActionOfRubyModule, def.Id) : "null"
                    );
                } else {
                    _output.Write("Define{0}Module(\"{1}\", typeof({2}), {3}, {4}, ",
                        def.IsGlobal ? "Global" : "",
                        def.QualifiedName,
                        TypeName(def.Extends),
                        (def.HasInstanceInitializer) ? String.Format("new {0}(Load{1}_Instance)", TypeActionOfRubyModule, def.Id) : "null",
                        (def.HasClassInitializer) ? String.Format("new {0}(Load{1}_Class)", TypeActionOfRubyModule, def.Id) : "null"
                    );
                }

                GenerateInclusions(def);

                _output.WriteLine(");");
                break;

            case ModuleKind.Singleton:
                if (def.Definition != null) {
                    _output.Write("object {0} = ", def.Definition);
                }

                _output.Write("DefineSingleton({0}, {1}, ",
                    (def.HasInstanceInitializer) ? String.Format("new {0}(Load{1}_Instance)", TypeActionOfRubyModule, def.Id) : "null",
                    (def.HasClassInitializer) ? String.Format("new {0}(Load{1}_Class)", TypeActionOfRubyModule, def.Id) : "null"
                );

                GenerateInclusions(def);

                _output.WriteLine(");");
                break;

            default:
                throw Assert.Unreachable;
        }

        if (def.BuildConfig != null) {
            _output.WriteLine("#endif");
        }
    }

    private void GenerateInclusions(ModuleDef/*!*/ def) {
        List<string> mixinRefs = new List<string>();
        AddMixinRefsRecursive(mixinRefs, def);

        if (mixinRefs.Count > 0) {
            _output.Write("new {0}[] {{", TypeRubyModule);
            foreach (string mixinRef in mixinRefs) {
                _output.Write("{0}, ", mixinRef);
            }
            _output.Write("}");
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
        Array.Sort(worklist, delegate(ModuleDef x, ModuleDef y) { return x.QualifiedName.CompareTo(y.QualifiedName); });

        foreach (ModuleDef moduleDef in worklist) {
            GenerateTraitInitialization(moduleDef);
        }
    }

    private void GenerateTraitInitialization(ModuleDef/*!*/ moduleDef) {
        GenerateTraitInitialization(moduleDef, true);
        GenerateTraitInitialization(moduleDef, false);
    }

    private void GenerateTraitInitialization(ModuleDef/*!*/ moduleDef, bool isInstance) {
        if (isInstance ? moduleDef.HasInstanceInitializer : moduleDef.HasClassInitializer) {
            if (moduleDef.BuildConfig != null) {
                _output.WriteLine("#if " + moduleDef.BuildConfig);
            }
            _output.WriteLine("private void Load{0}_{2}({1}/*!*/ module) {{", moduleDef.Id, TypeRubyModule, isInstance ? "Instance" : "Class");
            _output.Indent++;

            if (isInstance) {
                GenerateConstants(moduleDef);
            }

            GenerateIncludedTraitLoaders(moduleDef, isInstance);
            GenerateHiddenMethods(isInstance ? moduleDef.HiddenInstanceMethods : moduleDef.HiddenClassMethods);
            GenerateMethods(moduleDef.Trait, isInstance ? moduleDef.InstanceMethods : moduleDef.ClassMethods);

            _output.Indent--;
            _output.WriteLine("}");
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

            _output.Write("module.SetConstant(\"{0}\", {1}.{2}", constantDef.Name,
                TypeName(constantDef.Member.DeclaringType), constantDef.Member.Name);

            if (constantDef.Member is MethodInfo) {
                _output.Write("(module)");
            }

            _output.WriteLine(");");

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

    private void GenerateHiddenMethods(IDictionary<string, HiddenMethod>/*!*/ methods) {
        foreach (KeyValuePair<string, HiddenMethod> entry in methods) {
            if (entry.Value == HiddenMethod.Undefined) {
                _output.WriteLine("module.UndefineLibraryMethod(\"{0}\");", entry.Key);
            } else {
                _output.WriteLine("module.HideMethod(\"{0}\");", entry.Key);
            }
        }
    }

    private void GenerateMethods(Type/*!*/ type, IDictionary<string, MethodDef>/*!*/ methods) {
        foreach (MethodDef def in methods.Values) {
            if (def.BuildConfig != null) {
                _output.WriteLine("#if " + def.BuildConfig);
            }

            if (def.IsRuleGenerator) {
                _output.WriteLine("module.DefineRuleGenerator(\"{0}\", 0x{1:x}, {2}.{3}());",
                    def.Name,
                    (int)def.Attributes,
                    TypeName(def.Overloads[0].DeclaringType),
                    def.Overloads[0].Name);
            } else {
                _output.Write("module.DefineLibraryMethod(\"{0}\", 0x{1:x}", def.Name, (int)def.Attributes);

                _output.Write(", ");

                GenerateDelegatesArrayCreation(def.Overloads);

                _output.WriteLine(");");
            }

            _output.WriteLine();

            if (def.BuildConfig != null) {
                _output.WriteLine("#endif");
            }
        }
    }

    private void GenerateDelegatesArrayCreation(IEnumerable<MethodInfo>/*!*/ methods) {
        _output.WriteLine("new {0}[] {{", TypeDelegate);
        _output.Indent++;

        foreach (MethodInfo method in methods) {
            GenerateDelegateCreation(method);
            _output.WriteLine(",");
        }

        _output.Indent--;
        _output.Write("}");
    }

    private const string ExceptionFactoryPrefix = "ExceptionFactory__";

    private void GenerateExceptionFactoryDelegateArray(ModuleDef/*!*/ moduleDef) {
        Debug.Assert(moduleDef.IsException);
        _output.Write("new {0}[] {{ new {1}{2}({3}.{4}{5}) }}",
            TypeDelegate,
            TypeFunction,
            ReflectionUtils.FormatTypeArgs(new StringBuilder(), new[] { typeof(RubyClass), typeof(object), typeof(Exception) }),
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

                if (moduleDef.BuildConfig != null) {
                    _output.WriteLine("#endif");
                }
            }
        }
    }

    private void GenerateDelegateCreation(MethodInfo/*!*/ method) {
        ParameterInfo[] ps = method.GetParameters();
        Type[] paramTypes = Array.ConvertAll<ParameterInfo, Type>(ps, delegate(ParameterInfo p) { return p.ParameterType; });

        string delegateType;
        if (method.ReturnType != typeof(void)) {
            delegateType = TypeFunction;
            paramTypes = ArrayUtils.Append(paramTypes, method.ReturnType);
        } else if (paramTypes.Length == 0) {
            delegateType = TypeAction0;
        } else if (paramTypes.Length == 1) {
            delegateType = TypeAction1;
        } else {
            delegateType = TypeActionN;
        }

        _output.Write("new {0}{1}({2}.{3})",
            delegateType,
            ReflectionUtils.FormatTypeArgs(new StringBuilder(), paramTypes),
            TypeName(method.DeclaringType),
            method.Name
        );
    }

    #endregion

    #region Helpers

    private static string/*!*/ TypeName(Type/*!*/ type) {
        return ReflectionUtils.FormatTypeName(new StringBuilder(), type).ToString();
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

