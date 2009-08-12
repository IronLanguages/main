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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Text;
using Microsoft.Scripting.Actions;
using IronRuby.Runtime;

namespace IronRuby.Builtins {

    [RubyClass("Dir", Inherits = typeof(object)), Includes(typeof(Enumerable))]
    public class RubyDir {
        #region Private fields

        private readonly MutableString/*!*/ _dirName;
        private readonly string[]/*!*/ _rawEntries;
        private bool _closed;

        // _pos starts from -2 as ".", -1 as "..", 
        // 0 will be the first item from Directory.GetFileSystemEntries.
        private int _pos;

        #endregion

        public RubyDir([NotNull]MutableString/*!*/ dirname) {
            string strName = dirname.ConvertToString();
            try {
                _rawEntries = Directory.GetFileSystemEntries(strName);
            } catch (Exception ex) {
                throw ToRubyException(ex, strName, DirectoryOperation.Open);
            }
            _dirName = MutableString.Create(RubyUtils.CanonicalizePath(strName), RubyEncoding.Path);
            _closed = false;
            _pos = -2;
        }

        #region Singleton Methods

        /// <summary>
        /// raise a SystemCallError if the target directory does not exist
        /// </summary>
        /// <returns>0 if no block is given; otherwise, the value of the block</returns>
        [RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
        public static object ChangeDirectory(BlockParam block, object/*!*/ self, MutableString dir) {
            string strDir = dir.ConvertToString();

            if (block == null) {
                SetCurrentDirectory(strDir);
                return 0;
            } else {
                string current = Directory.GetCurrentDirectory();
                try {
                    SetCurrentDirectory(strDir);
                    object result;
                    block.Yield(dir, out result);
                    return result;
                } finally {
                    SetCurrentDirectory(current);
                }
            }
        }

        private static void SetCurrentDirectory(string/*!*/ dir) {
            try {
                Directory.SetCurrentDirectory(dir);
            } catch (Exception e) {
                throw ToRubyException(e, dir, DirectoryOperation.ChangeDir);
            }
        }

        [RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
        public static object ChangeDirectory(object self, MutableString/*!*/ dir) {
            return ChangeDirectory(null, self, dir);
        }

        /// <summary>
        /// change the directory to the value of the environment variable HOME or LOGDIR
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        [RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
        public static object ChangeDirectory(RubyContext/*!*/ context, object self) {
#if !SILVERLIGHT
            string defaultDirectory = RubyUtils.GetHomeDirectory(context.DomainManager.Platform);
            if (defaultDirectory == null)
                throw RubyExceptions.CreateArgumentError("HOME / USERPROFILE not set");

            return ChangeDirectory(self, MutableString.Create(defaultDirectory, RubyEncoding.Path));
#else
            throw new InvalidOperationException();
#endif
        }

        [RubyMethod("chroot", RubyMethodAttributes.PublicSingleton)]
        public static int ChangeRoot(object self) {
            throw new InvalidOperationException();
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("rmdir", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int RemoveDirectory(object self, [NotNull]MutableString/*!*/ dirname) {
            string strDir = dirname.ConvertToString();
            try {
                Directory.Delete(strDir);
            } catch (Exception ex) {
                throw ToRubyException(ex, strDir, DirectoryOperation.Delete);
            }
            return 0;
        }

        [RubyMethod("entries", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetEntries(object self, [NotNull]MutableString/*!*/ dirname) {
            string strDir = dirname.ConvertToString();
            string[] rawEntries = null;

            try {
                rawEntries = Directory.GetFileSystemEntries(strDir);
            } catch (Exception ex) {
                throw ToRubyException(ex, strDir, DirectoryOperation.Open);
            }

            RubyArray ret = new RubyArray(rawEntries.Length + 2);
            ret.Add(MutableString.CreateAscii("."));
            ret.Add(MutableString.CreateAscii(".."));
            foreach (string entry in rawEntries) {
                ret.Add(MutableString.Create(Path.GetFileName(entry), RubyEncoding.Path));
            }
            return ret;
        }

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static object ForEach(BlockParam block, object self, [NotNull]MutableString/*!*/ dirname) {
            // TODO: ??? block == nil
            foreach (object entry in GetEntries(self, dirname)) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                // TODO: ??? as
                object result;
                if (block.Yield(entry as MutableString, out result)) {
                    return result;
                }
            }

            return null;
        }

        [RubyMethod("getwd", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("pwd", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetCurrentDirectory(object self) {
            return MutableString.Create(RubyUtils.CanonicalizePath(Directory.GetCurrentDirectory()), RubyEncoding.Path);
        }

        #region glob

        [RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
        public static object Glob([NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [DefaultProtocol, Optional]int flags) {

            foreach (string fileName in IronRuby.Builtins.Glob.GlobResults(self.Context.DomainManager.Platform, pattern.ConvertToString(), flags)) {
                object result;
                if (block.Yield(MutableString.Create(fileName, pattern.Encoding).TaintBy(pattern), out result)) {
                    return result;
                }
            }
            return null;
        }

        [RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ Glob(RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [DefaultProtocol, Optional]int flags) {
            
            RubyArray result = new RubyArray();
            foreach (string fileName in IronRuby.Builtins.Glob.GlobResults(self.Context.DomainManager.Platform, pattern.ConvertToString(), flags)) {
                result.Add(MutableString.Create(fileName, pattern.Encoding).TaintBy(pattern));
            }

            return result;
        }

        #endregion

        [RubyMethod("mkdir", RubyMethodAttributes.PublicSingleton)]
        public static int MakeDirectory(object self, [NotNull]MutableString/*!*/ dirname, [Optional]object permissions) {
            string strDir = dirname.ConvertToString();
            try {
                Directory.CreateDirectory(strDir);
            } catch (Exception ex) {
                throw ToRubyException(ex, strDir, DirectoryOperation.Create);
            }
            return 0;
        }

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static object Open(BlockParam block, object self, [NotNull]MutableString/*!*/ dirname) {
            RubyDir rd = new RubyDir(dirname);

            try {
                object result;
                block.Yield(rd, out result);
                return result;
            } finally {
                Close(rd);
            }
        }

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static object Open(object self, [NotNull]MutableString/*!*/ dirname) {
            return new RubyDir(dirname);
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("close")]
        public static void Close(RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            self._closed = true;
        }

        [RubyMethod("each")]
        public static RubyDir/*!*/ Each(BlockParam block, RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            RubyDir.ForEach(block, self, self._dirName);
            return self;
        }

        [RubyMethod("path")]
        public static MutableString/*!*/ GetPath(RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            return self._dirName;
        }

        [RubyMethod("pos")]
        [RubyMethod("tell")]
        public static int GetCurrentPosition(RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            return self._pos + 2;
        }

        /// <summary>
        /// Synonym for Dir#seek, but returns the position parameter
        /// </summary>
        [RubyMethod("pos=")]
        public static int SetPosition(RubyDir/*!*/ self, int pos) {
            self.ThrowIfClosed();

            self._pos = pos - 2;
            return pos;
        }

        [RubyMethod("read")]
        public static MutableString/*!*/ Read(RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            if (self._pos + 1 > self._rawEntries.Length) {
                return null;
            }

            MutableString ret;
            if (self._pos == -2) {
                ret = MutableString.CreateAscii(".");
            } else if (self._pos == -1) {
                ret = MutableString.CreateAscii("..");
            } else {
                ret = MutableString.Create(Path.GetFileName(self._rawEntries[self._pos]), RubyEncoding.Path);
            }
            self._pos++;
            return ret;
        }

        [RubyMethod("rewind")]
        public static RubyDir/*!*/ Rewind(RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            self._pos = -2;
            return self;
        }

        [RubyMethod("seek")]
        public static RubyDir/*!*/ Seek(RubyDir/*!*/ self, int pos) {
            self.ThrowIfClosed();

            if (pos < 0) {
                self._pos = -2;
            } else if (pos > self._rawEntries.Length + 2) {
                self._pos = self._rawEntries.Length;
            } else {
                self._pos = pos - 2;
            }
            return self;
        }

        #endregion

        #region Helpers

        private void ThrowIfClosed() {
            if (_closed) {
                throw RubyExceptions.CreateIOError("closed directory");
            }
        }

        private enum DirectoryOperation {
            Delete,
            Create,
            Open,
            ChangeDir,
        }

        // TODO: to match the C-Ruby exception
        private static Exception ToRubyException(Exception/*!*/ ex, string path, DirectoryOperation op) {
            Assert.NotNull(ex);
            
            Type exceptionType = ex.GetType();

            switch (op) {
                case DirectoryOperation.ChangeDir:
                    return RubyExceptions.CreateEINVAL(path);

                case DirectoryOperation.Open:
                    return RubyExceptions.CreateENOENT(path);

                case DirectoryOperation.Delete:
                    if (ex is ArgumentException) {
                        return RubyExceptions.CreateEINVAL(path);
                    }
                    if (ex is IOException) {
                        return Errno.CreateEACCES(path);
                    }
                    break;

                case DirectoryOperation.Create:
                    if (ex is ArgumentException) {
                        return RubyExceptions.CreateEINVAL(path);
                    }
                    if (ex is IOException) {
                        return RubyExceptions.CreateEEXIST(path);
                    }
                    break;
            }

            // throw anyway
            return RubyExceptions.CreateSystemCallError(String.Format("unknown scenario - {0}, {1}, {2}", exceptionType, path, op));
        }

        #endregion
    }
}
