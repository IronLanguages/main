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
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using AstUtils = Microsoft.Scripting.Ast.Utils;
using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {

    public abstract class ScopeStatement : Statement {
        private ScopeStatement _parent;

        private bool _importStar;                   // from module import *
        private bool _unqualifiedExec;              // exec "code"
        private bool _nestedFreeVariables;          // nested function with free variable
        private bool _locals;                       // The scope needs locals dictionary
                                                    // due to "exec" or call to dir, locals, eval, vars...
        
        // the scope contains variables that are bound to parent scope forming a closure:
        private bool _closure;

        private Dictionary<SymbolId, PythonVariable> _variables;
        private Dictionary<SymbolId, PythonReference> _references;

        public ScopeStatement Parent {
            get { return _parent; }
            set { _parent = value; }
        }

        internal bool ContainsImportStar {
            get { return _importStar; }
            set { _importStar = value; }
        }

        internal bool ContainsUnqualifiedExec {
            get { return _unqualifiedExec; }
            set { _unqualifiedExec = value; }
        }

        internal bool ContainsNestedFreeVariables {
            get { return _nestedFreeVariables; }
            set { _nestedFreeVariables = value; }
        }

        internal bool NeedsLocalsDictionary {
            get { return _locals; }
            set { _locals = value; }
        }

        internal bool IsClosure {
            get { return _closure; }
            set { _closure = value; }
        }

        internal Dictionary<SymbolId, PythonVariable> Variables {
            get { return _variables; }
        }

        internal virtual bool IsGlobal {
            get { return false; }
        }

        protected abstract bool ExposesLocalVariables { get; }

        internal virtual void CreateVariables(AstGenerator ag, List<MSAst.Expression> init) {
            if (_variables != null) {
                foreach (KeyValuePair<SymbolId, PythonVariable> kv in _variables) {
                    PythonVariable pv = kv.Value;
                    // Publish variables for this context only (there may be references to the global variables
                    // in the dictionary also that were used for name binding lookups
                    // Do not publish parameters, they will get created separately.
                    if (pv.Scope == this && pv.Kind != VariableKind.Parameter) {
                        MSAst.Expression var = ag.Globals.CreateVariable(ag, pv);

                        //
                        // Initializes variable to Uninitialized.Instance:
                        //
                        // 1) Local variables (variables that has been assigned within the scope)
                        //    - do not initialize in module scope, ModuleGlobalWrappers do
                        //    - initialize variables that are read before initialized by assignment or deletion
                        //    - initialize variables that are accessed from within a nested scope:
                        //        def f(): 
                        //          def g(): 
                        //            read(a) 
                        //          g()
                        //          write(a)
                        //
                        //    - initialize in a scope that exposes locals (i.e. class scope, function scope with unqualified exec, eval, locals())
                        // 2) Global local variables (variables that weren't assigned within the child scope and were hoisted to the global scope)
                        //    - we need to initialize them because the runtime lookup is relying on that (ModuleGlobalWrapper checks for uninitialized and fethes the value then)
                        //      TODO: this is hacky, the global variable lookup should be implemented better
                        // 3) Hidden local variables (variables that weren't assigned within the scope that contains unqualified exec, eval, locals())
                        //    - initialize them to skip the local slot while looking up the name in scope chain
                        //      TODO: this is hacky as well
                        //
                        if (pv.Kind == VariableKind.Local && !IsGlobal && (pv.ReadBeforeInitialized || pv.AccessedInNestedScope || ExposesLocalVariables) ||
                            pv.Kind == VariableKind.GlobalLocal && pv.ReadBeforeInitialized ||
                            pv.Kind == VariableKind.HiddenLocal) {

                            Debug.Assert(pv.Kind != VariableKind.HiddenLocal || pv.ReadBeforeInitialized, "Hidden variable is always uninitialized");

                            init.Add(
                                ag.Globals.Assign(
                                    var,
                                    MSAst.Expression.Field(null, typeof(Uninitialized).GetField("Instance"))
                                )
                            );
                        }
                    }
                }
            }
        }

        private bool TryGetAnyVariable(SymbolId name, out PythonVariable variable) {
            if (_variables != null) {
                return _variables.TryGetValue(name, out variable);
            } else {
                variable = null;
                return false;
            }
        }

        internal bool TryGetVariable(SymbolId name, out PythonVariable variable) {
            if (TryGetAnyVariable(name, out variable) && variable.Kind != VariableKind.HiddenLocal) {
                return true;
            } else {
                variable = null;
                return false;
            }
        }

        internal virtual bool TryBindOuter(SymbolId name, out PythonVariable variable) {
            // Hide scope contents by default (only functions expose their locals)
            variable = null;
            return false;
        }

        internal abstract PythonVariable BindName(PythonNameBinder binder, SymbolId name);

        internal virtual void Bind(PythonNameBinder binder) {
            if (_references != null) {
                foreach (KeyValuePair<SymbolId, PythonReference> kv in _references) {
                    PythonVariable variable;
                    kv.Value.PythonVariable = variable = BindName(binder, kv.Key);

                    // Accessing outer scope variable which is being deleted?
                    if (variable != null &&
                        variable.Deleted &&
                        (object)variable.Scope != (object)this &&
                        !variable.Scope.IsGlobal) {

                        // report syntax error
                        binder.ReportSyntaxError(
                            String.Format(
                                System.Globalization.CultureInfo.InvariantCulture,
                                "can not delete variable '{0}' referenced in nested scope",
                                SymbolTable.IdToString(kv.Key)
                                ),
                            this);
                    }
                }
            }
        }

        private void EnsureVariables() {
            if (_variables == null) {
                _variables = new Dictionary<SymbolId, PythonVariable>();
            }
        }

        internal void AddGlobalVariable(PythonVariable variable) {
            EnsureVariables();
            _variables[variable.Name] = variable;
        }

        internal PythonReference Reference(SymbolId name) {
            if (_references == null) {
                _references = new Dictionary<SymbolId, PythonReference>();
            }
            PythonReference reference;
            if (!_references.TryGetValue(name, out reference)) {
                _references[name] = reference = new PythonReference(name);
            }
            return reference;
        }

        internal bool IsReferenced(SymbolId name) {
            PythonReference reference;
            return _references != null && _references.TryGetValue(name, out reference);
        }

        internal PythonVariable CreateVariable(SymbolId name, VariableKind kind) {
            EnsureVariables();
            Debug.Assert(!_variables.ContainsKey(name));
            PythonVariable variable;
            _variables[name] = variable = new PythonVariable(name, kind, this);
            return variable;
        }

        internal PythonVariable EnsureVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetVariable(name, out variable)) {
                return CreateVariable(name, VariableKind.Local);
            }
            return variable;
        }

        internal PythonVariable EnsureGlobalVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetVariable(name, out variable)) {
                return CreateVariable(name, VariableKind.Global);
            }
            return variable;
        }

        internal PythonVariable EnsureUnboundVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetVariable(name, out variable)) {
                return CreateVariable(name, VariableKind.GlobalLocal);
            }
            return variable;
        }

        internal PythonVariable EnsureHiddenVariable(SymbolId name) {
            PythonVariable variable;
            if (!TryGetAnyVariable(name, out variable)) {
                variable = CreateVariable(name, VariableKind.HiddenLocal);
            }
            return variable;
        }

        internal PythonVariable DefineParameter(SymbolId name) {
            return CreateVariable(name, VariableKind.Parameter);
        }

        protected internal PythonAst GetGlobalScope() {
            ScopeStatement global = this;
            while (global.Parent != null) {
                global = global.Parent;
            }
            Debug.Assert(global is PythonAst);
            return global as PythonAst;
        }
    }
}
