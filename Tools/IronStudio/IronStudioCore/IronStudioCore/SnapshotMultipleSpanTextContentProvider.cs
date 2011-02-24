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
    internal class SnapshotMultipleSpanTextContentProvider : TextContentProvider, ISnapshotTextContentProvider {
        private readonly NormalizedSnapshotSpanCollection _spans;

        internal SnapshotMultipleSpanTextContentProvider(NormalizedSnapshotSpanCollection spans) {
            _spans = spans;
        }

        internal static SourceUnit Make(ScriptEngine engine, NormalizedSnapshotSpanCollection spans) {
            return Make(engine, spans, "None");
        }

        internal static SourceUnit Make(ScriptEngine engine, NormalizedSnapshotSpanCollection spans, string path) {
            var textContent = new SnapshotMultipleSpanTextContentProvider(spans);
            var languageContext = HostingHelpers.GetLanguageContext(engine);
            return new SourceUnit(languageContext, textContent, path, SourceCodeKind.File);
        }

        #region TextContentProvider

        public override SourceCodeReader GetReader() {
            return new SourceCodeReader(new SnapshotMultipleSpanSourceCodeReader(_spans), Encoding.Default);
        }

        #endregion

        #region ISnapshotTextContentProvider

        public ITextSnapshot Snapshot {
            get {
                if (_spans.Count > 0) {
                    return _spans[0].Snapshot;
                }
                return null;
            }
        }

        #endregion
    }
}
