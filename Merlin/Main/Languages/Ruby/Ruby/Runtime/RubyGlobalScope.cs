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

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Diagnostics;

namespace IronRuby.Runtime {
    public class RubyGlobalScope : ScopeExtension {
        private RubyContext/*!*/ _context;
        private RubyObject/*!*/ _mainObject;
        private RubyTopLevelScope _topLocalScope;
        private bool _isHosted;

        public RubyContext/*!*/ Context {
            get { return _context; }
            set { _context = value; }
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
            internal set { _topLocalScope = value; }
        }

        public RubyGlobalScope(RubyContext/*!*/ context, Scope/*!*/ scope, RubyObject/*!*/ mainObject, bool isHosted)
            : base(scope) {
            Assert.NotNull(context, scope, mainObject);
            Debug.Assert(mainObject.ImmediateClass.IsSingletonClass);
                
            _context = context;
            _mainObject = mainObject;
            _isHosted = isHosted;
        }
    }
}
