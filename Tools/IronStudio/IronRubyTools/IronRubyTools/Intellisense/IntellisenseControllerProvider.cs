/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.IronStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools.Intellisense {
    [Export(typeof(IIntellisenseControllerProvider)), ContentType(RubyCoreConstants.ContentType), Order]
    class IntellisenseControllerProvider : IIntellisenseControllerProvider {
        [Import]
        internal ICompletionBroker _CompletionBroker = null; // Set via MEF
        [Import]
        internal IEditorOperationsFactoryService _EditOperationsFactory = null; // Set via MEF
        [Import]
        internal IVsEditorAdaptersFactoryService _adaptersFactory { get; set; }
        [Import]
        internal ISignatureHelpBroker _SigBroker = null; // Set via MEF
        [Import]
        internal IQuickInfoBroker _QuickInfoBroker = null; // Set via MEF

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers) {
            // Only use the analyzer if the view is actually file backed. If it is the REPL window
            // we don't use this.
            if (textView.GetFilePath() != null) {
                IronRubyToolsPackage.Instance.Analyzer.AnalyzeTextView(textView);
            }
            
            return new IntellisenseController(this, subjectBuffers, textView);
        }        
    }    
}
