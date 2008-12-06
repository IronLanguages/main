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

using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins {

    [RubyClass("FileTest", Inherits = typeof(object))]
    public class FileTestOps {
        [RubyMethod("exist?", RubyMethodAttributes.PublicSingleton)]
        [RubyMethod("exists?", RubyMethodAttributes.PublicSingleton)]
        public static bool Exists(RubyClass/*!*/ self, [DefaultProtocol, NotNull]MutableString/*!*/ path) {
            return RubyFileOps.Exists(self, path);
        }
    }
}