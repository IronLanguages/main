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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Represents a Dynamic Language Runtime in Hosting API. 
    /// Hosting API counterpart for <see cref="ScriptDomainManager"/>.
    /// </summary>
    public sealed class ScriptRuntime
#if !SILVERLIGHT
        : MarshalByRefObject
#endif
    {
        private readonly Dictionary<LanguageContext, ScriptEngine> _engines;
        private readonly ScriptDomainManager _manager;
        private readonly InvariantContext _invariantContext;
        private readonly ScriptIO _io;
        private readonly ScriptHost _host;
        private readonly ScriptRuntimeSetup _setup;
        private readonly object _lock = new object();
        private ScriptScope _globals;
        private Scope _scopeGlobals;
        private ScriptEngine _invariantEngine;

        /// <summary>
        /// Creates ScriptRuntime in the current app-domain and initialized according to the the specified settings.
        /// Creates an instance of host class specified in the setup and associates it with the created runtime.
        /// Both Runtime and ScriptHost are collocated in the current app-domain.
        /// </summary>
        public ScriptRuntime(ScriptRuntimeSetup setup) {
            ContractUtils.RequiresNotNull(setup, "setup");

            // Do this first, so we detect configuration errors immediately
            DlrConfiguration config = setup.ToConfiguration();
            _setup = setup;

            try {
                _host = (ScriptHost)Activator.CreateInstance(setup.HostType, setup.HostArguments.ToArray<object>());
            } catch (TargetInvocationException e) {
                throw new InvalidImplementationException(Strings.InvalidCtorImplementation(setup.HostType, e.InnerException.Message), e.InnerException);
            } catch (Exception e) {
                throw new InvalidImplementationException(Strings.InvalidCtorImplementation(setup.HostType, e.Message), e);
            }

            ScriptHostProxy hostProxy = new ScriptHostProxy(_host);

            _manager = new ScriptDomainManager(hostProxy, config);
            _invariantContext = new InvariantContext(_manager);

            _io = new ScriptIO(_manager.SharedIO);
            _engines = new Dictionary<LanguageContext, ScriptEngine>();

            bool freshEngineCreated;
            _globals = new ScriptScope(GetEngineNoLockNoNotification(_invariantContext, out freshEngineCreated), _manager.Globals);

            // runtime needs to be all set at this point, host code is called

            _host.SetRuntime(this);

            object noDefaultRefs;
            if (!setup.Options.TryGetValue("NoDefaultReferences", out noDefaultRefs) || Convert.ToBoolean(noDefaultRefs) == false) {
                LoadAssembly(typeof(string).Assembly);
                LoadAssembly(typeof(System.Diagnostics.Debug).Assembly);
            }
        }

        internal ScriptDomainManager Manager {
            get { return _manager; }
        }

        public ScriptHost Host {
            get { return _host; }
        }

        public ScriptIO IO {
            get { return _io; }
        }
        
        /// <summary>
        /// Creates a new runtime with languages set up according to the current application configuration 
        /// (using System.Configuration).
        /// </summary>
        public static ScriptRuntime CreateFromConfiguration() {
            return new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration());
        }

        #region Remoting

#if !SILVERLIGHT

        /// <summary>
        /// Creates ScriptRuntime in the current app-domain and initialized according to the the specified settings.
        /// Creates an instance of host class specified in the setup and associates it with the created runtime.
        /// Both Runtime and ScriptHost are collocated in the specified app-domain.
        /// </summary>
        public static ScriptRuntime CreateRemote(AppDomain domain, ScriptRuntimeSetup setup) {
            ContractUtils.RequiresNotNull(domain, "domain");
#if !CLR2
            return (ScriptRuntime)domain.CreateInstanceAndUnwrap(
                typeof(ScriptRuntime).Assembly.FullName, 
                typeof(ScriptRuntime).FullName, 
                false, 
                BindingFlags.Default, 
                null, 
                new object[] { setup }, 
                null,
                null
            );
#else
            //The API CreateInstanceAndUnwrap is obsolete in Dev10.
            return (ScriptRuntime)domain.CreateInstanceAndUnwrap(
                typeof(ScriptRuntime).Assembly.FullName, 
                typeof(ScriptRuntime).FullName, 
                false, 
                BindingFlags.Default, 
                null, 
                new object[] { setup }, 
                null,
                null,
                null
            );
#endif
        }

        // TODO: Figure out what is the right lifetime
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService() {
            return null;
        }
