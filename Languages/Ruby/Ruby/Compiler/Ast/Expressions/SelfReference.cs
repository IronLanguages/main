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
    /// <summary>
    /// self
    /// </summary>
    public partial class SelfReference : Expression {

        public SelfReference(SourceSpan location)
            : base(location) {
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return gen.CurrentSelfVariable;
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "self";
        }
    }
}
