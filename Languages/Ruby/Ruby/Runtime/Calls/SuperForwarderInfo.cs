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
using System.Reflection;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Runtime.Calls {
    /// <summary>
    /// Used internally for implementation of methods defined by Kernel#public/protected/private.
    /// Such a method is just a stub that calls "super" - a method resolution thus forwards to its super method.
    /// </summary>
    internal sealed class SuperForwarderInfo : RubyMemberInfo {
        // If the forwarder is aliased we need to know the super name of the method so that we can forward to it.
        private readonly string _superName;

        public SuperForwarderInfo(RubyMemberFlags flags, RubyModule/*!*/ declaringModule, string/*!*/ superName) 
            : base(flags, declaringModule) {
            _superName = superName;
        }

        internal override bool IsSuperForwarder {
            get { return true; }
        }

        public string SuperName {
            get { return _superName; }
        }

        internal protected override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule/*!*/ module) {
            return new SuperForwarderInfo(flags, module, _superName);
        }

        public override string/*!*/ ToString() {
            return base.ToString() + (_superName != null ? " forward to: " + _superName : null);
        }
    }
}
