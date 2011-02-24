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
#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Text;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting;
using System.Globalization;

namespace IronRuby.Builtins {
    [RubyClass("Encoding", Extends = typeof(RubyEncoding), Inherits = typeof(Object), BuildConfig = "!SILVERLIGHT")]
    public static class RubyEncodingOps {
        #region Exceptions

        [RubyException("CompatibilityError", Extends = typeof(EncodingCompatibilityError))]
        public static class CompatibilityErrorOps {
        }

        [RubyException("UndefinedConversionError", Extends = typeof(UndefinedConversionError))]
        public static class UndefinedConversionErrorOps {
        }

        [RubyException("InvalidByteSequenceError", Extends = typeof(InvalidByteSequenceError))]
        public static class InvalidByteSequenceErrorOps {
        }

        [RubyException("ConverterNotFoundError", Extends = typeof(ConverterNotFoundError))]
        public static class ConverterNotFoundErrorOps {
        }

        #endregion

        #region Constants

        [RubyConstant("ANSI_X3_4_1968")]
        [RubyConstant("US_ASCII")]
        [RubyConstant("ASCII")]
        public static readonly RubyEncoding US_ASCII = RubyEncoding.Ascii; 
    
        [RubyConstant]
        public static readonly RubyEncoding UTF_8 = RubyEncoding.UTF8;

        [RubyConstant]
        public static readonly RubyEncoding ASCII_8BIT = RubyEncoding.Binary;

        [RubyConstant]
        public static readonly RubyEncoding BINARY = RubyEncoding.Binary;

        [RubyConstant("SHIFT_JIS")]
        [RubyConstant("Shift_JIS")]
        public static readonly RubyEncoding SHIFT_JIS = RubyEncoding.SJIS;
        
        [RubyConstant]
        public static readonly RubyEncoding EUC_JP = RubyEncoding.EUCJP;

        [RubyConstant]
        public static readonly RubyEncoding KOI8_R = RubyEncoding.GetRubyEncoding(20866);

        [RubyConstant]
        public static readonly RubyEncoding TIS_620 = RubyEncoding.GetRubyEncoding(874);

        [RubyConstant("ISO8859_9")]
        [RubyConstant("ISO_8859_9")]
        public static readonly RubyEncoding ISO_8859_9 = RubyEncoding.GetRubyEncoding(28599);

        [RubyConstant("ISO8859_15")]
        [RubyConstant("ISO_8859_15")]
        public static readonly RubyEncoding ISO_8859_15 = RubyEncoding.GetRubyEncoding(28605);

        [RubyConstant("Big5")]
        [RubyConstant("BIG5")]
        public static readonly RubyEncoding Big5 = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageBig5);

        // TODO:
        // ...

        // TODO: lazy encoding load?

        [RubyConstant]
        public static readonly RubyEncoding UTF_7 = RubyEncoding.GetRubyEncoding(Encoding.UTF7);

        [RubyConstant]
        public static readonly RubyEncoding UTF_16BE = RubyEncoding.GetRubyEncoding(Encoding.BigEndianUnicode);

        [RubyConstant]
        public static readonly RubyEncoding UTF_16LE = RubyEncoding.GetRubyEncoding(Encoding.Unicode);

        [RubyConstant]
        public static readonly RubyEncoding UTF_32BE = RubyEncoding.GetRubyEncoding(RubyEncoding.CodePageUTF32BE);

        [RubyConstant]
        public static readonly RubyEncoding UTF_32LE = RubyEncoding.GetRubyEncoding(Encoding.UTF32);

        #endregion

        #region to_s, inspect, based_encoding, dummy?, ascii_compatible?

