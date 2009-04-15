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
 *attrb
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace IronRuby.Builtins {

    /// <summary>
    /// File builtin class. Derives from IO
    /// </summary>
    [RubyClass("File", Extends = typeof(RubyFile))]
    public class RubyFileOps {

        #region Construction

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, 
            [DefaultProtocol]Union<int, MutableString> descriptorOrPath, [Optional, DefaultProtocol]MutableString mode, [Optional]int permission) {

            if (descriptorOrPath.IsFixnum()) {
                // TODO: descriptor
                throw new NotImplementedException();
            } else {
                // TODO: permissions
                return CreateFile(self, descriptorOrPath.Second, mode);
            }
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self,
            [DefaultProtocol]Union<int, MutableString> descriptorOrPath, int mode, [Optional]int permission) {

            if (descriptorOrPath.IsFixnum()) {
                // TODO: descriptor
                throw new NotImplementedException();
            } else {
                // TODO: permissions
                return CreateFile(self, descriptorOrPath.Second, mode);
            }
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, MutableString/*!*/ path) {
            return new RubyFile(self.Context, path.ConvertToString(), "r");
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, MutableString/*!*/ path, MutableString mode) {
            return new RubyFile(self.Context, path.ConvertToString(), (mode != null) ? mode.ConvertToString() : "r");
        }

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(RubyClass/*!*/ self, MutableString/*!*/ path, int mode) {
            return new RubyFile(self.Context, path.ConvertToString(), (RubyFileMode)mode);
        }

        #endregion

        #region Declared Constants

        static RubyFileOps() {
            ALT_SEPARATOR = MutableString.Create(AltDirectorySeparatorChar.ToString()).Freeze();
            SEPARATOR = MutableString.Create(DirectorySeparatorChar.ToString()).Freeze();
            Separator = SEPARATOR;
            PATH_SEPARATOR = MutableString.Create(PathSeparatorChar.ToString()).Freeze();
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
            public readonly static int APPEND = (int)RubyFileMode.APPEND;
            [RubyConstant]
            public readonly static int BINARY = (int)RubyFileMode.BINARY;
            [RubyConstant]
            public readonly static int CREAT = (int)RubyFileMode.CREAT;
            [RubyConstant]
            public readonly static int EXCL = (int)RubyFileMode.EXCL;
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
            public readonly static int NONBLOCK = (int)RubyFileMode.NONBLOCK;
            [RubyConstant]
            public readonly static int RDONLY = (int)RubyFileMode.RDONLY;
            [RubyConstant]
            public readonly static int RDWR = (int)RubyFileMode.RDWR;
            [RubyConstant]
            public readonly static int TRUNC = (int)RubyFileMode.TRUNC;
            [RubyConstant]
            public readonly static int WRONLY = (int)RubyFileMode.WRONLY;
        }

        #endregion

        #region Public Singleton Methods

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Open() {
            return RubyIOOps.Open();
        }

        [RubyMethod("atime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime AccessTime(RubyClass/*!*/ self, [DefaultProtocol]MutableString/*!*/ path) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(self.Context, path));
        }

        private static bool WildcardExtensionMatch(string/*!*/ extension, string/*!*/ pattern) {
            for (int i = 0; i < pattern.Length; i++) {
                if (i >= extension.Length) {
                    return false;
                }

                if (pattern[i] == '*') {
                    return true;
                }

                if (extension[i] != pattern[i]) {
                    return false;
                }
            }
            return true;
        }

        private static MutableString/*!*/ TrimTrailingSlashes(MutableString/*!*/ path) {
            int offset = path.Length - 1;
            while (offset > 0) {
                if (path.GetChar(offset) != '/' && path.GetChar(offset) != '\\')
                    break;
                --offset;
            }
            return path.GetSlice(0, offset + 1);
        }

        [RubyMethod("basename", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ Basename(RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ path, [DefaultProtocol, NotNull, Optional]MutableString extensionFilter) {

            if (path.IsEmpty) {
                return path;
            }

            MutableString trimmedPath = TrimTrailingSlashes(path);

            // Special cases of drive letters C:\\ or C:/
            if (trimmedPath.Length == 2) {
                if (Char.IsLetter(trimmedPath.GetChar(0)) && trimmedPath.GetChar(1) == ':') {
                    var result = (path.Length > 2 ? MutableString.Create(path.GetChar(2).ToString()) : MutableString.CreateMutable());
                    return result.TaintBy(path);
                }
            }

            string trimmedPathAsString = trimmedPath.ConvertToString();
            if (trimmedPathAsString == "/") {
                return trimmedPath;
            }

            string filename = Path.GetFileName(trimmedPath.ConvertToString());

            // Handle UNC host names correctly
            string root = Path.GetPathRoot(trimmedPath.ConvertToString());
            if (MutableString.IsNullOrEmpty(extensionFilter)) {
                return MutableString.Create(trimmedPathAsString == root ? root : filename);
            }

            string fileExtension = Path.GetExtension(filename);
            string basename = Path.GetFileNameWithoutExtension(filename);

            string strResult = WildcardExtensionMatch(fileExtension, extensionFilter.ConvertToString()) ? basename : filename;
            return Glob.CanonicalizePath(MutableString.Create(strResult)).TaintBy(path);
        }

        [RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsBlockDevice(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsBlockDevice(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsCharDevice(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsCharDevice(RubyStatOps.Create(self.Context, path));
        }

        private static void Chmod(string path, int permission) {
#if !SILVERLIGHT
            FileAttributes oldAttributes = File.GetAttributes(path);
            if ((permission & 0x80) == 0) {
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
            // TODO: implement this correctly for windows
            Chmod(path.ConvertToString(), permission);
            return 1;
        }

        //chown

        [RubyMethod("ctime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime CreateTime(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(self.Context, path));
        }

        private static bool FileExists(RubyContext/*!*/ context, string/*!*/ path) {
            return context.DomainManager.Platform.FileExists(path);
        }

        private static bool DirectoryExists(RubyContext/*!*/ context, string/*!*/ path) {
            return context.DomainManager.Platform.DirectoryExists(path);
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int Delete(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            if (!FileExists(self.Context, strPath)) {
                throw new Errno.NoEntryError(String.Format("No such file or directory - {0}", strPath));
            }
#if !SILVERLIGHT
            FileAttributes oldAttributes = File.GetAttributes(strPath);
            if ((oldAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                // File.Delete throws UnauthorizedAccessException if the file is read-only
                File.SetAttributes(strPath, oldAttributes & ~FileAttributes.ReadOnly);
            }
#endif
            File.Delete(strPath);
            return 1;
        }

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int Delete(RubyClass/*!*/ self, [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ paths) {
            foreach (MutableString path in paths) {
                Delete(self, path);
            }

            return paths.Length;
        }

        [RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsDirectory(RubyContext/*!*/ context, object/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return DirectoryExists(context, path.ConvertToString());
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
                    return MutableString.Create(strPath);
                }

                string fileName = Path.GetFileName(strPath);
                if (!String.IsNullOrEmpty(fileName)) {
                    directoryName = StripPathCharacters(strPath.Substring(0, strPath.LastIndexOf(fileName)));
                }
            } else {
                if (directoryName.Length > 1) {
                    directoryName = "//";
                }
            }

            directoryName = String.IsNullOrEmpty(directoryName) ? "." : directoryName;
            return MutableString.Create(directoryName);
        }

        private static bool IsValidPath(string path) {
            int length = 0;
            foreach (char c in path.ToCharArray()) {
                if ((c == '/') || (c == '\\'))
                    continue;
                length++;
            }
            return (length > 0);

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

        [RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsExecutable(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsExecutable(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        public static bool Exists(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            return FileExists(self.Context, strPath) || DirectoryExists(self.Context, strPath);
        }

        [RubyMethod("extname", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetExtension(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return MutableString.Create(Path.GetExtension(path.ConvertToString())).TaintBy(path);
        }

        [RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsAFile(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return FileExists(self.Context, path.ConvertToString());
        }

        #region fnmatch

        [RubyMethod("fnmatch", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("fnmatch?", RubyMethodAttributes.PublicSingleton)]
        public static bool FnMatch(object/*!*/ self, [NotNull]MutableString/*!*/ pattern, [NotNull]MutableString/*!*/ path, [Optional]int flags) {
            return Glob.FnMatch(pattern, path, flags);
        }

        #endregion

        [RubyMethod("ftype", RubyMethodAttributes.PublicSingleton)]
        public static MutableString FileType(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.FileType(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsGroupOwned(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsGroupOwned(RubyStatOps.Create(self.Context, path));
        }

        //identical?

        #region join

        private static readonly MutableString InfiniteRecursionMarker = MutableString.Create("[...]").Freeze();

        [RubyMethod("join", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Join(ConversionStorage<MutableString>/*!*/ stringCast, RubyClass/*!*/ self, [NotNull]params object[] parts) {
            MutableString result = MutableString.CreateMutable();
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
                        str = MutableString.Empty;
                    } else if (visitedLists != null && visitedLists.ContainsKey(list)) {
                        str = InfiniteRecursionMarker;
                    } else {
                        if (visitedLists == null) {
                            visitedLists = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
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

        #region expand_path

#if !SILVERLIGHT
        // Algorithm to find HOME equivalents under Windows. This is equivalent to Ruby 1.9 behavior:
        // 
        // 1. Try get HOME
        // 2. Try to generate HOME equivalent using HOMEDRIVE + HOMEPATH
        // 3. Try to generate HOME equivalent from USERPROFILE
        // 4. Try to generate HOME equivalent from Personal special folder 

        internal static string/*!*/ GetHomeDirectory(RubyContext/*!*/ context) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            string result = pal.GetEnvironmentVariable("HOME");

            if (result == null) {
                string homeDrive = pal.GetEnvironmentVariable("HOMEDRIVE");
                string homePath = pal.GetEnvironmentVariable("HOMEPATH");
                if (homeDrive == null && homePath == null) {
                    string userEnvironment = pal.GetEnvironmentVariable("USERPROFILE");
                    if (userEnvironment == null) {
                        // This will always succeed with a non-null string, but it can fail
                        // if the Personal folder was renamed or deleted. In this case it returns
                        // an empty string.
                        result = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    } else {
                        result = userEnvironment;
                    }
                } else if (homeDrive == null) {
                    result = homePath;
                } else if (homePath == null) {
                    result = homeDrive;
                } else {
                    result = Path.Combine(homeDrive, homePath);
                }
            }
            return result;
        }

        // Expand directory path - these cases exist:
        //
        // 1. Empty string or nil means return current directory
        // 2. ~ with non-existent HOME directory throws exception
        // 3. ~, ~/ or ~\ which expands to HOME
        // 4. ~foo is left unexpanded
        // 5. Expand to full path if path is a relative path
        // 
        // No attempt is made to determine whether the path is valid or not
        // Returned path is always canonicalized to forward slashes

        private static MutableString/*!*/ ExpandPath(RubyContext/*!*/ context, MutableString/*!*/ path) {
            PlatformAdaptationLayer pal = context.DomainManager.Platform;
            int length = path.Length;
            bool raisingRubyException = false;
            try {
                if (path == null || length == 0)
                    return Glob.CanonicalizePath(MutableString.Create(Directory.GetCurrentDirectory()));

                if (path.GetChar(0) == '~') {
                    if (length == 1 || (path.GetChar(1) == Path.DirectorySeparatorChar ||
                                        path.GetChar(1) == Path.AltDirectorySeparatorChar)) {

                        string homeDirectory = pal.GetEnvironmentVariable("HOME");
                        if (homeDirectory == null) {
                            raisingRubyException = true;
                            throw RubyExceptions.CreateArgumentError("couldn't find HOME environment -- expanding `~'");
                        }
                        if (length <= 2) {
                            path = MutableString.Create(homeDirectory);
                        } else {
                            path = MutableString.Create(Path.Combine(homeDirectory, path.GetSlice(2).ConvertToString()));
                        }
                        return Glob.CanonicalizePath(path);
                    } else {
                        return path;
                    }
                } else {
                    return Glob.CanonicalizePath(MutableString.Create(Path.GetFullPath(path.ConvertToString())));
                }
            } catch (Exception e) {
                if (raisingRubyException) {
                    throw;
                }
                // Re-throw exception as a reasonable Ruby exception
                throw new Errno.InvalidError(path.ConvertToString(), e);
            }
        }

        [RubyMethod("expand_path", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static MutableString/*!*/ ExpandPath(RubyContext/*!*/ context, RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path, 
            [DefaultProtocol, Optional]MutableString basePath) {

            // We ignore basePath parameter if first string starts with a ~
            if (basePath == null || path.GetFirstChar() == '~') {
                return ExpandPath(context, path);
            } else {
                return Glob.CanonicalizePath(MutableString.Create(
                    Path.GetFullPath(Path.Combine(ExpandPath(context, basePath).ConvertToString(), path.ConvertToString()))
                ));
            }
        }
#endif

        #endregion

        //lchmod
        //lchown
        //link

        [RubyMethod("lstat", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("stat", RubyMethodAttributes.PublicSingleton)]
        public static FileSystemInfo/*!*/ Stat(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.Create(self.Context, path);
        }

        [RubyMethod("mtime", RubyMethodAttributes.PublicSingleton)]
        public static DateTime ModifiedTime(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsUserOwned(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsUserOwned(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsPipe(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsPipe(RubyStatOps.Create(self.Context, path));
        }

        private static bool IsReadableImpl(RubyContext/*!*/ context, string/*!*/ path) {
            FileSystemInfo fsi;
            if (RubyStatOps.TryCreate(context, path, out fsi)) {
                return RubyStatOps.IsReadable(fsi);
            } else {
                return false;
            }
        }

        [RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsReadable(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return IsReadableImpl(self.Context, path.ConvertToString());
        }

        [RubyMethod("readlink", RubyMethodAttributes.PublicSingleton)]
        public static bool Readlink(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ path) {
            throw new IronRuby.Builtins.NotImplementedError("readlink() function is unimplemented on this machine");
        }

        [RubyMethod("rename", RubyMethodAttributes.PublicSingleton)]
        public static int Rename(RubyContext/*!*/ context, RubyClass/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ oldPath, [DefaultProtocol, NotNull]MutableString/*!*/ newPath) {

            string strOldPath = oldPath.ConvertToString();
            if (!FileExists(context, strOldPath) && !DirectoryExists(context, strOldPath)) {
                throw new Errno.NoEntryError(String.Format("No such file or directory - {0}", oldPath));
            }

            // TODO: Change to raise a SystemCallError instead of a native CLR error
            File.Move(strOldPath, newPath.ToString());
            return 0;
        }

        [RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetGid(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsSetGid(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetUid(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsSetUid(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
        public static int Size(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.Size(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
        public static object NullableSize(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.NullableSize(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSocket(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsSocket(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("split", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Split(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            RubyArray result = new RubyArray(2);
            result.Add(DirName(self, path));
            result.Add(Basename(self, path, null));
            return result;
        }

        [RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSticky(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsSticky(RubyStatOps.Create(self.Context, path));
        }

        //truncate
        //umask
        
#if !SILVERLIGHT
        [RubyMethod("symlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object SymLink(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            throw new NotImplementedError("symlnk() function is unimplemented on this machine");
        }

        [RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static bool IsSymLink(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyStatOps.IsSymLink(RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int UpdateTimes(RubyClass/*!*/ self, DateTime accessTime, DateTime modifiedTime, [NotNull]MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            if (!FileExists(self.Context, strPath)) {
                throw new Errno.NoEntryError(String.Format("No such file or directory - {0}", strPath));
            }

            FileInfo info = new FileInfo(strPath);
            info.LastAccessTime = accessTime;
            info.LastWriteTime = modifiedTime;
            return 1;
        }

        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton)]
        public static int UpdateTimes(RubyClass/*!*/ self, object accessTime, object modifiedTime,
            [DefaultProtocol, NotNull, NotNullItems]params MutableString/*!*/[]/*!*/ paths) {

            DateTime atime = MakeTime(self.Context, accessTime);
            DateTime mtime = MakeTime(self.Context, modifiedTime);

            foreach (MutableString path in paths) {
                UpdateTimes(self, atime, mtime, path);
            }

            return paths.Length;
        }
#endif

        private static DateTime MakeTime(RubyContext/*!*/ context, object obj) {
            if (obj == null) {
                return DateTime.Now;
            } else if (obj is DateTime) {
                return (DateTime)obj;
            } else if (obj is int) {
                return TimeOps.Create(typeof(RubyFileOps), (int)obj);
            } else {
                string name = context.GetClassOf(obj).Name;
                throw RubyExceptions.CreateTypeConversionError(name, "time");
            }
        }

        private static bool IsWritableImpl(RubyContext/*!*/ context, string/*!*/ path) {
            FileSystemInfo fsi;
            if (RubyStatOps.TryCreate(context, path, out fsi)) {
                return RubyStatOps.IsWritable(fsi);
            } else {
                return false;
            }
        }

        [RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsWritable(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return IsWritableImpl(self.Context, path.ConvertToString());            
        }

        [RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsZeroLength(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string strPath = path.ConvertToString();

            // NUL/nul is a special-cased filename on Windows
            if (strPath.ToLower() == "nul") {
                return RubyStatOps.IsZeroLength(RubyStatOps.Create(self.Context, strPath));
            }

            if (DirectoryExists(self.Context, strPath) || !FileExists(self.Context, strPath)) {
                return false;
            }

            return RubyStatOps.IsZeroLength(RubyStatOps.Create(self.Context, strPath));
        }

        #endregion

        #region Public Instance Methods

        [RubyMethod("atime")]
        public static DateTime AccessTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(context, self.Path));
        }

        //chmod
        //chown

        [RubyMethod("ctime")]
        public static DateTime CreateTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(context, self.Path));
        }

        //flock

        [RubyMethod("lstat")]
        public static FileSystemInfo Stat(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.Create(context, self.Path);
        }

        [RubyMethod("mtime")]
        public static DateTime ModifiedTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(context, self.Path));
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return MutableString.CreateMutable("#<File:").Append(self.Path).Append('>');
        }

        [RubyMethod("path")]
        public static MutableString/*!*/ GetPath(RubyFile/*!*/ self) {
            return MutableString.Create(self.Path);
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
                return Create(context, path.ConvertToString());
            }

            internal static FileSystemInfo/*!*/ Create(RubyContext/*!*/ context, string/*!*/ path) {
                FileSystemInfo fsi;
                if (TryCreate(context, path, out fsi)) {
                    return fsi;
                } else {
                    throw new Errno.NoEntryError(String.Format("No such file or directory - {0}", path));
                }
            }

            internal static bool TryCreate(RubyContext/*!*/ context, string/*!*/ path, out FileSystemInfo result) {
                PlatformAdaptationLayer pal = context.DomainManager.Platform;
                result = null;
                if (pal.FileExists(path)) {
                    result = new FileInfo(path);                    
                } else if (pal.DirectoryExists(path)) {
                    result = new DirectoryInfo(path);                    
                } else if (path.ToUpper().Equals(NUL_VALUE)) {
                    result = null;
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
                return TimeOps.CompareTo(self.LastWriteTime, other.LastWriteTime);
            }

            [RubyMethod("<=>")]
            public static object Compare(FileSystemInfo/*!*/ self, object other) {
                Debug.Assert(other as FileSystemInfo == null);
                return null;
            }

            [RubyMethod("atime")]
            public static DateTime AccessTime(FileSystemInfo/*!*/ self) {
                return self.LastAccessTime;
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
            public static DateTime CreateTime(FileSystemInfo/*!*/ self) {
                return self.CreationTime;
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
                return self.Extension.Equals(".exe", StringComparison.InvariantCulture);
            }

            [RubyMethod("file?")]
            public static bool IsFile(FileSystemInfo/*!*/ self) {
                return (self is FileInfo);
            }

            [RubyMethod("ftype")]
            public static MutableString FileType(FileSystemInfo/*!*/ self) {
                string result = IsFile(self) ? "file" : "directory";
                return MutableString.Create(result);
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
               return MutableString.Create(String.Format(
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
            public static DateTime ModifiedTime(FileSystemInfo/*!*/ self) {
                return self.LastWriteTime;
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
                FileInfo info = (self as FileInfo);
                return (info == null) ? 0 : (int)info.Length;
            }

            [RubyMethod("size?")]
            public static object NullableSize(FileSystemInfo/*!*/ self) {
                FileInfo info = (self as FileInfo);
                if (info == null) {
                    return null;
                }
                return (int)info.Length;
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
                return (Size(self) == 0);
            }
        }

        #endregion
    }
}
