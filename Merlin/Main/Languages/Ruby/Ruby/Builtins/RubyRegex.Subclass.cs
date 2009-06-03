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
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Generation;

namespace IronRuby.Builtins {
    public partial class RubyRegex {
        public sealed partial class Subclass : RubyRegex, IRubyObject {
            private readonly RubyClass/*!*/ _class;
            private RubyInstanceData _instanceData;

            // called by Class#new rule when creating a Ruby subclass of String:
            public Subclass(RubyClass/*!*/ rubyClass) {
                Assert.NotNull(rubyClass);
                _class = rubyClass;
            }

            private Subclass(RubyRegex.Subclass/*!*/ regex)
                : base(regex) {
                _class = regex._class;
            }

            protected override RubyRegex/*!*/ Copy() {
                return new Subclass(this);
            }
        }
    }
}
