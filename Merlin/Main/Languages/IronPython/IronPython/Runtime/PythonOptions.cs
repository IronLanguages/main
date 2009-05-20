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
        private readonly bool _warnPy3k, _python30;
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
        private readonly bool _frames, _fullFrames;
        private readonly Version _version;
        private readonly bool _adaptiveCompilation;
        private bool _enableProfiler;
        private readonly bool _lightweightScopes;

        /// <summary>
        /// Gets the collection of command line arguments.
        /// </summary>
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

        /// <summary>
        /// Enables warnings related to Python 3.0 features.
        /// </summary>
        public bool WarnPython30 {
            get { return _warnPy3k; }
        }

        /// <summary>
        /// Enables 3.0 features that are implemented in IronPython.
        /// </summary>
        public bool Python30 {
            get {
                return _python30;
            }
        }

        public bool BytesWarning {
            get { return _bytesWarning; }
        }

        /// <summary>
        /// Enables debugging support.  When enabled a .NET debugger can be attached
        /// to the process to step through Python code.
        /// </summary>
        public bool Debug {
            get { return _debug; }
        }

        /// <summary>
        /// Enables inspect mode.  After running the main module the REPL will be started
        /// within that modules context.
        /// </summary>
        public bool Inspect {
            get { return _inspect; }
        }

        /// <summary>
        /// Suppresses addition of the user site directory.  This is ignored by IronPython
        /// except for updating sys.flags.
        /// </summary>
        public bool NoUserSite {
            get { return _noUserSite; }
        }

        /// <summary>
        /// Disables import site on startup.
        /// </summary>
        public bool NoSite {
            get { return _noSite; }
        }

        /// <summary>
        /// Ignore environment variables that configure the IronPython context.
        /// </summary>
        public bool IgnoreEnvironment {
            get { return _ignoreEnvironment; }
        }

        /// <summary>
        /// Enables the verbose option which traces import statements.  This is ignored by IronPython
        /// except for setting sys.flags.
        /// </summary>
        public bool Verbose {
            get { return _verbose; }
        }

        /// <summary>
        /// Sets the maximum recursion depth.  Setting to Int32.MaxValue will disable recursion
        /// enforcement.
        /// </summary>
        public int RecursionLimit {
            get { return _recursionLimit; }
        }

        /// <summary>
        /// Makes available sys._getframe.  Local variables will not be available in frames unless the
        /// function calls locals(), dir(), vars(), etc...  For ensuring locals are always available use
        /// the FullFrames option.
        /// </summary>
        public bool Frames {
            get {
                return _frames;
            }
        }

        /// <summary>
        /// Makes available sys._getframe.  All locals variables will live on the heap (for a considerable
        /// performance cost) enabling introspection of all code.
        /// </summary>
        public bool FullFrames {
            get {
                return _fullFrames;
            }
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

        /// <summary>
        /// Forces all code to be compiled in a mode in which the code can be reliably collected by the CLR.
        /// </summary>
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
            set { _enableProfiler = value; }
        }

        public PythonOptions() 
            : this(null) {
        }

        /// <summary>
        /// Gets the CPython version which IronPython will emulate.  Currently limited
        /// to either 2.6 or 3.0.
        /// </summary>
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
            _fullFrames = GetOption(options, "FullFrames", false);
            _frames = _fullFrames || GetOption(options, "Frames", false);

            object value;
            if (options != null && options.TryGetValue("PythonVersion", out value)) {
                if (value is Version) {
                    _version = (Version)value;
                } else if (value is string) {
                    _version = new Version((string)value);
                } else {
                    throw new ArgumentException("Expected string or Version for PythonVersion");
                }

                if (_version != new Version(2, 6) && _version != new Version(3, 0)) {
                    throw new ArgumentException("Expected Version to be 2.6 or 3.0");
                }
            } else {
                _version = new Version(2, 6);
            }

            _python30 = _version == new Version(3, 0);
        }
    }
}
