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
using System.Diagnostics;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    
    [RubyClass("TrueClass")]
    public static class TrueClass : Object {
        #region Public Instance Methods

        [RubyMethodAttribute("to_s")]
        public static MutableString/*!*/ ToString(bool self) {
            Debug.Assert(self == true);
            return MutableString.CreateAscii("true"); 
        }

        [RubyMethodAttribute("&")]
        public static bool And(bool self, object obj) {
            Debug.Assert(self == true);
            return obj != null;
        }

        [RubyMethodAttribute("&")]
        public static bool And(bool self, bool obj) {
            Debug.Assert(self == true);
            return obj;
        }

        [RubyMethodAttribute("^")]
        public static bool Xor(bool self, object obj) {
            Debug.Assert(self == true);
            return obj == null;
        }

        [RubyMethodAttribute("^")]
        public static bool Xor(bool self, bool obj) {
            Debug.Assert(self == true);
            return !obj;
        }

        [RubyMethodAttribute("|")]
        public static bool Or(bool self, object obj) {
            Debug.Assert(self == true);
            return true;
        }

        #endregion
    }
}
