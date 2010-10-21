using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.VisualStudio.Project {
    class BuildItemShim {
        private Build.BuildEngine.BuildItem _buildItem;

        public BuildItemShim(Build.BuildEngine.BuildItem buildItem) {
            _buildItem = buildItem;
        }
        public string FinalItemSpec { get { return _buildItem.FinalItemSpec; } }
    
        public  string Name { get { return _buildItem.Name; } }
    }
}
