/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Stores information needed to setup a ScriptRuntime
    /// </summary>
    [Serializable]
    public sealed class ScriptRuntimeSetup {
        // host specification:
        private Type _hostType;
        private IList<object> _hostArguments;

        // languages available in the runtime: 
        private IList<LanguageSetup> _languageSetups;

        // DLR options:
        private bool _debugMode;
        private bool _privateBinding;

        // common language options:
        private IDictionary<string, object> _options;
        
        // true if the ScriptRuntimeSetup is no longer mutable because it's been
        // used to start a ScriptRuntime
        private bool _frozen;

        public ScriptRuntimeSetup() {
            _languageSetups = new List<LanguageSetup>();
            _options = new Dictionary<string, object>();
            _hostType = typeof(ScriptHost);
            _hostArguments = ArrayUtils.EmptyObjects;
        }

        /// <summary>
        /// The list of language setup information for languages to load into
        /// the runtime
        /// </summary>
        public IList<LanguageSetup> LanguageSetups {
            get { return _languageSetups; }
        }

        /// <summary>
        /// Indicates that the script runtime is in debug mode.
        /// This means:
        /// 
        /// 1) Symbols are emitted for debuggable methods (methods associated with SourceUnit).
        /// 2) Debuggable methods are emitted to non-collectable types (this is due to CLR limitations on dynamic method debugging).
        /// 3) JIT optimization is disabled for all methods
        /// 4) Languages may disable optimizations based on this value.
        /// </summary>
        public bool DebugMode {
            get { return _debugMode; }
            set {
                CheckFrozen();
                _debugMode = value; 
            }
        }

        /// <summary>
        /// Ignore CLR visibility checks
        /// </summary>
        public bool PrivateBinding {
            get { return _privateBinding; }
            set {
                CheckFrozen();
                _privateBinding = value; 
            }
        }

        /// <summary>
        /// Can be any derived class of ScriptHost. When set, it allows the
        /// host to override certain methods to control behavior of the runtime
        /// </summary>
        public Type HostType {
            get { return _hostType; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                ContractUtils.Requires(typeof(ScriptHost).GetTypeInfo().IsAssignableFrom(value.GetTypeInfo()), "value", "Must be ScriptHost or a derived type of ScriptHost");
                CheckFrozen();
                _hostType = value;
            }
        }

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
        public IDictionary<string, object> Options {
            get { return _options; }
        }

        /// <summary>
        /// Arguments passed to the host type when it is constructed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<object> HostArguments {
            get {
                return _hostArguments;
            }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                CheckFrozen();
                _hostArguments = value;
            }
        }

        internal DlrConfiguration ToConfiguration() {
            ContractUtils.Requires(_languageSetups.Count > 0, "ScriptRuntimeSetup must have at least one LanguageSetup");

            // prepare
            ReadOnlyCollection<LanguageSetup> setups = new ReadOnlyCollection<LanguageSetup>(ArrayUtils.MakeArray(_languageSetups));
            var hostArguments = new ReadOnlyCollection<object>(ArrayUtils.MakeArray(_hostArguments));
            var options = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(_options));            
            var config = new DlrConfiguration(_debugMode, _privateBinding, options);

            // validate
            foreach (var language in setups) {
                config.AddLanguage(
                    language.TypeName,
                    language.DisplayName,
                    language.Names,
                    language.FileExtensions,
                    language.Options
                );
            }

            // commit
            _languageSetups = setups;
            _options = options;
            _hostArguments = hostArguments;

            Freeze(setups);

            return config;
        }

        private void Freeze(ReadOnlyCollection<LanguageSetup> setups) {
            foreach (var language in setups) {
                language.Freeze();
            }

            _frozen = true;
        }

        private void CheckFrozen() {
            if (_frozen) {
                throw new InvalidOperationException("Cannot modify ScriptRuntimeSetup after it has been used to create a ScriptRuntime");
            }            
        }
        
        /// <summary>
        /// Reads setup from .NET configuration system (.config files).
        /// If there is no configuration available returns an empty setup.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration() {
#if FEATURE_CONFIGURATION
            var setup = new ScriptRuntimeSetup();
            Configuration.Section.LoadRuntimeSetup(setup, null);
            return setup;
#else
            return new ScriptRuntimeSetup();
#endif
        }

#if FEATURE_CONFIGURATION
        /// <summary>
        /// Reads setup from a specified XML stream.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration(Stream configFileStream) {
            ContractUtils.RequiresNotNull(configFileStream, "configFileStream");
            var setup = new ScriptRuntimeSetup();
            Configuration.Section.LoadRuntimeSetup(setup, configFileStream);
            return setup;
        }

        /// <summary>
        /// Reads setup from a specified XML file.
        /// </summary>
        public static ScriptRuntimeSetup ReadConfiguration(string configFilePath) {
            ContractUtils.RequiresNotNull(configFilePath, "configFilePath");

            using (var stream = File.OpenRead(configFilePath)) {
                return ReadConfiguration(stream);
            }
        }
#endif        
    }
}
