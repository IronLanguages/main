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
#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using System.Diagnostics;
using Microsoft.Scripting;
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Represents a string literal encoded by the containing source file encoding.
    /// </summary>
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
    }
}
