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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {

    [Serializable]
    public sealed class RubyOptions : LanguageOptions {
        private readonly ReadOnlyCollection<string>/*!*/ _arguments;
        private readonly RubyEncoding/*!*/ _localeEncoding;
        private readonly RubyEncoding _defaultEncoding;
        private readonly string _standardLibraryPath;
        private readonly string _applicationBase;
        private readonly ReadOnlyCollection<string> _requirePaths;
        private readonly string _mainFile;
        private readonly bool _enableTracing;
        private readonly int _verbosity;
        private readonly bool _debugVariable;
        private readonly string _savePath;
        private readonly bool _loadFromDisk;
        private readonly bool _profile;
        private readonly bool _hasSearchPaths;
        private readonly bool _noAssemblyResolveHook;

#if DEBUG
        public static bool UseThreadAbortForSyncRaise;
        public static bool CompileRegexps;
        public static bool ShowRules;
#endif

        public ReadOnlyCollection<string>/*!*/ Arguments {
            get { return _arguments; }
        }

        public RubyEncoding/*!*/ LocaleEncoding {
            get { return _localeEncoding; }
        }

        public RubyEncoding DefaultEncoding {
            get { return _defaultEncoding; }
        }

        public string MainFile {
            get { return _mainFile; }
        }
        
        public int Verbosity {
            get { return _verbosity; }
        }

        public bool EnableTracing {
            get { return _enableTracing; }
        }

        public string SavePath {
            get { return _savePath; }
        }

        public bool LoadFromDisk {
            get { return _loadFromDisk; }
        }

        public bool Profile {
            get { return _profile; }
        }

        public bool NoAssemblyResolveHook {
            get { return _noAssemblyResolveHook; }
        }

        public string StandardLibraryPath {
            get { return _standardLibraryPath; }
        }

        public string ApplicationBase {
            get { return _applicationBase; }
        }

        public ReadOnlyCollection<string> RequirePaths {
            get { return _requirePaths; }
        }

        public bool HasSearchPaths {
            get { return _hasSearchPaths; }
        }

        public RubyCompatibility Compatibility {
            get { return RubyCompatibility.Default; }
        }

        /// <summary>
        /// The initial value of $DEBUG variable.
        /// </summary>
        public bool DebugVariable {
            get { return _debugVariable; }
        }

        public RubyOptions(IDictionary<string, object>/*!*/ options)
            : base(options) {
            _arguments = GetStringCollectionOption(options, "Arguments") ?? EmptyStringCollection;
            _localeEncoding = GetOption(options, "LocaleEncoding", RubyEncoding.UTF8);
            _defaultEncoding = GetOption<RubyEncoding>(options, "DefaultEncoding", null);

            _mainFile = GetOption(options, "MainFile", (string)null);
            _verbosity = GetOption(options, "Verbosity", 1);
            _debugVariable = GetOption(options, "DebugVariable", false);
            _enableTracing = GetOption(options, "EnableTracing", false);
            _savePath = GetOption(options, "SavePath", (string)null);
            _loadFromDisk = GetOption(options, "LoadFromDisk", false);
            _profile = GetOption(options, "Profile", false);
            _noAssemblyResolveHook = GetOption(options, "NoAssemblyResolveHook", false);
            _requirePaths = GetStringCollectionOption(options, "RequiredPaths", ';', ',');
            _hasSearchPaths = GetOption<object>(options, "SearchPaths", null) != null;
            _standardLibraryPath = GetOption(options, "StandardLibrary", (string)null);
            _applicationBase = GetOption(options, "ApplicationBase", (string)null);
        }
    }
}
