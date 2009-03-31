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
using System.Dynamic;
using System.Dynamic.Utils;
using Microsoft.Scripting;
using System.Collections.ObjectModel;

namespace IronPython {

    [CLSCompliant(true)]
    public enum PythonDivisionOptions {
        Old,
        New,
        Warn,
        WarnAll
    }

    [Serializable, CLSCompliant(true)]
    public sealed class PythonOptions : LanguageOptions {

        private readonly ReadOnlyCollection<string>/*!*/ _arguments;
        private readonly ReadOnlyCollection<string>/*!*/ _warningFilters;
        private readonly bool _warnPy3k;
        private readonly bool _bytesWarning;
        private readonly bool _debug;
        private readonly int _recursionLimit;
        private readonly Severity _indentationInconsistencySeverity;
        private readonly PythonDivisionOptions _division;
        private readonly bool _stripDocStrings;
        private readonly bool _optimize;
        private readonly bool _inspect;
        private readonly bool _noUserSite;
        private readonly bool _noSite;
        private readonly bool _ignoreEnvironment;
        private readonly bool _verbose;
        private readonly Version _version;
        private readonly bool _adaptiveCompilation;
        private readonly bool _enableProfiler;
        private readonly bool _lightweightScopes;

        public ReadOnlyCollection<string>/*!*/ Arguments {
            get { return _arguments; }
        }

        /// <summary>
        ///  Should we strip out all doc strings (the -O command line option).
        /// </summary>
        public bool Optimize {
            get { return _optimize; }
        }
        
        /// <summary>
        ///  Should we strip out all doc strings (the -OO command line option).
        /// </summary>
        public bool StripDocStrings {
            get { return _stripDocStrings; }
        }

        /// <summary>
        ///  List of -W (warning filter) options collected from the command line.
        /// </summary>
        public ReadOnlyCollection<string>/*!*/ WarningFilters {
            get { return _warningFilters; }
        }

        public bool WarnPy3k {
            get { return _warnPy3k; }
        }

        public bool BytesWarning {
            get { return _bytesWarning; }
        }

        public bool Debug {
            get { return _debug; }
        }

        public bool Inspect {
            get { return _inspect; }
        }

        public bool NoUserSite {
            get { return _noUserSite; }
        }

        public bool NoSite {
            get { return _noSite; }
        }

        public bool IgnoreEnvironment {
            get { return _ignoreEnvironment; }
        }

        public bool Verbose {
            get { return _verbose; }
        }

        public int RecursionLimit {
            get { return _recursionLimit; }
        }

        /// <summary> 
        /// Severity of a warning that indentation is formatted inconsistently.
        /// </summary>
        public Severity IndentationInconsistencySeverity {
            get { return _indentationInconsistencySeverity; }
        }

        /// <summary>
        /// The division options (old, new, warn, warnall)
        /// </summary>
        public PythonDivisionOptions DivisionOptions {
            get { return _division; }
        }

        /// <summary>
        /// Dynamically choose between interpreting, simple compilation and compilation
        /// that takes advantage of runtime history.
        /// </summary>
        public bool AdaptiveCompilation {
            get { return _adaptiveCompilation; }
        }

        public bool LightweightScopes {
            get {
                return _lightweightScopes;
            }
        }

        /// <summary>
        /// Enable profiling code
        /// </summary>
        public bool EnableProfiler {
            get { return _enableProfiler; }
        }

        public PythonOptions() 
            : this(null) {
        }

        public Version PythonVersion {
            get {
                return _version;
            }
        }
    
        public PythonOptions(IDictionary<string, object> options) 
            : base(options) {

            _arguments = GetStringCollectionOption(options, "Arguments") ?? EmptyStringCollection;
            _warningFilters = GetStringCollectionOption(options, "WarningFilters", ';', ',') ?? EmptyStringCollection;

            _warnPy3k = GetOption(options, "WarnPy3k", false);
            _bytesWarning = GetOption(options, "BytesWarning", false);
            _debug = GetOption(options, "Debug", false);
            _inspect = GetOption(options, "Inspect", false);
            _noUserSite = GetOption(options, "NoUserSite", false);
            _noSite = GetOption(options, "NoSite", false);
            _ignoreEnvironment = GetOption(options, "IgnoreEnvironment", false);
            _verbose = GetOption(options, "Verbose", false);
            _optimize = GetOption(options, "Optimize", false);
            _stripDocStrings = GetOption(options, "StripDocStrings", false);
            _division = GetOption(options, "DivisionOptions", PythonDivisionOptions.Old);
            _recursionLimit = GetOption(options, "RecursionLimit", Int32.MaxValue);
            _indentationInconsistencySeverity = GetOption(options, "IndentationInconsistencySeverity", Severity.Ignore);
            _adaptiveCompilation = GetOption(options, "AdaptiveCompilation", true);
            _enableProfiler = GetOption(options, "EnableProfiler", false);
            _lightweightScopes = GetOption(options, "LightweightScopes", false);

            object value;
            if (options != null && options.TryGetValue("PythonVersion", out value)) {
                if (value is Version) {
                    _version = (Version)value;
                } else if (value is string) {
                    _version = new Version((string)value);
                } else {
                    throw new ArgumentException("Expected string or Version for PythonVersion");
                }

                if (_version != new Version(2, 5) && _version != new Version(2, 6)) {
                    throw new ArgumentException("Expected Version to be 2.5 or 2.6");
                }
            } else {
                _version = new Version(2, 6);
            }
        }
    }
}
