/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
#if !SILVERLIGHT // Signals dont make much sense in Silverlight as cross-process communication is not allowed

using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    [RubyModule("Signal", BuildConfig="!SILVERLIGHT")]
    public static class Signal {
        #region Private Instance & Singleton Methods

        [RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ List(RubyContext/*!*/ context, RubyModule/*!*/ self) {
            Hash result = new Hash(context);
            result.Add(MutableString.CreateAscii("TERM"), ScriptingRuntimeHelpers.Int32ToObject(15));
            result.Add(MutableString.CreateAscii("SEGV"), ScriptingRuntimeHelpers.Int32ToObject(11));
            result.Add(MutableString.CreateAscii("KILL"), ScriptingRuntimeHelpers.Int32ToObject(9));
            result.Add(MutableString.CreateAscii("EXIT"), ScriptingRuntimeHelpers.Int32ToObject(0));
            result.Add(MutableString.CreateAscii("INT"), ScriptingRuntimeHelpers.Int32ToObject(2));
            result.Add(MutableString.CreateAscii("FPE"), ScriptingRuntimeHelpers.Int32ToObject(8));
            result.Add(MutableString.CreateAscii("ABRT"), ScriptingRuntimeHelpers.Int32ToObject(22));
            result.Add(MutableString.CreateAscii("ILL"), ScriptingRuntimeHelpers.Int32ToObject(4));
            return result;
        }

        /// <summary>
        /// Registers an interrupt handler. The host application is responsible for ensuring
        /// that the handler will actually be called.
        /// </summary>
        [RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
        public static object Trap(
            RubyContext/*!*/ context, 
            object self, 
            object signalId, 
            Proc proc) {

            if ((signalId is MutableString) && ((MutableString)signalId).ConvertToString() == "INT") {
                context.InterruptSignalHandler = () => proc.Call(null);
            } else {
                // TODO: For now, just ignore unknown signals. This should be changed to throw an
                // exception. We are not doing it yet as it is close to the V1 RTM, and throwing
                // an exception might cause some app to misbehave whereas it might have happenned
                // to work if no exception is thrown
            }
            return null;
        }

        /// <summary>
        /// Registers an interrupt handler. The host application is responsible for ensuring
        /// that the handler will actually be called.
        /// </summary>
        [RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
        public static object Trap(
            RubyContext/*!*/ context, 
            BlockParam block, 
            object self, 
            object signalId) {

            if ((signalId is MutableString) && ((MutableString)signalId).ConvertToString() == "INT") {
                context.InterruptSignalHandler = delegate() { object result; block.Yield(out result); };
            } else {
                // TODO: For now, just ignore unknown signals. This should be changed to throw an
                // exception. We are not doing it yet as it is close to the V1 RTM, and throwing
                // an exception might cause some app to misbehave whereas it might have happenned
                // to work if no exception is thrown
            }
            return null;
        }

        #endregion
    }
}
#endif
