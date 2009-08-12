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
using System.IO;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    [Flags]
    public enum RubyFileMode : int {
        APPEND   = 0x08,
        BINARY   = 0x8000,
        CREAT    = 0x100,
        EXCL     = 0x400,
        NONBLOCK = 0x01,
        RDONLY   = 0x00,
        RDWR     = 0x02,
        TRUNC    = 0x200,
        WRONLY   = 0x01,
        ReadWriteMask = 0x03,
    }

    public class RubyFile : RubyIO {
        private readonly string/*!*/ _path;

        private static Stream/*!*/ OpenFileStream(RubyContext/*!*/ context, string/*!*/ path, RubyFileMode mode) {
            FileMode fileMode;
            FileAccess access = FileAccess.Read;
            FileShare share = FileShare.ReadWrite;

            RubyFileMode readWriteFlags = mode & RubyFileMode.ReadWriteMask;

            if (readWriteFlags == RubyFileMode.WRONLY) {
                access = FileAccess.Write;
            } else if (readWriteFlags == RubyFileMode.RDONLY) {
                access = FileAccess.Read;
            } else if (readWriteFlags == RubyFileMode.RDWR) {
                access = FileAccess.ReadWrite;
            } else {
                throw RubyExceptions.CreateEINVAL(String.Format("illegal access mode {0}", mode));
            }

            if ((mode & RubyFileMode.APPEND) != 0) {
                fileMode = FileMode.Append;
            } else if ((mode & RubyFileMode.CREAT) != 0) {
                fileMode = FileMode.Create;
            } else if ((mode & RubyFileMode.TRUNC) != 0) {
                fileMode = FileMode.Truncate;
            } else {
                fileMode = FileMode.Open;
            }

            if ((mode & RubyFileMode.EXCL) != 0 && (mode & RubyFileMode.CREAT) != 0 && context.DomainManager.Platform.FileExists(path)) {
                throw RubyExceptions.CreateEEXIST(String.Format("No such file or directory - {0}", path));
            }

            try {
                return context.DomainManager.Platform.OpenInputFileStream(path, fileMode, access, share);
            } catch (DirectoryNotFoundException e) {
                throw RubyExceptions.CreateENOENT(e.Message, e);
            } catch (PathTooLongException e) {
                throw RubyExceptions.CreateENOENT(e.Message, e);
            } catch (ArgumentException e) {
                throw RubyExceptions.CreateEINVAL(e.Message, e);
            }
        }

        private static Stream/*!*/ OpenFileStream(RubyContext/*!*/ context, string/*!*/ path, string/*!*/ modeString) {
            FileMode mode;
            FileAccess access;
            
            // ignore "b":
            bool preserveEndOfLines;

            switch (RubyIO.ParseIOMode(modeString, out preserveEndOfLines)) {
                case IOMode.ReadOnlyFromStart:
                    mode = FileMode.Open; access = FileAccess.Read;
                    break;

                case IOMode.ReadWriteFromStart:
                    mode = FileMode.Open; access = FileAccess.ReadWrite;
                    break;

                case IOMode.WriteOnlyTruncate:
                    mode = FileMode.Create; access = FileAccess.Write;
                    break;

                case IOMode.ReadWriteTruncate:
                    mode = FileMode.Create; access = FileAccess.ReadWrite;
                    break;

                case IOMode.WriteOnlyAppend:
                    mode = FileMode.Append; access = FileAccess.Write;
                    break;

                case IOMode.ReadWriteAppend:
                    mode = FileMode.Append; access = FileAccess.ReadWrite;
                    break;

                default:
                    throw RubyExceptions.CreateEINVAL(String.Format("illegal access mode {0}", modeString));
            }

            try {
                return context.DomainManager.Platform.OpenInputFileStream(path, mode, access, FileShare.ReadWrite);
            } catch (DirectoryNotFoundException e) {
                throw RubyExceptions.CreateENOENT(e.Message, e);
            } catch (PathTooLongException e) {
                throw RubyExceptions.CreateENOENT(e.Message, e);
            } catch (ArgumentException e) {
                throw RubyExceptions.CreateEINVAL(e.Message, e);
            }
        }

        public RubyFile(RubyContext/*!*/ context, string/*!*/ path, string/*!*/ modeString)
            : base(context, OpenFileStream(context, path, modeString), modeString) {
            if (string.IsNullOrEmpty(path)) {
                throw RubyExceptions.CreateEINVAL();
            }

            _path = path;
        }

        public RubyFile(RubyContext/*!*/ context, string/*!*/ path, RubyFileMode mode)
            : base(context, OpenFileStream(context, path, mode), mode) {
            if (string.IsNullOrEmpty(path)) {
                throw RubyExceptions.CreateEINVAL();
            }

            _path = path;
        }

        public string/*!*/ Path {
            get { return _path; }
        }
    }
}