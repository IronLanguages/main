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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {

    public partial class Maplet : Node {
        private readonly Expression/*!*/ _key;
        private readonly Expression/*!*/ _value;

        public Expression/*!*/ Key {
            get { return _key; }
        }

        public Expression/*!*/ Value {
            get { return _value; }
        }

        public Maplet(Expression/*!*/ key, Expression/*!*/ value, SourceSpan location) 
            : base(location) {
            Assert.NotNull(key, value);

            _key = key;
            _value = value;
        }
    }
}
