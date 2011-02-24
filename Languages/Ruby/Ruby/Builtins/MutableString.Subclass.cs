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
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Runtime;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Generation;

namespace IronRuby.Builtins {
    // Represents any Ruby subclass of String. The actual class object is remembered.
    // We don't allow non-Ruby code to extend Ruby String.
    public partial class MutableString {
        public sealed partial class Subclass : MutableString, IRubyObject {
            // Called by Class#new rule when creating a Ruby subclass of String.
            // The encoding is set to BINARY.
            public Subclass(RubyClass/*!*/ rubyClass)
                : this(rubyClass, RubyEncoding.Binary) {
            }

            public Subclass(RubyClass/*!*/ rubyClass, RubyEncoding/*!*/ encoding) 
                : base(encoding) {
                Assert.NotNull(rubyClass);
                ImmediateClass = rubyClass;
            }

            private Subclass(RubyClass/*!*/ rubyClass, Content/*!*/ content, RubyEncoding/*!*/ encoding)
                : base(content, encoding) {
                Assert.NotNull(rubyClass);
                ImmediateClass = rubyClass;
            }

            private Subclass(Subclass/*!*/ str)
                : base(str) {
                ImmediateClass = str.ImmediateClass;
            }

            // creates an instance of self type with given content and encoding:
            internal override MutableString/*!*/ CreateInstance(Content/*!*/ content, RubyEncoding/*!*/ encoding) {
                return new Subclass(ImmediateClass, content, encoding);
            }

            // creates a blank instance of self type:
            public override MutableString/*!*/ CreateInstance() {
                return new Subclass(ImmediateClass, _encoding);
            }

            // creates a copy including the version and flags:
            public override MutableString/*!*/ Clone() {
                return new Subclass(this);
            }
        }
    }
}
