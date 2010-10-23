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
using IronRuby.Runtime;
using System.Collections.Generic;
using System.Reflection;

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

    public struct IOInfo {
        private readonly IOMode? _mode;
        private readonly RubyEncoding _externalEncoding;
        private readonly RubyEncoding _internalEncoding;

        public IOMode Mode { get { return _mode ?? IOMode.Default; } }
        public RubyEncoding ExternalEncoding { get { return _externalEncoding; } }
        public RubyEncoding InternalEncoding { get { return _internalEncoding; } }
        public bool HasEncoding { get { return _externalEncoding != null; } }

        public IOInfo(IOMode mode) 
            : this(mode, null, null) {
        }

        public IOInfo(IOMode? mode, RubyEncoding externalEncoding, RubyEncoding internalEncoding) {
            _mode = mode;
            _externalEncoding = externalEncoding;
            _internalEncoding = internalEncoding;
        }

        public static IOInfo Parse(RubyContext/*!*/ context, MutableString/*!*/ modeAndEncoding) {
            if (!modeAndEncoding.IsAscii()) {
                throw IOModeEnum.IllegalMode(modeAndEncoding.ToAsciiString());
            }

            string[] parts = modeAndEncoding.ToString().Split(':');
            return new IOInfo(
                IOModeEnum.Parse(parts[0]),
                (parts.Length > 1) ? TryParseEncoding(context, parts[1]) : null,
                (parts.Length > 2) ? TryParseEncoding(context, parts[2]) : null
            );
        }

        public IOInfo AddModeAndEncoding(RubyContext/*!*/ context, MutableString/*!*/ modeAndEncoding) {
            IOInfo info = Parse(context, modeAndEncoding);
            if (_mode.HasValue) {
                throw RubyExceptions.CreateArgumentError("mode specified twice");
            }

            if (!HasEncoding) {
                return info;
            }

            if (!info.HasEncoding) {
                return new IOInfo(info.Mode, _externalEncoding, _internalEncoding);
            }

            throw RubyExceptions.CreateArgumentError("encoding specified twice");
        }

        public IOInfo AddEncoding(RubyContext/*!*/ context, MutableString/*!*/ encoding) {
            if (!encoding.IsAscii()) {
                context.ReportWarning(String.Format("Unsupported encoding {0} ignored", encoding.ToAsciiString()));
                return this;
            }

            if (HasEncoding) {
                throw RubyExceptions.CreateArgumentError("encoding specified twice");
            }

            string[] parts = encoding.ToString().Split(':');
            return new IOInfo(
                _mode,
                TryParseEncoding(context, parts[0]),
                (parts.Length > 1) ? TryParseEncoding(context, parts[1]) : null
            );
        }

        public static RubyEncoding TryParseEncoding(RubyContext/*!*/ context, string/*!*/ str) {
            try {
                return context.GetRubyEncoding(str);
            } catch (ArgumentException) {
                context.ReportWarning(String.Format("Unsupported encoding {0} ignored", str));
                return null;
            }
        }

        public IOInfo AddOptions(ConversionStorage<MutableString>/*!*/ toStr, IDictionary<object, object> options) {
            var context = toStr.Context;

            IOInfo result = this;
            object optionValue;
            if (options.TryGetValue(context.CreateAsciiSymbol("encoding"), out optionValue)) {
                result = result.AddEncoding(context, Protocols.CastToString(toStr, optionValue));
            }

            if (options.TryGetValue(context.CreateAsciiSymbol("mode"), out optionValue)) {
                result = result.AddModeAndEncoding(context, Protocols.CastToString(toStr, optionValue));
            }

            return result;
        }
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
            if (String.IsNullOrEmpty(mode)) {
                throw IllegalMode(mode);
            }

            IOMode result = IOMode.Default;
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

        internal static Exception/*!*/ IllegalMode(string/*!*/ modeString) {
            return RubyExceptions.CreateArgumentError("illegal access mode {0}", modeString);
        }
    }
}
