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

using IronPython.Runtime;
using IronPython.Runtime.Operations;

using MSAst = System.Linq.Expressions;

namespace IronPython.Compiler.Ast {

    public abstract class ScopeStatement : Statement {
        private ScopeStatement _parent;

        private bool _importStar;                   // from module import *
        private bool _unqualifiedExec;              // exec "code"
        private bool _nestedFreeVariables;          // nested function with free variable
        private bool _locals;                       // The scope needs locals dictionary
                                                    // due to "exec" or call to dir, locals, eval, vars...
        private bool _hasLateboundVarSets;          // calls code which can assign to variables
        
        private Dictionary<SymbolId, PythonVariable> _variables;
        private Dictionary<SymbolId, PythonReference> _references;
        private Dictionary<SymbolId, PythonReference> _childReferences;
        private List<SymbolId> _freeVars, _globalVars, _cellVars;
        
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

        /// <summary>
        /// True if an inner scope is accessing a variable defined in this scope.
        /// </summary>
        internal bool ContainsNestedFreeVariables {
            get { return _nestedFreeVariables; }
            set { _nestedFreeVariables = value; }
        }

        /// <summary>
        /// True if we are forcing the creation of a dictionary for storing locals.
        /// 
        /// This occurs for calls to locals(), dir(), vars(), unqualified exec, and
        /// from ... import *.
        /// </summary>
        internal bool NeedsLocalsDictionary {
            get { return _locals; }
            set { _locals = value; }
        }

        /// <summary>
        /// True if variables can be set in a late bound fashion that we don't
        /// know about at code gen time - for example via from foo import *.
        /// 
        /// This is tracked independently of the ContainsUnqualifiedExec/NeedsLocalsDictionary
        /// </summary>
        internal bool HasLateBoundVariableSets {
            get {
                return _hasLateboundVarSets;
            }
            set {
                _hasLateboundVarSets = value;
            }
        }

        internal Dictionary<SymbolId, PythonVariable> Variables {
            get { return _variables; }
        }

        internal Dictionary<SymbolId, PythonReference> References {
            get { return _references; }
        }

        internal virtual bool IsGlobal {
            get { return false; }
        }

        internal SymbolId AddFreeVariable(SymbolId name) {
            if (_freeVars == null) {
                _freeVars = new List<SymbolId>();
            }
            if (!_freeVars.Contains(name)) {
                _freeVars.Add(name);
            }
            return name;
        }

        internal SymbolId AddReferencedGlobal(SymbolId name) {
            if (_globalVars == null) {
                _globalVars = new List<SymbolId>();
            }
            if (!_globalVars.Contains(name)) {
                _globalVars.Add(name);
            }
            return name;
        }

        internal SymbolId AddCellVariable(SymbolId name) {
            if (_cellVars == null) {
                _cellVars = new List<SymbolId>();
            }
            if (!_cellVars.Contains(name)) {
                _cellVars.Add(name);
            }
            return name;
        }

        internal void UpdateReferencedVariables(SymbolId name, PythonVariable variable, ScopeStatement parent) {
            if (variable.Kind == VariableKind.Global || variable.Kind == VariableKind.GlobalLocal) {
                AddReferencedGlobal(name);
            } else {
                name = AddFreeVariable(name);

                for (ScopeStatement innerParent = Parent; innerParent != parent; innerParent = innerParent.Parent) {
                    innerParent.AddFreeVariable(name);
                }
            }
        }

