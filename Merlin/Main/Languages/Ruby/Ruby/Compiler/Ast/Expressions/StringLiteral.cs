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

using System;
using System.Dynamic;
using System.Diagnostics;
using System.Text;

using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class StringLiteral : Expression {
        // string or byte[]
        private readonly object/*!*/ _value;

        internal StringLiteral(object/*!*/ value, SourceSpan location) 
            : base(location) {
            Debug.Assert(value is string || value is byte[]);
            _value = value;
        }

        public StringLiteral(string/*!*/ value, SourceSpan location)
            : this((object)value, location) {
        }

        public StringLiteral(byte[]/*!*/ value, SourceSpan location)
            : this((object)value, location) {
        }

        internal object/*!*/ Value {
            get { return _value; }
        }

        public MutableString/*!*/ GetMutableString(RubyEncoding/*!*/ encoding) {
            string str = _value as string;
            if (str != null) {
                return MutableString.Create(str, encoding);
            } else {
                return MutableString.CreateBinary((byte[])_value, encoding);
            }
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.CreateMutableStringL.OpCall(StringConstructor.MakeConstant(_value), AstUtils.Constant(gen.Encoding));
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return null;
        }
    }
}
