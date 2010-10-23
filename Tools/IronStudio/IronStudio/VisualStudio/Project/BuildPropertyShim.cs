using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class BuildPropertyShim {
        private readonly BuildProperty _property;
        public BuildPropertyShim(BuildProperty property) {
            _property = property;
        }

        public string Name { get { return _property.Name; } }
    }
}
