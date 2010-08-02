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

using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Generation;
using System.Diagnostics;

namespace IronRuby.Builtins {
    public partial class Range {
        public sealed partial class Subclass : Range, IRubyObject {
            // called by Class#new rule when creating a Ruby subclass of String:
            public Subclass(RubyClass/*!*/ rubyClass) {
                Assert.NotNull(rubyClass);
                Debug.Assert(!rubyClass.IsSingletonClass);
                ImmediateClass = rubyClass;
            }

            private Subclass(Range.Subclass/*!*/ range)
                : base(range) {
                ImmediateClass = range.ImmediateClass.NominalClass;
            }

            protected override Range/*!*/ Copy() {
                return new Subclass(this);
            }
        }
    }
}
