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
using IronRuby.Runtime;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Runtime;
using System.Text;

namespace IronRuby.Builtins {
    [RubyClass(Extends = typeof(char), Restrictions = ModuleRestrictions.None)]
    [Includes(typeof(ClrString), typeof(Enumerable), typeof(Comparable))]
    public static class CharOps {
        private static Exception/*!*/ EmptyError(string/*!*/ argType) {
            return RubyExceptions.CreateArgumentError("cannot convert an empty {0} to System::Char", argType);
        }

        [RubyConstructor]
        public static char Create(RubyClass/*!*/ self, int utf16) {
            try {
                return checked((char)utf16);
            } catch (OverflowException) {
                throw RubyExceptions.CreateRangeError("{0} is not a valid UTF-16 character code", utf16);
            }
        }

        [RubyConstructor]
        public static char Create(RubyClass/*!*/ self, char c) {
            return c;
        }

        [RubyConstructor]
        public static char Create(RubyClass/*!*/ self, [NotNull]char[]/*!*/ chars) {
            if (chars.Length == 0) {
                throw EmptyError("System::Char[]");
            }

            return chars[0];
        }

        [RubyConstructor]
        public static char Create(RubyClass/*!*/ self, [NotNull]string/*!*/ str) {
            if (str.Length == 0) {
                throw EmptyError("string");
            }

            return str[0];
        }

        [RubyConstructor]
        public static char Create(RubyClass/*!*/ self, [DefaultProtocol]MutableString/*!*/ str) {
            if (str.IsEmpty) {
                throw EmptyError("string");
            }

            return str.GetChar(0);
        }

        [RubyMethod("inspect")]
        public static MutableString/*!*/ Inspect(RubyContext/*!*/ context, char self) {
            return MutableString.Create(
                MutableString.AppendUnicodeRepresentation(
                    new StringBuilder().Append('\''), self.ToString(), MutableString.Escape.Special, '\'', -1
                ).Append("' (Char)").ToString(),
                context.GetIdentifierEncoding()
            );
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Dump(char self) {
            return MutableString.CreateAscii(
                MutableString.AppendUnicodeRepresentation(
                    new StringBuilder().Append('\''), self.ToString(), MutableString.Escape.Special | MutableString.Escape.NonAscii, '\'', -1
                ).Append("' (Char)").ToString()
            );
        }
    }
}
