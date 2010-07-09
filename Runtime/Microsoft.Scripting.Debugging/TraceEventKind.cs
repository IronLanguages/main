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
using System.Text;

namespace Microsoft.Scripting.Debugging {
    public enum TraceEventKind {
        // Fired when the execution enters a new frame
        //
        // Payload:
        //   none
        FrameEnter,

        // Fired when the execution leaves a frame
        //
        // Payload:
        //   return value from the function
        FrameExit,

        // Fired when the execution leaves a frame
        //
        // Payload:
        //   none
        ThreadExit,

        // Fired when the execution encounters a trace point
        //
        // Payload:
        //   none
        TracePoint,

        // Fired when an exception is thrown during the execution
        // 
        // Payload:
        //   the exception object that was thrown
        Exception,

        // Fired when an exception is thrown and is not handled by 
        // the current method.
        //
        // Payload:
        //   the exception object that was thrown
        ExceptionUnwind,
    }
}
