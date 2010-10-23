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
using Microsoft.VisualStudio.Text;

namespace Microsoft.IronStudio.Library.Repl {
    public class ReplSpan {
        public ReplSpan(bool wasCommand, bool wasException, SnapshotSpan input, SnapshotSpan? output) {
            WasCommand = wasCommand;
            WasException = wasException;
            Input = input;
            Output = output;
        }
        public readonly bool WasCommand;
        public readonly bool WasException;
        public readonly SnapshotSpan Input;
        public readonly SnapshotSpan? Output;
    }
}
