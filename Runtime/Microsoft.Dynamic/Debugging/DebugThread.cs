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
using System.Diagnostics;

using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Debugging {
    [DebuggerDisplay("ThreadId = {_threadId}")]
    public abstract class DebugThread {
        private readonly DebugContext _debugContext;
        private Exception _leafFrameException;
        private bool _isInTraceback;
        private readonly int _threadId;

        internal DebugThread(DebugContext debugContext) {
            _debugContext = debugContext;
            _threadId = ThreadingUtils.GetCurrentThreadId();
        }

        internal DebugContext DebugContext {
            get { return _debugContext; }
        }

        internal Exception ThrownException {
            get { return _leafFrameException; }
            set { _leafFrameException = value; }
        }

        internal bool IsCurrentThread {
            get 
            {
                return _threadId == ThreadingUtils.GetCurrentThreadId(); 
            }
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
