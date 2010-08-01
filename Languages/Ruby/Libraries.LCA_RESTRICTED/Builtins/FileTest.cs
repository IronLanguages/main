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

using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

#if CLR2
using Microsoft.Scripting.Utils;
using System;
#else
using System;
#endif

namespace IronRuby.Builtins {
    // TODO: conversion: to_io, to_path, to_str

    [RubyModule("FileTest")]
    public static class FileTest {
        [RubyMethod("blockdev?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("blockdev?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsBlockDevice(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsBlockDevice(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("chardev?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("chardev?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsCharDevice(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsCharDevice(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("directory?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("directory?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsDirectory(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return DirectoryExists(self.Context, Protocols.CastToPath(toPath, path));
        }

        [RubyMethod("executable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("executable?", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("executable_real?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("executable_real?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsExecutable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => RubyFileOps.RubyStatOps.IsExecutable(fsi));
        }

        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exist?", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PrivateInstance)]
        public static bool Exists(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            var p = Protocols.CastToPath(toPath, path);
            return FileExists(self.Context, p) || DirectoryExists(self.Context, p);
        }

        [RubyMethod("file?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("file?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsFile(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return FileExists(self.Context, Protocols.CastToPath(toPath, path));
        }

        [RubyMethod("grpowned?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("grpowned?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsGroupOwned(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsGroupOwned(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("identical?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("identical?", RubyMethodAttributes.PrivateInstance)]
        public static bool AreIdentical(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path1, object path2) {
            FileSystemInfo info1, info2;

            return RubyFileOps.RubyStatOps.TryCreate(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path1)), out info1)
                && RubyFileOps.RubyStatOps.TryCreate(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path2)), out info2)
                && RubyFileOps.RubyStatOps.AreIdentical(self.Context, info1, info2);
        }

        [RubyMethod("owned?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("owned?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsUserOwned(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsUserOwned(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("pipe?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("pipe?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsPipe(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsPipe(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("readable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("readable?", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("readable_real?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("readable_real?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsReadable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => { 
                return RubyFileOps.RubyStatOps.IsReadable(fsi); });
        }

        [RubyMethod("setgid?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("setgid?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsSetGid(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsSetGid(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("setuid?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("setuid?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsSetUid(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsSetUid(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("size", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("size", RubyMethodAttributes.PrivateInstance)]
        public static int Size(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.Size(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("size?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("size?", RubyMethodAttributes.PrivateInstance)]
        public static object NullableSize(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            FileSystemInfo fsi;
            if (RubyFileOps.RubyStatOps.TryCreate(self.Context, Protocols.CastToPath(toPath, path).ConvertToString(), out fsi)) {
                return RubyFileOps.RubyStatOps.NullableSize(fsi);
            } else {
                return null;
            }
        }

        [RubyMethod("socket?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("socket?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsSocket(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsSocket(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

        [RubyMethod("sticky?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("sticky?", RubyMethodAttributes.PrivateInstance)]
        public static object IsSticky(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsSticky(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }

#if !SILVERLIGHT
        [RubyMethod("symlink?", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
        [RubyMethod("symlink?", RubyMethodAttributes.PrivateInstance, BuildConfig = "!SILVERLIGHT")]
        public static bool IsSymLink(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RubyFileOps.RubyStatOps.IsSymLink(RubyFileOps.RubyStatOps.Create(self.Context, Protocols.CastToPath(toPath, path)));
        }
#endif

        [RubyMethod("writable?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("writable?", RubyMethodAttributes.PrivateInstance)]
        [RubyMethod("writable_real?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("writable_real?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsWritable(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            return RunIfFileExists(self.Context, Protocols.CastToPath(toPath, path), (FileSystemInfo fsi) => { 
                return RubyFileOps.RubyStatOps.IsWritable(fsi); });
        }

        [RubyMethod("zero?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("zero?", RubyMethodAttributes.PrivateInstance)]
        public static bool IsZeroLength(ConversionStorage<MutableString>/*!*/ toPath, RubyModule/*!*/ self, object path) {
            string strPath = self.Context.DecodePath(Protocols.CastToPath(toPath, path));

            // NUL/nul is a special-cased filename on Windows
            if (strPath.ToUpperInvariant() == "NUL") {
                return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, strPath));
            }

            if (self.Context.Platform.DirectoryExists(strPath) || !self.Context.Platform.FileExists(strPath)) {
                return false;
            }

            return RubyFileOps.RubyStatOps.IsZeroLength(RubyFileOps.RubyStatOps.Create(self.Context, strPath));
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
