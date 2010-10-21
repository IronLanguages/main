/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/


using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.Scripting;

namespace Microsoft.PyAnalysis.Values {
    /// <summary>
    /// Represents a collection of referneces for a namespace.  The collection
    /// version is the version of the referer.
    /// </summary>
    internal class NamespaceReferences {
        public int Version { get; private set; }
        public HashSet<SourceSpan> References { get; set; }

        public NamespaceReferences(int version) {
            Version = version;
        }
    }
}
