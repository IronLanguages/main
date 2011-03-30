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

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class UndefineStatement : Expression {
        private readonly List<ConstructedSymbol>/*!*/ _items;

        public List<ConstructedSymbol>/*!*/ Items {
            get { return _items; }
        }

        public UndefineStatement(List<ConstructedSymbol>/*!*/ items, SourceSpan location)
            : base(location) {
            Assert.NotNull(items);
            _items = items;
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            MSA.Expression[] result = new MSA.Expression[_items.Count + 1];
            for (int i = 0; i < _items.Count; i++) {
                result[i] = Methods.UndefineMethod.OpCall(gen.CurrentScopeVariable, _items[i].Transform(gen));
            }
            result[_items.Count] = AstUtils.Empty();
            return Ast.Block(result);
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Ast.Block(Transform(gen), AstUtils.Constant(null));
        }
    }
}
