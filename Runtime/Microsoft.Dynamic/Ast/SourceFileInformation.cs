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
namespace Microsoft.Scripting {
    /// <summary>
    /// Stores information needed to emit debugging symbol information for a
    /// source file, in particular the file name and unique language identifier
    /// </summary>
    public sealed class SourceFileInformation {
        private readonly string _fileName;

        // TODO: save storage space if these are not supplied?
        private readonly Guid _language;
        private readonly Guid _vendor;

        public SourceFileInformation(string fileName) {
            _fileName = fileName;
        }

        public SourceFileInformation(string fileName, Guid language) {
            _fileName = fileName;
            _language = language;
        }

        public SourceFileInformation(string fileName, Guid language, Guid vendor) {
            _fileName = fileName;
            _language = language;
            _vendor = vendor;
        }

        /// <summary>
        /// The source file name
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        /// <summary>
        /// Returns the language's unique identifier, if any
        /// </summary>
        public Guid LanguageGuid {
            get { return _language; }
        }

        /// <summary>
        /// Returns the language vendor's unique identifier, if any
        /// </summary>
        public Guid VendorGuid {
            get { return _vendor; }
        }
    }
}