#endif
        #endregion

        public ScriptRuntimeSetup Setup {
            get {
                return _setup;
            }
        }

        #region Engines

        public ScriptEngine GetEngine(string languageName) {
            ContractUtils.RequiresNotNull(languageName, "languageName");

            ScriptEngine engine;
            if (!TryGetEngine(languageName, out engine)) {
                throw new ArgumentException(String.Format("Unknown language name: '{0}'", languageName));
            }

            return engine;
        }

        public ScriptEngine GetEngineByTypeName(string assemblyQualifiedTypeName) {
            ContractUtils.RequiresNotNull(assemblyQualifiedTypeName, "assemblyQualifiedTypeName");
            return GetEngine(_manager.GetLanguageByTypeName(assemblyQualifiedTypeName));
        }

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ScriptEngine GetEngineByFileExtension(string fileExtension) {
            ContractUtils.RequiresNotNull(fileExtension, "fileExtension");

            ScriptEngine engine;
            if (!TryGetEngineByFileExtension(fileExtension, out engine)) {
                throw new ArgumentException(String.Format("Unknown file extension: '{0}'", fileExtension));
            }

            return engine;
        }

        public bool TryGetEngine(string languageName, out ScriptEngine engine) {
            LanguageContext language;
            if (!_manager.TryGetLanguage(languageName, out language)) {
                engine = null;
                return false;
            }

            engine = GetEngine(language);
            return true;
        }

        public bool TryGetEngineByFileExtension(string fileExtension, out ScriptEngine engine) {
            LanguageContext language;
            if (!_manager.TryGetLanguageByFileExtension(fileExtension, out language)) {
                engine = null;
                return false;
            }

            engine = GetEngine(language);
            return true;
        }

        /// <summary>
        /// Gets engine for the specified language.
        /// </summary>
        internal ScriptEngine GetEngine(LanguageContext language) {
            Assert.NotNull(language);

            ScriptEngine engine;
            bool freshEngineCreated;
            lock (_engines) {
                engine = GetEngineNoLockNoNotification(language, out freshEngineCreated);
            }

            if (freshEngineCreated && !ReferenceEquals(language, _invariantContext)) {
                _host.EngineCreated(engine);
            }

            return engine;
        }

        /// <summary>
        /// Looks up the engine for the specified language. It the engine hasn't been created in this Runtime, it is instantiated here.
        /// The method doesn't lock nor send notifications to the host.
        /// </summary>
        private ScriptEngine GetEngineNoLockNoNotification(LanguageContext language, out bool freshEngineCreated) {
            Debug.Assert(_engines != null, "Invalid ScriptRuntime initialiation order");

            ScriptEngine engine;
            if (freshEngineCreated = !_engines.TryGetValue(language, out engine)) {
                engine = new ScriptEngine(this, language);
                Thread.MemoryBarrier();
                _engines.Add(language, engine);
            }
            return engine;
        }

        #endregion

        #region Compilation, Module Creation

        public ScriptScope CreateScope() {
            return InvariantEngine.CreateScope();
        }

        public ScriptScope CreateScope(string languageId) {
            return GetEngine(languageId).CreateScope();
        }

        public ScriptScope CreateScope(IDynamicMetaObjectProvider storage) {
            return InvariantEngine.CreateScope(storage);
        }

        public ScriptScope CreateScope(string languageId, IDynamicMetaObjectProvider storage) {
            return GetEngine(languageId).CreateScope(storage);
        }

        [Obsolete("IAttributesCollection is obsolete, use CreateScope(IDynamicMetaObjectProvider) instead")]
        public ScriptScope CreateScope(IAttributesCollection dictionary) {
            return InvariantEngine.CreateScope(dictionary);
        }

        [Obsolete("IAttributesCollection is obsolete, use CreateScope(string, IDynamicMetaObjectProvider) instead")]
        public ScriptScope CreateScope(string languageId, IAttributesCollection storage) {
            return GetEngine(languageId).CreateScope(storage);
        }

        #endregion

        // TODO: file IO exceptions, parse exceptions, execution exceptions, etc.
        /// <exception cref="ArgumentException">
        /// path is empty, contains one or more of the invalid characters defined in GetInvalidPathChars or doesn't have an extension.
        /// </exception>
        public ScriptScope ExecuteFile(string path) {
            ContractUtils.RequiresNotEmpty(path, "path");
            string extension = Path.GetExtension(path);

            ScriptEngine engine;
            if (!TryGetEngineByFileExtension(extension, out engine)) {
                throw new ArgumentException(String.Format("File extension '{0}' is not associated with any language.", extension));
            }

            return engine.ExecuteFile(path);
        }

        /// <exception cref="ArgumentNullException">path is null</exception>
        /// <exception cref="ArgumentException">file extension does not map to language engine</exception>
        /// <exception cref="InvalidOperationException">language does not have any search paths</exception>
        /// <exception cref="FileNotFoundException">file does exist in language's search path</exception>
        public ScriptScope UseFile(string path) {
            ContractUtils.RequiresNotEmpty(path, "path");
            string extension = Path.GetExtension(path);

            ScriptEngine engine;
            if (!TryGetEngineByFileExtension(extension, out engine)) {
                throw new ArgumentException(string.Format("File extension '{0}' is not associated with any language.", extension));
            }

            var searchPaths = engine.GetSearchPaths();
            if (searchPaths.Count == 0) {
                throw new InvalidOperationException(string.Format("No search paths defined for language '{0}'", engine.Setup.DisplayName));
            }

            // See if the file is already loaded, if so return the scope
            foreach (string searchPath in searchPaths) {
                string filePath = Path.Combine(searchPath, path);
                ScriptScope scope = engine.GetScope(filePath);
                if (scope != null) {
                    return scope;
                }
            }

            // Find the file on disk, then load it
            foreach (string searchPath in searchPaths) {
                string filePath = Path.Combine(searchPath, path);
                if (_manager.Platform.FileExists(filePath)) {
                    return ExecuteFile(filePath);
                }
            }

            // Didn't find the file, throw
            string allPaths = searchPaths.Aggregate((x, y) => x + ", " + y);
            throw new FileNotFoundException(string.Format("File '{0}' not found in language's search path: {1}", path, allPaths));
        }

        /// <summary>
        /// This property returns the "global object" or name bindings of the ScriptRuntime as a ScriptScope.  
        /// 
        /// You can set the globals scope, which you might do if you created a ScriptScope with an 
        /// IAttributesCollection so that your host could late bind names.
        /// </summary>
        public ScriptScope Globals {
            get {
                Scope scope = _manager.Globals;
                if (_scopeGlobals == scope) {
                    return _globals;
                }
                lock (_lock) {
                    if (_scopeGlobals != scope) {
                        // make sure no one has changed the globals behind our back
                        _globals = new ScriptScope(InvariantEngine, scope); // TODO: Should get LC from Scope when it's there
                        _scopeGlobals = scope;
                    }

                    return _globals;
                }
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                lock (_lock) {
                    _globals = value;
                    _manager.Globals = value.Scope;
                }
            }
        }

        /// <summary>
        /// This method walks the assembly's namespaces and name bindings to ScriptRuntime.Globals 
        /// to represent the types available in the assembly.  Each top-level namespace name gets 
        /// bound in Globals to a dynamic object representing the namespace.  Within each top-level 
        /// namespace object, nested namespace names are bound to dynamic objects representing each 
        /// tier of nested namespaces.  When this method encounters the same namespace-qualified name, 
        /// it merges names together objects representing the namespaces.
        /// </summary>
        /// <param name="assembly"></param>
        public void LoadAssembly(Assembly assembly) {
            _manager.LoadAssembly(assembly);
        }

        public ObjectOperations Operations {
            get {
                return InvariantEngine.Operations;
            }
        }

        public ObjectOperations CreateOperations() {
            return InvariantEngine.CreateOperations();
        }

        public void Shutdown() {
            List<LanguageContext> lcs;
            lock (_engines) {
                lcs = new List<LanguageContext>(_engines.Keys);
            }

            foreach (var language in lcs) {
                language.Shutdown();
            }
        }

        internal ScriptEngine InvariantEngine {
            get {
                if (_invariantEngine == null) {
                    _invariantEngine = GetEngine(_invariantContext);
                }
                return _invariantEngine;
            }
        }
    }
}
