/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***********************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;

using IronPython.Runtime;
using IronPython.Runtime.Operations;

#if CLR2
using Microsoft.Scripting.Math;
#else
using System.Numerics;
#endif
#if !SILVERLIGHT

[assembly: PythonModule("msvcrt", typeof(IronPython.Modules.PythonMsvcrt))]
namespace IronPython.Modules {
    [PythonType("msvcrt")]
    public class PythonMsvcrt {
        public const string __doc__ = "msvcrt Module";

        #region Public API

        // python call: c2pread = msvcrt.open_osfhandle(c2pread.Detach(), 0)
        public static int open_osfhandle(CodeContext context, BigInteger os_handle, int arg1) {
            FileStream stream = new FileStream(new IntPtr((long)os_handle), FileAccess.ReadWrite, true);
            return context.LanguageContext.FileManager.AddToStrongMapping(stream);
        }

        public static object get_osfhandle(CodeContext context, int fd) {
            PythonFile pfile = context.LanguageContext.FileManager.GetFileFromId(context.LanguageContext, fd);

            FileStream stream = pfile._stream as FileStream;
            if (stream != null) {
                return stream.Handle.ToPython();
            }
            return -1;
        }

        public static int setmode(CodeContext context, int fd, int flags) {
            PythonFile pfile = context.LanguageContext.FileManager.GetFileFromId(context.LanguageContext, fd);
            int oldMode;
            if (flags == PythonNT.O_TEXT) {
                oldMode = pfile.SetMode(context, true) ? PythonNT.O_TEXT : PythonNT.O_BINARY;
            } else if (flags == PythonNT.O_BINARY) {
                oldMode = pfile.SetMode(context, false) ? PythonNT.O_TEXT : PythonNT.O_BINARY;
            } else {
                throw PythonOps.ValueError("unknown mode: {0}", flags);
            }
            return oldMode;
        }

        public static string getch() {
            return new string((char)_getch(), 1);
        }

        #endregion

        [DllImport("msvcr100")]
        private static extern int _getch();
    }
}
#endif