        internal List<SymbolId> AppendVariables(List<SymbolId> res) {
            if (Variables != null) {
                foreach (var variable in Variables) {
                    if (variable.Value.Kind != VariableKind.Local) {
                        continue;
                    }

                    if (CellVariables == null || !CellVariables.Contains(variable.Key)) {
                        res.Add(variable.Key);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Variables that are bound in an outer scope - but not a global scope
        /// </summary>
        internal IList<SymbolId> FreeVariables {
            get {
                return _freeVars;
            }
        }

        /// <summary>
        /// Variables that are bound to the global scope
        /// </summary>
        internal IList<SymbolId> GlobalVariables {
            get {
                return _globalVars;
            }
        }

        /// <summary>
        /// Variables that are referred to from a nested scope and need to be
        /// promoted to cells.
        /// </summary>
        internal IList<SymbolId> CellVariables {
            get {
                return _cellVars;
            }
        }

        internal abstract bool ExposesLocalVariable(PythonVariable variable);

        internal void CreateVariables(AstGenerator ag, MSAst.Expression parentContext, List<MSAst.Expression> init, bool emitDictionary, bool needsLocals) {
            if (_variables != null) {
                CreateLocalVariables(ag, init, emitDictionary);
            }

            if (_references != null) {
                CreateReferencedVariables(ag, init, emitDictionary, needsLocals);
            }

            if (_childReferences != null) {
                CreateChildReferencedVariables(ag, parentContext, init);
            }
        }

        /// <summary>
        /// Creates variables which are defined in this scope.
        /// </summary>
        private void CreateLocalVariables(AstGenerator ag, List<MSAst.Expression> init, bool emitDictionary) {
            foreach (KeyValuePair<SymbolId, PythonVariable> kv in _variables) {
                PythonVariable pv = kv.Value;
                // Publish variables for this context only (there may be references to the global variables
                // in the dictionary also that were used for name binding lookups)
                if (pv.Scope == this) {
                    // Do not publish parameters, they will get created separately.
                    if (pv.Kind != VariableKind.Parameter) {
                        MSAst.Expression var = ag.Globals.CreateVariable(ag, pv, emitDictionary);

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
                        if (pv.Kind == VariableKind.Local && !IsGlobal && (pv.ReadBeforeInitialized || pv.AccessedInNestedScope || ExposesLocalVariable(pv)) ||
                            pv.Kind == VariableKind.GlobalLocal && pv.ReadBeforeInitialized ||
                            pv.Kind == VariableKind.HiddenLocal) {

                            Debug.Assert(pv.Kind != VariableKind.HiddenLocal || pv.ReadBeforeInitialized, "Hidden variable is always uninitialized");

                            if (var is ClosureExpression) {
                                init.Add(((ClosureExpression)var).Create());
                            } else {
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
        }

        /// <summary>
        /// Creates variables which are defined in a parent scope and accessed in this scope.
        /// </summary>
        private void CreateReferencedVariables(AstGenerator ag, List<MSAst.Expression> init, bool emitDictionary, bool needsLocals) {
            MSAst.Expression localTuple = null;
            foreach (KeyValuePair<SymbolId, PythonReference> kv in _references) {
                PythonVariable var = kv.Value.PythonVariable;

                if (var == null || var.Scope == this) {
                    continue;
                }

                if ((var.Kind == VariableKind.Local || var.Kind == VariableKind.Parameter) && !var.Scope.IsGlobal) {
                    // closed over local, we need to pull in the closure variable
                    Type tupleType = ag.GetParentTupleType();
                    int index = ag.TupleIndex(var);

                    localTuple = EnsureLocalTuple(ag, init, localTuple, tupleType);

                    // get the closure cell from the tuple
                    MSAst.Expression tuplePath = localTuple;
                    foreach (var v in MutableTuple.GetAccessPath(tupleType, index)) {
                        tuplePath = MSAst.Expression.Property(tuplePath, v);
                    }

                    MSAst.ParameterExpression pe = ag.HiddenVariable(typeof(ClosureCell), SymbolTable.IdToString(var.Name));
                    init.Add(MSAst.Expression.Assign(pe, tuplePath));

                    ag.SetLocalLiftedVariable(var, new ClosureExpression(var, pe, null));

                    if (emitDictionary) {
                        ag.ReferenceVariable(var, index, localTuple, needsLocals);
                    }
                }
            }
        }

        /// <summary>
        /// Creates variables which are defined in a parent scope and used by a child scope.
        /// </summary>
        private void CreateChildReferencedVariables(AstGenerator ag, MSAst.Expression parentContext, List<MSAst.Expression> init) {
            MSAst.Expression localTuple = null;
            foreach (KeyValuePair<SymbolId, PythonReference> kv in _childReferences) {
                // a child scope refers to this closure value but we don't refer
                // to it directly.
                int index = ag.TupleIndex(kv.Value.PythonVariable);
                Type tupleType = ag.GetParentTupleType();

                if (localTuple == null) {
                    // pull the tuple from the context once
                    localTuple = ag.HiddenVariable(tupleType, "$parentClosureTuple");
                    init.Add(
                        MSAst.Expression.Assign(
                            localTuple,
                            MSAst.Expression.Convert(
                                MSAst.Expression.Call(
                                    typeof(PythonOps).GetMethod("GetClosureTupleFromContext"),
                                    parentContext
                                ),
                                tupleType
                            )
                        )
                    );
                }

                ag.ReferenceVariable(kv.Value.PythonVariable, index, localTuple, false);
            }
        }

        private MSAst.Expression EnsureLocalTuple(AstGenerator ag, List<System.Linq.Expressions.Expression> init, MSAst.Expression localTuple, Type tupleType) {
            if (localTuple == null) {
                // pull the tuple from the context once
                localTuple = ag.HiddenVariable(tupleType, "$closureTuple");
                init.Add(
                    MSAst.Expression.Assign(
                        localTuple,
                        MSAst.Expression.Convert(
                            GetClosureTuple(),
                            tupleType
                        )
                    )
                );
            }
            return localTuple;
        }
        
        public virtual MSAst.Expression GetClosureTuple() {
            // PythonAst will never call this.
            throw new NotSupportedException();
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
                    if (variable != null) {
                        if (variable.Deleted &&
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
                        
                        if (variable.Scope != this && 
                            variable.Kind != VariableKind.Global && variable.Kind != VariableKind.GlobalLocal &&
                            !variable.Scope.IsGlobal) {

                            ScopeStatement curScope = Parent;
                            while (curScope != variable.Scope) {
                                if (curScope._childReferences == null) {
                                    curScope._childReferences = new Dictionary<SymbolId, PythonReference>();
                                }

                                curScope._childReferences[kv.Key] = kv.Value;
                                curScope = curScope.Parent;
                            }
                        }
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
