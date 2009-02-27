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
        private bool _isCondition;

        public RubyRegexOptions Options {
            get { return _options; }
        }

        public List<Expression>/*!*/ Pattern {
            get { return _pattern; }
        }

        public bool IsCondition {
            get { return _isCondition; }
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
            var result = StringConstructor.TransformConcatentation(
                gen, 
                _pattern, 
                Methods.CreateRegex, 
                AstUtils.Constant(_options), 
                AstUtils.Constant(new StrongBox<RubyRegex>()));

            if (_isCondition) {
                result = Methods.MatchLastInputLine.OpCall(result, gen.CurrentScopeVariable);
            }

            return result;
        }

        internal override Expression/*!*/ ToCondition() {
            _isCondition = true;
            return this;
        }
    }
}
