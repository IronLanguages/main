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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronPython.Compiler.Ast;
using Microsoft.PyAnalysis.Interpreter;
using Microsoft.Scripting.Utils;
using Microsoft.Scripting;

namespace Microsoft.PyAnalysis.Values {
    abstract class DependentData<TStorageType>  where TStorageType : DependencyInfo {
        protected SingleDict<IProjectEntry, TStorageType> _dependencies;  

        public void ClearOldValues(IProjectEntry fromModule) {
            TStorageType deps;
            if (_dependencies.TryGetValue(fromModule, out deps)) {
                if (deps.Version != fromModule.Version) {
                    _dependencies.Remove(fromModule);
                }
            }
        }

        protected TStorageType GetDependentItems(IProjectEntry module) {
            TStorageType result;
            if (!_dependencies.TryGetValue(module, out result) || result.Version != module.Version) {
                _dependencies[module] = result = NewDefinition(module.Version);
            }
            return result;
        }

        protected abstract TStorageType NewDefinition(int version);

        /// <summary>
        /// Enqueues any nodes which depend upon this type into the provided analysis queue for
        /// further analysis.
        /// </summary>
        public void EnqueueDependents() {
            foreach (var val in _dependencies.Values) {
                foreach (var analysisUnit in val.DependentUnits) {
                    analysisUnit.Enqueue();
                }
            }
        }

        public void AddDependency(AnalysisUnit unit) {
            if (!unit.ForEval) {
                GetDependentItems(unit.DeclaringModule.ProjectEntry).AddDependentUnit(unit);
            }
        }
    }

    class DependentData : DependentData<DependencyInfo> {
        protected override DependencyInfo NewDefinition(int version) {
            return new DependencyInfo(version);
        }
    }

    /// <summary>
    /// A VariableDef represents a collection of type information and dependencies
    /// upon that type information.  
    /// 
    /// The collection of type information is represented by a HashSet of Namespace
    /// objects.  This set includes all of the types that are known to have been
    /// seen for this variable.
    /// 
    /// Dependency data is added when an one value is assigned to a variable.  
    /// For example for the statement:
    /// 
    /// foo = value
    /// 
    /// There will be a variable def for the name "foo", and "value" will evaluate
    /// to a collection of namespaces.  When value is assigned to
    /// foo the types in value will be propagated to foo's VariableDef by a call
    /// to AddDependentTypes.  If value adds any new type information to foo
    /// then the caller needs to re-analyze anyone who is dependent upon foo'
    /// s values.  If "value" was a VariableDef as well, rather than some arbitrary 
    /// expression, then reading "value" would have made the code being analyzed dependent 
    /// upon "value".  After a call to AddTypes the caller needs to check the 
    /// return value and if new types were added (returns true) needs to re-enque it's scope.
    /// 
    /// Dependecies are stored in a dictionary keyed off of the IProjectEntry object.
    /// This is a consistent object which always represents the same module even
    /// across multiple analysis.  The object is versioned so that when we encounter
    /// a new version all the old dependencies will be thrown away when a variable ref 
    /// is updated with new dependencies.
    /// 
    /// TODO: We should store built-in types not keyed off of the ModuleInfo.
    /// </summary>
    class VariableDef : DependentData<TypedDependencyInfo>, IReferenceable {
        public VariableDef() { }

        protected override TypedDependencyInfo NewDefinition(int version) {
            return new TypedDependencyInfo(version);
        }

        public bool AddTypes(Node node, AnalysisUnit unit, IEnumerable<Namespace> newTypes) {
            bool added = false;

            foreach (var value in newTypes) {
                var declaringModule = value.DeclaringModule;
                if (declaringModule == null || declaringModule.Version == value.DeclaringVersion) {
                    var dependencies = GetDependentItems(declaringModule ?? unit.ProjectEntry);

                    if (dependencies.Types.Add(value, unit.ProjectState)) {
                        added = true;
                    }
                }
            }

            if (added) {
                EnqueueDependents();
            }

            return added;
        }

        public ISet<Namespace> Types {
            get {
                if (_dependencies.Count != 0) {
                    HashSet<Namespace> res = new HashSet<Namespace>();
                    foreach (var mod in _dependencies.Values) {
                        res.UnionWith(mod.Types);
                    }
                    return res;
                }
                return EmptySet<Namespace>.Instance;
            }
        }

        public void AddReference(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                AddReference(new SimpleSrcLocation(node.Span), unit.DeclaringModule.ProjectEntry).AddDependentUnit(unit);
            }
        }

        public TypedDependencyInfo AddReference(SimpleSrcLocation location, IProjectEntry module) {
            var depUnits = GetDependentItems(module);
            depUnits.References.Add(location);
            return depUnits;
        }

        public void AddAssignment(SimpleSrcLocation location, IProjectEntry entry) {
            var depUnits = GetDependentItems(entry);
            depUnits.Assignments.Add(location);
        }

        public void AddAssignment(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                AddAssignment(new SimpleSrcLocation(node.Span), unit.DeclaringModule.ProjectEntry);
            }
        }

        public IEnumerable<KeyValuePair<IProjectEntry, SimpleSrcLocation>> References {
            get {
                if (_dependencies.Count != 0) {
                    foreach (var keyValue in _dependencies) {
                        foreach (var reference in keyValue.Value.References) {
                            yield return new KeyValuePair<IProjectEntry, SimpleSrcLocation>(keyValue.Key, reference);
                        }
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<IProjectEntry, SimpleSrcLocation>> Definitions {
            get {
                if (_dependencies.Count != 0) {
                    foreach (var keyValue in _dependencies) {
                        foreach (var reference in keyValue.Value.Assignments) {
                            yield return new KeyValuePair<IProjectEntry, SimpleSrcLocation>(keyValue.Key, reference);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A variable def which has a specific location where it is defined (currently just function parameters).
    /// </summary>
    sealed class LocatedVariableDef : VariableDef {
        private readonly IProjectEntry _entry;
        private readonly Node _location;
        
        public LocatedVariableDef(IProjectEntry entry, Node location) {
            _entry = entry;
            _location = location;
        }

        public IProjectEntry Entry {
            get {
                return _entry;
            }
        }

        public Node Node {
            get {
                return _location;
            }
        }
    }
}
