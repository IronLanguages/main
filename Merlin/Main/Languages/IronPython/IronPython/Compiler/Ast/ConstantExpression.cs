/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;

#if !CLR2
using MSAst = System.Linq.Expressions;
#else
using MSAst = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronPython.Compiler.Ast {
    using Ast = MSAst.Expression;

    public class ConstantExpression : Expression {
        private readonly object _value;

        public ConstantExpression(object value) {
            _value = value;
        }

        internal static ConstantExpression MakeUnicode(string value) {
            return new ConstantExpression(new UnicodeWrapper(value));
        }

        public object Value {
            get {
                UnicodeWrapper wrapper;
                if ((wrapper = _value as UnicodeWrapper) != null) {
                    return wrapper.Value;
                }
                
                return _value; 
            }
        }

        internal bool IsUnicodeString {
            get {
                return _value is UnicodeWrapper;
            }
        }

        public override MSAst.Expression Reduce() {
            UnicodeWrapper wrapper;
            if (_value == Ellipsis.Value) {
                return Ast.Property(
                    null,
                    typeof(PythonOps).GetProperty("Ellipsis")
                );
            } else if (_value is bool) {
                if ((bool)_value) {
                    return Ast.Field(null, typeof(ScriptingRuntimeHelpers).GetField("True"));
                } else {
                    return Ast.Field(null, typeof(ScriptingRuntimeHelpers).GetField("False"));
                }
            } else if ((wrapper = _value as UnicodeWrapper) != null) {
                return GlobalParent.Constant(wrapper.Value);
            }

            return GlobalParent.Constant(_value);
        }

        internal override ConstantExpression ConstantFold() {
            return this;
        }

        public override Type Type {
            get {
                return GlobalParent.CompilationMode.GetConstantType(Value);
            }
        }

        internal override string CheckAssign() {
            if (_value == null) {
                return "assignment to None";
            }

            return "can't assign to literal";
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
            }
            walker.PostWalk(this);
        }

        public override string NodeName {
            get {
                return "literal";
            }
        }

        internal override bool CanThrow {
            get {
                return false;
            }
        }

        class UnicodeWrapper {
            public readonly object Value;

            public UnicodeWrapper(string value) {
                Value = value;
            }
        }
    }
}
