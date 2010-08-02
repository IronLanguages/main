/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Scripting.Debugging.CompilerServices;

namespace Microsoft.Scripting.Debugging {
    public abstract class DebugThread {
        private readonly DebugContext _debugContext;
        private readonly Thread _managedThread;
        private Exception _leafFrameException;
        private bool _isInTraceback;

        internal DebugThread(DebugContext debugContext) {
            _debugContext = debugContext;
            _managedThread = Thread.CurrentThread;
        }

        internal DebugContext DebugContext {
            get { return _debugContext; }
        }

        internal Exception ThrownException {
            get { return _leafFrameException; }
            set { _leafFrameException = value; }
        }

        internal Thread ManagedThread {
            get { return _managedThread; }
        }

        internal bool IsInTraceback {
            get { return _isInTraceback; }
            set { _isInTraceback = value; }
        }

        #region Abstract Methods

        internal abstract IEnumerable<DebugFrame> Frames { get; }
        internal abstract DebugFrame GetLeafFrame();
        internal abstract bool TryGetLeafFrame(ref DebugFrame frame);
        internal abstract int FrameCount { get; }
        internal abstract void PushExistingFrame(DebugFrame frame);
        internal abstract bool PopFrame();
        internal abstract FunctionInfo GetLeafFrameFunctionInfo(out int stackDepth);

        #endregion
    }
}
