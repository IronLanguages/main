using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class TargetShim {
        private Target _target;

        public TargetShim(Target target) {
            _target = target;
        }

        public string Name { get { return _target.Name; } }

        public bool IsImported { get { return _target.IsImported; } }
    }
}
