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
    /// Represents a string literal.
    /// </summary>
    public partial class StringLiteral : Expression {
        // string or byte[]
        private readonly object/*!*/ _value;

        // TODO: we can save memory if we subclass StringLiteral (EncodedStringLiteral, EncodedSymbolLiteral) for _encoding != __ENCODING__
        private readonly RubyEncoding/*!*/ _encoding;

        internal StringLiteral(object/*!*/ value, RubyEncoding/*!*/ encoding, SourceSpan location) 
            : base(location) {
            Debug.Assert(value is string || value is byte[]);
            Debug.Assert(encoding != null);
            _value = value;
            _encoding = encoding;
        }

        public object/*!*/ Value {
            get { return _value; }
        }

        public RubyEncoding/*!*/ Encoding {
            get { return _encoding; }
        }

        public MutableString/*!*/ GetMutableString() {
            string str = _value as string;
            if (str != null) {
                return MutableString.Create(str, _encoding);
            } else {
                return MutableString.CreateBinary((byte[])_value, _encoding);
            }
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Transform(_value, _encoding);
        }

        internal static MSA.Expression/*!*/ Transform(object/*!*/ value, RubyEncoding/*!*/ encoding) {
            if (value is string) {
                return Methods.CreateMutableStringL.OpCall(AstUtils.Constant(value), encoding.Expression);
            } else {
                return Methods.CreateMutableStringB.OpCall(AstUtils.Constant(value), encoding.Expression);
            }
        }
    }
}
