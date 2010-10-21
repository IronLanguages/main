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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronRubyTools.Intellisense {
    class SnapshotCookie : IAnalysisCookie {
        private readonly ITextSnapshot _snapshot;
        
        public SnapshotCookie(ITextSnapshot snapshot) {
            _snapshot = snapshot;
        }

        public ITextSnapshot Snapshot {
            get {
                return _snapshot;
            }
        }

        #region IFileCookie Members

        public string GetLine(int lineNo) {
            return _snapshot.GetLineFromLineNumber(lineNo - 1).GetText();
        }

        #endregion
    }
}
