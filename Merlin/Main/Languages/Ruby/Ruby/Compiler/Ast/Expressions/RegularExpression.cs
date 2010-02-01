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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using MSAst = Microsoft.Scripting.Ast;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    // /pattern/options
    public partial class RegularExpression : Expression, StringConstructor.IFactory {
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
            return StringConstructor.TransformConcatentation(gen, _pattern, this);
        }

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpression(AstGenerator/*!*/ gen, string/*!*/ literal) {
            // TODO: create the regex here, not at runtime:
            return Methods.CreateRegexL.OpCall(
                Ast.Constant(literal), gen.EncodingConstant, AstUtils.Constant(_options), AstUtils.Constant(new StrongBox<RubyRegex>())
            );
        }

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpression(AstGenerator/*!*/ gen, string/*!*/ opSuffix, MSA.Expression/*!*/ arg) {
            return Methods.CreateRegex(opSuffix).OpCall(
                arg, gen.EncodingConstant, AstUtils.Constant(_options), AstUtils.Constant(new StrongBox<RubyRegex>())
            );
        }

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpression(AstGenerator/*!*/ gen, string/*!*/ opSuffix, MSAst.ExpressionCollectionBuilder/*!*/ args) {
            args.Add(gen.EncodingConstant);
            args.Add(AstUtils.Constant(_options));
            args.Add(AstUtils.Constant(new StrongBox<RubyRegex>()));
            return Methods.CreateRegex(opSuffix).OpCall(args);
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
