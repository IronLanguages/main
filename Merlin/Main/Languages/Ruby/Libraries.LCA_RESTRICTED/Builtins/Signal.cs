/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
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

        [RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
        public static object Trap(RubyContext/*!*/ context, object self, object signalId, Proc proc) {
            // TODO: For now, just ignore the signal handler. The full implementation will need to build on 
            // the signal and raise functions in msvcrt.
            return null;
        }

        [RubyMethod("trap", RubyMethodAttributes.PublicSingleton)]
        public static object Trap(RubyContext/*!*/ context, BlockParam block, object self, object signalId) {
            // TODO: For now, just ignore the signal handler. The full implementation will need to build on 
            // the signal and raise functions in msvcrt.
            return null;
        }


        #endregion
    }
}
#endif
