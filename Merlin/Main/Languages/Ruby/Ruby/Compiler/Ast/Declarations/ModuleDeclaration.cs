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

using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronRuby.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;
    using MSA = System.Linq.Expressions;

    public partial class ModuleDeclaration : DeclarationExpression {
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

        public ModuleDeclaration(LexicalScope/*!*/ definedScope, ConstantVariable/*!*/ qualifiedName, Body/*!*/ body, SourceSpan location)
            : base(definedScope, body, location) {
            ContractUtils.RequiresNotNull(qualifiedName, "qualifiedName");

            _qualifiedName = qualifiedName;
        }

        protected ModuleDeclaration(LexicalScope/*!*/ definedScope, Body/*!*/ body, SourceSpan location)
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
                    return Methods.DefineModule.OpCall(gen.CurrentScopeVariable, AstFactory.Box(transformedQualifier), name);
            }

            throw Assert.Unreachable;
        }

        internal sealed override MSA.Expression/*!*/ TransformRead(AstGenerator/*!*/ gen) {
            string debugString = (IsSingletonDeclaration) ? "SINGLETON" : ((this is ClassDeclaration) ? "CLASS" : "MODULE") + " " + QualifiedName.Name;

            ScopeBuilder outerLocals = gen.CurrentScope;
                
            // definition needs to take place outside the defined lexical scope:
            var definition = MakeDefinitionExpression(gen);
            var selfVariable = outerLocals.DefineHiddenVariable("#module", typeof(RubyModule));
            var rfcVariable = gen.CurrentRfcVariable;
            var parentScope = gen.CurrentScopeVariable;

            // inner locals:
            ScopeBuilder scope = new ScopeBuilder();
            var scopeVariable = scope.DefineHiddenVariable("#scope", typeof(RubyScope));
            
            gen.EnterModuleDefinition(
                scope,
                selfVariable, 
                scopeVariable, 
                IsSingletonDeclaration
            );

            // first, transform locals defined within the module body:
            DefinedScope.TransformLocals(scope);

            // second, transform body:
            MSA.Expression transformedBody = Body.TransformRead(gen);

            // outer local:
            MSA.Expression resultVariable = outerLocals.DefineHiddenVariable("#result", transformedBody.Type);
            
            // begin with new scope
            //   self = DefineModule/Class(... parent scope here ...)
            //   <body>
            // end
            MSA.Expression result = AstFactory.Block(
                gen.DebugMarker(debugString),
                Ast.Assign(selfVariable, definition),
                scope.CreateScope(
                    Ast.Block(
                        Ast.Assign(scopeVariable, 
                            Methods.CreateModuleScope.OpCall(scope.VisibleVariables(), parentScope, rfcVariable, selfVariable)),
                        Ast.Assign(resultVariable, transformedBody),
                        AstUtils.Empty()
                    )
                ),
                gen.DebugMarker("END OF " + debugString),
                resultVariable
            );

            gen.LeaveModuleDefinition();

            return result;
        }
    }
}
