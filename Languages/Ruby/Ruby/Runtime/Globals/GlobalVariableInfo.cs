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
    internal sealed class GlobalVariableInfo : GlobalVariable {
        private object _value;

        // whether the variable is considered defined:
        private bool _isDefined;

        internal GlobalVariableInfo(object value) 
            : this(value, true) {
        }
        
        internal GlobalVariableInfo(object value, bool isDefined) {
            _value = value;
            _isDefined = isDefined;
        }

        public override bool IsDefined {
            get { return _isDefined; }
        }

        public override object GetValue(RubyContext/*!*/ context, RubyScope scope) {
            return _value;
        }

        public override void SetValue(RubyContext/*!*/ context, RubyScope scope, string/*!*/ name, object value) {
            _value = value;
            _isDefined = true;
        }
    }
}
