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
    [RubyClass("Encoding", Extends = typeof(RubyEncoding), Inherits = typeof(Object), Compatibility = RubyCompatibility.Ruby19, BuildConfig = "!SILVERLIGHT")]
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

        // TODO:
        // Shift_JIS
        // SHIFT_JIS
        // US_ASCII
        // UTF_8
        // ...

        #endregion

        #region Public Instance Methods

        [RubyMethod("_dump")]
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

        #endregion

        #region Singleton Methods

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
            return RubyEncoding.GetRubyEncoding(name.ToString());
        }

        [RubyMethod("compatible?", RubyMethodAttributes.PublicSingleton)]
        public static bool IsCompatible(RubyClass/*!*/ self, [NotNull]RubyEncoding/*!*/ encoding1, [NotNull]RubyEncoding/*!*/ encoding2) {
            throw new NotImplementedException();
        }

        [RubyMethod("_load?", RubyMethodAttributes.PublicSingleton)]
        public static bool Load(RubyClass/*!*/ self) {
            throw new NotImplementedException();
        }

        [RubyMethod("default_external", RubyMethodAttributes.PublicSingleton)]
        public static RubyEncoding/*!*/ GetDefaultEncoding(RubyClass/*!*/ self) {
            return RubyEncoding.Default;
        }

        [RubyMethod("locale_charmap", RubyMethodAttributes.PublicSingleton)]
        public static MutableString/*!*/ GetDefaultCharmap(RubyClass/*!*/ self) {
            return MutableString.CreateAscii("CP" + StringUtils.DefaultEncoding.CodePage.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
#endif