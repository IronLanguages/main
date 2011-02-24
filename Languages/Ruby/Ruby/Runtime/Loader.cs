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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Conversions;

namespace IronRuby.Runtime {
    [Flags]
    public enum LoadFlags {
        None = 0,
        LoadOnce = 1,
        LoadIsolated = 2,
        AppendExtensions = 4,

        /// <summary>
        /// Returns a scope or assembly of already loaded targets.
        /// </summary>
        ResolveLoaded = 8,
        AnyLanguage = 16,

        Require = LoadOnce | AppendExtensions,
    }

    // TODO: thread safety
    public sealed class Loader {

        internal enum FileKind {
            RubySourceFile,
            NonRubySourceFile,
            Assembly,
            Type,
            Unknown,
        }

        private RubyContext/*!*/ _context;

        // $:
        private readonly RubyArray/*!*/ _loadPaths;

        // $"
        private readonly RubyArray/*!*/ _loadedFiles;

        // files that were required but their execution haven't completed yet:
        private readonly Stack<string>/*!*/ _unfinishedFiles;

        // lazy init
        private SynchronizedDictionary<string, Scope> _loadedScripts;

        // TODO: static
        // maps full normalized path to compiled code:
        private Dictionary<string, CompiledFile> _compiledFiles;
        private readonly object/*!*/ _compiledFileMutex = new object();

        private struct CompiledFile {
            public readonly ScriptCode/*!*/ CompiledCode;

            public CompiledFile(ScriptCode/*!*/ compiledCode) {
                Assert.NotNull(compiledCode);

                CompiledCode = compiledCode;
            }
        }

        // counters:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private int _cacheHitCount;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private int _compiledFileCount;

        /// <summary>
        /// TODO: Thread safety: the user of this object is responsible for locking it.
        /// </summary>
        public RubyArray/*!*/ LoadPaths {
            get { return _loadPaths; }
        }

        /// <summary>
        /// TODO: Thread safety: the user of this object is responsible for locking it.
        /// </summary>
        public RubyArray/*!*/ LoadedFiles {
            get { return _loadedFiles; }
        }

        /// <summary>
        /// Contains all loaded foreign language scripts. Maps path to scope created for each loaded script.
        /// A script is published here as soon as its scope is created just before it is executed.
        /// </summary>
        public IDictionary<string, Scope>/*!*/ LoadedScripts {
            get {
                if (_loadedScripts == null) {
                    Interlocked.CompareExchange(ref _loadedScripts, 
                        new SynchronizedDictionary<string, Scope>(new Dictionary<string, Scope>(DomainManager.Platform.PathComparer)), null
                    );
                }
                return _loadedScripts;
            }
        }

        private PlatformAdaptationLayer/*!*/ Platform {
            get { return DomainManager.Platform; }
        }

        private ScriptDomainManager/*!*/ DomainManager {
            get { return _context.DomainManager; }
        }

        internal Loader(RubyContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;

            _toStrStorage = new ConversionStorage<MutableString>(context);
            _loadPaths = MakeLoadPaths(context.RubyOptions);
            _loadedFiles = new RubyArray();
            _unfinishedFiles = new Stack<string>();

#if !SILVERLIGHT
            if (!context.RubyOptions.NoAssemblyResolveHook) {
                new AssemblyResolveHolder(this).HookAssemblyResolve();
            }
#endif
        }

        private RubyArray/*!*/ MakeLoadPaths(RubyOptions/*!*/ options) {
            var loadPaths = new RubyArray();
            
            if (options.HasSearchPaths) {
                foreach (string path in options.SearchPaths) {
                    loadPaths.Add(_context.EncodePath(path));
                }
            }

            AddStandardLibraryPath(loadPaths, options.StandardLibraryPath, options.ApplicationBase);
            // TODO: remove?
            loadPaths.Add(MutableString.CreateAscii("."));
            return loadPaths;
        }

