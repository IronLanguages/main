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

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    
    public partial class Literal : Expression {
        private readonly object _value;
        
        public object Value {
            get { return _value; }
        }

        private Literal(object value, SourceSpan location)
            : base(location) {
            _value = value;
        }

        public static Literal/*!*/ Integer(int value, SourceSpan location) {
            return new Literal(ScriptingRuntimeHelpers.Int32ToObject(value), location);
        }

        public static Literal/*!*/ Double(double value, SourceSpan location) {
            return new Literal(value, location);
        }

        public static Literal/*!*/ BigInteger(BigInteger/*!*/ value, SourceSpan location) {
            return new Literal(value, location);
        }

        public static Literal/*!*/ Nil(SourceSpan location) {
            return new Literal(null, location);
        }

        public static Literal/*!*/ True(SourceSpan location) {
            return new Literal(ScriptingRuntimeHelpers.True, location);
        }

        public static Literal/*!*/ False(SourceSpan location) {
            return new Literal(ScriptingRuntimeHelpers.False, location);
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return AstUtils.Constant(_value);
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            if (_value == null) {
                return "nil";
            } else if (_value is bool) {
                return (bool)_value ? "true" : "false";
            } else {
                return base.GetNodeName(gen);
            }
        }
    }
}
