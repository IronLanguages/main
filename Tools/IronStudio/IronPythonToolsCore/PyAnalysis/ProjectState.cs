/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.IronPythonTools;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.PyAnalysis.Values;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Library;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace Microsoft.PyAnalysis {
    /// <summary>
    /// Connects multiple source files together into one project state for combined analysis
    /// </summary>
    class ProjectState {
        private readonly ScriptEngine _pythonEngine;
        private readonly CodeContext _codeContext;
        private readonly CodeContext _codeContextCls;

        private readonly List<ProjectEntry> _projectEntries;
        private readonly Dictionary<string, ModuleReference> _modules;
        private readonly Dictionary<string, ModuleInfo> _modulesByFilename;
        private readonly Dictionary<string, XamlProjectEntry> _xamlByFilename = new Dictionary<string, XamlProjectEntry>();
        private readonly Dictionary<object, object> _itemCache;
        private readonly BuiltinModule _builtinModule;
        private readonly List<KeyValuePair<Assembly, TopNamespaceTracker>> _references;
        internal readonly Namespace _propertyObj, _classmethodObj, _staticmethodObj, _typeObj, _intType, _rangeFunc, _frozensetType;
        internal readonly HashSet<Namespace> _objectSet;
        internal readonly Namespace _functionType;
        internal readonly BuiltinClassInfo _dictType, _listType, _tupleType, _generatorType, _stringType, _boolType, _setType;
        internal readonly ConstantInfo _noneInst;
        private readonly Queue<AnalysisUnit> _queue;
        private readonly DocumentationProvider _docProvider;
        private HashSet<string> _assemblyLoadList = new HashSet<string>();

        private static object _nullKey = new object();

        public ProjectState(ScriptEngine pythonEngine) {
            _pythonEngine = pythonEngine;
            _projectEntries = new List<ProjectEntry>();
            _modules = new Dictionary<string, ModuleReference>();
            _modulesByFilename = new Dictionary<string, ModuleInfo>(StringComparer.OrdinalIgnoreCase);
            _itemCache = new Dictionary<object, object>();

            var pythonContext = HostingHelpers.GetLanguageContext(_pythonEngine) as PythonContext;
            _codeContextCls = new ModuleContext(new PythonDictionary(), pythonContext).GlobalContext;
            _codeContextCls.ModuleContext.ShowCls = true;

            _codeContext = new ModuleContext(
                new PythonDictionary(),
                HostingHelpers.GetLanguageContext(_pythonEngine) as PythonContext
                ).GlobalContext;

            InitializeBuiltinModules();
            
            // TODO: Use reflection-only!
            _references = new List<KeyValuePair<Assembly, TopNamespaceTracker>>();
            AddAssembly(LoadAssemblyInfo(typeof(string).Assembly));
            AddAssembly(LoadAssemblyInfo(typeof(Debug).Assembly));
            

            // cached for quick checks to see if we're a call to clr.AddReference
            SpecializeFunction("clr", "AddReference", (n, unit, args) => AddReference(n, null));
            SpecializeFunction("clr", "AddReferenceByPartialName", (n, unit, args) => AddReference(n, ClrModule.LoadAssemblyByPartialName));
            SpecializeFunction("clr", "AddReferenceByName", (n, unit, args) => AddReference(n, null));
            SpecializeFunction("clr", "AddReferenceToFile", (n, unit, args) => AddReference(n, (s) => ClrModule.LoadAssemblyFromFile(_codeContext, s)));
            SpecializeFunction("clr", "AddReferenceToFileAndPath", (n, unit, args) => AddReference(n, (s) => ClrModule.LoadAssemblyFromFileWithPath(_codeContext, s)));
            
            try {
                SpecializeFunction("wpf", "LoadComponent", LoadComponent);
            } catch (KeyNotFoundException) {
                // IronPython.Wpf.dll isn't available...
            }

            SpecializeFunction("__builtin__", "range", (n, unit, args) => unit.DeclaringModule.GetOrMakeNodeVariable(n, (nn) => new RangeInfo(ClrModule.GetPythonType(typeof(List)), unit.ProjectState).SelfSet));
            SpecializeFunction("__builtin__", "min", ReturnUnionOfInputs);
            SpecializeFunction("__builtin__", "max", ReturnUnionOfInputs);

            _builtinModule = (BuiltinModule)Modules["__builtin__"].Module;
            _propertyObj = GetBuiltin("property");
            _classmethodObj = GetBuiltin("classmethod");
            _staticmethodObj = GetBuiltin("staticmethod");
            _typeObj = GetBuiltin("type");
            _intType = GetBuiltin("int");
            _stringType = (BuiltinClassInfo)GetBuiltin("str");
            
            _objectSet = new HashSet<Namespace>(new[] { GetBuiltin("object") });

            _setType = (BuiltinClassInfo)GetNamespaceFromObjects(TypeCache.Set);
            _rangeFunc = GetBuiltin("range");
            _frozensetType = GetBuiltin("frozenset");
            _functionType = GetNamespaceFromObjects(TypeCache.Function);
            _generatorType = (BuiltinClassInfo)GetNamespaceFromObjects(DynamicHelpers.GetPythonTypeFromType(typeof(PythonGenerator)));
            _dictType = (BuiltinClassInfo)GetNamespaceFromObjects(TypeCache.Dict);
            _boolType = (BuiltinClassInfo)GetNamespaceFromObjects(TypeCache.Boolean);
            _noneInst = (ConstantInfo)GetNamespaceFromObjects(new object[] { null });
            _listType = (BuiltinClassInfo)GetNamespaceFromObjects(TypeCache.List);
            _tupleType = (BuiltinClassInfo)GetNamespaceFromObjects(TypeCache.PythonTuple);

            _queue = new Queue<AnalysisUnit>();

            _docProvider = CodeContext.LanguageContext.GetService<DocumentationProvider>();
        }

        /// <summary>
        /// Adds a new module of code to the list of available modules and returns a ProjectEntry object.
        /// </summary>
        /// <param name="moduleName">The name of the module; used to associate with imports</param>
        /// <param name="filePath">The path to the file on disk</param>
        /// <param name="cookie">An application-specific identifier for the module</param>
        /// <returns></returns>
        public ProjectEntry AddModule(string moduleName, string filePath, IAnalysisCookie cookie) {
            var entry = new ProjectEntry(this, moduleName, filePath, cookie);
            _projectEntries.Add(entry);
            if (moduleName != null) {
                Modules[moduleName] = new ModuleReference(entry.MyScope);
            }
            if (filePath != null) {
                _modulesByFilename[filePath] = entry.MyScope;
            }
            return entry;
        }

        public XamlProjectEntry AddXamlFile(string filePath) {
            var entry = new XamlProjectEntry(this, filePath);
            _xamlByFilename[filePath] = entry;
            return entry;
        }

        /// <summary>
        /// Gets a top-level list of all the available modules as a list of MemberResults.
        /// </summary>
        /// <returns></returns>
        public MemberResult[] GetModules() {
            var d = new Dictionary<string, HashSet<Namespace>>();
            foreach (var keyValue in Modules) {
                var modName = keyValue.Key;
                var moduleRef = keyValue.Value;

                HashSet<Namespace> l;
                if (!d.TryGetValue(modName, out l)) {
                    d[modName] = l = new HashSet<Namespace>();
                }
                if (moduleRef != null && moduleRef.Module != null) {
                    // The REPL shows up here with value=None
                    l.Add(moduleRef.Module);
                }
            }

            foreach (var r in _references) {
                foreach (string key in r.Value.GetMemberNames()) {
                    var value = PythonAssemblyOps.GetBoundMember(_codeContext, r.Key, key);
                    HashSet<Namespace> l2;
                    if (!d.TryGetValue(key, out l2)) {
                        d[key] = l2 = new HashSet<Namespace>();
                    }
                    l2.Add(GetNamespaceFromObjects(value));
                }
            }

            var result = new MemberResult[d.Count];
            int pos = 0;
            foreach (var kvp in d) {
                result[pos++] = new MemberResult(kvp.Key, kvp.Value);
            }
            return result;
        }

        /// <summary>
        /// returns the MemberResults associated with modules in the specified
        /// list of names.  The list of names is the path through the module, for example
        /// ['System', 'Runtime']
        /// </summary>
        /// <returns></returns>
        public MemberResult[] GetModuleMembers(string[] names) {
            return GetModuleMembers(names, true);
        }

        /// <summary>
        /// returns the MemberResults associated with modules in the specified
        /// list of names.  The list of names is the path through the module, for example
        /// ['System', 'Runtime']
        /// </summary>
        /// <returns></returns>
        public MemberResult[] GetModuleMembers(string[] names, bool bottom) {
            IDictionary<string, ISet<Namespace>> d = null;
            var nses = GetReflectedNamespaces(names, bottom);
            if (nses == null) {
                return new MemberResult[0];
            }
            var ns = GetNamespaceFromObjects(nses);
            if (ns != null) {
                d = ns.GetAllMembers(true);
            }

            ModuleReference moduleRef;
            if (Modules.TryGetValue(names[0], out moduleRef) && moduleRef.Module != null) {
                var module = moduleRef.Module;
                var newDict = new Dictionary<string, ISet<Namespace>>();
                if (d != null) {
                    Update(newDict, d);
                }

                d = newDict;

                var mod = module.SelfSet;
                if (bottom) {
                    for (int i = 1; i < names.Length; i++) {
                        var next = names[i];
                        // import Foo.Bar as Baz, we need to get Bar
                        VariableDef def;
                        ISet<Namespace> newMod = EmptySet<Namespace>.Instance;
                        bool madeSet = false;
                        foreach (var modItem in mod) {
                            BuiltinModule builtinMod = modItem as BuiltinModule;
                            if (builtinMod != null) {
                                ISet<Namespace> builtinValues;
                                if (builtinMod.VariableDict.TryGetValue(next, out builtinValues)) {
                                    newMod = newMod.Union(builtinValues, ref madeSet);
                                }
                            } else {
                                ModuleInfo userMod = modItem as ModuleInfo;
                                if (userMod != null && userMod.Scope.Variables.TryGetValue(next, out def)) {
                                    newMod = newMod.Union(def.Types, ref madeSet);
                                }
                            }
                        }

                        mod = newMod;
                        if (mod.Count == 0) {
                            break;
                        }
                    }
                }

                foreach (var modItem in mod) {
                    Update(d, modItem.GetAllMembers(false));
                }
            }

            MemberResult[] result;
            if (d != null) {
                result = new MemberResult[d.Count];
                int pos = 0;
                foreach (var kvp in d) {
                    result[pos++] = new MemberResult(kvp.Key, kvp.Value);
                }
            } else {
                result = new MemberResult[0];
            }

            return result;
        }

        /// <summary>
        /// Replaces a built-in function (specified by module name and function name) with a customized
        /// delegate which provides specific behavior for handling when that function is called.
        /// 
        /// Currently this just provides a hook when the function is called - it could be expanded
        /// to providing the interpretation of when the function is called as well.
        /// </summary>
        private void SpecializeFunction(string moduleName, string name, Func<CallExpression, AnalysisUnit, ISet<Namespace>[], ISet<Namespace>> dlg) {
            var module = Modules[moduleName];
            
            BuiltinModule builtin = module.Module as BuiltinModule;
            Debug.Assert(builtin != null);
            if (builtin != null) {
                foreach (var v in builtin.VariableDict[name]) {
                    BuiltinFunctionInfo funcInfo = v as BuiltinFunctionInfo;
                    if (funcInfo != null) {
                        builtin.VariableDict[name] = new SpecializedBuiltinFunction(this, funcInfo.Function, dlg).SelfSet;
                        break;
                    }
                }
            }
        }

        public List<ProjectEntry> ProjectEntries {
            get { return _projectEntries; }
        }
        
        internal Queue<AnalysisUnit> Queue {
            get {
                return _queue;
            }
        }

        private void InitializeBuiltinModules() {
            string installDir = PythonRuntimeHost.GetPythonInstallDir();
            if (installDir != null) {
                var dllDir = Path.Combine(installDir, "DLLs");
                if (Directory.Exists(dllDir)) {
                    foreach (var assm in Directory.GetFiles(dllDir)) {
                        try {
                            _pythonEngine.Runtime.LoadAssembly(Assembly.LoadFile(Path.Combine(dllDir, assm)));
                        } catch {
                        }
                    }
                }
            }

            var names = _pythonEngine.Operations.GetMember<PythonTuple>(_pythonEngine.GetSysModule(), "builtin_module_names");
            foreach (string modName in names) {
                PythonModule mod = Importer.Import(_codeContextCls, modName, PythonOps.EmptyTuple, 0) as PythonModule;
                Debug.Assert(mod != null);

                Modules[modName] = new ModuleReference(new BuiltinModule(mod, this, false));
            }
        }

        private KeyValuePair<Assembly, TopNamespaceTracker> LoadAssemblyInfo(Assembly assm) {
            var nsTracker = new TopNamespaceTracker(_codeContext.LanguageContext.DomainManager);
            nsTracker.LoadAssembly(assm);
            return new KeyValuePair<Assembly, TopNamespaceTracker>(assm, nsTracker);
        }

        private void AddAssembly(KeyValuePair<Assembly, TopNamespaceTracker> assembly) {
            _references.Add(assembly);
            ThreadPool.QueueUserWorkItem(state => {
                // start initializing assemblies on background thread for quick response.
                foreach (string key in assembly.Value.GetMemberNames()) {
                    var value = PythonAssemblyOps.GetBoundMember(_codeContext, assembly.Key, key);
                }
            });
        }

        private ISet<Namespace> ReturnUnionOfInputs(CallExpression call, AnalysisUnit unit, ISet<Namespace>[] args) {
            ISet<Namespace> res = EmptySet<Namespace>.Instance;
            bool madeSet = false;
            foreach (var set in args) {
                res = res.Union(set, ref madeSet);
            }
            return res;
        }

        private ISet<Namespace> LoadComponent(CallExpression node, AnalysisUnit unit, ISet<Namespace>[]args) {
            if (args.Length == 2) {
                var xaml = args[1];
                var self = args[0];

                foreach (var arg in xaml) {
                    string strConst = arg.GetConstantValue() as string;
                    if (strConst != null) {
                        // process xaml file, add attributes to self
                        string xamlPath = Path.Combine(Path.GetDirectoryName(unit.DeclaringModule.ProjectEntry.FilePath), strConst);
                        XamlProjectEntry xamlProject;
                        if (_xamlByFilename.TryGetValue(xamlPath, out xamlProject)) {
                            // TODO: Get existing analysis if it hasn't changed.
                            var analysis = xamlProject.Analysis;

                            if (analysis == null) {
                                xamlProject.Analyze();
                                analysis = xamlProject.Analysis;
                            }

                            xamlProject.AddDependency(unit);

                            var evalUnit = unit.CopyForEval();

                            // add named objects to instance
                            foreach (var keyValue in analysis.NamedObjects) {
                                var type = keyValue.Value;
                                if (type.Type.UnderlyingType != null) {
                                    var ns = GetNamespaceFromObjects(DynamicHelpers.GetPythonTypeFromType(type.Type.UnderlyingType));
                                    if (ns is BuiltinClassInfo) {
                                        ns = ((BuiltinClassInfo)ns).Instance;
                                    }
                                    self.SetMember(node, evalUnit, keyValue.Key, ns.SelfSet);
                                }

                                // TODO: Better would be if SetMember took something other than a node, then we'd
                                // track references w/o this extra effort.
                                foreach (var inst in self) {
                                    InstanceInfo instInfo = inst as InstanceInfo;
                                    if (instInfo != null) {
                                        VariableDef def;
                                        if (instInfo.InstanceAttributes.TryGetValue(keyValue.Key, out def)) {
                                            def.AddAssignment(
                                                new SimpleSrcLocation(type.LineNumber, type.LineOffset, keyValue.Key.Length),
                                                xamlProject
                                            );
                                        }
                                    }
                                }
                            }

                            // add references to event handlers
                            foreach (var keyValue in analysis.EventHandlers) {
                                // add reference to methods...
                                var member = keyValue.Value;

                                // TODO: Better would be if SetMember took something other than a node, then we'd
                                // track references w/o this extra effort.
                                foreach (var inst in self) {
                                    InstanceInfo instInfo = inst as InstanceInfo;
                                    if (instInfo != null) {
                                        ClassInfo ci = instInfo.ClassInfo;

                                        VariableDef def;
                                        if (ci.Scope.Variables.TryGetValue(keyValue.Key, out def)) {
                                            def.AddReference(
                                                new SimpleSrcLocation(member.LineNumber, member.LineOffset, keyValue.Key.Length),
                                                xamlProject
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // load component returns self
                return self;
            }
            
            return EmptySet<Namespace>.Instance;
        }

        private ISet<Namespace> AddReference(CallExpression node, Func<string, Assembly> partialLoader) {
            // processes a call to clr.AddReference updating project state
            // so that it contains the newly loaded assembly.
            foreach (var arg in node.Args) {
                var cexpr = arg.Expression as ConstantExpression;
                if (cexpr == null || !(cexpr.Value is string)) {
                    // can't process this add reference
                    continue;
                }

                // TODO: Should we do a .NET reflection only load rather than
                // relying on the CLR module here?  That would prevent any code from
                // running although at least we don't taint our own modules which
                // are loaded with this current code.
                var asmName = cexpr.Value as string;
                if (asmName != null && _assemblyLoadList.Add(asmName)) {
                    Assembly asm = null;
                    try {
                        if (partialLoader != null) {
                            asm = partialLoader(asmName);
                        } else {
                            try {
                                asm = ClrModule.LoadAssemblyByName(_codeContext, asmName);
                            } catch {
                                asm = ClrModule.LoadAssemblyByPartialName(asmName);
                            }
                        }
                    } catch {
                    }
                    AddAssembly(asm);                
                }
            }
            return null;
        }

        public void AddAssembly(Assembly asm) {
            if (asm != null && _references.FindIndex(reference => (reference.Key == asm)) == -1) {
                // TODO: When do assembly references get removed?
                var nsTracker = new TopNamespaceTracker(_codeContext.LanguageContext.DomainManager);
                nsTracker.LoadAssembly(asm);
                AddAssembly(new KeyValuePair<Assembly, TopNamespaceTracker>(asm, nsTracker));
            }
        }

        /// <summary>
        /// Gets a builtin value
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal Namespace GetBuiltin(string name) {
            return _builtinModule.VariableDict[name].First();
        }

        internal T GetCached<T>(object key, Func<T> maker) where T : class {
            object result;
            if (!_itemCache.TryGetValue(key, out result)) {
                _itemCache[key] = result = maker();
            } else {
                Debug.Assert(result is T);
            }
            return (result as T);
        }

        internal object[] GetReflectedNamespaces(IList<string> names, bool bottom) {
            if (names == null || names.Count == 0) {
                return null;
            }
            var topName = names[0];

            if (String.IsNullOrEmpty(topName)) {
                // user typed "import."
                return null;
            }

            var builtinRefs = new List<object>();
            foreach (var asm in _references) {
                object attr = NamespaceTrackerOps.GetCustomMember(_codeContext, asm.Value, topName);
                if (attr != null && attr != OperationFailed.Value) {
                    if (bottom) {
                        for (int i = 1; i < names.Count; i++) {
                            if (names[i] != null) {
                                object nextAttr;
                                if (!TryGetMember(attr, names[i], out nextAttr) || nextAttr == null) {
                                    attr = null;
                                    break;
                                }
                                attr = nextAttr;
                            }
                        }
                    }
                    if (attr != null) {
                        builtinRefs.Add(attr);
                    }
                }
            }

            return builtinRefs.ToArray();
        }

        internal BuiltinModule BuiltinModule {
            get { return _builtinModule; }
        }

        internal T GetMember<T>(object obj, string name) {
            return (T)Builtin.getattr(_codeContext, obj, name);
        }

        private bool TryGetMember(CodeContext codeContext, object obj, string name, out object value) {
            NamespaceTracker nt = obj as NamespaceTracker;
            if (nt != null) {
                value = NamespaceTrackerOps.GetCustomMember(codeContext, nt, name);
                return value != OperationFailed.Value;
            }

            object result = Builtin.getattr(codeContext, obj, name, this);
            if (result == this) {
                value = null;
                return false;
            } else {
                value = result;
                return true;
            }
        }

        internal bool TryGetMember(object obj, string name, out object value) {
            return TryGetMember(_codeContext, obj, name, out value);
        }

        internal bool TryGetMember(object obj, string name, bool showClr, out object value) {
            var cctx = showClr ? _codeContextCls : _codeContext;
            return TryGetMember(cctx, obj, name, out value);
        }

        internal SourceUnit GetSourceUnitForExpression(string expr) {
            return StringTextContentProvider.Make(_pythonEngine, expr, SourceCodeKind.Expression);
        }

        internal CodeContext CodeContext {
            get { return _codeContext; }
        }

        internal CodeContext CodeContextCls {
            get { return _codeContextCls; }
        }

        internal DocumentationProvider DocProvider {
            get { return _docProvider; }
        }

        internal BuiltinInstanceInfo GetInstance(Type type) {
            return GetInstance(ClrModule.GetPythonType(type));
        }

        internal BuiltinInstanceInfo GetInstance(PythonType type) {
            return GetBuiltinType(type).Instance;
        }

        internal BuiltinClassInfo GetBuiltinType(Type type) {
            return GetBuiltinType(ClrModule.GetPythonType(type));
        }

        internal BuiltinClassInfo GetBuiltinType(PythonType type) {
            return GetCached(type, () => new BuiltinClassInfo(type, this));
        }

        internal Namespace GetNamespaceFromObjects(params object[] attrs) {
            return GetNamespaceFromObjects((IEnumerable<object>)attrs);
        }

        internal Namespace GetNamespaceFromObjects(IEnumerable<object> attrs) {
            List<Namespace> values = new List<Namespace>();
            foreach (var attr in attrs) {
                var attrType = (attr != null) ? attr.GetType() : typeof(DynamicNull);
                if (attr is PythonType) {
                    values.Add(GetBuiltinType((PythonType)attr));
                } else if (attr is BuiltinFunction) {
                    var bf = (BuiltinFunction)attr;
                    if (bf.__self__ == null) {
                        values.Add(GetCached(attr, () => new BuiltinFunctionInfo(bf, this)));
                    } else {
                        values.Add(new BuiltinFunctionInfo(bf, this));
                    }
                } else if (attrType == typeof(BuiltinMethodDescriptor)) {
                    values.Add(GetCached(attr, () => new BuiltinMethodInfo((BuiltinMethodDescriptor)attr, this)));
                } else if (attrType.IsEnum) {
                    values.Add(GetCached(attr, () => new EnumInstanceInfo(attr, this)));
                } else if (attrType == typeof(ReflectedProperty) || attrType == typeof(ReflectedExtensionProperty)) {
                    values.Add(GetCached(attr, () => new BuiltinPropertyInfo((ReflectedGetterSetter)attr, this)));
                } else if (attrType == typeof(ReflectedField)) {
                    values.Add(GetCached(attr, () => new BuiltinFieldInfo((ReflectedField)attr, this)));
                } else if (attrType == typeof(ReflectedEvent)) {
                    values.Add(GetCached(attr, () => new BuiltinEventInfo((ReflectedEvent)attr, this)));
                } else if (attrType == typeof(bool) || attrType == typeof(int) || attrType == typeof(Complex64)
                    || attrType == typeof(string) || attrType == typeof(long) || attrType == typeof(double) ||
                    attr == null) {
                    var varRef = GetConstant(attr);
                    foreach (var constant in varRef) {
                        values.Add(constant);
                    }
                } else if (attr is NamespaceTracker || attr is TypeGroup) { // TODO: Need to do better for TypeGroup's.
                    values.Add(GetCached(attr, () => new ReflectedNamespace(new[] { attr }, this)));
                } else {
                    var pyAattrType = DynamicHelpers.GetPythonType(attr);
                    values.Add(GetCached(pyAattrType, () => new BuiltinClassInfo(pyAattrType, this)).Instance);
                }
            }

            if (values.Count == 1) {
                return values[0];
            } else if (values.Count > 1) {
                return GetCached(new NamespaceKey(values.ToArray()), () => new ReflectedNamespace(attrs, this));
            } else {
                return null;
            }
        }

        class NamespaceKey : IEquatable<NamespaceKey> {
            private readonly Namespace[] _values;

            public NamespaceKey(Namespace[] values) {
                _values = values;
            }

            public override bool Equals(object obj) {
                if (!(obj is NamespaceKey)) {
                    return false;
                }

                return Equals((NamespaceKey)obj);
            }

            public override int GetHashCode() {
                int res = 0;
                foreach (Namespace n in _values) {
                    res ^= n.GetHashCode();
                }
                return res;
            }

            #region IEquatable<NamespaceKey> Members

            public bool Equals(NamespaceKey other) {
                if (other._values.Length != _values.Length) {
                    return false;
                }

                for (int i = 0; i < _values.Length; i++) {
                    if (_values[i] != other._values[i]) {
                        return false;
                    }
                }

                return true;
            }

            #endregion
        }

        internal Dictionary<string, ModuleReference> Modules {
            get { return _modules; }
        }

        internal Dictionary<string, ModuleInfo> ModulesByFilename {
            get { return _modulesByFilename; }
        }

        internal ISet<Namespace> GetConstant(object value) {
            object key = value ?? _nullKey;
            return GetCached<ISet<Namespace>>(key, () => new ConstantInfo(value, this).SelfSet);
        }

        internal BuiltinClassInfo MakeGenericType(Type clrType, params Type[] clrIndexTypes) {
            var genType = clrType.MakeGenericType(clrIndexTypes);
            var pyType = ClrModule.GetPythonType(genType);
            return GetCached(pyType, () => new BuiltinClassInfo(pyType, this));
        }

        private static void Update<K, V>(IDictionary<K, V> dict, IDictionary<K, V> newValues) {
            foreach (var kvp in newValues) {
                dict[kvp.Key] = kvp.Value;
            }
        }
    }
}
