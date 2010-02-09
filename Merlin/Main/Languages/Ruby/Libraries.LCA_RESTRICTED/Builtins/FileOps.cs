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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using IronRuby.Compiler;
using System.Globalization;

namespace IronRuby.Builtins {

    /// <summary>
    /// File builtin class. Derives from IO
    /// </summary>
    [RubyClass("File", Extends = typeof(RubyFile))]
    [Includes(typeof(FileTest), Copy = true)]
    public static class RubyFileOps {

        #region Construction

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]Union<int, MutableString> descriptorOrPath, [Optional, DefaultProtocol]MutableString mode, [Optional]int permission) {

            if (descriptorOrPath.IsFixnum()) {
                return new RubyFile(
                    self.Context, RubyIOOps.GetDescriptorStream(self.Context, descriptorOrPath.Fixnum()), descriptorOrPath.Fixnum(), IOModeEnum.Parse(mode)
                );
            } else {
                // TODO: permissions
                return CreateFile(self, descriptorOrPath.Second, mode);
            }
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]Union<int, MutableString> descriptorOrPath, int mode, [Optional]int permission) {

            if (descriptorOrPath.IsFixnum()) {
                return new RubyFile(
                    self.Context, RubyIOOps.GetDescriptorStream(self.Context, descriptorOrPath.Fixnum()), descriptorOrPath.Fixnum(), (IOMode)mode
                );
            } else {
                // TODO: permissions
                return CreateFile(self, descriptorOrPath.Second, mode);
            }
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path) {
            return new RubyFile(self.Context, path, IOMode.Default);
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path, MutableString mode) {
            return new RubyFile(self.Context, path, IOModeEnum.Parse(mode));
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path, int mode) {
            return new RubyFile(self.Context, path, (IOMode)mode);
        }

        #endregion

        #region Declared Constants

        static RubyFileOps() {
            ALT_SEPARATOR = MutableString.CreateAscii(AltDirectorySeparatorChar.ToString()).Freeze();
            SEPARATOR = MutableString.CreateAscii(DirectorySeparatorChar.ToString()).Freeze();
            Separator = SEPARATOR;
            PATH_SEPARATOR = MutableString.CreateAscii(PathSeparatorChar.ToString()).Freeze();
        }

        private const char AltDirectorySeparatorChar = '\\';
        private const char DirectorySeparatorChar = '/';
        private const char PathSeparatorChar = ';';

        internal static bool IsDirectorySeparator(int c) {
            return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
        }

        [RubyConstant]
        public readonly static MutableString ALT_SEPARATOR;

        [RubyConstant]
        public readonly static MutableString PATH_SEPARATOR;

        [RubyConstant]
        public readonly static MutableString SEPARATOR;

        [RubyConstant]
        public readonly static MutableString Separator = SEPARATOR;

        private const string NUL_VALUE = "NUL";

        [RubyModule("Constants")]
        public static class Constants {
            [RubyConstant]
            public readonly static int APPEND = (int)IOMode.WriteAppends;
            [RubyConstant]
            public readonly static int BINARY = (int)IOMode.PreserveEndOfLines;
            [RubyConstant]
            public readonly static int CREAT = (int)IOMode.CreateIfNotExists;
            [RubyConstant]
            public readonly static int EXCL = (int)IOMode.ErrorIfExists;
            [RubyConstant]
            public readonly static int FNM_CASEFOLD = 0x08;
            [RubyConstant]
            public readonly static int FNM_DOTMATCH = 0x04;
            [RubyConstant]
            public readonly static int FNM_NOESCAPE = 0x01;
            [RubyConstant]
            public readonly static int FNM_PATHNAME = 0x02;
            [RubyConstant]
            public readonly static int FNM_SYSCASE = 0x08;
            [RubyConstant]
            public readonly static int LOCK_EX = 0x02;
            [RubyConstant]
            public readonly static int LOCK_NB = 0x04;
            [RubyConstant]
            public readonly static int LOCK_SH = 0x01;
            [RubyConstant]
            public readonly static int LOCK_UN = 0x08;
            [RubyConstant]
            public readonly static int NONBLOCK = (int)IOMode.WriteOnly;
            [RubyConstant]
            public readonly static int RDONLY = (int)IOMode.ReadOnly;
            [RubyConstant]
            public readonly static int RDWR = (int)IOMode.ReadWrite;
            [RubyConstant]
            public readonly static int TRUNC = (int)IOMode.Truncate;
            [RubyConstant]
            public readonly static int WRONLY = (int)IOMode.WriteOnly;
        }

        #endregion

        internal const int WriteModeMask = 0x80; // Oct 0200
        internal const int ReadWriteMode = 0x1B6; // Oct 0666

        #region Public Singleton Methods

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Open() {
            return RubyIOOps.Open();
        }

        [RubyMethod("atime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime AccessTime(RubyClass/*!*/ self, [DefaultProtocol]MutableString/*!*/ path) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("basename", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Basename(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultProtocol, NotNull, Optional]MutableString suffix) {

            if (path.IsEmpty) {
                return path;
            }

            string strPath = path.ConvertToString();
            string[] parts = strPath.Split(new[] { DirectorySeparatorChar, AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) {
                return MutableString.CreateMutable(path.Encoding).Append((char)path.GetLastChar()).TaintBy(path);
            }

            if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX) {
                string first = parts[0];
                if (strPath.Length >= 2 && IsDirectorySeparator(strPath[0]) && IsDirectorySeparator(strPath[1])) {
                    // UNC: skip 2 parts 
                    if (parts.Length <= 2) {
                        return MutableString.CreateMutable(path.Encoding).Append(DirectorySeparatorChar).TaintBy(path);
                    }
                } else if (first.Length == 2 && Tokenizer.IsLetter(first[0]) && first[1] == ':') {
                    // skip drive letter "X:"
                    if (parts.Length <= 1) {
                        var result = MutableString.CreateMutable(path.Encoding).TaintBy(path);
                        if (strPath.Length > 2) {
                            result.Append(strPath[2]);
                        }
                        return result;
                    }
                }
            }
            
            string last = parts[parts.Length - 1];
            if (MutableString.IsNullOrEmpty(suffix)) {
                return MutableString.CreateMutable(last, path.Encoding);
            }

            StringComparison comparison = Environment.OSVersion.Platform == PlatformID.Unix ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int matchLength = last.Length;

            if (suffix != null) {
                string strSuffix = suffix.ToString();
                if (strSuffix.LastCharacter() == '*' && strSuffix.Length > 1) {
                    int suffixIdx = last.LastIndexOf(
                        strSuffix.Substring(0, strSuffix.Length - 1),
                        comparison
                    );
                    if (suffixIdx >= 0 && suffixIdx + strSuffix.Length <= last.Length) {
                        matchLength = suffixIdx;
                    }
                } else if (last.EndsWith(strSuffix, comparison)) {
                    matchLength = last.Length - strSuffix.Length;
                }
            }

            return MutableString.CreateMutable(path.Encoding).Append(last, 0, matchLength).TaintBy(path);
        }

        internal static void Chmod(string/*!*/ path, int permission) {
#if !SILVERLIGHT
            FileAttributes oldAttributes = File.GetAttributes(path);
            if ((permission & WriteModeMask) == 0) {
                File.SetAttributes(path, oldAttributes | FileAttributes.ReadOnly);
            } else {
                File.SetAttributes(path, oldAttributes & ~FileAttributes.ReadOnly);
            }
#endif
        }

        [RubyMethod("chmod")]
        public static int Chmod(RubyFile/*!*/ self, [DefaultProtocol]int permission) {
            Chmod(self.Path, permission);
            return 0;
        }

        [RubyMethod("chmod", RubyMethodAttributes.PublicSingleton)]
        public static int Chmod(RubyClass/*!*/ self, [DefaultProtocol]int permission, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            Chmod(self.Context.DecodePath(path), permission);
            return 1;
        }

        [RubyMethod("chown")]
        public static int ChangeOwner(RubyFile/*!*/ self, [DefaultProtocol]int owner, [DefaultProtocol]int group) {
            return 0;
        }

        [RubyMethod("chown")]
        public static int ChangeOwner(RubyContext/*!*/ context, RubyFile/*!*/ self, object owner, object group) {
            if ((owner == null || owner is int) && (group == null || group is int)) {
                return 0;
            }
            throw RubyExceptions.CreateUnexpectedTypeError(context, owner, "Fixnum");
        }

        [RubyMethod("chown", RubyMethodAttributes.PublicSingleton)]
        public static int ChangeOwner(RubyClass/*!*/ self, [DefaultProtocol]int owner, [DefaultProtocol]int group, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return 0;
        }

        [RubyMethod("chown", RubyMethodAttributes.PublicSingleton)]
        public static int ChangeOwner(RubyContext/*!*/ context, RubyClass/*!*/ self, object owner, object group, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            if ((owner == null || owner is int) && (group == null || group is int)) {
                return 0;
            }
            throw RubyExceptions.CreateUnexpectedTypeError(context, owner, "Fixnum");
        }

        [RubyMethod("ctime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime CreateTime(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(self.Context, path));
        }

        internal static bool FileExists(RubyContext/*!*/ context, MutableString/*!*/ path) {
            return context.Platform.FileExists(context.DecodePath(path));
        }

        internal static bool DirectoryExists(RubyContext/*!*/ context, MutableString/*!*/ path) {
            return context.Platform.DirectoryExists(context.DecodePath(path));
        }

        internal static bool Exists(RubyContext/*!*/ context, MutableString/*!*/ path) {
            var strPath = context.DecodePath(path);
            return context.Platform.DirectoryExists(strPath) || context.Platform.FileExists(strPath);
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int Delete(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string strPath = self.Context.DecodePath(path);
            if (!self.Context.Platform.FileExists(strPath)) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", strPath);
            }

            Delete(self.Context, strPath);     
            return 1;
        }

        internal static void Delete(RubyContext/*!*/ context, string/*!*/ path) {
            try {
                context.Platform.DeleteFile(path, true);
            } catch (DirectoryNotFoundException) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", path);
            } catch (IOException e) {
                throw Errno.CreateEACCES(e.Message, e);
            }
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int Delete(RubyClass/*!*/ self, [DefaultProtocol, NotNullItems]params MutableString/*!*/[]/*!*/ paths) {
            foreach (MutableString path in paths) {
                Delete(self, path);
            }

            return paths.Length;
        }

        [RubyMethod("dirname", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ DirName(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            string directoryName = strPath;

            if (IsValidPath(strPath)) {
                strPath = StripPathCharacters(strPath);

                // handle top-level UNC paths
                directoryName = Path.GetDirectoryName(strPath);
                if (directoryName == null) {
                    return self.Context.EncodePath(strPath);
                }

                string fileName = Path.GetFileName(strPath);
                if (!String.IsNullOrEmpty(fileName)) {
                    directoryName = StripPathCharacters(strPath.Substring(0, strPath.LastIndexOf(fileName, StringComparison.Ordinal)));
                }
            } else {
                if (directoryName.Length > 1) {
                    directoryName = "//";
                }
            }

            directoryName = String.IsNullOrEmpty(directoryName) ? "." : directoryName;
            return self.Context.EncodePath(directoryName);
        }

        private static bool IsValidPath(string path) {
            foreach (char c in path) {
                if (c != '/' && c != '\\') {
                    return true;
                }
            }
            return false;

        }

        private static string StripPathCharacters(string path) {
            int limit = 0;
            for (int charIndex = path.Length - 1; charIndex > 0; charIndex--) {
                if (!((path[charIndex] == '/') || (path[charIndex] == '\\')))
                    break;
                limit++;
            }
            if (limit > 0) {
                limit--;
                if (path.Length == 3 && path[1] == ':') limit--;
                return path.Substring(0, path.Length - limit - 1);
            }
            return path;
        }

        [RubyMethod("extname", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetExtension(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string pathStr = path.ConvertToString();
            string extension = Path.GetExtension(pathStr);
            string filename = Path.GetFileName(pathStr);
            if (extension == filename) {
                // File.extname(".foo") should be "", but Path.GetExtension(".foo") returns ".foo"
                extension = String.Empty;
            }
            return MutableString.Create(extension, path.Encoding).TaintBy(path);
        }

        #region fnmatch

        [RubyMethod("fnmatch", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("fnmatch?", RubyMethodAttributes.PublicSingleton)]
        public static bool FnMatch(object/*!*/ self, [NotNull]MutableString/*!*/ pattern, [NotNull]MutableString/*!*/ path, [Optional]int flags) {
            return Glob.FnMatch(pattern.ConvertToString(), path.ConvertToString(), flags);
        }

        #endregion

        [RubyMethod("ftype", RubyMethodAttributes.PublicSingleton)]
        public static MutableString FileType(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.FileType(RubyStatOps.Create(self.Context, path));
        }

        #region join

        [RubyMethod("join", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Join(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, params object[]/*!*/ parts) {
            MutableString result = MutableString.CreateMutable(RubyEncoding.Binary);
            Dictionary<object, bool> visitedLists = null;
            var worklist = new Stack<object>();
            int current = 0;
            MutableString str;

            Push(worklist, parts);
            while (worklist.Count > 0) {
                object part = worklist.Pop();
                var list = part as IList;
                if (list != null) {
                    if (list.Count == 0) {
                        str = MutableString.FrozenEmpty;
                    } else if (visitedLists != null && visitedLists.ContainsKey(list)) {
                        str = RubyUtils.InfiniteRecursionMarker;
                    } else {
                        if (visitedLists == null) {
                            visitedLists = new Dictionary<object, bool>(ReferenceEqualityComparer.Instance);
                        }
                        visitedLists.Add(list, true);
                        Push(worklist, list);
                        continue;
                    }
                } else if (part == null) {
                    throw RubyExceptions.CreateTypeConversionError("NilClass", "String");
                } else {
                    str = Protocols.CastToString(stringCast, part);
                }

                if (current > 0) {
                    AppendDirectoryName(result, str);
                } else {
                    result.Append(str);
                }
                current++;
            }

            return result;
        }

        private static void Push(Stack<Object>/*!*/ stack, IList/*!*/ values) {
            for (int i = values.Count - 1; i >= 0; i--) {
                stack.Push(values[i]);
            }
        }

        private static void AppendDirectoryName(MutableString/*!*/ result, MutableString/*!*/ name) {
            int resultLength = result.GetCharCount();

            int i;
            for (i = resultLength - 1; i >= 0; i--) {
                if (!IsDirectorySeparator(result.GetChar(i))) {
                    break;
                }
            }

            if (i == resultLength - 1) {
                if (!IsDirectorySeparator(name.GetFirstChar())) {
                    result.Append(DirectorySeparatorChar);
                }
                result.Append(name);
            } else if (IsDirectorySeparator(name.GetFirstChar())) {
                result.Replace(i + 1, resultLength - i - 1, name);
            } else {
                result.Append(name);
            }
        }

        #endregion

#if !SILVERLIGHT

        [RubyMethod("expand_path", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static MutableString/*!*/ ExpandPath(
            RubyClass/*!*/ self, 
            [DefaultProtocol, NotNull]MutableString/*!*/ path,
            [DefaultProtocol, Optional]MutableString basePath) {

            var context = self.Context;

            string result = RubyUtils.ExpandPath(
                context.Platform,
                context.DecodePath(path),
                basePath == null ? null : context.DecodePath(basePath)
            );

            return self.Context.EncodePath(result);
        }
#endif

        //lchmod
        //lchown
        //link

        [RubyMethod("lstat", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("stat", RubyMethodAttributes.PublicSingleton)]
        public static FileSystemInfo/*!*/ Stat(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.Create(self.Context, path);
        }

        [RubyMethod("mtime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime ModifiedTime(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("readlink", RubyMethodAttributes.PublicSingleton)]
        public static bool Readlink(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path) {
            throw new IronRuby.Builtins.NotImplementedError("readlink() function is unimplemented on this machine");
        }

        [RubyMethod("rename", RubyMethodAttributes.PublicSingleton)]
        public static int Rename(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ oldPath, [DefaultProtocol, NotNull]MutableString/*!*/ newPath) {

            if (oldPath.IsEmpty || newPath.IsEmpty) {
                throw RubyExceptions.CreateENOENT();
            }

            var context = self.Context;

            string strOldPath = context.DecodePath(oldPath);
            if (!context.Platform.FileExists(strOldPath) && !context.Platform.DirectoryExists(strOldPath)) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", oldPath);
            }

            string strNewPath = context.DecodePath(newPath);
            if (RubyUtils.ExpandPath(context.Platform, strOldPath) == RubyUtils.ExpandPath(context.Platform, strNewPath)) {
                return 0;
            }

            if (context.Platform.FileExists(strNewPath)) {
                Delete(context, strNewPath);
            }

            try {
                context.Platform.MoveFileSystemEntry(strOldPath, strNewPath);
            } catch (IOException e) {
                throw Errno.CreateEACCES(e.Message, e);
            }

            return 0;
        }

        [RubyMethod("split", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Split(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            RubyArray result = new RubyArray(2);
            result.Add(DirName(self, path));
            result.Add(Basename(self, path, null));
            return result;
        }

#if !SILVERLIGHT
        [RubyMethod("truncate", BuildConfig = "!SILVERLIGHT")]
        public static int Truncate(RubyFile/*!*/ self, [DefaultProtocol]int size) {
            if (size < 0) {
                throw new InvalidError();
            }

            self.Length = size;
            return 0;
        }

        [RubyMethod("truncate", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int Truncate(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultProtocol]int size) {
            if (size < 0) {
                throw new InvalidError();
            }
            using (RubyFile f = new RubyFile(self.Context, path, IOMode.ReadWrite)) {
                f.Length = size;
            }
            return 0;
        }
#endif

        internal static readonly object UmaskKey = new object();

        [RubyMethod("umask", RubyMethodAttributes.PublicSingleton)]
        public static int GetUmask(RubyClass/*!*/ self, [DefaultProtocol]int mask) {
            int result = (int)self.Context.GetOrCreateLibraryData(UmaskKey, () => 0);
            self.Context.TrySetLibraryData(UmaskKey, CalculateUmask(mask));
            return result;
        }

        [RubyMethod("umask", RubyMethodAttributes.PublicSingleton)]
        public static int GetUmask(RubyClass/*!*/ self) {
            return (int)self.Context.GetOrCreateLibraryData(UmaskKey, () => 0);
        }

        private static int CalculateUmask(int mask) {
            return (mask % 512) / 128 * 128;
        }
        
#if !SILVERLIGHT
        [RubyMethod("symlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object SymLink(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            throw new NotImplementedError("symlnk() function is unimplemented on this machine");
        }

        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int UpdateTimes(RubyClass/*!*/ self, [NotNull]RubyTime/*!*/ accessTime, [NotNull]RubyTime/*!*/ modifiedTime, 
            [NotNull]MutableString/*!*/ path) {

            string strPath = self.Context.DecodePath(path);
            FileInfo info = new FileInfo(strPath);
            if (!info.Exists) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", strPath);
            }
            info.LastAccessTimeUtc = accessTime.ToUniversalTime();
            info.LastWriteTimeUtc = modifiedTime.ToUniversalTime();
            return 1;
        }

        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton)]
        public static int UpdateTimes(RubyClass/*!*/ self, object accessTime, object modifiedTime,
            [DefaultProtocol, NotNullItems]params MutableString/*!*/[]/*!*/ paths) {

            RubyTime atime = MakeTime(self.Context, accessTime);
            RubyTime mtime = MakeTime(self.Context, modifiedTime);

            foreach (MutableString path in paths) {
                UpdateTimes(self, atime, mtime, path);
            }

            return paths.Length;
        }
#endif

        private static RubyTime MakeTime(RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                return new RubyTime(DateTime.Now);
            } else if (obj is RubyTime) {
                return (RubyTime)obj;
            } else if (obj is int) {
                return new RubyTime(RubyTime.Epoch.AddSeconds((int)obj));
            } else if (obj is double) {
                return new RubyTime(RubyTime.Epoch.AddSeconds((double)obj));
            } else {
                string name = context.GetClassOf(obj).Name;
                throw RubyExceptions.CreateTypeConversionError(name, "time");
            }
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("atime")]
        public static RubyTime AccessTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(context, self.Path));
        }

        //chmod
        //chown

        [RubyMethod("ctime")]
        public static RubyTime CreateTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(context, self.Path));
        }

        //flock

        [RubyMethod("lstat")]
        [RubyMethod("stat")]
        public static FileSystemInfo Stat(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.Create(context, self.Path);
        }

        [RubyMethod("mtime")]
        public static RubyTime ModifiedTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(context, self.Path));
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return MutableString.CreateMutable(context.GetPathEncoding()).
                Append("#<File:").
                Append(self.Path).
                Append(self.Closed ? " (closed)" : "").
                Append('>');
        }

        [RubyMethod("path")]
        public static MutableString GetPath(RubyFile/*!*/ self) {
            return self.Path != null ? self.Context.EncodePath(self.Path) : null;
        }

        //truncate

        #endregion

        #region File::Stat

        /// <summary>
        /// Stat
        /// </summary>
        [RubyClass("Stat", Extends = typeof(FileSystemInfo), Inherits = typeof(object), BuildConfig = "!SILVERLIGHT"), Includes(typeof(Comparable))]
        public class RubyStatOps {

            internal static FileSystemInfo/*!*/ Create(RubyContext/*!*/ context, MutableString/*!*/ path) {
                return Create(context, context.DecodePath(path));
            }

            internal static FileSystemInfo/*!*/ Create(RubyContext/*!*/ context, string/*!*/ path) {
                FileSystemInfo fsi;
                if (TryCreate(context, path, out fsi)) {
                    return fsi;
                } else {
                    throw RubyExceptions.CreateENOENT("No such file or directory - {0}", path);
                }
            }

            internal static bool TryCreate(RubyContext/*!*/ context, string/*!*/ path, out FileSystemInfo result) {
                PlatformAdaptationLayer pal = context.Platform;
                result = null;
                if (pal.FileExists(path)) {
                    result = new FileInfo(path);                    
                } else if (pal.DirectoryExists(path)) {
                    result = new DirectoryInfo(path);                    
                } else if (path.ToUpperInvariant().Equals(NUL_VALUE)) {
                    result = new DeviceInfo(NUL_VALUE);
                } else {
                    return false;
                }
                return true;
            }


            [RubyConstructor]
            public static FileSystemInfo/*!*/ Create(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
                return Create(self.Context, path);
            }

            [RubyMethod("<=>")]
            public static int Compare(FileSystemInfo/*!*/ self, [NotNull]FileSystemInfo/*!*/ other) {
                return self.LastWriteTime.CompareTo(other.LastWriteTime);
            }

            [RubyMethod("<=>")]
            public static object Compare(FileSystemInfo/*!*/ self, object other) {
                Debug.Assert(other as FileSystemInfo == null);
                return null;
            }

            [RubyMethod("atime")]
            public static RubyTime/*!*/ AccessTime(FileSystemInfo/*!*/ self) {
                return new RubyTime(self.LastAccessTime);
            }

            [RubyMethod("blksize")]
            public static object BlockSize(FileSystemInfo/*!*/ self) {
                return null;
            }

            [RubyMethod("blockdev?")]
            public static bool IsBlockDevice(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("blocks")]
            public static object Blocks(FileSystemInfo/*!*/ self) {
                return null;
            }

            [RubyMethod("chardev?")]
            public static bool IsCharDevice(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("ctime")]
            public static RubyTime/*!*/ CreateTime(FileSystemInfo/*!*/ self) {
                return new RubyTime(self.CreationTime);
            }

            [RubyMethod("dev")]
            [RubyMethod("rdev")]
            public static object DeviceId(FileSystemInfo/*!*/ self) {
                // TODO: Map to drive letter?
                return 3;
            }

            [RubyMethod("dev_major")]
            [RubyMethod("rdev_major")]
            public static object DeviceIdMajor(FileSystemInfo/*!*/ self) {
                return null;
            }

            [RubyMethod("dev_minor")]
            [RubyMethod("rdev_minor")]
            public static object DeviceIdMinor(FileSystemInfo/*!*/ self) {
                return null;
            }

            [RubyMethod("directory?")]
            public static bool IsDirectory(FileSystemInfo/*!*/ self) {
                return (self is DirectoryInfo);
            }

            [RubyMethod("executable?")]
            [RubyMethod("executable_real?")]
            public static bool IsExecutable(FileSystemInfo/*!*/ self) {
                // TODO: Fix
                return self.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase);
            }

            [RubyMethod("file?")]
            public static bool IsFile(FileSystemInfo/*!*/ self) {
                return self is FileInfo;
            }

            [RubyMethod("ftype")]
            public static MutableString FileType(FileSystemInfo/*!*/ self) {
                return MutableString.CreateAscii(IsFile(self) ? "file" : "directory");
            }

            [RubyMethod("gid")]
            public static int GroupId(FileSystemInfo/*!*/ self) {
                return 0;
            }

            [RubyMethod("grpowned?")]
            public static bool IsGroupOwned(FileSystemInfo/*!*/ self) {
                return false;
            }
            
            [RubyMethod("ino")]
            public static int Inode(FileSystemInfo/*!*/ self) {
                return 0;
            }

            [RubyMethod("inspect")]
            public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, FileSystemInfo/*!*/ self) {
               return MutableString.CreateAscii(String.Format(CultureInfo.InvariantCulture, 
                    "#<File::Stat dev={0}, ino={1}, mode={2}, nlink={3}, uid={4}, gid={5}, rdev={6}, size={7}, blksize={8}, blocks={9}, atime={10}, mtime={11}, ctime={12}",
                    context.Inspect(DeviceId(self)),
                    context.Inspect(Inode(self)),
                    context.Inspect(Mode(self)),
                    context.Inspect(NumberOfLinks(self)),
                    context.Inspect(UserId(self)),
                    context.Inspect(GroupId(self)),
                    context.Inspect(DeviceId(self)),
                    context.Inspect(Size(self)),
                    context.Inspect(BlockSize(self)),
                    context.Inspect(Blocks(self)),
                    context.Inspect(AccessTime(self)),
                    context.Inspect(ModifiedTime(self)),
                    context.Inspect(CreateTime(self))
                ));
            }

            [RubyMethod("mode")]
            public static int Mode(FileSystemInfo/*!*/ self) {
                int mode = (self is FileInfo) ? 0x8000 : 0x4000;
                mode |= 0x100; // S_IREAD;
                if ((self.Attributes & FileAttributes.ReadOnly) == 0) {
                    mode |= 0x80; // S_IWRITE;
                }
                return mode;
            }

            [RubyMethod("mtime")]
            public static RubyTime/*!*/ ModifiedTime(FileSystemInfo/*!*/ self) {
                return new RubyTime(self.LastWriteTime);
            }

            [RubyMethod("nlink")]
            public static int NumberOfLinks(FileSystemInfo/*!*/ self) {
                return 1;
            }

            [RubyMethod("owned?")]
            public static bool IsUserOwned(FileSystemInfo/*!*/ self) {
                return true;
            }

            [RubyMethod("pipe?")]
            public static bool IsPipe(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("readable?")]
            [RubyMethod("readable_real?")]
            public static bool IsReadable(FileSystemInfo/*!*/ self) {
                // TODO: Security, including identifying that we're impersonating another principal
                // ie. System.Security.AccessControl control = info.GetAccessControl();
                return true;
            }

            [RubyMethod("setgid?")]
            public static bool IsSetGid(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("setuid?")]
            public static bool IsSetUid(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("size")]
            public static int Size(FileSystemInfo/*!*/ self) {
                if (self is DeviceInfo) {
                    return 0;
                }

                FileInfo info = (self as FileInfo);
                return (info == null) ? 0 : (int)info.Length;
            }

            [RubyMethod("size?")]
            public static object NullableSize(FileSystemInfo/*!*/ self) {
                if (self is DeviceInfo) {
                    return 0;
                }

                FileInfo info = (self as FileInfo);
                if (info == null) {
                    return null;
                }
                return (info.Length == 0) ? null : (object)(int)info.Length;
            }

            [RubyMethod("socket?")]
            public static bool IsSocket(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("sticky?")]
            public static bool IsSticky(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("symlink?")]
            public static bool IsSymLink(FileSystemInfo/*!*/ self) {
                return false;
            }

            [RubyMethod("uid")]
            public static int UserId(FileSystemInfo/*!*/ self) {
                return 0;
            }

            [RubyMethod("writable?")]
            [RubyMethod("writable_real?")]
            public static bool IsWritable(FileSystemInfo/*!*/ self) {
                // TODO: Security, including identifying that we're impersonating another principal
                // ie. System.Security.AccessControl control = info.GetAccessControl();
                return ((self.Attributes & FileAttributes.ReadOnly) == 0);
            }

            [RubyMethod("zero?")]
            public static bool IsZeroLength(FileSystemInfo/*!*/ self) {
                if (self is DeviceInfo) {
                    return true;
                }

                FileInfo info = (self as FileInfo);
                return (info == null) ? false : info.Length == 0;
            }

            internal class DeviceInfo : FileSystemInfo {
                
                private string/*!*/ _name;

                internal DeviceInfo(string/*!*/ name) {
                    _name = name;
                }

                public override void Delete() {
                    throw new NotImplementedException();
                }

                public override bool Exists {
                    get { return true; }
                }

                public override string Name {
                    get { return _name; }
                }
            }
        }

        #endregion
    }
}
