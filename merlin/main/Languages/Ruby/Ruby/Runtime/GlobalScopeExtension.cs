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

namespace IronRuby.Runtime {
    public class GlobalScopeExtension : ScopeExtension {
        private RubyContext/*!*/ _context;
        private object/*!*/ _mainObject;
        private bool _isHosted;

        public RubyContext/*!*/ Context {
            get { return _context; }
            set { _context = value; }
        }

        public object/*!*/ MainObject {
            get { return _mainObject; }
        }

        public bool IsHosted {
            get { return _isHosted; }
        }

        public GlobalScopeExtension(RubyContext/*!*/ context, Scope/*!*/ globalScope, object/*!*/ mainObject, bool isHosted)
            : base(globalScope) {
            Assert.NotNull(context, globalScope, mainObject);
            _context = context;
            _mainObject = mainObject;
            _isHosted = isHosted;
        }
    }
}
