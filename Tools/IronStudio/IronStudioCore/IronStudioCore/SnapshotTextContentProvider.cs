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
using Microsoft.IronStudio.Core;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronStudio.Library {
    public class SnapshotTextContentProvider : TextContentProvider, ISnapshotTextContentProvider {
        private readonly ITextSnapshot _snapshot;

        internal SnapshotTextContentProvider(ITextSnapshot snapshot) {
            _snapshot = snapshot;
        }

        public static SourceUnit Make(ScriptEngine engine, ITextSnapshot snapshot) {
            return Make(engine, snapshot, "None");
        }

        public static SourceUnit Make(ScriptEngine engine, ITextSnapshot snapshot, string path) {
            var textContent = new SnapshotTextContentProvider(snapshot);
            var languageContext = HostingHelpers.GetLanguageContext(engine);
            return new SourceUnit(languageContext, textContent, path, SourceCodeKind.File);
        }

        #region TextContentProvider

        public override SourceCodeReader GetReader() {
            var span = new SnapshotSpan(_snapshot, 0, _snapshot.Length);
            return new SourceCodeReader(new SnapshotSpanSourceCodeReader(span), Encoding.Default);
        }

        #endregion

        #region ISnapshotTextContentProvider

        public ITextSnapshot Snapshot {
            get {
                return _snapshot;
            }
        }

        #endregion
    }
}
