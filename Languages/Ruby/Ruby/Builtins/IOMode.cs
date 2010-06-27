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
    public enum IOMode {
        ReadOnly = 0,
        WriteOnly = 1,
        ReadWrite = 2,
        Closed = 3,
        ReadWriteMask = 3,

        WriteAppends = 0x08,

        CreateIfNotExists = 0x100,
        Truncate = 0x200,
        ErrorIfExists = 0x400,

        PreserveEndOfLines = 0x8000,

        Default = ReadOnly
    }

    public static class IOModeEnum {
        public static bool IsClosed(this IOMode mode) {
            return (mode & IOMode.ReadWriteMask) == IOMode.Closed;
        }

        public static bool CanRead(this IOMode mode) {
            return ((int)mode & 1) == 0;
        }

        public static bool CanWrite(this IOMode mode) {
            return ((int)mode & 1) != (((int)mode >> 1) & 1);
        }

        public static IOMode Close(this IOMode mode) {
            return (mode & ~IOMode.ReadWriteMask) | IOMode.Closed;
        }

        public static IOMode CloseRead(this IOMode mode) {
            return (mode & ~IOMode.ReadWriteMask) | (mode.CanWrite() ? IOMode.WriteOnly : IOMode.Closed);
        }

        public static IOMode CloseWrite(this IOMode mode) {
            return (mode & ~IOMode.ReadWriteMask) | (mode.CanRead() ? IOMode.ReadOnly : IOMode.Closed);
        }

        public static FileAccess ToFileAccess(this IOMode mode) {
            switch (mode & IOMode.ReadWriteMask) {
                case IOMode.WriteOnly: return FileAccess.Write;
                case IOMode.ReadOnly: return FileAccess.Read;
                case IOMode.ReadWrite: return FileAccess.ReadWrite;
                default: throw RubyExceptions.CreateEINVAL("illegal access mode {0}", mode);
            }
        }

        public static IOMode Parse(MutableString mode) {
            return Parse(mode, IOMode.Default);
        }

        public static IOMode Parse(MutableString mode, IOMode defaultMode) {
            return (mode != null) ? IOModeEnum.Parse(mode.ToString()) : defaultMode;
        }

        public static IOMode Parse(string mode) {
            IOMode result = IOMode.Default;
            if (String.IsNullOrEmpty(mode)) {
                return result;
            }

            int i = mode.Length - 1;

            bool plus = (mode[i] == '+');
            if (plus) {
                i--;
            }

            if (i < 0) {
                throw IllegalMode(mode);
            }

            if (mode[i] == 'b') {
                result |= IOMode.PreserveEndOfLines;
                i--;
            }

            if (i != 0) {
                throw IllegalMode(mode);
            }

            switch (mode[0]) {
                case 'r':
                    return result | (plus ? IOMode.ReadWrite : IOMode.ReadOnly);

                case 'w':
                    return result | (plus ? IOMode.ReadWrite : IOMode.WriteOnly) | IOMode.Truncate | IOMode.CreateIfNotExists;

                case 'a':
                    return result | (plus ? IOMode.ReadWrite : IOMode.WriteOnly) | IOMode.WriteAppends | IOMode.CreateIfNotExists;

                default:
                    throw IllegalMode(mode);
            }
        }

        private static Exception/*!*/ IllegalMode(string/*!*/ modeString) {
            return RubyExceptions.CreateArgumentError("illegal access mode {0}", modeString);
        }
    }
}
