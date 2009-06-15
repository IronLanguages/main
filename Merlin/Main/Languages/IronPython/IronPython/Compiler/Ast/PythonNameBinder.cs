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
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

/*
 * The name binding:
 *
 * The name binding happens in 2 passes.
 * In the first pass (full recursive walk of the AST) we resolve locals.
 * The second pass uses the "processed" list of all context statements (functions and class
 * bodies) and has each context statement resolve its free variables to determine whether
 * they are globals or references to lexically enclosing scopes.
 *
 * The second pass happens in post-order (the context statement is added into the "processed"
 * list after processing its nested functions/statements). This way, when the function is
 * processing its free variables, it also knows already which of its locals are being lifted
 * to the closure and can report error if such closure variable is being deleted.
 *
 * This is illegal in Python:
 *
 * def f():
 *     x = 10
 *     if (cond): del x        # illegal because x is a closure variable
 *     def g():
 *         print x
 */

namespace IronPython.Compiler.Ast {
    class DefineBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public DefineBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            _binder.DefineName(node.Name);
            return false;
        }
        public override bool Walk(ParenthesisExpression node) {
            return true;
        }
        public override bool Walk(TupleExpression node) {
            return true;
        }
        public override bool Walk(ListExpression node) {
            return true;
        }
    }

    class ParameterBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public ParameterBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            // Called for the sublist parameters. The elements of the tuple become regular
            // local variables, therefore don't make the parameters (DefineParameter), but
            // regular locals (DefineName)
            _binder.DefineName(node.Name);
            node.Reference = _binder.Reference(node.Name);
            return false;
        }
        public override bool Walk(Parameter node) {
            node.Variable = _binder.DefineParameter(node.Name);
            return false;
        }
        public override bool Walk(SublistParameter node) {
            node.Variable = _binder.DefineParameter(node.Name);
            return true;
        }
        public override bool Walk(TupleExpression node) {
            return true;
        }
    }

    class DeleteBinder : PythonWalkerNonRecursive {
        private PythonNameBinder _binder;
        public DeleteBinder(PythonNameBinder binder) {
            _binder = binder;
        }
        public override bool Walk(NameExpression node) {
            _binder.DefineDeleted(node.Name);
            return false;
        }
    }

    class PythonNameBinder : PythonWalker {
        private PythonAst _globalScope;
        private ScopeStatement _currentScope;
        private List<ScopeStatement> _scopes = new List<ScopeStatement>();

        #region Recursive binders

        private DefineBinder _define;
        private DeleteBinder _delete;
        private ParameterBinder _parameter;

        #endregion

        private readonly CompilerContext _context;

        private PythonNameBinder(CompilerContext context) {
            _define = new DefineBinder(this);
            _delete = new DeleteBinder(this);
            _parameter = new ParameterBinder(this);
            _context = context;
        }

        #region Public surface

        internal static void BindAst(PythonAst ast, CompilerContext context) {
            Assert.NotNull(ast, context);

            PythonNameBinder binder = new PythonNameBinder(context);
            binder.Bind(ast);
        }

        internal ModuleOptions Module {
            get {
                return ((PythonCompilerOptions)_context.Options).Module;
            }
        }

        #endregion

        private void Bind(PythonAst unboundAst) {
            Assert.NotNull(unboundAst);

            _currentScope = _globalScope = unboundAst;

            // Find all scopes and variables
            unboundAst.Walk(this);

            // Bind
            foreach (ScopeStatement scope in _scopes) {
                scope.Bind(this);
            }

            // Finish the globals
            unboundAst.Bind(this);

            // Run flow checker
            foreach (ScopeStatement scope in _scopes) {
                FlowChecker.Check(scope);
            }
        }

        private void PushScope(ScopeStatement node) {
            node.Parent = _currentScope;
            _currentScope = node;
        }

        internal PythonReference Reference(SymbolId name) {
            return _currentScope.Reference(name);
        }

        internal PythonVariable DefineName(SymbolId name) {
            return _currentScope.EnsureVariable(name);
        }

        internal PythonVariable DefineParameter(SymbolId name) {
            return _currentScope.DefineParameter(name);
        }

        internal PythonVariable DefineDeleted(SymbolId name) {
            PythonVariable variable = _currentScope.EnsureVariable(name);
            variable.Deleted = true;
            return variable;
        }

        internal void ReportSyntaxWarning(string message, Node node) {
            _context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.Warning);
        }

        internal void ReportSyntaxError(string message, Node node) {
            // TODO: Change the error code (-1)
            _context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.FatalError);
            throw PythonOps.SyntaxError(message, _context.SourceUnit, node.Span, -1);
        }

        #region AstBinder Overrides

        // AssignmentStatement
        public override bool Walk(AssignmentStatement node) {
            foreach (Expression e in node.Left) {
                e.Walk(_define);
            }
            return true;
        }

        public override bool Walk(AugmentedAssignStatement node) {
            node.Left.Walk(_define);
            return true;
        }

        public override void PostWalk(CallExpression node) {
            if (node.NeedsLocalsDictionary()) {
                _currentScope.NeedsLocalsDictionary = true;
            }
        }

        // ClassDefinition
        public override bool Walk(ClassDefinition node) {
            node.Variable = DefineName(node.Name);

            // Base references are in the outer context
            foreach (Expression b in node.Bases) b.Walk(this);

            // process the decorators in the outer context
            if (node.Decorators != null) {
                foreach (Expression dec in node.Decorators) {
                    dec.Walk(this);
                }
            }
            
            PushScope(node);

            node.ModuleNameVariable = _globalScope.EnsureGlobalVariable(this, Symbols.Name);

            // define the __doc__ and the __module__
            if (node.Body.Documentation != null) {
                node.DocVariable = DefineName(Symbols.Doc);
            }
            node.ModVariable = DefineName(Symbols.Module);

            // Walk the body
            node.Body.Walk(this);
            return false;
        }

        // ClassDefinition
        public override void PostWalk(ClassDefinition node) {
            Debug.Assert(node == _currentScope);
            _scopes.Add(_currentScope);
            _currentScope = _currentScope.Parent;
        }

        // DelStatement
        public override bool Walk(DelStatement node) {
            foreach (Expression e in node.Expressions) {
                e.Walk(_delete);
            }
            return true;
        }

        // ExecStatement
        public override bool Walk(ExecStatement node) {
            if (node.Locals == null && node.Globals == null) {
                Debug.Assert(_currentScope != null);
                _currentScope.ContainsUnqualifiedExec = true;
            }
            return true;
        }

        public override void PostWalk(ExecStatement node) {
            if (node.NeedsLocalsDictionary()) {
                _currentScope.NeedsLocalsDictionary = true;
            }
        }

        // ForEachStatement
        public override bool Walk(ForStatement node) {
            node.Left.Walk(_define);
            // Add locals
            return true;
        }

        // WithStatement
        public override bool Walk(WithStatement node) {
            if (node.Variable != null) {
                node.Variable.Walk(_define);
            }
            return true;
        }

        // FromImportStatement
        public override bool Walk(FromImportStatement node) {
            if (node.Names != FromImportStatement.Star) {
                PythonVariable[] variables = new PythonVariable[node.Names.Count];
                for (int i = 0; i < node.Names.Count; i++) {
                    SymbolId name = node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i];
                    variables[i] = DefineName(name);
                }
                node.Variables = variables;
            } else {
                Debug.Assert(_currentScope != null);
                _currentScope.ContainsImportStar = true;
                _currentScope.NeedsLocalsDictionary = true;
            }
            return true;
        }

        // FunctionDefinition
        public override bool Walk(FunctionDefinition node) {
            node._nameVariable = _globalScope.EnsureGlobalVariable(Symbols.Name);            
            
            // Name is defined in the enclosing context
            if (!node.IsLambda) {
                node.Variable = DefineName(node.Name);
            }
            
            // process the default arg values in the outer context
            foreach (Parameter p in node.Parameters) {
                if (p.DefaultValue != null) {
                    p.DefaultValue.Walk(this);
                }
            }
            // process the decorators in the outer context
            if (node.Decorators != null) {
                foreach (Expression dec in node.Decorators) {
                    dec.Walk(this);
                }
            }

            PushScope(node);

            foreach (Parameter p in node.Parameters) {
                p.Walk(_parameter);
            }

            node.Body.Walk(this);
            return false;
        }

        // FunctionDefinition
        public override void PostWalk(FunctionDefinition node) {
            Debug.Assert(_currentScope == node);
            _scopes.Add(_currentScope);
            _currentScope = _currentScope.Parent;
        }

        // GlobalStatement
        public override bool Walk(GlobalStatement node) {
            foreach (SymbolId n in node.Names) {
                PythonVariable conflict;
                // Check current scope for conflicting variable
                bool assignedGlobal = false;
                if (_currentScope.TryGetVariable(n, out conflict)) {
                    // conflict?
                    switch (conflict.Kind) {
                        case VariableKind.Global:
                        case VariableKind.Local:
                        case VariableKind.HiddenLocal:
                        case VariableKind.GlobalLocal:
                            assignedGlobal = true;
                            ReportSyntaxWarning(
                                String.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "name '{0}' is assigned to before global declaration",
                                    SymbolTable.IdToString(n)
                                ),
                                node
                            );
                            break;
                        
                        case VariableKind.Parameter:
                            ReportSyntaxError(
                                String.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Name '{0}' is a function parameter and declared global",
                                    SymbolTable.IdToString(n)),
                                node);
                            break;
                    }
                } else {
                    conflict = null;
                }

                // Check for the name being referenced previously. If it has been, issue warning.
                if (_currentScope.IsReferenced(n) && !assignedGlobal) {
                    ReportSyntaxWarning(
                        String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "name '{0}' is used prior to global declaration",
                        SymbolTable.IdToString(n)),
                    node);
                }


                // Create the variable in the global context and mark it as global
                PythonVariable variable = _globalScope.EnsureGlobalVariable(n);
                variable.Kind = VariableKind.Global;

                if (conflict == null) {
                    // no previously definied variables, add it to the current scope
                    _currentScope.AddGlobalVariable(variable);
                }
            }
            return true;
        }

        public override bool Walk(NameExpression node) {
            node.Reference = Reference(node.Name);
            return true;
        }

        // PythonAst
        public override bool Walk(PythonAst node) {
            if (node.Module) {
                node.NameVariable = DefineName(Symbols.Name);
                node.FileVariable = DefineName(Symbols.File);
                node.DocVariable = DefineName(Symbols.Doc);
            }
            return true;
        }

        // PythonAst
        public override void PostWalk(PythonAst node) {
            // Do not add the global suite to the list of processed nodes,
            // the publishing must be done after the class local binding.
            Debug.Assert(_currentScope == node);
            _currentScope = _currentScope.Parent;
        }

        // ImportStatement
        public override bool Walk(ImportStatement node) {
            PythonVariable[] variables = new PythonVariable[node.Names.Count];
            for (int i = 0; i < node.Names.Count; i++) {
                SymbolId name = node.AsNames[i] != SymbolId.Empty ? node.AsNames[i] : node.Names[i].Names[0];
                variables[i] = DefineName(name);
            }
            node.Variables = variables;
            return true;
        }

        // TryStatement
        public override bool Walk(TryStatement node) {
            if (node.Handlers != null) {
                foreach (TryStatementHandler tsh in node.Handlers) {
                    if (tsh.Target != null) {
                        tsh.Target.Walk(_define);
                    }
                }
            }

            return true;
        }

        // ListComprehensionFor
        public override bool Walk(ListComprehensionFor node) {
            node.Left.Walk(_define);
            return true;
        }

        #endregion
    }
}
