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

using System.Diagnostics;
using System.Dynamic;
using Microsoft.Scripting;
using MSA = System.Linq.Expressions;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
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
            bool unknownOwner = gen.CompilerOptions.IsEval || gen.GetCurrentNonSingletonModule() != null;

            switch (opKind) {
                case OpTryGet: return unknownOwner ? Methods.TryGetClassVariable : Methods.TryGetObjectClassVariable;
                case OpGet: return unknownOwner ? Methods.GetClassVariable : Methods.GetObjectClassVariable;
                case OpIsDefined: return unknownOwner ? Methods.IsDefinedClassVariable : Methods.IsDefinedObjectClassVariable;
                default: throw Assert.Unreachable;
            }
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            return TransformRead(gen, tryRead ? OpTryGet : OpGet);
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            if (gen.CompilerOptions.IsEval || gen.GetCurrentNonSingletonModule() != null) {
                return Methods.SetClassVariable.OpCall(AstFactory.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
            } else {
                return Methods.SetObjectClassVariable.OpCall(AstFactory.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
            }
        }

        internal override MSA.Expression TransformDefinedCondition(AstGenerator/*!*/ gen) {
            return TransformRead(gen, OpIsDefined);
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            return "class variable";
        }
    }
}
