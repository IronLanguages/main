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

using Microsoft.Scripting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronStudio.Intellisense {

    public interface IParser {
        /// <summary>
        /// Called when the specified content should be parsed.  If the content
        /// is associated with a text buffer the snap shot being parsed is also
        /// provided.
        /// </summary>
        void Parse(TextContentProvider content);
    }
}
