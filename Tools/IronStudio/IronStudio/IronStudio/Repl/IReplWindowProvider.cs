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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronStudio.Repl {
    /// <summary>
    /// Provides REPL windows for all languages.
    /// </summary>
    public interface IReplWindowProvider {
        /// <summary>
        /// Creates a REPL window and returns a ToolWindowPane which implements IReplWindow.
        /// </summary>
        ToolWindowPane CreateReplWindow(IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, string/*!*/ title, Guid languageServiceGuid, Guid replId);

        ToolWindowPane CreateReplWindow(IReplEvaluator/*!*/ evaluator, IContentType/*!*/ contentType, int id, string/*!*/ title, Guid languageServiceGuid, Guid guid);

        /// <summary>
        /// Finds the REPL w/ the specified ID or returns null if the window hasn't been created.
        /// </summary>
        ToolWindowPane FindReplWindow(Guid replId);

    }
}
