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
    using System.Reflection;
    using Microsoft.Scripting.Utils;

    public partial class ClassVariable : Variable {
        public ClassVariable(string/*!*/ name, SourceSpan location)
            : base(name, location) {
            Debug.Assert(name.StartsWith("@@"));
        }

        private const int OpTryGet = 0;
        private const int OpGet = 1;
        private const int OpIsDefined = 2;

        private MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen, int/*!*/ opKind) {
            // eval or in a non-singleton module/class declaration 
            // -> we find the right scope at runtime by walking the hierarchy
            // otherwise
            // -> variable is on Object

            return GetOp(gen, opKind).OpCall(gen.CurrentScopeVariable, AstUtils.Constant(Name));
        }

        private static MethodInfo/*!*/ GetOp(AstGenerator/*!*/ gen, int/*!*/ opKind) {
            switch (opKind) {
                case OpTryGet: return Methods.TryGetClassVariable;
                case OpGet: return Methods.GetClassVariable;
                case OpIsDefined: return Methods.IsDefinedClassVariable;
                default: throw Assert.Unreachable;
            }
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            return TransformRead(gen, tryRead ? OpTryGet : OpGet);
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            return Methods.SetClassVariable.OpCall(AstUtils.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return TransformRead(gen, OpIsDefined);
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "class variable";
        }
    }
}
