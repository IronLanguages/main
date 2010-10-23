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

#if SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting.Runtime;

namespace Microsoft.Scripting.Runtime {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class DynamicLanguageProviderAttribute : Attribute {
        private readonly Type _languageContextType;
        private readonly string[] _fileExtensions;
        private readonly string[] _names;
        private readonly string _displayName;

        /// <summary>
        /// LanguageContext implementation.
        /// </summary>
        public Type LanguageContextType {
            get { return _languageContextType; } 
        }

        /// <summary>
        /// Default display name.
        /// </summary>
        public string DisplayName {
            get { return _displayName; }
        }

        /// <summary>
        /// Default file extensions.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] FileExtensions {
            get { return _fileExtensions; }
        }

        /// <summary>
        /// Default names for the language.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Names {
            get { return _names; }
        }

        public DynamicLanguageProviderAttribute(Type languageContextType, string displayName, string names, string fileExtensions)
            : this(languageContextType, displayName, 
            StringUtils.Split(names, new[] { ';', ',' }, Int32.MaxValue, StringSplitOptions.RemoveEmptyEntries),
            StringUtils.Split(fileExtensions, new[] { ';', ',' }, Int32.MaxValue, StringSplitOptions.RemoveEmptyEntries)) {
        }

        public DynamicLanguageProviderAttribute(Type languageContextType, string displayName, string[] names, string[] fileExtensions) {
            ContractUtils.RequiresNotNull(languageContextType, "languageContextType");
            ContractUtils.RequiresNotNull(displayName, "displayName");
            ContractUtils.RequiresNotNull(names, "names");
            ContractUtils.RequiresNotNull(fileExtensions, "fileExtensions");
            
            _languageContextType = languageContextType;
            _displayName = displayName;
            _names = names;
            _fileExtensions = fileExtensions;
        }
    }
}

#endif
