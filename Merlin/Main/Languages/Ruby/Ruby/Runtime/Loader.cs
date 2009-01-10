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
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    [Flags]
    public enum LoadFlags {
        None = 0,
        LoadOnce = 1,
        LoadIsolated = 2,
        AppendExtensions = 4,
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

        internal static long _ILGenerationTimeTicks;
        internal static long _ScriptCodeGenerationTimeTicks;

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

        private PlatformAdaptationLayer/*!*/ Platform {
            get { return DomainManager.Platform; }
        }

        private ScriptDomainManager/*!*/ DomainManager {
            get { return _context.DomainManager; }
        }
        
        internal Loader(RubyContext/*!*/ context) {
            Assert.NotNull(context);
            _context = context;

            _loadPaths = MakeLoadPaths(context.RubyOptions);
            _loadedFiles = new RubyArray();
            _unfinishedFiles = new Stack<string>();
        }

        private RubyArray/*!*/ MakeLoadPaths(RubyOptions/*!*/ options) {
            var loadPaths = new RubyArray();
            
            if (options.HasSearchPaths) {
                foreach (string path in options.SearchPaths) {
                    loadPaths.Add(MutableString.Create(path.Replace('\\', '/')));
                }
            }
            
#if !SILVERLIGHT // no library paths on Silverlight
            string applicationBaseDir;
            try {
                applicationBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            } catch (SecurityException) {
                applicationBaseDir = null;
            }
            
            AddAbsoluteLibraryPaths(loadPaths, applicationBaseDir, options.LibraryPaths);
#endif
            loadPaths.Add(MutableString.Create("."));
            return loadPaths;
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
                result.Add(MutableString.Create(fullPath.Replace('\\', '/')));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom")] // TODO
        private Dictionary<string, CompiledFile>/*!*/ LoadCompiledCode() {
            Debug.Assert(_context.RubyOptions.LoadFromDisk);

            Dictionary<string, CompiledFile> result = new Dictionary<string, CompiledFile>();
            Utils.Log("LOADING", "LOADER");

            ScriptCode[] codes = ScriptCode.LoadFromAssembly(_context.DomainManager,
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
                    var assemblyPath = Path.Combine(savePath, Path.GetFileName(_context.RubyOptions.MainFile) + ".dll");

                    Utils.Log(String.Format("SAVING to {0}", Path.GetFullPath(assemblyPath)), "LOADER");

                    // TODO: allocate eagerly (as soon as config gets fixed)
                    if (_compiledFiles == null) {
                        _compiledFiles = new Dictionary<string, CompiledFile>();
                    }

                    ScriptCode[] codes = new ScriptCode[_compiledFiles.Count];
                    int i = 0;
                    foreach (CompiledFile file in _compiledFiles.Values) {
                        codes[i++] = file.CompiledCode;
                    }

                    ScriptCode.SaveToAssembly(assemblyPath, codes);
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

        /// <summary>
        /// Returns <b>true</b> if a Ruby file is successfully loaded, <b>false</b> if it is already loaded.
        /// </summary>
        public bool LoadFile(Scope globalScope, object self, MutableString/*!*/ path, LoadFlags flags) {
            Assert.NotNull(path);

            string assemblyName, typeName;

            string strPath = path.ConvertToString();
            if (TryParseAssemblyName(strPath, out typeName, out assemblyName)) {

                if (AlreadyLoaded(path, flags)) {
                    return false;
                }

                if (LoadAssembly(assemblyName, typeName, false)) {
                    FileLoaded(path, flags);
                    return true;
                }
            }

            return LoadFromPath(globalScope, self, strPath, flags);
        }

        public bool LoadAssembly(string/*!*/ assemblyName, string typeName, bool throwOnError) {
            Utils.Log(String.Format("Loading assembly '{0}' and type '{1}'", assemblyName, typeName), "LOADER");
            
            Assembly assembly;
            try {
                assembly = Platform.LoadAssembly(assemblyName);
            } catch (Exception e) {
                if (throwOnError) throw new LoadError(e.Message, e);
                return false;
            }

            Type initializerType;
            if (typeName != null) {
                // load Ruby library:
                try {
                    initializerType = assembly.GetType(typeName, true);
                } catch (Exception e) {
                    if (throwOnError) throw new LoadError(e.Message, e);
                    return false;
                }

                LoadLibrary(initializerType, false);
            } else {
                // load namespaces:
                try {
                    DomainManager.LoadAssembly(assembly);
                } catch (Exception e) {
                    if (throwOnError) throw new LoadError(e.Message, e);
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

        private class ResolvedFile {
            public readonly SourceUnit SourceUnit;
            public readonly string/*!*/ Path;
            public readonly string AppendedExtension;

            public ResolvedFile(SourceUnit/*!*/ sourceUnit, string appendedExtension) { 
                SourceUnit = sourceUnit; 
                Path = sourceUnit.Path; 
                AppendedExtension = appendedExtension;
            }

            public ResolvedFile(string/*!*/ libraryPath, string appendedExtension) { 
                Assert.NotNull(libraryPath); 
                Path = libraryPath;
                AppendedExtension = appendedExtension;
            }
        }

        private bool LoadFromPath(Scope globalScope, object self, string/*!*/ path, LoadFlags flags) {
            Assert.NotNull(path);

            ResolvedFile file = FindFile(path, (flags & LoadFlags.AppendExtensions) != 0);
            if (file == null) {
                throw new LoadError(String.Format("no such file to load -- {0}", path));
            }

            MutableString pathWithExtension = MutableString.Create(path);
            if (file.AppendedExtension != null) {
                pathWithExtension.Append(file.AppendedExtension);
            }

            if (AlreadyLoaded(pathWithExtension, flags) || _unfinishedFiles.Contains(pathWithExtension.ToString())) {
                return false;
            }

            try {
                // save path as is, no canonicalization nor combination with an extension or directory:
                _unfinishedFiles.Push(pathWithExtension.ToString());

                if (file.SourceUnit != null) {

                    RubyContext rubySource = file.SourceUnit.LanguageContext as RubyContext;
                    if (rubySource != null) {
                        ExecuteRubySourceUnit(file.SourceUnit, globalScope, flags);
                    } else {
                        file.SourceUnit.Execute();
                    }
                } else {
                    Debug.Assert(file.Path != null);
                    try {
                        Assembly asm = Platform.LoadAssemblyFromPath(Platform.GetFullPath(file.Path));
                        DomainManager.LoadAssembly(asm);
                    } catch (Exception e) {
                        throw new LoadError(e.Message, e);
                    }
                }

                FileLoaded(pathWithExtension, flags);
            } finally {
                _unfinishedFiles.Pop();
            }

            return true;
        }

        private void ExecuteRubySourceUnit(SourceUnit/*!*/ sourceUnit, Scope globalScope, LoadFlags flags) {
            Assert.NotNull(sourceUnit);
            
            // TODO: check file timestamp
            string fullPath = Platform.GetFullPath(sourceUnit.Path);
            CompiledFile compiledFile;
            if (TryGetCompiledFile(fullPath, out compiledFile)) {
                Utils.Log(String.Format("{0}: {1}", ++_cacheHitCount, sourceUnit.Path), "LOAD_CACHED");
                if (globalScope != null) {
                    compiledFile.CompiledCode.Run(globalScope);
                } else {
                    compiledFile.CompiledCode.Run();
                }
            } else {
                Utils.Log(String.Format("{0}: {1}", ++_compiledFileCount, sourceUnit.Path), "LOAD_COMPILED");

                RubyCompilerOptions options = new RubyCompilerOptions(_context.RubyOptions) {
                    FactoryKind = (flags & LoadFlags.LoadIsolated) != 0 ? TopScopeFactoryKind.WrappedFile : TopScopeFactoryKind.Default
                };

                long ts1 = Stopwatch.GetTimestamp();
                ScriptCode compiledCode = sourceUnit.Compile(options, _context.RuntimeErrorSink);
                long ts2 = Stopwatch.GetTimestamp();
                Interlocked.Add(ref _ScriptCodeGenerationTimeTicks, ts2 - ts1);

                AddCompiledFile(fullPath, compiledCode);

                CompileAndRun(globalScope, compiledCode, _context.Options.InterpretedMode);
            }
        }

        internal object CompileAndRun(Scope globalScope, ScriptCode/*!*/ code, bool tryEvaluate) {
            long ts1 = Stopwatch.GetTimestamp();
            code.EnsureCompiled();
            long ts2 = Stopwatch.GetTimestamp();
            Interlocked.Add(ref _ILGenerationTimeTicks, ts2 - ts1);

            return globalScope != null ? code.Run(globalScope) : code.Run();
        }

        private ResolvedFile FindFile(string/*!*/ path, bool appendExtensions) {
            Assert.NotNull(path);
            bool isAbsolutePath;
            string extension;
            string home = null;

#if !SILVERLIGHT
            if (path.StartsWith("~/") || path.StartsWith("~\\")) {
                try {
                    home = Environment.GetEnvironmentVariable("HOME");
                } catch (SecurityException) {
                    home = null;
                }

                if (home == null) {
                    throw RubyExceptions.CreateArgumentError(String.Format("couldn't find HOME environment -- expanding `{0}'", path));
                }
            }
#endif

            try {
                if (home != null) {
                    path = Path.Combine(home, path.Substring(2));
                }

                isAbsolutePath = Platform.IsAbsolutePath(path);
                extension = Path.GetExtension(path);
            } catch (ArgumentException e) {
                throw new LoadError(e.Message, e);
            }

            string[] knownExtensions = DomainManager.Configuration.GetFileExtensions();
            Array.Sort(knownExtensions, DlrConfiguration.FileExtensionComparer);

            // Absolute path -> load paths not consulted.
            if (isAbsolutePath) {
                return ResolveFile(path, extension, appendExtensions, knownExtensions);
            }

            string[] loadPaths = GetLoadPathStrings();

            if (loadPaths.Length == 0) {
                return null;
            }

            // If load paths are non-empty and the path starts with .\ or ..\ then MRI also ignores the load paths.
            if (path.StartsWith("./") || path.StartsWith("../") || path.StartsWith(".\\") || path.StartsWith("..\\")) {
                return ResolveFile(path, extension, appendExtensions, knownExtensions);
            }

            foreach (var dir in loadPaths) {
                try {
                    ResolvedFile result = ResolveFile(Path.Combine(dir, path), extension, appendExtensions, knownExtensions);
                    if (result != null) {
                        return result;
                    }
                } catch (ArgumentException) {
                    // invalid characters in path
                }
            }

            return null;
        }

        internal string[]/*!*/ GetLoadPathStrings() {
            var loadPaths = GetLoadPaths();
            var result = new string[loadPaths.Length];

            for (int i = 0; i < loadPaths.Length; i++) {
                if (loadPaths[i] == null) {
                    throw RubyExceptions.CreateTypeConversionError("nil", "String");
                }

                result[i] = _toStrSite.Target(_toStrSite, _context, loadPaths[i]).ConvertToString();
            }

            return result;
        }

        private ResolvedFile ResolveFile(string/*!*/ path, string/*!*/ extension, bool appendExtensions, string[]/*!*/ knownExtensions) {
            Debug.Assert(Path.GetExtension(path) == extension);

            // MRI doesn't load file w/o .rb extension:
            if (IsKnownExtension(extension, knownExtensions)) {
                return GetSourceUnit(path, extension, false);
            } else if (Utils.Array.IndexOf(_LibraryExtensions, extension, DlrConfiguration.FileExtensionComparer) != -1) {
                if (Platform.FileExists(path)) {
                    return new ResolvedFile(path, null);
                }
            } else if (!appendExtensions) {
                return GetSourceUnit(path, extension, false);
            }

            if (appendExtensions) {
                List<string> matchingExtensions = GetExtensionsOfExistingFiles(path, knownExtensions);

                if (matchingExtensions.Count == 1) {
                    return GetSourceUnit(path + matchingExtensions[0], matchingExtensions[0], true);
                } else if (matchingExtensions.Count > 1) {
                    Exception e = new AmbiguousFileNameException(path + matchingExtensions[0], path + matchingExtensions[1]);
                    throw new LoadError(e.Message, e);
                }

                foreach (string libExtension in _LibraryExtensions) {
                    if (Platform.FileExists(path + libExtension)) {
                        return new ResolvedFile(path + libExtension, libExtension);
                    }
                }
            }

            return null;
        }

        private static readonly string[] _LibraryExtensions = new string[] { ".dll", ".so", ".exe" };

        private static bool IsKnownExtension(string/*!*/ extension, string[]/*!*/ knownExtensions) {
            return extension.Length > 0 && Array.BinarySearch(knownExtensions, extension, DlrConfiguration.FileExtensionComparer) >= 0;
        }

        private ResolvedFile GetSourceUnit(string/*!*/ path, string/*!*/ extension, bool extensionAppended) {
            Assert.NotNull(path, extension);

            LanguageContext language;
            if (extension.Length == 0 || !DomainManager.TryGetLanguageByFileExtension(extension, out language)) {
                // Ruby by default:
                language = _context;
            }

            if (!DomainManager.Platform.FileExists(path)) {
                return null;
            }

            // TODO: default encoding:
            var sourceUnit = _context.CreateFileUnit(path, BinaryEncoding.Instance, SourceCodeKind.File);
            return new ResolvedFile(sourceUnit, extensionAppended ? extension : null);
        }

        private List<string>/*!*/ GetExtensionsOfExistingFiles(string/*!*/ path, IEnumerable<string>/*!*/ extensions) {
            // all extensions that could be appended to the path to get an sexisting file:
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

        private readonly CallSite<Func<CallSite, RubyContext, object, MutableString>> _toStrSite = 
            CallSite<Func<CallSite, RubyContext, object, MutableString>>.Create(ConvertToStrAction.Instance);

        internal object[]/*!*/ GetLoadPaths() {
            lock (_loadedFiles) {
                object[] result = new object[_loadPaths.Count];
                _loadPaths.CopyTo(result);
                return result;
            }
        }

        public void SetLoadPaths(IEnumerable<string/*!*/>/*!*/ paths) {
            ContractUtils.RequiresNotNullItems(paths, "paths");

            lock (_loadPaths) {
                _loadPaths.Clear();
                foreach (string path in paths) {
                    _loadPaths.Add(MutableString.Create(path));
                }
            }
        }

        internal void AddLoadPaths(IEnumerable<string/*!*/>/*!*/ paths) {
            Assert.NotNullItems(paths);

            lock (_loadPaths) {
                foreach (string path in paths) {
                    _loadPaths.Add(MutableString.Create(path));
                }
            }
        }

        internal void InsertLoadPaths(IEnumerable<string/*!*/>/*!*/ paths, int index) {
            Assert.NotNullItems(paths);

            lock (_loadPaths) {
                foreach (string path in paths) {
                    _loadPaths.Insert(0, MutableString.Create(path));
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

        internal object[]/*!*/ GetLoadedFiles() {
            lock (_loadedFiles) {
                object[] result = new object[_loadedFiles.Count];
                _loadedFiles.CopyTo(result);
                return result;
            }
        }

        private bool AlreadyLoaded(MutableString/*!*/ path, LoadFlags flags) {
            return (flags & LoadFlags.LoadOnce) != 0 && IsFileLoaded(path);
        }

        private void FileLoaded(MutableString/*!*/ path, LoadFlags flags) {
            if ((flags & LoadFlags.LoadOnce) != 0) {
                AddLoadedFile(path);
            }
        }

        private bool IsFileLoaded(MutableString/*!*/ path) {
            foreach (object file in GetLoadedFiles()) {
                if (file == null) {
                    throw RubyExceptions.CreateTypeConversionError("nil", "String");
                }

                // case sensitive comparison:
                if (path.Equals(_toStrSite.Target(_toStrSite, _context, file))) {
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
                throw new LoadError(e.Message, e);
            }

            LoadLibrary(initializerType, true);
        }

        public static string/*!*/ GetIronRubyAssemblyLongName(string/*!*/ baseName) {
            ContractUtils.RequiresNotNull(baseName, "baseName");
            string fullName = typeof(RubyContext).Assembly.FullName;
            int firstComma = fullName.IndexOf(',');
            return firstComma > 0 ? baseName + fullName.Substring(firstComma) : baseName;
        }

        /// <exception cref="LoadError"></exception>
        private void LoadLibrary(Type/*!*/ initializerType, bool builtin) {
            LibraryInitializer initializer;
            try {
                initializer = Activator.CreateInstance(initializerType) as LibraryInitializer;
            } catch (TargetInvocationException e) {
                throw new LoadError(e.Message, e);
            } catch (Exception e) {
                throw new LoadError(e.Message, e);
            }

            if (initializer == null) {
                throw new LoadError(String.Format("Specified type {0} is not a subclass of {1}", 
                    initializerType.FullName,
                    typeof(LibraryInitializer).FullName)
                );
            }

            try {
                initializer.LoadModules(_context, builtin);
            } catch (Exception e) {
                throw new LoadError(e.Message, e);
            }
        }

        #endregion
    }
}
