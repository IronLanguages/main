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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using Microsoft.Scripting.Debugging.CompilerServices;

namespace Microsoft.Scripting.Debugging {
    public static class RuntimeOps {
        [Obsolete("do not call this method", true)]
        public static DebugFrame CreateFrameForGenerator(DebugContext debugContext, FunctionInfo func) {
            return debugContext.CreateFrameForGenerator(func);
        }

        [Obsolete("do not call this method", true)]
        public static bool PopFrame(DebugThread thread) {
            return thread.PopFrame();
        }

        [Obsolete("do not call this method", true)]
        public static void OnTraceEvent(DebugThread thread, int debugMarker, Exception exception) {
            thread.DebugContext.DispatchDebugEvent(thread, debugMarker, exception != null ? TraceEventKind.Exception : TraceEventKind.TracePoint, exception);
        }

        [Obsolete("do not call this method", true)]
        public static void OnTraceEventUnwind(DebugThread thread, int debugMarker, Exception exception) {
            thread.DebugContext.DispatchDebugEvent(thread, debugMarker, TraceEventKind.ExceptionUnwind, exception);
        }

        [Obsolete("do not call this method", true)]
        public static void OnFrameEnterTraceEvent(DebugThread thread) {
            thread.DebugContext.DispatchDebugEvent(thread, 0, TraceEventKind.FrameEnter, null);
        }

        [Obsolete("do not call this method", true)]
        public static void OnFrameExitTraceEvent(DebugThread thread, int debugMarker, object retVal) {
            thread.DebugContext.DispatchDebugEvent(thread, debugMarker, TraceEventKind.FrameExit, retVal);
        }

        [Obsolete("do not call this method", true)]
        public static void OnThreadExitEvent(DebugThread thread) {
            thread.DebugContext.DispatchDebugEvent(thread, Int32.MaxValue, TraceEventKind.ThreadExit, null);
        }

        [Obsolete("do not call this method", true)]
        public static void ReplaceLiftedLocals(DebugFrame frame, IRuntimeVariables liftedLocals) {
            frame.ReplaceLiftedLocals(liftedLocals);
        }

        [Obsolete("do not call this method", true)]
        public static object GeneratorLoopProc(DebugThread thread) {
            bool moveNext;
            return thread.DebugContext.GeneratorLoopProc(thread.GetLeafFrame(), out moveNext);
        }

        [Obsolete("do not call this method", true)]
        public static IEnumerator<T> CreateDebugGenerator<T>(DebugFrame frame) {
            return new DebugGenerator<T>(frame);
        }

        [Obsolete("do not call this method", true)]
        public static int GetCurrentSequencePointForGeneratorFrame(DebugFrame frame) {
            Debug.Assert(frame != null);
            Debug.Assert(frame.Generator != null);

            return frame.CurrentLocationCookie;
        }

        [Obsolete("do not call this method", true)]
        public static int GetCurrentSequencePointForLeafGeneratorFrame(DebugThread thread) {
            DebugFrame frame = thread.GetLeafFrame();
            Debug.Assert(frame.Generator != null);

            return frame.CurrentLocationCookie;
        }

        [Obsolete("do not call this method", true)]
        public static bool IsCurrentLeafFrameRemappingToGenerator(DebugThread thread) {
            DebugFrame frame = null;
            if (thread.TryGetLeafFrame(ref frame)) {
                return frame.ForceSwitchToGeneratorLoop;
            }

            return false;
        }

        [Obsolete("do not call this method", true)]
        public static FunctionInfo CreateFunctionInfo(
            Delegate generatorFactory,
            string name,
            object locationSpanMap,
            object scopedVariables,
            object variables,
            object customPayload) {
            return DebugContext.CreateFunctionInfo(generatorFactory, name, (DebugSourceSpan[])locationSpanMap, (IList<VariableInfo>[])scopedVariables, (IList<VariableInfo>)variables, customPayload);
        }

        [Obsolete("do not call this method", true)]
        public static DebugThread GetCurrentThread(DebugContext debugContext) {
            return debugContext.GetCurrentThread();
        }

        [Obsolete("do not call this method", true)]
        public static DebugThread GetThread(DebugFrame frame) {
            return frame.Thread;
        }

        [Obsolete("do not call this method", true)]
        public static bool[] GetTraceLocations(FunctionInfo functionInfo) {
            return functionInfo.GetTraceLocations();
        }

        [Obsolete("do not call this method", true)]
        public static void LiftVariables(DebugThread thread, IRuntimeVariables runtimeVariables) {
            ((DefaultDebugThread)thread).LiftVariables(runtimeVariables);
        }
    }
}
