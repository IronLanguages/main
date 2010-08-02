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
using System.Collections.ObjectModel;
using System.Threading;

using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Hosting {
    /// <summary>
    /// Stores information needed to setup a language
    /// </summary>
    [Serializable]
    public sealed class LanguageSetup {
        private string _typeName;
        private string _displayName;
        private IList<string> _names;
        private IList<string> _fileExtensions;
        private IDictionary<string, object> _options;
        private bool _frozen;
        private bool? _interpretedMode, _exceptionDetail, _perfStats, _noAdaptiveCompilation;

        /// <summary>
        /// Creates a new LanguageSetup
        /// </summary>
        /// <param name="typeName">assembly qualified type name of the language
        /// provider</param>
        public LanguageSetup(string typeName)
            : this(typeName, "", ArrayUtils.EmptyStrings, ArrayUtils.EmptyStrings) {
        }

        /// <summary>
        /// Creates a new LanguageSetup with the provided options
        /// TODO: remove this overload?
        /// </summary>
        public LanguageSetup(string typeName, string displayName)
            : this(typeName, displayName, ArrayUtils.EmptyStrings, ArrayUtils.EmptyStrings) {
        }

        /// <summary>
        /// Creates a new LanguageSetup with the provided options
        /// </summary>
        public LanguageSetup(string typeName, string displayName, IEnumerable<string> names, IEnumerable<string> fileExtensions) {
            ContractUtils.RequiresNotEmpty(typeName, "typeName");
            ContractUtils.RequiresNotNull(displayName, "displayName");
            ContractUtils.RequiresNotNull(names, "names");
            ContractUtils.RequiresNotNull(fileExtensions, "fileExtensions");

            _typeName = typeName;
            _displayName = displayName;
            _names = new List<string>(names);
            _fileExtensions = new List<string>(fileExtensions);
            _options = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets an option as a strongly typed value.
        /// </summary>
        public T GetOption<T>(string name, T defaultValue) {
            object value;
            if (_options != null && _options.TryGetValue(name, out value)) {
                if (value is T) {
                    return (T)value;
                }
                return (T)Convert.ChangeType(value, typeof(T), Thread.CurrentThread.CurrentCulture);
            }
            return defaultValue;
        }

        /// <summary>
        /// The assembly qualified type name of the language provider
        /// </summary>
        public string TypeName {
            get { return _typeName; }
            set {
                ContractUtils.RequiresNotEmpty(value, "value");
                CheckFrozen();
                _typeName = value;
            }
        }

        /// <summary>
        /// Display name of the language. If empty, it will be set to the first
        /// name in the Names list.
        /// </summary>
        public string DisplayName {
            get { return _displayName; }
            set {
                ContractUtils.RequiresNotNull(value, "value");
                CheckFrozen();
                _displayName = value;
            }
        }

        /// <remarks>
        /// Case-insensitive language names.
        /// </remarks>
        public IList<string> Names {
            get { return _names; }
        }

        /// <remarks>
        /// Case-insensitive file extension, optionally starts with a dot.
        /// </remarks>
        public IList<string> FileExtensions {
            get { return _fileExtensions; }
        }

        /// <remarks>
        /// Option names are case-sensitive.
        /// </remarks>
        public IDictionary<string, object> Options {
            get { return _options; }
        }

        [Obsolete("This option is ignored")]
        public bool InterpretedMode {
            get { return GetCachedOption("InterpretedMode", ref _interpretedMode); }
            set { 
                CheckFrozen();
                Options["InterpretedMode"] = value; 
            }
        }

        [Obsolete("Use Options[\"NoAdaptiveCompilation\"] instead.")]
        public bool NoAdaptiveCompilation {
            get { return GetCachedOption("NoAdaptiveCompilation", ref _noAdaptiveCompilation); }
            set {
                CheckFrozen();
                Options["NoAdaptiveCompilation"] = value;
            }
        }

        public bool ExceptionDetail {
            get { return GetCachedOption("ExceptionDetail", ref _exceptionDetail); }
            set {
                CheckFrozen();
                Options["ExceptionDetail"] = value;
            }
        }

        [Obsolete("Use Options[\"PerfStats\"] instead.")]
        public bool PerfStats {
            get { return GetCachedOption("PerfStats", ref _perfStats); }
            set {
                CheckFrozen();
                Options["PerfStats"] = value;
            }
        }

        private bool GetCachedOption(string name, ref bool? storage) {
            if (storage.HasValue) {
                return storage.Value;
            }

            if (_frozen) {
                storage = GetOption<bool>(name, false);
                return storage.Value;
            }

            return GetOption<bool>(name, false);
        }

        internal void Freeze() {
            _frozen = true;

            _names = new ReadOnlyCollection<string>(ArrayUtils.MakeArray(_names));
            _fileExtensions = new ReadOnlyCollection<string>(ArrayUtils.MakeArray(_fileExtensions));
            _options = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(_options));
        }

        private void CheckFrozen() {
            if (_frozen) {
                throw new InvalidOperationException("Cannot modify LanguageSetup after it has been used to create a ScriptRuntime");
            }
        }        
    }
}
