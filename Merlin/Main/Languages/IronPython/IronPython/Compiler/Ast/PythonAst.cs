/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;

using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {
    using Ast = System.Linq.Expressions.Expression;

    public class PythonAst : ScopeStatement {
        private readonly Statement _body;
        private readonly bool _isModule;
        private readonly bool _printExpressions;        
        private readonly PythonLanguageFeatures _languageFeatures;
        private PythonVariable _docVariable, _nameVariable, _fileVariable;

        public PythonAst(Statement body, bool isModule, PythonLanguageFeatures languageFeatures, bool printExpressions) {
            ContractUtils.RequiresNotNull(body, "body");

            _body = body;
            _isModule = isModule;
            _printExpressions = printExpressions;
            _languageFeatures = languageFeatures;
        }

        /// <summary>
        /// True division is enabled in this AST.
        /// </summary>
        public bool TrueDivision {
            get { return (_languageFeatures & PythonLanguageFeatures.TrueDivision) != 0; }
        }

        /// <summary>
        /// True if the with statement is enabled in this AST.
        /// </summary>
        public bool AllowWithStatement {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AllowWithStatement) != 0;
            }
        }

        /// <summary>
        /// True if absolute imports are enabled
        /// </summary>
        public bool AbsoluteImports {
            get {
                return (_languageFeatures & PythonLanguageFeatures.AbsoluteImports) != 0;
            }
        }

        public Statement Body {
            get { return _body; }
        }

        public bool Module {
            get { return _isModule; }
        }

        internal PythonVariable DocVariable {
            get { return _docVariable; }
            set { _docVariable = value; }
        }

        internal PythonVariable NameVariable {
            get { return _nameVariable; }
            set { _nameVariable = value; }
        }

        internal PythonVariable FileVariable {
            get { return _fileVariable; }
            set { _fileVariable = value; }
        }
        
        internal override bool IsGlobal {
            get { return true; }
        }

        internal override bool ExposesLocalVariable(PythonVariable variable) {
            return true;
        }

        internal PythonVariable EnsureGlobalVariable(PythonNameBinder binder, SymbolId name) {
            PythonVariable variable;
            if (TryGetVariable(name, out variable)) {
                // use the current one if it is global only
                if (variable.Kind == VariableKind.Global) {
                    return variable;
                }
            }

            return EnsureUnboundVariable(name);
        }

        internal override PythonVariable BindName(PythonNameBinder binder, SymbolId name) {
            return EnsureVariable(name);
        }

        internal ScriptCode/*!*/ TransformToAst(CompilationMode mode, CompilerContext/*!*/ context) {
            // Create the ast generator
            // Use the PrintExpression value for the body (global level code)
            PythonCompilerOptions pco = context.Options as PythonCompilerOptions;
            Debug.Assert(pco != null);
            
            string name;
            if (!context.SourceUnit.HasPath || (pco.Module & ModuleOptions.ExecOrEvalCode) != 0) {
                name = "<module>";
            } else {
                name = context.SourceUnit.Path;
            }

            AstGenerator ag = new AstGenerator(mode, context, _body.Span, name, false, _printExpressions);
            
            MSAst.Expression transformed = Transform(ag);
            if (context.SourceUnit.Kind != SourceCodeKind.Expression) {
                transformed = ag.WrapScopeStatements(transformed);   // new ComboActionRewriter().VisitNode(Transform(ag))
            }

            MSAst.Expression body;

            // if we can change the language features or we're a module which needs __builtins__ initialized
            // then we need to make the ModuleStarted call.
            if (_languageFeatures != PythonLanguageFeatures.Default || _isModule) {
                body = Ast.Block(
                    Ast.Call(
                        AstGenerator.GetHelperMethod("ModuleStarted"),
                        ag.LocalContext,
                        AstUtils.Constant(_languageFeatures)
                    ),
                    transformed,
                    AstUtils.Empty()
                );
            } else {
                body = transformed;
            }

            if (_isModule) {
                string moduleName = pco.ModuleName;
                if (moduleName == null) {
#if !SILVERLIGHT
                    if (context.SourceUnit.HasPath && context.SourceUnit.Path.IndexOfAny(Path.GetInvalidFileNameChars()) == -1) {
                        moduleName = Path.GetFileNameWithoutExtension(context.SourceUnit.Path);
#else
                    if (context.SourceUnit.HasPath) {                    
                        moduleName = context.SourceUnit.Path;
#endif
                    } else {
                        moduleName = "<module>";
                    }
                }

                Debug.Assert(moduleName != null);

                body = Ast.Block(
                    ag.Globals.Assign(ag.Globals.GetVariable(ag, _fileVariable), Ast.Constant(name)),
                    ag.Globals.Assign(ag.Globals.GetVariable(ag, _nameVariable), Ast.Constant(moduleName)),
                    body // already typed to void
                );

                if ((pco.Module & ModuleOptions.Initialize) != 0) {
                    MSAst.Expression tmp = ag.HiddenVariable(typeof(object), "$originalModule");
                    // TODO: Should be try/fault
                    body = AstUtils.Try(
                        Ast.Assign(tmp, Ast.Call(AstGenerator.GetHelperMethod("PublishModule"), ag.LocalContext, Ast.Constant(moduleName))),
                        body
                    ).Catch(
                        typeof(Exception),
                        Ast.Call(AstGenerator.GetHelperMethod("RemoveModule"), ag.LocalContext, Ast.Constant(moduleName), tmp),
                        Ast.Rethrow(body.Type)
                    );
                }
            }

            if (ag.PyContext.PythonOptions.Frames) {
                body = Ast.Block(
                    new[] { FunctionDefinition._functionStack },
                    FunctionDefinition.AddFrame(
                        ag.LocalContext, 
                        Ast.Constant(null, typeof(PythonFunction)), 
                        body
                    )
                );
            }

            body = ag.AddProfiling(body);
            body = ag.AddReturnTarget(body);

            if (body.Type == typeof(void)) {
                body = Ast.Block(body, Ast.Constant(null));
            }

            return ag.MakeScriptCode(body, context, this);
        }

        internal override MSAst.Expression Transform(AstGenerator ag) {
            List<MSAst.Expression> block = new List<MSAst.Expression>();
            // Create the variables
            CreateVariables(ag, null, block, false, false);

            if (block.Count == 0 && _body is ReturnStatement && _languageFeatures == PythonLanguageFeatures.Default) {
                // for simple eval's we can construct a simple tree which just
                // leaves the value on the stack.  Return's can't exist in modules
                // so this is always safe.
                Debug.Assert(!_isModule);

                return ((ReturnStatement)_body).Expression.Transform(ag, typeof(object));                
            }

            MSAst.Expression bodyStmt = ag.Transform(_body);

            string doc = ag.GetDocumentation(_body);

            if (_isModule) {
                block.Add(ag.Globals.Assign(
                    ag.Globals.GetVariable(ag, _docVariable),
                    Ast.Constant(doc)
                ));
            }
            
            if (bodyStmt != null) {
                if (block.Count == 0 && bodyStmt.Type == typeof(void)) {
                    return bodyStmt;
                }

                block.Add(bodyStmt); //  bodyStmt could be null if we have an error - e.g. a top level break
            }
            block.Add(AstUtils.Empty());

            return Ast.Block(block);
        }

        public override void Walk(PythonWalker walker) {
            if (walker.Walk(this)) {
                if (_body != null) {
                    _body.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }        
    }
}
