using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    public class ProjectShim {
        private readonly Microsoft.Build.BuildEngine.Project _project;

        public ProjectShim(Microsoft.Build.BuildEngine.Project project) {
            _project = project;
        }

        public string FullFileName {
            get {
                return _project.FullFileName;
            }
        }

        internal BuildItemGroupShim GetEvaluatedItemsByNameIgnoringCondition(string itemName) {
            BuildItemGroup evaluatedItemsByNameIgnoringCondition = _project.GetEvaluatedItemsByNameIgnoringCondition(itemName);
            if (evaluatedItemsByNameIgnoringCondition != null) {
                return new BuildItemGroupShim(evaluatedItemsByNameIgnoringCondition);
            }
            return null;
        }

        internal string GetEvaluatedProperty(string propertyName) {
            return _project.GetEvaluatedProperty(propertyName);
        }

        internal void Load(string projectFilePath) {
            _project.Load(projectFilePath);
        }

        internal EngineShim ParentEngine {
            get {
                Engine engine = _project.ParentEngine;
                if (engine != null) {
                    return new EngineShim(engine);
                }
                return null;
            }
        }

        public Build.BuildEngine.Project Project { get { return _project; } }

        internal TargetCollectionShim Targets {
            get {
                TargetCollection targetCollection = this._project.Targets;
                if (targetCollection != null) {
                    return new TargetCollectionShim(targetCollection);
                }
                return null;
            }
        }

        internal BuildPropertyGroupCollectionShim PropertyGroups {
            get {
                BuildPropertyGroupCollection buildPropertyGroupCollection = this._project.PropertyGroups;
                if (buildPropertyGroupCollection != null) {
                    return new BuildPropertyGroupCollectionShim(buildPropertyGroupCollection);
                }
                return null;
            }
        }

        public UsingTaskCollection UsingTasks {
            get {
                return _project.UsingTasks;
            }
        }

        public ImportCollection Imports {
            get {
                return _project.Imports;
            }
        }

        internal BuildItemGroupCollectionShim ItemGroups {
            get {
                BuildItemGroupCollection buildItemGroupCollection = this._project.ItemGroups;
                if (buildItemGroupCollection != null) {
                    return new BuildItemGroupCollectionShim(buildItemGroupCollection);
                }
                return null;
            }
        }



    }
}
