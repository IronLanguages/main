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

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    /// <summary>
    /// Implements END block. This block behaves like Kernel#at_exit with a true block definition. 
    /// </summary>
    public partial class ShutdownHandlerStatement : Expression {
        private readonly BlockDefinition/*!*/ _block;

        public BlockDefinition/*!*/ Block {
            get { return _block; }
        }

        public ShutdownHandlerStatement(LexicalScope/*!*/ definedScope, Statements/*!*/ body, SourceSpan location)
            : base(location) {
            _block = new BlockDefinition(definedScope, null, body, location);
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return Methods.RegisterShutdownHandler.OpCall(_block.Transform(gen));
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Ast.Block(Transform(gen), AstUtils.Constant(null));
        }
    }

}
