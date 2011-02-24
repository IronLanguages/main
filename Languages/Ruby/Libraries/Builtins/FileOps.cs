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
using IronRuby.Runtime.Conversions;
using System.Runtime.CompilerServices;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins {

    /// <summary>
    /// File builtin class. Derives from IO
    /// </summary>
    [RubyClass("File", Extends = typeof(RubyFile))]
    public static class RubyFileOps {

        #region Construction

        [RubyConstructor]
        public static RubyFile/*!*/ CreateFile(
            ConversionStorage<int?>/*!*/ toInt,
            ConversionStorage<IDictionary<object, object>>/*!*/ toHash,
            ConversionStorage<MutableString>/*!*/ toPath,
            ConversionStorage<MutableString>/*!*/ toStr,
            RubyClass/*!*/ self,
            object descriptorOrPath, 
            [Optional]object optionsOrMode, 
            [Optional]object optionsOrPermissions,
            [DefaultParameterValue(null), DefaultProtocol]IDictionary<object, object> options) {

            return Reinitialize(toInt, toHash, toPath, toStr, new RubyFile(self.Context), descriptorOrPath, optionsOrMode, optionsOrPermissions, options);
        }

        [RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
        public static RubyFile/*!*/ Reinitialize(
            ConversionStorage<int?>/*!*/ toInt,
            ConversionStorage<IDictionary<object, object>>/*!*/ toHash,
            ConversionStorage<MutableString>/*!*/ toPath,
            ConversionStorage<MutableString>/*!*/ toStr,
            RubyFile/*!*/ self,
            object descriptorOrPath, 
            [Optional]object optionsOrMode, 
            [Optional]object optionsOrPermissions,
            [DefaultParameterValue(null), DefaultProtocol]IDictionary<object, object> options) {

            var context = self.Context;
            
            Protocols.TryConvertToOptions(toHash, ref options, ref optionsOrMode, ref optionsOrPermissions);
            var toIntSite = toInt.GetSite(TryConvertToFixnumAction.Make(toInt.Context));

            IOInfo info = new IOInfo();
            if (optionsOrMode != Missing.Value) {
                int? m = toIntSite.Target(toIntSite, optionsOrMode);
                info = m.HasValue ? new IOInfo((IOMode)m) : IOInfo.Parse(context, Protocols.CastToString(toStr, optionsOrMode));
            }

            int permissions = 0;
            if (optionsOrPermissions != Missing.Value) {
                int? p = toIntSite.Target(toIntSite, optionsOrPermissions);
                if (!p.HasValue) {
                    throw RubyExceptions.CreateTypeConversionError(context.GetClassName(optionsOrPermissions), "Integer");
                }
                permissions = p.Value;
            }

            if (options != null) {
                info = info.AddOptions(toStr, options);
            }

            // TODO: permissions
            
            // descriptor or path:
            int? descriptor = toIntSite.Target(toIntSite, descriptorOrPath);
            if (descriptor.HasValue) {
                RubyIOOps.Reinitialize(self, descriptor.Value, info);
            } else {
                Reinitialize(self, Protocols.CastToPath(toPath, descriptorOrPath), info, permissions);
            }

            return self;
        }

        private static void Reinitialize(RubyFile/*!*/ file, MutableString/*!*/ path, IOInfo info, int permission) {
            var strPath = file.Context.DecodePath(path);
            var stream = RubyFile.OpenFileStream(file.Context, strPath, info.Mode);

            file.Path = strPath;
            file.Mode = info.Mode;
            file.SetStream(stream);
            file.SetFileDescriptor(file.Context.AllocateFileDescriptor(stream));

            if (info.HasEncoding) {
                file.ExternalEncoding = info.ExternalEncoding;
                file.InternalEncoding = info.InternalEncoding;
            }
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

        [RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
        public static RuleGenerator/*!*/ Open() {
            return RubyIOOps.Open();
        }

        #region chmod, chown, lchmod, lchown, umask

        [RubyMethod("chmod")]
        public static int Chmod(RubyFile/*!*/ self, [DefaultProtocol]int permission) {
            self.RequireInitialized();
            // TODO:
            if (self.Path == null) {
                throw new NotSupportedException("TODO: cannot chmod for files without path");
            }
            Chmod(self.Path, permission);
            return 0;
        }

        [RubyMethod("chmod", RubyMethodAttributes.PublicSingleton)]
        public static int Chmod(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, [DefaultProtocol]int permission, object path) {
            Chmod(self.Context.DecodePath(Protocols.CastToPath(toPath, path)), permission);
            return 1;
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

        //lchmod
        //lchown

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
        
        #endregion

        #region delete, unlink, truncate, rename

        [RubyMethod("delete", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("unlink", RubyMethodAttributes.PublicSingleton)]
        public static int Delete(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            string strPath = self.Context.DecodePath(Protocols.CastToPath(toPath, path));
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
        public static int Delete(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, params object[] paths) {
            foreach (MutableString path in paths) {
                Delete(toPath, self, path);
            }

            return paths.Length;
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
        public static int Truncate(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path, [DefaultProtocol]int size) {
            if (size < 0) {
                throw new InvalidError();
            }
            using (RubyFile f = new RubyFile(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path)), IOMode.ReadWrite)) {
                f.Length = size;
            }
            return 0;
        }
#endif

        [RubyMethod("rename", RubyMethodAttributes.PublicSingleton)]
        public static int Rename(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object oldPath, object newPath) {
            var context = self.Context;

            string strOldPath = context.DecodePath(Protocols.CastToPath(toPath, oldPath));
            string strNewPath = context.DecodePath(Protocols.CastToPath(toPath, newPath));

            if (strOldPath.Length == 0 || strNewPath.Length == 0) {
                throw RubyExceptions.CreateENOENT();
            }

            if (!context.Platform.FileExists(strOldPath) && !context.Platform.DirectoryExists(strOldPath)) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", oldPath);
            }

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

        #endregion

        #region path, basename, dirname, extname, expand_path, absolute_path, fnmatch

        [RubyMethod("path", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ ToPath(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return Protocols.CastToPath(toPath, path);
        }

        [RubyMethod("basename", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ BaseName(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self,
            object path, [DefaultProtocol, NotNull, Optional]MutableString suffix) {
            return BaseName(Protocols.CastToPath(toPath, path), suffix);
        }

        private static MutableString/*!*/ BaseName(MutableString/*!*/ path, MutableString suffix) {
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

        [RubyMethod("dirname", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ DirName(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return DirName(Protocols.CastToPath(toPath, path));
        }

        private static MutableString/*!*/ DirName(MutableString/*!*/ path) {
            string strPath = path.ConvertToString();
            string directoryName = strPath;

            if (IsValidPath(strPath)) {
                strPath = StripPathCharacters(strPath);

                // handle top-level UNC paths
                directoryName = Path.GetDirectoryName(strPath);
                if (directoryName == null) {
                    return MutableString.CreateMutable(strPath, path.Encoding);
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
            return MutableString.CreateMutable(directoryName, path.Encoding);
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
        public static MutableString/*!*/ GetExtension(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            MutableString pathStr = Protocols.CastToPath(toPath, path);
            return MutableString.Create(RubyUtils.GetExtension(pathStr.ConvertToString()), pathStr.Encoding).TaintBy(pathStr);
        }

        [RubyMethod("expand_path", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ ExpandPath(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path,
            [DefaultParameterValue(null)]object basePath) {
            var context = self.Context;

            string result = RubyUtils.ExpandPath(
                context.Platform,
                context.DecodePath(Protocols.CastToPath(toPath, path)),
                (basePath == null) ? context.Platform.CurrentDirectory : context.DecodePath(Protocols.CastToPath(toPath, basePath)),
                true
            );

            return self.Context.EncodePath(result);
        }

        [RubyMethod("absolute_path", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ AbsolutePath(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path,
            [DefaultParameterValue(null)]object basePath) {
            var context = self.Context;

            string result = RubyUtils.ExpandPath(
                context.Platform,
                context.DecodePath(Protocols.CastToPath(toPath, path)),
                (basePath == null) ? context.Platform.CurrentDirectory : context.DecodePath(Protocols.CastToPath(toPath, basePath)),
                false
            );

            return self.Context.EncodePath(result);
        }

        [RubyMethod("fnmatch", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("fnmatch?", RubyMethodAttributes.PublicSingleton)]
        public static bool FnMatch(ConversionStorage<MutableString>/*!*/ toPath, object/*!*/ self,
            [DefaultProtocol, NotNull]MutableString/*!*/ pattern, object path, [Optional]int flags) {

            return Glob.FnMatch(pattern.ConvertToString(), Protocols.CastToPath(toPath, path).ConvertToString(), flags);
        }

        #endregion

        #region split, join

        [RubyMethod("split", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray Split(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            MutableString p = Protocols.CastToPath(toPath, path);
            RubyArray result = new RubyArray(2);
            result.Add(DirName(p));
            result.Add(BaseName(p, null));
            return result;
        }

        [RubyMethod("join", RubyMethodAttributes.PublicSingleton)]
        public static MutableString Join(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, params object[]/*!*/ parts) {
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
                            visitedLists = new Dictionary<object, bool>(ReferenceEqualityComparer<object>.Instance);
                        }
                        visitedLists.Add(list, true);
                        Push(worklist, list);
                        continue;
                    }
                } else if (part == null) {
                    throw RubyExceptions.CreateTypeConversionError("NilClass", "String");
                } else {
                    str = Protocols.CastToPath(toPath, part);
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

        #region flock, readlink, link, symlink

#if !SILVERLIGHT
        //flock

        [RubyMethod("readlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static bool Readlink(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            throw new IronRuby.Builtins.NotImplementedError("readlink() function is unimplemented on this machine");
        }

        [RubyMethod("link", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int Link(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object oldPath, object newPath) {
            Protocols.CastToPath(toPath, oldPath);
            Protocols.CastToPath(toPath, newPath);
            throw new IronRuby.Builtins.NotImplementedError("link not implemented");
        }

        [RubyMethod("symlink", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static object SymLink(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            throw new NotImplementedError("symlnk() function is unimplemented on this machine");
        }
#endif
        #endregion

        #region atime, ctime, mtime, utime

        [RubyMethod("atime")]
        public static RubyTime AccessTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(self));
        }
        
        [RubyMethod("atime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime AccessTime(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return RubyStatOps.AccessTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("ctime")]
        public static RubyTime CreateTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(self));
        }

        [RubyMethod("ctime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime CreateTime(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return RubyStatOps.CreateTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("mtime")]
        public static RubyTime ModifiedTime(RubyContext/*!*/ context, RubyFile/*!*/ self) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(self));
        }

        [RubyMethod("mtime", RubyMethodAttributes.PublicSingleton)]
        public static RubyTime ModifiedTime(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return RubyStatOps.ModifiedTime(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

#if !SILVERLIGHT
        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static int UpdateTimes(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, [NotNull]RubyTime/*!*/ accessTime, [NotNull]RubyTime/*!*/ modifiedTime,
            object path) {

            string strPath = self.Context.DecodePath(Protocols.CastToPath(toPath, path));
            FileInfo info = new FileInfo(strPath);
            if (!info.Exists) {
                throw RubyExceptions.CreateENOENT("No such file or directory - {0}", strPath);
            }
            info.LastAccessTimeUtc = accessTime.ToUniversalTime();
            info.LastWriteTimeUtc = modifiedTime.ToUniversalTime();
            return 1;
        }

        [RubyMethod("utime", RubyMethodAttributes.PublicSingleton)]
        public static int UpdateTimes(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object accessTime, object modifiedTime,
            params object[]/*!*/ paths) {

            RubyTime atime = MakeTime(self.Context, accessTime);
            RubyTime mtime = MakeTime(self.Context, modifiedTime);

            foreach (MutableString path in paths) {
                UpdateTimes(toPath, self, atime, mtime, path);
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

        #region ftype, stat, inspect, path, to_path

        [RubyMethod("ftype", RubyMethodAttributes.PublicSingleton)]
        public static MutableString FileType(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return RubyStatOps.FileType(RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("lstat", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("stat", RubyMethodAttributes.PublicSingleton)]
        public static FileSystemInfo/*!*/ Stat(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
            return RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path));
        }

        [RubyMethod("lstat")]
        [RubyMethod("stat")]
        public static FileSystemInfo Stat(RubyFile/*!*/ self) {
            return RubyStatOps.Create(self);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyFile/*!*/ self) {
            return MutableString.CreateMutable(self.Context.GetPathEncoding()).
                Append("#<").
                Append(self.Context.GetClassOf(self).GetName(self.Context)).
                Append(':').
                Append(self.Path).
                Append(self.Closed ? " (closed)" : "").
                Append('>');
        }

        [RubyMethod("path")]
        [RubyMethod("to_path")]
        public static MutableString GetPath(RubyFile/*!*/ self) {
            self.RequireInitialized();
            return self.Path != null ? self.Context.EncodePath(self.Path) : null;
        }

        #endregion

        #region File::Stat

        /// <summary>
        /// Stat
        /// </summary>
        [RubyClass("Stat", Extends = typeof(FileSystemInfo), Inherits = typeof(object), BuildConfig = "!SILVERLIGHT"), Includes(typeof(Comparable))]
        public class RubyStatOps {

            // TODO: should work for IO and files w/o paths:
            internal static FileSystemInfo/*!*/ Create(RubyFile/*!*/ file) {
                file.RequireInitialized();
                if (file.Path == null) {
                    throw new NotSupportedException("TODO: cannot get file info for files without path");
                }
                return Create(file.Context, file.Path);
            }

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
#if !SILVERLIGHT
                } else if (path.ToUpperInvariant().Equals(NUL_VALUE)) {
                    result = new DeviceInfo(NUL_VALUE);
#endif
                } else {
                    return false;
                }
                return true;
            }


            [RubyConstructor]
            public static FileSystemInfo/*!*/ Create(ConversionStorage<MutableString>/*!*/ toPath, RubyClass/*!*/ self, object path) {
                return Create(self.Context, Protocols.CastToPath(toPath, path));
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

            [RubyMethod("identical?")]
            public static bool AreIdentical(RubyContext/*!*/ context, FileSystemInfo/*!*/ self, [NotNull]FileSystemInfo/*!*/ other) {
                // TODO: links
                return self.Exists && other.Exists && context.Platform.PathComparer.Compare(self.FullName, other.FullName) == 0;
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
#if !SILVERLIGHT
                if (self is DeviceInfo) {
                    return 0;
                }
#endif

                FileInfo info = (self as FileInfo);
                return (info == null) ? 0 : (int)info.Length;
            }

            [RubyMethod("size?")]
            public static object NullableSize(FileSystemInfo/*!*/ self) {
#if !SILVERLIGHT
                if (self is DeviceInfo) {
                    return 0;
                }
#endif

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
            public static object IsSticky(FileSystemInfo/*!*/ self) {
                return null;
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
#if !SILVERLIGHT
                if (self is DeviceInfo) {
                    return true;
                }
#endif

                FileInfo info = (self as FileInfo);
                return (info == null) ? false : info.Length == 0;
            }

#if !SILVERLIGHT
            // cannot inherit from FileSystemInfo in Silverlight because the
            // constructor is SecurityCritical
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
#endif
        }
        #endregion

        #region FileTest methods (public singletons only)

        // TODO: conversion: to_io, to_path, to_str

        [RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsBlockDevice(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsBlockDevice(toPath, self, path);
        }

        [RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsCharDevice(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsCharDevice(toPath, self, path);
        }

        [RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsDirectory(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsDirectory(toPath, self, path);
        }

        [RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsExecutable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsExecutable(toPath, self, path);
        }

        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        public static bool Exists(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.Exists(toPath, self, path);
        }

        [RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsFile(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsFile(toPath, self, path);
        }

        [RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsGroupOwned(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsGroupOwned(toPath, self, path);
        }

        [RubyMethod("identical?", RubyMethodAttributes.PublicSingleton)]
        public static bool AreIdentical(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path1, object path2) {
            return FileTest.AreIdentical(toPath, self, path1, path2);
        }

        [RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsUserOwned(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsUserOwned(toPath, self, path);
        }

        [RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsPipe(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsPipe(toPath, self, path);
        }

        [RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsReadable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsReadable(toPath, self, path);
        }

        [RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetGid(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsSetGid(toPath, self, path);
        }

        [RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetUid(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsSetUid(toPath, self, path);
        }

        [RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
        public static int Size(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.Size(toPath, self, path);
        }

        [RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
        public static object NullableSize(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.NullableSize(toPath, self, path);
        }

        [RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSocket(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsSocket(toPath, self, path);
        }

        [RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
        public static object IsSticky(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsSticky(toPath, self, path);
        }

#if !SILVERLIGHT
        [RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static bool IsSymLink(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsSymLink(toPath, self, path);
        }
#endif

        [RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsWritable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsWritable(toPath, self, path);
        }

        [RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsZeroLength(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileTest.IsZeroLength(toPath, self, path);
        }

        #endregion
    }
}
