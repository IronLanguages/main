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
    public partial class MatchData {
        public sealed partial class Subclass : MatchData, IRubyObject {
            private readonly RubyClass/*!*/ _class;
            private RubyInstanceData _instanceData;

            // called by Class#new rule when creating a Ruby subclass of MatchData:
            public Subclass(RubyClass/*!*/ rubyClass) {
                Assert.NotNull(rubyClass);
                _class = rubyClass;
            }

            protected override MatchData/*!*/ CreateInstance() {
                return new Subclass(_class);
            }
        }
    }
}
