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

using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using MSA = System.Linq.Expressions;
    using Ast = System.Linq.Expressions.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    internal enum StringLiteralEncoding {
        // encoding not specified in the literal
        Default = 0,

        // literal doesn't contain non-ASCII characters (> 0x7f)
        Ascii = 1,

        // literal contains \u escape
        UTF8 = 2,
    }

    public partial class StringLiteral : Expression {
        private readonly string/*!*/ _value;
        private readonly StringLiteralEncoding _encoding;

        public string/*!*/ Value {
            get { return _value; }
        }

        public bool IsAscii {
            get { return _encoding == StringLiteralEncoding.Ascii; }
        }

        public bool IsUTF8 {
            get { return _encoding == StringLiteralEncoding.UTF8; }
        }

        internal StringLiteral(string/*!*/ value, StringLiteralEncoding encoding, SourceSpan location)
            : base(location) {
            _value = value;
            _encoding = encoding;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            if (IsAscii || gen.Encoding == BinaryEncoding.Instance) {
                return Methods.CreateMutableStringB.OpCall(Ast.Constant(_value));
            } else if (IsUTF8 || gen.Encoding == BinaryEncoding.UTF8) {
                return Methods.CreateMutableStringU.OpCall(Ast.Constant(_value));
            } else {
                return Methods.CreateMutableStringE.OpCall(
                    Ast.Constant(_value), Ast.Constant(RubyEncoding.GetCodePage(gen.Encoding))
                );
            }
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return null;
        }
    }
}