        private void AddStandardLibraryPath(RubyArray/*!*/ loadPaths, string path, string applicationBaseDir) {
#if !SILVERLIGHT // no library paths on Silverlight
            bool isFullPath;
            if (path != null) {
                try {
                    isFullPath = Platform.IsAbsolutePath(path);
                } catch {
                    loadPaths.Add(_context.EncodePath(path));
                    return;
                }
            } else {
                path = "../Lib";
                isFullPath = false;
            }

            if (!isFullPath) {
                try {
                    if (String.IsNullOrEmpty(applicationBaseDir)) {
                        applicationBaseDir = _context.Platform.GetEnvironmentVariable(RubyContext.BinDirEnvironmentVariable);
                        if (!Directory.Exists(applicationBaseDir)) {
                            applicationBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                        }
                    }
                } catch (SecurityException) {
                    applicationBaseDir = null;
                }

                try {
                    path = Platform.GetFullPath(RubyUtils.CombinePaths(applicationBaseDir, path));
                } catch {
                    loadPaths.Add(_context.EncodePath(path));
                    return;
                }
            }

            path = path.Replace('\\', '/');
            loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ironruby")));
            loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ruby/site_ruby/" + _context.StandardLibraryVersion)));
            loadPaths.Add(_context.EncodePath(RubyUtils.CombinePaths(path, "ruby/" + _context.StandardLibraryVersion)));
#endif
        }

        private void AddAbsoluteLibraryPaths(RubyArray/*!*/ result, string applicationBaseDir, ICollection<string>/*!*/ paths) {
            foreach (var path in paths) {
                string fullPath;
                if (applicationBaseDir != null) {
                    try {
                        fullPath = Platform.IsAbsolutePath(path) ? path : Platform.GetFullPath(Path.Combine(applicationBaseDir, path));
                    } catch (Exception) {
                        // error will be reported on first require:
                        fullPath = path;
                    }
                } else {
                    fullPath = path;
                }
                result.Add(_context.EncodePath(fullPath.Replace('\\', '/')));
            }
        }

        private Dictionary<string, CompiledFile>/*!*/ LoadCompiledCode() {
            Debug.Assert(_context.RubyOptions.LoadFromDisk);

            Dictionary<string, CompiledFile> result = new Dictionary<string, CompiledFile>();
            Utils.Log("LOADING", "LOADER");

            ScriptCode[] codes = SavableScriptCode.LoadFromAssembly(_context.DomainManager,
                Assembly.Load(Path.GetFileName(_context.RubyOptions.MainFile))
            );

            for (int i = 0; i < codes.Length; i++) {
                string path = codes[i].SourceUnit.Path;
                string fullPath = Platform.GetFullPath(path);
                result[fullPath] = new CompiledFile(codes[i]);
            }

            return result;
        }

        internal void SaveCompiledCode() {
            string savePath = _context.RubyOptions.SavePath;
            if (savePath != null) {
                lock (_compiledFileMutex) {
                    var assemblyPath = Path.Combine(savePath, (Path.GetFileName(_context.RubyOptions.MainFile) ?? "snippets") + ".dll");

                    Utils.Log(String.Format("SAVING to {0}", Path.GetFullPath(assemblyPath)), "LOADER");

                    // TODO: allocate eagerly (as soon as config gets fixed)
                    if (_compiledFiles == null) {
                        _compiledFiles = new Dictionary<string, CompiledFile>();
                    }

                    SavableScriptCode[] codes = new SavableScriptCode[_compiledFiles.Count];
                    int i = 0;
                    foreach (CompiledFile file in _compiledFiles.Values) {
                        codes[i++] = (SavableScriptCode)file.CompiledCode;
                    }

                    SavableScriptCode.SaveToAssembly(assemblyPath, codes);
                }
            }
        }

        private bool TryGetCompiledFile(string/*!*/ fullPath, out CompiledFile compiledFile) {
            if (!_context.RubyOptions.LoadFromDisk) {
                compiledFile = default(CompiledFile);
                return false;
            }

            lock (_compiledFileMutex) {
                if (_compiledFiles == null) {
                    _compiledFiles = LoadCompiledCode();
                }

                return _compiledFiles.TryGetValue(fullPath, out compiledFile);
            }
        }

        private void AddCompiledFile(string/*!*/ fullPath, ScriptCode/*!*/ compiledCode) {
            if (_context.RubyOptions.SavePath != null) {
                lock (_compiledFileMutex) {
                    // TODO: allocate eagerly (as soon as config gets fixed)
                    if (_compiledFiles == null) {
                        _compiledFiles = new Dictionary<string, CompiledFile>();
                    }
                    _compiledFiles[fullPath] = new CompiledFile(compiledCode);
                }
            }
        }

        public bool LoadFile(Scope globalScope, object self, MutableString/*!*/ path, LoadFlags flags) {
            object loaded;
            return LoadFile(globalScope, self, path, flags, out loaded);
        }

