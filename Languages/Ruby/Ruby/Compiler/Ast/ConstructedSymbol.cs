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

using System.Dynamic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public struct ConstructedSymbol {
        private readonly object _value;

        public ConstructedSymbol(string value) {
            _value = value;
        }

        public ConstructedSymbol(StringConstructor value) {
            _value = value;
        }

        internal ConstructedSymbol(object value) {
            Debug.Assert(value is string || value is StringConstructor);
            _value = value;
        }

        public object Value {
            get { return _value; }
        }

        internal MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            if (_value is string) {
                return Ast.Constant(_value, typeof(string));
            } else {
                return Methods.ConvertSymbolToClrString.OpCall(((StringConstructor)_value).TransformRead(gen));
            }
        }
    }
}
