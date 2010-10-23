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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    /// <summary>
    /// Represents definition of a lambda:
    /// <code>
    /// ->{parameters} { }
    /// </code>
    /// </summary>
    public partial class LambdaDefinition : Expression {
        private readonly BlockDefinition _block;

        public BlockDefinition Block {
            get { return _block; }
        }

        public LambdaDefinition(BlockDefinition block)
            : base(block.Location) {
            _block = block;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return _block.Transform(gen, true);
        }
    }
}