        /// <summary>
        /// Returns <b>true</b> if a Ruby file is successfully loaded, <b>false</b> if it is already loaded.
        /// </summary>
        /// <param name="globalScope">
        /// A scope against which the file should be executed or null to create a new scope.
        /// </param>
        /// <returns>True if the file was loaded/executed by this call.</returns>
        public bool LoadFile(Scope globalScope, object self, MutableString/*!*/ path, LoadFlags flags, out object loaded) {
            Assert.NotNull(path);

            string assemblyName, typeName;

            string strPath = path.ConvertToString();
            if (TryParseAssemblyName(strPath, out typeName, out assemblyName)) {

                if (AlreadyLoaded(strPath, (string)null, flags)) {
                    loaded = ((flags & LoadFlags.ResolveLoaded) != 0) ? GetAssembly(assemblyName, true, false) : null;
                    return false;
                }

                Assembly assembly = LoadAssembly(assemblyName, typeName, false, false);
                if (assembly != null) {
                    FileLoaded(path.Clone(), flags);
                    loaded = assembly;
                    return true;
                }
            }

            return LoadFromPath(globalScope, self, strPath, path.Encoding, flags, out loaded);
        }

        #region Assemblies

        public Assembly LoadAssembly(string/*!*/ assemblyName, string typeName, bool throwOnError, bool tryPartialName) {
            Assembly assembly = GetAssembly(assemblyName, throwOnError, tryPartialName);
            return (assembly != null && LoadAssembly(assembly, typeName, throwOnError)) ? assembly : null;
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
        private Assembly GetAssembly(string/*!*/ assemblyName, bool throwOnError, bool tryPartialName) {
#if SILVERLIGHT
            tryPartialName = false;
#endif
            try {
                return Platform.LoadAssembly(assemblyName);
            } catch (Exception e) {
                if (!tryPartialName || !(e is FileNotFoundException)) {
                    if (throwOnError) {
                        throw RubyExceptions.CreateLoadError(e);
                    } else {
                        return null;
                    }
                }
            }

#if SILVERLIGHT
            throw Assert.Unreachable;
#else
#pragma warning disable 618,612 // csc, gmcs
            Assembly assembly;
            try { 
                assembly = Assembly.LoadWithPartialName(assemblyName);
            } catch (Exception e) {
                if (throwOnError) {
                    throw RubyExceptions.CreateLoadError(e);
                } else {
                    return null;
                }
            }
            if (assembly == null && throwOnError) {
                throw RubyExceptions.CreateLoadError(String.Format("Assembly '{0}' not found", assemblyName));
            }
#pragma warning restore 618,612
            return assembly;
#endif
        }
        
        private bool LoadAssembly(Assembly/*!*/ assembly, string typeName, bool throwOnError) {
            Utils.Log(String.Format("Loading assembly '{0}' and type '{1}'", assembly, typeName), "LOADER");
            Type initializerType;
            if (typeName != null) {
                // load Ruby library:
                try {
                    initializerType = assembly.GetType(typeName, true);
                } catch (Exception e) {
                    if (throwOnError) {
                        throw new LoadError(e.Message, e);
                    }
                    return false;
                }

                LoadLibrary(initializerType, false);
            } else {
                // load namespaces:
                try {
                    DomainManager.LoadAssembly(assembly);
                } catch (Exception e) {
                    if (throwOnError) {
                        throw RubyExceptions.CreateLoadError(e);
                    }
                    return false;
                }
            }

            return true;
        }

        private static Regex _AssemblyNameRegex = new Regex(@"
            \s*((?<type>[\w.+]+)\s*,)?\s* # type name
            (?<assembly>
              [^,=]+\s*                   # assembly name
              (,\s*[\w]+\s*=\s*[^,]+\s*)+ # properties
            )", 
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline
        );

        internal static bool TryParseAssemblyName(string/*!*/ path, out string typeName, out string assemblyName) {
            Match match = _AssemblyNameRegex.Match(path);
            if (match.Success) {
                Group typeGroup = match.Groups["type"];
                Group assemblyGroup = match.Groups["assembly"];
                Debug.Assert(assemblyGroup.Success);

                typeName = typeGroup.Success ? typeGroup.Value : null;
                assemblyName = assemblyGroup.Value;
                return true;
            }

            if (path.Trim() == "mscorlib") {
                typeName = null;
                assemblyName = path;
                return true;
            }

            typeName = null;
            assemblyName = null;
            return false;
        }

#if !SILVERLIGHT
        private sealed class AssemblyResolveHolder {
            private readonly WeakReference _loader;

            [ThreadStatic]
            private static HashSet<string> _assembliesBeingResolved;

            public AssemblyResolveHolder(Loader/*!*/ loader) {
                _loader = new WeakReference(loader);
            }

            internal void HookAssemblyResolve() {
                try {
                    HookAssemblyResolveInternal();
                } catch (System.Security.SecurityException) {
                    // We may not have SecurityPermissionFlag.ControlAppDomain. 
                }
            }

            private void HookAssemblyResolveInternal() {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEvent;
            }

            private Assembly AssemblyResolveEvent(object sender, ResolveEventArgs args) {
                Loader loader = (Loader)_loader.Target;
                if (loader != null) {
                    string assemblyName = args.Name;
                    Utils.Log(String.Format("assembly resolve event: {0}", assemblyName), "RESOLVE_ASSEMBLY");

                    if (_assembliesBeingResolved == null) {
                        _assembliesBeingResolved = new HashSet<string>();
                    } else if (_assembliesBeingResolved.Contains(assemblyName)) {
                        Utils.Log(String.Format("recursive assembly resolution: {0}", assemblyName), "RESOLVE_ASSEMBLY");
                        return null;
                    }

                    _assembliesBeingResolved.Add(assemblyName);
                    try {
                        return loader.ResolveAssembly(assemblyName);
                    } catch (Exception e) {
                        // the exception might not be reported by the type loader, so at least report a warning:
                        loader._context.ReportWarning(
                            String.Format("An exception was risen while resolving an assembly `{0}': {1}", assemblyName, e.Message)
                        );
                        throw;
                    } finally {
                        _assembliesBeingResolved.Remove(assemblyName);
                    }
                } else {
                    AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEvent;
                    return null;
                }
            }
        }

        internal Assembly ResolveAssembly(string/*!*/ fullName) {
            Utils.Log(String.Format("Resolving assembly: '{0}'", fullName), "RESOLVE_ASSEMBLY");
            
            AssemblyName assemblyName = new AssemblyName(fullName);
            ResolvedFile file = FindFile(assemblyName.Name, true, ArrayUtils.EmptyStrings).FirstOrDefault();
            if (file == null || file.SourceUnit != null) {
                return null;
            }

            Utils.Log(String.Format("Assembly '{0}' resolved: found in '{1}'", fullName, file.Path), "RESOLVE_ASSEMBLY");
            try {
                Assembly assembly = Platform.LoadAssemblyFromPath(file.Path);
                if (AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName())) {
                    Utils.Log(String.Format("Assembly '{0}' loaded for '{1}'", assembly.GetName(), fullName), "RESOLVE_ASSEMBLY");
                    DomainManager.LoadAssembly(assembly);
                    return assembly;
                }
            } catch (Exception e) {
                throw RubyExceptions.CreateLoadError(e);
            }

            return null;
        }

#endif
        #endregion

