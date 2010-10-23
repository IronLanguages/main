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

using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Runtime.Calls;
using IronRuby.Builtins;

#if !CLR2
using MSA = System.Linq.Expressions;
#else
using MSA = Microsoft.Scripting.Ast;
#endif

using AstUtils = Microsoft.Scripting.Ast.Utils;
    
namespace IronRuby.Compiler.Ast {
    using Ast = Expression;

    public partial class ForLoopExpression : Expression {
        //	for variables in list 
        //		body
        //	end

        private readonly BlockDefinition/*!*/ _block;
        private readonly Expression/*!*/ _list;

        public BlockDefinition/*!*/ Block {
            get { return _block; }
        }

        public Expression/*!*/ List {
            get { return _list; }
        }

        public ForLoopExpression(LexicalScope/*!*/ definedScope, Parameters/*!*/ variables, Expression/*!*/ list, Statements body, SourceSpan location)
            : base(location) {
            Assert.NotNull(definedScope, variables, list);

            _block = new BlockDefinition(definedScope, variables, body, location);
            _list = list;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            Assert.NotNull(gen);

            MSA.Expression transformedBlock = _block.Transform(gen);

            MSA.Expression blockArgVariable = gen.CurrentScope.DefineHiddenVariable("#forloop-block", typeof(Proc));

            MSA.Expression result = CallSiteBuilder.InvokeMethod(gen.Context, "each", RubyCallSignature.WithScopeAndBlock(0),
                gen.CurrentScopeVariable,
                _list.TransformRead(gen),
                blockArgVariable
            );

            return gen.DebugMark(MethodCall.MakeCallWithBlockRetryable(gen, result, blockArgVariable, transformedBlock, true),
                "#RB: method call with a block ('for-loop')");
        }
    }
}
