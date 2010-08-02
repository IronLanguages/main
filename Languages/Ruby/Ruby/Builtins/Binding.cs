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

using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Builtins {
    public sealed class Binding {
        private readonly RubyScope/*!*/ _localScope;
        private readonly object _self;

        /// <summary>
        /// Local scope captured by the binding.
        /// </summary>
        public RubyScope/*!*/ LocalScope {
            get { return _localScope; }
        }

        /// <summary>
        /// Self object captured by the binding. Can be different from LocalScope.SelfObject in MRI 1.8.
        /// </summary>
        public object SelfObject {
            get { return _self; }
        }

        public Binding(RubyScope/*!*/ localScope) 
            : this(localScope, localScope.SelfObject) {
        }

        public Binding(RubyScope/*!*/ localScope, object self) {
            Assert.NotNull(localScope);
            _localScope = localScope;
            _self = self;
        }
    }
}