        [RubyMethod("name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyEncoding/*!*/ self) {
            return MutableString.CreateAscii(self.Name);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubyEncoding/*!*/ self) {
            // TODO: to_s overridden
            MutableString result = MutableString.CreateMutable(context.GetIdentifierEncoding());
            result.Append("#<");
            result.Append(context.GetClassDisplayName(self));
            result.Append(':');
            result.Append(self.Name);
            result.Append('>');
            return result;
        }

        [RubyMethod("based_encoding")]
        public static RubyEncoding BasedEncoding(RubyEncoding/*!*/ self) {
            return null;
        }

        [RubyMethod("dummy?")]
        public static bool IsDummy(RubyEncoding/*!*/ self) {
            return false;
        }

        [RubyMethod("ascii_compatible?")]
        public static bool IsAsciiCompatible(RubyEncoding/*!*/ self) {
            return self.IsAsciiIdentity;
        }

        // TODO:
        // Method "replicate". This would need a change in implementation of RubyEncoding - encodings are singletons now.
        // What properties are preserved during replication?

        [RubyMethod("names")]
        public static RubyArray/*!*/ GetAllNames(RubyContext/*!*/ context, RubyEncoding/*!*/ self) {
            var result = new RubyArray();
            
            string name = self.Name;
            result.Add(MutableString.Create(name));
            
            foreach (var alias in RubyEncoding.Aliases) {
                if (StringComparer.OrdinalIgnoreCase.Equals(alias.Value, name)) {
                    result.Add(MutableString.CreateAscii(alias.Key));
                }
            }

            if (self == context.RubyOptions.LocaleEncoding) {
                result.Add(MutableString.CreateAscii("locale"));
            }

            if (self == context.DefaultExternalEncoding) {
                result.Add(MutableString.CreateAscii("external"));
            }

            if (self == context.GetPathEncoding()) {
                result.Add(MutableString.CreateAscii("filesystem"));
            }

            return result;
        }

        #endregion

        #region aliases, name_list, list, find

        [RubyMethod("aliases", RubyMethodAttributes.PublicSingleton)]
        public static Hash/*!*/ GetAliases(RubyClass/*!*/ self) {
            var context = self.Context;
            var result = new Hash(context.EqualityComparer, RubyEncoding.Aliases.Count + 3);
            foreach (var alias in RubyEncoding.Aliases) {
                result.Add(MutableString.CreateAscii(alias.Key).Freeze(), MutableString.CreateAscii(alias.Value).Freeze());
            }

            result.Add(MutableString.CreateAscii("locale").Freeze(), MutableString.Create(context.RubyOptions.LocaleEncoding.Name).Freeze());
            result.Add(MutableString.CreateAscii("external").Freeze(), MutableString.Create(context.DefaultExternalEncoding.Name).Freeze());
            result.Add(MutableString.CreateAscii("filesystem").Freeze(), MutableString.Create(context.GetPathEncoding().Name).Freeze());
            return result;
        }

        [RubyMethod("name_list", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetNameList(RubyClass/*!*/ self) {
            var infos = Encoding.GetEncodings();
            var result = new RubyArray(1 + infos.Length);

            // Ruby specific:
            result.Add(MutableString.CreateAscii(RubyEncoding.Binary.Name));

            foreach (var info in infos) {
                result.Add(MutableString.Create(RubyEncoding.GetRubySpecificName(info.CodePage) ?? info.Name));
            }

            foreach (var alias in RubyEncoding.Aliases.Keys) {
                result.Add(MutableString.CreateAscii(alias));
            }

            result.Add(MutableString.CreateAscii("locale"));
            result.Add(MutableString.CreateAscii("external"));
            result.Add(MutableString.CreateAscii("filesystem"));
            
            return result;
        }

        [RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetAvailableEncodings(RubyClass/*!*/ self) {
            var infos = Encoding.GetEncodings();
            var result = new RubyArray(1 + infos.Length);

            // Ruby specific:
            result.Add(RubyEncoding.Binary);

            foreach (var info in infos) {
                result.Add(RubyEncoding.GetRubyEncoding(info.CodePage));
            }
            return result;
        }

        [RubyMethod("find", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ GetEncoding(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ name) {
            return self.Context.GetRubyEncoding(name);
        }

        #endregion

        #region compatible?

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ str1, [NotNull]MutableString/*!*/ str2) {
            return str1.GetCompatibleEncoding(str2);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubyEncoding/*!*/ encoding1, [NotNull]RubyEncoding/*!*/ encoding2) {
            return MutableString.GetCompatibleEncoding(encoding1, encoding2);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubyEncoding/*!*/ encoding, [NotNull]MutableString/*!*/ str) {
            return str.GetCompatibleEncoding(encoding);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ str, [NotNull]RubyEncoding/*!*/ encoding) {
            return str.GetCompatibleEncoding(encoding);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubyEncoding/*!*/ encoding, [NotNull]RubySymbol/*!*/ symbol) {
            return GetCompatible(self, encoding, symbol.String);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ str, [NotNull]RubySymbol/*!*/ symbol) {
            return GetCompatible(self, str, symbol.String);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubySymbol/*!*/ symbol, [NotNull]RubyEncoding/*!*/ encoding) {
            return GetCompatible(self, symbol.String, encoding);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubySymbol/*!*/ symbol, [NotNull]MutableString/*!*/ str) {
            return GetCompatible(self, symbol.String, str);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, [NotNull]RubySymbol/*!*/ encoding1, [NotNull]RubySymbol/*!*/ encoding2) {
            return GetCompatible(self, encoding1.String, encoding2.String);
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding GetCompatible(RubyClass/*!*/ self, object obj1, object obj2) {
            return null;
        }

        #endregion

        #region default_external, default_internal, locale_charmap

        [RubyMethod("default_external", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ GetDefaultExternalEncoding(RubyClass/*!*/ self) {
            return self.Context.DefaultExternalEncoding;
        }

        [RubyMethod("default_external=", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ SetDefaultExternalEncoding(RubyClass/*!*/ self, RubyEncoding encoding) {
            if (encoding == null) {
                throw RubyExceptions.CreateArgumentError("default external can not be nil");
            }
            var old = self.Context.DefaultExternalEncoding;
            self.Context.DefaultExternalEncoding = encoding;
            return old;
        }

        [RubyMethod("default_external=", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ SetDefaultExternalEncoding(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ encodingName) {
            return SetDefaultExternalEncoding(self, self.Context.GetRubyEncoding(encodingName));
        }

        [RubyMethod("default_internal", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ GetDefaultInternalEncoding(RubyClass/*!*/ self) {
            return self.Context.DefaultInternalEncoding;
        }

        [RubyMethod("default_internal=", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ SetDefaultInternalEncoding(RubyClass/*!*/ self, RubyEncoding encoding) {
            var old = self.Context.DefaultInternalEncoding;
            self.Context.DefaultInternalEncoding = encoding;
            return old;
        }

        [RubyMethod("default_internal=", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ SetDefaultInternalEncoding(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ encodingName) {
            return SetDefaultInternalEncoding(self, self.Context.GetRubyEncoding(encodingName));
        }

        [RubyMethod("locale_charmap", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetDefaultCharmap(RubyClass/*!*/ self) {
            return MutableString.Create(self.Context.RubyOptions.LocaleEncoding.Name);
        }

        #endregion
    }
}
#endif