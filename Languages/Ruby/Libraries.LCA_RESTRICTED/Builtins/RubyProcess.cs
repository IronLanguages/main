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
using System.Diagnostics;
using System.Security;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace IronRuby.Builtins {

    /// <summary>
    /// Process builtin module
    /// </summary>
    [RubyModule("Process", BuildConfig = "!SILVERLIGHT")]
    public static class RubyProcess {
        #region Utils

        internal static Process/*!*/ CreateProcess(RubyContext/*!*/ context, MutableString/*!*/ command, MutableString[]/*!*/ args) {
            var psi = new ProcessStartInfo(command.ToString(), JoinArguments(args).ToString());
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            try {
                Utils.Log(String.Format("Starting: '{0}' with args: '{1}'", psi.FileName, psi.Arguments), "PROCESS");
                Process p = Process.Start(psi);
                p.WaitForExit();
                context.ChildProcessExitStatus = new RubyProcess.Status(p);
                return p;
            } catch (Exception e) {
                throw RubyExceptions.CreateENOENT(psi.FileName, e);
            }
        }

        internal static Process/*!*/ CreateProcess(RubyContext/*!*/ context, MutableString/*!*/ command, bool redirectOutput) {
            return CreateProcess(context, command, false, redirectOutput, false);
        }

        internal static Process/*!*/ CreateProcess(RubyContext/*!*/ context, MutableString/*!*/ command,
            bool redirectInput, bool redirectOutput, bool redirectErrorOutput) {

            string fileName, arguments;
            RubyProcess.GetExecutable(context.DomainManager.Platform, command.ToString(), out fileName, out arguments);
            Utils.Log(String.Format("Starting: '{0}' with args: '{1}'", fileName, arguments), "PROCESS");

            var p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = redirectInput;
            p.StartInfo.RedirectStandardOutput = redirectOutput;
            p.StartInfo.RedirectStandardError = redirectErrorOutput;
            try {
                p.Start();
            } catch (Exception e) {
                throw RubyExceptions.CreateENOENT(fileName, e);
            }

            context.ChildProcessExitStatus = new RubyProcess.Status(p);
            return p;
        }

        private static void GetExecutable(PlatformAdaptationLayer/*!*/ pal, string/*!*/ command, out string executable, out string arguments) {
            command = command.Trim(' ');
            if (command.Length == 0) {
                throw RubyExceptions.CreateEINVAL(command);
            }

            // This seems to be quite complicated:
            // 1) If the first part of the command is a shell command (DIR, ECHO, ...) or
            //    if the command contains unquoted <, > or | then
            //    it uses ENV['COMSPEC'] to execute the command: %COMSPEC% /c "COMMAND"
            // 2) It looks for the shortest prefix of command that is separated by space from the rest of the command that is either
            //      a) An absolute path to an executable file.
            //      b) Try prepend paths from ENV['PATH']
            //      c) Try Environment.SpecialFolder.System.
            //      d) Try SHGetFolderPath(CSIDL_WINDOWS) - we can't get this from Environment.SpecialFolder, so we need to use 
            //         ENV["SystemRoot"] environment variable.
            //    In each step it tries to append ".exe" or ".com" extension if the path doesn't exist.
            // 
            //    For example, if the command is `x/a b/x` and the directory structure is
            //      x\a b\x.exe
            //      x\a.exe
            //    it executes a.exe.
            //    
            //    MRI probably calls CreateProcess Win32 API with lpApplicationName it resolves as described above and 
            //    lpCommandLine == command. System.Diagnostics.Process also uses this API with lpApplicationName == NULL and 
            //    lpCommandLine == '"{ProcessStartInfo.FileName}" {ProcessStartInfo.Arguments}'.
            //
            //    Although CreateProcess does all the searching for an executable if passed no lpApplicationName, 
            //    we need to do it ourselves because it is slightly different in MRI (is that a bug?) and also because System.Diagnostics.Process 
            //    quotes the FileName :(
            //    

            string comspec = pal.GetEnvironmentVariable("COMSPEC");
            if (!pal.FileExists(comspec)) {
                comspec = null;
            }

            if (comspec != null && IndexOfUnquotedSpecialCharacter(command) >= 0) {
                executable = comspec;
                arguments = "/c \"" + command + "\"";
                return;
            }

            int start = 0;
            while (true) {
                int next = command.IndexOf(' ', start);

                executable = (next >= 0) ? command.Substring(0, next) : command;
                arguments = (next >= 0) ? command.Substring(next + 1) : "";

                if (start == 0 && comspec != null && IsShellCommand(executable)) {
                    executable = comspec;
                    arguments = "/c \"" + command + "\"";
                    return;
                }

                try {
                    foreach (var path in GetExecutableFiles(pal, executable)) {
                        if (pal.FileExists(path)) {
                            // We need to set the path we found as executable. Althought makes command line of the target process
                            // different from when called by MRI it will execute the right process. If we passed the original executable name
                            // CreateProcess might resolve it to a different executable.
                            executable = path;
                            return;
                        }
                    }
                } catch (Exception e) {
                    if (next < 0) {
                        throw RubyExceptions.CreateENOENT(command, e);
                    }
                }

                if (next < 0) {
                    throw RubyExceptions.CreateENOENT(command);
                }

                start = next + 1;
                while (start < command.Length && command[start] == ' ') {
                    start++;
                }
            }
        }

        private static int IndexOfUnquotedSpecialCharacter(string/*!*/ str) {
            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (c == '"') {
                    inDoubleQuote = !inDoubleQuote;
                } else if (c == '\'') {
                    inSingleQuote = !inSingleQuote;
                } else if (c == '>' || c == '<' || c == '|') {
                    if (!inSingleQuote && !inDoubleQuote) {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static string[] _ExecutableExtensions = new[] { ".exe", ".com", ".bat", ".cmd" };

        private static IEnumerable<string>/*!*/ GetExecutableFiles(PlatformAdaptationLayer/*!*/ pal, string/*!*/ path) {
            if (path[0] == '"' || path[path.Length - 1] == '"') {
                if (path.Length >= 3 && path[0] == '"' && path[path.Length - 1] == '"') {
                    path = path.Substring(1, path.Length - 2);
                } else {
                    yield break;
                }
            }

            string extension = RubyUtils.GetExtension(path);
            bool hasExtension = !String.IsNullOrEmpty(extension);
            bool isExecutable = hasExtension && Array.IndexOf(_ExecutableExtensions, extension.ToLowerInvariant()) >= 0;

            if (!hasExtension || isExecutable) {
                foreach (var fullPath in GetAbsolutePaths(pal, path)) {
                    if (hasExtension) {
                        yield return fullPath;
                    } else {
                        foreach (var ext in _ExecutableExtensions) {
                            yield return fullPath + ext;
                        }
                    }
                }
            }
        }

        private static IEnumerable<string>/*!*/ GetAbsolutePaths(PlatformAdaptationLayer/*!*/ pal, string/*!*/ path) {
            if (pal.IsAbsolutePath(path)) {
                yield return path;
            } else {
                yield return pal.GetFullPath(path);

                string var = pal.GetEnvironmentVariable("PATH");
                if (!String.IsNullOrEmpty(var)) {
                    foreach (var prefix in var.Split(Path.PathSeparator)) {
                        if (prefix.Length > 0) {
                            yield return Path.Combine(prefix, path);
                        }
                    }
                }

                var = Environment.GetFolderPath(Environment.SpecialFolder.System);
                if (!String.IsNullOrEmpty(var)) {
                    yield return Path.Combine(var, path);
                }

                var = pal.GetEnvironmentVariable("SystemRoot");
                if (!String.IsNullOrEmpty(var)) {
                    yield return Path.Combine(var, path);
                }
            }
        }

        private static bool IsShellCommand(string/*!*/ str) {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT &&
                Environment.OSVersion.Platform != PlatformID.Win32Windows) {
                return false;
            }

            switch (str.ToUpperInvariant()) {
                case "ASSOC":
                case "BREAK":
                case "CALL":
                case "CD":
                case "CHDIR":
                case "CLS":
                case "COLOR":
                case "COPY":
                case "DATE":
                case "DEL":
                case "DIR":
                case "ECHO":
                case "ENDLOCAL":
                case "ERASE":
                case "EXIT":
                case "FOR":
                case "FTYPE":
                case "GOTO":
                case "IF":
                case "MD":
                case "MKDIR":
                case "MOVE":
                case "PATH":
                case "PAUSE":
                case "POPD":
                case "PROMPT":
                case "PUSHD":
                case "RD":
                case "REM":
                case "REN":
                case "RENAME":
                case "RMDIR":
                case "SET":
                case "SETLOCAL":
                case "SHIFT":
                case "START":
                case "TIME":
                case "TITLE":
                case "TYPE":
                case "VER":
                case "VERIFY":
                case "VOL":
                    return true;

                case "MKLINK":
                    return Environment.OSVersion.Version.Major >= 6; // Vista and later
            };
            return false;
        }

        private static MutableString/*!*/ JoinArguments(MutableString/*!*/[]/*!*/ args) {
            MutableString result = MutableString.CreateMutable(RubyEncoding.Binary);

            for (int i = 0; i < args.Length; i++) {
                result.Append(args[i]);
                if (args.Length > 1 && i < args.Length - 1) {
                    result.Append(' ');
                }
            }

            return result;
        }

        #endregion

        #region Status

        [RubyClass("Status", BuildConfig = "!SILVERLIGHT")]
        [HideMethod("new", IsStatic = true)]
        public sealed class Status {
            private readonly Process/*!*/ _process;

            internal Status(Process/*!*/ process) {
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

            [RubyMethod("inspect")]
            public static MutableString/*!*/ Inspect(Status/*!*/ self) {
                return MutableString.CreateAscii(String.Format(CultureInfo.InvariantCulture, "#<Process::Status: pid={0},{1}({2})>", 
                    Pid(self),
                    Exited(self) ? "exited" : "running",
                    ExitStatus(self)
                ));
            }
        }

        #endregion

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

    #region Struct::Tms

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
            context.GetClass(typeof(RubyStruct)).TryGetConstant(null, "Tms", out value);

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

    #endregion
}
#endif
