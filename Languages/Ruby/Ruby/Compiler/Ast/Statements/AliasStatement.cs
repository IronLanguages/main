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

using System.Dynamic;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;

    public partial class AliasStatement : Expression {
        private readonly ConstructedSymbol _newName;
        private readonly ConstructedSymbol _oldName;
        private readonly bool _isMethodAlias;

        public ConstructedSymbol NewName {
            get { return _newName; }
        }

        public ConstructedSymbol OldName {
            get { return _oldName; }
        }

        public bool IsMethodAlias {
            get { return _isMethodAlias; }
        }

        public bool IsGlobalVariableAlias {
            get { return !_isMethodAlias; }
        }

        public AliasStatement(bool isMethodAlias, ConstructedSymbol newName, ConstructedSymbol oldName, SourceSpan location)
            : base(location) {
            Assert.NotNull(newName, oldName);
            _newName = newName;
            _oldName = oldName;
            _isMethodAlias = isMethodAlias;
        }

        internal override MSA.Expression/*!*/ Transform(AstGenerator/*!*/ gen) {
            return (_isMethodAlias ? Methods.AliasMethod : Methods.AliasGlobalVariable).
                OpCall(gen.CurrentScopeVariable, _newName.Transform(gen), _oldName.Transform(gen));
        }

        internal override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            return Ast.Block(Transform(gen), AstUtils.Constant(null));
        }
    }
}
