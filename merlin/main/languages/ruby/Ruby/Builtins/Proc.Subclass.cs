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

using System.Linq.Expressions;
using System.Dynamic.Binders;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime;
using IronRuby.Compiler.Generation;

namespace IronRuby.Builtins {
    public partial class Proc {
        public sealed class Subclass : Proc, IRubyObject {
            private readonly RubyClass/*!*/ _class;
            private RubyInstanceData _instanceData;

            // called by Proc#new rule when creating a Ruby subclass of Proc:
            public Subclass(RubyClass/*!*/ rubyClass, Proc/*!*/ proc) 
                : base(proc) {
                Assert.NotNull(rubyClass);
                _class = rubyClass;
            }

            public override Proc/*!*/ Copy() {
                return new Subclass(_class, this);
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
