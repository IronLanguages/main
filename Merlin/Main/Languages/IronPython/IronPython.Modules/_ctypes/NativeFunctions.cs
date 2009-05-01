/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

#if !SILVERLIGHT
//[assembly: PythonModule("_ctypes", typeof(IronPython.Modules.CTypes))]
namespace IronPython.Modules {
    /// <summary>
    /// Native functions used for exposing ctypes functionality.
    /// </summary>
    static class NativeFunctions {
        private static SetMemoryDelegate _setMem = MemSet;
        private static MoveMemoryDelegate _moveMem = MoveMemory;

        private delegate IntPtr SetMemoryDelegate(IntPtr dest, byte value, IntPtr length);
        private delegate IntPtr MoveMemoryDelegate(IntPtr dest, IntPtr src, IntPtr length);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void SetLastError(int errorCode);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, int ordinal);

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, IntPtr Length);

        /// <summary>
        /// Allocates memory that's zero-filled
        /// </summary>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IntPtr Calloc(IntPtr size) {
            return LocalAlloc(LMEM_ZEROINIT, size);
        }

        public static IntPtr GetMemMoveAddress() {
            return Marshal.GetFunctionPointerForDelegate(_moveMem);
        }

        public static IntPtr GetMemSetAddress() {
            return Marshal.GetFunctionPointerForDelegate(_setMem);
        }

        [DllImport("kernel32.dll"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static extern IntPtr LocalAlloc(uint flags, IntPtr size);

        private const int LMEM_ZEROINIT = 0x0040;

        [DllImport("kernel32.dll")]
        private static extern void RtlMoveMemory(IntPtr Destination, IntPtr src, IntPtr length);

        [DllImport("kernel32.dll")]
        private static extern void FillMemory(IntPtr Destination, IntPtr Length, byte fill);

        /// <summary>
        /// Helper function for translating from memset to NT's FillMemory API.
        /// </summary>
        private static IntPtr MemSet(IntPtr dest, byte value, IntPtr length) {
            NativeFunctions.FillMemory(dest, length, value);
            return dest;
        }

        private static IntPtr MoveMemory(IntPtr dest, IntPtr src, IntPtr length) {
            RtlMoveMemory(dest, src, length);
            return dest;
        }
    }
}

#endif