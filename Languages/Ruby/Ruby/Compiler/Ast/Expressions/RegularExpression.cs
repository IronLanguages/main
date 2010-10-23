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

using System;
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

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpression(AstGenerator/*!*/ gen, string/*!*/ literal, RubyEncoding/*!*/ encoding) {
            // TODO: create the regex here, not at runtime:
            return Methods.CreateRegexL.OpCall(
                Ast.Constant(literal), encoding.Expression, AstUtils.Constant(_options), AstUtils.Constant(new StrongBox<RubyRegex>(null))
            );
        }

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpression(AstGenerator/*!*/ gen, byte[]/*!*/ literal, RubyEncoding/*!*/ encoding) {
            // TODO: create the regex here, not at runtime:
            return Methods.CreateRegexB.OpCall(
                Ast.Constant(literal), encoding.Expression, AstUtils.Constant(_options), AstUtils.Constant(new StrongBox<RubyRegex>(null))
            );
        }


        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpressionN(AstGenerator/*!*/ gen, IEnumerable<MSA.Expression>/*!*/ args) {
            return Methods.CreateRegex("N").OpCall(
                Ast.NewArrayInit(typeof(MutableString), args),
                AstUtils.Constant(_options), 
                AstUtils.Constant(new StrongBox<RubyRegex>(null))
            );
        }

        MSA.Expression/*!*/ StringConstructor.IFactory.CreateExpressionM(AstGenerator/*!*/ gen, MSAst.ExpressionCollectionBuilder/*!*/ args) {
            string suffix = new String('M', args.Count);
            args.Add(gen.Encoding.Expression);
            args.Add(AstUtils.Constant(_options));
            args.Add(AstUtils.Constant(new StrongBox<RubyRegex>(null)));
            return Methods.CreateRegex(suffix).OpCall(args);
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
