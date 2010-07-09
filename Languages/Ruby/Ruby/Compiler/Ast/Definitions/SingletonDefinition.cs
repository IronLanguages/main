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
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast {
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    public partial class SingletonDefinition : ModuleDefinition {
        private readonly Expression/*!*/ _singleton;

        public Expression/*!*/ Singleton {
            get { return _singleton; }
        }

        protected override bool IsSingletonDeclaration { get { return true; } }

        public SingletonDefinition(LexicalScope/*!*/ definedScope, Expression/*!*/ singleton, Body/*!*/ body, SourceSpan location)
            : base(definedScope, body, location) {
            ContractUtils.RequiresNotNull(singleton, "singleton");

            _singleton = singleton;
        }

        internal override MSA.Expression/*!*/ MakeDefinitionExpression(AstGenerator/*!*/ gen) {
            return Methods.DefineSingletonClass.OpCall(gen.CurrentScopeVariable, AstUtils.Box(_singleton.TransformRead(gen)));
        }
    }
}
