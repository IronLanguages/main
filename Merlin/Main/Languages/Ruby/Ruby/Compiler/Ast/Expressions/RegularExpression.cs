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

using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using System.Runtime.CompilerServices;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using MSA = System.Linq.Expressions;

    // /pattern/options
    public partial class RegularExpression : Expression {
        private readonly RubyRegexOptions _options;
        private readonly List<Expression>/*!*/ _pattern;

        public RubyRegexOptions Options {
            get { return _options; }
        }

        public List<Expression>/*!*/ Pattern {
            get { return _pattern; }
        }

        public RegularExpression(List<Expression>/*!*/ pattern, RubyRegexOptions options, SourceSpan location) 
            : this(pattern, options, false, location) {
        }

        public RegularExpression(List<Expression>/*!*/ pattern, RubyRegexOptions options, bool isCondition, SourceSpan location)
            : base(location) {
            Assert.NotNull(pattern);

            _pattern = pattern;
            _options = options;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return StringConstructor.TransformConcatentation(
                gen, 
                _pattern, 
                Methods.CreateRegex, 
                AstUtils.Constant(_options), 
                AstUtils.Constant(new StrongBox<RubyRegex>())
            );
        }

        internal override Expression/*!*/ ToCondition(LexicalScope/*!*/ currentScope) {
            return new RegularExpressionCondition(this);
        }
    }

    public partial class RegularExpressionCondition : Expression {
        private readonly RegularExpression/*!*/ _regex;

        public RegularExpression/*!*/ RegularExpression {
            get { return _regex; }
        }

        public RegularExpressionCondition(RegularExpression/*!*/ regex)
            : base(regex.Location) {
            Assert.NotNull(regex);
            _regex = regex;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Methods.MatchLastInputLine.OpCall(_regex.TransformRead(gen), gen.CurrentScopeVariable);
        }
    }
}
