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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using Ast = MSA.Expression;

    /// <summary>
    /// Represents a file initializer - BEGIN { ... } block.
    /// </summary>
    public partial class FileInitializerStatement : Expression {
        private readonly LexicalScope/*!*/ _definedScope;
        private readonly Statements/*!*/ _statements;

        public Statements/*!*/ Statements {
            get { return _statements; }
        }

        public FileInitializerStatement(LexicalScope/*!*/ definedScope, Statements/*!*/ statements, SourceSpan location)
            : base(location) {
            Assert.NotNull(definedScope);

            _definedScope = definedScope;
            _statements = statements;
        }

        private ScopeBuilder/*!*/ DefineLocals() {
            return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), null, _definedScope);
        }

        private void TransformBody(AstGenerator/*!*/ gen) {
            ScopeBuilder scope = DefineLocals();

            var scopeVariable = gen.TopLevelScope.Builder.DefineHiddenVariable("#scope", typeof(RubyScope));
            
            gen.EnterFileInitializer(
                scope,
                gen.TopLevelScope.SelfVariable,
                scopeVariable
            );

            // visit nested initializers depth-first:
            var body = gen.TransformStatements(_statements, ResultOperation.Ignore);

            gen.LeaveFileInitializer();

            gen.AddFileInitializer(
                scope.CreateScope(
                    scopeVariable,
                    Methods.CreateFileInitializerScope.OpCall(
                        scope.MakeLocalsStorage(), 
                        scope.GetVariableNamesExpression(), 
                        gen.TopLevelScope.RuntimeScopeVariable
                    ),
                    body
                )
            );
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            TransformBody(gen);
            return Ast.Empty();
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            TransformBody(gen);
            return AstUtils.Constant(null);
        }
    }
}
