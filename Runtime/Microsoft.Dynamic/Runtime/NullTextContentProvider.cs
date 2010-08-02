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

using System.IO;
using System.Text;

namespace Microsoft.Scripting.Runtime {
    /// <summary>
    /// A NullTextContentProvider to be provided when we have a pre-compiled ScriptCode which doesn't
    /// have source code associated with it.
    /// </summary>
    public sealed class NullTextContentProvider : TextContentProvider {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly NullTextContentProvider Null = new NullTextContentProvider();

        private NullTextContentProvider() {
        }

        public override SourceCodeReader GetReader() {
            return SourceCodeReader.Null;
        }
    }
}
