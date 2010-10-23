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
using System.Text;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;

namespace Microsoft.PyAnalysis.Values {
    internal class ModuleInfo : Namespace, IReferenceableContainer {
        private readonly string _name;
        private readonly ProjectEntry _projectEntry;
        private readonly Dictionary<Node, ISet<Namespace>> _sequences;  // sequences defined in the module
        private readonly Dictionary<Node, ImportInfo> _imports;         // imports performed during the module
        private readonly ModuleScope _scope;
        private readonly WeakReference _weakModule;
        private ModuleInfo _parentPackage;
        public bool ShowClr { get; set; }
        private DependentData _definition = new DependentData();

        public ModuleInfo(string moduleName, ProjectEntry projectEntry) {
            _name = moduleName;
            _projectEntry = projectEntry;
            ShowClr = false;
            _sequences = new Dictionary<Node, ISet<Namespace>>();
            _imports = new Dictionary<Node, ImportInfo>();
            _scope = new ModuleScope(this);
            _weakModule = new WeakReference(this);
        }

        public override IDictionary<string, ISet<Namespace>> GetAllMembers(bool showClr) {
            var res = new Dictionary<string, ISet<Namespace>>();
            foreach (var kvp in _scope.Variables) {
                res[kvp.Key] = kvp.Value.Types;
            }
            return res;
        }

        public ModuleInfo ParentPackage {
            get { return _parentPackage; }
            set { _parentPackage = value; }
        }

        public override ISet<Namespace> GetMember(Node node, AnalysisUnit unit, string name) {
            ModuleDefinition.AddDependency(unit);

            return Scope.CreateVariable(node, unit, name).Types;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, ISet<Namespace> value) {
            var variable = Scope.CreateVariable(node, unit, name, false);
            if (variable.AddTypes(node, unit, value)) {
                ModuleDefinition.EnqueueDependents();
            }
            
            variable.AddAssignment(node, unit);
        }

        /// <summary>
        /// Gets a weak reference to this module
        /// </summary>
        public WeakReference WeakModule {
            get {
                return _weakModule;
            }
        }

        public DependentData ModuleDefinition {
            get {
                return _definition;
            }
        }

        public ModuleScope Scope {
            get {
                return _scope;
            }
        }

        public string Name {
            get { return _name; }
        }

        public ProjectEntry ProjectEntry {
            get { return _projectEntry; }
        }

        public Dictionary<Node, ImportInfo> Imports {
            get {
                return _imports;
            }
        }

        public override ResultType ResultType {
            get {
                return ResultType.Module;
            }
        }

        public override string ToString() {
            return "Module " + base.ToString();
        }

        public override string ShortDescription {
            get {
                return "Python module " + Name;
            }
        }

        public override string Description {
            get {
                var result = new StringBuilder("Python module ");
                result.Append(Name);
                var doc = ((PythonAst)ProjectEntry.Tree).Documentation;
                if (doc != null) {
                    result.Append("\n\n");
                    result.Append(doc);
                }
                return result.ToString();
            }
        }

        public override LocationInfo Location {
            get {
                return new LocationInfo(ProjectEntry, 1, 1, 0);
            }
        }

        public Dictionary<Node, ISet<Namespace>> NodeVariables {
            get { return _sequences; }
        }

        /// <summary>
        /// Cached node variables so that we don't continually create new entries for basic nodes such
        /// as sequences, lambdas, etc...
        /// </summary>
        public ISet<Namespace> GetOrMakeNodeVariable(Node node, Func<Node, ISet<Namespace>> maker) {
            ISet<Namespace> result;
            if (!NodeVariables.TryGetValue(node, out result)) {
                result = NodeVariables[node] = maker(node);
            }
            return result;
        }

        #region IVariableDefContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            VariableDef def;
            if (_scope.Variables.TryGetValue(name, out def)) {
                yield return def;
            }
        }

        #endregion
    }
}
