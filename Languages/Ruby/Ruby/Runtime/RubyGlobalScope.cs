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

using System.Diagnostics;
using System.Threading;
using IronRuby.Builtins;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime {
    /// <summary>
    /// DLR scope extension.
    /// Thread safe.
    /// </summary>
    public sealed class RubyGlobalScope : ScopeExtension {
        private readonly RubyContext/*!*/ _context;
        private readonly RubyObject/*!*/ _mainObject;
        private readonly bool _isHosted;

        // interlocked:
        private RubyTopLevelScope _topLocalScope;

        public RubyContext/*!*/ Context {
            get { return _context; }
        }

        public RubyClass/*!*/ MainSingleton {
            get { return _mainObject.ImmediateClass; }
        }

        public RubyObject/*!*/ MainObject {
            get { return _mainObject; }
        }

        public bool IsHosted {
            get { return _isHosted; }
        }

        public RubyTopLevelScope TopLocalScope {
            get { return _topLocalScope; }
        }

        internal RubyGlobalScope(RubyContext/*!*/ context, Scope/*!*/ scope, RubyObject/*!*/ mainObject, bool isHosted)
            : base(scope) {
            Assert.NotNull(context, scope, mainObject);
            Debug.Assert(mainObject.ImmediateClass.IsSingletonClass);
                
            _context = context;
            _mainObject = mainObject;
            _isHosted = isHosted;
        }

        internal RubyTopLevelScope/*!*/ SetTopLocalScope(RubyTopLevelScope/*!*/ scope) {
            return Interlocked.CompareExchange(ref _topLocalScope, scope, null);
        }
    }
}
