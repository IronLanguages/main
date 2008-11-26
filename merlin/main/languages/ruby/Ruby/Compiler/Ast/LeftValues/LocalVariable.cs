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

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using MSA = System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public partial class LocalVariable : Variable {
        private MSA.ParameterExpression _transformed;

        public LocalVariable(string/*!*/ name, SourceSpan location)
            : base(name, location) {
        }

        internal void TransformDefinition(ScopeBuilder/*!*/ locals) {
            if (_transformed == null) {
                _transformed = locals.DefineVariable(Name);
            }
        }

        internal MSA.ParameterExpression/*!*/ TransformParameterDefinition() {
            Debug.Assert(_transformed == null);
            return _transformed = Ast.Parameter(typeof(object), Name);
        }

        internal MSA.ParameterExpression/*!*/ TransformBlockParameterDefinition() {
            Debug.Assert(_transformed == null);
            return _transformed = Ast.Parameter(typeof(Proc), Name);
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            if (_transformed != null) {
                // static lookup:
                return _transformed;
            } else {
                // dynamic lookup:
                return Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, AstUtils.Constant(Name));
            }
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            if (_transformed != null) {
                // static lookup:
                return Ast.Assign(_transformed, AstUtils.Convert(rightValue, _transformed.Type));
            } else {
                // dynamic lookup:
                return Methods.SetLocalVariable.OpCall(AstFactory.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
            }
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return null;
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            // TODO: 1.8/1.9 variables in a block
            return "local-variable";
        }
    }
}
