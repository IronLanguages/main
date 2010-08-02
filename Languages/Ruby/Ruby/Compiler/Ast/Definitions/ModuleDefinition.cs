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
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Ast {
    using Ast = MSA.Expression;
    using AstUtils = Microsoft.Scripting.Ast.Utils;
    using AstBlock = Microsoft.Scripting.Ast.BlockBuilder;
    
    public partial class ModuleDefinition : DefinitionExpression {
        /// <summary>
        /// Singleton classes don't have a name.
        /// </summary>
        private readonly ConstantVariable _qualifiedName;
        
        public ConstantVariable QualifiedName {
            get { return _qualifiedName; }
        }

        protected virtual bool IsSingletonDeclaration {
            get { return false; }
        }

        public ModuleDefinition(LexicalScope/*!*/ definedScope, ConstantVariable/*!*/ qualifiedName, Body/*!*/ body, SourceSpan location)
            : base(definedScope, body, location) {
            ContractUtils.RequiresNotNull(qualifiedName, "qualifiedName");

            _qualifiedName = qualifiedName;
        }

        protected ModuleDefinition(LexicalScope/*!*/ definedScope, Body/*!*/ body, SourceSpan location)
            : base(definedScope, body, location) {
            _qualifiedName = null;
        }
        
        internal virtual MSA.Expression/*!*/ MakeDefinitionExpression(AstGenerator/*!*/ gen) {
            MSA.Expression transformedQualifier;
            MSA.Expression name = QualifiedName.TransformName(gen);

            switch (QualifiedName.TransformQualifier(gen, out transformedQualifier)) {
                case StaticScopeKind.Global:
                    return Methods.DefineGlobalModule.OpCall(gen.CurrentScopeVariable, name);

                case StaticScopeKind.EnclosingModule:
                    return Methods.DefineNestedModule.OpCall(gen.CurrentScopeVariable, name);

                case StaticScopeKind.Explicit:
                    return Methods.DefineModule.OpCall(gen.CurrentScopeVariable, AstUtils.Box(transformedQualifier), name);
            }

            throw Assert.Unreachable;
        }

        private ScopeBuilder/*!*/ DefineLocals() {
            return new ScopeBuilder(DefinedScope.AllocateClosureSlotsForLocals(0), null, DefinedScope);
        }

        internal sealed override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            string debugString = (IsSingletonDeclaration) ? "SINGLETON" : ((this is ClassDefinition) ? "CLASS" : "MODULE") + " " + QualifiedName.Name;

            ScopeBuilder outerLocals = gen.CurrentScope;
                
            // definition needs to take place outside the defined lexical scope:
            var definition = MakeDefinitionExpression(gen);
            var selfVariable = outerLocals.DefineHiddenVariable("#module", typeof(RubyModule));
            var parentScope = gen.CurrentScopeVariable;

            // inner locals:
            ScopeBuilder scope = DefineLocals();
            var scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyScope));
            
            gen.EnterModuleDefinition(
                scope,
                selfVariable, 
                scopeVariable, 
                IsSingletonDeclaration
            );

            // transform body:
            MSA.Expression transformedBody = Body.TransformRead(gen);

            // outer local:
            MSA.Expression resultVariable = outerLocals.DefineHiddenVariable("#result", transformedBody.Type);
            
            // begin with new scope
            //   self = DefineModule/Class(... parent scope here ...)
            //   <body>
            // end
            MSA.Expression result = new AstBlock {
                gen.DebugMarker(debugString),
                Ast.Assign(selfVariable, definition),
                scope.CreateScope(
                    scopeVariable,
                    Methods.CreateModuleScope.OpCall(
                        scope.MakeLocalsStorage(),
                        scope.GetVariableNamesExpression(), 
                        parentScope, 
                        selfVariable
                    ),
                    Ast.Block(
                        Ast.Assign(resultVariable, transformedBody),
                        AstUtils.Empty()
                    )
                ),
                gen.DebugMarker("END OF " + debugString),
                resultVariable
            };

            gen.LeaveModuleDefinition();

            return result;
        }
    }
}
