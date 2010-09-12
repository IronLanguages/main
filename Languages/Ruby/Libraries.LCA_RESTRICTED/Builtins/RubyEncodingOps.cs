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

        // TODO: we need to fix class init generator
        //[RubyException("CompatibilityError", Extends = typeof(EncodingCompatibilityError), Inherits = typeof(EncodingError))]
        //public static class CompatibilityErrorOps {
        //}

        // TODO:
        // UndefinedConversionError
        // InvalidByteSequenceError
        // ConverterNotFoundError

        #endregion

        #region Constants

        [RubyConstant]
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
        public static readonly RubyEncoding EUC_JP = RubyEncoding.EUC;

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

        #region to_s, inspect, based_encoding, dummy?

        [RubyMethod("_dump")]
        [RubyMethod("name")]
        [RubyMethod("to_s")]
        public static MutableString/*!*/ ToS(RubyEncoding/*!*/ self) {
            return MutableString.CreateAscii(self.Name.ToUpperInvariant());
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, RubyEncoding/*!*/ self) {
            // TODO: to_s overridden
            MutableString result = MutableString.CreateMutable(context.GetIdentifierEncoding());
            result.Append("#<");
            result.Append(context.GetClassDisplayName(self));
            result.Append(':');
            result.Append(self.Name.ToUpperInvariant());
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

        #endregion

        #region aliases, name_list, list, find, _load?

        [RubyMethod("aliases", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetAliases(RubyClass/*!*/ self) {
            // TODO:
            return new RubyArray();
        }

        [RubyMethod("name_list", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetNameList(RubyClass/*!*/ self) {
            // TODO:
            return new RubyArray();
        }

        [RubyMethod("list", RubyMethodAttributes.PublicSingleton)]
        public static RubyArray/*!*/ GetAvailableEncodings(RubyClass/*!*/ self) {
            // TODO: loads all encodings, we should be lazy with encoding creation

            var infos = Encoding.GetEncodings();
            var result = new RubyArray(1 + infos.Length);

            // Ruby specific:
            result.Add(RubyEncoding.Binary);

            foreach (var info in infos) {
                result.Add(RubyEncoding.GetRubyEncoding(info.GetEncoding()));
            }
            return result;
        }

        [RubyMethod("find", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ GetEncoding(RubyClass/*!*/ self, [NotNull]MutableString/*!*/ name) {
            return self.Context.GetRubyEncoding(name);
        }

        [RubyMethod("_load?", RubyMethodAttributes.PublicSingleton)]
        public static bool Load(RubyClass/*!*/ self) {
            throw new NotImplementedException();
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
            return MutableString.CreateAscii("CP" + StringUtils.DefaultEncoding.CodePage.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
#endif