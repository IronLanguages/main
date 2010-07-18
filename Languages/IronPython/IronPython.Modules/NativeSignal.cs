/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System.Runtime.InteropServices;

namespace IronPython.Modules {
#if !SILVERLIGHT
    public static class NativeSignal {

        //Windows API expects to be given a function pointer like this to handle signals
        internal delegate bool WinSignalsHandler(uint winSignal);

        [DllImport("Kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetConsoleCtrlHandler(WinSignalsHandler Handler, [MarshalAs(UnmanagedType.Bool)]bool Add);

        [DllImport("Kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    }
#endif
}