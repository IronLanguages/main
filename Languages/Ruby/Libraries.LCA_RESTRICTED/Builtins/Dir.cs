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

using System;
using System.IO;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System.Collections.Generic;

namespace IronRuby.Builtins {
    [RubyClass("Dir", Inherits = typeof(object)), Includes(typeof(Enumerable))]
    public class RubyDir : RubyObject {
        // null if closed
        private MutableString _dirName;
        private string[] _rawEntries;

        // _pos starts from -2 as ".", -1 as "..", 
        // 0 will be the first item from Directory.GetFileSystemEntries.
        private int _pos;

        #region Construction

        public RubyDir(RubyClass/*!*/ cls) 
            : base(cls) {
        }

        public RubyDir(RubyClass/*!*/ cls, MutableString/*!*/ dirname) 
            : base(cls) {
            Reinitialize(this, dirname);
        }

        [RubyConstructor]
        public static RubyDir/*!*/ Create(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ dirname) {
            return new RubyDir(self, dirname);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyDir/*!*/ Reinitialize(RubyDir/*!*/ self, [NotNull]MutableString/*!*/ dirname) {
            self.Close();

            string strName = self.ImmediateClass.Context.DecodePath(dirname);
            try {
                self._rawEntries = self.Platform.GetFileSystemEntries(strName, "*");
            } catch (Exception ex) {
                throw ToRubyException(ex, strName, DirectoryOperation.Open);
            }
            self._dirName = dirname.Clone();
            self._pos = -2;
            return self;
        }

        #endregion

        #region Singleton Methods

        private static void SetCurrentDirectory(PlatformAdaptationLayer/*!*/ pal, string/*!*/ dir) {
            try {
                // TODO: MRI calls Win32 API SetCurrentDirctory directly, while BCL normalizes the path first.
                pal.CurrentDirectory = dir;
            } catch (Exception e) {
                throw ToRubyException(e, dir, DirectoryOperation.ChangeDir);
            }
        }

        /// <summary>
        /// raise a SystemCallError if the target directory does not exist
        /// </summary>
        /// <returns>0 if no block is given; otherwise, the value of the block</returns>
        [RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
        public static object ChangeDirectory(ConversionStorage<MutableString>/*!*/ toPath, BlockParam block, RubyClass/*!*/ self, object dir) {
            var d = Protocols.CastToPath(toPath, dir);
            return ChangeDirectory(self.Context.Platform, self.Context.DecodePath(d), d, block);
        }

        /// <summary>
        /// change the directory to the value of the environment variable HOME or LOGDIR
        /// </summary>
        [RubyMethod("chdir", RubyMethodAttributes.PublicSingleton)]
        public static object ChangeDirectory(BlockParam block, RubyClass/*!*/ self) {
#if !SILVERLIGHT
            string defaultDirectory = RubyUtils.GetHomeDirectory(self.Context.Platform);
            if (defaultDirectory == null) {
                throw RubyExceptions.CreateArgumentError("HOME / USERPROFILE not set");
            }

            return ChangeDirectory(self.Context.Platform, defaultDirectory, self.Context.EncodePath(defaultDirectory), block);
#else
            throw new InvalidOperationException();
#endif
        }

        private static object ChangeDirectory(PlatformAdaptationLayer/*!*/ pal, string/*!*/ strDir, MutableString/*!*/ dir, BlockParam block) {
            if (block == null) {
                SetCurrentDirectory(pal, strDir);
                return 0;
            }

            string current = pal.CurrentDirectory;
            try {
                SetCurrentDirectory(pal, strDir);
                object result;
                block.Yield(dir, out result);
                return result;
            } finally {
                SetCurrentDirectory(pal, current);
            }
        }

        [RubyMethod("chroot", RubyMethodAttributes.PublicSingleton)]
        public static int ChangeRoot(object self) {
            throw new InvalidOperationException();
        }

        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        public static bool Exists(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.DirectoryExists(self.Context, Protocols.CastToPath(toPath, path));
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("rmdir", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int RemoveDirectory(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object dirname) {
            string strDir = self.Context.DecodePath(Protocols.CastToPath(toPath, dirname));
            try {
                self.Context.Platform.DeleteDirectory(strDir, false);
            } catch (Exception ex) {
                throw ToRubyException(ex, strDir, DirectoryOperation.Delete);
            }
            return 0;
        }

        [RubyMethod("entries", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetEntries(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object dirname, [Optional]IDictionary<object, object> options) {
            // TODO: options[:encoding]
            return new RubyDir(self, Protocols.CastToPath(toPath, dirname)).GetEntries(self.Context);
        }

        [RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
        public static object ForEach(ConversionStorage<MutableString>/*!*/ toPath, BlockParam block, RubyClass/*!*/ self, object dirname) {
            return new RubyDir(self, Protocols.CastToPath(toPath, dirname)).EnumerateEntries(self.Context, block, null);
        }

        [RubyMethod("getwd", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("pwd", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetCurrentDirectory(RubyClass/*!*/ self) {
            return self.Context.EncodePath(RubyUtils.CanonicalizePath(self.Context.Platform.CurrentDirectory));
        }

        #region glob

        [RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
        public static object Glob([NotNull]BlockParam/*!*/ block, RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [DefaultProtocol, Optional]int flags) {

            foreach (var fileName in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, flags)) {
                object result;
                if (block.Yield(fileName, out result)) {
                    return result;
                }
            }
            return null;
        }

        [RubyMethod("glob", RubyMethodAttributes.PublicSingleton)]
        public static object Glob(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, [DefaultProtocol, Optional]int flags) {

            RubyArray result = new RubyArray();
            foreach (var fileName in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, flags)) {
                result.Add(fileName);
            }

            return result;
        }

        [RubyMethod("[]", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ Glob(RubyClass/*!*/ self, [DefaultProtocol, NotNullItems]params MutableString/*!*/[]/*!*/ patterns) {
            RubyArray result = new RubyArray();
            foreach (var pattern in patterns) {
                foreach (var fileName in IronRuby.Builtins.Glob.GetMatches(self.Context, pattern, 0)) {
                    result.Add(fileName);
                }
            }

            return result;
        }

        #endregion

        [RubyMethod("mkdir", RubyMethodAttributes.PublicSingleton)]
        public static int MakeDirectory(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object dirname, [Optional]object permissions) {
            var platform = self.Context.Platform;

            string strDir = self.Context.DecodePath(Protocols.CastToPath(toPath, dirname));
            if (platform.FileExists(strDir) || platform.DirectoryExists(strDir)) {
                throw RubyExceptions.CreateEEXIST(strDir);
            }

            string containingDir = platform.GetDirectoryName(strDir);
            if (!String.IsNullOrEmpty(containingDir) && !platform.DirectoryExists(containingDir)) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", containingDir);
            }
                
            try {
                platform.CreateDirectory(strDir);
            } catch (Exception ex) {
                throw ToRubyException(ex, strDir, DirectoryOperation.Create);
            }
            return 0;
        }

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static object Open(ConversionStorage<MutableString>/*!*/ toPath, BlockParam block, RubyClass/*!*/ self, object dirname) {
            RubyDir rd = new RubyDir(self, Protocols.CastToPath(toPath, dirname));

            try {
                object result;
                block.Yield(rd, out result);
                return result;
            } finally {
                Close(rd);
            }
        }

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RubyDir/*!*/ Open(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object dirname) {
            return new RubyDir(self, Protocols.CastToPath(toPath, dirname));
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("close")]
        public static void Close(RubyDir/*!*/ self) {
            self.ThrowIfClosed();
            self.Close();
        }

        [RubyMethod("each")]
        public static object Each(RubyContext/*!*/ context, BlockParam block, RubyDir/*!*/ self) {
            return self.EnumerateEntries(context, block, self);
        }

        [RubyMethod("to_path")]
        [RubyMethod("path")]
        public static MutableString GetPath(RubyContext/*!*/ context, RubyDir/*!*/ self) {
            if (context.RubyOptions.Compatibility < RubyCompatibility.Ruby19) {
                self.ThrowIfClosed();
            } else if (self.Closed) {
                return null;
            }

            return self._dirName.Clone();
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
        public static MutableString Read(RubyContext/*!*/ context, RubyDir/*!*/ self) {
            self.ThrowIfClosed();

            if (self._pos + 1 > self._rawEntries.Length) {
                return null;
            }

            MutableString ret;
            if (self._pos == -2) {
                ret = context.EncodePath(".");
            } else if (self._pos == -1) {
                ret = context.EncodePath("..");
            } else {
                ret = context.EncodePath(context.Platform.GetFileName(self._rawEntries[self._pos]));
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

        internal PlatformAdaptationLayer/*!*/ Platform {
            get { return ImmediateClass.Context.Platform; }
        }

        private bool Closed {
            get { return _dirName == null; } 
        }

        private void Close() {
            _dirName = null;
            _rawEntries = null;
        }

        private void ThrowIfClosed() {
            if (Closed) {
                throw RubyExceptions.CreateIOError("closed directory");
            }
        }

        private enum DirectoryOperation {
            Delete,
            Create,
            Open,
            ChangeDir,
        }

        private RubyArray/*!*/ GetEntries(RubyContext/*!*/ context) {
            ThrowIfClosed();

            RubyArray ret = new RubyArray(_rawEntries.Length + 2);
            ret.Add(context.EncodePath("."));
            ret.Add(context.EncodePath(".."));
            foreach (string entry in _rawEntries) {
                var encoded = context.TryEncodePath(context.Platform.GetFileName(entry));
                if (encoded != null) {
                    ret.Add(encoded);
                }
            }
            return ret;
        }

        private object EnumerateEntries(RubyContext/*!*/ context, BlockParam block, object defaultResult) {
            ThrowIfClosed();
            _pos = -2;

            foreach (object entry in GetEntries(context)) {
                if (block == null) {
                    throw RubyExceptions.NoBlockGiven();
                }

                _pos++;

                object blockResult;
                if (block.Yield(entry, out blockResult)) {
                    return blockResult;
                }
            }

            return defaultResult;
        }

        // TODO: to match the C-Ruby exception
        private static Exception ToRubyException(Exception/*!*/ ex, string path, DirectoryOperation op) {
            Assert.NotNull(ex);
            
            Type exceptionType = ex.GetType();

            switch (op) {
                case DirectoryOperation.ChangeDir:
                    return RubyExceptions.CreateEINVAL(path);

                case DirectoryOperation.Open:
                    return RubyExceptions.CreateENOENT("No such file or directory - {0}", path);

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

            if (ex is UnauthorizedAccessException) {
                return Errno.CreateEACCES(path, ex);
            }

            // throw anyway
            return RubyExceptions.CreateSystemCallError("unknown scenario - {0}, {1}, {2}", exceptionType, path, op);
        }

        #endregion
    }
}
