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

using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronStudio.Core {
    internal class SnapshotSpanTextContentProvider : TextContentProvider, ISnapshotTextContentProvider {
        private readonly SnapshotSpan _span;

        internal SnapshotSpanTextContentProvider(SnapshotSpan span) {
            _span = span;
        }

        #region TextContentProvider

        public override SourceCodeReader GetReader() {
            return new SourceCodeReader(new SnapshotSpanSourceCodeReader(_span), Encoding.Default);
        }

        #endregion

        #region ISnapshotTextContentProvider

        public ITextSnapshot Snapshot {
            get {
                return _span.Snapshot;
            }
        }

        #endregion
    }
}
