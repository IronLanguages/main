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
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins {
    public static partial class RubyException {
        public partial class Subclass : Exception {
            // called by Class#new rule when creating a Ruby subclass of Exception:
            public Subclass(RubyClass/*!*/ rubyClass) {
                Assert.NotNull(rubyClass);
                Debug.Assert(!rubyClass.IsSingletonClass);
                ImmediateClass = rubyClass;
            }
        }
    }
}
