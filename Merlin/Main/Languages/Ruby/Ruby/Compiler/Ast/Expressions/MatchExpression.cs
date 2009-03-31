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
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSA = System.Linq.Expressions;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using IronRuby.Runtime.Calls;

    public partial class MatchExpression : Expression {
        private readonly RegularExpression/*!*/ _regex;
        private readonly Expression/*!*/ _expression;

        public RegularExpression/*!*/ Regex {
            get { return _regex; }
        }

        public Expression/*!*/ Expression {
            get { return _expression; }
        }

        public MatchExpression(RegularExpression/*!*/ regex, Expression/*!*/ expression, SourceSpan location) 
            : base(location) {
            _regex = regex;
            _expression = expression;
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "method";
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.MatchString.OpCall(
                Ast.Dynamic(ConvertToStrAction.Make(gen.Context), typeof(MutableString), _expression.Transform(gen)),
                _regex.Transform(gen),
                gen.CurrentScopeVariable
            );
        }
    }
}
