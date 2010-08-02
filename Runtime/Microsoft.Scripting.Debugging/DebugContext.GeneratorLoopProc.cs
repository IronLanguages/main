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
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Scripting.Debugging.CompilerServices {
    public sealed partial class DebugContext {
        private static object _debugYieldValue;

        internal static object DebugYieldValue {
            get {
                if (_debugYieldValue == null)
                    _debugYieldValue = new object();
                return _debugYieldValue; 
            }
        }

        internal object GeneratorLoopProc(DebugFrame frame, out bool moveNext) {
            Debug.Assert(frame.Generator != null);

            moveNext = true;
            bool skipTraceEvent = true;
            object retVal;

            if (frame.ForceSwitchToGeneratorLoop) {
                // Reset ForceSwitchToGeneratorLoop flag
                frame.ForceSwitchToGeneratorLoop = false;
            }

            while (true) {
                if (!skipTraceEvent) {
                    if (frame.FunctionInfo.SequencePoints[frame.CurrentLocationCookie].SourceFile.DebugMode == DebugMode.FullyEnabled ||
                        frame.FunctionInfo.SequencePoints[frame.CurrentLocationCookie].SourceFile.DebugMode == DebugMode.TracePoints && frame.FunctionInfo.GetTraceLocations()[frame.CurrentLocationCookie]) {
                            Debug.Assert(((IEnumerator)frame.Generator).Current == DebugYieldValue);
                            frame.InGeneratorLoop = true;
                            try {
                                DispatchDebugEvent(frame.Thread, frame.CurrentLocationCookie, TraceEventKind.TracePoint, null);
                            }
#if DEBUG
                            catch (ForceToGeneratorLoopException) {
                                Debug.Assert(false, "ForceToGeneratorLoopException thrown in generator loop");
                                throw;
                            }
#endif
                            finally {
                                frame.InGeneratorLoop = false;
                            }
                    }
                } else {
                    skipTraceEvent = false;
                }

                // Advance to next yield
                try {
                    moveNext = ((IEnumerator)frame.Generator).MoveNext();
                    object current = ((IEnumerator)frame.Generator).Current;

                    // Update the last known marker
                    if (frame.Generator.YieldMarkerLocation != Int32.MaxValue)
                        frame.LastKnownGeneratorYieldMarker = frame.Generator.YieldMarkerLocation;

                    // Check if this was a user-code yield or a debug yield
                    if (current != DebugYieldValue || !moveNext) {
                        if (moveNext) {
                            retVal = current;
                        } else {
                            retVal = null;
                        }

                        break;
                    }
                } catch (ForceToGeneratorLoopException) {
                    // We land here when an exception is thrown from a nested catch block and if that exception is being cancelled.
                    skipTraceEvent = true;
                } catch (Exception ex) {
                    if (frame.DebugContext.DebugMode != DebugMode.Disabled) {
                        try {
                            frame.InGeneratorLoop = true;
                            DispatchDebugEvent(frame.Thread, frame.CurrentLocationCookie, TraceEventKind.ExceptionUnwind, ex);
                        } finally {
                            frame.InGeneratorLoop = false;
                        }
                    } else {
                        throw;
                    }

                    // Rethrow if the exception is not cancelled
                    if (frame.ThrownException != null)
                        throw;

                    skipTraceEvent = true;
                }
            }

            Debug.Assert(retVal != DebugYieldValue);
            return retVal;
        }
    }
}
