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
using System.Diagnostics;

namespace Microsoft.Scripting.Utils {
    public static class MonitorUtils {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static void Enter(object obj, ref bool lockTaken) {
#if CLR2
            Monitor.Enter(obj);
            lockTaken = true;
#else
            Monitor.Enter(obj, ref lockTaken);
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static bool TryEnter(object obj, ref bool lockTaken) {
#if CLR2
            return lockTaken = Monitor.TryEnter(obj);
#else
            Monitor.TryEnter(obj, ref lockTaken);
            return lockTaken;
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public static void Exit(object obj, ref bool lockTaken) {
            try {
            } finally {
                // finally prevents thread abort to leak the lock:
                lockTaken = false;
                Monitor.Exit(obj);
            }
        }
    }
}
