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

using Microsoft.VisualStudio.Text;

namespace Microsoft.IronStudio {
    /// <summary>
    /// IMixedBuffer can be attached as a property to an ITextBuffer.  Doing so indicates to tokenization and analysis
    /// services that the buffer contains both source code in a specific language as well as non-source code
    /// elements.  These services can then use this interface so that they only process the language specific elements.
    /// 
    /// Currently the only producer of this interface is the REPL window.
    /// </summary>
    public interface IMixedBuffer {
        /// <summary>
        /// Gets the spans which contain language code
        /// </summary>
        /// <returns></returns>
        SnapshotSpan[] GetLanguageSpans(ITextSnapshot snapshot);

        /// <summary>
        /// For a given line and snapshot gets the relevant span on that line which contains source code.
        /// 
        /// If the line does not contain any source code then null is returned.
        /// </summary>
        SnapshotSpan? GetLanguageSpanForLine(ITextSnapshot snapshot, int line);
    }
}
