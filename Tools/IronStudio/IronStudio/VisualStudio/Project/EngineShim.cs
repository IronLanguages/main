using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
   
    class EngineShim {
        private Engine _engine;

        public EngineShim()
            : this(new Engine()) {
        }

        public EngineShim(Engine engine) {
            _engine = engine;
        }

        internal ProjectShim CreateNewProject() {
            var project = _engine.CreateNewProject();
            if (project != null) {
                return new ProjectShim(project);
            }
            return null;
        }

        internal void UnloadProject(ProjectShim projectShim) {
            _engine.UnloadProject(projectShim.Project);
        }
    }

    enum SecurityCheckPass {
        Targets,
        Properties,
        Items,
    }
}
