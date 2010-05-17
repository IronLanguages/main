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

using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

#if CLR2
using Microsoft.Scripting.Utils;
#else
using System;
#endif

namespace IronRuby.Builtins {
    [RubyModule("FileTest")]
    public static class FileTest {
        [RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsBlockDevice(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsBlockDevice(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsCharDevice(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsCharDevice(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsDirectory(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.DirectoryExists(self.Context, path);
        }

        [RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsExecutable(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RunIfFileExists(self.Context, path, (FileSystemInfo fsi) => { 
                return RubyFileOps.RubyStatOps.IsExecutable(fsi); });
        }

        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        public static bool Exists(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.FileExists(self.Context, path) || RubyFileOps.DirectoryExists(self.Context, path);
        }

        [RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsFile(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.FileExists(self.Context, path);
        }

        [RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsGroupOwned(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsGroupOwned(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        //identical?

        [RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsUserOwned(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsUserOwned(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsPipe(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsPipe(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsReadable(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RunIfFileExists(self.Context, path, (FileSystemInfo fsi) => { 
                return RubyFileOps.RubyStatOps.IsReadable(fsi); });
        }

        [RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetGid(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsSetGid(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSetUid(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsSetUid(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
        public static int Size(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.Size(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
        public static object NullableSize(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            FileSystemInfo fsi;
            if (RubyFileOps.RubyStatOps.TryCreate(self.Context, path.ConvertToString(), out fsi)) {
                return RubyFileOps.RubyStatOps.NullableSize(fsi);
            } else {
                return null;
            }
        }

        [RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSocket(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsSocket(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

        [RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsSticky(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsSticky(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }

#if !SILVERLIGHT
        [RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        public static bool IsSymLink(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.RubyStatOps.IsSymLink(RubyFileOps.RubyStatOps.Create(self.Context, path));
        }
#endif

        [RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsWritable(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RunIfFileExists(self.Context, path, (FileSystemInfo fsi) => { 
                return RubyFileOps.RubyStatOps.IsWritable(fsi); });
        }

        [RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsZeroLength(RubyModule/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            string strPath = self.Context.DecodePath(path);

            // NUL/nul is a special-cased filename on Windows
            if (strPath.ToUpperInvariant() == "NUL") {
                return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, strPath));
            }

            if (self.Context.Platform.DirectoryExists(strPath) || !self.Context.Platform.FileExists(strPath)) {
                return false;
            }

            return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, strPath));
        }

        private static bool RunIfFileExists(RubyContext/*!*/ context, MutableString/*!*/ path, Func<FileSystemInfo, bool> del) {
            return RunIfFileExists(context, path.ConvertToString(), del);
        }

        private static bool RunIfFileExists(RubyContext/*!*/ context, string/*!*/ path, Func<FileSystemInfo, bool> del) {
            FileSystemInfo fsi;
            if (RubyFileOps.RubyStatOps.TryCreate(context, path, out fsi)) {
                return del(fsi);
            } else {
                return false;
            }
        }
    }
}
