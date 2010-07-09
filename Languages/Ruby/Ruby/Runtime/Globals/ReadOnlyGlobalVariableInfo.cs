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

using Microsoft.Scripting;

namespace IronRuby.Runtime {
    internal sealed class ReadOnlyGlobalVariableInfo : GlobalVariable {
        private object _value;

        public ReadOnlyGlobalVariableInfo(object value) {
            _value = value;
        }

        public override object GetValue(RubyContext/*!*/ context, RubyScope scope) {
            return _value;
        }

        public override void SetValue(RubyContext/*!*/ context, RubyScope scope, string/*!*/ name, object value) {
            throw ReadOnlyError(name);
        }
    }
}
