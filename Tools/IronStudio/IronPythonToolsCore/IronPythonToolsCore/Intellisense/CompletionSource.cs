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
using Microsoft.IronPythonTools.Internal;
using Microsoft.IronStudio.Core;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronPythonTools.Intellisense {
    class CompletionSource : ICompletionSource {
        private readonly ITextBuffer _textBuffer;
        private readonly CompletionSourceProvider _provider;
        private readonly IPythonRuntimeHost _host; 

        public CompletionSource(CompletionSourceProvider provider, ITextBuffer textBuffer, IPythonRuntimeHost host) {
            _textBuffer = textBuffer;
            _provider = provider;
            _host = host;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets) {
            var textView = session.TextView;
            var textBuffer = session.TextView.TextBuffer;
            var span = session.CreateTrackingSpan0(textBuffer);
            var provider = _provider._Analysis.GetCompletions(textBuffer.CurrentSnapshot, textBuffer, span, _host.IntersectMembers, _host.HideAdvancedMembers);

            var completions = provider.GetCompletions(_provider._glyphService);

            if (completions == null || completions.Completions.Count == 0) {
                return;
            }

            completionSets.Add(completions);
        }

        public void Dispose() {
        }
    }
}
