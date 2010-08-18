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
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = Expression;

    public partial class HashConstructor : Expression {
        // { key1 => value1, key2 => value2, ... }
        private readonly Maplet/*!*/[]/*!*/ _maplets;

        public Maplet/*!*/[]/*!*/ Maplets {
            get { return _maplets; }
        }

        public HashConstructor(Maplet/*!*/[]/*!*/ maplets, SourceSpan location)
            : base(location) {
            Assert.NotNullItems(maplets);
            _maplets = maplets;
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return gen.MakeHashOpCall(gen.TransformMapletsToExpressions(_maplets));
        }
    }
}
