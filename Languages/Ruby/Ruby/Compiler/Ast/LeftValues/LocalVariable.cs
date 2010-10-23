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
using IronRuby.Builtins;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class LocalVariable : Variable {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static new LocalVariable[] EmptyArray = new LocalVariable[0];

        // -1: the variable is defined in the outer runtime scope
        private readonly int _definitionLexicalDepth;

        // TODO: readonly + some mapping on AstGenerator?
        private int _closureIndex;

        internal LocalVariable(string/*!*/ name, SourceSpan location, int definitionLexicalDepth)
            : base(name, location) {
            Debug.Assert(definitionLexicalDepth >= -1);

            _definitionLexicalDepth = definitionLexicalDepth;
            _closureIndex = -1;
        }

        internal int ClosureIndex {
            get { return _closureIndex; }
        }

        internal void SetClosureIndex(int index) {
            Debug.Assert(_closureIndex == -1);
            _closureIndex = index;
        }

        internal override MSA.Expression/*!*/ TransformReadVariable(AstGenerator/*!*/ gen, bool tryRead) {
            if (_definitionLexicalDepth >= 0) {
                // static lookup:
                return gen.CurrentScope.GetVariableAccessor(_definitionLexicalDepth, _closureIndex);
            } else {
                // dynamic lookup:
                return Methods.GetLocalVariable.OpCall(gen.CurrentScopeVariable, AstUtils.Constant(Name));
            }
        }

        internal override MSA.Expression/*!*/ TransformWriteVariable(AstGenerator/*!*/ gen, MSA.Expression/*!*/ rightValue) {
            if (_definitionLexicalDepth >= 0) {
                // static lookup:
                return Ast.Assign(gen.CurrentScope.GetVariableAccessor(_definitionLexicalDepth, _closureIndex), AstUtils.Box(rightValue));
            } else {
                // dynamic lookup:
                return Methods.SetLocalVariable.OpCall(AstUtils.Box(rightValue), gen.CurrentScopeVariable, AstUtils.Constant(Name));
            }
        }

        internal override string/*!*/ GetNodeName(AstGenerator/*!*/ gen) {
            // TODO: 1.8/1.9 variables in a block
            return "local-variable";
        }
    }
}
