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

using System.Text;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    // Extension methods for System.String
    [RubyClass("ClrString", Extends = typeof(string), MixinInterfaces = true)]
    public static class StringOps { 

        [RubyMethod("to_str", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("to_s", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ ToStr(string/*!*/ self) {
            return MutableString.Create(self);
        }

        [RubyMethod("to_clr_string", RubyMethodAttributes.PublicInstance)]
        public static string/*!*/ ToClrString(string/*!*/ self) {
            return self;
        }

        [RubyMethod("inspect", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Inspect(string/*!*/ self) {
            return MutableString.Create(MutableString.AppendUnicodeRepresentation(
                new StringBuilder().Append('\''), self, false, false, '\'', -1).Append('\'').ToString()
            );
        }

        [RubyMethod("dump", RubyMethodAttributes.PublicInstance)]
        public static MutableString/*!*/ Dump(string/*!*/ self) {
            return MutableString.Create(MutableString.AppendUnicodeRepresentation(
                new StringBuilder().Append('\''), self, false, true, '\'', -1).Append('\'').ToString()
            );
        }

        [RubyMethod("===", RubyMethodAttributes.PublicInstance)]
        [RubyMethod("==", RubyMethodAttributes.PublicInstance)]
        public static bool Equals(string str, object other) {
            return str.Equals(other);
        }
    }
}
