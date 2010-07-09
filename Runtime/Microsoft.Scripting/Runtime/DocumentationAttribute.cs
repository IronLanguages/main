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

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// Provides a mechanism for providing documentation stored in an assembly as metadata.  
    /// 
    /// Applying this attribute will enable documentation to be provided to the user at run-time
    /// even if XML Docuementation files are unavailable.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DocumentationAttribute : Attribute {
        private readonly string _doc;

        public DocumentationAttribute(string documentation) {
            _doc = documentation;
        }

        public string Documentation {
            get { return _doc; }
        }
    }
}
