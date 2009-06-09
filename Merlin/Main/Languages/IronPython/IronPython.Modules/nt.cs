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
 * ***************************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

[assembly: PythonModule("nt", typeof(IronPython.Modules.PythonNT))]
namespace IronPython.Modules {
    public static class PythonNT {
        #region Public API Surface

#if !SILVERLIGHT // FailFast
        public static void abort() {
            System.Environment.FailFast("IronPython os.abort");
        }
#endif

        /// <summary>
        /// Checks for the specific permissions, provided by the mode parameter, are available for the provided path.  Permissions can be:
        /// 
        /// F_OK: Check to see if the file exists
        /// R_OK | W_OK | X_OK: Check for the specific permissions.  Only W_OK is respected.
        /// </summary>
        public static bool access(CodeContext/*!*/ context, string path, int mode) {
            if (path == null) throw PythonOps.TypeError("expected string, got None");

            if (mode == F_OK) {
                return context.LanguageContext.DomainManager.Platform.FileExists(path);
            }
#if !SILVERLIGHT
            // match the behavior of the VC C Runtime
            FileAttributes fa = File.GetAttributes(path);
            if ((fa & FileAttributes.Directory) != 0) {
                // directories have read & write access
                return true;
            }

            if ((fa & FileAttributes.ReadOnly) != 0 && (mode & W_OK) != 0) {
                // want to write but file is read-only
                return false;
            }

            return true;
#else
            return false;
#endif
        }

#if !SILVERLIGHT // SetCurrentDirectory, FileInfo
        public static void chdir([NotNull]string path) {
            if (String.IsNullOrEmpty(path)) {
                throw PythonExceptions.CreateThrowable(WindowsError, PythonErrorNumber.EINVAL, "Path cannot be an empty string");
            }

            try {
                Directory.SetCurrentDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static void chmod(string path, int mode) {
            FileInfo fi = new FileInfo(path);
            if ((mode & S_IWRITE) != 0) {
                fi.Attributes &= ~(FileAttributes.ReadOnly);
            } else {
                fi.Attributes |= FileAttributes.ReadOnly;
            }
        }
#endif

        public static void close(CodeContext/*!*/ context, int fd) {
            PythonContext pythonContext = PythonContext.GetContext(context);
            PythonFile pf = pythonContext.FileManager.GetFileFromId(pythonContext, fd);
            pf.close();
        }
        
#if !SILVERLIGHT
        /// <summary>
        /// single instance of environment dictionary is shared between multiple runtimes because the environment
        /// is shared by multiple runtimes.
        /// </summary>
        public static readonly object environ = new PythonDictionary(new EnvironmentDictionaryStorage());
#endif

        public static readonly PythonType error = Builtin.OSError;

        public static void _exit(CodeContext/*!*/ context, int code) {
            PythonContext.GetContext(context).DomainManager.Platform.TerminateScriptExecution(code);
        }

        public static object fdopen(CodeContext/*!*/ context, int fd) {
            return fdopen(context, fd, "r");
        }

        public static object fdopen(CodeContext/*!*/ context, int fd, string mode) {
            return fdopen(context, fd, mode, 0);
        }

        public static object fdopen(CodeContext/*!*/ context, int fd, string mode, int bufsize) {
            // check for a valid file mode...
            PythonFile.ValidateMode(mode);

            PythonContext pythonContext = PythonContext.GetContext(context);
            PythonFile pf = pythonContext.FileManager.GetFileFromId(pythonContext, fd);
            return pf;
        }

        public static object fstat(CodeContext/*!*/ context, int fd) {
            PythonContext pythonContext = PythonContext.GetContext(context);
            PythonFile pf = pythonContext.FileManager.GetFileFromId(pythonContext, fd);
            if (pf.IsConsole) {
                return new stat_result(8192);
            }
            return lstat(pf.name);
        }

        public static string getcwd(CodeContext/*!*/ context) {
            return context.LanguageContext.DomainManager.Platform.CurrentDirectory;
        }

        public static string getcwdu(CodeContext/*!*/ context) {
            return context.LanguageContext.DomainManager.Platform.CurrentDirectory;
        }

        public static string _getfullpathname(CodeContext/*!*/ context, [NotNull]string/*!*/ dir) {
            PlatformAdaptationLayer pal = context.LanguageContext.DomainManager.Platform;

            try {
                return context.LanguageContext.DomainManager.Platform.GetFullPath(dir);
            } catch (ArgumentException) {
                // .NET validates the path, CPython doesn't... so we replace invalid chars with 
                // Char.Maxvalue, get the full path, and then replace the Char.Maxvalue's back w/ 
                // their original value.
                string newdir = dir;
                
                foreach (char c in Path.GetInvalidPathChars()) {
                    newdir = newdir.Replace(c, Char.MaxValue);
                }

                // walk backwards through the path replacing the same characters.  We should have
                // only updated the directory leaving the filename which we're fixing.
                string res = context.LanguageContext.DomainManager.Platform.GetFullPath(newdir);
                int curDir = dir.Length;
                for (int curRes = res.Length - 1; curRes >= 0; curRes--) {
                    if (res[curRes] == Char.MaxValue) {
                        for (curDir--; curDir >= 0; curDir--) {
                            if (newdir[curDir] == Char.MaxValue) {
                                res = res.Substring(0, curRes) + dir[curDir] + res.Substring(curRes + 1);
                                break;
                            }
                        }
                    }
                }

                return res;
            }
        }

#if !SILVERLIGHT
        public static int getpid() {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
#endif

        public static List listdir(CodeContext/*!*/ context, [NotNull]string path) {
            if (path == String.Empty) {
                path = ".";
            }

            List ret = PythonOps.MakeList();
            try {
                string[] files = context.LanguageContext.DomainManager.Platform.GetFiles(path, "*");
                addBase(files, ret);
                addBase(context.LanguageContext.DomainManager.Platform.GetDirectories(path, "*"), ret);
                return ret;
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        // 
        // lstat(path) -> stat result
        // Like stat(path), but do not follow symbolic links.
        // 
        public static object lstat(string path) {
            return stat(path);
        }

#if !SILVERLIGHT
        public static void mkdir(string path) {
            if (Directory.Exists(path))
                throw DirectoryExists();

            try {
                Directory.CreateDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static void mkdir(string path, int mode) {
            if (Directory.Exists(path)) throw DirectoryExists();
            // we ignore mode

            try {
                Directory.CreateDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }
#endif

        public static object open(CodeContext/*!*/ context, string filename, int flag) {
            return open(context, filename, flag, 0777);
        }

        public static object open(CodeContext/*!*/ context, string filename, int flag, int mode) {
            try {
                FileStream fs = File.Open(filename, FileModeFromFlags(flag), FileAccessFromFlags(flag), FileShare.ReadWrite);

                string mode2;
                if (fs.CanRead && fs.CanWrite) mode2 = "w+";
                else if (fs.CanWrite) mode2 = "w";
                else mode2 = "r";

                if ((flag & O_BINARY) != 0) {
                    mode2 += "b";
                }

                return PythonContext.GetContext(context).FileManager.AddToStrongMapping(PythonFile.Create(context, fs, filename, mode2));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

#if !SILVERLIGHT
        public static PythonFile popen(CodeContext/*!*/ context, string command) {
            return popen(context, command, "r");
        }

        public static PythonFile popen(CodeContext/*!*/ context, string command, string mode) {
            return popen(context, command, mode, 4096);
        }

        public static PythonFile popen(CodeContext/*!*/ context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "r";
            ProcessStartInfo psi = GetProcessInfo(command);
            psi.CreateNoWindow = true;  // ipyw shouldn't create a new console window
            Process p;
            PythonFile res;

            try {
                switch (mode) {
                    case "r":
                        psi.RedirectStandardOutput = true;
                        p = Process.Start(psi);

                        res = new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r");
                        break;
                    case "w":
                        psi.RedirectStandardInput = true;
                        p = Process.Start(psi);

                        res = new POpenFile(context, command, p, p.StandardInput.BaseStream, "w");
                        break;
                    default:
                        throw PythonOps.ValueError("expected 'r' or 'w' for mode, got {0}", mode);
                }
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            return res;
        }

        public static PythonTuple popen2(CodeContext/*!*/ context, string command) {
            return popen2(context, command, "t");
        }

        public static PythonTuple popen2(CodeContext/*!*/ context, string command, string mode) {
            return popen2(context, command, "t", 4096);
        }

        public static PythonTuple popen2(CodeContext/*!*/ context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            try {
                ProcessStartInfo psi = GetProcessInfo(command);
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true; // ipyw shouldn't create a new console window
                Process p = Process.Start(psi);

                return PythonTuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                        new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static PythonTuple popen3(CodeContext/*!*/ context, string command) {
            return popen3(context, command, "t");
        }

        public static PythonTuple popen3(CodeContext/*!*/ context, string command, string mode) {
            return popen3(context, command, "t", 4096);
        }

        public static PythonTuple popen3(CodeContext/*!*/ context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            try {
                ProcessStartInfo psi = GetProcessInfo(command);
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true; // ipyw shouldn't create a new console window
                Process p = Process.Start(psi);

                return PythonTuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                        new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode),
                        new POpenFile(context, command, p, p.StandardError.BaseStream, "r+" + mode));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static void putenv(string varname, string value) {
            try {
                System.Environment.SetEnvironmentVariable(varname, value);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }
#endif

        public static string read(CodeContext/*!*/ context, int fd, int buffersize) {
            if (buffersize < 0) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, PythonErrorNumber.EINVAL, "Invalid argument");
            }

            try {
                PythonContext pythonContext = PythonContext.GetContext(context);
                PythonFile pf = pythonContext.FileManager.GetFileFromId(pythonContext, fd);
                return pf.read();
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static void remove(string path) {
            UnlinkWorker(path);
        }

        public static void rename(string src, string dst) {
            try {
                Directory.Move(src, dst);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static void rmdir(string path) {
            try {
                Directory.Delete(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

#if !SILVERLIGHT
        public static object spawnl(CodeContext/*!*/ context, int mode, string path, params object[] args) {
            return SpawnProcessImpl(context, MakeProcess(), mode, path, args);
        }

        public static object spawnle(CodeContext/*!*/ context, int mode, string path, params object[] args) {
            if (args.Length < 1) {
                throw PythonOps.TypeError("spawnle() takes at least three arguments ({0} given)", 2 + args.Length);
            }

            object env = args[args.Length - 1];
            object[] slicedArgs = ArrayUtils.RemoveFirst(args);

            Process process = MakeProcess();
            SetEnvironment(process.StartInfo.EnvironmentVariables, env);

            return SpawnProcessImpl(context, process, mode, path, slicedArgs);
        }

        public static object spawnv(CodeContext/*!*/ context, int mode, string path, object args) {
            return SpawnProcessImpl(context, MakeProcess(), mode, path, args);
        }

        public static object spawnve(CodeContext/*!*/ context, int mode, string path, object args, object env) {
            Process process = MakeProcess();
            SetEnvironment(process.StartInfo.EnvironmentVariables, env);

            return SpawnProcessImpl(context, process, mode, path, args);
        }

        private static Process MakeProcess() {
            try {
                return new Process();
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        private static object SpawnProcessImpl(CodeContext/*!*/ context, Process process, int mode, string path, object args) {
            try {
                process.StartInfo.Arguments = ArgumentsToString(context, args);
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = false;
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            if (!process.Start()) {
                throw PythonOps.OSError("Cannot start process: {0}", path);
            }
            if (mode == (int)P_WAIT) {
                process.WaitForExit();
                int exitCode = process.ExitCode;
                process.Close();
                return exitCode;
            } else {
                return process.Id;
            }
        }

        /// <summary>
        /// Copy elements from a Python mapping of dict environment variables to a StringDictionary.
        /// </summary>
        private static void SetEnvironment(System.Collections.Specialized.StringDictionary currentEnvironment, object newEnvironment) {
            PythonDictionary env = newEnvironment as PythonDictionary;
            if (env == null) {
                throw PythonOps.TypeError("env argument must be a dict");
            }

            currentEnvironment.Clear();

            string strKey, strValue;
            foreach (object key in env.keys()) {
                if (!Converter.TryConvertToString(key, out strKey)) {
                    throw PythonOps.TypeError("env dict contains a non-string key");
                }
                if (!Converter.TryConvertToString(env[key], out strValue)) {
                    throw PythonOps.TypeError("env dict contains a non-string value");
                }
                currentEnvironment[strKey] = strValue;
            }
        }
#endif

        /// <summary>
        /// Convert a sequence of args to a string suitable for using to spawn a process.
        /// </summary>
        private static string ArgumentsToString(CodeContext/*!*/ context, object args) {
            IEnumerator argsEnumerator;
            System.Text.StringBuilder sb = null;
            if (!PythonOps.TryGetEnumerator(context, args, out argsEnumerator)) {
                throw PythonOps.TypeError("args parameter must be sequence, not {0}", DynamicHelpers.GetPythonType(args));
            }

            bool space = false;
            try {
                // skip the first element, which is the name of the command being run
                argsEnumerator.MoveNext();
                while (argsEnumerator.MoveNext()) {
                    if (sb == null) sb = new System.Text.StringBuilder(); // lazy creation
                    string strarg = PythonOps.ToString(argsEnumerator.Current);
                    if (space) {
                        sb.Append(' ');
                    }
                    if (strarg.IndexOf(' ') != -1) {
                        sb.Append('"');
                        // double quote any existing quotes
                        sb.Append(strarg.Replace("\"", "\"\""));
                        sb.Append('"');
                    } else {
                        sb.Append(strarg);
                    }
                    space = true;
                }
            } finally {
                IDisposable disposable = argsEnumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            if (sb == null) return "";
            return sb.ToString();
        }

#if !SILVERLIGHT
        public static void startfile(string filename, [DefaultParameterValue("open")]string operation) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = operation;
            try {

                process.Start();
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }
#endif

        [PythonType]
        public class stat_result : ISequence {
            private readonly object _mode, _size, _atime, _mtime, _ctime, _st_atime, _st_mtime, _st_ctime, _ino, _dev, _nlink, _uid, _gid;

            public const int n_fields = 13;
            public const int n_sequence_fields = 10;
            public const int n_unnamed_fields = 3;

            internal stat_result(int mode) : this(mode, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero) {
                _mode = mode;
            }

            internal stat_result(int mode, BigInteger size, BigInteger st_atime, BigInteger st_mtime, BigInteger st_ctime) {
                _mode = mode;
                _size = size;
                _st_atime = _atime = TryShrinkToInt(st_atime);
                _st_mtime = _mtime = TryShrinkToInt(st_mtime);
                _st_ctime = _ctime = TryShrinkToInt(st_ctime);

                _ino = _dev = _nlink = _uid = _gid = ScriptingRuntimeHelpers.Int32ToObject(0);                
            }

            public stat_result(CodeContext/*!*/ context, ISequence statResult, [DefaultParameterValue(null)]PythonDictionary dict) {
                // dict is allowed by CPython's stat_result, but doesn't seem to do anything, so we ignore it here.

                if (statResult.__len__() < 10) {
                    throw PythonOps.TypeError("stat_result() takes an at least 10-sequence ({0}-sequence given)", statResult.__len__());
                }

                _mode = statResult[0];
                _ino = statResult[1];
                _dev = statResult[2];
                _nlink = statResult[3];
                _uid = statResult[4];
                _gid = statResult[5];
                _size = statResult[6];
                _atime = statResult[7];
                _mtime = statResult[8];
                _ctime = statResult[9];

                object dictTime;
                if (statResult.__len__() >= 11) {
                    _st_atime = TryShrinkToInt(statResult[10]);
                } else if (TryGetDictValue(dict, "st_atime", out dictTime)) {
                    _st_atime = dictTime;
                } else {
                    _st_atime = TryShrinkToInt(_atime);
                }

                if (statResult.__len__() >= 12) {
                    _st_mtime = TryShrinkToInt(statResult[11]);
                } else if (TryGetDictValue(dict, "st_mtime", out dictTime)) {
                    _st_mtime = dictTime;
                } else { 
                    _st_mtime = TryShrinkToInt(_mtime);
                }

                if (statResult.__len__() >= 13) {
                    _st_ctime = TryShrinkToInt(statResult[12]);
                } else if (TryGetDictValue(dict, "st_ctime", out dictTime)) {
                    _st_ctime = dictTime;
                } else { 
                    _st_ctime = TryShrinkToInt(_ctime);
                }
            }

            private static bool TryGetDictValue(PythonDictionary dict, string name, out object dictTime) {
                if (dict != null && dict.TryGetValue(name, out dictTime)) {
                    dictTime = TryShrinkToInt(dictTime);
                    return true;
                }

                dictTime = null;
                return false;
            }

            private static object TryShrinkToInt(object value) {
                BigInteger bi = value as BigInteger;

                if (Object.ReferenceEquals(bi, null)) {
                    return value;
                }
                
                return BigIntegerOps.__int__(bi);
            }

            public object st_atime {
                get {
                    return _st_atime;
                }
            }

            public object st_ctime {
                get {
                    return _st_ctime;
                }
            }


            public object st_mtime {
                get {
                    return _st_mtime;
                }
            }

            public object st_dev {
                get {
                    return TryShrinkToInt(_dev);
                }
            }

            public object st_gid {
                get {
                    return _gid;
                }
            }

            public object st_ino {
                get {
                    return _ino;
                }
            }

            public object st_mode {
                get {
                    return TryShrinkToInt(_mode);
                }
            }

            public object st_nlink {
                get {
                    return TryShrinkToInt(_nlink);
                }
            }

            public object st_size {
                get {
                    return _size;
                }
            }

            public object st_uid {
                get {
                    return _uid;
                }
            }

            public override string ToString() {
                return MakeTuple().ToString();
            }

            public string/*!*/ __repr__() {
                return ToString();
            }

            public PythonTuple __reduce__() {
                PythonDictionary timeDict = new PythonDictionary(3);
                timeDict["st_atime"] = st_atime;
                timeDict["st_ctime"] = st_ctime;
                timeDict["st_mtime"] = st_mtime;

                return PythonTuple.MakeTuple(
                    DynamicHelpers.GetPythonTypeFromType(typeof(stat_result)),
                    PythonTuple.MakeTuple(MakeTuple(), timeDict)
                );
            }

            #region ISequence Members

            //public object AddSequence(object other) {
            //    return MakeTuple().AddSequence(other);
            //}

            //public object MultiplySequence(object count) {
            //    return MakeTuple().MultiplySequence(count);
            //}

            public object this[int index] {
                get {
                    return MakeTuple()[index];
                }
            }

            public object this[Slice slice] {
                get {
                    return MakeTuple()[slice];
                }
            }

            public object __getslice__(int start, int stop) {
                return MakeTuple().__getslice__(start, stop);
            }

            public int __len__() {
                return MakeTuple().__len__();
            }

            public bool __contains__(object item) {
                return ((ICollection<object>)MakeTuple()).Contains(item);
            }

            #endregion

            private PythonTuple MakeTuple() {
                return PythonTuple.MakeTuple(
                    st_mode,
                    st_ino,
                    st_dev,
                    st_nlink,
                    st_uid,
                    st_gid,
                    st_size,
                    _atime,
                    _mtime,
                    _ctime
                );
            }

            #region Object overrides

            public override bool Equals(object obj) {
                if (obj is stat_result) {
                    return MakeTuple().Equals(((stat_result)obj).MakeTuple());
                } else {
                    return MakeTuple().Equals(obj);
                }

            }

            public override int GetHashCode() {
                return MakeTuple().GetHashCode();
            }

            #endregion
        }

        private static bool HasExecutableExtension(string path) {
            string extension = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            return (extension == ".exe" || extension == ".dll" || extension == ".com" || extension == ".bat");
        }

        [Documentation("stat(path) -> stat result\nGathers statistics about the specified file or directory")]
        public static object stat(string path) {
            if (path == null) throw PythonOps.TypeError("expected string, got NoneType");

            stat_result sr;

            try {
                FileInfo fi = new FileInfo(path);
                int mode = 0;
                long size;

                if (Directory.Exists(path)) {
                    size = 0;
                    mode = 0x4000 | S_IEXEC;
                } else if (File.Exists(path)) {
                    size = fi.Length;
                    mode = 0x8000;
                    if (HasExecutableExtension(path)) {
                        mode |= S_IEXEC;
                    }
                } else {
                    throw PythonExceptions.CreateThrowable(WindowsError, PythonErrorNumber.ENOENT, "file does not exist: " + path);
                }

                long st_atime = (long)PythonTime.TicksToTimestamp(fi.LastAccessTime.ToUniversalTime().Ticks);
                long st_ctime = (long)PythonTime.TicksToTimestamp(fi.CreationTime.ToUniversalTime().Ticks);
                long st_mtime = (long)PythonTime.TicksToTimestamp(fi.LastWriteTime.ToUniversalTime().Ticks);
                mode |= S_IREAD;
                if ((fi.Attributes & FileAttributes.ReadOnly) == 0) {
                    mode |= S_IWRITE;
                }

                sr = new stat_result(mode, size, st_atime, st_mtime, st_ctime);
            } catch (ArgumentException) {
                throw PythonExceptions.CreateThrowable(WindowsError, PythonErrorNumber.EINVAL, "The path is invalid: " + path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            return sr;
        }

        private static PythonType WindowsError {
            get {
#if !SILVERLIGHT
                return PythonExceptions.WindowsError;
#else
                return PythonExceptions.OSError;
#endif
            }
        }

#if !SILVERLIGHT
        [Documentation("system(command) -> int\nExecute the command (a string) in a subshell.")]
        public static int system(string command) {
            ProcessStartInfo psi = GetProcessInfo(command);
            psi.CreateNoWindow = false;

            Process process = Process.Start(psi);
            if (process == null) {
                return -1;
            }
            process.WaitForExit();
            return process.ExitCode;
        }

        public static string tempnam(CodeContext/*!*/ context) {
            return tempnam(context, null);
        }

        public static string tempnam(CodeContext/*!*/ context, string dir) {
            return tempnam(context, null, null);
        }

        public static string tempnam(CodeContext/*!*/ context, string dir, string prefix) {
            PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "tempnam is a potential security risk to your program");

            try {
                dir = Path.GetTempPath(); // Reasonably consistent with CPython behavior under Windows

                return Path.GetFullPath(Path.Combine(dir, prefix ?? String.Empty) + Path.GetRandomFileName());
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static object times() {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();

            return PythonTuple.MakeTuple(p.UserProcessorTime.TotalSeconds,
                p.PrivilegedProcessorTime.TotalSeconds,
                0,  // child process system time
                0,  // child process os time
                DateTime.Now.Subtract(p.StartTime).TotalSeconds);
        }
#endif

#if !SILVERLIGHT
        public static PythonFile/*!*/ tmpfile(CodeContext/*!*/ context) {
            try {
                FileStream sw = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

                PythonFile res = PythonFile.Create(context, sw, sw.Name, "w+b");
                return res;
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static string/*!*/ tmpnam(CodeContext/*!*/ context) {
            PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "tmpnam is a potential security risk to your program");
            return Path.GetFullPath(Path.GetTempPath() + Path.GetRandomFileName());
        }
#endif

        public static void unlink(string path) {
            UnlinkWorker(path);
        }

        private static void UnlinkWorker(string path) {
            if (path == null) throw new ArgumentNullException("path");

            if (!File.Exists(path)) {
                throw PythonExceptions.CreateThrowable(WindowsError, PythonErrorNumber.ENOENT, "The file could not be found for deletion: " + path);
            }

            try {
                File.Delete(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

#if !SILVERLIGHT
        public static void unsetenv(string varname) {
            System.Environment.SetEnvironmentVariable(varname, null);
        }
#endif

        public static object urandom(int n) {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[n];
            rng.GetBytes(data);

            return PythonBinaryReader.PackDataIntoString(data, n);
        }

        private static readonly object _umaskKey = new object();

        public static int umask(CodeContext/*!*/ context, int mask) {
            mask &= 0x180;
            object oldMask = PythonContext.GetContext(context).GetSetModuleState(_umaskKey, mask);
            if (oldMask == null) {
                return 0;
            } else {
                return (int)oldMask;
            }
        }

#if !SILVERLIGHT
        public static void utime(string path, PythonTuple times) {
            try {
                FileInfo fi = new FileInfo(path);
                if (times == null) {
                    fi.LastAccessTime = DateTime.Now;
                    fi.LastWriteTime = DateTime.Now;
                } else if (times.__len__() == 2) {
                    DateTime atime = new DateTime(PythonTime.TimestampToTicks(Converter.ConvertToDouble(times[0])), DateTimeKind.Utc);
                    DateTime mtime = new DateTime(PythonTime.TimestampToTicks(Converter.ConvertToDouble(times[1])), DateTimeKind.Utc);

                    fi.LastAccessTime = atime;
                    fi.LastWriteTime = mtime;
                } else {
                    throw PythonOps.TypeError("times value must be a 2-value tuple (atime, mtime)");
                }
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static PythonTuple waitpid(int pid, object options) {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
            if (process == null) {
                throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, PythonErrorNumber.ECHILD, "Cannot find process " + pid);
            }
            process.WaitForExit();
            return PythonTuple.MakeTuple(pid, process.ExitCode);
        }
#endif

        public static void write(CodeContext/*!*/ context, int fd, string text) {
            try {
                PythonContext pythonContext = PythonContext.GetContext(context);
                PythonFile pf = pythonContext.FileManager.GetFileFromId(pythonContext, fd);
                pf.write(text);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public const int O_APPEND = 0x8;
        public const int O_CREAT = 0x100;
        public const int O_TRUNC = 0x200;

        public const int O_EXCL = 0x400;
        public const int O_NOINHERIT = 0x80;

        public const int O_RANDOM = 0x10;
        public const int O_SEQUENTIAL = 0x20;

        public const int O_SHORT_LIVED = 0x1000;
        public const int O_TEMPORARY = 0x40;

        public const int O_WRONLY = 0x1;
        public const int O_RDONLY = 0x0;
        public const int O_RDWR = 0x2;

        public const int O_BINARY = 0x8000;
        public const int O_TEXT = 0x4000;

        public const int P_WAIT = 0;
        public const int P_NOWAIT = 1;
        public const int P_NOWAITO = 3;

        // Not implemented:
        // public static object P_OVERLAY = 2;
        // public static object P_DETACH = 4;

        #endregion

        #region Private implementation details

        private static Exception ToPythonException(Exception e) {
            if (e is ArgumentException || e is ArgumentNullException || e is ArgumentTypeException) {
                // rethrow reasonable exceptions
                return ExceptionHelpers.UpdateForRethrow(e);
            }

            string message = e.Message;
            int errorCode;

#if !SILVERLIGHT
            bool isWindowsError = false;
            Win32Exception winExcep = e as Win32Exception;
            if (winExcep != null) {
                errorCode = ToPythonErrorCode(winExcep.NativeErrorCode);
                message = GetFormattedException(e, errorCode);
                isWindowsError = true;
            } else {
#endif
                errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(e);
                if ((errorCode & ~0xfff) == (unchecked((int)0x80070000))) {
                    // Win32 HR, translate HR to Python error code if possible, otherwise
                    // report the HR.
                    errorCode = ToPythonErrorCode(errorCode & 0xfff);
                    message = GetFormattedException(e, errorCode);
#if !SILVERLIGHT
                    isWindowsError = true;
#endif
                }
#if !SILVERLIGHT
            }

            if (isWindowsError) {
                return PythonExceptions.CreateThrowable(WindowsError, errorCode, message);
            }
#endif

            return PythonExceptions.CreateThrowable(PythonExceptions.OSError, errorCode, message);
        }

        private static string GetFormattedException(Exception e, int hr) {
            return "[Errno " + hr.ToString() + "] " + e.Message;
        }

        private static int ToPythonErrorCode(int win32ErrorCode) {
            switch (win32ErrorCode) {
                case ERROR_FILE_EXISTS:       return PythonErrorNumber.EEXIST; 
                case ERROR_ACCESS_DENIED:     return PythonErrorNumber.EACCES; 
                case ERROR_DLL_NOT_FOUND:
                case ERROR_FILE_NOT_FOUND:
                case ERROR_PATH_NOT_FOUND:    return PythonErrorNumber.ENOENT; 
                case ERROR_CANCELLED:         return PythonErrorNumber.EINTR; 
                case ERROR_NOT_ENOUGH_MEMORY: return PythonErrorNumber.ENOMEM; 
                case ERROR_SHARING_VIOLATION: return PythonErrorNumber.EBUSY;  
                case ERROR_NO_ASSOCIATION:    return PythonErrorNumber.EINVAL; 
            }
            return win32ErrorCode;
        }

        // Win32 error codes
        private const int ERROR_FILE_EXISTS = 80;
        private const int ERROR_ACCESS_DENIED = 5; // Access to the specified file is denied. 
        private const int ERROR_FILE_NOT_FOUND = 2; //The specified file was not found. 
        private const int ERROR_PATH_NOT_FOUND = 3; // The specified path was not found. 
        private const int ERROR_NO_ASSOCIATION = 1155; //There is no application associated with the given file name extension. 
        private const int ERROR_DLL_NOT_FOUND = 1157; // One of the library files necessary to run the application can't be found. 
        private const int ERROR_CANCELLED = 1223; // The function prompted the user for additional information, but the user canceled the request. 
        private const int ERROR_NOT_ENOUGH_MEMORY = 8; // There is not enough memory to perform the specified action. 
        private const int ERROR_SHARING_VIOLATION = 32; //A sharing violation occurred. 
        private const int ERROR_ALREADY_EXISTS = 183;

        private const int S_IWRITE = 0x80 + 0x10 + 0x02; // owner / group / world
        private const int S_IREAD = 0x100 + 0x20 + 0x04; // owner / group / world
        private const int S_IEXEC = 0x40 + 0x08 + 0x01; // owner / group / world

        public const int F_OK = 0;
        public const int X_OK = 1;
        public const int W_OK = 2;
        public const int R_OK = 4;

        private static void addBase(string[] files, List ret) {
            foreach (string file in files) {
                ret.AddNoLock(Path.GetFileName(file));
            }
        }

        private static FileMode FileModeFromFlags(int flags) {
            if ((flags & O_APPEND) != 0) return FileMode.Append;
            if ((flags & O_CREAT) != 0) return FileMode.CreateNew;
            if ((flags & O_TRUNC) != 0) return FileMode.Truncate;
            return FileMode.Open;
        }

        private static FileAccess FileAccessFromFlags(int flags) {
            if ((flags & O_RDWR) != 0) return FileAccess.ReadWrite;
            if ((flags & O_WRONLY) != 0) return FileAccess.Write;

            return FileAccess.Read;
        }
#if !SILVERLIGHT // Processes
        [PythonType]
        private class POpenFile : PythonFile {
            private Process _process;

            public static object __new__(CodeContext/*!*/ context, string command, Process process, Stream stream, string mode) {
                return new POpenFile(context, command, process, stream, mode);
            }

            internal POpenFile(CodeContext/*!*/ context, string command, Process process, Stream stream, string mode) 
                : base(PythonContext.GetContext(context)) {
                __init__(stream, PythonContext.GetContext(context).DefaultEncoding, command, mode);
                this._process = process;
            }

            public override object close() {
                base.close();

                if (_process.HasExited && _process.ExitCode != 0) {
                    return _process.ExitCode;
                }

                return null;
            }
        }

        private static ProcessStartInfo GetProcessInfo(string command) {
            // TODO: always run through cmd.exe ?
            command = command.Trim();
            string baseCommand, args;
            if (!TryGetExecutableCommand(command, out baseCommand, out args)) {
                if (!TryGetShellCommand(command, out baseCommand, out args)) {
                    throw PythonOps.WindowsError("The system can not find command '{0}'", command);
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo(baseCommand, args);
            psi.UseShellExecute = false;

            return psi;
        }

        private static bool TryGetExecutableCommand(string command, out string baseCommand, out string args) {
            baseCommand = command;
            args = String.Empty;
            int pos;

            if (command[0] == '\"') {
                for (pos = 1; pos < command.Length; pos++) {
                    if (command[pos] == '\"') {
                        baseCommand = command.Substring(1, pos - 1).Trim();
                        if (pos + 1 < command.Length) {
                            args = command.Substring(pos + 1);
                        }
                        break;
                    }
                }
                if (pos == command.Length)
                    throw PythonOps.ValueError("mismatch quote in command");
            } else {
                pos = command.IndexOf(' ');
                if (pos != -1) {
                    baseCommand = command.Substring(0, pos);
                    // pos won't be the last one
                    args = command.Substring(pos + 1);
                }
            }
            string fullpath = Path.GetFullPath(baseCommand);
            if (File.Exists(fullpath)) {
                baseCommand = fullpath;
                return true;
            }

            // TODO: need revisit
            string sysdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System);
            foreach (string suffix in new string[] { string.Empty, ".com", ".exe", "cmd", ".bat" }) {
                fullpath = Path.Combine(sysdir, baseCommand + suffix);
                if (File.Exists(fullpath)) {
                    baseCommand = fullpath;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetShellCommand(string command, out string baseCommand, out string args) {
            baseCommand = Environment.GetEnvironmentVariable("COMSPEC");
            args = String.Empty;
            if (baseCommand == null) {
                baseCommand = Environment.GetEnvironmentVariable("SHELL");
                if (baseCommand == null) {
                    return false;
                }
                args = String.Format("-c \"{0}\"", command);
            } else {
                args = String.Format("/c {0}", command);
            }
            return true;
        }

        private static Exception DirectoryExists() {
            PythonExceptions._WindowsError err = new PythonExceptions._WindowsError();
            err.__init__(ERROR_ALREADY_EXISTS, "directory already exists");
            err.errno = PythonErrorNumber.EEXIST;

            return PythonExceptions.ToClr(err);
        }
#endif

        #endregion
    }
}