        private class ResolvedFile {
            public readonly SourceUnit SourceUnit;
            public readonly string/*!*/ Path;
            public readonly string AppendedExtension;

            public ResolvedFile(SourceUnit/*!*/ sourceUnit, string/*!*/ fullPath, string appendedExtension) {
                Assert.NotNull(sourceUnit, fullPath);
                SourceUnit = sourceUnit;
                Path = fullPath; 
                AppendedExtension = appendedExtension;
            }

            public ResolvedFile(string/*!*/ fullLibraryPath, string appendedExtension) {
                Assert.NotNull(fullLibraryPath);
                Path = fullLibraryPath;
                AppendedExtension = appendedExtension;
            }
        }

        private bool LoadFromPath(Scope globalScope, object self, string/*!*/ path, RubyEncoding/*!*/ pathEncoding, LoadFlags flags, out object loaded) {
            Assert.NotNull(pathEncoding, path);

            string[] sourceFileExtensions;
            if ((flags & LoadFlags.AnyLanguage) != 0) {
                sourceFileExtensions = DomainManager.Configuration.GetFileExtensions();
            } else {
                sourceFileExtensions = DomainManager.Configuration.GetFileExtensions(_context);
            }

            IList<ResolvedFile> files = FindFile(path, (flags & LoadFlags.AppendExtensions) != 0, sourceFileExtensions);

            if (files.Count == 0) {
                // MRI: doesn't throw an exception if the path is in $" (performs resolution first though):
                if (AlreadyLoaded(path, null, flags, sourceFileExtensions)) {
                    loaded = null;
                    return false;
                }
                throw RubyExceptions.CreateLoadError(String.Format("no such file to load -- {0}", path));
            }

            ResolvedFile file = files.First();

            string pathWithExtension = path;
            if (file.AppendedExtension != null) {
                pathWithExtension += file.AppendedExtension;
            }

            if (AlreadyLoaded(path, files, flags) || _unfinishedFiles.Contains(file.Path)) {
                if ((flags & LoadFlags.ResolveLoaded) != 0) {
                    if (file.SourceUnit != null) {
                        Scope loadedScope;
                        if (!LoadedScripts.TryGetValue(file.Path, out loadedScope)) {
                            throw RubyExceptions.CreateLoadError(String.Format("no such file to load -- {0}", file.Path));
                        }
                        loaded = loadedScope;
                    } else {
                        loaded = Platform.LoadAssemblyFromPath(file.Path);
                    }
                } else {
                    loaded = null;
                }
                return false;
            }

            try {
                // save path as is, no canonicalization nor combination with an extension or directory:
                _unfinishedFiles.Push(file.Path);

                if (file.SourceUnit != null) {
                    AddScriptLines(file.SourceUnit);

                    ScriptCode compiledCode;
                    if (file.SourceUnit.LanguageContext == _context) {
                        compiledCode = CompileRubySource(file.SourceUnit, flags);
                    } else {
                        compiledCode = file.SourceUnit.Compile();
                    }
                    loaded = Execute(globalScope, compiledCode);
                } else {
                    Debug.Assert(file.Path != null);
                    try {
                        Assembly assembly = Platform.LoadAssemblyFromPath(file.Path);
                        DomainManager.LoadAssembly(assembly);
                        loaded = assembly;
                    } catch (Exception e) {
                        throw RubyExceptions.CreateLoadError(e);
                    }
                }

                FileLoaded(MutableString.Create(file.Path, pathEncoding), flags);
            } finally {
                _unfinishedFiles.Pop();
            }

            return true;
        }

