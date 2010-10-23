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

#if !SILVERLIGHT

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Builtins;

namespace IronRuby.StandardLibrary.Sockets {
    [RubyClass("SocketError", BuildConfig = "!SILVERLIGHT", Extends = typeof(SocketException), Inherits = typeof(SystemException))]
    [HideMethod("message")] // SocketException overrides Message so we have to hide it here
    public static class SocketErrorOps {
        [RubyConstructor]
        public static Exception/*!*/ Create(RubyClass/*!*/ self, [DefaultParameterValue(null)]object message) {
            return RubyExceptionData.InitializeException(new SocketException(0), message ?? MutableString.CreateAscii("SocketError"));

        }

        public static Exception/*!*/ Create(MutableString/*!*/ message) {
            return RubyExceptionData.InitializeException(new SocketException(0), message);
        }
    }
}

#endif
