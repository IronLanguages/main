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

using System;
using System.ComponentModel.Composition;
using Microsoft.IronRubyTools.Editor.Core;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools.Editor {
    /// <summary>
    /// Watches for text views to be created for IronRuby code.  Then wires up whatever event handling
    /// we're interested in.  Currently this just handles highlighting matching braces but we should replace
    /// our CodeWindowManager created via our language service and instead just use this.  
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType(RubyCoreConstants.ContentType)]
    class TextViewCreationListener : IVsTextViewCreationListener {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;

        public void VsTextViewCreated(VisualStudio.TextManager.Interop.IVsTextView textViewAdapter) {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView != null) {
                BraceMatcher.WatchBraceHighlights(textView, IronRubyToolsPackage.ComponentModel);
            }
        }
    }
}