        private ScriptCode/*!*/ CompileRubySource(SourceUnit/*!*/ sourceUnit, LoadFlags flags) {
            Assert.NotNull(sourceUnit);
            
            // TODO: check file timestamp
            string fullPath = Platform.GetFullPath(sourceUnit.Path);
            CompiledFile compiledFile;
            if (TryGetCompiledFile(fullPath, out compiledFile)) {
                Utils.Log(String.Format("{0}: {1}", ++_cacheHitCount, sourceUnit.Path), "LOAD_CACHED");

                return compiledFile.CompiledCode;
            } else {
                Utils.Log(String.Format("{0}: {1}", ++_compiledFileCount, sourceUnit.Path), "LOAD_COMPILED");

                RubyCompilerOptions options = new RubyCompilerOptions(_context.RubyOptions) {
                    FactoryKind = (flags & LoadFlags.LoadIsolated) != 0 ? TopScopeFactoryKind.WrappedFile : TopScopeFactoryKind.File
                };

                ScriptCode compiledCode = sourceUnit.Compile(options, _context.RuntimeErrorSink);
                AddCompiledFile(fullPath, compiledCode);
                return compiledCode;
            }
        }

        internal Scope Execute(Scope globalScope, ScriptCode/*!*/ code) {
            if (globalScope == null || code.LanguageContext != _context) {
                if (globalScope == null) {
                    globalScope = code.CreateScope();
                }

                if (code.SourceUnit.Path != null) {
                    LoadedScripts[Platform.GetFullPath(code.SourceUnit.Path)] = globalScope;
                }
                code.Run(globalScope);
                return globalScope;
            } else {
                code.Run(globalScope);
                return null;
            }
        }

