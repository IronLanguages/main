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
using System.Collections.ObjectModel;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {

    [Serializable]
    public sealed class RubyOptions : LanguageOptions {
        private readonly ReadOnlyCollection<string>/*!*/ _arguments;
        private readonly RubyEncoding/*!*/ _argumentEncoding;
        private readonly ReadOnlyCollection<string>/*!*/ _libraryPaths;
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
        private readonly RubyCompatibility _compatibility;
        private readonly RubyEncoding _kcode = null;

#if DEBUG
        public static bool UseThreadAbortForSyncRaise;
        public static bool CompileRegexps;
        public static bool ShowRules;
#endif

        public ReadOnlyCollection<string>/*!*/ Arguments {
            get { return _arguments; }
        }

        public RubyEncoding/*!*/ ArgumentEncoding {
            get { return _argumentEncoding; }
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

        public ReadOnlyCollection<string>/*!*/ LibraryPaths {
            get { return _libraryPaths; }
        }

        public ReadOnlyCollection<string> RequirePaths {
            get { return _requirePaths; }
        }

        public bool HasSearchPaths {
            get { return _hasSearchPaths; }
        }

        public RubyCompatibility Compatibility {
            get { return _compatibility; }
        }

        public RubyEncoding KCode {
            get { return _kcode; }
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
            _argumentEncoding = GetOption(options, "ArgumentEncoding", RubyEncoding.Default);

            _mainFile = GetOption(options, "MainFile", (string)null);
            _verbosity = GetOption(options, "Verbosity", 1);
            _debugVariable = GetOption(options, "DebugVariable", false);
            _enableTracing = GetOption(options, "EnableTracing", false);
            _savePath = GetOption(options, "SavePath", (string)null);
            _loadFromDisk = GetOption(options, "LoadFromDisk", false);
            _profile = GetOption(options, "Profile", false);
            _noAssemblyResolveHook = GetOption(options, "NoAssemblyResolveHook", false);
            _libraryPaths = GetStringCollectionOption(options, "LibraryPaths", ';', ',') ?? new ReadOnlyCollection<string>(new[] { "." });
            _requirePaths = GetStringCollectionOption(options, "RequiredPaths", ';', ',');
            _hasSearchPaths = GetOption<object>(options, "SearchPaths", null) != null;
            _compatibility = GetCompatibility(options, "Compatibility", RubyCompatibility.Default);

            if (_compatibility < RubyCompatibility.Ruby19) {
                _kcode = GetKCoding(options, "KCode", null);
            }
        }

        private static RubyCompatibility GetCompatibility(IDictionary<string, object>/*!*/ options, string/*!*/ name, RubyCompatibility defaultValue) {
            object value;
            if (options != null && options.TryGetValue(name, out value)) {
                if (value is RubyCompatibility) {
                    return (RubyCompatibility)value;
                }

                string str = value as string;
                if (str != null) {
                    switch (str) {
                        case "1.8": return RubyCompatibility.Ruby186;
                        case "1.9": return RubyCompatibility.Ruby19;
                        case "2.0": return RubyCompatibility.Ruby20;
                    }
                }

                return (RubyCompatibility)Convert.ChangeType(value, typeof(RubyCompatibility), Thread.CurrentThread.CurrentCulture);
            }
            return defaultValue;
        }

        private static RubyEncoding GetKCoding(IDictionary<string, object>/*!*/ options, string/*!*/ name, RubyEncoding defaultValue) {
            object value;
            if (options != null && options.TryGetValue(name, out value)) {
                RubyEncoding rubyEncoding = value as RubyEncoding;
                if (rubyEncoding != null && rubyEncoding.IsKCoding) {
                    return rubyEncoding;
                }

                throw new ArgumentException(String.Format("Invalid value for option {0}. Specify one of RubyEncoding.KCode* encodings.", name));
            }
            return defaultValue;
        }
    }
}
