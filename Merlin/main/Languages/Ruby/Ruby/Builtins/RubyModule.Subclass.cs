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
    public partial class RubyModule {
        public sealed class Subclass : RubyModule, IRubyObject {
            private readonly RubyClass/*!*/ _class;
            private RubyInstanceData _instanceData;

            // called by Class#new rule when creating a Ruby subclass:
            public Subclass(RubyClass/*!*/ rubyClass)
                : this(rubyClass, null) {
            }

            // called by Class#new rule when creating a Ruby subclass:
            internal Subclass(RubyClass/*!*/ rubyClass, string name)
                : base(rubyClass, name) {
                Assert.NotNull(rubyClass);
                _class = rubyClass;
            }

            protected override RubyModule/*!*/ CreateInstance(string name) {
                return new Subclass(_class, name);
            }

            #region IRubyObject Members

            [Emitted]
            public RubyClass/*!*/ Class {
                get { return _class; }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            #endregion
        }
    }
}
