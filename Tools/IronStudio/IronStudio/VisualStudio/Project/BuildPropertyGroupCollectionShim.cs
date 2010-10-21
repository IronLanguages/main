using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class BuildPropertyGroupCollectionShim : IEnumerable<BuildPropertyGroupShim> {
        private Build.BuildEngine.BuildPropertyGroupCollection buildPropertyGroupCollection;

        public BuildPropertyGroupCollectionShim(Build.BuildEngine.BuildPropertyGroupCollection buildPropertyGroupCollection) {
            // TODO: Complete member initialization
            this.buildPropertyGroupCollection = buildPropertyGroupCollection;
        }

        #region IEnumerable<BuildPropertyGroupShim> Members

        public IEnumerator<BuildPropertyGroupShim> GetEnumerator() {
            foreach (var v in buildPropertyGroupCollection) {
                yield return new BuildPropertyGroupShim((BuildPropertyGroup)v);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach (var v in buildPropertyGroupCollection) {
                yield return new BuildPropertyGroupShim((BuildPropertyGroup)v);
            }
        }

        #endregion
    }
}
