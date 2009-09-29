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
#if !SILVERLIGHT 

using System;
using System.Diagnostics;
using System.Security;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    /// <summary>
    /// Process builtin module
    /// </summary>
    [RubyModule("Process", BuildConfig = "!SILVERLIGHT")]
    public static class RubyProcess {
        
        [RubyClass("Status", BuildConfig = "!SILVERLIGHT")]
        public class Status {
            private readonly Process/*!*/ _process;

            public Status(Process/*!*/ process) {
                _process = process;
            }

            [RubyMethod("coredump?")]
            public static bool CoreDump(Status/*!*/ self) {
                // Always false on Windows
                return false;
            }

            [RubyMethod("exitstatus")]
            public static int ExitStatus(Status/*!*/ self) {
                return self._process.ExitCode;
            }

            [RubyMethod("exited?")]
            public static bool Exited(Status/*!*/ self) {
                return self._process.HasExited;
            }

            [RubyMethod("pid")]
            public static int Pid(Status/*!*/ self) {
                return self._process.Id;
            }

            [RubyMethod("stopped?")]
            public static bool Stopped(Status/*!*/ self) {
                // Always false on Windows
                return false;
            }

            [RubyMethod("stopsig")]
            public static object StopSig(Status/*!*/ self) {
                // Always nil on Windows
                return null;
            }

            [RubyMethod("success?")]
            public static bool Success(Status/*!*/ self) {
                return self._process.ExitCode == 0;
            }

            [RubyMethod("termsig")]
            public static object TermSig(Status/*!*/ self) {
                // Always nil on Windows
                return null;
            }
        }

        // abort
        // detach
        // egid
        // egid=

        [RubyMethod("euid", RubyMethodAttributes.PublicSingleton)]
        public static int EffectiveUserId(RubyModule/*!*/ self) {
            return 0; // always 0 on Windows?
        }

        // euid=
        // exit
        // exit!
        // fork
        // getpgid
        // getpriority
        // getrlimit
        // gid
        // gid=
        // groups
        // groups=
        // initgroups

        [RubyMethod("kill", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("kill", RubyMethodAttributes.PublicSingleton)]
        public static object Kill(RubyModule/*!*/ self, object signalId, object pid) {
            throw RubyExceptions.CreateNotImplementedError("Signals are not currently implemented. Signal.trap just pretends to work");
        }

        // maxgroups
        // maxgroups=

        [RubyMethod("pid", RubyMethodAttributes.PublicSingleton)]
        public static int GetPid(RubyModule/*!*/ self) {
            return Process.GetCurrentProcess().Id;
        }

        [RubyMethod("ppid", RubyMethodAttributes.PublicSingleton)]
        public static int GetParentPid(RubyModule/*!*/ self) {
            return 0;
        }

        // setpgid
        // setpgrp
        // setpriority
        // setrlimit
        // setsid

        [RubyMethod("times", RubyMethodAttributes.PublicSingleton)]
        public static RubyStruct/*!*/ GetTimes(RubyModule/*!*/ self) {
            var result = RubyStruct.Create(RubyStructOps.GetTmsClass(self.Context));
            try {
                FillTimes(result);
            } catch (SecurityException) {
                RubyStructOps.TmsSetUserTime(result, 0.0);
                RubyStructOps.TmsSetSystemTime(result, 0.0);
                RubyStructOps.TmsSetChildUserTime(result, 0.0);
                RubyStructOps.TmsSetChildSystemTime(result, 0.0);
            }

            return result;
        }

        private static void FillTimes(RubyStruct/*!*/ result) {
            var process = Process.GetCurrentProcess();
            RubyStructOps.TmsSetUserTime(result, process.UserProcessorTime.TotalSeconds);
            RubyStructOps.TmsSetSystemTime(result, process.PrivilegedProcessorTime.TotalSeconds);
            RubyStructOps.TmsSetChildUserTime(result, 0.0);
            RubyStructOps.TmsSetChildSystemTime(result, 0.0);
        }

        [RubyMethod("uid", RubyMethodAttributes.PublicSingleton)]
        public static int UserId(RubyModule/*!*/ self) {
            return 0; // always 0 on Windows?
        }

        [RubyMethod("uid=", RubyMethodAttributes.PublicSingleton)]
        public static void SetUserId(RubyModule/*!*/ self, object temp) {
            throw new NotImplementedError("uid=() function is unimplemented on this machine");
        }

        [RubyMethod("wait", RubyMethodAttributes.PublicSingleton)]
        public static void Wait(RubyModule/*!*/ self) {
            throw new Errno.ChildError();
        }

        [RubyMethod("wait2", RubyMethodAttributes.PublicSingleton)]
        public static void Wait2(RubyModule/*!*/ self) {
            throw new Errno.ChildError();
        }

        [RubyMethod("waitall", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Waitall(RubyModule/*!*/ self) {
            return new RubyArray(); //Process.waitall always returns an empty array on Windows?
        }

        // waitpid
        // waitpid2  
    }

    public static partial class RubyStructOps {
        #region Tms

        internal static readonly object TmsStructClassKey = new object();
        
        [RubyConstant("Tms", BuildConfig = "!SILVERLIGHT")]
        internal static RubyClass/*!*/ CreateTmsClass(RubyModule/*!*/ module) {
            // class is available for the library even if the constant is removed => store it on the context:
            return (RubyClass)module.Context.GetOrCreateLibraryData(TmsStructClassKey, () => RubyStruct.DefineStruct(
                (RubyClass)module, 
                "Tms",
                new[] { "utime", "stime", "cutime", "cstime" }
            ));
        }

        public static RubyClass/*!*/ GetTmsClass(RubyContext/*!*/ context) {
            ContractUtils.RequiresNotNull(context, "context");

            object value;
            if (context.TryGetLibraryData(TmsStructClassKey, out value)) {
                return (RubyClass)value;
            }

            // trigger constant initialization of Struct class:
            context.GetClass(typeof(RubyStruct)).TryGetConstant(null, String.Empty, out value);

            // try again:
            if (context.TryGetLibraryData(TmsStructClassKey, out value)) {
                return (RubyClass)value;
            }

            throw Assert.Unreachable;
        }

        public static void TmsSetUserTime(RubyStruct/*!*/ tms, double value) {
            tms[0] = value;
        }

        public static void TmsSetSystemTime(RubyStruct/*!*/ tms, double value) {
            tms[1] = value;
        }

        public static void TmsSetChildUserTime(RubyStruct/*!*/ tms, double value) {
            tms[2] = value;
        }

        public static void TmsSetChildSystemTime(RubyStruct/*!*/ tms, double value) {
            tms[3] = value;
        }

        #endregion
    }
}
#endif
