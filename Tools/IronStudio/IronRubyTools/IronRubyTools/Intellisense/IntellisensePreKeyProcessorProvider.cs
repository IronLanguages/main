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

using System.ComponentModel.Composition;
using Microsoft.IronStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.IronRubyTools.Intellisense {
    [Export(typeof(IKeyProcessorProvider))]
    [Name("Intellisense Sample Preprocess KeyProcessor")]
    [Order(After = "DefaultKeyProcessor")]
    [ContentType(RubyCoreConstants.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class IntellisensePreKeyProcessorProvider : IKeyProcessorProvider {
        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return new IntellisensePreKeyProcessor(wpfTextView);
        }
    }
}
