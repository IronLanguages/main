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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Dynamic;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace Microsoft.Scripting.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")] // TODO: fix
    public sealed class ScriptDomainManager {
        private readonly DynamicRuntimeHostingProvider _hostingProvider;
        private readonly SharedIO _sharedIO;
        private List<Assembly> _loadedAssemblies = new List<Assembly>();

        // last id assigned to a language context:
        private int _lastContextId;

        private Scope _globals;
        private readonly DlrConfiguration _configuration;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public PlatformAdaptationLayer Platform {
            get {
                PlatformAdaptationLayer result = _hostingProvider.PlatformAdaptationLayer;
                if (result == null) {
                    throw new InvalidImplementationException();
                }
                return result;
            }
        }

        public SharedIO SharedIO {
            get { return _sharedIO; }
        }

        public DynamicRuntimeHostingProvider Host {
            get { return _hostingProvider; }
        }

        public DlrConfiguration Configuration {
            get { return _configuration; }
        }

        public ScriptDomainManager(DynamicRuntimeHostingProvider hostingProvider, DlrConfiguration configuration) {
            ContractUtils.RequiresNotNull(hostingProvider, "hostingProvider");
            ContractUtils.RequiresNotNull(configuration, "configuration");

            configuration.Freeze();

            _hostingProvider = hostingProvider;
            _configuration = configuration;

            _sharedIO = new SharedIO();

            // create the initial default scope
            _globals = new Scope();
        }

        #region Language Registration

        internal ContextId GenerateContextId() {
            return new ContextId(Interlocked.Increment(ref _lastContextId));
        }

        public LanguageContext GetLanguage(Type providerType) {
            ContractUtils.RequiresNotNull(providerType, "providerType");
            return GetLanguageByTypeName(providerType.AssemblyQualifiedName);
        }

        public LanguageContext GetLanguageByTypeName(string providerAssemblyQualifiedTypeName) {
            ContractUtils.RequiresNotNull(providerAssemblyQualifiedTypeName, "providerAssemblyQualifiedTypeName");
            var aqtn = AssemblyQualifiedTypeName.ParseArgument(providerAssemblyQualifiedTypeName, "providerAssemblyQualifiedTypeName");

            LanguageContext language;
            if (!_configuration.TryLoadLanguage(this, aqtn, out language)) {
                throw Error.UnknownLanguageProviderType();
            }
            return language;
        }

        public bool TryGetLanguage(string languageName, out LanguageContext language) {
            ContractUtils.RequiresNotNull(languageName, "languageName");
            return _configuration.TryLoadLanguage(this, languageName, false, out language);
        }

        public LanguageContext GetLanguageByName(string languageName) {
            LanguageContext language;
            if (!TryGetLanguage(languageName, out language)) {
                throw new ArgumentException(String.Format("Unknown language name: '{0}'", languageName));
            }
            return language;
        }

        public bool TryGetLanguageByFileExtension(string fileExtension, out LanguageContext language) {
            ContractUtils.RequiresNotEmpty(fileExtension, "fileExtension");
            return _configuration.TryLoadLanguage(this, DlrConfiguration.NormalizeExtension(fileExtension), true, out language);
        }

        public LanguageContext GetLanguageByExtension(string fileExtension) {
            LanguageContext language;
            if (!TryGetLanguageByFileExtension(fileExtension, out language)) {
                throw new ArgumentException(String.Format("Unknown file extension: '{0}'", fileExtension));
            }
            return language;
        }

        #endregion

        /// <summary>
        /// A collection of environment variables.
        /// </summary>
        public Scope Globals {
            get {
                return _globals;
            }
            set {
                _globals = value;
            }
        }

        /// <summary>
        /// Event for when a host calls LoadAssembly.  After hooking this
        /// event languages will need to call GetLoadedAssemblyList to
        /// get any assemblies which were loaded before the language was
        /// loaded.
        /// </summary>
        public event EventHandler<AssemblyLoadedEventArgs> AssemblyLoaded;

        public bool LoadAssembly(Assembly assembly) {
            ContractUtils.RequiresNotNull(assembly, "assembly");

            lock (_loadedAssemblies) {
                if (_loadedAssemblies.Contains(assembly)) {
                    // only deliver the event if we've never added the assembly before
                    return false;
                }
                _loadedAssemblies.Add(assembly);
            }

            EventHandler<AssemblyLoadedEventArgs> assmLoaded = AssemblyLoaded;
            if (assmLoaded != null) {
                assmLoaded(this, new AssemblyLoadedEventArgs(assembly));
            }

            return true;
        }
        
        public IList<Assembly> GetLoadedAssemblyList() {
            lock (_loadedAssemblies) {
                return _loadedAssemblies.ToArray();
            }
        }
    }
}
