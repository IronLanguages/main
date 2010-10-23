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

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = Expression;

    public partial class InstanceVariable : Variable {
        public InstanceVariable(string/*!*/ name, SourceSpan location)
            : base(name, location) {
            Debug.Assert(name.StartsWith("@"));
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            return Methods.GetInstanceVariable.OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, AstUtils.Constant(Name));
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            return Methods.SetInstanceVariable.OpCall(gen.CurrentSelfVariable, AstUtils.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return Methods.IsDefinedInstanceVariable.OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, AstUtils.Constant(Name));
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "instance-variable";
        }
    }
}
