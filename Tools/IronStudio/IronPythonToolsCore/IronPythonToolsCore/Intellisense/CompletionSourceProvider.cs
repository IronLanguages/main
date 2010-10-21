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

using System.ComponentModel.Composition;
using Microsoft.IronStudio;
using Microsoft.IronStudio.Core;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Microsoft.IronPythonTools.Library;

namespace Microsoft.IronPythonTools.Intellisense {
    [Export(typeof(ICompletionSourceProvider)), ContentType(PythonCoreConstants.ContentType), Order, Name("CompletionProvider")]
    internal class CompletionSourceProvider : ICompletionSourceProvider {
        private readonly IPythonRuntimeHost _host;

        [Import]
        internal IGlyphService _glyphService = null; // Assigned from MEF
        [Import]
        internal IPythonAnalyzer _Analysis;

        [ImportingConstructor]
        public CompletionSourceProvider(IPythonRuntimeHost host) {
            _host = host;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer) {
            return new CompletionSource(this, textBuffer, _host);
        }
    }
}
