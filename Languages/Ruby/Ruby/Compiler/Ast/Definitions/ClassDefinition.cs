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
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    
    // class Name
    //   <statements>
    // end
    public partial class ClassDefinition : ModuleDefinition {
        private readonly Expression _superClass;

        public Expression SuperClass {
            get { return _superClass; }
        }

        public ClassDefinition(LexicalScope/*!*/ definedScope, ConstantVariable/*!*/ name, Expression superClass, Body/*!*/ body, SourceSpan location)
            : base(definedScope, name, body, location) {
            ContractUtils.RequiresNotNull(name, "name");
            
            _superClass = superClass;
        }

        internal override MSA.Expression/*!*/ MakeDefinitionExpression(AstGenerator/*!*/ gen) {
            MSA.Expression transformedQualifier;
            MSA.Expression name = QualifiedName.TransformName(gen);
            MSA.Expression transformedSuper = (_superClass != null) ? AstUtils.Box(_superClass.TransformRead(gen)) : AstUtils.Constant(null);

            switch (QualifiedName.TransformQualifier(gen, out transformedQualifier)) {
                case StaticScopeKind.Global:
                    return Methods.DefineGlobalClass.OpCall(gen.CurrentScopeVariable, name, transformedSuper);

                case StaticScopeKind.EnclosingModule:
                    return Methods.DefineNestedClass.OpCall(gen.CurrentScopeVariable, name, transformedSuper);

                case StaticScopeKind.Explicit:
                    return Methods.DefineClass.OpCall(gen.CurrentScopeVariable, transformedQualifier, name, transformedSuper);
            }

            throw Assert.Unreachable;
        }
    }
}