        /// <summary>
        /// Searches file in load directories and then appends extensions.
        /// </summary>
        private IList<ResolvedFile> FindFile(string/*!*/ path, bool appendExtensions, string[] sourceFileExtensions) {
            Assert.NotNull(path);
            bool isAbsolutePath;

#if SILVERLIGHT
            {
#else
            if (path.StartsWith("~/", StringComparison.Ordinal) || path.StartsWith("~\\", StringComparison.Ordinal)) {
                path = RubyUtils.ExpandPath(_context.Platform, path);
                isAbsolutePath = true;
            } else {
#endif
                try {
                    isAbsolutePath = Platform.IsAbsolutePath(path);
                } catch (ArgumentException e) {
                    throw RubyExceptions.CreateLoadError(e);
                }
            }

            string extension = RubyUtils.GetExtension(path);

            // Absolute path -> load paths not consulted.
            if (isAbsolutePath) {
                var file = ResolveFile(path, extension, appendExtensions, sourceFileExtensions);
                return file != null ? new[] { file } : new ResolvedFile[0];
            }

            string[] loadPaths = GetLoadPathStrings();

            if (loadPaths.Length == 0) {
                return new ResolvedFile[0];
            }

            // If load paths are non-empty and the path starts with .\ or ..\ then MRI also ignores the load paths.
            if (path.StartsWith("./", StringComparison.Ordinal) ||
                path.StartsWith("../", StringComparison.Ordinal) ||
                path.StartsWith(".\\", StringComparison.Ordinal) ||
                path.StartsWith("..\\", StringComparison.Ordinal)) {

                var file = ResolveFile(path, extension, appendExtensions, sourceFileExtensions);
                return file != null ? new[] { file } : new ResolvedFile[0];
            }

            var result = new List<ResolvedFile>();
            foreach (var dir in loadPaths) {
                ResolvedFile file = ResolveFile(RubyUtils.CombinePaths(dir, path), extension, appendExtensions, sourceFileExtensions);
                if (file != null) {
                    result.Add(file);
                }
            }

            return result;
        }

        internal string[]/*!*/ GetLoadPathStrings() {
            var loadPaths = GetLoadPaths();
            var result = new string[loadPaths.Length];
            var toPath = _toStrStorage.GetSite(CompositeConversionAction.Make(_context, CompositeConversion.ToPathToStr));

            for (int i = 0; i < loadPaths.Length; i++) {
                if (loadPaths[i] == null) {
                    throw RubyExceptions.CreateTypeConversionError("nil", "String");
                }

                result[i] = Protocols.CastToPath(toPath, loadPaths[i]).ConvertToString();
            }

            return result;
        }

        /// <summary>
        /// Appends extensions if applicable.
        /// </summary>
        private ResolvedFile ResolveFile(string/*!*/ path, string/*!*/ extension, bool appendExtensions, string[]/*!*/ knownExtensions) {
            Debug.Assert(RubyUtils.GetExtension(path) == extension);

            string expandedPath = RubyUtils.ExpandPath(_context.Platform, path);

            // MRI doesn't load file w/o .rb extension:
            if (IsKnownExtension(extension, knownExtensions)) {
                return GetSourceUnit(path, expandedPath, extension, false);
            } 
            
            if (_LibraryExtensions.IndexOf(extension, DlrConfiguration.FileExtensionComparer) != -1) {
                if (Platform.FileExists(expandedPath)) {
                    return new ResolvedFile(expandedPath, null);
                }
            } else if (!appendExtensions) {
                return GetSourceUnit(path, expandedPath, extension, false);
            }

            if (appendExtensions) {
                List<string> matchingExtensions = GetExtensionsOfExistingFiles(expandedPath, knownExtensions);

                if (matchingExtensions.Count == 1) {
                    return GetSourceUnit(path + matchingExtensions[0], expandedPath + matchingExtensions[0], matchingExtensions[0], true);
                } else if (matchingExtensions.Count > 1) {
                    Exception e = new AmbiguousFileNameException(expandedPath + matchingExtensions[0], expandedPath + matchingExtensions[1]);
                    throw RubyExceptions.CreateLoadError(e);
                }

                foreach (string libExtension in _LibraryExtensions) {
                    if (Platform.FileExists(expandedPath + libExtension)) {
                        return new ResolvedFile(expandedPath + libExtension, libExtension);
                    }
                }
            }

            return null;
        }

        private static readonly string[] _LibraryExtensions = new string[] { ".dll", ".so", ".exe" };

        private static bool IsKnownExtension(string/*!*/ extension, string[]/*!*/ knownExtensions) {
            return extension.Length > 0 && knownExtensions.IndexOf(extension, DlrConfiguration.FileExtensionComparer) >= 0;
        }

        private ResolvedFile GetSourceUnit(string/*!*/ path, string/*!*/ fullPath, string/*!*/ extension, bool extensionAppended) {
            Assert.NotNull(path, fullPath, extension);

            LanguageContext language;
            if (extension.Length == 0 || !DomainManager.TryGetLanguageByFileExtension(extension, out language)) {
                // Ruby by default:
                language = _context;
            }

            if (!DomainManager.Platform.FileExists(fullPath)) {
                return null;
            }

            var sourceUnit = language.CreateFileUnit(path, RubyEncoding.Ascii.Encoding, SourceCodeKind.File);
            return new ResolvedFile(sourceUnit, fullPath, extensionAppended ? extension : null);
        }

        private List<string>/*!*/ GetExtensionsOfExistingFiles(string/*!*/ path, IEnumerable<string>/*!*/ extensions) {
            // all extensions that could be appended to the path to get an existing file:
            List<string> result = new List<string>();
            foreach (string extension in extensions) {
                Debug.Assert(extension != null && extension.StartsWith("."));
                string fullPath = path + extension;
                if (Platform.FileExists(fullPath)) {
                    result.Add(extension);
                }
            }
            return result;
        }

        #region Global Variables

        private readonly ConversionStorage<MutableString>/*!*/ _toStrStorage;

        internal object[]/*!*/ GetLoadPaths() {
            lock (_loadedFiles) {
                return _loadPaths.ToArray();
            }
        }

        public void SetLoadPaths(IEnumerable<string/*!*/>/*!*/ paths) {
            ContractUtils.RequiresNotNullItems(paths, "paths");

            lock (_loadPaths) {
                _loadPaths.Clear();
                foreach (string path in paths) {
                    _loadPaths.Add(_context.EncodePath(path));
                }
            }
        }

        internal void AddLoadPaths(IEnumerable<string/*!*/>/*!*/ paths) {
            Assert.NotNullItems(paths);

            lock (_loadPaths) {
                foreach (string path in paths) {
                    _loadPaths.Add(_context.EncodePath(path));
                }
            }
        }

        internal void InsertLoadPaths(IEnumerable<string/*!*/>/*!*/ paths, int index) {
            Assert.NotNullItems(paths);

            lock (_loadPaths) {
                foreach (string path in paths) {
                    _loadPaths.Insert(0, _context.EncodePath(path));
                }
            }
        }

        internal void InsertLoadPaths(IEnumerable<string/*!*/>/*!*/ paths) {
            InsertLoadPaths(paths, 0);
        }

        private void AddLoadedFile(MutableString/*!*/ path) {
            lock (_loadedFiles) {
                _loadedFiles.Add(path);
            }
        }

        private void FileLoaded(MutableString/*!*/ path, LoadFlags flags) {
            if ((flags & LoadFlags.LoadOnce) != 0) {
                AddLoadedFile(path);
            }
        }

        /// <summary>
        /// If the SCRIPT_LINES__ constant is set, we need to publish the file being loaded,
        /// along with the contents of the file
        /// </summary>
        private void AddScriptLines(SourceUnit file) {            
            ConstantStorage storage;
            if (!_context.ObjectClass.TryResolveConstant(null, "SCRIPT_LINES__", out storage)) {
                return;
            }

            IDictionary scriptLines = storage.Value as IDictionary;
            if (scriptLines == null) {
                return;
            }

            lock (scriptLines) {
                // Read in the contents of the file

                RubyArray lines = new RubyArray();
                SourceCodeReader reader = file.GetReader();
                RubyEncoding encoding = RubyEncoding.GetRubyEncoding(reader.Encoding);
                using (reader) {
                    reader.SeekLine(1);
                    while (true) {
                        string lineStr = reader.ReadLine();
                        if (lineStr == null) {
                            break;
                        }
                        MutableString line = MutableString.CreateMutable(lineStr.Length + 1, encoding);
                        line.Append(lineStr).Append('\n');
                        lines.Add(line);
                    }
                }

                // Publish the contents of the file, keyed by the file name
                MutableString path = MutableString.Create(file.Document.FileName, _context.GetPathEncoding());
                scriptLines[path] = lines;
            }
        }

        internal object[]/*!*/ GetLoadedFiles() {
            lock (_loadedFiles) {
                return _loadedFiles.ToArray();
            }
        }

        private bool AlreadyLoaded(string/*!*/ path, string fullPath, LoadFlags flags) {
            return AlreadyLoaded(path, fullPath, flags, ArrayUtils.EmptyStrings);
        }
        
        private bool AlreadyLoaded(string/*!*/ path, string fullPath, LoadFlags flags, string[]/*!*/ sourceFileExtensions) {
            Debug.Assert(fullPath == null || RubyUtils.GetExtension(path) == RubyUtils.GetExtension(fullPath));
            return (flags & LoadFlags.LoadOnce) != 0 && AnyFileLoaded(GetPathsToTestLoaded(path, fullPath, flags, sourceFileExtensions));
        }

        private IEnumerable<MutableString>/*!*/ GetPathsToTestLoaded(string/*!*/ path, string fullPath, LoadFlags flags, string[]/*!*/ sourceFileExtensions) {
            List<MutableString> paths = new List<MutableString>();
            paths.Add(_context.EncodePath(path));

            if (fullPath != null) {
                paths.Add(_context.EncodePath(path));
            }

            if ((flags & LoadFlags.AppendExtensions) != 0 && RubyUtils.GetExtension(path).Length == 0) {
                foreach (var extension in sourceFileExtensions) {
                    paths.Add(_context.EncodePath(path + extension));
                }
                foreach (var extension in _LibraryExtensions) {
                    paths.Add(_context.EncodePath(path + extension));
                }
            }

            return paths;
        }

        /// <summary>
        /// Return true if any of the files has alraedy been loaded.
        /// </summary>
        private bool AlreadyLoaded(string/*!*/ path, IEnumerable<ResolvedFile>/*!*/ files, LoadFlags flags) {
            return (flags & LoadFlags.LoadOnce) != 0 && AnyFileLoaded(
                new[] { _context.EncodePath(path) }.Concat(files.Select((file) => _context.EncodePath(file.Path)))
            );
        }

        private bool AnyFileLoaded(IEnumerable<MutableString>/*!*/ paths) {
            var toPath = _toStrStorage.GetSite(CompositeConversionAction.Make(_context, CompositeConversion.ToPathToStr));

            foreach (object file in GetLoadedFiles()) {
                if (file == null) {
                    throw RubyExceptions.CreateTypeConversionError("nil", "String");
                }

                // use case sensitive comparison
                MutableString loadedPath = Protocols.CastToPath(toPath, file);
                if (paths.Any((path) => loadedPath.Equals(path))) {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IronRuby Libraries

        /// <exception cref="LoadError"></exception>
        internal void LoadBuiltins() {
            Type initializerType;
            try {
                Assembly assembly = _context.DomainManager.Platform.LoadAssembly(GetIronRubyAssemblyLongName("IronRuby.Libraries"));
                initializerType = assembly.GetType(LibraryInitializer.GetBuiltinsFullTypeName());
            } catch (Exception e) {
                throw RubyExceptions.CreateLoadError(e);
            }

            LoadLibrary(initializerType, true);
        }

        public static string/*!*/ GetIronRubyAssemblyLongName(string/*!*/ baseName) {
            ContractUtils.RequiresNotNull(baseName, "baseName");
            string fullName = typeof(RubyContext).Assembly.FullName;
            int firstComma = fullName.IndexOf(',');
            return firstComma > 0 ? baseName + fullName.Substring(firstComma) : baseName;
        }

        private readonly HashSet<Type> _loadedTypes = new HashSet<Type>();

        /// <exception cref="LoadError"></exception>
        private void LoadLibrary(Type/*!*/ initializerType, bool builtin) {
            lock (_loadedTypes) {
                if (_loadedTypes.Contains(initializerType)) {
                    return;
                }
            }
            
            LibraryInitializer initializer;
            try {
                initializer = Activator.CreateInstance(initializerType) as LibraryInitializer;
            } catch (TargetInvocationException e) {
                throw RubyExceptions.CreateLoadError(e.InnerException);
            } catch (Exception e) {
                throw RubyExceptions.CreateLoadError(e);
            }

            if (initializer == null) {
                throw RubyExceptions.CreateLoadError(String.Format("Specified type {0} is not a subclass of {1}", 
                    initializerType.FullName,
                    typeof(LibraryInitializer).FullName)
                );
            }

            // Propagate exceptions from initializers (do not wrap them to LoadError).
            // E.g. TypeError (can't modify frozen module) can be thrown.
            initializer.LoadModules(_context, builtin);

            lock (_loadedTypes) {
                _loadedTypes.Add(initializerType);
            }
        }

        #endregion
    }
}
