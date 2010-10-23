/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Values;

namespace Microsoft.PyAnalysis.Interpreter {
    /// <summary>
    /// Encapsulates a single piece of code which can be analyzed.  Currently this could be a top-level module, a class definition, or
    /// a function definition.  AnalysisUnit holds onto both the AST of the code which is to be analyzed along with
    /// the scope in which the object is declared.
    /// </summary>
    internal class AnalysisUnit {
        private readonly Node _ast;
        private readonly InterpreterScope[] _scopes;
        private readonly AnalysisUnit _parent;
        private bool _inQueue, _forEval;

        public AnalysisUnit(Node node, InterpreterScope[] scopes, AnalysisUnit parent) {
            _ast = node;
            _scopes = scopes;
            _parent = parent;
        }

        private AnalysisUnit(Node ast, InterpreterScope[] scopes, AnalysisUnit parent, bool forEval) {
            _ast = ast;
            _scopes = scopes;
            _parent = parent;
            _forEval = forEval;
        }

        public bool IsInQueue {
            get {
                var cur = this;
                do {
                    if (_inQueue) {
                        return true;
                    }

                    cur = cur._parent;
                } while (cur != null);

                return false;
            }
            set {
                _inQueue = value;
            }
        }

        /// <summary>
        /// True if this analysis unit is being used to evaluate the result of the analysis.  In this
        /// mode we don't track references or re-queue items.
        /// </summary>
        public bool ForEval {
            get {
                return _forEval;
            }
        }

        public AnalysisUnit CopyForEval() {
            return new AnalysisUnit(_ast, _scopes, _parent, true);
        }

        public AnalysisUnit Parent {
            get {
                return _parent;
            }
        }

        public void Enqueue() {
            if (!ForEval && !IsInQueue) {
                Queue.Enqueue(this);
                this.IsInQueue = true;
            }
        }

        /// <summary>
        /// The queue this analysis unit is associated with
        /// </summary>
        public Queue<AnalysisUnit> Queue {
            get {
                return ProjectState.Queue;
            }
        }

        /// <summary>
        /// The global scope that the code associated with this analysis unit is declared within.
        /// </summary>
        public ModuleInfo DeclaringModule {
            get {

                Debug.Assert(_scopes[0] != null);
                return ((ModuleScope)_scopes[0]).Module;
            }
        }

        public IProjectEntry ProjectEntry {
            get {
                return DeclaringModule.ProjectEntry;
            }
        }

        public ProjectState ProjectState {
            get {
                return DeclaringModule.ProjectEntry.ProjectState;
            }
        }

        /// The AST which will be analyzed when this node is analyzed
        /// </summary>
        public Node Ast {
            get { return _ast; }
        }

        /// <summary>
        /// The chain of scopes in which this analysis is defined.
        /// </summary>
        public InterpreterScope[] Scopes {
            get { return _scopes; }
        }

        public override string ToString() {
            return String.Format(
                "<_AnalysisUnit: ModuleName={0}, NodeType={1}, ScopeName={2}>",
                ((ModuleScope)_scopes[1]).Name,
                _ast.GetType().Name,
                _scopes[_scopes.Length - 1].Name
                );
        }
    }
}
